using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ツール欄の中の 1 パーツを、別ウィンドウへ分離したり元の場所へ戻したりする処理
	// 見出しをドラッグすると分離し、分離中のウィンドウを元の場所の空きへ重ねると戻る
	// （ボタン・見出しのダブルクリック・×ボタンでも戻せる）
	//-------------------------------------------------------------------------------
	internal sealed class ToolPartDocker : IDisposable
	{
		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

		private const int WM_NCLBUTTONDOWN = 0xA1;
		private const uint RDW_INVALIDATE = 0x1;
		private const uint RDW_ERASE = 0x4;
		private const uint RDW_ALLCHILDREN = 0x80;
		private const uint RDW_FRAME = 0x400;
		private const int HTCAPTION = 2;
		// 元の場所の空きが狭いときでも、戻す先として扱う最低限の高さ（論理ピクセル）
		private const int MinimumDockZoneHeight = 80;

		private readonly Form owner;
		private readonly Control part;
		private readonly Control header;
		private readonly Label title;
		private readonly Button floatButton;
		private readonly Control home;
		// 元の場所での並び順（分離した時点の順を覚える。並べ替えたあとでも元の位置へ戻せるように）
		private int homeIndex;
		private readonly Func<string> floatingTitle;
		private readonly Func<string> dockedTitle;
		private readonly Func<string> windowTitle;
		private readonly KeyEventHandler keyDown;
		private MapToolFloatForm floatForm;
		private MapEditor.DockHintForm hintForm;
		private Rectangle floatBounds = Rectangle.Empty;
		private bool dragArmed;
		private Point dragStart;
		private bool hidden;

		//-------------------------------------------------------------------------------
		// 分離できるパーツを登録する処理
		// part: 分離するパーツ全体、header/title: ドラッグで掴む見出し、floatButton: 分離／戻すボタン
		// dockedTitle/floatingTitle: 見出しの文字、windowTitle: 分離したウィンドウの題名（どれも言語切り替えに合わせて毎回取り直す）
		// keyDown: 分離中のウィンドウで受けるショートカット
		//-------------------------------------------------------------------------------
		public ToolPartDocker(Form owner, Control part, Control header, Label title, Button floatButton, Func<string> dockedTitle, Func<string> floatingTitle, Func<string> windowTitle, KeyEventHandler keyDown)
		{
			this.owner = owner;
			this.part = part;
			this.header = header;
			this.title = title;
			this.floatButton = floatButton;
			this.home = part.Parent;
			this.homeIndex = this.home.Controls.GetChildIndex(part);
			this.dockedTitle = dockedTitle;
			this.floatingTitle = floatingTitle;
			this.windowTitle = windowTitle;
			this.keyDown = keyDown;
			foreach (Control control in new Control[] { header, title })
			{
				control.MouseDown += this.Header_MouseDown;
				control.MouseMove += this.Header_MouseMove;
				control.MouseUp += this.Header_MouseUp;
				control.DoubleClick += this.Header_DoubleClick;
			}
			title.Cursor = Cursors.SizeAll;
			floatButton.Click += this.FloatButton_Click;
			this.UpdateHeader();
		}

		//-------------------------------------------------------------------------------
		// 分離中かどうか
		//-------------------------------------------------------------------------------
		public bool IsFloating { get; private set; }

		//-------------------------------------------------------------------------------
		// 分離ウィンドウの位置と大きさ（分離中は今の窓、そうでなければ次に分離するときに使う値。設定の保存・復元用）
		//-------------------------------------------------------------------------------
		public Rectangle FloatBounds
		{
			get
			{
				if (this.IsFloating && this.floatForm != null && !this.floatForm.IsDisposed && this.floatForm.WindowState == FormWindowState.Normal)
				{
					return this.floatForm.Bounds;
				}
				return this.floatBounds;
			}
			set
			{
				this.floatBounds = value;
			}
		}

		// 分離した・戻したときに知らせる（ツールチップの切り替えなどに使う）
		public event EventHandler StateChanged;

		//-------------------------------------------------------------------------------
		// 分離中のウィンドウにフォーカスがあるか（ショートカットの判定用）
		//-------------------------------------------------------------------------------
		public bool ContainsFocus
		{
			get { return this.floatForm != null && !this.floatForm.IsDisposed && this.floatForm.ContainsFocus; }
		}

		//-------------------------------------------------------------------------------
		// 見出しの押下位置を記録する処理
		//-------------------------------------------------------------------------------
		private void Header_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.dragArmed = true;
				this.dragStart = Cursor.Position;
			}
		}

		//-------------------------------------------------------------------------------
		// 見出しのドラッグ量が一定を超えたら、分離して（分離中ならそのまま）ウィンドウの移動を始める処理
		//-------------------------------------------------------------------------------
		private void Header_MouseMove(object sender, MouseEventArgs e)
		{
			if (!this.dragArmed || (Control.MouseButtons & MouseButtons.Left) == 0)
			{
				return;
			}
			Point current = Cursor.Position;
			Size dragSize = SystemInformation.DragSize;
			if (Math.Abs(current.X - this.dragStart.X) <= dragSize.Width * 2 && Math.Abs(current.Y - this.dragStart.Y) <= dragSize.Height * 2)
			{
				return;
			}
			this.dragArmed = false;
			if (!this.IsFloating)
			{
				this.Float(current);
			}
			this.BeginWindowDrag();
		}

		//-------------------------------------------------------------------------------
		// 見出しのドラッグ待ちを解除する処理
		//-------------------------------------------------------------------------------
		private void Header_MouseUp(object sender, MouseEventArgs e)
		{
			this.dragArmed = false;
		}

		//-------------------------------------------------------------------------------
		// 見出しのダブルクリックで分離／戻すを切り替える処理
		//-------------------------------------------------------------------------------
		private void Header_DoubleClick(object sender, EventArgs e)
		{
			this.Toggle();
		}

		//-------------------------------------------------------------------------------
		// 見出し右端のボタンで分離／戻すを切り替える処理
		//-------------------------------------------------------------------------------
		private void FloatButton_Click(object sender, EventArgs e)
		{
			this.Toggle();
		}

		//-------------------------------------------------------------------------------
		// 分離／戻すを切り替える処理
		//-------------------------------------------------------------------------------
		public void Toggle()
		{
			if (this.IsFloating)
			{
				this.Dock();
			}
			else
			{
				this.Float(null);
			}
		}

		//-------------------------------------------------------------------------------
		// パーツを別ウィンドウへ分離する処理（cursor 指定時は、見出しを掴んだままの位置に出す）
		//-------------------------------------------------------------------------------
		public void Float(Point? cursor)
		{
			if (this.IsFloating)
			{
				return;
			}
			MapToolFloatForm form = this.EnsureFloatForm();
			Rectangle current = this.part.RectangleToScreen(this.part.ClientRectangle);
			// 窓枠の分だけ大きくして、今と同じ大きさで中身が見えるようにする
			Size chrome = form.Size - form.ClientSize;
			Size size = this.floatBounds.IsEmpty
				? new Size(Math.Max(form.MinimumSize.Width, current.Width + chrome.Width), Math.Max(form.MinimumSize.Height, current.Height + chrome.Height))
				: this.floatBounds.Size;
			Point location;
			if (cursor.HasValue)
			{
				location = new Point(cursor.Value.X - size.Width / 2, cursor.Value.Y - SystemInformation.CaptionHeight / 2);
			}
			else if (!this.floatBounds.IsEmpty)
			{
				location = this.floatBounds.Location;
			}
			else
			{
				location = new Point(current.X - chrome.Width / 2, current.Y - SystemInformation.CaptionHeight);
			}
			form.Bounds = ClampToWorkingArea(new Rectangle(location, size));

			this.homeIndex = this.home.Controls.GetChildIndex(this.part);
			this.home.SuspendLayout();
			this.home.Controls.Remove(this.part);
			form.ContentHost.Controls.Add(this.part);
			this.IsFloating = true;
			this.home.ResumeLayout(true);
			if (!this.hidden)
			{
				form.Show(this.owner);
			}
			this.UpdateHeader();
			this.RedrawPart();
			this.StateChanged?.Invoke(this, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------------
		// 分離中のパーツを元の場所（元の並び順）へ戻す処理
		//-------------------------------------------------------------------------------
		public void Dock()
		{
			if (!this.IsFloating || this.floatForm == null)
			{
				return;
			}
			this.HideHint();
			if (this.floatForm.WindowState == FormWindowState.Normal)
			{
				this.floatBounds = this.floatForm.Bounds;
			}
			this.home.SuspendLayout();
			this.floatForm.ContentHost.Controls.Remove(this.part);
			this.home.Controls.Add(this.part);
			this.home.Controls.SetChildIndex(this.part, Math.Min(this.homeIndex, this.home.Controls.Count - 1));
			this.IsFloating = false;
			this.floatForm.Hide();
			this.home.ResumeLayout(true);
			this.UpdateHeader();
			this.RedrawPart();
			this.StateChanged?.Invoke(this, EventArgs.Empty);
			Form homeForm = this.home.FindForm();
			homeForm?.Activate();
		}

		//-------------------------------------------------------------------------------
		// 置き場所を移したパーツを、枠やスクロールバーも含めて描き直す処理（移した直後に古い絵が残らないように）
		//-------------------------------------------------------------------------------
		private void RedrawPart()
		{
			if (this.part.IsHandleCreated)
			{
				RedrawWindow(this.part.Handle, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_ALLCHILDREN | RDW_FRAME);
			}
		}

		//-------------------------------------------------------------------------------
		// 分離中のウィンドウを隠す／出す処理（F4 でツール欄をまとめて隠すときに使う）
		//-------------------------------------------------------------------------------
		public void SetHidden(bool hide)
		{
			this.hidden = hide;
			if (!this.IsFloating || this.floatForm == null)
			{
				return;
			}
			if (hide)
			{
				this.floatForm.Hide();
			}
			else if (!this.floatForm.Visible)
			{
				this.floatForm.WindowState = FormWindowState.Normal;
				this.floatForm.Show(this.owner);
			}
		}

		//-------------------------------------------------------------------------------
		// 見出しの文字・ボタンの絵・ウィンドウの題名を、今の状態と言語に合わせる処理
		//-------------------------------------------------------------------------------
		public void UpdateHeader()
		{
			this.title.Text = this.IsFloating ? this.floatingTitle() : this.dockedTitle();
			UiTheme.SetGlyph(this.floatButton, this.IsFloating ? UiTheme.GlyphDock : UiTheme.GlyphUndock, this.IsFloating ? Localizer.T("戻") : Localizer.T("分"));
			if (this.floatForm != null && !this.floatForm.IsDisposed)
			{
				this.floatForm.Text = this.windowTitle();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離用のウィンドウを必要になった時点で作る処理
		//-------------------------------------------------------------------------------
		private MapToolFloatForm EnsureFloatForm()
		{
			if (this.floatForm != null && !this.floatForm.IsDisposed)
			{
				return this.floatForm;
			}
			this.floatForm = new MapToolFloatForm();
			this.floatForm.Owner = this.owner;
			// 幅を広げて使う用途があるので、最大化もできるようにする
			this.floatForm.MaximizeBox = true;
			this.floatForm.MinimumSize = new Size(240, 240);
			this.floatForm.Text = this.windowTitle();
			this.floatForm.FormClosing += this.FloatForm_FormClosing;
			this.floatForm.Move += this.FloatForm_Move;
			this.floatForm.ResizeEnd += this.FloatForm_ResizeEnd;
			this.floatForm.Resize += this.FloatForm_Resize;
			if (this.keyDown != null)
			{
				this.floatForm.KeyDown += this.keyDown;
			}
			AppIconHelper.Apply(this.floatForm);
			UiTheme.Apply(this.floatForm);
			return this.floatForm;
		}

		//-------------------------------------------------------------------------------
		// ×ボタンなどでは閉じずに元の場所へ戻す処理（アプリの終了時だけは閉じる）
		//-------------------------------------------------------------------------------
		private void FloatForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.IsFloating && !IsApplicationClosing(e.CloseReason))
			{
				e.Cancel = true;
				this.Dock();
			}
		}

		//-------------------------------------------------------------------------------
		// アプリそのものが終わるための「閉じる」かを判定する処理（それ以外は閉じずに戻す）
		//-------------------------------------------------------------------------------
		internal static bool IsApplicationClosing(CloseReason reason)
		{
			return reason == CloseReason.FormOwnerClosing || reason == CloseReason.WindowsShutDown || reason == CloseReason.ApplicationExitCall;
		}

		//-------------------------------------------------------------------------------
		// 最小化されたら元の場所へ戻す処理（別ウィンドウが見失われないように）
		//-------------------------------------------------------------------------------
		private void FloatForm_Resize(object sender, EventArgs e)
		{
			if (this.floatForm.WindowState == FormWindowState.Minimized)
			{
				this.floatForm.WindowState = FormWindowState.Normal;
				this.Dock();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離中のウィンドウを動かしている間、元の場所の空きに重なったら戻す先を示す処理
		//-------------------------------------------------------------------------------
		private void FloatForm_Move(object sender, EventArgs e)
		{
			if (!this.IsFloating || (Control.MouseButtons & MouseButtons.Left) == 0)
			{
				this.HideHint();
				return;
			}
			Rectangle zone;
			if (this.TryGetDockZone(out zone) && zone.Contains(Cursor.Position))
			{
				this.ShowHint(zone);
			}
			else
			{
				this.HideHint();
			}
		}

		//-------------------------------------------------------------------------------
		// ウィンドウの移動を終えたとき、戻す先を示していれば元の場所へ戻す処理
		//-------------------------------------------------------------------------------
		private void FloatForm_ResizeEnd(object sender, EventArgs e)
		{
			bool dock = this.hintForm != null && !this.hintForm.IsDisposed && this.hintForm.Visible;
			this.HideHint();
			if (dock)
			{
				this.Dock();
			}
			else if (this.floatForm.WindowState == FormWindowState.Normal)
			{
				this.floatBounds = this.floatForm.Bounds;
			}
		}

		//-------------------------------------------------------------------------------
		// 戻す先の範囲（元の場所で、ほかのパーツの下に空いた部分）を画面座標で求める処理
		// 元の場所が見えていない（隠れている）ときは false を返す
		//-------------------------------------------------------------------------------
		private bool TryGetDockZone(out Rectangle zone)
		{
			zone = Rectangle.Empty;
			if (!this.home.Visible || this.home.Width <= 0 || this.home.Height <= 0)
			{
				return false;
			}
			Form homeForm = this.home.FindForm();
			if (homeForm == null || !homeForm.Visible || homeForm.WindowState == FormWindowState.Minimized)
			{
				return false;
			}
			Rectangle client = this.home.DisplayRectangle;
			int top = client.Top;
			foreach (Control control in this.home.Controls)
			{
				if (control.Visible && control.Dock == DockStyle.Top)
				{
					top = Math.Max(top, control.Bottom);
				}
			}
			int minimum = this.home.LogicalToDeviceUnits(MinimumDockZoneHeight);
			if (client.Bottom - top < minimum)
			{
				top = Math.Max(client.Top, client.Bottom - minimum);
			}
			zone = this.home.RectangleToScreen(new Rectangle(client.Left, top, client.Width, client.Bottom - top));
			return true;
		}

		//-------------------------------------------------------------------------------
		// 戻す先の範囲を半透明の枠で示す処理
		//-------------------------------------------------------------------------------
		private void ShowHint(Rectangle zone)
		{
			if (this.hintForm == null || this.hintForm.IsDisposed)
			{
				this.hintForm = new MapEditor.DockHintForm(UiTheme.Accent);
			}
			this.hintForm.Bounds = zone;
			if (!this.hintForm.Visible)
			{
				this.hintForm.Show(this.owner);
			}
			this.floatForm?.BringToFront();
		}

		//-------------------------------------------------------------------------------
		// 戻す先の表示を消す処理
		//-------------------------------------------------------------------------------
		private void HideHint()
		{
			if (this.hintForm != null && !this.hintForm.IsDisposed && this.hintForm.Visible)
			{
				this.hintForm.Hide();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離中のウィンドウのタイトルバーを掴んだ状態にして、OS 標準のドラッグ移動を始める処理
		//-------------------------------------------------------------------------------
		private void BeginWindowDrag()
		{
			if (this.floatForm == null || !this.floatForm.Visible)
			{
				return;
			}
			ReleaseCapture();
			SendMessage(this.floatForm.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
		}

		//-------------------------------------------------------------------------------
		// ウィンドウ位置が画面外にはみ出さないよう作業領域内へ収める処理
		//-------------------------------------------------------------------------------
		private static Rectangle ClampToWorkingArea(Rectangle bounds)
		{
			Rectangle area = Screen.FromPoint(bounds.Location).WorkingArea;
			int width = Math.Min(bounds.Width, area.Width);
			int height = Math.Min(bounds.Height, area.Height);
			int x = Math.Max(area.Left, Math.Min(bounds.X, area.Right - width));
			int y = Math.Max(area.Top, Math.Min(bounds.Y, area.Bottom - height));
			return new Rectangle(x, y, width, height);
		}

		//-------------------------------------------------------------------------------
		// 分離用のウィンドウと戻す先の表示を片付ける処理（メイン画面を閉じるとき）
		//-------------------------------------------------------------------------------
		public void Dispose()
		{
			if (this.hintForm != null && !this.hintForm.IsDisposed)
			{
				this.hintForm.Dispose();
			}
			this.hintForm = null;
			if (this.floatForm != null && !this.floatForm.IsDisposed)
			{
				this.floatForm.FormClosing -= this.FloatForm_FormClosing;
				this.floatForm.ContentHost.Controls.Remove(this.part);
				this.floatForm.Close();
				this.floatForm.Dispose();
			}
			this.floatForm = null;
		}
	}
}
