using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップ作成補助」の 2 段階目「サポート作成」で書き換える、マップの並びの写し
	// 画面はこの写しを書き換えてから、変わったマスだけをマップエディタへ渡す（1 回の「戻る」で元に戻せる）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistWork
	{
		public int Width;
		public int Height;
		// 行ごとに左から。ブロックはマップの通し番号、移動エリアは 0〜63
		public int[] Blocks;
		public int[] Collisions;

		//-------------------------------------------------------------------------------
		// 今のマップの並びを写す処理（マップが無ければ null）
		//-------------------------------------------------------------------------------
		public static MapAssistWork FromContext(MapAssistContext context)
		{
			if (context == null || context.MapWidth <= 0 || context.MapHeight <= 0)
			{
				return null;
			}
			return new MapAssistWork
			{
				Width = context.MapWidth,
				Height = context.MapHeight,
				Blocks = (int[])context.MapBlocks.Clone(),
				Collisions = (int[])context.MapCollisions.Clone(),
			};
		}

		//-------------------------------------------------------------------------------
		// マップの中の位置かを返す処理
		//-------------------------------------------------------------------------------
		public bool Contains(int x, int y)
		{
			return x >= 0 && y >= 0 && x < this.Width && y < this.Height;
		}

		//-------------------------------------------------------------------------------
		// 元の並びと比べて、ブロックか移動エリアが変わったマスの数を返す処理
		//-------------------------------------------------------------------------------
		public int CountChanges(MapAssistContext context)
		{
			int count = 0;
			for (int i = 0; i < this.Blocks.Length; i++)
			{
				if (this.Blocks[i] != context.MapBlocks[i] || this.Collisions[i] != context.MapCollisions[i])
				{
					count++;
				}
			}
			return count;
		}
	}

	//-------------------------------------------------------------------------------
	// 「サポート作成」で何をしたかの数（画面の案内に使う）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistSupportReport
	{
		// 縁を直したマス・直せなかったマス（細すぎる・枠が空）
		public int EdgeFixed;
		public int EdgeSkipped;
		// くり返しの並びを直したマス
		public int RepeatFixed;
		// 移動エリアを付け直したマス
		public int CollisionSet;
		// 塗ったマス
		public int Painted;
	}

	//-------------------------------------------------------------------------------
	// 「サポート作成」の中身（ある程度作ったマップに肉付けする）
	//
	//   塗る               … 選んだパーツで範囲を塗る（面 = 1 種類のブロック、縁つき = 中央で塗ってから縁を付ける、
	//                         部品 = 範囲の左上に置く、くり返し = 範囲の左上から並べる。上端・下端も付ける）
	//   縁を整える         … 縁つきのパーツのブロックがつながった所ごとに、上下左右・斜めを見て辺・角のブロックに置き換える
	//   くり返しを整える   … 森・地面の模様（くり返しのパーツ）の並びのずれを直す（上端・下端は変えない）
	//   移動エリアを付ける … ブロックごとに、パーツの指定（無ければ ROM の全マップでいちばん多い値）を付ける
	//
	// 範囲の外のマスは変えない。ただし、縁やくり返しの判定には範囲の外のマスも使う
	// （仮の決め方: 縁は 13 個の枠だけで直し、幅 1 マスの所は直さない。くり返しは上端・下端だけで、左右の端は付けない）
	//-------------------------------------------------------------------------------
	internal static class MapAssistSupport
	{
		// 一度に扱えるパーツの数（つながりの判定に 32 ビットの印を使うため）
		private const int MaxParts = 32;
		// くり返しのずらし方を決めるときに見る、まわりのマスの広さ（上下左右にこのマス数）
		private const int RepeatWindow = 2;
		// 今のままのずらし方より、これだけ点が高いときだけ並べ直す（まわりの 1 マスが合うと 2 点）
		private const int RepeatMargin = 5;

		// 上下左右（つながりを調べる向き）
		private static readonly int[] StepX = { 1, -1, 0, 0 };
		private static readonly int[] StepY = { 0, 0, 1, -1 };

		//-------------------------------------------------------------------------------
		// ブロックごとに付ける移動エリアの表を作る処理
		// fromMaps（ROM の全マップでいちばん多い値）を元に、面・縁つき・くり返しのパーツで値を決めてあるブロックは、その値にする
		//-------------------------------------------------------------------------------
		public static int[] BuildCollisionTable(MapAssistContext context, IEnumerable<MapAssistPart> parts, int[] fromMaps)
		{
			int[] table = new int[context.TotalBlocks];
			for (int id = 0; id < table.Length; id++)
			{
				table[id] = fromMaps != null && id < fromMaps.Length ? fromMaps[id] : -1;
			}
			foreach (MapAssistPart part in parts.Where(p => p.Kind != MapAssistKind.Stamp))
			{
				foreach (MapAssistCell cell in part.AllCells())
				{
					int id = context.ToGlobal(cell);
					if (id >= 0 && cell.Collision >= 0)
					{
						table[id] = cell.Collision;
					}
				}
			}
			return table;
		}

		//-------------------------------------------------------------------------------
		// 選んだパーツで範囲を塗る処理（塗れなければ理由、塗れたら null）
		// member は、面のときに使うブロックの番号（-1 なら先頭のブロック）
		//-------------------------------------------------------------------------------
		public static string Paint(MapAssistContext context, MapAssistWork work, IList<MapAssistPart> parts, MapAssistPart part, Rectangle range, int member, int[] table, MapAssistAutoInput samples, MapAssistSupportReport report)
		{
			range = Rectangle.Intersect(range, new Rectangle(0, 0, work.Width, work.Height));
			if (range.Width <= 0 || range.Height <= 0)
			{
				return Localizer.T("塗る範囲がマップの外です。");
			}
			switch (part.Kind)
			{
				case MapAssistKind.Area:
				{
					MapAssistGrid blocks = part.GetSlot(MapAssistPart.SlotBlocks);
					List<MapAssistCell> cells = blocks != null ? blocks.Cells.Where(c => context.ToGlobal(c) >= 0).ToList() : new List<MapAssistCell>();
					if (cells.Count == 0)
					{
						return Localizer.T("この面には、今のタイルセットで使えるブロックが入っていません。");
					}
					MapAssistCell pick = member >= 0 && member < blocks.Cells.Length && context.ToGlobal(blocks.Cells[member]) >= 0 ? blocks.Cells[member] : cells[0];
					FillRect(context, work, range, (x, y) => pick, table, report);
					return null;
				}
				case MapAssistKind.Edge:
				{
					MapAssistGrid center = part.GetSlot("C");
					if (center == null || context.ToGlobal(center.Cells[0]) < 0)
					{
						return Localizer.T("この縁つきには、中央のブロックが入っていません。");
					}
					FillRect(context, work, range, (x, y) => center.Cells[0], table, report);
					// 塗った所と、そのすぐ外側の縁を付け直す（塗ったパーツを優先する）
					// 内側の縁（2 周目）を持つパーツは、もう 1 マス外まで付け直す
					int reach = MapAssistPart.EdgeRing2Slots.Any(n => part.GetSlot(n) != null) ? 2 : 1;
					Rectangle around = Rectangle.Intersect(Rectangle.Inflate(range, reach, reach), new Rectangle(0, 0, work.Width, work.Height));
					FixEdges(context, work, parts, around, part, table, samples, report);
					return null;
				}
				case MapAssistKind.Stamp:
				{
					MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
					if (body == null)
					{
						return Localizer.T("この部品には、並びが入っていません。");
					}
					// 範囲の左上に置く（範囲の大きさは使わない。マップからはみ出す分は置かない）
					Rectangle placed = Rectangle.Intersect(new Rectangle(range.X, range.Y, body.Width, body.Height), new Rectangle(0, 0, work.Width, work.Height));
					FillRect(context, work, placed, (x, y) => body[x - range.X, y - range.Y], table, report);
					return null;
				}
				default:
				{
					MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
					if (body == null)
					{
						return Localizer.T("このくり返しには、くり返す単位が入っていません。");
					}
					if (MapAssistTrees.IsStackedTree(part))
					{
						// 重なって並ぶ木: 木を 1 本ずつ置き、重なりと森の端の絵を付ける
						MapAssistTrees.Paint(context, work, part, range, table, report);
						return null;
					}
					MapAssistGrid top = SameWidth(part.GetSlot(MapAssistPart.SlotTop), body);
					MapAssistGrid bottom = SameWidth(part.GetSlot(MapAssistPart.SlotBottom), body);
					int topRows = top != null ? top.Height : 0;
					int bottomRows = bottom != null ? bottom.Height : 0;
					// 上端・下端を付けると本体が 1 行も残らないほど低い範囲では、本体だけで塗る
					if (range.Height < topRows + bottomRows + 1)
					{
						topRows = 0;
						bottomRows = 0;
					}
					FillRect(context, work, range, (x, y) =>
					{
						int column = (x - range.X) % body.Width;
						int row = y - range.Y;
						if (row < topRows)
						{
							return top[column, row];
						}
						if (range.Bottom - 1 - y < bottomRows)
						{
							return bottom[column, bottomRows - (range.Bottom - y)];
						}
						return body[column, (row - topRows) % body.Height];
					}, table, report);
					return null;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 範囲を整える処理（くり返し → 縁 → 移動エリアの順）
		//-------------------------------------------------------------------------------
		public static void Tidy(MapAssistContext context, MapAssistWork work, IList<MapAssistPart> parts, Rectangle range, bool edges, bool repeats, bool collisions, int[] table, MapAssistAutoInput samples, MapAssistSupportReport report)
		{
			range = Rectangle.Intersect(range, new Rectangle(0, 0, work.Width, work.Height));
			if (range.Width <= 0 || range.Height <= 0)
			{
				return;
			}
			if (repeats)
			{
				FixRepeats(context, work, parts, range, report);
			}
			if (edges)
			{
				FixEdges(context, work, parts, range, null, table, samples, report);
			}
			if (collisions)
			{
				for (int y = range.Top; y < range.Bottom; y++)
				{
					for (int x = range.Left; x < range.Right; x++)
					{
						int i = y * work.Width + x;
						int id = work.Blocks[i];
						int value = id >= 0 && id < table.Length ? table[id] : -1;
						if (value >= 0 && work.Collisions[i] != value)
						{
							work.Collisions[i] = value;
							report.CollisionSet++;
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 範囲のマスを、位置ごとに決めたマスで塗る処理（空きのマスは塗らない）
		// 移動エリアは、マスに値があればその値、無ければ表の値（表にも無ければ今の値のまま）
		//-------------------------------------------------------------------------------
		internal static void FillRect(MapAssistContext context, MapAssistWork work, Rectangle range, Func<int, int, MapAssistCell> cellAt, int[] table, MapAssistSupportReport report)
		{
			for (int y = range.Top; y < range.Bottom; y++)
			{
				for (int x = range.Left; x < range.Right; x++)
				{
					MapAssistCell cell = cellAt(x, y);
					int id = context.ToGlobal(cell);
					if (id < 0)
					{
						continue;
					}
					int i = y * work.Width + x;
					work.Blocks[i] = id;
					int collision = cell.Collision >= 0 ? cell.Collision : table[id];
					if (collision >= 0)
					{
						work.Collisions[i] = collision;
					}
					report.Painted++;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// くり返しの上端・下端が本体と同じ幅なら返す処理（違えば使わない）
		//-------------------------------------------------------------------------------
		internal static MapAssistGrid SameWidth(MapAssistGrid grid, MapAssistGrid body)
		{
			return grid != null && grid.Width == body.Width ? grid : null;
		}

		//-------------------------------------------------------------------------------
		// マスごとに、そのブロックを含むパーツの印（ビット）を作る処理
		//-------------------------------------------------------------------------------
		private static uint[] Membership(MapAssistContext context, MapAssistWork work, List<MapAssistPart> list)
		{
			Dictionary<int, uint> masks = new Dictionary<int, uint>();
			for (int p = 0; p < list.Count; p++)
			{
				foreach (MapAssistCell cell in list[p].AllCells())
				{
					int id = context.ToGlobal(cell);
					if (id >= 0)
					{
						uint old;
						masks.TryGetValue(id, out old);
						masks[id] = old | (1U << p);
					}
				}
			}
			uint[] member = new uint[work.Blocks.Length];
			for (int i = 0; i < member.Length; i++)
			{
				uint mask;
				member[i] = masks.TryGetValue(work.Blocks[i], out mask) ? mask : 0U;
			}
			return member;
		}

		//-------------------------------------------------------------------------------
		// 同じパーツの印を持つマスが上下左右につながった所ごとに、番号を付ける処理（印の無いマスは -1）
		//-------------------------------------------------------------------------------
		private static int[] Components(MapAssistWork work, uint[] member, out List<List<int>> groups)
		{
			int[] group = new int[member.Length];
			for (int i = 0; i < group.Length; i++)
			{
				group[i] = -1;
			}
			groups = new List<List<int>>();
			Queue<int> queue = new Queue<int>();
			for (int start = 0; start < member.Length; start++)
			{
				if (member[start] == 0U || group[start] >= 0)
				{
					continue;
				}
				List<int> cells = new List<int>();
				group[start] = groups.Count;
				queue.Enqueue(start);
				while (queue.Count > 0)
				{
					int i = queue.Dequeue();
					cells.Add(i);
					int x = i % work.Width;
					int y = i / work.Width;
					for (int d = 0; d < 4; d++)
					{
						int nx = x + StepX[d];
						int ny = y + StepY[d];
						if (!work.Contains(nx, ny))
						{
							continue;
						}
						int n = ny * work.Width + nx;
						if (group[n] < 0 && (member[n] & member[i]) != 0U)
						{
							group[n] = groups.Count;
							queue.Enqueue(n);
						}
					}
				}
				groups.Add(cells);
			}
			return group;
		}

		//-------------------------------------------------------------------------------
		// つながった所が、どのパーツのものかを決める処理
		// そのパーツにしか無いブロック（縁つきなら、辺・角のブロックを先に見る）の数で多数決。決まらなければ preferred、それも無ければ先頭のパーツ
		//-------------------------------------------------------------------------------
		private static int Winner(List<int> cells, uint[] member, Func<int, uint> distinctive, int preferred)
		{
			int[] votes = new int[MaxParts];
			uint common = 0U;
			foreach (int i in cells)
			{
				common |= member[i];
				uint mask = distinctive(i);
				if (mask == 0U || (mask & (mask - 1U)) != 0U)
				{
					mask = member[i];
				}
				if (mask != 0U && (mask & (mask - 1U)) == 0U)
				{
					votes[BitIndex(mask)]++;
				}
			}
			int best = -1;
			for (int p = 0; p < MaxParts; p++)
			{
				if (votes[p] > 0 && (best < 0 || votes[p] > votes[best]))
				{
					best = p;
				}
			}
			if (best >= 0)
			{
				return best;
			}
			if (preferred >= 0 && (common & (1U << preferred)) != 0U)
			{
				return preferred;
			}
			return BitIndex(common & (~common + 1U));
		}

		//-------------------------------------------------------------------------------
		// 1 つだけ立っているビットの位置を返す処理
		//-------------------------------------------------------------------------------
		private static int BitIndex(uint single)
		{
			int index = 0;
			while (index < 31 && (single & (1U << index)) == 0U)
			{
				index++;
			}
			return index;
		}

		//-------------------------------------------------------------------------------
		// 縁つきのパーツの縁を整える処理
		// つながった所の中のマスごとに、上下左右・斜めが同じ所か（マップの外は同じ所とみなす）を見て、枠を決める。
		//   上と左が外 = 左上の角、上だけ外 = 上の辺 …、上下左右が中で左上だけ外 = 内側の角（左上）、すべて中 = 中央
		// 上下（または左右）の両方が外の細い所と、決まった枠が空のときは変えない
		//-------------------------------------------------------------------------------
		private static void FixEdges(MapAssistContext context, MapAssistWork work, IList<MapAssistPart> parts, Rectangle range, MapAssistPart preferred, int[] table, MapAssistAutoInput samples, MapAssistSupportReport report)
		{
			List<MapAssistPart> list = parts.Where(p => p.Kind == MapAssistKind.Edge && p.GetSlot("C") != null).Take(MaxParts).ToList();
			if (list.Count == 0)
			{
				return;
			}
			uint[] member = Membership(context, work, list);
			// 辺・角（中央以外）の枠に入っているブロックの印。どのパーツの所かを決める手がかりにする。
			// あわせて、ブロックごとに「どれかのパーツで入っている枠の名前」を集める（もう正しい枠のブロックなら変えないため）
			Dictionary<int, uint> edgeMasks = new Dictionary<int, uint>();
			Dictionary<int, HashSet<string>> slotsOf = new Dictionary<int, HashSet<string>>();
			for (int p = 0; p < list.Count; p++)
			{
				foreach (string slot in MapAssistPart.AllSlotNamesOf(MapAssistKind.Edge))
				{
					MapAssistGrid grid = list[p].GetSlot(slot);
					int id = grid != null ? context.ToGlobal(grid.Cells[0]) : -1;
					if (id < 0)
					{
						continue;
					}
					HashSet<string> names;
					if (!slotsOf.TryGetValue(id, out names))
					{
						names = new HashSet<string>();
						slotsOf[id] = names;
					}
					names.Add(slot);
					if (slot != "C")
					{
						uint old;
						edgeMasks.TryGetValue(id, out old);
						edgeMasks[id] = old | (1U << p);
					}
				}
			}
			List<List<int>> groups;
			int[] group = Components(work, member, out groups);
			HashSet<int> shore = ShoreBlocks(context, samples, new HashSet<int>(slotsOf.Keys));
			int preferredIndex = preferred != null ? list.IndexOf(preferred) : -1;
			Func<int, uint> distinctive = i =>
			{
				uint mask;
				return edgeMasks.TryGetValue(work.Blocks[i], out mask) ? mask : 0U;
			};
			int[] result = (int[])work.Blocks.Clone();
			int[] resultCollision = (int[])work.Collisions.Clone();
			for (int g = 0; g < groups.Count; g++)
			{
				// 範囲にかからない所は調べない
				if (!groups[g].Any(i => range.Contains(i % work.Width, i / work.Width)))
				{
					continue;
				}
				int winner = Winner(groups[g], member, distinctive, preferredIndex);
				MapAssistPart part = list[winner];
				int groupIndex = g;
				// マスの、外側の縁（1 周目）の枠を決める処理
				Func<int, string> outerSlot = cellIndex =>
				{
					int cx = cellIndex % work.Width;
					int cy = cellIndex / work.Width;
					Func<int, int, bool> inside = (dx, dy) =>
					{
						if (!work.Contains(cx + dx, cy + dy))
						{
							return true;
						}
						int n = (cy + dy) * work.Width + cx + dx;
						return group[n] == groupIndex || (member[n] == 0U && shore.Contains(work.Blocks[n]));
					};
					return EdgeSlot(inside(0, -1), inside(0, 1), inside(-1, 0), inside(1, 0), inside(-1, -1), inside(1, -1), inside(-1, 1), inside(1, 1));
				};
				// 内側の縁（2 周目）を持つパーツ: 外側の縁でないマス（1 周目が「中央」のマス）だけを中身と見て、もう一度縁を決める
				bool hasRing2 = MapAssistPart.EdgeRing2Slots.Any(n => part.GetSlot(n) != null);
				HashSet<int> core = null;
				if (hasRing2)
				{
					core = new HashSet<int>(groups[g].Where(c => outerSlot(c) == "C"));
				}
				foreach (int i in groups[g])
				{
					int x = i % work.Width;
					int y = i / work.Width;
					if (!range.Contains(x, y) || (member[i] & (1U << winner)) == 0U)
					{
						continue;
					}
					string slot = outerSlot(i);
					if (slot == "C" && hasRing2)
					{
						Func<int, int, bool> inCore = (dx, dy) => !work.Contains(x + dx, y + dy) || core.Contains((y + dy) * work.Width + x + dx);
						string inner = EdgeSlot(inCore(0, -1), inCore(0, 1), inCore(-1, 0), inCore(1, 0), inCore(-1, -1), inCore(1, -1), inCore(-1, 1), inCore(1, 1));
						if (inner != null && inner != "C" && part.GetSlot("2" + inner) != null)
						{
							slot = "2" + inner;
						}
					}
					// 今のブロックが、どれかのパーツでその枠に入っているものなら変えない（元からある池の岸の絵柄などを残す）
					HashSet<string> current;
					if (slot != null && slotsOf.TryGetValue(work.Blocks[i], out current) && current.Contains(slot))
					{
						continue;
					}
					MapAssistGrid grid = slot != null ? part.GetSlot(slot) : null;
					int id = grid != null ? context.ToGlobal(grid.Cells[0]) : -1;
					if (id < 0)
					{
						report.EdgeSkipped++;
						continue;
					}
					if (result[i] != id)
					{
						result[i] = id;
						report.EdgeFixed++;
					}
					int collision = grid.Cells[0].Collision >= 0 ? grid.Cells[0].Collision : table[id];
					if (collision >= 0)
					{
						resultCollision[i] = collision;
					}
				}
			}
			// すべてのマスの枠を元の並びで決めてから書き換える（書き換えた結果で隣の判定が変わらないように）
			Array.Copy(result, work.Blocks, result.Length);
			Array.Copy(resultCollision, work.Collisions, resultCollision.Length);
		}

		//-------------------------------------------------------------------------------
		// 縁つきのパーツに入っていないが、ROM のマップでたいてい（6 割以上）パーツのブロックの隣に置かれているブロックを集める処理
		// 池の角など、パーツの枠に無い岸のブロックを「外（陸）」と見て、周りの縁を崩さないようにするため
		//-------------------------------------------------------------------------------
		private static HashSet<int> ShoreBlocks(MapAssistContext context, MapAssistAutoInput samples, HashSet<int> members)
		{
			HashSet<int> shore = new HashSet<int>();
			if (samples == null)
			{
				return shore;
			}
			Dictionary<int, int> total = new Dictionary<int, int>();
			Dictionary<int, int> near = new Dictionary<int, int>();
			foreach (MapAssistSample sample in samples.Samples)
			{
				// 今のタイルセットで意味のあるブロックだけ（タイルセット2 が違うマップでは、タイルセット1 のブロックだけ）
				Func<int, bool> usable = id => context.IsValidBlock(id) && (id < context.PrimarySlots || sample.SamePair);
				for (int y = 0; y < sample.Height; y++)
				{
					for (int x = 0; x < sample.Width; x++)
					{
						int id = sample.Blocks[y * sample.Width + x];
						if (!usable(id) || members.Contains(id))
						{
							continue;
						}
						int count;
						total.TryGetValue(id, out count);
						total[id] = count + 1;
						for (int d = 0; d < 4; d++)
						{
							int nx = x + StepX[d];
							int ny = y + StepY[d];
							if (nx >= 0 && ny >= 0 && nx < sample.Width && ny < sample.Height && members.Contains(sample.Blocks[ny * sample.Width + nx]) && usable(sample.Blocks[ny * sample.Width + nx]))
							{
								near.TryGetValue(id, out count);
								near[id] = count + 1;
								break;
							}
						}
					}
				}
			}
			foreach (KeyValuePair<int, int> pair in near)
			{
				if (pair.Value * 10 >= total[pair.Key] * 6)
				{
					shore.Add(pair.Key);
				}
			}
			return shore;
		}

		//-------------------------------------------------------------------------------
		// 上下左右・斜めが同じ所かどうかから、縁つきの枠の名前を決める処理（細い所は null）
		//-------------------------------------------------------------------------------
		internal static string EdgeSlot(bool n, bool s, bool w, bool e, bool nw, bool ne, bool sw, bool se)
		{
			if ((!n && !s) || (!w && !e))
			{
				return null;
			}
			if (!n)
			{
				return !w ? "NW" : (!e ? "NE" : "N");
			}
			if (!s)
			{
				return !w ? "SW" : (!e ? "SE" : "S");
			}
			if (!w)
			{
				return "W";
			}
			if (!e)
			{
				return "E";
			}
			// 上下左右がすべて同じ所: 斜めが外なら内側の角（外側がある向きで呼ぶ）
			if (!nw)
			{
				return "INW";
			}
			if (!ne)
			{
				return "INE";
			}
			if (!sw)
			{
				return "ISW";
			}
			return !se ? "ISE" : "C";
		}

		//-------------------------------------------------------------------------------
		// くり返しのパーツ（森・地面の模様）の並びを整える処理
		// マスごとに、まわりの並びにいちばん合う「ずらし方」を選び、そのずらし方で置くべきブロックにする（並びのずれた所だけが変わる）。
		// 縦に見て、つながった所の上から上端の行数ぶん・下から下端の行数ぶんは変えない（マップの外は続いているとみなす）。
		// 上の行に森を塗り足して、前の上端が森の中になった所は、本体のブロックにする
		//-------------------------------------------------------------------------------
		private static void FixRepeats(MapAssistContext context, MapAssistWork work, IList<MapAssistPart> parts, Rectangle range, MapAssistSupportReport report)
		{
			// 重なって並ぶ木は、並びを自分で決めるので対象にしない
			List<MapAssistPart> list = parts.Where(p => p.Kind == MapAssistKind.Repeat && p.GetSlot(MapAssistPart.SlotBody) != null && !MapAssistTrees.IsStackedTree(p)).Take(MaxParts).ToList();
			if (list.Count == 0)
			{
				return;
			}
			uint[] member = Membership(context, work, list);
			List<List<int>> groups;
			int[] group = Components(work, member, out groups);
			Dictionary<int, int> changes = new Dictionary<int, int>();
			for (int g = 0; g < groups.Count; g++)
			{
				List<int> cells = groups[g];
				if (!cells.Any(i => range.Contains(i % work.Width, i / work.Width)))
				{
					continue;
				}
				int winner = Winner(cells, member, i => 0U, -1);
				MapAssistPart part = list[winner];
				MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
				MapAssistGrid top = SameWidth(part.GetSlot(MapAssistPart.SlotTop), body);
				MapAssistGrid bottom = SameWidth(part.GetSlot(MapAssistPart.SlotBottom), body);
				int topRows = top != null ? top.Height : 0;
				int bottomRows = bottom != null ? bottom.Height : 0;
				HashSet<int> bodyBlocks = new HashSet<int>(body.Cells.Select(c => context.ToGlobal(c)).Where(id => id >= 0));
				HashSet<int> endBlocks = new HashSet<int>((top != null ? top.Cells : new MapAssistCell[0]).Concat(bottom != null ? bottom.Cells : new MapAssistCell[0]).Select(c => context.ToGlobal(c)).Where(id => id >= 0));
				// 縦の向きで、つながった所の端までのマスの数（マップの端に着いたら十分に遠いとみなす）
				Dictionary<int, int> above = new Dictionary<int, int>();
				Dictionary<int, int> below = new Dictionary<int, int>();
				foreach (int i in cells)
				{
					above[i] = Distance(work, group, g, i, -1, topRows);
					below[i] = Distance(work, group, g, i, 1, bottomRows);
				}
				foreach (int i in cells)
				{
					int x = i % work.Width;
					int y = i / work.Width;
					if (!range.Contains(x, y) || (member[i] & (1U << winner)) == 0U)
					{
						continue;
					}
					// ずらし方は、まわり（5×5）の本体のマスでいちばん多く合うもの。同じなら今のブロックのままになるもの。
					// つながった所の中でもずらし方が違う所（マップの端で切れた木など）は、それぞれのまま残す
					int bestX = 0;
					int bestY = 0;
					int bestScore = -1;
					// 今のブロックのままになるずらし方の中で、いちばん高い点
					int keepScore = -1;
					for (int py = 0; py < body.Height; py++)
					{
						for (int px = 0; px < body.Width; px++)
						{
							int score = 0;
							for (int dy = -RepeatWindow; dy <= RepeatWindow; dy++)
							{
								for (int dx = -RepeatWindow; dx <= RepeatWindow; dx++)
								{
									int nx = x + dx;
									int ny = y + dy;
									if (!work.Contains(nx, ny))
									{
										continue;
									}
									int j = ny * work.Width + nx;
									if (group[j] != g || above[j] < topRows || below[j] < bottomRows)
									{
										continue;
									}
									if (context.ToGlobal(body[(nx + px) % body.Width, (ny + py) % body.Height]) == work.Blocks[j])
									{
										score += 2;
									}
								}
							}
							if (context.ToGlobal(RepeatCell(body, top, bottom, x, y, px, py, above[i], below[i])) == work.Blocks[i])
							{
								score++;
								keepScore = Math.Max(keepScore, score);
							}
							if (score > bestScore)
							{
								bestScore = score;
								bestX = px;
								bestY = py;
							}
						}
					}
					// 今のままのずらし方と比べて、まわりで合うマスが 3 つ以上多くなければ変えない（もとの並びが入り組んでいる所を崩さない）
					if (keepScore >= 0 && bestScore < keepScore + RepeatMargin)
					{
						continue;
					}
					int id = context.ToGlobal(RepeatCell(body, top, bottom, x, y, bestX, bestY, above[i], below[i]));
					if (id < 0 || work.Blocks[i] == id)
					{
						continue;
					}
					// 整えるときは、本体の所を本体のブロックにするだけにする（上端・下端は塗るときに付ける）。
					// 1 本だけの木や、森の横の端・角に上端・下端を付けて崩さないため
					bool atEnd = (top != null && above[i] < top.Height) || (bottom != null && below[i] < bottom.Height);
					if (atEnd || !(bodyBlocks.Contains(work.Blocks[i]) || endBlocks.Contains(work.Blocks[i])))
					{
						continue;
					}
					changes[i] = id;
				}
			}
			// すべてのマスのずらし方を元の並びで決めてから書き換える
			foreach (KeyValuePair<int, int> change in changes)
			{
				work.Blocks[change.Key] = change.Value;
				report.RepeatFixed++;
			}
		}

		//-------------------------------------------------------------------------------
		// くり返しで、その位置に置くマスを返す処理（上端・下端の行なら上端・下端、それ以外は本体。px・py はずらし方）
		//-------------------------------------------------------------------------------
		internal static MapAssistCell RepeatCell(MapAssistGrid body, MapAssistGrid top, MapAssistGrid bottom, int x, int y, int px, int py, int above, int below)
		{
			int column = (x + px) % body.Width;
			if (top != null && above < top.Height)
			{
				return top[column, above];
			}
			if (bottom != null && below < bottom.Height)
			{
				return bottom[column, bottom.Height - 1 - below];
			}
			return body[column, (y + py) % body.Height];
		}

		//-------------------------------------------------------------------------------
		// 縦の向き（step = -1 で上、1 で下）に、同じ所が何マス続くかを数える処理（limit まで数えれば十分。マップの端は続いているとみなす）
		//-------------------------------------------------------------------------------
		private static int Distance(MapAssistWork work, int[] group, int g, int i, int step, int limit)
		{
			int x = i % work.Width;
			int y = i / work.Width;
			for (int d = 0; d < limit; d++)
			{
				int ny = y + step * (d + 1);
				if (ny < 0 || ny >= work.Height)
				{
					return limit;
				}
				if (group[ny * work.Width + x] != g)
				{
					return d;
				}
			}
			return limit;
		}
	}
}
