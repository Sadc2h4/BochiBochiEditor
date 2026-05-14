using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x02000026 RID: 38
	public sealed class TextConverter
	{
		// Token: 0x06000BA3 RID: 2979 RVA: 0x00056F0C File Offset: 0x0005510C
		public static void LoadCharTable(string fileName)
		{
			TextConverter.charTable.Clear();
			TextConverter.reverseCharTable = null;
			TextConverter.maxByteLength = 1;
			string text = AppAssetLocator.FindRequiredFile(Path.Combine("txt", fileName));
			string[] array = File.ReadAllLines(text, Encoding.UTF8);
			foreach (string text2 in array)
			{
				bool flag = string.IsNullOrWhiteSpace(text2) || text2.StartsWith(";");
				if (!flag)
				{
					string[] array3 = text2.Split(new char[] { '=' }, 2);
					bool flag2 = array3.Length == 2;
					if (flag2)
					{
						string text3 = array3[0].Trim().Replace(" ", "");
						string text4 = array3[1];
						text4 = text4.Replace("\\n", "\n").Replace("\\r", "\r");
						int num = text3.Length / 2;
						bool flag3 = num > TextConverter.maxByteLength;
						if (flag3)
						{
							TextConverter.maxByteLength = num;
						}
						bool flag4 = !TextConverter.charTable.ContainsKey(text3);
						if (flag4)
						{
							TextConverter.charTable.Add(text3, text4);
						}
						else
						{
							TextConverter.charTable[text3] = text4;
						}
					}
				}
			}
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x00057058 File Offset: 0x00055258
		public static string BytesToPokemonString(byte[] bytes, int offset, int maxLength = 12)
		{
			bool flag = bytes == null;
			checked
			{
				string text;
				if (flag)
				{
					text = string.Empty;
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = Math.Min(bytes.Length - offset, maxLength);
					int i = 0;
					while (i < num)
					{
						int num2 = offset + i;
						byte b = bytes[num2];
						bool flag2 = b == byte.MaxValue;
						if (flag2)
						{
							break;
						}
						bool flag3 = b == 254;
						if (flag3)
						{
							stringBuilder.Append(Environment.NewLine);
							i++;
						}
						else
						{
							bool flag4 = false;
							int num3 = TextConverter.maxByteLength;
							for (int j = num3; j >= 1; j += -1)
							{
								bool flag5 = i + j > num;
								if (!flag5)
								{
									string text2 = BitConverter.ToString(bytes, num2, j).Replace("-", "");
									bool flag6 = TextConverter.charTable.ContainsKey(text2);
									if (flag6)
									{
										stringBuilder.Append(TextConverter.charTable[text2]);
										i += j;
										flag4 = true;
										break;
									}
								}
							}
							bool flag7 = !flag4;
							if (flag7)
							{
								stringBuilder.Append(string.Format("[{0:X2}]", b));
								i++;
							}
						}
					}
					text = stringBuilder.ToString();
				}
				return text;
			}
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x00057190 File Offset: 0x00055390
		public static byte[] PokemonStringToBytes(string text, int maxLength = 11)
		{
			bool flag = string.IsNullOrEmpty(text);
			checked
			{
				byte[] array;
				if (flag)
				{
					array = new byte[] { byte.MaxValue };
				}
				else
				{
					List<byte> list = new List<byte>();
					Dictionary<string, byte[]> dictionary = TextConverter.GetReverseCharTable();
					int i = 0;
					while (i < text.Length)
					{
						bool flag2 = list.Count >= maxLength;
						if (flag2)
						{
							break;
						}
						bool flag3 = i < text.Length - 1 && Operators.CompareString(text.Substring(i, 2), Environment.NewLine, false) == 0;
						if (flag3)
						{
							list.Add(254);
							i += 2;
						}
						else
						{
							bool flag4 = Operators.CompareString(Conversions.ToString(text[i]), "\r", false) == 0 || Operators.CompareString(Conversions.ToString(text[i]), "\n", false) == 0;
							if (flag4)
							{
								list.Add(254);
								i++;
							}
							else
							{
								string text2 = text[i].ToString();
								bool flag5 = dictionary.ContainsKey(text2);
								if (flag5)
								{
									list.AddRange(dictionary[text2]);
								}
								else
								{
									bool flag6 = Operators.CompareString(text2, "\\", false) == 0 && i < text.Length - 1;
									if (flag6)
									{
										string text3 = text[i + 1].ToString();
										string text4 = "\\" + text3;
										bool flag7 = dictionary.ContainsKey(text4);
										if (flag7)
										{
											list.AddRange(dictionary[text4]);
											i += 2;
											continue;
										}
									}
									list.Add(0);
								}
								i++;
							}
						}
					}
					list.Add(byte.MaxValue);
					array = list.ToArray();
				}
				return array;
			}
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x0005734C File Offset: 0x0005554C
		public static byte[] ConvertPokedexCategoryToBytes(string categoryText)
		{
			bool flag = string.IsNullOrEmpty(categoryText);
			byte[] array;
			if (flag)
			{
				array = new byte[checked(MyProject.Forms.PokemonEditor.POKEDEX_CATEGORY_LENGTH - 1 + 1)];
			}
			else
			{
				List<byte> list = new List<byte>();
				Dictionary<string, byte[]> dictionary = TextConverter.GetReverseCharTable();
				foreach (char c in categoryText)
				{
					bool flag2 = list.Count >= MyProject.Forms.PokemonEditor.POKEDEX_CATEGORY_LENGTH;
					if (flag2)
					{
						break;
					}
					string text = c.ToString();
					bool flag3 = Operators.CompareString(text, "\u3000", false) == 0;
					if (flag3)
					{
						list.Add(0);
					}
					else
					{
						bool flag4 = dictionary.ContainsKey(text);
						if (flag4)
						{
							list.AddRange(dictionary[text]);
						}
						else
						{
							list.Add(0);
						}
					}
				}
				while (list.Count < MyProject.Forms.PokemonEditor.POKEDEX_CATEGORY_LENGTH)
				{
					list.Add(0);
				}
				array = list.ToArray();
			}
			return array;
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x00057468 File Offset: 0x00055668
		public static Dictionary<string, byte[]> GetReverseCharTable()
		{
			bool flag = TextConverter.reverseCharTable == null;
			checked
			{
				if (flag)
				{
					TextConverter.reverseCharTable = new Dictionary<string, byte[]>();
					{
						foreach (KeyValuePair<string, string> keyValuePair in TextConverter.charTable)
						{
							string key = keyValuePair.Key;
							byte[] array = new byte[key.Length / 2 - 1 + 1];
							int num = array.Length - 1;
							for (int i = 0; i <= num; i++)
							{
								array[i] = Convert.ToByte(key.Substring(i * 2, 2), 16);
							}
							bool flag2 = !TextConverter.reverseCharTable.ContainsKey(keyValuePair.Value);
							if (flag2)
							{
								TextConverter.reverseCharTable.Add(keyValuePair.Value, array);
							}
						}
					}
				}
				return TextConverter.reverseCharTable;
			}
		}

		// Token: 0x0400066A RID: 1642
		public static Dictionary<string, string> charTable = new Dictionary<string, string>();

		// Token: 0x0400066B RID: 1643
		public static Dictionary<string, byte[]> reverseCharTable = null;

		// Token: 0x0400066C RID: 1644
		public static int maxByteLength = 1;
	}
}
