using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップチップ取り込みウィザードから ROM を読み書きするための窓口（ウィザードは ROM を直接触らない）
	//-------------------------------------------------------------------------------
	internal interface ITileImportHost
	{
		string DescribeTileset(bool secondary);
		TileImportEngine.TargetState GetTargetState(bool secondary);
		Color[] GetPalette(int slot);
		string DescribePaletteUsage(int slot);
		int CountPaletteUsers(int slot);
		int SuggestPaletteSlot(bool secondary);
		int TotalBlocks { get; }
		Bitmap GetBlockImage(int blockId);
		ushort[] GetBlockEntries(int blockId);
		TileImportFormat Format { get; }
		int PrimaryBlocks { get; }
		Bitmap GetBlockPaletteImage();
		string BackupFileName { get; }
		string Commit(TileImportEngine.Plan plan, TileImportEngine.Options options, bool makeBackup);
	}

	//-------------------------------------------------------------------------------
	// マップエディタ側のマップチップ取り込み処理（空き枠の調査と ROM への書き込み）
	//-------------------------------------------------------------------------------
	public partial class MapEditor : ITileImportHost
	{
		// 取り込み中のタイルセットの形（ウィザードを開いている間は変えない。開いていなければ null）
		private TileImportFormat importFormat;

		// 画像ボタンの画像（ファイルごとに 1 回だけ読む。読めなければ null で、文字のボタンになる）
		private readonly Dictionary<string, Image> buttonImages = new Dictionary<string, Image>();

		//-------------------------------------------------------------------------------
		// 画像ボタンの画像を、今の言語のもの（img\<名前>_JP.png / _EN.png）で返す処理
		//-------------------------------------------------------------------------------
		private Image LoadButtonImage(string baseName)
		{
			string suffix = Localizer.Language == Localizer.Japanese ? "JP" : "EN";
			string fileName = baseName + "_" + suffix + ".png";
			Image image;
			if (!this.buttonImages.TryGetValue(fileName, out image))
			{
				try
				{
					string path = AppAssetLocator.FindRequiredFile(Path.Combine("img", fileName));
					// ファイルを掴んだままにしないよう、読み込んだ画像を複製して使う
					using (Bitmap loaded = new Bitmap(path))
					{
						image = new Bitmap(loaded);
					}
				}
				catch (Exception)
				{
					image = null;
				}
				this.buttonImages[fileName] = image;
			}
			return image;
		}

		//-------------------------------------------------------------------------------
		// チップ取り込みボタンの画像を、今の言語のもの（img\Mapchip_import_JP.png / _EN.png）に切り替える処理
		//-------------------------------------------------------------------------------
		private void UpdateImportButtonImage()
		{
			this.btnImportChips.ButtonImage = this.LoadButtonImage("Mapchip_import");
		}

		//-------------------------------------------------------------------------------
		// 「チップ取り込み」ボタンでウィザードを開く処理
		//-------------------------------------------------------------------------------
		private void btnImportChips_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			this.OpenChipImportWizard(null, true);
		}

		//-------------------------------------------------------------------------------
		// マップチップ取り込みのウィザードを開く処理
		// imagePath を渡すと、その画像を読み込んだ状態で開く（マップタイルの移植の「一部だけ取り込み」から使う）。secondary は取り込み先の初期値
		//-------------------------------------------------------------------------------
		internal void OpenChipImportWizard(string imagePath, bool secondary)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.romData == null || this.tempHeader == null || this.tempTileset1 == null || this.tempTileset2 == null || this.tempFooter == null)
			{
				MessageBox.Show(this, Localizer.T("先にマップを選んでください。取り込み先はそのマップのタイルセットになります。"), Localizer.T("マップチップ取り込み"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (!this.ConfirmPendingChangesBeforeTilesetEdit())
			{
				return;
			}
			TileImportFormat format = TileImportFormat.FromCurrentGame();
			if (!format.IsSupported)
			{
				MessageBox.Show(this, string.Format(Localizer.T("このタイルセットの形（ブロック {0} バイト・挙動 {1} バイト）には、取り込みが対応していません。"), format.BlockBytes, format.BehaviorBytes), Localizer.T("マップチップ取り込み"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			this.importFormat = format;
			try
			{
				using (TileImportWizard wizard = new TileImportWizard(this))
				{
					AppIconHelper.Apply(wizard);
					UiTheme.Apply(wizard);
					if (imagePath != null)
					{
						wizard.PreloadImage(imagePath, secondary);
					}
					wizard.ShowDialog(this);
				}
			}
			finally
			{
				this.importFormat = null;
			}
		}

		//-------------------------------------------------------------------------------
		// 取り込みに使うタイルセットの形を返す処理（ウィザードを開いている間は、開いたときの形）
		//-------------------------------------------------------------------------------
		TileImportFormat ITileImportHost.Format
		{
			get { return this.importFormat ?? TileImportFormat.FromCurrentGame(); }
		}

		//-------------------------------------------------------------------------------
		// 画面を出さずに、今のマップのタイルセットへ画像を取り込む処理（検証用）
		// ウィザードと同じ順番で計画を作って空きへ自動配置し、書き込む。戻り値は書き込めなかった理由（成功なら null）
		//-------------------------------------------------------------------------------
		internal string ImportChipsForTest(Bitmap source, bool secondary, int paletteSlot, TileImportEngine.LayerStyle layer, int baseBlockId, out TileImportEngine.Plan plan)
		{
			plan = null;
			if (this.IsRomReadOnly)
			{
				return Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。");
			}
			if (source == null || this.romData == null || this.tempHeader == null || this.tempTileset1 == null || this.tempTileset2 == null || this.tempFooter == null)
			{
				return Localizer.T("先にマップを選んでください。取り込み先はそのマップのタイルセットになります。");
			}
			this.importFormat = TileImportFormat.FromCurrentGame();
			try
			{
				ITileImportHost host = this;
				Color corner = source.GetPixel(0, 0);
				int slot = paletteSlot >= 0 ? paletteSlot : host.SuggestPaletteSlot(secondary);
				TileImportEngine.Options options = new TileImportEngine.Options
				{
					Source = source,
					TransparentColor = Color.FromArgb(255, corner.R, corner.G, corner.B),
					IsSecondary = secondary,
					PaletteSlot = slot,
					Mode = TileImportEngine.PaletteMode.Overwrite,
					ExistingPalette = host.GetPalette(slot),
					Layer = layer
				};
				if (layer != TileImportEngine.LayerStyle.ArtOnBottom)
				{
					options.BaseEntries = host.GetBlockEntries(baseBlockId);
				}
				TileImportEngine.TargetState state = host.GetTargetState(secondary);
				plan = TileImportEngine.BuildPlan(options, state);
				plan.AutoAssign();
				if (!plan.CanWrite)
				{
					List<string> reasons = new List<string>(plan.Errors);
					if (plan.Blocks.Count == 0)
					{
						reasons.Add(Localizer.T("ブロック : なし"));
					}
					if (plan.UnassignedCount > 0)
					{
						reasons.Add(string.Format(Localizer.T("ブロックの空きが {0} 個足りません。次の「配置」で、使用中のブロックへの差し替え先を選んでください。"), plan.UnassignedCount));
					}
					string problem = plan.FindAssignmentProblem();
					if (problem != null)
					{
						reasons.Add(problem);
					}
					return string.Join(Environment.NewLine, reasons);
				}
				string report;
				return this.CommitImport(plan, options, false, out report);
			}
			finally
			{
				this.importFormat = null;
			}
		}

		//-------------------------------------------------------------------------------
		// タイルセットを書き換える前に、未反映の変更を保存するか確認する処理（キャンセルなら false）
		//-------------------------------------------------------------------------------
		private bool ConfirmPendingChangesBeforeTilesetEdit()
		{
			if (!this.hasUnsavedChanges)
			{
				return true;
			}
			DialogResult answer = MessageBox.Show(this, Localizer.T("現在のマップに未反映の変更があります。先に反映しますか？\n（「いいえ」で変更を破棄して続けます）"), Localizer.T("マップチップ取り込み"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (answer == DialogResult.Cancel)
			{
				return false;
			}
			if (answer == DialogResult.Yes)
			{
				this.btnSave_Click(null, null);
			}
			else
			{
				this.SetUnsavedChanges(false);
				if (this.originalHeader != null)
				{
					this.tempHeader = this.originalHeader.Clone();
					this.RefreshEditorView(MapEditor.ViewUpdateLevel.FooterAndGraphics);
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// 取り込み先タイルセットの見出し情報を返す処理
		//-------------------------------------------------------------------------------
		private TilesetHeader GetImportTileset(bool secondary)
		{
			return secondary ? this.tempTileset2 : this.tempTileset1;
		}

		//-------------------------------------------------------------------------------
		// 取り込み先タイルセットの見出しが置かれている ROM 上の位置を返す処理
		//-------------------------------------------------------------------------------
		private uint GetImportTilesetHeaderAddress(bool secondary)
		{
			return secondary ? this.tempFooter.Tileset2Address : this.tempFooter.Tileset1Address;
		}

		//-------------------------------------------------------------------------------
		// タイルセットのブロック数（第1・第2とも ROM の配置から推定した、実際に使われている数）を返す処理
		//-------------------------------------------------------------------------------
		private int GetImportBlockCount(bool secondary, uint headerAddress)
		{
			if (!secondary)
			{
				return this.GetPrimaryBlockCount(this.ReadTilesetHeader((int)headerAddress));
			}
			return this.GetSecondaryBlockCount(this.AddressToTilesetIndex(headerAddress), this.ReadTilesetHeader((int)headerAddress));
		}

		//-------------------------------------------------------------------------------
		// タイルセットのタイル画像（展開済み）を、そのタイルセットの上限枚数までに揃えて返す処理
		//-------------------------------------------------------------------------------
		private byte[] GetImportTileImage(TilesetHeader tileset, bool secondary)
		{
			byte[] raw = this.LoadTilesetRawImage(tileset);
			int max = ((ITileImportHost)this).Format.TileCapacity(secondary) * TileImportEngine.TileBytes;
			int length = Math.Min(raw.Length, max) / TileImportEngine.TileBytes * TileImportEngine.TileBytes;
			byte[] result = new byte[length];
			Array.Copy(raw, result, length);
			return result;
		}

		//-------------------------------------------------------------------------------
		// ウィザードに表示するタイルセットの説明文を返す処理
		//-------------------------------------------------------------------------------
		string ITileImportHost.DescribeTileset(bool secondary)
		{
			uint header = this.GetImportTilesetHeaderAddress(secondary);
			return string.Format(Localizer.T("{0}（番号 {1}、見出し 0x{2:X6}）"), secondary ? Localizer.T("タイルセット2") : Localizer.T("タイルセット1"), this.AddressToTilesetIndex(header), header);
		}

		//-------------------------------------------------------------------------------
		// 取り込み先の空きタイル・空きブロック・再利用できるタイルを調べる処理
		//-------------------------------------------------------------------------------
		TileImportEngine.TargetState ITileImportHost.GetTargetState(bool secondary)
		{
			TileImportEngine.TargetState state = new TileImportEngine.TargetState();
			TileImportFormat format = ((ITileImportHost)this).Format;
			state.Format = format;
			TilesetHeader target = this.GetImportTileset(secondary);
			uint headerAddress = this.GetImportTilesetHeaderAddress(secondary);
			int tileBase = format.TileBase(secondary);
			state.TileImage = this.GetImportTileImage(target, secondary);
			state.TileCapacity = format.TileCapacity(secondary);

			// どのブロックからも参照されているタイル（通し番号）
			HashSet<int> referencedTiles = secondary ? this.CollectReferencedTilesForCurrentPair() : this.CollectReferencedTilesInAllTilesets();

			// 空きタイル: 全面透明で、どのブロックからも使われていないもの（第1のタイル 0 は予約扱い）
			int tileCount = state.TileImage.Length / TileImportEngine.TileBytes;
			for (int local = 0; local < tileCount; local++)
			{
				byte[] tile = new byte[TileImportEngine.TileBytes];
				Array.Copy(state.TileImage, local * TileImportEngine.TileBytes, tile, 0, TileImportEngine.TileBytes);
				int global = tileBase + local;
				bool blank = TileImportEngine.IsBlankTile(tile);
				if (blank && !referencedTiles.Contains(global) && !(!secondary && local == 0))
				{
					state.FreeTileSlots.Add(local);
				}
				else if (!blank)
				{
					state.ReusableTiles[global] = tile;
				}
			}

			// 第1タイルセットのタイルは、第2タイルセットのブロックからも使える
			byte[] primaryImage = secondary ? this.GetImportTileImage(this.tempTileset1, false) : state.TileImage;
			for (int local = 0; local < primaryImage.Length / TileImportEngine.TileBytes; local++)
			{
				byte[] tile = new byte[TileImportEngine.TileBytes];
				Array.Copy(primaryImage, local * TileImportEngine.TileBytes, tile, 0, TileImportEngine.TileBytes);
				if (TileImportEngine.IsBlankTile(tile))
				{
					if (state.BlankTileId < 0)
					{
						state.BlankTileId = local;
					}
				}
				else if (secondary)
				{
					state.ReusableTiles[local] = tile;
				}
			}
			// 第1に全面透明のタイルが無ければ、取り込み先（第2）の透明タイルを使う（新しいタイルの置き場所からは外される）
			if (state.BlankTileId < 0 && secondary)
			{
				for (int local = 0; local < tileCount; local++)
				{
					byte[] tile = new byte[TileImportEngine.TileBytes];
					Array.Copy(state.TileImage, local * TileImportEngine.TileBytes, tile, 0, TileImportEngine.TileBytes);
					if (TileImportEngine.IsBlankTile(tile))
					{
						state.BlankTileId = tileBase + local;
						break;
					}
				}
			}

			// 空きブロック: 中身が全部 0 で、このタイルセットを使うどのマップ・ボーダーにも置かれていないもの
			int blockCount = this.GetImportBlockCount(secondary, headerAddress);
			int blockBase = format.BlockBase(secondary);
			int blockBytes = format.BlockBytes;
			Dictionary<int, int> usedOnMaps = this.CollectBlocksUsedOnMaps(secondary, headerAddress);
			state.BlockStart = blockBase;
			state.BlockCount = blockCount;
			state.BlockMapUsage = usedOnMaps;
			state.ProtectedBlockIds = this.CollectProtectedImportBlocks(format, blockBase, blockCount);
			state.CombinedTiles = this.BuildCombinedTileImage(this.tempTileset1, this.tempTileset2);
			Color[] palettes = this.LoadAllPalettes(this.tempTileset1, this.tempTileset2);
			Array.Copy(palettes, state.AllPalettes, Math.Min(palettes.Length, state.AllPalettes.Length));
			for (int local = 0; local < blockCount; local++)
			{
				int id = blockBase + local;
				if (id == 0 || usedOnMaps.ContainsKey(id) || state.ProtectedBlockIds.Contains(id))
				{
					continue;
				}
				long offset = (long)target.BlockImageAddress + (long)local * blockBytes;
				if (target.BlockImageAddress == 0 || offset + blockBytes > this.romData.Length)
				{
					break;
				}
				bool empty = true;
				for (int i = 0; i < blockBytes; i++)
				{
					if (this.romData[offset + i] != 0)
					{
						empty = false;
						break;
					}
				}
				if (empty)
				{
					state.FreeBlockIds.Add(id);
				}
			}
			return state;
		}

		//-------------------------------------------------------------------------------
		// 取り込み先の範囲で、置いてはいけないブロック番号を集める処理
		// （ファイアレッドで、前のブロックがレイヤー 0x30 のとき、その次の番号は 3 層目として使われている）
		//-------------------------------------------------------------------------------
		private HashSet<int> CollectProtectedImportBlocks(TileImportFormat format, int blockBase, int blockCount)
		{
			HashSet<int> result = new HashSet<int>();
			if (!format.TripleLayerByNextBlock)
			{
				return result;
			}
			for (int id = Math.Max(1, blockBase); id < blockBase + blockCount; id++)
			{
				if (this.IsTripleLayerBlock(id - 1))
				{
					result.Add(id);
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 今のマップの第1・第2タイルセットのブロックが参照しているタイル番号を集める処理
		//-------------------------------------------------------------------------------
		private HashSet<int> CollectReferencedTilesForCurrentPair()
		{
			HashSet<int> result = new HashSet<int>();
			this.AddReferencedTiles(result, this.tempTileset1.BlockImageAddress, this.GetPrimaryBlockCount(this.tempTileset1));
			this.AddReferencedTiles(result, this.tempTileset2.BlockImageAddress, this.GetImportBlockCount(true, this.tempFooter.Tileset2Address));
			return result;
		}

		//-------------------------------------------------------------------------------
		// ROM 内のすべてのタイルセットのブロックが参照しているタイル番号を集める処理
		// （第1タイルセットは複数の第2タイルセットと組み合わせて使われるため、全部を調べる）
		//-------------------------------------------------------------------------------
		private HashSet<int> CollectReferencedTilesInAllTilesets()
		{
			HashSet<int> result = new HashSet<int>();
			// 調べる見出しの位置（連続した見出しの表と、地形データから実際に使われている見出し。重複は除く）
			List<uint> headers = new List<uint>();
			HashSet<uint> seen = new HashSet<uint>();
			for (int index = 0; index < 256; index++)
			{
				int header = MapEditor.TILESET_INDEX_START_OFFSET + index * MapEditor.TILESET_HEADER_SIZE;
				if (header + 24 > this.romData.Length)
				{
					break;
				}
				if (!this.IsImportTilesetHeader((uint)header))
				{
					break;
				}
				if (seen.Add((uint)header))
				{
					headers.Add((uint)header);
				}
			}
			foreach (uint header in this.CollectTerrainTilesetHeaders())
			{
				if (seen.Add(header))
				{
					headers.Add(header);
				}
			}
			foreach (uint header in headers)
			{
				uint blockPointer = BitConverter.ToUInt32(this.romData, (int)header + 12);
				bool secondary = this.romData[header + 1] == 1;
				this.AddReferencedTiles(result, blockPointer - 0x08000000u, this.GetImportBlockCount(secondary, header));
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 指定位置がタイルセットの見出しとして読めるか（絵とブロックの位置が ROM を指し、第1/第2の印が 0 か 1）を返す処理
		//-------------------------------------------------------------------------------
		private bool IsImportTilesetHeader(uint header)
		{
			if (header == 0 || (long)header + 24 > this.romData.Length)
			{
				return false;
			}
			uint imagePointer = BitConverter.ToUInt32(this.romData, (int)header + 4);
			uint blockPointer = BitConverter.ToUInt32(this.romData, (int)header + 12);
			return this.IsRomPointer(imagePointer) && this.IsRomPointer(blockPointer) && this.romData[header + 1] <= 1;
		}

		//-------------------------------------------------------------------------------
		// 地形データの表から、各地形データ（+16 が第1、+20 が第2）が使っているタイルセットの見出しの位置を集める処理
		//-------------------------------------------------------------------------------
		private List<uint> CollectTerrainTilesetHeaders()
		{
			List<uint> result = new List<uint>();
			if (MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET <= 0)
			{
				return result;
			}
			for (int i = 0; i < MapEditor.MAP_TERRAIN_ID_COUNT; i++)
			{
				long entry = (long)MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET + i * 4L;
				if (entry + 4 > this.romData.Length)
				{
					break;
				}
				uint footerPointer = BitConverter.ToUInt32(this.romData, (int)entry);
				if (!this.IsRomPointer(footerPointer))
				{
					continue;
				}
				long footer = footerPointer - 0x08000000u;
				if (footer + 24 > this.romData.Length)
				{
					continue;
				}
				foreach (int field in new int[] { 16, 20 })
				{
					uint pointer = BitConverter.ToUInt32(this.romData, (int)footer + field);
					if (this.IsRomPointer(pointer) && this.IsImportTilesetHeader(pointer - 0x08000000u))
					{
						result.Add(pointer - 0x08000000u);
					}
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// ブロックデータ（1 ブロック BlockBytes バイト、全層）から参照タイル番号を集める処理
		//-------------------------------------------------------------------------------
		private void AddReferencedTiles(HashSet<int> result, uint blockAddress, int blockCount)
		{
			if (blockAddress == 0)
			{
				return;
			}
			int entriesPerBlock = ((ITileImportHost)this).Format.EntriesPerBlock;
			for (int i = 0; i < blockCount * entriesPerBlock; i++)
			{
				long offset = (long)blockAddress + i * 2;
				if (offset + 2 > this.romData.Length)
				{
					return;
				}
				result.Add(BitConverter.ToUInt16(this.romData, (int)offset) & 0x3FF);
			}
		}

		//-------------------------------------------------------------------------------
		// 指定タイルセットを使うすべてのマップ（本体とボーダー）に置かれているブロック番号を集める処理
		//-------------------------------------------------------------------------------
		private Dictionary<int, int> CollectBlocksUsedOnMaps(bool secondary, uint tilesetHeaderAddress)
		{
			Dictionary<int, int> used = new Dictionary<int, int>();
			foreach (MapHeader header in this.mapHeaders)
			{
				MapFooter footer = this.TryReadFooter(header);
				if (footer == null)
				{
					continue;
				}
				uint address = secondary ? footer.Tileset2Address : footer.Tileset1Address;
				if (address != tilesetHeaderAddress)
				{
					continue;
				}
				// 1 マップの中で何回使われていても 1 と数える（「何マップで使われているか」）
				HashSet<int> inThisMap = new HashSet<int>();
				this.AddUsedBlocks(inThisMap, footer.MapDataAddress, footer.MapWidth * footer.MapHeight);
				this.AddUsedBlocks(inThisMap, footer.BorderDataAddress, footer.BorderWidth * footer.BorderHeight);
				foreach (int id in inThisMap)
				{
					int count;
					used.TryGetValue(id, out count);
					used[id] = count + 1;
				}
			}
			return used;
		}

		//-------------------------------------------------------------------------------
		// マップデータ（1 マス 2 バイト、下位 10bit がブロック番号）からブロック番号を集める処理
		//-------------------------------------------------------------------------------
		private void AddUsedBlocks(HashSet<int> used, uint address, int cellCount)
		{
			if (address == 0 || cellCount <= 0 || address + (long)cellCount * 2 > this.romData.Length)
			{
				return;
			}
			for (int i = 0; i < cellCount; i++)
			{
				used.Add(BitConverter.ToUInt16(this.romData, (int)address + i * 2) & 0x3FF);
			}
		}

		//-------------------------------------------------------------------------------
		// パレット枠（0〜12）の現在の 16 色を返す処理
		//-------------------------------------------------------------------------------
		Color[] ITileImportHost.GetPalette(int slot)
		{
			uint address = this.GetPaletteSlotAddress(slot);
			Color[] colors = new Color[16];
			if (address == 0 || address + 32 > this.romData.Length)
			{
				return colors;
			}
			byte[] bytes = new byte[32];
			Array.Copy(this.romData, (int)address, bytes, 0, 32);
			return ImageProcessor.LoadPalette(bytes, false);
		}

		//-------------------------------------------------------------------------------
		// パレット枠の ROM 上の位置を返す処理（第1の本数より前は第1、以降は第2タイルセットのパレット表）
		// 第2の側でも、表の先頭から枠番号の通しで数える
		//-------------------------------------------------------------------------------
		private uint GetPaletteSlotAddress(int slot)
		{
			TilesetHeader tileset = slot < ((ITileImportHost)this).Format.PrimaryPaletteCount ? this.tempTileset1 : this.tempTileset2;
			return tileset.PaletteAddress == 0 ? 0 : tileset.PaletteAddress + (uint)(slot * 32);
		}

		//-------------------------------------------------------------------------------
		// パレット枠を使っているブロック数を数える処理（上書きしたときに色が変わるブロックの目安）
		// 第1のパレットは第2のブロックからも使えるため、今のマップの第1・第2の両方のブロックを数える
		//-------------------------------------------------------------------------------
		private int CountBlocksUsingPalette(int slot)
		{
			TileImportFormat format = ((ITileImportHost)this).Format;
			int blockBytes = format.BlockBytes;
			int entriesPerBlock = format.EntriesPerBlock;
			int count = 0;
			foreach (bool secondary in new bool[] { false, true })
			{
				TilesetHeader tileset = this.GetImportTileset(secondary);
				int blockCount = this.GetImportBlockCount(secondary, this.GetImportTilesetHeaderAddress(secondary));
				for (int b = 0; b < blockCount; b++)
				{
					long offset = (long)tileset.BlockImageAddress + (long)b * blockBytes;
					if (tileset.BlockImageAddress == 0 || offset + blockBytes > this.romData.Length)
					{
						break;
					}
					bool empty = true;
					bool uses = false;
					for (int i = 0; i < entriesPerBlock; i++)
					{
						ushort entry = BitConverter.ToUInt16(this.romData, (int)offset + i * 2);
						empty &= entry == 0;
						uses |= (entry >> 12) == slot;
					}
					if (!empty && uses)
					{
						count++;
					}
				}
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// パレット枠を使っているブロック数を返す処理（ウィザード用）
		//-------------------------------------------------------------------------------
		int ITileImportHost.CountPaletteUsers(int slot)
		{
			return this.CountBlocksUsingPalette(slot);
		}

		//-------------------------------------------------------------------------------
		// パレット枠の使用状況を説明する文を返す処理
		//-------------------------------------------------------------------------------
		string ITileImportHost.DescribePaletteUsage(int slot)
		{
			int count = this.CountBlocksUsingPalette(slot);
			return count == 0 ? Localizer.T("未使用") : string.Format(Localizer.T("{0} ブロックで使用中"), count);
		}

		//-------------------------------------------------------------------------------
		// 既定で選ぶパレット枠（使っているブロックが最も少ない枠）を返す処理
		//-------------------------------------------------------------------------------
		int ITileImportHost.SuggestPaletteSlot(bool secondary)
		{
			TileImportFormat format = ((ITileImportHost)this).Format;
			int first = format.PaletteFirst(secondary);
			int last = format.PaletteEndExclusive(secondary) - 1;
			int best = last;
			int bestCount = int.MaxValue;
			for (int slot = last; slot >= first; slot--)
			{
				int count = this.CountBlocksUsingPalette(slot);
				if (count < bestCount)
				{
					bestCount = count;
					best = slot;
				}
			}
			return best;
		}

		//-------------------------------------------------------------------------------
		// ブロック総数（第1＋第2）を返す処理
		//-------------------------------------------------------------------------------
		int ITileImportHost.TotalBlocks
		{
			get { return this.totalBlocks; }
		}

		//-------------------------------------------------------------------------------
		// 指定ブロックの絵（16x16）を返す処理（下地ブロックのプレビュー用）
		//-------------------------------------------------------------------------------
		Bitmap ITileImportHost.GetBlockImage(int blockId)
		{
			if (this.blockPaletteBitmap == null || blockId < 0 || blockId >= this.totalBlocks)
			{
				return null;
			}
			Rectangle src = new Rectangle(blockId % PaletteColumns * 16, blockId / PaletteColumns * 16, 16, 16);
			return this.blockPaletteBitmap.Clone(src, this.blockPaletteBitmap.PixelFormat);
		}

		//-------------------------------------------------------------------------------
		// 指定ブロックの全層分（2 層なら 8 枠、3 層なら 12 枠）のタイル指定を返す処理（範囲外は null）
		//-------------------------------------------------------------------------------
		ushort[] ITileImportHost.GetBlockEntries(int blockId)
		{
			if (blockId < 0 || blockId >= this.totalBlocks)
			{
				return null;
			}
			TileImportFormat format = ((ITileImportHost)this).Format;
			bool secondary = blockId >= format.PrimaryBlockCount;
			TilesetHeader tileset = this.GetImportTileset(secondary);
			int local = blockId - format.BlockBase(secondary);
			// 第1タイルセットの枠のうち、実際のブロックが無い番号は読まない（後ろは別のデータ）
			if (!secondary && local >= this.GetPrimaryBlockCount(tileset))
			{
				return null;
			}
			long offset = (long)tileset.BlockImageAddress + (long)local * format.BlockBytes;
			if (tileset.BlockImageAddress == 0 || offset + format.BlockBytes > this.romData.Length)
			{
				return null;
			}
			ushort[] entries = new ushort[format.EntriesPerBlock];
			for (int i = 0; i < entries.Length; i++)
			{
				entries[i] = BitConverter.ToUInt16(this.romData, (int)offset + i * 2);
			}
			return entries;
		}

		//-------------------------------------------------------------------------------
		// 第1タイルセットのブロック数（一覧で第2との境目を示すのに使う）を返す処理
		//-------------------------------------------------------------------------------
		int ITileImportHost.PrimaryBlocks
		{
			get { return this.primaryBlockCount; }
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧の絵（横 8 ブロック、番号順）の複製を返す処理（無ければ null）
		//-------------------------------------------------------------------------------
		Bitmap ITileImportHost.GetBlockPaletteImage()
		{
			return this.blockPaletteBitmap == null ? null : (Bitmap)this.blockPaletteBitmap.Clone();
		}

		//-------------------------------------------------------------------------------
		// 書き込み前に作るバックアップファイルの名前を返す処理
		//-------------------------------------------------------------------------------
		string ITileImportHost.BackupFileName
		{
			get
			{
				if (string.IsNullOrEmpty(this.loadedRomPath))
				{
					return null;
				}
				return Path.GetFileNameWithoutExtension(this.loadedRomPath) + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(this.loadedRomPath);
			}
		}

		//-------------------------------------------------------------------------------
		// 取り込み計画をメモリ上の ROM へ書き込み、画面を更新する処理（結果の説明文を返す）
		// 書き込めない場合は ROM を変えずに、理由を付けた例外を投げる（ウィザードが「書き込みに失敗しました」と表示する）
		//-------------------------------------------------------------------------------
		string ITileImportHost.Commit(TileImportEngine.Plan plan, TileImportEngine.Options options, bool makeBackup)
		{
			if (this.BlockIfReadOnly()) return Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。");
			string report;
			string error = this.CommitImport(plan, options, makeBackup, out report);
			if (error != null)
			{
				throw new InvalidOperationException(error);
			}
			return report;
		}

		//-------------------------------------------------------------------------------
		// 取り込み計画を確かめてから ROM へまとめて書き込み、画面を更新する処理
		// 戻り値は書き込めなかった理由（成功なら null。その場合 ROM は 1 バイトも変えていない）、report は結果の説明文
		//-------------------------------------------------------------------------------
		private string CommitImport(TileImportEngine.Plan plan, TileImportEngine.Options options, bool makeBackup, out string report)
		{
			report = null;
			string suffix = Environment.NewLine + Localizer.T("ROM は変更していません。");
			if (plan == null || options == null || this.romData == null || this.tempTileset1 == null || this.tempTileset2 == null || this.tempFooter == null)
			{
				return Localizer.T("書き込む内容がありません。") + suffix;
			}
			// 0. 形と計画を確かめる
			TileImportFormat format = ((ITileImportHost)this).Format;
			TileImportFormat currentFormat = TileImportFormat.FromCurrentGame();
			if (!format.IsSupported || !format.Equals(currentFormat))
			{
				return string.Format(Localizer.T("このタイルセットの形（ブロック {0} バイト・挙動 {1} バイト）には、取り込みが対応していません。"), currentFormat.BlockBytes, currentFormat.BehaviorBytes) + suffix;
			}
			if (plan.Format == null || !plan.Format.Equals(format))
			{
				return Localizer.T("計画を作ったときと、タイルセットの形が変わっています。取り込みをやり直してください。") + suffix;
			}
			if (plan.Errors.Count > 0 || plan.Blocks.Count == 0 || plan.UnassignedCount > 0 || plan.HasDuplicateAssignment)
			{
				return Localizer.T("計画に書き込めない点が残っています（エラー・未配置・番号の重複）。") + suffix;
			}
			bool secondary = options.IsSecondary;
			TilesetHeader tileset = this.GetImportTileset(secondary);
			uint headerAddress = this.GetImportTilesetHeaderAddress(secondary);
			List<string> lines = new List<string>();
			// 書く内容（ROM 上の位置とバイト列）
			List<KeyValuePair<long, byte[]>> patches = new List<KeyValuePair<long, byte[]>>();

			// 1. ブロックと挙動データ（挙動はレイヤー以外の値を保ち、重なり方だけ設定する）
			int blockBase = format.BlockBase(secondary);
			int blockCount = this.GetImportBlockCount(secondary, headerAddress);
			HashSet<int> protectedIds = this.CollectProtectedImportBlocks(format, blockBase, blockCount);
			HashSet<int> seenIds = new HashSet<int>();
			if (tileset.BlockImageAddress == 0)
			{
				return Localizer.T("取り込み先のブロック表の位置を読めませんでした。") + suffix;
			}
			// 第2タイルセットのブロック数は推定なので、表の後ろにある次のデータを越えないかも確かめる
			long blockLimit = secondary ? Math.Min((long)this.FindNextDataStart(tileset.BlockImageAddress), this.romData.Length) : this.romData.Length;
			long behaviorLimit = (secondary && tileset.BlockBehaviorAddress != 0) ? Math.Min((long)this.FindNextDataStart(tileset.BlockBehaviorAddress), this.romData.Length) : this.romData.Length;
			foreach (TileImportEngine.PlannedBlock block in plan.Blocks)
			{
				int id = block.BlockId;
				if (id <= 0 || id < blockBase || id >= blockBase + blockCount)
				{
					return string.Format(Localizer.T("ブロック 0x{0:X3} は取り込み先のタイルセットの範囲外です。"), id) + suffix;
				}
				if (!seenIds.Add(id))
				{
					return string.Format(Localizer.T("ブロック 0x{0:X3} に 2 個以上のブロックを割り当てています。"), id) + suffix;
				}
				if (protectedIds.Contains(id))
				{
					return string.Format(Localizer.T("ブロック 0x{0:X3} は前のブロックの 3 層目として使われているため置けません。"), id) + suffix;
				}
				if (block.Entries == null || block.Entries.Length != format.EntriesPerBlock)
				{
					return Localizer.T("ブロックのタイル指定の数がタイルセットの形と合いません。") + suffix;
				}
				int local = id - blockBase;
				long blockOffset = (long)tileset.BlockImageAddress + (long)local * format.BlockBytes;
				if (blockOffset + format.BlockBytes > blockLimit)
				{
					return string.Format(Localizer.T("ブロック 0x{0:X3} の書き込み先がブロック表の外にはみ出します。"), id) + suffix;
				}
				byte[] blockBytes = new byte[format.BlockBytes];
				for (int i = 0; i < format.EntriesPerBlock; i++)
				{
					blockBytes[i * 2] = (byte)(block.Entries[i] & 0xFF);
					blockBytes[i * 2 + 1] = (byte)(block.Entries[i] >> 8);
				}
				patches.Add(new KeyValuePair<long, byte[]>(blockOffset, blockBytes));
				if (tileset.BlockBehaviorAddress != 0)
				{
					long behaviorOffset = (long)tileset.BlockBehaviorAddress + (long)local * format.BehaviorBytes;
					if (behaviorOffset + format.BehaviorBytes > behaviorLimit)
					{
						return string.Format(Localizer.T("ブロック 0x{0:X3} の挙動データの書き込み先が挙動表の外にはみ出します。"), id) + suffix;
					}
					byte[] current = new byte[format.BehaviorBytes];
					Array.Copy(this.romData, behaviorOffset, current, 0, format.BehaviorBytes);
					patches.Add(new KeyValuePair<long, byte[]>(behaviorOffset, TileImportEngine.BuildBehaviorPatch(current, format, options.Layer)));
				}
			}

			// 2. パレット（上書きを選んだ場合だけ。色 0 の透明は変えない）
			if (options.Mode == TileImportEngine.PaletteMode.Overwrite)
			{
				if (options.PaletteSlot < format.PaletteFirst(secondary) || options.PaletteSlot >= format.PaletteEndExclusive(secondary))
				{
					return string.Format(Localizer.T("パレット枠 {0} は取り込み先のタイルセットの枠ではありません（使えるのは {1}〜{2}）。"), options.PaletteSlot, format.PaletteFirst(secondary), format.PaletteEndExclusive(secondary) - 1) + suffix;
				}
				uint paletteAddress = this.GetPaletteSlotAddress(options.PaletteSlot);
				if (paletteAddress == 0 || (long)paletteAddress + 32 > this.romData.Length || plan.Palette == null || plan.Palette.Length < 16)
				{
					return Localizer.T("パレットの書き込み先を読めませんでした。") + suffix;
				}
				byte[] paletteBytes = new byte[30];
				for (int i = 1; i < 16; i++)
				{
					ushort value = TileImportEngine.ToBgr555(plan.Palette[i]);
					paletteBytes[(i - 1) * 2] = (byte)(value & 0xFF);
					paletteBytes[(i - 1) * 2 + 1] = (byte)(value >> 8);
				}
				patches.Add(new KeyValuePair<long, byte[]>((long)paletteAddress + 2, paletteBytes));
			}

			// 3. タイル画像: 新しいタイルを入れて圧縮し直し、空き領域へ書いて見出しのポインタを付け替える
			uint newAddress = 0;
			int compressedLength = 0;
			if (plan.NewTiles.Count > 0)
			{
				int capacity = format.TileCapacity(secondary);
				foreach (KeyValuePair<int, byte[]> pair in plan.NewTiles)
				{
					if (pair.Key < 0 || pair.Key >= capacity || pair.Value == null || pair.Value.Length != TileImportEngine.TileBytes)
					{
						return string.Format(Localizer.T("新しいタイル（番号 {0}）がタイルセットの容量（{1} 枚）の外か、大きさが正しくありません。"), pair.Key, capacity) + suffix;
					}
				}
				if ((long)headerAddress + 8 > this.romData.Length)
				{
					return Localizer.T("取り込み先のタイルセットの見出しを読めませんでした。") + suffix;
				}
				byte[] current = this.GetImportTileImage(tileset, secondary);
				int tiles = Math.Max(current.Length / TileImportEngine.TileBytes, plan.NewTiles.Keys.Max() + 1);
				byte[] image = new byte[tiles * TileImportEngine.TileBytes];
				Array.Copy(current, image, current.Length);
				foreach (KeyValuePair<int, byte[]> pair in plan.NewTiles)
				{
					Array.Copy(pair.Value, 0, image, pair.Key * TileImportEngine.TileBytes, TileImportEngine.TileBytes);
				}
				// VRAM へ直接展開しても壊れない形式（参照距離 2 以上）で圧縮する
				byte[] compressed = ImageProcessor.LZ77Comp(image, true);
				if (!this.TryFindFreeSpaceForNewEvent(compressed.Length, ref newAddress))
				{
					return string.Format(Localizer.T("タイル画像（圧縮後 {0} バイト）を書く空き領域が見つかりませんでした。何も書き込んでいません。"), compressed.Length);
				}
				compressedLength = compressed.Length;
				patches.Add(new KeyValuePair<long, byte[]>(newAddress, compressed));
				patches.Add(new KeyValuePair<long, byte[]>(headerAddress, new byte[] { 1 }));
				patches.Add(new KeyValuePair<long, byte[]>((long)headerAddress + 4, BitConverter.GetBytes(newAddress + 0x08000000u)));
			}

			// 4. 書く範囲がすべて ROM の中にあり、互いに重ならないことを確かめる
			List<KeyValuePair<long, byte[]>> ordered = patches.OrderBy(p => p.Key).ToList();
			for (int i = 0; i < ordered.Count; i++)
			{
				if (ordered[i].Key < 0 || ordered[i].Key + ordered[i].Value.Length > this.romData.Length)
				{
					return string.Format(Localizer.T("書き込み先 0x{0:X6} が ROM の外にはみ出します。"), ordered[i].Key) + suffix;
				}
				if (i > 0 && ordered[i - 1].Key + ordered[i - 1].Value.Length > ordered[i].Key)
				{
					return string.Format(Localizer.T("書き込み先 0x{0:X6} が別の書き込み先と重なります。"), ordered[i].Key) + suffix;
				}
			}

			// 5. 元のファイルのバックアップ（ディスク上のファイルを複製する）
			if (makeBackup && !string.IsNullOrEmpty(this.loadedRomPath) && File.Exists(this.loadedRomPath))
			{
				string backup = Path.Combine(Path.GetDirectoryName(this.loadedRomPath), ((ITileImportHost)this).BackupFileName);
				try
				{
					// AdvanceMap などが元の ROM を開いたままでも複製できるよう、共有を許可して読む
					using (FileStream source = new FileStream(this.loadedRomPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
					using (FileStream target = new FileStream(backup, FileMode.CreateNew, FileAccess.Write))
					{
						source.CopyTo(target);
					}
				}
				catch (Exception ex)
				{
					return Localizer.T("バックアップを作れませんでした。") + Environment.NewLine + ex.Message + suffix;
				}
				lines.Add(Localizer.T("バックアップ: ") + Path.GetFileName(backup));
			}

			// 6. まとめて書き込む（途中で失敗したら、書く前の内容へ戻す）
			List<KeyValuePair<long, byte[]>> originals = patches.Select(p =>
			{
				byte[] saved = new byte[p.Value.Length];
				Array.Copy(this.romData, p.Key, saved, 0, saved.Length);
				return new KeyValuePair<long, byte[]>(p.Key, saved);
			}).ToList();
			try
			{
				foreach (KeyValuePair<long, byte[]> patch in patches)
				{
					Array.Copy(patch.Value, 0, this.romData, patch.Key, patch.Value.Length);
				}
			}
			catch (Exception ex)
			{
				foreach (KeyValuePair<long, byte[]> saved in originals)
				{
					Array.Copy(saved.Value, 0, this.romData, saved.Key, saved.Value.Length);
				}
				return Localizer.T("書き込みの途中で問題が起きたため、書き込む前の内容に戻しました。") + Environment.NewLine + ex.Message;
			}
			if (plan.NewTiles.Count > 0)
			{
				lines.Add(string.Format(Localizer.T("タイル画像: 新しいタイル {0} 枚を追加し、0x{1:X6} へ圧縮して書き込みました（{2} バイト）。"), plan.NewTiles.Count, newAddress, compressedLength));
			}
			if (options.Mode == TileImportEngine.PaletteMode.Overwrite)
			{
				lines.Add(string.Format(Localizer.T("パレット: 枠 {0} を上書きしました。"), options.PaletteSlot));
			}
			List<int> ids = plan.Blocks.Select(b => b.BlockId).OrderBy(i => i).ToList();
			lines.Add(string.Format(Localizer.T("ブロック: {0} 個（0x{1:X3}〜0x{2:X3}）を書き込みました。"), ids.Count, ids.First(), ids.Last()));
			if (plan.ReplacingCount > 0)
			{
				lines.Add(string.Format(Localizer.T("そのうち {0} 個は使用中のブロックの差し替えです（置かれているマップの見た目が変わります）。"), plan.ReplacingCount));
			}

			// 7. タイルセットを読み直して画面へ反映し、最初の新しいブロックを選ぶ
			MainForm.romData = this.romData;
			this.InvalidateRomLayoutCache();
			this.LoadTileset(1, this.tempFooter.Tileset1Address);
			this.LoadTileset(2, this.tempFooter.Tileset2Address);
			this.RefreshEditorView(MapEditor.ViewUpdateLevel.GraphicsOnly);
			// 同じタイルセットを使うほかのマップも見た目が変わり得るため、サムネイルは全体を作り直す
			this.ResetMapThumbnails();
			this.AssignMapThumbnailKeys(this.tvwMapSelector.Nodes);
			this.StartMapThumbnailGeneration();
			int first = ids.First();
			this.selectedBlockRect = new Rectangle(first % PaletteColumns, first / PaletteColumns, 1, 1);
			this.selectionAnchor = new Point(this.selectedBlockRect.X, this.selectedBlockRect.Y);
			this.SetEditorMode(this.tabBlock);
			this.EnsurePaletteRowVisible(this.selectedBlockRect.Y);
			// 取り込んだブロックを「最近使ったブロック」に並べて、すぐ選べるようにする
			this.recentBlockSelections.Clear();
			foreach (int id in ids.Take(RecentBlockCapacity))
			{
				this.recentBlockSelections.Add(new Rectangle(id % PaletteColumns, id / PaletteColumns, 1, 1));
			}
			this.UpdateBlockIndexLabel();
			this.pnlTilesetPalette.Invalidate();
			lines.Add(Localizer.T("取り込んだブロックは、ツール欄の「最近使ったブロック」に並べました。"));
			lines.Add("");
			lines.Add(Localizer.T("まだメモリ上の変更です。ファイルへ反映するには「ROMを保存」を押してください。"));
			report = string.Join(Environment.NewLine, lines);
			return null;
		}
	}
}
