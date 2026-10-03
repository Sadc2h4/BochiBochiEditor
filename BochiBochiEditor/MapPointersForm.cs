using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	public partial class MapPointersForm : Form
	{
		private readonly List<MapPointerEntry> entries = new List<MapPointerEntry>();
		private byte[] rom;
		private readonly Func<uint, string> describeLocation;
		private bool isUpdatingSelection;
		private string highlightedEventKind;
		private int highlightedEventIndex = -1;
		private DataGridViewRow highlightedEventRow;
		internal event EventHandler<MapPointerEntry> EventRowSelected;

		//-------------------------------------------------------------------------------
		// デザイナー用の初期化と、部品の文字の翻訳を行う処理
		//-------------------------------------------------------------------------------
		public MapPointersForm()
		{
			InitializeComponent();
			this.dgvMapPointers.CurrentCellChanged += this.dgvMapPointers_CurrentCellChanged;
			this.dgvMapPointers.CellClick += this.dgvMapPointers_CellClick;
			Localizer.Apply(this);
			foreach (DataGridViewColumn column in this.dgvMapPointers.Columns.Cast<DataGridViewColumn>().Concat(this.dgvReferences.Columns.Cast<DataGridViewColumn>()))
			{
				column.HeaderText = Localizer.T(column.HeaderText);
			}
			foreach (ToolStripItem item in this.mapPointersContextMenu.Items.Cast<ToolStripItem>().Concat(this.referencesContextMenu.Items.Cast<ToolStripItem>()))
			{
				item.Text = Localizer.T(item.Text);
			}
		}

		//-------------------------------------------------------------------------------
		// 集めたポインタとマップの説明を受け取って初期化する処理
		//-------------------------------------------------------------------------------
		internal MapPointersForm(List<MapPointerEntry> entries, string summary, byte[] rom, Func<uint, string> describeLocation) : this()
		{
			this.describeLocation = describeLocation;
			this.SetEntries(entries, summary, rom);
		}

		//-------------------------------------------------------------------------------
		// 表示前にテーブル情報画面と同じ配色を一覧へ適用する処理
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			foreach (DataGridView grid in new[] { this.dgvMapPointers, this.dgvReferences })
			{
				grid.EnableHeadersVisualStyles = false;
				grid.BackgroundColor = UiTheme.Canvas;
				grid.GridColor = UiTheme.Border;
				grid.BorderStyle = BorderStyle.None;
				grid.DefaultCellStyle.BackColor = UiTheme.Input;
				grid.DefaultCellStyle.ForeColor = UiTheme.Text;
				grid.DefaultCellStyle.SelectionBackColor = UiTheme.Accent;
				grid.DefaultCellStyle.SelectionForeColor = UiTheme.AccentText;
				grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Card;
				grid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Text;
				grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = UiTheme.Card;
				grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			}
			this.lblMapPointersResult.ForeColor = UiTheme.TextMuted;
			this.lblFindResult.ForeColor = UiTheme.TextMuted;
			this.lblFindBytes.ForeColor = UiTheme.TextMuted;
		}

		//-------------------------------------------------------------------------------
		// 開いたままの一覧を今のマップの内容へ入れ替える処理
		//-------------------------------------------------------------------------------
		internal void SetEntries(List<MapPointerEntry> entries, string summary, byte[] rom)
		{
			// 検索の結果は、別の ROM に変わったときだけ消す（マップを切り替えても残す）
			if (!ReferenceEquals(this.rom, rom))
			{
				this.dgvReferences.Rows.Clear();
				this.lblFindResult.Text = string.Empty;
			}
			this.rom = rom;
			this.entries.Clear();
			if (entries != null) this.entries.AddRange(entries);
			this.lblMapPointersSummary.Text = summary;
			this.FillRows();
		}

		//-------------------------------------------------------------------------------
		// 絞り込みに合わせて一覧を作り、ポインタでない値を薄い文字で示す処理
		//-------------------------------------------------------------------------------
		private void FillRows()
		{
			bool wasUpdatingSelection = this.isUpdatingSelection;
			this.isUpdatingSelection = true;
			this.dgvMapPointers.SuspendLayout();
			try
			{
				this.highlightedEventRow = null;
				this.dgvMapPointers.Rows.Clear();
				foreach (MapPointerEntry entry in this.entries)
				{
					if (this.chkPointersOnlyEvents.Checked && entry.Group != "イベント" && entry.Group != "人物"
						&& entry.Group != "踏むスクリプト" && entry.Group != "看板" && entry.Group != "マップスクリプト") continue;
					int index = this.dgvMapPointers.Rows.Add(Localizer.T(entry.Group), entry.Name,
						MapPointerReport.FormatAddress(entry.Location), MapPointerReport.FormatValue(entry), MapPointerReport.FormatBytes(entry));
					DataGridViewRow row = this.dgvMapPointers.Rows[index];
					row.Tag = entry;
					if (!entry.IsPointer)
					{
						DataGridViewCell cell = row.Cells[this.colValue.Index];
						cell.Style.ForeColor = UiTheme.TextMuted;
						cell.Style.SelectionForeColor = UiTheme.TextMuted;
						cell.ToolTipText = Localizer.T("ポインタではない");
					}
				}
				this.HighlightEvent(this.highlightedEventKind, this.highlightedEventIndex);
			}
			finally
			{
				this.dgvMapPointers.ResumeLayout();
				this.isUpdatingSelection = wasUpdatingSelection;
			}
		}

		//-------------------------------------------------------------------------------
		// 指定されたイベントの行を選んで、見える位置までスクロールする処理
		//-------------------------------------------------------------------------------
		internal void SelectEvent(string kind, int index)
		{
			if (string.IsNullOrEmpty(kind) || index < 0) return;
			foreach (DataGridViewRow row in this.dgvMapPointers.Rows)
			{
				MapPointerEntry entry = row.Tag as MapPointerEntry;
				if (entry == null || entry.EventKind != kind || entry.EventIndex != index) continue;
				bool wasUpdatingSelection = this.isUpdatingSelection;
				this.isUpdatingSelection = true;
				try
				{
					this.dgvMapPointers.ClearSelection();
					this.dgvMapPointers.CurrentCell = row.Cells[this.colLocation.Index];
					row.Selected = true;
					this.dgvMapPointers.FirstDisplayedScrollingRowIndex = row.Index;
				}
				finally
				{
					this.isUpdatingSelection = wasUpdatingSelection;
				}
				return;
			}
		}

		//-------------------------------------------------------------------------------
		// 編集中のイベントの行を選択し、別の行を選んだ後も残る色で強調する処理
		//-------------------------------------------------------------------------------
		internal void HighlightEvent(string kind, int index)
		{
			this.HighlightEvent(kind, index, true);
		}

		//-------------------------------------------------------------------------------
		// 編集中のイベントの色と案内を更新し、必要なときだけその行を選択する処理
		//-------------------------------------------------------------------------------
		internal void HighlightEvent(string kind, int index, bool selectRow)
		{
			if (this.highlightedEventRow != null)
			{
				this.highlightedEventRow.DefaultCellStyle.BackColor = Color.Empty;
				this.highlightedEventRow.DefaultCellStyle.SelectionBackColor = Color.Empty;
				this.highlightedEventRow = null;
			}
			this.highlightedEventKind = kind;
			this.highlightedEventIndex = index;
			string name = kind == "person" ? Localizer.T("人物") : kind == "trap" ? Localizer.T("踏むスクリプト")
				: kind == "sign" ? Localizer.T("看板") : null;
			this.lblMapPointersResult.Text = name != null && index >= 0
				? string.Format(Localizer.T("選択中のイベント: {0} {1}"), name, index + 1)
				: Localizer.T("薄い文字の値はポインタではありません。");
			if (name == null || index < 0) return;
			foreach (DataGridViewRow row in this.dgvMapPointers.Rows)
			{
				if (!(row.Tag is MapPointerEntry entry) || entry.EventKind != kind || entry.EventIndex != index) continue;
				this.highlightedEventRow = row;
				Color highlight = ControlPaint.Dark(UiTheme.Warning);
				row.DefaultCellStyle.BackColor = highlight;
				row.DefaultCellStyle.SelectionBackColor = highlight;
				if (selectRow) this.SelectEvent(kind, index);
				return;
			}
		}

		//-------------------------------------------------------------------------------
		// キー操作で現在の行が変わったときに通知する処理（クリックは別に扱う）
		//-------------------------------------------------------------------------------
		private void dgvMapPointers_CurrentCellChanged(object sender, EventArgs e)
		{
			if (Control.MouseButtons == MouseButtons.None) this.NotifyEventRowSelected(this.dgvMapPointers.CurrentRow);
		}

		//-------------------------------------------------------------------------------
		// 同じ行をクリックし直した場合も、その行のイベントへの移動を通知する処理
		//-------------------------------------------------------------------------------
		private void dgvMapPointers_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0) this.NotifyEventRowSelected(this.dgvMapPointers.Rows[e.RowIndex]);
		}

		//-------------------------------------------------------------------------------
		// 利用者が選んだイベント行だけを通知し、一覧の再構築や自動選択を除く処理
		//-------------------------------------------------------------------------------
		private void NotifyEventRowSelected(DataGridViewRow row)
		{
			if (this.isUpdatingSelection || !(row?.Tag is MapPointerEntry entry) || entry.EventKind == null) return;
			this.EventRowSelected?.Invoke(this, entry);
		}

		//-------------------------------------------------------------------------------
		// 選択行を表示順にコピーして、結果を画面内へ表示する処理（列 -1 は行全体）
		//-------------------------------------------------------------------------------
		private void CopyRows(int column, DataGridViewRow singleRow = null, DataGridView grid = null)
		{
			grid = grid ?? this.dgvMapPointers;
			List<DataGridViewRow> rows = singleRow != null ? new List<DataGridViewRow> { singleRow }
				: grid.SelectedRows.Cast<DataGridViewRow>().OrderBy(row => row.Index).ToList();
			if (rows.Count == 0)
			{
				this.lblMapPointersResult.Text = Localizer.T("コピーする行を選択してください。");
				return;
			}
			List<string> values = new List<string>();
			foreach (DataGridViewRow row in rows)
			{
				values.Add(column < 0 ? string.Join("\t", row.Cells.Cast<DataGridViewCell>().Select(cell => Convert.ToString(cell.Value)))
					: Convert.ToString(row.Cells[column].Value));
			}
			try
			{
				Clipboard.SetText(string.Join(Environment.NewLine, values));
				string name = column < 0 ? Localizer.T("行") : grid.Columns[column].HeaderText;
				this.lblMapPointersResult.Text = rows.Count == 1
					? string.Format(Localizer.T("{0} をコピーしました: {1}"), name, values[0])
					: string.Format(Localizer.T("{0} をコピーしました: {1} 件"), name, rows.Count);
			}
			catch (Exception)
			{
				this.lblMapPointersResult.Text = Localizer.T("クリップボードへコピーできませんでした。");
			}
		}

		//-------------------------------------------------------------------------------
		// Ctrl+C で選択行の置き場所をコピーする処理
		//-------------------------------------------------------------------------------
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.C))
			{
				if (this.dgvReferences.ContainsFocus)
				{
					this.CopyRows(this.colReferenceLocation.Index, grid: this.dgvReferences);
					return true;
				}
				if (this.dgvMapPointers.ContainsFocus)
				{
					this.CopyRows(this.colLocation.Index);
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		//-------------------------------------------------------------------------------
		// ダブルクリックしたセルの値をコピーする処理
		//-------------------------------------------------------------------------------
		private void dgvMapPointers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
			int column = e.ColumnIndex == this.colValue.Index || e.ColumnIndex == this.colBytes.Index ? e.ColumnIndex : this.colLocation.Index;
			this.CopyRows(column, this.dgvMapPointers.Rows[e.RowIndex]);
		}

		//-------------------------------------------------------------------------------
		// 右クリックした行を選択する処理（複数行の選択中は選択範囲を保つ）
		//-------------------------------------------------------------------------------
		private void dgvMapPointers_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
			DataGridView grid = (DataGridView)sender;
			DataGridViewRow row = grid.Rows[e.RowIndex];
			if (row.Selected) return;
			grid.ClearSelection();
			grid.CurrentCell = row.Cells[grid == this.dgvReferences ? this.colReferenceLocation.Index : this.colLocation.Index];
			row.Selected = true;
		}

		//-------------------------------------------------------------------------------
		// 入力したアドレスのバイト列を更新し、前の検索結果を消す処理
		//-------------------------------------------------------------------------------
		private void txtFindAddress_TextChanged(object sender, EventArgs e)
		{
			this.lblFindBytes.Text = string.Format(Localizer.T("バイト列 : {0}"),
				MapPointerReport.TryParseAddress(this.txtFindAddress.Text, out uint offset) ? MapPointerReport.FormatBytes(offset) : "-");
			this.dgvReferences.Rows.Clear();
			this.lblFindResult.Text = string.Empty;
		}

		//-------------------------------------------------------------------------------
		// 入力したアドレスを参照する場所を ROM 全体から探して説明とともに表示する処理
		//-------------------------------------------------------------------------------
		private void btnFindReferences_Click(object sender, EventArgs e)
		{
			this.dgvReferences.Rows.Clear();
			if (!MapPointerReport.TryParseAddress(this.txtFindAddress.Text, out uint offset))
			{
				this.lblFindResult.Text = Localizer.T("アドレスを解釈できません。");
				return;
			}
			Cursor previousCursor = Cursor.Current;
			this.UseWaitCursor = true;
			Cursor.Current = Cursors.WaitCursor;
			this.dgvReferences.SuspendLayout();
			try
			{
				const int limit = 500;
				List<uint> locations = MapPointerReport.FindReferences(this.rom, offset, limit);
				foreach (uint location in locations)
				{
					string description = this.describeLocation?.Invoke(location)
						?? Localizer.T("（マップのデータの表には無い場所。スクリプトの中や、別の表の可能性）");
					this.dgvReferences.Rows.Add(MapPointerReport.FormatAddress(location), description);
				}
				this.lblFindResult.Text = locations.Count == limit
					? string.Format(Localizer.T("{0} 件で打ち切りました。"), limit)
					: locations.Count == 0 ? Localizer.T("見つかりませんでした。")
					: string.Format(Localizer.T("{0} か所見つかりました。"), locations.Count);
			}
			finally
			{
				this.dgvReferences.ResumeLayout();
				this.UseWaitCursor = false;
				Cursor.Current = previousCursor;
			}
		}

		//-------------------------------------------------------------------------------
		// 上の一覧で選んだ行のポインタを入力欄へ移して参照元を探す処理
		//-------------------------------------------------------------------------------
		private void btnFindSelected_Click(object sender, EventArgs e)
		{
			MapPointerEntry entry = this.dgvMapPointers.CurrentRow?.Tag as MapPointerEntry;
			if (entry == null || !entry.IsPointer) return;
			this.txtFindAddress.Text = MapPointerReport.FormatValue(entry);
			this.btnFindReferences_Click(sender, e);
		}

		//-------------------------------------------------------------------------------
		// 入力したアドレスのバイト列をコピーして既存の案内欄に結果を示す処理
		//-------------------------------------------------------------------------------
		private void btnCopyFindBytes_Click(object sender, EventArgs e)
		{
			if (!MapPointerReport.TryParseAddress(this.txtFindAddress.Text, out uint offset))
			{
				this.lblMapPointersResult.Text = Localizer.T("アドレスを解釈できません。");
				return;
			}
			string bytes = MapPointerReport.FormatBytes(offset);
			try
			{
				Clipboard.SetText(bytes);
				this.lblMapPointersResult.Text = string.Format(Localizer.T("バイト列 {0} をコピーしました。"), bytes);
			}
			catch (Exception)
			{
				this.lblMapPointersResult.Text = Localizer.T("クリップボードへコピーできませんでした。");
			}
		}

		//-------------------------------------------------------------------------------
		// 検索結果のダブルクリックした行の置き場所をコピーする処理
		//-------------------------------------------------------------------------------
		private void dgvReferences_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
			this.CopyRows(this.colReferenceLocation.Index, this.dgvReferences.Rows[e.RowIndex], this.dgvReferences);
		}

		//-------------------------------------------------------------------------------
		// 検索結果で選択した行の置き場所をコピーする処理
		//-------------------------------------------------------------------------------
		private void menuCopyReferenceLocation_Click(object sender, EventArgs e)
		{
			this.CopyRows(this.colReferenceLocation.Index, grid: this.dgvReferences);
		}

		//-------------------------------------------------------------------------------
		// 検索結果で選択した行の置き場所と説明をタブ区切りでコピーする処理
		//-------------------------------------------------------------------------------
		private void menuCopyReferenceRow_Click(object sender, EventArgs e)
		{
			this.CopyRows(-1, grid: this.dgvReferences);
		}

		//-------------------------------------------------------------------------------
		// イベントとマップスクリプトの絞り込みを切り替える処理
		//-------------------------------------------------------------------------------
		private void chkPointersOnlyEvents_CheckedChanged(object sender, EventArgs e)
		{
			this.FillRows();
		}

		//-------------------------------------------------------------------------------
		// 選択行の置き場所をコピーする処理
		//-------------------------------------------------------------------------------
		private void btnCopyLocation_Click(object sender, EventArgs e)
		{
			this.CopyRows(this.colLocation.Index);
		}

		//-------------------------------------------------------------------------------
		// 選択行の値をコピーする処理
		//-------------------------------------------------------------------------------
		private void btnCopyValue_Click(object sender, EventArgs e)
		{
			this.CopyRows(this.colValue.Index);
		}

		//-------------------------------------------------------------------------------
		// 選択行のバイト列をコピーする処理
		//-------------------------------------------------------------------------------
		private void btnCopyBytes_Click(object sender, EventArgs e)
		{
			this.CopyRows(this.colBytes.Index);
		}

		//-------------------------------------------------------------------------------
		// 選択行をタブ区切りでコピーする処理
		//-------------------------------------------------------------------------------
		private void btnCopyRow_Click(object sender, EventArgs e)
		{
			this.CopyRows(-1);
		}

		//-------------------------------------------------------------------------------
		// ポインタ一覧の画面を閉じる処理
		//-------------------------------------------------------------------------------
		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
