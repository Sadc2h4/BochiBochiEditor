using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップ作成補助」の画面（MapAssistForm）を開き、今のマップの材料を渡す処理
	// 画面はマップを切り替えても開いたままにし、中身だけを入れ替える（ポインタ一覧と同じ）
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 開いている「マップ作成補助」の画面（1 つだけ開く）
		private MapAssistForm mapAssistForm;

		//-------------------------------------------------------------------------------
		// 「マップ作成補助を開く…」
		//-------------------------------------------------------------------------------
		private void btnOpenMapAssist_Click(object sender, EventArgs e)
		{
			this.ShowMapAssist();
		}

		//-------------------------------------------------------------------------------
		// 「マップ作成補助」の画面を開く処理（開いていれば前面に出して、今のマップの内容にする）
		//-------------------------------------------------------------------------------
		internal void ShowMapAssist()
		{
			if (this.romData == null)
			{
				return;
			}
			if (this.mapAssistForm != null && !this.mapAssistForm.IsDisposed)
			{
				this.mapAssistForm.SetContext(this.BuildMapAssistContext());
				if (this.mapAssistForm.WindowState == FormWindowState.Minimized)
				{
					this.mapAssistForm.WindowState = FormWindowState.Normal;
				}
				this.mapAssistForm.BringToFront();
				this.mapAssistForm.Activate();
				return;
			}
			this.mapAssistForm = new MapAssistForm(this.BuildMapAssistContext());
			this.mapAssistForm.ReloadRequested += (sender, e) => this.RefreshMapAssistIfOpen();
			// 補助の画面で保存して閉じたら、自動ペンのパーツを読み直す
			this.mapAssistForm.FormClosed += (sender, e) => this.RefreshPartBrush();
			AppIconHelper.Apply(this.mapAssistForm);
			UiTheme.Apply(this.mapAssistForm);
			this.mapAssistForm.Show(this);
		}

		//-------------------------------------------------------------------------------
		// 画面が開いていれば、今のマップの内容を渡し直す処理（マップの切り替え・確定・ROM の開き直しの後に呼ぶ）
		//-------------------------------------------------------------------------------
		private void RefreshMapAssistIfOpen()
		{
			// 「パーツで塗る」の帯も、今のマップのタイルセットのパーツにする
			this.RefreshPartBrush();
			if (this.mapAssistForm == null || this.mapAssistForm.IsDisposed)
			{
				return;
			}
			this.mapAssistForm.SetContext(this.BuildMapAssistContext());
		}

		//-------------------------------------------------------------------------------
		// 今のマップの材料（ブロック一覧の絵の写し・マップの並びの写し・タイルセットの見分け方）を作る処理
		// マップを選んでいない・タイルセットが読めていないときは null
		//-------------------------------------------------------------------------------
		private MapAssistContext BuildMapAssistContext()
		{
			if (this.romData == null || this.tempHeader == null || this.tempFooter == null || this.tempTileset1 == null || this.tempTileset2 == null || this.blockPaletteBitmap == null)
			{
				return null;
			}
			MapAssistContext context = new MapAssistContext
			{
				MapLabel = string.Format("({0}, {1}) {2}", this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this)),
				BlockSheet = new Bitmap(this.blockPaletteBitmap),
				TotalBlocks = this.totalBlocks,
				PrimarySlots = this.primaryBlockCount,
				PrimaryUsed = this.primaryUsedBlockCount,
			};
			context.AutoInputProvider = () => this.BuildMapAssistAutoInput(context);
			context.ApplyHandler = this.ApplyMapAssistResult;
			context.Primary = this.BuildAssistTilesetInfo(1, this.tempFooter.Tileset1Address, this.tempTileset1.BlockImageAddress, this.primaryUsedBlockCount);
			context.Secondary = this.BuildAssistTilesetInfo(2, this.tempFooter.Tileset2Address, this.tempTileset2.BlockImageAddress, Math.Max(0, this.totalBlocks - this.primaryBlockCount));
			if (this.mapMatrix != null)
			{
				// 編集中（まだ確定していない内容も含む）の並びを写す
				context.MapWidth = this.mapMatrix.GetLength(0);
				context.MapHeight = this.mapMatrix.GetLength(1);
				context.MapBlocks = new int[context.MapWidth * context.MapHeight];
				context.MapCollisions = new int[context.MapWidth * context.MapHeight];
				for (int y = 0; y < context.MapHeight; y++)
				{
					for (int x = 0; x < context.MapWidth; x++)
					{
						context.MapBlocks[y * context.MapWidth + x] = this.mapMatrix[x, y].BlockIndex;
						context.MapCollisions[y * context.MapWidth + x] = this.mapMatrix[x, y].Collision;
					}
				}
			}
			return context;
		}

		//-------------------------------------------------------------------------------
		// 「マップから候補を作る」の材料を集める処理
		// ROM の全マップのうち、タイルセット1 が今のマップと同じ地形データの並びを集める（タイルセット2 も同じかどうかの印つき）。
		// 今のマップは、編集中（確定していない内容も含む）の並びを使う。あわせて、ブロックごとの挙動の値を読む
		//-------------------------------------------------------------------------------
		private MapAssistAutoInput BuildMapAssistAutoInput(MapAssistContext context)
		{
			MapAssistAutoInput input = new MapAssistAutoInput();
			if (this.romData == null || this.tempHeader == null || this.tempFooter == null || this.tempTileset1 == null || this.tempTileset2 == null)
			{
				return input;
			}
			// 今のマップ
			if (context.MapWidth > 0 && context.MapHeight > 0)
			{
				input.Samples.Add(new MapAssistSample { Width = context.MapWidth, Height = context.MapHeight, Blocks = context.MapBlocks, Collisions = context.MapCollisions, SamePair = true });
			}
			// ほかのマップ（同じ地形データは 1 回だけ数える）
			System.Collections.Generic.HashSet<uint> seen = new System.Collections.Generic.HashSet<uint> { this.tempHeader.FooterAddress };
			foreach (MapEditor.MapHeader header in this.mapHeaders)
			{
				if (header.FooterAddress == 0U || !seen.Add(header.FooterAddress) || !this.IsRomRange(header.FooterAddress, 24))
				{
					continue;
				}
				MapEditor.MapFooter footer = this.ReadMapFooter((int)header.FooterAddress);
				int cells = footer.MapWidth * footer.MapHeight;
				if (footer.Tileset1Address != this.tempFooter.Tileset1Address || cells <= 0 || footer.MapDataAddress == 0U || !this.IsRomRange(footer.MapDataAddress, cells * 2))
				{
					continue;
				}
				MapAssistSample sample = new MapAssistSample
				{
					Width = footer.MapWidth,
					Height = footer.MapHeight,
					Blocks = new int[cells],
					Collisions = new int[cells],
					SamePair = footer.Tileset2Address == this.tempFooter.Tileset2Address,
				};
				for (int i = 0; i < cells; i++)
				{
					ushort value = BitConverter.ToUInt16(this.romData, (int)footer.MapDataAddress + i * 2);
					sample.Blocks[i] = value & 1023;
					sample.Collisions[i] = value >> 10;
				}
				input.Samples.Add(sample);
			}
			// ブロックごとの挙動（1 バイト目）
			int behaviorBytes = GameProfile.Current.BehaviorBytes;
			input.Behaviors = new int[context.TotalBlocks];
			for (int id = 0; id < context.TotalBlocks; id++)
			{
				input.Behaviors[id] = -1;
				if (!context.IsValidBlock(id))
				{
					continue;
				}
				bool primary = id < context.PrimarySlots;
				uint table = primary ? this.tempTileset1.BlockBehaviorAddress : this.tempTileset2.BlockBehaviorAddress;
				long address = (long)table + (long)(primary ? id : id - context.PrimarySlots) * behaviorBytes;
				if (table != 0U && address >= 0 && address < this.romData.Length)
				{
					input.Behaviors[id] = this.romData[address];
				}
			}
			return input;
		}

		//-------------------------------------------------------------------------------
		// 「サポート作成」で作った並びを、編集中のマップへ入れる処理（入れられなければ理由、入れたら null）
		// 変わったマスだけを 1 回の操作として履歴に積むので、メインの画面の「戻る」（Ctrl+Z）でまとめて元に戻せる。ROM はまだ変えない（確定は今までどおり）
		//-------------------------------------------------------------------------------
		private string ApplyMapAssistResult(int[] blocks, int[] collisions)
		{
			if (this.IsRomReadOnly)
			{
				return Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。");
			}
			if (this.mapMatrix == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			int width = this.mapMatrix.GetLength(0);
			int height = this.mapMatrix.GetLength(1);
			if (blocks == null || collisions == null || blocks.Length != width * height || collisions.Length != width * height)
			{
				return Localizer.T("マップの大きさが変わっています。「今のマップを読み直す」を押してからやり直してください。");
			}
			System.Collections.Generic.List<MapEditor.MapEditAction> actions = new System.Collections.Generic.List<MapEditor.MapEditAction>();
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int i = y * width + x;
					MapEditor.MapCell cell = this.mapMatrix[x, y];
					if (cell.BlockIndex != blocks[i])
					{
						actions.Add(new MapEditor.MapEditAction { MapX = x, MapY = y, OldBlockIndex = cell.BlockIndex, NewBlockIndex = blocks[i], OldCollision = cell.Collision, NewCollision = cell.Collision, IsBlockEdit = true });
					}
					if (cell.Collision != collisions[i])
					{
						actions.Add(new MapEditor.MapEditAction { MapX = x, MapY = y, OldBlockIndex = cell.BlockIndex, NewBlockIndex = cell.BlockIndex, OldCollision = cell.Collision, NewCollision = collisions[i], IsBlockEdit = false });
					}
				}
			}
			if (actions.Count == 0)
			{
				return null;
			}
			this.EndMapEditStroke();
			this.currentStroke = actions;
			this.EndMapEditStroke();
			this.ApplyMapEditActions(actions, true);
			return null;
		}

		//-------------------------------------------------------------------------------
		// タイルセット 1 つの見分け方を作る処理
		// 識別子は、ブロックのデータ（ブロック数 × 1 ブロックのバイト数）から作る。同じタイルセットを別の ROM へ移しても同じ値になる
		//-------------------------------------------------------------------------------
		private MapAssistTilesetInfo BuildAssistTilesetInfo(int kind, uint headerOffset, uint blockDataOffset, int blockCount)
		{
			MapAssistTilesetInfo info = new MapAssistTilesetInfo
			{
				Kind = kind,
				Game = GameProfile.Current.Code ?? string.Empty,
				HeaderOffset = headerOffset,
				BlockCount = blockCount,
				Name = string.Format("{0} TS{1} 0x{2:X6}", GameProfile.Current.Code, kind, headerOffset),
			};
			long length = (long)blockCount * MapEditor.BLOCK_DATA_SIZE;
			if (blockDataOffset != 0U && length > 0 && this.IsRomRange(blockDataOffset, (int)length))
			{
				using (SHA1 sha = SHA1.Create())
				{
					byte[] hash = sha.ComputeHash(this.romData, (int)blockDataOffset, (int)length);
					info.Fingerprint = BitConverter.ToString(hash, 0, 8).Replace("-", string.Empty).ToLowerInvariant();
				}
			}
			return info;
		}
	}
}
