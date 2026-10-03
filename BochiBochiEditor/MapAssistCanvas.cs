using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップ作成補助」の画面に渡す、今のマップの材料（ブロック一覧の絵・マップの並び・タイルセットの見分け方）
	// 画面はこの写しだけを見て動く（ROM や編集中のマップには触らない）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistContext : IDisposable
	{
		// ブロック一覧の絵で、横に並ぶブロックの数（マップエディタのブロック一覧と同じ）
		public const int SheetColumns = 8;

		public string MapLabel = string.Empty;
		public MapAssistTilesetInfo Primary;
		public MapAssistTilesetInfo Secondary;
		// ブロック一覧の絵（1 ブロック 16×16、横 8 個）。この入れ物が持ち主
		public Bitmap BlockSheet;
		// ブロックの総数、タイルセット1 の枠の数（タイルセット2 の先頭の番号）、タイルセット1 の実際のブロック数
		public int TotalBlocks;
		public int PrimarySlots;
		public int PrimaryUsed;
		// マップの並び（行ごとに左から）。マップが無ければ幅・高さは 0
		public int MapWidth;
		public int MapHeight;
		public int[] MapBlocks = new int[0];
		public int[] MapCollisions = new int[0];
		// 「マップから候補を作る」の材料（同じタイルセットを使うマップの並びと、ブロックの挙動）を集める処理。マップエディタが入れる
		public Func<MapAssistAutoInput> AutoInputProvider;
		// 「サポート作成」で作った並び（ブロック・移動エリア）を、編集中のマップへ入れる処理。入れられなければ理由を返す。マップエディタが入れる
		public Func<int[], int[], string> ApplyHandler;

		//-------------------------------------------------------------------------------
		// マップの通し番号のブロックに、データがあるかを返す処理（タイルセット1 の空の枠・範囲外は false）
		//-------------------------------------------------------------------------------
		public bool IsValidBlock(int id)
		{
			return (id >= 0 && id < this.PrimaryUsed) || (id >= this.PrimarySlots && id < this.TotalBlocks);
		}

		//-------------------------------------------------------------------------------
		// マップの通し番号を、「タイルセットと、その中の番号」のマスにする処理（データの無い番号は空き）
		//-------------------------------------------------------------------------------
		public MapAssistCell ToCell(int id, int collision)
		{
			if (!this.IsValidBlock(id))
			{
				return MapAssistCell.Empty;
			}
			return id < this.PrimarySlots
				? new MapAssistCell { Tileset = 1, Block = id, Collision = collision }
				: new MapAssistCell { Tileset = 2, Block = id - this.PrimarySlots, Collision = collision };
		}

		//-------------------------------------------------------------------------------
		// マスを、マップの通し番号にする処理（空き・範囲外は -1）
		//-------------------------------------------------------------------------------
		public int ToGlobal(MapAssistCell cell)
		{
			if (cell.IsEmpty)
			{
				return -1;
			}
			int id = cell.Tileset == 1 ? cell.Block : cell.Block + this.PrimarySlots;
			return this.IsValidBlock(id) ? id : -1;
		}

		//-------------------------------------------------------------------------------
		// 今のマップで、そのブロックにいちばん多く付いている移動エリアの値を返す処理（マップに無ければ -1）
		//-------------------------------------------------------------------------------
		public int MostCommonCollision(int id)
		{
			int[] counts = new int[64];
			bool found = false;
			for (int i = 0; i < this.MapBlocks.Length; i++)
			{
				if (this.MapBlocks[i] == id && this.MapCollisions[i] >= 0 && this.MapCollisions[i] < 64)
				{
					counts[this.MapCollisions[i]]++;
					found = true;
				}
			}
			if (!found)
			{
				return -1;
			}
			int best = 0;
			for (int i = 1; i < counts.Length; i++)
			{
				if (counts[i] > counts[best])
				{
					best = i;
				}
			}
			return best;
		}

		//-------------------------------------------------------------------------------
		// 持っている絵を破棄する処理
		//-------------------------------------------------------------------------------
		public void Dispose()
		{
			if (this.BlockSheet != null)
			{
				this.BlockSheet.Dispose();
				this.BlockSheet = null;
			}
		}
	}

	//-------------------------------------------------------------------------------
	// ブロックの絵（ブロック一覧・マップ）を拡大して出し、ドラッグで範囲を選べる部品
	// 選んだ範囲はマス単位（1 マス = 16×16 の 1 ブロック）で持つ
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistCanvas : Control
	{
		// 1 マスの大きさ（絵の上でのピクセル）
		public const int CellPixels = 16;

		private Bitmap image;
		private int zoom = 2;
		private Rectangle selection = Rectangle.Empty;
		private Point anchor;
		private bool dragging;

		// 範囲が変わったとき（ドラッグの途中も含む）
		public event EventHandler SelectionChanged;

		// マスに付ける印の色を返す処理（印を付けないマスは null）
		public Func<int, int, Color?> MarkerProvider;

		//-------------------------------------------------------------------------------
		// ちらつきを抑える描画の設定
		//-------------------------------------------------------------------------------
		public MapAssistCanvas()
		{
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
		}

		// 選んでいる範囲（マス単位。選んでいなければ空）
		public Rectangle Selection
		{
			get { return this.selection; }
		}

		// 横・縦のマスの数
		public int Columns
		{
			get { return this.image != null ? this.image.Width / CellPixels : 0; }
		}

		public int Rows
		{
			get { return this.image != null ? this.image.Height / CellPixels : 0; }
		}

		//-------------------------------------------------------------------------------
		// 出す絵と倍率を設定する処理（絵の持ち主は呼び出し側。大きさは絵 × 倍率に合わせる）
		//-------------------------------------------------------------------------------
		public void SetImage(Bitmap newImage, int newZoom)
		{
			this.image = newImage;
			this.zoom = Math.Max(1, newZoom);
			this.selection = Rectangle.Empty;
			this.Size = newImage != null ? new Size(newImage.Width * this.zoom, newImage.Height * this.zoom) : new Size(1, 1);
			this.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 範囲を設定する処理（絵の外にはみ出す分は切り捨てる）
		//-------------------------------------------------------------------------------
		public void SetSelection(Rectangle cells)
		{
			Rectangle bounded = Rectangle.Intersect(cells, new Rectangle(0, 0, this.Columns, this.Rows));
			this.selection = bounded.Width > 0 && bounded.Height > 0 ? bounded : Rectangle.Empty;
			this.Invalidate();
			this.SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------------
		// 絵（最近傍で拡大）・印・選択の枠を描く処理
		//-------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(UiTheme.Canvas);
			if (this.image == null)
			{
				return;
			}
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			int cell = CellPixels * this.zoom;
			// 描き直す範囲にかかるマスだけを描く
			int firstX = Math.Max(0, e.ClipRectangle.Left / cell);
			int firstY = Math.Max(0, e.ClipRectangle.Top / cell);
			int lastX = Math.Min(this.Columns - 1, (e.ClipRectangle.Right - 1) / cell);
			int lastY = Math.Min(this.Rows - 1, (e.ClipRectangle.Bottom - 1) / cell);
			if (lastX < firstX || lastY < firstY)
			{
				return;
			}
			Rectangle source = new Rectangle(firstX * CellPixels, firstY * CellPixels, (lastX - firstX + 1) * CellPixels, (lastY - firstY + 1) * CellPixels);
			Rectangle target = new Rectangle(firstX * cell, firstY * cell, (lastX - firstX + 1) * cell, (lastY - firstY + 1) * cell);
			g.DrawImage(this.image, target, source, GraphicsUnit.Pixel);
			g.PixelOffsetMode = PixelOffsetMode.Default;
			if (this.MarkerProvider != null)
			{
				int mark = Math.Max(5, cell / 4);
				for (int y = firstY; y <= lastY; y++)
				{
					for (int x = firstX; x <= lastX; x++)
					{
						Color? color = this.MarkerProvider(x, y);
						if (color == null)
						{
							continue;
						}
						Rectangle box = new Rectangle(x * cell + 1, y * cell + 1, mark, mark);
						using (SolidBrush brush = new SolidBrush(color.Value))
						{
							g.FillRectangle(brush, box);
						}
						g.DrawRectangle(Pens.Black, box);
					}
				}
			}
			if (!this.selection.IsEmpty)
			{
				Rectangle box = new Rectangle(this.selection.X * cell, this.selection.Y * cell, this.selection.Width * cell - 1, this.selection.Height * cell - 1);
				using (SolidBrush fill = new SolidBrush(Color.FromArgb(70, UiTheme.Accent)))
				using (Pen pen = new Pen(Color.White, 2f))
				using (Pen shadow = new Pen(Color.Black, 1f))
				{
					g.FillRectangle(fill, box);
					g.DrawRectangle(pen, box);
					g.DrawRectangle(shadow, Rectangle.Inflate(box, 2, 2));
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マウスの位置をマスの位置にする処理（絵の外は端のマスに寄せる）
		//-------------------------------------------------------------------------------
		private Point ToCell(Point location)
		{
			int cell = CellPixels * this.zoom;
			return new Point(Math.Max(0, Math.Min(this.Columns - 1, location.X / cell)), Math.Max(0, Math.Min(this.Rows - 1, location.Y / cell)));
		}

		//-------------------------------------------------------------------------------
		// 左ボタンを押した所から範囲を選び始める処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			this.Focus();
			if (e.Button != MouseButtons.Left || this.image == null || this.Columns == 0 || this.Rows == 0)
			{
				return;
			}
			this.anchor = this.ToCell(e.Location);
			this.dragging = true;
			this.SetSelection(new Rectangle(this.anchor, new Size(1, 1)));
		}

		//-------------------------------------------------------------------------------
		// ドラッグ中は、押した所から今の所までを範囲にする処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!this.dragging)
			{
				return;
			}
			Point current = this.ToCell(e.Location);
			Rectangle next = Rectangle.FromLTRB(Math.Min(this.anchor.X, current.X), Math.Min(this.anchor.Y, current.Y), Math.Max(this.anchor.X, current.X) + 1, Math.Max(this.anchor.Y, current.Y) + 1);
			if (next != this.selection)
			{
				this.SetSelection(next);
			}
		}

		//-------------------------------------------------------------------------------
		// ボタンを離したら範囲を決める処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			this.dragging = false;
		}
	}
}
