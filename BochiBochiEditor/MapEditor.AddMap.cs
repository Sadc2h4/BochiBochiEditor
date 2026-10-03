using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// バンクにマップを 1 つ追加する処理（「データ作成」タブの「マップを追加…」）。新しいバンクを作って、その最初のマップとして追加することもできる
	// 今選んでいるマップをもとに、次のものを空き領域に新しく作る:
	//   見出し（もとのマップの見出しを写し、ポインタと地形データの番号だけ差し替える）、
	//   地形データ（もとのマップの形・タイルセット・並び・ボーダーを写す）、空のイベント、空のマップスクリプト（接続は無し）
	// あわせて、バンクのマップ一覧と地形データの表に 1 件ずつ足す。後ろに空きが無ければ、一覧・表を空き領域へ移して参照を付け替える
	// 一覧・表の後ろには、終わりの印として必ず 0xFF の 1 語を残す（件数の自動判定がそこで止まるように）
	// 新しいバンクを作るときは、バンクの表（バンクごとのマップ一覧を指すポインタの並び）にも 1 件足す（後ろに空きが無ければ、同じように移す）
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 一覧・表を移すときに、後ろへ足せるように空けておく件数
		private const int RelocatedListSpareEntries = 16;

		//-------------------------------------------------------------------------------
		// ポインタの並び（一覧・表）の、count 件目とその次が空き（0xFF）かを返す処理（その場に 1 件足せるか）
		//-------------------------------------------------------------------------------
		private bool CanAppendInPlace(uint listOffset, int count)
		{
			long slot = (long)listOffset + count * 4L;
			if (!this.IsRomRange((uint)slot, 8))
			{
				return false;
			}
			for (int i = 0; i < 8; i++)
			{
				if (this.romData[slot + i] != 0xFF)
				{
					return false;
				}
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// ROM の中から、指定の場所を指している 4 バイト境界のポインタの位置をすべて集める処理
		//-------------------------------------------------------------------------------
		private List<int> FindAlignedReferences(uint offset)
		{
			uint pointer = 0x08000000U + offset;
			List<int> result = new List<int>();
			for (int i = 0; i + 4 <= this.romData.Length; i += 4)
			{
				if (BitConverter.ToUInt32(this.romData, i) == pointer)
				{
					result.Add(i);
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// バンクの数を数える処理（バンクの表の先頭から、マップの見出しを指す一覧へのポインタが続く数。読み込みのときの数え方と同じ）
		//-------------------------------------------------------------------------------
		internal int CountBanks()
		{
			int count = 0;
			if (this.romData == null)
			{
				return 0;
			}
			while (count < 256)
			{
				long entry = (long)MapEditor.MAP_BANK_TABLE_OFFSET + count * 4L;
				if (!this.IsRomRange((uint)entry, 4))
				{
					break;
				}
				uint pointer = BitConverter.ToUInt32(this.romData, (int)entry);
				if (!this.IsRomPointer(pointer) || !this.IsRomRange(pointer - 0x08000000U, 4) || !this.LooksLikeMapHeaderPointer(BitConverter.ToUInt32(this.romData, (int)(pointer - 0x08000000U))))
				{
					break;
				}
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 今選んでいるマップをもとに、バンク bank の最後にマップを 1 つ追加する処理（成功なら null、失敗なら理由）
		// bank が今のバンクの数と同じなら、新しいバンクを作って、その 0 番のマップとして追加する
		// newNumber には追加したマップの番号を返す。書き込み先は空き領域の一覧から 1 回だけ選んでもらい、その中に全部を並べる
		//-------------------------------------------------------------------------------
		internal string AddMapToBank(int bank, out int newNumber)
		{
			newNumber = -1;
			if (this.IsRomReadOnly)
			{
				return Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。");
			}
			if (this.romData == null || this.tempHeader == null || this.tempHeader.FooterAddress == 0 || this.chkTerrainIdMode.Checked)
			{
				return Localizer.T("先に、もとにするマップを左の一覧で選んでください（「マップ地形ID」の一覧ではなく、番号順かマップ名の一覧で）。");
			}
			if (this.hasUnsavedChanges)
			{
				return Localizer.T("今のマップに確定していない変更があります。先に「編集中のMAPを確定」を押してください。");
			}
			// バンクのマップ一覧と、追加する番号（新しいバンクなら、一覧はこれから作る）
			int bankCount = this.CountBanks();
			bool newBank = bank == bankCount && bankCount > 0;
			uint bankTable = (uint)MapEditor.MAP_BANK_TABLE_OFFSET;
			if (newBank && bankCount >= 255)
			{
				return Localizer.T("これ以上バンクを足せません（最大 255 個）。");
			}
			long bankEntry = (long)MapEditor.MAP_BANK_TABLE_OFFSET + bank * 4L;
			if (!newBank && (bank < 0 || !this.IsRomRange((uint)bankEntry, 4) || !this.IsRomPointer(BitConverter.ToUInt32(this.romData, (int)bankEntry))))
			{
				return string.Format(Localizer.T("バンク {0} が見つかりません。"), bank);
			}
			uint listOffset = newBank ? 0U : BitConverter.ToUInt32(this.romData, (int)bankEntry) - 0x08000000U;
			List<MapHeader> inBank = this.mapHeaders.Where(h => h.Bank == bank).ToList();
			int count = newBank || inBank.Count == 0 ? 0 : inBank.Max(h => h.Number) + 1;
			if (count >= 255)
			{
				return string.Format(Localizer.T("バンク {0} にはこれ以上マップを足せません（最大 255 個）。"), bank);
			}
			// もとにするマップの見出し（28 バイト）と地形データ
			long templateEntry = (long)BitConverter.ToUInt32(this.romData, MapEditor.MAP_BANK_TABLE_OFFSET + this.tempHeader.Bank * 4) - 0x08000000L + this.tempHeader.Number * 4L;
			uint templatePointer = BitConverter.ToUInt32(this.romData, (int)templateEntry);
			if (!this.IsRomPointer(templatePointer) || !this.IsRomRange(templatePointer - 0x08000000U, 28))
			{
				return Localizer.T("もとにするマップの見出しが読めません。");
			}
			byte[] headerBytes = new byte[28];
			Array.Copy(this.romData, templatePointer - 0x08000000U, headerBytes, 0, 28);
			MapFooter templateFooter = this.ReadMapFooter((int)this.tempHeader.FooterAddress);
			int mapBytes = templateFooter.MapWidth * templateFooter.MapHeight * 2;
			int borderBytes = templateFooter.BorderWidth * templateFooter.BorderHeight * 2;
			if (!this.IsRomRange(templateFooter.MapDataAddress, mapBytes) || !this.IsRomRange(templateFooter.BorderDataAddress, borderBytes))
			{
				return Localizer.T("もとにするマップの地形データが読めません。");
			}
			// 地形データの表
			int layoutCount = MapEditor.MAP_TERRAIN_ID_COUNT;
			uint layoutTable = (uint)MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET;
			if (layoutCount <= 0 || layoutCount >= 0xFFFF || MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET <= 0)
			{
				return Localizer.T("地形データの表が見つからないため、マップを追加できません。");
			}
			bool layoutInPlace = this.CanAppendInPlace(layoutTable, layoutCount);
			// 新しいバンクの一覧は空き領域に新しく作る（付け替える参照は無い）
			bool listInPlace = !newBank && this.CanAppendInPlace(listOffset, count);
			List<int> layoutRefs = layoutInPlace ? new List<int>() : this.FindAlignedReferences(layoutTable);
			List<int> listRefs = listInPlace || newBank ? new List<int>() : this.FindAlignedReferences(listOffset);
			if (!layoutInPlace && layoutRefs.Count == 0 || !listInPlace && !newBank && listRefs.Count == 0)
			{
				return Localizer.T("地形データの表、またはバンクのマップ一覧を指しているポインタが見つからないため、移せません。何も変更していません。");
			}
			// 新しいバンクを作るとき: バンクの表に 1 件足せるか（足せなければ表を移す）
			bool bankTableInPlace = !newBank || this.CanAppendInPlace(bankTable, bankCount);
			List<int> bankTableRefs = bankTableInPlace ? new List<int>() : this.FindAlignedReferences(bankTable);
			if (!bankTableInPlace && bankTableRefs.Count == 0)
			{
				return Localizer.T("バンクの表を指しているポインタが見つからないため、移せません。何も変更していません。");
			}

			// 書く物の大きさ（4 バイト境界にそろえる）と並べ方: [地形データの表][バンクの表][バンクの一覧][見出し][イベント][マップスクリプト][地形データ]
			int layoutTableBytes = layoutInPlace ? 0 : (layoutCount + 1 + RelocatedListSpareEntries) * 4;
			int bankTableBytes = bankTableInPlace ? 0 : (bankCount + 1 + RelocatedListSpareEntries) * 4;
			int listBytes = listInPlace ? 0 : (count + 1 + RelocatedListSpareEntries) * 4;
			NewDataGenerator.EventGenerator events = new NewDataGenerator.EventGenerator { PersonCount = 0, WarpCount = 0, TrapCount = 0, SignCount = 0 };
			int eventBytes = Align4(this.CalculateNewEventDataLength(events));
			NewDataGenerator.MapFooterGenerator footer = this.CreateMapFooterGenerator(templateFooter.MapWidth, templateFooter.MapHeight, templateFooter.BorderWidth, templateFooter.BorderHeight, 0, 0);
			int footerBytes = Align4(footer.CalculateLength());
			int total = layoutTableBytes + bankTableBytes + listBytes + 28 + eventBytes + 4 + footerBytes;

			string listNote = newBank
				? string.Format(Localizer.T("バンク {0} を新しく作ります。\n"), bank) + (bankTableInPlace ? "" : string.Format(Localizer.T("バンクの表の後ろに空きが無いため、表を空き領域へ移します（参照 {0} か所を付け替え）。\n"), bankTableRefs.Count))
				: (listInPlace ? "" : string.Format(Localizer.T("バンク {0} のマップ一覧の後ろに空きが無いため、一覧を空き領域へ移します（参照 {1} か所を付け替え）。\n"), bank, listRefs.Count));
			string question = string.Format(Localizer.T("バンク {0} に、マップ ({0}, {1}) を追加します。\nもとにするのは ({2}, {3}) {4} です（見出しの設定・地形データの形・タイルセットを写し、イベントとマップスクリプトは空、接続は無しにします）。\n{5}{6}追加すると ROM（メモリ上）にすぐ書き込みます。ファイルへは「ROMを保存」で書き出します。よろしいですか？"),
				bank, count, this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this),
				layoutInPlace ? "" : string.Format(Localizer.T("地形データの表の後ろに空きが無いため、表を空き領域へ移します（参照 {0} か所を付け替え）。\n"), layoutRefs.Count),
				listNote);
			if (MessageBox.Show(this, question, Localizer.T("マップを追加"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return Localizer.T("キャンセルしました。");
			}
			uint region;
			if (!this.PickFreeSpaceAddress(Localizer.T("追加するマップのデータ"), total, this, out region))
			{
				return Localizer.T("書き込み先を選ばなかったため、何も書き込んでいません。");
			}
			uint checkedRegion;
			bool occupied;
			string error = this.ResolveNewDataAddress(string.Format("{0:X8}", 0x08000000U + region), false, total, out checkedRegion, out occupied);
			if (error != null)
			{
				return error;
			}
			if (occupied)
			{
				return Localizer.T("選んだ場所に空きでないデータがあるため、何も書き込んでいません。");
			}

			uint cursor = region;
			// 地形データの表（移すときは、今の件数分を写して、その後ろは空き 0xFF のまま）
			uint newLayoutTable = layoutTable;
			if (!layoutInPlace)
			{
				newLayoutTable = cursor;
				Array.Copy(this.romData, layoutTable, this.romData, newLayoutTable, layoutCount * 4);
				cursor += (uint)layoutTableBytes;
			}
			// バンクの表（新しいバンクを作るときだけ。移すときは、今の件数分を写す）
			uint newBankTable = bankTable;
			if (!bankTableInPlace)
			{
				newBankTable = cursor;
				Array.Copy(this.romData, bankTable, this.romData, newBankTable, bankCount * 4);
				cursor += (uint)bankTableBytes;
			}
			// バンクのマップ一覧（新しいバンクなら、空の一覧を新しく置く）
			uint newList = listOffset;
			if (!listInPlace)
			{
				newList = cursor;
				if (!newBank)
				{
					Array.Copy(this.romData, listOffset, this.romData, newList, count * 4);
				}
				cursor += (uint)listBytes;
			}
			uint headerAt = cursor;
			cursor += 28;
			uint eventsAt = cursor;
			cursor += (uint)eventBytes;
			uint scriptAt = cursor;
			cursor += 4;
			uint footerAt = cursor;
			// 空のイベント・空のマップスクリプト（終わりの 0 だけ）・地形データ
			error = this.WriteNewData(events, eventsAt);
			if (error == null)
			{
				Array.Clear(this.romData, (int)scriptAt, 4);
				error = this.WriteNewData(footer, footerAt);
			}
			if (error != null)
			{
				return error;
			}
			MapFooter created = this.ReadMapFooter((int)footer.HeaderAddress);
			Array.Copy(this.romData, templateFooter.MapDataAddress, this.romData, created.MapDataAddress, mapBytes);
			Array.Copy(this.romData, templateFooter.BorderDataAddress, this.romData, created.BorderDataAddress, borderBytes);
			// タイルセットはもとのマップと同じ見出しを指す（番号にそろっていない見出しでも使えるよう、ポインタをそのまま写す）
			Array.Copy(this.romData, this.tempHeader.FooterAddress + 16, this.romData, footer.HeaderAddress + 16, 8);
			// 地形データの表に 1 件足す（番号は 1 から数える）
			int newLayoutId = layoutCount + 1;
			WritePointerAt(this.romData, newLayoutTable + (uint)layoutCount * 4, footer.HeaderAddress);
			// 見出し: もとのマップを写し、地形データ・イベント・マップスクリプト・接続・地形データの番号を差し替える
			Array.Copy(headerBytes, 0, this.romData, headerAt, 28);
			WritePointerAt(this.romData, headerAt + 0, footer.HeaderAddress);
			WritePointerAt(this.romData, headerAt + 4, events.HeaderAddress);
			WritePointerAt(this.romData, headerAt + 8, scriptAt);
			Array.Clear(this.romData, (int)headerAt + 12, 4);
			this.romData[headerAt + 18] = (byte)(newLayoutId & 0xFF);
			this.romData[headerAt + 19] = (byte)(newLayoutId >> 8);
			// バンクのマップ一覧に見出しを足す
			WritePointerAt(this.romData, newList + (uint)count * 4, headerAt);
			// 移した表・一覧の参照を付け替える
			foreach (int reference in layoutRefs)
			{
				WritePointerAt(this.romData, (uint)reference, newLayoutTable);
			}
			foreach (int reference in listRefs)
			{
				WritePointerAt(this.romData, (uint)reference, newList);
			}
			// 新しいバンク: バンクの表に一覧を足し、表を移したなら参照を付け替える
			if (newBank)
			{
				WritePointerAt(this.romData, newBankTable + (uint)bankCount * 4, newList);
				foreach (int reference in bankTableRefs)
				{
					WritePointerAt(this.romData, (uint)reference, newBankTable);
				}
				MapEditor.MAP_BANK_TABLE_OFFSET = (int)newBankTable;
			}
			MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET = (int)newLayoutTable;
			MapEditor.MAP_TERRAIN_ID_COUNT = newLayoutId;
			// 「マップの設定」の地形データ番号の欄の上限も広げる（狭いままだと、新しい番号が上限に切られて確定で別の番号が書かれるため）
			this.nudTerrainId.Maximum = new decimal(MapEditor.MAP_TERRAIN_ID_COUNT);
			this.AfterNewDataWritten();
			newNumber = count;
			return null;
		}

		//-------------------------------------------------------------------------------
		// 値を 4 の倍数に切り上げる処理
		//-------------------------------------------------------------------------------
		private static int Align4(int value)
		{
			return (value + 3) & ~3;
		}

		//-------------------------------------------------------------------------------
		// ROM の offset に、target を指すポインタ（0x08000000 を足した値）を書く処理
		//-------------------------------------------------------------------------------
		private static void WritePointerAt(byte[] rom, uint offset, uint target)
		{
			Array.Copy(BitConverter.GetBytes(0x08000000U + target), 0, rom, offset, 4);
		}

		//-------------------------------------------------------------------------------
		// 「マップを追加…」: バンクを選んでもらい、今のマップをもとにマップを 1 つ追加して、一覧を読み直して選ぶ処理
		//-------------------------------------------------------------------------------
		private void btnGuideAddMap_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.romData == null || this.tempHeader == null)
			{
				MessageBox.Show(this, Localizer.T("先に、もとにするマップを左の一覧で選んでください（「マップ地形ID」の一覧ではなく、番号順かマップ名の一覧で）。"), Localizer.T("マップを追加"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			int bank = this.tempHeader.Bank;
			List<int> banks = this.mapHeaders.Select(h => h.Bank).Distinct().OrderBy(b => b).ToList();
			// 一覧の最後に「新しいバンクを作る」を足す（番号は今のバンクの数）
			int newBankNumber = this.CountBanks();
			using (Form ask = new AddMapBankForm(banks, bank, newBankNumber > 0 && newBankNumber < 255 ? newBankNumber : -1))
			{
				AppIconHelper.Apply(ask);
				UiTheme.Apply(ask);
				if (ask.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				bank = ((AddMapBankForm)ask).SelectedBank;
			}
			this.AddMapAndSelect(bank);
		}

		//-------------------------------------------------------------------------------
		// マップを追加し、マップの一覧を読み直して、追加したマップを選ぶ処理（画面用）
		//-------------------------------------------------------------------------------
		internal void AddMapAndSelect(int bank)
		{
			int newNumber;
			string error = this.AddMapToBank(bank, out newNumber);
			if (error != null)
			{
				if (error != Localizer.T("キャンセルしました。"))
				{
					MessageBox.Show(this, error, Localizer.T("マップを追加"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				return;
			}
			this.ReloadMapListAfterAdding(bank, newNumber);
			MessageBox.Show(this, string.Format(Localizer.T("マップ ({0}, {1}) を追加して選びました。\n続けて ①〜⑦ の手順で作り替えてください（地形データはもとのマップの写しです）。ファイルへは「ROMを保存」で書き出します。"), bank, newNumber), Localizer.T("マップを追加"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		//-------------------------------------------------------------------------------
		// マップを追加した後に、マップの見出しを読み直して一覧・サムネイルを作り直し、追加したマップを選ぶ処理
		//-------------------------------------------------------------------------------
		internal void ReloadMapListAfterAdding(int bank, int number)
		{
			this.mapLoadWarnings.Clear();
			this.ReadAllMapHeaders();
			this.ResetMapThumbnails();
			this.ApplyMapSearchFilter();
			foreach (TreeNode group in this.tvwMapSelector.Nodes)
			{
				foreach (TreeNode node in group.Nodes)
				{
					MapHeader header = node.Tag as MapHeader;
					if (header != null && header.Bank == bank && header.Number == number)
					{
						this.tvwMapSelector.SelectedNode = node;
						node.EnsureVisible();
					}
				}
			}
			this.StartMapThumbnailGeneration();
		}
	}
}
