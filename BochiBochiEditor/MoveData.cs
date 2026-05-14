using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x0200001C RID: 28
	public sealed class MoveData
	{
		// Token: 0x06000781 RID: 1921 RVA: 0x00039D90 File Offset: 0x00037F90
		public static List<string> GetMoveNames(byte[] romData)
		{
			List<string> list = new List<string>();
			checked
			{
				int num = MoveData.TOTAL_MOVE_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					string text = MoveData.ExtractMoveNameFromRom(romData, i);
					list.Add(text);
				}
				return list;
			}
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x00039DD0 File Offset: 0x00037FD0
		public static string ExtractMoveNameFromRom(byte[] romData, int moveIndex)
		{
			checked
			{
				int num = MoveData.MOVE_NAME_TABLE_OFFSET + moveIndex * MoveData.MOVE_NAME_LENGTH;
				byte[] array = new byte[MoveData.MOVE_NAME_LENGTH - 1 + 1];
				Array.Copy(romData, num, array, 0, MoveData.MOVE_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, MoveData.MOVE_NAME_LENGTH);
			}
		}

		// Token: 0x04000422 RID: 1058
		public static readonly int MOVE_NAME_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("MOVE_NAME_TABLE_OFFSET");

		// Token: 0x04000423 RID: 1059
		public static readonly int MOVE_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("MOVE_NAME_LENGTH");

		// Token: 0x04000424 RID: 1060
		public static readonly int TOTAL_MOVE_COUNT = RomIniReader.ReadHexOrDecimal("TOTAL_MOVE_COUNT");
	}
}
