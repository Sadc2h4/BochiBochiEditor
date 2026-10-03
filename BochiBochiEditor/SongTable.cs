using System;
using System.Security.Cryptography;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 読み込んだ ROM の曲の表（1 曲 8 バイト: 曲の見出しへのポインタ＋再生に使うプレイヤー番号×2）を調べる処理
	// ROM には曲名が入っていないので、曲の中身から作った「指紋」を元のゲーム（BgmCatalog）のものと見比べて、
	// 元のままの曲・差し替えられた曲・追加された曲を見分ける
	//-------------------------------------------------------------------------------
	internal sealed class SongTable
	{
		// 曲の種類（元のまま・差し替え・追加）
		public enum SongKind
		{
			Original,
			Replaced,
			Added,
		}

		// 表とみなす最低の曲数（元のファイアレッドは 347 曲、エメラルドは 559 曲。効果音を含む）
		private const int MinimumSongs = 200;
		// 指紋に使う、各トラックの先頭のバイト数
		private const int PrintBytesPerTrack = 48;

		private static byte[] lastRom;
		private static SongTable lastTable;

		private readonly byte[] rom;

		//-------------------------------------------------------------------------------
		// 見つけた表の位置と曲数を持つ処理
		//-------------------------------------------------------------------------------
		private SongTable(byte[] rom, int offset, int count)
		{
			this.rom = rom;
			this.Offset = offset;
			this.Count = count;
		}

		// 表の位置（ROM 上）と、表に入っている曲の数
		public int Offset { get; }
		public int Count { get; }

		//-------------------------------------------------------------------------------
		// ROM の中から曲の表を探す処理（見つからなければ null。同じ ROM なら前回の結果を使う）
		// 条件: 見出しへのポインタが正しく、プレイヤー番号が 2 つとも同じ小さな値で、見出しの中身も曲らしい項目が 200 以上続く所
		// 見つけた後は、空の曲（エメラルドの 270〜349 番）や、プレイヤー番号が 2 つで違う項目（改造版）も表の続きとして数える
		//-------------------------------------------------------------------------------
		public static SongTable Find(byte[] rom)
		{
			if (rom == null)
			{
				return null;
			}
			if (rom == lastRom)
			{
				return lastTable;
			}
			SongTable found = null;
			for (int offset = 0; offset + 8 * MinimumSongs <= rom.Length; offset += 4)
			{
				if (!IsSongEntry(rom, offset))
				{
					continue;
				}
				int count = 0;
				while (offset + (count + 1) * 8 <= rom.Length && IsSongEntry(rom, offset + count * 8))
				{
					count++;
				}
				if (count >= MinimumSongs)
				{
					while (offset + (count + 1) * 8 <= rom.Length && IsLooseSongEntry(rom, offset + count * 8))
					{
						count++;
					}
					found = new SongTable(rom, offset, count);
					break;
				}
				offset += count * 8;
			}
			lastRom = rom;
			lastTable = found;
			return found;
		}

		//-------------------------------------------------------------------------------
		// 指定した位置が、曲の表の 1 項目らしいかを判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsSongEntry(byte[] rom, int offset)
		{
			uint pointer = BitConverter.ToUInt32(rom, offset);
			ushort player = BitConverter.ToUInt16(rom, offset + 4);
			ushort player2 = BitConverter.ToUInt16(rom, offset + 6);
			if (player != player2 || player > 7 || !IsRomPointer(rom, pointer))
			{
				return false;
			}
			// 曲の見出し: トラック数（0〜16）、ブロック数、優先度、残響、音色の表へのポインタ、各トラックへのポインタ
			int header = (int)(pointer - 0x08000000u);
			if (header + 8 > rom.Length)
			{
				return false;
			}
			int tracks = rom[header];
			if (tracks > 16 || header + 8 + tracks * 4 > rom.Length || !IsRomPointer(rom, BitConverter.ToUInt32(rom, header + 4)))
			{
				return false;
			}
			for (int t = 0; t < tracks; t++)
			{
				if (!IsRomPointer(rom, BitConverter.ToUInt32(rom, header + 8 + t * 4)))
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// 表の続きとして数えてよい項目かを判定する処理（表の先頭を探すときの条件より緩い）
		// プレイヤー番号は 2 つとも小さな値ならよく、見出しは空の曲（トラック数 0）か、曲らしい中身ならよい
		//-------------------------------------------------------------------------------
		private static bool IsLooseSongEntry(byte[] rom, int offset)
		{
			uint pointer = BitConverter.ToUInt32(rom, offset);
			if (BitConverter.ToUInt16(rom, offset + 4) > 7 || BitConverter.ToUInt16(rom, offset + 6) > 7 || !IsRomPointer(rom, pointer))
			{
				return false;
			}
			int header = (int)(pointer - 0x08000000u);
			int tracks = rom[header];
			if (tracks == 0)
			{
				return true;
			}
			if (tracks > 16 || header + 8 + tracks * 4 > rom.Length || !IsRomPointer(rom, BitConverter.ToUInt32(rom, header + 4)))
			{
				return false;
			}
			for (int t = 0; t < tracks; t++)
			{
				if (!IsRomPointer(rom, BitConverter.ToUInt32(rom, header + 8 + t * 4)))
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// その番号の曲を鳴らすプレイヤーの番号を返す処理（0 = BGM。効果音は 1 以上）
		//-------------------------------------------------------------------------------
		public int GetPlayer(int id)
		{
			return id < 0 || id >= this.Count ? -1 : BitConverter.ToUInt16(this.rom, this.Offset + id * 8 + 4);
		}

		//-------------------------------------------------------------------------------
		// ROM の中を指すポインタかを判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsRomPointer(byte[] rom, uint pointer)
		{
			return pointer >= 0x08000000u && pointer - 0x08000000u < (uint)rom.Length;
		}

		//-------------------------------------------------------------------------------
		// 曲の「指紋」を作る処理（見出しの先頭 4 バイトと、各トラックの先頭 48 バイトから作る）
		// トラックの中の飛び先（GOTO=B2・PATT=B3 の後ろ 4 バイト）は、曲の置き場所が変わると値が変わるので消してから使う
		//-------------------------------------------------------------------------------
		public string Fingerprint(int id)
		{
			if (id < 0 || id >= this.Count)
			{
				return null;
			}
			int header = (int)(BitConverter.ToUInt32(this.rom, this.Offset + id * 8) - 0x08000000u);
			int tracks = this.rom[header];
			byte[] data = new byte[4 + tracks * PrintBytesPerTrack];
			Array.Copy(this.rom, header, data, 0, 4);
			for (int t = 0; t < tracks; t++)
			{
				int track = (int)(BitConverter.ToUInt32(this.rom, header + 8 + t * 4) - 0x08000000u);
				int length = Math.Min(PrintBytesPerTrack, this.rom.Length - track);
				int start = 4 + t * PrintBytesPerTrack;
				Array.Copy(this.rom, track, data, start, length);
				for (int j = 0; j < length; )
				{
					byte command = data[start + j];
					if (command == 0xB2 || command == 0xB3)
					{
						for (int k = 1; k <= 4 && j + k < length; k++)
						{
							data[start + j + k] = 0;
						}
						j += 5;
					}
					else
					{
						j++;
					}
				}
				// 読めなかった分（ROM の終わり）は 0 のまま残す
			}
			// 元の指紋（Python で作った表）と同じ作り方にするため、トラックの長さが足りない場合も 48 バイトぶん並べる
			byte[] hash = MD5.HashData(TrimToRead(data, header, tracks));
			return BitConverter.ToString(hash, 0, 6).Replace("-", string.Empty).ToLowerInvariant();
		}

		//-------------------------------------------------------------------------------
		// 指紋に使うバイト列を返す処理（ROM の終わりで読めなかった部分は含めない）
		//-------------------------------------------------------------------------------
		private byte[] TrimToRead(byte[] data, int header, int tracks)
		{
			int total = 4;
			for (int t = 0; t < tracks; t++)
			{
				int track = (int)(BitConverter.ToUInt32(this.rom, header + 8 + t * 4) - 0x08000000u);
				total += Math.Min(PrintBytesPerTrack, this.rom.Length - track);
			}
			if (total == data.Length)
			{
				return data;
			}
			// トラックごとに読めた長さだけを詰め直す
			byte[] trimmed = new byte[total];
			Array.Copy(data, 0, trimmed, 0, 4);
			int at = 4;
			for (int t = 0; t < tracks; t++)
			{
				int track = (int)(BitConverter.ToUInt32(this.rom, header + 8 + t * 4) - 0x08000000u);
				int length = Math.Min(PrintBytesPerTrack, this.rom.Length - track);
				Array.Copy(data, 4 + t * PrintBytesPerTrack, trimmed, at, length);
				at += length;
			}
			return trimmed;
		}

		//-------------------------------------------------------------------------------
		// 曲の種類を返す処理（元のゲームにある番号で、指紋が同じなら元のまま）
		//-------------------------------------------------------------------------------
		public SongKind GetKind(int id)
		{
			if (!BgmCatalog.Contains(id))
			{
				return SongKind.Added;
			}
			return BgmCatalog.IsOriginalPrint(id, this.Fingerprint(id)) ? SongKind.Original : SongKind.Replaced;
		}
	}
}
