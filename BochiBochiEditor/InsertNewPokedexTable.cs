using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000015 RID: 21
	public partial class InsertNewPokedexTable : Form
	{
		// Token: 0x060002FA RID: 762 RVA: 0x00017380 File Offset: 0x00015580
		public InsertNewPokedexTable()
		{
			base.FormClosing += this.InsertNewPokedexTable_FormClosing;
			this.InitializeComponent();
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060002FD RID: 765 RVA: 0x00017730 File Offset: 0x00015930
		// (set) Token: 0x060002FE RID: 766 RVA: 0x0001773C File Offset: 0x0001593C
		internal virtual Button btnInsertNewPokedexTable
		{
			[CompilerGenerated]
			get
			{
				return this._btnInsertNewPokedexTable;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnInsertNewPokedexTable_Click);
				Button button = this._btnInsertNewPokedexTable;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnInsertNewPokedexTable = value;
				button = this._btnInsertNewPokedexTable;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060002FF RID: 767 RVA: 0x0001777F File Offset: 0x0001597F
		// (set) Token: 0x06000300 RID: 768 RVA: 0x00017789 File Offset: 0x00015989
		internal virtual NumericUpDown nudPokedexPageNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000301 RID: 769 RVA: 0x00017792 File Offset: 0x00015992
		// (set) Token: 0x06000302 RID: 770 RVA: 0x0001779C File Offset: 0x0001599C
		internal virtual Label lblPokedexPageNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000303 RID: 771 RVA: 0x000177A5 File Offset: 0x000159A5
		// (set) Token: 0x06000304 RID: 772 RVA: 0x000177AF File Offset: 0x000159AF
		internal virtual TextBox txtPokedexTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000305 RID: 773 RVA: 0x000177B8 File Offset: 0x000159B8
		// (set) Token: 0x06000306 RID: 774 RVA: 0x000177C2 File Offset: 0x000159C2
		internal virtual Label lblPokedexTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000307 RID: 775 RVA: 0x000177CB File Offset: 0x000159CB
		// (set) Token: 0x06000308 RID: 776 RVA: 0x000177D5 File Offset: 0x000159D5
		public uint NewTableAddress { get; set; }

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000309 RID: 777 RVA: 0x000177DE File Offset: 0x000159DE
		// (set) Token: 0x0600030A RID: 778 RVA: 0x000177E8 File Offset: 0x000159E8
		public int NewPageCount { get; set; }

		// Token: 0x0600030B RID: 779 RVA: 0x000177F4 File Offset: 0x000159F4
		private void btnInsertNewPokedexTable_Click(object sender, EventArgs e)
		{
			string text = this.txtPokedexTableAddress.Text.Trim();
			this.NewTableAddress = Convert.ToUInt32(text, 16);
			this.NewPageCount = Convert.ToInt32(this.nudPokedexPageNum.Value);
			base.DialogResult = DialogResult.OK;
			base.Close();
		}

		// Token: 0x0600030C RID: 780 RVA: 0x00017848 File Offset: 0x00015A48
		private void InsertNewPokedexTable_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = base.DialogResult != DialogResult.OK;
			if (flag)
			{
				base.DialogResult = DialogResult.Cancel;
			}
		}
	}
}
