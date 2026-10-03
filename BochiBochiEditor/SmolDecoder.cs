using System;
using System.IO;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// pokeemerald-expansion の圧縮形式（smol）を展開する処理
	// 新しい pokeemerald-expansion でビルドした ROM は、タイルセットの絵などを LZ77 ではなくこの形式で持つ。
	// 形式（データの並び）は pokeemerald-expansion のもの。展開の手順は、Hex Maniac Advance の改造ビルド（MIT ライセンス）の実装で確かめた。
	//
	// 先頭 8 バイトの見出し:
	//   1 語目: 下位 4 ビット = 方式（1〜6）、次の 14 ビット × 4 = 展開後のバイト数、上位 14 ビット = 記号（2 バイト単位）の数
	//   2 語目: 下位 6 ビット = 符号の初めの状態、次の 13 ビット = ビット列の長さ（4 バイト単位）、上位 13 ビット = 命令のバイト数
	// 方式: 4 以上なら命令が符号化されている。2・3・5・6 なら記号が符号化されている。3・6 は記号が差分になっている。
	// 本体: [命令の頻度表 12 バイト][記号の頻度表 12 バイト][ビット列][符号化していない記号][符号化していない命令]（無いものは詰める）
	// 命令は「長さ・位置」の組（1〜2 バイトの可変長の数）。長さ 0 = 位置の数だけ記号をそのまま写す。
	// 長さ 1 以上 = 記号を 1 つ写してから、位置 × 2 バイト前から 長さ × 2 バイトを写す
	//-------------------------------------------------------------------------------
	internal static class SmolDecoder
	{
		// 符号の状態の数（6 ビット）
		private const int StateCount = 64;

		//-------------------------------------------------------------------------------
		// その場所が smol の見出しらしいかを調べる処理（LZ77 は先頭が 0x10 なので、方式 1〜6 とは重ならない）
		//-------------------------------------------------------------------------------
		public static bool LooksLikeSmol(byte[] rom, long offset)
		{
			if (rom == null || offset < 0 || (offset & 3) != 0 || offset + 8 > rom.Length)
			{
				return false;
			}
			uint head = BitConverter.ToUInt32(rom, (int)offset);
			uint head2 = BitConverter.ToUInt32(rom, (int)offset + 4);
			int mode = (int)(head & 15);
			int size = (int)((head >> 4) & 0x3FFF) * 4;
			int symbols = (int)(head >> 18);
			int instructions = (int)(head2 >> 19);
			return mode >= 1 && mode <= 6 && size > 0 && symbols > 0 && instructions > 0;
		}

		//-------------------------------------------------------------------------------
		// 展開後のバイト数を返す処理
		//-------------------------------------------------------------------------------
		public static int DecodedSize(byte[] rom, long offset)
		{
			return (int)((BitConverter.ToUInt32(rom, (int)offset) >> 4) & 0x3FFF) * 4;
		}

		//-------------------------------------------------------------------------------
		// 頻度表（12 バイト: 6 ビット × 15 個と、各語の上位 2 ビットを合わせた 16 個目）から、状態ごとの「記号・読むビット数・次の状態の基準」を作る処理
		//-------------------------------------------------------------------------------
		private static void BuildTable(byte[] rom, int offset, int[] symbol, int[] bits, int[] next)
		{
			int[] frequency = new int[16];
			for (int word = 0; word < 3; word++)
			{
				uint value = BitConverter.ToUInt32(rom, offset + word * 4);
				for (int j = 0; j < 5; j++)
				{
					frequency[word * 5 + j] = (int)((value >> (6 * j)) & 63);
				}
				frequency[15] |= (int)((value >> 30) << (2 * word));
			}
			int position = 0;
			for (int s = 0; s < 16; s++)
			{
				for (int n = frequency[s]; n < 2 * frequency[s]; n++)
				{
					int k = 0;
					while ((n << k) < StateCount)
					{
						k++;
					}
					if (position >= StateCount)
					{
						throw new InvalidDataException("smol: frequency table is too long");
					}
					symbol[position] = s;
					bits[position] = k;
					next[position] = (n << k) - StateCount;
					position++;
				}
			}
			if (position != StateCount)
			{
				throw new InvalidDataException("smol: frequency table does not fill the states");
			}
		}

		//-------------------------------------------------------------------------------
		// smol のデータを展開して返す処理（形が合わなければ InvalidDataException）
		//-------------------------------------------------------------------------------
		public static byte[] Decode(byte[] rom, long start)
		{
			if (!LooksLikeSmol(rom, start))
			{
				throw new InvalidDataException("smol: not a smol stream");
			}
			int p = (int)start;
			uint head = BitConverter.ToUInt32(rom, p);
			uint head2 = BitConverter.ToUInt32(rom, p + 4);
			int mode = (int)(head & 15);
			int size = (int)((head >> 4) & 0x3FFF) * 4;
			int symbolCount = (int)(head >> 18);
			int instructionCount = (int)(head2 >> 19);
			int bitWords = (int)((head2 >> 6) & 0x1FFF);
			int state = (int)(head2 & 63);
			bool instructionsCoded = mode >= 4;
			bool symbolsCoded = mode == 2 || mode == 3 || mode == 5 || mode == 6;
			bool delta = mode == 3 || mode == 6;

			int payload = p + 8;
			int bitStart = payload + (instructionsCoded ? 12 : 0) + (symbolsCoded ? 12 : 0);
			int rawStart = bitStart + ((instructionsCoded || symbolsCoded) ? bitWords * 4 : 0);
			int rawLength = (symbolsCoded ? 0 : symbolCount * 2) + (instructionsCoded ? 0 : instructionCount);
			if ((long)rawStart + rawLength > rom.Length)
			{
				throw new InvalidDataException("smol: stream runs past the end of the ROM");
			}
			int bit = 0;
			int bitLimit = bitWords * 32;

			// 符号化された 4 ビットの記号を count 個読む処理（差分なら前の値に足していく）
			Func<int, int, bool, byte[]> decodeNibbles = (tableOffset, count, isDelta) =>
			{
				int[] symbol = new int[StateCount];
				int[] bits = new int[StateCount];
				int[] next = new int[StateCount];
				BuildTable(rom, tableOffset, symbol, bits, next);
				byte[] output = new byte[(count + 1) / 2];
				int previous = 0;
				for (int n = 0; n < count; n++)
				{
					if (state < 0 || state >= StateCount)
					{
						throw new InvalidDataException("smol: invalid state");
					}
					int value = symbol[state];
					if (isDelta)
					{
						previous = (previous + value) & 15;
						value = previous;
					}
					output[n / 2] |= (byte)(value << ((n % 2) * 4));
					int read = 0;
					for (int k = 0; k < bits[state]; k++)
					{
						if (bit >= bitLimit)
						{
							throw new InvalidDataException("smol: bit stream exhausted");
						}
						read |= ((rom[bitStart + bit / 8] >> (bit % 8)) & 1) << k;
						bit++;
					}
					state = next[state] + read;
				}
				return output;
			};

			byte[] instructions = instructionsCoded ? decodeNibbles(payload, instructionCount * 2, false) : null;
			byte[] symbols = symbolsCoded ? decodeNibbles(payload + (instructionsCoded ? 12 : 0), symbolCount * 4, delta) : null;
			int raw = rawStart;
			if (!symbolsCoded)
			{
				symbols = new byte[symbolCount * 2];
				Array.Copy(rom, raw, symbols, 0, symbols.Length);
				raw += symbols.Length;
			}
			if (!instructionsCoded)
			{
				instructions = new byte[instructionCount];
				Array.Copy(rom, raw, instructions, 0, instructionCount);
			}

			byte[] result = new byte[size];
			int ip = 0;
			int sp = 0;
			int rp = 0;
			// 命令の中の数（7 ビット。上位ビットが立っていれば次のバイトが上の桁）を読む処理
			Func<int> number = () =>
			{
				if (ip >= instructions.Length)
				{
					throw new InvalidDataException("smol: truncated instruction");
				}
				int value = instructions[ip++];
				if ((value & 0x80) == 0)
				{
					return value;
				}
				if (ip >= instructions.Length)
				{
					throw new InvalidDataException("smol: truncated number");
				}
				return (value & 0x7F) | (instructions[ip++] << 7);
			};
			// 記号を 1 つ（2 バイト）そのまま写す処理
			Action literal = () =>
			{
				if (sp + 2 > symbols.Length || rp + 2 > size)
				{
					throw new InvalidDataException("smol: literal overflow");
				}
				result[rp++] = symbols[sp++];
				result[rp++] = symbols[sp++];
			};
			while (ip < instructions.Length)
			{
				int length = number();
				int distance = number();
				if (length == 0)
				{
					for (int i = 0; i < distance; i++)
					{
						literal();
					}
					continue;
				}
				literal();
				if (distance < 1 || distance * 2 > rp || rp + length * 2 > size)
				{
					throw new InvalidDataException("smol: copy overflow");
				}
				for (int i = 0; i < length * 2; i++)
				{
					result[rp] = result[rp - distance * 2];
					rp++;
				}
			}
			if (rp != size)
			{
				throw new InvalidDataException("smol: size mismatch");
			}
			return result;
		}
	}
}
