namespace BochiBochiEditor
{
	// Token: 0x0200000D RID: 13
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class EggMoveEditor : global::System.Windows.Forms.Form
	{
		// Token: 0x0600009D RID: 157 RVA: 0x00007390 File Offset: 0x00005590
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

		// Token: 0x0600009E RID: 158 RVA: 0x000073E0 File Offset: 0x000055E0
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.EggMoveEditor));
			this.lstEggMoveTable = new global::System.Windows.Forms.ListBox();
			this.btnPokemonInsert = new global::System.Windows.Forms.Button();
			this.btnPokemonReplace = new global::System.Windows.Forms.Button();
			this.cmbPokemonList = new global::System.Windows.Forms.ComboBox();
			this.btnDeletePokemon = new global::System.Windows.Forms.Button();
			this.btnMoveInsert = new global::System.Windows.Forms.Button();
			this.btnMoveReplace = new global::System.Windows.Forms.Button();
			this.cmbMoveList = new global::System.Windows.Forms.ComboBox();
			this.btnSave = new global::System.Windows.Forms.Button();
			this.picPokemon = new global::System.Windows.Forms.PictureBox();
			this.lblwarning = new global::System.Windows.Forms.Label();
			this.grpControlPokemon = new global::System.Windows.Forms.GroupBox();
			this.grpControlMove = new global::System.Windows.Forms.GroupBox();
			this.btnDeleteMove = new global::System.Windows.Forms.Button();
			((global::System.ComponentModel.ISupportInitialize)this.picPokemon).BeginInit();
			this.grpControlPokemon.SuspendLayout();
			this.grpControlMove.SuspendLayout();
			base.SuspendLayout();
			this.lstEggMoveTable.FormattingEnabled = true;
			this.lstEggMoveTable.ItemHeight = 12;
			this.lstEggMoveTable.Location = new global::System.Drawing.Point(12, 12);
			this.lstEggMoveTable.Name = "lstEggMoveTable";
			this.lstEggMoveTable.Size = new global::System.Drawing.Size(152, 340);
			this.lstEggMoveTable.TabIndex = 0;
			this.btnPokemonInsert.Location = new global::System.Drawing.Point(16, 30);
			this.btnPokemonInsert.Name = "btnPokemonInsert";
			this.btnPokemonInsert.Size = new global::System.Drawing.Size(78, 23);
			this.btnPokemonInsert.TabIndex = 1;
			this.btnPokemonInsert.Text = "挿入";
			this.btnPokemonInsert.UseVisualStyleBackColor = true;
			this.btnPokemonReplace.Location = new global::System.Drawing.Point(16, 60);
			this.btnPokemonReplace.Name = "btnPokemonReplace";
			this.btnPokemonReplace.Size = new global::System.Drawing.Size(78, 23);
			this.btnPokemonReplace.TabIndex = 1;
			this.btnPokemonReplace.Text = "置換";
			this.btnPokemonReplace.UseVisualStyleBackColor = true;
			this.cmbPokemonList.FormattingEnabled = true;
			this.cmbPokemonList.Location = new global::System.Drawing.Point(104, 23);
			this.cmbPokemonList.Name = "cmbPokemonList";
			this.cmbPokemonList.Size = new global::System.Drawing.Size(120, 20);
			this.cmbPokemonList.TabIndex = 2;
			this.btnDeletePokemon.Location = new global::System.Drawing.Point(16, 90);
			this.btnDeletePokemon.Name = "btnDeletePokemon";
			this.btnDeletePokemon.Size = new global::System.Drawing.Size(78, 23);
			this.btnDeletePokemon.TabIndex = 1;
			this.btnDeletePokemon.Text = "削除";
			this.btnDeletePokemon.UseVisualStyleBackColor = true;
			this.btnMoveInsert.Location = new global::System.Drawing.Point(16, 30);
			this.btnMoveInsert.Name = "btnMoveInsert";
			this.btnMoveInsert.Size = new global::System.Drawing.Size(78, 23);
			this.btnMoveInsert.TabIndex = 1;
			this.btnMoveInsert.Text = "挿入";
			this.btnMoveInsert.UseVisualStyleBackColor = true;
			this.btnMoveReplace.Location = new global::System.Drawing.Point(16, 60);
			this.btnMoveReplace.Name = "btnMoveReplace";
			this.btnMoveReplace.Size = new global::System.Drawing.Size(78, 23);
			this.btnMoveReplace.TabIndex = 1;
			this.btnMoveReplace.Text = "置換";
			this.btnMoveReplace.UseVisualStyleBackColor = true;
			this.cmbMoveList.FormattingEnabled = true;
			this.cmbMoveList.Location = new global::System.Drawing.Point(104, 23);
			this.cmbMoveList.Name = "cmbMoveList";
			this.cmbMoveList.Size = new global::System.Drawing.Size(120, 20);
			this.cmbMoveList.TabIndex = 2;
			this.btnSave.Location = new global::System.Drawing.Point(192, 20);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new global::System.Drawing.Size(120, 23);
			this.btnSave.TabIndex = 3;
			this.btnSave.Text = "変更を保存";
			this.btnSave.UseVisualStyleBackColor = true;
			this.picPokemon.Location = new global::System.Drawing.Point(130, 52);
			this.picPokemon.Name = "picPokemon";
			this.picPokemon.Size = new global::System.Drawing.Size(64, 64);
			this.picPokemon.TabIndex = 4;
			this.picPokemon.TabStop = false;
			this.lblwarning.AutoSize = true;
			this.lblwarning.Location = new global::System.Drawing.Point(180, 336);
			this.lblwarning.Name = "lblwarning";
			this.lblwarning.Size = new global::System.Drawing.Size(211, 12);
			this.lblwarning.TabIndex = 5;
			this.lblwarning.Text = "※タマゴ技が増える場合は、要テーブル移動";
			this.grpControlPokemon.Controls.Add(this.btnPokemonInsert);
			this.grpControlPokemon.Controls.Add(this.btnPokemonReplace);
			this.grpControlPokemon.Controls.Add(this.picPokemon);
			this.grpControlPokemon.Controls.Add(this.cmbPokemonList);
			this.grpControlPokemon.Controls.Add(this.btnDeletePokemon);
			this.grpControlPokemon.Location = new global::System.Drawing.Point(176, 56);
			this.grpControlPokemon.Name = "grpControlPokemon";
			this.grpControlPokemon.Size = new global::System.Drawing.Size(240, 130);
			this.grpControlPokemon.TabIndex = 6;
			this.grpControlPokemon.TabStop = false;
			this.grpControlPokemon.Text = "ポケモン";
			this.grpControlMove.Controls.Add(this.btnDeleteMove);
			this.grpControlMove.Controls.Add(this.btnMoveInsert);
			this.grpControlMove.Controls.Add(this.btnMoveReplace);
			this.grpControlMove.Controls.Add(this.cmbMoveList);
			this.grpControlMove.Location = new global::System.Drawing.Point(176, 196);
			this.grpControlMove.Name = "grpControlMove";
			this.grpControlMove.Size = new global::System.Drawing.Size(240, 130);
			this.grpControlMove.TabIndex = 7;
			this.grpControlMove.TabStop = false;
			this.grpControlMove.Text = "技";
			this.btnDeleteMove.Location = new global::System.Drawing.Point(16, 90);
			this.btnDeleteMove.Name = "btnDeleteMove";
			this.btnDeleteMove.Size = new global::System.Drawing.Size(78, 23);
			this.btnDeleteMove.TabIndex = 3;
			this.btnDeleteMove.Text = "削除";
			this.btnDeleteMove.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(430, 365);
			base.Controls.Add(this.grpControlMove);
			base.Controls.Add(this.grpControlPokemon);
			base.Controls.Add(this.lblwarning);
			base.Controls.Add(this.btnSave);
			base.Controls.Add(this.lstEggMoveTable);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "EggMoveEditor";
			this.Text = "タマゴ技";
			((global::System.ComponentModel.ISupportInitialize)this.picPokemon).EndInit();
			this.grpControlPokemon.ResumeLayout(false);
			this.grpControlMove.ResumeLayout(false);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400005F RID: 95
				private ListBox _lstEggMoveTable;
		private Button _btnPokemonInsert;
		private Button _btnPokemonReplace;
		private ComboBox _cmbPokemonList;
		private Button _btnDeletePokemon;
		private Button _btnMoveInsert;
		private Button _btnMoveReplace;
		private Button _btnSave;
		private Button _btnDeleteMove;
        private global::System.ComponentModel.IContainer components;
	}
}
