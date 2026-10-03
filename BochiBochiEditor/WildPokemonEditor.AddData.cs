using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 出現ポケモンの「エリア追加」「マップ追加」を、ROM のほかのデータを壊さずに行う処理
	// ・エリア追加: アドレス欄が空欄なら空き領域の候補を一覧で出して選んでもらう（アドレスの指定も可）。追加した時点で場所を確保し、元に戻すと確保前の内容へ戻す
	// ・マップ追加: 表の後ろに空きが無ければ、表を空き領域へ移して参照元のポインタをすべて付け替えてから 1 件足す
	//-------------------------------------------------------------------------------
	public partial class WildPokemonEditor
	{
		// 空き領域の検索とアドレスの確認に使うマップエディタ（組み込みで使うときに設定される）
		internal MapEditor AddressHost;

		// 種類ごと（草むら・水上・いわくだき・つり）の、追加したときの出現率の初期値（FR の道路でいちばん多い値）
		private static readonly byte[] NewAreaDefaultRates = new byte[] { 21, 2, 50, 20 };

		// 同じく、エメラルド系の初期値（道路の草むら 20、水上 4、いわくだき 20、つり 30）
		private static readonly byte[] NewAreaDefaultRatesEmerald = new byte[] { 20, 4, 20, 30 };

		// 追加した枠のレベルの初期値
		private const byte NewSlotDefaultLevel = 5;

		// 表を移すときに、後ろへ足せるように空けておく件数
		private const int RelocatedTableSpareEntries = 16;

		// 追加したが保存していない領域（書き込み先と、確保する前の内容）。元に戻すときに書き戻す
		private readonly List<KeyValuePair<int, byte[]>> pendingNewAreaBlocks = new List<KeyValuePair<int, byte[]>>();

		// 時間帯ごとの表の先頭（表を移したら書き換える）
		private int[] tableBaseAddresses;

		// 試験用: 設定されていれば、ダイアログを出さずにこの関数の答えを使う（検証用ツールから使う）
		internal static Func<string, MessageBoxButtons, DialogResult> TestDialogAnswer;

		//-------------------------------------------------------------------------------
		// 追加の処理で使うダイアログを出す処理（試験中は TestDialogAnswer の答えを返す）
		//-------------------------------------------------------------------------------
		private DialogResult ShowAddDataMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
		{
			if (TestDialogAnswer != null)
			{
				return TestDialogAnswer(text, buttons);
			}
			return MessageBox.Show(text, caption, buttons, icon, defaultButton);
		}

		//-------------------------------------------------------------------------------
		// 時間帯ごとの表の先頭を返す処理（0 = 表なし）
		//-------------------------------------------------------------------------------
		private int[] GetTableBaseAddresses()
		{
			if (this.tableBaseAddresses == null)
			{
				this.tableBaseAddresses = new int[] { this.WILD_ENCOUNTER_TABLE_MORNING_OFFSET, this.WILD_ENCOUNTER_TABLE_DAY_OFFSET, this.WILD_ENCOUNTER_TABLE_EVENING_OFFSET, this.WILD_ENCOUNTER_TABLE_NIGHT_OFFSET };
			}
			return this.tableBaseAddresses;
		}

		//-------------------------------------------------------------------------------
		// 新しいデータの書き込み先を決める処理（決まれば true）
		// 入力が空欄なら空き領域の候補を一覧で出して選んでもらう。指定があれば範囲と 4 バイト境界を確かめ、空きでなければ上書きしてよいか聞く
		// purpose は一覧の画面に出す「何を書くか」の説明
		//-------------------------------------------------------------------------------
		private bool ResolveWildDataAddress(string text, int length, string purpose, out int address)
		{
			address = 0;
			if (string.IsNullOrWhiteSpace(text) && this.AddressHost != null)
			{
				// 候補の一覧を出して選んでもらう（「新規」タブと同じ画面。MapEditor.PickFreeSpaceAddress）
				uint picked;
				if (!this.AddressHost.PickFreeSpaceAddress(purpose, length, this.TopLevelControl as Form, out picked))
				{
					return false;
				}
				text = string.Format("{0:X8}", 0x08000000U + picked);
			}
			uint found;
			bool occupied;
			string error = this.AddressHost != null
				? this.AddressHost.ResolveNewDataAddress(text, false, length, out found, out occupied)
				: this.ResolveWildDataAddressWithoutHost(text, length, out found, out occupied);
			if (error == null && found % 4 != 0)
			{
				error = string.Format(Localizer.T("アドレス 0x{0:X6} は 4 の倍数ではありません。出現ポケモンのデータは 4 の倍数のアドレスに置く必要があります。"), found);
			}
			if (error != null)
			{
				this.ShowAddDataMessage(error, "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
			if (occupied)
			{
				DialogResult answer = this.ShowAddDataMessage(string.Format(Localizer.T("書き込み先 0x{0:X6} から {1} バイトの範囲に、空き（0xFF）でないデータがあります。\r\nこのまま上書きしますか？"), found, length), "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				if (answer != DialogResult.Yes)
				{
					return false;
				}
			}
			address = (int)found;
			return true;
		}

		//-------------------------------------------------------------------------------
		// マップエディタに組み込まれていないときの書き込み先の確認（アドレスの指定が必要。自動の検索はしない）
		//-------------------------------------------------------------------------------
		private string ResolveWildDataAddressWithoutHost(string text, int length, out uint address, out bool occupied)
		{
			address = 0;
			occupied = false;
			string value = (text ?? "").Trim();
			if (value.Length == 0)
			{
				return Localizer.T("アドレスを入力してください。");
			}
			if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				value = value.Substring(2);
			}
			if (!uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out address))
			{
				return Localizer.T("アドレスは16進数で入力してください。");
			}
			if (address >= 0x08000000U)
			{
				address -= 0x08000000U;
			}
			if ((ulong)address + (ulong)length > (ulong)this.romData.Length)
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
		// 表示中のデータ（マップ）の、指定した時間帯の項目を返す処理
		//-------------------------------------------------------------------------------
		private WildEncounterEntry GetShownEntry(int timeIndex)
		{
			if (!this.loadedEncounterTables.ContainsKey(timeIndex))
			{
				return null;
			}
			int tableIdx = Convert.ToInt32(this.nudTableIndex.Value);
			return this.loadedEncounterTables[timeIndex].FirstOrDefault(x => x.TableIndex == tableIdx);
		}

		//-------------------------------------------------------------------------------
		// 「エリア追加」: 選んだ種類の出現データ（見出し 8 バイト + 枠 × 4 バイト）を作る処理
		// 書き込み先はこの時点で確保し（ほかの追加と重ならないように）、表からの参照は「変更を保存」で書く
		//-------------------------------------------------------------------------------
		private void AddNewArea()
		{
			int timeIndex = this.GetSelectedLoadTimeIndex();
			WildEncounterEntry entry = this.GetShownEntry(timeIndex);
			int areaIndex = this.cmbNewArea.SelectedIndex;
			if (entry == null || areaIndex < 0 || areaIndex > 3)
			{
				return;
			}
			WildArea area = entry.Areas[areaIndex];
			if (area.IsActive)
			{
				this.ShowAddDataMessage(Localizer.T("この種類の出現データはすでにあります。中身は上のタブで編集してください。"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			int slotCount = this.SLOT_COUNTS[areaIndex];
			int length = 8 + slotCount * 4;
			int address;
			if (!this.ResolveWildDataAddress(this.txtNewAreaAddress.Text, length, string.Format(Localizer.T("出現ポケモンのエリア（{0}）"), this.cmbNewArea.Text), out address))
			{
				return;
			}
			// 確保する前の内容を覚えてから、初期値のデータを書いて場所を確保する
			byte[] before = new byte[length];
			Array.Copy(this.romData, address, before, 0, length);
			this.pendingNewAreaBlocks.Add(new KeyValuePair<int, byte[]>(address, before));
			byte rate = (GameProfile.Current.EmeraldHeaderLayout ? NewAreaDefaultRatesEmerald : NewAreaDefaultRates)[areaIndex];
			int dataAddress = address + 8;
			this.romData[address] = rate;
			this.romData[address + 1] = 0;
			this.romData[address + 2] = 0;
			this.romData[address + 3] = 0;
			Array.Copy(BitConverter.GetBytes((uint)(0x08000000 + dataAddress)), 0, this.romData, address + 4, 4);
			area.IsActive = true;
			area.EncounterRate = rate;
			area.OriginalHeaderAddress = address;
			area.OriginalDataAddress = dataAddress;
			area.Slots.Clear();
			for (int i = 0; i < slotCount; i++)
			{
				int slot = dataAddress + i * 4;
				this.romData[slot] = NewSlotDefaultLevel;
				this.romData[slot + 1] = NewSlotDefaultLevel;
				this.romData[slot + 2] = 0;
				this.romData[slot + 3] = 0;
				area.Slots.Add(new WildPokemonSlot(NewSlotDefaultLevel, NewSlotDefaultLevel, 0));
			}
			this.LoadMapEntry(entry, timeIndex);
			this.tabAreaData.SelectedIndex = areaIndex;
			this.txtNewAreaAddress.Text = string.Format("{0:X8}", 0x08000000 + address);
			this.SetUnsavedChanges(true);
		}

		//-------------------------------------------------------------------------------
		// 追加したが保存していない領域を、確保する前の内容へ戻す処理（「元に戻す」のとき）
		//-------------------------------------------------------------------------------
		private void RestorePendingNewAreaBlocks()
		{
			for (int i = this.pendingNewAreaBlocks.Count - 1; i >= 0; i--)
			{
				KeyValuePair<int, byte[]> block = this.pendingNewAreaBlocks[i];
				Array.Copy(block.Value, 0, this.romData, block.Key, block.Value.Length);
			}
			this.pendingNewAreaBlocks.Clear();
		}

		//-------------------------------------------------------------------------------
		// 保存したので、追加した領域を確定する処理
		//-------------------------------------------------------------------------------
		private void CommitPendingNewAreaBlocks()
		{
			this.pendingNewAreaBlocks.Clear();
		}

		//-------------------------------------------------------------------------------
		// 「マップ追加」で新しい項目を置く場所を返す処理（置けなければ -1）
		// 表の終わりの印の後ろ 20 バイト（新しい終わりの印の場所）が空きでなければ、表を空き領域へ移す
		//-------------------------------------------------------------------------------
		private int PrepareNewMapEntryAddress(int timeIndex, List<WildEncounterEntry> list)
		{
			// 同じマップの項目が 2 つあっても、ゲームは先の 1 つしか使わないので作らない
			int bank = Convert.ToInt32(this.nudMapBankSearch.Value);
			int number = Convert.ToInt32(this.nudMapNumberSearch.Value);
			if (list.Any(x => x.MapBank == bank && x.MapNumber == number))
			{
				this.ShowAddDataMessage(string.Format(Localizer.T("マップ ({0}, {1}) の出現データはすでにあります。種類を足すときは「エリア追加」を使ってください。"), bank, number), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return -1;
			}
			int[] bases = this.GetTableBaseAddresses();
			int tableBase = bases[timeIndex];
			if (tableBase <= 0)
			{
				this.ShowAddDataMessage(Localizer.T("この時間帯の出現ポケモンの表が ROM にありません。"), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return -1;
			}
			int end = list.Count > 0 ? list.Last().OriginalEntryAddress + 20 : tableBase;
			if (end + 40 > this.romData.Length || this.romData[end] != 0xFF || this.romData[end + 1] != 0xFF)
			{
				this.ShowAddDataMessage(Localizer.T("出現ポケモンの表の終わりが見つかりません。何も変更していません。"), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return -1;
			}
			bool roomAfter = true;
			for (int i = end + 20; i < end + 40; i++)
			{
				if (this.romData[i] != 0xFF)
				{
					roomAfter = false;
					break;
				}
			}
			if (roomAfter)
			{
				return end;
			}
			// 表の後ろにはほかのデータが続いているので、表を移す
			int tableLength = end + 20 - tableBase;
			int newLength = tableLength + 20 * (1 + RelocatedTableSpareEntries);
			uint oldPointer = (uint)(0x08000000 + tableBase);
			List<int> references = new List<int>();
			for (int i = 0; i + 4 <= this.romData.Length; i += 4)
			{
				if (BitConverter.ToUInt32(this.romData, i) == oldPointer)
				{
					references.Add(i);
				}
			}
			if (references.Count == 0)
			{
				this.ShowAddDataMessage(Localizer.T("出現ポケモンの表を指しているポインタが見つからないため、表を移せません。何も変更していません。"), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return -1;
			}
			DialogResult answer = this.ShowAddDataMessage(string.Format(Localizer.T("出現ポケモンの表（0x{0:X6}）のすぐ後ろには別のデータがあるため、このままでは追加できません。\r\n表を空き領域へ移し、表を指しているポインタ {1} か所を付け替えてから追加しますか？\r\n（元の場所の表はそのまま残します）"), tableBase, references.Count), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (answer != DialogResult.Yes)
			{
				return -1;
			}
			int newBase;
			if (!this.ResolveWildDataAddress("", newLength, Localizer.T("出現ポケモンの表の移動先"), out newBase))
			{
				return -1;
			}
			Array.Copy(this.romData, tableBase, this.romData, newBase, tableLength);
			byte[] newPointer = BitConverter.GetBytes((uint)(0x08000000 + newBase));
			foreach (int reference in references)
			{
				Array.Copy(newPointer, 0, this.romData, reference, 4);
			}
			int shift = newBase - tableBase;
			foreach (WildEncounterEntry item in list)
			{
				item.OriginalEntryAddress += shift;
			}
			bases[timeIndex] = newBase;
			MainForm.romData = this.romData;
			this.ShowAddDataMessage(string.Format(Localizer.T("出現ポケモンの表を 0x{0:X6} へ移しました（ポインタ {1} か所を付け替え）。"), newBase, references.Count), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return newBase + tableLength - 20;
		}

		//-------------------------------------------------------------------------------
		// 「変更を保存」の前に、出現率 0 や「なし」の枠が残っていないか確かめる処理（保存してよければ true）
		//-------------------------------------------------------------------------------
		private bool ConfirmIncompleteAreas()
		{
			WildEncounterEntry entry = this.GetShownEntry(this.GetSelectedLoadTimeIndex());
			if (entry == null)
			{
				return true;
			}
			List<string> problems = new List<string>();
			string[] names = new string[] { Localizer.T("陸上"), Localizer.T("水上"), Localizer.T("岩砕き"), Localizer.T("釣り") };
			for (int i = 0; i < 4; i++)
			{
				WildArea area = entry.Areas[i];
				if (!area.IsActive)
				{
					continue;
				}
				if (area.EncounterRate == 0)
				{
					problems.Add(string.Format(Localizer.T("{0}: 出現率が 0 です（ゲームでは出現しません）"), names[i]));
				}
				int empty = area.Slots.Count(s => s.PokemonID == 0);
				if (empty > 0)
				{
					problems.Add(string.Format(Localizer.T("{0}: ポケモンが「なし」の枠が {1} 個あります"), names[i], empty));
				}
				if (area.Slots.Any(s => s.MinLevel == 0 || s.MinLevel > s.MaxLevel))
				{
					problems.Add(string.Format(Localizer.T("{0}: レベルが 0、または最小が最大より大きい枠があります"), names[i]));
				}
			}
			if (problems.Count == 0)
			{
				return true;
			}
			DialogResult answer = this.ShowAddDataMessage(Localizer.T("次の点がゲームで不具合の元になる可能性があります。") + "\r\n\r\n" + string.Join("\r\n", problems) + "\r\n\r\n" + Localizer.T("このまま保存しますか？"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			return answer == DialogResult.Yes;
		}
	}
}
