namespace BochiBochiEditor
{
	// Token: 0x02000027 RID: 39
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class TmHmTutorEditor : global::System.Windows.Forms.Form
	{
		// Token: 0x06000BA9 RID: 2985 RVA: 0x000575D0 File Offset: 0x000557D0
		[global::System.Diagnostics.DebuggerNonUserCode]
		protected override void Dispose(bool disposing)
		{
			try
			{
				bool flag = disposing && this.components != null;
				if (flag)
				{
					this.components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x00057620 File Offset: 0x00055820
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.TmHmTutorEditor));
			this.cmbMoveList1 = new global::System.Windows.Forms.ComboBox();
			this.cmbMoveList2 = new global::System.Windows.Forms.ComboBox();
			this.btnChangeMoveName1 = new global::System.Windows.Forms.Button();
			this.btnChangeMoveName2 = new global::System.Windows.Forms.Button();
			this.btnSave = new global::System.Windows.Forms.Button();
			this.lstTmHmList = new global::System.Windows.Forms.ListBox();
			this.lstTutorList = new global::System.Windows.Forms.ListBox();
			base.SuspendLayout();
			this.cmbMoveList1.FormattingEnabled = true;
			this.cmbMoveList1.Location = new global::System.Drawing.Point(14, 342);
			this.cmbMoveList1.Name = "cmbMoveList1";
			this.cmbMoveList1.Size = new global::System.Drawing.Size(188, 20);
			this.cmbMoveList1.TabIndex = 1;
			this.cmbMoveList2.FormattingEnabled = true;
			this.cmbMoveList2.Location = new global::System.Drawing.Point(216, 342);
			this.cmbMoveList2.Name = "cmbMoveList2";
			this.cmbMoveList2.Size = new global::System.Drawing.Size(188, 20);
			this.cmbMoveList2.TabIndex = 1;
			this.btnChangeMoveName1.Location = new global::System.Drawing.Point(14, 368);
			this.btnChangeMoveName1.Name = "btnChangeMoveName1";
			this.btnChangeMoveName1.Size = new global::System.Drawing.Size(188, 23);
			this.btnChangeMoveName1.TabIndex = 2;
			this.btnChangeMoveName1.Text = "技名の変更を反映";
			this.btnChangeMoveName1.UseVisualStyleBackColor = true;
			this.btnChangeMoveName2.Location = new global::System.Drawing.Point(216, 368);
			this.btnChangeMoveName2.Name = "btnChangeMoveName2";
			this.btnChangeMoveName2.Size = new global::System.Drawing.Size(188, 23);
			this.btnChangeMoveName2.TabIndex = 2;
			this.btnChangeMoveName2.Text = "技名の変更を反映";
			this.btnChangeMoveName2.UseVisualStyleBackColor = true;
			this.btnSave.Location = new global::System.Drawing.Point(16, 16);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new global::System.Drawing.Size(100, 23);
			this.btnSave.TabIndex = 3;
			this.btnSave.Text = "変更を保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.lstTmHmList.FormattingEnabled = true;
			this.lstTmHmList.ItemHeight = 12;
			this.lstTmHmList.Location = new global::System.Drawing.Point(15, 50);
			this.lstTmHmList.Name = "lstTmHmList";
			this.lstTmHmList.Size = new global::System.Drawing.Size(186, 280);
			this.lstTmHmList.TabIndex = 4;
			this.lstTutorList.FormattingEnabled = true;
			this.lstTutorList.ItemHeight = 12;
			this.lstTutorList.Location = new global::System.Drawing.Point(217, 50);
			this.lstTutorList.Name = "lstTutorList";
			this.lstTutorList.Size = new global::System.Drawing.Size(186, 280);
			this.lstTutorList.TabIndex = 5;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(422, 403);
			base.Controls.Add(this.lstTutorList);
			base.Controls.Add(this.lstTmHmList);
			base.Controls.Add(this.btnSave);
			base.Controls.Add(this.btnChangeMoveName2);
			base.Controls.Add(this.btnChangeMoveName1);
			base.Controls.Add(this.cmbMoveList2);
			base.Controls.Add(this.cmbMoveList1);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "TmHmTutorEditor";
			this.Text = "技マシン/教え技";
			base.ResumeLayout(false);
		}

		// Token: 0x0400066D RID: 1645
				private Button _btnChangeMoveName1;
		private Button _btnChangeMoveName2;
		private Button _btnSave;
		private ListBox _lstTmHmList;
		private ListBox _lstTutorList;
        private global::System.ComponentModel.IContainer components;
	}
}
