using System;
using System.Drawing.Imaging;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x02000011 RID: 17
	public class NewDataGenerator
	{
		// Token: 0x02000038 RID: 56
		public interface INewDataGenerator
		{
			// Token: 0x06000ED3 RID: 3795
			bool GenerateData(byte[] rom, uint startAddress);

			// Token: 0x17000597 RID: 1431
			// (get) Token: 0x06000ED4 RID: 3796
			string Description { get; }
		}

		// Token: 0x02000039 RID: 57
		public class TilesetGenerator : NewDataGenerator.INewDataGenerator
		{
			// Token: 0x17000598 RID: 1432
			// (get) Token: 0x06000ED6 RID: 3798 RVA: 0x0006ACCB File Offset: 0x00068ECB
			// (set) Token: 0x06000ED7 RID: 3799 RVA: 0x0006ACD5 File Offset: 0x00068ED5
			public byte[] ImageBytes { get; set; }

			// Token: 0x17000599 RID: 1433
			// (get) Token: 0x06000ED8 RID: 3800 RVA: 0x0006ACDE File Offset: 0x00068EDE
			// (set) Token: 0x06000ED9 RID: 3801 RVA: 0x0006ACE8 File Offset: 0x00068EE8
			public byte PaletteType { get; set; }

			// Token: 0x1700059A RID: 1434
			// (get) Token: 0x06000EDA RID: 3802 RVA: 0x0006ACF1 File Offset: 0x00068EF1
			// (set) Token: 0x06000EDB RID: 3803 RVA: 0x0006ACFB File Offset: 0x00068EFB
			public byte CompressType { get; set; }

			// Token: 0x1700059B RID: 1435
			// (get) Token: 0x06000EDC RID: 3804 RVA: 0x0006AD04 File Offset: 0x00068F04
			// (set) Token: 0x06000EDD RID: 3805 RVA: 0x0006AD0E File Offset: 0x00068F0E
			public int BlockCount { get; set; }

			// Token: 0x1700059C RID: 1436
			// (get) Token: 0x06000EDE RID: 3806 RVA: 0x0006AD17 File Offset: 0x00068F17
			// (set) Token: 0x06000EDF RID: 3807 RVA: 0x0006AD21 File Offset: 0x00068F21
			public int TilesetIndexStartOffset { get; set; }

			// Token: 0x1700059D RID: 1437
			// (get) Token: 0x06000EE0 RID: 3808 RVA: 0x0006AD2A File Offset: 0x00068F2A
			// (set) Token: 0x06000EE1 RID: 3809 RVA: 0x0006AD34 File Offset: 0x00068F34
			public int OutTilesetIndex { get; set; }

			// Token: 0x06000EE2 RID: 3810 RVA: 0x0006AD40 File Offset: 0x00068F40
			public bool GenerateData(byte[] rom, uint startAddress)
			{
				checked
				{
					int num = (int)startAddress - this.TilesetIndexStartOffset;
					int num2 = num / 24;
					bool flag = num % 24 != 0;
					if (flag)
					{
						num2++;
					}
					this.OutTilesetIndex = num2;
					int num3 = this.TilesetIndexStartOffset + num2 * 24;
					rom[num3 + 0] = this.CompressType;
					rom[num3 + 1] = this.PaletteType;
					rom[num3 + 2] = 0;
					rom[num3 + 3] = 0;
					this.WritePointer(rom, num3 + 4, 0U);
					this.WritePointer(rom, num3 + 8, 0U);
					this.WritePointer(rom, num3 + 12, 0U);
					this.WritePointer(rom, num3 + 16, 0U);
					this.WritePointer(rom, num3 + 20, 0U);
					uint num4 = (uint)(num3 + 24);
					uint num5 = num4;
					Array.Clear(rom, (int)num4, 512);
					num4 = (uint)(unchecked((ulong)num4) + 512UL);
					uint num6 = num4;
					int num7 = this.BlockCount * 16;
					Array.Clear(rom, (int)num4, num7);
					num4 += (uint)num7;
					uint num8 = num4;
					int num9 = this.BlockCount * 4;
					Array.Clear(rom, (int)num4, num9);
					num4 += (uint)num9;
					uint num10 = num4;
					Array.Copy(this.ImageBytes, 0, rom, (int)num4, this.ImageBytes.Length);
					this.WritePointer(rom, num3 + 4, num10);
					this.WritePointer(rom, num3 + 8, num5);
					this.WritePointer(rom, num3 + 12, num6);
					this.WritePointer(rom, num3 + 16, 0U);
					this.WritePointer(rom, num3 + 20, num8);
					return true;
				}
			}

			// Token: 0x06000EE3 RID: 3811 RVA: 0x0006AEB4 File Offset: 0x000690B4
			private void WritePointer(byte[] rom, int offset, uint addr)
			{
				uint num = (((ulong)addr != 0UL) ? checked(addr + 134217728U) : 0U);
				Array.Copy(BitConverter.GetBytes(num), 0, rom, offset, 4);
			}

			// Token: 0x1700059E RID: 1438
			// (get) Token: 0x06000EE4 RID: 3812 RVA: 0x0006AEE4 File Offset: 0x000690E4
			public string Description
			{
				get
				{
					return "Tileset Data";
				}
			}
		}

		// Token: 0x0200003A RID: 58
		public class PaletteGenerator : NewDataGenerator.INewDataGenerator
		{
			// Token: 0x1700059F RID: 1439
			// (get) Token: 0x06000EE6 RID: 3814 RVA: 0x0006AF03 File Offset: 0x00069103
			// (set) Token: 0x06000EE7 RID: 3815 RVA: 0x0006AF0D File Offset: 0x0006910D
			public int TilesetIndex { get; set; }

			// Token: 0x170005A0 RID: 1440
			// (get) Token: 0x06000EE8 RID: 3816 RVA: 0x0006AF16 File Offset: 0x00069116
			// (set) Token: 0x06000EE9 RID: 3817 RVA: 0x0006AF20 File Offset: 0x00069120
			public int PaletteIndex { get; set; }

			// Token: 0x170005A1 RID: 1441
			// (get) Token: 0x06000EEA RID: 3818 RVA: 0x0006AF29 File Offset: 0x00069129
			// (set) Token: 0x06000EEB RID: 3819 RVA: 0x0006AF33 File Offset: 0x00069133
			public ColorPalette SourcePalette { get; set; }

			// Token: 0x06000EEC RID: 3820 RVA: 0x0006AF3C File Offset: 0x0006913C
			public bool GenerateData(byte[] rom, uint startAddress)
			{
				checked
				{
					int num = MyProject.Forms.MapEditor.TILESET_INDEX_START_OFFSET + this.TilesetIndex * 24;
					uint num2 = BitConverter.ToUInt32(rom, num + 8);
					int num3 = (int)(num2 - 134217728U);
					byte[] array = ImageProcessor.ConvertPaletteToBytes(this.SourcePalette);
					int num4 = num3 + this.PaletteIndex * 32;
					Array.Copy(array, 0, rom, num4, Math.Min(array.Length, 32));
					return true;
				}
			}

			// Token: 0x170005A2 RID: 1442
			// (get) Token: 0x06000EED RID: 3821 RVA: 0x0006AFB0 File Offset: 0x000691B0
			public string Description
			{
				get
				{
					return string.Format("Palette {0} for Tileset {1}", this.PaletteIndex, this.TilesetIndex);
				}
			}
		}

		// Token: 0x0200003B RID: 59
		public class MapFooterGenerator : NewDataGenerator.INewDataGenerator
		{
			// Token: 0x170005A3 RID: 1443
			// (get) Token: 0x06000EEF RID: 3823 RVA: 0x0006AFEA File Offset: 0x000691EA
			// (set) Token: 0x06000EF0 RID: 3824 RVA: 0x0006AFF4 File Offset: 0x000691F4
			public byte MapWidth { get; set; }

			// Token: 0x170005A4 RID: 1444
			// (get) Token: 0x06000EF1 RID: 3825 RVA: 0x0006AFFD File Offset: 0x000691FD
			// (set) Token: 0x06000EF2 RID: 3826 RVA: 0x0006B007 File Offset: 0x00069207
			public byte MapHeight { get; set; }

			// Token: 0x170005A5 RID: 1445
			// (get) Token: 0x06000EF3 RID: 3827 RVA: 0x0006B010 File Offset: 0x00069210
			// (set) Token: 0x06000EF4 RID: 3828 RVA: 0x0006B01A File Offset: 0x0006921A
			public byte BorderWidth { get; set; }

			// Token: 0x170005A6 RID: 1446
			// (get) Token: 0x06000EF5 RID: 3829 RVA: 0x0006B023 File Offset: 0x00069223
			// (set) Token: 0x06000EF6 RID: 3830 RVA: 0x0006B02D File Offset: 0x0006922D
			public byte BorderHeight { get; set; }

			// Token: 0x170005A7 RID: 1447
			// (get) Token: 0x06000EF7 RID: 3831 RVA: 0x0006B036 File Offset: 0x00069236
			// (set) Token: 0x06000EF8 RID: 3832 RVA: 0x0006B040 File Offset: 0x00069240
			public int Tileset1Index { get; set; }

			// Token: 0x170005A8 RID: 1448
			// (get) Token: 0x06000EF9 RID: 3833 RVA: 0x0006B049 File Offset: 0x00069249
			// (set) Token: 0x06000EFA RID: 3834 RVA: 0x0006B053 File Offset: 0x00069253
			public int Tileset2Index { get; set; }

			// Token: 0x170005A9 RID: 1449
			// (get) Token: 0x06000EFB RID: 3835 RVA: 0x0006B05C File Offset: 0x0006925C
			// (set) Token: 0x06000EFC RID: 3836 RVA: 0x0006B066 File Offset: 0x00069266
			public int TilesetIndexStartOffset { get; set; }

			// Token: 0x170005AA RID: 1450
			// (get) Token: 0x06000EFD RID: 3837 RVA: 0x0006B06F File Offset: 0x0006926F
			// (set) Token: 0x06000EFE RID: 3838 RVA: 0x0006B079 File Offset: 0x00069279
			public uint HeaderAddress { get; set; }

			// Token: 0x06000EFF RID: 3839 RVA: 0x0006B084 File Offset: 0x00069284
			public bool GenerateData(byte[] rom, uint startAddress)
			{
				checked
				{
					uint num = (uint)(this.TilesetIndexStartOffset + this.Tileset1Index * 24);
					uint num2 = (uint)(this.TilesetIndexStartOffset + this.Tileset2Index * 24);
					int num3 = (int)(unchecked(this.BorderWidth * this.BorderHeight) * 2);
					int num4 = (int)(unchecked(this.MapWidth * this.MapHeight) * 2);
					uint num5 = (uint)(unchecked((ulong)startAddress) + 28UL);
					uint num6 = num5;
					num5 += (uint)num3;
					uint num7 = num5;
					int num8 = (int)startAddress;
					Array.Clear(rom, num8, 28);
					rom[num8 + 0] = this.MapWidth;
					rom[num8 + 4] = this.MapHeight;
					this.WritePointer(rom, num8 + 8, num6);
					this.WritePointer(rom, num8 + 12, num7);
					this.WritePointer(rom, num8 + 16, num);
					this.WritePointer(rom, num8 + 20, num2);
					rom[num8 + 24] = this.BorderWidth;
					rom[num8 + 25] = this.BorderHeight;
					bool flag = num3 > 0;
					if (flag)
					{
						Array.Clear(rom, (int)num6, num3);
					}
					bool flag2 = num4 > 0;
					if (flag2)
					{
						Array.Clear(rom, (int)num7, num4);
					}
					this.HeaderAddress = startAddress;
					return true;
				}
			}

			// Token: 0x06000F00 RID: 3840 RVA: 0x0006B1A4 File Offset: 0x000693A4
			private void WritePointer(byte[] rom, int offset, uint addr)
			{
				uint num = (((ulong)addr != 0UL) ? checked(addr + 134217728U) : 0U);
				Array.Copy(BitConverter.GetBytes(num), 0, rom, offset, 4);
			}

			// Token: 0x170005AB RID: 1451
			// (get) Token: 0x06000F01 RID: 3841 RVA: 0x0006B1D4 File Offset: 0x000693D4
			public string Description
			{
				get
				{
					return "Map Footer Data";
				}
			}
		}

		// Token: 0x0200003C RID: 60
		public class EventGenerator : NewDataGenerator.INewDataGenerator
		{
			// Token: 0x170005AC RID: 1452
			// (get) Token: 0x06000F03 RID: 3843 RVA: 0x0006B1F3 File Offset: 0x000693F3
			// (set) Token: 0x06000F04 RID: 3844 RVA: 0x0006B1FD File Offset: 0x000693FD
			public byte PersonCount { get; set; }

			// Token: 0x170005AD RID: 1453
			// (get) Token: 0x06000F05 RID: 3845 RVA: 0x0006B206 File Offset: 0x00069406
			// (set) Token: 0x06000F06 RID: 3846 RVA: 0x0006B210 File Offset: 0x00069410
			public byte WarpCount { get; set; }

			// Token: 0x170005AE RID: 1454
			// (get) Token: 0x06000F07 RID: 3847 RVA: 0x0006B219 File Offset: 0x00069419
			// (set) Token: 0x06000F08 RID: 3848 RVA: 0x0006B223 File Offset: 0x00069423
			public byte TrapCount { get; set; }

			// Token: 0x170005AF RID: 1455
			// (get) Token: 0x06000F09 RID: 3849 RVA: 0x0006B22C File Offset: 0x0006942C
			// (set) Token: 0x06000F0A RID: 3850 RVA: 0x0006B236 File Offset: 0x00069436
			public byte SignCount { get; set; }

			// Token: 0x170005B0 RID: 1456
			// (get) Token: 0x06000F0B RID: 3851 RVA: 0x0006B23F File Offset: 0x0006943F
			// (set) Token: 0x06000F0C RID: 3852 RVA: 0x0006B249 File Offset: 0x00069449
			public uint StartAddress { get; set; }

			// Token: 0x170005B1 RID: 1457
			// (get) Token: 0x06000F0D RID: 3853 RVA: 0x0006B252 File Offset: 0x00069452
			// (set) Token: 0x06000F0E RID: 3854 RVA: 0x0006B25C File Offset: 0x0006945C
			public uint HeaderAddress { get; set; }

			// Token: 0x06000F0F RID: 3855 RVA: 0x0006B268 File Offset: 0x00069468
			public bool GenerateData(byte[] rom, uint startAddress)
			{
				checked
				{
					int num = (int)(this.PersonCount * 24);
					int num2 = (int)(this.WarpCount * 8);
					int num3 = (int)(this.TrapCount * 16);
					int num4 = (int)(this.SignCount * 12);
					int num11 = 20 + num + num2 + num3 + num4;
					bool flag5 = rom == null || unchecked((ulong)startAddress) + (ulong)num11 > (ulong)((long)rom.Length);
					if (flag5)
					{
						return false;
					}
					uint num5 = (uint)(unchecked((ulong)startAddress) + 20UL);
					uint num6 = num5;
					num5 += (uint)num;
					uint num7 = num5;
					num5 += (uint)num2;
					uint num8 = num5;
					num5 += (uint)num3;
					uint num9 = num5;
					int num10 = (int)startAddress;
					Array.Clear(rom, num10, 20);
					rom[num10 + 0] = this.PersonCount;
					rom[num10 + 1] = this.WarpCount;
					rom[num10 + 2] = this.TrapCount;
					rom[num10 + 3] = this.SignCount;
					this.WritePointer(rom, num10 + 4, num6);
					this.WritePointer(rom, num10 + 8, num7);
					this.WritePointer(rom, num10 + 12, num8);
					this.WritePointer(rom, num10 + 16, num9);
					bool flag = num > 0;
					if (flag)
					{
						Array.Clear(rom, (int)num6, num);
					}
					bool flag2 = num2 > 0;
					if (flag2)
					{
						Array.Clear(rom, (int)num7, num2);
					}
					bool flag3 = num3 > 0;
					if (flag3)
					{
						Array.Clear(rom, (int)num8, num3);
					}
					bool flag4 = num4 > 0;
					if (flag4)
					{
						Array.Clear(rom, (int)num9, num4);
					}
					this.HeaderAddress = startAddress;
					return true;
				}
			}

			// Token: 0x06000F10 RID: 3856 RVA: 0x0006B3A8 File Offset: 0x000695A8
			private void WritePointer(byte[] rom, int offset, uint addr)
			{
				uint num = (((ulong)addr != 0UL) ? checked(addr + 134217728U) : 0U);
				Array.Copy(BitConverter.GetBytes(num), 0, rom, offset, 4);
			}

			// Token: 0x170005B2 RID: 1458
			// (get) Token: 0x06000F11 RID: 3857 RVA: 0x0006B3D8 File Offset: 0x000695D8
			public string Description
			{
				get
				{
					return "Event Header and Data";
				}
			}
		}

		// Token: 0x0200003D RID: 61
		public class MapScriptGenerator : NewDataGenerator.INewDataGenerator
		{
			// Token: 0x170005B3 RID: 1459
			// (get) Token: 0x06000F13 RID: 3859 RVA: 0x0006B3F7 File Offset: 0x000695F7
			// (set) Token: 0x06000F14 RID: 3860 RVA: 0x0006B401 File Offset: 0x00069601
			public bool HasType01 { get; set; }

			// Token: 0x170005B4 RID: 1460
			// (get) Token: 0x06000F15 RID: 3861 RVA: 0x0006B40A File Offset: 0x0006960A
			// (set) Token: 0x06000F16 RID: 3862 RVA: 0x0006B414 File Offset: 0x00069614
			public bool HasType02 { get; set; }

			// Token: 0x170005B5 RID: 1461
			// (get) Token: 0x06000F17 RID: 3863 RVA: 0x0006B41D File Offset: 0x0006961D
			// (set) Token: 0x06000F18 RID: 3864 RVA: 0x0006B427 File Offset: 0x00069627
			public bool HasType03 { get; set; }

			// Token: 0x170005B6 RID: 1462
			// (get) Token: 0x06000F19 RID: 3865 RVA: 0x0006B430 File Offset: 0x00069630
			// (set) Token: 0x06000F1A RID: 3866 RVA: 0x0006B43A File Offset: 0x0006963A
			public bool HasType04 { get; set; }

			// Token: 0x170005B7 RID: 1463
			// (get) Token: 0x06000F1B RID: 3867 RVA: 0x0006B443 File Offset: 0x00069643
			// (set) Token: 0x06000F1C RID: 3868 RVA: 0x0006B44D File Offset: 0x0006964D
			public bool HasType05 { get; set; }

			// Token: 0x170005B8 RID: 1464
			// (get) Token: 0x06000F1D RID: 3869 RVA: 0x0006B456 File Offset: 0x00069656
			// (set) Token: 0x06000F1E RID: 3870 RVA: 0x0006B460 File Offset: 0x00069660
			public bool HasType06 { get; set; }

			// Token: 0x170005B9 RID: 1465
			// (get) Token: 0x06000F1F RID: 3871 RVA: 0x0006B469 File Offset: 0x00069669
			// (set) Token: 0x06000F20 RID: 3872 RVA: 0x0006B473 File Offset: 0x00069673
			public bool HasType07 { get; set; }

			// Token: 0x170005BA RID: 1466
			// (get) Token: 0x06000F21 RID: 3873 RVA: 0x0006B47C File Offset: 0x0006967C
			// (set) Token: 0x06000F22 RID: 3874 RVA: 0x0006B486 File Offset: 0x00069686
			public int Type02Count { get; set; }

			// Token: 0x170005BB RID: 1467
			// (get) Token: 0x06000F23 RID: 3875 RVA: 0x0006B48F File Offset: 0x0006968F
			// (set) Token: 0x06000F24 RID: 3876 RVA: 0x0006B499 File Offset: 0x00069699
			public int Type04Count { get; set; }

			// Token: 0x170005BC RID: 1468
			// (get) Token: 0x06000F25 RID: 3877 RVA: 0x0006B4A2 File Offset: 0x000696A2
			// (set) Token: 0x06000F26 RID: 3878 RVA: 0x0006B4AC File Offset: 0x000696AC
			public uint HeaderAddress { get; set; }

			// Token: 0x06000F27 RID: 3879 RVA: 0x0006B4B8 File Offset: 0x000696B8
			public bool GenerateData(byte[] rom, uint startAddress)
			{
				int num = 0;
				bool hasType = this.HasType01;
				checked
				{
					if (hasType)
					{
						num++;
					}
					bool hasType2 = this.HasType02;
					if (hasType2)
					{
						num++;
					}
					bool hasType3 = this.HasType03;
					if (hasType3)
					{
						num++;
					}
					bool hasType4 = this.HasType04;
					if (hasType4)
					{
						num++;
					}
					bool hasType5 = this.HasType05;
					if (hasType5)
					{
						num++;
					}
					bool hasType6 = this.HasType06;
					if (hasType6)
					{
						num++;
					}
					bool hasType7 = this.HasType07;
					if (hasType7)
					{
						num++;
					}
					int num2 = num * 5 + 1;
					uint num3 = startAddress + (uint)num2;
					int num4 = (int)startAddress;
					int num5 = num4;
					Array.Clear(rom, num4, num2);
					bool hasType8 = this.HasType01;
					if (hasType8)
					{
						this.ProcessType(rom, ref num5, ref num3, 1, false, 0);
					}
					bool hasType9 = this.HasType02;
					if (hasType9)
					{
						this.ProcessType(rom, ref num5, ref num3, 2, true, this.Type02Count);
					}
					bool hasType10 = this.HasType03;
					if (hasType10)
					{
						this.ProcessType(rom, ref num5, ref num3, 3, false, 0);
					}
					bool hasType11 = this.HasType04;
					if (hasType11)
					{
						this.ProcessType(rom, ref num5, ref num3, 4, true, this.Type04Count);
					}
					bool hasType12 = this.HasType05;
					if (hasType12)
					{
						this.ProcessType(rom, ref num5, ref num3, 5, false, 0);
					}
					bool hasType13 = this.HasType06;
					if (hasType13)
					{
						this.ProcessType(rom, ref num5, ref num3, 6, false, 0);
					}
					bool hasType14 = this.HasType07;
					if (hasType14)
					{
						this.ProcessType(rom, ref num5, ref num3, 7, false, 0);
					}
					rom[num5] = 0;
					this.HeaderAddress = startAddress;
					return true;
				}
			}

			// Token: 0x06000F28 RID: 3880 RVA: 0x0006B630 File Offset: 0x00069830
			private void ProcessType(byte[] rom, ref int writeHeaderOffset, ref uint currentAddr, byte type, bool isList, int count)
			{
				rom[writeHeaderOffset] = type;
				checked
				{
					if (isList)
					{
						Array.Copy(BitConverter.GetBytes(currentAddr + 134217728U), 0, rom, writeHeaderOffset + 1, 4);
						int num = count * 8 + 2;
						Array.Clear(rom, (int)currentAddr, num);
						int num2 = count - 1;
						for (int i = 0; i <= num2; i++)
						{
							Array.Copy(BitConverter.GetBytes(16384), 0, rom, (int)currentAddr + i * 8, 2);
							Array.Copy(BitConverter.GetBytes(134217728U), 0, rom, (int)currentAddr + i * 8 + 4, 4);
						}
						currentAddr += (uint)num;
					}
					else
					{
						Array.Copy(BitConverter.GetBytes(134217728U), 0, rom, writeHeaderOffset + 1, 4);
					}
					writeHeaderOffset += 5;
				}
			}

			// Token: 0x170005BD RID: 1469
			// (get) Token: 0x06000F29 RID: 3881 RVA: 0x0006B6E8 File Offset: 0x000698E8
			public string Description
			{
				get
				{
					return "Map Script Data";
				}
			}
		}

		// Token: 0x0200003E RID: 62
		public class MapConnectionGenerator : NewDataGenerator.INewDataGenerator
		{
			// Token: 0x170005BE RID: 1470
			// (get) Token: 0x06000F2B RID: 3883 RVA: 0x0006B707 File Offset: 0x00069907
			// (set) Token: 0x06000F2C RID: 3884 RVA: 0x0006B711 File Offset: 0x00069911
			public byte ConnectionCount { get; set; }

			// Token: 0x170005BF RID: 1471
			// (get) Token: 0x06000F2D RID: 3885 RVA: 0x0006B71A File Offset: 0x0006991A
			// (set) Token: 0x06000F2E RID: 3886 RVA: 0x0006B724 File Offset: 0x00069924
			public uint HeaderAddress { get; set; }

			// Token: 0x06000F2F RID: 3887 RVA: 0x0006B730 File Offset: 0x00069930
			public bool GenerateData(byte[] rom, uint startAddress)
			{
				checked
				{
					int num = (int)(this.ConnectionCount * 12);
					int num2 = 8 + num;
					int num3 = (int)startAddress;
					int num4 = num3 + 8;
					Array.Clear(rom, num3, num2);
					rom[num3 + 0] = this.ConnectionCount;
					this.WritePointer(rom, num3 + 4, (uint)num4);
					this.HeaderAddress = startAddress;
					return true;
				}
			}

			// Token: 0x06000F30 RID: 3888 RVA: 0x0006B784 File Offset: 0x00069984
			private void WritePointer(byte[] rom, int offset, uint addr)
			{
				uint num = (((ulong)addr != 0UL) ? checked(addr + 134217728U) : 0U);
				Array.Copy(BitConverter.GetBytes(num), 0, rom, offset, 4);
			}

			// Token: 0x170005C0 RID: 1472
			// (get) Token: 0x06000F31 RID: 3889 RVA: 0x0006B7B4 File Offset: 0x000699B4
			public string Description
			{
				get
				{
					return "Map Connection Data";
				}
			}
		}
	}
}
