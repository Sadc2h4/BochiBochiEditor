using System;
using System.Collections.Generic;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 拡張した形式のエメラルド（decomp ベースの改造。PokeKyoto など）への対応
	// 元のエメラルドと違う所:
	//   マップの 1 マス（2 バイト）… ブロック番号が 11 ビット（0〜2047）、ビット 11 が通れるかどうか、ビット 12〜15 が高さ
	//                                （元は ブロック番号 10 ビット、ビット 10〜11 が通れるかどうか、ビット 12〜15 が高さ）
	//   ブロック番号の区切り … タイルセット 1 が 0〜671、タイルセット 2 が 672〜（元は 512 で区切る）
	//   タイルセットの容量の表 … 1 件 28 バイト（見出しへのポインタ・圧縮していない絵・タイルの上位ビットの表・予約 2 つ・タイル数・ブロック数・印）
	//   タイルセットの絵 … LZ77 のほかに、smol 圧縮（SmolDecoder.cs）と、圧縮なしがある
	// この形式かどうかは、容量の表があるかどうかで見分ける。
	// エディタの中では、移動エリアの値を元のゲームと同じ 6 ビット（下位 2 ビット = 通れるかどうか、上位 4 ビット = 高さ）で持ち、読み書きのときに変換する。
	// タイルの上位ビットの表（ブロックが 1024 番以降のタイルを使うためのもの）はまだ読まない。マップの表示と、マップの並び・移動エリア・イベントの編集に対応する
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 容量の表の 1 件
		private sealed class TilesetCapacity
		{
			public int TileCount;
			public int BlockCount;
		}

		private const int CapacityEntrySize = 28;

		// マップの 1 マスのうち、ブロック番号に使うビット数（元のゲームは 10、拡張した形式は 11）
		internal static int MAP_BLOCK_ID_BITS = 10;
		// 容量の表から読んだ、タイルセットの見出しの場所ごとのタイル数・ブロック数（表が無い ROM では空）
		private static readonly Dictionary<long, TilesetCapacity> tilesetCapacities = new Dictionary<long, TilesetCapacity>();
		// 同じ内容を、絵の場所・ブロックの表の場所から引けるようにしたもの（見出しの写ししか手元に無い所で使う）
		private static readonly Dictionary<long, TilesetCapacity> capacityByImage = new Dictionary<long, TilesetCapacity>();
		private static readonly Dictionary<long, TilesetCapacity> capacityByBlocks = new Dictionary<long, TilesetCapacity>();

		// 拡張した形式の ROM か
		internal static bool IsExpandedMapFormat
		{
			get { return MAP_BLOCK_ID_BITS > 10; }
		}

		// ブロック番号の上限（番号に使える数）
		internal static int BlockIdCapacity
		{
			get { return 1 << MAP_BLOCK_ID_BITS; }
		}

		//-------------------------------------------------------------------------------
		// マップの 1 マス（2 バイト）からブロック番号を取り出す処理
		//-------------------------------------------------------------------------------
		internal static int CellBlock(int raw)
		{
			return raw & (BlockIdCapacity - 1);
		}

		//-------------------------------------------------------------------------------
		// マップの 1 マス（2 バイト）から移動エリアの値（6 ビット: 下位 2 ビット = 通れるかどうか、上位 4 ビット = 高さ）を取り出す処理
		//-------------------------------------------------------------------------------
		internal static int CellCollision(int raw)
		{
			raw &= 0xFFFF;
			if (!IsExpandedMapFormat)
			{
				return raw >> 10;
			}
			return ((raw >> 12) << 2) | ((raw >> 11) & 1);
		}

		//-------------------------------------------------------------------------------
		// ブロック番号と移動エリアの値から、マップの 1 マス（2 バイト）を作る処理
		// 拡張した形式は通れるかどうかが 1 ビットなので、移動エリアの下位 2 ビットのうち 1 ビット目だけを書く
		//-------------------------------------------------------------------------------
		internal static ushort MakeCell(int block, int collision)
		{
			if (!IsExpandedMapFormat)
			{
				return (ushort)((block & 1023) | (collision << 10));
			}
			return (ushort)((block & 0x7FF) | ((collision & 1) << 11) | (((collision >> 2) & 15) << 12));
		}

		//-------------------------------------------------------------------------------
		// タイルセットの容量の表を探して、拡張した形式かどうかを決める処理（ROM を読み込んだときに、表の場所が決まった後で呼ぶ）
		// 表の 1 件は 28 バイトで、先頭が「地形データから参照されているタイルセットの見出し」へのポインタ。それが 20 件以上続く所を表とする
		//-------------------------------------------------------------------------------
		private static void DetectExpandedFormat()
		{
			byte[] rom = MainForm.romData;
			MAP_BLOCK_ID_BITS = 10;
			tilesetCapacities.Clear();
			capacityByImage.Clear();
			capacityByBlocks.Clear();
			GameProfile profile = GameProfile.Current;
			// ブロック番号の区切りを、ゲームの元の値に戻しておく（前に拡張した形式の ROM を開いていた場合のため）
			profile.PrimaryBlockCount = profile.PrimaryTileCount;
			if (rom == null || !profile.EmeraldHeaderLayout)
			{
				return;
			}
			HashSet<long> headers = new HashSet<long>(DetectTilesetHeaderOffsets().Select(o => (long)o));
			if (headers.Count < 20)
			{
				return;
			}
			int limit = rom.Length - CapacityEntrySize;
			for (int start = 0; start <= limit; start += 4)
			{
				if (!IsCapacityEntry(rom, start, headers))
				{
					continue;
				}
				int count = 0;
				while (start + (count + 1) * CapacityEntrySize <= rom.Length && IsCapacityEntry(rom, start + count * CapacityEntrySize, headers))
				{
					count++;
				}
				if (count < 20)
				{
					continue;
				}
				for (int i = 0; i < count; i++)
				{
					int entry = start + i * CapacityEntrySize;
					long header = BitConverter.ToUInt32(rom, entry) - 0x08000000u;
					TilesetCapacity capacity = new TilesetCapacity { TileCount = BitConverter.ToUInt16(rom, entry + 20), BlockCount = BitConverter.ToUInt16(rom, entry + 22) };
					tilesetCapacities[header] = capacity;
					if (CanReadAutoData(rom, header, 16))
					{
						uint image = BitConverter.ToUInt32(rom, (int)header + 4);
						uint blocks = BitConverter.ToUInt32(rom, (int)header + 12);
						if (IsAutoRomPointer(rom, image)) capacityByImage[image - 0x08000000u] = capacity;
						if (IsAutoRomPointer(rom, blocks)) capacityByBlocks[blocks - 0x08000000u] = capacity;
					}
				}
				break;
			}
			if (tilesetCapacities.Count == 0)
			{
				return;
			}
			MAP_BLOCK_ID_BITS = 11;
			// タイルセット 1 のブロック数のいちばん大きいものを、ブロック番号の区切りにする（PokeKyoto は 672）
			int primaryBlocks = 0;
			foreach (KeyValuePair<long, TilesetCapacity> pair in tilesetCapacities)
			{
				if (pair.Key + 2 <= rom.Length && rom[pair.Key + 1] == 0)
				{
					primaryBlocks = Math.Max(primaryBlocks, pair.Value.BlockCount);
				}
			}
			if (primaryBlocks > 0 && primaryBlocks < BlockIdCapacity)
			{
				profile.PrimaryBlockCount = primaryBlocks;
			}
		}

		//-------------------------------------------------------------------------------
		// その場所が容量の表の 1 件らしいかを調べる処理
		// （見出しへのポインタ、0 か ROM 内へのポインタが 2 つ、予約の 0 が 2 つ、タイル数 1〜4096、ブロック数 1〜2048）
		//-------------------------------------------------------------------------------
		private static bool IsCapacityEntry(byte[] rom, int offset, HashSet<long> headers)
		{
			uint header = BitConverter.ToUInt32(rom, offset);
			if (!IsAutoRomPointer(rom, header) || !headers.Contains(header - 0x08000000u))
			{
				return false;
			}
			for (int field = 4; field <= 8; field += 4)
			{
				uint pointer = BitConverter.ToUInt32(rom, offset + field);
				if (pointer != 0 && !IsAutoRomPointer(rom, pointer))
				{
					return false;
				}
			}
			if (BitConverter.ToUInt32(rom, offset + 12) != 0 || BitConverter.ToUInt32(rom, offset + 16) != 0)
			{
				return false;
			}
			int tiles = BitConverter.ToUInt16(rom, offset + 20);
			int blocks = BitConverter.ToUInt16(rom, offset + 22);
			return tiles >= 1 && tiles <= 4096 && blocks >= 1 && blocks <= 2048;
		}

		//-------------------------------------------------------------------------------
		// 容量の表にあるタイルセットなら、そのブロック数を返す処理（無ければ -1）
		//-------------------------------------------------------------------------------
		private static int CapacityBlockCount(TilesetHeader tileset)
		{
			TilesetCapacity capacity;
			return tileset != null && tileset.BlockImageAddress != 0 && capacityByBlocks.TryGetValue(tileset.BlockImageAddress, out capacity) ? capacity.BlockCount : -1;
		}

		//-------------------------------------------------------------------------------
		// 拡張した形式の ROM で、まだ対応していない編集（ブロック・タイル・タイルセットを書き換えるもの）の入口を止め、案内する処理
		//-------------------------------------------------------------------------------
		private bool BlockIfExpandedFormat()
		{
			if (!IsExpandedMapFormat)
			{
				return false;
			}
			System.Windows.Forms.MessageBox.Show(this, Localizer.T("この ROM は拡張した形式（ブロック番号が 11 ビット・タイルセットの容量の表つき）です。\nマップの並び・移動エリア・イベントの編集と保存はできますが、ブロック・タイル・タイルセットを書き換える機能はまだ使えません。"),
				Localizer.T("未対応の機能"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
			return true;
		}

		//-------------------------------------------------------------------------------
		// タイルセットの絵（4bpp の並び）を、圧縮の種類を見分けて読む処理
		//   圧縮の印が 0 … そのまま読む（枚数は容量の表、無ければゲームの決まった枚数まで）
		//   先頭が 0x10 … LZ77
		//   smol の見出しの形 … smol
		// 読めないときは空の並びを返す（絵が出ないだけで、マップは開ける）
		//-------------------------------------------------------------------------------
		private byte[] ReadTilesetImage(TilesetHeader tileset)
		{
			if (tileset == null || tileset.ImageAddress == 0 || tileset.ImageAddress >= this.romData.Length)
			{
				return new byte[0];
			}
			try
			{
				if (tileset.ImageCompressType != 1)
				{
					int tiles = 1024;
					TilesetCapacity capacity;
					if (capacityByImage.TryGetValue(tileset.ImageAddress, out capacity))
					{
						tiles = capacity.TileCount;
					}
					int length = (int)Math.Min((long)tiles * 32, this.romData.Length - tileset.ImageAddress);
					byte[] raw = new byte[Math.Max(0, length)];
					Array.Copy(this.romData, (int)tileset.ImageAddress, raw, 0, raw.Length);
					return raw;
				}
				if (this.romData[tileset.ImageAddress] != 0x10 && SmolDecoder.LooksLikeSmol(this.romData, tileset.ImageAddress))
				{
					return SmolDecoder.Decode(this.romData, tileset.ImageAddress);
				}
				if (this.romData[tileset.ImageAddress] == 0x10 && tileset.ImageAddress + 4 <= this.romData.Length)
				{
					// LZ77: 展開後の大きさが 16KB を超えるタイルセット（1024 枚など）もあるので、圧縮データを長めに渡して展開する
					int size = this.romData[tileset.ImageAddress + 1] | (this.romData[tileset.ImageAddress + 2] << 8) | (this.romData[tileset.ImageAddress + 3] << 16);
					if (size > 0x4000 && size <= 0x20000)
					{
						int available = (int)Math.Min(this.romData.Length - tileset.ImageAddress, (long)size * 2 + 0x100);
						byte[] source = new byte[available];
						Array.Copy(this.romData, (int)tileset.ImageAddress, source, 0, available);
						byte[] image = new byte[size];
						ImageProcessor.LZ77UnComp(source, image);
						return image;
					}
				}
				return ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, tileset.ImageAddress, false);
			}
			catch (Exception ex)
			{
				this.WriteErrorLog(Localizer.T("タイルセットの絵の読み込み"), ex);
				return new byte[0];
			}
		}
	}
}
