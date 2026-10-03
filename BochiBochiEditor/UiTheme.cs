using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// アプリ全体の配色（ダークテーマ）をまとめて管理・適用するクラス
	// 色を変えたいときは下の色定義だけを書き換える
	//-------------------------------------------------------------------------------
	internal static class UiTheme
	{
		// ---- 色定義（濃紺ベース＋紫の強調色） ----
		public static readonly Color Window = Color.FromArgb(27, 30, 46);        // 最背面
		public static readonly Color Surface = Color.FromArgb(34, 38, 58);       // パネル・タブページ
		public static readonly Color Card = Color.FromArgb(42, 47, 72);          // グループ枠（カード）
		public static readonly Color Raised = Color.FromArgb(52, 58, 90);        // ボタン
		public static readonly Color RaisedHover = Color.FromArgb(64, 71, 110);  // ボタン（ホバー）
		public static readonly Color RaisedPressed = Color.FromArgb(46, 52, 82); // ボタン（押下）
		public static readonly Color Input = Color.FromArgb(24, 27, 41);         // 入力欄・ツリー
		public static readonly Color Canvas = Color.FromArgb(20, 23, 36);        // マップ・パレットの下地
		public static readonly Color Border = Color.FromArgb(60, 66, 100);       // 枠線
		public static readonly Color Text = Color.FromArgb(228, 230, 240);       // 通常文字
		public static readonly Color TextMuted = Color.FromArgb(154, 160, 191);  // 見出し・補足
		public static readonly Color Accent = Color.FromArgb(115, 103, 240);     // 強調（選択中）
		public static readonly Color AccentText = Color.White;
		public static readonly Color Warning = Color.FromArgb(255, 107, 129);    // 注意喚起の赤文字

		// カード（グループ枠）の角丸の半径（論理ピクセル）
		private const int CardRadius = 8;

		// ---- 記号フォント（Windows 10/11 標準の Segoe MDL2 Assets） ----
		public const string GlyphUndo = "";
		public const string GlyphRedo = "";
		public const string GlyphUndock = "";
		public const string GlyphDock = "";
		private static readonly Lazy<string> glyphFontName = new Lazy<string>(FindGlyphFont);

		// マップやパレットなど「絵を描く面」として扱う部品
		private static readonly ConditionalWeakTable<Control, object> canvasControls = new ConditionalWeakTable<Control, object>();

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		//-------------------------------------------------------------------------------
		// 指定した部品とその子部品すべてにテーマを適用する処理
		//-------------------------------------------------------------------------------
		public static void Apply(Control root)
		{
			ApplyOne(root);
			foreach (Control child in root.Controls)
			{
				Apply(child);
			}
			// 後から追加される部品にも自動で適用する
			root.ControlAdded -= Root_ControlAdded;
			root.ControlAdded += Root_ControlAdded;
		}

		//-------------------------------------------------------------------------------
		// 後から追加された子部品へテーマを適用する処理
		//-------------------------------------------------------------------------------
		private static void Root_ControlAdded(object sender, ControlEventArgs e)
		{
			Apply(e.Control);
		}

		//-------------------------------------------------------------------------------
		// 部品の種類ごとに配色を設定する処理
		//-------------------------------------------------------------------------------
		private static void ApplyOne(Control c)
		{
			if (canvasControls.TryGetValue(c, out _))
			{
				c.BackColor = Canvas;
				return;
			}
			switch (c)
			{
				case Form form:
					form.BackColor = Window;
					form.ForeColor = Text;
					WhenHandleReady(form, () => UseDarkTitleBar(form.Handle));
					break;
				case SplitContainer split:
					split.BackColor = Window;
					split.Panel1.BackColor = Surface;
					split.Panel2.BackColor = Surface;
					break;
				case TabControl tab:
					tab.BackColor = Surface;
					tab.ForeColor = Text;
					break;
				case TabPage page:
					page.UseVisualStyleBackColor = false;
					page.BackColor = Surface;
					page.ForeColor = Text;
					// 縦スクロールが出るタブ（イベントなど）は、スクロールバーも暗い配色にする
					if (page.AutoScroll)
					{
						TabPage scrollPage = page;
						WhenHandleReady(scrollPage, () => SetWindowTheme(scrollPage.Handle, "DarkMode_Explorer", null));
					}
					break;
				case GroupBox group:
					group.BackColor = Card;
					group.ForeColor = TextMuted;
					group.Paint -= GroupBox_Paint;
					group.Paint += GroupBox_Paint;
					break;
				case ImageButton:
					// 画像ボタンは自分で描くので、色の設定はしない
					break;
				case Button button:
					StyleButton(button, Raised, Text);
					break;
				case CheckBox check:
					check.ForeColor = Text;
					check.BackColor = Color.Transparent;
					break;
				case RadioButton radio:
					radio.ForeColor = Text;
					radio.BackColor = Color.Transparent;
					break;
				case Label label:
					label.ForeColor = IsWarningColor(label.ForeColor) ? Warning : Text;
					label.BackColor = Color.Transparent;
					break;
				case TextBoxBase textBox:
					textBox.BackColor = Input;
					textBox.ForeColor = Text;
					if (textBox.BorderStyle == BorderStyle.Fixed3D)
					{
						textBox.BorderStyle = BorderStyle.FixedSingle;
					}
					break;
				case NumericUpDown number:
					number.BackColor = Input;
					number.ForeColor = Text;
					number.BorderStyle = BorderStyle.FixedSingle;
					foreach (Control inner in number.Controls)
					{
						Control target = inner;
						WhenHandleReady(target, () => SetWindowTheme(target.Handle, "DarkMode_Explorer", null));
						// 上下ボタンは標準の描画だと明るい色のままなので、描かれた後に暗い配色で描き直す
						if (!(inner is TextBoxBase))
						{
							inner.Paint -= UpDownButtons_Paint;
							inner.Paint += UpDownButtons_Paint;
						}
					}
					break;
				case ComboBox combo:
					combo.BackColor = Input;
					combo.ForeColor = Text;
					combo.FlatStyle = FlatStyle.Standard;
					WhenHandleReady(combo, () => SetWindowTheme(combo.Handle, "DarkMode_CFD", null));
					break;
				case TreeView tree:
					tree.BackColor = Input;
					tree.ForeColor = Text;
					tree.LineColor = Border;
					tree.BorderStyle = BorderStyle.None;
					WhenHandleReady(tree, () => SetWindowTheme(tree.Handle, "DarkMode_Explorer", null));
					break;
				case ListBox list:
					list.BackColor = Input;
					list.ForeColor = Text;
					list.BorderStyle = BorderStyle.FixedSingle;
					WhenHandleReady(list, () => SetWindowTheme(list.Handle, "DarkMode_Explorer", null));
					break;
				case ScrollBar bar:
					WhenHandleReady(bar, () => SetWindowTheme(bar.Handle, "DarkMode_Explorer", null));
					break;
				case Panel panel:
					// カード（グループ枠）の中に置いた並べ用のパネルは、カードと同じ色にする
					panel.BackColor = (panel.Parent is GroupBox) ? Card : Surface;
					panel.ForeColor = Text;
					if (panel.AutoScroll)
					{
						WhenHandleReady(panel, () => SetWindowTheme(panel.Handle, "DarkMode_Explorer", null));
					}
					break;
				default:
					c.ForeColor = Text;
					break;
			}
		}

		//-------------------------------------------------------------------------------
		// 数値入力（NumericUpDown）の上下ボタンを、暗い配色で描き直す処理
		// 標準の描画の後に呼ばれるので、上半分（▲）と下半分（▼）を塗り直して三角を描く。マウスを乗せた側・押した側は色を変える
		//-------------------------------------------------------------------------------
		private static void UpDownButtons_Paint(object sender, PaintEventArgs e)
		{
			Control buttons = (Control)sender;
			Graphics g = e.Graphics;
			Rectangle area = buttons.ClientRectangle;
			if (area.Width <= 0 || area.Height <= 1)
			{
				return;
			}
			bool enabled = buttons.Enabled && (buttons.Parent == null || buttons.Parent.Enabled);
			Point mouse = buttons.PointToClient(Cursor.Position);
			bool pressed = (Control.MouseButtons & MouseButtons.Left) != 0;
			int half = area.Height / 2;
			Rectangle[] halves = { new Rectangle(area.X, area.Y, area.Width, half), new Rectangle(area.X, area.Y + half, area.Width, area.Height - half) };
			for (int i = 0; i < halves.Length; i++)
			{
				Rectangle r = halves[i];
				bool hot = enabled && r.Contains(mouse);
				Color back = !enabled ? Card : (hot ? (pressed ? RaisedPressed : RaisedHover) : Raised);
				using (SolidBrush brush = new SolidBrush(back))
				{
					g.FillRectangle(brush, r);
				}
				// 三角（上向き／下向き）
				int size = Math.Max(2, Math.Min(r.Width, r.Height) / 3);
				int cx = r.X + r.Width / 2;
				int cy = r.Y + r.Height / 2;
				Point[] triangle = i == 0
					? new[] { new Point(cx - size, cy + size / 2), new Point(cx + size, cy + size / 2), new Point(cx, cy - size / 2 - 1) }
					: new[] { new Point(cx - size, cy - size / 2), new Point(cx + size, cy - size / 2), new Point(cx, cy + size / 2 + 1) };
				using (SolidBrush arrow = new SolidBrush(enabled ? Text : ControlPaint.Dark(TextMuted, 0.2f)))
				{
					g.FillPolygon(arrow, triangle);
				}
			}
			using (Pen pen = new Pen(Border))
			{
				g.DrawLine(pen, area.X, area.Y + half, area.Right, area.Y + half);
				g.DrawLine(pen, area.X, area.Y, area.X, area.Bottom);
			}
		}

		//-------------------------------------------------------------------------------
		// 指定部品を「絵を描く面」として登録し、下地色を設定する処理
		//-------------------------------------------------------------------------------
		public static void MarkCanvas(Control control)
		{
			if (control == null)
			{
				return;
			}
			canvasControls.AddOrUpdate(control, null);
			control.BackColor = Canvas;
		}

		//-------------------------------------------------------------------------------
		// ボタンをフラットな見た目に揃える処理
		//-------------------------------------------------------------------------------
		private static void StyleButton(Button button, Color back, Color fore)
		{
			button.UseVisualStyleBackColor = false;
			button.FlatStyle = FlatStyle.Flat;
			enabledColors.AddOrUpdate(button, new ButtonColors { Back = back, Fore = fore });
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.MouseOverBackColor = back == Accent ? ControlPaint.Light(Accent, 0.15f) : RaisedHover;
			button.FlatAppearance.MouseDownBackColor = back == Accent ? ControlPaint.Dark(Accent, 0.05f) : RaisedPressed;
			button.EnabledChanged -= Button_EnabledChanged;
			button.EnabledChanged += Button_EnabledChanged;
			ApplyButtonEnabledLook(button);
		}

		// 押せる状態のときのボタンの色（押せない状態から戻すときに使う）
		private sealed class ButtonColors
		{
			public Color Back;
			public Color Fore;
		}

		private static readonly ConditionalWeakTable<Button, ButtonColors> enabledColors = new ConditionalWeakTable<Button, ButtonColors>();

		//-------------------------------------------------------------------------------
		// ボタンの押せる／押せないが変わったら見た目を合わせる処理
		//-------------------------------------------------------------------------------
		private static void Button_EnabledChanged(object sender, EventArgs e)
		{
			ApplyButtonEnabledLook((Button)sender);
		}

		//-------------------------------------------------------------------------------
		// 押せないボタンは背景を周りと同化させ、枠線も薄くして、押せるボタンと区別できるようにする処理
		//-------------------------------------------------------------------------------
		private static void ApplyButtonEnabledLook(Button button)
		{
			ButtonColors colors;
			if (!enabledColors.TryGetValue(button, out colors))
			{
				return;
			}
			if (button.Enabled)
			{
				button.BackColor = colors.Back;
				button.ForeColor = colors.Fore;
				button.FlatAppearance.BorderColor = colors.Back == Accent ? Accent : Border;
			}
			else
			{
				Color surroundings = button.Parent != null ? button.Parent.BackColor : Surface;
				button.BackColor = surroundings;
				button.ForeColor = ControlPaint.Dark(TextMuted, 0.1f);
				button.FlatAppearance.BorderColor = Color.FromArgb(
					(Border.R + surroundings.R) / 2, (Border.G + surroundings.G) / 2, (Border.B + surroundings.B) / 2);
			}
		}

		//-------------------------------------------------------------------------------
		// 切替ボタン（編集モードなど）の選択状態を色で示す処理
		//-------------------------------------------------------------------------------
		public static void SetToggleButtonState(Button button, bool selected)
		{
			if (button == null)
			{
				return;
			}
			StyleButton(button, selected ? Accent : Raised, selected ? AccentText : Text);
		}

		//-------------------------------------------------------------------------------
		// ボタンに記号フォントのアイコンを設定する処理（フォントが無ければ文字で代用）
		//-------------------------------------------------------------------------------
		public static void SetGlyph(Button button, string glyph, string fallbackText)
		{
			if (button == null)
			{
				return;
			}
			string fontName = glyphFontName.Value;
			if (fontName == null)
			{
				button.Text = fallbackText;
				return;
			}
			if (button.Font == null || button.Font.Name != fontName)
			{
				button.Font = new Font(fontName, 10.5f, FontStyle.Regular, GraphicsUnit.Point);
			}
			button.Text = glyph;
		}

		//-------------------------------------------------------------------------------
		// 使用可能な記号フォントを探す処理
		//-------------------------------------------------------------------------------
		private static string FindGlyphFont()
		{
			string[] candidates = { "Segoe Fluent Icons", "Segoe MDL2 Assets" };
			foreach (string name in candidates)
			{
				if (FontFamily.Families.Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)))
				{
					return name;
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// グループ枠を角丸のカードと見出し文字で描き直す処理
		//-------------------------------------------------------------------------------
		private static void GroupBox_Paint(object sender, PaintEventArgs e)
		{
			GroupBox group = (GroupBox)sender;
			Graphics g = e.Graphics;
			Color outside = group.Parent != null ? group.Parent.BackColor : Window;
			g.Clear(outside);
			Size textSize = TextRenderer.MeasureText(g, group.Text, group.Font, Size.Empty, TextFormatFlags.NoPadding);
			int top = string.IsNullOrEmpty(group.Text) ? 0 : textSize.Height / 2;
			Rectangle card = new Rectangle(0, top, group.Width - 1, group.Height - top - 1);
			int radius = group.LogicalToDeviceUnits(CardRadius);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = CreateRoundRect(card, radius))
			using (SolidBrush fill = new SolidBrush(group.BackColor))
			using (Pen pen = new Pen(Border))
			{
				g.FillPath(fill, path);
				g.DrawPath(pen, path);
			}
			g.SmoothingMode = SmoothingMode.Default;
			if (!string.IsNullOrEmpty(group.Text))
			{
				Rectangle textRect = new Rectangle(radius + 2, 0, textSize.Width + 6, textSize.Height);
				using (GraphicsPath pill = CreateRoundRect(textRect, textSize.Height / 2))
				using (SolidBrush back = new SolidBrush(group.BackColor))
				{
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.FillPath(back, pill);
					g.SmoothingMode = SmoothingMode.Default;
				}
				Color color = group.Enabled ? TextMuted : ControlPaint.Dark(TextMuted, 0.2f);
				TextRenderer.DrawText(g, group.Text, group.Font, new Point(textRect.X + 3, 0), color, TextFormatFlags.NoPadding);
			}
		}

		//-------------------------------------------------------------------------------
		// 角丸四角形の図形を作る処理
		//-------------------------------------------------------------------------------
		public static GraphicsPath CreateRoundRect(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
			path.AddArc(r.X, r.Y, d, d, 180, 90);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
			path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
			path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		//-------------------------------------------------------------------------------
		// ToolStrip（ステータスバーなど）を背景色だけで平坦に描く描画クラス
		//-------------------------------------------------------------------------------
		internal sealed class FlatToolStripRenderer : ToolStripProfessionalRenderer
		{
			//-------------------------------------------------------------------------------
			// 背景を最背面の色で塗り、上端に境界線を引く処理
			//-------------------------------------------------------------------------------
			protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
			{
				using (SolidBrush brush = new SolidBrush(Window))
				{
					e.Graphics.FillRectangle(brush, e.AffectedBounds);
				}
				using (Pen pen = new Pen(Border))
				{
					e.Graphics.DrawLine(pen, 0, 0, e.ToolStrip.Width, 0);
				}
			}

			//-------------------------------------------------------------------------------
			// 既定の立体的な枠線を描かないようにする処理
			//-------------------------------------------------------------------------------
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
			}

			//-------------------------------------------------------------------------------
			// 項目の文字色をテーマに合わせて描く処理
			//-------------------------------------------------------------------------------
			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				if (e.Item.ForeColor == SystemColors.ControlText)
				{
					e.TextColor = TextMuted;
				}
				base.OnRenderItemText(e);
			}
		}

		//-------------------------------------------------------------------------------
		// 既存画面で赤文字にしている注意喚起ラベルかどうか判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsWarningColor(Color color)
		{
			return color.R > 180 && color.G < 90 && color.B < 90;
		}

		//-------------------------------------------------------------------------------
		// タイトルバーをダーク表示にする処理（Windows 10 20H1 以降）
		//-------------------------------------------------------------------------------
		private static void UseDarkTitleBar(IntPtr handle)
		{
			int enabled = 1;
			if (DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int)) != 0)
			{
				DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
			}
			// Windows 11 ではタイトルバーの色を背景色と揃える（未対応の OS では何も起きない）
			int caption = ColorTranslator.ToWin32(Window);
			DwmSetWindowAttribute(handle, 35, ref caption, sizeof(int));
			int border = ColorTranslator.ToWin32(Window);
			DwmSetWindowAttribute(handle, 34, ref border, sizeof(int));
		}

		//-------------------------------------------------------------------------------
		// ウィンドウハンドル作成後に処理を実行する（作成済みなら即実行）処理
		//-------------------------------------------------------------------------------
		private static void WhenHandleReady(Control control, Action action)
		{
			if (control.IsHandleCreated)
			{
				action();
				return;
			}
			EventHandler handler = null;
			handler = (s, e) =>
			{
				control.HandleCreated -= handler;
				action();
			};
			control.HandleCreated += handler;
		}
	}

	//-------------------------------------------------------------------------------
	// ダークテーマ用に見出しと枠を自前で描画するタブコントロール
	//-------------------------------------------------------------------------------
	public class ThemedTabControl : TabControl
	{
		//-------------------------------------------------------------------------------
		// 自前描画を有効にする処理
		//-------------------------------------------------------------------------------
		public ThemedTabControl()
		{
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

		//-------------------------------------------------------------------------------
		// タブ見出し・選択中の強調線・ページ枠を描画する処理
		//-------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(UiTheme.Window);
			if (this.TabCount == 0)
			{
				return;
			}
			Rectangle page = this.DisplayRectangle;
			page.Inflate(2, 2);
			using (SolidBrush surface = new SolidBrush(UiTheme.Surface))
			{
				g.FillRectangle(surface, page);
			}
			for (int i = 0; i < this.TabCount; i++)
			{
				Rectangle r = this.GetTabRect(i);
				bool selected = i == this.SelectedIndex;
				using (SolidBrush brush = new SolidBrush(selected ? UiTheme.Surface : UiTheme.Window))
				{
					g.FillRectangle(brush, r);
				}
				if (selected)
				{
					using (SolidBrush accent = new SolidBrush(UiTheme.Accent))
					{
						g.FillRectangle(accent, new Rectangle(r.Left, r.Top, r.Width, this.LogicalToDeviceUnits(2)));
					}
				}
				Color textColor = !this.Enabled ? ControlPaint.Dark(UiTheme.TextMuted, 0.2f) : (selected ? UiTheme.Text : UiTheme.TextMuted);
				TextRenderer.DrawText(g, this.TabPages[i].Text, this.Font, r, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
			}
		}
	}
}
