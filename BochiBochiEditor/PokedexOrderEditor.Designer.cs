namespace BochiBochiEditor
{
	// Token: 0x0200001F RID: 31
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class PokedexOrderEditor : global::System.Windows.Forms.Form
	{
		// Token: 0x06000885 RID: 2181 RVA: 0x00042658 File Offset: 0x00040858
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

		// Token: 0x06000886 RID: 2182 RVA: 0x000426A8 File Offset: 0x000408A8
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.PokedexOrderEditor));
			this.lstPokemonCodePokedexOrder = new global::System.Windows.Forms.ListBox();
			this.txtPokemonCodeShow = new global::System.Windows.Forms.TextBox();
			this.picPokedexIcon = new global::System.Windows.Forms.PictureBox();
			this.btnUpdatePokedexOrder = new global::System.Windows.Forms.Button();
			this.lblPokedexOrderNumber = new global::System.Windows.Forms.Label();
			this.grpNote = new global::System.Windows.Forms.GroupBox();
			this.lblNote2 = new global::System.Windows.Forms.Label();
			this.lblNote1 = new global::System.Windows.Forms.Label();
			this.lstPokedexOrderUnused = new global::System.Windows.Forms.ListBox();
			this.lblPokedexOrderUnused = new global::System.Windows.Forms.Label();
			this.btnChangePokedexOrder = new global::System.Windows.Forms.Button();
			this.lblWarning = new global::System.Windows.Forms.Label();
			this.nudPokedexOrderNumber = new global::System.Windows.Forms.NumericUpDown();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexIcon).BeginInit();
			this.grpNote.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.nudPokedexOrderNumber).BeginInit();
			base.SuspendLayout();
			this.lstPokemonCodePokedexOrder.DrawMode = global::System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.lstPokemonCodePokedexOrder.FormattingEnabled = true;
			this.lstPokemonCodePokedexOrder.ItemHeight = 12;
			this.lstPokemonCodePokedexOrder.Location = new global::System.Drawing.Point(12, 12);
			this.lstPokemonCodePokedexOrder.Name = "lstPokemonCodePokedexOrder";
			this.lstPokemonCodePokedexOrder.Size = new global::System.Drawing.Size(152, 268);
			this.lstPokemonCodePokedexOrder.TabIndex = 0;
			this.txtPokemonCodeShow.Location = new global::System.Drawing.Point(224, 34);
			this.txtPokemonCodeShow.Name = "txtPokemonCodeShow";
			this.txtPokemonCodeShow.ReadOnly = true;
			this.txtPokemonCodeShow.Size = new global::System.Drawing.Size(120, 19);
			this.txtPokemonCodeShow.TabIndex = 1;
			this.picPokedexIcon.Location = new global::System.Drawing.Point(184, 22);
			this.picPokedexIcon.Name = "picPokedexIcon";
			this.picPokedexIcon.Size = new global::System.Drawing.Size(32, 32);
			this.picPokedexIcon.TabIndex = 2;
			this.picPokedexIcon.TabStop = false;
			this.btnUpdatePokedexOrder.Location = new global::System.Drawing.Point(356, 60);
			this.btnUpdatePokedexOrder.Name = "btnUpdatePokedexOrder";
			this.btnUpdatePokedexOrder.Size = new global::System.Drawing.Size(76, 23);
			this.btnUpdatePokedexOrder.TabIndex = 4;
			this.btnUpdatePokedexOrder.Text = "更新";
			this.btnUpdatePokedexOrder.UseVisualStyleBackColor = true;
			this.lblPokedexOrderNumber.AutoSize = true;
			this.lblPokedexOrderNumber.Location = new global::System.Drawing.Point(184, 66);
			this.lblPokedexOrderNumber.Name = "lblPokedexOrderNumber";
			this.lblPokedexOrderNumber.Size = new global::System.Drawing.Size(59, 12);
			this.lblPokedexOrderNumber.TabIndex = 5;
			this.lblPokedexOrderNumber.Text = "図鑑番号 :";
			this.grpNote.Controls.Add(this.lblNote2);
			this.grpNote.Controls.Add(this.lblNote1);
			this.grpNote.Location = new global::System.Drawing.Point(184, 96);
			this.grpNote.Name = "grpNote";
			this.grpNote.Size = new global::System.Drawing.Size(184, 68);
			this.grpNote.TabIndex = 6;
			this.grpNote.TabStop = false;
			this.grpNote.Text = "備考";
			this.lblNote2.AutoSize = true;
			this.lblNote2.Location = new global::System.Drawing.Point(18, 40);
			this.lblNote2.Name = "lblNote2";
			this.lblNote2.Size = new global::System.Drawing.Size(123, 12);
			this.lblNote2.TabIndex = 0;
			this.lblNote2.Text = "黄色 : 図鑑番号範囲外";
			this.lblNote1.AutoSize = true;
			this.lblNote1.Location = new global::System.Drawing.Point(18, 20);
			this.lblNote1.Name = "lblNote1";
			this.lblNote1.Size = new global::System.Drawing.Size(130, 12);
			this.lblNote1.TabIndex = 0;
			this.lblNote1.Text = "赤色 : 重複する図鑑番号";
			this.lstPokedexOrderUnused.FormattingEnabled = true;
			this.lstPokedexOrderUnused.ItemHeight = 12;
			this.lstPokedexOrderUnused.Location = new global::System.Drawing.Point(184, 194);
			this.lstPokedexOrderUnused.Name = "lstPokedexOrderUnused";
			this.lstPokedexOrderUnused.Size = new global::System.Drawing.Size(152, 88);
			this.lstPokedexOrderUnused.TabIndex = 7;
			this.lblPokedexOrderUnused.AutoSize = true;
			this.lblPokedexOrderUnused.Location = new global::System.Drawing.Point(184, 174);
			this.lblPokedexOrderUnused.Name = "lblPokedexOrderUnused";
			this.lblPokedexOrderUnused.Size = new global::System.Drawing.Size(105, 12);
			this.lblPokedexOrderUnused.TabIndex = 8;
			this.lblPokedexOrderUnused.Text = "未使用の図鑑番号 :";
			this.btnChangePokedexOrder.Enabled = false;
			this.btnChangePokedexOrder.Location = new global::System.Drawing.Point(356, 260);
			this.btnChangePokedexOrder.Name = "btnChangePokedexOrder";
			this.btnChangePokedexOrder.Size = new global::System.Drawing.Size(75, 23);
			this.btnChangePokedexOrder.TabIndex = 9;
			this.btnChangePokedexOrder.Text = "変更を保存";
			this.btnChangePokedexOrder.UseVisualStyleBackColor = true;
			this.lblWarning.AutoSize = true;
			this.lblWarning.Location = new global::System.Drawing.Point(22, 294);
			this.lblWarning.Name = "lblWarning";
			this.lblWarning.Size = new global::System.Drawing.Size(399, 12);
			this.lblWarning.TabIndex = 10;
			this.lblWarning.Text = "※「ポケモン」の図鑑タブは図鑑番号準拠なので図鑑番号を変更する場合は要修正";
			this.nudPokedexOrderNumber.Location = new global::System.Drawing.Point(248, 62);
			global::System.Windows.Forms.NumericUpDown nudPokedexOrderNumber = this.nudPokedexOrderNumber;
			int[] array = new int[4];
			array[0] = 65535;
			nudPokedexOrderNumber.Maximum = new decimal(array);
			this.nudPokedexOrderNumber.Name = "nudPokedexOrderNumber";
			this.nudPokedexOrderNumber.Size = new global::System.Drawing.Size(96, 19);
			this.nudPokedexOrderNumber.TabIndex = 11;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(452, 321);
			base.Controls.Add(this.nudPokedexOrderNumber);
			base.Controls.Add(this.lblWarning);
			base.Controls.Add(this.btnChangePokedexOrder);
			base.Controls.Add(this.lblPokedexOrderUnused);
			base.Controls.Add(this.lstPokedexOrderUnused);
			base.Controls.Add(this.grpNote);
			base.Controls.Add(this.lblPokedexOrderNumber);
			base.Controls.Add(this.btnUpdatePokedexOrder);
			base.Controls.Add(this.picPokedexIcon);
			base.Controls.Add(this.txtPokemonCodeShow);
			base.Controls.Add(this.lstPokemonCodePokedexOrder);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "PokedexOrderEditor";
			this.Text = "図鑑番号";
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexIcon).EndInit();
			this.grpNote.ResumeLayout(false);
			this.grpNote.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.nudPokedexOrderNumber).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040004B9 RID: 1209
				private ListBox _lstPokemonCodePokedexOrder;
		private Button _btnUpdatePokedexOrder;
		private Button _btnChangePokedexOrder;
        private global::System.ComponentModel.IContainer components;
	}
}
