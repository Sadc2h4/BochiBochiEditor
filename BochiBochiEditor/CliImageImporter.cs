using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// CLI向けの画像導入処理をまとめて実行する処理
	//-------------------------------------------------------------------------------
	internal static class CliImageImporter
	{
		private const uint GbaRomPointerBase = 134217728U;
		private const int PokemonTableEntryLength = 8;
		private static readonly Regex NumericNamePattern = new Regex(@"\d+", RegexOptions.Compiled);

		//-------------------------------------------------------------------------------
		// CLIオプションを元に画像導入処理を実行する処理
		//-------------------------------------------------------------------------------
		public static ImportBatchResult Import(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			string requiredOption = RequireOption(options, "target");
			string requiredOption2 = RequireOption(options, "source-dir");
			string pokemonOrderMode = GetPokemonOrderMode(options);
			if (!Directory.Exists(requiredOption2))
			{
				throw new DirectoryNotFoundException("画像フォルダが見つかりません。: " + requiredOption2);
			}
			uint requiredRomAddressOption = RequireRomAddressOption(options, "start");
			string romPath = RequireOption(options, "rom");
			string romOutputPath = GetRomOutputPath(options, romPath);
			string changeLogOutputPath = GetChangeLogOutputPath(options, romOutputPath, requiredOption);
			ImportBatchResult importBatchResult = new ImportBatchResult();
			importBatchResult.Target = requiredOption;
			importBatchResult.SourceDirectory = Path.GetFullPath(requiredOption2);
			importBatchResult.InputRomPath = Path.GetFullPath(romPath);
			importBatchResult.OutputRomPath = Path.GetFullPath(romOutputPath);
			importBatchResult.ChangeLogPath = Path.GetFullPath(changeLogOutputPath);
			importBatchResult.StartAddress = FormatRomAddress(requiredRomAddressOption);
			importBatchResult.OrderMode = pokemonOrderMode;
			importBatchResult.Entries = new List<ImportEntryResult>();
			List<SourceImageEntry> sourceImageEntries = EnumerateSourceImages(requiredOption2);
			if (sourceImageEntries.Count == 0)
			{
				throw new InvalidOperationException("対象フォルダにPNG/BMP画像が見つかりません。");
			}
			uint num = requiredRomAddressOption;
			foreach (SourceImageEntry sourceImageEntry in sourceImageEntries)
			{
				ImportEntryResult item;
				switch (requiredOption.ToLowerInvariant())
				{
				case "pokemon-sprite":
					item = ImportPokemonSprite(romData, sourceImageEntry, ref num, pokemonOrderMode);
					break;
				case "item-image":
					item = ImportItemImage(romData, sourceImageEntry, ref num);
					break;
				case "pokemon-icon":
					item = ImportPokemonIcon(romData, sourceImageEntry, ref num, options, pokemonOrderMode);
					break;
				case "trainer-image":
					item = ImportTrainerImage(romData, sourceImageEntry, ref num);
					break;
				default:
					throw new ArgumentException("未対応のtargetです。: " + requiredOption);
				}
				importBatchResult.Entries.Add(item);
			}
			importBatchResult.ImportCount = importBatchResult.Entries.Count;
			importBatchResult.LastAllocatedAddress = FormatRomAddress(num);
			File.WriteAllBytes(romOutputPath, romData);
			File.WriteAllText(changeLogOutputPath, BuildChangeLogText(importBatchResult), new UTF8Encoding(false));
			return importBatchResult;
		}

		//-------------------------------------------------------------------------------
		// CLIオプションを元に画像書き出し処理を実行する処理
		//-------------------------------------------------------------------------------
		public static ExportBatchResult Export(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			string requiredOption = RequireOption(options, "target");
			string requiredOption2 = RequireOption(options, "source-dir");
			string pokemonOrderMode = GetPokemonOrderMode(options);
			Directory.CreateDirectory(requiredOption2);
			List<ImageTargetEntry> imageTargetEntries = GetImageTargetEntries(romData, requiredOption, pokemonOrderMode);
			ExportBatchResult exportBatchResult = new ExportBatchResult();
			exportBatchResult.Target = requiredOption;
			exportBatchResult.SourceDirectory = Path.GetFullPath(requiredOption2);
			exportBatchResult.InputRomPath = Path.GetFullPath(RequireOption(options, "rom"));
			exportBatchResult.OrderMode = pokemonOrderMode;
			exportBatchResult.Entries = new List<ExportEntryResult>();
			foreach (ImageTargetEntry imageTargetEntry in imageTargetEntries)
			{
				string text = Path.Combine(requiredOption2, imageTargetEntry.RequestedId.ToString("D3", CultureInfo.InvariantCulture) + ".png");
				switch (requiredOption.ToLowerInvariant())
				{
				case "pokemon-sprite":
					ExportPokemonSprite(romData, imageTargetEntry.InternalId, text);
					break;
				case "item-image":
					ExportItemImage(romData, imageTargetEntry.InternalId, text);
					break;
				case "pokemon-icon":
					ExportPokemonIcon(romData, imageTargetEntry.InternalId, text);
					break;
				case "trainer-image":
					ExportTrainerImage(romData, imageTargetEntry.InternalId, text);
					break;
				default:
					throw new ArgumentException("未対応のtargetです。: " + requiredOption);
				}
				exportBatchResult.Entries.Add(new ExportEntryResult
				{
					Target = requiredOption,
					RequestedId = imageTargetEntry.RequestedId,
					InternalId = imageTargetEntry.InternalId,
					Name = imageTargetEntry.Name,
					OutputPath = Path.GetFullPath(text)
				});
			}
			exportBatchResult.ExportCount = exportBatchResult.Entries.Count;
			return exportBatchResult;
		}

		//-------------------------------------------------------------------------------
		// CLIオプションを元に一覧シート画像を書き出す処理
		//-------------------------------------------------------------------------------
		public static SheetExportResult ExportSheet(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			string requiredOption = RequireOption(options, "target");
			string pokemonOrderMode = GetPokemonOrderMode(options);
			string requiredOption2 = RequireOption(options, "sheet-out");
			string variant = GetVariant(options, requiredOption);
			List<ImageTargetEntry> imageTargetEntries = GetImageTargetEntries(romData, requiredOption, pokemonOrderMode);
			Size targetCellSize = GetCellSize(requiredOption, variant);
			int num = 16;
			int num2 = (int)Math.Ceiling(imageTargetEntries.Count / (double)num);
			using (Bitmap bitmap = new Bitmap(targetCellSize.Width * num, targetCellSize.Height * Math.Max(1, num2)))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.Clear(Color.Transparent);
					for (int i = 0; i < imageTargetEntries.Count; i++)
					{
						ImageTargetEntry imageTargetEntry = imageTargetEntries[i];
						using (Bitmap bitmap2 = LoadPreviewBitmap(romData, requiredOption, imageTargetEntry.InternalId, variant))
						{
							int x = i % num * targetCellSize.Width;
							int y = i / num * targetCellSize.Height;
							graphics.DrawImage(bitmap2, x, y, targetCellSize.Width, targetCellSize.Height);
						}
					}
				}
				string directoryName = Path.GetDirectoryName(Path.GetFullPath(requiredOption2));
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				bitmap.Save(requiredOption2, ImageFormat.Png);
			}
			return new SheetExportResult
			{
				Target = requiredOption,
				OrderMode = pokemonOrderMode,
				Variant = variant,
				OutputPath = Path.GetFullPath(requiredOption2),
				ImageCount = imageTargetEntries.Count,
				Columns = num,
				Rows = num2
			};
		}

		//-------------------------------------------------------------------------------
		// 導入対象の画像ファイル一覧を番号順で取得する処理
		//-------------------------------------------------------------------------------
		private static List<SourceImageEntry> EnumerateSourceImages(string sourceDirectory)
		{
			return Directory.EnumerateFiles(sourceDirectory)
				.Where((string path) => IsSupportedImageExtension(path))
				.Select(CreateSourceImageEntry)
				.OrderBy((SourceImageEntry entry) => entry.Id)
				.ThenBy((SourceImageEntry entry) => entry.FileName, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		//-------------------------------------------------------------------------------
		// 画像ファイルから導入対象情報を生成する処理
		//-------------------------------------------------------------------------------
		private static SourceImageEntry CreateSourceImageEntry(string path)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			Match match = NumericNamePattern.Match(fileNameWithoutExtension ?? string.Empty);
			if (!match.Success || !int.TryParse(match.Value, out int result))
			{
				throw new InvalidOperationException("画像ファイル名にID番号が含まれていません。: " + Path.GetFileName(path));
			}
			SourceImageEntry sourceImageEntry = new SourceImageEntry();
			sourceImageEntry.Id = result;
			sourceImageEntry.FileName = Path.GetFileName(path);
			sourceImageEntry.FullPath = Path.GetFullPath(path);
			return sourceImageEntry;
		}

		//-------------------------------------------------------------------------------
		// ポケモン複合画像をROMへ導入する処理
		//-------------------------------------------------------------------------------
		private static ImportEntryResult ImportPokemonSprite(byte[] romData, SourceImageEntry sourceImageEntry, ref uint nextSearchAddress, string pokemonOrderMode)
		{
			int num = ResolvePokemonInternalId(romData, sourceImageEntry.Id, pokemonOrderMode);
			PokemonImageTableOffsets pokemonImageTableOffsets = ReadPokemonImageTableOffsets();
			string pokemonName = ReadPokemonName(romData, num);
			uint pointerAddress = ReadPointerAddress(romData, pokemonImageTableOffsets.FrontImageTableOffset, num, PokemonTableEntryLength);
			uint pointerAddress2 = ReadPointerAddress(romData, pokemonImageTableOffsets.BackImageTableOffset, num, PokemonTableEntryLength);
			uint pointerAddress3 = ReadPointerAddress(romData, pokemonImageTableOffsets.NormalPaletteTableOffset, num, PokemonTableEntryLength);
			uint pointerAddress4 = ReadPointerAddress(romData, pokemonImageTableOffsets.ShinyPaletteTableOffset, num, PokemonTableEntryLength);
			using (Bitmap bitmap = new Bitmap(sourceImageEntry.FullPath))
			{
				PokemonSpriteImportData pokemonSpriteImportData = BuildPokemonSpriteImportData(bitmap);
				uint num2 = AllocateAndWriteData(romData, pokemonSpriteImportData.FrontImageData, ref nextSearchAddress);
				uint num3 = AllocateAndWriteData(romData, pokemonSpriteImportData.BackImageData, ref nextSearchAddress);
				uint num4 = AllocateAndWriteData(romData, pokemonSpriteImportData.NormalPaletteData, ref nextSearchAddress);
				uint num5 = AllocateAndWriteData(romData, pokemonSpriteImportData.ShinyPaletteData, ref nextSearchAddress);
				WritePointerAddress(romData, pokemonImageTableOffsets.FrontImageTableOffset, num, PokemonTableEntryLength, num2);
				WritePointerAddress(romData, pokemonImageTableOffsets.BackImageTableOffset, num, PokemonTableEntryLength, num3);
				WritePointerAddress(romData, pokemonImageTableOffsets.NormalPaletteTableOffset, num, PokemonTableEntryLength, num4);
				WritePointerAddress(romData, pokemonImageTableOffsets.ShinyPaletteTableOffset, num, PokemonTableEntryLength, num5);
				ImportEntryResult importEntryResult = CreateEntryResult("pokemon-sprite", sourceImageEntry, sourceImageEntry.Id, pokemonName);
				importEntryResult.InternalId = num;
				importEntryResult.Before = string.Format(CultureInfo.InvariantCulture, "front={0}, back={1}, normalPal={2}, shinyPal={3}", FormatRomAddress(pointerAddress), FormatRomAddress(pointerAddress2), FormatRomAddress(pointerAddress3), FormatRomAddress(pointerAddress4));
				importEntryResult.After = string.Format(CultureInfo.InvariantCulture, "front={0}, back={1}, normalPal={2}, shinyPal={3}", FormatRomAddress(num2), FormatRomAddress(num3), FormatRomAddress(num4), FormatRomAddress(num5));
				return importEntryResult;
			}
		}

		//-------------------------------------------------------------------------------
		// アイテム画像をROMへ導入する処理
		//-------------------------------------------------------------------------------
		private static ImportEntryResult ImportItemImage(byte[] romData, SourceImageEntry sourceImageEntry, ref uint nextSearchAddress)
		{
			ValidateIndexRange(sourceImageEntry.Id, ItemData.TOTAL_ITEM_COUNT, "item");
			ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(romData, checked((ushort)sourceImageEntry.Id));
			using (Bitmap bitmap = new Bitmap(sourceImageEntry.FullPath))
			{
				EnsureIndexedBitmap(bitmap, ItemData.ITEM_IMAGE_WIDTH, ItemData.ITEM_IMAGE_HEIGHT, 16, "アイテム画像");
				byte[,] indexedPixels = ReadIndexedPixels(bitmap);
				byte[] array = EncodeSprite(indexedPixels, 0, 0, ItemData.ITEM_IMAGE_WIDTH, ItemData.ITEM_IMAGE_HEIGHT, 0);
				byte[] array2 = ImageProcessor.LZ77Comp(array, false);
				byte[] array3 = ImageProcessor.LZ77Comp(ConvertPaletteEntriesToBytes(ReadPaletteSegment(bitmap.Palette, 0, 16)), true);
				uint num = AllocateAndWriteData(romData, array2, ref nextSearchAddress);
				uint num2 = AllocateAndWriteData(romData, array3, ref nextSearchAddress);
				int num3 = ItemData.ITEM_IMAGE_TABLE_OFFSET + sourceImageEntry.Id * ItemData.ITEM_IMAGE_ENTRY_LENGTH;
				WriteRawPointer(romData, num3, num);
				WriteRawPointer(romData, num3 + 4, num2);
				ImportEntryResult importEntryResult = CreateEntryResult("item-image", sourceImageEntry, sourceImageEntry.Id, itemInfo.Name);
				importEntryResult.InternalId = sourceImageEntry.Id;
				importEntryResult.Before = string.Format(CultureInfo.InvariantCulture, "image={0}, palette={1}", FormatRomAddress(itemInfo.ImageAddress), FormatRomAddress(itemInfo.PaletteAddress));
				importEntryResult.After = string.Format(CultureInfo.InvariantCulture, "image={0}, palette={1}", FormatRomAddress(num), FormatRomAddress(num2));
				return importEntryResult;
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン手持ちアイコン画像をROMへ導入する処理
		//-------------------------------------------------------------------------------
		private static ImportEntryResult ImportPokemonIcon(byte[] romData, SourceImageEntry sourceImageEntry, ref uint nextSearchAddress, Dictionary<string, string> options, string pokemonOrderMode)
		{
			int num = ResolvePokemonInternalId(romData, sourceImageEntry.Id, pokemonOrderMode);
			int num2 = RomIniReader.ReadHexOrDecimal("ICON_IMAGE_TABLE_OFFSET");
			int num3 = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_ID_TABLE_OFFSET");
			int num4 = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_COUNT");
			uint rawPointer = ReadRawPointer(romData, num2 + num * 4);
			byte b = romData[num3 + num];
			int iconPaletteId = b;
			if (options.TryGetValue("icon-palette-id", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				iconPaletteId = ParseInteger(value);
			}
			if (iconPaletteId < 0 || iconPaletteId >= num4)
			{
				throw new ArgumentOutOfRangeException("icon-palette-id", "有効なアイコンパレットIDを指定してください。");
			}
			string pokemonName = ReadPokemonName(romData, num);
			using (Bitmap bitmap = new Bitmap(sourceImageEntry.FullPath))
			{
				EnsureIndexedBitmap(bitmap, 32, 64, 16, "ポケモンアイコン");
				byte[,] indexedPixels = ReadIndexedPixels(bitmap);
				byte[] data = EncodeSprite(indexedPixels, 0, 0, 32, 64, 0);
				uint num5 = AllocateAndWriteData(romData, data, ref nextSearchAddress);
				WriteRawPointer(romData, num2 + num * 4, num5);
				romData[num3 + num] = checked((byte)iconPaletteId);
				ImportEntryResult importEntryResult = CreateEntryResult("pokemon-icon", sourceImageEntry, sourceImageEntry.Id, pokemonName);
				importEntryResult.InternalId = num;
				importEntryResult.Before = string.Format(CultureInfo.InvariantCulture, "image={0}, paletteId={1}", FormatRomAddress(rawPointer), b);
				importEntryResult.After = string.Format(CultureInfo.InvariantCulture, "image={0}, paletteId={1}", FormatRomAddress(num5), iconPaletteId);
				return importEntryResult;
			}
		}

		//-------------------------------------------------------------------------------
		// トレーナー画像をROMへ導入する処理
		//-------------------------------------------------------------------------------
		private static ImportEntryResult ImportTrainerImage(byte[] romData, SourceImageEntry sourceImageEntry, ref uint nextSearchAddress)
		{
			int num = RomIniReader.ReadHexOrDecimal("MAX_TRAINER_SPRITE_COUNT");
			ValidateIndexRange(sourceImageEntry.Id, num, "trainer-image");
			int num2 = RomIniReader.ReadHexOrDecimal("TRAINER_SPRITE_TABLE_OFFSET");
			int num3 = RomIniReader.ReadHexOrDecimal("TRAINER_PALETTE_TABLE_OFFSET");
			uint rawPointer = ReadPointerAddress(romData, num2, sourceImageEntry.Id, PokemonTableEntryLength);
			uint rawPointer2 = ReadPointerAddress(romData, num3, sourceImageEntry.Id, PokemonTableEntryLength);
			using (Bitmap bitmap = new Bitmap(sourceImageEntry.FullPath))
			{
				EnsureIndexedBitmap(bitmap, 64, 64, 16, "トレーナー画像");
				byte[,] indexedPixels = ReadIndexedPixels(bitmap);
				byte[] array = ImageProcessor.LZ77Comp(EncodeSprite(indexedPixels, 0, 0, 64, 64, 0), false);
				byte[] array2 = ImageProcessor.LZ77Comp(ConvertPaletteEntriesToBytes(ReadPaletteSegment(bitmap.Palette, 0, 16)), true);
				uint num4 = AllocateAndWriteData(romData, array, ref nextSearchAddress);
				uint num5 = AllocateAndWriteData(romData, array2, ref nextSearchAddress);
				WritePointerAddress(romData, num2, sourceImageEntry.Id, PokemonTableEntryLength, num4);
				WritePointerAddress(romData, num3, sourceImageEntry.Id, PokemonTableEntryLength, num5);
				ImportEntryResult importEntryResult = CreateEntryResult("trainer-image", sourceImageEntry, sourceImageEntry.Id, BuildTrainerName(romData, sourceImageEntry.Id));
				importEntryResult.InternalId = sourceImageEntry.Id;
				importEntryResult.Before = string.Format(CultureInfo.InvariantCulture, "image={0}, palette={1}", FormatRomAddress(rawPointer), FormatRomAddress(rawPointer2));
				importEntryResult.After = string.Format(CultureInfo.InvariantCulture, "image={0}, palette={1}", FormatRomAddress(num4), FormatRomAddress(num5));
				return importEntryResult;
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン複合画像から導入用データを組み立てる処理
		//-------------------------------------------------------------------------------
		private static PokemonSpriteImportData BuildPokemonSpriteImportData(Bitmap bitmap)
		{
			EnsureIndexedBitmap(bitmap, 256, 64, 32, "ポケモン画像");
			byte[,] indexedPixels = ReadIndexedPixels(bitmap);
			ValidatePokemonSpriteLayout(indexedPixels);
			byte[] array = EncodeSprite(indexedPixels, 0, 0, 64, 64, 0);
			byte[] array2 = EncodeSprite(indexedPixels, 128, 0, 64, 64, 0);
			byte[] array3 = ConvertPaletteEntriesToBytes(ReadPaletteSegment(bitmap.Palette, 0, 16));
			byte[] array4 = ConvertPaletteEntriesToBytes(ReadPaletteSegment(bitmap.Palette, 16, 16));
			PokemonSpriteImportData pokemonSpriteImportData = new PokemonSpriteImportData();
			pokemonSpriteImportData.FrontImageData = ImageProcessor.LZ77Comp(array, false);
			pokemonSpriteImportData.BackImageData = ImageProcessor.LZ77Comp(array2, false);
			pokemonSpriteImportData.NormalPaletteData = ImageProcessor.LZ77Comp(array3, true);
			pokemonSpriteImportData.ShinyPaletteData = ImageProcessor.LZ77Comp(array4, true);
			return pokemonSpriteImportData;
		}

		//-------------------------------------------------------------------------------
		// ポケモン複合画像のレイアウト条件を検証する処理
		//-------------------------------------------------------------------------------
		private static void ValidatePokemonSpriteLayout(byte[,] indexedPixels)
		{
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					byte b = indexedPixels[i, j];
					byte b2 = indexedPixels[i, j + 64];
					byte b3 = indexedPixels[i, j + 128];
					byte b4 = indexedPixels[i, j + 192];
					if (b > 15)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ポケモン画像の1枠目はパレット0-15のみ使用できます。座標({0},{1})", j, i));
					}
					if (b3 > 15)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ポケモン画像の3枠目はパレット0-15のみ使用できます。座標({0},{1})", j + 128, i));
					}
					if (b2 < 16 || b2 > 31)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ポケモン画像の2枠目はパレット16-31のみ使用できます。座標({0},{1})", j + 64, i));
					}
					if (b4 < 16 || b4 > 31)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ポケモン画像の4枠目はパレット16-31のみ使用できます。座標({0},{1})", j + 192, i));
					}
					if (b != b2 - 16)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ポケモン画像の1枠目と2枠目の形状が一致していません。座標({0},{1})", j, i));
					}
					if (b3 != b4 - 16)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "ポケモン画像の3枠目と4枠目の形状が一致していません。座標({0},{1})", j, i));
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 指定領域のインデックス画像をGBAタイルデータへ変換する処理
		//-------------------------------------------------------------------------------
		private static byte[] EncodeSprite(byte[,] indexedPixels, int startX, int startY, int width, int height, int paletteBaseIndex)
		{
			List<byte> list = new List<byte>(width * height / 2);
			for (int i = startY; i < startY + height; i += 8)
			{
				for (int j = startX; j < startX + width; j += 8)
				{
					for (int k = 0; k < 8; k++)
					{
						for (int l = 0; l < 8; l += 2)
						{
							int num = indexedPixels[i + k, j + l] - paletteBaseIndex;
							int num2 = indexedPixels[i + k, j + l + 1] - paletteBaseIndex;
							if (num < 0 || num > 15 || num2 < 0 || num2 > 15)
							{
								throw new InvalidOperationException("画像のパレットインデックスが4bpp範囲外です。");
							}
							list.Add((byte)(num | (num2 << 4)));
						}
					}
				}
			}
			return list.ToArray();
		}

		//-------------------------------------------------------------------------------
		// GBAタイルデータをインデックス画像へ復元する処理
		//-------------------------------------------------------------------------------
		private static byte[,] DecodeSprite(byte[] spriteData, int width, int height, int paletteBaseIndex)
		{
			byte[,] array = new byte[height, width];
			int num = 0;
			for (int i = 0; i < height; i += 8)
			{
				for (int j = 0; j < width; j += 8)
				{
					for (int k = 0; k < 8; k++)
					{
						for (int l = 0; l < 8; l += 2)
						{
							byte b = (num < spriteData.Length) ? spriteData[num] : ((byte)0);
							num++;
							array[i + k, j + l] = (byte)((b & 15) + paletteBaseIndex);
							array[i + k, j + l + 1] = (byte)(((b >> 4) & 15) + paletteBaseIndex);
						}
					}
				}
			}
			return array;
		}

		//-------------------------------------------------------------------------------
		// 指定Bitmapのインデックス配列を読み取る処理
		//-------------------------------------------------------------------------------
		private static byte[,] ReadIndexedPixels(Bitmap bitmap)
		{
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			try
			{
				int num = Math.Abs(bitmapData.Stride);
				byte[] array = new byte[num * bitmap.Height];
				System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
				byte[,] array2 = new byte[bitmap.Height, bitmap.Width];
				for (int i = 0; i < bitmap.Height; i++)
				{
					int num2 = (bitmapData.Stride < 0) ? ((bitmap.Height - 1 - i) * num) : (i * num);
					for (int j = 0; j < bitmap.Width; j++)
					{
						switch (bitmap.PixelFormat)
						{
						case PixelFormat.Format4bppIndexed:
						{
							byte b = array[num2 + j / 2];
							array2[i, j] = ((j % 2 == 0) ? (byte)(b >> 4) : (byte)(b & 15));
							break;
						}
						case PixelFormat.Format8bppIndexed:
							array2[i, j] = array[num2 + j];
							break;
						default:
							throw new InvalidOperationException("インデックスカラー画像のみ対応しています。");
						}
					}
				}
				return array2;
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}
		}

		//-------------------------------------------------------------------------------
		// Bitmapの形式とサイズを検証する処理
		//-------------------------------------------------------------------------------
		private static void EnsureIndexedBitmap(Bitmap bitmap, int width, int height, int maxPaletteCount, string label)
		{
			if (bitmap.Width != width || bitmap.Height != height)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "{0}のサイズは{1}x{2}である必要があります。", label, width, height));
			}
			if (bitmap.PixelFormat != PixelFormat.Format4bppIndexed && bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
			{
				throw new InvalidOperationException(label + "は4bppまたは8bppのインデックスカラー画像のみ対応しています。");
			}
			if (bitmap.Palette.Entries.Length < maxPaletteCount)
			{
				throw new InvalidOperationException(label + "のパレット数が不足しています。");
			}
		}

		//-------------------------------------------------------------------------------
		// パレットの指定範囲を配列として読み出す処理
		//-------------------------------------------------------------------------------
		private static Color[] ReadPaletteSegment(ColorPalette palette, int startIndex, int count)
		{
			Color[] array = new Color[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = palette.Entries[startIndex + i];
			}
			return array;
		}

		//-------------------------------------------------------------------------------
		// 色配列をGBAパレットデータへ変換する処理
		//-------------------------------------------------------------------------------
		private static byte[] ConvertPaletteEntriesToBytes(Color[] colors)
		{
			byte[] array = new byte[32];
			for (int i = 0; i < Math.Min(colors.Length, 16); i++)
			{
				Color color = colors[i];
				int num = color.R >> 3;
				int num2 = color.G >> 3;
				int num3 = color.B >> 3;
				ushort num4 = (ushort)((num3 << 10) | (num2 << 5) | num);
				byte[] bytes = BitConverter.GetBytes(num4);
				array[i * 2] = bytes[0];
				array[i * 2 + 1] = bytes[1];
			}
			return array;
		}

		//-------------------------------------------------------------------------------
		// インデックス画像から8bpp PNGを書き出す処理
		//-------------------------------------------------------------------------------
		private static void SaveIndexedBitmap(string outputPath, byte[,] indexedPixels, Color[] paletteEntries)
		{
			using (Bitmap bitmap = new Bitmap(indexedPixels.GetLength(1), indexedPixels.GetLength(0), PixelFormat.Format8bppIndexed))
			{
				ColorPalette palette = bitmap.Palette;
				for (int i = 0; i < palette.Entries.Length; i++)
				{
					palette.Entries[i] = (i < paletteEntries.Length) ? paletteEntries[i] : Color.Transparent;
				}
				bitmap.Palette = palette;
				Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
				BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
				try
				{
					int num = Math.Abs(bitmapData.Stride);
					byte[] array = new byte[num * bitmap.Height];
					for (int j = 0; j < bitmap.Height; j++)
					{
						int num2 = (bitmapData.Stride < 0) ? ((bitmap.Height - 1 - j) * num) : (j * num);
						for (int k = 0; k < bitmap.Width; k++)
						{
							array[num2 + k] = indexedPixels[j, k];
						}
					}
					System.Runtime.InteropServices.Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
				}
				finally
				{
					bitmap.UnlockBits(bitmapData);
				}
				string directoryName = Path.GetDirectoryName(Path.GetFullPath(outputPath));
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				bitmap.Save(outputPath, ImageFormat.Png);
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン複合画像を書き出す処理
		//-------------------------------------------------------------------------------
		private static void ExportPokemonSprite(byte[] romData, int pokemonId, string outputPath)
		{
			PokemonSpriteResource pokemonSpriteResource = LoadPokemonSpriteResource(romData, pokemonId);
			byte[,] array = new byte[64, 256];
			BlitIndexedPixels(array, DecodeSprite(pokemonSpriteResource.FrontImage, 64, 64, 0), 0, 0);
			BlitIndexedPixels(array, DecodeSprite(pokemonSpriteResource.FrontImage, 64, 64, 16), 64, 0);
			BlitIndexedPixels(array, DecodeSprite(pokemonSpriteResource.BackImage, 64, 64, 0), 128, 0);
			BlitIndexedPixels(array, DecodeSprite(pokemonSpriteResource.BackImage, 64, 64, 16), 192, 0);
			Color[] array2 = new Color[32];
			Array.Copy(pokemonSpriteResource.NormalPalette, 0, array2, 0, Math.Min(16, pokemonSpriteResource.NormalPalette.Length));
			Array.Copy(pokemonSpriteResource.ShinyPalette, 0, array2, 16, Math.Min(16, pokemonSpriteResource.ShinyPalette.Length));
			SaveIndexedBitmap(outputPath, array, array2);
		}

		//-------------------------------------------------------------------------------
		// アイテム画像を書き出す処理
		//-------------------------------------------------------------------------------
		private static void ExportItemImage(byte[] romData, int itemId, string outputPath)
		{
			ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(romData, checked((ushort)itemId));
			byte[] compressedImagePaletteFromROM = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, itemInfo.ImageAddress, false);
			byte[] compressedImagePaletteFromROM2 = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, itemInfo.PaletteAddress, true);
			Color[] palette = ImageProcessor.LoadPalette(compressedImagePaletteFromROM2, true);
			SaveIndexedBitmap(outputPath, DecodeSprite(compressedImagePaletteFromROM, ItemData.ITEM_IMAGE_WIDTH, ItemData.ITEM_IMAGE_HEIGHT, 0), palette);
		}

		//-------------------------------------------------------------------------------
		// ポケモンアイコン画像を書き出す処理
		//-------------------------------------------------------------------------------
		private static void ExportPokemonIcon(byte[] romData, int pokemonId, string outputPath)
		{
			int num = RomIniReader.ReadHexOrDecimal("ICON_IMAGE_TABLE_OFFSET");
			int num2 = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_ID_TABLE_OFFSET");
			uint rawPointer = ReadRawPointer(romData, num + pokemonId * 4);
			byte[] compressedImagePaletteFromROM = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, rawPointer, false);
			Color[] palette = LoadIconPaletteColors(romData, romData[num2 + pokemonId]);
			SaveIndexedBitmap(outputPath, DecodeSprite(compressedImagePaletteFromROM, 32, 64, 0), palette);
		}

		//-------------------------------------------------------------------------------
		// トレーナー画像を書き出す処理
		//-------------------------------------------------------------------------------
		private static void ExportTrainerImage(byte[] romData, int trainerId, string outputPath)
		{
			int num = RomIniReader.ReadHexOrDecimal("TRAINER_SPRITE_TABLE_OFFSET");
			int num2 = RomIniReader.ReadHexOrDecimal("TRAINER_PALETTE_TABLE_OFFSET");
			uint pointerAddress = ReadPointerAddress(romData, num, trainerId, PokemonTableEntryLength);
			uint pointerAddress2 = ReadPointerAddress(romData, num2, trainerId, PokemonTableEntryLength);
			byte[] compressedImagePaletteFromROM = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, pointerAddress, false);
			byte[] compressedImagePaletteFromROM2 = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, pointerAddress2, true);
			Color[] palette = ImageProcessor.LoadPalette(compressedImagePaletteFromROM2, true);
			SaveIndexedBitmap(outputPath, DecodeSprite(compressedImagePaletteFromROM, 64, 64, 0), palette);
		}

		//-------------------------------------------------------------------------------
		// インデックス画像を指定座標へ貼り付ける処理
		//-------------------------------------------------------------------------------
		private static void BlitIndexedPixels(byte[,] dest, byte[,] source, int startX, int startY)
		{
			for (int i = 0; i < source.GetLength(0); i++)
			{
				for (int j = 0; j < source.GetLength(1); j++)
				{
					dest[startY + i, startX + j] = source[i, j];
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ROM内の空き領域を確保してデータを書き込む処理
		//-------------------------------------------------------------------------------
		private static uint AllocateAndWriteData(byte[] romData, byte[] data, ref uint nextSearchAddress)
		{
			uint num = FindFreeSpace(romData, nextSearchAddress, checked((uint)data.Length));
			Array.Copy(data, 0, romData, num, data.Length);
			nextSearchAddress = AlignAddress(checked(num + (uint)data.Length));
			return num;
		}

		//-------------------------------------------------------------------------------
		// 指定位置以降からFF連続領域を探して返す処理
		//-------------------------------------------------------------------------------
		private static uint FindFreeSpace(byte[] romData, uint startAddress, uint length)
		{
			uint num = AlignAddress(startAddress);
			while (num + length <= romData.Length)
			{
				bool flag = true;
				for (uint num2 = 0; num2 < length; num2 += 1U)
				{
					if (romData[num + num2] != byte.MaxValue)
					{
						flag = false;
						num = AlignAddress(num + num2 + 1U);
						break;
					}
				}
				if (flag)
				{
					return num;
				}
			}
			throw new InvalidOperationException("空き領域が不足しています。");
		}

		//-------------------------------------------------------------------------------
		// 4byte境界へアドレスを揃える処理
		//-------------------------------------------------------------------------------
		private static uint AlignAddress(uint address)
		{
			return (address + 3U) & 4294967292U;
		}

		//-------------------------------------------------------------------------------
		// ポケモン画像テーブル位置を読み出す処理
		//-------------------------------------------------------------------------------
		private static PokemonImageTableOffsets ReadPokemonImageTableOffsets()
		{
			PokemonImageTableOffsets pokemonImageTableOffsets = new PokemonImageTableOffsets();
			pokemonImageTableOffsets.FrontImageTableOffset = RomIniReader.ReadHexOrDecimal("FRONT_IMAGE_TABLE_OFFSET");
			pokemonImageTableOffsets.BackImageTableOffset = RomIniReader.ReadHexOrDecimal("BACK_IMAGE_TABLE_OFFSET");
			pokemonImageTableOffsets.NormalPaletteTableOffset = RomIniReader.ReadHexOrDecimal("NORMAL_PALETTE_TABLE_OFFSET");
			pokemonImageTableOffsets.ShinyPaletteTableOffset = RomIniReader.ReadHexOrDecimal("SHINY_PALETTE_TABLE_OFFSET");
			return pokemonImageTableOffsets;
		}

		//-------------------------------------------------------------------------------
		// ポケモン名をROMから取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadPokemonName(byte[] romData, int pokemonId)
		{
			int num = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_OFFSET");
			int num2 = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_LENGTH");
			int num3 = num + pokemonId * num2;
			return ReadPokemonString(romData, num3, num2);
		}

		//-------------------------------------------------------------------------------
		// 任意オフセットのポケモン文字列を読み出す処理
		//-------------------------------------------------------------------------------
		private static string ReadPokemonString(byte[] romData, int offset, int length)
		{
			return TextConverter.BytesToPokemonString(romData, offset, length);
		}

		//-------------------------------------------------------------------------------
		// テーブル上のポインタ値をROMオフセットとして取得する処理
		//-------------------------------------------------------------------------------
		private static uint ReadPointerAddress(byte[] romData, int tableOffset, int entryIndex, int entryLength)
		{
			int offset = tableOffset + entryIndex * entryLength;
			return ReadRawPointer(romData, offset);
		}

		//-------------------------------------------------------------------------------
		// テーブル上のポインタ値を書き戻す処理
		//-------------------------------------------------------------------------------
		private static void WritePointerAddress(byte[] romData, int tableOffset, int entryIndex, int entryLength, uint rawAddress)
		{
			int offset = tableOffset + entryIndex * entryLength;
			WriteRawPointer(romData, offset, rawAddress);
		}

		//-------------------------------------------------------------------------------
		// 4byteポインタをROMオフセットとして読み出す処理
		//-------------------------------------------------------------------------------
		private static uint ReadRawPointer(byte[] romData, int offset)
		{
			uint num = BitConverter.ToUInt32(romData, offset);
			if (num == 0U)
			{
				return 0U;
			}
			return num - GbaRomPointerBase;
		}

		//-------------------------------------------------------------------------------
		// 4byteポインタへROMオフセットを書き込む処理
		//-------------------------------------------------------------------------------
		private static void WriteRawPointer(byte[] romData, int offset, uint rawAddress)
		{
			byte[] bytes = BitConverter.GetBytes(rawAddress + GbaRomPointerBase);
			Array.Copy(bytes, 0, romData, offset, 4);
		}

		//-------------------------------------------------------------------------------
		// 出力ROMの保存先を決定する処理
		//-------------------------------------------------------------------------------
		private static string GetRomOutputPath(Dictionary<string, string> options, string romPath)
		{
			if (options.TryGetValue("out-rom", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				string fullPath = Path.GetFullPath(value);
				string directoryName = Path.GetDirectoryName(fullPath);
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				return fullPath;
			}
			return Path.GetFullPath(romPath);
		}

		//-------------------------------------------------------------------------------
		// 変更履歴ファイルの保存先を決定する処理
		//-------------------------------------------------------------------------------
		private static string GetChangeLogOutputPath(Dictionary<string, string> options, string romOutputPath, string target)
		{
			if (options.TryGetValue("log-out", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				string fullPath = Path.GetFullPath(value);
				string directoryName = Path.GetDirectoryName(fullPath);
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				return fullPath;
			}
			string directoryName2 = Path.GetDirectoryName(romOutputPath) ?? string.Empty;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(romOutputPath);
			return Path.Combine(directoryName2, fileNameWithoutExtension + "." + target + ".log.txt");
		}

		//-------------------------------------------------------------------------------
		// 変更履歴テキストを組み立てる処理
		//-------------------------------------------------------------------------------
		private static string BuildChangeLogText(ImportBatchResult result)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("BochiBochiEditor Image Import Log");
			stringBuilder.AppendLine("Target: " + result.Target);
			stringBuilder.AppendLine("OrderMode: " + result.OrderMode);
			stringBuilder.AppendLine("InputRom: " + result.InputRomPath);
			stringBuilder.AppendLine("OutputRom: " + result.OutputRomPath);
			stringBuilder.AppendLine("SourceDirectory: " + result.SourceDirectory);
			stringBuilder.AppendLine("StartAddress: " + result.StartAddress);
			stringBuilder.AppendLine("Count: " + result.ImportCount.ToString(CultureInfo.InvariantCulture));
			stringBuilder.AppendLine();
			foreach (ImportEntryResult importEntryResult in result.Entries)
			{
				stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0}] RequestedID={1} InternalID={2} Name={3}", importEntryResult.Target, importEntryResult.Id, importEntryResult.InternalId, importEntryResult.Name));
				stringBuilder.AppendLine("File: " + importEntryResult.FileName);
				stringBuilder.AppendLine("Before: " + importEntryResult.Before);
				stringBuilder.AppendLine("After: " + importEntryResult.After);
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		//-------------------------------------------------------------------------------
		// 変更履歴1件分の結果を生成する処理
		//-------------------------------------------------------------------------------
		private static ImportEntryResult CreateEntryResult(string target, SourceImageEntry sourceImageEntry, int id, string name)
		{
			ImportEntryResult importEntryResult = new ImportEntryResult();
			importEntryResult.Target = target;
			importEntryResult.Id = id;
			importEntryResult.Name = name;
			importEntryResult.FileName = sourceImageEntry.FileName;
			importEntryResult.SourcePath = sourceImageEntry.FullPath;
			return importEntryResult;
		}

		//-------------------------------------------------------------------------------
		// ROMアドレス表記文字列を生成する処理
		//-------------------------------------------------------------------------------
		private static string FormatRomAddress(uint rawAddress)
		{
			return string.Format(CultureInfo.InvariantCulture, "raw=0x{0:X8}, gba=0x{1:X8}", rawAddress, rawAddress + GbaRomPointerBase);
		}

		//-------------------------------------------------------------------------------
		// ポケモン画像の並び順指定を取得する処理
		//-------------------------------------------------------------------------------
		private static string GetPokemonOrderMode(Dictionary<string, string> options)
		{
			if (options.ContainsKey("vanilla"))
			{
				return "vanilla";
			}
			if (options.ContainsKey("neworder"))
			{
				return "neworder";
			}
			if (options.TryGetValue("order", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				return value.ToLowerInvariant();
			}
			return "neworder";
		}

		//-------------------------------------------------------------------------------
		// 出力バリアント指定を取得する処理
		//-------------------------------------------------------------------------------
		private static string GetVariant(Dictionary<string, string> options, string target)
		{
			if (options.TryGetValue("variant", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				return value.ToLowerInvariant();
			}
			return target.Equals("pokemon-icon", StringComparison.OrdinalIgnoreCase) ? "frame1" : "front-normal";
		}

		//-------------------------------------------------------------------------------
		// 画像拡張子が対象か判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsSupportedImageExtension(string path)
		{
			string extension = Path.GetExtension(path);
			return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".bmp", StringComparison.OrdinalIgnoreCase);
		}

		//-------------------------------------------------------------------------------
		// 対象IDが有効範囲か検証する処理
		//-------------------------------------------------------------------------------
		private static void ValidateIndexRange(int index, int totalCount, string label)
		{
			if (index < 0 || index >= totalCount)
			{
				throw new ArgumentOutOfRangeException(label, string.Format(CultureInfo.InvariantCulture, "ID {0} は有効範囲外です。", index));
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン画像の指定IDを内部IDへ解決する処理
		//-------------------------------------------------------------------------------
		private static int ResolvePokemonInternalId(byte[] romData, int requestedId, string orderMode)
		{
			if (string.Equals(orderMode, "neworder", StringComparison.OrdinalIgnoreCase))
			{
				ValidateIndexRange(requestedId, RomIniReader.ReadHexOrDecimal("TOTAL_POKEMON_COUNT") - 1, "pokemon");
				return requestedId;
			}
			if (!string.Equals(orderMode, "vanilla", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("order には vanilla または neworder を指定してください。");
			}
			foreach (ImageTargetEntry imageTargetEntry in GetPokemonOrderEntries(romData, "vanilla"))
			{
				if (imageTargetEntry.RequestedId == requestedId)
				{
					return imageTargetEntry.InternalId;
				}
			}
			throw new ArgumentOutOfRangeException("pokemon", string.Format(CultureInfo.InvariantCulture, "vanilla順ID {0} に対応する内部IDが見つかりません。", requestedId));
		}

		//-------------------------------------------------------------------------------
		// targetに応じた画像対象一覧を取得する処理
		//-------------------------------------------------------------------------------
		private static List<ImageTargetEntry> GetImageTargetEntries(byte[] romData, string target, string orderMode)
		{
			switch (target.ToLowerInvariant())
			{
			case "pokemon-sprite":
			case "pokemon-icon":
				return GetPokemonOrderEntries(romData, orderMode);
			case "item-image":
				return Enumerable.Range(0, ItemData.TOTAL_ITEM_COUNT).Select((int id) => new ImageTargetEntry
				{
					RequestedId = id,
					InternalId = id,
					Name = ItemData.GetItemInfo(romData, checked((ushort)id)).Name
				}).ToList();
			case "trainer-image":
			{
				int num = RomIniReader.ReadHexOrDecimal("MAX_TRAINER_SPRITE_COUNT");
				return Enumerable.Range(0, num).Select((int id) => new ImageTargetEntry
				{
					RequestedId = id,
					InternalId = id,
					Name = BuildTrainerName(romData, id)
				}).ToList();
			}
			default:
				throw new ArgumentException("未対応のtargetです。: " + target);
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン系画像の対象一覧を並び順に応じて取得する処理
		//-------------------------------------------------------------------------------
		private static List<ImageTargetEntry> GetPokemonOrderEntries(byte[] romData, string orderMode)
		{
			int num = RomIniReader.ReadHexOrDecimal("TOTAL_POKEMON_COUNT") - 1;
			if (string.Equals(orderMode, "neworder", StringComparison.OrdinalIgnoreCase))
			{
				List<ImageTargetEntry> list = new List<ImageTargetEntry>();
				for (int i = 1; i <= num; i++)
				{
					list.Add(new ImageTargetEntry
					{
						RequestedId = i,
						InternalId = i,
						Name = ReadPokemonName(romData, i)
					});
				}
				return list;
			}
			if (!string.Equals(orderMode, "vanilla", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("order には vanilla または neworder を指定してください。");
			}
			int num2 = RomIniReader.ReadHexOrDecimal("POKEDEX_ORDER_TABLE_OFFSET");
			int num3 = RomIniReader.ReadHexOrDecimal("POKEDEX_ORDER_ENTRY_LENGTH");
			List<ImageTargetEntry> list2 = new List<ImageTargetEntry>();
			for (int j = 1; j <= num; j++)
			{
				int num4 = num2 + (j - 1) * num3;
				int num5 = BitConverter.ToUInt16(romData, num4);
				list2.Add(new ImageTargetEntry
				{
					RequestedId = num5,
					InternalId = j,
					Name = ReadPokemonName(romData, j)
				});
			}
			return list2.OrderBy((ImageTargetEntry entry) => entry.RequestedId).ThenBy((ImageTargetEntry entry) => entry.InternalId).ToList();
		}

		//-------------------------------------------------------------------------------
		// トレーナー画像名を生成する処理
		//-------------------------------------------------------------------------------
		private static string BuildTrainerName(byte[] romData, int trainerId)
		{
			TrainerSpriteRepresentative trainerSpriteRepresentative = FindRepresentativeTrainerForSprite(romData, trainerId);
			if (trainerSpriteRepresentative == null)
			{
				return "TrainerSprite " + trainerId.ToString("D3", CultureInfo.InvariantCulture);
			}
			if (string.IsNullOrWhiteSpace(trainerSpriteRepresentative.TrainerName))
			{
				return trainerSpriteRepresentative.TrainerClassName;
			}
			return trainerSpriteRepresentative.TrainerClassName + " " + trainerSpriteRepresentative.TrainerName;
		}

		//-------------------------------------------------------------------------------
		// 指定スプライトを使っている代表トレーナー情報を逆引きする処理
		//-------------------------------------------------------------------------------
		private static TrainerSpriteRepresentative FindRepresentativeTrainerForSprite(byte[] romData, int spriteId)
		{
			int num = RomIniReader.ReadHexOrDecimal("TRAINER_DATA_OFFSET");
			int num2 = RomIniReader.ReadHexOrDecimal("TRAINER_DATA_LENGTH");
			int num3 = RomIniReader.ReadHexOrDecimal("TRAINER_ENTRY_COUNT");
			int num4 = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_TABLE_OFFSET");
			int num5 = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_LENGTH");
			int num6 = RomIniReader.ReadHexOrDecimal("TRAINER_NAME_LENGTH");
			for (int i = 1; i < num3; i++)
			{
				int num7 = num + i * num2;
				if (romData[num7 + 3] == spriteId)
				{
					byte b = romData[num7 + 1];
					int num8 = num4 + b * num5;
					string pokemonString = ReadPokemonString(romData, num8, num5);
					string pokemonString2 = ReadPokemonString(romData, num7 + 4, num6);
					return new TrainerSpriteRepresentative
					{
						TrainerId = i,
						TrainerClassName = string.IsNullOrWhiteSpace(pokemonString) ? ("Class " + b.ToString(CultureInfo.InvariantCulture)) : pokemonString,
						TrainerName = pokemonString2
					};
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 必須オプション文字列を取得する処理
		//-------------------------------------------------------------------------------
		private static string RequireOption(Dictionary<string, string> options, string key)
		{
			if (!options.TryGetValue(key, out string value) || string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException("--" + key + " を指定してください。");
			}
			return value;
		}

		//-------------------------------------------------------------------------------
		// ROMアドレスまたはポインタ表記をROMオフセットへ変換する処理
		//-------------------------------------------------------------------------------
		private static uint RequireRomAddressOption(Dictionary<string, string> options, string key)
		{
			string requiredOption = RequireOption(options, key);
			uint num = ParseUnsignedInteger(requiredOption);
			if (num >= GbaRomPointerBase)
			{
				num -= GbaRomPointerBase;
			}
			return num;
		}

		//-------------------------------------------------------------------------------
		// 整数文字列を32bit整数へ変換する処理
		//-------------------------------------------------------------------------------
		private static int ParseInteger(string text)
		{
			if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return Convert.ToInt32(text.Substring(2), 16);
			}
			return Convert.ToInt32(text, CultureInfo.InvariantCulture);
		}

		//-------------------------------------------------------------------------------
		// 整数文字列を符号なし32bit整数へ変換する処理
		//-------------------------------------------------------------------------------
		private static uint ParseUnsignedInteger(string text)
		{
			if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return Convert.ToUInt32(text.Substring(2), 16);
			}
			return Convert.ToUInt32(text, CultureInfo.InvariantCulture);
		}

		//-------------------------------------------------------------------------------
		// ポケモン画像4種を一括で読み出す処理
		//-------------------------------------------------------------------------------
		private static PokemonSpriteResource LoadPokemonSpriteResource(byte[] romData, int pokemonId)
		{
			PokemonImageTableOffsets pokemonImageTableOffsets = ReadPokemonImageTableOffsets();
			uint pointerAddress = ReadPointerAddress(romData, pokemonImageTableOffsets.FrontImageTableOffset, pokemonId, PokemonTableEntryLength);
			uint pointerAddress2 = ReadPointerAddress(romData, pokemonImageTableOffsets.BackImageTableOffset, pokemonId, PokemonTableEntryLength);
			uint pointerAddress3 = ReadPointerAddress(romData, pokemonImageTableOffsets.NormalPaletteTableOffset, pokemonId, PokemonTableEntryLength);
			uint pointerAddress4 = ReadPointerAddress(romData, pokemonImageTableOffsets.ShinyPaletteTableOffset, pokemonId, PokemonTableEntryLength);
			return new PokemonSpriteResource
			{
				FrontImage = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, pointerAddress, false),
				BackImage = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, pointerAddress2, false),
				NormalPalette = ImageProcessor.LoadPalette(ImageProcessor.LoadCompressedImagePaletteFromROM(romData, pointerAddress3, true), true),
				ShinyPalette = ImageProcessor.LoadPalette(ImageProcessor.LoadCompressedImagePaletteFromROM(romData, pointerAddress4, true), true)
			};
		}

		//-------------------------------------------------------------------------------
		// ポケモンアイコン用のパレット色配列を取得する処理
		//-------------------------------------------------------------------------------
		private static Color[] LoadIconPaletteColors(byte[] romData, int paletteId)
		{
			int num = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_TABLE_OFFSET");
			uint rawPointer = ReadPointerAddress(romData, num, paletteId, PokemonTableEntryLength);
			byte[] array = new byte[32];
			Array.Copy(romData, (long)((ulong)rawPointer), array, 0L, 32L);
			return ImageProcessor.LoadPalette(array, false);
		}

		//-------------------------------------------------------------------------------
		// 対象とバリアントに応じたセルサイズを返す処理
		//-------------------------------------------------------------------------------
		private static Size GetCellSize(string target, string variant)
		{
			switch (target.ToLowerInvariant())
			{
			case "pokemon-sprite":
				return new Size(64, 64);
			case "item-image":
				return new Size(ItemData.ITEM_IMAGE_WIDTH, ItemData.ITEM_IMAGE_HEIGHT);
			case "pokemon-icon":
				return string.Equals(variant, "full", StringComparison.OrdinalIgnoreCase) ? new Size(32, 64) : new Size(32, 32);
			case "trainer-image":
				return new Size(64, 64);
			default:
				throw new ArgumentException("未対応のtargetです。: " + target);
			}
		}

		//-------------------------------------------------------------------------------
		// 指定targetの一覧シート用プレビュー画像を生成する処理
		//-------------------------------------------------------------------------------
		private static Bitmap LoadPreviewBitmap(byte[] romData, string target, int internalId, string variant)
		{
			switch (target.ToLowerInvariant())
			{
			case "pokemon-sprite":
			{
				PokemonSpriteResource pokemonSpriteResource = LoadPokemonSpriteResource(romData, internalId);
				return BuildBitmapFromDecodedSprite(pokemonSpriteResource, variant);
			}
			case "item-image":
			{
				ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(romData, checked((ushort)internalId));
				byte[] compressedImagePaletteFromROM = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, itemInfo.ImageAddress, false);
				Color[] palette = ImageProcessor.LoadPalette(ImageProcessor.LoadCompressedImagePaletteFromROM(romData, itemInfo.PaletteAddress, true), true);
				return BuildBitmapFromIndexedPixels(DecodeSprite(compressedImagePaletteFromROM, ItemData.ITEM_IMAGE_WIDTH, ItemData.ITEM_IMAGE_HEIGHT, 0), palette);
			}
			case "pokemon-icon":
			{
				int num = RomIniReader.ReadHexOrDecimal("ICON_IMAGE_TABLE_OFFSET");
				int num2 = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_ID_TABLE_OFFSET");
				uint rawPointer = ReadRawPointer(romData, num + internalId * 4);
				byte[] compressedImagePaletteFromROM2 = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, rawPointer, false);
				Color[] palette2 = LoadIconPaletteColors(romData, romData[num2 + internalId]);
				byte[,] array = DecodeSprite(compressedImagePaletteFromROM2, 32, 64, 0);
				if (string.Equals(variant, "frame2", StringComparison.OrdinalIgnoreCase))
				{
					return BuildBitmapFromIndexedPixels(CropIndexedPixels(array, 0, 32, 32, 32), palette2);
				}
				if (string.Equals(variant, "full", StringComparison.OrdinalIgnoreCase))
				{
					return BuildBitmapFromIndexedPixels(array, palette2);
				}
				return BuildBitmapFromIndexedPixels(CropIndexedPixels(array, 0, 0, 32, 32), palette2);
			}
			case "trainer-image":
			{
				int num3 = RomIniReader.ReadHexOrDecimal("TRAINER_SPRITE_TABLE_OFFSET");
				int num4 = RomIniReader.ReadHexOrDecimal("TRAINER_PALETTE_TABLE_OFFSET");
				byte[] compressedImagePaletteFromROM3 = ImageProcessor.LoadCompressedImagePaletteFromROM(romData, ReadPointerAddress(romData, num3, internalId, PokemonTableEntryLength), false);
				Color[] palette3 = ImageProcessor.LoadPalette(ImageProcessor.LoadCompressedImagePaletteFromROM(romData, ReadPointerAddress(romData, num4, internalId, PokemonTableEntryLength), true), true);
				return BuildBitmapFromIndexedPixels(DecodeSprite(compressedImagePaletteFromROM3, 64, 64, 0), palette3);
			}
			default:
				throw new ArgumentException("未対応のtargetです。: " + target);
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン画像のバリアントに応じたBitmapを生成する処理
		//-------------------------------------------------------------------------------
		private static Bitmap BuildBitmapFromDecodedSprite(PokemonSpriteResource resource, string variant)
		{
			switch (variant.ToLowerInvariant())
			{
			case "front-normal":
				return BuildBitmapFromIndexedPixels(DecodeSprite(resource.FrontImage, 64, 64, 0), resource.NormalPalette);
			case "front-shiny":
				return BuildBitmapFromIndexedPixels(DecodeSprite(resource.FrontImage, 64, 64, 0), resource.ShinyPalette);
			case "back-normal":
				return BuildBitmapFromIndexedPixels(DecodeSprite(resource.BackImage, 64, 64, 0), resource.NormalPalette);
			case "back-shiny":
				return BuildBitmapFromIndexedPixels(DecodeSprite(resource.BackImage, 64, 64, 0), resource.ShinyPalette);
			default:
				throw new ArgumentException("variant には front-normal / front-shiny / back-normal / back-shiny を指定してください。");
			}
		}

		//-------------------------------------------------------------------------------
		// インデックス画像から通常Bitmapを生成する処理
		//-------------------------------------------------------------------------------
		private static Bitmap BuildBitmapFromIndexedPixels(byte[,] indexedPixels, Color[] palette)
		{
			Bitmap bitmap = new Bitmap(indexedPixels.GetLength(1), indexedPixels.GetLength(0));
			for (int i = 0; i < indexedPixels.GetLength(0); i++)
			{
				for (int j = 0; j < indexedPixels.GetLength(1); j++)
				{
					int num = indexedPixels[i, j];
					bitmap.SetPixel(j, i, (num >= 0 && num < palette.Length) ? palette[num] : Color.Transparent);
				}
			}
			return bitmap;
		}

		//-------------------------------------------------------------------------------
		// インデックス画像の一部を切り出す処理
		//-------------------------------------------------------------------------------
		private static byte[,] CropIndexedPixels(byte[,] source, int startX, int startY, int width, int height)
		{
			byte[,] array = new byte[height, width];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					array[i, j] = source[startY + i, startX + j];
				}
			}
			return array;
		}

		private sealed class SourceImageEntry
		{
			public int Id { get; set; }

			public string FileName { get; set; }

			public string FullPath { get; set; }
		}

		private sealed class PokemonImageTableOffsets
		{
			public int FrontImageTableOffset { get; set; }

			public int BackImageTableOffset { get; set; }

			public int NormalPaletteTableOffset { get; set; }

			public int ShinyPaletteTableOffset { get; set; }
		}

		private sealed class PokemonSpriteImportData
		{
			public byte[] FrontImageData { get; set; }

			public byte[] BackImageData { get; set; }

			public byte[] NormalPaletteData { get; set; }

			public byte[] ShinyPaletteData { get; set; }
		}

		private sealed class PokemonSpriteResource
		{
			public byte[] FrontImage { get; set; }

			public byte[] BackImage { get; set; }

			public Color[] NormalPalette { get; set; }

			public Color[] ShinyPalette { get; set; }
		}

		private sealed class ImageTargetEntry
		{
			public int RequestedId { get; set; }

			public int InternalId { get; set; }

			public string Name { get; set; }
		}

		private sealed class TrainerSpriteRepresentative
		{
			public int TrainerId { get; set; }

			public string TrainerClassName { get; set; }

			public string TrainerName { get; set; }
		}

		public sealed class ImportBatchResult
		{
			public string Target { get; set; }

			public string SourceDirectory { get; set; }

			public string InputRomPath { get; set; }

			public string OutputRomPath { get; set; }

			public string ChangeLogPath { get; set; }

			public string StartAddress { get; set; }

			public string OrderMode { get; set; }

			public string LastAllocatedAddress { get; set; }

			public int ImportCount { get; set; }

			public List<ImportEntryResult> Entries { get; set; }
		}

		public sealed class ImportEntryResult
		{
			public string Target { get; set; }

			public int Id { get; set; }

			public int InternalId { get; set; }

			public string Name { get; set; }

			public string FileName { get; set; }

			public string SourcePath { get; set; }

			public string Before { get; set; }

			public string After { get; set; }
		}

		public sealed class ExportBatchResult
		{
			public string Target { get; set; }

			public string InputRomPath { get; set; }

			public string SourceDirectory { get; set; }

			public string OrderMode { get; set; }

			public int ExportCount { get; set; }

			public List<ExportEntryResult> Entries { get; set; }
		}

		public sealed class ExportEntryResult
		{
			public string Target { get; set; }

			public int RequestedId { get; set; }

			public int InternalId { get; set; }

			public string Name { get; set; }

			public string OutputPath { get; set; }
		}

		public sealed class SheetExportResult
		{
			public string Target { get; set; }

			public string OrderMode { get; set; }

			public string Variant { get; set; }

			public string OutputPath { get; set; }

			public int ImageCount { get; set; }

			public int Columns { get; set; }

			public int Rows { get; set; }
		}
	}
}
