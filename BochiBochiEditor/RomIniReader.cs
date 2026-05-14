using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000025 RID: 37
	internal sealed class RomIniReader
	{
		// Token: 0x06000B9D RID: 2973 RVA: 0x00056C14 File Offset: 0x00054E14
		public static string ReadValue(string key)
		{
			string text = AppAssetLocator.FindRequiredFile(Path.Combine("ini", "Rom.ini"));
			foreach (string text2raw in File.ReadAllLines(text)) { string text2 = text2raw.Trim();
				bool flag = string.IsNullOrEmpty(text2) || text2.StartsWith(";");
				if (!flag)
				{
					bool flag2 = text2.StartsWith(key + " =");
					if (flag2)
					{
						string[] array2 = text2.Split(new char[] { '=' });
						bool flag3 = array2.Length >= 2;
						if (flag3)
						{
							return array2[1].Trim();
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x00056CD8 File Offset: 0x00054ED8
		public static int ReadHexOrDecimal(string key)
		{
			string text = RomIniReader.ReadValue(key);
			bool flag = text[0] == '*';
			int num3;
			if (flag)
			{
				string text2 = text.Substring(1).Trim();
				int num = RomIniReader.ParseHexOrDecimal(text2);
				uint num2 = BitConverter.ToUInt32(MainForm.romData, num);
				num3 = checked((int)(num2 - 134217728U));
			}
			else
			{
				bool flag2 = text.StartsWith("\"");
				if (flag2)
				{
					num3 = RomIniReader.SearchBinaryAndReadPointer(text);
				}
				else
				{
					num3 = RomIniReader.ParseHexOrDecimal(text);
				}
			}
			return num3;
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x00056D50 File Offset: 0x00054F50
		private static int ParseHexOrDecimal(string str)
		{
			str = str.Trim();
			bool flag = str.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
			int num;
			if (flag)
			{
				num = Convert.ToInt32(str, 16);
			}
			else
			{
				num = Convert.ToInt32(str);
			}
			return num;
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x00056D8C File Offset: 0x00054F8C
		public static bool ReadBoolean(string key)
		{
			string text = RomIniReader.ReadValue(key);
			bool flag = string.IsNullOrEmpty(text);
			return !flag && text.Equals("True", StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x00056DC0 File Offset: 0x00054FC0
		private static int SearchBinaryAndReadPointer(string valueStr)
		{
			string[] array = valueStr.Split(new char[] { ',' });
			string text = array[0].Trim().Trim(new char[] { '"' }).Replace(" ", "");
			int num = 0;
			bool flag = array.Length > 1;
			if (flag)
			{
				num = RomIniReader.ParseHexOrDecimal(array[1]);
			}
			checked
			{
				byte[] array2 = new byte[text.Length / 2 - 1 + 1];
				int num2 = array2.Length - 1;
				for (int i = 0; i <= num2; i++)
				{
					array2[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
				}
				byte[] romData = MainForm.romData;
				int num3 = -1;
				int num4 = romData.Length - array2.Length;
				for (int j = 0; j <= num4; j++)
				{
					bool flag2 = true;
					int num5 = array2.Length - 1;
					for (int k = 0; k <= num5; k++)
					{
						bool flag3 = romData[j + k] != array2[k];
						if (flag3)
						{
							flag2 = false;
							break;
						}
					}
					bool flag4 = flag2;
					if (flag4)
					{
						num3 = j + array2.Length;
						break;
					}
				}
				int num6 = num3 + num;
				uint num7 = BitConverter.ToUInt32(romData, num6);
				return (int)(num7 - 134217728U);
			}
		}
	}
}
