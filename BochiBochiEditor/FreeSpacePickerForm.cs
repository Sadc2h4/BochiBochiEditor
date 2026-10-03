using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 空き領域の候補を一覧で出し、書き込み先を選んでもらう画面
	// （出現ポケモンのエリア追加・表の移動で、アドレス欄が空欄のときに使う）
	//-------------------------------------------------------------------------------
	public partial class FreeSpacePickerForm : Form
	{
		private readonly IList<MapEditor.FreeSpaceCandidate> candidates;

		// 選ばれた候補（キャンセルなら null）
		internal MapEditor.FreeSpaceCandidate SelectedCandidate { get; private set; }

		//-------------------------------------------------------------------------------
		// 画面を作る処理（purpose は何を書くかの説明、length は書くバイト数）
		//-------------------------------------------------------------------------------
		internal FreeSpacePickerForm(string purpose, int length, IList<MapEditor.FreeSpaceCandidate> candidates)
		{
			this.InitializeComponent();
			this.candidates = candidates;
			Localizer.Apply(this);
			this.lblPickerSummary.Text = string.Format(Localizer.T("{0}（{1} バイト）の書き込み先を選んでください。空き領域の探し始めの位置から順に {2} 件を並べています。"), purpose, length, candidates.Count);
			for (int i = 0; i < candidates.Count; i++)
			{
				MapEditor.FreeSpaceCandidate c = candidates[i];
				int row = this.dgvCandidates.Rows.Add(
					i + 1,
					string.Format("{0:X8}", 0x08000000U + c.Address),
					c.RunLength,
					string.Format("{0:X6}-{1:X6}", c.RunStart, c.RunStart + (uint)c.RunLength - 1),
					BitConverter.ToString(c.BytesBefore).Replace("-", " "));
				this.dgvCandidates.Rows[row].Tag = c;
			}
			this.colRunLength.DefaultCellStyle.Format = "N0";
			this.colRunLength.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			this.UpdateOkButton();
		}

		//-------------------------------------------------------------------------------
		// 表示前にポインタ一覧の画面と同じ配色を一覧へ適用する処理
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataGridView grid = this.dgvCandidates;
			grid.EnableHeadersVisualStyles = false;
			grid.BackgroundColor = UiTheme.Canvas;
			grid.GridColor = UiTheme.Border;
			grid.BorderStyle = BorderStyle.None;
			grid.DefaultCellStyle.BackColor = UiTheme.Input;
			grid.DefaultCellStyle.ForeColor = UiTheme.Text;
			grid.DefaultCellStyle.SelectionBackColor = UiTheme.Accent;
			grid.DefaultCellStyle.SelectionForeColor = UiTheme.AccentText;
			grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Card;
			grid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Text;
			grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = UiTheme.Card;
			grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			this.lblPickerNote.ForeColor = UiTheme.TextMuted;
		}

		//-------------------------------------------------------------------------------
		// 行を選んでいるときだけ「この場所に書く」を押せるようにする処理
		//-------------------------------------------------------------------------------
		private void UpdateOkButton()
		{
			this.btnPickerOk.Enabled = this.dgvCandidates.CurrentRow != null && this.dgvCandidates.CurrentRow.Tag is MapEditor.FreeSpaceCandidate;
		}

		//-------------------------------------------------------------------------------
		// 選んでいる行が変わったときの処理
		//-------------------------------------------------------------------------------
		private void dgvCandidates_SelectionChanged(object sender, EventArgs e)
		{
			this.UpdateOkButton();
		}

		//-------------------------------------------------------------------------------
		// 行をダブルクリックしたら、その場所に決める処理
		//-------------------------------------------------------------------------------
		private void dgvCandidates_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				this.btnPickerOk_Click(sender, EventArgs.Empty);
			}
		}

		//-------------------------------------------------------------------------------
		// 「この場所に書く」: 選んだ候補を返して閉じる処理
		//-------------------------------------------------------------------------------
		private void btnPickerOk_Click(object sender, EventArgs e)
		{
			MapEditor.FreeSpaceCandidate c = this.dgvCandidates.CurrentRow != null ? this.dgvCandidates.CurrentRow.Tag as MapEditor.FreeSpaceCandidate : null;
			if (c == null)
			{
				return;
			}
			this.SelectedCandidate = c;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
