using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 入口画面（ROM を読み込んでいないときにメイン画面へ重ねて出す）
	//   左: ロゴ・「ROM を開く…」・最近開いた ROM の一覧（ドロップでも開ける）
	//   右: ヘルプ（README・リリースノート・docs・resource）と、このツールについて
	// 全部を自分で描き、クリックできる所は「当たり判定」の一覧で持つ。ROM や設定は触らず、
	// 開く・一覧から外す・一覧を消すは持ち主（マップエディタ）へイベントで知らせる
	//-------------------------------------------------------------------------------
	internal sealed class StartPage : Control
	{
		// 最近開いた ROM の 1 件（表示用。中身は開くときに読む）
		public sealed class RecentEntry
		{
			public string Path;
			public bool Exists;
			// ゲームの表示名（判別できなければ ROM の題名、読めなければ空）
			public string GameName;
			// バッジの絵のファイル名（img の下）
			public string IconFile;
		}

		// クリックできる所
		private sealed class HotSpot
		{
			public Rectangle Bounds;
			public Action Click;
			public bool Enabled = true;
			public string ToolTip;
		}

		// 「ROM を開く…」が押されたとき
		public event EventHandler OpenRequested;
		// 最近開いた ROM（またはドロップした ROM）が選ばれたとき
		public event EventHandler<string> RomSelected;
		// 最近開いた ROM の一覧から 1 件外す・全部消すとき（null なら全部）
		public event EventHandler<string> RemoveRecentRequested;
		// 「編集に戻る」が押されたとき（ROM を読んだままホームを出しているとき）
		public event EventHandler ResumeRequested;

		// 読み込んでいる ROM の名前（null なら読んでいない。読んでいれば「編集に戻る」を出す）
		private string currentRom;
		public string CurrentRom
		{
			get { return this.currentRom; }
			set
			{
				this.currentRom = value;
				this.hovering = null;
				this.Invalidate();
			}
		}

		private readonly List<RecentEntry> recent = new List<RecentEntry>();
		private readonly List<HotSpot> hotSpots = new List<HotSpot>();
		private readonly Dictionary<string, Image> icons = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
		private readonly ToolTip toolTip = new ToolTip { InitialDelay = 400, ReshowDelay = 100 };
		private Image logo;
		// ヘルプの各項目の左に出す印（Bochi のアイコン）
		private Image helpIcon;
		private HotSpot hovering;
		private Font fontBig;
		private Font fontHeading;
		private Font fontSmall;
		private Font fontTitle;

		//-------------------------------------------------------------------------------
		// ちらつきを抑える設定と、ドロップの受け付け
		//-------------------------------------------------------------------------------
		public StartPage()
		{
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			this.AllowDrop = true;
			this.BackColor = UiTheme.Window;
			this.ForeColor = UiTheme.Text;
			this.logo = LoadImage("Logo.png");
			this.helpIcon = LoadImage("Bochi_icon.png");
			if (this.logo is Bitmap bitmap && bitmap.Width > 0 && bitmap.Height > 0)
			{
				// ロゴの地（左上の 1 ピクセルの色）は透明にして、画面の背景に溶け込ませる
				bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
			}
			// 言語が切り替わったら、文字を描き直す（ゲーム名も今の言語で読み直す）
			Localizer.LanguageChanged += this.Localizer_LanguageChanged;
		}

		//-------------------------------------------------------------------------------
		// 言語が切り替わったときの処理（描いている文字はすべて描くときに訳すので、描き直すだけでよい）
		//-------------------------------------------------------------------------------
		private void Localizer_LanguageChanged(object sender, EventArgs e)
		{
			foreach (RecentEntry entry in this.recent)
			{
				if (entry.Exists)
				{
					ReadGameInfo(entry);
				}
			}
			this.hovering = null;
			this.toolTip.SetToolTip(this, null);
			this.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 最近開いた ROM の一覧を入れ替える処理（ファイルの有無とゲームの種類は、ここで読む）
		//-------------------------------------------------------------------------------
		public void SetRecent(IEnumerable<string> paths)
		{
			this.recent.Clear();
			foreach (string path in paths ?? Enumerable.Empty<string>())
			{
				if (string.IsNullOrWhiteSpace(path))
				{
					continue;
				}
				RecentEntry entry = new RecentEntry { Path = path, Exists = File.Exists(path), GameName = string.Empty, IconFile = "gba_Small_icon.png" };
				if (entry.Exists)
				{
					ReadGameInfo(entry);
				}
				this.recent.Add(entry);
			}
			this.hovering = null;
			this.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// ROM の先頭だけを読んで、ゲームの種類（表示名とバッジの絵）を決める処理。読めなくても例外にしない
		//-------------------------------------------------------------------------------
		private static void ReadGameInfo(RecentEntry entry)
		{
			try
			{
				byte[] head = new byte[0xC0];
				using (FileStream stream = new FileStream(entry.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					int read = 0;
					while (read < head.Length)
					{
						int n = stream.Read(head, read, head.Length - read);
						if (n <= 0)
						{
							break;
						}
						read += n;
					}
				}
				GameProfile profile = GameProfile.Detect(head);
				string code = GameProfile.ReadGameCode(head);
				entry.GameName = profile != null ? Localizer.T(profile.DisplayName) : GameProfile.ReadTitle(head);
				entry.IconFile = code.StartsWith("BPR", StringComparison.Ordinal) ? "FR_icon.png"
					: (code.StartsWith("BPE", StringComparison.Ordinal) ? "EM_icon.png" : "gba_Small_icon.png");
			}
			catch (Exception)
			{
				// 読めない ROM は名前なしのまま出す
			}
		}

		//-------------------------------------------------------------------------------
		// img の下の絵を読む処理（無ければ null）
		//-------------------------------------------------------------------------------
		private static Image LoadImage(string file)
		{
			try
			{
				string path = AppAssetLocator.GetPathOrDefault(Path.Combine("img", file));
				if (path != null && File.Exists(path))
				{
					using (Bitmap loaded = new Bitmap(path))
					{
						return new Bitmap(loaded);
					}
				}
			}
			catch (Exception)
			{
				// 絵が無くても画面は出す
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// バッジの絵を返す処理（1 度読んだら持っておく）
		//-------------------------------------------------------------------------------
		private Image GetIcon(string file)
		{
			Image image;
			if (!this.icons.TryGetValue(file, out image))
			{
				image = LoadImage(file);
				this.icons[file] = image;
			}
			return image;
		}

		//-------------------------------------------------------------------------------
		// 文字の大きさごとのフォントを作る処理（画面のフォントが変わったら作り直す）
		//-------------------------------------------------------------------------------
		private void EnsureFonts()
		{
			if (this.fontBig != null)
			{
				return;
			}
			FontFamily family = this.Font.FontFamily;
			this.fontBig = new Font(family, 17f, FontStyle.Regular);
			this.fontTitle = new Font(family, 12f, FontStyle.Bold);
			this.fontHeading = new Font(family, 10f, FontStyle.Bold);
			this.fontSmall = new Font(family, Math.Max(7f, this.Font.SizeInPoints - 1f), FontStyle.Regular);
		}

		//-------------------------------------------------------------------------------
		// フォントが変わったら、作ってあったフォントを捨てる処理
		//-------------------------------------------------------------------------------
		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			this.DisposeFonts();
			this.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 作ったフォントを捨てる処理
		//-------------------------------------------------------------------------------
		private void DisposeFonts()
		{
			this.fontBig?.Dispose();
			this.fontTitle?.Dispose();
			this.fontHeading?.Dispose();
			this.fontSmall?.Dispose();
			this.fontBig = this.fontTitle = this.fontHeading = this.fontSmall = null;
		}

		//-------------------------------------------------------------------------------
		// 持っている絵・フォントを捨てる処理
		//-------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Localizer.LanguageChanged -= this.Localizer_LanguageChanged;
				this.DisposeFonts();
				this.logo?.Dispose();
				this.helpIcon?.Dispose();
				foreach (Image image in this.icons.Values)
				{
					image?.Dispose();
				}
				this.icons.Clear();
				this.toolTip.Dispose();
			}
			base.Dispose(disposing);
		}

		//-------------------------------------------------------------------------------
		// 画面を描く処理（描きながら、クリックできる所の一覧を作り直す）
		//-------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			this.EnsureFonts();
			this.hotSpots.Clear();
			Graphics g = e.Graphics;
			g.Clear(UiTheme.Window);
			this.DrawHexPattern(g);
			float s = this.DeviceDpi / 96f;
			int pad = (int)(40 * s);
			int gap = (int)(28 * s);
			Rectangle area = Rectangle.Inflate(this.ClientRectangle, -pad, -pad);
			if (area.Width < 10 || area.Height < 10)
			{
				return;
			}
			int leftWidth = Math.Max((int)(380 * s), area.Width * 54 / 100);
			Rectangle left = new Rectangle(area.Left, area.Top, leftWidth, area.Height);
			Rectangle right = new Rectangle(left.Right + gap, area.Top, Math.Max(0, area.Right - left.Right - gap), area.Height);
			this.DrawLeftColumn(g, left, s);
			if (right.Width > (int)(200 * s))
			{
				this.DrawRightColumn(g, right, s);
			}
		}

		//-------------------------------------------------------------------------------
		// 背景の六角形の模様を描く処理（薄い線。画面の雰囲気づくりだけ）
		//-------------------------------------------------------------------------------
		private void DrawHexPattern(Graphics g)
		{
			float s = this.DeviceDpi / 96f;
			float r = 16f * s;
			float w = (float)(Math.Sqrt(3) * r);
			float h = 1.5f * r;
			using (Pen pen = new Pen(Color.FromArgb(28, UiTheme.TextMuted), 1f))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				int rows = (int)(this.ClientSize.Height / h) + 2;
				int cols = (int)(this.ClientSize.Width / w) + 2;
				PointF[] hex = new PointF[6];
				for (int row = -1; row < rows; row++)
				{
					float cy = row * h;
					float offset = (row & 1) == 0 ? 0f : w / 2f;
					for (int col = -1; col < cols; col++)
					{
						float cx = col * w + offset;
						for (int k = 0; k < 6; k++)
						{
							double angle = Math.PI / 180.0 * (60 * k - 30);
							hex[k] = new PointF(cx + (float)(r * Math.Cos(angle)), cy + (float)(r * Math.Sin(angle)));
						}
						g.DrawPolygon(pen, hex);
					}
				}
				g.SmoothingMode = SmoothingMode.Default;
			}
		}

		//-------------------------------------------------------------------------------
		// 左の列（ロゴ・開く・最近開いた ROM）を描く処理
		//-------------------------------------------------------------------------------
		private void DrawLeftColumn(Graphics g, Rectangle area, float s)
		{
			int y = area.Top;
			// ロゴ（列の幅いっぱい。元の絵の 2 倍までで、高さは画面の 3 割まで）
			if (this.logo != null)
			{
				int logoWidth = Math.Min(area.Width, (int)(this.logo.Width * 2 * s));
				int maxHeight = Math.Max(this.logo.Height, this.ClientSize.Height * 3 / 10);
				if (logoWidth * this.logo.Height / this.logo.Width > maxHeight)
				{
					logoWidth = maxHeight * this.logo.Width / this.logo.Height;
				}
				int logoHeight = logoWidth * this.logo.Height / this.logo.Width;
				// ちょうど整数倍ならドット絵のまま（最近傍）、それ以外はなめらかに拡大する
				bool integer = logoWidth % this.logo.Width == 0;
				g.InterpolationMode = integer ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.DrawImage(this.logo, new Rectangle(area.Left, y, logoWidth, logoHeight));
				g.PixelOffsetMode = PixelOffsetMode.Default;
				g.InterpolationMode = InterpolationMode.Default;
				y += logoHeight + (int)(10 * s);
			}
			else
			{
				TextRenderer.DrawText(g, "BochiBochiEditor", this.fontTitle, new Point(area.Left, y), UiTheme.Text);
				y += this.fontTitle.Height + (int)(6 * s);
			}
			TextRenderer.DrawText(g, Localizer.T("ポケモン（ファイアレッド・エメラルド）の GBA ROM のマップを編集するツール"), this.Font, new Rectangle(area.Left, y, area.Width, this.Font.Height * 2), UiTheme.TextMuted, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			y += this.Font.Height * 2 + (int)(18 * s);

			// 「ROM を開く…」
			string open = Localizer.T("ROM を開く…");
			Size openSize = TextRenderer.MeasureText(g, open, this.fontBig);
			Rectangle openBounds = new Rectangle(area.Left, y, openSize.Width, openSize.Height);
			this.DrawLink(g, open, this.fontBig, openBounds, true, () => this.OpenRequested?.Invoke(this, EventArgs.Empty), Localizer.T("GBA の ROM（.gba）を選んで開きます"));
			y += openSize.Height + (int)(2 * s);
			TextRenderer.DrawText(g, Localizer.T("この画面へ .gba ファイルをドロップしても開けます"), this.fontSmall, new Point(area.Left, y), UiTheme.TextMuted);
			y += this.fontSmall.Height + (int)(12 * s);
			// ROM を読んだままホームを出しているときは、「編集に戻る」
			if (this.currentRom != null)
			{
				string resume = Localizer.F("編集に戻る（{0}）", this.currentRom);
				Size resumeSize = TextRenderer.MeasureText(g, resume, this.fontHeading);
				this.DrawLink(g, resume, this.fontHeading, new Rectangle(area.Left, y, resumeSize.Width, resumeSize.Height), true, () => this.ResumeRequested?.Invoke(this, EventArgs.Empty), Localizer.T("読み込んでいる ROM の編集画面に戻ります（上の「ホーム」でまたここへ来られます）"));
				y += resumeSize.Height + (int)(2 * s);
			}
			y += (int)(14 * s);

			// 最近開いた ROM
			string heading = Localizer.T("最近開いた ROM");
			TextRenderer.DrawText(g, heading, this.fontHeading, new Point(area.Left, y), UiTheme.TextMuted);
			if (this.recent.Count > 0)
			{
				string clear = Localizer.T("一覧を消す");
				Size clearSize = TextRenderer.MeasureText(g, clear, this.fontSmall);
				Rectangle clearBounds = new Rectangle(area.Right - clearSize.Width, y + (this.fontHeading.Height - clearSize.Height) / 2, clearSize.Width, clearSize.Height);
				this.DrawLink(g, clear, this.fontSmall, clearBounds, true, () => this.RemoveRecentRequested?.Invoke(this, null), Localizer.T("最近開いた ROM の一覧を空にします（ファイルは消しません）"));
			}
			y += this.fontHeading.Height + (int)(6 * s);
			using (Pen line = new Pen(UiTheme.Border))
			{
				g.DrawLine(line, area.Left, y, area.Right, y);
			}
			y += (int)(6 * s);
			if (this.recent.Count == 0)
			{
				TextRenderer.DrawText(g, Localizer.T("まだありません。ROM を開くと、ここに並びます。"), this.Font, new Point(area.Left, y + (int)(4 * s)), UiTheme.TextMuted);
				return;
			}
			int rowHeight = (int)(44 * s);
			foreach (RecentEntry entry in this.recent)
			{
				if (y + rowHeight > area.Bottom)
				{
					break;
				}
				this.DrawRecentRow(g, entry, new Rectangle(area.Left, y, area.Width, rowHeight), s);
				y += rowHeight + (int)(2 * s);
			}
		}

		//-------------------------------------------------------------------------------
		// 最近開いた ROM の 1 行（バッジ・ファイル名・ゲーム名・フォルダ）を描く処理
		//-------------------------------------------------------------------------------
		private void DrawRecentRow(Graphics g, RecentEntry entry, Rectangle bounds, float s)
		{
			string path = entry.Path;
			HotSpot spot = new HotSpot
			{
				Bounds = bounds,
				Enabled = true,
				Click = () => this.RomSelected?.Invoke(this, path),
				ToolTip = entry.Exists ? path : Localizer.F("{0}\n（ファイルが見つかりません。クリックすると一覧から外せます）", path),
			};
			this.hotSpots.Add(spot);
			bool hover = this.hovering != null && this.hovering.Bounds == bounds;
			if (hover)
			{
				using (SolidBrush fill = new SolidBrush(UiTheme.Raised))
				{
					g.FillRectangle(fill, bounds);
				}
			}
			int inset = (int)(8 * s);
			int iconSize = (int)(26 * s);
			Rectangle iconBounds = new Rectangle(bounds.Left + inset, bounds.Top + (bounds.Height - iconSize) / 2, iconSize, iconSize);
			Image icon = this.GetIcon(entry.IconFile);
			if (icon != null)
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				int iconWidth = iconSize * icon.Width / Math.Max(1, icon.Height);
				g.DrawImage(icon, new Rectangle(iconBounds.Left, iconBounds.Top, Math.Min(iconWidth, iconSize * 2), iconSize));
				g.InterpolationMode = InterpolationMode.Default;
			}
			int textLeft = iconBounds.Right + inset + (int)(6 * s);
			int textWidth = Math.Max(10, bounds.Right - inset - textLeft);
			Color nameColor = entry.Exists ? (hover ? UiTheme.AccentText : UiTheme.Text) : UiTheme.TextMuted;
			string name = Path.GetFileName(path) + (entry.Exists ? string.Empty : Localizer.T("（見つかりません）"));
			string game = entry.Exists && !string.IsNullOrEmpty(entry.GameName) ? "  " + entry.GameName : string.Empty;
			Rectangle nameBounds = new Rectangle(textLeft, bounds.Top + (int)(5 * s), textWidth, this.Font.Height);
			Size nameSize = TextRenderer.MeasureText(g, name, this.Font);
			TextRenderer.DrawText(g, name, this.Font, nameBounds, nameColor, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			if (game.Length > 0 && nameSize.Width + (int)(40 * s) < textWidth)
			{
				TextRenderer.DrawText(g, game, this.fontSmall, new Rectangle(textLeft + nameSize.Width, nameBounds.Top + 1, textWidth - nameSize.Width, this.fontSmall.Height), UiTheme.TextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			}
			string folder = Path.GetDirectoryName(path) ?? string.Empty;
			TextRenderer.DrawText(g, folder, this.fontSmall, new Rectangle(textLeft, nameBounds.Bottom + (int)(2 * s), textWidth, this.fontSmall.Height), UiTheme.TextMuted, TextFormatFlags.Left | TextFormatFlags.PathEllipsis | TextFormatFlags.NoPadding);
		}

		//-------------------------------------------------------------------------------
		// 右の列（ヘルプ・このツールについて）を描く処理
		//-------------------------------------------------------------------------------
		private void DrawRightColumn(Graphics g, Rectangle area, float s)
		{
			int y = area.Top;
			// ヘルプ
			List<Tuple<string, string, string>> docs = new List<Tuple<string, string, string>>
			{
				Tuple.Create(Localizer.T("README（使い方）"), Localizer.T("各機能の使い方と、配布フォルダの構成。"), FindDocument("README.md")),
				Tuple.Create(Localizer.T("リリースノート"), Localizer.T("今の版で足したもの・直したもの・既知の制限。"), FindNewestDocument("リリースノート_*.md")),
				Tuple.Create(Localizer.T("docs フォルダ"), Localizer.T("引き継ぎ書・調査メモ（開発者向け）。"), FindFolder("docs")),
				Tuple.Create(Localizer.T("resource フォルダ"), Localizer.T("ini（アドレスの定義）・lang（英語表示）・mapassist（パーツの指定）。"), AppAssetLocator.ResourceDirectory),
			};
			int cardHeight = (int)(14 * s) + this.fontHeading.Height + (int)(8 * s) + docs.Count * ((int)(46 * s)) + (int)(10 * s);
			Rectangle help = new Rectangle(area.Left, y, area.Width, cardHeight);
			this.DrawCard(g, help, Localizer.T("ヘルプ"), s);
			int rowTop = help.Top + (int)(14 * s) + this.fontHeading.Height + (int)(8 * s);
			// 各項目の左に Bochi のアイコンを出し、文字はその右に寄せる
			int iconSize = this.helpIcon != null ? (int)(36 * s) : 0;
			int textOffset = this.helpIcon != null ? iconSize + (int)(10 * s) : 0;
			foreach (Tuple<string, string, string> doc in docs)
			{
				Rectangle row = new Rectangle(help.Left + (int)(12 * s), rowTop, help.Width - (int)(24 * s), (int)(44 * s));
				string target = doc.Item3;
				bool exists = !string.IsNullOrEmpty(target) && (File.Exists(target) || Directory.Exists(target));
				if (this.helpIcon != null)
				{
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.DrawImage(this.helpIcon, new Rectangle(row.Left, row.Top + (int)(2 * s), iconSize, iconSize));
					g.InterpolationMode = InterpolationMode.Default;
				}
				Size titleSize = TextRenderer.MeasureText(g, doc.Item1, this.Font);
				this.DrawLink(g, doc.Item1, this.Font, new Rectangle(row.Left + textOffset, row.Top, titleSize.Width, titleSize.Height), exists, () => OpenDocument(target), exists ? target : Localizer.T("見つかりません"));
				TextRenderer.DrawText(g, doc.Item2, this.fontSmall, new Rectangle(row.Left + textOffset, row.Top + titleSize.Height + (int)(2 * s), row.Width - textOffset, this.fontSmall.Height), UiTheme.TextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				rowTop += (int)(46 * s);
			}
			y = help.Bottom + (int)(16 * s);

			// このツールについて
			List<string> lines = new List<string>
			{
				Localizer.F("バージョン {0}（{1}）", GetVersionText(), GetBuildDateText()),
				Localizer.T("作者: C2H4"),
				Localizer.T("個人が作った非営利のツールです。ポケットモンスターは任天堂・クリーチャーズ・ゲームフリークの著作物です。"),
				Localizer.T("正規に入手したソフトから自分で吸い出した ROM のコピーでお使いください。ROM の配布はしないでください。"),
			};
			int lineGap = (int)(4 * s);
			int textWidth = area.Width - (int)(24 * s);
			int aboutHeight = (int)(14 * s) + this.fontHeading.Height + (int)(8 * s) + (int)(12 * s);
			List<int> heights = new List<int>();
			foreach (string line in lines)
			{
				int h = TextRenderer.MeasureText(g, line, this.Font, new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
				heights.Add(h);
				aboutHeight += h + lineGap;
			}
			if (y + aboutHeight > area.Bottom)
			{
				return;
			}
			Rectangle about = new Rectangle(area.Left, y, area.Width, aboutHeight);
			this.DrawCard(g, about, Localizer.T("このツールについて"), s);
			int top = about.Top + (int)(14 * s) + this.fontHeading.Height + (int)(8 * s);
			for (int i = 0; i < lines.Count; i++)
			{
				Color color = i < 2 ? UiTheme.Text : UiTheme.TextMuted;
				TextRenderer.DrawText(g, lines[i], this.Font, new Rectangle(about.Left + (int)(12 * s), top, textWidth, heights[i]), color, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				top += heights[i] + lineGap;
			}
			y = about.Bottom + (int)(16 * s);

			// 使いかた（ざっくりした流れ）
			string[] steps =
			{
				Localizer.T("ROM を開く（上の「ROMを選択」か、この画面）。ROM はコピーを使うのがおすすめです。"),
				Localizer.T("左の一覧からマップを選ぶ。右のブロック一覧で選んで、マップに描く。"),
				Localizer.T("「編集中のMAPを確定」で、編集した内容を ROM のデータに反映する。"),
				Localizer.T("「ROMを保存」でファイルに書き出す（保存するまで ROM のファイルは変わりません）。"),
			};
			int stepsHeight = (int)(14 * s) + this.fontHeading.Height + (int)(8 * s) + (int)(12 * s);
			List<int> stepHeights = new List<int>();
			int numberWidth = (int)(26 * s);
			foreach (string step in steps)
			{
				int h = TextRenderer.MeasureText(g, step, this.Font, new Size(textWidth - numberWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
				stepHeights.Add(h);
				stepsHeight += h + (int)(6 * s);
			}
			if (y + stepsHeight > area.Bottom)
			{
				return;
			}
			Rectangle guide = new Rectangle(area.Left, y, area.Width, stepsHeight);
			this.DrawCard(g, guide, Localizer.T("使いかた（ざっくりした流れ）"), s);
			top = guide.Top + (int)(14 * s) + this.fontHeading.Height + (int)(8 * s);
			for (int i = 0; i < steps.Length; i++)
			{
				TextRenderer.DrawText(g, (i + 1).ToString() + ".", this.fontHeading, new Rectangle(guide.Left + (int)(12 * s), top, numberWidth, this.fontHeading.Height), UiTheme.Accent, TextFormatFlags.Left | TextFormatFlags.NoPadding);
				TextRenderer.DrawText(g, steps[i], this.Font, new Rectangle(guide.Left + (int)(12 * s) + numberWidth, top, textWidth - numberWidth, stepHeights[i]), UiTheme.Text, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				top += stepHeights[i] + (int)(6 * s);
			}
		}

		//-------------------------------------------------------------------------------
		// 枠（カード）と見出しを描く処理
		//-------------------------------------------------------------------------------
		private void DrawCard(Graphics g, Rectangle bounds, string title, float s)
		{
			using (SolidBrush fill = new SolidBrush(UiTheme.Card))
			using (Pen border = new Pen(UiTheme.Border))
			{
				g.FillRectangle(fill, bounds);
				g.DrawRectangle(border, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);
			}
			TextRenderer.DrawText(g, title, this.fontHeading, new Point(bounds.Left + (int)(12 * s), bounds.Top + (int)(12 * s)), UiTheme.TextMuted);
		}

		//-------------------------------------------------------------------------------
		// クリックできる文字（リンク）を描いて、当たり判定に入れる処理。乗っている間は下線を付ける
		//-------------------------------------------------------------------------------
		private void DrawLink(Graphics g, string text, Font font, Rectangle bounds, bool enabled, Action click, string tip)
		{
			HotSpot spot = new HotSpot { Bounds = bounds, Enabled = enabled, Click = click, ToolTip = tip };
			this.hotSpots.Add(spot);
			bool hover = enabled && this.hovering != null && this.hovering.Bounds == bounds;
			Color color = !enabled ? UiTheme.TextMuted : (hover ? Color.FromArgb(170, 160, 255) : UiTheme.Accent);
			TextRenderer.DrawText(g, text, font, bounds, color, TextFormatFlags.Left | TextFormatFlags.NoPadding);
			if (hover)
			{
				using (Pen pen = new Pen(color))
				{
					g.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マウスが動いたら、乗っている所を更新する処理（変わったときだけ描き直す）
		//-------------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			HotSpot next = this.hotSpots.FirstOrDefault(spot => spot.Bounds.Contains(e.Location));
			if (!ReferenceEquals(next, this.hovering))
			{
				this.hovering = next;
				this.Cursor = next != null && next.Enabled ? Cursors.Hand : Cursors.Default;
				this.toolTip.SetToolTip(this, next != null ? next.ToolTip : null);
				this.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// マウスが外れたら、乗っている所を無しにする処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			if (this.hovering != null)
			{
				this.hovering = null;
				this.Cursor = Cursors.Default;
				this.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// クリックされた所の処理を呼ぶ
		//-------------------------------------------------------------------------------
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			HotSpot spot = this.hotSpots.FirstOrDefault(h => h.Bounds.Contains(e.Location));
			if (spot != null && spot.Enabled && spot.Click != null)
			{
				spot.Click();
			}
		}

		//-------------------------------------------------------------------------------
		// ドラッグされてきたものが .gba ファイル 1 つなら受け付ける処理
		//-------------------------------------------------------------------------------
		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);
			e.Effect = GetDroppedRom(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
		}

		//-------------------------------------------------------------------------------
		// ドロップされた .gba ファイルを開くよう知らせる処理
		//-------------------------------------------------------------------------------
		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);
			string path = GetDroppedRom(e);
			if (path != null)
			{
				this.RomSelected?.Invoke(this, path);
			}
		}

		//-------------------------------------------------------------------------------
		// ドラッグの中身から .gba ファイルの場所を取り出す処理（1 つだけのとき。違えば null）
		//-------------------------------------------------------------------------------
		private static string GetDroppedRom(DragEventArgs e)
		{
			string[] files = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop) ? e.Data.GetData(DataFormats.FileDrop) as string[] : null;
			if (files == null || files.Length != 1)
			{
				return null;
			}
			return string.Equals(Path.GetExtension(files[0]), ".gba", StringComparison.OrdinalIgnoreCase) && File.Exists(files[0]) ? files[0] : null;
		}

		//-------------------------------------------------------------------------------
		// 文書やフォルダを、OS に関連付けられたアプリで開く処理
		//-------------------------------------------------------------------------------
		private static void OpenDocument(string target)
		{
			if (string.IsNullOrEmpty(target))
			{
				return;
			}
			try
			{
				Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
			}
			catch (Exception ex)
			{
				MessageBox.Show(Localizer.F("開けませんでした: {0}\n{1}", target, ex.Message), Localizer.T("ヘルプ"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		//-------------------------------------------------------------------------------
		// 文書を探す候補のフォルダ（exe の隣と、開発中なら bin の上のプロジェクトのフォルダ）を返す処理
		//-------------------------------------------------------------------------------
		private static IEnumerable<string> DocumentFolders()
		{
			string folder = AppContext.BaseDirectory;
			for (int depth = 0; depth < 5 && !string.IsNullOrEmpty(folder); depth++)
			{
				yield return folder;
				folder = Path.GetDirectoryName(folder.TrimEnd(Path.DirectorySeparatorChar));
			}
		}

		//-------------------------------------------------------------------------------
		// 名前の文書を探す処理（無ければ null）
		//-------------------------------------------------------------------------------
		private static string FindDocument(string fileName)
		{
			foreach (string folder in DocumentFolders())
			{
				string path = Path.Combine(folder, fileName);
				if (File.Exists(path))
				{
					return path;
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 名前のフォルダを探す処理（無ければ null）
		//-------------------------------------------------------------------------------
		private static string FindFolder(string name)
		{
			foreach (string folder in DocumentFolders())
			{
				string path = Path.Combine(folder, name);
				if (Directory.Exists(path))
				{
					return path;
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 名前の形に合う文書のうち、いちばん新しい名前のものを探す処理（exe の隣 → docs フォルダの順。無ければ null）
		//-------------------------------------------------------------------------------
		private static string FindNewestDocument(string pattern)
		{
			foreach (string folder in DocumentFolders())
			{
				foreach (string dir in new[] { folder, Path.Combine(folder, "docs") })
				{
					try
					{
						if (!Directory.Exists(dir))
						{
							continue;
						}
						string found = Directory.GetFiles(dir, pattern).OrderByDescending(p => Path.GetFileName(p), StringComparer.Ordinal).FirstOrDefault();
						if (found != null)
						{
							return found;
						}
					}
					catch (Exception)
					{
						// 読めないフォルダは飛ばす
					}
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// バージョンの文字（アセンブリのバージョン）を返す処理
		//-------------------------------------------------------------------------------
		private static string GetVersionText()
		{
			Version version = typeof(StartPage).Assembly.GetName().Version;
			return version != null ? version.ToString(3) : "?";
		}

		//-------------------------------------------------------------------------------
		// ビルドした日の文字（exe の更新日）を返す処理
		//-------------------------------------------------------------------------------
		private static string GetBuildDateText()
		{
			try
			{
				string exe = Environment.ProcessPath ?? typeof(StartPage).Assembly.Location;
				if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
				{
					return File.GetLastWriteTime(exe).ToString("yyyy-MM-dd");
				}
			}
			catch (Exception)
			{
				// 日付が取れなくても表示は続ける
			}
			return "-";
		}
	}
}
