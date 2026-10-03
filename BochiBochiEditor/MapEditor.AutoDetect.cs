using System;
using System.Collections.Generic;
using System.Linq;

namespace BochiBochiEditor
{
	public partial class MapEditor
	{
		public static int TILESET_HEADER_SIZE = 24;
		public static int MAP_NAME_ENTRY_SIZE = 4;
		public static int MAP_NAME_POINTER_OFFSET = 0;
		// ブロック 1 つのバイト数（2 層 × 4 タイル = 16。pokeemerald-expansion の 3 層ブロックの改造版は 24）
		public static int BLOCK_DATA_SIZE = 16;
		// 人物の絵の番号が 2 バイト（+2〜+3）か（pokeemerald-expansion の改造版。元のゲームは 1 バイト（+1））
		public static bool OBJECT_EVENT_GFX_16BIT = false;

		//-------------------------------------------------------------------------------
		// 定義ファイルの数値・目印・自動検出を読み分け、未指定なら既定値を使う処理
		//-------------------------------------------------------------------------------
		private static int ReadIniOffset(string key, Func<int> auto, int? fallbackWhenMissing)
		{
			string value = RomIniReader.ReadValue(key);
			if (string.IsNullOrWhiteSpace(value) && fallbackWhenMissing.HasValue)
			{
				return fallbackWhenMissing.Value;
			}
			if (string.Equals(value?.Trim(), "auto", StringComparison.OrdinalIgnoreCase) && auto != null)
			{
				return auto();
			}
			// 目印のバイト列で探す書き方で、目印が見つからない（プログラムの並びが違う decomp ベースの ROM など）か、
			// 求めた場所が ROM の外のときは、データの形から求める
			if (auto != null && value != null && value.TrimStart().StartsWith("\""))
			{
				try
				{
					int found = RomIniReader.ReadHexOrDecimal(key);
					if (found > 0 && MainForm.romData != null && found < MainForm.romData.Length)
					{
						return found;
					}
				}
				catch (Exception)
				{
				}
				return auto();
			}
			return RomIniReader.ReadHexOrDecimal(key);
		}

		//-------------------------------------------------------------------------------
		// 自動検出で使う読み取り範囲と GBA ポインタの有効性を調べる処理
		//-------------------------------------------------------------------------------
		private static bool CanReadAutoData(byte[] rom, long offset, int length)
		{
			return rom != null && offset >= 0 && length >= 0 && offset + length <= rom.Length;
		}

		private static bool IsAutoRomPointer(byte[] rom, uint pointer)
		{
			return rom != null && pointer >= 0x08000000u && (ulong)pointer < 0x08000000UL + (ulong)rom.Length;
		}

		//-------------------------------------------------------------------------------
		// 地形表の空項目または有効ポインタが続く件数を数える処理
		//-------------------------------------------------------------------------------
		private static int DetectTerrainIdCount()
		{
			byte[] rom = MainForm.romData;
			int count = 0;
			while (count < 4096)
			{
				long offset = (long)MAP_TERRAIN_ID_TABLE_OFFSET + count * 4L;
				if (!CanReadAutoData(rom, offset, 4)) break;
				uint pointer = BitConverter.ToUInt32(rom, (int)offset);
				if (pointer != 0 && !IsAutoRomPointer(rom, pointer)) break;
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 地形データの表の件数を、空の欄（0）か「地形データらしい場所（幅・高さが 1〜255）を指すポインタ」が続く数で数える処理
		// 表の後ろに別のポインタの表が続いていても、そこで止まる（設定ファイルの件数より表が長い ROM 用）
		//-------------------------------------------------------------------------------
		private static int DetectTerrainIdCountStrict()
		{
			byte[] rom = MainForm.romData;
			int count = 0;
			if (rom == null || MAP_TERRAIN_ID_TABLE_OFFSET <= 0)
			{
				return 0;
			}
			while (count < 4096)
			{
				long offset = (long)MAP_TERRAIN_ID_TABLE_OFFSET + count * 4L;
				if (!CanReadAutoData(rom, offset, 4)) break;
				uint pointer = BitConverter.ToUInt32(rom, (int)offset);
				if (pointer != 0)
				{
					if (!IsAutoRomPointer(rom, pointer) || !CanReadAutoData(rom, pointer - 0x08000000u, 8)) break;
					uint width = BitConverter.ToUInt32(rom, (int)(pointer - 0x08000000u));
					uint height = BitConverter.ToUInt32(rom, (int)(pointer - 0x08000000u) + 4);
					if (width < 1 || width > 255 || height < 1 || height > 255) break;
				}
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 人物の絵の番号が 2 バイトの形かを調べる処理（1 なら 2 バイト、0 なら 1 バイト）
		// 先頭側のマップの人物を最大 300 件見て、「+1 が 0 で +2 が 0 以外」の件数が「+1 が 0 以外」の件数より多ければ 2 バイトとする
		//-------------------------------------------------------------------------------
		private static int DetectObjectEventGfx16()
		{
			byte[] rom = MainForm.romData;
			int wide = 0;
			int narrow = 0;
			int sampled = 0;
			for (int bank = 0; bank < 64 && sampled < 300; bank++)
			{
				long bankEntry = (long)MAP_BANK_TABLE_OFFSET + bank * 4L;
				if (!CanReadAutoData(rom, bankEntry, 4)) break;
				uint bankPointer = BitConverter.ToUInt32(rom, (int)bankEntry);
				if (!IsAutoRomPointer(rom, bankPointer)) break;
				for (int map = 0; map < 64 && sampled < 300; map++)
				{
					long mapEntry = (long)bankPointer - 0x08000000u + map * 4L;
					if (!CanReadAutoData(rom, mapEntry, 4)) break;
					uint header = BitConverter.ToUInt32(rom, (int)mapEntry);
					if (!IsAutoRomPointer(rom, header) || !CanReadAutoData(rom, header - 0x08000000u, 8)) break;
					uint events = BitConverter.ToUInt32(rom, (int)(header - 0x08000000u) + 4);
					if (!IsAutoRomPointer(rom, events) || !CanReadAutoData(rom, events - 0x08000000u, 8)) continue;
					int count = rom[events - 0x08000000u];
					uint objects = BitConverter.ToUInt32(rom, (int)(events - 0x08000000u) + 4);
					if (count == 0 || count > 64 || !IsAutoRomPointer(rom, objects) || !CanReadAutoData(rom, objects - 0x08000000u, count * 24)) continue;
					for (int i = 0; i < count && sampled < 300; i++, sampled++)
					{
						int entry = (int)(objects - 0x08000000u) + i * 24;
						if (rom[entry + 1] != 0) narrow++;
						else if (rom[entry + 2] != 0) wide++;
					}
				}
			}
			return wide > narrow ? 1 : 0;
		}

		//-------------------------------------------------------------------------------
		// ブロック 1 つのバイト数を調べる処理
		// 最初の有効なマップの第1タイルセットで、ブロック表から挙動表までの長さが「第1のブロック数 × 24」なら 24、それ以外は 16
		//-------------------------------------------------------------------------------
		private static int DetectBlockDataSize()
		{
			byte[] rom = MainForm.romData;
			for (int i = 0; i < MAP_TERRAIN_ID_COUNT; i++)
			{
				long entry = (long)MAP_TERRAIN_ID_TABLE_OFFSET + i * 4L;
				if (!CanReadAutoData(rom, entry, 4)) break;
				uint layout = BitConverter.ToUInt32(rom, (int)entry);
				if (!IsAutoRomPointer(rom, layout) || !CanReadAutoData(rom, layout - 0x08000000u, 24)) continue;
				uint tileset = BitConverter.ToUInt32(rom, (int)(layout - 0x08000000u) + 16);
				if (!IsAutoRomPointer(rom, tileset) || !CanReadAutoData(rom, tileset - 0x08000000u, 24)) continue;
				int header = (int)(tileset - 0x08000000u);
				uint blocks = BitConverter.ToUInt32(rom, header + 12);
				uint behavior = BitConverter.ToUInt32(rom, header + GameProfile.Current.TilesetBehaviorOffset);
				if (!IsAutoRomPointer(rom, blocks) || !IsAutoRomPointer(rom, behavior)) continue;
				return (long)behavior - blocks == GameProfile.Current.PrimaryBlockCount * 24L ? 24 : 16;
			}
			return 16;
		}

		//-------------------------------------------------------------------------------
		// 地形データから参照されるタイルセット見出しの位置を重複なしの昇順で集める処理
		//-------------------------------------------------------------------------------
		private static List<int> DetectTilesetHeaderOffsets()
		{
			byte[] rom = MainForm.romData;
			HashSet<int> offsets = new HashSet<int>();
			for (int i = 0; i < MAP_TERRAIN_ID_COUNT; i++)
			{
				long entry = (long)MAP_TERRAIN_ID_TABLE_OFFSET + i * 4L;
				if (!CanReadAutoData(rom, entry, 4)) break;
				uint layout = BitConverter.ToUInt32(rom, (int)entry);
				if (!IsAutoRomPointer(rom, layout)) continue;
				long offset = layout - 0x08000000u;
				if (!CanReadAutoData(rom, offset, 24)) continue;
				foreach (int field in new[] { 16, 20 })
				{
					uint pointer = BitConverter.ToUInt32(rom, (int)offset + field);
					if (IsAutoRomPointer(rom, pointer)) offsets.Add((int)(pointer - 0x08000000u));
				}
			}
			return offsets.OrderBy(offset => offset).ToList();
		}

		private static int DetectTilesetStartOffset()
		{
			return DetectTilesetHeaderOffsets().DefaultIfEmpty(-1).First();
		}

		//-------------------------------------------------------------------------------
		// 隣接するタイルセット見出しの間隔のうち、最も多いもの（24〜64 バイト）を見出しの大きさとする処理
		// 最小の間隔では、改造版で一部だけ詰まって並んだ見出しに引きずられる（Spades は 28 バイトが大半で、24 バイトの間隔が少し混じる）
		// 妥当な間隔が無ければ 24 バイトにする。同数なら小さい方
		//-------------------------------------------------------------------------------
		private static int DetectTilesetHeaderSize()
		{
			List<int> offsets = DetectTilesetHeaderOffsets();
			Dictionary<int, int> counts = new Dictionary<int, int>();
			for (int i = 1; i < offsets.Count; i++)
			{
				int gap = offsets[i] - offsets[i - 1];
				if (gap < 24 || gap > 64 || gap % 4 != 0) continue;
				counts.TryGetValue(gap, out int count);
				counts[gap] = count + 1;
			}
			int size = 24;
			int best = 0;
			foreach (KeyValuePair<int, int> pair in counts.OrderBy(pair => pair.Key))
			{
				if (pair.Value > best)
				{
					best = pair.Value;
					size = pair.Key;
				}
			}
			return size;
		}

		//-------------------------------------------------------------------------------
		// マップ名の表で、有効な名前ポインタが続く件数を数える処理
		//-------------------------------------------------------------------------------
		private static int DetectMapNameCount()
		{
			byte[] rom = MainForm.romData;
			int count = 0;
			while (count < 256)
			{
				long entry = (long)MAP_NAME_TABLE_OFFSET + count * (long)MAP_NAME_ENTRY_SIZE + MAP_NAME_POINTER_OFFSET;
				if (!CanReadAutoData(rom, entry, 4) || !IsAutoRomPointer(rom, BitConverter.ToUInt32(rom, (int)entry))) break;
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 人物画像の見出しを指すポインタが 100 個以上続く最初の表を探す処理
		//-------------------------------------------------------------------------------
		private static int DetectOverworldDataTableOffset()
		{
			byte[] rom = MainForm.romData;
			if (rom == null) return -1;
			int consecutive = 0;
			for (int entry = 0; entry <= rom.Length - 4; entry += 4)
			{
				uint pointer = BitConverter.ToUInt32(rom, entry);
				long offset = (long)pointer - 0x08000000u;
				if (IsAutoRomPointer(rom, pointer) && CanReadAutoData(rom, offset, 12)
					&& BitConverter.ToUInt16(rom, (int)offset) == 0xFFFF
					&& IsOverworldDimension(BitConverter.ToUInt16(rom, (int)offset + 8))
					&& IsOverworldDimension(BitConverter.ToUInt16(rom, (int)offset + 10)))
				{
					if (++consecutive >= 100) return entry - (consecutive - 1) * 4;
				}
				else consecutive = 0;
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// 空き領域を探し始める位置を求める処理（ROM の末尾に続く 0xFF の並びの先頭）
		// データの最後が 0xFF で終わっている場合に備えて 0x100 バイトの余裕を取り、16 バイト境界に切り上げる
		// 末尾の空きが 0x1000 バイトに満たなければ、空きなし（ROM の大きさを返す = 探しても見つからない）とする
		//-------------------------------------------------------------------------------
		private static int DetectFreeSpaceStart()
		{
			byte[] rom = MainForm.romData;
			if (rom == null)
			{
				return 0;
			}
			int end = rom.Length;
			while (end > 0 && rom[end - 1] == 0xFF)
			{
				end--;
			}
			int start = (end + 0x100 + 15) & ~15;
			return rom.Length - start >= 0x1000 ? start : rom.Length;
		}

		private static bool IsOverworldDimension(ushort size)
		{
			return size == 16 || size == 32 || size == 48 || size == 64 || size == 128;
		}
	}
}
