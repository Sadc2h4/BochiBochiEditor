using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ブロック（マップチップ）選択パレットの表示・拡大・選択補助をまとめた処理群
	// パレット画像は 1 行 8 ブロック（16px 四方）で、第2タイルセットは第1タイルセットの直後から並ぶ
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		private const int PaletteBlockPixels = 16;
		private const int PaletteColumns = 8;
		private const int PaletteMaxScale = 4;
		private const int RecentBlockCapacity = 8;

		// 第1タイルセットのブロック数（CreateTilesetBitmap で設定）
		private int primaryBlockCount = 640;
		// 第1タイルセットで実際に使われているブロック数（枠の数より少ないことがある。残りは空として扱う）
		private int primaryUsedBlockCount = 640;

		// 0 のときは表示幅に合わせて自動で倍率を決める
		private int paletteFixedScale;

		// 横スクロールバーを使っているか（Visible は親が非表示だと常に false になるため別に持つ）
		private bool paletteHorizontalScrollActive;

		// マウスを乗せているブロック番号（無い時は -1）
		private int paletteHoverBlockId = -1;

		// 最近使ったブロック選択範囲（先頭が最新）
		private readonly List<Rectangle> recentBlockSelections = new List<Rectangle>();

		//-------------------------------------------------------------------------------
		// パレット関連の部品にイベントと初期値を設定する処理
		//-------------------------------------------------------------------------------
		private void InitializeBlockPaletteUI()
		{
			this.cmbPaletteZoom.Items.Clear();
			this.cmbPaletteZoom.Items.AddRange(new object[] { "幅に合わせる", "1倍", "2倍", "3倍", "4倍" });
			this.cmbPaletteZoom.SelectedIndex = 0;
			Localizer.RegisterComboItems(this.cmbPaletteZoom);
			this.cmbPaletteZoom.SelectedIndexChanged += this.cmbPaletteZoom_SelectedIndexChanged;
			this.pnlTilesetPalette.MouseWheel += this.pnlTilesetPalette_MouseWheel;
			this.pnlTilesetPalette.MouseMove += this.pnlTilesetPalette_HoverMove;
			this.pnlTilesetPalette.MouseLeave += this.pnlTilesetPalette_MouseLeave;
			this.pnlTilesetPalette.MouseEnter += this.pnlTilesetPalette_MouseEnter;
			this.pnlSelectedBlockPreview.Paint += this.pnlSelectedBlockPreview_Paint;
			this.pnlRecentBlocks.Paint += this.pnlRecentBlocks_Paint;
			this.pnlRecentBlocks.MouseDown += this.pnlRecentBlocks_MouseDown;
			this.pnlRecentBlocks.MouseMove += this.pnlRecentBlocks_MouseMove;
			EnableDoubleBuffer(this.pnlTilesetPalette);
			EnableDoubleBuffer(this.pnlSelectedBlockPreview);
			EnableDoubleBuffer(this.pnlRecentBlocks);
			UiTheme.MarkCanvas(this.pnlSelectedBlockPreview);
			UiTheme.MarkCanvas(this.pnlRecentBlocks);
			this.UpdatePaletteInfoLabel();
		}

		//-------------------------------------------------------------------------------
		// パネルのちらつきを抑えるためダブルバッファを有効にする処理
		//-------------------------------------------------------------------------------
		private static void EnableDoubleBuffer(Control control)
		{
			typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(control, true, null);
		}

		//-------------------------------------------------------------------------------
		// パレットの並べ方（倍率・段の数・1 段の行数）
		// 幅が広いときは、8 列のまとまりを保ったまま行を段に分けて横に並べる（1 段目の続きが 2 段目の先頭）
		//-------------------------------------------------------------------------------
		private struct PaletteLayout
		{
			public int Scale;
			public int Cell;
			public int Bands;
			public int RowsPerBand;
			public int Gap;

			// 1 段の幅と、段の先頭どうしの間隔
			public int BandWidth { get { return PaletteColumns * this.Cell; } }
			public int BandPitch { get { return this.BandWidth + this.Gap; } }
			public int ContentWidth { get { return this.Bands * this.BandWidth + (this.Bands - 1) * this.Gap; } }
			public int ContentHeight { get { return this.RowsPerBand * this.Cell; } }
		}

		//-------------------------------------------------------------------------------
		// パレットの行数（8 ブロックで 1 行）を返す処理
		//-------------------------------------------------------------------------------
		private int GetPaletteRowCount()
		{
			return this.blockPaletteBitmap != null ? this.blockPaletteBitmap.Height / PaletteBlockPixels : 0;
		}

		//-------------------------------------------------------------------------------
		// 今の表示領域に合わせたパレットの並べ方を求める処理
		// 「幅に合わせる」のときは、下へスクロールしなくても全部見える中でいちばん大きい倍率を選ぶ
		// どの倍率でも収まらなければ、幅に合わせた倍率（従来どおり）で、入るだけの段に並べる
		//-------------------------------------------------------------------------------
		private PaletteLayout GetPaletteLayout()
		{
			int width = Math.Max(1, this.pnlTilesetPalette.ClientSize.Width);
			int height = Math.Max(1, this.pnlTilesetPalette.ClientSize.Height);
			int rows = Math.Max(1, this.GetPaletteRowCount());
			int gap = this.pnlTilesetPalette.LogicalToDeviceUnits(10);
			if (this.paletteFixedScale > 0)
			{
				return MakePaletteLayout(this.paletteFixedScale, width, rows, gap);
			}
			for (int scale = PaletteMaxScale; scale >= 1; scale--)
			{
				PaletteLayout fit = MakePaletteLayout(scale, width, rows, gap);
				if (fit.ContentHeight <= height && fit.BandWidth <= width)
				{
					return fit;
				}
			}
			int widthScale = Math.Max(1, Math.Min(PaletteMaxScale, width / (PaletteColumns * PaletteBlockPixels)));
			return MakePaletteLayout(widthScale, width, rows, gap);
		}

		//-------------------------------------------------------------------------------
		// 倍率を決めたときの並べ方（幅に入るだけの段に分ける。空の段は作らない）を求める処理
		//-------------------------------------------------------------------------------
		private static PaletteLayout MakePaletteLayout(int scale, int width, int rows, int gap)
		{
			PaletteLayout layout = new PaletteLayout { Scale = scale, Cell = PaletteBlockPixels * scale, Gap = gap };
			int bands = Math.Max(1, (width + gap) / layout.BandPitch);
			bands = Math.Min(bands, rows);
			layout.RowsPerBand = (rows + bands - 1) / bands;
			// 行数が割り切れず最後の段が空になる分は減らす
			layout.Bands = (rows + layout.RowsPerBand - 1) / layout.RowsPerBand;
			return layout;
		}

		//-------------------------------------------------------------------------------
		// 現在のパレット表示倍率を返す処理
		//-------------------------------------------------------------------------------
		private int GetPaletteScale()
		{
			return this.GetPaletteLayout().Scale;
		}

		//-------------------------------------------------------------------------------
		// パレット上の範囲（列・行）を、画面上の四角に直す処理（段の境目をまたぐ範囲は段ごとに分ける）
		//-------------------------------------------------------------------------------
		private List<Rectangle> GetPaletteDisplayRects(Rectangle cells, PaletteLayout layout, int scrollX, int scrollY)
		{
			List<Rectangle> result = new List<Rectangle>();
			for (int band = 0; band < layout.Bands; band++)
			{
				int first = Math.Max(cells.Top, band * layout.RowsPerBand);
				int last = Math.Min(cells.Bottom, (band + 1) * layout.RowsPerBand);
				if (first >= last)
				{
					continue;
				}
				result.Add(new Rectangle(band * layout.BandPitch + cells.X * layout.Cell - scrollX, (first - band * layout.RowsPerBand) * layout.Cell - scrollY,
					cells.Width * layout.Cell, (last - first) * layout.Cell));
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 表示倍率の選択が変わったときに再描画する処理
		//-------------------------------------------------------------------------------
		private void cmbPaletteZoom_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.SetPaletteFixedScale(this.cmbPaletteZoom.SelectedIndex);
		}

		//-------------------------------------------------------------------------------
		// パレットの表示倍率を変更し、選択中ブロックが見える位置を保つ処理
		//-------------------------------------------------------------------------------
		private void SetPaletteFixedScale(int scale)
		{
			scale = Math.Max(0, Math.Min(PaletteMaxScale, scale));
			if (this.paletteFixedScale == scale)
			{
				return;
			}
			this.paletteFixedScale = scale;
			if (this.cmbPaletteZoom.SelectedIndex != scale)
			{
				this.cmbPaletteZoom.SelectedIndex = scale;
			}
			this.UpdateTilesetPaletteScrollRange();
			this.EnsurePaletteRowVisible(this.selectedBlockRect.Y);
			this.pnlTilesetPalette.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// パレット画像を作り直した直後にスクロール位置を先頭へ戻す処理
		//-------------------------------------------------------------------------------
		private void ResetTilesetPaletteScroll()
		{
			this.vsbTilesetScroll.Minimum = 0;
			this.hsbTilesetScroll.Minimum = 0;
			this.vsbTilesetScroll.Value = 0;
			this.hsbTilesetScroll.Value = 0;
			this.UpdateTilesetPaletteScrollRange();
			this.recentBlockSelections.Clear();
			this.pnlRecentBlocks.Invalidate();
			this.UpdatePaletteInfoLabel();
			this.pnlTilesetPalette.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// パレットのスクロールバーの範囲を、表示倍率と表示領域に合わせて更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateTilesetPaletteScrollRange()
		{
			PaletteLayout layout = this.GetPaletteLayout();
			int viewWidth = Math.Max(1, this.pnlTilesetPalette.ClientSize.Width);
			int viewHeight = Math.Max(1, this.pnlTilesetPalette.ClientSize.Height);
			int contentWidth = (this.blockPaletteBitmap != null) ? layout.ContentWidth : 0;
			int contentHeight = (this.blockPaletteBitmap != null) ? layout.ContentHeight : 0;
			ConfigurePaletteScrollBar(this.vsbTilesetScroll, contentHeight, viewHeight, layout.Cell);
			ConfigurePaletteScrollBar(this.hsbTilesetScroll, contentWidth, viewWidth, layout.Cell);
			this.paletteHorizontalScrollActive = contentWidth > viewWidth;
			this.hsbTilesetScroll.Visible = this.paletteHorizontalScrollActive;
		}

		//-------------------------------------------------------------------------------
		// スクロールバー 1 本分の最大値・ページ量・現在値を設定する処理
		//-------------------------------------------------------------------------------
		private static void ConfigurePaletteScrollBar(ScrollBar bar, int content, int view, int step)
		{
			bar.SmallChange = Math.Max(1, step);
			bar.LargeChange = Math.Max(1, view);
			bar.Maximum = Math.Max(0, content - 1);
			bar.Enabled = content > view;
			int maxValue = Math.Max(0, bar.Maximum - bar.LargeChange + 1);
			if (bar.Value > maxValue)
			{
				bar.Value = maxValue;
			}
		}

		//-------------------------------------------------------------------------------
		// 指定行のブロックがパレットの表示範囲に入るようスクロールする処理
		//-------------------------------------------------------------------------------
		private void EnsurePaletteRowVisible(int row)
		{
			PaletteLayout layout = this.GetPaletteLayout();
			int cell = layout.Cell;
			// 段に分けているときは、その行が入っている段の中での高さで見る
			int top = (row % Math.Max(1, layout.RowsPerBand)) * cell;
			int view = this.pnlTilesetPalette.ClientSize.Height;
			int value = this.vsbTilesetScroll.Value;
			if (top < value)
			{
				value = top;
			}
			else if (top + cell > value + view)
			{
				value = top + cell - view;
			}
			int maxValue = Math.Max(0, this.vsbTilesetScroll.Maximum - this.vsbTilesetScroll.LargeChange + 1);
			this.vsbTilesetScroll.Value = Math.Max(0, Math.Min(maxValue, value));
		}

		//-------------------------------------------------------------------------------
		// パレットを描画する処理（拡大・グリッド・タイルセット境界・選択枠・ホバー枠）
		//-------------------------------------------------------------------------------
		private void pnlTilesetPalette_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			if (this.blockPaletteBitmap == null)
			{
				TextRenderer.DrawText(g, Localizer.T("マップを選ぶとブロックが表示されます"), this.Font, this.pnlTilesetPalette.ClientRectangle, UiTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
				return;
			}
			PaletteLayout layout = this.GetPaletteLayout();
			int cell = layout.Cell;
			int scrollX = this.paletteHorizontalScrollActive ? this.hsbTilesetScroll.Value : 0;
			int scrollY = this.vsbTilesetScroll.Value;
			int rows = this.GetPaletteRowCount();
			int visibleRight = Math.Min(layout.ContentWidth - scrollX, this.pnlTilesetPalette.ClientSize.Width);

			// 段ごとに、その段の行を絵から切り出して並べる（拡大はドット感を保つため最近傍補間で描く）
			for (int band = 0; band < layout.Bands; band++)
			{
				int firstRow = band * layout.RowsPerBand;
				int bandRows = Math.Min(layout.RowsPerBand, rows - firstRow);
				if (bandRows <= 0)
				{
					continue;
				}
				int left = band * layout.BandPitch - scrollX;
				int bandHeight = bandRows * cell;
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.DrawImage(this.blockPaletteBitmap, new Rectangle(left, -scrollY, layout.BandWidth, bandHeight),
					new Rectangle(0, firstRow * PaletteBlockPixels, PaletteColumns * PaletteBlockPixels, bandRows * PaletteBlockPixels), GraphicsUnit.Pixel);
				g.PixelOffsetMode = PixelOffsetMode.Default;
				if (this.chkShowGrid.Checked)
				{
					int bottom = Math.Min(bandHeight - scrollY, this.pnlTilesetPalette.ClientSize.Height);
					using (Pen grid = new Pen(Color.FromArgb(70, 255, 255, 255)))
					{
						for (int x = 0; x <= layout.BandWidth; x += cell)
						{
							g.DrawLine(grid, left + x, Math.Max(0, -scrollY), left + x, bottom);
						}
						for (int y = 0; y <= bandHeight; y += cell)
						{
							int sy = y - scrollY;
							if (sy >= 0 && sy <= bottom)
							{
								g.DrawLine(grid, left, sy, left + layout.BandWidth, sy);
							}
						}
					}
				}
			}

			this.DrawPaletteTilesetBoundary(g, layout, scrollX, scrollY, visibleRight);
			this.DrawPartBrushPaletteMask(g, layout, scrollX, scrollY);

			if (this.paletteHoverBlockId >= 0)
			{
				Rectangle hoverCell = new Rectangle(this.paletteHoverBlockId % PaletteColumns, this.paletteHoverBlockId / PaletteColumns, 1, 1);
				using (Pen pen = new Pen(Color.FromArgb(200, 255, 255, 255), 1f))
				{
					foreach (Rectangle hover in this.GetPaletteDisplayRects(hoverCell, layout, scrollX, scrollY))
					{
						g.DrawRectangle(pen, hover.X, hover.Y, hover.Width - 1, hover.Height - 1);
					}
				}
			}

			// 選択枠（段の境目をまたぐ選択は、段ごとに分けて囲む）
			using (Pen outer = new Pen(Color.Black, 3f))
			using (Pen inner = new Pen(Color.FromArgb(255, 214, 64), 2f))
			{
				foreach (Rectangle sel in this.GetPaletteDisplayRects(this.selectedBlockRect, layout, scrollX, scrollY))
				{
					g.DrawRectangle(outer, sel.X + 1, sel.Y + 1, sel.Width - 2, sel.Height - 2);
					g.DrawRectangle(inner, sel.X + 1, sel.Y + 1, sel.Width - 2, sel.Height - 2);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 第1タイルセットと第2タイルセットの境目に線と見出しを描く処理
		//-------------------------------------------------------------------------------
		private void DrawPaletteTilesetBoundary(Graphics g, PaletteLayout layout, int scrollX, int scrollY, int visibleRight)
		{
			if (this.primaryBlockCount <= 0 || this.primaryBlockCount >= this.totalBlocks)
			{
				return;
			}
			int cell = layout.Cell;
			int row = (this.primaryBlockCount + PaletteColumns - 1) / PaletteColumns;
			// 境目の行が入っている段の中に線を引く
			int band = Math.Min(layout.Bands - 1, row / Math.Max(1, layout.RowsPerBand));
			int left = band * layout.BandPitch - scrollX;
			int y = (row - band * layout.RowsPerBand) * cell - scrollY;
			if (y < -cell || y > this.pnlTilesetPalette.ClientSize.Height + cell)
			{
				return;
			}
			using (Pen line = new Pen(UiTheme.Accent, 2f))
			{
				g.DrawLine(line, left, y, Math.Min(left + layout.BandWidth, visibleRight), y);
			}
			// 見出しはブロックに重ねず、その段の右側の余白に出す（余白が足りなければ線だけ）
			string text = string.Format("TS2 0x{0:X3}〜", this.primaryBlockCount);
			Size size = TextRenderer.MeasureText(g, text, this.Font);
			int contentRight = left + layout.BandWidth;
			int nextLeft = (band + 1 < layout.Bands) ? (band + 1) * layout.BandPitch - scrollX : this.pnlTilesetPalette.ClientSize.Width;
			int margin = nextLeft - contentRight;
			if (margin >= size.Width + 8)
			{
				Rectangle label = new Rectangle(contentRight + 4, y - size.Height / 2, size.Width + 2, size.Height);
				using (SolidBrush back = new SolidBrush(UiTheme.Accent))
				{
					g.FillRectangle(back, label);
				}
				TextRenderer.DrawText(g, text, this.Font, label, UiTheme.AccentText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}

		//-------------------------------------------------------------------------------
		// パレット上の座標を有効なブロック位置（セル座標とブロック番号）へ変換する処理
		//-------------------------------------------------------------------------------
		private bool TryGetTilesetPaletteCell(Point point, out int cellX, out int cellY, out int blockId)
		{
			cellX = 0;
			cellY = 0;
			blockId = -1;
			if (this.blockPaletteBitmap == null || this.totalBlocks <= 0)
			{
				return false;
			}
			PaletteLayout layout = this.GetPaletteLayout();
			int cell = layout.Cell;
			int x = point.X + (this.paletteHorizontalScrollActive ? this.hsbTilesetScroll.Value : 0);
			int y = point.Y + this.vsbTilesetScroll.Value;
			if (x < 0 || y < 0 || y >= layout.ContentHeight)
			{
				return false;
			}
			// どの段の、どの列か（段と段の間の隙間は外れ）
			int band = x / layout.BandPitch;
			int inBand = x % layout.BandPitch;
			if (band >= layout.Bands || inBand >= layout.BandWidth)
			{
				return false;
			}
			cellX = inBand / cell;
			cellY = band * layout.RowsPerBand + y / cell;
			if (cellY >= this.GetPaletteRowCount())
			{
				return false;
			}
			blockId = cellY * PaletteColumns + cellX;
			return blockId >= 0 && blockId < this.totalBlocks;
		}

		//-------------------------------------------------------------------------------
		// ホイールで上下スクロール、Ctrl+ホイールで表示倍率を変更する処理
		//-------------------------------------------------------------------------------
		private void pnlTilesetPalette_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e is HandledMouseEventArgs handled)
			{
				handled.Handled = true;
			}
			if (this.blockPaletteBitmap == null)
			{
				return;
			}
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
			{
				int current = this.GetPaletteScale();
				this.SetPaletteFixedScale(current + (e.Delta > 0 ? 1 : -1) < 1 ? 1 : current + (e.Delta > 0 ? 1 : -1));
				return;
			}
			int rows = Math.Max(1, SystemInformation.MouseWheelScrollLines);
			int step = rows * PaletteBlockPixels * this.GetPaletteScale() * (e.Delta > 0 ? -1 : 1);
			int maxValue = Math.Max(0, this.vsbTilesetScroll.Maximum - this.vsbTilesetScroll.LargeChange + 1);
			this.vsbTilesetScroll.Value = Math.Max(0, Math.Min(maxValue, this.vsbTilesetScroll.Value + step));
			this.UpdatePaletteHover(e.Location);
			this.pnlTilesetPalette.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// パレット上のマウス位置からホバー中のブロックを更新する処理
		//-------------------------------------------------------------------------------
		private void pnlTilesetPalette_HoverMove(object sender, MouseEventArgs e)
		{
			this.UpdatePaletteHover(e.Location);
		}

		//-------------------------------------------------------------------------------
		// パレットにマウスが入ったらホイール操作を受けられるようフォーカスを移す処理
		//-------------------------------------------------------------------------------
		private void pnlTilesetPalette_MouseEnter(object sender, EventArgs e)
		{
			if (!this.IsTextInputFocused() && this.pnlTilesetPalette.FindForm() == Form.ActiveForm)
			{
				this.pnlTilesetPalette.Focus();
			}
		}

		//-------------------------------------------------------------------------------
		// パレットからマウスが離れたらホバー表示を消す処理
		//-------------------------------------------------------------------------------
		private void pnlTilesetPalette_MouseLeave(object sender, EventArgs e)
		{
			if (this.paletteHoverBlockId != -1)
			{
				this.paletteHoverBlockId = -1;
				this.pnlTilesetPalette.Invalidate();
				this.UpdatePaletteInfoLabel();
			}
		}

		//-------------------------------------------------------------------------------
		// ホバー中のブロック番号を更新し、情報欄と枠を再描画する処理
		//-------------------------------------------------------------------------------
		private void UpdatePaletteHover(Point location)
		{
			int blockId;
			if (!this.TryGetTilesetPaletteCell(location, out _, out _, out blockId))
			{
				blockId = -1;
			}
			if (blockId == this.paletteHoverBlockId)
			{
				return;
			}
			this.paletteHoverBlockId = blockId;
			this.pnlTilesetPalette.Invalidate();
			this.UpdatePaletteInfoLabel();
		}

		//-------------------------------------------------------------------------------
		// パレット下の情報欄（ホバー中ブロック、またはブロック数の内訳）を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdatePaletteInfoLabel()
		{
			if (this.lblPaletteInfo == null)
			{
				return;
			}
			if (this.blockPaletteBitmap == null)
			{
				this.lblPaletteInfo.Text = Localizer.T("ホイール: スクロール / Ctrl+ホイール: 拡大縮小");
				return;
			}
			if (this.paletteHoverBlockId >= 0)
			{
				bool primary = this.paletteHoverBlockId < this.primaryBlockCount;
				string text = string.Format(Localizer.T("ブロック 0x{0:X3} ({0})　{1}"), this.paletteHoverBlockId, primary ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"));
				// 2 行目に、踏んだときの効果（挙動）と重ね方を出す（MapEditor.BlockInfo.cs）
				string behavior = this.DescribeBlockBehavior(this.paletteHoverBlockId);
				this.lblPaletteInfo.Text = behavior != null ? text + Environment.NewLine + behavior : text;
				return;
			}
			int secondary = Math.Max(0, this.totalBlocks - this.primaryBlockCount);
			this.lblPaletteInfo.Text = string.Format(Localizer.T("タイルセット1: {0} / タイルセット2: {1} ブロック"), this.primaryUsedBlockCount, secondary);
		}

		//-------------------------------------------------------------------------------
		// 選択中ブロック（範囲）を拡大して表示する処理
		//-------------------------------------------------------------------------------
		private void pnlSelectedBlockPreview_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle area = this.pnlSelectedBlockPreview.ClientRectangle;
			if (this.blockPaletteBitmap == null)
			{
				TextRenderer.DrawText(g, Localizer.T("選択中のブロック"), this.Font, area, UiTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				return;
			}
			Rectangle src = new Rectangle(this.selectedBlockRect.X * PaletteBlockPixels, this.selectedBlockRect.Y * PaletteBlockPixels, this.selectedBlockRect.Width * PaletteBlockPixels, this.selectedBlockRect.Height * PaletteBlockPixels);
			src.Intersect(new Rectangle(Point.Empty, this.blockPaletteBitmap.Size));
			if (src.Width <= 0 || src.Height <= 0)
			{
				return;
			}
			int pad = 6;
			int scale = Math.Max(1, Math.Min((area.Width - pad * 2) / src.Width, (area.Height - pad * 2) / src.Height));
			scale = Math.Min(scale, 4);
			Size size = new Size(src.Width * scale, src.Height * scale);
			Rectangle dest = new Rectangle(area.Left + (area.Width - size.Width) / 2, area.Top + (area.Height - size.Height) / 2, size.Width, size.Height);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(this.blockPaletteBitmap, dest, src, GraphicsUnit.Pixel);
			g.PixelOffsetMode = PixelOffsetMode.Default;
			using (Pen pen = new Pen(UiTheme.Border))
			{
				g.DrawRectangle(pen, dest.X - 1, dest.Y - 1, dest.Width + 1, dest.Height + 1);
			}
		}

		//-------------------------------------------------------------------------------
		// 現在の選択範囲を「最近使ったブロック」の先頭へ登録する処理
		//-------------------------------------------------------------------------------
		private void PushRecentBlockSelection()
		{
			if (this.blockPaletteBitmap == null)
			{
				return;
			}
			Rectangle current = this.selectedBlockRect;
			this.recentBlockSelections.Remove(current);
			this.recentBlockSelections.Insert(0, current);
			if (this.recentBlockSelections.Count > RecentBlockCapacity)
			{
				this.recentBlockSelections.RemoveRange(RecentBlockCapacity, this.recentBlockSelections.Count - RecentBlockCapacity);
			}
			this.pnlRecentBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 「最近使ったブロック」の 1 枠分の表示位置を返す処理
		//-------------------------------------------------------------------------------
		private Rectangle GetRecentBlockSlot(int index)
		{
			int size = this.pnlRecentBlocks.ClientSize.Height - 6;
			return new Rectangle(3 + index * (size + 4), 3, size, size);
		}

		//-------------------------------------------------------------------------------
		// 「最近使ったブロック」の一覧を描画する処理
		//-------------------------------------------------------------------------------
		private void pnlRecentBlocks_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			if (this.blockPaletteBitmap == null || this.recentBlockSelections.Count == 0)
			{
				TextRenderer.DrawText(g, Localizer.T("最近使ったブロック（選ぶとここに並びます）"), this.Font, this.pnlRecentBlocks.ClientRectangle, UiTheme.TextMuted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
				return;
			}
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			for (int i = 0; i < this.recentBlockSelections.Count; i++)
			{
				Rectangle slot = this.GetRecentBlockSlot(i);
				if (slot.Right > this.pnlRecentBlocks.ClientSize.Width)
				{
					break;
				}
				Rectangle r = this.recentBlockSelections[i];
				Rectangle src = new Rectangle(r.X * PaletteBlockPixels, r.Y * PaletteBlockPixels, r.Width * PaletteBlockPixels, r.Height * PaletteBlockPixels);
				src.Intersect(new Rectangle(Point.Empty, this.blockPaletteBitmap.Size));
				if (src.Width > 0 && src.Height > 0)
				{
					g.DrawImage(this.blockPaletteBitmap, slot, src, GraphicsUnit.Pixel);
				}
				bool isCurrent = r == this.selectedBlockRect;
				using (Pen pen = new Pen(isCurrent ? Color.FromArgb(255, 214, 64) : UiTheme.Border, isCurrent ? 2f : 1f))
				{
					g.DrawRectangle(pen, slot.X, slot.Y, slot.Width - 1, slot.Height - 1);
				}
			}
			g.PixelOffsetMode = PixelOffsetMode.Default;
		}

		//-------------------------------------------------------------------------------
		// 「最近使ったブロック」をクリックして選択し直す処理
		//-------------------------------------------------------------------------------
		private void pnlRecentBlocks_MouseDown(object sender, MouseEventArgs e)
		{
			for (int i = 0; i < this.recentBlockSelections.Count; i++)
			{
				if (this.GetRecentBlockSlot(i).Contains(e.Location))
				{
					this.selectedBlockRect = this.recentBlockSelections[i];
					this.selectionAnchor = new Point(this.selectedBlockRect.X, this.selectedBlockRect.Y);
					this.SetEditorMode(this.tabBlock);
					this.EnsurePaletteRowVisible(this.selectedBlockRect.Y);
					this.UpdateBlockIndexLabel();
					this.pnlTilesetPalette.Invalidate();
					this.pnlRecentBlocks.Invalidate();
					return;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 「最近使ったブロック」にマウスを乗せたときにブロック番号をツールチップ表示する処理
		//-------------------------------------------------------------------------------
		private void pnlRecentBlocks_MouseMove(object sender, MouseEventArgs e)
		{
			string tip = null;
			for (int i = 0; i < this.recentBlockSelections.Count; i++)
			{
				if (this.GetRecentBlockSlot(i).Contains(e.Location))
				{
					Rectangle r = this.recentBlockSelections[i];
					int id = r.Y * PaletteColumns + r.X;
					tip = (r.Width > 1 || r.Height > 1) ? string.Format(Localizer.T("0x{0:X3} から {1}×{2}"), id, r.Width, r.Height) : string.Format("0x{0:X3}", id);
					break;
				}
			}
			if (this.mapToolTip.GetToolTip(this.pnlRecentBlocks) != (tip ?? string.Empty))
			{
				this.mapToolTip.SetToolTip(this.pnlRecentBlocks, tip);
			}
		}
	}
}
