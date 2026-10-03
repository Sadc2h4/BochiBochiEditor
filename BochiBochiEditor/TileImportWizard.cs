using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップチップ画像を取り込むウィザード（画像 → 色 → タイル → 書き込みの 4 段階）
	//-------------------------------------------------------------------------------
	public partial class TileImportWizard : Form
	{
		private readonly ITileImportHost host;
		// タイルセットの形（ホストから開いたときに受け取る。デザイナー用はファイアレッドの形）
		private readonly TileImportFormat format = TileImportFormat.FireRed();
		private readonly Panel[] pages;
		private readonly Label[] stepLabels;
		private int pageIndex;
		private Bitmap sourceImage;
		private Color transparentColor = Color.Magenta;
		private TileImportEngine.Plan plan;
		private TileImportEngine.Options planOptions;
		private bool isWritten;
		private bool isUpdating;

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public TileImportWizard()
		{
			InitializeComponent();
			this.pages = new Panel[] { this.pnlPageImage, this.pnlPageColor, this.pnlPageTiles, this.pnlPagePlace, this.pnlPageConfirm };
			this.stepLabels = new Label[] { this.lblStep1, this.lblStep2, this.lblStep3, this.lblStep4, this.lblStep5 };
		}

		//-------------------------------------------------------------------------------
		// マップエディタ（ROM の窓口）を受け取って初期化する処理
		//-------------------------------------------------------------------------------
		internal TileImportWizard(ITileImportHost host) : this()
		{
			this.host = host;
			if (host != null)
			{
				this.format = host.Format;
			}
			foreach (Control canvas in new Control[] { this.pnlSourcePreview, this.pnlConvertPreview, this.pnlPaletteBefore, this.pnlPaletteAfter, this.pnlBaseBlockPreview, this.pnlLayerPreview, this.pnlTileMeter, this.pnlBlockMeter })
			{
				typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(canvas, true, null);
			}
			this.toolTip.SetToolTip(this.pnlSourcePreview, Localizer.T("クリックした場所の色を「透明にする色」にします"));
			this.toolTip.SetToolTip(this.nudBaseBlock, Localizer.T("透明な部分の下に見せるブロック（床や草など）。マップ画面のパレットで番号を確認できます"));
			this.toolTip.SetToolTip(this.btnPickBaseBlock, Localizer.T("ブロックの一覧を開いて、下地にするブロックをクリックで選びます"));
			// 番号の上限は、今のマップで使えるブロックの数に合わせる
			if (host != null && host.TotalBlocks > 0)
			{
				this.nudBaseBlock.Maximum = host.TotalBlocks - 1;
			}
			this.SetLayerToolTips();
			Localizer.RegisterToolTip(this.toolTip);
			Localizer.Apply(this);
			this.UpdateTargetInfo();
			this.ShowPage(0);
		}

		//-------------------------------------------------------------------------------
		// 重ね方の選択肢に、実際に絵を置く層の説明（ブロックの層の数に合わせたもの）をツールチップで付ける処理
		//-------------------------------------------------------------------------------
		private void SetLayerToolTips()
		{
			bool triple = this.format.LayersPerBlock >= 3;
			this.toolTip.SetToolTip(this.rbLayerBottom, triple
				? Localizer.T("絵を下層に置きます（中層・上層は透明）。")
				: Localizer.T("絵を下層に置きます（上層は透明）。"));
			this.toolTip.SetToolTip(this.rbLayerBelow, triple
				? Localizer.T("下地ブロックの下層の上に、絵を中層として置きます（上層は透明）。")
				: Localizer.T("下地ブロックの下層の上に、絵を上層として置きます（挙動のレイヤーで、プレイヤーの下に描きます）。"));
			this.toolTip.SetToolTip(this.rbLayerAbove, triple
				? Localizer.T("下地ブロックの下層・中層の上に、絵を上層として置きます（プレイヤーより手前に描きます）。")
				: Localizer.T("下地ブロックの下層の上に、絵を上層として置きます（プレイヤーより手前に描きます）。"));
		}

		//-------------------------------------------------------------------------------
		// フォーム表示後にテーマの色を整える処理（見出し色・強調ボタン）
		//-------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			UiTheme.MarkCanvas(this.pnlSourcePreview);
			UiTheme.MarkCanvas(this.pnlConvertPreview);
			UiTheme.MarkCanvas(this.pnlBaseBlockPreview);
			this.pnlSteps.BackColor = UiTheme.Window;
			this.pnlButtons.BackColor = UiTheme.Window;
			this.UpdateStepLabels();
			this.UpdatePaletteWarning();
		}

		//-------------------------------------------------------------------------------
		// 指定ページを表示し、ボタンと手順表示を更新する処理
		//-------------------------------------------------------------------------------
		private void ShowPage(int index)
		{
			this.pageIndex = index;
			for (int i = 0; i < this.pages.Length; i++)
			{
				this.pages[i].Visible = i == index;
			}
			this.btnBack.Enabled = index > 0 && !this.isWritten;
			this.btnNext.Visible = index < this.pages.Length - 1;
			this.btnNext.Enabled = this.CanLeavePage(index);
			this.AcceptButton = index < this.pages.Length - 1 ? this.btnNext : null;
			this.UpdateStepLabels();
			if (index == 1)
			{
				this.PreparePalettePage();
			}
			// 計画は設定を変えたときだけ作り直す（戻って見返しただけでは配置を消さない）
			if (index >= 1 && this.plan == null)
			{
				this.RebuildPlan();
			}
			if (index == 3)
			{
				this.PreparePlacePage();
			}
			if (index == 4)
			{
				this.UpdateSummary();
			}
		}

		//-------------------------------------------------------------------------------
		// 左側の手順表示で、今のページを強調する処理
		//-------------------------------------------------------------------------------
		private void UpdateStepLabels()
		{
			for (int i = 0; i < this.stepLabels.Length; i++)
			{
				bool current = i == this.pageIndex;
				this.stepLabels[i].BackColor = current ? UiTheme.Accent : Color.Transparent;
				this.stepLabels[i].ForeColor = current ? UiTheme.AccentText : (i < this.pageIndex ? UiTheme.Text : UiTheme.TextMuted);
			}
		}

		//-------------------------------------------------------------------------------
		// 今のページから次へ進めるか判定する処理
		//-------------------------------------------------------------------------------
		private bool CanLeavePage(int index)
		{
			if (index == 0)
			{
				return this.sourceImage != null;
			}
			if (index == 2)
			{
				return this.plan != null && this.plan.TilesReady;
			}
			if (index == 3)
			{
				return this.plan != null && this.plan.CanWrite;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// 「次へ」ボタンの処理
		//-------------------------------------------------------------------------------
		private void btnNext_Click(object sender, EventArgs e)
		{
			if (this.pageIndex < this.pages.Length - 1 && this.CanLeavePage(this.pageIndex))
			{
				this.ShowPage(this.pageIndex + 1);
			}
		}

		//-------------------------------------------------------------------------------
		// 「戻る」ボタンの処理
		//-------------------------------------------------------------------------------
		private void btnBack_Click(object sender, EventArgs e)
		{
			if (this.pageIndex > 0)
			{
				this.ShowPage(this.pageIndex - 1);
			}
		}

		//-------------------------------------------------------------------------------
		// 取り込み先がタイルセット2（第2）かどうかを返す処理
		//-------------------------------------------------------------------------------
		private bool IsSecondaryTarget
		{
			get { return this.rbTargetSecondary.Checked; }
		}

		//-------------------------------------------------------------------------------
		// 書き込み先の切り替え時に説明とパレット枠の候補を更新する処理
		//-------------------------------------------------------------------------------
		private void Target_CheckedChanged(object sender, EventArgs e)
		{
			if (sender is RadioButton radio && !radio.Checked)
			{
				return;
			}
			this.UpdateTargetInfo();
			this.cmbPaletteSlot.Items.Clear();
			this.plan = null;
		}

		//-------------------------------------------------------------------------------
		// 書き込み先の説明文を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateTargetInfo()
		{
			if (this.host == null)
			{
				return;
			}
			string name = this.host.DescribeTileset(this.IsSecondaryTarget);
			this.lblTargetInfo.Text = this.IsSecondaryTarget
				? name + Localizer.T("\r\n\r\nこのマップと、同じタイルセット2を使うマップだけに影響します。通常はこちらを選びます。")
				: name + Localizer.T("\r\n\r\n多くのマップで共有されています。空いているタイル・ブロックは少なめです。");
		}

		//-------------------------------------------------------------------------------
		// 「画像を選ぶ」ボタンでファイルを開く処理
		//-------------------------------------------------------------------------------
		private void btnPickImage_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = Localizer.T("画像ファイル|*.png;*.bmp;*.gif|すべてのファイル|*.*");
				dialog.Title = Localizer.T("取り込むマップチップ画像を選択");
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					this.LoadSourceImage(dialog.FileName);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 画像ファイルがドラッグされたら受け付ける処理
		//-------------------------------------------------------------------------------
		private void TileImportWizard_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = (e.Data.GetDataPresent(DataFormats.FileDrop) && this.pageIndex == 0) ? DragDropEffects.Copy : DragDropEffects.None;
		}

		//-------------------------------------------------------------------------------
		// ドロップされた画像ファイルを読み込む処理
		//-------------------------------------------------------------------------------
		private void TileImportWizard_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
			{
				this.LoadSourceImage(files[0]);
			}
		}

		//-------------------------------------------------------------------------------
		// 開く前に画像と取り込み先を決めておく処理（マップタイルの移植の「一部だけ取り込み」から使う）
		//-------------------------------------------------------------------------------
		internal void PreloadImage(string path, bool secondary)
		{
			this.rbTargetSecondary.Checked = secondary;
			this.rbTargetPrimary.Checked = !secondary;
			this.LoadSourceImage(path);
			// 一時ファイルから読んだので、パスの欄には何から読んだかを出す
			this.txtImagePath.Text = Localizer.T("（マップタイルの移植で選んだブロック）");
		}

		//-------------------------------------------------------------------------------
		// 画像を読み込み、透明色（左上の色）と情報表示を初期化する処理
		//-------------------------------------------------------------------------------
		private void LoadSourceImage(string path)
		{
			Bitmap loaded;
			try
			{
				// ファイルをロックしないよう、読み込んだ画像を複製して使う
				using (Bitmap temp = new Bitmap(path))
				{
					loaded = new Bitmap(temp);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, Localizer.T("画像を開けませんでした。\n") + ex.Message, Localizer.T("マップチップ取り込み"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (loaded.Width > 128 * 4 || loaded.Height > 512)
			{
				MessageBox.Show(this, Localizer.T("画像が大きすぎます（幅 512・高さ 512 ピクセルまで）。必要な部分だけを切り出してください。"), Localizer.T("マップチップ取り込み"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				loaded.Dispose();
				return;
			}
			this.sourceImage?.Dispose();
			this.sourceImage = loaded;
			this.txtImagePath.Text = path;
			Color corner = loaded.GetPixel(0, 0);
			this.transparentColor = Color.FromArgb(255, corner.R, corner.G, corner.B);
			this.plan = null;
			this.UpdateImageInfo();
			this.pnlSourcePreview.Invalidate();
			this.btnNext.Enabled = this.CanLeavePage(this.pageIndex);
		}

		//-------------------------------------------------------------------------------
		// 画像の大きさ・色数・透明色の表示を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateImageInfo()
		{
			this.pnlTransparentSwatch.BackColor = this.transparentColor;
			if (this.sourceImage == null)
			{
				this.lblImageInfo.Text = Localizer.T("画像を選んでください。");
				return;
			}
			int w = this.sourceImage.Width;
			int h = this.sourceImage.Height;
			int cols = (w + 15) / 16;
			int rows = (h + 15) / 16;
			int colors = TileImportEngine.CountColors(TileImportEngine.ReadPixels(this.sourceImage), this.transparentColor);
			string size = (w % 16 == 0 && h % 16 == 0)
				? string.Format(Localizer.T("{0}×{1} ピクセル"), w, h)
				: string.Format(Localizer.T("{0}×{1} ピクセル → 余白を足して {2}×{3}"), w, h, cols * 16, rows * 16);
			this.lblImageInfo.Text = string.Format(Localizer.T("大きさ : {0}\r\nブロック : 横 {1} × 縦 {2} = {3} 個分\r\n色数 : {4} 色（透明を除く）{5}\r\n透明色 : #{6:X2}{7:X2}{8:X2}"),
				size, cols, rows, cols * rows, colors, colors > 15 ? Localizer.T("\r\n→ 15 色を超えるため、次の手順で減色します") : string.Empty,
				this.transparentColor.R, this.transparentColor.G, this.transparentColor.B);
		}

		//-------------------------------------------------------------------------------
		// 元画像のプレビュー（背景は市松模様、ドットを保ったまま拡大）を描く処理
		//-------------------------------------------------------------------------------
		private void pnlSourcePreview_Paint(object sender, PaintEventArgs e)
		{
			if (this.sourceImage == null)
			{
				TextRenderer.DrawText(e.Graphics, Localizer.T("画像を選ぶか、この画面へドラッグ＆ドロップしてください"), this.Font, this.pnlSourcePreview.ClientRectangle, UiTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
				return;
			}
			Rectangle dest = this.GetPreviewRect(this.pnlSourcePreview, this.sourceImage.Size);
			DrawChecker(e.Graphics, dest);
			e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
			e.Graphics.DrawImage(this.sourceImage, dest);
			this.DrawCellGrid(e.Graphics, dest, this.sourceImage.Size);
		}

		//-------------------------------------------------------------------------------
		// プレビューをクリックした位置の色を透明色にする処理
		//-------------------------------------------------------------------------------
		private void pnlSourcePreview_MouseClick(object sender, MouseEventArgs e)
		{
			if (this.sourceImage == null)
			{
				return;
			}
			Rectangle dest = this.GetPreviewRect(this.pnlSourcePreview, this.sourceImage.Size);
			if (!dest.Contains(e.Location))
			{
				return;
			}
			int x = (e.X - dest.X) * this.sourceImage.Width / dest.Width;
			int y = (e.Y - dest.Y) * this.sourceImage.Height / dest.Height;
			Color c = this.sourceImage.GetPixel(Math.Min(x, this.sourceImage.Width - 1), Math.Min(y, this.sourceImage.Height - 1));
			this.transparentColor = Color.FromArgb(255, c.R, c.G, c.B);
			this.plan = null;
			this.UpdateImageInfo();
			this.pnlSourcePreview.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 画像を枠内に整数倍で収める表示位置を求める処理
		//-------------------------------------------------------------------------------
		private Rectangle GetPreviewRect(Control canvas, Size image)
		{
			Rectangle area = canvas.ClientRectangle;
			area.Inflate(-8, -8);
			int scale = Math.Max(1, Math.Min(area.Width / Math.Max(1, image.Width), area.Height / Math.Max(1, image.Height)));
			scale = Math.Min(scale, 8);
			Size size = new Size(image.Width * scale, image.Height * scale);
			return new Rectangle(area.X + (area.Width - size.Width) / 2, area.Y + (area.Height - size.Height) / 2, size.Width, size.Height);
		}

		//-------------------------------------------------------------------------------
		// 透明部分が分かるよう市松模様を描く処理
		//-------------------------------------------------------------------------------
		private static void DrawChecker(Graphics g, Rectangle rect)
		{
			using (HatchBrush brush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.FromArgb(60, 64, 90), Color.FromArgb(44, 48, 70)))
			{
				g.FillRectangle(brush, rect);
			}
		}

		//-------------------------------------------------------------------------------
		// 16x16（ブロック）単位の区切り線を描く処理
		//-------------------------------------------------------------------------------
		private void DrawCellGrid(Graphics g, Rectangle dest, Size image)
		{
			float scale = (float)dest.Width / image.Width;
			using (Pen pen = new Pen(Color.FromArgb(90, 255, 255, 255)))
			{
				for (int x = 16; x < image.Width; x += 16)
				{
					g.DrawLine(pen, dest.X + x * scale, dest.Y, dest.X + x * scale, dest.Bottom);
				}
				for (int y = 16; y < image.Height; y += 16)
				{
					g.DrawLine(pen, dest.X, dest.Y + y * scale, dest.Right, dest.Y + y * scale);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// パレット枠の選択肢（使用状況つき）を用意する処理
		//-------------------------------------------------------------------------------
		private void PreparePalettePage()
		{
			if (this.cmbPaletteSlot.Items.Count > 0)
			{
				return;
			}
			this.isUpdating = true;
			int first = this.format.PaletteFirst(this.IsSecondaryTarget);
			int last = this.format.PaletteEndExclusive(this.IsSecondaryTarget) - 1;
			int suggested = this.host.SuggestPaletteSlot(this.IsSecondaryTarget);
			for (int slot = first; slot <= last; slot++)
			{
				this.cmbPaletteSlot.Items.Add(new PaletteSlotItem(slot, this.host.DescribePaletteUsage(slot), this.host.CountPaletteUsers(slot) > 0));
				if (slot == suggested)
				{
					this.cmbPaletteSlot.SelectedIndex = this.cmbPaletteSlot.Items.Count - 1;
				}
			}
			this.isUpdating = false;
			this.UpdatePaletteWarning();
		}

		//-------------------------------------------------------------------------------
		// 選んでいるパレット枠の番号を返す処理
		//-------------------------------------------------------------------------------
		private int SelectedPaletteSlot
		{
			get { return (this.cmbPaletteSlot.SelectedItem as PaletteSlotItem)?.Slot ?? (this.format.PaletteEndExclusive(this.IsSecondaryTarget) - 1); }
		}

		//-------------------------------------------------------------------------------
		// パレットの選び方が変わったら計画を作り直す処理
		//-------------------------------------------------------------------------------
		private void PaletteOption_Changed(object sender, EventArgs e)
		{
			if (this.isUpdating || (sender is RadioButton radio && !radio.Checked))
			{
				return;
			}
			this.UpdatePaletteWarning();
			this.RebuildPlan();
		}

		//-------------------------------------------------------------------------------
		// 使用中の枠を上書きしようとしているときに注意を出す処理
		//-------------------------------------------------------------------------------
		private void UpdatePaletteWarning()
		{
			PaletteSlotItem item = this.cmbPaletteSlot.SelectedItem as PaletteSlotItem;
			bool inUse = item != null && item.InUse;
			this.lblPaletteWarning.ForeColor = UiTheme.Warning;
			this.lblPaletteWarning.Text = (inUse && this.rbPaletteOverwrite.Checked)
				? string.Format(Localizer.T("⚠ 枠 {0} は {1} です。上書きすると、その枠を使っている既存ブロックの色も変わります。未使用の枠を選ぶか、「今の色に近い色で描く」を選んでください。"), item.Slot, item.Usage)
				: string.Empty;
		}

		//-------------------------------------------------------------------------------
		// 今の設定で取り込み計画を作り直し、各ページの表示を更新する処理
		//-------------------------------------------------------------------------------
		private void RebuildPlan()
		{
			if (this.sourceImage == null || this.host == null)
			{
				return;
			}
			Cursor previous = this.Cursor;
			this.Cursor = Cursors.WaitCursor;
			try
			{
				TileImportEngine.Options options = new TileImportEngine.Options
				{
					Source = this.sourceImage,
					TransparentColor = this.transparentColor,
					IsSecondary = this.IsSecondaryTarget,
					PaletteSlot = this.SelectedPaletteSlot,
					Mode = this.rbPaletteMatch.Checked ? TileImportEngine.PaletteMode.MatchExisting : TileImportEngine.PaletteMode.Overwrite,
					ExistingPalette = this.host.GetPalette(this.SelectedPaletteSlot),
					DedupeFlipped = this.chkDedupeFlip.Checked,
					ReuseExistingTiles = this.chkReuseTiles.Checked,
					SkipEmptyCells = this.chkSkipEmpty.Checked,
					DedupeBlocks = this.chkDedupeBlocks.Checked,
					Layer = this.rbLayerAbove.Checked ? TileImportEngine.LayerStyle.OverBaseAbovePlayer : (this.rbLayerBelow.Checked ? TileImportEngine.LayerStyle.OverBaseBelowPlayer : TileImportEngine.LayerStyle.ArtOnBottom)
				};
				if (options.Layer != TileImportEngine.LayerStyle.ArtOnBottom)
				{
					options.BaseEntries = this.host.GetBlockEntries((int)this.nudBaseBlock.Value);
				}
				TileImportEngine.TargetState state = this.host.GetTargetState(options.IsSecondary);
				this.plan?.ConvertedPreview?.Dispose();
				this.plan = TileImportEngine.BuildPlan(options, state);
				this.planOptions = options;
				this.AddBaseBlockNotes();
			}
			finally
			{
				this.Cursor = previous;
			}
			this.UpdatePlanViews();
		}

		//-------------------------------------------------------------------------------
		// 計画の内容を色・タイル・ブロックの各表示に反映する処理
		//-------------------------------------------------------------------------------
		private void UpdatePlanViews()
		{
			if (this.plan == null)
			{
				return;
			}
			this.lblColorInfo.Text = string.Format(Localizer.T("元の画像 : {0} 色　→　書き込み後 : {1} 色（透明を除く）\r\n{2}"),
				this.plan.SourceColorCount, this.plan.UsedColorCount, string.Join(" ", this.plan.ColorNotes));
			this.lblTileStats.Text = string.Format(Localizer.T("タイル（8×8） : 新しく書く {0} 枚 ／ 空き {1} 枚　　（全 {2} 枚のうち、既存を使う {3}・同じ絵をまとめる {4}・反転でまとめる {5}）"),
				this.plan.NewTiles.Count, this.plan.AvailableTileSlots, this.plan.TotalTileCount, this.plan.ReusedTileCount, this.plan.DuplicateTileCount, this.plan.FlippedTileCount);
			this.lblBlockStats.Text = string.Format(Localizer.T("ブロック（16×16） : 必要 {0} 個 ／ 空き {1} 個　　（同じ絵でまとめたマス {2}・全部透明で省いたマス {3}）"),
				this.plan.RequiredBlocks, this.plan.AvailableBlockSlots, this.plan.DuplicateCellCount, this.plan.EmptyCellCount);
			List<string> issues = new List<string>();
			issues.AddRange(this.plan.Errors.Select(m => "✖ " + m));
			issues.AddRange(this.plan.Notes.Select(m => Localizer.T("・") + m));
			if (issues.Count == 0)
			{
				issues.Add(Localizer.T("✓ 問題は見つかりませんでした。次へ進んで書き込めます。"));
			}
			this.txtIssues.Text = string.Join(Environment.NewLine, issues);
			bool baseNeeded = !this.rbLayerBottom.Checked;
			this.lblBaseBlock.Enabled = baseNeeded;
			this.nudBaseBlock.Enabled = baseNeeded;
			this.btnPickBaseBlock.Enabled = baseNeeded;
			this.pnlPaletteBefore.Invalidate();
			this.pnlPaletteAfter.Invalidate();
			this.pnlConvertPreview.Invalidate();
			this.pnlTileMeter.Invalidate();
			this.pnlBlockMeter.Invalidate();
			this.pnlBaseBlockPreview.Invalidate();
			this.pnlLayerPreview.Invalidate();
			this.InvalidatePlaceCaches();
			this.btnNext.Enabled = this.CanLeavePage(this.pageIndex);
		}

		//-------------------------------------------------------------------------------
		// タイル・ブロックの作り方が変わったら計画を作り直す処理
		//-------------------------------------------------------------------------------
		private void TileOption_Changed(object sender, EventArgs e)
		{
			if (sender is RadioButton radio && !radio.Checked)
			{
				return;
			}
			if (this.pageIndex >= 1)
			{
				this.RebuildPlan();
			}
		}

		//-------------------------------------------------------------------------------
		// 今のパレット（16 色）を描く処理
		//-------------------------------------------------------------------------------
		private void pnlPaletteBefore_Paint(object sender, PaintEventArgs e)
		{
			if (this.host != null)
			{
				DrawSwatches(e.Graphics, this.pnlPaletteBefore.ClientRectangle, this.host.GetPalette(this.SelectedPaletteSlot));
			}
		}

		//-------------------------------------------------------------------------------
		// 書き込む予定のパレット（16 色）を描く処理
		//-------------------------------------------------------------------------------
		private void pnlPaletteAfter_Paint(object sender, PaintEventArgs e)
		{
			if (this.plan != null)
			{
				DrawSwatches(e.Graphics, this.pnlPaletteAfter.ClientRectangle, this.plan.Palette);
			}
		}

		//-------------------------------------------------------------------------------
		// 16 色の色見本を横一列に描く処理（色 0 は透明なので斜線を付ける）
		//-------------------------------------------------------------------------------
		private static void DrawSwatches(Graphics g, Rectangle area, Color[] colors)
		{
			int size = Math.Min(area.Height - 2, (area.Width - 2) / 16);
			for (int i = 0; i < 16 && i < colors.Length; i++)
			{
				Rectangle r = new Rectangle(area.X + i * (size + 0), area.Y, size - 2, size - 2);
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, colors[i])))
				{
					g.FillRectangle(brush, r);
				}
				if (i == 0)
				{
					using (Pen slash = new Pen(Color.FromArgb(200, 255, 80, 80), 2f))
					{
						g.DrawLine(slash, r.Left, r.Bottom, r.Right, r.Top);
					}
				}
				using (Pen border = new Pen(UiTheme.Border))
				{
					g.DrawRectangle(border, r);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 減色後の見た目（左）と元画像（右）を並べて描く処理
		//-------------------------------------------------------------------------------
		private void pnlConvertPreview_Paint(object sender, PaintEventArgs e)
		{
			if (this.plan == null || this.plan.ConvertedPreview == null || this.sourceImage == null)
			{
				return;
			}
			Rectangle area = this.pnlConvertPreview.ClientRectangle;
			int half = area.Width / 2;
			Rectangle left = new Rectangle(area.X, area.Y + 18, half, area.Height - 18);
			Rectangle right = new Rectangle(area.X + half, area.Y + 18, area.Width - half, area.Height - 18);
			TextRenderer.DrawText(e.Graphics, Localizer.T("書き込み後の見た目"), this.Font, new Point(left.X + 8, 2), UiTheme.TextMuted);
			TextRenderer.DrawText(e.Graphics, Localizer.T("元の画像"), this.Font, new Point(right.X + 8, 2), UiTheme.TextMuted);
			this.DrawPreviewInto(e.Graphics, left, this.plan.ConvertedPreview);
			this.DrawPreviewInto(e.Graphics, right, this.sourceImage);
		}

		//-------------------------------------------------------------------------------
		// 指定範囲へ画像を整数倍で拡大して描く処理
		//-------------------------------------------------------------------------------
		private void DrawPreviewInto(Graphics g, Rectangle area, Image image)
		{
			area.Inflate(-8, -8);
			int scale = Math.Max(1, Math.Min(Math.Min(area.Width / Math.Max(1, image.Width), area.Height / Math.Max(1, image.Height)), 8));
			Rectangle dest = new Rectangle(area.X + (area.Width - image.Width * scale) / 2, area.Y + (area.Height - image.Height * scale) / 2, image.Width * scale, image.Height * scale);
			DrawChecker(g, dest);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(image, dest);
			g.PixelOffsetMode = PixelOffsetMode.Default;
		}

		//-------------------------------------------------------------------------------
		// タイルの必要数と空きをメーターで描く処理
		//-------------------------------------------------------------------------------
		private void pnlTileMeter_Paint(object sender, PaintEventArgs e)
		{
			if (this.plan != null)
			{
				DrawMeter(e.Graphics, this.pnlTileMeter.ClientRectangle, this.plan.RequiredNewTiles, this.plan.AvailableTileSlots);
			}
		}

		//-------------------------------------------------------------------------------
		// ブロックの必要数と空きをメーターで描く処理
		//-------------------------------------------------------------------------------
		private void pnlBlockMeter_Paint(object sender, PaintEventArgs e)
		{
			if (this.plan != null)
			{
				DrawMeter(e.Graphics, this.pnlBlockMeter.ClientRectangle, this.plan.RequiredBlocks, this.plan.AvailableBlockSlots);
			}
		}

		//-------------------------------------------------------------------------------
		// 必要数／空きの割合を横棒で描く処理（足りない場合は赤）
		//-------------------------------------------------------------------------------
		private static void DrawMeter(Graphics g, Rectangle area, int required, int available)
		{
			using (SolidBrush back = new SolidBrush(UiTheme.Input))
			{
				g.FillRectangle(back, area);
			}
			if (available <= 0 && required <= 0)
			{
				return;
			}
			float ratio = available <= 0 ? 1f : Math.Min(1f, (float)required / available);
			Color color = required > available ? UiTheme.Warning : UiTheme.Accent;
			using (SolidBrush fill = new SolidBrush(color))
			{
				g.FillRectangle(fill, new RectangleF(area.X, area.Y, area.Width * ratio, area.Height));
			}
		}

		//-------------------------------------------------------------------------------
		// 確認ページの概要文を作る処理
		//-------------------------------------------------------------------------------
		private void UpdateSummary()
		{
			if (this.plan == null || this.planOptions == null)
			{
				this.txtSummary.Text = string.Empty;
				return;
			}
			List<int> ids = this.plan.Blocks.Select(b => b.BlockId).OrderBy(i => i).ToList();
			string layer = this.planOptions.Layer == TileImportEngine.LayerStyle.ArtOnBottom ? Localizer.T("絵だけで置く")
				: string.Format(Localizer.T("下地ブロック 0x{0:X3} の上に重ねる（{1}）"), (int)this.nudBaseBlock.Value, this.planOptions.Layer == TileImportEngine.LayerStyle.OverBaseAbovePlayer ? Localizer.T("プレイヤーより手前") : Localizer.T("プレイヤーの下"));
			List<string> lines = new List<string>
			{
				Localizer.T("画像 : ") + Path.GetFileName(this.txtImagePath.Text),
				Localizer.T("書き込み先 : ") + this.host.DescribeTileset(this.planOptions.IsSecondary),
				string.Format(Localizer.T("パレット : 枠 {0} を{1}"), this.planOptions.PaletteSlot, this.planOptions.Mode == TileImportEngine.PaletteMode.Overwrite ? Localizer.T("画像の色で上書き") : Localizer.T("そのまま使う（色を近づける）")),
				string.Format(Localizer.T("タイル : 新しく {0} 枚（圧縮し直して空き領域へ書き、参照先を付け替えます）"), this.plan.NewTiles.Count),
				ids.Count > 0 ? string.Format(Localizer.T("ブロック : {0} 個（0x{1:X3}〜0x{2:X3}）"), ids.Count, ids.First(), ids.Last()) : Localizer.T("ブロック : なし"),
				Localizer.T("重ね方 : ") + layer,
				this.plan.ReplacingCount > 0 ? string.Format(Localizer.T("差し替え : 使用中のブロック {0} 個の絵を置き換えます（置かれているマップの見た目が変わります）"), this.plan.ReplacingCount) : Localizer.T("差し替え : なし（すべて空きの枠へ追加）"),
				"",
				Localizer.T("書き込みはメモリ上の ROM に対して行います。ファイルへ残すには、書き込み後に「ROMを保存」を押してください。")
			};
			this.txtSummary.Text = string.Join(Environment.NewLine, lines);
			string backup = this.host.BackupFileName;
			this.chkBackup.Enabled = backup != null && !this.isWritten;
			this.chkBackup.Text = backup != null ? Localizer.T("書き込む前にバックアップを作る（") + backup + "）" : Localizer.T("書き込む前にバックアップを作る（ROM ファイルが無いため不可）");
			this.btnWrite.Enabled = this.plan.CanWrite && !this.isWritten;
			UiTheme.SetToggleButtonState(this.btnWrite, this.btnWrite.Enabled);
		}

		//-------------------------------------------------------------------------------
		// 「ROM に書き込む」ボタンの処理
		//-------------------------------------------------------------------------------
		private void btnWrite_Click(object sender, EventArgs e)
		{
			if (this.plan == null || !this.plan.CanWrite || this.isWritten)
			{
				return;
			}
			try
			{
				this.txtResult.Text = this.host.Commit(this.plan, this.planOptions, this.chkBackup.Checked && this.chkBackup.Enabled);
				this.isWritten = true;
				this.btnWrite.Enabled = false;
				UiTheme.SetToggleButtonState(this.btnWrite, false);
				this.btnBack.Enabled = false;
				this.chkBackup.Enabled = false;
				this.btnCancel.Text = Localizer.T("閉じる");
				this.btnCancel.DialogResult = DialogResult.OK;
			}
			catch (Exception ex)
			{
				this.txtResult.Text = Localizer.T("書き込みに失敗しました。\r\n") + ex.Message;
			}
		}

		//-------------------------------------------------------------------------------
		// パレット枠の選択肢 1 件分
		//-------------------------------------------------------------------------------
		private sealed class PaletteSlotItem
		{
			public readonly int Slot;
			public readonly string Usage;
			public readonly bool InUse;

			//-------------------------------------------------------------------------------
			// 枠番号と使用状況を保持する処理
			//-------------------------------------------------------------------------------
			public PaletteSlotItem(int slot, string usage, bool inUse)
			{
				this.Slot = slot;
				this.Usage = usage;
				this.InUse = inUse;
			}

			//-------------------------------------------------------------------------------
			// コンボボックスに表示する文字列を返す処理
			//-------------------------------------------------------------------------------
			public override string ToString()
			{
				return string.Format(Localizer.T("枠 {0}（{1}）"), this.Slot, this.Usage);
			}
		}
	}
}
