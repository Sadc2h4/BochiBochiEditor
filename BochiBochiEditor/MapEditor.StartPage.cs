using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 入口画面（StartPage）をメイン画面に重ねる処理と、「最近開いた ROM」の記憶
	//   settings.ini の RecentRoms = 場所|場所|…（新しい順、8 件まで）
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 最近開いた ROM の記憶の上限と、settings.ini の名前・区切り
		private const int RecentRomLimit = 8;
		private const string RecentRomsSetting = "RecentRoms";
		private const char RecentRomsSeparator = '|';

		// 入口画面（ROM を読み込んでいない間だけ見せる）
		private StartPage startPage;

		//-------------------------------------------------------------------------------
		// 入口画面を作ってメイン画面に重ねる処理（画面の部品を作った後、ROM を読む前に呼ぶ）
		// 上のボタン列と状態バーは残し、その間を覆う
		//-------------------------------------------------------------------------------
		private void InitializeStartPage()
		{
			this.startPage = new StartPage { Dock = DockStyle.Fill, Visible = false, Name = "startPage" };
			this.startPage.OpenRequested += (sender, e) => this.btnLoadRom_Click(this.btnLoadRom, EventArgs.Empty);
			this.startPage.RomSelected += (sender, path) => this.OpenRomFromPath(path);
			this.startPage.RemoveRecentRequested += (sender, path) => this.RemoveRecentRom(path);
			this.startPage.ResumeRequested += (sender, e) => this.HideStartPage();
			this.Controls.Add(this.startPage);
			this.startPage.BringToFront();
		}

		//-------------------------------------------------------------------------------
		// 「ホーム」ボタン: ROM を読んだままで入口画面を出す（「編集に戻る」で戻れる）
		//-------------------------------------------------------------------------------
		private void btnHome_Click(object sender, EventArgs e)
		{
			this.ShowStartPage();
		}

		//-------------------------------------------------------------------------------
		// 入口画面を出す処理（最近開いた ROM を読み直し、ROM を読んでいれば「編集に戻る」を出す）
		//-------------------------------------------------------------------------------
		private void ShowStartPage()
		{
			if (this.startPage == null)
			{
				return;
			}
			this.startPage.SetRecent(this.LoadRecentRoms());
			this.startPage.CurrentRom = this.romData != null ? (string.IsNullOrEmpty(this.loadedRomPath) ? Localizer.T("（名前なし）") : Path.GetFileName(this.loadedRomPath)) : null;
			this.startPage.BringToFront();
			this.startPage.Visible = true;
		}

		//-------------------------------------------------------------------------------
		// 入口画面を隠して編集に戻る処理（ROM を読んでいないときは隠さない）
		//-------------------------------------------------------------------------------
		private void HideStartPage()
		{
			if (this.startPage == null || this.romData == null)
			{
				return;
			}
			this.startPage.Visible = false;
		}

		//-------------------------------------------------------------------------------
		// ROM の有無に合わせて入口画面を出し入れする処理（SetRomLoadedUI から呼ぶ）
		//-------------------------------------------------------------------------------
		private void UpdateStartPageVisibility(bool loaded)
		{
			if (this.startPage == null)
			{
				return;
			}
			if (!loaded)
			{
				this.ShowStartPage();
				return;
			}
			this.startPage.Visible = false;
		}

		//-------------------------------------------------------------------------------
		// 場所を指定して ROM を開く処理（最近開いた ROM・ドロップから。見つからないときは一覧から外すかを聞く）
		//-------------------------------------------------------------------------------
		internal void OpenRomFromPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			if (!File.Exists(path))
			{
				if (MessageBox.Show(this, Localizer.F("ファイルが見つかりません:\n{0}\n\n最近開いた ROM の一覧から外しますか？", path), Localizer.T("ROM を開く"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					this.RemoveRecentRom(path);
				}
				return;
			}
			if (!this.ConfirmSaveIfNeeded() || !this.ConfirmWildPokemonSaved())
			{
				return;
			}
			byte[] data;
			if (RomFile.TryRead(this, path, out data))
			{
				this.ApplyLoadedRom(data, path);
				this.ShowToolPane();
			}
		}

		//-------------------------------------------------------------------------------
		// settings.ini から最近開いた ROM の一覧を読む処理（新しい順）
		//-------------------------------------------------------------------------------
		private List<string> LoadRecentRoms()
		{
			string saved = AppSettings.Get(RecentRomsSetting) ?? string.Empty;
			return saved.Split(new[] { RecentRomsSeparator }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Take(RecentRomLimit).ToList();
		}

		//-------------------------------------------------------------------------------
		// 一覧を settings.ini に書く処理
		//-------------------------------------------------------------------------------
		private void SaveRecentRoms(IEnumerable<string> paths)
		{
			AppSettings.Set(RecentRomsSetting, string.Join(RecentRomsSeparator.ToString(), paths.Take(RecentRomLimit)));
		}

		//-------------------------------------------------------------------------------
		// 開いた ROM を一覧の先頭に入れる処理（ROM の読み込みに成功したときに呼ぶ。場所が無いときは何もしない）
		//-------------------------------------------------------------------------------
		private void RememberRecentRom(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			string full;
			try
			{
				full = Path.GetFullPath(path);
			}
			catch (Exception)
			{
				return;
			}
			List<string> list = this.LoadRecentRoms();
			list.RemoveAll(p => string.Equals(p, full, StringComparison.OrdinalIgnoreCase));
			list.Insert(0, full);
			this.SaveRecentRoms(list);
		}

		//-------------------------------------------------------------------------------
		// 一覧から 1 件外す処理（null なら全部消す）。入口画面が出ていれば描き直す
		//-------------------------------------------------------------------------------
		private void RemoveRecentRom(string path)
		{
			List<string> list = path == null ? new List<string>() : this.LoadRecentRoms().Where(p => !string.Equals(p, path, StringComparison.OrdinalIgnoreCase)).ToList();
			this.SaveRecentRoms(list);
			if (this.startPage != null && this.startPage.Visible)
			{
				this.startPage.SetRecent(list);
			}
		}
	}
}
