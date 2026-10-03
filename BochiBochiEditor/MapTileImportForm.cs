using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップタイルの取り込み画面（「マップタイル」タブ →「マップタイルをインポート」）
	// ・まとめて取り込む: 書き出したタイルセット 1・2 を、空き領域に新しいタイルセットとして作る（同じ種類のゲーム同士）
	// ・一部だけ取り込む: 書き出したブロック一覧の画像から範囲を選び、今のマップのタイルセットへマップチップ取り込みで入れる
	//-------------------------------------------------------------------------------
	public partial class MapTileImportForm : Form
	{
		// ブロック一覧の画像を画面に出す倍率と、1 ブロックの大きさ（ピクセル）
		private const int SheetZoom = 2;
		private const int BlockPixels = 16;
		// マップチップ取り込みが受け付ける画像の大きさ（ピクセル）
		private const int ChipImportMaxPixels = 512;

		private readonly MapEditor host;
		private MapTilePackage package;
		private string packageFolder;
		// 書き出したブロック一覧の画像（[0] = タイルセット1、[1] = タイルセット2。無ければ null）
		private readonly Bitmap[] sheets = new Bitmap[2];
		// 選んでいる範囲（ブロック単位）と、ドラッグを始めたブロック
		private Rectangle selection = Rectangle.Empty;
		private Point dragStart;
		private bool dragging;
		// 「まとめて取り込む」の説明文（形の違うゲームのときは理由に差し替えるので、元の文を覚えておく）
		private string allNoteText = "";

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public MapTileImportForm()
		{
			this.InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// マップエディタ（ROM の窓口）と、最初に読むフォルダ（無ければ null）を受け取って初期化する処理
		//-------------------------------------------------------------------------------
		internal MapTileImportForm(MapEditor host, string initialFolder) : this()
		{
			this.host = host;
			typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this.pnlSheet, true);
			Localizer.Apply(this);
			this.allNoteText = this.lblAllNote.Text = Localizer.T("・選んだタイルセットを、空き領域に新しいタイルセットとして作ります（タイル画像・パレット・ブロック・挙動をすべて入れます）。今あるタイルセットやマップは変わりません。\n・作った後は、「マップの設定」タブでマップのタイルセットに指定するか、「マップフッターを作成」で新しいマップの形を作るときに番号を指定してください。\n・タイルセット2 だけを取り込むときは、ブロックが指しているタイルセット1 の絵が、移植先でも同じである必要があります（違うと絵が崩れます）。\n・タイルアニメ（水面・花など）は移しません。\n・同じ種類のゲーム同士（FR 同士、エメラルド同士）にだけ使えます。");
			this.lblPartNote.Text = Localizer.T("・選んだブロックの絵を、今開いているマップのタイルセットへ「マップチップ取り込み」で入れます。取り込み先・パレット枠・置き場所はウィザードで選べます。\n・色はパレット 1 本（15 色＋透明）に減色されます。色の多いブロックは、少しずつ分けて取り込んでください。\n・一度に選べるのは 32×32 ブロックまでです。\n・ゲームの種類が違う ROM（FR ⇔ エメラルド）にも取り込めます。");
			if (!string.IsNullOrEmpty(initialFolder) && File.Exists(Path.Combine(initialFolder, MapTilePackage.InfoFileName)))
			{
				this.LoadFolder(initialFolder);
			}
			this.UpdateUi();
		}

		//-------------------------------------------------------------------------------
		// 表示前にテーマの色を整える処理
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.pnlSheetHost.BackColor = UiTheme.Canvas;
			this.lblPartNote.ForeColor = UiTheme.TextMuted;
			// テーマの適用で文字色が戻るので、読み込み結果の色と説明をもう一度合わせる
			this.lblSummary.ForeColor = this.package != null || string.IsNullOrEmpty(this.packageFolder) ? UiTheme.Text : UiTheme.Warning;
			this.UpdateUi();
		}

		//-------------------------------------------------------------------------------
		// 「選ぶ…」: 書き出したフォルダを選ぶ処理
		//-------------------------------------------------------------------------------
		private void btnBrowse_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.Description = Localizer.T("「マップタイルをエクスポート」で作ったフォルダ（map_info.txt が入っているフォルダ）を選んでください。");
				dialog.UseDescriptionForTitle = true;
				if (!string.IsNullOrEmpty(this.packageFolder))
				{
					dialog.SelectedPath = this.packageFolder;
				}
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					this.LoadFolder(dialog.SelectedPath);
					this.UpdateUi();
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 書き出したフォルダを読み、概要とブロック一覧を用意する処理
		//-------------------------------------------------------------------------------
		internal void LoadFolder(string folder)
		{
			this.package = null;
			this.packageFolder = folder;
			this.txtFolder.Text = folder;
			this.DisposeSheets();
			this.selection = Rectangle.Empty;
			try
			{
				this.package = MapTilePackage.Load(folder);
			}
			catch (Exception ex)
			{
				this.lblSummary.Text = Localizer.T("読み込めませんでした: ") + ex.Message;
				this.lblSummary.ForeColor = UiTheme.Warning;
				return;
			}
			for (int t = 0; t < 2; t++)
			{
				string path = Path.Combine(folder, t == 0 ? "tileset1_blocks.png" : "tileset2_blocks.png");
				if (File.Exists(path))
				{
					// ファイルをロックしないよう、読み込んだ画像を複製して使う
					using (Bitmap temp = new Bitmap(path))
					{
						this.sheets[t] = new Bitmap(temp);
					}
				}
			}
			MapTilePackage p = this.package;
			string summary = string.Format(Localizer.T("書き出し元: {0} の ({1}, {2}) {3}（{4}×{5}）"), p.GameCode, p.Bank, p.Number, p.MapName, p.MapWidth, p.MapHeight);
			for (int t = 0; t < 2; t++)
			{
				MapTilePackage.TilesetPart part = p.Tilesets[t];
				if (part != null)
				{
					summary += "\n" + string.Format(Localizer.T("タイルセット{0}: 番号 {1}・タイル {2} 枚・ブロック {3} 個"), t + 1, part.Index, part.TileCount, part.BlockCount);
				}
			}
			this.lblSummary.Text = summary;
			this.lblSummary.ForeColor = UiTheme.Text;
			this.chkPrimary.Enabled = p.Tilesets[0] != null;
			this.chkSecondary.Enabled = p.Tilesets[1] != null;
			this.rbPartPrimary.Enabled = this.sheets[0] != null;
			this.rbPartSecondary.Enabled = this.sheets[1] != null;
			if (!this.rbPartSecondary.Enabled && this.rbPartPrimary.Enabled)
			{
				this.rbPartPrimary.Checked = true;
			}
			this.UpdateSheetSize();
		}

		//-------------------------------------------------------------------------------
		// 取り込み方の切り替え・ボタンの文言と押せるかどうかを、今の状態に合わせる処理
		//-------------------------------------------------------------------------------
		private void UpdateUi()
		{
			bool all = this.rbModeAll.Checked;
			this.grpAll.Visible = all;
			this.grpPart.Visible = !all;
			this.btnRun.Text = all ? Localizer.T("取り込む") : Localizer.T("マップチップ取り込みへ…");
			string incompatible = this.package != null ? this.package.CheckCompatibleWithCurrentGame() : null;
			// 形の違うゲームのデータなら、変換できるかを確かめ、変換の内容（できなければ理由）を説明の代わりに出す
			string conversion = null;
			if (incompatible != null)
			{
				MapTilePackage probe = this.package;
				string notes;
				string error = this.host.ConvertTilePackageForCurrentGame(ref probe, out notes);
				if (error == null)
				{
					incompatible = null;
					conversion = notes;
				}
				else
				{
					incompatible = error;
				}
			}
			this.lblAllNote.Text = incompatible ?? (conversion ?? this.allNoteText);
			this.lblAllNote.ForeColor = incompatible != null ? UiTheme.Warning : (conversion != null ? UiTheme.Text : UiTheme.TextMuted);
			this.btnRun.Enabled = this.package != null && (all
				? incompatible == null && (this.chkPrimary.Checked && this.chkPrimary.Enabled || this.chkSecondary.Checked && this.chkSecondary.Enabled)
				: !this.selection.IsEmpty);
			if (this.selection.IsEmpty)
			{
				this.lblSelection.Text = Localizer.T("選んでいるブロック : なし（左の一覧をドラッグして選んでください）");
			}
			else
			{
				int first = this.selection.Y * MapEditor.TileTransferSheetColumns + this.selection.X;
				this.lblSelection.Text = string.Format(Localizer.T("選んでいるブロック : 横 {0}×縦 {1}（左上は一覧の {2} 番目）"), this.selection.Width, this.selection.Height, first);
			}
		}

		//-------------------------------------------------------------------------------
		// 取り込み方が変わったときの処理
		//-------------------------------------------------------------------------------
		private void rbMode_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateUi();
		}

		//-------------------------------------------------------------------------------
		// 取り込むタイルセットのチェックが変わったときの処理
		//-------------------------------------------------------------------------------
		private void chkTileset_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateUi();
		}

		//-------------------------------------------------------------------------------
		// 一部だけ取り込むときの、ブロック一覧（タイルセット 1・2）の切り替え
		//-------------------------------------------------------------------------------
		private void rbPartSheet_CheckedChanged(object sender, EventArgs e)
		{
			this.selection = Rectangle.Empty;
			this.UpdateSheetSize();
			this.UpdateUi();
		}

		//-------------------------------------------------------------------------------
		// 今のブロック一覧の画像を返す処理（無ければ null）
		//-------------------------------------------------------------------------------
		private Bitmap CurrentSheet
		{
			get { return this.sheets[this.rbPartPrimary.Checked ? 0 : 1]; }
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧を描く部品の大きさを、画像の大きさ × 倍率にそろえる処理
		//-------------------------------------------------------------------------------
		private void UpdateSheetSize()
		{
			Bitmap sheet = this.CurrentSheet;
			this.pnlSheet.Size = sheet == null ? new Size(1, 1) : new Size(sheet.Width * SheetZoom, sheet.Height * SheetZoom);
			this.pnlSheet.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧（市松模様の上に拡大）・ブロックの区切り・選んでいる範囲を描く処理
		//-------------------------------------------------------------------------------
		private void pnlSheet_Paint(object sender, PaintEventArgs e)
		{
			Bitmap sheet = this.CurrentSheet;
			if (sheet == null)
			{
				return;
			}
			Graphics g = e.Graphics;
			int cell = BlockPixels * SheetZoom;
			using (SolidBrush light = new SolidBrush(Color.FromArgb(70, 70, 90)))
			using (SolidBrush dark = new SolidBrush(Color.FromArgb(50, 50, 66)))
			{
				for (int y = 0; y < this.pnlSheet.Height; y += 8)
				{
					for (int x = 0; x < this.pnlSheet.Width; x += 8)
					{
						g.FillRectangle(((x + y) / 8 % 2 == 0) ? light : dark, x, y, 8, 8);
					}
				}
			}
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(sheet, new Rectangle(0, 0, sheet.Width * SheetZoom, sheet.Height * SheetZoom));
			using (Pen grid = new Pen(Color.FromArgb(60, 255, 255, 255)))
			{
				for (int x = cell; x < this.pnlSheet.Width; x += cell)
				{
					g.DrawLine(grid, x, 0, x, this.pnlSheet.Height);
				}
				for (int y = cell; y < this.pnlSheet.Height; y += cell)
				{
					g.DrawLine(grid, 0, y, this.pnlSheet.Width, y);
				}
			}
			if (!this.selection.IsEmpty)
			{
				using (Pen pen = new Pen(UiTheme.Accent, 3))
				{
					g.DrawRectangle(pen, this.selection.X * cell + 1, this.selection.Y * cell + 1, this.selection.Width * cell - 2, this.selection.Height * cell - 2);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 画面の位置を、ブロック一覧の中のブロックの位置（範囲内に収めたもの）にする処理
		//-------------------------------------------------------------------------------
		private Point CellAt(Point location)
		{
			Bitmap sheet = this.CurrentSheet;
			int cell = BlockPixels * SheetZoom;
			int columns = sheet == null ? 1 : sheet.Width / BlockPixels;
			int rows = sheet == null ? 1 : sheet.Height / BlockPixels;
			return new Point(Math.Max(0, Math.Min(columns - 1, location.X / cell)), Math.Max(0, Math.Min(rows - 1, location.Y / cell)));
		}

		//-------------------------------------------------------------------------------
		// 2 つのブロックの位置を角とする範囲を選ぶ処理（マップチップ取り込みの大きさの上限までに縮める）
		//-------------------------------------------------------------------------------
		internal void SelectBlocks(Point a, Point b)
		{
			int max = ChipImportMaxPixels / BlockPixels;
			int left = Math.Min(a.X, b.X);
			int top = Math.Min(a.Y, b.Y);
			int width = Math.Min(max, Math.Abs(a.X - b.X) + 1);
			int height = Math.Min(max, Math.Abs(a.Y - b.Y) + 1);
			this.selection = new Rectangle(left, top, width, height);
			this.pnlSheet.Invalidate();
			this.UpdateUi();
		}

		//-------------------------------------------------------------------------------
		// ドラッグの始まり
		//-------------------------------------------------------------------------------
		private void pnlSheet_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left || this.CurrentSheet == null)
			{
				return;
			}
			this.dragging = true;
			this.dragStart = this.CellAt(e.Location);
			this.SelectBlocks(this.dragStart, this.dragStart);
		}

		//-------------------------------------------------------------------------------
		// ドラッグ中は範囲を広げる
		//-------------------------------------------------------------------------------
		private void pnlSheet_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.dragging)
			{
				this.SelectBlocks(this.dragStart, this.CellAt(e.Location));
			}
		}

		//-------------------------------------------------------------------------------
		// ドラッグの終わり
		//-------------------------------------------------------------------------------
		private void pnlSheet_MouseUp(object sender, MouseEventArgs e)
		{
			if (this.dragging)
			{
				this.dragging = false;
				this.SelectBlocks(this.dragStart, this.CellAt(e.Location));
			}
		}

		//-------------------------------------------------------------------------------
		// 選んでいる範囲のブロックの絵を切り出す処理（範囲が無ければ null）
		//-------------------------------------------------------------------------------
		internal Bitmap CropSelection()
		{
			Bitmap sheet = this.CurrentSheet;
			if (sheet == null || this.selection.IsEmpty)
			{
				return null;
			}
			Rectangle area = new Rectangle(this.selection.X * BlockPixels, this.selection.Y * BlockPixels, this.selection.Width * BlockPixels, this.selection.Height * BlockPixels);
			area.Intersect(new Rectangle(0, 0, sheet.Width, sheet.Height));
			return sheet.Clone(area, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		}

		//-------------------------------------------------------------------------------
		// 「取り込む」／「マップチップ取り込みへ…」の処理
		//-------------------------------------------------------------------------------
		private void btnRun_Click(object sender, EventArgs e)
		{
			if (this.package == null || this.host == null)
			{
				return;
			}
			if (this.rbModePart.Checked)
			{
				using (Bitmap part = this.CropSelection())
				{
					if (part != null)
					{
						this.host.OpenChipImportWithImage(part, true);
					}
				}
				return;
			}
			int[] created;
			string error = this.host.ImportMapTilePackage(this.package, this.chkPrimary.Checked && this.chkPrimary.Enabled, this.chkSecondary.Checked && this.chkSecondary.Enabled, out created);
			if (error != null)
			{
				MessageBox.Show(this, error, Localizer.T("マップタイルの取り込み"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			// 作った番号を「データ作成」タブ②の欄へ入れておく（続けて地形データを作れるように）
			this.host.SetGuideFooterTilesets(created);
			string message = Localizer.T("取り込みました。");
			for (int t = 0; t < 2; t++)
			{
				if (created[t] >= 0)
				{
					long header = MapEditor.TILESET_INDEX_START_OFFSET + (long)created[t] * MapEditor.TILESET_HEADER_SIZE;
					message += "\n" + string.Format(Localizer.T("タイルセット{0}: 番号 {1}（見出しのアドレス {2:X8}）"), t + 1, created[t], 0x08000000L + header);
				}
			}
			if (!string.IsNullOrEmpty(this.host.LastTileConversionNotes))
			{
				message += "\n\n" + this.host.LastTileConversionNotes;
			}
			message += "\n\n" + Localizer.T("「マップの設定」タブでマップのタイルセットに指定するか、「マップフッターを作成」でこの番号を指定して使ってください。ファイルへ書き出すには「ROMを保存」を押してください。");
			MessageBox.Show(this, message, Localizer.T("マップタイルの取り込み"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧の画像を片付ける処理
		//-------------------------------------------------------------------------------
		private void DisposeSheets()
		{
			for (int t = 0; t < 2; t++)
			{
				this.sheets[t]?.Dispose();
				this.sheets[t] = null;
			}
		}
	}
}
