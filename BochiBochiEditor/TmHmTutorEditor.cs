using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x02000027 RID: 39
	public partial class TmHmTutorEditor : Form
	{
		// Token: 0x06000BA8 RID: 2984 RVA: 0x00057564 File Offset: 0x00055764
		public TmHmTutorEditor()
		{
			base.Load += this.TmHmTutorEditor_Load;
			base.FormClosing += this.TmHmTutorEditor_FormClosing;
			this.tmHmIds = new List<ushort>();
			this.tutorIds = new List<ushort>();
			this.moveNames = new List<string>();
			this.hasUnsavedChanges = false;
			this.InitializeComponent();
		}

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06000BAB RID: 2987 RVA: 0x00057A59 File Offset: 0x00055C59
		// (set) Token: 0x06000BAC RID: 2988 RVA: 0x00057A63 File Offset: 0x00055C63
		internal virtual ComboBox cmbMoveList1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06000BAD RID: 2989 RVA: 0x00057A6C File Offset: 0x00055C6C
		// (set) Token: 0x06000BAE RID: 2990 RVA: 0x00057A76 File Offset: 0x00055C76
		internal virtual ComboBox cmbMoveList2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06000BAF RID: 2991 RVA: 0x00057A7F File Offset: 0x00055C7F
		// (set) Token: 0x06000BB0 RID: 2992 RVA: 0x00057A8C File Offset: 0x00055C8C
		internal virtual Button btnChangeMoveName1
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeMoveName1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeMoveName1_Click);
				Button button = this._btnChangeMoveName1;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeMoveName1 = value;
				button = this._btnChangeMoveName1;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x06000BB1 RID: 2993 RVA: 0x00057ACF File Offset: 0x00055CCF
		// (set) Token: 0x06000BB2 RID: 2994 RVA: 0x00057ADC File Offset: 0x00055CDC
		internal virtual Button btnChangeMoveName2
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeMoveName2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeMoveName2_Click);
				Button button = this._btnChangeMoveName2;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeMoveName2 = value;
				button = this._btnChangeMoveName2;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x06000BB3 RID: 2995 RVA: 0x00057B1F File Offset: 0x00055D1F
		// (set) Token: 0x06000BB4 RID: 2996 RVA: 0x00057B2C File Offset: 0x00055D2C
		internal virtual Button btnSave
		{
			[CompilerGenerated]
			get
			{
				return this._btnSave;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSave_Click);
				Button button = this._btnSave;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSave = value;
				button = this._btnSave;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x06000BB5 RID: 2997 RVA: 0x00057B6F File Offset: 0x00055D6F
		// (set) Token: 0x06000BB6 RID: 2998 RVA: 0x00057B7C File Offset: 0x00055D7C
		internal virtual ListBox lstTmHmList
		{
			[CompilerGenerated]
			get
			{
				return this._lstTmHmList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstTmHmList_SelectedIndexChanged);
				ListBox listBox = this._lstTmHmList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstTmHmList = value;
				listBox = this._lstTmHmList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x06000BB7 RID: 2999 RVA: 0x00057BBF File Offset: 0x00055DBF
		// (set) Token: 0x06000BB8 RID: 3000 RVA: 0x00057BCC File Offset: 0x00055DCC
		internal virtual ListBox lstTutorList
		{
			[CompilerGenerated]
			get
			{
				return this._lstTutorList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstTutorList_SelectedIndexChanged);
				ListBox listBox = this._lstTutorList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstTutorList = value;
				listBox = this._lstTutorList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x00057C10 File Offset: 0x00055E10
		private void TmHmTutorEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.InitializeMoveComboBoxes();
			this.InitializeTmHmList();
			this.InitializeTutorList();
			this.lstTmHmList.SelectedIndex = 0;
			this.lstTutorList.SelectedIndex = 0;
			this.hasUnsavedChanges = false;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x00057C68 File Offset: 0x00055E68
		private void InitializeMoveComboBoxes()
		{
			this.moveNames = MoveData.GetMoveNames(this.romData);
			this.cmbMoveList1.Items.AddRange(this.moveNames.ToArray());
			this.cmbMoveList2.Items.AddRange(this.moveNames.ToArray());
			this.cmbMoveList1.SelectedIndex = 0;
			this.cmbMoveList2.SelectedIndex = 0;
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x00057CDC File Offset: 0x00055EDC
		private void InitializeTmHmList()
		{
			this.tmHmIds.Clear();
			this.lstTmHmList.Items.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TM_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = MyProject.Forms.PokemonEditor.TM_HM_LIST_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					this.tmHmIds.Add(num3);
					string text = this.moveNames[(int)num3];
					this.lstTmHmList.Items.Add(string.Format("TM{0:00} - {1}", i + 1, text));
				}
				int num4 = MyProject.Forms.PokemonEditor.HM_COUNT - 1;
				for (int j = 0; j <= num4; j++)
				{
					int num5 = MyProject.Forms.PokemonEditor.TM_HM_LIST_OFFSET + (MyProject.Forms.PokemonEditor.TM_COUNT + j) * 2;
					ushort num6 = BitConverter.ToUInt16(this.romData, num5);
					this.tmHmIds.Add(num6);
					string text2 = this.moveNames[(int)num6];
					this.lstTmHmList.Items.Add(string.Format("HM{0:00} - {1}", j + 1, text2));
				}
			}
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x00057E28 File Offset: 0x00056028
		private void InitializeTutorList()
		{
			this.tutorIds.Clear();
			this.lstTutorList.Items.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.MOVE_TUTOR_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = MyProject.Forms.PokemonEditor.MOVE_TUTOR_LIST_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					this.tutorIds.Add(num3);
					string text = this.moveNames[(int)num3];
					this.lstTutorList.Items.Add(text);
				}
			}
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x00057EC0 File Offset: 0x000560C0
		private void lstTmHmList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstTmHmList.SelectedIndex >= 0 && this.lstTmHmList.SelectedIndex < this.tmHmIds.Count;
			if (flag)
			{
				ushort num = this.tmHmIds[this.lstTmHmList.SelectedIndex];
				bool flag2 = (int)num < this.cmbMoveList1.Items.Count;
				if (flag2)
				{
					this.cmbMoveList1.SelectedIndex = (int)num;
				}
			}
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x00057F3C File Offset: 0x0005613C
		private void lstTutorList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstTutorList.SelectedIndex >= 0 && this.lstTutorList.SelectedIndex < this.tutorIds.Count;
			if (flag)
			{
				ushort num = this.tutorIds[this.lstTutorList.SelectedIndex];
				bool flag2 = (int)num < this.cmbMoveList2.Items.Count;
				if (flag2)
				{
					this.cmbMoveList2.SelectedIndex = (int)num;
				}
			}
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x00057FB8 File Offset: 0x000561B8
		private void btnChangeMoveName1_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstTmHmList.SelectedIndex;
			checked
			{
				ushort num = (ushort)this.cmbMoveList1.SelectedIndex;
				ushort num2 = this.tmHmIds[selectedIndex];
				bool flag = num != num2;
				if (flag)
				{
					string text = this.cmbMoveList1.SelectedItem.ToString();
					this.tmHmIds[selectedIndex] = num;
					bool flag2 = selectedIndex < MyProject.Forms.PokemonEditor.TM_COUNT;
					string text2;
					if (flag2)
					{
						text2 = string.Format("TM{0:00}", selectedIndex + 1);
					}
					else
					{
						text2 = string.Format("HM{0:00}", selectedIndex - MyProject.Forms.PokemonEditor.TM_COUNT + 1);
					}
					this.lstTmHmList.Items[selectedIndex] = string.Format("{0} - {1}", text2, text);
					this.SetUnsavedChanges();
				}
			}
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x00058098 File Offset: 0x00056298
		private void btnChangeMoveName2_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstTutorList.SelectedIndex;
			ushort num = checked((ushort)this.cmbMoveList2.SelectedIndex);
			ushort num2 = this.tutorIds[selectedIndex];
			bool flag = num != num2;
			if (flag)
			{
				string text = this.cmbMoveList2.SelectedItem.ToString();
				this.tutorIds[selectedIndex] = num;
				this.lstTutorList.Items[selectedIndex] = text;
				this.SetUnsavedChanges();
			}
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x00058114 File Offset: 0x00056314
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveChanges();
			this.hasUnsavedChanges = false;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x0005812C File Offset: 0x0005632C
		private void TmHmTutorEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveChanges();
				}
				else
				{
					bool flag3 = dialogResult == DialogResult.Cancel;
					if (flag3)
					{
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x0005817C File Offset: 0x0005637C
		private void SetUnsavedChanges()
		{
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x0005818D File Offset: 0x0005638D
		private void UpdateSaveButtonState()
		{
			this.btnSave.Enabled = this.hasUnsavedChanges;
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x000581A4 File Offset: 0x000563A4
		private void SaveChanges()
		{
			checked
			{
				int num = this.tmHmIds.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = MyProject.Forms.PokemonEditor.TM_HM_LIST_OFFSET + i * 2;
					byte[] bytes = BitConverter.GetBytes(this.tmHmIds[i]);
					Array.Copy(bytes, 0, this.romData, num2, 2);
				}
				int num3 = this.tutorIds.Count - 1;
				for (int j = 0; j <= num3; j++)
				{
					int num4 = MyProject.Forms.PokemonEditor.MOVE_TUTOR_LIST_OFFSET + j * 2;
					byte[] bytes2 = BitConverter.GetBytes(this.tutorIds[j]);
					Array.Copy(bytes2, 0, this.romData, num4, 2);
				}
				MainForm.romData = this.romData;
			}
		}

		// Token: 0x04000675 RID: 1653
		private byte[] romData;

		// Token: 0x04000676 RID: 1654
		private List<ushort> tmHmIds;

		// Token: 0x04000677 RID: 1655
		private List<ushort> tutorIds;

		// Token: 0x04000678 RID: 1656
		private List<string> moveNames;

		// Token: 0x04000679 RID: 1657
		private bool hasUnsavedChanges;
	}
}
