using System;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 読み込んだ ROM の道具のデータの表から、道具の名前を読む処理
	// 1 件の大きさは日本語版 40 バイト・英語版 44 バイト（名前の欄の長さが違う）。名前の直後に道具番号（2 バイト）がある
	// 大きさは、先頭の数件で「名前の直後の道具番号が、その件の番号と同じ」になるほうを選ぶ
	// どちらも当てはまらない ROM（道具のデータの形を作り替えた改造版）では、名前を出さない
	//-------------------------------------------------------------------------------
	internal sealed class ItemNameTable
	{
		// 1 件の大きさの候補と、名前の欄より後ろの大きさ（1 件の大きさ - 名前の欄の長さ）
		private static readonly int[] EntrySizes = new int[] { 40, 44 };
		private const int BytesAfterName = 30;
		// 番号の上限（壊れたデータで ROM の外を読まないように）
		private const int MaxItems = 4096;

		private readonly byte[] rom;
		private readonly int offset;
		private readonly int entrySize;

		//-------------------------------------------------------------------------------
		// 表の場所と 1 件の大きさを受け取って作る処理
		//-------------------------------------------------------------------------------
		private ItemNameTable(byte[] rom, int offset, int entrySize)
		{
			this.rom = rom;
			this.offset = offset;
			this.entrySize = entrySize;
		}

		//-------------------------------------------------------------------------------
		// ini の ITEM_INFO_TABLE_OFFSET から表を調べて作る処理（設定が無い・形が合わないときは null）
		// （ini の「*アドレス」は MainForm.romData を読むので、先に MainForm.romData を同じ ROM にしておくこと）
		//-------------------------------------------------------------------------------
		public static ItemNameTable Create(byte[] rom)
		{
			if (rom == null || string.IsNullOrWhiteSpace(RomIniReader.ReadValue("ITEM_INFO_TABLE_OFFSET")))
			{
				return null;
			}
			int offset;
			try
			{
				offset = RomIniReader.ReadHexOrDecimal("ITEM_INFO_TABLE_OFFSET");
			}
			catch (Exception)
			{
				return null;
			}
			if (offset <= 0 || offset >= rom.Length)
			{
				return null;
			}
			foreach (int size in EntrySizes)
			{
				bool matches = true;
				for (int id = 1; id <= 5 && matches; id++)
				{
					int idField = offset + id * size + (size - BytesAfterName);
					matches = idField + 2 <= rom.Length && BitConverter.ToUInt16(rom, idField) == id;
				}
				if (matches)
				{
					return new ItemNameTable(rom, offset, size);
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 道具番号から名前を返す処理（表の外・名前として読めないときは null）
		// その件の道具番号の欄が、番号そのものか 0（使われていない枠）のときだけ、その件を道具として扱う
		//-------------------------------------------------------------------------------
		public string GetName(int id)
		{
			if (id < 0 || id >= MaxItems)
			{
				return null;
			}
			int nameLength = this.entrySize - BytesAfterName;
			long start = (long)this.offset + (long)id * this.entrySize;
			if (start + this.entrySize > this.rom.Length)
			{
				return null;
			}
			int idField = BitConverter.ToUInt16(this.rom, (int)start + nameLength);
			if (idField != id && idField != 0)
			{
				return null;
			}
			// 名前の欄の中に終わりの印（FF）があること
			bool terminated = false;
			for (int i = 0; i < nameLength; i++)
			{
				if (this.rom[start + i] == 0xFF)
				{
					terminated = i > 0;
					break;
				}
			}
			if (!terminated)
			{
				return null;
			}
			string name = TextConverter.BytesToPokemonString(this.rom, (int)start, nameLength);
			// 文字表に無い文字（[XX] と出る）が混じっていれば、道具の名前ではないとみなす
			return string.IsNullOrEmpty(name) || name.Contains("[") ? null : name;
		}
	}
}
