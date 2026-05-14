namespace BochiBochiEditor
{
	// Token: 0x02000016 RID: 22
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class InsertNewPokemonData : global::System.Windows.Forms.Form
	{
		// Token: 0x0600030E RID: 782 RVA: 0x000178A4 File Offset: 0x00015AA4
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

		// Token: 0x0600030F RID: 783 RVA: 0x000178F4 File Offset: 0x00015AF4
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.InsertNewPokemonData));
			this.btnInsertNewPokemonData = new global::System.Windows.Forms.Button();
			this.lblNewTrainerDataType = new global::System.Windows.Forms.Label();
			this.lblNewAddress = new global::System.Windows.Forms.Label();
			this.txtNewPokemonDataAddress = new global::System.Windows.Forms.TextBox();
			this.cmbNewTrainerDataType = new global::System.Windows.Forms.ComboBox();
			this.nudNewPokemonDataNum = new global::System.Windows.Forms.NumericUpDown();
			this.lblNewPokemonDataNum = new global::System.Windows.Forms.Label();
			((global::System.ComponentModel.ISupportInitialize)this.nudNewPokemonDataNum).BeginInit();
			base.SuspendLayout();
			this.btnInsertNewPokemonData.Location = new global::System.Drawing.Point(236, 66);
			this.btnInsertNewPokemonData.Name = "btnInsertNewPokemonData";
			this.btnInsertNewPokemonData.Size = new global::System.Drawing.Size(76, 23);
			this.btnInsertNewPokemonData.TabIndex = 9;
			this.btnInsertNewPokemonData.Text = "生成";
			this.btnInsertNewPokemonData.UseVisualStyleBackColor = true;
			this.lblNewTrainerDataType.AutoSize = true;
			this.lblNewTrainerDataType.Location = new global::System.Drawing.Point(16, 20);
			this.lblNewTrainerDataType.Name = "lblNewTrainerDataType";
			this.lblNewTrainerDataType.Size = new global::System.Drawing.Size(65, 12);
			this.lblNewTrainerDataType.TabIndex = 7;
			this.lblNewTrainerDataType.Text = "データタイプ :";
			this.lblNewAddress.AutoSize = true;
			this.lblNewAddress.Location = new global::System.Drawing.Point(16, 46);
			this.lblNewAddress.Name = "lblNewAddress";
			this.lblNewAddress.Size = new global::System.Drawing.Size(83, 12);
			this.lblNewAddress.TabIndex = 6;
			this.lblNewAddress.Text = "生成先アドレス :";
			this.txtNewPokemonDataAddress.Location = new global::System.Drawing.Point(110, 42);
			this.txtNewPokemonDataAddress.Name = "txtNewPokemonDataAddress";
			this.txtNewPokemonDataAddress.Size = new global::System.Drawing.Size(100, 19);
			this.txtNewPokemonDataAddress.TabIndex = 5;
			this.txtNewPokemonDataAddress.Text = "00000000";
			this.cmbNewTrainerDataType.DropDownStyle = global::System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbNewTrainerDataType.FormattingEnabled = true;
			this.cmbNewTrainerDataType.Items.AddRange(new object[] { "00 - 普通", "01 - 技指定可", "02 - アイテム指定可", "03 - 両方指定可" });
			this.cmbNewTrainerDataType.Location = new global::System.Drawing.Point(110, 15);
			this.cmbNewTrainerDataType.Name = "cmbNewTrainerDataType";
			this.cmbNewTrainerDataType.Size = new global::System.Drawing.Size(128, 20);
			this.cmbNewTrainerDataType.TabIndex = 10;
			this.nudNewPokemonDataNum.Location = new global::System.Drawing.Point(110, 68);
			global::System.Windows.Forms.NumericUpDown nudNewPokemonDataNum = this.nudNewPokemonDataNum;
			int[] array = new int[4];
			array[0] = 6;
			nudNewPokemonDataNum.Maximum = new decimal(array);
			global::System.Windows.Forms.NumericUpDown nudNewPokemonDataNum2 = this.nudNewPokemonDataNum;
			int[] array2 = new int[4];
			array2[0] = 1;
			nudNewPokemonDataNum2.Minimum = new decimal(array2);
			this.nudNewPokemonDataNum.Name = "nudNewPokemonDataNum";
			this.nudNewPokemonDataNum.Size = new global::System.Drawing.Size(48, 19);
			this.nudNewPokemonDataNum.TabIndex = 11;
			global::System.Windows.Forms.NumericUpDown nudNewPokemonDataNum3 = this.nudNewPokemonDataNum;
			int[] array3 = new int[4];
			array3[0] = 1;
			nudNewPokemonDataNum3.Value = new decimal(array3);
			this.lblNewPokemonDataNum.AutoSize = true;
			this.lblNewPokemonDataNum.Location = new global::System.Drawing.Point(16, 72);
			this.lblNewPokemonDataNum.Name = "lblNewPokemonDataNum";
			this.lblNewPokemonDataNum.Size = new global::System.Drawing.Size(56, 12);
			this.lblNewPokemonDataNum.TabIndex = 12;
			this.lblNewPokemonDataNum.Text = "手持ち数 :";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(330, 107);
			base.Controls.Add(this.lblNewPokemonDataNum);
			base.Controls.Add(this.nudNewPokemonDataNum);
			base.Controls.Add(this.cmbNewTrainerDataType);
			base.Controls.Add(this.btnInsertNewPokemonData);
			base.Controls.Add(this.lblNewTrainerDataType);
			base.Controls.Add(this.lblNewAddress);
			base.Controls.Add(this.txtNewPokemonDataAddress);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "InsertNewPokemonData";
			this.Text = "新しい手持ちデータを生成";
			((global::System.ComponentModel.ISupportInitialize)this.nudNewPokemonDataNum).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400019D RID: 413
				private Button _btnInsertNewPokemonData;
        private global::System.ComponentModel.IContainer components;
	}
}
