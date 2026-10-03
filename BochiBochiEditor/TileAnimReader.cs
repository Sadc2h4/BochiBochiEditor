using System;
using System.Collections.Generic;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// タイルアニメ 1 つ（どのタイルから何枚を、どのコマの絵で、何フレームごとに差し替えるか）
	//-------------------------------------------------------------------------------
	internal sealed class TileAnimation
	{
		// 差し替える先頭のタイル番号（第 1 タイルセットの先頭を 0 とした通し番号。VRAM の並びと同じ）
		public int DestTile;
		// 差し替えるタイルの枚数
		public int TileCount;
		// コマごとの絵の場所（ROM の中の位置）。同じ絵が何度か出てくることもある
		public uint[] Frames = new uint[0];
		// 何フレーム（1/60 秒）ごとに次のコマへ進むか
		public int Divisor = 16;
		// コマのずれ（差し替え先を表から選ぶアニメで、k 番目の差し替え先は k コマ遅れて進む）
		public int Delay;

		//-------------------------------------------------------------------------------
		// ゲームのタイマーの値のときに出すコマの番号を返す処理
		//-------------------------------------------------------------------------------
		public int FrameAt(long timer)
		{
			if (this.Frames.Length == 0)
			{
				return 0;
			}
			long step = timer / Math.Max(1, this.Divisor) - this.Delay;
			return (int)(((step % this.Frames.Length) + this.Frames.Length) % this.Frames.Length);
		}
	}

	//-------------------------------------------------------------------------------
	// タイルセットの見出しの「アニメ処理」（ゲーム本体のプログラム）を読んで、タイルアニメの定義を取り出す処理
	//
	// ゲーム（pokefirered・pokeemerald と同じ作り）は、次の 3 段で動いている。
	//   1. 初期化（見出しが指す処理）: 毎フレーム呼ぶ処理（TilesetAnim_XXX）を変数に入れる
	//   2. 毎フレームの処理: タイマーの値を見て、差し替え処理（QueueAnimTiles_XXX）を呼ぶ（timer / 16 などを渡す）
	//   3. 差し替え処理: コマの表（絵へのポインタの並び）[i % コマ数] を、VRAM の決まった場所へ決まった大きさで写す
	//      （エメラルドのカナズミ・キンセツ・サイユウは、差し替え先の表 [timer % 8] へ、timer % 8 コマ遅れた絵を写す）
	// プログラムを命令単位で少しだけ読み（THUMB の ldr・bl・mov・lsl・lsr だけ）、定数（リテラル）から
	// コマの表・VRAM の場所・大きさを、毎フレームの処理の割り算から速さを拾う。
	// 日本語版・英語版・改造版でアドレスが違っても、同じ作りなら読める。読めないアニメ（パレットを変えるものなど）は飛ばす。
	// キンセツの花は、2 周目から別の絵（_B）になるが、1 周目の絵だけで動かす
	//-------------------------------------------------------------------------------
	internal static class TileAnimReader
	{
		// VRAM の BG のタイルの先頭
		private const uint VramBase = 0x06000000;
		// 1 つの処理で読む命令の数の上限（読み違えたときに遠くまで読まないため）
		private const int InitLimit = 48;
		private const int CallbackLimit = 256;
		private const int QueueLimit = 80;
		// コマの数・タイルの枚数の上限
		private const int MaxFrames = 32;
		private const int MaxTiles = 256;

		// 命令 1 つ（種類・使うレジスタ・値・命令の場所）
		private struct Op
		{
			public int Pc;
			public char Kind;   // L = ldr（リテラル）、B = bl、M = mov 即値、S = lsl、R = lsr
			public int Rd;
			public int Rm;
			public uint Value;
		}

		//-------------------------------------------------------------------------------
		// 見出しの「アニメ処理」の場所（ROM の中の位置。THUMB の印の 1 が付いていてもよい）から、アニメの一覧を作る処理
		// 読めなければ空の一覧
		//-------------------------------------------------------------------------------
		public static List<TileAnimation> Read(byte[] rom, uint callbackOffset)
		{
			List<TileAnimation> result = new List<TileAnimation>();
			try
			{
				if (rom == null || callbackOffset == 0U || (callbackOffset & ~1U) >= rom.Length)
				{
					return result;
				}
				int init = (int)(callbackOffset & ~1U);
				// 1. 初期化の処理の定数のうち、ROM の中の THUMB の処理（奇数のアドレス）が、毎フレームの処理
				foreach (Op op in Decode(rom, init, InitLimit).Where(o => o.Kind == 'L'))
				{
					if ((op.Value & 1U) == 0U || !IsRom(rom, op.Value) || (int)(op.Value & ~1U) - 0x08000000 == init)
					{
						continue;
					}
					ReadCallback(rom, (int)((op.Value & ~1U) - 0x08000000), result);
				}
			}
			catch (Exception)
			{
				// 読み違えて範囲の外を読んだときなどは、そこまでに読めた分だけを返す
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 毎フレームの処理を読み、呼んでいる差し替え処理ごとにアニメを作る処理
		//-------------------------------------------------------------------------------
		private static void ReadCallback(byte[] rom, int start, List<TileAnimation> result)
		{
			List<Op> body = Decode(rom, start, CallbackLimit);
			// 差し替え処理と、その前に決まる速さ
			List<KeyValuePair<int, int>> calls = new List<KeyValuePair<int, int>>();
			for (int i = 0; i < body.Count; i++)
			{
				if (body[i].Kind == 'B' && IsRom(rom, body[i].Value + 0x08000000U) && !calls.Any(c => c.Key == (int)body[i].Value))
				{
					calls.Add(new KeyValuePair<int, int>((int)body[i].Value, DivisorBefore(body, i)));
				}
			}
			// 差し替え処理ごとの「コマの表・VRAM の場所・大きさ」
			List<Tuple<List<uint>, List<uint>, List<int>, int>> queues = new List<Tuple<List<uint>, List<uint>, List<int>, int>>();
			// 差し替え先を表から選ぶもの（差し替え先の表の一覧・コマの表の一覧・大きさ・速さ）
			List<Tuple<List<uint[]>, List<uint>, List<int>, int>> tableQueues = new List<Tuple<List<uint[]>, List<uint>, List<int>, int>>();
			HashSet<uint> arrays = new HashSet<uint>();
			foreach (KeyValuePair<int, int> call in calls)
			{
				List<Op> ops = Decode(rom, call.Key, QueueLimit);
				List<uint> vram = new List<uint>();
				List<uint> tables = new List<uint>();
				List<uint> destAddresses = new List<uint>();
				foreach (Op op in ops.Where(o => o.Kind == 'L'))
				{
					if (op.Value >= VramBase && op.Value < VramBase + 0x10000U && !vram.Contains(op.Value))
					{
						vram.Add(op.Value);
					}
					else if ((op.Value & 3U) == 0U && IsRom(rom, op.Value) && IsRom(rom, ReadU32(rom, (int)(op.Value - 0x08000000U))) && !tables.Contains(op.Value))
					{
						tables.Add(op.Value);
					}
					else if ((op.Value & 3U) == 0U && IsRom(rom, op.Value) && !destAddresses.Contains(op.Value) && IsVram(ReadU32(rom, (int)(op.Value - 0x08000000U))))
					{
						destAddresses.Add(op.Value);
					}
				}
				// 差し替え先の表も並んで置かれているので、次の表の手前で止める
				List<uint[]> destTables = new List<uint[]>();
				foreach (uint address in destAddresses)
				{
					uint limit = destAddresses.Where(a => a > address).DefaultIfEmpty(uint.MaxValue).Min();
					uint[] dests = ReadVramTable(rom, address, limit);
					if (dests.Length >= 2)
					{
						destTables.Add(dests);
					}
				}
				if (tables.Count == 0 || (vram.Count == 0 && destTables.Count == 0))
				{
					continue;
				}
				foreach (uint table in tables)
				{
					arrays.Add(table);
				}
				if (vram.Count > 0)
				{
					queues.Add(Tuple.Create(vram, tables, Sizes(ops), call.Value));
				}
				else
				{
					tableQueues.Add(Tuple.Create(destTables, tables, Sizes(ops), call.Value));
				}
			}
			// コマの表は、次のコマの表の手前まで（表が並んで置かれていることがあるため）
			List<uint> sortedArrays = arrays.OrderBy(a => a).ToList();
			foreach (Tuple<List<uint>, List<uint>, List<int>, int> queue in queues)
			{
				for (int i = 0; i < queue.Item1.Count; i++)
				{
					uint table = queue.Item2[Math.Min(i, queue.Item2.Count - 1)];
					uint[] frames = ReadFrames(rom, table, sortedArrays);
					if (frames.Length == 0)
					{
						continue;
					}
					int size = queue.Item3.Count > 0 ? queue.Item3[Math.Min(i, queue.Item3.Count - 1)] : FrameSpacing(frames);
					int tiles = size / 32;
					if (tiles <= 0 || tiles > MaxTiles || frames.Any(f => f + (uint)size > rom.Length))
					{
						continue;
					}
					result.Add(new TileAnimation
					{
						DestTile = (int)((queue.Item1[i] - VramBase) / 32U),
						TileCount = tiles,
						Frames = frames,
						Divisor = queue.Item4,
					});
				}
			}
			// 差し替え先の表ごとに、k 番目の差し替え先を k コマ遅らせたアニメにする（コマの表と差し替え先の表は、出てくる順に組にする）
			foreach (Tuple<List<uint[]>, List<uint>, List<int>, int> queue in tableQueues)
			{
				for (int t = 0; t < queue.Item1.Count; t++)
				{
					uint[] frames = ReadFrames(rom, queue.Item2[Math.Min(t, queue.Item2.Count - 1)], sortedArrays);
					int size = queue.Item3.Count > 0 ? queue.Item3[Math.Min(t, queue.Item3.Count - 1)] : FrameSpacing(frames);
					int tiles = size / 32;
					if (frames.Length == 0 || tiles <= 0 || tiles > MaxTiles || frames.Any(f => f + (uint)size > rom.Length))
					{
						continue;
					}
					for (int k = 0; k < queue.Item1[t].Length; k++)
					{
						result.Add(new TileAnimation
						{
							DestTile = (int)((queue.Item1[t][k] - VramBase) / 32U),
							TileCount = tiles,
							Frames = frames,
							Divisor = queue.Item4,
							Delay = k,
						});
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 差し替え先の表（VRAM の場所の並び）を読む処理（VRAM を指す値が続く間。16 個まで）
		//-------------------------------------------------------------------------------
		private static uint[] ReadVramTable(byte[] rom, uint table, uint limit)
		{
			List<uint> dests = new List<uint>();
			for (uint at = table; at < limit && dests.Count < 16; at += 4)
			{
				uint value = ReadU32(rom, (int)(at - 0x08000000U));
				if (!IsVram(value) || (value & 31U) != 0U)
				{
					break;
				}
				dests.Add(value);
			}
			return dests.ToArray();
		}

		//-------------------------------------------------------------------------------
		// VRAM の BG のタイルの場所（0x06000000 から 64 KB）かを返す処理
		//-------------------------------------------------------------------------------
		private static bool IsVram(uint value)
		{
			return value >= VramBase && value < VramBase + 0x10000U;
		}

		//-------------------------------------------------------------------------------
		// bl の前の命令から、差し替え処理に渡すタイマーの割り算（何フレームごとに進むか）を読む処理
		//   timer / 16 → lsr r0, rX, #4（u16・u8 にそろえる lsl #16・lsr #16 などは数えない）
		//   timer / 12 → mov r1, #12 の後に割り算の処理を bl
		// 読めなければ 16
		//-------------------------------------------------------------------------------
		private static int DivisorBefore(List<Op> body, int index)
		{
			for (int i = index - 1; i >= 0; i--)
			{
				Op op = body[i];
				if (op.Kind == 'R' && op.Rd == 0)
				{
					int shift = (int)op.Value;
					if (shift == 16 || shift == 24 || shift == 0)
					{
						continue;
					}
					int k = shift > 24 ? shift - 24 : (shift > 16 ? shift - 16 : shift);
					return 1 << k;
				}
				if (op.Kind == 'B')
				{
					// 割り算の処理を呼んでいる: その前の mov r1, #即値 が割る数
					for (int j = i - 1; j >= 0 && body[j].Kind != 'B'; j--)
					{
						if (body[j].Kind == 'M' && body[j].Rd == 1 && body[j].Value >= 2U && body[j].Value <= 64U)
						{
							return (int)body[j].Value;
						}
					}
					return 16;
				}
			}
			return 16;
		}

		//-------------------------------------------------------------------------------
		// 差し替え処理の中で、写す大きさ（r2 に入れる値）を出てくる順に集める処理（mov r2, #即値 と、続く lsl r2, r2, #n）
		//-------------------------------------------------------------------------------
		private static List<int> Sizes(List<Op> ops)
		{
			List<int> sizes = new List<int>();
			for (int i = 0; i < ops.Count; i++)
			{
				if (ops[i].Kind != 'M' || ops[i].Rd != 2 || ops[i].Value == 0U)
				{
					continue;
				}
				int size = (int)ops[i].Value;
				if (i + 1 < ops.Count && ops[i + 1].Kind == 'S' && ops[i + 1].Rd == 2 && ops[i + 1].Rm == 2)
				{
					size <<= (int)ops[i + 1].Value;
				}
				if (size % 32 == 0)
				{
					sizes.Add(size);
				}
			}
			return sizes;
		}

		//-------------------------------------------------------------------------------
		// コマの表を読む処理（ROM を指すポインタが続く間。次のコマの表の手前で止める）
		//-------------------------------------------------------------------------------
		private static uint[] ReadFrames(byte[] rom, uint table, List<uint> sortedArrays)
		{
			uint limit = sortedArrays.Where(a => a > table).DefaultIfEmpty(uint.MaxValue).First();
			List<uint> frames = new List<uint>();
			for (uint at = table; at < limit && frames.Count < MaxFrames; at += 4)
			{
				uint value = ReadU32(rom, (int)(at - 0x08000000U));
				if (!IsRom(rom, value))
				{
					break;
				}
				frames.Add(value - 0x08000000U);
			}
			return frames.ToArray();
		}

		//-------------------------------------------------------------------------------
		// 大きさが読めなかったときに、コマの絵の間隔（並んで置かれている前提）を大きさにする処理
		//-------------------------------------------------------------------------------
		private static int FrameSpacing(uint[] frames)
		{
			List<uint> distinct = frames.Distinct().OrderBy(f => f).ToList();
			int best = 0;
			for (int i = 1; i < distinct.Count; i++)
			{
				int gap = (int)(distinct[i] - distinct[i - 1]);
				if (gap > 0 && (best == 0 || gap < best))
				{
					best = gap;
				}
			}
			return best;
		}

		//-------------------------------------------------------------------------------
		// THUMB の命令を、処理の終わり（bx・pop {pc}）か上限まで読む処理（使う命令だけを返す）
		//-------------------------------------------------------------------------------
		private static List<Op> Decode(byte[] rom, int start, int limit)
		{
			List<Op> ops = new List<Op>();
			int pc = start;
			for (int n = 0; n < limit && pc >= 0 && pc + 2 <= rom.Length; n++)
			{
				int h = rom[pc] | (rom[pc + 1] << 8);
				if ((h & 0xF800) == 0x4800)
				{
					// ldr rd, [pc, #imm8 * 4]
					int literal = ((pc + 4) & ~3) + (h & 0xFF) * 4;
					if (literal + 4 <= rom.Length)
					{
						ops.Add(new Op { Pc = pc, Kind = 'L', Rd = (h >> 8) & 7, Value = ReadU32(rom, literal) });
					}
				}
				else if ((h & 0xF800) == 0xF000 && pc + 4 <= rom.Length && ((rom[pc + 2] | (rom[pc + 3] << 8)) & 0xF800) == 0xF800)
				{
					// bl（2 つの半分で 1 命令）
					int low = rom[pc + 2] | (rom[pc + 3] << 8);
					int offset = ((h & 0x7FF) << 12) | ((low & 0x7FF) << 1);
					if ((offset & 0x400000) != 0)
					{
						offset -= 0x800000;
					}
					ops.Add(new Op { Pc = pc, Kind = 'B', Value = (uint)(pc + 4 + offset) });
					pc += 4;
					continue;
				}
				else if ((h & 0xFF87) == 0x4700 || (h & 0xFF00) == 0xBD00)
				{
					// bx rN・pop {…, pc}: 処理の終わり
					break;
				}
				else if ((h & 0xF800) == 0x2000)
				{
					ops.Add(new Op { Pc = pc, Kind = 'M', Rd = (h >> 8) & 7, Value = (uint)(h & 0xFF) });
				}
				else if ((h & 0xF800) == 0x0000 && h != 0)
				{
					ops.Add(new Op { Pc = pc, Kind = 'S', Rd = h & 7, Rm = (h >> 3) & 7, Value = (uint)((h >> 6) & 31) });
				}
				else if ((h & 0xF800) == 0x0800)
				{
					ops.Add(new Op { Pc = pc, Kind = 'R', Rd = h & 7, Rm = (h >> 3) & 7, Value = (uint)((h >> 6) & 31) });
				}
				pc += 2;
			}
			return ops;
		}

		//-------------------------------------------------------------------------------
		// ROM の中を指すポインタ（0x08000000 から ROM の大きさまで）かを返す処理
		//-------------------------------------------------------------------------------
		private static bool IsRom(byte[] rom, uint value)
		{
			return value >= 0x08000000U && value - 0x08000000U < (uint)rom.Length;
		}

		//-------------------------------------------------------------------------------
		// 4 バイトを読む処理（範囲の外は 0）
		//-------------------------------------------------------------------------------
		private static uint ReadU32(byte[] rom, int offset)
		{
			return offset >= 0 && offset + 4 <= rom.Length ? BitConverter.ToUInt32(rom, offset) : 0U;
		}
	}
}
