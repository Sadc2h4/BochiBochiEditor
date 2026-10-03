using System;
using System.Collections.Generic;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップ関連の表の場所を、データの形から探す処理
	// 定義ファイルは、表の場所を「プログラムの中の目印のバイト列」で探す書き方をしている。
	// 新しいコンパイラで作り直した ROM（pokeemerald-expansion など decomp ベースのもの）では、プログラムの並びが元の ROM と違うので目印が見つからない。
	// そのときは、ここの処理で、表そのものの形（ポインタの並び方・値の範囲）から場所を求める。
	// 元のエメラルド（日・英）・Spades・拡張版のビルドで、目印で求めた場所と同じ結果になることを確かめてある
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 調べた結果（ROM ごとに 1 回だけ調べる）
		private static byte[] autoTableRom;
		private static int autoLayoutTable = -1;
		private static int autoMapBankTable = -1;
		private static int autoMapNameTable = -1;
		private static int autoOverworldPaletteTable = -1;

		//-------------------------------------------------------------------------------
		// 地形データ（幅・高さ・ボーダー・並び・タイルセット 1・2）らしい場所かを調べる処理
		//-------------------------------------------------------------------------------
		private static bool LooksLikeMapLayout(byte[] rom, long offset)
		{
			if (offset < 0 || (offset & 3) != 0 || !CanReadAutoData(rom, offset, 24))
			{
				return false;
			}
			int o = (int)offset;
			uint width = BitConverter.ToUInt32(rom, o);
			uint height = BitConverter.ToUInt32(rom, o + 4);
			uint secondary = BitConverter.ToUInt32(rom, o + 20);
			return width >= 1 && width <= 255 && height >= 1 && height <= 255
				&& IsAutoRomPointer(rom, BitConverter.ToUInt32(rom, o + 8))
				&& IsAutoRomPointer(rom, BitConverter.ToUInt32(rom, o + 12))
				&& IsAutoRomPointer(rom, BitConverter.ToUInt32(rom, o + 16))
				&& (secondary == 0 || IsAutoRomPointer(rom, secondary));
		}

		//-------------------------------------------------------------------------------
		// マップの見出し（地形データ・イベント・マップスクリプト・接続へのポインタで始まる）らしい場所かを調べる処理
		//-------------------------------------------------------------------------------
		private static bool LooksLikeMapHeader(byte[] rom, long offset)
		{
			if (offset < 0 || (offset & 3) != 0 || !CanReadAutoData(rom, offset, 28))
			{
				return false;
			}
			int o = (int)offset;
			uint layout = BitConverter.ToUInt32(rom, o);
			if (!IsAutoRomPointer(rom, layout) || !LooksLikeMapLayout(rom, layout - 0x08000000u))
			{
				return false;
			}
			for (int field = 4; field <= 12; field += 4)
			{
				uint pointer = BitConverter.ToUInt32(rom, o + field);
				if (pointer != 0 && !IsAutoRomPointer(rom, pointer))
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// 印の付いた欄が続く、いちばん長い並びの先頭（欄の番号）と長さを返す処理
		// allowZero なら、途中の 0 の欄（空の項目）も並びの続きとして数える（末尾の 0 は数えない）。印の数が minMarked に満たない並びは使わない
		//-------------------------------------------------------------------------------
		private static void LongestRun(byte[] rom, bool[] marked, bool allowZero, int minMarked, out int start, out int length)
		{
			start = -1;
			length = 0;
			int n = marked.Length;
			int i = 0;
			while (i < n)
			{
				if (!marked[i])
				{
					i++;
					continue;
				}
				int j = i;
				int count = 0;
				while (j < n && (marked[j] || (allowZero && BitConverter.ToUInt32(rom, j * 4) == 0)))
				{
					if (marked[j])
					{
						count++;
					}
					j++;
				}
				int end = j;
				while (end > i && !marked[end - 1])
				{
					end--;
				}
				if (count >= minMarked && end - i > length)
				{
					start = i;
					length = end - i;
				}
				i = j;
			}
		}

		//-------------------------------------------------------------------------------
		// 4 つの表（地形データの表・マップバンクの表・マップ名の表・人物のパレットの表）をまとめて探す処理（同じ ROM では 1 回だけ）
		//-------------------------------------------------------------------------------
		private static void DetectTablesByShape()
		{
			byte[] rom = MainForm.romData;
			if (rom == null || ReferenceEquals(rom, autoTableRom))
			{
				return;
			}
			autoTableRom = rom;
			autoLayoutTable = autoMapBankTable = autoMapNameTable = autoOverworldPaletteTable = -1;
			int n = rom.Length / 4;
			bool[] toLayout = new bool[n];
			bool[] toHeader = new bool[n];
			for (int i = 0; i < n; i++)
			{
				uint value = BitConverter.ToUInt32(rom, i * 4);
				if (!IsAutoRomPointer(rom, value))
				{
					continue;
				}
				long target = value - 0x08000000u;
				if (LooksLikeMapLayout(rom, target))
				{
					toLayout[i] = true;
				}
				else if (LooksLikeMapHeader(rom, target))
				{
					toHeader[i] = true;
				}
			}
			// 地形データの表: 0 か、地形データを指すポインタが続くいちばん長い並び
			int start;
			int length;
			LongestRun(rom, toLayout, true, 50, out start, out length);
			autoLayoutTable = start >= 0 ? start * 4 : -1;
			// マップバンクの表: 「マップの見出しを指すポインタの並び（バンク）」の先頭を指すポインタが続く並び
			bool[] toBank = new bool[n];
			for (int i = 0; i < n; i++)
			{
				uint value = BitConverter.ToUInt32(rom, i * 4);
				if (!IsAutoRomPointer(rom, value))
				{
					continue;
				}
				long target = value - 0x08000000u;
				if ((target & 3) == 0 && target / 4 < n && toHeader[target / 4])
				{
					toBank[i] = true;
				}
			}
			LongestRun(rom, toBank, false, 5, out start, out length);
			autoMapBankTable = start >= 0 ? start * 4 : -1;
			// マップ名の表: x・y・幅・高さ（小さい数）+ 名前へのポインタ の 8 バイトが続くいちばん長い並び
			int bestCount = 0;
			for (int phase = 0; phase < 2; phase++)
			{
				int i = phase;
				while (i + 1 < n)
				{
					if (!LooksLikeMapNameEntry(rom, i))
					{
						i += 2;
						continue;
					}
					int j = i;
					while (j + 1 < n && LooksLikeMapNameEntry(rom, j))
					{
						j += 2;
					}
					if ((j - i) / 2 > bestCount)
					{
						bestCount = (j - i) / 2;
						autoMapNameTable = i * 4;
					}
					i = j;
				}
			}
			if (bestCount < 20)
			{
				autoMapNameTable = -1;
			}
			// 人物のパレットの表: ポインタ + 札（0x11xx）+ 0 の 8 バイトが続くいちばん長い並び
			bestCount = 0;
			int k = 0;
			while (k + 1 < n)
			{
				if (!LooksLikeOverworldPaletteEntry(rom, k))
				{
					k++;
					continue;
				}
				int j = k;
				while (j + 1 < n && LooksLikeOverworldPaletteEntry(rom, j))
				{
					j += 2;
				}
				if ((j - k) / 2 > bestCount)
				{
					bestCount = (j - k) / 2;
					autoOverworldPaletteTable = k * 4;
				}
				k = j;
			}
			if (bestCount < 8)
			{
				autoOverworldPaletteTable = -1;
			}
		}

		//-------------------------------------------------------------------------------
		// 欄の番号の所が、マップ名の表の 1 件（x・y が 64 未満、幅・高さが 1〜16、続く 4 バイトが ROM 内へのポインタ）らしいかを調べる処理
		//-------------------------------------------------------------------------------
		private static bool LooksLikeMapNameEntry(byte[] rom, int word)
		{
			int o = word * 4;
			return rom[o] < 64 && rom[o + 1] < 64 && rom[o + 2] >= 1 && rom[o + 2] <= 16 && rom[o + 3] >= 1 && rom[o + 3] <= 16
				&& IsAutoRomPointer(rom, BitConverter.ToUInt32(rom, o + 4));
		}

		//-------------------------------------------------------------------------------
		// 欄の番号の所が、人物のパレットの表の 1 件（ROM 内へのポインタ + 0x11xx の札 + 0）らしいかを調べる処理
		//-------------------------------------------------------------------------------
		private static bool LooksLikeOverworldPaletteEntry(byte[] rom, int word)
		{
			int o = word * 4;
			return IsAutoRomPointer(rom, BitConverter.ToUInt32(rom, o)) && (BitConverter.ToUInt32(rom, o + 4) & 0xFFFFFF00u) == 0x1100u;
		}

		//-------------------------------------------------------------------------------
		// マップバンクの表の場所を、データの形から求めて返す処理（見つからなければ -1）。定義ファイルの目印が見つからないときに使う
		//-------------------------------------------------------------------------------
		private static int DetectMapBankTableOffset()
		{
			DetectTablesByShape();
			return autoMapBankTable;
		}

		//-------------------------------------------------------------------------------
		// 地形データの表の場所を、データの形から求めて返す処理（見つからなければ -1）
		//-------------------------------------------------------------------------------
		private static int DetectMapLayoutTableOffset()
		{
			DetectTablesByShape();
			return autoLayoutTable;
		}

		//-------------------------------------------------------------------------------
		// マップ名の表の場所を、データの形から求めて返す処理（見つからなければ -1）
		//-------------------------------------------------------------------------------
		private static int DetectMapNameTableOffset()
		{
			DetectTablesByShape();
			return autoMapNameTable;
		}

		//-------------------------------------------------------------------------------
		// 人物のパレットの表の場所を、データの形から求めて返す処理（見つからなければ -1）
		//-------------------------------------------------------------------------------
		private static int DetectOverworldPaletteTableOffset()
		{
			DetectTablesByShape();
			return autoOverworldPaletteTable;
		}
	}
}
