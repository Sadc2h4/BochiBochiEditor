using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x0200000C RID: 12
	internal sealed class CryProcessor
	{
		// Token: 0x06000090 RID: 144 RVA: 0x000067CC File Offset: 0x000049CC
		public static Cry LoadCryFromAddress(uint cryAddress, byte[] romData)
		{
			Cry cry = new Cry();
			checked
			{
				cry.Offset = (int)cryAddress;
				cry.Compressed = BitConverter.ToInt16(romData, (int)cryAddress) == 1;
				cry.Looped = BitConverter.ToInt16(romData, (int)(unchecked((ulong)cryAddress) + 2UL)) == 16384;
				cry.SampleRate = BitConverter.ToInt32(romData, (int)(unchecked((ulong)cryAddress) + 4UL)) >> 10;
				cry.LoopStart = BitConverter.ToInt32(romData, (int)(unchecked((ulong)cryAddress) + 8UL));
				cry.Size = BitConverter.ToInt32(romData, (int)(unchecked((ulong)cryAddress) + 12UL)) + 1;
				cry.Data = CryProcessor.DecompressCryData(romData, (int)(unchecked((ulong)cryAddress) + 16UL), cry.Size);
				return cry;
			}
		}

		// Token: 0x06000091 RID: 145 RVA: 0x0000686C File Offset: 0x00004A6C
		public static sbyte[] DecompressCryData(byte[] romData, int startOffset, int expectedSize)
		{
			sbyte[] array = new sbyte[]
			{
				0, 1, 4, 9, 16, 25, 36, 49, -64, -49,
				-36, -25, -16, -9, -4, -1
			};
			List<sbyte> list = new List<sbyte>(expectedSize);
			int num = startOffset;
			int num2 = 0;
			sbyte b = 0;
			checked
			{
				while (list.Count < expectedSize)
				{
					bool flag = num2 == 0;
					if (flag)
					{
						bool flag2 = num < romData.Length;
						if (!flag2)
						{
							break;
						}
						byte b2 = romData[num];
						bool flag3 = b2 <= 127;
						if (flag3)
						{
							b = (sbyte)b2;
						}
						else
						{
							b = (sbyte)((int)b2 - 256);
						}
						list.Add(b);
						num++;
						num2 = 32;
					}
					bool flag4 = num >= romData.Length || list.Count >= expectedSize;
					if (flag4)
					{
						break;
					}
					byte b3 = romData[num];
					num++;
					bool flag5 = num2 < 32;
					if (flag5)
					{
						int num3 = (int)(unchecked((byte)((uint)b3 >> 4)));
						b = CryProcessor.SafeAddSByte(b, array[num3]);
						list.Add(b);
						bool flag6 = list.Count >= expectedSize;
						if (flag6)
						{
							break;
						}
					}
					int num4 = (int)(b3 & 15);
					b = CryProcessor.SafeAddSByte(b, array[num4]);
					list.Add(b);
					num2--;
				}
				return list.ToArray();
			}
		}

		// Token: 0x06000092 RID: 146 RVA: 0x000069A4 File Offset: 0x00004BA4
		public static sbyte SafeAddSByte(sbyte a, sbyte b)
		{
			checked
			{
				int num = (int)(a + b);
				bool flag = num > 127;
				sbyte b2;
				if (flag)
				{
					b2 = sbyte.MaxValue;
				}
				else
				{
					bool flag2 = num < -128;
					if (flag2)
					{
						b2 = sbyte.MinValue;
					}
					else
					{
						b2 = (sbyte)num;
					}
				}
				return b2;
			}
		}

		// Token: 0x06000093 RID: 147 RVA: 0x000069DC File Offset: 0x00004BDC
		public static void WriteWavToStream(Cry cry, Stream stream)
		{
			checked
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.ASCII, true))
				{
					binaryWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
					binaryWriter.Write(0);
					binaryWriter.Write(Encoding.ASCII.GetBytes("WAVE"));
					binaryWriter.Write(Encoding.ASCII.GetBytes("fmt "));
					binaryWriter.Write(16);
					binaryWriter.Write(1);
					binaryWriter.Write(1);
					binaryWriter.Write(cry.SampleRate);
					binaryWriter.Write(cry.SampleRate);
					binaryWriter.Write(1);
					binaryWriter.Write(8);
					binaryWriter.Write(Encoding.ASCII.GetBytes("data"));
					binaryWriter.Write(cry.Data.Length);
					foreach (sbyte b in cry.Data)
					{
						binaryWriter.Write((byte)((int)b + 128));
					}
					binaryWriter.Seek(4, SeekOrigin.Begin);
					binaryWriter.Write((int)(stream.Length - 8L));
				}
			}
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00006B14 File Offset: 0x00004D14
		public static void PlayCry(Cry cry)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				CryProcessor.WriteWavToStream(cry, memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				using (SoundPlayer soundPlayer = new SoundPlayer(memoryStream))
				{
					soundPlayer.Play();
				}
			}
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00006B84 File Offset: 0x00004D84
		public static void ExportCryToWav(Cry cry, string filename)
		{
			using (FileStream fileStream = File.Create(filename))
			{
				CryProcessor.WriteWavToStream(cry, fileStream);
			}
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00006BC0 File Offset: 0x00004DC0
		public static void PlayCryFromAddress(uint cryAddress, byte[] romData)
		{
			Cry cry = CryProcessor.LoadCryFromAddress(cryAddress, romData);
			CryProcessor.PlayCry(cry);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00006BE0 File Offset: 0x00004DE0
		public static void ExportCryFromAddress(uint cryAddress, byte[] romData, string pokemonCode)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = "WAVファイル|*.wav";
				saveFileDialog.Title = "鳴き声をエクスポート";
				saveFileDialog.FileName = string.Format("cry_{0}.wav", pokemonCode);
				bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					Cry cry = CryProcessor.LoadCryFromAddress(cryAddress, romData);
					CryProcessor.ExportCryToWav(cry, saveFileDialog.FileName);
				}
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00006C64 File Offset: 0x00004E64
		public static byte[] CompressCryData(sbyte[] data)
		{
			sbyte[] array = new sbyte[]
			{
				0, 1, 4, 9, 16, 25, 36, 49, -64, -49,
				-36, -25, -16, -9, -4, -1
			};
			int num = data.Length / 64;
			bool flag = data.Length % 64 > 0;
			checked
			{
				if (flag)
				{
					num++;
				}
				bool flag2 = data.Length % 64 == 0;
				int num2;
				if (flag2)
				{
					num2 = 33;
				}
				else
				{
					num2 = 1 + data.Length % 64 / 2 + ((data.Length % 64 % 2 == 0) ? 0 : 1);
				}
				byte[][] array2 = new byte[num - 1 + 1][];
				int num3 = num - 1;
				for (int i = 0; i <= num3; i++)
				{
					bool flag3 = i < num - 1;
					if (flag3)
					{
						array2[i] = new byte[33];
					}
					else
					{
						array2[i] = new byte[num2 - 1 + 1];
					}
					int num4 = i * 64;
					int num5 = 0;
					bool flag4 = num4 < data.Length;
					if (flag4)
					{
						array2[i][num5] = (byte)((int)data[num4] & 255);
					}
					num5++;
					sbyte b = 0;
					bool flag5 = num4 < data.Length;
					if (flag5)
					{
						b = data[num4];
					}
					num4++;
					int num6 = 1;
					while (num6 < 64 && num4 < data.Length)
					{
						sbyte b2 = data[num4];
						num4++;
						int num7 = (int)(b2 - b);
						int num8 = -1;
						int num9 = 0;
						do
						{
							bool flag6 = (int)array[num9] == num7 && b + array[num9] <= sbyte.MaxValue && b + array[num9] >= sbyte.MinValue;
							if (flag6)
							{
								goto Block_9;
							}
							num9++;
						}
						while (num9 <= 15);
						IL_0155:
						bool flag7 = num8 == -1;
						if (flag7)
						{
							int num10 = 255;
							int num11 = 0;
							do
							{
								int num12 = Math.Abs((int)array[num11] - num7);
								bool flag8 = num12 < num10 && b + array[num11] <= sbyte.MaxValue && b + array[num11] >= sbyte.MinValue;
								if (flag8)
								{
									num8 = num11;
									num10 = num12;
								}
								num11++;
							}
							while (num11 <= 15);
						}
						bool flag9 = num6 % 2 == 0;
						if (flag9)
						{
							array2[i][num5] = (byte)(array2[i][num5] | (byte)(num8 << 4));
						}
						else
						{
							array2[i][num5] = (byte)(array2[i][num5] | (byte)num8);
							num5++;
						}
						b += array[num8];
						num6++;
						continue;
						Block_9:
						num8 = num9;
						goto IL_0155;
					}
				}
				List<byte> list = new List<byte>();
				int num13 = num - 1;
				for (int j = 0; j <= num13; j++)
				{
					list.AddRange(array2[j]);
				}
				return list.ToArray();
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00006EDC File Offset: 0x000050DC
		public static Cry ImportAndCompressWav(string filename)
		{
			Cry cry = new Cry();
			using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(filename)))
			{
				string @string = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
				bool flag = Operators.CompareString(@string, "RIFF", false) != 0;
				if (flag)
				{
					MessageBox.Show("WAVEファイルではありません", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				int num = binaryReader.ReadInt32();
				bool flag2 = (long)(checked(num + 8)) != binaryReader.BaseStream.Length;
				if (flag2)
				{
					MessageBox.Show("ファイルサイズが不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				string string2 = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
				bool flag3 = Operators.CompareString(string2, "WAVE", false) != 0;
				if (flag3)
				{
					MessageBox.Show("WAVEファイルではありません", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				string string3 = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
				bool flag4 = Operators.CompareString(string3, "fmt ", false) != 0;
				if (flag4)
				{
					MessageBox.Show("fmtチャンクが見つかりません", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				int num2 = binaryReader.ReadInt32();
				bool flag5 = num2 != 16;
				if (flag5)
				{
					MessageBox.Show("不正なfmtチャンクです", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				short num3 = binaryReader.ReadInt16();
				bool flag6 = num3 != 1;
				if (flag6)
				{
					MessageBox.Show("PCM形式のWAVEファイルのみ対応しています", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				short num4 = binaryReader.ReadInt16();
				bool flag7 = num4 != 1;
				if (flag7)
				{
					MessageBox.Show("モノラルのWAVEファイルのみ対応しています", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				cry.SampleRate = binaryReader.ReadInt32();
				int num5 = binaryReader.ReadInt32();
				short num6 = binaryReader.ReadInt16();
				short num7 = binaryReader.ReadInt16();
				bool flag8 = num7 != 8;
				if (flag8)
				{
					MessageBox.Show("8ビットのWAVEファイルのみ対応しています", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				string string4 = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
				bool flag9 = Operators.CompareString(string4, "data", false) != 0;
				if (flag9)
				{
					MessageBox.Show("dataチャンクが見つかりません", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return null;
				}
				int num8 = binaryReader.ReadInt32();
				checked
				{
					cry.Data = new sbyte[num8 - 1 + 1];
					int num9 = num8 - 1;
					for (int i = 0; i <= num9; i++)
					{
						cry.Data[i] = (sbyte)(binaryReader.ReadByte() - 128);
					}
				}
			}
			cry.Compressed = true;
			cry.Looped = false;
			cry.LoopStart = 0;
			cry.Size = cry.Data.Length;
			return cry;
		}

		// Token: 0x0600009A RID: 154 RVA: 0x000071C0 File Offset: 0x000053C0
		public static void SaveCompressedCryToROM(Cry cry, uint address, byte[] romData)
		{
			byte[] array = CryProcessor.CompressCryData(cry.Data);
			ushort num = (ushort)(cry.Compressed ? 1 : 0);
			ushort num2 = (ushort)(cry.Looped ? 16384 : 0);
			checked
			{
				uint num3 = (uint)cry.SampleRate << 10;
				uint num4 = (uint)(cry.Data.Length - 1);
				int num5 = (int)address;
				byte[] bytes = BitConverter.GetBytes(num);
				romData[num5] = bytes[0];
				romData[num5 + 1] = bytes[1];
				num5 += 2;
				byte[] bytes2 = BitConverter.GetBytes(num2);
				romData[num5] = bytes2[0];
				romData[num5 + 1] = bytes2[1];
				num5 += 2;
				byte[] bytes3 = BitConverter.GetBytes(num3);
				int num6 = 0;
				do
				{
					romData[num5 + num6] = bytes3[num6];
					num6++;
				}
				while (num6 <= 3);
				num5 += 4;
				byte[] bytes4 = BitConverter.GetBytes(cry.LoopStart);
				int num7 = 0;
				do
				{
					romData[num5 + num7] = bytes4[num7];
					num7++;
				}
				while (num7 <= 3);
				num5 += 4;
				byte[] bytes5 = BitConverter.GetBytes(num4);
				int num8 = 0;
				do
				{
					romData[num5 + num8] = bytes5[num8];
					num8++;
				}
				while (num8 <= 3);
				num5 += 4;
				Array.Copy(array, 0, romData, num5, array.Length);
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x000072E0 File Offset: 0x000054E0
		public static void ImportAndSaveWavToAddress(string filename, uint address, byte[] romData)
		{
			Cry cry = CryProcessor.ImportAndCompressWav(filename);
			CryProcessor.SaveCompressedCryToROM(cry, address, romData);
		}
	}
}
