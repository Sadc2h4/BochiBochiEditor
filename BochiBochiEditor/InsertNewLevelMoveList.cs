using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000013 RID: 19
	public partial class InsertNewLevelMoveList : Form
	{
		// Token: 0x060002D5 RID: 725 RVA: 0x000169C0 File Offset: 0x00014BC0
		public InsertNewLevelMoveList()
		{
			this.InitializeComponent();
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060002D8 RID: 728 RVA: 0x00016D92 File Offset: 0x00014F92
		// (set) Token: 0x060002D9 RID: 729 RVA: 0x00016D9C File Offset: 0x00014F9C
		internal virtual TextBox txtNewAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060002DA RID: 730 RVA: 0x00016DA5 File Offset: 0x00014FA5
		// (set) Token: 0x060002DB RID: 731 RVA: 0x00016DAF File Offset: 0x00014FAF
		internal virtual Label lblNewAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060002DC RID: 732 RVA: 0x00016DB8 File Offset: 0x00014FB8
		// (set) Token: 0x060002DD RID: 733 RVA: 0x00016DC2 File Offset: 0x00014FC2
		internal virtual Label lblNewNumMove
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060002DE RID: 734 RVA: 0x00016DCB File Offset: 0x00014FCB
		// (set) Token: 0x060002DF RID: 735 RVA: 0x00016DD5 File Offset: 0x00014FD5
		internal virtual NumericUpDown nudNewNumMove
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x00016DDE File Offset: 0x00014FDE
		// (set) Token: 0x060002E1 RID: 737 RVA: 0x00016DE8 File Offset: 0x00014FE8
		internal virtual Button btnNewLevelMoveList
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewLevelMoveList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewLevelMoveList_Click);
				Button button = this._btnNewLevelMoveList;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewLevelMoveList = value;
				button = this._btnNewLevelMoveList;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060002E2 RID: 738 RVA: 0x00016E2B File Offset: 0x0001502B
		// (set) Token: 0x060002E3 RID: 739 RVA: 0x00016E35 File Offset: 0x00015035
		public string NewLevelMoveAddress { get; set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060002E4 RID: 740 RVA: 0x00016E3E File Offset: 0x0001503E
		// (set) Token: 0x060002E5 RID: 741 RVA: 0x00016E48 File Offset: 0x00015048
		public int NewLevelMoveNum { get; set; }

		// Token: 0x060002E6 RID: 742 RVA: 0x00016E51 File Offset: 0x00015051
		private void btnNewLevelMoveList_Click(object sender, EventArgs e)
		{
			this.NewLevelMoveAddress = this.txtNewAddress.Text.Trim();
			this.NewLevelMoveNum = Convert.ToInt32(this.nudNewNumMove.Value);
			base.DialogResult = DialogResult.OK;
			base.Close();
		}
	}
}
