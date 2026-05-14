namespace BochiBochiEditor
{
	// Token: 0x0200001E RID: 30
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class PokedexListEditor : global::System.Windows.Forms.Form
	{
		// Token: 0x0600083C RID: 2108 RVA: 0x00040448 File Offset: 0x0003E648
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

		// Token: 0x0600083D RID: 2109 RVA: 0x00040498 File Offset: 0x0003E698
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.PokedexListEditor));
			this.btnSave = new global::System.Windows.Forms.Button();
			this.lstPokedexListAiueo = new global::System.Windows.Forms.ListBox();
			this.grpPokedexListAiueo = new global::System.Windows.Forms.GroupBox();
			this.btnChangePokemon1 = new global::System.Windows.Forms.Button();
			this.txtPokedexListAiueoPokemonCode = new global::System.Windows.Forms.TextBox();
			this.picPokedexListAiueoIcon = new global::System.Windows.Forms.PictureBox();
			this.cmbPokedexListAiueo = new global::System.Windows.Forms.ComboBox();
			this.gtpPokedexListType = new global::System.Windows.Forms.GroupBox();
			this.btnChangePokemon2 = new global::System.Windows.Forms.Button();
			this.txtPokedexListTypePokemonCode = new global::System.Windows.Forms.TextBox();
			this.picPokedexListTypeIcon = new global::System.Windows.Forms.PictureBox();
			this.cmbPokedexListType = new global::System.Windows.Forms.ComboBox();
			this.lstPokedexListType = new global::System.Windows.Forms.ListBox();
			this.gtpPokedexListLight = new global::System.Windows.Forms.GroupBox();
			this.btnChangePokemon3 = new global::System.Windows.Forms.Button();
			this.txtPokedexListLightPokemonCode = new global::System.Windows.Forms.TextBox();
			this.picPokedexListLightIcon = new global::System.Windows.Forms.PictureBox();
			this.cmbPokedexListLight = new global::System.Windows.Forms.ComboBox();
			this.lstPokedexListLight = new global::System.Windows.Forms.ListBox();
			this.gtpPokedexListSmall = new global::System.Windows.Forms.GroupBox();
			this.btnChangePokemon4 = new global::System.Windows.Forms.Button();
			this.txtPokedexListSmallPokemonCode = new global::System.Windows.Forms.TextBox();
			this.picPokedexListSmallIcon = new global::System.Windows.Forms.PictureBox();
			this.cmbPokedexListSmall = new global::System.Windows.Forms.ComboBox();
			this.lstPokedexListSmall = new global::System.Windows.Forms.ListBox();
			this.grpPokedexListAiueo.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListAiueoIcon).BeginInit();
			this.gtpPokedexListType.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListTypeIcon).BeginInit();
			this.gtpPokedexListLight.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListLightIcon).BeginInit();
			this.gtpPokedexListSmall.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListSmallIcon).BeginInit();
			base.SuspendLayout();
			this.btnSave.Location = new global::System.Drawing.Point(16, 16);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new global::System.Drawing.Size(96, 23);
			this.btnSave.TabIndex = 0;
			this.btnSave.Text = "変更を保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.lstPokedexListAiueo.FormattingEnabled = true;
			this.lstPokedexListAiueo.ItemHeight = 12;
			this.lstPokedexListAiueo.Location = new global::System.Drawing.Point(12, 62);
			this.lstPokedexListAiueo.Name = "lstPokedexListAiueo";
			this.lstPokedexListAiueo.Size = new global::System.Drawing.Size(152, 172);
			this.lstPokedexListAiueo.TabIndex = 1;
			this.grpPokedexListAiueo.Controls.Add(this.btnChangePokemon1);
			this.grpPokedexListAiueo.Controls.Add(this.txtPokedexListAiueoPokemonCode);
			this.grpPokedexListAiueo.Controls.Add(this.picPokedexListAiueoIcon);
			this.grpPokedexListAiueo.Controls.Add(this.cmbPokedexListAiueo);
			this.grpPokedexListAiueo.Controls.Add(this.lstPokedexListAiueo);
			this.grpPokedexListAiueo.Location = new global::System.Drawing.Point(16, 52);
			this.grpPokedexListAiueo.Name = "grpPokedexListAiueo";
			this.grpPokedexListAiueo.Size = new global::System.Drawing.Size(178, 308);
			this.grpPokedexListAiueo.TabIndex = 2;
			this.grpPokedexListAiueo.TabStop = false;
			this.grpPokedexListAiueo.Text = "あいうえお順";
			this.btnChangePokemon1.Location = new global::System.Drawing.Point(12, 270);
			this.btnChangePokemon1.Name = "btnChangePokemon1";
			this.btnChangePokemon1.Size = new global::System.Drawing.Size(152, 23);
			this.btnChangePokemon1.TabIndex = 5;
			this.btnChangePokemon1.Text = "ポケモンを変更";
			this.btnChangePokemon1.UseVisualStyleBackColor = true;
			this.txtPokedexListAiueoPokemonCode.Location = new global::System.Drawing.Point(52, 33);
			this.txtPokedexListAiueoPokemonCode.Name = "txtPokedexListAiueoPokemonCode";
			this.txtPokedexListAiueoPokemonCode.ReadOnly = true;
			this.txtPokedexListAiueoPokemonCode.Size = new global::System.Drawing.Size(112, 19);
			this.txtPokedexListAiueoPokemonCode.TabIndex = 4;
			this.picPokedexListAiueoIcon.Location = new global::System.Drawing.Point(12, 20);
			this.picPokedexListAiueoIcon.Name = "picPokedexListAiueoIcon";
			this.picPokedexListAiueoIcon.Size = new global::System.Drawing.Size(32, 32);
			this.picPokedexListAiueoIcon.TabIndex = 3;
			this.picPokedexListAiueoIcon.TabStop = false;
			this.cmbPokedexListAiueo.FormattingEnabled = true;
			this.cmbPokedexListAiueo.Location = new global::System.Drawing.Point(12, 242);
			this.cmbPokedexListAiueo.Name = "cmbPokedexListAiueo";
			this.cmbPokedexListAiueo.Size = new global::System.Drawing.Size(152, 20);
			this.cmbPokedexListAiueo.TabIndex = 2;
			this.gtpPokedexListType.Controls.Add(this.btnChangePokemon2);
			this.gtpPokedexListType.Controls.Add(this.txtPokedexListTypePokemonCode);
			this.gtpPokedexListType.Controls.Add(this.picPokedexListTypeIcon);
			this.gtpPokedexListType.Controls.Add(this.cmbPokedexListType);
			this.gtpPokedexListType.Controls.Add(this.lstPokedexListType);
			this.gtpPokedexListType.Location = new global::System.Drawing.Point(206, 52);
			this.gtpPokedexListType.Name = "gtpPokedexListType";
			this.gtpPokedexListType.Size = new global::System.Drawing.Size(178, 308);
			this.gtpPokedexListType.TabIndex = 3;
			this.gtpPokedexListType.TabStop = false;
			this.gtpPokedexListType.Text = "タイプ順";
			this.btnChangePokemon2.Location = new global::System.Drawing.Point(12, 270);
			this.btnChangePokemon2.Name = "btnChangePokemon2";
			this.btnChangePokemon2.Size = new global::System.Drawing.Size(152, 23);
			this.btnChangePokemon2.TabIndex = 5;
			this.btnChangePokemon2.Text = "ポケモンを変更";
			this.btnChangePokemon2.UseVisualStyleBackColor = true;
			this.txtPokedexListTypePokemonCode.Location = new global::System.Drawing.Point(52, 33);
			this.txtPokedexListTypePokemonCode.Name = "txtPokedexListTypePokemonCode";
			this.txtPokedexListTypePokemonCode.ReadOnly = true;
			this.txtPokedexListTypePokemonCode.Size = new global::System.Drawing.Size(112, 19);
			this.txtPokedexListTypePokemonCode.TabIndex = 4;
			this.picPokedexListTypeIcon.Location = new global::System.Drawing.Point(12, 20);
			this.picPokedexListTypeIcon.Name = "picPokedexListTypeIcon";
			this.picPokedexListTypeIcon.Size = new global::System.Drawing.Size(32, 32);
			this.picPokedexListTypeIcon.TabIndex = 3;
			this.picPokedexListTypeIcon.TabStop = false;
			this.cmbPokedexListType.FormattingEnabled = true;
			this.cmbPokedexListType.Location = new global::System.Drawing.Point(12, 242);
			this.cmbPokedexListType.Name = "cmbPokedexListType";
			this.cmbPokedexListType.Size = new global::System.Drawing.Size(152, 20);
			this.cmbPokedexListType.TabIndex = 2;
			this.lstPokedexListType.FormattingEnabled = true;
			this.lstPokedexListType.ItemHeight = 12;
			this.lstPokedexListType.Location = new global::System.Drawing.Point(12, 62);
			this.lstPokedexListType.Name = "lstPokedexListType";
			this.lstPokedexListType.Size = new global::System.Drawing.Size(152, 172);
			this.lstPokedexListType.TabIndex = 1;
			this.gtpPokedexListLight.Controls.Add(this.btnChangePokemon3);
			this.gtpPokedexListLight.Controls.Add(this.txtPokedexListLightPokemonCode);
			this.gtpPokedexListLight.Controls.Add(this.picPokedexListLightIcon);
			this.gtpPokedexListLight.Controls.Add(this.cmbPokedexListLight);
			this.gtpPokedexListLight.Controls.Add(this.lstPokedexListLight);
			this.gtpPokedexListLight.Location = new global::System.Drawing.Point(396, 52);
			this.gtpPokedexListLight.Name = "gtpPokedexListLight";
			this.gtpPokedexListLight.Size = new global::System.Drawing.Size(178, 308);
			this.gtpPokedexListLight.TabIndex = 4;
			this.gtpPokedexListLight.TabStop = false;
			this.gtpPokedexListLight.Text = "かるい順";
			this.btnChangePokemon3.Location = new global::System.Drawing.Point(12, 270);
			this.btnChangePokemon3.Name = "btnChangePokemon3";
			this.btnChangePokemon3.Size = new global::System.Drawing.Size(152, 23);
			this.btnChangePokemon3.TabIndex = 5;
			this.btnChangePokemon3.Text = "ポケモンを変更";
			this.btnChangePokemon3.UseVisualStyleBackColor = true;
			this.txtPokedexListLightPokemonCode.Location = new global::System.Drawing.Point(52, 33);
			this.txtPokedexListLightPokemonCode.Name = "txtPokedexListLightPokemonCode";
			this.txtPokedexListLightPokemonCode.ReadOnly = true;
			this.txtPokedexListLightPokemonCode.Size = new global::System.Drawing.Size(112, 19);
			this.txtPokedexListLightPokemonCode.TabIndex = 4;
			this.picPokedexListLightIcon.Location = new global::System.Drawing.Point(12, 20);
			this.picPokedexListLightIcon.Name = "picPokedexListLightIcon";
			this.picPokedexListLightIcon.Size = new global::System.Drawing.Size(32, 32);
			this.picPokedexListLightIcon.TabIndex = 3;
			this.picPokedexListLightIcon.TabStop = false;
			this.cmbPokedexListLight.FormattingEnabled = true;
			this.cmbPokedexListLight.Location = new global::System.Drawing.Point(12, 242);
			this.cmbPokedexListLight.Name = "cmbPokedexListLight";
			this.cmbPokedexListLight.Size = new global::System.Drawing.Size(152, 20);
			this.cmbPokedexListLight.TabIndex = 2;
			this.lstPokedexListLight.FormattingEnabled = true;
			this.lstPokedexListLight.ItemHeight = 12;
			this.lstPokedexListLight.Location = new global::System.Drawing.Point(12, 62);
			this.lstPokedexListLight.Name = "lstPokedexListLight";
			this.lstPokedexListLight.Size = new global::System.Drawing.Size(152, 172);
			this.lstPokedexListLight.TabIndex = 1;
			this.gtpPokedexListSmall.Controls.Add(this.btnChangePokemon4);
			this.gtpPokedexListSmall.Controls.Add(this.txtPokedexListSmallPokemonCode);
			this.gtpPokedexListSmall.Controls.Add(this.picPokedexListSmallIcon);
			this.gtpPokedexListSmall.Controls.Add(this.cmbPokedexListSmall);
			this.gtpPokedexListSmall.Controls.Add(this.lstPokedexListSmall);
			this.gtpPokedexListSmall.Location = new global::System.Drawing.Point(586, 52);
			this.gtpPokedexListSmall.Name = "gtpPokedexListSmall";
			this.gtpPokedexListSmall.Size = new global::System.Drawing.Size(178, 308);
			this.gtpPokedexListSmall.TabIndex = 5;
			this.gtpPokedexListSmall.TabStop = false;
			this.gtpPokedexListSmall.Text = "ひくい順";
			this.btnChangePokemon4.Location = new global::System.Drawing.Point(12, 270);
			this.btnChangePokemon4.Name = "btnChangePokemon4";
			this.btnChangePokemon4.Size = new global::System.Drawing.Size(152, 23);
			this.btnChangePokemon4.TabIndex = 5;
			this.btnChangePokemon4.Text = "ポケモンを変更";
			this.btnChangePokemon4.UseVisualStyleBackColor = true;
			this.txtPokedexListSmallPokemonCode.Location = new global::System.Drawing.Point(52, 33);
			this.txtPokedexListSmallPokemonCode.Name = "txtPokedexListSmallPokemonCode";
			this.txtPokedexListSmallPokemonCode.ReadOnly = true;
			this.txtPokedexListSmallPokemonCode.Size = new global::System.Drawing.Size(112, 19);
			this.txtPokedexListSmallPokemonCode.TabIndex = 4;
			this.picPokedexListSmallIcon.Location = new global::System.Drawing.Point(12, 20);
			this.picPokedexListSmallIcon.Name = "picPokedexListSmallIcon";
			this.picPokedexListSmallIcon.Size = new global::System.Drawing.Size(32, 32);
			this.picPokedexListSmallIcon.TabIndex = 3;
			this.picPokedexListSmallIcon.TabStop = false;
			this.cmbPokedexListSmall.FormattingEnabled = true;
			this.cmbPokedexListSmall.Location = new global::System.Drawing.Point(12, 242);
			this.cmbPokedexListSmall.Name = "cmbPokedexListSmall";
			this.cmbPokedexListSmall.Size = new global::System.Drawing.Size(152, 20);
			this.cmbPokedexListSmall.TabIndex = 2;
			this.lstPokedexListSmall.FormattingEnabled = true;
			this.lstPokedexListSmall.ItemHeight = 12;
			this.lstPokedexListSmall.Location = new global::System.Drawing.Point(12, 62);
			this.lstPokedexListSmall.Name = "lstPokedexListSmall";
			this.lstPokedexListSmall.Size = new global::System.Drawing.Size(152, 172);
			this.lstPokedexListSmall.TabIndex = 1;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(782, 377);
			base.Controls.Add(this.gtpPokedexListSmall);
			base.Controls.Add(this.gtpPokedexListLight);
			base.Controls.Add(this.gtpPokedexListType);
			base.Controls.Add(this.grpPokedexListAiueo);
			base.Controls.Add(this.btnSave);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "PokedexListEditor";
			this.Text = "図鑑索引";
			this.grpPokedexListAiueo.ResumeLayout(false);
			this.grpPokedexListAiueo.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListAiueoIcon).EndInit();
			this.gtpPokedexListType.ResumeLayout(false);
			this.gtpPokedexListType.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListTypeIcon).EndInit();
			this.gtpPokedexListLight.ResumeLayout(false);
			this.gtpPokedexListLight.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListLightIcon).EndInit();
			this.gtpPokedexListSmall.ResumeLayout(false);
			this.gtpPokedexListSmall.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.picPokedexListSmallIcon).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x0400048D RID: 1165
				private Button _btnSave;
        private global::System.ComponentModel.IContainer components;
	}
}
