using System;
using System.Collections.Generic;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ROM の表（テーブル）の形を、ini の値だけに頼らず ROM の中身から確かめる処理
	// 改造を重ねた ROM では、表が拡張領域へ移されたり、1 項目の長さや項目数が変えられたりしているため、
	// ini の値で読むとずれることがある。中身が「その表らしいか」を数えて、いちばん当てはまる形を選ぶ
	// 判定の結果は Results に残しておき、あとで「ROM の構成」として確認できるようにする
	//-------------------------------------------------------------------------------
	internal static class RomTableDetector
	{
		// 判定の結果（表の名前 → 説明文）。ROM を開き直すたびに上書きされる
		public static readonly Dictionary<string, string> Results = new Dictionary<string, string>();

		// 名前の 1 項目の長さとして試す範囲（元の日本語版は 6、海外版は 11）
		private const int MinNameLength = 5;
		private const int MaxNameLength = 16;

		//-------------------------------------------------------------------------------
		// ポケモン名の表の 1 匹ぶんの長さを判定する処理
		// 各項目が「1 文字以上の名前 → 終わりの印（FF）→ 残りは FF か 00 で埋まる」形になっている割合がいちばん高い長さを選ぶ
		// ini の長さでも同じくらい当てはまるなら ini の値を使い、どれも当てはまらなければ ini の値のままにする
		//-------------------------------------------------------------------------------
		public static int DetectNameLength(byte[] rom, int offset, int iniLength, int count)
		{
			if (rom == null || offset < 0 || count <= 1)
			{
				return iniLength;
			}
			int samples = Math.Min(count, 412) - 1;
			int bestLength = iniLength;
			int bestScore = ScoreNameLength(rom, offset, iniLength, samples);
			int iniScore = bestScore;
			for (int length = MinNameLength; length <= MaxNameLength; length++)
			{
				int score = ScoreNameLength(rom, offset, length, samples);
				if (score > bestScore)
				{
					bestScore = score;
					bestLength = length;
				}
			}
			// 9 割以上の名前が正しく読める長さだけを信用する
			if (bestScore * 10 < samples * 9)
			{
				Results["ポケモン名の長さ"] = string.Format("ini の値 {0} を使用（ROM の中身からは判定できず）", iniLength);
				return iniLength;
			}
			Results["ポケモン名の長さ"] = bestLength == iniLength
				? string.Format("{0}（ini と一致）", bestLength)
				: string.Format("{0}（ROM の中身から判定。ini は {1}、当てはまった名前 {2}/{3} → {4}/{3}）", bestLength, iniLength, iniScore, samples, bestScore);
			return bestLength;
		}

		//-------------------------------------------------------------------------------
		// 名前の 1 項目の長さを判定する処理（判定結果を Results に残さない版。テーブル情報の一覧で使う）
		//-------------------------------------------------------------------------------
		public static int DetectNameLengthQuiet(byte[] rom, int offset, int iniLength, int count)
		{
			if (rom == null || offset < 0 || count <= 1)
			{
				return iniLength;
			}
			int samples = count - 1;
			int bestLength = iniLength;
			int bestScore = ScoreNameLength(rom, offset, iniLength, samples);
			for (int length = MinNameLength; length <= MaxNameLength; length++)
			{
				int score = ScoreNameLength(rom, offset, length, samples);
				if (score > bestScore)
				{
					bestScore = score;
					bestLength = length;
				}
			}
			return bestScore * 10 < samples * 9 ? iniLength : bestLength;
		}

		//-------------------------------------------------------------------------------
		// その長さ・件数で読んだとき、名前として正しい形の項目がいくつあるかを返す処理（0 番は除く）
		//-------------------------------------------------------------------------------
		public static int CountReadableNames(byte[] rom, int offset, int length, int count)
		{
			return rom == null || offset < 0 || count <= 1 ? 0 : ScoreNameLength(rom, offset, length, count - 1);
		}

		//-------------------------------------------------------------------------------
		// その長さで区切ったとき、名前として正しい形の項目がいくつあるかを数える処理
		//-------------------------------------------------------------------------------
		private static int ScoreNameLength(byte[] rom, int offset, int length, int samples)
		{
			if (length <= 1)
			{
				return 0;
			}
			int score = 0;
			for (int species = 1; species <= samples; species++)
			{
				int start = offset + species * length;
				if (start + length > rom.Length)
				{
					break;
				}
				if (IsNameEntry(rom, start, length))
				{
					score++;
				}
			}
			return score;
		}

		//-------------------------------------------------------------------------------
		// 1 項目が「名前 → FF → FF か 00 の埋め草」の形かを判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsNameEntry(byte[] rom, int start, int length)
		{
			byte first = rom[start];
			if (first == 0xFF || first == 0x00)
			{
				return false;
			}
			for (int i = 1; i < length; i++)
			{
				if (rom[start + i] != 0xFF)
				{
					continue;
				}
				for (int j = i + 1; j < length; j++)
				{
					byte pad = rom[start + j];
					if (pad != 0xFF && pad != 0x00)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}
