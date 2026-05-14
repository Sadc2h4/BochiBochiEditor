using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000016 RID: 22
	public partial class InsertNewPokemonData : Form
	{
		// Token: 0x0600030D RID: 781 RVA: 0x00017870 File Offset: 0x00015A70
		public InsertNewPokemonData()
		{
			base.Load += this.InsertNewPokemonData_Load;
			base.FormClosing += this.btnInsertNewPokemonData_FormClosing;
			this.InitializeComponent();
		}

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000310 RID: 784 RVA: 0x00017DA7 File Offset: 0x00015FA7
		// (set) Token: 0x06000311 RID: 785 RVA: 0x00017DB4 File Offset: 0x00015FB4
		internal virtual Button btnInsertNewPokemonData
		{
			[CompilerGenerated]
			get
			{
				return this._btnInsertNewPokemonData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnInsertNewPokemonData_Click);
				Button button = this._btnInsertNewPokemonData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnInsertNewPokemonData = value;
				button = this._btnInsertNewPokemonData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000312 RID: 786 RVA: 0x00017DF7 File Offset: 0x00015FF7
		// (set) Token: 0x06000313 RID: 787 RVA: 0x00017E01 File Offset: 0x00016001
		internal virtual Label lblNewTrainerDataType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000314 RID: 788 RVA: 0x00017E0A File Offset: 0x0001600A
		// (set) Token: 0x06000315 RID: 789 RVA: 0x00017E14 File Offset: 0x00016014
		internal virtual Label lblNewAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000316 RID: 790 RVA: 0x00017E1D File Offset: 0x0001601D
		// (set) Token: 0x06000317 RID: 791 RVA: 0x00017E27 File Offset: 0x00016027
		internal virtual TextBox txtNewPokemonDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000318 RID: 792 RVA: 0x00017E30 File Offset: 0x00016030
		// (set) Token: 0x06000319 RID: 793 RVA: 0x00017E3A File Offset: 0x0001603A
		internal virtual ComboBox cmbNewTrainerDataType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600031A RID: 794 RVA: 0x00017E43 File Offset: 0x00016043
		// (set) Token: 0x0600031B RID: 795 RVA: 0x00017E4D File Offset: 0x0001604D
		internal virtual NumericUpDown nudNewPokemonDataNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600031C RID: 796 RVA: 0x00017E56 File Offset: 0x00016056
		// (set) Token: 0x0600031D RID: 797 RVA: 0x00017E60 File Offset: 0x00016060
		internal virtual Label lblNewPokemonDataNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600031E RID: 798 RVA: 0x00017E69 File Offset: 0x00016069
		// (set) Token: 0x0600031F RID: 799 RVA: 0x00017E73 File Offset: 0x00016073
		public int NewTrainerDataType { get; set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000320 RID: 800 RVA: 0x00017E7C File Offset: 0x0001607C
		// (set) Token: 0x06000321 RID: 801 RVA: 0x00017E86 File Offset: 0x00016086
		public string NewPokemonDataAddress { get; set; }

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000322 RID: 802 RVA: 0x00017E8F File Offset: 0x0001608F
		// (set) Token: 0x06000323 RID: 803 RVA: 0x00017E99 File Offset: 0x00016099
		public int NewPokemonDataNum { get; set; }

		// Token: 0x06000324 RID: 804 RVA: 0x00017EA2 File Offset: 0x000160A2
		private void InsertNewPokemonData_Load(object sender, EventArgs e)
		{
			this.cmbNewTrainerDataType.SelectedIndex = 0;
		}

		// Token: 0x06000325 RID: 805 RVA: 0x00017EB4 File Offset: 0x000160B4
		private void btnInsertNewPokemonData_Click(object sender, EventArgs e)
		{
			this.NewTrainerDataType = this.cmbNewTrainerDataType.SelectedIndex;
			this.NewPokemonDataAddress = this.txtNewPokemonDataAddress.Text.Trim();
			this.NewPokemonDataNum = Convert.ToInt32(this.nudNewPokemonDataNum.Value);
			base.DialogResult = DialogResult.OK;
			base.Close();
		}

		// Token: 0x06000326 RID: 806 RVA: 0x00017F14 File Offset: 0x00016114
		private void btnInsertNewPokemonData_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = base.DialogResult != DialogResult.OK;
			if (flag)
			{
				base.DialogResult = DialogResult.Cancel;
			}
		}
	}
}
