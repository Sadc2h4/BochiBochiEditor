using System;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 読み込んだ ROM のポケモン名の表（場所・1 匹ぶんの長さ・匹数）
	// 「出現ポケモン」の画面が、ポケモン編集画面（ファイアレッド専用の設定を大量に読む）を作らずに名前を読めるようにするためのもの
	// 場所は ini の POKEMON_NAME_OFFSET、長さは ini の値を ROM の中身で確かめ、匹数は ini が auto（または未記載）なら ROM から数える
	//-------------------------------------------------------------------------------
	internal sealed class PokemonNameTable
	{
		// ini に長さが無いときに試す長さ（海外版の長さ）
		private const int FallbackNameLength = 11;
		// ini に匹数が無く、ROM からも数えられないときの匹数（元のゲームの匹数）
		private const int FallbackCount = 412;
		// 数えるときの上限（壊れたデータで延々と読まないように）
		private const int MaxCount = 4096;

		private readonly byte[] rom;

		// 名前の表の先頭
		public int Offset { get; }
		// 1 匹ぶんの長さ（終わりの印を含む）
		public int NameLength { get; }
		// 匹数（0 番の「なし」を含む）
		public int Count { get; }

		//-------------------------------------------------------------------------------
		// 表の場所・長さ・匹数を受け取って作る処理
		//-------------------------------------------------------------------------------
		private PokemonNameTable(byte[] rom, int offset, int nameLength, int count)
		{
			this.rom = rom;
			this.Offset = offset;
			this.NameLength = nameLength;
			this.Count = count;
		}

		//-------------------------------------------------------------------------------
		// ini の設定と ROM の中身から、名前の表を調べて作る処理
		// （ini の「*アドレス」は MainForm.romData を読むので、先に MainForm.romData を同じ ROM にしておくこと）
		//-------------------------------------------------------------------------------
		public static PokemonNameTable Create(byte[] rom)
		{
			int offset = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_OFFSET");
			int iniLength = ReadNumberOrAuto("POKEMON_NAME_LENGTH");
			int iniCount = ReadNumberOrAuto("TOTAL_POKEMON_COUNT");
			// 長さ: ini の値（無ければ海外版の長さ）を出発点に、ROM の中身でいちばん当てはまる長さを選ぶ
			int length = RomTableDetector.DetectNameLength(rom, offset, iniLength > 0 ? iniLength : FallbackNameLength, iniCount > 0 ? iniCount : FallbackCount);
			int count = iniCount > 0 ? iniCount : CountEntries(rom, offset, length);
			return new PokemonNameTable(rom, offset, length, count);
		}

		//-------------------------------------------------------------------------------
		// ini の数値を読む処理（未記載・auto なら -1）
		//-------------------------------------------------------------------------------
		private static int ReadNumberOrAuto(string key)
		{
			string value = RomIniReader.ReadValue(key);
			if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase))
			{
				return -1;
			}
			return RomIniReader.ReadHexOrDecimal(key);
		}

		//-------------------------------------------------------------------------------
		// 名前の表の匹数を ROM から数える処理
		// 元のゲームでは名前の表のすぐ後ろにわざ名の表が続くので、その間の大きさ ÷ 1 匹ぶんの長さを匹数とする
		// （改造版では名前が空の匹が途中にあるため、名前として読める間だけ数える方法は使わない）
		// わざ名の表が後ろに続いていない ROM では、名前として読める項目が続く数を使う
		//-------------------------------------------------------------------------------
		private static int CountEntries(byte[] rom, int offset, int length)
		{
			if (rom == null || offset < 0 || length <= 0)
			{
				return FallbackCount;
			}
			try
			{
				string moveValue = RomIniReader.ReadValue("MOVE_NAME_TABLE_OFFSET");
				if (!string.IsNullOrWhiteSpace(moveValue))
				{
					int moveNames = RomIniReader.ReadHexOrDecimal("MOVE_NAME_TABLE_OFFSET");
					int span = moveNames - offset;
					if (span > 0 && span % length == 0 && span / length >= 2 && span / length <= MaxCount)
					{
						return span / length;
					}
				}
			}
			catch (Exception)
			{
				// わざ名の表が読めないときは、下の数え方へ進む
			}
			int count = 1;
			while (count < MaxCount && RomTableDetector.CountReadableNames(rom, offset + (count - 1) * length, length, 2) == 1)
			{
				count++;
			}
			return count >= 2 ? count : FallbackCount;
		}

		//-------------------------------------------------------------------------------
		// 指定した番号のポケモンの名前を読む処理（表の外や ROM の外なら空文字）
		//-------------------------------------------------------------------------------
		public string GetName(int index)
		{
			if (index < 0 || index >= this.Count)
			{
				return string.Empty;
			}
			long start = (long)this.Offset + (long)index * this.NameLength;
			if (this.Offset < 0 || start + this.NameLength > this.rom.Length)
			{
				return string.Empty;
			}
			return TextConverter.BytesToPokemonString(this.rom, (int)start, this.NameLength);
		}
	}
}
