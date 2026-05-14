namespace BochiBochiEditor
{
	// Token: 0x02000014 RID: 20
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class InsertNewPokedexData : global::System.Windows.Forms.Form
	{
		// Token: 0x060002E8 RID: 744 RVA: 0x00016EB4 File Offset: 0x000150B4
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

		// Token: 0x060002E9 RID: 745 RVA: 0x00016F04 File Offset: 0x00015104
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.InsertNewPokedexData));
			this.lblPokedexDataAddress = new global::System.Windows.Forms.Label();
			this.txtPokedexDataAddress = new global::System.Windows.Forms.TextBox();
			this.lblPokedexPokemonNum = new global::System.Windows.Forms.Label();
			this.nudPokedexPokemonNum = new global::System.Windows.Forms.NumericUpDown();
			this.btnInsertNewPokedexData = new global::System.Windows.Forms.Button();
			((global::System.ComponentModel.ISupportInitialize)this.nudPokedexPokemonNum).BeginInit();
			base.SuspendLayout();
			this.lblPokedexDataAddress.AutoSize = true;
			this.lblPokedexDataAddress.Location = new global::System.Drawing.Point(16, 20);
			this.lblPokedexDataAddress.Name = "lblPokedexDataAddress";
			this.lblPokedexDataAddress.Size = new global::System.Drawing.Size(83, 12);
			this.lblPokedexDataAddress.TabIndex = 0;
			this.lblPokedexDataAddress.Text = "生成先アドレス :";
			this.txtPokedexDataAddress.Location = new global::System.Drawing.Point(110, 15);
			this.txtPokedexDataAddress.Name = "txtPokedexDataAddress";
			this.txtPokedexDataAddress.Size = new global::System.Drawing.Size(100, 19);
			this.txtPokedexDataAddress.TabIndex = 1;
			this.txtPokedexDataAddress.Text = "00000000";
			this.lblPokedexPokemonNum.AutoSize = true;
			this.lblPokedexPokemonNum.Location = new global::System.Drawing.Point(16, 46);
			this.lblPokedexPokemonNum.Name = "lblPokedexPokemonNum";
			this.lblPokedexPokemonNum.Size = new global::System.Drawing.Size(60, 12);
			this.lblPokedexPokemonNum.TabIndex = 2;
			this.lblPokedexPokemonNum.Text = "ポケモン数 :";
			this.nudPokedexPokemonNum.Location = new global::System.Drawing.Point(110, 42);
			global::System.Windows.Forms.NumericUpDown nudPokedexPokemonNum = this.nudPokedexPokemonNum;
			int[] array = new int[4];
			array[0] = 4;
			nudPokedexPokemonNum.Maximum = new decimal(array);
			this.nudPokedexPokemonNum.Name = "nudPokedexPokemonNum";
			this.nudPokedexPokemonNum.Size = new global::System.Drawing.Size(48, 19);
			this.nudPokedexPokemonNum.TabIndex = 3;
			this.btnInsertNewPokedexData.Location = new global::System.Drawing.Point(228, 40);
			this.btnInsertNewPokedexData.Name = "btnInsertNewPokedexData";
			this.btnInsertNewPokedexData.Size = new global::System.Drawing.Size(76, 23);
			this.btnInsertNewPokedexData.TabIndex = 4;
			this.btnInsertNewPokedexData.Text = "生成";
			this.btnInsertNewPokedexData.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(324, 81);
			base.Controls.Add(this.btnInsertNewPokedexData);
			base.Controls.Add(this.nudPokedexPokemonNum);
			base.Controls.Add(this.lblPokedexPokemonNum);
			base.Controls.Add(this.txtPokedexDataAddress);
			base.Controls.Add(this.lblPokedexDataAddress);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "InsertNewPokedexData";
			this.Text = "新しい図鑑データを生成";
			((global::System.ComponentModel.ISupportInitialize)this.nudPokedexPokemonNum).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400018D RID: 397
				private Button _btnInsertNewPokedexData;
        private global::System.ComponentModel.IContainer components;
	}
}
