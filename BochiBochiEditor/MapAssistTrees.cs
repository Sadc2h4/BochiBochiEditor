using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 重なって並ぶ木（くり返しのパーツのうち、「BodyEdge」の枠を持つもの）を置く処理
	// 木 1 本は幅 2・高さ 3（先端・上・下）。上下に並ぶ木は 2 行おきで、下の木の先端が上の木の下の段に重なる。
	//   先端だけ     … Top（先端。地面の上に出る）
	//   上の段       … Body の 1 行目／BodyEdge の 1 行目（森の端のとき）／BodyTip（真上に木が無いとき。ファイアレッドで使う）
	//   下の段＋先端 … Body の 2 行目（つなぎ目）／BodyEdge の 2 行目（森の端のとき）
	//   下の段だけ   … Bottom／BottomEdge（森の端のとき）
	// マップにもうある木（上の段の左のブロックで見分ける）と位置をそろえ、まわりの木の端の絵も付け直す
	//-------------------------------------------------------------------------------
	internal static class MapAssistTrees
	{
		public const string SlotBodyEdge = "BodyEdge";
		public const string SlotBottomEdge = "BottomEdge";
		// 真上に木が無い（先端が地面に出ている）木の、上の段の絵（2×1。無ければふつうの上の段を使う）
		public const string SlotBodyTip = "BodyTip";

		// 木の段（マスが木のどの部分か）
		[Flags]
		private enum Layer
		{
			None = 0,
			Tip = 1,
			Upper = 2,
			Lower = 4,
		}

		// パーツから取り出した、木のブロックの番号（左・右）
		private sealed class TreeBlocks
		{
			public MapAssistCell[] Tip = new MapAssistCell[2];
			public MapAssistCell[] Upper = new MapAssistCell[2];
			public MapAssistCell[] UpperEdge = new MapAssistCell[2];
			public MapAssistCell[] UpperTip = new MapAssistCell[2];
			public MapAssistCell[] Join = new MapAssistCell[2];
			public MapAssistCell[] JoinEdge = new MapAssistCell[2];
			public MapAssistCell[] Bottom = new MapAssistCell[2];
			public MapAssistCell[] BottomEdge = new MapAssistCell[2];
			// 上の段の左のブロック（マップにある木を見分ける）と、この木のブロック全部
			public HashSet<int> UpperLeft = new HashSet<int>();
			public HashSet<int> All = new HashSet<int>();
		}

		//-------------------------------------------------------------------------------
		// パーツが「重なって並ぶ木」かどうかを返す処理（本体 2×2・端 2×2・先端 2×1・下端 2×1 がそろっているもの）
		//-------------------------------------------------------------------------------
		public static bool IsStackedTree(MapAssistPart part)
		{
			if (part == null || part.Kind != MapAssistKind.Repeat)
			{
				return false;
			}
			MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
			MapAssistGrid edge = part.GetSlot(SlotBodyEdge);
			MapAssistGrid top = part.GetSlot(MapAssistPart.SlotTop);
			MapAssistGrid bottom = part.GetSlot(MapAssistPart.SlotBottom);
			return body != null && edge != null && top != null && bottom != null
				&& body.Width == 2 && body.Height == 2 && edge.Width == 2 && edge.Height == 2
				&& top.Width == 2 && top.Height == 1 && bottom.Width == 2 && bottom.Height == 1;
		}

		//-------------------------------------------------------------------------------
		// パーツから、木のブロックの表を作る処理
		//-------------------------------------------------------------------------------
		private static TreeBlocks Read(MapAssistContext context, MapAssistPart part)
		{
			MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
			MapAssistGrid edge = part.GetSlot(SlotBodyEdge);
			MapAssistGrid top = part.GetSlot(MapAssistPart.SlotTop);
			MapAssistGrid bottom = part.GetSlot(MapAssistPart.SlotBottom);
			MapAssistGrid bottomEdge = part.GetSlot(SlotBottomEdge);
			if (bottomEdge == null || bottomEdge.Width != 2 || bottomEdge.Height != 1)
			{
				bottomEdge = bottom;
			}
			MapAssistGrid bodyTip = part.GetSlot(SlotBodyTip);
			if (bodyTip != null && (bodyTip.Width != 2 || bodyTip.Height != 1))
			{
				bodyTip = null;
			}
			TreeBlocks blocks = new TreeBlocks();
			for (int side = 0; side < 2; side++)
			{
				blocks.UpperTip[side] = bodyTip != null ? bodyTip[side, 0] : body[side, 0];
				blocks.Tip[side] = top[side, 0];
				blocks.Upper[side] = body[side, 0];
				blocks.Join[side] = body[side, 1];
				blocks.UpperEdge[side] = edge[side, 0];
				blocks.JoinEdge[side] = edge[side, 1];
				blocks.Bottom[side] = bottom[side, 0];
				blocks.BottomEdge[side] = bottomEdge[side, 0];
			}
			foreach (MapAssistCell cell in new[] { blocks.Upper[0], blocks.UpperEdge[0], blocks.UpperTip[0] })
			{
				int id = context.ToGlobal(cell);
				if (id >= 0)
				{
					blocks.UpperLeft.Add(id);
				}
			}
			foreach (MapAssistCell[] pair in new[] { blocks.Tip, blocks.Upper, blocks.UpperEdge, blocks.UpperTip, blocks.Join, blocks.JoinEdge, blocks.Bottom, blocks.BottomEdge })
			{
				foreach (MapAssistCell cell in pair)
				{
					int id = context.ToGlobal(cell);
					if (id >= 0)
					{
						blocks.All.Add(id);
					}
				}
			}
			return blocks;
		}

		//-------------------------------------------------------------------------------
		// マップにもうある木（上の段の左のマス）を、範囲の中から集める処理
		//-------------------------------------------------------------------------------
		private static HashSet<Point> ExistingTrees(TreeBlocks blocks, int width, int height, Func<int, int, int> blockAt, Rectangle area)
		{
			HashSet<Point> trees = new HashSet<Point>();
			area = Rectangle.Intersect(area, new Rectangle(0, 0, width, height));
			for (int y = area.Top; y < area.Bottom; y++)
			{
				for (int x = area.Left; x < area.Right; x++)
				{
					if (x + 1 < width && blocks.UpperLeft.Contains(blockAt(x, y)))
					{
						trees.Add(new Point(x, y));
					}
				}
			}
			return trees;
		}

		//-------------------------------------------------------------------------------
		// 範囲をなぞったときに置く木（上の段の左のマス）を返す処理
		// 近くにもう木があれば、その木と位置（横 2 マス・縦 2 マスの区切り）をそろえる。無ければ範囲の左上を区切りにする
		//-------------------------------------------------------------------------------
		public static List<Point> NewTrees(MapAssistContext context, MapAssistPart part, int width, int height, Func<int, int, int> blockAt, Rectangle range)
		{
			TreeBlocks blocks = Read(context, part);
			return NewTrees(blocks, width, height, blockAt, range, ExistingTrees(blocks, width, height, blockAt, Rectangle.Inflate(range, 5, 5)));
		}

		//-------------------------------------------------------------------------------
		// 範囲をなぞったときに置く木を、もうある木（existing）と位置をそろえて決める処理
		//-------------------------------------------------------------------------------
		private static List<Point> NewTrees(TreeBlocks blocks, int width, int height, Func<int, int, int> blockAt, Rectangle range, HashSet<Point> existing)
		{
			int phaseX = range.Left & 1;
			int phaseY = range.Top & 1;
			// いちばん近い木に位置をそろえる
			Point center = new Point(range.Left + range.Width / 2, range.Top + range.Height / 2);
			int best = int.MaxValue;
			foreach (Point tree in existing)
			{
				int distance = Math.Abs(tree.X - center.X) + Math.Abs(tree.Y - center.Y);
				if (distance < best)
				{
					best = distance;
					phaseX = tree.X & 1;
					phaseY = tree.Y & 1;
				}
			}
			List<Point> result = new List<Point>();
			HashSet<Point> seen = new HashSet<Point>();
			for (int y = range.Top; y < range.Bottom; y++)
			{
				for (int x = range.Left; x < range.Right; x++)
				{
					Point anchor = new Point(x - ((x - phaseX) & 1), y - ((y - phaseY) & 1));
					if (anchor.X < 0 || anchor.X + 1 >= width || anchor.Y < 0 || anchor.Y >= height || !seen.Add(anchor))
					{
						continue;
					}
					result.Add(anchor);
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// カーソルの所に出す、置くはずのブロック（マスと、ブロックの番号）を返す処理。まわりの木との重なりは入れない、木だけの絵
		//-------------------------------------------------------------------------------
		public static List<KeyValuePair<Point, int>> Preview(MapAssistContext context, MapAssistPart part, int width, int height, Func<int, int, int> blockAt, Rectangle range)
		{
			TreeBlocks blocks = Read(context, part);
			List<KeyValuePair<Point, int>> cells = new List<KeyValuePair<Point, int>>();
			HashSet<Point> trees = new HashSet<Point>(NewTrees(blocks, width, height, blockAt, range, ExistingTrees(blocks, width, height, blockAt, Rectangle.Inflate(range, 5, 5))));
			foreach (Point tree in trees)
			{
				for (int side = 0; side < 2; side++)
				{
					bool below = trees.Contains(new Point(tree.X, tree.Y + 2));
					bool above = trees.Contains(new Point(tree.X, tree.Y - 2));
					if (!above)
					{
						Add(context, cells, width, height, tree.X + side, tree.Y - 1, blocks.Tip[side]);
					}
					Add(context, cells, width, height, tree.X + side, tree.Y, blocks.UpperEdge[side]);
					Add(context, cells, width, height, tree.X + side, tree.Y + 1, below ? blocks.JoinEdge[side] : blocks.BottomEdge[side]);
				}
			}
			return cells;
		}

		//-------------------------------------------------------------------------------
		// マップの中のマスで、ブロックが使えるときだけ、一覧に足す処理
		//-------------------------------------------------------------------------------
		private static void Add(MapAssistContext context, List<KeyValuePair<Point, int>> cells, int width, int height, int x, int y, MapAssistCell cell)
		{
			int id = context.ToGlobal(cell);
			if (id >= 0 && x >= 0 && y >= 0 && x < width && y < height)
			{
				cells.Add(new KeyValuePair<Point, int>(new Point(x, y), id));
			}
		}

		//-------------------------------------------------------------------------------
		// 範囲をなぞって木を置く処理（置いた木の数を返す）
		// 置く木と、まわりにもうある木をあわせて、マスごとに「先端・上の段・下の段」のどれが重なるかを出し、ブロックを決める
		//-------------------------------------------------------------------------------
		public static int Paint(MapAssistContext context, MapAssistWork work, MapAssistPart part, Rectangle range, int[] table, MapAssistSupportReport report)
		{
			TreeBlocks blocks = Read(context, part);
			Func<int, int, int> blockAt = (x, y) => work.Blocks[y * work.Width + x];
			range = Rectangle.Intersect(range, new Rectangle(0, 0, work.Width, work.Height));
			if (range.Width <= 0 || range.Height <= 0)
			{
				return 0;
			}
			HashSet<Point> existing = ExistingTrees(blocks, work.Width, work.Height, blockAt, Rectangle.Inflate(range, 6, 6));
			List<Point> added = NewTrees(blocks, work.Width, work.Height, blockAt, range, existing);
			if (added.Count == 0)
			{
				return 0;
			}
			HashSet<Point> trees = new HashSet<Point>(existing);
			foreach (Point tree in added)
			{
				// 1 マスずれて重なる木（位置のそろっていない古い木）は取り除く
				trees.RemoveWhere(t => t != tree && Math.Abs(t.X - tree.X) < 2 && Math.Abs(t.Y - tree.Y) < 2);
				trees.Add(tree);
			}
			// マスごとの段と、左右のどちらか
			Dictionary<Point, Layer> layers = new Dictionary<Point, Layer>();
			Dictionary<Point, int> sides = new Dictionary<Point, int>();
			Action<int, int, Layer, int> mark = (x, y, layer, side) =>
			{
				Point at = new Point(x, y);
				Layer old;
				layers.TryGetValue(at, out old);
				layers[at] = old | layer;
				sides[at] = side;
			};
			foreach (Point tree in trees)
			{
				for (int side = 0; side < 2; side++)
				{
					mark(tree.X + side, tree.Y - 1, Layer.Tip, side);
					mark(tree.X + side, tree.Y, Layer.Upper, side);
					mark(tree.X + side, tree.Y + 1, Layer.Lower, side);
				}
			}
			// 森の続きかどうか（木の上の段・下の段があるマス。マップの外は続いているとみなす）
			Func<int, int, bool> solid = (x, y) =>
			{
				if (x < 0 || y < 0 || x >= work.Width || y >= work.Height)
				{
					return true;
				}
				Layer layer;
				if (layers.TryGetValue(new Point(x, y), out layer) && (layer & (Layer.Upper | Layer.Lower)) != 0)
				{
					return true;
				}
				// 調べた範囲の外にある木のブロック
				int id = blockAt(x, y);
				return blocks.All.Contains(id) && context.ToGlobal(blocks.Tip[0]) != id && context.ToGlobal(blocks.Tip[1]) != id;
			};
			// 書き換えるのは、置いた木のマスと、そのまわりで木のブロックがあるマス
			HashSet<Point> own = new HashSet<Point>();
			int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
			foreach (Point tree in added)
			{
				for (int side = 0; side < 2; side++)
				{
					for (int row = -1; row <= 1; row++)
					{
						own.Add(new Point(tree.X + side, tree.Y + row));
					}
				}
				left = Math.Min(left, tree.X - 1);
				right = Math.Max(right, tree.X + 2);
				top = Math.Min(top, tree.Y - 1);
				bottom = Math.Max(bottom, tree.Y + 2);
			}
			for (int y = Math.Max(0, top); y <= Math.Min(work.Height - 1, bottom); y++)
			{
				for (int x = Math.Max(0, left); x <= Math.Min(work.Width - 1, right); x++)
				{
					Point at = new Point(x, y);
					Layer layer;
					if (!layers.TryGetValue(at, out layer))
					{
						continue;
					}
					int index = y * work.Width + x;
					if (!own.Contains(at) && !blocks.All.Contains(work.Blocks[index]))
					{
						continue;
					}
					int side = sides[at];
					bool open = !solid(side == 0 ? x - 1 : x + 1, y);
					MapAssistCell cell;
					if ((layer & Layer.Upper) != 0)
					{
						// 森の端は端の絵。真上の木（2 行上）が無いとき（先端が地面に出ている木）は、そのための絵（無ければふつうの絵）
						Point tree = new Point(x - side, y);
						bool above = tree.Y - 2 < 0 || trees.Contains(new Point(tree.X, tree.Y - 2));
						cell = open ? blocks.UpperEdge[side] : (above ? blocks.Upper[side] : blocks.UpperTip[side]);
					}
					else if ((layer & Layer.Lower) != 0)
					{
						cell = (layer & Layer.Tip) != 0
							? (open ? blocks.JoinEdge[side] : blocks.Join[side])
							: (open ? blocks.BottomEdge[side] : blocks.Bottom[side]);
					}
					else
					{
						cell = blocks.Tip[side];
					}
					int id = context.ToGlobal(cell);
					if (id < 0)
					{
						continue;
					}
					if (work.Blocks[index] != id)
					{
						work.Blocks[index] = id;
						report.Painted++;
					}
					int collision = cell.Collision >= 0 ? cell.Collision : table[id];
					if (collision >= 0)
					{
						work.Collisions[index] = collision;
					}
				}
			}
			return added.Count;
		}
	}
}
