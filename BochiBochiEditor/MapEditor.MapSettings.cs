using System;
using System.Drawing;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// メインタブ「マップの設定」（元の「詳細」タブ）: 目的ごとの枠と各項目の説明、BGM の曲名選択、詳細な値の折りたたみ
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// BGM の一覧と番号欄を互いに合わせている最中か（合わせる処理どうしで呼び合わないように）
		private bool isSyncingMusic;
		// BGM の一覧を作ったときの ROM（別の ROM を開いたら作り直す）
		private byte[] musicListRom;

		// 一覧の 1 項目（曲番号と表示名）
		private sealed class MusicItem
		{
			public int Id;
			public string Label;

			//-------------------------------------------------------------------------------
			// 一覧に出す文字を返す処理
			//-------------------------------------------------------------------------------
			public override string ToString()
			{
				return this.Label;
			}
		}

		//-------------------------------------------------------------------------------
		// 「マップの設定」タブの初期化処理
		//-------------------------------------------------------------------------------
		private void InitializeMapSettingsTab()
		{
			this.RebuildMusicList();
			this.cmbMusic.SelectedIndexChanged += this.cmbMusic_SelectedIndexChanged;
			this.nudMusicCode.ValueChanged += (sender, e) => this.SyncMusicListToCode();
			// マップデータの場所（アドレス）の枠は、中身の枠と同じときに押せる／押せないを切り替える
			this.grpMapFooter.EnabledChanged += (sender, e) => this.grpFooterAddress.Enabled = this.grpMapFooter.Enabled;
			// 大きさは、数字を変えるだけではデータが作り直されず危ないので、表示だけにする（変えるのは「大きさを変える…」= MapEditor.Resize.cs）
			foreach (NumericUpDown size in new[] { this.nudMapWidth, this.nudMapHeight, this.nudBorderWidth, this.nudBorderHeight })
			{
				size.Enabled = false;
			}
			this.ApplyMapSettingsDescriptionColors();
			this.UpdateMapSettingsAdvancedButton();
			// 形の使い回しの説明画像は、幅に合わせて高さを決める（縦横比を保ち、上下に余白を作らない）
			this.picTerrainShareNote.SizeChanged += (sender, e) => this.FitTerrainShareNoteHeight();
			// 枠の高さは中の表に合わせる（説明文が折り返して表が高くなっても、下が切れないように）
			foreach (var card in new (GroupBox Group, TableLayoutPanel Table)[] { (this.grpPlaceInfo, this.tlpPlaceInfo), (this.grpAtmosphere, this.tlpAtmosphere),
				(this.grpMapFooter, this.tlpMapFooter), (this.grpFooterAddress, this.tlpFooterAddress) })
			{
				GroupBox group = card.Group;
				TableLayoutPanel table = card.Table;
				group.AutoSize = false;
				table.SizeChanged += (sender, e) => FitCardHeight(group, table);
				FitCardHeight(group, table);
			}
		}

		//-------------------------------------------------------------------------------
		// 枠の高さを、中の表がちょうど収まる高さにする処理
		//-------------------------------------------------------------------------------
		private static void FitCardHeight(GroupBox group, TableLayoutPanel table)
		{
			int height = table.Bottom + group.Padding.Bottom + 2;
			if (group.Height != height)
			{
				group.Height = height;
			}
		}

		//-------------------------------------------------------------------------------
		// 各項目の説明文を、見出しより控えめな色にする処理（テーマを当てた後に呼ぶ）
		//-------------------------------------------------------------------------------
		private void ApplyMapSettingsDescriptionColors()
		{
			foreach (Label label in new[] { this.lblDescMapName, this.lblDescMapNameType, this.lblDescTerrainType, this.lblDescBicycle, this.lblDescLevel,
				this.lblDescTerrainId, this.lblDescMusic, this.lblDescWeather, this.lblDescSight, this.lblDescBattleType, this.lblDescTilesets,
				this.lblDescMapSize, this.lblDescBorderSize, this.lblDescFooterAddress, this.lblMapSettingsNote, this.lblDescTerrainSync })
			{
				label.ForeColor = UiTheme.TextMuted;
			}
		}

		//-------------------------------------------------------------------------------
		// 別の ROM を開いていたら、BGM の一覧をその ROM の曲で作り直す処理
		//-------------------------------------------------------------------------------
		private void RebuildMusicListIfRomChanged()
		{
			if (this.musicListRom != this.romData)
			{
				this.RebuildMusicList();
			}
		}

		//-------------------------------------------------------------------------------
		// BGM の一覧を作り直す処理（選んでいる曲はそのまま）
		// ROM の曲の表が見つかれば、その ROM にある曲（ファイアレッドは 256 番以降、エメラルドは 350 番以降）を並べ、中身を元のゲームと見比べて名前を付ける
		//   元のまま → 曲名、差し替え → 「差し替えられた曲（元は ○○）」、追加 → 「追加された曲」
		// 見つからなければ、元のゲームの曲の一覧を出す
		//-------------------------------------------------------------------------------
		private void RebuildMusicList()
		{
			this.musicListRom = this.romData;
			SongTable table = SongTable.Find(this.romData);
			this.isSyncingMusic = true;
			try
			{
				this.cmbMusic.BeginUpdate();
				this.cmbMusic.Items.Clear();
				if (table != null)
				{
					for (int id = BgmCatalog.FirstMusicId; id < table.Count; id++)
					{
						// 元のゲームで声の効果音が入っている番号（英語版エメラルド）は、効果音のままなら曲の一覧に出さない
						if (BgmCatalog.IsOriginalSoundEffectSlot(id) && table.GetPlayer(id) != 0)
						{
							continue;
						}
						this.cmbMusic.Items.Add(new MusicItem { Id = id, Label = DescribeSong(table, id) });
					}
					// 曲の表に入っていない特別な番号（「なし」など）
					foreach (int id in BgmCatalog.SpecialIds)
					{
						this.cmbMusic.Items.Add(new MusicItem { Id = id, Label = string.Format("{0} ({1})", BgmCatalog.GetName(id), id) });
					}
				}
				else
				{
					foreach (int id in BgmCatalog.Ids)
					{
						this.cmbMusic.Items.Add(new MusicItem { Id = id, Label = string.Format("{0} ({1})", BgmCatalog.GetName(id), id) });
					}
				}
				this.cmbMusic.EndUpdate();
			}
			finally
			{
				this.isSyncingMusic = false;
			}
			this.SyncMusicListToCode();
		}

		//-------------------------------------------------------------------------------
		// ROM の曲 1 つを、一覧に出す名前にする処理
		//-------------------------------------------------------------------------------
		private static string DescribeSong(SongTable table, int id)
		{
			string print = table.Fingerprint(id);
			switch (table.GetKind(id))
			{
				case SongTable.SongKind.Original:
					return string.Format("{0} ({1})", BgmCatalog.GetName(id), id);
				case SongTable.SongKind.Replaced:
				{
					// 別の番号にあった元の曲がここへ移されていれば、その曲名を出す
					int moved = BgmCatalog.FindByPrint(print);
					return moved >= 0
						? string.Format(Localizer.T("{0} ({1}) ※元の {2} 番の曲"), BgmCatalog.GetName(moved), id, moved)
						: string.Format(Localizer.T("差し替えられた曲 ({0}) ※元は{1}"), id, BgmCatalog.GetName(id));
				}
				default:
				{
					int moved = BgmCatalog.FindByPrint(print);
					return moved >= 0
						? string.Format(Localizer.T("{0} ({1}) ※元の {2} 番の曲"), BgmCatalog.GetName(moved), id, moved)
						: string.Format(Localizer.T("追加された曲 ({0})"), id);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// BGM の説明欄に、選んでいる曲をこの ROM のどのマップが使っているかを書き足す処理
		//-------------------------------------------------------------------------------
		private void UpdateMusicUsage()
		{
			string text = Localizer.T("このマップで流れる曲です。一覧から選ぶか、右の欄に番号を直接入れます。");
			if (this.romData != null && this.tempHeader != null && this.mapHeaders != null)
			{
				int code = (int)this.nudMusicCode.Value;
				System.Collections.Generic.List<string> users = new System.Collections.Generic.List<string>();
				foreach (MapHeader header in this.mapHeaders)
				{
					if (header.MusicCode == code)
					{
						users.Add(header.GetMapName(this));
					}
				}
				System.Collections.Generic.List<string> names = new System.Collections.Generic.List<string>(new System.Collections.Generic.HashSet<string>(users));
				text += Environment.NewLine + (users.Count == 0
					? Localizer.T("この曲を使っているマップは、ほかにありません。")
					: string.Format(Localizer.T("この曲を使っているマップ（{0} 件）: {1}"), users.Count, string.Join(Localizer.T("、"), names.GetRange(0, Math.Min(6, names.Count))) + (names.Count > 6 ? Localizer.T(" ほか") : string.Empty)));
			}
			this.lblDescMusic.Text = text;
		}

		//-------------------------------------------------------------------------------
		// 一覧で曲を選んだら、番号欄をその曲の番号にする処理（番号欄の変更が保存対象になる）
		//-------------------------------------------------------------------------------
		private void cmbMusic_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.isSyncingMusic)
			{
				return;
			}
			MusicItem item = this.cmbMusic.SelectedItem as MusicItem;
			if (item == null || item.Id < 0)
			{
				return;
			}
			decimal value = Math.Max(this.nudMusicCode.Minimum, Math.Min(this.nudMusicCode.Maximum, item.Id));
			if (this.nudMusicCode.Value != value)
			{
				this.nudMusicCode.Value = value;
			}
		}

		//-------------------------------------------------------------------------------
		// 番号欄の曲を一覧で選んだ状態にする処理（一覧に無い番号は「一覧にない曲」として一時的に足す）
		//-------------------------------------------------------------------------------
		private void SyncMusicListToCode()
		{
			if (this.isSyncingMusic)
			{
				return;
			}
			this.isSyncingMusic = true;
			try
			{
				// 前に足した「一覧にない曲」は消しておく
				for (int i = this.cmbMusic.Items.Count - 1; i >= 0; i--)
				{
					MusicItem old = this.cmbMusic.Items[i] as MusicItem;
					if (old != null && old.Id < 0)
					{
						this.cmbMusic.Items.RemoveAt(i);
					}
				}
				if (this.romData == null || this.tempHeader == null)
				{
					this.cmbMusic.SelectedIndex = -1;
					return;
				}
				int code = (int)this.nudMusicCode.Value;
				this.UpdateMusicUsage();
				for (int i = 0; i < this.cmbMusic.Items.Count; i++)
				{
					if (((MusicItem)this.cmbMusic.Items[i]).Id == code)
					{
						this.cmbMusic.SelectedIndex = i;
						return;
					}
				}
				// 改造 ROM で足した曲など、表に無い番号（番号は Id を負にして区別し、選んでも番号欄を変えない）
				MusicItem unknown = new MusicItem { Id = -1 - code, Label = string.Format(Localizer.T("一覧にない曲 ({0})"), code) };
				this.cmbMusic.Items.Insert(0, unknown);
				this.cmbMusic.SelectedIndex = 0;
			}
			finally
			{
				this.isSyncingMusic = false;
			}
		}

		//-------------------------------------------------------------------------------
		// 「詳細な値」を開く／閉じる処理
		//-------------------------------------------------------------------------------
		private void btnToggleMapSettingsAdvanced_Click(object sender, EventArgs e)
		{
			this.flpMapSettingsAdvanced.Visible = !this.flpMapSettingsAdvanced.Visible;
			this.UpdateMapSettingsAdvancedButton();
		}

		//-------------------------------------------------------------------------------
		// 「詳細な値」ボタンの文字を、開いているかどうかに合わせる処理
		//-------------------------------------------------------------------------------
		private void UpdateMapSettingsAdvancedButton()
		{
			this.btnToggleMapSettingsAdvanced.Text = this.flpMapSettingsAdvanced.Visible
				? Localizer.T("▼ 詳細な値を隠す")
				: Localizer.T("▶ 詳細な値を表示（アドレス・タイルセットの内部設定）");
		}

		//-------------------------------------------------------------------------------
		// 形の使い回しの説明画像の高さを、今の幅と画像の縦横比から決める処理（値が変わるときだけ設定する）
		//-------------------------------------------------------------------------------
		private void FitTerrainShareNoteHeight()
		{
			Image image = this.picTerrainShareNote.Image;
			if (image == null || image.Width <= 0 || this.picTerrainShareNote.Width <= 0)
			{
				return;
			}
			int height = this.picTerrainShareNote.Width * image.Height / image.Width;
			if (this.picTerrainShareNote.Height == height || !this.IsHandleCreated)
			{
				return;
			}
			// 並べ直しの途中で高さを変えると、下の行（ボタンなど）の位置が更新されず重なるため、並べ直しが終わってから変える
			this.BeginInvoke(new Action(() =>
			{
				if (this.picTerrainShareNote.Height != height)
				{
					this.picTerrainShareNote.Height = height;
					this.tlpPlaceInfo.PerformLayout();
				}
			}));
		}

		//-------------------------------------------------------------------------------
		// 入れ物の中の入力欄を、中の入れ物までたどって空にする処理（枠の中に枠がある「マップの設定」タブ用）
		//-------------------------------------------------------------------------------
		private void ResetControlsDeep(Control container)
		{
			foreach (Control control in container.Controls)
			{
				if (control is TextBox text)
				{
					text.Text = string.Empty;
				}
				else if (control is NumericUpDown number)
				{
					number.Value = Math.Max(number.Minimum, Math.Min(number.Maximum, 0m));
				}
				else if (control is ComboBox combo)
				{
					combo.SelectedIndex = -1;
				}
				else if (control is CheckBox check)
				{
					// 「確定するときに地形データの表も自動で合わせる」はマップの値ではなく設定なので、空にしない
					if (check != this.chkSyncTerrainId)
					{
						check.Checked = false;
					}
				}
				else if (control.HasChildren)
				{
					this.ResetControlsDeep(control);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マップ・ボーダーの大きさが変えられていないか確かめる処理（変えられていたら元に戻して案内し、false を返す）
		// 数字だけを変えると、データの並びがずれてマップが崩れたり、後ろにある別のデータを壊したりするため
		//-------------------------------------------------------------------------------
		private bool CheckMapSizeChangeAllowed()
		{
			if (this.tempFooter == null)
			{
				return true;
			}
			bool changed = this.nudMapWidth.Value != this.tempFooter.MapWidth || this.nudMapHeight.Value != this.tempFooter.MapHeight
				|| this.nudBorderWidth.Value != this.tempFooter.BorderWidth || this.nudBorderHeight.Value != this.tempFooter.BorderHeight;
			if (!changed)
			{
				return true;
			}
			this.nudMapWidth.Value = this.tempFooter.MapWidth;
			this.nudMapHeight.Value = this.tempFooter.MapHeight;
			this.nudBorderWidth.Value = this.tempFooter.BorderWidth;
			this.nudBorderHeight.Value = this.tempFooter.BorderHeight;
			MessageBox.Show(this, Localizer.T("マップやボーダーの大きさは、「大きさを変える…」のボタンから変えてください。数字だけを変えるとマップが崩れたり、ROM の別のデータを壊したりするためです。大きさを元に戻しました。"),
				Localizer.T("マップの設定"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			return false;
		}
	}
}
