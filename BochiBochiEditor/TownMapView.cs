using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// タウンマップを描き、今の場所を赤い枠で囲む部品
	// 絵は縦横比を保って拡大し（ドット絵なので補間しない）、マウスを乗せたマスを白い枠で示す
	//-------------------------------------------------------------------------------
	internal sealed class TownMapView : Control
	{
		private TownMapData data;
		private Image image;
		private List<Point> markedCells = new List<Point>();
		private Point hoverCell = new Point(-1, -1);

		// マウスを乗せたマスが変わったとき（マスの外に出たら (-1, -1)）
		public event EventHandler<Point> HoverCellChanged;

		// マスをクリックしたとき
		public event EventHandler<Point> CellClicked;

		//-------------------------------------------------------------------------------
		// 描くタウンマップの形（null なら何も描かない）
		//-------------------------------------------------------------------------------
		public TownMapData Data
		{
			get { return this.data; }
			set
			{
				if (!ReferenceEquals(this.data, value))
				{
					this.data = value;
					this.Invalidate();
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 部品を初期化する処理（自分ですべて描くので、ちらつかないように二重描画にする）
		//-------------------------------------------------------------------------------
		public TownMapView()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		}

		//-------------------------------------------------------------------------------
		// 描く地方の絵（null なら何も描かない）
		//-------------------------------------------------------------------------------
		public Image MapImage
		{
			get { return this.image; }
			set
			{
				if (!ReferenceEquals(this.image, value))
				{
					this.image = value;
					this.Invalidate();
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 赤い枠で囲むマス（今のマップの場所）を設定する処理
		//-------------------------------------------------------------------------------
		public void SetMarkedCells(List<Point> cells)
		{
			this.markedCells = cells ?? new List<Point>();
			this.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 絵を描く範囲（地図の部分 ViewArea を、縦横比を保って中央に置く）を求める処理
		//-------------------------------------------------------------------------------
		private RectangleF GetImageRectangle()
		{
			Rectangle view = this.Data.ViewArea;
			float scale = Math.Min((float)this.ClientSize.Width / view.Width, (float)this.ClientSize.Height / view.Height);
			float width = view.Width * scale;
			float height = view.Height * scale;
			return new RectangleF((this.ClientSize.Width - width) / 2f, (this.ClientSize.Height - height) / 2f, width, height);
		}

		//-------------------------------------------------------------------------------
		// マス (x, y) が画面のどこに当たるかを求める処理
		//-------------------------------------------------------------------------------
		private RectangleF GetCellRectangle(RectangleF area, int x, int y)
		{
			Rectangle view = this.Data.ViewArea;
			float scale = area.Width / view.Width;
			return new RectangleF(area.X + (this.Data.GridOriginX + x * this.Data.CellSize - view.X) * scale, area.Y + (this.Data.GridOriginY + y * this.Data.CellSize - view.Y) * scale,
				this.Data.CellSize * scale, this.Data.CellSize * scale);
		}

		//-------------------------------------------------------------------------------
		// 画面上の位置から、マスの位置を求める処理（マスの外なら (-1, -1)）
		//-------------------------------------------------------------------------------
		private Point HitTest(Point location)
		{
			if (this.Data == null || this.image == null)
			{
				return new Point(-1, -1);
			}
			RectangleF area = this.GetImageRectangle();
			Rectangle view = this.Data.ViewArea;
			float scale = area.Width / view.Width;
			if (scale <= 0f)
			{
				return new Point(-1, -1);
			}
			int x = (int)Math.Floor(((location.X - area.X) / scale + view.X - this.Data.GridOriginX) / this.Data.CellSize);
			int y = (int)Math.Floor(((location.Y - area.Y) / scale + view.Y - this.Data.GridOriginY) / this.Data.CellSize);
			if (x < 0 || y < 0 || x >= this.Data.GridWidth || y >= this.Data.GridHeight)
			{
				return new Point(-1, -1);
			}
			return new Point(x, y);
		}

		//-------------------------------------------------------------------------------
		// タウンマップと枠を描く処理
		//-------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(this.Parent != null ? this.Parent.BackColor : UiTheme.Surface);
			if (this.Data == null || this.image == null)
			{
				return;
			}
			RectangleF area = this.GetImageRectangle();
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(this.image, area, this.Data.ViewArea, GraphicsUnit.Pixel);
			g.PixelOffsetMode = PixelOffsetMode.Default;
			g.SmoothingMode = SmoothingMode.None;

			// 今の場所: 薄い赤で塗り、まとまりの外周だけを太い赤線で囲む（道路のように複数マスでも 1 つの枠に見えるように）
			// 町の赤い点と重なっても目立つよう、線はマスの外側に描き、さらに外側を暗い色で縁取る
			if (this.markedCells.Count > 0)
			{
				HashSet<Point> set = new HashSet<Point>(this.markedCells);
				float width = Math.Max(3f, this.LogicalToDeviceUnits(3));
				using (SolidBrush fill = new SolidBrush(Color.FromArgb(60, 255, 0, 0)))
				using (Pen halo = new Pen(Color.FromArgb(200, 20, 0, 0), width + 2f))
				using (Pen pen = new Pen(Color.FromArgb(255, 255, 40, 40), width))
				{
					foreach (Point cell in this.markedCells)
					{
						g.FillRectangle(fill, this.GetCellRectangle(area, cell.X, cell.Y));
					}
					foreach (Pen outline in new[] { halo, pen })
					{
						float o = width / 2f;
						foreach (Point cell in this.markedCells)
						{
							RectangleF rect = this.GetCellRectangle(area, cell.X, cell.Y);
							RectangleF edge = RectangleF.FromLTRB(rect.Left - o, rect.Top - o, rect.Right + o, rect.Bottom + o);
							if (!set.Contains(new Point(cell.X, cell.Y - 1)))
							{
								g.DrawLine(outline, edge.Left, edge.Top, edge.Right, edge.Top);
							}
							if (!set.Contains(new Point(cell.X, cell.Y + 1)))
							{
								g.DrawLine(outline, edge.Left, edge.Bottom, edge.Right, edge.Bottom);
							}
							if (!set.Contains(new Point(cell.X - 1, cell.Y)))
							{
								g.DrawLine(outline, edge.Left, edge.Top, edge.Left, edge.Bottom);
							}
							if (!set.Contains(new Point(cell.X + 1, cell.Y)))
							{
								g.DrawLine(outline, edge.Right, edge.Top, edge.Right, edge.Bottom);
							}
						}
					}
				}
			}

			// マウスを乗せたマス
			if (this.hoverCell.X >= 0)
			{
				RectangleF rect = this.GetCellRectangle(area, this.hoverCell.X, this.hoverCell.Y);
				using (Pen pen = new Pen(Color.White, 1f))
				{
					g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マウスを乗せたマスを記録し、変わったら知らせる処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			this.SetHoverCell(this.HitTest(e.Location));
		}

		//-------------------------------------------------------------------------------
		// マウスが離れたら、乗せていたマスの表示を消す処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this.SetHoverCell(new Point(-1, -1));
		}

		//-------------------------------------------------------------------------------
		// クリックしたマスを知らせる処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			Point cell = this.HitTest(e.Location);
			if (e.Button == MouseButtons.Left && cell.X >= 0)
			{
				this.CellClicked?.Invoke(this, cell);
			}
		}

		//-------------------------------------------------------------------------------
		// 乗せているマスを変える処理（変わったときだけ描き直して知らせる）
		//-------------------------------------------------------------------------------
		private void SetHoverCell(Point cell)
		{
			if (cell == this.hoverCell)
			{
				return;
			}
			this.hoverCell = cell;
			this.Invalidate();
			this.HoverCellChanged?.Invoke(this, cell);
		}
	}
}
