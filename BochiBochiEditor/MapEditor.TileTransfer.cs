using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップタイルの移植（メインタブ「マップタイル」。画面の表示は MapEditor.MapTileTab.cs）
	// ・書き出し: 今のマップのタイルセット 1・2（タイル画像・パレット・ブロック・挙動）とマップの並びをフォルダへ出す
	// ・まとめて取り込み: 書き出したフォルダから、選んだタイルセットを空き領域に新しく作る（パレット・ブロック・挙動も入れる）
	// ・一部だけ取り込み: 書き出したブロック一覧の画像から範囲を選び、マップチップ取り込みのウィザードへ渡す
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// ブロック一覧の画像の横の個数（ブロックエディタ・ブロック一覧と同じ 8 個）
		internal const int TileTransferSheetColumns = 8;

		// 最後に書き出したフォルダ（取り込みの画面を開いたときに最初に読む）
		private string lastTileExportFolder;

		//-------------------------------------------------------------------------------
		// 「マップタイルをエクスポート」: 書き出す先を選び、その中に map_… のフォルダを作って書き出す処理
		//-------------------------------------------------------------------------------
		private void btnExportMapTiles_Click(object sender, EventArgs e)
		{
			if (this.romData == null || this.tempHeader == null)
			{
				MessageBox.Show(this, Localizer.T("先にマップを選んでください。書き出すのは、そのマップのタイルセットと並びです。"), Localizer.T("マップタイルの書き出し"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.Description = string.Format(Localizer.T("書き出す先を選んでください。この中に「{0}」フォルダを作ります。"), this.GetTileExportFolderName());
				dialog.UseDescriptionForTitle = true;
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				string folder = Path.Combine(dialog.SelectedPath, this.GetTileExportFolderName());
				if (Directory.Exists(folder) && MessageBox.Show(this, string.Format(Localizer.T("「{0}」はすでにあります。中のファイルを上書きしますか？"), folder), Localizer.T("マップタイルの書き出し"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				{
					return;
				}
				string error;
				try
				{
					error = this.ExportMapTiles(folder);
				}
				catch (Exception ex)
				{
					this.WriteErrorLog("マップタイルの書き出し", ex);
					error = Localizer.T("書き出せませんでした: ") + ex.Message;
				}
				if (error != null)
				{
					MessageBox.Show(this, error, Localizer.T("マップタイルの書き出し"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				this.lastTileExportFolder = folder;
				if (MessageBox.Show(this, string.Format(Localizer.T("書き出しました。\n{0}\n\n別の ROM を開いてから「マップタイルをインポート」で取り込めます。フォルダを開きますか？"), folder), Localizer.T("マップタイルの書き出し"), MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
				{
					System.Diagnostics.Process.Start("explorer.exe", "\"" + folder + "\"");
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 「マップタイルをインポート」: 取り込みの画面を開く処理
		//-------------------------------------------------------------------------------
		private void btnImportMapTiles_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.BlockIfExpandedFormat()) return;
			if (this.romData == null)
			{
				return;
			}
			using (MapTileImportForm form = new MapTileImportForm(this, this.lastTileExportFolder))
			{
				AppIconHelper.Apply(form);
				UiTheme.Apply(form);
				form.ShowDialog(this);
			}
		}

		//-------------------------------------------------------------------------------
		// 書き出すフォルダの名前（例: map_3_0_マサラタウン）を返す処理（ファイル名に使えない文字は _ にする）
		//-------------------------------------------------------------------------------
		internal string GetTileExportFolderName()
		{
			string name = this.tempHeader != null ? this.tempHeader.GetMapName(this) : "";
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}
			name = name.Replace(' ', '_').Replace('　', '_').Trim('_');
			return this.tempHeader == null ? "map" : string.Format("map_{0}_{1}{2}", this.tempHeader.Bank, this.tempHeader.Number, name.Length > 0 ? "_" + name : "");
		}

		//-------------------------------------------------------------------------------
		// 今のマップのマップタイルを folder へ書き出す処理（成功なら null、失敗なら理由）
		// 書くもの: map_info.txt、tileset1.png / tileset2.png（16 色のタイル画像）、tilesetN_paletteNN.png（パレット 1 本ずつ）、
		//           tilesetN_blocks.png（ブロック一覧。一部だけ取り込みに使う）、map_preview.png（マップ全体）
		// 書き出すのは確定済みの内容（ROM の中身）なので、未確定の変更があるときは断る
		//-------------------------------------------------------------------------------
		internal string ExportMapTiles(string folder)
		{
			if (this.romData == null || this.tempHeader == null || this.tempFooter == null || this.tempTileset1 == null || this.tempTileset2 == null)
			{
				return Localizer.T("先にマップを選んでください。書き出すのは、そのマップのタイルセットと並びです。");
			}
			if (this.hasUnsavedChanges)
			{
				return Localizer.T("このマップには確定していない変更があります。書き出すのは ROM の中身（確定済みの内容）なので、先に「編集中のMAPを確定」を押してください。");
			}
			GameProfile profile = GameProfile.Current;
			MapFooter footer = this.ReadMapFooter((int)this.tempHeader.FooterAddress);
			MapTilePackage package = new MapTilePackage
			{
				GameCode = profile.Code,
				PrimaryTileCount = profile.PrimaryTileCount,
				PrimaryBlockCount = profile.PrimaryBlockCount,
				PrimaryPaletteCount = profile.PrimaryPaletteCount,
				BlockBytes = MapEditor.BLOCK_DATA_SIZE,
				BehaviorBytes = profile.BehaviorBytes,
				Bank = this.tempHeader.Bank,
				Number = this.tempHeader.Number,
				MapName = this.tempHeader.GetMapName(this),
				MapWidth = footer.MapWidth,
				MapHeight = footer.MapHeight,
				BorderWidth = footer.BorderWidth,
				BorderHeight = footer.BorderHeight
			};
			package.Layout = this.ReadUInt16Grid(footer.MapDataAddress, footer.MapWidth * footer.MapHeight);
			package.Border = this.ReadUInt16Grid(footer.BorderDataAddress, footer.BorderWidth * footer.BorderHeight);
			TilesetHeader[] headers = new TilesetHeader[] { this.ReadTilesetHeader((int)footer.Tileset1Address), this.ReadTilesetHeader((int)footer.Tileset2Address) };
			uint[] headerAddresses = new uint[] { footer.Tileset1Address, footer.Tileset2Address };
			for (int t = 0; t < 2; t++)
			{
				string error;
				package.Tilesets[t] = this.ReadTilesetPart(headers[t], headerAddresses[t], t == 1, out error);
				if (package.Tilesets[t] == null)
				{
					return error;
				}
			}

			Directory.CreateDirectory(folder);
			MapThumbnailRenderer.Settings settings = MapThumbnailRenderer.Settings.FromCurrentGame();
			for (int t = 0; t < 2; t++)
			{
				MapTilePackage.TilesetPart part = package.Tilesets[t];
				string prefix = t == 0 ? "tileset1" : "tileset2";
				// タイル画像は、そのタイルセットが受け持つ最初のパレットの色で出す（取り込みで使うのは色の番号だけ）
				int shownPalette = t == 0 ? 0 : profile.PrimaryPaletteCount;
				ImageProcessor.ExportSpriteTo4bppPng(Path.Combine(folder, part.ImageFile), part.TileImage, MapTilePackage.ToColors(part.Palettes, shownPalette), 128, part.TileCount / 16 * 8);
				int first = t == 0 ? 0 : profile.PrimaryPaletteCount;
				int end = t == 0 ? profile.PrimaryPaletteCount : 13;
				for (int p = first; p < end; p++)
				{
					SavePaletteSwatch(Path.Combine(folder, string.Format("{0}_palette{1:D2}.png", prefix, p)), MapTilePackage.ToColors(part.Palettes, p));
				}
				int firstBlock = t == 0 ? 0 : profile.PrimaryBlockCount;
				using (Bitmap sheet = MapThumbnailRenderer.RenderBlockSheet(this.romData, footer.Tileset1Address, footer.Tileset2Address, settings, firstBlock, part.BlockCount, TileTransferSheetColumns))
				{
					sheet?.Save(Path.Combine(folder, prefix + "_blocks.png"), ImageFormat.Png);
				}
				// タイルアニメのコマの絵（コマを縦に並べた 16 色 PNG。色はそのタイルを使うブロックのパレット）
				for (int a = 0; a < part.Animations.Count; a++)
				{
					MapTilePackage.AnimationPart anim = part.Animations[a];
					byte[] all = new byte[anim.Frames.Count * anim.TileCount * 32];
					for (int f = 0; f < anim.Frames.Count; f++)
					{
						Array.Copy(anim.Frames[f], 0, all, f * anim.TileCount * 32, anim.TileCount * 32);
					}
					int palette = this.FindPaletteForTile(package, anim.DestTile, shownPalette);
					ImageProcessor.ExportSpriteTo4bppPng(Path.Combine(folder, anim.ImageFile), all, MapTilePackage.ToColors(package.Tilesets[palette < profile.PrimaryPaletteCount ? 0 : 1].Palettes, palette), anim.TileCount * 8, anim.Frames.Count * 8);
				}
			}
			using (Bitmap preview = MapThumbnailRenderer.RenderFullMap(this.romData, this.tempHeader.FooterAddress, settings))
			{
				preview?.Save(Path.Combine(folder, "map_preview.png"), ImageFormat.Png);
			}
			package.SaveInfo(folder);
			return null;
		}

		//-------------------------------------------------------------------------------
		// 書き出し用に、タイルセット 1 つ分（タイル画像・パレット 16 本・ブロック・挙動）を ROM から読む処理（読めなければ null と理由）
		//-------------------------------------------------------------------------------
		private MapTilePackage.TilesetPart ReadTilesetPart(TilesetHeader header, uint headerAddress, bool secondary, out string error)
		{
			error = null;
			string label = secondary ? Localizer.T("タイルセット2") : Localizer.T("タイルセット1");
			if (header == null || header.ImageAddress == 0 || header.PaletteAddress == 0 || header.BlockImageAddress == 0 || !this.IsRomRange(header.PaletteAddress, 512))
			{
				error = string.Format(Localizer.T("{0} の見出しが読めないため、書き出せません。"), label);
				return null;
			}
			GameProfile profile = GameProfile.Current;
			int capacity = secondary ? 1024 - profile.PrimaryTileCount : profile.PrimaryTileCount;
			byte[] raw = this.LoadTilesetRawImage(header);
			int tiles = raw.Length / 32;
			if (header.ImageCompressType != 1)
			{
				// 圧縮していない画像は長さが分からないので、次のデータの手前までとする
				uint next = this.FindNextTilesetDataStart(header, header.ImageAddress);
				tiles = (int)Math.Min((long)tiles, ((long)next - header.ImageAddress) / 32);
			}
			tiles = Math.Max(16, Math.Min(tiles, capacity));
			// PNG の 1 行（横 16 枚）にそろえる。足りない分は色 0 のタイル
			int rows = (tiles + 15) / 16;
			byte[] image = new byte[rows * 16 * 32];
			Array.Copy(raw, 0, image, 0, Math.Min(raw.Length, Math.Min(tiles * 32, image.Length)));
			int blockCount = secondary ? this.GetSecondaryBlockCount(this.GetTilesetIndexFromHeaderAddress(headerAddress), header) : this.GetPrimaryBlockCount(header);
			int blockBytes = MapEditor.BLOCK_DATA_SIZE;
			int behaviorBytes = profile.BehaviorBytes;
			if (blockCount <= 0 || !this.IsRomRange(header.BlockImageAddress, blockCount * blockBytes))
			{
				error = string.Format(Localizer.T("{0} のブロック表が読めないため、書き出せません。"), label);
				return null;
			}
			MapTilePackage.TilesetPart part = new MapTilePackage.TilesetPart
			{
				Index = this.GetTilesetIndexFromHeaderAddress(headerAddress),
				Compressed = header.ImageCompressType == 1,
				TileCount = rows * 16,
				TileImage = image,
				ImageFile = secondary ? "tileset2.png" : "tileset1.png",
				BlockCount = blockCount,
				Blocks = new byte[blockCount * blockBytes],
				Behaviors = new byte[blockCount * behaviorBytes]
			};
			Array.Copy(this.romData, header.PaletteAddress, part.Palettes, 0, 512);
			Array.Copy(this.romData, header.BlockImageAddress, part.Blocks, 0, part.Blocks.Length);
			if (header.BlockBehaviorAddress != 0 && this.IsRomRange(header.BlockBehaviorAddress, part.Behaviors.Length))
			{
				Array.Copy(this.romData, header.BlockBehaviorAddress, part.Behaviors, 0, part.Behaviors.Length);
			}
			// タイルアニメ（見出しのアニメ処理から読めたもの。コマの絵も写す）
			part.AnimationCallback = header.AnimationAddress;
			int number = 0;
			foreach (TileAnimation anim in TileAnimReader.Read(this.romData, header.AnimationAddress))
			{
				if (anim.TileCount <= 0 || anim.Frames.Length == 0 || anim.Frames.Any(f => !this.IsRomRange(f, anim.TileCount * 32)))
				{
					continue;
				}
				MapTilePackage.AnimationPart item = new MapTilePackage.AnimationPart
				{
					DestTile = anim.DestTile,
					TileCount = anim.TileCount,
					Divisor = anim.Divisor,
					Delay = anim.Delay,
					ImageFile = string.Format("{0}_anim{1:D2}.png", secondary ? "tileset2" : "tileset1", number++),
				};
				foreach (uint frame in anim.Frames)
				{
					byte[] bytes = new byte[anim.TileCount * 32];
					Array.Copy(this.romData, frame, bytes, 0, bytes.Length);
					item.Frames.Add(bytes);
				}
				part.Animations.Add(item);
			}
			return part;
		}

		//-------------------------------------------------------------------------------
		// そのタイル（通し番号）を使っているブロックのパレット番号を探す処理（無ければ fallback。アニメの絵の色に使う）
		//-------------------------------------------------------------------------------
		private int FindPaletteForTile(MapTilePackage package, int tile, int fallback)
		{
			foreach (MapTilePackage.TilesetPart part in package.Tilesets)
			{
				if (part == null)
				{
					continue;
				}
				for (int i = 0; i + 1 < part.Blocks.Length; i += 2)
				{
					ushort value = BitConverter.ToUInt16(part.Blocks, i);
					if ((value & 0x3FF) == tile)
					{
						return value >> 12;
					}
				}
			}
			return fallback;
		}

		//-------------------------------------------------------------------------------
		// 作ったタイルセットに、書き出し元のタイルアニメの処理を引き継ぐ処理（引き継げたらアニメの数、だめなら -1。reason に理由）
		// 引き継ぐのは、取り込み先の ROM の同じ場所に同じ処理（同じ差し替え先・枚数・速さ・コマの絵）があるときだけ。
		// 形の違うゲームから変換したデータ（タイルの番号が変わっている）には引き継がない
		//-------------------------------------------------------------------------------
		private int InheritTileAnimation(MapTilePackage.TilesetPart part, uint createdHeaderAddress, bool converted, out string reason)
		{
			reason = null;
			if (part.AnimationCallback == 0U || part.Animations.Count == 0)
			{
				return 0;
			}
			if (converted)
			{
				reason = Localizer.T("形の違うゲームから変換したため、タイルアニメの処理は引き継げません（取り込んだ後にアニメの処理を付け直してください）。");
				return -1;
			}
			List<TileAnimation> here = TileAnimReader.Read(this.romData, part.AnimationCallback);
			if (here.Count != part.Animations.Count)
			{
				reason = string.Format(Localizer.T("取り込み先の ROM の同じ場所（0x{0:X6}）に同じアニメ処理が無いため、タイルアニメは引き継げません。"), part.AnimationCallback & ~1U);
				return -1;
			}
			for (int i = 0; i < here.Count; i++)
			{
				TileAnimation a = here[i];
				MapTilePackage.AnimationPart b = part.Animations[i];
				bool same = a.DestTile == b.DestTile && a.TileCount == b.TileCount && a.Divisor == b.Divisor && a.Delay == b.Delay && a.Frames.Length == b.Frames.Count;
				for (int f = 0; same && f < a.Frames.Length; f++)
				{
					same = this.IsRomRange(a.Frames[f], b.TileCount * 32) && new ArraySegment<byte>(this.romData, (int)a.Frames[f], b.TileCount * 32).SequenceEqual(b.Frames[f]);
				}
				if (!same)
				{
					reason = string.Format(Localizer.T("取り込み先の ROM のアニメ処理（0x{0:X6}）の中身が書き出し元と違うため、タイルアニメは引き継げません。"), part.AnimationCallback & ~1U);
					return -1;
				}
			}
			this.WritePointerToRom((int)createdHeaderAddress + GameProfile.Current.TilesetCallbackOffset, part.AnimationCallback);
			return here.Count;
		}

		//-------------------------------------------------------------------------------
		// タイルセット見出しの位置から、タイルセット番号を返す処理（表の区切りに合わない位置なら -1）
		//-------------------------------------------------------------------------------
		private int GetTilesetIndexFromHeaderAddress(uint headerAddress)
		{
			long distance = (long)headerAddress - MapEditor.TILESET_INDEX_START_OFFSET;
			if (distance < 0 || distance % MapEditor.TILESET_HEADER_SIZE != 0)
			{
				return -1;
			}
			return (int)(distance / MapEditor.TILESET_HEADER_SIZE);
		}

		//-------------------------------------------------------------------------------
		// ROM の指定位置から、2 バイト値を count 個読む処理（読めなければ空）
		//-------------------------------------------------------------------------------
		private ushort[] ReadUInt16Grid(uint address, int count)
		{
			if (count <= 0 || !this.IsRomRange(address, count * 2))
			{
				return new ushort[0];
			}
			ushort[] values = new ushort[count];
			for (int i = 0; i < count; i++)
			{
				values[i] = BitConverter.ToUInt16(this.romData, (int)address + i * 2);
			}
			return values;
		}

		//-------------------------------------------------------------------------------
		// パレット 1 本を、「パレットを変更」でそのまま取り込める 16 色 PNG（色見本 16 個を横に並べた 128×8）で保存する処理
		//-------------------------------------------------------------------------------
		private static void SavePaletteSwatch(string path, Color[] colors)
		{
			byte[] image = new byte[16 * 32];
			for (int c = 0; c < 16; c++)
			{
				for (int b = 0; b < 32; b++)
				{
					image[c * 32 + b] = (byte)(c | (c << 4));
				}
			}
			ImageProcessor.ExportSpriteTo4bppPng(path, image, colors, 128, 8);
		}

		//-------------------------------------------------------------------------------
		// 書き出したデータから、選んだタイルセットを空き領域に新しく作る処理（成功なら null、失敗なら理由）
		// タイル画像・パレット 16 本・ブロック・挙動をすべて入れる。作ったタイルセットの番号は createdIndexes に入れる（作らなかった方は -1）
		// 書き込み先は、空き領域の候補から選んでもらう（「新規」タブと同じ一覧）
		//-------------------------------------------------------------------------------
		internal string ImportMapTilePackage(MapTilePackage package, bool importPrimary, bool importSecondary, out int[] createdIndexes)
		{
			createdIndexes = new int[] { -1, -1 };
			if (this.IsRomReadOnly)
			{
				return Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。");
			}
			if (this.romData == null)
			{
				return Localizer.T("ROM が読み込まれていません。");
			}
			string notes;
			MapTilePackage original = package;
			string compatible = this.ConvertTilePackageForCurrentGame(ref package, out notes);
			if (compatible != null)
			{
				return compatible;
			}
			bool converted = !ReferenceEquals(original, package);
			this.LastTileConversionNotes = notes;
			bool[] wanted = new bool[] { importPrimary, importSecondary };
			if (!wanted[0] && !wanted[1])
			{
				return Localizer.T("取り込むタイルセットを選んでください。");
			}
			// 先に全部を確かめてから書く（途中で断ると、片方だけ作られてしまうため）
			for (int t = 0; t < 2; t++)
			{
				if (!wanted[t])
				{
					continue;
				}
				MapTilePackage.TilesetPart part = package.Tilesets[t];
				if (part == null)
				{
					return string.Format(Localizer.T("書き出したデータに{0}がありません。"), t == 0 ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"));
				}
				int limit = GetNewTilesetBlockLimit(t == 1);
				if (part.BlockCount < 1 || part.BlockCount > limit)
				{
					return string.Format(Localizer.T("{0}のブロック数 {1} が、この ROM で作れる数（1〜{2}）を超えています。"), t == 0 ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"), part.BlockCount, limit);
				}
				int tileCapacity = t == 1 ? 1024 - package.PrimaryTileCount : package.PrimaryTileCount;
				if (part.TileCount > tileCapacity)
				{
					return string.Format(Localizer.T("{0}のタイル画像（{1} 枚）が、置ける枚数（{2} 枚）を超えています。"), t == 0 ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"), part.TileCount, tileCapacity);
				}
			}
			for (int t = 0; t < 2; t++)
			{
				if (!wanted[t])
				{
					continue;
				}
				MapTilePackage.TilesetPart part = package.Tilesets[t];
				bool secondary = t == 1;
				byte[] imageBytes = part.Compressed ? ImageProcessor.LZ77Comp(part.TileImage, false) : part.TileImage;
				NewDataGenerator.TilesetGenerator generator = this.CreateTilesetGenerator(imageBytes, secondary, part.Compressed, part.BlockCount);
				int length = this.GetNewTilesetMaxLength(generator);
				uint picked;
				string purpose = secondary ? Localizer.T("移植するタイルセット2") : Localizer.T("移植するタイルセット1");
				if (!this.PickFreeSpaceAddress(purpose, length, this, out picked))
				{
					return createdIndexes[0] >= 0
						? string.Format(Localizer.T("タイルセット1 は番号 {0} として作りましたが、タイルセット2 は作っていません（書き込み先を選ばなかったため）。"), createdIndexes[0])
						: Localizer.T("書き込み先を選ばなかったため、何も書き込んでいません。");
				}
				uint address;
				bool occupied;
				string error = this.ResolveNewDataAddress(string.Format("{0:X8}", 0x08000000U + picked), false, length, out address, out occupied);
				if (error == null)
				{
					error = this.WriteNewTileset(generator, address);
				}
				if (error != null)
				{
					return error;
				}
				// 作ったタイルセットの見出しから、パレット・ブロック表・挙動表の場所を読み、中身を入れる
				TilesetHeader created = this.ReadTilesetHeader((int)generator.HeaderAddress);
				Array.Copy(part.Palettes, 0, this.romData, created.PaletteAddress, 512);
				Array.Copy(part.Blocks, 0, this.romData, created.BlockImageAddress, part.Blocks.Length);
				if (created.BlockBehaviorAddress != 0)
				{
					Array.Copy(part.Behaviors, 0, this.romData, created.BlockBehaviorAddress, part.Behaviors.Length);
				}
				// タイルアニメの処理を引き継ぐ（同じゲームで、取り込み先に同じ処理があるときだけ）
				string animReason;
				int inherited = this.InheritTileAnimation(part, generator.HeaderAddress, converted, out animReason);
				string animNote = inherited > 0
					? string.Format(Localizer.T("{0}: タイルアニメの処理（{1} 種類）を引き継ぎました。"), secondary ? Localizer.T("タイルセット2") : Localizer.T("タイルセット1"), inherited)
					: (animReason != null ? (secondary ? Localizer.T("タイルセット2") : Localizer.T("タイルセット1")) + ": " + animReason : null);
				if (animNote != null)
				{
					this.LastTileConversionNotes = string.IsNullOrEmpty(this.LastTileConversionNotes) ? animNote : this.LastTileConversionNotes + Environment.NewLine + animNote;
				}
				createdIndexes[t] = generator.OutTilesetIndex;
				this.AfterNewDataWritten();
			}
			return null;
		}

		// 直前の取り込みで、形の違うゲームから変換したときの注意（無ければ空）
		internal string LastTileConversionNotes = string.Empty;

		//-------------------------------------------------------------------------------
		// 書き出しデータが今のゲームと形が違えば、今のゲーム向けに変換する処理（取り込み・地形データの作成の前に呼ぶ）
		// 同じ形ならそのまま。変換できなければ理由を返す。notes は変換の注意（改行区切り。変換しなければ空）
		//-------------------------------------------------------------------------------
		internal string ConvertTilePackageForCurrentGame(ref MapTilePackage package, out string notes)
		{
			notes = string.Empty;
			if (package.CheckCompatibleWithCurrentGame() == null)
			{
				return null;
			}
			MapTilePackage converted;
			List<string> list;
			string error = MapTileConverter.Convert(package, GetNewTilesetBlockLimit(false), GetNewTilesetBlockLimit(true), out converted, out list);
			if (error != null)
			{
				return error;
			}
			package = converted;
			notes = string.Join(Environment.NewLine, list);
			return null;
		}

		//-------------------------------------------------------------------------------
		// 書き出したマップの並び・ボーダーから、地形データ（マップフッター）を空き領域に新しく作る処理（成功なら null、失敗なら理由）
		// 大きさ・ボーダーの大きさは書き出しのまま。タイルセットは tileset1Index・tileset2Index（「データ作成」タブ②の欄の番号）を使う
		// ブロック番号はそのまま書くので、タイルセットは書き出し元と同じもの（またはまとめて取り込みで作ったもの）を指定する前提
		//-------------------------------------------------------------------------------
		internal string CreateFooterFromPackage(MapTilePackage package, int tileset1Index, int tileset2Index, out uint footerAddress)
		{
			footerAddress = 0;
			if (this.IsRomReadOnly)
			{
				return Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。");
			}
			if (this.romData == null)
			{
				return Localizer.T("ROM が読み込まれていません。");
			}
			string notes;
			string compatible = this.ConvertTilePackageForCurrentGame(ref package, out notes);
			if (compatible != null)
			{
				return compatible;
			}
			int width = package.MapWidth;
			int height = package.MapHeight;
			if (width < 1 || height < 1 || width > 255 || height > 255 || package.Layout.Length != width * height)
			{
				return Localizer.T("書き出したデータにマップの並びが入っていないか、大きさが読めません。");
			}
			bool hasBorderSize = GameProfile.Current.HasBorderSize;
			int borderWidth = hasBorderSize ? package.BorderWidth : 2;
			int borderHeight = hasBorderSize ? package.BorderHeight : 2;
			if (package.BorderWidth < 1 || package.BorderHeight < 1 || package.Border.Length != package.BorderWidth * package.BorderHeight)
			{
				return Localizer.T("書き出したデータのボーダーが読めません。");
			}
			NewDataGenerator.MapFooterGenerator generator = this.CreateMapFooterGenerator((byte)width, (byte)height, (byte)borderWidth, (byte)borderHeight, tileset1Index, tileset2Index);
			string error = this.CheckNewMapFooter(generator);
			if (error != null)
			{
				return error;
			}
			uint picked;
			if (!this.PickFreeSpaceAddress(Localizer.T("書き出したマップから作る地形データ"), generator.CalculateLength(), this, out picked))
			{
				return Localizer.T("書き込み先を選ばなかったため、何も書き込んでいません。");
			}
			uint address;
			bool occupied;
			error = this.ResolveNewDataAddress(string.Format("{0:X8}", 0x08000000U + picked), false, generator.CalculateLength(), out address, out occupied);
			if (error == null)
			{
				error = this.WriteNewMapFooter(generator, address);
			}
			if (error != null)
			{
				return error;
			}
			// 作った地形データの見出しから、並びとボーダーの場所を読んで中身を入れる
			MapFooter created = this.ReadMapFooter((int)generator.HeaderAddress);
			for (int i = 0; i < width * height; i++)
			{
				Array.Copy(BitConverter.GetBytes(package.Layout[i]), 0, this.romData, created.MapDataAddress + i * 2, 2);
			}
			// ボーダーは、大きさが同じならそのまま、違う（2×2 固定のゲーム）なら左上から写す
			for (int y = 0; y < borderHeight; y++)
			{
				for (int x = 0; x < borderWidth; x++)
				{
					int source = y * package.BorderWidth + x;
					ushort value = x < package.BorderWidth && y < package.BorderHeight && source < package.Border.Length ? package.Border[source] : (ushort)0;
					Array.Copy(BitConverter.GetBytes(value), 0, this.romData, created.BorderDataAddress + (y * borderWidth + x) * 2, 2);
				}
			}
			this.AfterNewDataWritten();
			footerAddress = generator.HeaderAddress;
			return null;
		}

		//-------------------------------------------------------------------------------
		// 「データ作成」タブ②「書き出したマップの並びから作る…」: 書き出したフォルダを選び、並びを入れた地形データを作る処理
		//-------------------------------------------------------------------------------
		private void btnGuideFooterFromExport_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.BlockIfExpandedFormat()) return;
			if (this.romData == null)
			{
				return;
			}
			string folder;
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.Description = Localizer.T("「マップタイルをエクスポート」で作ったフォルダ（map_info.txt が入っているフォルダ）を選んでください。");
				dialog.UseDescriptionForTitle = true;
				if (!string.IsNullOrEmpty(this.lastTileExportFolder))
				{
					dialog.SelectedPath = this.lastTileExportFolder;
				}
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				folder = dialog.SelectedPath;
			}
			MapTilePackage package;
			try
			{
				package = MapTilePackage.Load(folder);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, Localizer.T("読み込めませんでした: ") + ex.Message, Localizer.T("② マップの形（地形データ）を作る"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			int tileset1 = Convert.ToInt32(this.nudNewMapFooterTileset1Index.Value);
			int tileset2 = Convert.ToInt32(this.nudNewMapFooterTileset2Index.Value);
			string question = string.Format(Localizer.T("書き出したマップ ({0}, {1}) {2}（{3}×{4}）の並びとボーダーを入れた地形データを作ります。\nタイルセットは②の欄の番号（タイルセット1: {5}、タイルセット2: {6}）を使います。ブロック番号はそのまま使うので、書き出し元と同じタイルセット（または「マップタイル」タブでまとめて取り込んだもの）を指定してください。\n作りますか？"),
				package.Bank, package.Number, package.MapName, package.MapWidth, package.MapHeight, tileset1, tileset2);
			if (MessageBox.Show(this, question, Localizer.T("② マップの形（地形データ）を作る"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			uint footerAddress;
			string error = this.CreateFooterFromPackage(package, tileset1, tileset2, out footerAddress);
			if (error != null)
			{
				MessageBox.Show(this, error, Localizer.T("② マップの形（地形データ）を作る"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			this.lastTileExportFolder = folder;
			this.OnDataGenerated(footerAddress);
			this.RememberCreatedGuideData(footerAddress, 0, 0);
		}

		//-------------------------------------------------------------------------------
		// まとめて取り込みで作ったタイルセットの番号を、「データ作成」タブ②のタイルセット番号の欄へ入れておく処理（作らなかった方は変えない）
		//-------------------------------------------------------------------------------
		internal void SetGuideFooterTilesets(int[] createdIndexes)
		{
			if (createdIndexes[0] >= 0)
			{
				this.nudNewMapFooterTileset1Index.Value = Math.Min(this.nudNewMapFooterTileset1Index.Maximum, createdIndexes[0]);
			}
			if (createdIndexes[1] >= 0)
			{
				this.nudNewMapFooterTileset2Index.Value = Math.Min(this.nudNewMapFooterTileset2Index.Maximum, createdIndexes[1]);
			}
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧の画像から切り出した部分を、マップチップ取り込みのウィザードへ渡して開く処理
		// 取り込み先は今のマップのタイルセット（ウィザードで 1・2 を選ぶ）
		//-------------------------------------------------------------------------------
		internal void OpenChipImportWithImage(Bitmap part, bool secondary)
		{
			string path = Path.Combine(Path.GetTempPath(), "BochiBochiEditor_blocks_" + Guid.NewGuid().ToString("N") + ".png");
			part.Save(path, ImageFormat.Png);
			try
			{
				this.OpenChipImportWizard(path, secondary);
			}
			finally
			{
				try
				{
					File.Delete(path);
				}
				catch (IOException)
				{
				}
			}
		}
	}
}
