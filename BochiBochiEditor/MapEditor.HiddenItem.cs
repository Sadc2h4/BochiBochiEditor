using System;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// イベントタブの「看板」のうち、隠しアイテム（タイプ 05〜07）の中身を編集する処理
	// 隠しアイテムの看板は、スクリプトのポインタの 4 バイトが「道具番号・フラグ番号（・個数・真下専用）」になっている
	//   ファイアレッド: 道具 16 ビット、フラグ 8 ビット、個数 7 ビット、真下専用 1 ビット（フラグは 0x3E8 からの数）
	//   エメラルド   : 道具 16 ビット、フラグ 16 ビット（フラグは 0x1F4 からの数）
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 道具の名前の表（ROM ごとに 1 回調べる。調べられなければ null のまま）
		private ItemNameTable itemNameTable;
		private byte[] itemNameTableRom;

		//-------------------------------------------------------------------------------
		// 隠しアイテム用の欄の初期化（値を変えたときの処理・説明の登録）
		//-------------------------------------------------------------------------------
		private void InitializeHiddenItemFields()
		{
			this.nudSignItem.ValueChanged += this.OnEventDataChanged;
			this.nudSignItemFlag.ValueChanged += this.OnEventDataChanged;
			this.nudSignItemQuantity.ValueChanged += this.OnEventDataChanged;
			this.chkSignItemUnderfoot.CheckedChanged += this.OnEventDataChanged;
			this.UpdateSignPayloadLayout(false);
		}

		//-------------------------------------------------------------------------------
		// 隠しアイテム用の欄の説明（ツールチップ）を付ける処理（ROM を開いたとき・言語を変えたときに呼ぶ）
		//-------------------------------------------------------------------------------
		private void UpdateHiddenItemToolTips()
		{
			if (this.mapToolTip == null)
			{
				return;
			}
			this.mapToolTip.SetToolTip(this.nudSignItem, Localizer.T("拾える道具の番号です。"));
			this.mapToolTip.SetToolTip(this.nudSignItemFlag, string.Format(Localizer.T("拾ったかどうかを覚えるフラグの番号です（隠しアイテム用のフラグの先頭 0x{0:X} からの数）。ほかの隠しアイテムと同じ番号にすると、片方を拾うともう片方も拾えなくなります。"), HiddenItemFlagBase));
			this.mapToolTip.SetToolTip(this.nudSignItemQuantity, Localizer.T("拾える個数です（元のゲームは 1）。"));
			this.mapToolTip.SetToolTip(this.chkSignItemUnderfoot, Localizer.T("チェックすると、正面から調べても拾えず、真上に立ってダウジングマシンを使ったときだけ見つかります。"));
		}

		//-------------------------------------------------------------------------------
		// 看板のタイプが隠しアイテムかを返す処理（ファイアレッド・エメラルドとも 05〜07）
		//-------------------------------------------------------------------------------
		private static bool IsHiddenItemSignType(byte signType)
		{
			return signType >= 5 && signType <= 7;
		}

		// 隠しアイテム用のフラグの先頭（フラグ番号の欄は、ここからの数）
		private static int HiddenItemFlagBase
		{
			get { return GameProfile.Current.EmeraldHeaderLayout ? 0x1F4 : 0x3E8; }
		}

		// 個数と「真下専用」を持つゲームか（ファイアレッドだけ）
		private static bool HiddenItemHasQuantity
		{
			get { return !GameProfile.Current.EmeraldHeaderLayout; }
		}

		//-------------------------------------------------------------------------------
		// 道具・フラグ・個数・真下専用を、看板の 4 バイトの値にまとめる処理
		//-------------------------------------------------------------------------------
		private static uint EncodeHiddenItem(int item, int flag, int quantity, bool underfoot)
		{
			if (HiddenItemHasQuantity)
			{
				return (uint)(item & 0xFFFF) | ((uint)(flag & 0xFF) << 16) | ((uint)(quantity & 0x7F) << 24) | (underfoot ? 0x80000000U : 0U);
			}
			return (uint)(item & 0xFFFF) | ((uint)(flag & 0xFFFF) << 16);
		}

		//-------------------------------------------------------------------------------
		// ゲームに合わせて、看板のタイプの選択肢とフラグ番号の上限を切り替える処理（ROM を開くたびに呼ぶ）
		//-------------------------------------------------------------------------------
		private void ApplySignTypeChoiceList()
		{
			bool updating = this.isUpdatingUI;
			this.isUpdatingUI = true;
			try
			{
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", GameProfile.Current.EmeraldHeaderLayout ? "EventSignType_EM.txt" : "EventSignType.txt"), this.cmbSignType);
				this.nudSignItemFlag.Value = 0m;
				this.nudSignItemFlag.Maximum = HiddenItemHasQuantity ? 255m : 65535m;
			}
			finally
			{
				this.isUpdatingUI = updating;
			}
			this.UpdateHiddenItemToolTips();
		}

		//-------------------------------------------------------------------------------
		// 道具の名前の表を用意する処理（別の ROM になっていれば調べ直す）
		//-------------------------------------------------------------------------------
		private ItemNameTable GetItemNameTable()
		{
			if (this.itemNameTableRom != this.romData)
			{
				this.itemNameTableRom = this.romData;
				try
				{
					MainForm.romData = this.romData;
					this.itemNameTable = ItemNameTable.Create(this.romData);
				}
				catch (Exception)
				{
					this.itemNameTable = null;
				}
			}
			return this.itemNameTable;
		}

		//-------------------------------------------------------------------------------
		// 看板の中身の欄を、タイプに合わせて出し分ける処理（スクリプトの欄か、隠しアイテムの欄か）
		// 枠の高さも、見えている欄に合わせる
		//-------------------------------------------------------------------------------
		private void UpdateSignPayloadLayout(bool hidden)
		{
			bool quantity = hidden && HiddenItemHasQuantity;
			this.lblSignScriptAddress.Visible = !hidden;
			this.txtSignScriptAddress.Visible = !hidden;
			this.lblSignItem.Visible = hidden;
			this.nudSignItem.Visible = hidden;
			this.lblSignItemName.Visible = hidden;
			this.lblSignItemFlag.Visible = hidden;
			this.nudSignItemFlag.Visible = hidden;
			this.lblSignItemFlagInfo.Visible = hidden;
			this.lblSignItemQuantity.Visible = quantity;
			this.nudSignItemQuantity.Visible = quantity;
			this.chkSignItemUnderfoot.Visible = quantity;
			Control last = !hidden ? (Control)this.txtSignScriptAddress : (quantity ? (Control)this.chkSignItemUnderfoot : this.lblSignItemFlagInfo);
			int height = last.Bottom + this.LogicalToDeviceUnits(7);
			if (this.grpSignEvent.Height != height)
			{
				this.grpSignEvent.Height = height;
			}
		}

		//-------------------------------------------------------------------------------
		// 選んだ看板の中身を欄に出す処理（画面を書き換えている最中 = isUpdatingUI が true の間に呼ぶ）
		//-------------------------------------------------------------------------------
		private void ShowSignPayload(MapEditor.SignEvent sign)
		{
			bool hidden = IsHiddenItemSignType(sign.SignType);
			this.UpdateSignPayloadLayout(hidden);
			if (!hidden)
			{
				return;
			}
			uint raw = sign.RawScriptValue;
			bool full = HiddenItemHasQuantity;
			this.nudSignItem.Value = raw & 0xFFFF;
			this.nudSignItemFlag.Value = Math.Min(this.nudSignItemFlag.Maximum, full ? ((raw >> 16) & 0xFF) : (raw >> 16));
			this.nudSignItemQuantity.Value = full ? ((raw >> 24) & 0x7F) : 0;
			this.chkSignItemUnderfoot.Checked = full && (raw & 0x80000000U) != 0;
			this.UpdateSignItemInfo();
		}

		//-------------------------------------------------------------------------------
		// 看板の中身を、画面の欄から編集中のデータへ取り込む処理
		// タイプを「スクリプトの看板 ⇔ 隠しアイテム」で変えたときは、中身の意味が違うので初期値にする
		//-------------------------------------------------------------------------------
		private void SyncSignPayloadFromUI(MapEditor.SignEvent sign, byte previousType)
		{
			bool wasHidden = IsHiddenItemSignType(previousType);
			bool hidden = IsHiddenItemSignType(sign.SignType);
			if (wasHidden != hidden)
			{
				// 隠しアイテムにしたとき: 道具 0・フラグ 0・個数 1。スクリプトの看板にしたとき: スクリプトなし
				sign.RawScriptValue = hidden ? EncodeHiddenItem(0, 0, 1, false) : 0U;
				sign.ScriptAddress = this.PointerToOffset(sign.RawScriptValue);
				bool updating = this.isUpdatingUI;
				this.isUpdatingUI = true;
				try
				{
					this.txtSignScriptAddress.Text = string.Format("{0:X8}", sign.ScriptAddress);
					this.ShowSignPayload(sign);
				}
				finally
				{
					this.isUpdatingUI = updating;
				}
				return;
			}
			if (hidden)
			{
				// 確定では、この 4 バイトをそのまま書く（WriteScriptPointerToRom。ScriptAddress を同じ値から作っておく）
				sign.RawScriptValue = EncodeHiddenItem((int)this.nudSignItem.Value, (int)this.nudSignItemFlag.Value, (int)this.nudSignItemQuantity.Value, this.chkSignItemUnderfoot.Checked);
				sign.ScriptAddress = this.PointerToOffset(sign.RawScriptValue);
				this.UpdateSignItemInfo();
			}
			else
			{
				sign.ScriptAddress = this.ParseHex8(this.txtSignScriptAddress.Text);
			}
		}

		//-------------------------------------------------------------------------------
		// 道具の名前と、実際のフラグ番号の表示を書き直す処理
		//-------------------------------------------------------------------------------
		private void UpdateSignItemInfo()
		{
			ItemNameTable table = this.GetItemNameTable();
			string name = table != null ? table.GetName((int)this.nudSignItem.Value) : null;
			this.lblSignItemName.Text = string.IsNullOrEmpty(name) ? string.Empty : "→ " + name;
			this.lblSignItemFlagInfo.Text = string.Format(Localizer.T("実際のフラグ: 0x{0:X}"), HiddenItemFlagBase + (int)this.nudSignItemFlag.Value);
		}
	}
}
