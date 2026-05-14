namespace BochiBochiEditor
{
	// Token: 0x02000015 RID: 21
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class InsertNewPokedexTable : global::System.Windows.Forms.Form
	{
		// Token: 0x060002FB RID: 763 RVA: 0x000173A0 File Offset: 0x000155A0
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

		// Token: 0x060002FC RID: 764 RVA: 0x000173F0 File Offset: 0x000155F0
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.InsertNewPokedexTable));
			this.btnInsertNewPokedexTable = new global::System.Windows.Forms.Button();
			this.nudPokedexPageNum = new global::System.Windows.Forms.NumericUpDown();
			this.lblPokedexPageNum = new global::System.Windows.Forms.Label();
			this.txtPokedexTableAddress = new global::System.Windows.Forms.TextBox();
			this.lblPokedexTableAddress = new global::System.Windows.Forms.Label();
			((global::System.ComponentModel.ISupportInitialize)this.nudPokedexPageNum).BeginInit();
			base.SuspendLayout();
			this.btnInsertNewPokedexTable.Location = new global::System.Drawing.Point(228, 40);
			this.btnInsertNewPokedexTable.Name = "btnInsertNewPokedexTable";
			this.btnInsertNewPokedexTable.Size = new global::System.Drawing.Size(76, 23);
			this.btnInsertNewPokedexTable.TabIndex = 9;
			this.btnInsertNewPokedexTable.Text = "生成";
			this.btnInsertNewPokedexTable.UseVisualStyleBackColor = true;
			this.nudPokedexPageNum.Location = new global::System.Drawing.Point(110, 42);
			global::System.Windows.Forms.NumericUpDown nudPokedexPageNum = this.nudPokedexPageNum;
			int[] array = new int[4];
			array[0] = 255;
			nudPokedexPageNum.Maximum = new decimal(array);
			this.nudPokedexPageNum.Name = "nudPokedexPageNum";
			this.nudPokedexPageNum.Size = new global::System.Drawing.Size(48, 19);
			this.nudPokedexPageNum.TabIndex = 8;
			this.lblPokedexPageNum.AutoSize = true;
			this.lblPokedexPageNum.Location = new global::System.Drawing.Point(16, 46);
			this.lblPokedexPageNum.Name = "lblPokedexPageNum";
			this.lblPokedexPageNum.Size = new global::System.Drawing.Size(65, 12);
			this.lblPokedexPageNum.TabIndex = 7;
			this.lblPokedexPageNum.Text = "総ページ数 :";
			this.txtPokedexTableAddress.Location = new global::System.Drawing.Point(110, 15);
			this.txtPokedexTableAddress.Name = "txtPokedexTableAddress";
			this.txtPokedexTableAddress.Size = new global::System.Drawing.Size(100, 19);
			this.txtPokedexTableAddress.TabIndex = 6;
			this.txtPokedexTableAddress.Text = "00000000";
			this.lblPokedexTableAddress.AutoSize = true;
			this.lblPokedexTableAddress.Location = new global::System.Drawing.Point(16, 20);
			this.lblPokedexTableAddress.Name = "lblPokedexTableAddress";
			this.lblPokedexTableAddress.Size = new global::System.Drawing.Size(83, 12);
			this.lblPokedexTableAddress.TabIndex = 5;
			this.lblPokedexTableAddress.Text = "生成先アドレス :";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(324, 81);
			base.Controls.Add(this.btnInsertNewPokedexTable);
			base.Controls.Add(this.nudPokedexPageNum);
			base.Controls.Add(this.lblPokedexPageNum);
			base.Controls.Add(this.txtPokedexTableAddress);
			base.Controls.Add(this.lblPokedexTableAddress);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "InsertNewPokedexTable";
			this.Text = "新しい図鑑テーブルを生成";
			((global::System.ComponentModel.ISupportInitialize)this.nudPokedexPageNum).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000195 RID: 405
				private Button _btnInsertNewPokedexTable;
        private global::System.ComponentModel.IContainer components;
	}
}
