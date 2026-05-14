using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000014 RID: 20
	public partial class InsertNewPokedexData : Form
	{
		// Token: 0x060002E7 RID: 743 RVA: 0x00016E91 File Offset: 0x00015091
		public InsertNewPokedexData()
		{
			base.FormClosing += this.InsertNewPokedexData_FormClosing;
			this.InitializeComponent();
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060002EA RID: 746 RVA: 0x0001723F File Offset: 0x0001543F
		// (set) Token: 0x060002EB RID: 747 RVA: 0x00017249 File Offset: 0x00015449
		internal virtual Label lblPokedexDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060002EC RID: 748 RVA: 0x00017252 File Offset: 0x00015452
		// (set) Token: 0x060002ED RID: 749 RVA: 0x0001725C File Offset: 0x0001545C
		internal virtual TextBox txtPokedexDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060002EE RID: 750 RVA: 0x00017265 File Offset: 0x00015465
		// (set) Token: 0x060002EF RID: 751 RVA: 0x0001726F File Offset: 0x0001546F
		internal virtual Label lblPokedexPokemonNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x00017278 File Offset: 0x00015478
		// (set) Token: 0x060002F1 RID: 753 RVA: 0x00017282 File Offset: 0x00015482
		internal virtual NumericUpDown nudPokedexPokemonNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0001728B File Offset: 0x0001548B
		// (set) Token: 0x060002F3 RID: 755 RVA: 0x00017298 File Offset: 0x00015498
		internal virtual Button btnInsertNewPokedexData
		{
			[CompilerGenerated]
			get
			{
				return this._btnInsertNewPokedexData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnInsertNewPokedexData_Click);
				Button button = this._btnInsertNewPokedexData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnInsertNewPokedexData = value;
				button = this._btnInsertNewPokedexData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x000172DB File Offset: 0x000154DB
		// (set) Token: 0x060002F5 RID: 757 RVA: 0x000172E5 File Offset: 0x000154E5
		public uint NewDataAddress { get; set; }

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x000172EE File Offset: 0x000154EE
		// (set) Token: 0x060002F7 RID: 759 RVA: 0x000172F8 File Offset: 0x000154F8
		public int NewPokemonCount { get; set; }

		// Token: 0x060002F8 RID: 760 RVA: 0x00017304 File Offset: 0x00015504
		private void btnInsertNewPokedexData_Click(object sender, EventArgs e)
		{
			string text = this.txtPokedexDataAddress.Text.Trim();
			this.NewDataAddress = Convert.ToUInt32(text, 16);
			this.NewPokemonCount = Convert.ToInt32(this.nudPokedexPokemonNum.Value);
			base.DialogResult = DialogResult.OK;
			base.Close();
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x00017358 File Offset: 0x00015558
		private void InsertNewPokedexData_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = base.DialogResult != DialogResult.OK;
			if (flag)
			{
				base.DialogResult = DialogResult.Cancel;
			}
		}
	}
}
