using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ROM のタウンマップ（地方ごとの絵と、マスごとの場所番号の表）を読み込んで持つクラス
	//-------------------------------------------------------------------------------
	internal sealed class TownMapData
	{
		private const int AutoOffset = int.MinValue;

		// ゲームごとのタウンマップの形
		public int RegionCount { get; private set; }
		public int GridWidth { get; private set; }
		public int GridHeight { get; private set; }
		public int LayerCount { get; private set; }
		public int CellSize { get; private set; }
		public int GridOriginX { get; private set; }
		public int GridOriginY { get; private set; }
		public int ImageWidth { get; private set; }
		public int ImageHeight { get; private set; }
		public Rectangle ViewArea { get; private set; }
		public string[] RegionNames { get; private set; }

		// 読み込んだ元の ROM（別の ROM を開いたら作り直すための目印）
		public byte[] Rom { get; private set; }

		// 地方ごとの絵（読めなかった地方は null）
		public Bitmap[] Images { get; private set; }

		// 地方ごとの場所の表（層 × 行 × 列 の順。読めなかった地方は null）
		private byte[][] sections;

		// どの場所にも当たらないマスの値
		public byte NoneSection { get; private set; }

		//-------------------------------------------------------------------------------
		// ROM からタウンマップを読み込む処理（ini に定義が無い・データが読めないときは null）
		//-------------------------------------------------------------------------------
		public static TownMapData Load(byte[] rom)
		{
			if (rom == null)
			{
				return null;
			}
			try
			{
				return GameProfile.Current.EmeraldRegionMap ? LoadEmerald(rom) : LoadFireRed(rom);
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// ファイアレッド形のタウンマップを読み込む処理
		//-------------------------------------------------------------------------------
		private static TownMapData LoadFireRed(byte[] rom)
		{
			int gfxOffset = ReadOffset(rom, "REGION_MAP_GFX_OFFSET");
			int paletteOffset = ReadOffset(rom, "REGION_MAP_PALETTE_OFFSET");
			byte[] gfx = Lz77Decompress(rom, gfxOffset);
			if (gfx == null || paletteOffset < 0)
			{
				return null;
			}
			TownMapData data = new TownMapData
			{
				RegionCount = 4,
				GridWidth = 22,
				GridHeight = 15,
				LayerCount = 2,
				CellSize = 8,
				GridOriginX = 32,
				GridOriginY = 32,
				ImageWidth = 240,
				ImageHeight = 160,
				ViewArea = new Rectangle(24, 16, 192, 144),
				RegionNames = new[] { "カントー", "1・2・3のしま", "4・5のしま", "6・7のしま" },
				Rom = rom,
				Images = new Bitmap[4],
				sections = new byte[4][],
				NoneSection = (byte)ReadOffset(rom, "REGION_MAP_SECTION_NONE", 0xC5)
			};
			int sectionLength = data.GridWidth * data.GridHeight * data.LayerCount;
			for (int region = 0; region < data.RegionCount; region++)
			{
				int tilemapOffset = ReadOffset(rom, "REGION_MAP_TILEMAP_OFFSET_" + region);
				byte[] tilemap = Lz77Decompress(rom, tilemapOffset);
				if (tilemap != null && tilemap.Length >= data.ImageWidth / 8 * (data.ImageHeight / 8) * 2)
				{
					data.Images[region] = RenderRegion(rom, gfx, paletteOffset, tilemap, data.ImageWidth, data.ImageHeight);
				}
				int sectionOffset = ReadOffset(rom, "REGION_MAP_SECTION_OFFSET_" + region);
				if (sectionOffset >= 0 && sectionOffset + sectionLength <= rom.Length)
				{
					data.sections[region] = new byte[sectionLength];
					Array.Copy(rom, sectionOffset, data.sections[region], 0, sectionLength);
				}
			}
			return data.Images[0] != null && data.sections[0] != null ? data : null;
		}

		//-------------------------------------------------------------------------------
		// エメラルド形のタウンマップを読み込む処理
		//-------------------------------------------------------------------------------
		private static TownMapData LoadEmerald(byte[] rom)
		{
			int gfxOffset = ReadOffset(rom, "REGION_MAP_GFX_OFFSET");
			int gfxEnd;
			byte[] gfx = Lz77Decompress(rom, gfxOffset, out gfxEnd);
			if (gfx == null)
			{
				return null;
			}

			int tilemapOffset = ReadOffset(rom, "REGION_MAP_TILEMAP_OFFSET_0");
			if (tilemapOffset == AutoOffset)
			{
				tilemapOffset = DetectEmeraldTilemap(rom, gfxEnd);
			}
			byte[] tilemap = Lz77Decompress(rom, tilemapOffset);
			if (tilemap == null || tilemap.Length != 0x1000)
			{
				return null;
			}

			int paletteOffset = ReadOffset(rom, "REGION_MAP_PALETTE_OFFSET");
			if (paletteOffset == AutoOffset)
			{
				paletteOffset = gfxOffset - 0x40;
			}
			if (paletteOffset < 0 || paletteOffset + 0x40 > rom.Length)
			{
				return null;
			}
			int paletteBase = ReadOffset(rom, "REGION_MAP_PALETTE_BASE", 0x70);

			TownMapData data = new TownMapData
			{
				RegionCount = 1,
				GridWidth = 28,
				GridHeight = 15,
				LayerCount = 1,
				CellSize = 8,
				GridOriginX = 8,
				GridOriginY = 16,
				ImageWidth = 240,
				ImageHeight = 160,
				ViewArea = new Rectangle(0, 0, 240, 160),
				RegionNames = new[] { "ホウエン" },
				Rom = rom,
				Images = new Bitmap[1],
				sections = new byte[1][]
			};
			data.Images[0] = RenderEmeraldRegion(rom, gfx, paletteOffset, paletteBase, tilemap);

			int[] expected = BuildEmeraldSectionLayout(rom, data.GridWidth, data.GridHeight);
			if (expected == null)
			{
				return null;
			}
			int sectionOffset = ReadOffset(rom, "REGION_MAP_SECTION_OFFSET_0");
			byte automaticNone = (byte)MapEditor.MAP_NAME_COUNT;
			if (sectionOffset == AutoOffset)
			{
				data.sections[0] = DetectSectionLayout(rom, expected, out automaticNone);
			}
			else if (sectionOffset >= 0 && sectionOffset + expected.Length <= rom.Length)
			{
				data.sections[0] = new byte[expected.Length];
				Array.Copy(rom, sectionOffset, data.sections[0], 0, expected.Length);
				automaticNone = FindMostCommonValue(data.sections[0]);
			}
			if (data.sections[0] == null)
			{
				data.sections[0] = MakeSectionLayoutFromRectangles(expected, automaticNone);
			}

			int noneSection = ReadOffset(rom, "REGION_MAP_SECTION_NONE", AutoOffset);
			data.NoneSection = noneSection == AutoOffset ? automaticNone : (byte)noneSection;
			return data.Images[0] != null ? data : null;
		}

		//-------------------------------------------------------------------------------
		// 絵の LZ の直後から、展開サイズが 0x1000 の最初の LZ を探す処理
		//-------------------------------------------------------------------------------
		private static int DetectEmeraldTilemap(byte[] rom, int gfxEnd)
		{
			int start = (gfxEnd + 3) & ~3;
			int end = Math.Min(rom.Length - 4, start + 0x100);
			for (int offset = start; offset <= end; offset += 4)
			{
				if (rom[offset] == 0x10 && (rom[offset + 1] | (rom[offset + 2] << 8) | (rom[offset + 3] << 16)) == 0x1000)
				{
					byte[] tilemap = Lz77Decompress(rom, offset);
					if (tilemap != null && tilemap.Length == 0x1000)
					{
						return offset;
					}
				}
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// gRegionMapEntries の長方形から、場所の表の期待値を作る処理
		//-------------------------------------------------------------------------------
		private static int[] BuildEmeraldSectionLayout(byte[] rom, int width, int height)
		{
			int count = MapEditor.MAP_NAME_COUNT;
			int entrySize = MapEditor.MAP_NAME_ENTRY_SIZE;
			int tableOffset = MapEditor.MAP_NAME_TABLE_OFFSET;
			long tableEnd = (long)tableOffset + count * (long)entrySize;
			if (count <= 0 || count > byte.MaxValue || entrySize < 4 || tableOffset < 0 || tableEnd > rom.Length)
			{
				return null;
			}
			int[] expected = new int[width * height];
			for (int i = 0; i < expected.Length; i++)
			{
				expected[i] = -1;
			}
			for (int i = 0; i < count; i++)
			{
				int entry = tableOffset + i * entrySize;
				int left = rom[entry];
				int top = rom[entry + 1];
				int right = Math.Min(width, left + rom[entry + 2]);
				int bottom = Math.Min(height, top + rom[entry + 3]);
				for (int y = top; y < bottom; y++)
				{
					for (int x = left; x < right; x++)
					{
						int cell = y * width + x;
						if (expected[cell] < 0)
						{
							expected[cell] = i;
						}
					}
				}
			}
			return expected;
		}

		//-------------------------------------------------------------------------------
		// ROM 内から期待値に最も近い 28×15 の場所の表を探す処理（見つからなければ null）
		//-------------------------------------------------------------------------------
		private static byte[] DetectSectionLayout(byte[] rom, int[] expected, out byte none)
		{
			none = (byte)MapEditor.MAP_NAME_COUNT;
			int bestScore;
			int nearStart = Math.Max(0, MapEditor.MAP_NAME_TABLE_OFFSET - 0x40000);
			int nearEnd = Math.Min(rom.Length - expected.Length, MapEditor.MAP_NAME_TABLE_OFFSET + 0x40000);
			int offset = FindSectionLayout(rom, expected, nearStart, nearEnd, out bestScore);
			if (bestScore < 380)
			{
				offset = FindSectionLayout(rom, expected, 0, rom.Length - expected.Length, out bestScore);
			}
			if (offset < 0 || bestScore < 380)
			{
				return null;
			}
			byte[] result = new byte[expected.Length];
			Array.Copy(rom, offset, result, 0, result.Length);
			none = FindMostCommonValue(result);
			return result;
		}

		//-------------------------------------------------------------------------------
		// 指定範囲を 4 バイト刻みで調べ、場所の表との一致数が最大の位置を返す処理
		//-------------------------------------------------------------------------------
		private static int FindSectionLayout(byte[] rom, int[] expected, int start, int end, out int bestScore)
		{
			int count = MapEditor.MAP_NAME_COUNT;
			int bestOffset = -1;
			bestScore = -1;
			start = (Math.Max(0, start) + 3) & ~3;
			end = Math.Min(end, rom.Length - expected.Length);
			for (int offset = start; offset <= end; offset += 4)
			{
				int firstRowScore = 0;
				for (int i = 0; i < 28; i++)
				{
					if (expected[i] < 0 ? rom[offset + i] >= count : rom[offset + i] == expected[i])
					{
						firstRowScore++;
					}
				}
				if (firstRowScore < 24)
				{
					continue;
				}
				int score = firstRowScore;
				for (int i = 28; i < expected.Length; i++)
				{
					if (expected[i] < 0 ? rom[offset + i] >= count : rom[offset + i] == expected[i])
					{
						score++;
					}
				}
				if (score > bestScore)
				{
					bestScore = score;
					bestOffset = offset;
				}
			}
			return bestOffset;
		}

		//-------------------------------------------------------------------------------
		// 長方形から作った期待値を、場所の表として使えるバイト列にする処理
		//-------------------------------------------------------------------------------
		private static byte[] MakeSectionLayoutFromRectangles(int[] expected, byte none)
		{
			byte[] result = new byte[expected.Length];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = expected[i] >= 0 ? (byte)expected[i] : none;
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 場所の表で最も多い値（「場所なし」）を返す処理
		//-------------------------------------------------------------------------------
		private static byte FindMostCommonValue(byte[] table)
		{
			int[] counts = new int[256];
			foreach (byte value in table)
			{
				counts[value]++;
			}
			int best = 0;
			for (int i = 1; i < counts.Length; i++)
			{
				if (counts[i] > counts[best])
				{
					best = i;
				}
			}
			return (byte)best;
		}

		//-------------------------------------------------------------------------------
		// 地方 region・層 layer のマス (x, y) の場所番号を返す処理（表が無ければ NoneSection）
		//-------------------------------------------------------------------------------
		public byte GetSection(int region, int layer, int x, int y)
		{
			byte[] table = (region >= 0 && region < this.RegionCount) ? this.sections[region] : null;
			if (table == null || layer < 0 || layer >= this.LayerCount || x < 0 || y < 0 || x >= this.GridWidth || y >= this.GridHeight)
			{
				return this.NoneSection;
			}
			return table[layer * this.GridWidth * this.GridHeight + y * this.GridWidth + x];
		}

		//-------------------------------------------------------------------------------
		// 場所番号 section が載っている地方とマスを探す処理（地上の層を先に見て、無ければダンジョンの層）
		// 見つかれば true
		//-------------------------------------------------------------------------------
		public bool FindSection(byte section, out int region, out List<Point> cells)
		{
			region = -1;
			cells = new List<Point>();
			if (section == this.NoneSection)
			{
				return false;
			}
			for (int r = 0; r < this.RegionCount; r++)
			{
				for (int layer = 0; layer < this.LayerCount; layer++)
				{
					for (int y = 0; y < this.GridHeight; y++)
					{
						for (int x = 0; x < this.GridWidth; x++)
						{
							if (this.GetSection(r, layer, x, y) == section && !cells.Contains(new Point(x, y)))
							{
								cells.Add(new Point(x, y));
							}
						}
					}
				}
				if (cells.Count > 0)
				{
					region = r;
					return true;
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// ini の値を読み、自動検出・ポインタ・目印・数値を読み分ける処理（読めなければ fallback）
		//-------------------------------------------------------------------------------
		private static int ReadOffset(byte[] rom, string key, int fallback = -1)
		{
			string text = RomIniReader.ReadValue(key);
			if (string.IsNullOrWhiteSpace(text))
			{
				return fallback;
			}
			if (string.Equals(text.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
			{
				return AutoOffset;
			}
			try
			{
				return RomIniReader.ReadHexOrDecimal(key);
			}
			catch (Exception)
			{
				return fallback;
			}
		}

		//-------------------------------------------------------------------------------
		// GBA の LZ77（先頭 0x10）を展開する処理（壊れていれば null）
		//-------------------------------------------------------------------------------
		private static byte[] Lz77Decompress(byte[] rom, int offset)
		{
			int endOffset;
			return Lz77Decompress(rom, offset, out endOffset);
		}

		//-------------------------------------------------------------------------------
		// GBA の LZ77 を展開し、圧縮データを読み終えた位置も返す処理
		//-------------------------------------------------------------------------------
		private static byte[] Lz77Decompress(byte[] rom, int offset, out int endOffset)
		{
			endOffset = -1;
			if (offset < 0 || offset + 4 > rom.Length || rom[offset] != 0x10)
			{
				return null;
			}
			int size = rom[offset + 1] | (rom[offset + 2] << 8) | (rom[offset + 3] << 16);
			if (size <= 0 || size > 0x40000)
			{
				return null;
			}
			byte[] output = new byte[size];
			int written = 0;
			int position = offset + 4;
			while (written < size)
			{
				if (position >= rom.Length)
				{
					return null;
				}
				byte flags = rom[position++];
				for (int bit = 0; bit < 8 && written < size; bit++)
				{
					if ((flags & (0x80 >> bit)) != 0)
					{
						if (position + 1 >= rom.Length)
						{
							return null;
						}
						int value = (rom[position] << 8) | rom[position + 1];
						position += 2;
						int length = (value >> 12) + 3;
						int distance = (value & 0xFFF) + 1;
						if (distance > written)
						{
							return null;
						}
						for (int i = 0; i < length && written < size; i++)
						{
							output[written] = output[written - distance];
							written++;
						}
					}
					else
					{
						if (position >= rom.Length)
						{
							return null;
						}
						output[written++] = rom[position++];
					}
				}
			}
			endOffset = position;
			return output;
		}

		//-------------------------------------------------------------------------------
		// 4bpp の絵のかけら・パレット・並べ方の表から、FR の地方の絵を作る処理
		//-------------------------------------------------------------------------------
		private static Bitmap RenderRegion(byte[] rom, byte[] gfx, int paletteOffset, byte[] tilemap, int imageWidth, int imageHeight)
		{
			int tilemapWidth = imageWidth / 8;
			int tilemapHeight = imageHeight / 8;
			// 並べ方の表が使うパレットの数だけ色を読む（1 本 16 色）
			int paletteCount = 1;
			for (int i = 0; i < tilemapWidth * tilemapHeight; i++)
			{
				paletteCount = Math.Max(paletteCount, (BitConverter.ToUInt16(tilemap, i * 2) >> 12) + 1);
			}
			int[] colors = new int[paletteCount * 16];
			for (int i = 0; i < colors.Length; i++)
			{
				int address = paletteOffset + i * 2;
				int color = address + 1 < rom.Length ? BitConverter.ToUInt16(rom, address) : 0;
				colors[i] = ToArgb(color);
			}
			int[] pixels = new int[imageWidth * imageHeight];
			for (int ty = 0; ty < tilemapHeight; ty++)
			{
				for (int tx = 0; tx < tilemapWidth; tx++)
				{
					int entry = BitConverter.ToUInt16(tilemap, (ty * tilemapWidth + tx) * 2);
					int tile = entry & 0x3FF;
					bool flipX = (entry & 0x400) != 0;
					bool flipY = (entry & 0x800) != 0;
					int palette = entry >> 12;
					for (int y = 0; y < 8; y++)
					{
						for (int x = 0; x < 8; x++)
						{
							int sx = flipX ? 7 - x : x;
							int sy = flipY ? 7 - y : y;
							int index = tile * 32 + sy * 4 + sx / 2;
							int value = index < gfx.Length ? ((sx & 1) != 0 ? gfx[index] >> 4 : gfx[index] & 0xF) : 0;
							pixels[(ty * 8 + y) * imageWidth + tx * 8 + x] = colors[palette * 16 + value];
						}
					}
				}
			}
			return MakeBitmap(pixels, imageWidth, imageHeight);
		}

		//-------------------------------------------------------------------------------
		// 8bpp の絵と並べ方の表から、エメラルドの地方の絵（240×160）を作る処理
		// 並べ方の表は、元のゲームではアフィン BG（64×64、1 マス 1 バイト）。改造版（Spades など）では
		// 通常の BG（1 行 32 マス、1 マス 2 バイト = タイル番号 10 ビット + 左右・上下反転）のことがある
		//-------------------------------------------------------------------------------
		private static Bitmap RenderEmeraldRegion(byte[] rom, byte[] gfx, int paletteOffset, int paletteBase, byte[] tilemap)
		{
			const int imageWidth = 240;
			const int imageHeight = 160;
			int[] colors = new int[32];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = ToArgb(BitConverter.ToUInt16(rom, paletteOffset + i * 2));
			}
			int[] pixels = new int[imageWidth * imageHeight];
			int black = unchecked((int)0xFF000000);
			bool wideEntries = IsSixteenBitTilemap(tilemap);
			for (int ty = 0; ty < 20; ty++)
			{
				for (int tx = 0; tx < 30; tx++)
				{
					int tile;
					bool flipX = false;
					bool flipY = false;
					if (wideEntries)
					{
						int entry = BitConverter.ToUInt16(tilemap, (ty * 32 + tx) * 2);
						tile = entry & 0x3FF;
						flipX = (entry & 0x400) != 0;
						flipY = (entry & 0x800) != 0;
					}
					else
					{
						tile = tilemap[ty * 64 + tx];
					}
					for (int y = 0; y < 8; y++)
					{
						for (int x = 0; x < 8; x++)
						{
							int gfxIndex = tile * 64 + (flipY ? 7 - y : y) * 8 + (flipX ? 7 - x : x);
							int value = gfxIndex < gfx.Length ? gfx[gfxIndex] : 0;
							int colorIndex = value - paletteBase;
							pixels[(ty * 8 + y) * imageWidth + tx * 8 + x] = value != 0 && colorIndex >= 0 && colorIndex < colors.Length ? colors[colorIndex] : black;
						}
					}
				}
			}
			return MakeBitmap(pixels, imageWidth, imageHeight);
		}

		//-------------------------------------------------------------------------------
		// 並べ方の表が 1 マス 2 バイトの形かを調べる処理
		// 2 バイトの形なら、奇数番目のバイトはタイル番号の上位 2 ビットと反転の 2 ビットだけ（0x0F 以下）になる
		// 1 バイトの形（元のゲーム）は、奇数番目にもタイル番号（0x10 以上を含む）が入る
		//-------------------------------------------------------------------------------
		private static bool IsSixteenBitTilemap(byte[] tilemap)
		{
			for (int i = 1; i < tilemap.Length; i += 2)
			{
				if ((tilemap[i] & 0xF0) != 0)
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// GBA の 15 ビット色を 32 ビット ARGB に変換する処理
		//-------------------------------------------------------------------------------
		private static int ToArgb(int color)
		{
			int red = (color & 0x1F) * 255 / 31;
			int green = ((color >> 5) & 0x1F) * 255 / 31;
			int blue = ((color >> 10) & 0x1F) * 255 / 31;
			return unchecked((int)0xFF000000) | (red << 16) | (green << 8) | blue;
		}

		//-------------------------------------------------------------------------------
		// 32 ビットのピクセル列から Bitmap を作る処理
		//-------------------------------------------------------------------------------
		private static Bitmap MakeBitmap(int[] pixels, int width, int height)
		{
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			BitmapData locked = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			try
			{
				for (int y = 0; y < height; y++)
				{
					Marshal.Copy(pixels, y * width, locked.Scan0 + y * locked.Stride, width);
				}
			}
			finally
			{
				bitmap.UnlockBits(locked);
			}
			return bitmap;
		}
	}
}
