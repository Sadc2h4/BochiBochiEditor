using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「開いている ROM のテーブル情報」画面
	// 各表を ROM のどこから・何バイト×何件で読んでいるかと、ini や元のゲームとの違いを一覧で見せる
	// （読み込みがおかしいと思ったときに、どの表が原因かをすぐ見つけられるようにするため）
	//-------------------------------------------------------------------------------
	public partial class RomTablesForm : Form
	{
		private readonly List<RomTableEntry> entries = new List<RomTableEntry>();

		//-------------------------------------------------------------------------------
		// デザイナー用の初期化処理
		//-------------------------------------------------------------------------------
		public RomTablesForm()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// 調べた結果と、ROM の概要を受け取って初期化する処理
		//-------------------------------------------------------------------------------
		internal RomTablesForm(List<RomTableEntry> entries, string summary) : this()
		{
			this.entries.AddRange(entries);
			Localizer.Apply(this);
			this.lblRomTablesSummary.Text = summary;
		}

		//-------------------------------------------------------------------------------
		// 表示前に、一覧の色（ダークテーマ）と中身を整える処理
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataGridView grid = this.dgvRomTables;
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
			this.lblRomTablesLegend.ForeColor = UiTheme.TextMuted;
			this.FillRows();
		}

		//-------------------------------------------------------------------------------
		// 一覧に行を入れる処理（「変更・要確認がある表だけ」のときは OK の行を出さない）
		//-------------------------------------------------------------------------------
		private void FillRows()
		{
			this.dgvRomTables.SuspendLayout();
			this.dgvRomTables.Rows.Clear();
			foreach (RomTableEntry entry in this.entries)
			{
				if (this.chkRomTablesIssuesOnly.Checked && entry.Status == RomTableEntry.Level.Ok)
				{
					continue;
				}
				int index = this.dgvRomTables.Rows.Add(
					Localizer.T(RomTableReport.StatusText(entry.Status)),
					Localizer.T(entry.Category),
					Localizer.T(entry.Name),
					Localizer.T(entry.Usage),
					entry.Address >= 0 ? string.Format("0x{0:X6}", entry.Address) : "-",
					entry.Source ?? "-",
					entry.EntrySize ?? "-",
					entry.Count ?? "-",
					string.Join(Environment.NewLine, entry.Notes),
					entry.IniKey);
				// 状態の欄を色分けする（要確認・読めないは目立つ色）
				DataGridViewCell status = this.dgvRomTables.Rows[index].Cells[0];
				status.Style.ForeColor = StatusColor(entry.Status);
				status.Style.SelectionForeColor = StatusColor(entry.Status);
			}
			this.dgvRomTables.ResumeLayout();
		}

		//-------------------------------------------------------------------------------
		// 状態ごとの文字の色を返す処理
		//-------------------------------------------------------------------------------
		private static Color StatusColor(RomTableEntry.Level level)
		{
			switch (level)
			{
				case RomTableEntry.Level.Error:
					return UiTheme.Warning;
				case RomTableEntry.Level.Warning:
					return Color.FromArgb(255, 196, 64);
				case RomTableEntry.Level.Info:
					return Color.FromArgb(120, 190, 255);
				default:
					return Color.FromArgb(110, 210, 140);
			}
		}

		//-------------------------------------------------------------------------------
		// 「変更・要確認がある表だけ表示」を切り替えたら一覧を作り直す処理
		//-------------------------------------------------------------------------------
		private void chkRomTablesIssuesOnly_CheckedChanged(object sender, EventArgs e)
		{
			this.FillRows();
		}

		//-------------------------------------------------------------------------------
		// 一覧をタブ区切りの文字でクリップボードへ写す処理（表計算ソフトへ貼ったり、報告に使ったりできる）
		//-------------------------------------------------------------------------------
		private void btnRomTablesCopy_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetText(this.lblRomTablesSummary.Text + Environment.NewLine + RomTableReport.ToText(this.entries));
				this.btnRomTablesCopy.Text = Localizer.T("コピーしました");
			}
			catch (Exception)
			{
				MessageBox.Show(this, Localizer.T("クリップボードへコピーできませんでした。"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		//-------------------------------------------------------------------------------
		// 閉じる処理
		//-------------------------------------------------------------------------------
		private void btnRomTablesClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
