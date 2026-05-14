using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000017 RID: 23
	public sealed class ItemData
	{
		// Token: 0x06000328 RID: 808 RVA: 0x00017FA4 File Offset: 0x000161A4
		public static ItemData.ItemInfo GetItemInfo(byte[] romData, ushort itemId)
		{
			ItemData.ItemInfo itemInfo = default(ItemData.ItemInfo);
			itemInfo.Index = itemId;
			checked
			{
				int num = ItemData.ITEM_INFO_TABLE_OFFSET + (int)itemId * ItemData.ITEM_INFO_ENTRY_LENGTH;
				byte[] array = new byte[10];
				Array.Copy(romData, num + 0, array, 0, 10);
				itemInfo.Name = TextConverter.BytesToPokemonString(array, 0, 10);
				itemInfo.ItemId = BitConverter.ToUInt16(romData, num + 10);
				itemInfo.Price = BitConverter.ToUInt16(romData, num + 12);
				itemInfo.HeldEffectId = romData[num + 14];
				itemInfo.EffectValue = romData[num + 15];
				uint num2 = BitConverter.ToUInt32(romData, num + 16);
				itemInfo.DescriptionAddress = Conversions.ToUInteger((unchecked((ulong)num2) == 0UL) ? 0 : (num2 - 134217728U));
				itemInfo.CanHold = romData[num + 20];
				itemInfo.UnknownValue = romData[num + 21];
				itemInfo.PocketId = romData[num + 22];
				itemInfo.FieldUseType = romData[num + 23];
				uint num3 = BitConverter.ToUInt32(romData, num + 24);
				itemInfo.FieldUseAddress = Conversions.ToUInteger((unchecked((ulong)num3) == 0UL) ? 0 : (num3 - 134217728U));
				itemInfo.BattleUseType = romData[num + 28];
				uint num4 = BitConverter.ToUInt32(romData, num + 32);
				itemInfo.BattleUseAddress = Conversions.ToUInteger((unchecked((ulong)num4) == 0UL) ? 0 : (num4 - 134217728U));
				itemInfo.SpecialValue = romData[num + 36];
				int num5 = ItemData.ITEM_IMAGE_TABLE_OFFSET + (int)itemId * ItemData.ITEM_IMAGE_ENTRY_LENGTH;
				itemInfo.ImageAddress = BitConverter.ToUInt32(romData, num5) - 134217728U;
				itemInfo.PaletteAddress = BitConverter.ToUInt32(romData, num5 + 4) - 134217728U;
				return itemInfo;
			}
		}

		// Token: 0x06000329 RID: 809 RVA: 0x00018158 File Offset: 0x00016358
		public static List<string> GetItemNames(byte[] romData)
		{
			List<string> list = new List<string>();
			ushort num = checked((ushort)(ItemData.TOTAL_ITEM_COUNT - 1));
			for (ushort num2 = 0; num2 <= num; num2 += 1)
			{
				ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(romData, num2);
				list.Add(itemInfo.Name);
			}
			return list;
		}

		// Token: 0x0600032A RID: 810 RVA: 0x000181A0 File Offset: 0x000163A0
		public static void DisplayItemImage(PictureBox picBox, byte[] romData, ushort itemId)
		{
			try
			{
				ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(romData, itemId);
				bool flag = (ulong)itemInfo.ImageAddress == 0UL || (ulong)itemInfo.PaletteAddress == 0UL;
				if (flag)
				{
					picBox.Image = null;
				}
				else
				{
					byte[] array = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, itemInfo.ImageAddress, false);
					bool flag2 = array.Length == 0;
					if (flag2)
					{
						picBox.Image = null;
					}
					else
					{
						byte[] array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, itemInfo.PaletteAddress, true);
						bool flag3 = array2.Length < 32;
						if (flag3)
						{
							picBox.Image = null;
						}
						else
						{
							Color[] array3 = ImageProcessor.LoadPalette(array2, true);
							Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, 24, 24, false);
							picBox.Image = bitmap;
							picBox.SizeMode = PictureBoxSizeMode.CenterImage;
							picBox.Refresh();
						}
					}
				}
			}
			catch (Exception ex)
			{
				picBox.Image = null;
			}
		}

		// Token: 0x040001A8 RID: 424
		public static readonly int ITEM_INFO_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ITEM_INFO_TABLE_OFFSET");

		// Token: 0x040001A9 RID: 425
		public static readonly int ITEM_INFO_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("ITEM_INFO_ENTRY_LENGTH");

		// Token: 0x040001AA RID: 426
		public static readonly int TOTAL_ITEM_COUNT = RomIniReader.ReadHexOrDecimal("TOTAL_ITEM_COUNT");

		// Token: 0x040001AB RID: 427
		public const int ITEM_NAME_OFFSET = 0;

		// Token: 0x040001AC RID: 428
		public const int ITEM_ID_OFFSET = 10;

		// Token: 0x040001AD RID: 429
		public const int ITEM_PRICE_OFFSET = 12;

		// Token: 0x040001AE RID: 430
		public const int ITEM_HELD_EFFECT_ID_OFFSET = 14;

		// Token: 0x040001AF RID: 431
		public const int ITEM_EFFECT_VALUE_OFFSET = 15;

		// Token: 0x040001B0 RID: 432
		public const int ITEM_DESCRIPTION_ADDRESS_OFFSET = 16;

		// Token: 0x040001B1 RID: 433
		public const int ITEM_CAN_HOLD_OFFSET = 20;

		// Token: 0x040001B2 RID: 434
		public const int ITEM_UNKNOWN_VALUE_OFFSET = 21;

		// Token: 0x040001B3 RID: 435
		public const int ITEM_POCKET_ID_OFFSET = 22;

		// Token: 0x040001B4 RID: 436
		public const int ITEM_FIELD_USE_TYPE_OFFSET = 23;

		// Token: 0x040001B5 RID: 437
		public const int ITEM_FIELD_USE_ADDRESS_OFFSET = 24;

		// Token: 0x040001B6 RID: 438
		public const int ITEM_BATTLE_USE_TYPE_OFFSET = 28;

		// Token: 0x040001B7 RID: 439
		public const int ITEM_BATTLE_USE_ADDRESS_OFFSET = 32;

		// Token: 0x040001B8 RID: 440
		public const int ITEM_SPECIAL_VALUE_OFFSET = 36;

		// Token: 0x040001B9 RID: 441
		public static readonly int ITEM_NAME_MAX_DISPLAY_LENGTH = RomIniReader.ReadHexOrDecimal("ITEM_NAME_MAX_DISPLAY_LENGTH");

		// Token: 0x040001BA RID: 442
		public static readonly int ITEM_IMAGE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ITEM_IMAGE_TABLE_OFFSET");

		// Token: 0x040001BB RID: 443
		public static readonly int ITEM_IMAGE_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("ITEM_IMAGE_ENTRY_LENGTH");

		// Token: 0x040001BC RID: 444
		public const int ITEM_IMAGE_WIDTH = 24;

		// Token: 0x040001BD RID: 445
		public const int ITEM_IMAGE_HEIGHT = 24;

		// Token: 0x02000040 RID: 64
		public struct ItemInfo
		{
			// Token: 0x04000851 RID: 2129
			public ushort Index;

			// Token: 0x04000852 RID: 2130
			public string Name;

			// Token: 0x04000853 RID: 2131
			public ushort ItemId;

			// Token: 0x04000854 RID: 2132
			public ushort Price;

			// Token: 0x04000855 RID: 2133
			public byte HeldEffectId;

			// Token: 0x04000856 RID: 2134
			public byte EffectValue;

			// Token: 0x04000857 RID: 2135
			public uint DescriptionAddress;

			// Token: 0x04000858 RID: 2136
			public byte CanHold;

			// Token: 0x04000859 RID: 2137
			public byte UnknownValue;

			// Token: 0x0400085A RID: 2138
			public byte PocketId;

			// Token: 0x0400085B RID: 2139
			public byte FieldUseType;

			// Token: 0x0400085C RID: 2140
			public uint FieldUseAddress;

			// Token: 0x0400085D RID: 2141
			public byte BattleUseType;

			// Token: 0x0400085E RID: 2142
			public uint BattleUseAddress;

			// Token: 0x0400085F RID: 2143
			public byte SpecialValue;

			// Token: 0x04000860 RID: 2144
			public uint ImageAddress;

			// Token: 0x04000861 RID: 2145
			public uint PaletteAddress;

			// Token: 0x04000862 RID: 2146
			public byte[] TemporaryImageData;

			// Token: 0x04000863 RID: 2147
			public byte[] TemporaryPaletteData;
		}
	}
}
