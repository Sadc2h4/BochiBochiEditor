namespace BochiBochiEditor
{
	// Token: 0x02000019 RID: 25
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class ItemUseCoordinate : global::System.Windows.Forms.Form
	{
		// Token: 0x060003CA RID: 970 RVA: 0x0001CC24 File Offset: 0x0001AE24
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

		// Token: 0x060003CB RID: 971 RVA: 0x0001CC74 File Offset: 0x0001AE74
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.ItemUseCoordinate));
			this.btnSave = new global::System.Windows.Forms.Button();
			this.grpItemUse1 = new global::System.Windows.Forms.GroupBox();
			this.nudItemUse1Y = new global::System.Windows.Forms.NumericUpDown();
			this.nudItemUse1X = new global::System.Windows.Forms.NumericUpDown();
			this.picItemUse1BackGround = new global::System.Windows.Forms.PictureBox();
			this.grpItemUse2 = new global::System.Windows.Forms.GroupBox();
			this.nudItemUse2Y = new global::System.Windows.Forms.NumericUpDown();
			this.picItemUse2BackGround1 = new global::System.Windows.Forms.PictureBox();
			this.nudItemUse2X = new global::System.Windows.Forms.NumericUpDown();
			this.cmbPokemonCode = new global::System.Windows.Forms.ComboBox();
			this.grpItemUse2Zoom = new global::System.Windows.Forms.GroupBox();
			this.picItemUse2BackGround2 = new global::System.Windows.Forms.PictureBox();
			this.nudItemUse2Zoom = new global::System.Windows.Forms.NumericUpDown();
			this.txtPokemonCodeShow = new global::System.Windows.Forms.TextBox();
			this.picPokemonIcon = new global::System.Windows.Forms.PictureBox();
			this.grpItemUse1.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse1Y).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse1X).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.picItemUse1BackGround).BeginInit();
			this.grpItemUse2.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse2Y).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.picItemUse2BackGround1).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse2X).BeginInit();
			this.grpItemUse2Zoom.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picItemUse2BackGround2).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse2Zoom).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.picPokemonIcon).BeginInit();
			base.SuspendLayout();
			this.btnSave.Location = new global::System.Drawing.Point(16, 18);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new global::System.Drawing.Size(100, 23);
			this.btnSave.TabIndex = 0;
			this.btnSave.Text = "変更を保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.grpItemUse1.Controls.Add(this.nudItemUse1Y);
			this.grpItemUse1.Controls.Add(this.nudItemUse1X);
			this.grpItemUse1.Controls.Add(this.picItemUse1BackGround);
			this.grpItemUse1.Location = new global::System.Drawing.Point(16, 52);
			this.grpItemUse1.Name = "grpItemUse1";
			this.grpItemUse1.Size = new global::System.Drawing.Size(272, 228);
			this.grpItemUse1.TabIndex = 1;
			this.grpItemUse1.TabStop = false;
			this.grpItemUse1.Text = "道具";
			this.nudItemUse1Y.Location = new global::System.Drawing.Point(90, 194);
			global::System.Windows.Forms.NumericUpDown nudItemUse1Y = this.nudItemUse1Y;
			int[] array = new int[4];
			array[0] = 255;
			nudItemUse1Y.Maximum = new decimal(array);
			this.nudItemUse1Y.Name = "nudItemUse1Y";
			this.nudItemUse1Y.Size = new global::System.Drawing.Size(64, 19);
			this.nudItemUse1Y.TabIndex = 1;
			this.nudItemUse1X.Location = new global::System.Drawing.Point(16, 194);
			global::System.Windows.Forms.NumericUpDown nudItemUse1X = this.nudItemUse1X;
			int[] array2 = new int[4];
			array2[0] = 255;
			nudItemUse1X.Maximum = new decimal(array2);
			this.nudItemUse1X.Name = "nudItemUse1X";
			this.nudItemUse1X.Size = new global::System.Drawing.Size(64, 19);
			this.nudItemUse1X.TabIndex = 1;
			this.picItemUse1BackGround.Location = new global::System.Drawing.Point(16, 24);
			this.picItemUse1BackGround.Name = "picItemUse1BackGround";
			this.picItemUse1BackGround.Size = new global::System.Drawing.Size(240, 160);
			this.picItemUse1BackGround.TabIndex = 0;
			this.picItemUse1BackGround.TabStop = false;
			this.grpItemUse2.Controls.Add(this.nudItemUse2Y);
			this.grpItemUse2.Controls.Add(this.picItemUse2BackGround1);
			this.grpItemUse2.Controls.Add(this.nudItemUse2X);
			this.grpItemUse2.Location = new global::System.Drawing.Point(302, 52);
			this.grpItemUse2.Name = "grpItemUse2";
			this.grpItemUse2.Size = new global::System.Drawing.Size(272, 228);
			this.grpItemUse2.TabIndex = 2;
			this.grpItemUse2.TabStop = false;
			this.grpItemUse2.Text = "技マシン";
			this.nudItemUse2Y.Location = new global::System.Drawing.Point(90, 194);
			global::System.Windows.Forms.NumericUpDown nudItemUse2Y = this.nudItemUse2Y;
			int[] array3 = new int[4];
			array3[0] = 255;
			nudItemUse2Y.Maximum = new decimal(array3);
			this.nudItemUse2Y.Name = "nudItemUse2Y";
			this.nudItemUse2Y.Size = new global::System.Drawing.Size(64, 19);
			this.nudItemUse2Y.TabIndex = 1;
			this.picItemUse2BackGround1.Location = new global::System.Drawing.Point(16, 24);
			this.picItemUse2BackGround1.Name = "picItemUse2BackGround1";
			this.picItemUse2BackGround1.Size = new global::System.Drawing.Size(240, 160);
			this.picItemUse2BackGround1.TabIndex = 0;
			this.picItemUse2BackGround1.TabStop = false;
			this.nudItemUse2X.Location = new global::System.Drawing.Point(16, 194);
			global::System.Windows.Forms.NumericUpDown nudItemUse2X = this.nudItemUse2X;
			int[] array4 = new int[4];
			array4[0] = 255;
			nudItemUse2X.Maximum = new decimal(array4);
			this.nudItemUse2X.Name = "nudItemUse2X";
			this.nudItemUse2X.Size = new global::System.Drawing.Size(64, 19);
			this.nudItemUse2X.TabIndex = 1;
			this.cmbPokemonCode.FormattingEnabled = true;
			this.cmbPokemonCode.Location = new global::System.Drawing.Point(172, 20);
			this.cmbPokemonCode.Name = "cmbPokemonCode";
			this.cmbPokemonCode.Size = new global::System.Drawing.Size(120, 20);
			this.cmbPokemonCode.TabIndex = 3;
			this.grpItemUse2Zoom.Controls.Add(this.picItemUse2BackGround2);
			this.grpItemUse2Zoom.Controls.Add(this.nudItemUse2Zoom);
			this.grpItemUse2Zoom.Location = new global::System.Drawing.Point(302, 290);
			this.grpItemUse2Zoom.Name = "grpItemUse2Zoom";
			this.grpItemUse2Zoom.Size = new global::System.Drawing.Size(272, 228);
			this.grpItemUse2Zoom.TabIndex = 2;
			this.grpItemUse2Zoom.TabStop = false;
			this.grpItemUse2Zoom.Text = "技マシン（ズーム補正）";
			this.picItemUse2BackGround2.Location = new global::System.Drawing.Point(16, 24);
			this.picItemUse2BackGround2.Name = "picItemUse2BackGround2";
			this.picItemUse2BackGround2.Size = new global::System.Drawing.Size(240, 160);
			this.picItemUse2BackGround2.TabIndex = 0;
			this.picItemUse2BackGround2.TabStop = false;
			this.nudItemUse2Zoom.Location = new global::System.Drawing.Point(16, 194);
			global::System.Windows.Forms.NumericUpDown nudItemUse2Zoom = this.nudItemUse2Zoom;
			int[] array5 = new int[4];
			array5[0] = 255;
			nudItemUse2Zoom.Maximum = new decimal(array5);
			this.nudItemUse2Zoom.Name = "nudItemUse2Zoom";
			this.nudItemUse2Zoom.Size = new global::System.Drawing.Size(64, 19);
			this.nudItemUse2Zoom.TabIndex = 1;
			this.txtPokemonCodeShow.Location = new global::System.Drawing.Point(302, 20);
			this.txtPokemonCodeShow.Name = "txtPokemonCodeShow";
			this.txtPokemonCodeShow.ReadOnly = true;
			this.txtPokemonCodeShow.Size = new global::System.Drawing.Size(120, 19);
			this.txtPokemonCodeShow.TabIndex = 4;
			this.picPokemonIcon.Location = new global::System.Drawing.Point(128, 8);
			this.picPokemonIcon.Name = "picPokemonIcon";
			this.picPokemonIcon.Size = new global::System.Drawing.Size(32, 32);
			this.picPokemonIcon.TabIndex = 5;
			this.picPokemonIcon.TabStop = false;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(590, 535);
			base.Controls.Add(this.picPokemonIcon);
			base.Controls.Add(this.txtPokemonCodeShow);
			base.Controls.Add(this.cmbPokemonCode);
			base.Controls.Add(this.grpItemUse2Zoom);
			base.Controls.Add(this.grpItemUse2);
			base.Controls.Add(this.grpItemUse1);
			base.Controls.Add(this.btnSave);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "ItemUseCoordinate";
			this.Text = "アイテム使用表示位置";
			this.grpItemUse1.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse1Y).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse1X).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.picItemUse1BackGround).EndInit();
			this.grpItemUse2.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse2Y).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.picItemUse2BackGround1).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse2X).EndInit();
			this.grpItemUse2Zoom.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.picItemUse2BackGround2).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudItemUse2Zoom).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.picPokemonIcon).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000202 RID: 514
				private Button _btnSave;
		private NumericUpDown _nudItemUse1Y;
		private NumericUpDown _nudItemUse1X;
		private ComboBox _cmbPokemonCode;
		private NumericUpDown _nudItemUse2Y;
		private NumericUpDown _nudItemUse2X;
		private NumericUpDown _nudItemUse2Zoom;
        private global::System.ComponentModel.IContainer components;
	}
}
