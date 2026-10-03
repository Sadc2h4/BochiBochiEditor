using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップチップ画像を GBA のタイルセット用データ（16色パレット・8x8 タイル・16x16 ブロック）へ変換する処理
	// ROM には触れず、「何をどこへ書くか」の計画（TileImportPlan）だけを作る
	//-------------------------------------------------------------------------------
	internal static class TileImportEngine
	{
		public const int TileBytes = 32;

		//-------------------------------------------------------------------------------
		// パレットの作り方
		//-------------------------------------------------------------------------------
		public enum PaletteMode
		{
			// 画像の色を 15 色以内に減らし、選んだパレット枠を上書きする
			Overwrite,
			// 選んだパレット枠の既存の色に近い色へ置き換える（パレットは変更しない）
			MatchExisting
		}

		//-------------------------------------------------------------------------------
		// ブロックの重ね方
		//-------------------------------------------------------------------------------
		public enum LayerStyle
		{
			// 絵を下層に置く（下地なし。透明部分は背景色になる）
			ArtOnBottom,
			// 下地ブロックの下層の上に、絵を重ねる（プレイヤーの下に描く：家具・床の模様など。3 層では中層に置く）
			OverBaseBelowPlayer,
			// 下地ブロックの下層の上に、絵を重ねる（プレイヤーより手前に描く：木の上部・屋根など。3 層では下層と中層を残して上層に置く）
			OverBaseAbovePlayer
		}

		//-------------------------------------------------------------------------------
		// 取り込み条件（ウィザードで選んだ内容）
		//-------------------------------------------------------------------------------
		public sealed class Options
		{
			public Bitmap Source;
			public Color TransparentColor;
			public bool IsSecondary = true;
			public int PaletteSlot;
			public PaletteMode Mode = PaletteMode.Overwrite;
			public Color[] ExistingPalette = new Color[16];
			public bool DedupeFlipped = true;
			public bool ReuseExistingTiles = true;
			public bool SkipEmptyCells = true;
			// 同じ絵の 16x16 マスは 1 ブロックにまとめる（ブロックの空きを節約する）
			public bool DedupeBlocks = true;
			public LayerStyle Layer = LayerStyle.ArtOnBottom;
			// 下地ブロックの全層分のタイル指定（LayerStyle が OverBase〜 のときに、絵の層より下の層を写す）
			public ushort[] BaseEntries;
		}

		//-------------------------------------------------------------------------------
		// 取り込み先タイルセットの現状（既存タイル・空き枠）
		//-------------------------------------------------------------------------------
		public sealed class TargetState
		{
			// 取り込み先タイルセットのタイル画像（展開済み、4bpp）
			public byte[] TileImage = new byte[0];
			// 取り込み先で「空き」として使ってよいタイル番号（タイルセット内の番号）
			public List<int> FreeTileSlots = new List<int>();
			// 取り込み先で「空き」として使ってよいブロック番号（全体の通し番号）
			public List<int> FreeBlockIds = new List<int>();
			// 新規タイルを末尾へ追加できる上限（タイルセット内の枚数）
			public int TileCapacity;
			// 既存タイルとして再利用候補にするタイル画像（通し番号 → 32 バイト）。第2タイルセットでは第1のタイルも含む
			public Dictionary<int, byte[]> ReusableTiles = new Dictionary<int, byte[]>();
			// 全面透明（色番号 0 のみ）のタイルとして使える通し番号（無ければ -1）
			public int BlankTileId = -1;
			// 取り込み先タイルセットのブロック番号の範囲（通し番号）
			public int BlockStart;
			public int BlockCount;
			// ブロック番号 → そのブロックを置いているマップの数（0 のブロックは載せない）
			public Dictionary<int, int> BlockMapUsage = new Dictionary<int, int>();
			// 置いてはいけないブロック番号（ファイアレッドで、前のブロックの 3 層目として使われている枠）
			public HashSet<int> ProtectedBlockIds = new HashSet<int>();
			// 描画用: 第1（規定の枚数にそろえたもの）＋第2のタイル画像と、全パレット（16 色 × 16 本）
			public byte[] CombinedTiles = new byte[0];
			public Color[] AllPalettes = new Color[256];
			// タイルセットの形（第1の枚数・ブロックのバイト数など。必ず設定する）
			public TileImportFormat Format;
		}

		//-------------------------------------------------------------------------------
		// 取り込み計画 1 ブロック分
		//-------------------------------------------------------------------------------
		public sealed class PlannedBlock
		{
			// 書き込み先のブロック番号（まだ決まっていなければ -1）
			public int BlockId = -1;
			// 代表のマス（最初に出てきた位置）
			public int CellX;
			public int CellY;
			// このブロックを使う画像内のマス（同じ絵のマスをまとめた場合は複数）
			public List<Point> Cells = new List<Point>();
			// タイル指定（Format.EntriesPerBlock 個。2 層なら 8、3 層なら 12）
			public ushort[] Entries = new ushort[0];
		}

		//-------------------------------------------------------------------------------
		// 取り込み計画（書き込む内容と集計）
		//-------------------------------------------------------------------------------
		public sealed class Plan
		{
			public Color[] Palette = new Color[16];
			public Bitmap ConvertedPreview;
			public int SourceColorCount;
			public int UsedColorCount;
			public int CellColumns;
			public int CellRows;
			// 書き込むタイル（タイルセット内の番号 → 32 バイト）
			public Dictionary<int, byte[]> NewTiles = new Dictionary<int, byte[]>();
			public List<PlannedBlock> Blocks = new List<PlannedBlock>();
			// 画像のマス（x, y）→ Blocks の添字（全部透明で省いたマスは -1）
			public int[,] CellBlockIndex = new int[0, 0];
			// 取り込み先の情報（配置画面で使う）
			public TargetState Target;
			public int TotalTileCount;
			public int ReusedTileCount;
			public int FlippedTileCount;
			public int DuplicateTileCount;
			public int EmptyCellCount;
			public int DuplicateCellCount;
			public int RequiredNewTiles;
			public int RequiredBlocks;
			public int AvailableTileSlots;
			public int AvailableBlockSlots;
			public List<string> Errors = new List<string>();
			public List<string> Notes = new List<string>();
			// Notes のうち色（減色）に関するもの
			public List<string> ColorNotes = new List<string>();

			//-------------------------------------------------------------------------------
			// 書き込み可能な計画か（空き不足などのエラーが無いか）を返す処理
			//-------------------------------------------------------------------------------
			public bool CanWrite
			{
				get { return this.Errors.Count == 0 && this.Blocks.Count > 0 && this.UnassignedCount == 0 && !this.HasDuplicateAssignment && this.FindAssignmentProblem() == null; }
			}

			//-------------------------------------------------------------------------------
			// 計画を作ったときのタイルセットの形を返す処理（取り込み先の情報が無ければ null）
			//-------------------------------------------------------------------------------
			public TileImportFormat Format
			{
				get { return this.Target == null ? null : this.Target.Format; }
			}

			//-------------------------------------------------------------------------------
			// 割り当て済みのブロックに、書き込めない番号・形のものが無いかを調べる処理（問題が無ければ null）
			//-------------------------------------------------------------------------------
			public string FindAssignmentProblem()
			{
				if (this.Target == null || this.Target.Format == null)
				{
					return Localizer.T("取り込み先の情報がありません。");
				}
				int entryCount = this.Target.Format.EntriesPerBlock;
				foreach (PlannedBlock block in this.Blocks)
				{
					if (block.Entries == null || block.Entries.Length != entryCount)
					{
						return Localizer.T("ブロックのタイル指定の数がタイルセットの形と合いません。");
					}
					if (block.BlockId < 0)
					{
						continue;
					}
					if (block.BlockId == 0 || block.BlockId < this.Target.BlockStart || block.BlockId >= this.Target.BlockStart + this.Target.BlockCount)
					{
						return string.Format(Localizer.T("ブロック 0x{0:X3} は取り込み先のタイルセットの範囲外です。"), block.BlockId);
					}
					if (this.Target.ProtectedBlockIds.Contains(block.BlockId))
					{
						return string.Format(Localizer.T("ブロック 0x{0:X3} は前のブロックの 3 層目として使われているため置けません。"), block.BlockId);
					}
				}
				return null;
			}

			//-------------------------------------------------------------------------------
			// タイル・パレットの面で問題が無いか（ブロックの配置は問わない）を返す処理
			//-------------------------------------------------------------------------------
			public bool TilesReady
			{
				get { return this.Errors.Count == 0 && this.Blocks.Count > 0; }
			}

			//-------------------------------------------------------------------------------
			// まだ書き込み先の決まっていないブロックの数を返す処理
			//-------------------------------------------------------------------------------
			public int UnassignedCount
			{
				get { return this.Blocks.Count(b => b.BlockId < 0); }
			}

			//-------------------------------------------------------------------------------
			// 同じ番号に 2 個以上のブロックを割り当てていないかを返す処理
			//-------------------------------------------------------------------------------
			public bool HasDuplicateAssignment
			{
				get
				{
					List<int> ids = this.Blocks.Where(b => b.BlockId >= 0).Select(b => b.BlockId).ToList();
					return ids.Count != ids.Distinct().Count();
				}
			}

			//-------------------------------------------------------------------------------
			// 使用中のブロック（マップに置かれている番号）へ割り当てたブロックの数を返す処理
			//-------------------------------------------------------------------------------
			public int ReplacingCount
			{
				get { return this.Target == null ? 0 : this.Blocks.Count(b => b.BlockId >= 0 && this.Target.BlockMapUsage.ContainsKey(b.BlockId)); }
			}

			//-------------------------------------------------------------------------------
			// 空いているブロックへ、まだ番号の無いブロックを順に割り当てる処理（既に決まっているものは変えない）
			//-------------------------------------------------------------------------------
			public void AutoAssign()
			{
				if (this.Target == null)
				{
					return;
				}
				HashSet<int> taken = new HashSet<int>(this.Blocks.Where(b => b.BlockId >= 0).Select(b => b.BlockId));
				Queue<int> free = new Queue<int>(this.Target.FreeBlockIds.Where(id => !taken.Contains(id) && !this.Target.ProtectedBlockIds.Contains(id)));
				foreach (PlannedBlock block in this.Blocks)
				{
					if (block.BlockId < 0 && free.Count > 0)
					{
						block.BlockId = free.Dequeue();
					}
				}
			}

			//-------------------------------------------------------------------------------
			// すべての割り当てを外す処理
			//-------------------------------------------------------------------------------
			public void ClearAssignments()
			{
				foreach (PlannedBlock block in this.Blocks)
				{
					block.BlockId = -1;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 色を GBA の 15bit カラー（各 5bit）に丸める処理
		//-------------------------------------------------------------------------------
		public static Color ToGbaColor(Color c)
		{
			int r = (c.R >> 3) << 3;
			int g = (c.G >> 3) << 3;
			int b = (c.B >> 3) << 3;
			return Color.FromArgb(255, r | (r >> 5), g | (g >> 5), b | (b >> 5));
		}

		//-------------------------------------------------------------------------------
		// 色を GBA のパレット形式（BGR555 の 2 バイト）へ変換する処理
		//-------------------------------------------------------------------------------
		public static ushort ToBgr555(Color c)
		{
			return (ushort)((c.R >> 3) | ((c.G >> 3) << 5) | ((c.B >> 3) << 10));
		}

		//-------------------------------------------------------------------------------
		// 画像の 1 ピクセルが透明扱いか判定する処理（アルファが低い、または指定した透明色）
		//-------------------------------------------------------------------------------
		private static bool IsTransparent(Color pixel, Color transparent)
		{
			if (pixel.A < 128)
			{
				return true;
			}
			return pixel.R == transparent.R && pixel.G == transparent.G && pixel.B == transparent.B;
		}

		//-------------------------------------------------------------------------------
		// 画像を 32bit のピクセル配列として読み出す処理（高速化のため LockBits を使う）
		//-------------------------------------------------------------------------------
		public static Color[,] ReadPixels(Bitmap source)
		{
			int w = source.Width;
			int h = source.Height;
			Color[,] pixels = new Color[w, h];
			using (Bitmap copy = new Bitmap(w, h, PixelFormat.Format32bppArgb))
			{
				using (Graphics g = Graphics.FromImage(copy))
				{
					g.DrawImage(source, new Rectangle(0, 0, w, h));
				}
				BitmapData data = copy.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				int[] raw = new int[w * h];
				for (int y = 0; y < h; y++)
				{
					System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * data.Stride, raw, y * w, w);
				}
				copy.UnlockBits(data);
				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						pixels[x, y] = Color.FromArgb(raw[y * w + x]);
					}
				}
			}
			return pixels;
		}

		//-------------------------------------------------------------------------------
		// 画像に含まれる色数（透明を除き、GBA の色に丸めた後）を数える処理
		//-------------------------------------------------------------------------------
		public static int CountColors(Color[,] pixels, Color transparent)
		{
			HashSet<int> colors = new HashSet<int>();
			foreach (Color c in pixels)
			{
				if (!IsTransparent(c, transparent))
				{
					colors.Add(ToGbaColor(c).ToArgb());
				}
			}
			return colors.Count;
		}

		//-------------------------------------------------------------------------------
		// 取り込み計画を作る処理（減色 → タイル分割 → 重複・反転の整理 → 空き枠への割り当て）
		//-------------------------------------------------------------------------------
		public static Plan BuildPlan(Options options, TargetState target)
		{
			if (target == null || target.Format == null)
			{
				throw new ArgumentException("TargetState.Format is required.", nameof(target));
			}
			Plan plan = new Plan();
			Color[,] pixels = ReadPixels(options.Source);
			int srcW = pixels.GetLength(0);
			int srcH = pixels.GetLength(1);
			plan.CellColumns = (srcW + 15) / 16;
			plan.CellRows = (srcH + 15) / 16;
			plan.SourceColorCount = CountColors(pixels, options.TransparentColor);
			if (srcW % 16 != 0 || srcH % 16 != 0)
			{
				plan.Notes.Add(string.Format(Localizer.T("画像サイズ {0}×{1} は 16 の倍数ではないため、右と下に透明な余白を足して {2}×{3} として扱います。"), srcW, srcH, plan.CellColumns * 16, plan.CellRows * 16));
			}

			// 1. パレットを決める（色 0 は透明）
			plan.Palette = BuildPalette(pixels, options, plan);

			// 2. 各ピクセルを色番号へ変換する
			int width = plan.CellColumns * 16;
			int height = plan.CellRows * 16;
			byte[,] indices = new byte[width, height];
			Dictionary<int, byte> nearestCache = new Dictionary<int, byte>();
			HashSet<byte> used = new HashSet<byte>();
			for (int y = 0; y < srcH; y++)
			{
				for (int x = 0; x < srcW; x++)
				{
					Color c = pixels[x, y];
					if (IsTransparent(c, options.TransparentColor))
					{
						continue;
					}
					int key = ToGbaColor(c).ToArgb();
					byte index;
					if (!nearestCache.TryGetValue(key, out index))
					{
						index = NearestPaletteIndex(Color.FromArgb(key), plan.Palette);
						nearestCache[key] = index;
					}
					indices[x, y] = index;
					used.Add(index);
				}
			}
			plan.UsedColorCount = used.Count;
			plan.ConvertedPreview = RenderIndices(indices, plan.Palette);

			// 対応していない形（ブロックのバイト数など）のときは、タイル・ブロックを作らずにエラーにする
			if (!target.Format.IsSupported)
			{
				plan.Target = target;
				plan.CellBlockIndex = new int[plan.CellColumns, plan.CellRows];
				for (int y = 0; y < plan.CellRows; y++)
				{
					for (int x = 0; x < plan.CellColumns; x++)
					{
						plan.CellBlockIndex[x, y] = -1;
					}
				}
				plan.Errors.Add(string.Format(Localizer.T("このタイルセットの形（ブロック {0} バイト・挙動 {1} バイト）には、取り込みが対応していません。"), target.Format.BlockBytes, target.Format.BehaviorBytes));
				return plan;
			}

			// 3. 8x8 タイルに分けて、重複・反転・既存タイルを整理しながら番号を割り当てる
			AssignTilesAndBlocks(indices, options, target, plan);
			return plan;
		}

		//-------------------------------------------------------------------------------
		// パレット（16 色、色 0 は透明）を作る処理
		//-------------------------------------------------------------------------------
		private static Color[] BuildPalette(Color[,] pixels, Options options, Plan plan)
		{
			Color[] palette = new Color[16];
			for (int i = 0; i < 16; i++)
			{
				palette[i] = (options.ExistingPalette != null && i < options.ExistingPalette.Length) ? ToGbaColor(options.ExistingPalette[i]) : Color.Black;
			}
			if (options.Mode == PaletteMode.MatchExisting)
			{
				return palette;
			}
			// 画像の色（GBA の色に丸めたもの）と出現数を集める
			Dictionary<int, int> histogram = new Dictionary<int, int>();
			foreach (Color c in pixels)
			{
				if (IsTransparent(c, options.TransparentColor))
				{
					continue;
				}
				int key = ToGbaColor(c).ToArgb();
				int count;
				histogram.TryGetValue(key, out count);
				histogram[key] = count + 1;
			}
			List<Color> colors;
			if (histogram.Count <= 15)
			{
				colors = histogram.OrderByDescending(p => p.Value).Select(p => Color.FromArgb(p.Key)).ToList();
			}
			else
			{
				colors = MedianCut(histogram, 15);
				string colorNote = string.Format(Localizer.T("画像は {0} 色あるため、15 色に減らしました（1 パレットは透明を含めて 16 色まで）。"), histogram.Count);
				plan.Notes.Add(colorNote);
				plan.ColorNotes.Add(colorNote);
			}
			// 色 0 は透明なので既存の値のままにし、1〜15 を画像の色で埋める
			for (int i = 1; i < 16; i++)
			{
				palette[i] = (i - 1 < colors.Count) ? ToGbaColor(colors[i - 1]) : Color.Black;
			}
			return palette;
		}

		//-------------------------------------------------------------------------------
		// メディアンカット法で色数を減らす処理（出現数で重み付けした箱を分割していく）
		//-------------------------------------------------------------------------------
		private static List<Color> MedianCut(Dictionary<int, int> histogram, int targetCount)
		{
			List<List<KeyValuePair<Color, int>>> boxes = new List<List<KeyValuePair<Color, int>>>
			{
				histogram.Select(p => new KeyValuePair<Color, int>(Color.FromArgb(p.Key), p.Value)).ToList()
			};
			while (boxes.Count < targetCount)
			{
				// 色の幅が最も大きい箱を選ぶ
				int bestIndex = -1;
				int bestRange = -1;
				int bestChannel = 0;
				for (int i = 0; i < boxes.Count; i++)
				{
					if (boxes[i].Count < 2)
					{
						continue;
					}
					int[] ranges =
					{
						boxes[i].Max(p => p.Key.R) - boxes[i].Min(p => p.Key.R),
						boxes[i].Max(p => p.Key.G) - boxes[i].Min(p => p.Key.G),
						boxes[i].Max(p => p.Key.B) - boxes[i].Min(p => p.Key.B)
					};
					int channel = Array.IndexOf(ranges, ranges.Max());
					if (ranges[channel] > bestRange)
					{
						bestRange = ranges[channel];
						bestIndex = i;
						bestChannel = channel;
					}
				}
				if (bestIndex < 0)
				{
					break;
				}
				List<KeyValuePair<Color, int>> box = boxes[bestIndex];
				Func<Color, int> channelOf = bestChannel == 0 ? (Func<Color, int>)(c => c.R) : (bestChannel == 1 ? (Func<Color, int>)(c => c.G) : (c => c.B));
				box.Sort((a, b) => channelOf(a.Key).CompareTo(channelOf(b.Key)));
				// 出現数の累計が半分になる位置で分ける
				long total = box.Sum(p => (long)p.Value);
				long acc = 0;
				int split = 1;
				for (int i = 0; i < box.Count - 1; i++)
				{
					acc += box[i].Value;
					if (acc * 2 >= total)
					{
						split = i + 1;
						break;
					}
				}
				boxes[bestIndex] = box.Take(split).ToList();
				boxes.Add(box.Skip(split).ToList());
			}
			// 各箱の加重平均色を代表色にする
			return boxes.Where(b => b.Count > 0).Select(b =>
			{
				long w = b.Sum(p => (long)p.Value);
				int r = (int)(b.Sum(p => (long)p.Key.R * p.Value) / w);
				int g = (int)(b.Sum(p => (long)p.Key.G * p.Value) / w);
				int bl = (int)(b.Sum(p => (long)p.Key.B * p.Value) / w);
				return Color.FromArgb(r, g, bl);
			}).OrderByDescending(c => c.GetBrightness()).ToList();
		}

		//-------------------------------------------------------------------------------
		// パレットの色 1〜15 の中から最も近い色の番号を返す処理（色 0 は透明専用なので使わない）
		//-------------------------------------------------------------------------------
		public static byte NearestPaletteIndex(Color c, Color[] palette)
		{
			byte best = 1;
			int bestDistance = int.MaxValue;
			for (int i = 1; i < 16; i++)
			{
				int dr = c.R - palette[i].R;
				int dg = c.G - palette[i].G;
				int db = c.B - palette[i].B;
				// 人の目の感度に合わせて緑を重く、青を軽く扱う
				int distance = dr * dr * 3 + dg * dg * 4 + db * db * 2;
				if (distance < bestDistance)
				{
					bestDistance = distance;
					best = (byte)i;
				}
			}
			return best;
		}

		//-------------------------------------------------------------------------------
		// 色番号の配列をプレビュー用の画像にする処理（色 0 は透明）
		//-------------------------------------------------------------------------------
		private static Bitmap RenderIndices(byte[,] indices, Color[] palette)
		{
			int w = indices.GetLength(0);
			int h = indices.GetLength(1);
			Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int[] raw = new int[w * h];
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					byte i = indices[x, y];
					raw[y * w + x] = i == 0 ? 0 : palette[i].ToArgb();
				}
			}
			for (int y = 0; y < h; y++)
			{
				System.Runtime.InteropServices.Marshal.Copy(raw, y * w, data.Scan0 + y * data.Stride, w);
			}
			bmp.UnlockBits(data);
			return bmp;
		}

		//-------------------------------------------------------------------------------
		// 8x8 タイルの色番号を GBA の 4bpp 形式（32 バイト）に詰める処理
		//-------------------------------------------------------------------------------
		public static byte[] PackTile(byte[,] indices, int left, int top)
		{
			byte[] tile = new byte[TileBytes];
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x += 2)
				{
					byte lo = indices[left + x, top + y];
					byte hi = indices[left + x + 1, top + y];
					tile[y * 4 + x / 2] = (byte)((lo & 0x0F) | ((hi & 0x0F) << 4));
				}
			}
			return tile;
		}

		//-------------------------------------------------------------------------------
		// 4bpp タイルを左右反転する処理
		//-------------------------------------------------------------------------------
		public static byte[] FlipTileH(byte[] tile)
		{
			byte[] result = new byte[TileBytes];
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					int value = GetTilePixel(tile, x, y);
					SetTilePixel(result, 7 - x, y, value);
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 4bpp タイルを上下反転する処理
		//-------------------------------------------------------------------------------
		public static byte[] FlipTileV(byte[] tile)
		{
			byte[] result = new byte[TileBytes];
			for (int y = 0; y < 8; y++)
			{
				Array.Copy(tile, y * 4, result, (7 - y) * 4, 4);
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 4bpp タイルの 1 ピクセルの色番号を読む処理
		//-------------------------------------------------------------------------------
		private static int GetTilePixel(byte[] tile, int x, int y)
		{
			byte b = tile[y * 4 + x / 2];
			return (x % 2 == 0) ? (b & 0x0F) : (b >> 4);
		}

		//-------------------------------------------------------------------------------
		// 4bpp タイルの 1 ピクセルに色番号を書く処理
		//-------------------------------------------------------------------------------
		private static void SetTilePixel(byte[] tile, int x, int y, int value)
		{
			int i = y * 4 + x / 2;
			tile[i] = (x % 2 == 0) ? (byte)((tile[i] & 0xF0) | (value & 0x0F)) : (byte)((tile[i] & 0x0F) | ((value & 0x0F) << 4));
		}

		//-------------------------------------------------------------------------------
		// タイルが全面透明（色番号 0 のみ）か判定する処理
		//-------------------------------------------------------------------------------
		public static bool IsBlankTile(byte[] tile)
		{
			for (int i = 0; i < tile.Length; i++)
			{
				if (tile[i] != 0)
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// タイル内容を辞書のキーにする処理
		//-------------------------------------------------------------------------------
		private static string TileKey(byte[] tile)
		{
			return Convert.ToBase64String(tile);
		}

		//-------------------------------------------------------------------------------
		// 画像をタイルとブロックに分け、タイル番号・反転・ブロック番号を割り当てる処理
		//-------------------------------------------------------------------------------
		private static void AssignTilesAndBlocks(byte[,] indices, Options options, TargetState target, Plan plan)
		{
			TileImportFormat format = target.Format;
			int tileBase = format.TileBase(options.IsSecondary);
			// 通し番号 → 使うときの反転（0: なし, 1: 左右, 2: 上下, 3: 両方）を引くための辞書
			Dictionary<string, KeyValuePair<int, int>> known = new Dictionary<string, KeyValuePair<int, int>>();
			// 既存タイルを登録する（同じ絵なら新しく書かずに再利用する）
			if (options.ReuseExistingTiles)
			{
				foreach (KeyValuePair<int, byte[]> pair in target.ReusableTiles)
				{
					RegisterTile(known, pair.Value, pair.Key, options.DedupeFlipped, false);
				}
			}
			// 透明タイルとして使う番号は、新しいタイルの置き場所にしない（上書きすると透明でなくなるため）
			List<int> freeSlots = target.FreeTileSlots.Where(local => tileBase + local != target.BlankTileId).ToList();
			Queue<int> freeTiles = new Queue<int>(freeSlots);
			int nextAppend = target.TileImage.Length / TileBytes;
			int blankId = target.BlankTileId;
			plan.Target = target;
			plan.CellBlockIndex = new int[plan.CellColumns, plan.CellRows];
			for (int y = 0; y < plan.CellRows; y++)
			{
				for (int x = 0; x < plan.CellColumns; x++)
				{
					plan.CellBlockIndex[x, y] = -1;
				}
			}
			Dictionary<string, int> cellToBlock = new Dictionary<string, int>();
			plan.AvailableTileSlots = freeSlots.Count + Math.Max(0, target.TileCapacity - nextAppend);
			plan.AvailableBlockSlots = target.FreeBlockIds.Count(id => !target.ProtectedBlockIds.Contains(id));

			// パレット枠は、取り込み先のタイルセットが持つ枠だけを使える
			if (options.PaletteSlot < format.PaletteFirst(options.IsSecondary) || options.PaletteSlot >= format.PaletteEndExclusive(options.IsSecondary))
			{
				plan.Errors.Add(string.Format(Localizer.T("パレット枠 {0} は取り込み先のタイルセットの枠ではありません（使えるのは {1}〜{2}）。"), options.PaletteSlot, format.PaletteFirst(options.IsSecondary), format.PaletteEndExclusive(options.IsSecondary) - 1));
			}
			// 下地を使う重ね方では、絵の層より下の層を下地ブロックから写す
			int artLayer = format.ArtLayer(options.Layer);
			int baseEntryCount = artLayer * 4;
			ushort[] baseEntries = new ushort[baseEntryCount];
			if (baseEntryCount > 0)
			{
				if (options.BaseEntries == null || options.BaseEntries.Length < baseEntryCount)
				{
					plan.Errors.Add(Localizer.T("下地ブロックのタイル指定を読めませんでした。下地ブロックの番号を選び直してください。"));
				}
				else
				{
					Array.Copy(options.BaseEntries, baseEntries, baseEntryCount);
					// 第1タイルセットへ取り込む場合、下地が第2タイルセットのタイル・パレットを使っていると、組み合わせる第2によって絵が変わる
					if (!options.IsSecondary && baseEntries.Any(e => (e & 0x3FF) >= format.PrimaryTileCount || (e >> 12) >= format.PrimaryPaletteCount))
					{
						plan.Errors.Add(Localizer.T("下地ブロックがタイルセット2のタイルかパレットを使っているため、タイルセット1への取り込みには使えません。タイルセット1のブロックを下地に選んでください。"));
					}
				}
			}
			// 絵の層より上に余る層（透明タイルで埋める）があるか
			bool needsBlankFill = artLayer < format.LayersPerBlock - 1;
			bool blankShortage = false;

			ushort palBits = (ushort)((options.PaletteSlot & 0x0F) << 12);
			HashSet<int> newTileIds = new HashSet<int>();
			int shortageTiles = 0;
			for (int cy = 0; cy < plan.CellRows; cy++)
			{
				for (int cx = 0; cx < plan.CellColumns; cx++)
				{
					byte[][] quarter = new byte[4][];
					bool allBlank = true;
					for (int q = 0; q < 4; q++)
					{
						quarter[q] = PackTile(indices, cx * 16 + (q % 2) * 8, cy * 16 + (q / 2) * 8);
						allBlank &= IsBlankTile(quarter[q]);
					}
					if (allBlank && options.SkipEmptyCells)
					{
						plan.EmptyCellCount++;
						continue;
					}
					ushort[] art = new ushort[4];
					for (int q = 0; q < 4; q++)
					{
						plan.TotalTileCount++;
						byte[] tile = quarter[q];
						if (IsBlankTile(tile) && blankId >= 0)
						{
							art[q] = (ushort)blankId;
							plan.ReusedTileCount++;
							continue;
						}
						KeyValuePair<int, int> hit;
						if (known.TryGetValue(TileKey(tile), out hit))
						{
							art[q] = (ushort)(hit.Key | (hit.Value << 10) | palBits);
							if (newTileIds.Contains(hit.Key))
							{
								if (hit.Value != 0) { plan.FlippedTileCount++; } else { plan.DuplicateTileCount++; }
							}
							else
							{
								plan.ReusedTileCount++;
							}
							continue;
						}
						// 新しいタイルとして空き枠（無ければ末尾）へ置く
						int local;
						if (freeTiles.Count > 0)
						{
							local = freeTiles.Dequeue();
						}
						else if (nextAppend < target.TileCapacity)
						{
							local = nextAppend++;
						}
						else
						{
							shortageTiles++;
							continue;
						}
						int globalId = tileBase + local;
						plan.NewTiles[local] = tile;
						newTileIds.Add(globalId);
						RegisterTile(known, tile, globalId, options.DedupeFlipped, true);
						if (IsBlankTile(tile) && blankId < 0)
						{
							blankId = globalId;
						}
						art[q] = (ushort)(globalId | palBits);
					}
					// 同じ絵（同じタイル・同じ反転）のマスは、すでに作るブロックを使い回す
					string cellKey = string.Join(",", art);
					int existing;
					if (options.DedupeBlocks && cellToBlock.TryGetValue(cellKey, out existing))
					{
						plan.DuplicateCellCount++;
						plan.Blocks[existing].Cells.Add(new Point(cx, cy));
						plan.CellBlockIndex[cx, cy] = existing;
						continue;
					}
					PlannedBlock block = new PlannedBlock { CellX = cx, CellY = cy };
					block.Cells.Add(new Point(cx, cy));
					cellToBlock[cellKey] = plan.Blocks.Count;
					plan.CellBlockIndex[cx, cy] = plan.Blocks.Count;
					// 絵の層より上に余る層は全面透明のタイルで埋める。透明タイルが無ければ新しく 1 枚作る
					if (needsBlankFill && blankId < 0 && !blankShortage)
					{
						int blankLocal;
						if (freeTiles.Count > 0)
						{
							blankLocal = freeTiles.Dequeue();
						}
						else if (nextAppend < target.TileCapacity)
						{
							blankLocal = nextAppend++;
						}
						else
						{
							blankLocal = -1;
							blankShortage = true;
						}
						if (blankLocal >= 0)
						{
							byte[] blankTile = new byte[TileBytes];
							blankId = tileBase + blankLocal;
							plan.NewTiles[blankLocal] = blankTile;
							newTileIds.Add(blankId);
							RegisterTile(known, blankTile, blankId, options.DedupeFlipped, true);
						}
					}
					// 絵の層より下は下地ブロックの同じ層、絵の層に絵、残りは透明タイル
					block.Entries = ComposeEntries(format, options.Layer, art, baseEntries, (ushort)Math.Max(0, blankId));
					plan.Blocks.Add(block);
					plan.RequiredBlocks++;
				}
			}
			plan.RequiredNewTiles = plan.NewTiles.Count + shortageTiles;
			if (shortageTiles > 0)
			{
				plan.Errors.Add(string.Format(Localizer.T("タイルの空きが足りません（あと {0} 枚必要）。画像を小さくするか、反転の省略・既存タイルの再利用を有効にしてください。"), shortageTiles));
			}
			// ブロックの番号は、まず空きへ自動で割り当てる（足りない分は「配置」の手順で決める）
			plan.AutoAssign();
			if (plan.UnassignedCount > 0)
			{
				plan.Notes.Add(string.Format(Localizer.T("ブロックの空きが {0} 個足りません。次の「配置」で、使用中のブロックへの差し替え先を選んでください。"), plan.UnassignedCount));
			}
			if (blankShortage)
			{
				plan.Errors.Add(Localizer.T("全面透明のタイルが無く、新しく作る空きもありません。絵の無い層を埋められないため、画像を小さくするか空きタイルを増やしてください。"));
			}
		}

		//-------------------------------------------------------------------------------
		// 1 ブロック分のタイル指定を組み立てる処理
		// 絵の層より下の層は下地から写し、絵の層に絵（4 枠）、残りの層は透明タイルにする
		//-------------------------------------------------------------------------------
		public static ushort[] ComposeEntries(TileImportFormat format, LayerStyle style, ushort[] art, ushort[] baseEntries, ushort blankTileId)
		{
			int artLayer = format.ArtLayer(style);
			int baseEntryCount = artLayer * 4;
			ushort[] entries = Enumerable.Repeat(blankTileId, format.EntriesPerBlock).ToArray();
			if (baseEntryCount > 0 && baseEntries != null)
			{
				Array.Copy(baseEntries, 0, entries, 0, Math.Min(baseEntryCount, baseEntries.Length));
			}
			Array.Copy(art, 0, entries, artLayer * 4, 4);
			return entries;
		}

		//-------------------------------------------------------------------------------
		// 1 ブロック分の挙動データのうち、レイヤーの値だけを重ね方に合わせて置き換えた内容を返す処理
		// 2 バイト形式は +1 の上位 4 ビット、4 バイト形式は +3 の上位 6 ビットだけを変え、ほか（野生の出現など）は保つ
		//-------------------------------------------------------------------------------
		public static byte[] BuildBehaviorPatch(byte[] current, TileImportFormat format, LayerStyle style)
		{
			if (current == null || current.Length != format.BehaviorBytes)
			{
				throw new ArgumentException("Behavior data length does not match the format.", nameof(current));
			}
			// 2 層のブロックで「プレイヤーの下」に描くときだけ、レイヤーの値を立てる（FR は 0x20、エメラルドは 0x10）
			byte layerValue = 0;
			if (format.BlockBytes == 16 && style == LayerStyle.OverBaseBelowPlayer)
			{
				layerValue = format.BehaviorBytes == 4 ? (byte)0x20 : (byte)0x10;
			}
			byte[] result = (byte[])current.Clone();
			if (format.BehaviorBytes == 2)
			{
				result[1] = (byte)((result[1] & 0x0F) | layerValue);
			}
			else if (format.BehaviorBytes == 4)
			{
				result[3] = (byte)((result[3] & 0x03) | layerValue);
			}
			else
			{
				throw new NotSupportedException("Unsupported behavior format.");
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 計画を反映したブロック（16x16）の絵を作る処理
		// 新しいタイルと、上書きするパレットを反映して描く（下層→中層→上層、色 0 は透明）
		// layerMask で描く層を選べる（ビット 0 が下層。既定は全層。重ね方の見本・下地の選択で使う）
		//-------------------------------------------------------------------------------
		public static Bitmap RenderBlock(ushort[] entries, Plan plan, Options options, int layerMask = -1)
		{
			Bitmap bmp = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
			TargetState target = plan.Target;
			if (target == null || target.Format == null || entries == null)
			{
				return bmp;
			}
			int tileBase = target.Format.TileBase(options.IsSecondary);
			Color[] palettes = (Color[])target.AllPalettes.Clone();
			if (options.Mode == PaletteMode.Overwrite && options.PaletteSlot >= 0 && (options.PaletteSlot + 1) * 16 <= palettes.Length)
			{
				for (int i = 1; i < 16; i++)
				{
					palettes[options.PaletteSlot * 16 + i] = plan.Palette[i];
				}
			}
			int layers = Math.Min(target.Format.LayersPerBlock, entries.Length / 4);
			for (int layer = 0; layer < layers; layer++)
			{
				if ((layerMask & (1 << layer)) == 0)
				{
					continue;
				}
				for (int q = 0; q < 4; q++)
				{
					ushort entry = entries[layer * 4 + q];
					int tileId = entry & 0x3FF;
					bool flipH = (entry & 0x400) != 0;
					bool flipV = (entry & 0x800) != 0;
					int pal = entry >> 12;
					byte[] tile;
					if (tileId >= tileBase && plan.NewTiles.TryGetValue(tileId - tileBase, out tile))
					{
						// 今回書き込む新しいタイル
					}
					else
					{
						tile = new byte[TileBytes];
						int offset = tileId * TileBytes;
						if (offset + TileBytes <= target.CombinedTiles.Length)
						{
							Array.Copy(target.CombinedTiles, offset, tile, 0, TileBytes);
						}
					}
					for (int y = 0; y < 8; y++)
					{
						for (int x = 0; x < 8; x++)
						{
							int sx = flipH ? 7 - x : x;
							int sy = flipV ? 7 - y : y;
							int index = GetTilePixel(tile, sx, sy);
							if (index == 0)
							{
								continue;
							}
							Color c = palettes[(pal * 16 + index) % palettes.Length];
							bmp.SetPixel((q % 2) * 8 + x, (q / 2) * 8 + y, Color.FromArgb(255, c.R, c.G, c.B));
						}
					}
				}
			}
			return bmp;
		}

		//-------------------------------------------------------------------------------
		// タイル（と必要なら反転した形）を「既知のタイル」として登録する処理
		//-------------------------------------------------------------------------------
		private static void RegisterTile(Dictionary<string, KeyValuePair<int, int>> known, byte[] tile, int globalId, bool withFlips, bool overwrite)
		{
			AddKnown(known, TileKey(tile), globalId, 0, overwrite);
			if (!withFlips)
			{
				return;
			}
			byte[] h = FlipTileH(tile);
			byte[] v = FlipTileV(tile);
			AddKnown(known, TileKey(h), globalId, 1, false);
			AddKnown(known, TileKey(v), globalId, 2, false);
			AddKnown(known, TileKey(FlipTileV(h)), globalId, 3, false);
		}

		//-------------------------------------------------------------------------------
		// 既知タイルの辞書へ 1 件追加する処理（先に登録された方を優先する）
		//-------------------------------------------------------------------------------
		private static void AddKnown(Dictionary<string, KeyValuePair<int, int>> known, string key, int globalId, int flip, bool overwrite)
		{
			if (overwrite || !known.ContainsKey(key))
			{
				known[key] = new KeyValuePair<int, int>(globalId, flip);
			}
		}
	}
}
