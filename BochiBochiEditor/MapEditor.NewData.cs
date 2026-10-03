using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	public partial class MapEditor
	{
		// 自動で探した空き領域の手前に残す余白（直前のデータが 0xFF の終端で終わっていても、それを上書きしないため）
		private const int NewDataFreeSpaceGuard = 4;
		// 空き領域を探し始める位置（ROM を読み込んだ時点の値。-1 はまだ求めていない）
		// 末尾の空きから自動で決めるゲームでは、データを書くたびに求め直すと毎回すき間が空くため、読み込み時の値を使い続ける
		private long newDataFreeSpaceStart = -1;

		//-------------------------------------------------------------------------------
		// 「新規」タブの選択肢・上限・案内を、今のゲームの形に合わせる処理（ROM を読み込むたびに呼ぶ）
		//-------------------------------------------------------------------------------
		private void ApplyNewTabChoiceLists()
		{
			GameProfile profile = GameProfile.Current;
			this.newDataFreeSpaceStart = -1;
			int primaryPalettes = profile.PrimaryPaletteCount;
			string[] paletteTypes = new string[]
			{
				string.Format("[00]パレット0-{0}", primaryPalettes - 1),
				string.Format("[01]パレット{0}-12", primaryPalettes)
			};
			this.SetComboChoices(this.cmbTileset1PaletteType, paletteTypes);
			this.SetComboChoices(this.cmbTileset2PaletteType, paletteTypes);
			this.SetComboChoices(this.cmbNewTilesetType, new string[]
			{
				string.Format("[00]1, 128x{0}(固定) ", GetNewTilesetImageHeight(false)),
				string.Format("[01]2, 128x{0}(可変)", GetNewTilesetImageHeight(true))
			});
			this.ApplyNewMapFooterBorderState();
			string hint = Localizer.T("空欄にすると、空き領域を自動で探します。アドレスを入れた場合は、書き込む範囲が空き（0xFF）かを確かめます。");
			foreach (TextBox box in new TextBox[] { this.txtNewTilesetAddress, this.txtNewMapFooterAddress, this.txtNewMapScriptAddress, this.txtNewMapConnectionAddress, this.txtNewMapAddress })
			{
				box.PlaceholderText = Localizer.T("空欄=候補から");
				this.mapToolTip.SetToolTip(box, hint);
			}
		}

		//-------------------------------------------------------------------------------
		// 新しい地形データのボーダーの欄を、今のゲームに合わせる処理
		// ボーダーの大きさの欄が無いゲーム（エメラルド系）は 2x2 固定なので、2 を入れて変えられないようにする
		//-------------------------------------------------------------------------------
		private void ApplyNewMapFooterBorderState()
		{
			bool hasBorderSize = GameProfile.Current.HasBorderSize;
			if (!hasBorderSize)
			{
				this.SetNumericValueWithinRange(this.nudNewMapFooterBorderSizeX, 2);
				this.SetNumericValueWithinRange(this.nudNewMapFooterBorderSizeY, 2);
			}
			this.nudNewMapFooterBorderSizeX.Enabled = hasBorderSize;
			this.nudNewMapFooterBorderSizeY.Enabled = hasBorderSize;
		}

		//-------------------------------------------------------------------------------
		// プルダウンの選択肢を入れ替える処理（選んでいた位置は保ち、表示は今の言語にする）
		//-------------------------------------------------------------------------------
		private void SetComboChoices(ComboBox combo, string[] items)
		{
			int selected = combo.SelectedIndex;
			combo.BeginUpdate();
			combo.Items.Clear();
			combo.Items.AddRange(items);
			combo.EndUpdate();
			Localizer.RegisterComboItems(combo);
			if (selected >= 0 && selected < combo.Items.Count)
			{
				combo.SelectedIndex = selected;
			}
		}

		//-------------------------------------------------------------------------------
		// 新しく作るタイルセットの画像の高さ（第1はちょうどこの高さ、第2はこの高さまで）を返す処理
		//-------------------------------------------------------------------------------
		private static int GetNewTilesetImageHeight(bool secondary)
		{
			int tiles = secondary ? 1024 - GameProfile.Current.PrimaryTileCount : GameProfile.Current.PrimaryTileCount;
			return tiles / 16 * 8;
		}

		//-------------------------------------------------------------------------------
		// 新しく作るタイルセットのブロック数の上限を返す処理
		//-------------------------------------------------------------------------------
		private static int GetNewTilesetBlockLimit(bool secondary)
		{
			return secondary ? 1024 - GameProfile.Current.PrimaryBlockCount : GameProfile.Current.PrimaryBlockCount;
		}

		//-------------------------------------------------------------------------------
		// 新しいデータ用の空き領域を探す処理
		// 見つけた場所の手前に余白を残す（直前のデータの終端の 0xFF を空きと見間違えて上書きしないため）
		//-------------------------------------------------------------------------------
		private bool TryFindFreeSpaceForNewData(int length, ref uint address)
		{
			uint found = 0;
			if (this.romData == null || length <= 0)
			{
				return false;
			}
			if (this.newDataFreeSpaceStart < 0)
			{
				this.newDataFreeSpaceStart = this.NormalizeRomAddress((uint)MapEditor.ReadIniOffset("FREE_SPACE_FINDER_OFFSET", MapEditor.DetectFreeSpaceStart, null));
			}
			if (!this.TryFindAlignedFreeSpace(this.romData, (uint)this.newDataFreeSpaceStart, checked(length + NewDataFreeSpaceGuard), ref found))
			{
				return false;
			}
			address = found + NewDataFreeSpaceGuard;
			return true;
		}

		//-------------------------------------------------------------------------------
		// 空き領域の候補 1 件（書き込み先・その空きの範囲・空きの直前のバイト列）
		//-------------------------------------------------------------------------------
		internal sealed class FreeSpaceCandidate
		{
			public uint Address;
			public uint RunStart;
			public int RunLength;
			public byte[] BytesBefore;
		}

		//-------------------------------------------------------------------------------
		// 指定の大きさのデータを置ける空き領域（0xFF の並び）を、探し始める位置から順に最大 maxCount 件集める処理
		// 書き込み先は、空きの先頭から余白を空けて 4 バイト境界にそろえた位置（自動で探すときと同じ決め方）
		//-------------------------------------------------------------------------------
		internal List<FreeSpaceCandidate> FindFreeSpaceCandidates(int length, int maxCount)
		{
			List<FreeSpaceCandidate> result = new List<FreeSpaceCandidate>();
			if (this.romData == null || length <= 0 || maxCount <= 0)
			{
				return result;
			}
			if (this.newDataFreeSpaceStart < 0)
			{
				this.newDataFreeSpaceStart = this.NormalizeRomAddress((uint)MapEditor.ReadIniOffset("FREE_SPACE_FINDER_OFFSET", MapEditor.DetectFreeSpaceStart, null));
			}
			byte[] rom = this.romData;
			long i = this.newDataFreeSpaceStart;
			while (i < rom.Length && result.Count < maxCount)
			{
				if (rom[i] != 0xFF)
				{
					i++;
					continue;
				}
				long start = i;
				while (i < rom.Length && rom[i] == 0xFF)
				{
					i++;
				}
				long address = (start + NewDataFreeSpaceGuard + 3) & ~3L;
				if (address + length <= i)
				{
					byte[] before = new byte[(int)Math.Min(8, start)];
					Array.Copy(rom, start - before.Length, before, 0, before.Length);
					result.Add(new FreeSpaceCandidate { Address = (uint)address, RunStart = (uint)start, RunLength = (int)(i - start), BytesBefore = before });
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 新しいデータの書き込み先を決める処理（問題が無ければ null、あれば理由を返す）
		// 自動、または入力が空欄なら空き領域を探す。アドレスの指定があれば、書く範囲がすべて ROM の中にあるかを確かめる
		// occupied には、指定された範囲に空き（0xFF）でないデータがあるかを返す
		//-------------------------------------------------------------------------------
		internal string ResolveNewDataAddress(string text, bool forceAuto, int length, out uint address, out bool occupied)
		{
			address = 0;
			occupied = false;
			if (this.romData == null)
			{
				return Localizer.T("ROM が読み込まれていません。");
			}
			if (length <= 0)
			{
				return Localizer.T("書き込む内容がありません。");
			}
			if (forceAuto || string.IsNullOrWhiteSpace(text))
			{
				if (!this.TryFindFreeSpaceForNewData(length, ref address))
				{
					return string.Format(Localizer.T("空き領域が見つかりませんでした。\r\n必要バイト数: {0}"), length);
				}
				return null;
			}
			if (!this.TryParseHex(text.Trim(), ref address))
			{
				return Localizer.T("アドレスは16進数で入力してください。");
			}
			if (!this.IsRomRange(address, length))
			{
				return string.Format(Localizer.T("書き込む範囲（0x{0:X6} から {1} バイト）が ROM の外にはみ出します。"), address, length);
			}
			for (int i = 0; i < length; i++)
			{
				if (this.romData[address + i] != 0xFF)
				{
					occupied = true;
					break;
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 「マップ名を変更」の「変更前のマップ名」を、今開いているマップの名前（「マップの設定」タブで選んでいる名前）に合わせる処理
		// 2 つの一覧は同じ順番（マップ名の表の順）で作っているので、同じ番号を選ぶ。マップが無ければ先頭
		//-------------------------------------------------------------------------------
		private void SyncNewMapNameFromCurrentMap()
		{
			int index = this.tempHeader != null ? this.cmbMapNameId.SelectedIndex : -1;
			if (index < 0 || index >= this.cmbNewMapName.Items.Count)
			{
				index = this.cmbNewMapName.Items.Count > 0 ? 0 : -1;
			}
			if (this.cmbNewMapName.SelectedIndex != index)
			{
				this.cmbNewMapName.SelectedIndex = index;
			}
		}

		// 空き領域の候補を一覧に出す最大の件数
		private const int FreeSpaceCandidateCount = 50;

		// 試験用: 設定されていれば、候補の一覧を出さずにこの関数が返す番号を選ぶ（-1 はキャンセル）
		internal static Func<IList<FreeSpaceCandidate>, int> TestPickAnswer;

		//-------------------------------------------------------------------------------
		// 空き領域の候補を一覧で出し、書き込み先を選んでもらう処理（選ばれたら true）
		// 候補が無ければ案内して false。purpose は一覧に出す「何を書くか」の説明
		//-------------------------------------------------------------------------------
		internal bool PickFreeSpaceAddress(string purpose, int length, IWin32Window owner, out uint address)
		{
			address = 0;
			List<FreeSpaceCandidate> candidates = this.FindFreeSpaceCandidates(length, FreeSpaceCandidateCount);
			if (candidates.Count == 0)
			{
				string message = string.Format(Localizer.T("空き領域が見つかりませんでした。\r\n必要バイト数: {0}"), length);
				if (TestPickAnswer != null)
				{
					Console.WriteLine("  [dialog] " + message);
				}
				else
				{
					MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				return false;
			}
			FreeSpaceCandidate picked = null;
			if (TestPickAnswer != null)
			{
				int index = TestPickAnswer(candidates);
				picked = index >= 0 && index < candidates.Count ? candidates[index] : null;
			}
			else
			{
				using (FreeSpacePickerForm picker = new FreeSpacePickerForm(purpose, length, candidates))
				{
					AppIconHelper.Apply(picker);
					UiTheme.Apply(picker);
					if ((owner != null ? picker.ShowDialog(owner) : picker.ShowDialog()) == DialogResult.OK)
					{
						picked = picker.SelectedCandidate;
					}
				}
			}
			if (picked == null)
			{
				return false;
			}
			address = picked.Address;
			return true;
		}

		//-------------------------------------------------------------------------------
		// 「新規」タブの入力欄ごとの、一覧に出す「何を書くか」の説明を返す処理
		//-------------------------------------------------------------------------------
		private string GetNewDataPurpose(TextBox txtBox)
		{
			if (txtBox == this.txtNewTilesetAddress) return Localizer.T("タイルセット");
			if (txtBox == this.txtNewMapFooterAddress) return Localizer.T("マップフッター");
			if (txtBox == this.txtNewMapScriptAddress) return Localizer.T("マップスクリプト");
			if (txtBox == this.txtNewMapConnectionAddress) return Localizer.T("接続マップ");
			if (txtBox == this.txtNewMapAddress) return Localizer.T("マップ名");
			if (txtBox == this.txtNewEventAddress) return Localizer.T("イベント");
			return Localizer.T("新しいデータ");
		}

		//-------------------------------------------------------------------------------
		// 「新規」タブで書き込み先を決める処理（画面用）
		// 入力欄が空欄（イベントは「空き領域の一覧から選ぶ」がオン）なら、空き領域の候補を一覧で出して選んでもらう
		// 問題があれば案内し、空きでない範囲へ書くときは上書きしてよいかを確かめる。決まったアドレスは入力欄へ戻す
		//-------------------------------------------------------------------------------
		private bool ResolveNewDataAddressOnNewTab(TextBox txtBox, bool forceAuto, int length, ref uint address)
		{
			string text = txtBox.Text;
			if (forceAuto || string.IsNullOrWhiteSpace(text))
			{
				uint picked;
				if (!this.PickFreeSpaceAddress(this.GetNewDataPurpose(txtBox), length, this, out picked))
				{
					return false;
				}
				text = string.Format("{0:X8}", 0x08000000U + picked);
			}
			bool occupied;
			string error = this.ResolveNewDataAddress(text, false, length, out address, out occupied);
			if (error != null)
			{
				MessageBox.Show(error, "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
			if (occupied)
			{
				DialogResult answer = MessageBox.Show(string.Format(Localizer.T("書き込み先 0x{0:X6} から {1} バイトの範囲に、空き（0xFF）でないデータがあります。\r\nこのまま上書きしますか？"), address, length), "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				if (answer != DialogResult.Yes)
				{
					return false;
				}
			}
			txtBox.Text = string.Format("{0:X8}", address);
			return true;
		}

		//-------------------------------------------------------------------------------
		// 新しいデータを書いた後の共通の後始末（配置の記録を捨て、ほかの画面が持つ ROM も合わせる）
		//-------------------------------------------------------------------------------
		private void AfterNewDataWritten()
		{
			this.InvalidateRomLayoutCache();
			MainForm.romData = this.romData;
		}

		//-------------------------------------------------------------------------------
		// 生成処理でデータを書き込む処理（イベント・マップスクリプト・接続で共通。成功なら null）
		//-------------------------------------------------------------------------------
		internal string WriteNewData(NewDataGenerator.INewDataGenerator generator, uint address)
		{
			if (this.romData == null || generator == null)
			{
				return Localizer.T("ROM が読み込まれていません。");
			}
			if (!generator.GenerateData(this.romData, address))
			{
				return Localizer.T("書き込む範囲が ROM の外にはみ出します。ROM は変更していません。");
			}
			this.AfterNewDataWritten();
			return null;
		}

		//-------------------------------------------------------------------------------
		// 指定位置がタイルセットの見出しらしいかを確かめる処理（圧縮・種類が 0 か 1、画像・パレット・ブロックが ROM 内のポインタ）
		//-------------------------------------------------------------------------------
		private bool IsTilesetHeaderAt(long offset)
		{
			if (this.romData == null || offset < 0 || offset + MapEditor.TILESET_HEADER_SIZE > this.romData.Length)
			{
				return false;
			}
			int start = (int)offset;
			if (this.romData[start] > 1 || this.romData[start + 1] > 1)
			{
				return false;
			}
			foreach (int field in new int[] { 4, 8, 12 })
			{
				if (!this.IsRomPointer(BitConverter.ToUInt32(this.romData, start + field)))
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// タイルセット番号から見出しの位置を返す処理
		//-------------------------------------------------------------------------------
		private static long GetTilesetHeaderOffset(int tilesetIndex)
		{
			return (long)MapEditor.TILESET_INDEX_START_OFFSET + (long)tilesetIndex * MapEditor.TILESET_HEADER_SIZE;
		}

		//-------------------------------------------------------------------------------
		// 今のゲームの形に合わせた、地形データ（フッター）の生成処理を作る処理
		//-------------------------------------------------------------------------------
		internal NewDataGenerator.MapFooterGenerator CreateMapFooterGenerator(byte mapWidth, byte mapHeight, byte borderWidth, byte borderHeight, int tileset1Index, int tileset2Index)
		{
			bool hasBorderSize = GameProfile.Current.HasBorderSize;
			return new NewDataGenerator.MapFooterGenerator
			{
				MapWidth = mapWidth,
				MapHeight = mapHeight,
				BorderWidth = hasBorderSize ? borderWidth : (byte)2,
				BorderHeight = hasBorderSize ? borderHeight : (byte)2,
				Tileset1Index = tileset1Index,
				Tileset2Index = tileset2Index,
				TilesetIndexStartOffset = MapEditor.TILESET_INDEX_START_OFFSET,
				HeaderSize = hasBorderSize ? 28 : 24,
				HasBorderSize = hasBorderSize
			};
		}

		//-------------------------------------------------------------------------------
		// 地形データ（フッター）を書き込む前に、内容を確かめる処理（問題が無ければ null）
		//-------------------------------------------------------------------------------
		internal string CheckNewMapFooter(NewDataGenerator.MapFooterGenerator generator)
		{
			if (generator.MapWidth == 0 || generator.MapHeight == 0)
			{
				return Localizer.T("マップの幅と高さは 1 以上にしてください。");
			}
			foreach (int index in new int[] { generator.Tileset1Index, generator.Tileset2Index })
			{
				if (!this.IsTilesetHeaderAt(GetTilesetHeaderOffset(index)))
				{
					return string.Format(Localizer.T("タイルセット番号 {0} の位置に、タイルセットの見出しがありません。"), index);
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 地形データ（フッター）を書き込む処理（成功なら null）
		//-------------------------------------------------------------------------------
		internal string WriteNewMapFooter(NewDataGenerator.MapFooterGenerator generator, uint address)
		{
			string problem = this.CheckNewMapFooter(generator);
			return problem ?? this.WriteNewData(generator, address);
		}

		//-------------------------------------------------------------------------------
		// タイルセットのパレット 1 本（32 バイト）を書き換える処理（成功なら null）
		//-------------------------------------------------------------------------------
		internal string WriteNewPalette(int tilesetIndex, int paletteIndex, byte[] paletteBytes)
		{
			if (this.romData == null)
			{
				return Localizer.T("ROM が読み込まれていません。");
			}
			if (paletteBytes == null || paletteBytes.Length < 32)
			{
				return Localizer.T("パレットのデータが足りません。");
			}
			if (paletteIndex < 0 || paletteIndex > 12)
			{
				return Localizer.T("パレット番号は 0〜12 で指定してください。");
			}
			long header = GetTilesetHeaderOffset(tilesetIndex);
			if (!this.IsTilesetHeaderAt(header))
			{
				return string.Format(Localizer.T("タイルセット番号 {0} の位置に、タイルセットの見出しがありません。"), tilesetIndex);
			}
			long palette = (long)BitConverter.ToUInt32(this.romData, (int)header + 8) - 0x08000000L;
			long target = palette + (long)paletteIndex * 32;
			if (target < 0 || target + 32 > this.romData.Length)
			{
				return Localizer.T("パレットの書き込み先が ROM の外にはみ出します。");
			}
			Array.Copy(paletteBytes, 0, this.romData, (int)target, 32);
			this.AfterNewDataWritten();
			return null;
		}

		//-------------------------------------------------------------------------------
		// マップ名を、ゲームの文字コードのバイト列（終端つき）にする処理
		//-------------------------------------------------------------------------------
		internal byte[] EncodeNewMapName(string name)
		{
			return TextConverter.PokemonStringToBytes(name, GameProfile.Current.MapNameMaxLength);
		}

		//-------------------------------------------------------------------------------
		// マップ名の表の、指定番号の「名前へのポインタ」の位置を返す処理（表の外なら -1）
		//-------------------------------------------------------------------------------
		private long GetMapNamePointerOffset(int index)
		{
			if (this.romData == null || index < 0 || index >= MapEditor.MAP_NAME_COUNT)
			{
				return -1;
			}
			long entry = (long)MapEditor.MAP_NAME_TABLE_OFFSET + index * (long)MapEditor.MAP_NAME_ENTRY_SIZE + MapEditor.MAP_NAME_POINTER_OFFSET;
			return entry >= 0 && entry + 4 <= this.romData.Length ? entry : -1;
		}

		//-------------------------------------------------------------------------------
		// マップ名の文字列を書き、マップ名の表のポインタを付け替える処理（成功なら null）
		//-------------------------------------------------------------------------------
		internal string WriteNewMapName(int index, byte[] nameBytes, uint address)
		{
			long pointerOffset = this.GetMapNamePointerOffset(index);
			if (pointerOffset < 0)
			{
				return Localizer.T("置き換えるマップ名を選んでください。");
			}
			if (nameBytes == null || nameBytes.Length == 0)
			{
				return Localizer.T("新しいマップ名を入力してください。");
			}
			if (!this.IsRomRange(address, nameBytes.Length))
			{
				return Localizer.T("書き込む範囲が ROM の外にはみ出します。ROM は変更していません。");
			}
			// 名前の文字列が、付け替えるポインタ自身に重ならないこと
			if (address < pointerOffset + 4 && pointerOffset < (long)address + nameBytes.Length)
			{
				return Localizer.T("書き込み先がマップ名の表と重なります。ROM は変更していません。");
			}
			Array.Copy(nameBytes, 0, this.romData, (int)address, nameBytes.Length);
			Array.Copy(BitConverter.GetBytes(address + 0x08000000u), 0, this.romData, (int)pointerOffset, 4);
			this.AfterNewDataWritten();
			return null;
		}

		//-------------------------------------------------------------------------------
		// 新しく作るタイルセットの画像の大きさを確かめる処理（問題が無ければ null）
		//-------------------------------------------------------------------------------
		internal string CheckNewTilesetImageSize(int width, int height, bool secondary)
		{
			int limit = GetNewTilesetImageHeight(secondary);
			if (!secondary && (width != 128 || height != limit))
			{
				return string.Format(Localizer.T("タイルセット1の画像サイズは128x{0}である必要があります。"), limit);
			}
			if (secondary && (width != 128 || height > limit || height <= 0 || height % 8 != 0))
			{
				return string.Format(Localizer.T("タイルセット2の画像サイズは128x{0}以下（高さは 8 の倍数）である必要があります。"), limit);
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 今のゲームの形に合わせた、タイルセットの生成処理を作る処理
		// imageBytes は ROM に書く形（圧縮するなら圧縮済み）のタイル画像
		//-------------------------------------------------------------------------------
		internal NewDataGenerator.TilesetGenerator CreateTilesetGenerator(byte[] imageBytes, bool secondary, bool compressed, int blockCount)
		{
			GameProfile profile = GameProfile.Current;
			return new NewDataGenerator.TilesetGenerator
			{
				ImageBytes = imageBytes,
				PaletteType = secondary ? (byte)1 : (byte)0,
				CompressType = compressed ? (byte)1 : (byte)0,
				BlockCount = blockCount,
				TilesetIndexStartOffset = MapEditor.TILESET_INDEX_START_OFFSET,
				BlockBytes = MapEditor.BLOCK_DATA_SIZE,
				BehaviorBytes = profile.BehaviorBytes,
				BehaviorOffset = profile.TilesetBehaviorOffset,
				CallbackOffset = profile.TilesetCallbackOffset
			};
		}

		//-------------------------------------------------------------------------------
		// 新しく作るタイルセットが使う最大のバイト数（見出しの位置合わせの余りを含む）を返す処理
		//-------------------------------------------------------------------------------
		internal int GetNewTilesetMaxLength(NewDataGenerator.TilesetGenerator generator)
		{
			checked
			{
				return MapEditor.TILESET_HEADER_SIZE * 2 + 512 + generator.BlockCount * (generator.BlockBytes + generator.BehaviorBytes) + (generator.ImageBytes == null ? 0 : generator.ImageBytes.Length);
			}
		}

		//-------------------------------------------------------------------------------
		// タイルセットを書き込む処理（成功なら null）
		// 第2タイルセットのブロック数を設定ファイルで持つゲーム（FR）だけ、設定ファイルへブロック数を書き足す
		//-------------------------------------------------------------------------------
		internal string WriteNewTileset(NewDataGenerator.TilesetGenerator generator, uint address)
		{
			if (this.romData == null || generator == null || generator.ImageBytes == null)
			{
				return Localizer.T("ROM が読み込まれていません。");
			}
			bool secondary = generator.PaletteType == 1;
			int blockLimit = GetNewTilesetBlockLimit(secondary);
			if (generator.BlockCount < 1 || generator.BlockCount > blockLimit)
			{
				return string.Format(Localizer.T("ブロック数は 1〜{0} で指定してください。"), blockLimit);
			}
			if (address < MapEditor.TILESET_INDEX_START_OFFSET)
			{
				return Localizer.T("タイルセットは、タイルセットの表の先頭より後ろに作ってください。");
			}
			if (!generator.GenerateData(this.romData, address))
			{
				return Localizer.T("書き込む範囲が ROM の外にはみ出します。ROM は変更していません。");
			}
			if (GameProfile.Current.UseTileset2BlockLimitIni && secondary && generator.BlockCount < blockLimit && !this.tileset2BlockLimits.ContainsKey(generator.OutTilesetIndex))
			{
				string path = this.FindWritableAssetPath("ini", "Tileset2BlockLimit.ini");
				using (StreamWriter writer = new StreamWriter(path, true, Encoding.UTF8))
				{
					writer.WriteLine(string.Format("{0}={1}", generator.OutTilesetIndex, generator.BlockCount));
				}
				this.tileset2BlockLimits[generator.OutTilesetIndex] = generator.BlockCount;
			}
			this.AfterNewDataWritten();
			return null;
		}
	}
}
