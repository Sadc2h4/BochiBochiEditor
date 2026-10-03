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

		// わざの名前の表の位置・1 項目の長さ・件数（今開いている ROM から決める。別の ROM を開いたら読み直す）
		// 以前は最初に使った時点の ROM で決まったままだったため、ROM を開き直すと古い位置で読んでいた
		public static int MOVE_NAME_TABLE_OFFSET
		{
			get { MoveData.EnsureCurrentRom(); return MoveData.moveNameTableOffset; }
		}

		public static int MOVE_NAME_LENGTH
		{
			get { MoveData.EnsureCurrentRom(); return MoveData.moveNameLength; }
		}

		public static int TOTAL_MOVE_COUNT
		{
			get { MoveData.EnsureCurrentRom(); return MoveData.totalMoveCount; }
		}

		private static byte[] loadedRom;
		private static int moveNameTableOffset;
		private static int moveNameLength;
		private static int totalMoveCount;

		//-------------------------------------------------------------------------------
		// 今の ROM（MainForm.romData）が前と違えば、表の位置と長さを読み直す処理
		// 名前の長さは ini の値のままにせず、ROM の中身から判定する（改造で長くしている ROM があるため）
		//-------------------------------------------------------------------------------
		private static void EnsureCurrentRom()
		{
			byte[] rom = MainForm.romData;
			if (rom == null || ReferenceEquals(rom, MoveData.loadedRom))
			{
				return;
			}
			MoveData.moveNameTableOffset = RomIniReader.ReadHexOrDecimal("MOVE_NAME_TABLE_OFFSET");
			MoveData.totalMoveCount = RomIniReader.ReadHexOrDecimal("TOTAL_MOVE_COUNT");
			int iniLength = RomIniReader.ReadHexOrDecimal("MOVE_NAME_LENGTH");
			MoveData.moveNameLength = RomTableDetector.DetectNameLengthQuiet(rom, MoveData.moveNameTableOffset, iniLength, MoveData.totalMoveCount);
			MoveData.loadedRom = rom;
		}
	}
}
