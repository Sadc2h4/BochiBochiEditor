using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 読み込んだ ROM の種類を上部バーの札（アイコン＋ゲーム名）で示す処理
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 札のアイコン（ファイル名ごとに 1 回だけ読む。読めなければ null で、文字だけの札になる）
		private readonly Dictionary<string, Image> romBadgeIcons = new Dictionary<string, Image>();
		// 札をクリックしたときの処理を登録済みか
		private bool romBadgeClickReady;
		// 開いている「テーブル情報」画面（1 つだけ開く）
		private RomTablesForm romTablesForm;

		//-------------------------------------------------------------------------------
		// ROM の種類に合わせて札のアイコンと文字を切り替える処理（ROM が無ければ札を隠す）
		// ファイアレッド系（BPR）は FR_icon、エメラルド系（BPE）は EM_icon、それ以外は gba_Small_icon を使う
		//-------------------------------------------------------------------------------
		private void UpdateRomBadge()
		{
			if (this.romBadge == null)
			{
				return;
			}
			// ROM を開き直したら、BGM の一覧もその ROM の曲で作り直す
			this.RebuildMusicListIfRomChanged();
			if (!this.romBadgeClickReady)
			{
				this.romBadgeClickReady = true;
				this.romBadge.Cursor = System.Windows.Forms.Cursors.Hand;
				this.romBadge.Click += (sender, e) => this.ShowRomTables();
			}
			if (this.romData == null)
			{
				this.romBadge.Visible = false;
				return;
			}
			string code = GameProfile.Current.Code ?? string.Empty;
			string file = code.StartsWith("BPR", StringComparison.Ordinal) ? "FR_icon.png"
				: (code.StartsWith("BPE", StringComparison.Ordinal) ? "EM_icon.png" : "gba_Small_icon.png");
			this.romBadge.BadgeIcon = this.GetRomBadgeIcon(file);
			this.romBadge.Text = Localizer.T(GameProfile.Current.DisplayName);
			this.mapToolTip?.SetToolTip(this.romBadge, string.Format(Localizer.T("読み込んだ ROM の種類（ゲームコード {0}）。クリックすると、この ROM の表をどこから読んでいるかの一覧を開きます"), code));
			this.romBadge.Visible = true;
		}

		//-------------------------------------------------------------------------------
		// 「開いている ROM のテーブル情報」画面を開く処理（開いていれば前面に出して中身を新しくする）
		//-------------------------------------------------------------------------------
		private void ShowRomTables()
		{
			if (this.romData == null)
			{
				return;
			}
			System.Windows.Forms.Cursor previous = this.Cursor;
			this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			System.Collections.Generic.List<RomTableEntry> entries;
			try
			{
				// 設定ファイルの「*アドレス」の読み取りは MainForm.romData を見るので、今の ROM に合わせておく
				MainForm.romData = this.romData;
				entries = RomTableReport.Build(this.romData);
			}
			catch (Exception ex)
			{
				string log = this.WriteErrorLog(Localizer.T("テーブル情報の作成"), ex);
				System.Windows.Forms.MessageBox.Show(this, Localizer.T("テーブル情報を作れませんでした。") + Environment.NewLine + ex.Message + Environment.NewLine + log, this.Text,
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
				return;
			}
			finally
			{
				this.Cursor = previous;
			}
			int changed = 0, warnings = 0, errors = 0;
			foreach (RomTableEntry entry in entries)
			{
				if (entry.Status == RomTableEntry.Level.Info) changed++;
				else if (entry.Status == RomTableEntry.Level.Warning) warnings++;
				else if (entry.Status == RomTableEntry.Level.Error) errors++;
			}
			string summary = string.Format(Localizer.T("ROM: {0}（{1}、ゲームコード {2}、{3:N0} バイト）　設定ファイル: ini/{4}{5}調べた表 {6} 件のうち、変更あり {7} 件・要確認 {8} 件・読めない {9} 件"),
				string.IsNullOrEmpty(this.loadedRomPath) ? "-" : Path.GetFileName(this.loadedRomPath), Localizer.T(GameProfile.Current.DisplayName), GameProfile.Current.Code,
				this.romData.Length, GameProfile.Current.IniFileName, Environment.NewLine, entries.Count, changed, warnings, errors);
			if (this.romTablesForm != null && !this.romTablesForm.IsDisposed)
			{
				this.romTablesForm.Close();
			}
			this.romTablesForm = new RomTablesForm(entries, summary);
			AppIconHelper.Apply(this.romTablesForm);
			UiTheme.Apply(this.romTablesForm);
			this.romTablesForm.Show(this);
		}

		//-------------------------------------------------------------------------------
		// テーブル情報の画面が開いていれば閉じる処理（ROM を開き直したとき、前の ROM の内容を出し続けないように）
		//-------------------------------------------------------------------------------
		private void CloseRomTablesForm()
		{
			if (this.romTablesForm != null && !this.romTablesForm.IsDisposed)
			{
				this.romTablesForm.Close();
			}
			this.romTablesForm = null;
		}

		//-------------------------------------------------------------------------------
		// 札のアイコン（img フォルダの画像）を読み込む処理（ファイルを掴んだままにしないよう複製して持つ）
		//-------------------------------------------------------------------------------
		private Image GetRomBadgeIcon(string file)
		{
			Image image;
			if (this.romBadgeIcons.TryGetValue(file, out image))
			{
				return image;
			}
			try
			{
				string path = AppAssetLocator.FindRequiredFile(Path.Combine("img", file));
				using (Bitmap loaded = new Bitmap(path))
				{
					image = new Bitmap(loaded);
				}
			}
			catch (Exception)
			{
				image = null;
			}
			this.romBadgeIcons[file] = image;
			return image;
		}
	}
}
