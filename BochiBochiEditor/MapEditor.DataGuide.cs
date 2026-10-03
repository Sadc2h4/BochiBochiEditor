using System;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「データ作成」タブの手順ガイド（①〜⑦）
	// ・②で作った地形データ、⑥で作ったマップスクリプト・接続のアドレスを覚えておき、③・⑥のボタンで今のマップに割り当てる
	//  （割り当ては「マップの設定」タブのアドレス欄と同じ処理。確定するまで ROM には書かない）
	// ・④・⑦のボタンで、続きの作業をするタブを開く
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// このセッションで作った地形データ・マップスクリプト・接続の場所（ROM 内の位置。0 = まだ作っていない）
		private uint guideFooterAddress;
		private uint guideMapScriptAddress;
		private uint guideConnectionAddress;
		// 上の場所を覚えたときの ROM（別の ROM を開いたら忘れる）
		private byte[] guideRom;

		//-------------------------------------------------------------------------------
		// 「データ作成」タブで作ったデータの場所を覚える処理（0 の引数は変えない）
		//-------------------------------------------------------------------------------
		private void RememberCreatedGuideData(uint footer, uint mapScript, uint connection)
		{
			this.ForgetGuideDataIfRomChanged();
			this.guideRom = this.romData;
			if (footer != 0)
			{
				this.guideFooterAddress = footer;
			}
			if (mapScript != 0)
			{
				this.guideMapScriptAddress = mapScript;
			}
			if (connection != 0)
			{
				this.guideConnectionAddress = connection;
			}
			this.UpdateDataGuideStatus();
		}

		//-------------------------------------------------------------------------------
		// 別の ROM を開いていたら、覚えていた場所を忘れる処理
		//-------------------------------------------------------------------------------
		private void ForgetGuideDataIfRomChanged()
		{
			if (this.guideRom != this.romData)
			{
				this.guideFooterAddress = 0;
				this.guideMapScriptAddress = 0;
				this.guideConnectionAddress = 0;
				this.guideRom = this.romData;
			}
		}

		//-------------------------------------------------------------------------------
		// ③・⑥の「作ったもの」の表示と、割り当てボタンを押せるかを今の状態に合わせる処理
		//-------------------------------------------------------------------------------
		private void UpdateDataGuideStatus()
		{
			this.ForgetGuideDataIfRomChanged();
			bool hasMap = this.romData != null && this.tempHeader != null;
			this.lblGuideFooterStatus.Text = this.guideFooterAddress == 0
				? Localizer.T("作った地形データ : まだありません")
				: string.Format(Localizer.T("作った地形データ : {0:X8}{1}"), 0x08000000U + this.guideFooterAddress,
					hasMap && this.tempHeader.FooterAddress == this.guideFooterAddress ? Localizer.T("（今のマップに割り当て済み）") : "");
			this.btnGuideAssignFooter.Enabled = hasMap && this.guideFooterAddress != 0 && this.tempHeader.FooterAddress != this.guideFooterAddress;
			string script = this.guideMapScriptAddress == 0 ? Localizer.T("まだありません") : string.Format("{0:X8}", 0x08000000U + this.guideMapScriptAddress);
			string connection = this.guideConnectionAddress == 0 ? Localizer.T("まだありません") : string.Format("{0:X8}", 0x08000000U + this.guideConnectionAddress);
			this.lblGuideScriptStatus.Text = string.Format(Localizer.T("作ったマップスクリプト : {0}　接続 : {1}"), script, connection);
			bool scriptPending = this.guideMapScriptAddress != 0 && hasMap && this.tempHeader.MapScriptAddress != this.guideMapScriptAddress;
			bool connectionPending = this.guideConnectionAddress != 0 && hasMap && this.tempHeader.ConnectionAddress != this.guideConnectionAddress;
			this.btnGuideAssignScripts.Enabled = scriptPending || connectionPending;
		}

		//-------------------------------------------------------------------------------
		// ③「作った地形データを割り当てる」: 今のマップの地形データを、②で作ったものに変える処理（確定するまで ROM には書かない）
		//-------------------------------------------------------------------------------
		private void btnGuideAssignFooter_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.tempHeader == null || this.guideFooterAddress == 0)
			{
				return;
			}
			string message = string.Format(Localizer.T("({0}, {1}) {2} の地形データ（マップの形）を、②で作った {3:X8} に変えます。\n今の形はこのマップでは使わなくなります（元のデータは ROM に残ります）。\n「編集中のMAPを確定」を押すまで ROM には書かれません。よろしいですか？"),
				this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this), 0x08000000U + this.guideFooterAddress);
			if (MessageBox.Show(this, message, Localizer.T("③ 今のマップに割り当てる"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			this.txtAddressMapFooter.Text = string.Format("{0:X8}", 0x08000000U + this.guideFooterAddress);
			this.btnChangeAddressMapHeader_Click(this, EventArgs.Empty);
			this.UpdateDataGuideStatus();
		}

		//-------------------------------------------------------------------------------
		// ⑥「作ったマップスクリプト・接続を割り当てる」: 今のマップのマップスクリプト・接続を、作ったものに変える処理
		//-------------------------------------------------------------------------------
		private void btnGuideAssignScripts_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.tempHeader == null)
			{
				return;
			}
			if (this.chkTerrainIdMode.Checked)
			{
				MessageBox.Show(this, Localizer.T("マップ地形IDの一覧では、マップスクリプト・接続は割り当てられません。「マップ地形ID」のチェックを外してマップを選び直してください。"), Localizer.T("⑥ マップスクリプト・接続を作る"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			string parts = "";
			if (this.guideMapScriptAddress != 0)
			{
				parts += "\n" + string.Format(Localizer.T("マップスクリプト → {0:X8}"), 0x08000000U + this.guideMapScriptAddress);
			}
			if (this.guideConnectionAddress != 0)
			{
				parts += "\n" + string.Format(Localizer.T("接続 → {0:X8}"), 0x08000000U + this.guideConnectionAddress);
			}
			string message = string.Format(Localizer.T("({0}, {1}) {2} に、作ったデータを割り当てます。{3}\n「編集中のMAPを確定」を押すまで ROM には書かれません。よろしいですか？"),
				this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this), parts);
			if (MessageBox.Show(this, message, Localizer.T("⑥ マップスクリプト・接続を作る"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			if (this.guideMapScriptAddress != 0)
			{
				this.txtAddressMapScript.Text = string.Format("{0:X8}", 0x08000000U + this.guideMapScriptAddress);
			}
			if (this.guideConnectionAddress != 0)
			{
				this.txtAddressMapConnection.Text = string.Format("{0:X8}", 0x08000000U + this.guideConnectionAddress);
			}
			this.btnChangeAddressMapHeader_Click(this, EventArgs.Empty);
			this.UpdateDataGuideStatus();
		}

		//-------------------------------------------------------------------------------
		// ④「「マップ」タブを開く」
		//-------------------------------------------------------------------------------
		private void btnGuideOpenMap_Click(object sender, EventArgs e)
		{
			this.tabMain.SelectedTab = this.tabMapEdit;
		}

		//-------------------------------------------------------------------------------
		// ⑦「「出現ポケモン」タブ」
		//-------------------------------------------------------------------------------
		private void btnGuideOpenWild_Click(object sender, EventArgs e)
		{
			this.tabMain.SelectedTab = this.tabWildPokemon;
		}

		//-------------------------------------------------------------------------------
		// ⑦「「マップの設定」タブ」
		//-------------------------------------------------------------------------------
		private void btnGuideOpenSettings_Click(object sender, EventArgs e)
		{
			this.tabMain.SelectedTab = this.tabMapInfo;
		}
	}
}
