using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 読み込んだ ROM から、ポケモンのミニアイコンを直接作る処理（3 段階）
	//   1. アイコン画像の表（1 匹 4 バイトのポインタ）から、その匹のドット絵（4bpp、32×64 = 2 コマ）を読む
	//   2. パレット番号の表（1 匹 1 バイト）と、パレットの表（1 本 8 バイト: ポインタ＋タグ）から、16 色を読む
	//   3. 1 と 2 を合わせて、色 0 を透明にした 32×32 の絵を作る（作った絵は覚えておく）
	// 表の場所は ini の設定（ヘッダーのポインタ経由）で探し、匹数とパレットの本数は ROM の中身から数える
	// （改造 ROM でポケモンやパレットが増えていても、ini の数に縛られずに読めるようにするため）
	//-------------------------------------------------------------------------------
	internal sealed class PokemonIconReader
	{
		// 数えるときの上限（壊れたデータで延々と読まないように）
		private const int MaxIcons = 4096;
		private const int MaxPalettes = 256;
		private const int IconFrameBytes = 32 * 32 / 2;

		private readonly byte[] rom;
		private readonly int imageTable;
		private readonly int paletteIdTable;
		private readonly int paletteTable;
		private readonly Dictionary<int, Bitmap> cache = new Dictionary<int, Bitmap>();
		private readonly Dictionary<int, Color[]> paletteCache = new Dictionary<int, Color[]>();

		//-------------------------------------------------------------------------------
		// ROM を受け取り、表の場所と、アイコン・パレットの数を調べる処理
		//-------------------------------------------------------------------------------
		private PokemonIconReader(byte[] rom, int imageTable, int paletteIdTable, int paletteTable)
		{
			this.rom = rom;
			this.imageTable = imageTable;
			this.paletteIdTable = paletteIdTable;
			this.paletteTable = paletteTable;
			// アイコンの数: 表のポインタが ROM の中を指している間だけ数える
			int icons = 0;
			while (icons < MaxIcons && this.IsRomPointer(this.imageTable + icons * 4))
			{
				icons++;
			}
			this.IconCount = icons;
			// パレットの本数: パレットの表のポインタが ROM の中を指している間だけ数える
			int palettes = 0;
			while (palettes < MaxPalettes && this.IsRomPointer(this.paletteTable + palettes * 8))
			{
				palettes++;
			}
			this.PaletteCount = palettes;
		}

		//-------------------------------------------------------------------------------
		// アイコンの表に入っている数（元のファイアレッドは 440。卵やアンノーンの形も含む）
		//-------------------------------------------------------------------------------
		public int IconCount { get; }

		//-------------------------------------------------------------------------------
		// アイコン用パレットの本数（改造 ROM では 3 本より多いことがある）
		//-------------------------------------------------------------------------------
		public int PaletteCount { get; }

		// ポケモンの匹数（名前の表から数えた数）。アイコンの表の途中に空きがあっても、この数までは読む。0 なら IconCount まで
		public int SpeciesLimit { get; set; }

		// 読み込みに使った ROM（別の ROM を開いたら作り直すための判定に使う）
		public byte[] Rom
		{
			get { return this.rom; }
		}

		//-------------------------------------------------------------------------------
		// ini の設定から表の場所を調べて作る処理（調べられなければ null）
		//-------------------------------------------------------------------------------
		public static PokemonIconReader Create(byte[] rom)
		{
			if (rom == null)
			{
				return null;
			}
			try
			{
				int image = ResolveOffset(rom, RomIniReader.ReadValue("ICON_IMAGE_TABLE_OFFSET"));
				int paletteId = ResolveOffset(rom, RomIniReader.ReadValue("ICON_PALETTE_ID_TABLE_OFFSET"));
				int palette = ResolveOffset(rom, RomIniReader.ReadValue("ICON_PALETTE_TABLE_OFFSET"));
				if (image < 0 || paletteId < 0 || palette < 0)
				{
					return null;
				}
				return new PokemonIconReader(rom, image, paletteId, palette);
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// ROM のアイコン用パレットの本数を返す処理（数えられなければ fallback を返す）
		// 同じ ROM で何度も呼ばれるので、最後に数えた ROM の結果を覚えておく
		//-------------------------------------------------------------------------------
		public static int CountPalettesOrDefault(byte[] rom, int fallback)
		{
			if (rom == null)
			{
				return fallback;
			}
			if (rom != lastCountedRom)
			{
				PokemonIconReader reader = Create(rom);
				lastCountedRom = rom;
				lastPaletteCount = reader != null && reader.PaletteCount > 0 ? reader.PaletteCount : -1;
			}
			return lastPaletteCount > 0 ? lastPaletteCount : fallback;
		}

		private static byte[] lastCountedRom;
		private static int lastPaletteCount = -1;

		//-------------------------------------------------------------------------------
		// ini の値（「*アドレス」はそこに書かれたポインタを読む）を ROM 上の位置に直す処理（直せなければ -1）
		//-------------------------------------------------------------------------------
		private static int ResolveOffset(byte[] rom, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return -1;
			}
			value = value.Trim();
			bool indirect = value.StartsWith("*", StringComparison.Ordinal);
			string number = indirect ? value.Substring(1).Trim() : value;
			int offset = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
				? int.Parse(number.Substring(2), NumberStyles.HexNumber)
				: int.Parse(number, CultureInfo.InvariantCulture);
			if (!indirect)
			{
				return offset;
			}
			if (offset < 0 || offset + 4 > rom.Length)
			{
				return -1;
			}
			uint pointer = BitConverter.ToUInt32(rom, offset);
			long target = (long)pointer - 0x08000000L;
			return target >= 0 && target < rom.Length ? (int)target : -1;
		}

		//-------------------------------------------------------------------------------
		// 指定した位置に、ROM の中を指すポインタが書かれているかを判定する処理
		//-------------------------------------------------------------------------------
		private bool IsRomPointer(int offset)
		{
			if (offset < 0 || offset + 4 > this.rom.Length)
			{
				return false;
			}
			uint pointer = BitConverter.ToUInt32(this.rom, offset);
			return pointer >= 0x08000000u && pointer - 0x08000000u < (uint)this.rom.Length;
		}

		//-------------------------------------------------------------------------------
		// その匹が使うパレットの番号を返す処理（表の外や、本数を超える番号は 0 にする）
		//-------------------------------------------------------------------------------
		public int GetPaletteId(int species)
		{
			int offset = this.paletteIdTable + species;
			if (species < 0 || offset >= this.rom.Length)
			{
				return 0;
			}
			int id = this.rom[offset];
			return id < this.PaletteCount ? id : 0;
		}

		//-------------------------------------------------------------------------------
		// パレット（16 色、色 0 は透明）を読む処理
		//-------------------------------------------------------------------------------
		public Color[] GetPalette(int paletteId)
		{
			Color[] colors;
			if (this.paletteCache.TryGetValue(paletteId, out colors))
			{
				return colors;
			}
			colors = new Color[16];
			int entry = this.paletteTable + paletteId * 8;
			if (paletteId >= 0 && paletteId < this.PaletteCount && this.IsRomPointer(entry))
			{
				int address = (int)(BitConverter.ToUInt32(this.rom, entry) - 0x08000000u);
				for (int i = 0; i < 16; i++)
				{
					if (address + i * 2 + 2 > this.rom.Length)
					{
						break;
					}
					ushort bgr = BitConverter.ToUInt16(this.rom, address + i * 2);
					// GBA の色（各 5 ビット）を 8 ビットへ広げる
					int r = (bgr & 0x1F) * 255 / 31;
					int g = ((bgr >> 5) & 0x1F) * 255 / 31;
					int b = ((bgr >> 10) & 0x1F) * 255 / 31;
					colors[i] = i == 0 ? Color.Transparent : Color.FromArgb(255, r, g, b);
				}
			}
			this.paletteCache[paletteId] = colors;
			return colors;
		}

		//-------------------------------------------------------------------------------
		// その匹のミニアイコン（32×32、1 コマ目）を返す処理（読めなければ null。作った絵は使い回すので破棄しないこと）
		//-------------------------------------------------------------------------------
		public Bitmap GetIcon(int species)
		{
			Bitmap icon;
			if (this.cache.TryGetValue(species, out icon))
			{
				return icon;
			}
			icon = null;
			// 数えた数より後ろでも、匹数の範囲内でポインタが ROM の中を指していれば読む（表の途中に空きがある改造版のため）
			bool listed = species >= 0 && (species < this.IconCount || (species < this.SpeciesLimit && this.IsRomPointer(this.imageTable + species * 4)));
			if (listed)
			{
				int address = (int)(BitConverter.ToUInt32(this.rom, this.imageTable + species * 4) - 0x08000000u);
				if (address >= 0 && address + IconFrameBytes <= this.rom.Length)
				{
					icon = this.DecodeFrame(address, this.GetPalette(this.GetPaletteId(species)));
				}
			}
			this.cache[species] = icon;
			return icon;
		}

		//-------------------------------------------------------------------------------
		// 4bpp のタイル（8×8 を横 4 × 縦 4 枚）を 32×32 の絵に並べる処理
		//-------------------------------------------------------------------------------
		private Bitmap DecodeFrame(int address, Color[] palette)
		{
			Bitmap bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
			for (int tile = 0; tile < 16; tile++)
			{
				int tileX = tile % 4 * 8;
				int tileY = tile / 4 * 8;
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x += 2)
					{
						byte pair = this.rom[address + tile * 32 + y * 4 + x / 2];
						bitmap.SetPixel(tileX + x, tileY + y, palette[pair & 0x0F]);
						bitmap.SetPixel(tileX + x + 1, tileY + y, palette[pair >> 4]);
					}
				}
			}
			return bitmap;
		}
	}
}
