using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「ランダム作成」の設定（画面で決める。同じ種と設定なら同じマップになる）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistRandomOptions
	{
		// 乱数の種（0〜999999）
		public int Seed;
		// 水・森・草むらが範囲に占める割合（%）
		public int WaterPercent = 10;
		public int ForestPercent = 15;
		public int GrassPercent = 10;
		// 置く建物の数
		public int Buildings = 3;
		// 小物を置く割合（地面 100 マスあたりの数）
		public int DecorationPercent = 3;
		// 範囲のまわりを森で囲む
		public bool ForestBorder = true;
		// 建物の入口から道を引く
		public bool Paths = true;
	}

	//-------------------------------------------------------------------------------
	// 「ランダム作成」の中身（パーツの指定から、それらしいマップのたたきをゼロから作る）
	//
	//   1. 地面で範囲を埋める（くり返しの地面があればその模様、無ければ面の先頭のブロック）
	//   2. まわりを森で囲む（設定）→ 森のかたまり → 水のかたまり → 草むらのかたまり を、割合に合わせて置く
	//      （森と水は、縁や上端・下端が付けられるように、2×2 に満たない細い所を削る。
	//        水の縁つきに内側の角が無いときは、くぼみのできない楕円だけにする）
	//   3. 建物を、地面の上で重ならない所に置く（入口の下 1 行は空ける）
	//   4. 道（役割「道」の面）があれば、建物の入口から範囲の中心へ L 字に引く
	//   5. 小物（役割「飾り」の面・部品）を地面にまばらに置く
	//   6. 最後に「整える」（水の縁・移動エリア）
	//
	// 結果はたたきで、人が直す前提。範囲の外のマスは変えない
	//-------------------------------------------------------------------------------
	internal static class MapAssistRandom
	{
		// 水・森のかたまり 1 つの大きさ（マス数）の範囲
		private const int BlobMin = 10;
		private const int BlobMax = 60;
		// 建物を置く場所を探す回数の上限（1 軒あたり）
		private const int PlaceTries = 60;

		// 画面の言語にする処理
		private static Func<string, string> nameOf = text => text;

		// 範囲のマスの種類
		private enum Layer { Ground, Forest, Water, Grass, Building, Path, Decoration }

		//-------------------------------------------------------------------------------
		// マップのたたきを作る処理（作れなければ理由を返し、作れたら null。summary に案内文）
		//-------------------------------------------------------------------------------
		public static string Generate(MapAssistContext context, MapAssistWork work, IList<MapAssistPart> parts, Rectangle range, MapAssistRandomOptions options, int[] table, MapAssistAutoInput samples, MapAssistSupportReport report, Func<string, string> translate, out string summary)
		{
			summary = string.Empty;
			nameOf = translate ?? (text => text);
			range = Rectangle.Intersect(range, new Rectangle(0, 0, work.Width, work.Height));
			if (range.Width < 4 || range.Height < 4)
			{
				return nameOf("範囲が小さすぎます（4×4 以上にしてください）。");
			}
			// 使うパーツ（役割で選ぶ。同じ役割が複数あれば、一覧で先のもの）
			Func<MapAssistPart, bool> usableArea = p => p.Kind == MapAssistKind.Area && p.GetSlot(MapAssistPart.SlotBlocks) != null && p.GetSlot(MapAssistPart.SlotBlocks).Cells.Any(c => context.ToGlobal(c) >= 0);
			MapAssistPart groundRepeat = parts.FirstOrDefault(p => p.Kind == MapAssistKind.Repeat && p.Role == MapAssistRole.Ground && p.GetSlot(MapAssistPart.SlotBody) != null);
			MapAssistPart groundArea = parts.FirstOrDefault(p => p.Role == MapAssistRole.Ground && usableArea(p));
			if (groundRepeat == null && groundArea == null)
			{
				return nameOf("役割が「地面」の面（またはくり返し）がありません。先にパーツを指定するか、「マップから候補を作る（自動）」を押してください。");
			}
			MapAssistPart path = parts.FirstOrDefault(p => p.Role == MapAssistRole.Path && usableArea(p));
			MapAssistPart grass = parts.FirstOrDefault(p => p.Role == MapAssistRole.Grass && usableArea(p));
			MapAssistPart water = parts.Where(p => p.Kind == MapAssistKind.Edge && p.Role == MapAssistRole.Water && p.GetSlot("C") != null && context.ToGlobal(p.GetSlot("C").Cells[0]) >= 0)
				.OrderByDescending(p => p.Slots.Count).FirstOrDefault();
			MapAssistPart forest = parts.Where(p => p.Kind == MapAssistKind.Repeat && p.Role == MapAssistRole.Tree && p.GetSlot(MapAssistPart.SlotBody) != null)
				.OrderByDescending(p => p.GetSlot(MapAssistPart.SlotTop) != null ? 1 : 0).FirstOrDefault();
			List<MapAssistPart> buildings = parts.Where(p => p.Kind == MapAssistKind.Stamp && p.Role == MapAssistRole.Building && p.GetSlot(MapAssistPart.SlotBody) != null).ToList();
			List<MapAssistPart> decoStamps = parts.Where(p => p.Kind == MapAssistKind.Stamp && p.Role == MapAssistRole.Decoration && p.GetSlot(MapAssistPart.SlotBody) != null).ToList();
			List<MapAssistCell> decoCells = parts.Where(p => p.Role == MapAssistRole.Decoration && usableArea(p)).SelectMany(p => p.GetSlot(MapAssistPart.SlotBlocks).Cells).Where(c => context.ToGlobal(c) >= 0).ToList();

			Random rng = new Random(options.Seed);
			int w = range.Width;
			int h = range.Height;
			Layer[] layer = new Layer[w * h];
			// 置いた部品（建物・小物）のマス: 範囲の中の位置 → 置くマス
			Dictionary<int, MapAssistCell> stamped = new Dictionary<int, MapAssistCell>();
			List<Point> entrances = new List<Point>();
			List<string> skipped = new List<string>();

			// まわりの囲み（森）
			if (forest != null && options.ForestBorder)
			{
				AddBorder(rng, layer, w, h, Layer.Forest);
			}
			if (forest == null && (options.ForestPercent > 0 || options.ForestBorder))
			{
				skipped.Add(nameOf("森（役割「木・森」のくり返し）"));
			}
			// 建物（広い地面が要るので、かたまりより先に置く）
			int placed = 0;
			if (buildings.Count > 0)
			{
				for (int n = 0; n < options.Buildings; n++)
				{
					MapAssistPart building = buildings[rng.Next(buildings.Count)];
					Point? at = PlaceStamp(rng, context, layer, w, h, building.GetSlot(MapAssistPart.SlotBody), stamped, Layer.Building, true);
					if (at != null)
					{
						placed++;
						MapAssistGrid body = building.GetSlot(MapAssistPart.SlotBody);
						entrances.Add(new Point(at.Value.X + body.Width / 2, at.Value.Y + body.Height));
					}
				}
			}
			else if (options.Buildings > 0)
			{
				skipped.Add(nameOf("建物（役割「建物」の部品）"));
			}
			// 道（かたまりは道を避けて広がる）
			int pathCells = 0;
			if (options.Paths && entrances.Count > 0)
			{
				if (path != null)
				{
					pathCells = AddPaths(rng, layer, w, h, entrances);
				}
				else
				{
					skipped.Add(nameOf("道（役割「道」の面）"));
				}
			}
			// 水
			if (water != null)
			{
				bool inner = MapAssistPart.EdgeInnerSlots.All(s => water.GetSlot(s) != null && context.ToGlobal(water.GetSlot(s).Cells[0]) >= 0);
				int target = w * h * options.WaterPercent / 100;
				if (inner)
				{
					AddBlobs(rng, layer, w, h, Layer.Water, target, true);
				}
				else
				{
					AddEllipses(rng, layer, w, h, Layer.Water, target);
				}
			}
			else if (options.WaterPercent > 0)
			{
				skipped.Add(nameOf("水（役割「水」の縁つき）"));
			}
			// 森のかたまり
			if (forest != null)
			{
				AddBlobs(rng, layer, w, h, Layer.Forest, w * h * options.ForestPercent / 100, true);
			}
			// 草むら
			if (grass != null)
			{
				AddBlobs(rng, layer, w, h, Layer.Grass, w * h * options.GrassPercent / 100, false);
			}
			else if (options.GrassPercent > 0)
			{
				skipped.Add(nameOf("草むら（役割「草むら」の面）"));
			}
			// 小物
			int decorations = 0;
			if (decoCells.Count > 0 || decoStamps.Count > 0)
			{
				decorations = AddDecorations(rng, context, layer, w, h, decoCells, decoStamps, stamped, options.DecorationPercent);
			}
			else if (options.DecorationPercent > 0)
			{
				skipped.Add(nameOf("小物（役割「飾り」の面・部品）"));
			}

			// 写しへ書く
			MapAssistGrid groundBody = groundRepeat != null ? groundRepeat.GetSlot(MapAssistPart.SlotBody) : null;
			MapAssistCell groundCell = groundArea != null ? groundArea.GetSlot(MapAssistPart.SlotBlocks).Cells.First(c => context.ToGlobal(c) >= 0) : MapAssistCell.Empty;
			MapAssistCell grassCell = grass != null ? grass.GetSlot(MapAssistPart.SlotBlocks).Cells.First(c => context.ToGlobal(c) >= 0) : MapAssistCell.Empty;
			MapAssistCell pathCell = path != null ? path.GetSlot(MapAssistPart.SlotBlocks).Cells.First(c => context.ToGlobal(c) >= 0) : MapAssistCell.Empty;
			MapAssistCell waterCell = water != null ? water.GetSlot("C").Cells[0] : MapAssistCell.Empty;
			MapAssistGrid forestBody = forest != null ? forest.GetSlot(MapAssistPart.SlotBody) : null;
			MapAssistGrid forestTop = forest != null ? MapAssistSupport.SameWidth(forest.GetSlot(MapAssistPart.SlotTop), forestBody) : null;
			MapAssistGrid forestBottom = forest != null ? MapAssistSupport.SameWidth(forest.GetSlot(MapAssistPart.SlotBottom), forestBody) : null;
			int counted = 0;
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					int i = y * w + x;
					int mx = range.X + x;
					int my = range.Y + y;
					MapAssistCell cell;
					switch (layer[i])
					{
						case Layer.Forest:
							cell = MapAssistSupport.RepeatCell(forestBody, forestTop, forestBottom, mx, my, 0, 0, RunLength(layer, w, h, x, y, 0, -1, Layer.Forest, forestTop != null ? forestTop.Height : 0), RunLength(layer, w, h, x, y, 0, 1, Layer.Forest, forestBottom != null ? forestBottom.Height : 0));
							break;
						case Layer.Water:
							cell = waterCell;
							break;
						case Layer.Grass:
							cell = grassCell;
							break;
						case Layer.Path:
							cell = pathCell;
							break;
						case Layer.Building:
						case Layer.Decoration:
							if (!stamped.TryGetValue(i, out cell))
							{
								cell = GroundAt(groundBody, groundCell, mx, my);
							}
							break;
						default:
							cell = GroundAt(groundBody, groundCell, mx, my);
							break;
					}
					int id = context.ToGlobal(cell);
					if (id < 0)
					{
						continue;
					}
					int at = my * work.Width + mx;
					work.Blocks[at] = id;
					int collision = cell.Collision >= 0 ? cell.Collision : table[id];
					if (collision >= 0)
					{
						work.Collisions[at] = collision;
					}
					counted++;
				}
			}
			report.Painted += counted;
			// 水の縁と移動エリアを整える（くり返しは、置いた森がそのまま正しいので整えない）
			MapAssistSupport.Tidy(context, work, parts, range, water != null, false, true, table, samples, report);
			int waterCount = layer.Count(l => l == Layer.Water);
			int forestCount = layer.Count(l => l == Layer.Forest);
			int grassCount = layer.Count(l => l == Layer.Grass);
			summary = string.Format(nameOf("ランダム作成（種 {0}）: 森 {1} マス、水 {2} マス、草むら {3} マス、建物 {4} 軒、道 {5} マス、小物 {6} 個。"),
				options.Seed, forestCount, waterCount, grassCount, placed, pathCells, decorations);
			if (placed < options.Buildings && buildings.Count > 0)
			{
				summary += string.Format(nameOf("建物は {0} 軒のうち {1} 軒しか置けませんでした（地面の広い所が足りません）。"), options.Buildings, placed);
			}
			if (skipped.Count > 0)
			{
				summary += string.Format(nameOf("パーツが無いため省いたもの: {0}。"), string.Join(nameOf("、"), skipped));
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 地面のマスを返す処理（くり返しの地面なら、マップの座標で決まる模様の位置。無ければ面の先頭）
		//-------------------------------------------------------------------------------
		private static MapAssistCell GroundAt(MapAssistGrid body, MapAssistCell fallback, int mx, int my)
		{
			return body != null ? body[mx % body.Width, my % body.Height] : fallback;
		}

		//-------------------------------------------------------------------------------
		// 同じ種類のマスが、その向きに何マス続くかを数える処理（limit まで。範囲の端に着いたら続いているとみなす）
		//-------------------------------------------------------------------------------
		private static int RunLength(Layer[] layer, int w, int h, int x, int y, int dx, int dy, Layer kind, int limit)
		{
			for (int d = 0; d < limit; d++)
			{
				int nx = x + dx * (d + 1);
				int ny = y + dy * (d + 1);
				if (nx < 0 || ny < 0 || nx >= w || ny >= h)
				{
					return limit;
				}
				if (layer[ny * w + nx] != kind)
				{
					return d;
				}
			}
			return limit;
		}

		//-------------------------------------------------------------------------------
		// 範囲のまわりを 2〜3 マスの帯で囲む処理（ところどころ内側へふくらませて、自然に見せる）
		//-------------------------------------------------------------------------------
		private static void AddBorder(Random rng, Layer[] layer, int w, int h, Layer kind)
		{
			// 小さいマップ（短い辺が 30 マス以下）は 2 マス、それより大きければ 3 マス
			int thickness = Math.Min(w, h) <= 30 ? 2 : 3;
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					int edge = Math.Min(Math.Min(x, w - 1 - x), Math.Min(y, h - 1 - y));
					if (edge < thickness || (edge == thickness && rng.Next(4) == 0))
					{
						layer[y * w + x] = kind;
					}
				}
			}
			// ふくらませた所は 1 マスの出っ張りになりやすいので、2×2 に満たない所を削る（囲みの帯は残る）
			Thin(layer, w, h, kind);
		}

		//-------------------------------------------------------------------------------
		// 地面の上に、不定形のかたまりを目標のマス数になるまで置く処理
		// thick なら、2×2 に満たない細い所を削る（縁・上端・下端を付けるため）
		//-------------------------------------------------------------------------------
		private static void AddBlobs(Random rng, Layer[] layer, int w, int h, Layer kind, int target, bool thick)
		{
			int placed = 0;
			int tries = 0;
			while (placed < target && tries < 200)
			{
				tries++;
				int size = Math.Min(target - placed + BlobMin, rng.Next(BlobMin, BlobMax + 1));
				List<int> cells = GrowBlob(rng, layer, w, h, size);
				if (cells.Count == 0)
				{
					continue;
				}
				if (thick)
				{
					cells = ThickCells(cells, w, h);
					if (cells.Count < 4)
					{
						continue;
					}
				}
				// 縁を付けるかたまり（森・水）は、ほかの種類とくっつくと縁が決めにくいので 1 マスあけて置く（草むらはくっついてよい）
				if (thick && cells.Any(i => Touches(layer, w, h, i, kind)))
				{
					continue;
				}
				foreach (int i in cells)
				{
					layer[i] = kind;
				}
				placed += cells.Count;
			}
		}

		//-------------------------------------------------------------------------------
		// 地面の 1 マスから、隣へ気ままに広がるかたまりを作る処理（地面でないマスには広がらない）
		//-------------------------------------------------------------------------------
		private static List<int> GrowBlob(Random rng, Layer[] layer, int w, int h, int size)
		{
			List<int> result = new List<int>();
			int start = -1;
			for (int t = 0; t < 30 && start < 0; t++)
			{
				int i = rng.Next(w * h);
				if (layer[i] == Layer.Ground)
				{
					start = i;
				}
			}
			if (start < 0)
			{
				return result;
			}
			HashSet<int> inside = new HashSet<int> { start };
			List<int> frontier = new List<int> { start };
			result.Add(start);
			while (result.Count < size && frontier.Count > 0)
			{
				int pick = rng.Next(frontier.Count);
				int at = frontier[pick];
				int x = at % w;
				int y = at / w;
				List<int> next = new List<int>();
				foreach (Point d in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) })
				{
					int nx = x + d.X;
					int ny = y + d.Y;
					if (nx >= 0 && ny >= 0 && nx < w && ny < h && layer[ny * w + nx] == Layer.Ground && !inside.Contains(ny * w + nx))
					{
						next.Add(ny * w + nx);
					}
				}
				if (next.Count == 0)
				{
					frontier.RemoveAt(pick);
					continue;
				}
				int chosen = next[rng.Next(next.Count)];
				inside.Add(chosen);
				result.Add(chosen);
				frontier.Add(chosen);
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// かたまりのうち、2×2 の四角に入るマスだけを残す処理（1 マス幅の出っ張りや線を落とす）
		//-------------------------------------------------------------------------------
		private static List<int> ThickCells(List<int> cells, int w, int h)
		{
			HashSet<int> set = new HashSet<int>(cells);
			HashSet<int> keep = new HashSet<int>();
			foreach (int i in cells)
			{
				int x = i % w;
				int y = i / w;
				// このマスを左上とする 2×2 がすべて入っていれば、4 マスとも残す
				if (x + 1 < w && y + 1 < h && set.Contains(i + 1) && set.Contains(i + w) && set.Contains(i + w + 1))
				{
					keep.Add(i);
					keep.Add(i + 1);
					keep.Add(i + w);
					keep.Add(i + w + 1);
				}
			}
			return keep.ToList();
		}

		//-------------------------------------------------------------------------------
		// かたまりを 2×2 に満たない所から削って、残りを置き直す処理（囲みの帯で使う）
		//-------------------------------------------------------------------------------
		private static void Thin(Layer[] layer, int w, int h, Layer kind)
		{
			List<int> cells = Enumerable.Range(0, w * h).Where(i => layer[i] == kind).ToList();
			HashSet<int> keep = new HashSet<int>(ThickCells(cells, w, h));
			foreach (int i in cells)
			{
				if (!keep.Contains(i))
				{
					layer[i] = Layer.Ground;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// そのマスの上下左右・斜めに、地面でも自分と同じ種類でもないマスがあるかを返す処理（別の種類とくっつけないため）
		//-------------------------------------------------------------------------------
		private static bool Touches(Layer[] layer, int w, int h, int i, Layer kind)
		{
			int x = i % w;
			int y = i / w;
			for (int dy = -1; dy <= 1; dy++)
			{
				for (int dx = -1; dx <= 1; dx++)
				{
					int nx = x + dx;
					int ny = y + dy;
					if (nx < 0 || ny < 0 || nx >= w || ny >= h)
					{
						continue;
					}
					Layer other = layer[ny * w + nx];
					if (other != Layer.Ground && other != kind)
					{
						return true;
					}
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 楕円（くぼみの無い形）を目標のマス数になるまで置く処理（内側の角が無い縁つきの水に使う）
		//-------------------------------------------------------------------------------
		private static void AddEllipses(Random rng, Layer[] layer, int w, int h, Layer kind, int target)
		{
			int placed = 0;
			for (int tries = 0; placed < target && tries < 200; tries++)
			{
				int rx = rng.Next(2, Math.Max(3, Math.Min(7, w / 4)));
				int ry = rng.Next(2, Math.Max(3, Math.Min(6, h / 4)));
				int cx = rng.Next(rx, Math.Max(rx + 1, w - rx));
				int cy = rng.Next(ry, Math.Max(ry + 1, h - ry));
				List<int> cells = new List<int>();
				bool ok = true;
				for (int y = cy - ry; y <= cy + ry && ok; y++)
				{
					for (int x = cx - rx; x <= cx + rx; x++)
					{
						double ex = (x - cx + 0.5) / (rx + 0.5);
						double ey = (y - cy + 0.5) / (ry + 0.5);
						if (ex * ex + ey * ey > 1.0)
						{
							continue;
						}
						if (x < 0 || y < 0 || x >= w || y >= h || layer[y * w + x] != Layer.Ground)
						{
							ok = false;
							break;
						}
						cells.Add(y * w + x);
					}
				}
				if (!ok || cells.Count < 4 || cells.Any(i => Touches(layer, w, h, i, kind)))
				{
					continue;
				}
				foreach (int i in ThickCells(cells, w, h))
				{
					layer[i] = kind;
					placed++;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 部品を、地面の上で重ならない所に置く処理（置けたら左上の位置。置けなければ null）
		// clearBelow なら、入口の下 1 行も地面であることを求める（建物の前に立てるように）
		//-------------------------------------------------------------------------------
		private static Point? PlaceStamp(Random rng, MapAssistContext context, Layer[] layer, int w, int h, MapAssistGrid body, Dictionary<int, MapAssistCell> stamped, Layer kind, bool clearBelow)
		{
			int margin = 1;
			if (body.Width + margin * 2 > w || body.Height + margin * 2 + (clearBelow ? 1 : 0) > h)
			{
				return null;
			}
			for (int t = 0; t < PlaceTries; t++)
			{
				int x0 = rng.Next(margin, w - body.Width - margin + 1);
				int y0 = rng.Next(margin, h - body.Height - margin - (clearBelow ? 1 : 0) + 1);
				bool ok = true;
				for (int y = y0 - margin; y < y0 + body.Height + margin + (clearBelow ? 1 : 0) && ok; y++)
				{
					for (int x = x0 - margin; x < x0 + body.Width + margin; x++)
					{
						if (x < 0 || y < 0 || x >= w || y >= h || layer[y * w + x] != Layer.Ground)
						{
							ok = false;
							break;
						}
					}
				}
				if (!ok)
				{
					continue;
				}
				for (int y = 0; y < body.Height; y++)
				{
					for (int x = 0; x < body.Width; x++)
					{
						MapAssistCell cell = body[x, y];
						int i = (y0 + y) * w + x0 + x;
						// 部品の空きのマスは地面のまま（ただし、ほかの物が重ならないように印は付ける）
						layer[i] = kind;
						if (context.ToGlobal(cell) >= 0)
						{
							stamped[i] = cell;
						}
					}
				}
				return new Point(x0, y0);
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 建物の入口から、範囲の中心へ L 字の道を引く処理（地面のマスだけを道にする。置いた道のマス数を返す）
		// 入口の下へ 1 マス出てから、先に横、次に縦へ進む（建物のそばを通り過ぎないように、縦は入口の列から離れてから）
		//-------------------------------------------------------------------------------
		private static int AddPaths(Random rng, Layer[] layer, int w, int h, List<Point> entrances)
		{
			int count = 0;
			Point hub = new Point(w / 2, h / 2);
			// 中心が地面でなければ、地面の所まで近くを探す
			if (layer[hub.Y * w + hub.X] != Layer.Ground)
			{
				int best = -1;
				int bestDistance = int.MaxValue;
				for (int i = 0; i < layer.Length; i++)
				{
					if (layer[i] != Layer.Ground)
					{
						continue;
					}
					int d = Math.Abs(i % w - hub.X) + Math.Abs(i / w - hub.Y);
					if (d < bestDistance)
					{
						bestDistance = d;
						best = i;
					}
				}
				if (best < 0)
				{
					return 0;
				}
				hub = new Point(best % w, best / w);
			}
			foreach (Point entrance in entrances)
			{
				Point at = entrance;
				if (at.Y >= h)
				{
					continue;
				}
				count += PathCell(layer, w, h, at.X, at.Y);
				// 横
				int stepX = Math.Sign(hub.X - at.X);
				while (at.X != hub.X)
				{
					at.X += stepX;
					count += PathCell(layer, w, h, at.X, at.Y);
				}
				// 縦
				int stepY = Math.Sign(hub.Y - at.Y);
				while (at.Y != hub.Y)
				{
					at.Y += stepY;
					count += PathCell(layer, w, h, at.X, at.Y);
				}
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 1 マスを道にする処理（地面のときだけ。道にしたら 1、しなければ 0）
		//-------------------------------------------------------------------------------
		private static int PathCell(Layer[] layer, int w, int h, int x, int y)
		{
			if (x < 0 || y < 0 || x >= w || y >= h || layer[y * w + x] != Layer.Ground)
			{
				return 0;
			}
			layer[y * w + x] = Layer.Path;
			return 1;
		}

		//-------------------------------------------------------------------------------
		// 小物を地面にまばらに置く処理（置いた数を返す）
		// 地面 100 マスあたり percent 個。6 回に 1 回は部品の飾り（置ける所があれば）、それ以外は 1 マスの飾り
		//-------------------------------------------------------------------------------
		private static int AddDecorations(Random rng, MapAssistContext context, Layer[] layer, int w, int h, List<MapAssistCell> cells, List<MapAssistPart> stamps, Dictionary<int, MapAssistCell> stamped, int percent)
		{
			int ground = layer.Count(l => l == Layer.Ground);
			int target = ground * percent / 100;
			int placed = 0;
			for (int t = 0; t < target * 4 && placed < target; t++)
			{
				if (stamps.Count > 0 && (cells.Count == 0 || rng.Next(6) == 0))
				{
					MapAssistPart stamp = stamps[rng.Next(stamps.Count)];
					if (PlaceStamp(rng, context, layer, w, h, stamp.GetSlot(MapAssistPart.SlotBody), stamped, Layer.Decoration, false) != null)
					{
						placed++;
					}
					continue;
				}
				int i = rng.Next(w * h);
				// 道や建物のそばは避ける（上下左右が地面のときだけ）
				if (layer[i] != Layer.Ground || Touches(layer, w, h, i, Layer.Ground))
				{
					continue;
				}
				layer[i] = Layer.Decoration;
				stamped[i] = cells[rng.Next(cells.Count)];
				placed++;
			}
			return placed;
		}
	}
}
