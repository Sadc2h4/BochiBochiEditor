using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ブロックの一覧を表示して、クリックで 1 つ選ばせるウィンドウ（取り込みウィザードの下地ブロック選択用）
	//-------------------------------------------------------------------------------
	public partial class BlockPickerForm : Form
	{
		// 一覧の並び（マップ画面のパレットと同じ横 8 ブロック）
		private const int Columns = 8;

		// 全層の絵・下層だけの絵（下層だけの絵が無いときは全層の絵だけを使う）
		private readonly Bitmap fullImage;
		private readonly Bitmap bottomImage;
		// 上層にも絵があるブロック（右上に印を付ける）
		private readonly bool[] hasTop;
		private readonly int totalBlocks;
		private readonly int primaryBlocks;
		private int selectedBlock = -1;
		private int hoveredBlock = -1;

		//-------------------------------------------------------------------------------
		// デザイナー用の初期化処理
		//-------------------------------------------------------------------------------
		public BlockPickerForm()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// 一覧に出す絵とブロック数、最初に選んでおくブロックを受け取って初期化する処理
		// layerHint を渡すと、印の説明（下地に使われる層の案内）をその文に置き換える
		//-------------------------------------------------------------------------------
		internal BlockPickerForm(Bitmap full, Bitmap bottom, bool[] hasTop, int total, int primary, int selected, string layerHint = null) : this()
		{
			this.fullImage = full;
			this.bottomImage = bottom;
			this.hasTop = hasTop;
			this.totalBlocks = total;
			this.primaryBlocks = primary;
			this.selectedBlock = (selected >= 0 && selected < total) ? selected : -1;
			typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(this.pnlPickerBlocks, true, null);
			// 下層だけの絵が無い場合は、全層の絵しか出せないので切り替えを隠す
			this.chkPickerShowTop.Visible = bottom != null && full != null;
			Localizer.Apply(this);
			this.lblPickerHint.Text = Localizer.T("クリックで選び、ダブルクリックか「決定」で確定します。") + Environment.NewLine + (layerHint ?? Localizer.T("右上の印は、上層にも絵があるブロックです（下地に使われるのは下層だけ）。"));
			this.UpdateContentSize();
			this.UpdateSelectedLabel();
		}

		//-------------------------------------------------------------------------------
		// 画面の拡大率に合わせた後で、ウィンドウの幅を一覧に合わせる処理
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.FitWidthToBlocks();
		}

		//-------------------------------------------------------------------------------
		// ウィンドウの幅を、一覧（横 8 ブロック）とスクロールバーがちょうど収まる幅にする処理
		//-------------------------------------------------------------------------------
		private void FitWidthToBlocks()
		{
			int border = this.pnlPickerBlocks.Width - this.pnlPickerBlocks.ClientSize.Width;
			int wanted = this.pnlPickerBlocks.AutoScrollMinSize.Width + SystemInformation.VerticalScrollBarWidth + border + this.Padding.Horizontal;
			this.ClientSize = new Size(Math.Max(wanted, this.MinimumSize.Width - (this.Width - this.ClientSize.Width)), this.ClientSize.Height);
		}

		//-------------------------------------------------------------------------------
		// 選ばれたブロックの番号（選ばれていなければ -1）
		//-------------------------------------------------------------------------------
		public int SelectedBlock
		{
			get { return this.selectedBlock; }
		}

		//-------------------------------------------------------------------------------
		// 表示後に、選択中のブロックが見える位置までスクロールし、一覧の下地の色を整える処理
		//-------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			UiTheme.MarkCanvas(this.pnlPickerBlocks);
			if (this.selectedBlock >= 0)
			{
				int cell = this.CellSize;
				int y = this.selectedBlock / Columns * cell - (this.pnlPickerBlocks.ClientSize.Height - cell) / 2;
				this.pnlPickerBlocks.AutoScrollPosition = new Point(0, Math.Max(0, y));
			}
			this.pnlPickerBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 1 ブロックの表示の大きさ（16px を 2 倍にし、画面の拡大率に合わせる）
		//-------------------------------------------------------------------------------
		private int CellSize
		{
			get { return this.LogicalToDeviceUnits(32); }
		}

		//-------------------------------------------------------------------------------
		// 今表示する一覧の絵（上層も表示するか、下層だけか）を返す処理
		//-------------------------------------------------------------------------------
		private Bitmap CurrentImage
		{
			get { return (this.bottomImage != null && !this.chkPickerShowTop.Checked) ? this.bottomImage : this.fullImage; }
		}

		//-------------------------------------------------------------------------------
		// 一覧全体の大きさをスクロール範囲に設定する処理
		//-------------------------------------------------------------------------------
		private void UpdateContentSize()
		{
			int cell = this.CellSize;
			int rows = (this.totalBlocks + Columns - 1) / Columns;
			this.pnlPickerBlocks.AutoScrollMinSize = new Size(Columns * cell, rows * cell);
		}

		//-------------------------------------------------------------------------------
		// 選択中・カーソル位置のブロック番号を下の欄に出す処理
		//-------------------------------------------------------------------------------
		private void UpdateSelectedLabel()
		{
			int shown = Math.Max(0, this.selectedBlock);
			this.lblPickerSelected.Text = this.hoveredBlock >= 0
				? string.Format(Localizer.T("選択中: 0x{0:X3}\r\nカーソル: 0x{1:X3}"), shown, this.hoveredBlock)
				: string.Format(Localizer.T("選択中: 0x{0:X3}"), shown);
			this.btnPickerOK.Enabled = this.selectedBlock >= 0;
		}

		//-------------------------------------------------------------------------------
		// 一覧上の座標からブロック番号を求める処理（ブロックの無い所は -1）
		//-------------------------------------------------------------------------------
		private int HitTest(Point point)
		{
			int cell = this.CellSize;
			int x = point.X - this.pnlPickerBlocks.AutoScrollPosition.X;
			int y = point.Y - this.pnlPickerBlocks.AutoScrollPosition.Y;
			if (x < 0 || y < 0 || x >= Columns * cell)
			{
				return -1;
			}
			int id = y / cell * Columns + x / cell;
			return id < this.totalBlocks ? id : -1;
		}

		//-------------------------------------------------------------------------------
		// ブロックの一覧を描く処理（選択中は黄色、カーソル位置は白の枠。第1と第2の境目に線）
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(UiTheme.Canvas);
			Bitmap image = this.CurrentImage;
			if (image == null)
			{
				return;
			}
			int cell = this.CellSize;
			Point scroll = this.pnlPickerBlocks.AutoScrollPosition;
			g.TranslateTransform(scroll.X, scroll.Y);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			// 見えている行だけを描く
			int rows = (this.totalBlocks + Columns - 1) / Columns;
			int firstRow = Math.Max(0, -scroll.Y / cell);
			int lastRow = Math.Min(rows - 1, (-scroll.Y + this.pnlPickerBlocks.ClientSize.Height) / cell);
			for (int row = firstRow; row <= lastRow; row++)
			{
				int sourceY = row * 16;
				if (sourceY + 16 > image.Height)
				{
					break;
				}
				g.DrawImage(image, new Rectangle(0, row * cell, Columns * cell, cell), new Rectangle(0, sourceY, Columns * 16, 16), GraphicsUnit.Pixel);
			}
			g.PixelOffsetMode = PixelOffsetMode.Default;

			// 上層にも絵があるブロックの印（右上の小さな三角）
			if (this.hasTop != null)
			{
				int mark = Math.Max(4, cell / 6);
				using (SolidBrush brush = new SolidBrush(UiTheme.Accent))
				{
					for (int row = firstRow; row <= lastRow; row++)
					{
						for (int column = 0; column < Columns; column++)
						{
							int id = row * Columns + column;
							if (id < this.hasTop.Length && this.hasTop[id])
							{
								int right = (column + 1) * cell;
								int top = row * cell;
								g.FillPolygon(brush, new[] { new Point(right - mark, top), new Point(right, top), new Point(right, top + mark) });
							}
						}
					}
				}
			}

			// 第1タイルセットと第2タイルセットの境目
			if (this.primaryBlocks > 0 && this.primaryBlocks < this.totalBlocks)
			{
				int y = (this.primaryBlocks + Columns - 1) / Columns * cell;
				using (Pen line = new Pen(UiTheme.Accent, 2f))
				{
					g.DrawLine(line, 0, y, Columns * cell, y);
				}
			}

			// カーソル位置と選択中のブロック
			if (this.hoveredBlock >= 0 && this.hoveredBlock != this.selectedBlock)
			{
				using (Pen pen = new Pen(Color.White, 2f))
				{
					g.DrawRectangle(pen, this.CellRect(this.hoveredBlock, cell));
				}
			}
			if (this.selectedBlock >= 0)
			{
				using (Pen pen = new Pen(Color.FromArgb(255, 214, 64), 3f))
				{
					g.DrawRectangle(pen, this.CellRect(this.selectedBlock, cell));
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ブロックの枠（一覧の中の位置、内側に 1px 寄せる）を返す処理
		//-------------------------------------------------------------------------------
		private Rectangle CellRect(int id, int cell)
		{
			return new Rectangle(id % Columns * cell + 1, id / Columns * cell + 1, cell - 2, cell - 2);
		}

		//-------------------------------------------------------------------------------
		// マウスを動かしたら、カーソル位置のブロックを枠で示す処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_MouseMove(object sender, MouseEventArgs e)
		{
			int id = this.HitTest(e.Location);
			if (id != this.hoveredBlock)
			{
				this.hoveredBlock = id;
				this.UpdateSelectedLabel();
				this.pnlPickerBlocks.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// 一覧に入ったらホイールで動かせるようにし、出たらカーソル位置の枠を消す処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_MouseEnter(object sender, EventArgs e)
		{
			this.pnlPickerBlocks.Focus();
		}

		//-------------------------------------------------------------------------------
		// 一覧から出たらカーソル位置の枠を消す処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_MouseLeave(object sender, EventArgs e)
		{
			this.hoveredBlock = -1;
			this.UpdateSelectedLabel();
			this.pnlPickerBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// クリックしたブロックを選ぶ処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_MouseClick(object sender, MouseEventArgs e)
		{
			int id = this.HitTest(e.Location);
			if (id >= 0)
			{
				this.selectedBlock = id;
				this.UpdateSelectedLabel();
				this.pnlPickerBlocks.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// ダブルクリックしたブロックで確定する処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			int id = this.HitTest(e.Location);
			if (id >= 0)
			{
				this.selectedBlock = id;
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		//-------------------------------------------------------------------------------
		// スクロールしたら描き直す処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_Scroll(object sender, ScrollEventArgs e)
		{
			this.pnlPickerBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// ホイールでスクロールしたら、カーソル位置の枠を付け直して描き直す処理
		//-------------------------------------------------------------------------------
		private void pnlPickerBlocks_MouseWheel(object sender, MouseEventArgs e)
		{
			this.hoveredBlock = this.HitTest(e.Location);
			this.UpdateSelectedLabel();
			this.pnlPickerBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 上層も表示するかを切り替えたら描き直す処理
		//-------------------------------------------------------------------------------
		private void chkPickerShowTop_CheckedChanged(object sender, EventArgs e)
		{
			this.pnlPickerBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 「決定」で選んだブロックを確定する処理
		//-------------------------------------------------------------------------------
		private void btnPickerOK_Click(object sender, EventArgs e)
		{
			if (this.selectedBlock < 0)
			{
				return;
			}
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
