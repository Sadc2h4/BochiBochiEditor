using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップを追加…」で、マップを足すバンクを選んでもらう画面
	//-------------------------------------------------------------------------------
	public partial class AddMapBankForm : Form
	{
		// 選ばれたバンク
		internal int SelectedBank { get; private set; }

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public AddMapBankForm()
		{
			this.InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// 選べるバンクの一覧と、最初に選んでおくバンクを受け取って初期化する処理
		// newBankNumber が 0 以上なら、一覧の最後に「新しいバンクを作る」を足す（選ぶと、その番号を返す）
		//-------------------------------------------------------------------------------
		internal AddMapBankForm(IList<int> banks, int initialBank, int newBankNumber = -1) : this()
		{
			Localizer.Apply(this);
			foreach (int bank in banks)
			{
				this.cmbAddMapBank.Items.Add(string.Format(Localizer.T("バンク {0}"), bank));
			}
			this.banks = new List<int>(banks);
			if (newBankNumber >= 0)
			{
				this.cmbAddMapBank.Items.Add(string.Format(Localizer.T("新しいバンク {0} を作る"), newBankNumber));
				this.banks.Add(newBankNumber);
			}
			int index = this.banks.IndexOf(initialBank);
			this.cmbAddMapBank.SelectedIndex = index >= 0 ? index : 0;
		}

		private List<int> banks = new List<int>();

		//-------------------------------------------------------------------------------
		// 「追加する」: 選んだバンクを返して閉じる処理
		//-------------------------------------------------------------------------------
		private void btnAddMapOk_Click(object sender, EventArgs e)
		{
			if (this.cmbAddMapBank.SelectedIndex < 0 || this.cmbAddMapBank.SelectedIndex >= this.banks.Count)
			{
				return;
			}
			this.SelectedBank = this.banks[this.cmbAddMapBank.SelectedIndex];
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
