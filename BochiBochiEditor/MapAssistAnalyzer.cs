using System;
using System.Collections.Generic;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 自動の割り振りに使う材料: 同じタイルセットを使っているマップ 1 枚ぶんの並び
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistSample
	{
		public int Width;
		public int Height;
		// 行ごとに左から。ブロックはマップの通し番号、移動エリアは 0〜63
		public int[] Blocks;
		public int[] Collisions;
		// タイルセット2 も今のマップと同じか（違うマップでは、タイルセット1 のブロックだけを数える）
		public bool SamePair;
	}

	//-------------------------------------------------------------------------------
	// 自動の割り振りに使う材料のまとまり（マップの並びと、ブロックごとの挙動の値）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistAutoInput
	{
		public readonly List<MapAssistSample> Samples = new List<MapAssistSample>();
		// マップの通し番号ごとの挙動（踏んだときの効果）の値。データの無い番号は -1
		public int[] Behaviors = new int[0];
	}

	//-------------------------------------------------------------------------------
	// マップでの使われ方と挙動の値から、パーツの候補を作る処理（「マップから候補を作る」）
	//
	//   草むら・水・段差 … ブロックの挙動の値で決める（マップで使われていなくても拾える）
	//   水の縁           … 水のブロックごとに「上下左右・斜めが水か」の傾向を調べ、縁つきの枠に当てはめる
	//   地面             … 通れて、よく使われ、まわりも通れるマスであることが多いブロック。2×2 の並びがくり返されていれば、くり返しにする
	//   森               … 通れないブロックの 2×2 の並びが、縦横にくり返されている所
	//   建物・飾り       … 通れないブロックのかたまり（ほかのかたまりを含むものは、分けて数える）
	//
	// 結果は大まかな候補で、間違いを含む。名前の先頭に「自動: 」を付けて、利用者が確かめて直す前提にする
	//-------------------------------------------------------------------------------
	internal static class MapAssistAnalyzer
	{
		// 自動で作った候補の名前の先頭（もう一度候補を作るとき、この名前のままのパーツは作り直す）
		public const string AutoPrefix = "自動: ";

		// 候補にする数の上限
		private const int MaxGroundBlocks = 8;
		private const int MaxGroundTilings = 2;
		private const int MaxForests = 4;
		private const int MaxWaters = 3;
		private const int MaxBuildings = 12;
		private const int MaxDecorations = 8;
		// 部品の候補にするかたまりの、1 辺の上限
		private const int MaxStampSide = 12;

		// 候補の名前を画面の言語にする処理（Analyze の間だけ使う）
		private static Func<string, string> nameOf = text => text;

		// 解析の途中で使う、ブロックごとの集計
		private sealed class Stats
		{
			public MapAssistContext Context;
			public List<MapAssistSample> Samples;
			// マップごとの並び（今のタイルセットで意味のあるブロックだけ。それ以外は -1）
			public List<int[]> Grids;
			public int[] Count;
			// いちばん多く付いている移動エリアの値と、通れる値の中でいちばん多いもの（無ければ -1）
			public int[] Dominant;
			public int[] PassableDominant;
			// 通れる値・通れない値が付いていた回数
			public int[] PassableCount;
			public int[] BlockedCount;
			public int[] SelfAdjacent;
			// 上下左右の隣のマスの数と、そのうち通れるマスの数
			public int[] Neighbors;
			public int[] OpenNeighbors;
			public long Cells;
			public bool[] Grass;
			public bool[] Water;
			public bool[] Ledge;
			public bool[] Door;
			public bool[] Background;

			// ほぼいつも通れないブロックか（9 割以上。マップで使われていないブロックは false）
			public bool Blocked(int id)
			{
				return this.Count[id] > 0 && this.BlockedCount[id] * 10 >= this.Count[id] * 9;
			}

			// 通れる所に置かれることがあるブロックか（4 分の 1 以上）。
			// 地面のブロックは、マップの外側を埋めるのにも使われて「通れない」が付くことが多いので、多数決では決めない
			public bool Passable(int id)
			{
				return this.Count[id] > 0 && this.PassableCount[id] * 4 >= this.Count[id];
			}

			// 開けた所に置かれるブロックか（上下左右の隣の 6 割以上が通れるマス）。地面・道・水面などで、建物の入口や屋根は外れる
			public bool Open(int id)
			{
				return this.Neighbors[id] > 0 && this.OpenNeighbors[id] * 10 >= this.Neighbors[id] * 6;
			}

			// パーツのマスにする（移動エリアは、通れるブロックなら通れる値の中でいちばん多いもの、それ以外はいちばん多い値）
			public MapAssistCell Cell(int id)
			{
				if (id < 0)
				{
					return MapAssistCell.Empty;
				}
				return this.Context.ToCell(id, this.Passable(id) && this.PassableDominant[id] >= 0 ? this.PassableDominant[id] : this.Dominant[id]);
			}
		}

		//-------------------------------------------------------------------------------
		// 候補のパーツを作る処理
		// reserved は、利用者が自分で指定したパーツで使っているブロック（面の候補からは外す）
		// translate は、候補の名前を画面の言語にする処理（null ならそのまま）
		//-------------------------------------------------------------------------------
		public static List<MapAssistPart> Analyze(MapAssistContext context, MapAssistAutoInput input, HashSet<int> reserved, Func<string, string> translate)
		{
			List<MapAssistPart> parts = new List<MapAssistPart>();
			if (context == null || input == null || context.TotalBlocks <= 0)
			{
				return parts;
			}
			nameOf = translate ?? (text => text);
			reserved = reserved ?? new HashSet<int>();
			Stats stats = Collect(context, input);
			int total = context.TotalBlocks;
			bool[] taken = new bool[total];
			foreach (int id in reserved)
			{
				if (id >= 0 && id < total)
				{
					taken[id] = true;
				}
			}

			// 地面（2×2 のくり返しと、それ以外のブロック）
			Func<int, bool> plain = id => !taken[id] && stats.Passable(id) && stats.Open(id) && !stats.Grass[id] && !stats.Water[id] && !stats.Ledge[id] && !stats.Door[id];
			HashSet<int> groundBlocks = new HashSet<int>();
			AddTilings(parts, stats, groundBlocks, plain, "地面（くり返し）", MapAssistRole.Ground, MaxGroundTilings, false);
			foreach (int id in groundBlocks)
			{
				taken[id] = true;
			}
			List<int> ground = FindGround(stats, taken);
			AddArea(parts, stats, "地面", MapAssistRole.Ground, ground, taken);
			// 草むら・段差（挙動の値で決める）
			AddArea(parts, stats, "草むら", MapAssistRole.Grass, Enumerable.Range(0, total).Where(id => stats.Grass[id] && !taken[id]).OrderByDescending(id => stats.Count[id]).ToList(), taken);
			// 水（縁が分かれば縁つき、分からなければ面）
			AddWater(parts, stats, taken);
			AddArea(parts, stats, "段差", MapAssistRole.Cliff, Enumerable.Range(0, total).Where(id => stats.Ledge[id] && !taken[id]).OrderByDescending(id => stats.Count[id]).ToList(), taken);
			// 森（2×2 のくり返し）
			HashSet<int> forestBlocks = new HashSet<int>();
			AddTilings(parts, stats, forestBlocks, id => stats.Blocked(id), "森", MapAssistRole.Tree, MaxForests, true);
			// 建物・飾り（通れないブロックのかたまり）と、1 マスだけの小物
			AddStamps(parts, stats, forestBlocks, taken);
			return parts;
		}

		//-------------------------------------------------------------------------------
		// ブロックごとに、付けるとよい移動エリアの値を返す処理（マップの通し番号ごと。どのマップにも無いブロックは -1）
		// 候補のパーツのマスと同じ決め方（通れるブロックなら通れる値の中でいちばん多いもの、それ以外はいちばん多い値）
		//-------------------------------------------------------------------------------
		public static int[] CollisionTable(MapAssistContext context, MapAssistAutoInput input)
		{
			if (context == null || input == null || context.TotalBlocks <= 0)
			{
				return new int[0];
			}
			Stats stats = Collect(context, input);
			int[] table = new int[context.TotalBlocks];
			for (int id = 0; id < table.Length; id++)
			{
				table[id] = stats.Count[id] > 0 ? stats.Cell(id).Collision : -1;
			}
			return table;
		}

		//-------------------------------------------------------------------------------
		// マップの並びから、ブロックごとの集計を作る処理
		//-------------------------------------------------------------------------------
		private static Stats Collect(MapAssistContext context, MapAssistAutoInput input)
		{
			int total = context.TotalBlocks;
			Stats stats = new Stats
			{
				Context = context,
				Samples = input.Samples,
				Grids = new List<int[]>(),
				Count = new int[total],
				Dominant = new int[total],
				PassableDominant = new int[total],
				PassableCount = new int[total],
				BlockedCount = new int[total],
				SelfAdjacent = new int[total],
				Neighbors = new int[total],
				OpenNeighbors = new int[total],
				Grass = new bool[total],
				Water = new bool[total],
				Ledge = new bool[total],
				Door = new bool[total],
				Background = new bool[total],
			};
			int[,] collisions = new int[total, 64];
			foreach (MapAssistSample sample in input.Samples)
			{
				int[] grid = new int[sample.Blocks.Length];
				for (int i = 0; i < grid.Length; i++)
				{
					int id = sample.Blocks[i];
					// データの無いブロックと、タイルセット2 が違うマップのタイルセット2 のブロックは数えない
					grid[i] = context.IsValidBlock(id) && (id < context.PrimarySlots || sample.SamePair) ? id : -1;
				}
				stats.Grids.Add(grid);
				for (int y = 0; y < sample.Height; y++)
				{
					for (int x = 0; x < sample.Width; x++)
					{
						int i = y * sample.Width + x;
						int id = grid[i];
						if (id < 0)
						{
							continue;
						}
						stats.Count[id]++;
						stats.Cells++;
						int collision = sample.Collisions[i];
						if (collision >= 0 && collision < 64)
						{
							collisions[id, collision]++;
							if ((collision & 3) == 0)
							{
								stats.PassableCount[id]++;
							}
							else
							{
								stats.BlockedCount[id]++;
							}
						}
						if (x + 1 < sample.Width && grid[i + 1] == id)
						{
							stats.SelfAdjacent[id]++;
						}
						if (y + 1 < sample.Height && grid[i + sample.Width] == id)
						{
							stats.SelfAdjacent[id]++;
						}
						foreach (int next in new[] { x > 0 ? i - 1 : -1, x + 1 < sample.Width ? i + 1 : -1, y > 0 ? i - sample.Width : -1, y + 1 < sample.Height ? i + sample.Width : -1 })
						{
							if (next < 0)
							{
								continue;
							}
							stats.Neighbors[id]++;
							if ((sample.Collisions[next] & 3) == 0)
							{
								stats.OpenNeighbors[id]++;
							}
						}
					}
				}
			}
			for (int id = 0; id < total; id++)
			{
				stats.Dominant[id] = -1;
				stats.PassableDominant[id] = -1;
				int best = 0;
				int bestPassable = 0;
				for (int c = 0; c < 64; c++)
				{
					if (collisions[id, c] > best)
					{
						best = collisions[id, c];
						stats.Dominant[id] = c;
					}
					if ((c & 3) == 0 && collisions[id, c] > bestPassable)
					{
						bestPassable = collisions[id, c];
						stats.PassableDominant[id] = c;
					}
				}
				int behavior = id < input.Behaviors.Length ? input.Behaviors[id] : -1;
				if (!context.IsValidBlock(id) || behavior < 0)
				{
					continue;
				}
				// 挙動の値（ファイアレッド・エメラルドで共通の番号）
				stats.Grass[id] = behavior == 0x02 || behavior == 0x03;
				stats.Water[id] = behavior >= 0x10 && behavior <= 0x15;
				stats.Ledge[id] = behavior >= 0x38 && behavior <= 0x3B;
				stats.Door[id] = behavior >= 0x60 && behavior <= 0x6F;
			}
			// 下地（かたまりを探すときに、物として扱わないブロック）: 通れて、開けた所に置かれるもの。
			// 同じブロックどうしが隣り合うかでは決めない（ファイアレッドの地面は、4 種類のブロックを 2×2 で並べているので隣り合わない）
			long threshold = Math.Max(4, stats.Cells / 2000);
			for (int id = 0; id < total; id++)
			{
				stats.Background[id] = stats.Grass[id] || (stats.Passable(id) && stats.Count[id] >= threshold && stats.Open(id));
			}
			return stats;
		}

		//-------------------------------------------------------------------------------
		// 面の候補を 1 つ足す処理（ブロックが 1 つも無ければ足さない）。使ったブロックは taken に入れる
		//-------------------------------------------------------------------------------
		private static void AddArea(List<MapAssistPart> parts, Stats stats, string name, MapAssistRole role, List<int> blocks, bool[] taken)
		{
			if (blocks.Count == 0)
			{
				return;
			}
			MapAssistGrid grid = new MapAssistGrid(blocks.Count, 1);
			for (int i = 0; i < blocks.Count; i++)
			{
				grid.Cells[i] = stats.Cell(blocks[i]);
				taken[blocks[i]] = true;
			}
			MapAssistPart part = new MapAssistPart { Name = nameOf(AutoPrefix) + nameOf(name), Kind = MapAssistKind.Area, Role = role };
			part.Slots[MapAssistPart.SlotBlocks] = grid;
			parts.Add(part);
		}

		//-------------------------------------------------------------------------------
		// 地面のブロックを選ぶ処理
		// 通れて、草むら・水・段差・ワープではなく、よく使われ、開けた所に置かれるもの（多い順に数個）
		//-------------------------------------------------------------------------------
		private static List<int> FindGround(Stats stats, bool[] taken)
		{
			long threshold = Math.Max(8, stats.Cells / 400);
			List<int> ground = Enumerable.Range(0, stats.Count.Length)
				.Where(id => !taken[id] && stats.Passable(id) && !stats.Grass[id] && !stats.Water[id] && !stats.Ledge[id] && !stats.Door[id]
					&& stats.Count[id] >= threshold && stats.Open(id))
				.OrderByDescending(id => stats.Count[id]).Take(MaxGroundBlocks).ToList();
			if (ground.Count == 0)
			{
				// 条件に合うものが無ければ（マップが小さいなど）、通れるブロックでいちばん多いものを 1 つ
				int best = Enumerable.Range(0, stats.Count.Length)
					.Where(id => !taken[id] && stats.Passable(id) && !stats.Grass[id] && !stats.Water[id] && !stats.Ledge[id] && !stats.Door[id])
					.OrderByDescending(id => stats.Count[id]).DefaultIfEmpty(-1).First();
				if (best >= 0 && stats.Count[best] > 0)
				{
					ground.Add(best);
				}
			}
			return ground;
		}

		//-------------------------------------------------------------------------------
		// 水の候補を足す処理
		// 水のブロックごとに「上下左右が水か」「（上下左右がすべて水のとき）斜めが水か」の傾向を数え、
		// 中央（すべて水）・辺（1 方向だけ水でない）・外側の角（隣り合う 2 方向が水でない）・内側の角（斜め 1 つだけ水でない）に当てはめる。
		// 中央と、辺のうち 3 つ以上が見つかれば縁つきにする（絵柄の違う水や岸があれば、数個まで。同じ水に草の岸と砂浜があれば別の縁つきにする）。
		// 残った水のブロックは、まとめて面にする。
		// 岸のブロックは、挙動が水でないことがある（ファイアレッドの下側の岸は、陸のブロック）ので、
		// 「ほとんどいつも水の隣に置かれるブロック」も、水の仲間として数える
		//-------------------------------------------------------------------------------
		private static void AddWater(List<MapAssistPart> parts, Stats stats, bool[] taken)
		{
			int total = stats.Count.Length;
			List<int> water = Enumerable.Range(0, total).Where(id => stats.Water[id] && !taken[id]).OrderByDescending(id => stats.Count[id]).ToList();
			if (water.Count == 0)
			{
				return;
			}
			// 岸のブロック（水ではないが、6 割以上が水の隣に置かれているもの）を探す
			int[] placed = new int[total];
			int[] beside = new int[total];
			for (int s = 0; s < stats.Samples.Count; s++)
			{
				MapAssistSample sample = stats.Samples[s];
				int[] grid = stats.Grids[s];
				int w = sample.Width;
				for (int y = 1; y < sample.Height - 1; y++)
				{
					for (int x = 1; x < w - 1; x++)
					{
						int id = grid[y * w + x];
						if (id < 0 || stats.Water[id])
						{
							continue;
						}
						placed[id]++;
						foreach (int next in new[] { grid[(y - 1) * w + x], grid[(y + 1) * w + x], grid[y * w + x - 1], grid[y * w + x + 1] })
						{
							if (next >= 0 && stats.Water[next])
							{
								beside[id]++;
								break;
							}
						}
					}
				}
			}
			bool[] wet = new bool[total];
			for (int id = 0; id < total; id++)
			{
				wet[id] = (stats.Water[id] && !taken[id])
					|| (!stats.Water[id] && !taken[id] && !stats.Grass[id] && !stats.Ledge[id] && !stats.Door[id] && placed[id] >= 3 && beside[id] * 10 >= placed[id] * 6);
			}
			// 傾向の番号: 下位 4 ビット = 上・下・左・右が水か、上位 4 ビット = 左上・右上・左下・右下が水か（上下左右がすべて水のときだけ数える）
			Dictionary<int, int>[] patterns = new Dictionary<int, int>[total];
			// 水のブロックどうしの隣り合い（右隣・下隣）の回数。縁のブロックを、同じ絵柄の水から選ぶのに使う
			Dictionary<long, int> rightOf = new Dictionary<long, int>();
			Dictionary<long, int> below = new Dictionary<long, int>();
			for (int s = 0; s < stats.Samples.Count; s++)
			{
				MapAssistSample sample = stats.Samples[s];
				int[] grid = stats.Grids[s];
				int w = sample.Width;
				// マップの端のマスは、外側が分からないので数えない
				for (int y = 1; y < sample.Height - 1; y++)
				{
					for (int x = 1; x < w - 1; x++)
					{
						int id = grid[y * w + x];
						if (id < 0 || !wet[id])
						{
							continue;
						}
						int[] around = { grid[(y - 1) * w + x], grid[(y + 1) * w + x], grid[y * w + x - 1], grid[y * w + x + 1], grid[(y - 1) * w + x - 1], grid[(y - 1) * w + x + 1], grid[(y + 1) * w + x - 1], grid[(y + 1) * w + x + 1] };
						if (around.Any(v => v < 0))
						{
							continue;
						}
						if (wet[around[3]])
						{
							BumpPair(rightOf, id, around[3]);
						}
						if (wet[around[1]])
						{
							BumpPair(below, id, around[1]);
						}
						int key = 0;
						for (int k = 0; k < 4; k++)
						{
							if (wet[around[k]])
							{
								key |= 1 << k;
							}
						}
						if (key == 0x0F)
						{
							for (int k = 4; k < 8; k++)
							{
								if (wet[around[k]])
								{
									key |= 1 << k;
								}
							}
						}
						if (patterns[id] == null)
						{
							patterns[id] = new Dictionary<int, int>();
						}
						int seen;
						patterns[id].TryGetValue(key, out seen);
						patterns[id][key] = seen + 1;
					}
				}
			}
			// ブロックごとの、いちばん多い傾向とその回数
			int[] topKey = new int[total];
			int[] topCount = new int[total];
			List<int> candidates = new List<int>();
			for (int id = 0; id < total; id++)
			{
				if (!wet[id] || patterns[id] == null)
				{
					continue;
				}
				KeyValuePair<int, int> top = patterns[id].OrderByDescending(p => p.Value).First();
				topKey[id] = top.Key;
				topCount[id] = top.Value;
				candidates.Add(id);
			}
			// 枠ごとの傾向の番号
			Dictionary<string, int> slotKeys = new Dictionary<string, int>
			{
				{ "C", 0xFF },
				{ "N", 0x0E }, { "S", 0x0D }, { "W", 0x0B }, { "E", 0x07 },
				{ "NW", 0x0A }, { "NE", 0x06 }, { "SW", 0x09 }, { "SE", 0x05 },
				{ "INW", 0xEF }, { "INE", 0xDF }, { "ISW", 0xBF }, { "ISE", 0x7F },
			};
			string[] sideNames = { "N", "S", "W", "E" };
			string[] cornerNames = { "NW", "NE", "SW", "SE" };
			HashSet<int> used = new HashSet<int>();
			HashSet<int> skipped = new HashSet<int>();
			int number = 1;
			// 中央にするブロックを多い順に試す（縁が見つからない水は飛ばして、次の絵柄を試す。中央は、別の岸の組でも使い回す）
			for (int attempt = 0; attempt < MaxWaters * 3 && number <= MaxWaters; attempt++)
			{
				Dictionary<string, int> chosen = new Dictionary<string, int>();
				HashSet<int> mine = new HashSet<int>();
				HashSet<int> usedEdges = new HashSet<int>(used);
				// 枠ごとの「先に決まった枠のブロックと、どう隣り合っているか」の数え方（右隣・下隣の組）。
				// 中央はいちばん多いものを選び、辺は中央の隣、角は辺の隣に実際に出るものを選ぶ（絵柄の違う水の縁が混ざらないように）
				Func<string, int> pick = name =>
				{
					int id;
					return chosen.TryGetValue(name, out id) ? id : -1;
				};
				Dictionary<string, Func<int, int>> scores = new Dictionary<string, Func<int, int>>
				{
					{ "C", b => stats.Water[b] && !skipped.Contains(b) ? topCount[b] : 0 },
					{ "N", b => PairCount(below, b, pick("C")) },
					{ "S", b => PairCount(below, pick("C"), b) },
					{ "W", b => PairCount(rightOf, b, pick("C")) },
					{ "E", b => PairCount(rightOf, pick("C"), b) },
					{ "NW", b => Both(PairCount(rightOf, b, pick("N")), PairCount(below, b, pick("W"))) },
					{ "NE", b => Both(PairCount(rightOf, pick("N"), b), PairCount(below, b, pick("E"))) },
					{ "SW", b => Both(PairCount(rightOf, b, pick("S")), PairCount(below, pick("W"), b)) },
					{ "SE", b => Both(PairCount(rightOf, pick("S"), b), PairCount(below, pick("E"), b)) },
					{ "INW", b => Both(PairCount(rightOf, pick("N"), b), PairCount(below, pick("W"), b)) },
					{ "INE", b => Both(PairCount(rightOf, b, pick("N")), PairCount(below, pick("E"), b)) },
					{ "ISW", b => Both(PairCount(rightOf, pick("S"), b), PairCount(below, b, pick("W"))) },
					{ "ISE", b => Both(PairCount(rightOf, b, pick("S")), PairCount(below, b, pick("E"))) },
				};
				// 枠に合うブロックを、点数の高い順に並べる処理（そのブロックでいちばん多い傾向が、この枠の傾向であること）
				Func<string, List<KeyValuePair<int, int>>> ranked = name => candidates
					.Where(id => topKey[id] == slotKeys[name] && (name == "C" ? !skipped.Contains(id) : !usedEdges.Contains(id)) && !mine.Contains(id))
					.Select(id => new KeyValuePair<int, int>(id, scores[name](id))).Where(p => p.Value > 0)
					.OrderByDescending(p => p.Value).ThenBy(p => p.Key).ToList();
				List<KeyValuePair<int, int>> centers = ranked("C");
				if (centers.Count == 0)
				{
					break;
				}
				chosen["C"] = centers[0].Key;
				mine.Add(centers[0].Key);
				// 辺: 中央の隣によく出るもの（各 3 つまで）の組み合わせのうち、外側の角が「隣り合う 2 つの辺の両方に接して出る」ものがいちばん多い組を選ぶ
				// （上が草の岸なのに下は砂浜、のように絵柄が混ざると、その間の角が無いので選ばれない。残った岸は、次の組で同じ中央と組み合わせる）
				Dictionary<string, List<int>> sideRanks = sideNames.ToDictionary(name => name, name => ranked(name).Take(3).Select(p => p.Key).ToList());
				int[] bestSides = null;
				long bestValue = -1;
				foreach (int n in sideRanks["N"].Concat(new[] { -1 }))
				{
					foreach (int s in sideRanks["S"].Concat(new[] { -1 }))
					{
						foreach (int w in sideRanks["W"].Concat(new[] { -1 }))
						{
							foreach (int e in sideRanks["E"].Concat(new[] { -1 }))
							{
								int[] sides = { n, s, w, e };
								if (sides.Count(v => v >= 0) < 3 || sides.Where(v => v >= 0).Distinct().Count() != sides.Count(v => v >= 0))
								{
									continue;
								}
								chosen["N"] = n;
								chosen["S"] = s;
								chosen["W"] = w;
								chosen["E"] = e;
								int corners = cornerNames.Count(name => candidates.Any(id => topKey[id] == slotKeys[name] && !usedEdges.Contains(id) && !sides.Contains(id) && scores[name](id) >= 100000));
								long adjacency = sides.Select((v, k) => v < 0 ? 0 : Math.Min(9999, scores[sideNames[k]](v))).Sum();
								long value = corners * 100000000L + sides.Count(v => v >= 0) * 1000000L + adjacency;
								if (value > bestValue)
								{
									bestValue = value;
									bestSides = sides;
								}
							}
						}
					}
				}
				foreach (string name in sideNames)
				{
					chosen.Remove(name);
				}
				if (bestSides != null)
				{
					for (int k = 0; k < sideNames.Length; k++)
					{
						if (bestSides[k] >= 0)
						{
							chosen[sideNames[k]] = bestSides[k];
							mine.Add(bestSides[k]);
						}
					}
				}
				// 角: 先に決まった辺の隣に出るもの
				foreach (string name in new[] { "NW", "NE", "SW", "SE", "INW", "INE", "ISW", "ISE" })
				{
					List<KeyValuePair<int, int>> list = ranked(name);
					if (list.Count > 0)
					{
						chosen[name] = list[0].Key;
						mine.Add(list[0].Key);
					}
				}
				if (sideNames.Count(name => chosen.ContainsKey(name)) < 3)
				{
					skipped.Add(chosen["C"]);
					continue;
				}
				MapAssistPart edge = new MapAssistPart { Name = nameOf(AutoPrefix) + nameOf("水") + (number > 1 ? " " + number : string.Empty), Kind = MapAssistKind.Edge, Role = MapAssistRole.Water };
				foreach (string name in slotKeys.Keys.Where(chosen.ContainsKey))
				{
					MapAssistGrid one = new MapAssistGrid(1, 1);
					one.Cells[0] = stats.Cell(chosen[name]);
					edge.Slots[name] = one;
					taken[chosen[name]] = true;
					if (name != "C")
					{
						used.Add(chosen[name]);
					}
				}
				parts.Add(edge);
				number++;
			}
			AddArea(parts, stats, number > 1 ? "水（ほか）" : "水", MapAssistRole.Water, water.Where(id => !used.Contains(id)).ToList(), taken);
		}

		//-------------------------------------------------------------------------------
		// 2 つのブロックの組の回数を 1 増やす処理・回数を読む処理（どちらかが -1 なら 0）
		//-------------------------------------------------------------------------------
		private static void BumpPair(Dictionary<long, int> counts, int first, int second)
		{
			long key = ((long)first << 16) | (long)second;
			int seen;
			counts.TryGetValue(key, out seen);
			counts[key] = seen + 1;
		}

		private static int PairCount(Dictionary<long, int> counts, int first, int second)
		{
			int seen;
			return first >= 0 && second >= 0 && counts.TryGetValue(((long)first << 16) | (long)second, out seen) ? seen : 0;
		}

		//-------------------------------------------------------------------------------
		// 角の点数を返す処理: 2 つの辺の両方と隣り合っていれば高く、片方だけなら低くする
		//-------------------------------------------------------------------------------
		private static int Both(int first, int second)
		{
			return first > 0 && second > 0 ? 100000 + first + second : first + second;
		}

		// 2 マスの横の並び
		private struct Pair : IEquatable<Pair>
		{
			public int Left;
			public int Right;

			public Pair(int left, int right)
			{
				this.Left = left;
				this.Right = right;
			}

			public bool Equals(Pair other)
			{
				return this.Left == other.Left && this.Right == other.Right;
			}

			public override bool Equals(object obj)
			{
				return obj is Pair && this.Equals((Pair)obj);
			}

			public override int GetHashCode()
			{
				return this.Left * 1031 + this.Right;
			}
		}

		//-------------------------------------------------------------------------------
		// 2×2 のくり返しの候補（森・地面）を足す処理。usable は、並びに使えるブロックの条件（森なら通れないブロック、地面なら通れるブロック）
		// 1. 条件に合うブロックの 2×2 の並びが、右隣と下隣にも同じ並びで続いている所を数える（ずれた 4 通りは同じ並びとして数える）
		// 2. 多い並びについて、上の端（並びのすぐ上が、並びの行でない所）を調べ、いちばん多い行の組を「上端」にする。
		//    並びの向き（どの行・どの列から始まるか）も、上の端・左の端の出方に合わせる
		// 3. 同じように、下の端のすぐ下の行の組を「下端」にする
		// 端の行が下地（地面など）なら、上端・下端は空のままにする。edges が false なら（地面）、上端・下端は調べない
		//-------------------------------------------------------------------------------
		private static void AddTilings(List<MapAssistPart> parts, Stats stats, HashSet<int> forestBlocks, Func<int, bool> usable, string name, MapAssistRole role, int limit, bool edges)
		{
			Dictionary<long, int> tilings = new Dictionary<long, int>();
			Dictionary<long, int[]> units = new Dictionary<long, int[]>();
			for (int s = 0; s < stats.Samples.Count; s++)
			{
				MapAssistSample sample = stats.Samples[s];
				int[] g = stats.Grids[s];
				int w = sample.Width;
				for (int y = 0; y + 3 < sample.Height; y++)
				{
					for (int x = 0; x + 3 < w; x++)
					{
						int a = g[y * w + x], b = g[y * w + x + 1], c = g[(y + 1) * w + x], d = g[(y + 1) * w + x + 1];
						if (a < 0 || b < 0 || c < 0 || d < 0 || (a == b && b == c && c == d)
							|| !usable(a) || !usable(b) || !usable(c) || !usable(d))
						{
							continue;
						}
						if (g[y * w + x + 2] != a || g[y * w + x + 3] != b || g[(y + 1) * w + x + 2] != c || g[(y + 1) * w + x + 3] != d
							|| g[(y + 2) * w + x] != a || g[(y + 2) * w + x + 1] != b || g[(y + 3) * w + x] != c || g[(y + 3) * w + x + 1] != d)
						{
							continue;
						}
						int[] unit = Canonical(a, b, c, d);
						long key = ((long)unit[0] << 30) | ((long)unit[1] << 20) | ((long)unit[2] << 10) | (long)unit[3];
						int seen;
						tilings.TryGetValue(key, out seen);
						tilings[key] = seen + 1;
						units[key] = unit;
					}
				}
			}
			int number = 1;
			int walls = 1;
			foreach (KeyValuePair<long, int> tiling in tilings.Where(t => t.Value >= 4).OrderByDescending(t => t.Value).ThenBy(t => t.Key))
			{
				if (number + walls - 1 > limit)
				{
					break;
				}
				int[] unit = units[tiling.Key];
				// 先に選んだ森と同じブロックを使う並びは、向きが違うだけのことが多いので飛ばす
				if (unit.Any(forestBlocks.Contains))
				{
					continue;
				}
				// 森の並びのうち、横か縦に同じブロックが続くもの（木のように 2 マスで 1 つの絵にならないもの）は、崖・壁として出す
				bool wall = edges && ((unit[0] == unit[1] && unit[2] == unit[3]) || (unit[0] == unit[2] && unit[1] == unit[3]));
				MapAssistPart part = wall
					? BuildForest(stats, unit, nameOf("崖・壁") + " " + walls++, MapAssistRole.Cliff, edges)
					: BuildForest(stats, unit, nameOf(name) + " " + number, role, edges);
				foreach (MapAssistCell cell in part.AllCells())
				{
					forestBlocks.Add(stats.Context.ToGlobal(cell));
				}
				parts.Add(part);
				if (!wall)
				{
					number++;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 2×2 の並びの、ずれた 4 通りのうち、代表（番号の並びがいちばん小さいもの）を返す処理
		//-------------------------------------------------------------------------------
		private static int[] Canonical(int a, int b, int c, int d)
		{
			int[][] phases = { new[] { a, b, c, d }, new[] { b, a, d, c }, new[] { c, d, a, b }, new[] { d, c, b, a } };
			int[] best = phases[0];
			foreach (int[] phase in phases)
			{
				for (int i = 0; i < 4; i++)
				{
					if (phase[i] != best[i])
					{
						if (phase[i] < best[i])
						{
							best = phase;
						}
						break;
					}
				}
			}
			return best;
		}

		//-------------------------------------------------------------------------------
		// くり返し 1 つぶんのパーツ（上端・くり返す単位・下端）を作る処理。edges が false なら、くり返す単位だけにする
		//-------------------------------------------------------------------------------
		private static MapAssistPart BuildForest(Stats stats, int[] unit, string name, MapAssistRole role, bool edges)
		{
			// 並びの行（左右が入れ替わったものも含む）と、その相手の行（同じ並びの、もう片方の行）
			// 1 行目と 2 行目が同じ並び（縦に同じブロックが続く壁など）のときは、相手は自分自身になる
			Dictionary<Pair, Pair> partner = new Dictionary<Pair, Pair>();
			partner[new Pair(unit[0], unit[1])] = new Pair(unit[2], unit[3]);
			partner[new Pair(unit[2], unit[3])] = new Pair(unit[0], unit[1]);
			Pair mirroredTop = new Pair(unit[1], unit[0]);
			Pair mirroredBottom = new Pair(unit[3], unit[2]);
			if (!partner.ContainsKey(mirroredTop))
			{
				partner[mirroredTop] = mirroredBottom;
			}
			if (!partner.ContainsKey(mirroredBottom))
			{
				partner[mirroredBottom] = mirroredTop;
			}
			Dictionary<Pair, int> topRows = new Dictionary<Pair, int>();
			Dictionary<Pair, Dictionary<Pair, int>> above = new Dictionary<Pair, Dictionary<Pair, int>>();
			Dictionary<Pair, Dictionary<Pair, int>> below = new Dictionary<Pair, Dictionary<Pair, int>>();
			for (int s = 0; s < stats.Samples.Count; s++)
			{
				MapAssistSample sample = stats.Samples[s];
				int[] g = stats.Grids[s];
				int w = sample.Width;
				for (int y = 0; y < sample.Height; y++)
				{
					for (int x = 0; x + 1 < w; x++)
					{
						Pair row = new Pair(g[y * w + x], g[y * w + x + 1]);
						Pair other;
						if (!partner.TryGetValue(row, out other))
						{
							continue;
						}
						// 左の端から数える（1 つ左が、この行の右のブロックなら、並びの途中なので数えない）。
						// マップの左端は、木が半分で切れていることが多く、どこから始まるか分からないので数えない
						if (x == 0 || g[y * w + x - 1] == row.Right)
						{
							continue;
						}
						// 上の端: 下が相手の行で、上が並びの行でない
						if (y > 0 && y + 1 < sample.Height && new Pair(g[(y + 1) * w + x], g[(y + 1) * w + x + 1]).Equals(other))
						{
							Pair up = new Pair(g[(y - 1) * w + x], g[(y - 1) * w + x + 1]);
							if (up.Left >= 0 && up.Right >= 0 && !partner.ContainsKey(up))
							{
								Bump(topRows, row);
								Bump(above, row, up);
							}
						}
						// 下の端: 上が相手の行で、下が並びの行でない
						if (y > 0 && y + 1 < sample.Height && new Pair(g[(y - 1) * w + x], g[(y - 1) * w + x + 1]).Equals(other))
						{
							Pair down = new Pair(g[(y + 1) * w + x], g[(y + 1) * w + x + 1]);
							if (down.Left >= 0 && down.Right >= 0 && !partner.ContainsKey(down))
							{
								Bump(below, row, down);
							}
						}
					}
				}
			}
			// 上の端にいちばん多く出る行を、くり返す単位の 1 行目にする（上の端が 1 つも無ければ、代表の並びのまま）
			Pair first = topRows.Count > 0 ? topRows.OrderByDescending(p => p.Value).First().Key : new Pair(unit[0], unit[1]);
			Pair second = partner[first];
			MapAssistPart part = new MapAssistPart { Name = nameOf(AutoPrefix) + name, Kind = MapAssistKind.Repeat, Role = role };
			MapAssistGrid body = new MapAssistGrid(2, 2);
			body[0, 0] = stats.Cell(first.Left);
			body[1, 0] = stats.Cell(first.Right);
			body[0, 1] = stats.Cell(second.Left);
			body[1, 1] = stats.Cell(second.Right);
			part.Slots[MapAssistPart.SlotBody] = body;
			if (!edges)
			{
				return part;
			}
			MapAssistGrid top = EdgeRow(stats, above, first);
			if (top != null)
			{
				part.Slots[MapAssistPart.SlotTop] = top;
			}
			// 下端は、くり返す単位の 2 行目のすぐ下に出る行
			MapAssistGrid bottom = EdgeRow(stats, below, second);
			if (bottom != null)
			{
				part.Slots[MapAssistPart.SlotBottom] = bottom;
			}
			return part;
		}

		//-------------------------------------------------------------------------------
		// 端の行の組（上端・下端）を選ぶ処理
		// その行の隣にいちばん多く出る組を使う。下地のブロックだけの組や、出る回数が少ない（3 割未満）組は使わない
		//-------------------------------------------------------------------------------
		private static MapAssistGrid EdgeRow(Stats stats, Dictionary<Pair, Dictionary<Pair, int>> neighbours, Pair row)
		{
			Dictionary<Pair, int> counts;
			if (!neighbours.TryGetValue(row, out counts) || counts.Count == 0)
			{
				return null;
			}
			int all = counts.Values.Sum();
			KeyValuePair<Pair, int> best = counts.OrderByDescending(p => p.Value).First();
			if (best.Value * 10 < all * 3 || (stats.Background[best.Key.Left] && stats.Background[best.Key.Right]))
			{
				return null;
			}
			MapAssistGrid grid = new MapAssistGrid(2, 1);
			grid[0, 0] = stats.Cell(best.Key.Left);
			grid[1, 0] = stats.Cell(best.Key.Right);
			return grid;
		}

		//-------------------------------------------------------------------------------
		// 数を 1 増やす処理（表に無ければ 1 から）
		//-------------------------------------------------------------------------------
		private static void Bump(Dictionary<Pair, int> counts, Pair key)
		{
			int seen;
			counts.TryGetValue(key, out seen);
			counts[key] = seen + 1;
		}

		//-------------------------------------------------------------------------------
		// 行ごとの表で、数を 1 増やす処理
		//-------------------------------------------------------------------------------
		private static void Bump(Dictionary<Pair, Dictionary<Pair, int>> counts, Pair row, Pair key)
		{
			Dictionary<Pair, int> inner;
			if (!counts.TryGetValue(row, out inner))
			{
				inner = new Dictionary<Pair, int>();
				counts[row] = inner;
			}
			Bump(inner, key);
		}

		// 部品の候補 1 つ（かたまりを囲む四角の並び。かたまりに入らないマスは -1）
		private sealed class Piece
		{
			public int Width;
			public int Height;
			public int[] Ids;
			public int[] Collisions;
			// 空きでないマスの数、同じ並びが出た回数、入口（ワープ）があるか
			public int Cells;
			public int Count;
			public bool HasDoor;
			// マップの上で、ひとつのかたまりとして実際に見つかったか（ほかのかたまりを分けた残りは false）
			public bool Standalone;
			public string Key;
		}

		//-------------------------------------------------------------------------------
		// マスの集まり（位置・ブロック・移動エリア）から、部品の候補を作る処理（囲む四角の左上を原点にする）
		//-------------------------------------------------------------------------------
		private static Piece MakePiece(Stats stats, List<int[]> cells, int count)
		{
			int left = cells.Min(c => c[0]), top = cells.Min(c => c[1]);
			Piece piece = new Piece { Width = cells.Max(c => c[0]) - left + 1, Height = cells.Max(c => c[1]) - top + 1, Cells = cells.Count, Count = count };
			piece.Ids = Enumerable.Repeat(-1, piece.Width * piece.Height).ToArray();
			piece.Collisions = new int[piece.Width * piece.Height];
			foreach (int[] cell in cells)
			{
				int i = (cell[1] - top) * piece.Width + cell[0] - left;
				piece.Ids[i] = cell[2];
				piece.Collisions[i] = cell[3];
				piece.HasDoor |= stats.Door[cell[2]];
			}
			piece.Key = piece.Width + "x" + piece.Height + ":" + string.Join(",", piece.Ids);
			return piece;
		}

		//-------------------------------------------------------------------------------
		// 部品にできる大きさ・形かを返す処理
		// 大きすぎる・すき間が多い（崖や壁の続き）ものは外す。幅か高さが 1 のものは、長さ 2〜4 で、ブロックがすべて違うものだけ
		// （同じブロックが続く柵や崖の切れ端を外し、木や看板のような決まった形を残す）
		//-------------------------------------------------------------------------------
		private static bool IsStampShape(Piece piece)
		{
			if (piece.Width > MaxStampSide || piece.Height > MaxStampSide || piece.Cells < 2 || piece.Cells * 10 < piece.Width * piece.Height * 6)
			{
				return false;
			}
			if (piece.Width == 1 || piece.Height == 1)
			{
				return piece.Cells <= 4 && piece.Ids.Distinct().Count() == piece.Ids.Length;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// かたまりの中に、別のかたまりと同じ並びがあるかを探す処理（あれば、その位置を返す）
		//-------------------------------------------------------------------------------
		private static bool FindInside(Piece outer, Piece inner, out int offsetX, out int offsetY)
		{
			offsetX = 0;
			offsetY = 0;
			if (inner.Width > outer.Width || inner.Height > outer.Height)
			{
				return false;
			}
			int anchor = Array.FindIndex(inner.Ids, id => id >= 0);
			int anchorX = anchor % inner.Width;
			int anchorY = anchor / inner.Width;
			for (int i = 0; i < outer.Ids.Length; i++)
			{
				if (outer.Ids[i] != inner.Ids[anchor])
				{
					continue;
				}
				int ox = i % outer.Width - anchorX;
				int oy = i / outer.Width - anchorY;
				if (ox < 0 || oy < 0 || ox + inner.Width > outer.Width || oy + inner.Height > outer.Height)
				{
					continue;
				}
				bool same = true;
				for (int k = 0; k < inner.Ids.Length && same; k++)
				{
					same = inner.Ids[k] < 0 || outer.Ids[(oy + k / inner.Width) * outer.Width + ox + k % inner.Width] == inner.Ids[k];
				}
				if (same)
				{
					offsetX = ox;
					offsetY = oy;
					return true;
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// かたまりから、含まれていた別のかたまりのマスを除き、残りを「つながったかたまり」ごとに分けて返す処理
		//-------------------------------------------------------------------------------
		private static List<Piece> Remainders(Stats stats, Piece outer, Piece inner, int offsetX, int offsetY)
		{
			int[] ids = (int[])outer.Ids.Clone();
			for (int k = 0; k < inner.Ids.Length; k++)
			{
				if (inner.Ids[k] >= 0)
				{
					ids[(offsetY + k / inner.Width) * outer.Width + offsetX + k % inner.Width] = -1;
				}
			}
			List<Piece> rest = new List<Piece>();
			bool[] visited = new bool[ids.Length];
			for (int start = 0; start < ids.Length; start++)
			{
				if (visited[start] || ids[start] < 0)
				{
					continue;
				}
				List<int[]> cells = new List<int[]>();
				Queue<int> queue = new Queue<int>();
				queue.Enqueue(start);
				visited[start] = true;
				while (queue.Count > 0)
				{
					int at = queue.Dequeue();
					int x = at % outer.Width;
					int y = at / outer.Width;
					cells.Add(new[] { x, y, ids[at], outer.Collisions[at] });
					foreach (int next in new[] { x > 0 ? at - 1 : -1, x + 1 < outer.Width ? at + 1 : -1, y > 0 ? at - outer.Width : -1, y + 1 < outer.Height ? at + outer.Width : -1 })
					{
						if (next >= 0 && !visited[next] && ids[next] >= 0)
						{
							visited[next] = true;
							queue.Enqueue(next);
						}
					}
				}
				rest.Add(MakePiece(stats, cells, outer.Count));
			}
			return rest;
		}

		//-------------------------------------------------------------------------------
		// 建物・飾りの候補（部品）と、1 マスだけの小物（面）を足す処理
		// 1. マップごとに、「通れない（またはワープの）ブロックで、下地・水・草むら・段差・森でないもの」がつながったかたまりを探す。
		//    マップの端にかかるもの・中身の分からないブロック（別のタイルセット2 のブロック）に接しているものは、
		//    途中で切れているかもしれないので使わない
		// 2. かたまりのすぐ上にある「通れるが下地ではないブロック」を 1 マスぶんだけ足す（屋根の上の段など、物の一部で通れるマスのため）
		// 3. かたまりの中に、別の（もっと小さい）かたまりと同じ並びがあれば、その分を除いて残りを別のかたまりにする。
		//    （ポケモンセンターの隣に木が立っている、建物が 2 つ並んでいる、といった所を、1 つずつに分けるため）
		// 4. 入口（ワープ）のあるものを建物、それ以外を飾りとして、同じ並びが出た回数の多い順（同じなら大きい順）に、上限まで採る。
		//    部品のマスのうち、かたまりに入らない所（四角のすみの地面など）は空きにする
		//-------------------------------------------------------------------------------
		private static void AddStamps(List<MapAssistPart> parts, Stats stats, HashSet<int> forestBlocks, bool[] taken)
		{
			Dictionary<string, Piece> found = new Dictionary<string, Piece>();
			Dictionary<int, int> singles = new Dictionary<int, int>();
			Func<int, bool> special = id => stats.Water[id] || stats.Grass[id] || stats.Ledge[id] || forestBlocks.Contains(id);
			Func<int, bool> isObject = id => id >= 0 && !taken[id] && !special(id) && !stats.Background[id] && (stats.Blocked(id) || stats.Door[id]);
			Func<int, bool> attachable = id => id >= 0 && !taken[id] && !special(id) && !stats.Background[id];
			Action<Dictionary<string, Piece>, Piece> merge = (table, piece) =>
			{
				Piece same;
				if (table.TryGetValue(piece.Key, out same))
				{
					same.Count += piece.Count;
					same.Standalone |= piece.Standalone;
				}
				else
				{
					table[piece.Key] = piece;
				}
			};
			for (int s = 0; s < stats.Samples.Count; s++)
			{
				MapAssistSample sample = stats.Samples[s];
				int[] g = stats.Grids[s];
				int w = sample.Width;
				int h = sample.Height;
				bool[] visited = new bool[g.Length];
				for (int start = 0; start < g.Length; start++)
				{
					if (visited[start] || !isObject(g[start]))
					{
						continue;
					}
					// つながったかたまりを集める
					List<int> cells = new List<int>();
					Queue<int> queue = new Queue<int>();
					queue.Enqueue(start);
					visited[start] = true;
					while (queue.Count > 0)
					{
						int at = queue.Dequeue();
						cells.Add(at);
						int x = at % w;
						int y = at / w;
						foreach (int next in new[] { x > 0 ? at - 1 : -1, x + 1 < w ? at + 1 : -1, y > 0 ? at - w : -1, y + 1 < h ? at + w : -1 })
						{
							if (next >= 0 && !visited[next] && isObject(g[next]))
							{
								visited[next] = true;
								queue.Enqueue(next);
							}
						}
					}
					// マップの端にかかるもの、大きすぎるものは使わない
					int left = cells.Min(i => i % w), right = cells.Max(i => i % w), top = cells.Min(i => i / w), bottom = cells.Max(i => i / w);
					if (left == 0 || top == 0 || right == w - 1 || bottom == h - 1 || right - left >= MaxStampSide * 2 || bottom - top >= MaxStampSide * 2)
					{
						continue;
					}
					// 囲む四角とその 1 マス外側に、中身の分からないブロックがあるものも使わない
					bool unknown = false;
					for (int y = top - 1; y <= bottom + 1 && !unknown; y++)
					{
						for (int x = left - 1; x <= right + 1 && !unknown; x++)
						{
							unknown = g[y * w + x] < 0;
						}
					}
					if (unknown)
					{
						continue;
					}
					if (cells.Count == 1)
					{
						int seen;
						singles.TryGetValue(g[start], out seen);
						singles[g[start]] = seen + 1;
						continue;
					}
					// すぐ上の「通れるが下地ではないマス」を 1 マスぶんだけ足す
					HashSet<int> member = new HashSet<int>(cells);
					foreach (int at in cells)
					{
						int up = at - w;
						if (up >= 0 && !visited[up] && attachable(g[up]) && !isObject(g[up]))
						{
							member.Add(up);
						}
					}
					Piece whole = MakePiece(stats, member.Select(i => new[] { i % w, i / w, g[i], sample.Collisions[i] }).ToList(), 1);
					whole.Standalone = true;
					merge(found, whole);
				}
			}
			// かたまりの端に付いている「1 マスだけで置かれることのあるブロック」（看板・木の端など）を外す
			Dictionary<string, Piece> trimmed = new Dictionary<string, Piece>();
			HashSet<int> loose = new HashSet<int>(singles.Where(p => p.Value >= 2).Select(p => p.Key));
			foreach (Piece piece in found.Values.OrderBy(p => p.Key, StringComparer.Ordinal))
			{
				Piece result = TrimLoose(stats, piece, loose, singles);
				if (result != null)
				{
					merge(trimmed, result);
				}
			}
			// 別のかたまりを含むものを分ける（分けた残りが、また別のかたまりを含む・含まれることがあるので、変わらなくなるまで数回くり返す）
			List<Piece> pieces = trimmed.Values.ToList();
			for (int round = 0; round < 6; round++)
			{
				bool changed = false;
				Dictionary<string, Piece> next = new Dictionary<string, Piece>();
				List<Piece> whole = new List<Piece>();
				foreach (Piece piece in pieces.OrderBy(p => p.Cells).ThenBy(p => p.Key, StringComparer.Ordinal))
				{
					Piece inner = null;
					int offsetX = 0;
					int offsetY = 0;
					// 大きいものから順に探す。目印にするのは、ひとつのかたまりとして実際に見つかり、入口があるか 2 回以上出た、部品にできる形のものだけ
					// （分けた残りを目印にすると、建物どうしで共通の壁のブロックなどで、建物が細かく切れてしまう）
					for (int k = whole.Count - 1; k >= 0 && inner == null; k--)
					{
						Piece other = whole[k];
						if (other.Standalone && other.Cells < piece.Cells && (other.HasDoor || other.Count >= 2) && IsStampShape(other) && FindInside(piece, other, out offsetX, out offsetY))
						{
							inner = other;
						}
					}
					if (inner == null)
					{
						merge(next, piece);
						whole.Add(piece);
						continue;
					}
					changed = true;
					inner.Count += piece.Count;
					foreach (Piece rest in Remainders(stats, piece, inner, offsetX, offsetY))
					{
						if (rest.Cells == 1)
						{
							int id = rest.Ids[0];
							int seen;
							singles.TryGetValue(id, out seen);
							singles[id] = seen + rest.Count;
						}
						else
						{
							merge(next, rest);
						}
					}
				}
				pieces = next.Values.ToList();
				if (!changed)
				{
					break;
				}
			}
			List<Piece> usable = pieces.Where(IsStampShape).ToList();
			int number = 1;
			foreach (Piece piece in usable.Where(p => p.HasDoor).OrderByDescending(p => p.Count).ThenByDescending(p => p.Cells).ThenBy(p => p.Key, StringComparer.Ordinal).Take(MaxBuildings))
			{
				AddStamp(parts, stats, piece, nameOf("建物") + " " + number++, MapAssistRole.Building);
			}
			number = 1;
			foreach (Piece piece in usable.Where(p => !p.HasDoor && p.Count >= 2 && IsOwnShape(stats, p)).OrderByDescending(p => p.Count).ThenByDescending(p => p.Cells).ThenBy(p => p.Key, StringComparer.Ordinal).Take(MaxDecorations))
			{
				AddStamp(parts, stats, piece, nameOf("飾り") + " " + number++, MapAssistRole.Decoration);
			}
			// 1 マスだけの物（岩・看板など）は、まとめて面にする
			AddArea(parts, stats, "小物", MapAssistRole.Decoration, singles.Where(p => !taken[p.Key]).OrderByDescending(p => p.Value).Select(p => p.Key).Take(24).ToList(), taken);
		}

		//-------------------------------------------------------------------------------
		// かたまりの端に付いている 1 マスの物を外す処理
		// 「1 マスだけで置かれることのあるブロック」で、かたまりの中で隣り合うマスが 1 つ以下のものを、無くなるまで外す。
		// 外した分は 1 マスの物として数える。残りが 1 マス以下なら null
		//-------------------------------------------------------------------------------
		private static Piece TrimLoose(Stats stats, Piece piece, HashSet<int> loose, Dictionary<int, int> singles)
		{
			if (!piece.Ids.Any(loose.Contains))
			{
				return piece;
			}
			int[] ids = (int[])piece.Ids.Clone();
			bool removed = true;
			bool any = false;
			while (removed)
			{
				removed = false;
				for (int i = 0; i < ids.Length; i++)
				{
					if (ids[i] < 0 || !loose.Contains(ids[i]))
					{
						continue;
					}
					int x = i % piece.Width;
					int y = i / piece.Width;
					int around = new[] { x > 0 ? i - 1 : -1, x + 1 < piece.Width ? i + 1 : -1, y > 0 ? i - piece.Width : -1, y + 1 < piece.Height ? i + piece.Width : -1 }.Count(n => n >= 0 && ids[n] >= 0);
					if (around <= 1)
					{
						int seen;
						singles.TryGetValue(ids[i], out seen);
						singles[ids[i]] = seen + piece.Count;
						ids[i] = -1;
						removed = true;
						any = true;
					}
				}
			}
			if (!any)
			{
				return piece;
			}
			List<int[]> cells = new List<int[]>();
			for (int i = 0; i < ids.Length; i++)
			{
				if (ids[i] >= 0)
				{
					cells.Add(new[] { i % piece.Width, i / piece.Width, ids[i], piece.Collisions[i] });
				}
			}
			if (cells.Count < 2)
			{
				return null;
			}
			Piece result = MakePiece(stats, cells, piece.Count);
			result.Standalone = piece.Standalone;
			return result;
		}

		//-------------------------------------------------------------------------------
		// かたまりが「その形で置かれる物」かを返す処理
		// どのブロックも、使われている回数の 3 割以上がこの並びの中であること
		// （崖や壁のブロックは、大きなかたまりの中でたくさん使われているので、その切れ端を飾りにしないため）
		//-------------------------------------------------------------------------------
		private static bool IsOwnShape(Stats stats, Piece piece)
		{
			foreach (IGrouping<int, int> block in piece.Ids.Where(id => id >= 0).GroupBy(id => id))
			{
				if ((long)piece.Count * block.Count() * 10 < (long)stats.Count[block.Key] * 3)
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// 部品の候補 1 つを、パーツにして足す処理
		//-------------------------------------------------------------------------------
		private static void AddStamp(List<MapAssistPart> parts, Stats stats, Piece piece, string name, MapAssistRole role)
		{
			MapAssistGrid grid = new MapAssistGrid(piece.Width, piece.Height);
			for (int i = 0; i < piece.Ids.Length; i++)
			{
				grid.Cells[i] = piece.Ids[i] < 0 ? MapAssistCell.Empty : stats.Context.ToCell(piece.Ids[i], piece.Collisions[i]);
			}
			MapAssistPart part = new MapAssistPart { Name = nameOf(AutoPrefix) + name, Kind = MapAssistKind.Stamp, Role = role };
			part.Slots[MapAssistPart.SlotBody] = grid;
			parts.Add(part);
		}
	}
}
