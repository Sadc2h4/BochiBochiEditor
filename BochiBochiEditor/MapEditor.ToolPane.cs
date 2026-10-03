using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップエディタ右側のツール欄を「ドッキング／分離」するための処理群
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int WM_NCLBUTTONDOWN = 0xA1;
		private const int HTCAPTION = 2;

		// ドッキング判定を行う右端の帯の幅（論理ピクセル）
		private const int ToolDockZoneWidth = 72;

		private MapToolFloatForm toolFloatForm;
		private DockHintForm toolDockHintForm;
		private bool isToolPaneFloating;
		private bool isToolPaneHiddenByUser;
		private bool toolHeaderDragArmed;
		private Point toolHeaderDragStart;
		private Rectangle toolFloatBounds = Rectangle.Empty;
		// ブロック／移動エリア／イベントのタブ部分を単独で分離する処理
		private ToolPartDocker toolTabsDocker;

		//-------------------------------------------------------------------------------
		// ツール欄のドラッグ分離に必要なイベントを登録する処理
		//-------------------------------------------------------------------------------
		private void InitializeToolPaneDocking()
		{
			foreach (Control control in new Control[] { this.pnlToolHeader, this.lblToolTitle })
			{
				control.MouseDown += this.ToolHeader_MouseDown;
				control.MouseMove += this.ToolHeader_MouseMove;
				control.MouseUp += this.ToolHeader_MouseUp;
				control.DoubleClick += this.ToolHeader_DoubleClick;
			}
			this.lblToolTitle.Cursor = Cursors.SizeAll;
			this.toolTabsDocker = new ToolPartDocker(this, this.pnlToolTabsPart, this.pnlToolTabsHeader, this.lblToolTabsTitle, this.btnToolTabsFloat,
				() => Localizer.T("ブロック・移動エリア・イベント　（ドラッグで分離）"),
				() => Localizer.T("ブロック・移動エリア・イベント　（元の場所へドラッグで戻す）"),
				() => Localizer.T("ブロック・移動エリア・イベント"),
				this.MapEditor_KeyDown);
			this.toolTabsDocker.StateChanged += (sender, e) => this.UpdateToolFloatButton();
			this.UpdateToolFloatButton();
		}

		//-------------------------------------------------------------------------------
		// ツール欄ヘッダーの押下位置を記録する処理
		//-------------------------------------------------------------------------------
		private void ToolHeader_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			this.toolHeaderDragArmed = true;
			this.toolHeaderDragStart = Cursor.Position;
		}

		//-------------------------------------------------------------------------------
		// ツール欄ヘッダーのドラッグ量が一定を超えたら分離（または移動）を始める処理
		//-------------------------------------------------------------------------------
		private void ToolHeader_MouseMove(object sender, MouseEventArgs e)
		{
			if (!this.toolHeaderDragArmed || (Control.MouseButtons & MouseButtons.Left) == 0)
			{
				return;
			}
			Point current = Cursor.Position;
			Size dragSize = SystemInformation.DragSize;
			bool moved = Math.Abs(current.X - this.toolHeaderDragStart.X) > dragSize.Width * 2 || Math.Abs(current.Y - this.toolHeaderDragStart.Y) > dragSize.Height * 2;
			if (!moved)
			{
				return;
			}
			this.toolHeaderDragArmed = false;
			if (!this.isToolPaneFloating)
			{
				this.FloatToolPane(current);
			}
			this.BeginToolFloatFormDrag();
		}

		//-------------------------------------------------------------------------------
		// ツール欄ヘッダーのドラッグ待ちを解除する処理
		//-------------------------------------------------------------------------------
		private void ToolHeader_MouseUp(object sender, MouseEventArgs e)
		{
			this.toolHeaderDragArmed = false;
		}

		//-------------------------------------------------------------------------------
		// ツール欄ヘッダーのダブルクリックで分離／ドッキングを切り替える処理
		//-------------------------------------------------------------------------------
		private void ToolHeader_DoubleClick(object sender, EventArgs e)
		{
			this.ToggleToolPaneFloating();
		}

		//-------------------------------------------------------------------------------
		// ヘッダー右端のボタンで分離／ドッキングを切り替える処理
		//-------------------------------------------------------------------------------
		private void btnToolFloat_Click(object sender, EventArgs e)
		{
			this.ToggleToolPaneFloating();
		}

		//-------------------------------------------------------------------------------
		// 上部バーのボタンでツール欄の表示／非表示を切り替える処理
		//-------------------------------------------------------------------------------
		private void btnToggleToolPane_Click(object sender, EventArgs e)
		{
			this.ToggleToolPaneVisible();
		}

		//-------------------------------------------------------------------------------
		// ツール欄の分離／ドッキングを切り替える処理
		//-------------------------------------------------------------------------------
		private void ToggleToolPaneFloating()
		{
			if (this.isToolPaneFloating)
			{
				this.DockToolPane();
			}
			else
			{
				this.FloatToolPane(null);
			}
		}

		//-------------------------------------------------------------------------------
		// ツール欄を別ウィンドウへ分離する処理（cursor 指定時はその位置へ出す）
		//-------------------------------------------------------------------------------
		private void FloatToolPane(Point? cursor)
		{
			if (this.isToolPaneFloating)
			{
				return;
			}
			MapToolFloatForm form = this.EnsureToolFloatForm();
			Size size = this.toolFloatBounds.IsEmpty
				? new Size(this.splitWork.Panel2.Width + 24, Math.Max(form.MinimumSize.Height, this.splitWork.Height))
				: this.toolFloatBounds.Size;
			Point location;
			if (cursor.HasValue)
			{
				// ヘッダーを掴んだままの感覚になるよう、タイトルバー付近をカーソル位置へ合わせる
				location = new Point(cursor.Value.X - size.Width / 2, cursor.Value.Y - SystemInformation.CaptionHeight / 2);
			}
			else if (!this.toolFloatBounds.IsEmpty)
			{
				location = this.toolFloatBounds.Location;
			}
			else
			{
				Point panelOrigin = this.splitWork.Panel2.PointToScreen(Point.Empty);
				location = new Point(panelOrigin.X - 24, panelOrigin.Y - SystemInformation.CaptionHeight);
			}
			form.Bounds = this.ClampToWorkingArea(new Rectangle(location, size));

			this.SuspendLayout();
			this.splitWork.Panel2.Controls.Remove(this.pnlToolPane);
			form.ContentHost.Controls.Add(this.pnlToolPane);
			this.splitWork.Panel2Collapsed = true;
			this.isToolPaneFloating = true;
			this.ResumeLayout(true);

			if (!this.isToolPaneHiddenByUser)
			{
				form.Show(this);
			}
			this.UpdateToolFloatButton();
		}

		//-------------------------------------------------------------------------------
		// 分離中のツール欄をメイン画面の右側へ戻す処理
		//-------------------------------------------------------------------------------
		private void DockToolPane()
		{
			if (!this.isToolPaneFloating || this.toolFloatForm == null)
			{
				return;
			}
			this.HideToolDockHint();
			if (this.toolFloatForm.WindowState == FormWindowState.Normal)
			{
				this.toolFloatBounds = this.toolFloatForm.Bounds;
			}
			this.SuspendLayout();
			this.toolFloatForm.ContentHost.Controls.Remove(this.pnlToolPane);
			this.splitWork.Panel2.Controls.Add(this.pnlToolPane);
			this.isToolPaneFloating = false;
			this.toolFloatForm.Hide();
			this.UpdateDockedToolPaneVisibility();
			this.ResumeLayout(true);
			this.UpdateToolFloatButton();
			this.Activate();
		}

		//-------------------------------------------------------------------------------
		// ツール欄の表示／非表示を切り替える処理（分離中はウィンドウごと）
		//-------------------------------------------------------------------------------
		private void ToggleToolPaneVisible()
		{
			this.isToolPaneHiddenByUser = !this.isToolPaneHiddenByUser;
			if (this.isToolPaneFloating)
			{
				if (this.isToolPaneHiddenByUser)
				{
					this.toolFloatForm.Hide();
				}
				else
				{
					this.toolFloatForm.WindowState = FormWindowState.Normal;
					this.toolFloatForm.Show(this);
					this.toolFloatForm.Activate();
				}
			}
			else
			{
				this.UpdateDockedToolPaneVisibility();
			}
			// 単独で分離しているパーツも一緒に隠す／出す
			this.toolTabsDocker?.SetHidden(this.isToolPaneHiddenByUser);
			this.townMapDocker?.SetHidden(this.isToolPaneHiddenByUser);
			foreach (ToolPart part in this.toolParts.Where(p => p.Header != null))
			{
				part.Docker.SetHidden(this.isToolPaneHiddenByUser);
			}
		}

		//-------------------------------------------------------------------------------
		// ROM読込時などにツール欄を表示状態へ戻す処理
		//-------------------------------------------------------------------------------
		private void ShowToolPane()
		{
			if (this.isToolPaneHiddenByUser)
			{
				return;
			}
			if (this.isToolPaneFloating && this.toolFloatForm != null && !this.toolFloatForm.Visible)
			{
				this.toolFloatForm.Show(this);
			}
			this.toolTabsDocker?.SetHidden(false);
			this.townMapDocker?.SetHidden(false);
			foreach (ToolPart part in this.toolParts.Where(p => p.Header != null))
			{
				part.Docker.SetHidden(false);
			}
			this.UpdateDockedToolPaneVisibility();
		}

		//-------------------------------------------------------------------------------
		// ツール欄の中身だけを有効／無効にする処理（ヘッダーは常に操作可能にする）
		//-------------------------------------------------------------------------------
		private void SetToolPaneContentEnabled(bool enabled)
		{
			// 部品は入れ物ではなく中身を切り替える（分離中の部品も、見出しは操作できるように残す）
			foreach (ToolPart part in this.toolParts)
			{
				part.Content.Enabled = enabled;
			}
			// タブ部分は、分離中でも戻せるよう見出しを残して中身だけを切り替える
			this.tabEditorMode.Enabled = enabled;
		}

		//-------------------------------------------------------------------------------
		// ドッキング中のツール欄をマップタブ表示時だけ出す処理
		//-------------------------------------------------------------------------------
		private void UpdateDockedToolPaneVisibility()
		{
			if (this.isToolPaneFloating)
			{
				this.splitWork.Panel2Collapsed = true;
				return;
			}
			bool show = !this.isToolPaneHiddenByUser && this.tabMain.SelectedTab == this.tabMapEdit;
			this.splitWork.Panel2Collapsed = !show;
		}

		//-------------------------------------------------------------------------------
		// 分離／ドッキング状態に合わせてヘッダーのボタン表示を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateToolFloatButton()
		{
			UiTheme.SetGlyph(this.btnToolFloat, this.isToolPaneFloating ? UiTheme.GlyphDock : UiTheme.GlyphUndock, this.isToolPaneFloating ? Localizer.T("戻") : Localizer.T("分"));
			this.lblToolTitle.Text = this.isToolPaneFloating ? Localizer.T("マップ編集ツール　（右端へドラッグで戻す）") : Localizer.T("マップ編集ツール　（ドラッグで分離）");
			this.mapToolTip?.SetToolTip(this.btnToolFloat, this.isToolPaneFloating ? Localizer.T("ツール欄をメイン画面へ戻す") : Localizer.T("ツール欄を別ウィンドウへ分離する"));
			if (this.toolTabsDocker != null)
			{
				this.toolTabsDocker.UpdateHeader();
				this.mapToolTip?.SetToolTip(this.btnToolTabsFloat, this.toolTabsDocker.IsFloating ? Localizer.T("元の場所へ戻す") : Localizer.T("この部分だけを別ウィンドウへ分離する"));
			}
			if (this.townMapDocker != null)
			{
				this.townMapDocker.UpdateHeader();
				this.mapToolTip?.SetToolTip(this.btnTownMapFloat, this.townMapDocker.IsFloating ? Localizer.T("元の場所へ戻す") : Localizer.T("この部分だけを別ウィンドウへ分離する"));
			}
			foreach (ToolPart part in this.toolParts.Where(p => p.Header != null))
			{
				part.Docker.UpdateHeader();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウを必要になった時点で作成する処理
		//-------------------------------------------------------------------------------
		private MapToolFloatForm EnsureToolFloatForm()
		{
			if (this.toolFloatForm != null && !this.toolFloatForm.IsDisposed)
			{
				return this.toolFloatForm;
			}
			this.toolFloatForm = new MapToolFloatForm();
			this.toolFloatForm.Owner = this;
			this.toolFloatForm.FormClosing += this.ToolFloatForm_FormClosing;
			this.toolFloatForm.Move += this.ToolFloatForm_Move;
			this.toolFloatForm.ResizeEnd += this.ToolFloatForm_ResizeEnd;
			this.toolFloatForm.Resize += this.ToolFloatForm_Resize;
			this.toolFloatForm.KeyDown += this.MapEditor_KeyDown;
			AppIconHelper.Apply(this.toolFloatForm);
			UiTheme.Apply(this.toolFloatForm);
			return this.toolFloatForm;
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウの×ボタンなどでは閉じずにドッキングへ戻す処理（アプリの終了時だけは閉じる）
		//-------------------------------------------------------------------------------
		private void ToolFloatForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.isToolPaneFloating && !ToolPartDocker.IsApplicationClosing(e.CloseReason))
			{
				e.Cancel = true;
				this.DockToolPane();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウの最小化時は隠して、F4 などで再表示できるようにする処理
		//-------------------------------------------------------------------------------
		private void ToolFloatForm_Resize(object sender, EventArgs e)
		{
			if (this.toolFloatForm.WindowState == FormWindowState.Minimized)
			{
				this.toolFloatForm.WindowState = FormWindowState.Normal;
				this.isToolPaneHiddenByUser = true;
				this.toolFloatForm.Hide();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウの移動中にドッキング候補の表示を更新する処理
		//-------------------------------------------------------------------------------
		private void ToolFloatForm_Move(object sender, EventArgs e)
		{
			if (!this.isToolPaneFloating || (Control.MouseButtons & MouseButtons.Left) == 0)
			{
				this.HideToolDockHint();
				return;
			}
			if (this.IsCursorInToolDockZone())
			{
				this.ShowToolDockHint();
			}
			else
			{
				this.HideToolDockHint();
			}
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウの移動終了時、右端に重なっていればドッキングする処理
		//-------------------------------------------------------------------------------
		private void ToolFloatForm_ResizeEnd(object sender, EventArgs e)
		{
			bool dock = this.toolDockHintForm != null && this.toolDockHintForm.Visible;
			this.HideToolDockHint();
			if (dock)
			{
				this.DockToolPane();
			}
			else if (this.toolFloatForm.WindowState == FormWindowState.Normal)
			{
				this.toolFloatBounds = this.toolFloatForm.Bounds;
			}
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウのタイトルバーを掴んだ状態にして OS 標準のドラッグ移動を始める処理
		//-------------------------------------------------------------------------------
		private void BeginToolFloatFormDrag()
		{
			if (this.toolFloatForm == null || !this.toolFloatForm.Visible)
			{
				return;
			}
			ReleaseCapture();
			SendMessage(this.toolFloatForm.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
		}

		//-------------------------------------------------------------------------------
		// マウスカーソルがメイン画面右端のドッキング帯に入っているか判定する処理
		//-------------------------------------------------------------------------------
		private bool IsCursorInToolDockZone()
		{
			if (!this.Visible || this.WindowState == FormWindowState.Minimized)
			{
				return false;
			}
			Rectangle work = this.splitWork.RectangleToScreen(this.splitWork.ClientRectangle);
			int zone = this.LogicalToDeviceUnits(ToolDockZoneWidth);
			Rectangle zoneRect = new Rectangle(work.Right - zone, work.Top, zone, work.Height);
			return zoneRect.Contains(Cursor.Position);
		}

		//-------------------------------------------------------------------------------
		// ドッキング先の範囲を半透明の枠で示す処理
		//-------------------------------------------------------------------------------
		private void ShowToolDockHint()
		{
			if (this.toolDockHintForm == null || this.toolDockHintForm.IsDisposed)
			{
				this.toolDockHintForm = new DockHintForm(UiTheme.Accent);
			}
			Rectangle work = this.splitWork.RectangleToScreen(this.splitWork.ClientRectangle);
			int width = Math.Min(work.Width / 2, Math.Max(this.LogicalToDeviceUnits(260), this.splitWork.Panel2.Width));
			this.toolDockHintForm.Bounds = new Rectangle(work.Right - width, work.Top, width, work.Height);
			if (!this.toolDockHintForm.Visible)
			{
				this.toolDockHintForm.Show(this);
			}
			if (this.toolFloatForm != null)
			{
				this.toolFloatForm.BringToFront();
			}
		}

		//-------------------------------------------------------------------------------
		// ドッキング先の表示を消す処理
		//-------------------------------------------------------------------------------
		private void HideToolDockHint()
		{
			if (this.toolDockHintForm != null && !this.toolDockHintForm.IsDisposed && this.toolDockHintForm.Visible)
			{
				this.toolDockHintForm.Hide();
			}
		}

		//-------------------------------------------------------------------------------
		// ウィンドウ位置が画面外にはみ出さないよう作業領域内へ収める処理
		//-------------------------------------------------------------------------------
		private Rectangle ClampToWorkingArea(Rectangle bounds)
		{
			Rectangle area = Screen.FromPoint(bounds.Location).WorkingArea;
			int width = Math.Min(bounds.Width, area.Width);
			int height = Math.Min(bounds.Height, area.Height);
			int x = Math.Max(area.Left, Math.Min(bounds.X, area.Right - width));
			int y = Math.Max(area.Top, Math.Min(bounds.Y, area.Bottom - height));
			return new Rectangle(x, y, width, height);
		}

		//-------------------------------------------------------------------------------
		// 分離用ウィンドウにフォーカスがあるか判定する処理（ショートカット用）
		//-------------------------------------------------------------------------------
		private bool IsToolFloatFormFocused()
		{
			return (this.toolFloatForm != null && !this.toolFloatForm.IsDisposed && this.toolFloatForm.ContainsFocus)
				|| (this.toolTabsDocker != null && this.toolTabsDocker.ContainsFocus)
				|| (this.townMapDocker != null && this.townMapDocker.ContainsFocus)
				|| this.toolParts.Any(p => p.Header != null && p.Docker.ContainsFocus);
		}

		//-------------------------------------------------------------------------------
		// フォーム終了時に分離用ウィンドウとドッキング表示を破棄する処理
		//-------------------------------------------------------------------------------
		private void DisposeToolFloatForm()
		{
			this.toolTabsDocker?.Dispose();
			this.toolTabsDocker = null;
			this.townMapDocker?.Dispose();
			this.townMapDocker = null;
			foreach (ToolPart part in this.toolParts.Where(p => p.Header != null))
			{
				part.Docker.Dispose();
			}
			if (this.toolDockHintForm != null && !this.toolDockHintForm.IsDisposed)
			{
				this.toolDockHintForm.Dispose();
			}
			this.toolDockHintForm = null;
			if (this.toolFloatForm != null && !this.toolFloatForm.IsDisposed)
			{
				this.toolFloatForm.FormClosing -= this.ToolFloatForm_FormClosing;
				this.toolFloatForm.ContentHost.Controls.Remove(this.pnlToolPane);
				this.toolFloatForm.Close();
				this.toolFloatForm.Dispose();
			}
			this.toolFloatForm = null;
		}

		//-------------------------------------------------------------------------------
		// F4（ツール欄の表示切替）、1/2/3（編集モード切替）、B/G/R（描画ツール切替）のショートカットを処理する処理
		//-------------------------------------------------------------------------------
		private bool TryProcessLayoutShortcut(Keys keyData)
		{
			if (keyData == Keys.F4)
			{
				this.ToggleToolPaneVisible();
				return true;
			}
			if ((keyData & Keys.Modifiers) != Keys.None || this.romData == null || this.IsTextInputFocused())
			{
				return false;
			}
			switch (keyData & Keys.KeyCode)
			{
				case Keys.D1:
				case Keys.NumPad1:
					this.SetEditorMode(this.tabBlock);
					return true;
				case Keys.D2:
				case Keys.NumPad2:
					this.SetEditorMode(this.tabCollision);
					return true;
				case Keys.D3:
				case Keys.NumPad3:
					this.SetEditorMode(this.tabEvent);
					return true;
				case Keys.B:
					this.SetPaintTool(MapPaintTool.Pen);
					return true;
				case Keys.G:
					this.SetPaintTool(MapPaintTool.Fill);
					return true;
				case Keys.R:
					this.SetPaintTool(MapPaintTool.Rectangle);
					return true;
				case Keys.P:
					this.SetPaintTool(MapPaintTool.Parts);
					return true;
				case Keys.Delete:
					// イベントのモードでは、選んでいるイベントを削除する
					if (this.tabMain.SelectedTab == this.tabMapEdit && this.tabEditorMode.SelectedTab == this.tabEvent)
					{
						return this.DeleteSelectedEvent();
					}
					return false;
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 文字や数値を入力する部品にフォーカスがあるか判定する処理
		//-------------------------------------------------------------------------------
		private bool IsTextInputFocused()
		{
			Control focused = this.FindFocusedControl(Form.ActiveForm);
			for (Control c = focused; c != null; c = c.Parent)
			{
				if (c is TextBoxBase || c is NumericUpDown || c is ComboBox)
				{
					return true;
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 指定コンテナ内で実際にフォーカスを持つ部品を探す処理
		//-------------------------------------------------------------------------------
		private Control FindFocusedControl(Control container)
		{
			Control current = container;
			while (current is ContainerControl containerControl && containerControl.ActiveControl != null)
			{
				current = containerControl.ActiveControl;
			}
			return current;
		}

		//-------------------------------------------------------------------------------
		// ドッキング先を示す半透明のオーバーレイ（フォーカスを奪わない）
		//-------------------------------------------------------------------------------
		internal sealed class DockHintForm : Form
		{
			//-------------------------------------------------------------------------------
			// オーバーレイの見た目を初期化する処理
			//-------------------------------------------------------------------------------
			public DockHintForm(Color color)
			{
				this.FormBorderStyle = FormBorderStyle.None;
				this.ShowInTaskbar = false;
				this.StartPosition = FormStartPosition.Manual;
				this.BackColor = color;
				this.Opacity = 0.35;
			}

			//-------------------------------------------------------------------------------
			// 表示時にアクティブ化しないようにする処理
			//-------------------------------------------------------------------------------
			protected override bool ShowWithoutActivation
			{
				get { return true; }
			}

			//-------------------------------------------------------------------------------
			// マウス操作を透過させ、アクティブ化もしないウィンドウスタイルを付ける処理
			//-------------------------------------------------------------------------------
			protected override CreateParams CreateParams
			{
				get
				{
					const int WS_EX_TRANSPARENT = 0x20;
					const int WS_EX_TOOLWINDOW = 0x80;
					const int WS_EX_NOACTIVATE = 0x08000000;
					CreateParams cp = base.CreateParams;
					cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
					return cp;
				}
			}
		}
	}
}
