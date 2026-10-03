using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「ここにアイテム（落とし物）を追加…」で、拾えるアイテムと、拾った後に消すためのフラグを決めてもらう画面
	// 決めた内容は呼び出し元（MapEditor.EventAdd.cs）が、人のイベントとして追加する。この画面は ROM もマップも変えない
	//-------------------------------------------------------------------------------
	public partial class ItemBallForm : Form
	{
		private readonly List<int> itemIds = new List<int>();

		// 決めたアイテムの番号（選べていなければ -1）
		internal int ItemId
		{
			get { return this.cmbItemBallItem.SelectedIndex >= 0 ? this.itemIds[this.cmbItemBallItem.SelectedIndex] : -1; }
		}

		// 決めたフラグの番号（16 進で読めなければ -1）
		internal int Flag
		{
			get
			{
				int value;
				return int.TryParse(this.txtItemBallFlag.Text.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value) && value >= 0 && value <= 0xFFFF ? value : -1;
			}
		}

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public ItemBallForm()
		{
			this.InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// アイテムの一覧（番号と名前）と、フラグの候補を入れる初期化処理
		// suggestedFlag = ほかの人のイベントが使っていない番号（見つからなければ -1）。flagNote = フラグの欄の下に出す説明
		//-------------------------------------------------------------------------------
		internal ItemBallForm(IEnumerable<KeyValuePair<int, string>> items, int suggestedFlag, string flagNote) : this()
		{
			Localizer.Apply(this);
			foreach (KeyValuePair<int, string> item in items)
			{
				this.itemIds.Add(item.Key);
				this.cmbItemBallItem.Items.Add(string.Format("{0:X3}  {1}", item.Key, item.Value));
			}
			if (this.cmbItemBallItem.Items.Count > 0)
			{
				this.cmbItemBallItem.SelectedIndex = 0;
			}
			this.txtItemBallFlag.Text = suggestedFlag >= 0 ? suggestedFlag.ToString("X4") : string.Empty;
			this.lblItemBallFlagNote.Text = flagNote;
			this.UpdateOkButton();
		}

		//-------------------------------------------------------------------------------
		// アイテムとフラグが決まっているときだけ、OK を押せるようにする処理
		//-------------------------------------------------------------------------------
		private void UpdateOkButton()
		{
			this.btnItemBallOk.Enabled = this.ItemId >= 0 && this.Flag >= 0;
		}

		//-------------------------------------------------------------------------------
		// アイテムを選び直したとき・フラグを打ち直したとき
		//-------------------------------------------------------------------------------
		private void ItemBallInput_Changed(object sender, EventArgs e)
		{
			this.UpdateOkButton();
		}
	}
}
