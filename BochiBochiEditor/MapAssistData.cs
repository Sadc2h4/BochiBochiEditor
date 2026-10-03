using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップ作成補助」で使う、タイルセットごとの指定（どのブロックが何のパーツか）の入れ物と、保存・読み込み
	//
	// 指定の種類（MapAssistKind）
	//   面       : 地面・草むらなど、同じ役割で置き換えられるブロックの集まり
	//   縁つき   : 水・道・崖など、周りとの境目に専用のブロックがある地形（中央・4 辺・外側の角 4 つ・内側の角 4 つ）
	//   部品     : 家・大きい木など、決まった並びをそのまま使うもの（移動エリアも一緒に持つ）
	//   くり返し : 森のように、同じ並びをくり返して広げるもの（上端・くり返す単位・下端）
	//
	// 保存先は resource\mapassist の下で、タイルセット 1 つにつき 1 ファイル。
	// タイルセットは「ブロックのデータから作った識別子」で見分けるので、同じタイルセットを別の ROM へ移しても同じ指定を使える。
	// 識別子が合わないとき（ブロックを編集した後など）は、ゲームと見出しの場所が同じファイルを使う。
	//-------------------------------------------------------------------------------

	// パーツの役割（自動でマップを作るときに、何として扱うか）
	internal enum MapAssistRole
	{
		Ground,
		Grass,
		Path,
		Water,
		Tree,
		Cliff,
		Fence,
		Building,
		Decoration,
		Other,
	}

	// 指定の種類
	internal enum MapAssistKind
	{
		Area,
		Edge,
		Stamp,
		Repeat,
	}

	//-------------------------------------------------------------------------------
	// 並びの 1 マス（どのタイルセットの何番のブロックか、移動エリアの値）
	//-------------------------------------------------------------------------------
	internal struct MapAssistCell
	{
		// 0 = 空き、1 = タイルセット1、2 = タイルセット2
		public int Tileset;
		// タイルセットの中での番号（タイルセット2 は、タイルセット2 の先頭を 0 とした番号）
		public int Block;
		// 移動エリアの値（-1 = 決めていない）
		public int Collision;

		public bool IsEmpty
		{
			get { return this.Tileset == 0; }
		}

		//-------------------------------------------------------------------------------
		// 空きのマスを返す処理
		//-------------------------------------------------------------------------------
		public static MapAssistCell Empty
		{
			get { return new MapAssistCell { Tileset = 0, Block = 0, Collision = -1 }; }
		}

		//-------------------------------------------------------------------------------
		// ファイルに書く文字にする処理（例: 1:00D:0C。移動エリアを決めていなければ 1:00D:--、空きは -）
		//-------------------------------------------------------------------------------
		public string ToText()
		{
			if (this.IsEmpty)
			{
				return "-";
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1:X3}:{2}", this.Tileset, this.Block, this.Collision < 0 ? "--" : this.Collision.ToString("X2", CultureInfo.InvariantCulture));
		}

		//-------------------------------------------------------------------------------
		// ファイルの文字から読む処理（読めなければ false）
		//-------------------------------------------------------------------------------
		public static bool TryParse(string text, out MapAssistCell cell)
		{
			cell = Empty;
			text = (text ?? string.Empty).Trim();
			if (text == "-")
			{
				return true;
			}
			string[] fields = text.Split(':');
			int tileset;
			int block;
			if (fields.Length != 3 || !int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out tileset) || (tileset != 1 && tileset != 2)
				|| !int.TryParse(fields[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out block) || block < 0 || block > 1023)
			{
				return false;
			}
			int collision = -1;
			if (fields[2] != "--" && (!int.TryParse(fields[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out collision) || collision < 0 || collision > 63))
			{
				return false;
			}
			cell = new MapAssistCell { Tileset = tileset, Block = block, Collision = collision };
			return true;
		}
	}

	//-------------------------------------------------------------------------------
	// マスの並び（幅 × 高さ）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistGrid
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public MapAssistCell[] Cells { get; private set; }

		//-------------------------------------------------------------------------------
		// 空きのマスで埋めた並びを作る処理
		//-------------------------------------------------------------------------------
		public MapAssistGrid(int width, int height)
		{
			this.Width = Math.Max(1, width);
			this.Height = Math.Max(1, height);
			this.Cells = new MapAssistCell[this.Width * this.Height];
			for (int i = 0; i < this.Cells.Length; i++)
			{
				this.Cells[i] = MapAssistCell.Empty;
			}
		}

		public MapAssistCell this[int x, int y]
		{
			get { return this.Cells[y * this.Width + x]; }
			set { this.Cells[y * this.Width + x] = value; }
		}

		// 空きでないマスが 1 つも無いか
		public bool IsBlank
		{
			get { return this.Cells.All(c => c.IsEmpty); }
		}

		//-------------------------------------------------------------------------------
		// ファイルに書く文字にする処理（例: 2,2;1:00D:0C,1:00E:0C,-,2:004:01）
		//-------------------------------------------------------------------------------
		public string ToText()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0},{1};{2}", this.Width, this.Height, string.Join(",", this.Cells.Select(c => c.ToText())));
		}

		//-------------------------------------------------------------------------------
		// ファイルの文字から読む処理（読めなければ false）
		//-------------------------------------------------------------------------------
		public static bool TryParse(string text, out MapAssistGrid grid)
		{
			grid = null;
			string[] halves = (text ?? string.Empty).Split(';');
			if (halves.Length != 2)
			{
				return false;
			}
			string[] size = halves[0].Split(',');
			int width;
			int height;
			if (size.Length != 2 || !int.TryParse(size[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out width) || !int.TryParse(size[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out height)
				|| width < 1 || height < 1 || width > 255 || height > 255)
			{
				return false;
			}
			string[] cells = halves[1].Split(',');
			if (cells.Length != width * height)
			{
				return false;
			}
			MapAssistGrid result = new MapAssistGrid(width, height);
			for (int i = 0; i < cells.Length; i++)
			{
				MapAssistCell cell;
				if (!MapAssistCell.TryParse(cells[i], out cell))
				{
					return false;
				}
				result.Cells[i] = cell;
			}
			grid = result;
			return true;
		}
	}

	//-------------------------------------------------------------------------------
	// パーツ 1 つ（名前・種類・役割と、枠ごとの並び）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistPart
	{
		// 枠の名前
		public const string SlotBlocks = "Blocks";   // 面: 属するブロックを横に並べたもの
		public const string SlotBody = "Body";       // 部品・くり返し: 本体
		public const string SlotTop = "Top";         // くり返し: 上端
		public const string SlotBottom = "Bottom";   // くり返し: 下端
		// 縁つき: 中央、辺（上下左右）、外側の角、内側の角（どちらも、外側がある向きで呼ぶ）
		public static readonly string[] EdgeOuterSlots = { "NW", "N", "NE", "W", "C", "E", "SW", "S", "SE" };
		public static readonly string[] EdgeInnerSlots = { "INW", "INE", "ISW", "ISE" };
		// 縁つき: 内側の縁（外側の縁のもう 1 マス内側に付く 2 周目。山のように縁が二重のもの用）。画面では編集しない
		public static readonly string[] EdgeRing2Slots = { "2NW", "2N", "2NE", "2W", "2E", "2SW", "2S", "2SE", "2INW", "2INE", "2ISW", "2ISE" };
		// くり返し: 重なって並ぶ木の、森の端の絵（MapAssistTrees.cs）。画面では編集しない
		public static readonly string[] RepeatExtraSlots = { "BodyEdge", "BottomEdge", "BodyTip" };

		public string Name = string.Empty;
		public MapAssistKind Kind;
		public MapAssistRole Role;
		// 画面の一覧での並び順（2 つのファイルに分けて保存しても、読み込んだときに同じ順に戻すため）
		public int Order;
		public readonly Dictionary<string, MapAssistGrid> Slots = new Dictionary<string, MapAssistGrid>();

		// タイルセット1 とタイルセット2 の両方のブロックを使うパーツが、どのタイルセット1 と組で使うものか（タイルセット2 のファイルに書く）
		public string PrimaryFingerprint = string.Empty;
		public string PrimaryGame = string.Empty;
		public long PrimaryOffset = -1;

		//-------------------------------------------------------------------------------
		// その種類が持つ枠の名前を、画面に出す順で返す処理
		//-------------------------------------------------------------------------------
		public static string[] SlotNamesOf(MapAssistKind kind)
		{
			switch (kind)
			{
				case MapAssistKind.Area:
					return new[] { SlotBlocks };
				case MapAssistKind.Edge:
					return EdgeOuterSlots.Concat(EdgeInnerSlots).ToArray();
				case MapAssistKind.Stamp:
					return new[] { SlotBody };
				default:
					return new[] { SlotTop, SlotBody, SlotBottom };
			}
		}

		//-------------------------------------------------------------------------------
		// その種類が持つ枠の名前を、画面に出さないものも含めて返す処理（保存・ブロックの集計に使う）
		//-------------------------------------------------------------------------------
		public static string[] AllSlotNamesOf(MapAssistKind kind)
		{
			switch (kind)
			{
				case MapAssistKind.Edge:
					return SlotNamesOf(kind).Concat(EdgeRing2Slots).ToArray();
				case MapAssistKind.Repeat:
					return SlotNamesOf(kind).Concat(RepeatExtraSlots).ToArray();
				default:
					return SlotNamesOf(kind);
			}
		}

		//-------------------------------------------------------------------------------
		// 枠の並びを返す処理（入っていなければ null）
		//-------------------------------------------------------------------------------
		public MapAssistGrid GetSlot(string name)
		{
			MapAssistGrid grid;
			return this.Slots.TryGetValue(name, out grid) ? grid : null;
		}

		//-------------------------------------------------------------------------------
		// すべての枠の、空きでないマスを返す処理
		//-------------------------------------------------------------------------------
		public IEnumerable<MapAssistCell> AllCells()
		{
			foreach (string name in AllSlotNamesOf(this.Kind))
			{
				MapAssistGrid grid = this.GetSlot(name);
				if (grid == null)
				{
					continue;
				}
				foreach (MapAssistCell cell in grid.Cells)
				{
					if (!cell.IsEmpty)
					{
						yield return cell;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// そのタイルセット（1 か 2）のブロックを使っているかを返す処理
		//-------------------------------------------------------------------------------
		public bool UsesTileset(int tileset)
		{
			return this.AllCells().Any(c => c.Tileset == tileset);
		}
	}

	//-------------------------------------------------------------------------------
	// タイルセット 1 つの見分け方（ゲーム・見出しの場所・ブロックのデータから作った識別子）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistTilesetInfo
	{
		// 1 = タイルセット1、2 = タイルセット2
		public int Kind;
		public string Game = string.Empty;
		public long HeaderOffset = -1;
		public string Fingerprint = string.Empty;
		public int BlockCount;
		public string Name = string.Empty;

		//-------------------------------------------------------------------------------
		// 同じ ROM の同じタイルセットか（ゲームと見出しの場所で見る。ブロックを編集しても変わらない）
		//-------------------------------------------------------------------------------
		public bool SamePlace(MapAssistTilesetInfo other)
		{
			return other != null && this.Kind == other.Kind && this.HeaderOffset >= 0 && this.HeaderOffset == other.HeaderOffset && string.Equals(this.Game, other.Game, StringComparison.Ordinal);
		}

		//-------------------------------------------------------------------------------
		// 中身が同じタイルセットか（識別子で見る。別の ROM へ移したタイルセットも同じになる）
		//-------------------------------------------------------------------------------
		public bool SameContent(MapAssistTilesetInfo other)
		{
			return other != null && this.Kind == other.Kind && this.Fingerprint.Length > 0 && string.Equals(this.Fingerprint, other.Fingerprint, StringComparison.OrdinalIgnoreCase);
		}
	}

	//-------------------------------------------------------------------------------
	// 保存ファイル 1 つ（タイルセット 1 つぶんの指定）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistFile
	{
		public MapAssistTilesetInfo Tileset = new MapAssistTilesetInfo();
		public readonly List<MapAssistPart> Parts = new List<MapAssistPart>();
		public string FilePath;

		//-------------------------------------------------------------------------------
		// ファイルを読む処理（読めない・形が違うときは null。壊れたパーツは飛ばす）
		// headerOnly なら、タイルセットの見分け方だけを読む
		//-------------------------------------------------------------------------------
		public static MapAssistFile Load(string path, bool headerOnly)
		{
			string[] lines;
			try
			{
				lines = File.ReadAllLines(path, Encoding.UTF8);
			}
			catch (Exception)
			{
				return null;
			}
			MapAssistFile file = new MapAssistFile { FilePath = path };
			string section = string.Empty;
			MapAssistPart part = null;
			bool partBroken = false;
			bool sawTileset = false;
			foreach (string raw in lines)
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal))
				{
					continue;
				}
				if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
				{
					if (part != null && !partBroken)
					{
						file.Parts.Add(part);
					}
					part = null;
					partBroken = false;
					section = line.Substring(1, line.Length - 2);
					if (section == "Part")
					{
						if (headerOnly)
						{
							break;
						}
						part = new MapAssistPart();
					}
					else if (section == "Tileset")
					{
						sawTileset = true;
					}
					continue;
				}
				int equal = line.IndexOf('=');
				if (equal <= 0)
				{
					continue;
				}
				string key = line.Substring(0, equal).Trim();
				string value = line.Substring(equal + 1).Trim();
				if (section == "Tileset")
				{
					ReadTilesetField(file.Tileset, key, value);
				}
				else if (section == "Part" && part != null && !ReadPartField(part, key, value))
				{
					partBroken = true;
				}
			}
			if (part != null && !partBroken)
			{
				file.Parts.Add(part);
			}
			return sawTileset && (file.Tileset.Kind == 1 || file.Tileset.Kind == 2) ? file : null;
		}

		//-------------------------------------------------------------------------------
		// [Tileset] の 1 項目を読む処理
		//-------------------------------------------------------------------------------
		private static void ReadTilesetField(MapAssistTilesetInfo info, string key, string value)
		{
			int number;
			long offset;
			switch (key)
			{
				case "Name":
					info.Name = value;
					break;
				case "Kind":
					if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
					{
						info.Kind = number;
					}
					break;
				case "Game":
					info.Game = value;
					break;
				case "HeaderOffset":
					if (long.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset))
					{
						info.HeaderOffset = offset;
					}
					break;
				case "Fingerprint":
					info.Fingerprint = value;
					break;
				case "BlockCount":
					if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
					{
						info.BlockCount = number;
					}
					break;
			}
		}

		//-------------------------------------------------------------------------------
		// [Part] の 1 項目を読む処理（読めない値なら false = このパーツは捨てる）
		//-------------------------------------------------------------------------------
		private static bool ReadPartField(MapAssistPart part, string key, string value)
		{
			long offset;
			int order;
			switch (key)
			{
				case "Name":
					part.Name = value;
					return true;
				case "Order":
					if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out order))
					{
						part.Order = order;
					}
					return true;
				case "Kind":
					return Enum.TryParse(value, out part.Kind) && Enum.IsDefined(typeof(MapAssistKind), part.Kind);
				case "Role":
					return Enum.TryParse(value, out part.Role) && Enum.IsDefined(typeof(MapAssistRole), part.Role);
				case "Primary":
					part.PrimaryFingerprint = value;
					return true;
				case "PrimaryGame":
					part.PrimaryGame = value;
					return true;
				case "PrimaryOffset":
					if (long.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset))
					{
						part.PrimaryOffset = offset;
					}
					return true;
			}
			if (key.StartsWith("Slot.", StringComparison.Ordinal))
			{
				MapAssistGrid grid;
				if (!MapAssistGrid.TryParse(value, out grid))
				{
					return false;
				}
				part.Slots[key.Substring(5)] = grid;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// ファイルへ書く処理（フォルダが無ければ作る）
		//-------------------------------------------------------------------------------
		public void Save()
		{
			StringBuilder text = new StringBuilder();
			text.AppendLine("; BochiBochiEditor マップ作成補助の指定（タイルセット 1 つぶん）");
			text.AppendLine("; マスの書き方: タイルセット(1/2):ブロック番号(16進):移動エリア(16進。決めていなければ --)。空きは -");
			text.AppendLine("[Tileset]");
			text.AppendLine("Name=" + this.Tileset.Name);
			text.AppendLine("Kind=" + this.Tileset.Kind.ToString(CultureInfo.InvariantCulture));
			text.AppendLine("Game=" + this.Tileset.Game);
			text.AppendLine("HeaderOffset=" + this.Tileset.HeaderOffset.ToString("X6", CultureInfo.InvariantCulture));
			text.AppendLine("Fingerprint=" + this.Tileset.Fingerprint);
			text.AppendLine("BlockCount=" + this.Tileset.BlockCount.ToString(CultureInfo.InvariantCulture));
			foreach (MapAssistPart part in this.Parts)
			{
				text.AppendLine();
				text.AppendLine("[Part]");
				text.AppendLine("Name=" + part.Name.Replace("\r", " ").Replace("\n", " "));
				text.AppendLine("Kind=" + part.Kind);
				text.AppendLine("Role=" + part.Role);
				text.AppendLine("Order=" + part.Order.ToString(CultureInfo.InvariantCulture));
				if (part.PrimaryFingerprint.Length > 0 || part.PrimaryOffset >= 0)
				{
					text.AppendLine("Primary=" + part.PrimaryFingerprint);
					text.AppendLine("PrimaryGame=" + part.PrimaryGame);
					text.AppendLine("PrimaryOffset=" + part.PrimaryOffset.ToString("X6", CultureInfo.InvariantCulture));
				}
				foreach (string name in MapAssistPart.AllSlotNamesOf(part.Kind))
				{
					MapAssistGrid grid = part.GetSlot(name);
					if (grid != null && !grid.IsBlank)
					{
						text.AppendLine("Slot." + name + "=" + grid.ToText());
					}
				}
			}
			Directory.CreateDirectory(Path.GetDirectoryName(this.FilePath));
			File.WriteAllText(this.FilePath, text.ToString(), new UTF8Encoding(false));
		}
	}

	//-------------------------------------------------------------------------------
	// タイルセット 1 と 2 の組で使う指定のまとまり（画面が編集する対象）
	//-------------------------------------------------------------------------------
	internal sealed class MapAssistSet
	{
		public MapAssistTilesetInfo Primary;
		public MapAssistTilesetInfo Secondary;
		// この組で使えるパーツ（画面に出す・編集するもの）
		public readonly List<MapAssistPart> Parts = new List<MapAssistPart>();
		// タイルセット2 のファイルに入っている、別のタイルセット1 と組で使うパーツ（画面には出さず、保存のときにそのまま書き戻す）
		private readonly List<MapAssistPart> otherPrimaryParts = new List<MapAssistPart>();
		private string primaryPath;
		private string secondaryPath;

		// 読み込んだファイルの名前（無ければ null。画面の案内に使う）
		public string PrimaryFileName
		{
			get { return this.primaryPath != null ? Path.GetFileName(this.primaryPath) : null; }
		}

		public string SecondaryFileName
		{
			get { return this.secondaryPath != null ? Path.GetFileName(this.secondaryPath) : null; }
		}

		//-------------------------------------------------------------------------------
		// タイルセットの組に合う保存ファイルを探して読み込む処理（無ければ空のまとまり）
		//-------------------------------------------------------------------------------
		public static MapAssistSet Load(MapAssistTilesetInfo primary, MapAssistTilesetInfo secondary)
		{
			MapAssistSet set = new MapAssistSet { Primary = primary, Secondary = secondary };
			MapAssistFile primaryFile = MapAssistStore.FindFor(primary);
			if (primaryFile != null)
			{
				set.primaryPath = primaryFile.FilePath;
				set.Parts.AddRange(primaryFile.Parts.Where(p => !p.UsesTileset(2)));
			}
			MapAssistFile secondaryFile = MapAssistStore.FindFor(secondary);
			if (secondaryFile != null)
			{
				set.secondaryPath = secondaryFile.FilePath;
				foreach (MapAssistPart part in secondaryFile.Parts)
				{
					// タイルセット1 のブロックも使うパーツは、そのタイルセット1 と組のときだけ使える
					bool usable = !part.UsesTileset(1)
						|| (part.PrimaryFingerprint.Length > 0 && string.Equals(part.PrimaryFingerprint, primary.Fingerprint, StringComparison.OrdinalIgnoreCase))
						|| (part.PrimaryOffset >= 0 && part.PrimaryOffset == primary.HeaderOffset && string.Equals(part.PrimaryGame, primary.Game, StringComparison.Ordinal));
					(usable ? set.Parts : set.otherPrimaryParts).Add(part);
				}
			}
			// 保存したときの並び順に戻す（同じ順番のものは、読んだ順のまま）
			List<MapAssistPart> ordered = set.Parts.OrderBy(p => p.Order).ToList();
			set.Parts.Clear();
			set.Parts.AddRange(ordered);
			return set;
		}

		//-------------------------------------------------------------------------------
		// 保存する処理（書いたファイルの名前を返す）
		// タイルセット2 のブロックを使うパーツはタイルセット2 のファイルへ、それ以外はタイルセット1 のファイルへ書く
		//-------------------------------------------------------------------------------
		public List<string> Save()
		{
			List<string> written = new List<string>();
			MapAssistFile primaryFile = new MapAssistFile { Tileset = this.Primary, FilePath = this.primaryPath ?? MapAssistStore.NewFilePath(this.Primary) };
			MapAssistFile secondaryFile = new MapAssistFile { Tileset = this.Secondary, FilePath = this.secondaryPath ?? MapAssistStore.NewFilePath(this.Secondary) };
			int order = 0;
			foreach (MapAssistPart part in this.Parts)
			{
				part.Order = order++;
				if (part.UsesTileset(2))
				{
					bool both = part.UsesTileset(1);
					part.PrimaryFingerprint = both ? this.Primary.Fingerprint : string.Empty;
					part.PrimaryGame = both ? this.Primary.Game : string.Empty;
					part.PrimaryOffset = both ? this.Primary.HeaderOffset : -1;
					secondaryFile.Parts.Add(part);
				}
				else
				{
					part.PrimaryFingerprint = string.Empty;
					part.PrimaryGame = string.Empty;
					part.PrimaryOffset = -1;
					primaryFile.Parts.Add(part);
				}
			}
			secondaryFile.Parts.AddRange(this.otherPrimaryParts);
			// 中身が無く、ファイルもまだ無いタイルセットには、空のファイルを作らない
			if (primaryFile.Parts.Count > 0 || this.primaryPath != null)
			{
				primaryFile.Save();
				this.primaryPath = primaryFile.FilePath;
				written.Add(Path.GetFileName(primaryFile.FilePath));
			}
			if (secondaryFile.Parts.Count > 0 || this.secondaryPath != null)
			{
				secondaryFile.Save();
				this.secondaryPath = secondaryFile.FilePath;
				written.Add(Path.GetFileName(secondaryFile.FilePath));
			}
			return written;
		}
	}

	//-------------------------------------------------------------------------------
	// 保存ファイルの置き場所と、タイルセットに合うファイルの探し方
	//-------------------------------------------------------------------------------
	internal static class MapAssistStore
	{
		// 保存先のフォルダの名前（resource の下）
		public const string FolderName = "mapassist";

		// 検証用: 設定されていれば、resource\mapassist の代わりにこのフォルダを使う
		internal static string DirectoryOverride;

		//-------------------------------------------------------------------------------
		// 保存先のフォルダ（exe と同じ場所の resource\mapassist）
		//-------------------------------------------------------------------------------
		public static string DirectoryPath
		{
			get { return DirectoryOverride ?? Path.Combine(AppAssetLocator.ResourceDirectory, FolderName); }
		}

		//-------------------------------------------------------------------------------
		// タイルセットに合う保存ファイルを探して読む処理（無ければ null）
		// 1. 識別子（ブロックのデータ）が同じファイル。複数あれば、ゲームと見出しの場所も同じものを先に選ぶ
		// 2. 無ければ、ゲームと見出しの場所が同じファイル（ブロックを編集して識別子が変わった後でも、同じ指定を使うため）
		//-------------------------------------------------------------------------------
		public static MapAssistFile FindFor(MapAssistTilesetInfo info)
		{
			if (info == null || !Directory.Exists(DirectoryPath))
			{
				return null;
			}
			string byContent = null;
			string byContentAndPlace = null;
			string byPlace = null;
			foreach (string path in Directory.GetFiles(DirectoryPath, "*.txt").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
			{
				MapAssistFile header = MapAssistFile.Load(path, true);
				if (header == null)
				{
					continue;
				}
				bool content = info.SameContent(header.Tileset);
				bool place = info.SamePlace(header.Tileset);
				if (content && place && byContentAndPlace == null)
				{
					byContentAndPlace = path;
				}
				else if (content && byContent == null)
				{
					byContent = path;
				}
				else if (place && byPlace == null)
				{
					byPlace = path;
				}
			}
			string chosen = byContentAndPlace ?? byContent ?? byPlace;
			return chosen != null ? MapAssistFile.Load(chosen, false) : null;
		}

		//-------------------------------------------------------------------------------
		// 新しい保存ファイルの名前を決める処理（ゲーム_TS番号_見出しの場所_識別子の先頭.txt。同じ名前があれば番号を足す）
		//-------------------------------------------------------------------------------
		public static string NewFilePath(MapAssistTilesetInfo info)
		{
			string stem = string.Format(CultureInfo.InvariantCulture, "{0}_TS{1}_{2:X6}_{3}", SafeName(info.Game), info.Kind, info.HeaderOffset,
				info.Fingerprint.Length >= 8 ? info.Fingerprint.Substring(0, 8) : "00000000");
			string path = Path.Combine(DirectoryPath, stem + ".txt");
			for (int i = 2; File.Exists(path) && i < 1000; i++)
			{
				path = Path.Combine(DirectoryPath, stem + "_" + i.ToString(CultureInfo.InvariantCulture) + ".txt");
			}
			return path;
		}

		//-------------------------------------------------------------------------------
		// ファイル名に使えない文字を除く処理
		//-------------------------------------------------------------------------------
		private static string SafeName(string text)
		{
			string cleaned = new string((text ?? string.Empty).Where(c => char.IsLetterOrDigit(c)).ToArray());
			return cleaned.Length > 0 ? cleaned : "ROM";
		}
	}
}
