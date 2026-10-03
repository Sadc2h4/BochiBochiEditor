using System;
using System.Drawing;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップエディタの表示言語（日本語／English）の切り替え処理
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		private bool isChangingLanguage;

		//-------------------------------------------------------------------------------
		// 保存された言語を読み込み、言語プルダウンと画面に反映する処理（起動時に 1 回）
		//-------------------------------------------------------------------------------
		private void InitializeLanguage()
		{
			Localizer.RegisterToolTip(this.mapToolTip);
			Localizer.LoadSavedLanguage();
			this.isChangingLanguage = true;
			this.cmbLanguage.SelectedIndex = Localizer.Language == Localizer.English ? 1 : 0;
			this.isChangingLanguage = false;
			this.ApplyLanguageToScreen();
		}

		//-------------------------------------------------------------------------------
		// 言語プルダウンの選択が変わったら表示言語を切り替えて保存する処理
		//-------------------------------------------------------------------------------
		private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.isChangingLanguage)
			{
				return;
			}
			Localizer.SetLanguage(this.cmbLanguage.SelectedIndex == 1 ? Localizer.English : Localizer.Japanese, true);
			this.ApplyLanguageToScreen();
		}

		//-------------------------------------------------------------------------------
		// 画面全体（分離中のツール欄を含む）と、コードで組み立てる表示を現在の言語で作り直す処理
		//-------------------------------------------------------------------------------
		private void ApplyLanguageToScreen()
		{
			// 選択肢の入れ替えで「変更あり」と判定されないよう、更新中の印を立てて未保存状態を保つ
			bool wasUpdating = this.isUpdatingUI;
			bool hadChanges = this.hasUnsavedChanges;
			this.isUpdatingUI = true;
			this.SuspendLayout();
			try
			{
				Localizer.Apply(this);
				if (this.toolFloatForm != null && !this.toolFloatForm.IsDisposed)
				{
					Localizer.Apply(this.toolFloatForm);
				}
				// 単独で分離しているタブ部分は、どちらの画面にも入っていないので個別に切り替える
				if (this.toolTabsDocker != null && this.toolTabsDocker.IsFloating)
				{
					Localizer.Apply(this.pnlToolTabsPart);
				}
				if (this.townMapDocker != null && this.townMapDocker.IsFloating)
				{
					Localizer.Apply(this.pnlTownMapPart);
				}
				foreach (ToolPart part in this.toolParts.Where(p => p.Header != null && p.Docker.IsFloating))
				{
					Localizer.Apply(part.Holder);
				}
				this.ApplyMapEditorButtonIcons();
				this.UpdateToolFloatButton();
				this.UpdateWindowTitle();
				this.RefreshCurrentMapLabel();
				this.UpdateBlockIndexLabel();
				this.UpdatePaletteInfoLabel();
				this.RefreshBankNodeNames();
				if (this.isMapSafetyReady)
				{
					this.RunMapSafetyChecks();
				}
				this.UpdateStatusBar();
				this.UpdateStatusCursor(new Point(-1, -1));
				this.pnlTilesetPalette.Invalidate();
				// 移動エリアの説明は、言語を変えたら必ず書き直す（同じ値でも文が変わるため）
				this.lblCollisionTitle.Text = string.Empty;
				this.UpdateCollisionInfo();
				this.RebuildMusicList();
				this.UpdateMapSettingsAdvancedButton();
				if (this.tabMain.SelectedTab == this.tabWildPokemon && this.wildEditor != null)
				{
					this.RefreshWildPokemonTab();
				}
				this.RefreshMapTileTabIfShown();
				this.pnlSelectedBlockPreview.Invalidate();
				this.pnlRecentBlocks.Invalidate();
			}
			finally
			{
				this.ResumeLayout(true);
				this.isUpdatingUI = wasUpdating;
				this.SetUnsavedChanges(hadChanges);
			}
		}

		//-------------------------------------------------------------------------------
		// 「現在マップ」の表示を現在の言語で書き直す処理
		//-------------------------------------------------------------------------------
		private void RefreshCurrentMapLabel()
		{
			if (this.romData == null)
			{
				this.lblCurrentMap.Text = Localizer.T("現在マップ :");
				return;
			}
			if (this.chkTerrainIdMode.Checked)
			{
				TreeNode node = this.tvwMapSelector.SelectedNode;
				this.lblCurrentMap.Text = (node != null) ? string.Format(Localizer.T("現在マップ : マップ地形ID {0:D4}"), node.Index + 1) : Localizer.T("現在マップ :");
				return;
			}
			this.lblCurrentMap.Text = (this.tempHeader != null)
				? string.Format(Localizer.T("現在マップ : ({0}, {1}) {2}"), this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this))
				: Localizer.T("現在マップ :");
		}

		//-------------------------------------------------------------------------------
		// マップ一覧の「バンク N」の見出しだけを現在の言語で書き直す処理（開閉や選択は保つ）
		//-------------------------------------------------------------------------------
		private void RefreshBankNodeNames()
		{
			if (!this.rbMapSortIndex.Checked || this.chkTerrainIdMode.Checked)
			{
				return;
			}
			foreach (TreeNode node in this.tvwMapSelector.Nodes)
			{
				object tag = node.Tag;
				System.Reflection.PropertyInfo bank = (tag != null) ? tag.GetType().GetProperty("Bank") : null;
				if (bank != null)
				{
					node.Text = string.Format(Localizer.T("バンク {0}"), bank.GetValue(tag));
				}
			}
		}
	}
}
