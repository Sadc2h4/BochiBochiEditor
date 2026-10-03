using System;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// メインタブ「出現ポケモン」: 左で選んだマップの出現ポケモン（草むら・水上・いわくだき・つり）を編集する
	// 中身は既存の野生ポケモン編集画面（WildPokemonEditor）を部品として組み込み、ROM のデータは同じものを共有する
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		private WildPokemonEditor wildEditor;
		// 組み込んだ編集画面が読み込んだ ROM（別の ROM を開いたら作り直す）
		private byte[] wildEditorRom;

		//-------------------------------------------------------------------------------
		// 「出現ポケモン」タブを開いたときに、選択中のマップの内容を出すよう登録する処理
		//-------------------------------------------------------------------------------
		private void InitializeWildPokemonTab()
		{
			this.tabMain.SelectedIndexChanged += (sender, e) =>
			{
				if (this.tabMain.SelectedTab == this.tabWildPokemon)
				{
					this.RefreshWildPokemonTab();
				}
			};
		}

		//-------------------------------------------------------------------------------
		// 選択中のマップの出現ポケモンを表示し、上の案内文を書き直す処理
		//-------------------------------------------------------------------------------
		private void RefreshWildPokemonTab()
		{
			if (this.romData == null || this.tempHeader == null)
			{
				this.lblWildMap.Text = Localizer.T("ROM を読み込んで左の一覧からマップを選ぶと、そのマップの出現ポケモンを表示します。");
				if (this.wildEditor != null && !this.wildEditor.IsDisposed)
				{
					this.wildEditor.Visible = false;
				}
				return;
			}
			if (!this.EnsureWildEditor())
			{
				return;
			}
			this.wildEditor.Visible = true;
			int bank = this.tempHeader.Bank;
			int number = this.tempHeader.Number;
			string name = this.tempHeader.GetMapName(this);
			bool? found = this.wildEditor.ShowMap(bank, number);
			if (found == null)
			{
				// 前のマップの変更の保存確認でキャンセルされたので、前のマップの表示のまま残す
				this.lblWildMap.Text = Localizer.T("保存していない変更があるため、前のマップの出現ポケモンを表示したままにしています。保存するか元に戻してから、もう一度マップを選んでください。");
				return;
			}
			this.lblWildMap.Text = found.Value
				? string.Format(Localizer.T("({0}, {1}) {2} の出現ポケモンです。種類ごとのタブ（草むら・水上・いわくだき・つり）で、出現するポケモンとレベル、出現率を変えられます。{3}この画面の「変更を保存」で読み込み中の ROM へ反映し、ファイルへは上の「ROMを保存」で書き出します。"), bank, number, name, Environment.NewLine)
				: string.Format(Localizer.T("({0}, {1}) {2} には、出現ポケモンのデータがありません。{3}野生のポケモンを出したいときは「マップ追加」でこのマップのデータを作り、「エリア追加」で草むら・水上などの種類を足します。"), bank, number, name, Environment.NewLine);
		}

		//-------------------------------------------------------------------------------
		// 組み込む編集画面を用意する処理（まだ無い、または別の ROM を開いたときは作り直す）
		//-------------------------------------------------------------------------------
		private bool EnsureWildEditor()
		{
			if (this.wildEditor != null && !this.wildEditor.IsDisposed && this.wildEditorRom == this.romData)
			{
				return true;
			}
			this.DisposeWildEditor();
			// 編集画面は MainForm.romData を読むので、マップエディタの ROM と同じものを渡す（書き込みも同じ ROM に入る）
			MainForm.romData = this.romData;
			// ポケモン名の表の位置・長さ・匹数は、編集画面が読み込みのたびに ROM から調べる（PokemonNameTable）
			try
			{
				WildPokemonEditor editor = new WildPokemonEditor();
				editor.PrepareEmbedded();
				// エリア追加・マップ追加の書き込み先は、「新規」タブと同じ方法（空き領域の検索・範囲の確認）で決める
				editor.AddressHost = this;
				this.pnlWildHost.Controls.Add(editor);
				Localizer.Apply(editor);
				UiTheme.Apply(editor);
				editor.Show();
				this.wildEditor = editor;
				this.wildEditorRom = this.romData;
				return true;
			}
			catch (Exception ex)
			{
				this.WriteErrorLog("出現ポケモンの読み込み", ex);
				this.DisposeWildEditor();
				this.lblWildMap.Text = Localizer.T("出現ポケモンのデータを読み込めませんでした。詳しくは error.log を見てください。");
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		// 組み込んだ編集画面を片付ける処理
		//-------------------------------------------------------------------------------
		private void DisposeWildEditor()
		{
			if (this.wildEditor != null && !this.wildEditor.IsDisposed)
			{
				this.pnlWildHost.Controls.Remove(this.wildEditor);
				this.wildEditor.Dispose();
			}
			this.wildEditor = null;
			this.wildEditorRom = null;
		}

		//-------------------------------------------------------------------------------
		// 出現ポケモンに保存していない変更があれば、保存するか確認する処理（キャンセルなら false）
		// 別の ROM を開くときや、エディタを閉じるときに使う
		//-------------------------------------------------------------------------------
		private bool ConfirmWildPokemonSaved()
		{
			if (this.wildEditor == null || this.wildEditor.IsDisposed || !this.wildEditor.HasUnsavedEdits)
			{
				return true;
			}
			this.tabMain.SelectedTab = this.tabWildPokemon;
			return this.wildEditor.ConfirmSaveBeforeLeave();
		}
	}
}
