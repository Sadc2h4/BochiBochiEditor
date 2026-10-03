using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	public partial class MapEditor
	{
		private MapPointersForm mapPointersForm;
		private Dictionary<uint, List<string>> mapPointerIndex;
		private byte[] mapPointerIndexRom;
		private bool isSyncingPointerSelection;

		//-------------------------------------------------------------------------------
		// マップの設定にあるポインタ一覧ボタンの操作を登録する処理
		//-------------------------------------------------------------------------------
		private void InitializeMapPointers()
		{
			this.UpdateEventPointerButtonImage();
			this.btnShowMapPointers.Click += this.btnShowMapPointers_Click;
			this.btnEventPointers.Click += (sender, e) => this.ShowMapPointers();
			this.cmbEventType.SelectedIndexChanged += this.MapPointerEventSelectionChanged;
			this.nudEventNo.ValueChanged += this.MapPointerEventSelectionChanged;
			this.nudEventNo.EnabledChanged += this.MapPointerEventSelectionChanged;
			this.tabEditorMode.SelectedIndexChanged += this.MapPointerEventSelectionChanged;
			this.mapToolTip.SetToolTip(this.btnEventPointers, Localizer.T("このマップのポインタ（置き場所・値・バイト列）の一覧を開きます。アドレスの参照元も探せます（Ctrl+Shift+P）。"));
		}

		//-------------------------------------------------------------------------------
		// ポインタ一覧ボタンの画像を現在の表示言語に合わせる処理
		//-------------------------------------------------------------------------------
		private void UpdateEventPointerButtonImage()
		{
			this.btnEventPointers.ButtonImage = this.LoadButtonImage("Pointa_List");
		}

		//-------------------------------------------------------------------------------
		// イベントの種類・番号・編集モードの変更をポインタ一覧へ反映する処理
		//-------------------------------------------------------------------------------
		private void MapPointerEventSelectionChanged(object sender, EventArgs e)
		{
			this.HighlightSelectedPointerEvent();
		}

		//-------------------------------------------------------------------------------
		// 選択中のイベントを強調し、一覧からの操作中は一覧の選択位置を保つ処理
		//-------------------------------------------------------------------------------
		private void HighlightSelectedPointerEvent()
		{
			if (this.mapPointersForm == null || this.mapPointersForm.IsDisposed) return;
			string type = this.cmbEventType.SelectedItem?.ToString();
			string kind = type == "歩行グラフィック" || type == Localizer.T("歩行グラフィック") ? "person"
				: type == "踏むスクリプト" || type == Localizer.T("踏むスクリプト") ? "trap"
				: type == "看板" || type == Localizer.T("看板") ? "sign" : null;
			int index = Convert.ToInt32(this.nudEventNo.Value);
			if (this.tabEditorMode.SelectedTab != this.tabEvent || !this.nudEventNo.Enabled
				|| !this.TryGetPointerEventPosition(kind, index, out _, out _)) kind = null;
			this.mapPointersForm.HighlightEvent(kind, index, !this.isSyncingPointerSelection);
		}

		//-------------------------------------------------------------------------------
		// 編集中のイベントが存在するか確認し、そのマスの座標を取得する処理
		//-------------------------------------------------------------------------------
		private bool TryGetPointerEventPosition(string kind, int index, out int x, out int y)
		{
			x = 0;
			y = 0;
			if (this.tempHeader == null || index < 0) return false;
			switch (kind)
			{
				case "person":
					if (this.tempHeader.Persons == null || index >= this.tempHeader.Persons.Count) return false;
					x = this.tempHeader.Persons[index].X;
					y = this.tempHeader.Persons[index].Y;
					return true;
				case "trap":
					if (this.tempHeader.Traps == null || index >= this.tempHeader.Traps.Count) return false;
					x = this.tempHeader.Traps[index].X;
					y = this.tempHeader.Traps[index].Y;
					return true;
				case "sign":
					if (this.tempHeader.Signs == null || index >= this.tempHeader.Signs.Count) return false;
					x = this.tempHeader.Signs[index].X;
					y = this.tempHeader.Signs[index].Y;
					return true;
				default:
					return false;
			}
		}

		//-------------------------------------------------------------------------------
		// 一覧で選んだイベントへ編集欄を切り替え、編集中の座標へマップを移動する処理
		//-------------------------------------------------------------------------------
		private void MapPointersForm_EventRowSelected(object sender, MapPointerEntry entry)
		{
			if (this.isSyncingPointerSelection || this.romData == null || entry == null
				|| !this.TryGetPointerEventPosition(entry.EventKind, entry.EventIndex, out int x, out int y)) return;
			string type = entry.EventKind == "person" ? "歩行グラフィック" : entry.EventKind == "trap" ? "踏むスクリプト" : "看板";
			int typeIndex = this.cmbEventType.Items.IndexOf(Localizer.T(type));
			if (typeIndex < 0) typeIndex = this.cmbEventType.Items.IndexOf(type);
			if (typeIndex < 0) return;
			this.isSyncingPointerSelection = true;
			try
			{
				this.tabMain.SelectedTab = this.tabMapEdit;
				this.SetEditorMode(this.tabEvent);
				this.cmbEventType.SelectedIndex = typeIndex;
				if (!this.nudEventNo.Enabled || entry.EventIndex < this.nudEventNo.Minimum || entry.EventIndex > this.nudEventNo.Maximum) return;
				this.nudEventNo.Value = entry.EventIndex;
				this.ScrollMapToCell(x, y);
				this.pnlMapCanvas.Invalidate();
			}
			finally
			{
				this.HighlightSelectedPointerEvent();
				this.isSyncingPointerSelection = false;
			}
		}

		//-------------------------------------------------------------------------------
		// 接続マップの余白と拡大率を含め、指定のマスを表示範囲の中央付近へ移す処理
		//-------------------------------------------------------------------------------
		private void ScrollMapToCell(int x, int y)
		{
			int cellSize = 16 * this.GetMapZoomScale();
			if (this.hsbMapDataPreview.Enabled)
			{
				int maximum = Math.Max(this.hsbMapDataPreview.Minimum, this.hsbMapDataPreview.Maximum - this.hsbMapDataPreview.LargeChange + 1);
				int center = (x + this.primaryMapOffsetX) * cellSize + cellSize / 2;
				int value = Math.Max(this.hsbMapDataPreview.Minimum, Math.Min(maximum, center - this.pnlMapCanvas.ClientSize.Width / 2));
				if (this.hsbMapDataPreview.Value != value) this.hsbMapDataPreview.Value = value;
			}
			if (this.vsbMapDataPreview.Enabled)
			{
				int maximum = Math.Max(this.vsbMapDataPreview.Minimum, this.vsbMapDataPreview.Maximum - this.vsbMapDataPreview.LargeChange + 1);
				int center = (y + this.primaryMapOffsetY) * cellSize + cellSize / 2;
				int value = Math.Max(this.vsbMapDataPreview.Minimum, Math.Min(maximum, center - this.pnlMapCanvas.ClientSize.Height / 2));
				if (this.vsbMapDataPreview.Value != value) this.vsbMapDataPreview.Value = value;
			}
		}

		//-------------------------------------------------------------------------------
		// ボタンまたはメニューからポインタ一覧を開く処理
		//-------------------------------------------------------------------------------
		private void btnShowMapPointers_Click(object sender, EventArgs e)
		{
			this.ShowMapPointers();
		}

		//-------------------------------------------------------------------------------
		// 今のマップのポインタ一覧を開き、選択中のイベントの行を示す処理
		//-------------------------------------------------------------------------------
		private void ShowMapPointers()
		{
			if (this.romData == null) return;
			// マップを選んでいないときも開けるようにする（中身は空で、マップを選ぶと一覧が出る）
			bool hasMap = this.tempHeader != null;
			List<MapPointerEntry> entries = hasMap ? MapPointerReport.Collect(this.romData, this.tempHeader.Bank, this.tempHeader.Number) : new List<MapPointerEntry>();
			string summary = hasMap ? this.BuildMapPointersSummary() : BuildMapPointersEmptySummary();
			if (this.mapPointersForm != null && !this.mapPointersForm.IsDisposed)
			{
				this.mapPointersForm.SetEntries(entries, summary, this.romData);
				if (this.mapPointersForm.WindowState == FormWindowState.Minimized) this.mapPointersForm.WindowState = FormWindowState.Normal;
				this.mapPointersForm.BringToFront();
				this.mapPointersForm.Activate();
			}
			else
			{
				this.mapPointersForm = new MapPointersForm(entries, summary, this.romData, this.DescribePointerLocation);
				this.mapPointersForm.EventRowSelected += this.MapPointersForm_EventRowSelected;
				AppIconHelper.Apply(this.mapPointersForm);
				UiTheme.Apply(this.mapPointersForm);
				this.mapPointersForm.Show(this);
			}
			this.HighlightSelectedPointerEvent();
		}

		//-------------------------------------------------------------------------------
		// マップを選んでいないときに、一覧の上へ出す案内文を作る処理
		//-------------------------------------------------------------------------------
		private static string BuildMapPointersEmptySummary()
		{
			return Localizer.T("マップが選ばれていません。左の一覧からマップを選ぶと、ここにポインタの一覧が出ます。");
		}

		//-------------------------------------------------------------------------------
		// マップ名とポインタの置き場所の意味を説明する文を作る処理
		//-------------------------------------------------------------------------------
		private string BuildMapPointersSummary()
		{
			return string.Format(Localizer.T("({0}, {1}) {2} のポインタ一覧（確定済みの ROM）。{3}置き場所 = そのポインタの 4 バイトが書かれている場所のアドレスです。"),
				this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this), Environment.NewLine);
		}

		//-------------------------------------------------------------------------------
		// 一覧が開いていれば、マップの切り替え・確定後の内容を反映する処理
		//-------------------------------------------------------------------------------
		private void RefreshMapPointersIfOpen()
		{
			this.mapPointerIndex = null;
			this.mapPointerIndexRom = null;
			if (this.mapPointersForm == null || this.mapPointersForm.IsDisposed) return;
			if (this.romData == null || this.tempHeader == null)
			{
				// マップを選び直す途中（バンクの見出しを選んだとき、ROM を開き直したときなど）は、
				// ウィンドウを閉じずに中身だけ空にする（検索の結果は ROM が同じなら残る）
				this.mapPointersForm.SetEntries(new List<MapPointerEntry>(), BuildMapPointersEmptySummary(), this.romData);
				this.mapPointersForm.HighlightEvent(null, -1, false);
				return;
			}
			this.mapPointersForm.SetEntries(MapPointerReport.Collect(this.romData, this.tempHeader.Bank, this.tempHeader.Number), this.BuildMapPointersSummary(), this.romData);
			this.HighlightSelectedPointerEvent();
		}

		//-------------------------------------------------------------------------------
		// ROM の開き直しやエディタの終了時にポインタ一覧を閉じる処理
		//-------------------------------------------------------------------------------
		private void CloseMapPointersForm()
		{
			this.mapPointerIndex = null;
			this.mapPointerIndexRom = null;
			if (this.mapPointersForm != null && !this.mapPointersForm.IsDisposed)
			{
				this.mapPointersForm.Close();
			}
			this.mapPointersForm = null;
		}

		//-------------------------------------------------------------------------------
		// 全マップのポインタの置き場所と説明を集め、必要になった時だけ索引を作る処理
		//-------------------------------------------------------------------------------
		private void BuildMapPointerIndex()
		{
			byte[] rom = this.romData;
			Dictionary<uint, List<string>> index = new Dictionary<uint, List<string>>();
			if (rom != null && this.mapHeaders != null)
			{
				foreach (MapHeader header in this.mapHeaders)
				{
					try
					{
						string mapName = header.GetMapName(this);
						foreach (MapPointerEntry entry in MapPointerReport.Collect(rom, header.Bank, header.Number))
						{
							if (!index.TryGetValue(entry.Location, out List<string> descriptions))
							{
								descriptions = new List<string>();
								index.Add(entry.Location, descriptions);
							}
							descriptions.Add(string.Format(Localizer.T("({0}, {1}) {2}: {3} / {4}"),
								header.Bank, header.Number, mapName, Localizer.T(entry.Group), Localizer.T(entry.Name)));
						}
					}
					catch (Exception)
					{
						// 読み取れないマップがあっても、残りのマップの索引を作る。
					}
				}
			}
			this.mapPointerIndex = index;
			this.mapPointerIndexRom = rom;
		}

		//-------------------------------------------------------------------------------
		// 参照元の場所を説明し、共通の場所は先頭 3 件と残りの件数を示す処理
		//-------------------------------------------------------------------------------
		private string DescribePointerLocation(uint location)
		{
			if (this.mapPointerIndex == null || !ReferenceEquals(this.mapPointerIndexRom, this.romData)) this.BuildMapPointerIndex();
			if (!this.mapPointerIndex.TryGetValue(location, out List<string> descriptions) || descriptions.Count == 0)
			{
				return Localizer.T("（マップのデータの表には無い場所。スクリプトの中や、別の表の可能性）");
			}
			string result = string.Join(" ／ ", descriptions.GetRange(0, Math.Min(3, descriptions.Count)));
			if (descriptions.Count > 3) result += " ／ " + string.Format(Localizer.T("ほか {0} 件"), descriptions.Count - 3);
			return result;
		}

		//-------------------------------------------------------------------------------
		// 既存のイベント用メニューへバイト列のコピーと一覧を開く項目を足す処理
		//-------------------------------------------------------------------------------
		private void ExtendEventScriptPointerContextMenu()
		{
			if (this.eventScriptPointerContextMenu == null || this.eventScriptPointerContextMenu.Items.ContainsKey("copyEventScriptBytes")) return;
			ToolStripMenuItem copyBytes = new ToolStripMenuItem(Localizer.T("バイト列（リトルエンディアン）のコピー"));
			copyBytes.Name = "copyEventScriptBytes";
			copyBytes.Click += this.CopyEventScriptBytesMenuItem_Click;
			ToolStripMenuItem showPointers = new ToolStripMenuItem(Localizer.T("このマップのポインタ一覧を開く…"));
			showPointers.Click += this.btnShowMapPointers_Click;
			this.eventScriptPointerContextMenu.Items.Add(copyBytes);
			this.eventScriptPointerContextMenu.Items.Add(new ToolStripSeparator());
			this.eventScriptPointerContextMenu.Items.Add(showPointers);
			this.eventScriptPointerContextMenu.Opening += (sender, e) =>
			{
				uint location = 0;
				string error = string.Empty;
				copyBytes.Text = Localizer.T("バイト列（リトルエンディアン）のコピー");
				showPointers.Text = Localizer.T("このマップのポインタ一覧を開く…");
				copyBytes.Enabled = this.romData != null && this.TryGetSelectedEventScriptPointerOffset(ref location, ref error);
				showPointers.Enabled = this.romData != null && this.tempHeader != null;
			};
		}

		//-------------------------------------------------------------------------------
		// 選択中のイベントのスクリプト欄の 4 バイトをコピーして案内する処理
		//-------------------------------------------------------------------------------
		private void CopyEventScriptBytesMenuItem_Click(object sender, EventArgs e)
		{
			uint location = 0;
			string error = string.Empty;
			if (!this.TryGetSelectedEventScriptPointerOffset(ref location, ref error))
			{
				MessageBox.Show(error, "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			if (this.romData == null || !this.IsRomRange(location, 4)) return;
			string bytes = MapPointerReport.FormatBytes(new MapPointerEntry { RawValue = BitConverter.ToUInt32(this.romData, (int)location) });
			try
			{
				Clipboard.SetText(bytes);
				MessageBox.Show(string.Format(Localizer.T("バイト列 {0} をコピーしました。"), bytes), "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			catch (Exception)
			{
				MessageBox.Show(Localizer.T("クリップボードへコピーできませんでした。"), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}
}
