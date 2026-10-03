using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ROM のバイト列からマップ全体の縮小画像を作る処理
	//-------------------------------------------------------------------------------
	internal static class MapThumbnailRenderer
	{
		private const uint RomPointerBase = 0x08000000U;
		private const int PaletteCount = 13;
		private const int TileBytes = 32;
		private const int BlockPixels = 16;
		private const int MaximumMapPixels = 4000000;

		internal sealed class Settings
		{
			public int PrimaryTileCount;
			public int PrimaryBlockCount;
			public int PrimaryPaletteCount;
			public int BlockBytes;
			public bool TripleLayerByNextBlock;
			public int TilesetBehaviorOffset;

			//-------------------------------------------------------------------------------
			// 現在のゲーム定義を背景処理へ渡せる設定値に写す処理
			//-------------------------------------------------------------------------------
			public static Settings FromCurrentGame()
			{
				GameProfile profile = GameProfile.Current;
				return new Settings
				{
					PrimaryTileCount = profile.PrimaryTileCount,
					PrimaryBlockCount = profile.PrimaryBlockCount,
					PrimaryPaletteCount = profile.PrimaryPaletteCount,
					BlockBytes = MapEditor.BLOCK_DATA_SIZE,
					TripleLayerByNextBlock = profile.SupportsTripleLayer && MapEditor.BLOCK_DATA_SIZE == 16,
					TilesetBehaviorOffset = profile.TilesetBehaviorOffset
				};
			}
		}

		internal sealed class TilesetCache
		{
			internal readonly Dictionary<(uint Primary, uint Secondary), TilesetData> Items = new Dictionary<(uint Primary, uint Secondary), TilesetData>();
		}

		private sealed class TilesetHeader
		{
			public byte ImageCompressType;
			public uint ImageAddress;
			public uint PaletteAddress;
			public uint BlockAddress;
			public uint BehaviorAddress;
		}

		internal sealed class TilesetData
		{
			public byte[] TileImage;
			public int[] Palettes;
			public uint PrimaryBlockAddress;
			public uint SecondaryBlockAddress;
			public uint PrimaryBehaviorAddress;
			public uint SecondaryBehaviorAddress;
			public readonly Dictionary<int, int[]> Blocks = new Dictionary<int, int[]>();
		}

		//-------------------------------------------------------------------------------
		// ROM の地形データから、指定サイズに収まるマップの縮小画像を作る処理
		//-------------------------------------------------------------------------------
		public static Bitmap Render(byte[] rom, uint footerOffset, Settings settings, TilesetCache cache, int size)
		{
			try
			{
				if (rom == null || settings == null || cache == null || size <= 0 ||
					settings.PrimaryTileCount <= 0 || settings.PrimaryBlockCount <= 0 ||
					settings.PrimaryPaletteCount < 0 || settings.PrimaryPaletteCount > PaletteCount ||
					settings.BlockBytes < 8 || settings.BlockBytes % 8 != 0 ||
					!CanRead(rom, footerOffset, 24))
				{
					return null;
				}

				int mapWidth = BitConverter.ToInt32(rom, (int)footerOffset);
				int mapHeight = BitConverter.ToInt32(rom, (int)footerOffset + 4);
				if (mapWidth < 1 || mapWidth > 512 || mapHeight < 1 || mapHeight > 512)
				{
					return null;
				}

				if (!TryReadPointer(rom, (long)footerOffset + 12, out uint mapAddress) ||
					!TryReadPointer(rom, (long)footerOffset + 16, out uint primaryTilesetAddress) ||
					!TryReadPointer(rom, (long)footerOffset + 20, out uint secondaryTilesetAddress))
				{
					return null;
				}

				long mapBytes = (long)mapWidth * mapHeight * 2;
				if (!CanRead(rom, mapAddress, mapBytes))
				{
					return null;
				}

				TilesetData tilesets = GetTilesetData(rom, primaryTilesetAddress, secondaryTilesetAddress, settings, cache);
				if (tilesets == null)
				{
					return null;
				}

				int sampleStep = GetSampleStep(mapWidth, mapHeight);
				int sampledWidth = (mapWidth + sampleStep - 1) / sampleStep;
				int sampledHeight = (mapHeight + sampleStep - 1) / sampleStep;
				int sourceWidth = sampledWidth * BlockPixels;
				int sourceHeight = sampledHeight * BlockPixels;
				int[] sourcePixels = new int[sourceWidth * sourceHeight];

				for (int sampleY = 0, mapY = 0; mapY < mapHeight; sampleY++, mapY += sampleStep)
				{
					for (int sampleX = 0, mapX = 0; mapX < mapWidth; sampleX++, mapX += sampleStep)
					{
						int mapDataOffset = (int)mapAddress + (mapY * mapWidth + mapX) * 2;
						int blockNumber = BitConverter.ToUInt16(rom, mapDataOffset) & 0x03FF;
						int[] block = GetBlockPixels(rom, blockNumber, settings, tilesets);
						CopyBlock(block, sourcePixels, sourceWidth, sampleX * BlockPixels, sampleY * BlockPixels);
					}
				}

				return ScaleToBitmap(sourcePixels, sourceWidth, sourceHeight, size);
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// マップ画像が 400 万画素以内になるブロックの間引き幅を求める処理
		//-------------------------------------------------------------------------------
		private static int GetSampleStep(int mapWidth, int mapHeight)
		{
			int step = 1;
			while ((long)((mapWidth + step - 1) / step) * ((mapHeight + step - 1) / step) * BlockPixels * BlockPixels > MaximumMapPixels)
			{
				step++;
			}
			return step;
		}

		//-------------------------------------------------------------------------------
		// タイルセットの組に対応する展開済みデータを取得する処理
		//-------------------------------------------------------------------------------
		private static TilesetData GetTilesetData(byte[] rom, uint primaryAddress, uint secondaryAddress, Settings settings, TilesetCache cache)
		{
			var key = (Primary: primaryAddress, Secondary: secondaryAddress);
			if (cache.Items.TryGetValue(key, out TilesetData cached))
			{
				return cached;
			}

			TilesetHeader primary = ReadTilesetHeader(rom, primaryAddress, settings);
			TilesetHeader secondary = ReadTilesetHeader(rom, secondaryAddress, settings);
			if (primary == null || secondary == null)
			{
				return null;
			}

			byte[] primaryImage = LoadTileImage(rom, primary);
			byte[] secondaryImage = LoadTileImage(rom, secondary);
			if (primaryImage == null || secondaryImage == null)
			{
				return null;
			}

			int primaryBytes = checked(settings.PrimaryTileCount * TileBytes);
			byte[] combinedImage = new byte[checked(primaryBytes + secondaryImage.Length)];
			Array.Copy(primaryImage, 0, combinedImage, 0, Math.Min(primaryImage.Length, primaryBytes));
			Array.Copy(secondaryImage, 0, combinedImage, primaryBytes, secondaryImage.Length);

			int[] palettes = LoadPalettes(rom, primary.PaletteAddress, secondary.PaletteAddress, settings.PrimaryPaletteCount);
			if (palettes == null)
			{
				return null;
			}

			TilesetData result = new TilesetData
			{
				TileImage = combinedImage,
				Palettes = palettes,
				PrimaryBlockAddress = primary.BlockAddress,
				SecondaryBlockAddress = secondary.BlockAddress,
				PrimaryBehaviorAddress = primary.BehaviorAddress,
				SecondaryBehaviorAddress = secondary.BehaviorAddress
			};
			cache.Items.Add(key, result);
			return result;
		}

		//-------------------------------------------------------------------------------
		// ROM からタイルセット見出しを境界確認付きで読む処理
		//-------------------------------------------------------------------------------
		private static TilesetHeader ReadTilesetHeader(byte[] rom, uint address, Settings settings)
		{
			int requiredBytes = Math.Max(16, settings.TilesetBehaviorOffset + 4);
			if (!CanRead(rom, address, requiredBytes) ||
				!TryReadPointer(rom, (long)address + 4, out uint imageAddress) ||
				!TryReadPointer(rom, (long)address + 8, out uint paletteAddress) ||
				!TryReadPointer(rom, (long)address + 12, out uint blockAddress))
			{
				return null;
			}

			uint behaviorAddress = 0;
			if (settings.TripleLayerByNextBlock && !TryReadPointer(rom, (long)address + settings.TilesetBehaviorOffset, out behaviorAddress))
			{
				return null;
			}
			if (!settings.TripleLayerByNextBlock)
			{
				TryReadPointer(rom, (long)address + settings.TilesetBehaviorOffset, out behaviorAddress);
			}

			return new TilesetHeader
			{
				ImageCompressType = rom[(int)address],
				ImageAddress = imageAddress,
				PaletteAddress = paletteAddress,
				BlockAddress = blockAddress,
				BehaviorAddress = behaviorAddress
			};
		}

		//-------------------------------------------------------------------------------
		// タイルセットの 4bpp 画像を ROM から読み、必要なら LZ77 展開する処理
		//-------------------------------------------------------------------------------
		private static byte[] LoadTileImage(byte[] rom, TilesetHeader header)
		{
			if (header.ImageCompressType == 1)
			{
				if (!CanRead(rom, header.ImageAddress, 4))
				{
					return null;
				}
				int decompressedSize = rom[(int)header.ImageAddress + 1] |
					(rom[(int)header.ImageAddress + 2] << 8) |
					(rom[(int)header.ImageAddress + 3] << 16);
				if (decompressedSize <= 0 || decompressedSize > 1048576)
				{
					return null;
				}
				return ImageProcessor.LoadCompressedImagePaletteFromROM(rom, header.ImageAddress, false);
			}

			int length = (int)Math.Min(32768L, rom.LongLength - header.ImageAddress);
			if (length <= 0)
			{
				return null;
			}
			byte[] result = new byte[length];
			Array.Copy(rom, (long)header.ImageAddress, result, 0L, length);
			return result;
		}

		//-------------------------------------------------------------------------------
		// 第1・第2タイルセットから 13 本分の ARGB パレットを読む処理
		//-------------------------------------------------------------------------------
		private static int[] LoadPalettes(byte[] rom, uint primaryAddress, uint secondaryAddress, int primaryCount)
		{
			int secondaryCount = PaletteCount - primaryCount;
			long primaryBytes = (long)primaryCount * 32;
			long secondaryStart = (long)secondaryAddress + primaryBytes;
			long secondaryBytes = (long)secondaryCount * 32;
			if (!CanRead(rom, primaryAddress, primaryBytes) || !CanRead(rom, secondaryStart, secondaryBytes))
			{
				return null;
			}

			int[] palettes = new int[PaletteCount * 16];
			for (int palette = 0; palette < primaryCount; palette++)
			{
				ReadPalette(rom, (long)primaryAddress + palette * 32L, palettes, palette * 16);
			}
			for (int palette = 0; palette < secondaryCount; palette++)
			{
				ReadPalette(rom, secondaryStart + palette * 32L, palettes, (primaryCount + palette) * 16);
			}
			return palettes;
		}

		//-------------------------------------------------------------------------------
		// GBA の 16 色パレット 1 本を ARGB 配列へ変換する処理
		//-------------------------------------------------------------------------------
		private static void ReadPalette(byte[] rom, long address, int[] destination, int destinationOffset)
		{
			for (int color = 0; color < 16; color++)
			{
				int source = (int)(address + color * 2L);
				ushort value = (ushort)(rom[source] | (rom[source + 1] << 8));
				int red = (value & 0x1F) * 8;
				int green = ((value >> 5) & 0x1F) * 8;
				int blue = ((value >> 10) & 0x1F) * 8;
				destination[destinationOffset + color] = color == 0 ? 0 : unchecked((int)(0xFF000000U | (uint)(red << 16) | (uint)(green << 8) | (uint)blue));
			}
		}

		//-------------------------------------------------------------------------------
		// 必要になったブロックだけを 16×16 の ARGB 配列へ描いてキャッシュする処理
		//-------------------------------------------------------------------------------
		private static int[] GetBlockPixels(byte[] rom, int blockNumber, Settings settings, TilesetData tilesets)
		{
			if (tilesets.Blocks.TryGetValue(blockNumber, out int[] cached))
			{
				return cached;
			}

			int[] pixels = new int[BlockPixels * BlockPixels];
			if (TryGetBlockAddress(rom, blockNumber, settings, tilesets, out uint blockAddress))
			{
				int layers = settings.BlockBytes / 8;
				for (int layer = 0; layer < layers; layer++)
				{
					DrawBlockLayer(rom, blockAddress, layer, tilesets.TileImage, tilesets.Palettes, pixels);
				}
				if (settings.TripleLayerByNextBlock && IsTripleLayerBlock(rom, blockNumber, settings, tilesets) &&
					TryGetBlockAddress(rom, blockNumber + 1, settings, tilesets, out uint nextBlockAddress))
				{
					DrawBlockLayer(rom, nextBlockAddress, 0, tilesets.TileImage, tilesets.Palettes, pixels);
				}
			}

			tilesets.Blocks[blockNumber] = pixels;
			return pixels;
		}

		//-------------------------------------------------------------------------------
		// ブロック番号に対応する第1・第2ブロック表内の位置を求める処理
		//-------------------------------------------------------------------------------
		private static bool TryGetBlockAddress(byte[] rom, int blockNumber, Settings settings, TilesetData tilesets, out uint address)
		{
			address = 0;
			if (blockNumber < 0 || blockNumber > 1023)
			{
				return false;
			}

			uint tableAddress;
			int tableIndex;
			if (blockNumber < settings.PrimaryBlockCount)
			{
				tableAddress = tilesets.PrimaryBlockAddress;
				tableIndex = blockNumber;
			}
			else
			{
				tableAddress = tilesets.SecondaryBlockAddress;
				tableIndex = blockNumber - settings.PrimaryBlockCount;
			}

			long result = (long)tableAddress + (long)tableIndex * settings.BlockBytes;
			if (!CanRead(rom, result, settings.BlockBytes))
			{
				return false;
			}
			address = (uint)result;
			return true;
		}

		//-------------------------------------------------------------------------------
		// FR の挙動データから次ブロックを第3層に使う印を調べる処理
		//-------------------------------------------------------------------------------
		private static bool IsTripleLayerBlock(byte[] rom, int blockNumber, Settings settings, TilesetData tilesets)
		{
			uint tableAddress;
			int tableIndex;
			if (blockNumber < settings.PrimaryBlockCount)
			{
				tableAddress = tilesets.PrimaryBehaviorAddress;
				tableIndex = blockNumber;
			}
			else
			{
				tableAddress = tilesets.SecondaryBehaviorAddress;
				tableIndex = blockNumber - settings.PrimaryBlockCount;
			}
			long address = (long)tableAddress + tableIndex * 4L;
			return tableAddress != 0 && CanRead(rom, address, 4) && (rom[(int)address + 3] & 0xFC) == 0x30;
		}

		//-------------------------------------------------------------------------------
		// ブロック内の 1 層（4 タイル）を透明色を残して重ね描きする処理
		//-------------------------------------------------------------------------------
		private static void DrawBlockLayer(byte[] rom, uint blockAddress, int layer, byte[] tileImage, int[] palettes, int[] pixels)
		{
			long layerAddress = (long)blockAddress + layer * 8L;
			if (!CanRead(rom, layerAddress, 8))
			{
				return;
			}
			for (int tilePosition = 0; tilePosition < 4; tilePosition++)
			{
				int entryAddress = (int)layerAddress + tilePosition * 2;
				ushort entry = (ushort)(rom[entryAddress] | (rom[entryAddress + 1] << 8));
				DrawTile(entry, tilePosition % 2 * 8, tilePosition / 2 * 8, tileImage, palettes, pixels);
			}
		}

		//-------------------------------------------------------------------------------
		// 4bpp タイル 1 枚を反転・パレット指定に従ってブロック画像へ描く処理
		//-------------------------------------------------------------------------------
		private static void DrawTile(ushort entry, int offsetX, int offsetY, byte[] tileImage, int[] palettes, int[] pixels)
		{
			int tileNumber = entry & 0x03FF;
			int tileAddress = tileNumber * TileBytes;
			int paletteNumber = (entry >> 12) & 0x0F;
			if (paletteNumber >= PaletteCount || tileAddress < 0 || tileAddress + TileBytes > tileImage.Length)
			{
				return;
			}

			bool flipX = (entry & 0x0400) != 0;
			bool flipY = (entry & 0x0800) != 0;
			int paletteOffset = paletteNumber * 16;
			for (int sourceY = 0; sourceY < 8; sourceY++)
			{
				for (int sourceX = 0; sourceX < 8; sourceX++)
				{
					byte packed = tileImage[tileAddress + sourceY * 4 + sourceX / 2];
					int colorNumber = (sourceX & 1) == 0 ? packed & 0x0F : packed >> 4;
					if (colorNumber == 0)
					{
						continue;
					}
					int destinationX = offsetX + (flipX ? 7 - sourceX : sourceX);
					int destinationY = offsetY + (flipY ? 7 - sourceY : sourceY);
					pixels[destinationY * BlockPixels + destinationX] = palettes[paletteOffset + colorNumber];
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 16×16 のブロック画像をマップ全体の ARGB 配列へ写す処理
		//-------------------------------------------------------------------------------
		private static void CopyBlock(int[] block, int[] destination, int destinationWidth, int x, int y)
		{
			for (int row = 0; row < BlockPixels; row++)
			{
				Array.Copy(block, row * BlockPixels, destination, (y + row) * destinationWidth + x, BlockPixels);
			}
		}

		//-------------------------------------------------------------------------------
		// ARGB 配列を縦横比を保って面積平均で縮小し、中央配置の Bitmap にする処理
		//-------------------------------------------------------------------------------
		private static Bitmap ScaleToBitmap(int[] source, int sourceWidth, int sourceHeight, int size)
		{
			double scale = Math.Min(1.0, Math.Min((double)size / sourceWidth, (double)size / sourceHeight));
			int targetWidth = Math.Max(1, Math.Min(size, (int)Math.Round(sourceWidth * scale)));
			int targetHeight = Math.Max(1, Math.Min(size, (int)Math.Round(sourceHeight * scale)));
			int[] output = new int[size * size];
			int offsetX = (size - targetWidth) / 2;
			int offsetY = (size - targetHeight) / 2;

			for (int y = 0; y < targetHeight; y++)
			{
				int sourceTop = y * sourceHeight / targetHeight;
				int sourceBottom = Math.Max(sourceTop + 1, (int)Math.Ceiling((double)(y + 1) * sourceHeight / targetHeight));
				for (int x = 0; x < targetWidth; x++)
				{
					int sourceLeft = x * sourceWidth / targetWidth;
					int sourceRight = Math.Max(sourceLeft + 1, (int)Math.Ceiling((double)(x + 1) * sourceWidth / targetWidth));
					long alpha = 0;
					long red = 0;
					long green = 0;
					long blue = 0;
					int count = 0;
					for (int sourceY = sourceTop; sourceY < sourceBottom; sourceY++)
					{
						for (int sourceX = sourceLeft; sourceX < sourceRight; sourceX++)
						{
							int color = source[sourceY * sourceWidth + sourceX];
							alpha += (uint)color >> 24;
							red += (color >> 16) & 0xFF;
							green += (color >> 8) & 0xFF;
							blue += color & 0xFF;
							count++;
						}
					}
					output[(offsetY + y) * size + offsetX + x] = (int)((alpha / count << 24) | (red / count << 16) | (green / count << 8) | blue / count);
				}
			}

			Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			try
			{
				if (data.Stride == size * 4)
				{
					Marshal.Copy(output, 0, data.Scan0, output.Length);
				}
				else
				{
					for (int row = 0; row < size; row++)
					{
						Marshal.Copy(output, row * size, IntPtr.Add(data.Scan0, row * data.Stride), size);
					}
				}
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
			return bitmap;
		}

		//-------------------------------------------------------------------------------
		// 書き出し用: マップ全体を等倍（1 ブロック 16×16）で描く処理（400 万画素を超えるマップはブロック単位で間引く）
		// 描けなければ null
		//-------------------------------------------------------------------------------
		public static Bitmap RenderFullMap(byte[] rom, uint footerOffset, Settings settings)
		{
			try
			{
				if (rom == null || settings == null || !CanRead(rom, footerOffset, 24))
				{
					return null;
				}
				int mapWidth = BitConverter.ToInt32(rom, (int)footerOffset);
				int mapHeight = BitConverter.ToInt32(rom, (int)footerOffset + 4);
				if (mapWidth < 1 || mapWidth > 512 || mapHeight < 1 || mapHeight > 512 ||
					!TryReadPointer(rom, (long)footerOffset + 12, out uint mapAddress) ||
					!TryReadPointer(rom, (long)footerOffset + 16, out uint primaryTilesetAddress) ||
					!TryReadPointer(rom, (long)footerOffset + 20, out uint secondaryTilesetAddress) ||
					!CanRead(rom, mapAddress, (long)mapWidth * mapHeight * 2))
				{
					return null;
				}
				TilesetData tilesets = GetTilesetData(rom, primaryTilesetAddress, secondaryTilesetAddress, settings, new TilesetCache());
				if (tilesets == null)
				{
					return null;
				}
				int step = GetSampleStep(mapWidth, mapHeight);
				int width = (mapWidth + step - 1) / step * BlockPixels;
				int height = (mapHeight + step - 1) / step * BlockPixels;
				int[] pixels = new int[width * height];
				for (int sampleY = 0, mapY = 0; mapY < mapHeight; sampleY++, mapY += step)
				{
					for (int sampleX = 0, mapX = 0; mapX < mapWidth; sampleX++, mapX += step)
					{
						int blockNumber = BitConverter.ToUInt16(rom, (int)mapAddress + (mapY * mapWidth + mapX) * 2) & 0x03FF;
						CopyBlock(GetBlockPixels(rom, blockNumber, settings, tilesets), pixels, width, sampleX * BlockPixels, sampleY * BlockPixels);
					}
				}
				return ToBitmap(pixels, width, height);
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// 書き出し用: 指定の番号から count 個のブロックを、横 columns 個ずつ並べて描く処理（透明部分は透明のまま）
		// primaryHeader・secondaryHeader はタイルセット見出しの ROM 内の位置。描けなければ null
		//-------------------------------------------------------------------------------
		public static Bitmap RenderBlockSheet(byte[] rom, uint primaryHeader, uint secondaryHeader, Settings settings, int firstBlock, int count, int columns)
		{
			try
			{
				if (rom == null || settings == null || count <= 0 || columns <= 0)
				{
					return null;
				}
				TilesetData tilesets = GetTilesetData(rom, primaryHeader, secondaryHeader, settings, new TilesetCache());
				if (tilesets == null)
				{
					return null;
				}
				int rows = (count + columns - 1) / columns;
				int width = columns * BlockPixels;
				int height = rows * BlockPixels;
				int[] pixels = new int[width * height];
				for (int i = 0; i < count; i++)
				{
					CopyBlock(GetBlockPixels(rom, firstBlock + i, settings, tilesets), pixels, width, i % columns * BlockPixels, i / columns * BlockPixels);
				}
				return ToBitmap(pixels, width, height);
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// ARGB 配列をそのままの大きさの Bitmap にする処理
		//-------------------------------------------------------------------------------
		private static Bitmap ToBitmap(int[] pixels, int width, int height)
		{
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			try
			{
				for (int row = 0; row < height; row++)
				{
					Marshal.Copy(pixels, row * width, IntPtr.Add(data.Scan0, row * data.Stride), width);
				}
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
			return bitmap;
		}

		//-------------------------------------------------------------------------------
		// ROM ポインタを ROM 内オフセットへ変換して読む処理
		//-------------------------------------------------------------------------------
		private static bool TryReadPointer(byte[] rom, long address, out uint offset)
		{
			offset = 0;
			if (!CanRead(rom, address, 4))
			{
				return false;
			}
			uint pointer = BitConverter.ToUInt32(rom, (int)address);
			if (pointer < RomPointerBase)
			{
				return false;
			}
			uint result = pointer - RomPointerBase;
			if (result >= rom.Length)
			{
				return false;
			}
			offset = result;
			return true;
		}

		//-------------------------------------------------------------------------------
		// 指定範囲が ROM の中に完全に収まるか調べる処理
		//-------------------------------------------------------------------------------
		private static bool CanRead(byte[] rom, long address, long length)
		{
			return rom != null && address >= 0 && length >= 0 && address <= rom.LongLength - length;
		}
	}
}
