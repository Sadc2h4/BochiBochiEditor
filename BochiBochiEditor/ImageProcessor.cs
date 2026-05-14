using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	// Token: 0x02000010 RID: 16
	public sealed class ImageProcessor
	{
		//-------------------------------------------------------------------------------
		// GBA LZ77 形式データを展開する処理
		//-------------------------------------------------------------------------------
		public static int LZ77UnComp(byte[] Source, byte[] Dest)
		{
			int decompressedSize = ((int)Source[0] | ((int)Source[1] * 256) | ((int)Source[2] * 65536) | ((int)Source[3] * 16777216)) / 256;
			int srcPos = 4;
			int dstPos = 0;
			int remaining = decompressedSize;

			while (remaining > 0)
			{
				byte flags = Source[srcPos++];
				for (int bit = 0; bit < 8; bit++)
				{
					if (remaining <= 0) break;
					if ((flags & 0x80) != 0)
					{
						// 後方参照コピー
						int ref0 = (int)Source[srcPos] * 256 + (int)Source[srcPos + 1];
						srcPos += 2;
						int copyLen = ref0 / 4096 + 3;
						int disp = ref0 & 0xFFF;
						int copyFrom = dstPos - disp - 1;
						for (int i = 0; i < copyLen; i++)
						{
							if (copyFrom >= 0 && copyFrom < Dest.Length && dstPos < Dest.Length)
								Dest[dstPos] = Dest[copyFrom];
							dstPos++;
							copyFrom++;
							remaining--;
							if (remaining <= 0) break;
						}
					}
					else
					{
						// リテラルコピー
						if (srcPos < Source.Length && dstPos < Dest.Length)
							Dest[dstPos] = Source[srcPos];
						dstPos++;
						srcPos++;
						remaining--;
					}
					flags = (byte)((int)(flags * 2) % 256);
				}
			}
			return decompressedSize;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x000108E4 File Offset: 0x0000EAE4
		public static byte[] LZ77Comp(byte[] inputData, bool for16Bit = false)
		{
			List<byte> list = new List<byte>();
			list.Add(16);
			checked
			{
				list.Add((byte)(inputData.Length & 255));
				list.Add((byte)((inputData.Length >> 8) & 255));
				list.Add((byte)((inputData.Length >> 16) & 255));
				int i = 0;
				while (i < inputData.Length)
				{
					int count = list.Count;
					list.Add(0);
					byte b = 0;
					int num = 7;
					do
					{
						bool flag = i >= inputData.Length;
						if (flag)
						{
							break;
						}
						int num2 = 0;
						int num3 = 0;
						int num4 = Math.Min(i, 4096);
						int num5 = Math.Min(18, inputData.Length - i);
						bool flag2 = num4 > 0 && num5 >= 3;
						if (flag2)
						{
							int num6 = (for16Bit ? 2 : 1);
							int num7 = num6;
							int num8 = num4;
							for (int j = num7; j <= num8; j++)
							{
								int num9 = 0;
								while (num9 < num5 && inputData[i - j + num9] == inputData[i + num9])
								{
									num9++;
								}
								bool flag3 = num9 > num2;
								if (flag3)
								{
									num2 = num9;
									num3 = j;
								}
								bool flag4 = num2 == num5;
								if (flag4)
								{
									break;
								}
							}
						}
						bool flag5 = num2 >= 3;
						if (flag5)
						{
							b = (byte)((int)b | (1 << num));
							byte b2 = (byte)((num2 - 3 << 4) | ((num3 - 1 >> 8) & 15));
							byte b3 = (byte)((num3 - 1) & 255);
							list.Add(b2);
							list.Add(b3);
							i += num2;
						}
						else
						{
							list.Add(inputData[i]);
							i++;
						}
						num += -1;
					}
					while (num >= 0);
					IL_0199:
					list[count] = b;
					continue;
					goto IL_0199;
				}
				return list.ToArray();
			}
		}

		// Token: 0x0600020C RID: 524 RVA: 0x00010AB0 File Offset: 0x0000ECB0
		public static Color[] LoadPalette(byte[] Bits, bool makeFirstColorTransparent = true)
		{
			Color[] array = new Color[16];
			byte b = 0;
			do
			{
				checked
				{
					bool flag = (int)(b + 1) >= Bits.Length;
					if (flag)
					{
						break;
					}
					byte b2 = Bits[(int)b];
					byte b3 = Bits[(int)(b + 1)];
					ushort num = (ushort)((int)b3 * 256 + (int)b2);
					ushort num2 = (ushort)((num & 31) * 8);
					ushort num3 = (ushort)(((num & 992) >> 5) * 8);
					ushort num4 = (ushort)(((num & 31744) >> 10) * 8);
					bool flag2 = b == 0 && makeFirstColorTransparent;
					if (flag2)
					{
						array[(int)(b / 2)] = Color.FromArgb(0, (int)num2, (int)num3, (int)num4);
					}
					else
					{
						array[(int)(b / 2)] = Color.FromArgb(255, (int)num2, (int)num3, (int)num4);
					}
				}
				b += 2;
			}
			while (b <= 31);
			return array;
		}

		// Token: 0x0600020D RID: 525 RVA: 0x00010B78 File Offset: 0x0000ED78
		public static Bitmap LoadSprite(ref byte[] Bits, Color[] Palette, int Width = 64, int Height = 64, bool ShowBackColor = true)
		{
			Bitmap bitmap = new Bitmap(Width, Height);
			int num = 0;
			checked
			{
				int num2 = Height - 1;
				for (int i = 0; i <= num2; i += 8)
				{
					int num3 = Width - 1;
					for (int j = 0; j <= num3; j += 8)
					{
						int num4 = 0;
						for (;;)
						{
							int num5 = 0;
							do
							{
								bool flag = num >= Bits.Length;
								if (flag)
								{
									break;
								}
								byte b = Bits[num];
								if (ShowBackColor)
								{
									bitmap.SetPixel(j + num5 + 1, i + num4, Palette[(b & 240) >> 4]);
									bitmap.SetPixel(j + num5, i + num4, Palette[(int)(b & 15)]);
								}
								else
								{
									bool flag2 = Palette[(b & 240) >> 4] != Palette[0];
									if (flag2)
									{
										bitmap.SetPixel(j + num5 + 1, i + num4, Palette[(b & 240) >> 4]);
									}
									bool flag3 = Palette[(int)(b & 15)] != Palette[0];
									if (flag3)
									{
										bitmap.SetPixel(j + num5, i + num4, Palette[(int)(b & 15)]);
									}
								}
								num++;
								num5 += 2;
							}
							while (num5 <= 7);
							IL_0121:
							num4++;
							if (num4 > 7)
							{
								break;
							}
							continue;
							goto IL_0121;
						}
					}
				}
				return bitmap;
			}
		}

		// Token: 0x0600020E RID: 526 RVA: 0x00010CD4 File Offset: 0x0000EED4
		public static byte[] LoadCompressedImagePaletteFromROM(byte[] romData, uint address, bool isPalette)
		{
			checked
			{
				int num = BitConverter.ToInt32(romData, (int)address);
				int num2 = num >> 8;
				bool flag = num2 <= 0;
				byte[] array;
				if (flag)
				{
					array = new byte[0];
				}
				else
				{
					int num3 = (isPalette ? 1024 : 16384);
					num3 = (int)Math.Min(unchecked((long)num3), unchecked((long)romData.Length) - (long)(unchecked((ulong)address)));
					byte[] array2 = new byte[num3 - 1 + 1];
					unchecked
					{
						Array.Copy(romData, (long)((ulong)address), array2, 0L, (long)num3);
					}
					byte[] array3 = new byte[num2 - 1 + 1];
					ImageProcessor.LZ77UnComp(array2, array3);
					array = array3;
				}
				return array;
			}
		}

		// Token: 0x0600020F RID: 527 RVA: 0x00010D5C File Offset: 0x0000EF5C
		public static void DisplayGBASprite(PictureBox picBox, byte[] romData, uint imageAddress, uint paletteAddress, int width = 64, int height = 64, bool isImageCompressed = true, bool isPaletteCompressed = true)
		{
			try
			{
				byte[] array;
				if (isImageCompressed)
				{
					array = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, imageAddress, false);
				}
				else
				{
					int num;
					checked
					{
						num = (int)Math.Min(4096L, unchecked((long)romData.Length) - (long)(unchecked((ulong)imageAddress)));
						array = new byte[num - 1 + 1];
					}
					Array.Copy(romData, (long)((ulong)imageAddress), array, 0L, (long)num);
				}
				bool flag = array.Length == 0;
				if (flag)
				{
					picBox.Image = null;
				}
				else
				{
					byte[] array2;
					if (isPaletteCompressed)
					{
						array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, paletteAddress, true);
					}
					else
					{
						int num2;
						checked
						{
							num2 = (int)Math.Min(32L, unchecked((long)romData.Length) - (long)(unchecked((ulong)paletteAddress)));
							array2 = new byte[num2 - 1 + 1];
						}
						Array.Copy(romData, (long)((ulong)paletteAddress), array2, 0L, (long)num2);
					}
					bool flag2 = array2.Length < 32;
					if (flag2)
					{
						picBox.Image = null;
					}
					else
					{
						Color[] array3 = ImageProcessor.LoadPalette(array2, true);
						Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, width, height, false);
						picBox.Image = bitmap;
						picBox.Refresh();
					}
				}
			}
			catch (Exception ex)
			{
				picBox.Image = null;
			}
		}

		// Token: 0x06000210 RID: 528 RVA: 0x00010E78 File Offset: 0x0000F078
		public static void ExportSpriteTo4bppPng(string filePath, byte[] imageData, Color[] palette, int width, int height)
		{
			checked
			{
				using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format4bppIndexed))
				{
					ColorPalette palette2 = bitmap.Palette;
					int num = Math.Min(15, palette.Length - 1);
					for (int i = 0; i <= num; i++)
					{
						palette2.Entries[i] = palette[i];
					}
					bitmap.Palette = palette2;
					Rectangle rectangle = new Rectangle(0, 0, width, height);
					BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, bitmap.PixelFormat);
					int stride = bitmapData.Stride;
					byte[] array = new byte[stride * height - 1 + 1];
					int num2 = 0;
					bool flag = true;
					int num3 = height - 1;
					for (int j = 0; j <= num3; j += 8)
					{
						int num4 = width - 1;
						int k = 0;
						while (k <= num4)
						{
							int num5 = 0;
							for (;;)
							{
								int num6 = 0;
								do
								{
									bool flag2 = num2 >= imageData.Length;
									if (flag2)
									{
										goto Block_4;
									}
									byte b = imageData[num2];
									byte b2 = (byte)(b & 15);
									byte b3 = unchecked((byte)((uint)b >> 4));
									int num7 = k + num6;
									int num8 = j + num5;
									int num9 = num8 * stride + num7 / 2;
									array[num9] = (byte)(unchecked((byte)(b2 << 4)) | b3);
									num2++;
									num6 += 2;
								}
								while (num6 <= 7);
								IL_010F:
								bool flag3 = !flag;
								if (flag3)
								{
									break;
								}
								num5++;
								if (num5 > 7)
								{
									break;
								}
								continue;
								Block_4:
								flag = false;
								goto IL_010F;
							}
							IL_0127:
							bool flag4 = !flag;
							if (flag4)
							{
								break;
							}
							k += 8;
							continue;
							goto IL_0127;
						}
						bool flag5 = !flag;
						if (flag5)
						{
							break;
						}
					}
					Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
					bitmap.UnlockBits(bitmapData);
					bitmap.Save(filePath, ImageFormat.Png);
				}
			}
		}

		// Token: 0x06000211 RID: 529 RVA: 0x00011038 File Offset: 0x0000F238
		public static byte[] ImportSpriteFrom4bppPng(Bitmap bmp)
		{
			int width = bmp.Width;
			int height = bmp.Height;
			Rectangle rectangle = new Rectangle(0, 0, width, height);
			BitmapData bitmapData = bmp.LockBits(rectangle, ImageLockMode.ReadOnly, bmp.PixelFormat);
			int stride = bitmapData.Stride;
			int num = Math.Abs(stride);
			checked
			{
				int num2 = num * height;
				byte[] array = new byte[num2 - 1 + 1];
				Marshal.Copy(bitmapData.Scan0, array, 0, num2);
				List<byte> list = new List<byte>();
				int num3 = height - 1;
				for (int i = 0; i <= num3; i += 8)
				{
					int num4 = width - 1;
					for (int j = 0; j <= num4; j += 8)
					{
						int num5 = 0;
						do
						{
							int num6 = 0;
							do
							{
								int num7 = j + num6;
								int num8 = i + num5;
								bool flag = stride < 0;
								int num9;
								if (flag)
								{
									num9 = (height - 1 - num8) * num;
								}
								else
								{
									num9 = num8 * num;
								}
								int num10 = num9 + num7 / 2;
								bool flag2 = num10 < 0 || num10 >= array.Length;
								if (flag2)
								{
									list.Add(0);
								}
								else
								{
									byte b = array[num10];
									int num11 = (int)(unchecked((byte)((uint)b >> 4)) & 15);
									int num12 = (int)(b & 15);
									byte b2 = (byte)((num12 << 4) | num11);
									list.Add(b2);
								}
								num6 += 2;
							}
							while (num6 <= 7);
							num5++;
						}
						while (num5 <= 7);
					}
				}
				return list.ToArray();
			}
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0001119C File Offset: 0x0000F39C
		public static Bitmap Decode1BppFootprintSprite(ref byte[] Bits, Color[] Palette)
		{
			Bitmap bitmap = new Bitmap(16, 16);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.Transparent);
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			checked
			{
				do
				{
					int num4 = num3 % 2 * 8;
					int num5 = num3 / 2 * 8;
					int num6 = 0;
					for (;;)
					{
						int num7 = 0;
						do
						{
							bool flag = num >= Bits.Length;
							if (flag)
							{
								break;
							}
							BitArray bitArray = new BitArray(new byte[] { Bits[num] });
							bool flag2 = bitArray[num2];
							bitmap.SetPixel(num4 + num7, num5 + num6, flag2 ? Palette[1] : Palette[0]);
							num2++;
							bool flag3 = num2 == 8;
							if (flag3)
							{
								num2 = 0;
								num++;
							}
							num7++;
						}
						while (num7 <= 7);
						IL_00C4:
						num6++;
						if (num6 > 7)
						{
							break;
						}
						continue;
						goto IL_00C4;
					}
					num3++;
				}
				while (num3 <= 3);
				return bitmap;
			}
		}

		// Token: 0x06000213 RID: 531 RVA: 0x000112A0 File Offset: 0x0000F4A0
		public static byte[] EncodeImageToFootprintData(Bitmap image)
		{
			byte[] array = new byte[32];
			Array.Clear(array, 0, array.Length);
			int num = 0;
			checked
			{
				do
				{
					int num2 = 0;
					do
					{
						int num3 = num2 * 8;
						int num4 = num * 8;
						int num5 = num * 2 + num2;
						int num6 = num5 * 8;
						int num7 = 0;
						do
						{
							byte b = 0;
							int num8 = 0;
							do
							{
								int num9 = num3 + num8;
								int num10 = num4 + num7;
								bool flag = (num9 < 16) & (num10 < 16);
								if (flag)
								{
									Color pixel = image.GetPixel(num9, num10);
									bool flag2 = (pixel.R < 128) & (pixel.G < 128) & (pixel.B < 128);
									if (flag2)
									{
										b |= (byte)(1 << num8);
									}
								}
								num8++;
							}
							while (num8 <= 7);
							bool flag3 = num6 + num7 < array.Length;
							if (flag3)
							{
								array[num6 + num7] = b;
							}
							num7++;
						}
						while (num7 <= 7);
						num2++;
					}
					while (num2 <= 1);
					num++;
				}
				while (num <= 1);
				return array;
			}
		}

		// Token: 0x06000214 RID: 532 RVA: 0x000113A4 File Offset: 0x0000F5A4
		public static byte[] ConvertPaletteToBytes(ColorPalette palette)
		{
			byte[] array = new byte[32];
			checked
			{
				int num = Math.Min(15, palette.Entries.Length - 1);
				for (int i = 0; i <= num; i++)
				{
					Color color = palette.Entries[i];
					int num2;
					int num3;
					int num4;
					unchecked
					{
						num2 = (int)((byte)((uint)color.R >> 3));
						num3 = (int)((byte)((uint)color.G >> 3));
						num4 = (int)((byte)((uint)color.B >> 3));
					}
					ushort num5 = (ushort)((num4 << 10) | (num3 << 5) | num2);
					byte[] bytes = BitConverter.GetBytes(num5);
					array[i * 2] = bytes[0];
					array[i * 2 + 1] = bytes[1];
				}
				return array;
			}
		}
	}
}
