using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「表示機能の選択」の画面
	// 右のツール欄に出す機能（操作ボタン・マップチップ取り込み・マップ移動・タウンマップ…）を、チェックで選び、上へ・下へで並べ替える。
	// 決めた内容は呼び出し元（MapEditor.ToolParts.cs）が右のツール欄に反映する。この画面は ROM もマップも変えない
	//-------------------------------------------------------------------------------
	public partial class ToolPartSelectForm : Form
	{
		// 一覧の 1 行分（機能のキー・名前・説明・出すかどうか）
		internal sealed class Entry
		{
			public string Key;
			public string Title;
			public string Description;
			public bool Visible;
		}

		private bool filling;

		// 「初めの状態に戻す」が押されたか（表示・並び順・分離をすべて戻す）
		internal bool ResetRequested { get; private set; }

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public ToolPartSelectForm()
		{
			this.InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// 今の並びと表示の状態を一覧に入れる初期化処理
		//-------------------------------------------------------------------------------
		internal ToolPartSelectForm(IEnumerable<Entry> entries) : this()
		{
			Localizer.Apply(this);
			this.filling = true;
			foreach (Entry entry in entries)
			{
				ListViewItem item = new ListViewItem(new[] { entry.Title, entry.Description }) { Tag = entry, Checked = entry.Visible };
				this.lvwToolParts.Items.Add(item);
			}
			this.filling = false;
			if (this.lvwToolParts.Items.Count > 0)
			{
				this.lvwToolParts.Items[0].Selected = true;
			}
			this.UpdateButtons();
		}

		//-------------------------------------------------------------------------------
		// 画面を出すときに、一覧の色を暗い配色に合わせる処理（配色の適用はこの画面を作った後に行われるので、ここで上書きする）
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.lvwToolParts.BackColor = UiTheme.Input;
			this.lvwToolParts.ForeColor = UiTheme.Text;
		}

		//-------------------------------------------------------------------------------
		// 決めた内容（上からの並びと、出すかどうか）を返す処理
		//-------------------------------------------------------------------------------
		internal List<Entry> GetResult()
		{
			List<Entry> result = new List<Entry>();
			foreach (ListViewItem item in this.lvwToolParts.Items)
			{
				Entry entry = (Entry)item.Tag;
				entry.Visible = item.Checked;
				result.Add(entry);
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 選んでいる行に合わせて、上へ・下へのボタンを使えるかどうかを切り替える処理
		//-------------------------------------------------------------------------------
		private void UpdateButtons()
		{
			int index = this.lvwToolParts.SelectedIndices.Count > 0 ? this.lvwToolParts.SelectedIndices[0] : -1;
			this.btnToolPartUp.Enabled = index > 0;
			this.btnToolPartDown.Enabled = index >= 0 && index < this.lvwToolParts.Items.Count - 1;
		}

		//-------------------------------------------------------------------------------
		// 選んでいる行を 1 つ上（direction = -1）／下（+1）へ動かす処理
		//-------------------------------------------------------------------------------
		private void MoveSelected(int direction)
		{
			if (this.lvwToolParts.SelectedIndices.Count == 0)
			{
				return;
			}
			int index = this.lvwToolParts.SelectedIndices[0];
			int target = index + direction;
			if (target < 0 || target >= this.lvwToolParts.Items.Count)
			{
				return;
			}
			this.filling = true;
			ListViewItem item = this.lvwToolParts.Items[index];
			bool wasChecked = item.Checked;
			this.lvwToolParts.Items.RemoveAt(index);
			this.lvwToolParts.Items.Insert(target, item);
			item.Checked = wasChecked;
			item.Selected = true;
			item.Focused = true;
			this.filling = false;
			this.lvwToolParts.EnsureVisible(target);
			this.UpdateButtons();
			this.lvwToolParts.Focus();
		}

		//-------------------------------------------------------------------------------
		// 行を選び直したとき
		//-------------------------------------------------------------------------------
		private void lvwToolParts_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this.filling)
			{
				this.UpdateButtons();
			}
		}

		//-------------------------------------------------------------------------------
		// 「上へ」
		//-------------------------------------------------------------------------------
		private void btnToolPartUp_Click(object sender, EventArgs e)
		{
			this.MoveSelected(-1);
		}

		//-------------------------------------------------------------------------------
		// 「下へ」
		//-------------------------------------------------------------------------------
		private void btnToolPartDown_Click(object sender, EventArgs e)
		{
			this.MoveSelected(1);
		}

		//-------------------------------------------------------------------------------
		// 「すべて表示」: 全部の行にチェックを入れる
		//-------------------------------------------------------------------------------
		private void btnToolPartShowAll_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem item in this.lvwToolParts.Items)
			{
				item.Checked = true;
			}
		}

		//-------------------------------------------------------------------------------
		// 「初めの状態に戻す」: 表示・並び順・分離をすべて戻して閉じる
		//-------------------------------------------------------------------------------
		private void btnToolPartReset_Click(object sender, EventArgs e)
		{
			this.ResetRequested = true;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
