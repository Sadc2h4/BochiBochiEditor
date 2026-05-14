namespace BochiBochiEditor
{
	// Token: 0x02000029 RID: 41
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class TrainerSpriteEditor : global::System.Windows.Forms.Form
	{
		// Token: 0x06000C67 RID: 3175 RVA: 0x0005CD48 File Offset: 0x0005AF48
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

		// Token: 0x06000C68 RID: 3176 RVA: 0x0005CD98 File Offset: 0x0005AF98
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.TrainerSpriteEditor));
			this.nudTrainerSpriteYPosition = new global::System.Windows.Forms.NumericUpDown();
			this.lblTrainerSpriteAnimationPointer = new global::System.Windows.Forms.Label();
			this.lblTrainerSpriteAnimationData = new global::System.Windows.Forms.Label();
			this.txtTrainerSpriteAnimationPointer = new global::System.Windows.Forms.TextBox();
			this.txtTrainerSpriteAnimationData = new global::System.Windows.Forms.TextBox();
			this.lblTrainerSpriteYPosition = new global::System.Windows.Forms.Label();
			this.btnSaveTrainerSprite = new global::System.Windows.Forms.Button();
			this.txtTrainerSpritePalAddress = new global::System.Windows.Forms.TextBox();
			this.txtTrainerSpriteImgAddress = new global::System.Windows.Forms.TextBox();
			this.nudTrainerSpriteID = new global::System.Windows.Forms.NumericUpDown();
			this.picTrainerSprite = new global::System.Windows.Forms.PictureBox();
			this.nudPrizeMoneyRate = new global::System.Windows.Forms.NumericUpDown();
			this.lblPrizeMoneyRate = new global::System.Windows.Forms.Label();
			this.btnSaveTrainerClass = new global::System.Windows.Forms.Button();
			this.btnChangeTrainerClassName = new global::System.Windows.Forms.Button();
			this.lblTrainerClassName = new global::System.Windows.Forms.Label();
			this.txtTrainerClassName = new global::System.Windows.Forms.TextBox();
			this.cmbTrainerClassName = new global::System.Windows.Forms.ComboBox();
			this.grpTrainerSprite = new global::System.Windows.Forms.GroupBox();
			this.btnChangeTrainerSpriteAddress = new global::System.Windows.Forms.Button();
			this.grpImportExport = new global::System.Windows.Forms.GroupBox();
			this.btnExportTrainerSprite = new global::System.Windows.Forms.Button();
			this.txtImportTrainerSpriteAddress = new global::System.Windows.Forms.TextBox();
			this.btnImportTrainerSprite = new global::System.Windows.Forms.Button();
			this.rbTrainerSpritePalAddress = new global::System.Windows.Forms.RadioButton();
			this.rbTrainerSpriteImgAddress = new global::System.Windows.Forms.RadioButton();
			this.grpTrainerSpritePreview = new global::System.Windows.Forms.GroupBox();
			this.grpTrainerClass = new global::System.Windows.Forms.GroupBox();
			((global::System.ComponentModel.ISupportInitialize)this.nudTrainerSpriteYPosition).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudTrainerSpriteID).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.picTrainerSprite).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudPrizeMoneyRate).BeginInit();
			this.grpTrainerSprite.SuspendLayout();
			this.grpImportExport.SuspendLayout();
			this.grpTrainerSpritePreview.SuspendLayout();
			this.grpTrainerClass.SuspendLayout();
			base.SuspendLayout();
			this.nudTrainerSpriteYPosition.Location = new global::System.Drawing.Point(178, 137);
			global::System.Windows.Forms.NumericUpDown nudTrainerSpriteYPosition = this.nudTrainerSpriteYPosition;
			int[] array = new int[4];
			array[0] = 255;
			nudTrainerSpriteYPosition.Maximum = new decimal(array);
			this.nudTrainerSpriteYPosition.Name = "nudTrainerSpriteYPosition";
			this.nudTrainerSpriteYPosition.Size = new global::System.Drawing.Size(80, 19);
			this.nudTrainerSpriteYPosition.TabIndex = 7;
			this.lblTrainerSpriteAnimationPointer.AutoSize = true;
			this.lblTrainerSpriteAnimationPointer.Location = new global::System.Drawing.Point(108, 168);
			this.lblTrainerSpriteAnimationPointer.Name = "lblTrainerSpriteAnimationPointer";
			this.lblTrainerSpriteAnimationPointer.Size = new global::System.Drawing.Size(108, 12);
			this.lblTrainerSpriteAnimationPointer.TabIndex = 6;
			this.lblTrainerSpriteAnimationPointer.Text = "アニメーションポインタ :";
			this.lblTrainerSpriteAnimationData.AutoSize = true;
			this.lblTrainerSpriteAnimationData.Location = new global::System.Drawing.Point(108, 193);
			this.lblTrainerSpriteAnimationData.Name = "lblTrainerSpriteAnimationData";
			this.lblTrainerSpriteAnimationData.Size = new global::System.Drawing.Size(100, 12);
			this.lblTrainerSpriteAnimationData.TabIndex = 6;
			this.lblTrainerSpriteAnimationData.Text = "アニメーションデータ :";
			this.txtTrainerSpriteAnimationPointer.Location = new global::System.Drawing.Point(230, 164);
			this.txtTrainerSpriteAnimationPointer.Name = "txtTrainerSpriteAnimationPointer";
			this.txtTrainerSpriteAnimationPointer.ReadOnly = true;
			this.txtTrainerSpriteAnimationPointer.Size = new global::System.Drawing.Size(80, 19);
			this.txtTrainerSpriteAnimationPointer.TabIndex = 5;
			this.txtTrainerSpriteAnimationData.Location = new global::System.Drawing.Point(230, 189);
			this.txtTrainerSpriteAnimationData.Name = "txtTrainerSpriteAnimationData";
			this.txtTrainerSpriteAnimationData.ReadOnly = true;
			this.txtTrainerSpriteAnimationData.Size = new global::System.Drawing.Size(80, 19);
			this.txtTrainerSpriteAnimationData.TabIndex = 5;
			this.lblTrainerSpriteYPosition.AutoSize = true;
			this.lblTrainerSpriteYPosition.Location = new global::System.Drawing.Point(126, 140);
			this.lblTrainerSpriteYPosition.Name = "lblTrainerSpriteYPosition";
			this.lblTrainerSpriteYPosition.Size = new global::System.Drawing.Size(30, 12);
			this.lblTrainerSpriteYPosition.TabIndex = 4;
			this.lblTrainerSpriteYPosition.Text = "Y軸 :";
			this.btnSaveTrainerSprite.Location = new global::System.Drawing.Point(12, 20);
			this.btnSaveTrainerSprite.Name = "btnSaveTrainerSprite";
			this.btnSaveTrainerSprite.Size = new global::System.Drawing.Size(92, 23);
			this.btnSaveTrainerSprite.TabIndex = 3;
			this.btnSaveTrainerSprite.Text = "変更を保存";
			this.btnSaveTrainerSprite.UseVisualStyleBackColor = true;
			this.txtTrainerSpritePalAddress.Location = new global::System.Drawing.Point(178, 80);
			this.txtTrainerSpritePalAddress.Name = "txtTrainerSpritePalAddress";
			this.txtTrainerSpritePalAddress.Size = new global::System.Drawing.Size(80, 19);
			this.txtTrainerSpritePalAddress.TabIndex = 2;
			this.txtTrainerSpriteImgAddress.Location = new global::System.Drawing.Point(178, 56);
			this.txtTrainerSpriteImgAddress.Name = "txtTrainerSpriteImgAddress";
			this.txtTrainerSpriteImgAddress.Size = new global::System.Drawing.Size(80, 19);
			this.txtTrainerSpriteImgAddress.TabIndex = 2;
			this.nudTrainerSpriteID.Location = new global::System.Drawing.Point(26, 138);
			global::System.Windows.Forms.NumericUpDown nudTrainerSpriteID = this.nudTrainerSpriteID;
			int[] array2 = new int[4];
			array2[0] = 255;
			nudTrainerSpriteID.Maximum = new decimal(array2);
			this.nudTrainerSpriteID.Name = "nudTrainerSpriteID";
			this.nudTrainerSpriteID.Size = new global::System.Drawing.Size(64, 19);
			this.nudTrainerSpriteID.TabIndex = 1;
			this.picTrainerSprite.Location = new global::System.Drawing.Point(8, 14);
			this.picTrainerSprite.Name = "picTrainerSprite";
			this.picTrainerSprite.Size = new global::System.Drawing.Size(64, 64);
			this.picTrainerSprite.TabIndex = 0;
			this.picTrainerSprite.TabStop = false;
			this.nudPrizeMoneyRate.Location = new global::System.Drawing.Point(152, 74);
			global::System.Windows.Forms.NumericUpDown nudPrizeMoneyRate = this.nudPrizeMoneyRate;
			int[] array3 = new int[4];
			array3[0] = 255;
			nudPrizeMoneyRate.Maximum = new decimal(array3);
			this.nudPrizeMoneyRate.Name = "nudPrizeMoneyRate";
			this.nudPrizeMoneyRate.Size = new global::System.Drawing.Size(80, 19);
			this.nudPrizeMoneyRate.TabIndex = 5;
			this.lblPrizeMoneyRate.AutoSize = true;
			this.lblPrizeMoneyRate.Location = new global::System.Drawing.Point(150, 54);
			this.lblPrizeMoneyRate.Name = "lblPrizeMoneyRate";
			this.lblPrizeMoneyRate.Size = new global::System.Drawing.Size(59, 12);
			this.lblPrizeMoneyRate.TabIndex = 4;
			this.lblPrizeMoneyRate.Text = "賞金倍率 :";
			this.btnSaveTrainerClass.Location = new global::System.Drawing.Point(12, 20);
			this.btnSaveTrainerClass.Name = "btnSaveTrainerClass";
			this.btnSaveTrainerClass.Size = new global::System.Drawing.Size(92, 23);
			this.btnSaveTrainerClass.TabIndex = 3;
			this.btnSaveTrainerClass.Text = "変更を保存";
			this.btnSaveTrainerClass.UseVisualStyleBackColor = true;
			this.btnChangeTrainerClassName.Location = new global::System.Drawing.Point(12, 125);
			this.btnChangeTrainerClassName.Name = "btnChangeTrainerClassName";
			this.btnChangeTrainerClassName.Size = new global::System.Drawing.Size(128, 23);
			this.btnChangeTrainerClassName.TabIndex = 3;
			this.btnChangeTrainerClassName.Text = "肩書き名を変更";
			this.btnChangeTrainerClassName.UseVisualStyleBackColor = true;
			this.lblTrainerClassName.AutoSize = true;
			this.lblTrainerClassName.Location = new global::System.Drawing.Point(12, 54);
			this.lblTrainerClassName.Name = "lblTrainerClassName";
			this.lblTrainerClassName.Size = new global::System.Drawing.Size(56, 12);
			this.lblTrainerClassName.TabIndex = 2;
			this.lblTrainerClassName.Text = "肩書き名 :";
			this.txtTrainerClassName.Location = new global::System.Drawing.Point(12, 100);
			this.txtTrainerClassName.Name = "txtTrainerClassName";
			this.txtTrainerClassName.Size = new global::System.Drawing.Size(128, 19);
			this.txtTrainerClassName.TabIndex = 1;
			this.cmbTrainerClassName.FormattingEnabled = true;
			this.cmbTrainerClassName.Location = new global::System.Drawing.Point(12, 74);
			this.cmbTrainerClassName.Name = "cmbTrainerClassName";
			this.cmbTrainerClassName.Size = new global::System.Drawing.Size(128, 20);
			this.cmbTrainerClassName.TabIndex = 0;
			this.grpTrainerSprite.Controls.Add(this.btnChangeTrainerSpriteAddress);
			this.grpTrainerSprite.Controls.Add(this.grpImportExport);
			this.grpTrainerSprite.Controls.Add(this.rbTrainerSpritePalAddress);
			this.grpTrainerSprite.Controls.Add(this.rbTrainerSpriteImgAddress);
			this.grpTrainerSprite.Controls.Add(this.grpTrainerSpritePreview);
			this.grpTrainerSprite.Controls.Add(this.nudTrainerSpriteID);
			this.grpTrainerSprite.Controls.Add(this.nudTrainerSpriteYPosition);
			this.grpTrainerSprite.Controls.Add(this.lblTrainerSpriteAnimationPointer);
			this.grpTrainerSprite.Controls.Add(this.btnSaveTrainerSprite);
			this.grpTrainerSprite.Controls.Add(this.txtTrainerSpritePalAddress);
			this.grpTrainerSprite.Controls.Add(this.lblTrainerSpriteAnimationData);
			this.grpTrainerSprite.Controls.Add(this.txtTrainerSpriteImgAddress);
			this.grpTrainerSprite.Controls.Add(this.lblTrainerSpriteYPosition);
			this.grpTrainerSprite.Controls.Add(this.txtTrainerSpriteAnimationPointer);
			this.grpTrainerSprite.Controls.Add(this.txtTrainerSpriteAnimationData);
			this.grpTrainerSprite.Location = new global::System.Drawing.Point(16, 16);
			this.grpTrainerSprite.Name = "grpTrainerSprite";
			this.grpTrainerSprite.Size = new global::System.Drawing.Size(498, 220);
			this.grpTrainerSprite.TabIndex = 8;
			this.grpTrainerSprite.TabStop = false;
			this.grpTrainerSprite.Text = "トレーナー画像";
			this.btnChangeTrainerSpriteAddress.Location = new global::System.Drawing.Point(128, 106);
			this.btnChangeTrainerSpriteAddress.Name = "btnChangeTrainerSpriteAddress";
			this.btnChangeTrainerSpriteAddress.Size = new global::System.Drawing.Size(130, 23);
			this.btnChangeTrainerSpriteAddress.TabIndex = 11;
			this.btnChangeTrainerSpriteAddress.Text = "アドレスの変更を反映";
			this.btnChangeTrainerSpriteAddress.UseVisualStyleBackColor = true;
			this.grpImportExport.Controls.Add(this.btnExportTrainerSprite);
			this.grpImportExport.Controls.Add(this.txtImportTrainerSpriteAddress);
			this.grpImportExport.Controls.Add(this.btnImportTrainerSprite);
			this.grpImportExport.Location = new global::System.Drawing.Point(274, 46);
			this.grpImportExport.Name = "grpImportExport";
			this.grpImportExport.Size = new global::System.Drawing.Size(204, 86);
			this.grpImportExport.TabIndex = 10;
			this.grpImportExport.TabStop = false;
			this.grpImportExport.Text = "インポート/エクスポート";
			this.btnExportTrainerSprite.Location = new global::System.Drawing.Point(108, 50);
			this.btnExportTrainerSprite.Name = "btnExportTrainerSprite";
			this.btnExportTrainerSprite.Size = new global::System.Drawing.Size(80, 23);
			this.btnExportTrainerSprite.TabIndex = 11;
			this.btnExportTrainerSprite.Text = "エクスポート";
			this.btnExportTrainerSprite.UseVisualStyleBackColor = true;
			this.txtImportTrainerSpriteAddress.Location = new global::System.Drawing.Point(16, 24);
			this.txtImportTrainerSpriteAddress.Name = "txtImportTrainerSpriteAddress";
			this.txtImportTrainerSpriteAddress.Size = new global::System.Drawing.Size(80, 19);
			this.txtImportTrainerSpriteAddress.TabIndex = 10;
			this.btnImportTrainerSprite.Location = new global::System.Drawing.Point(108, 22);
			this.btnImportTrainerSprite.Name = "btnImportTrainerSprite";
			this.btnImportTrainerSprite.Size = new global::System.Drawing.Size(80, 23);
			this.btnImportTrainerSprite.TabIndex = 11;
			this.btnImportTrainerSprite.Text = "インポート";
			this.btnImportTrainerSprite.UseVisualStyleBackColor = true;
			this.rbTrainerSpritePalAddress.AutoSize = true;
			this.rbTrainerSpritePalAddress.Location = new global::System.Drawing.Point(110, 82);
			this.rbTrainerSpritePalAddress.Name = "rbTrainerSpritePalAddress";
			this.rbTrainerSpritePalAddress.Size = new global::System.Drawing.Size(63, 16);
			this.rbTrainerSpritePalAddress.TabIndex = 9;
			this.rbTrainerSpritePalAddress.TabStop = true;
			this.rbTrainerSpritePalAddress.Text = "パレット :";
			this.rbTrainerSpritePalAddress.UseVisualStyleBackColor = true;
			this.rbTrainerSpriteImgAddress.AutoSize = true;
			this.rbTrainerSpriteImgAddress.Location = new global::System.Drawing.Point(110, 58);
			this.rbTrainerSpriteImgAddress.Name = "rbTrainerSpriteImgAddress";
			this.rbTrainerSpriteImgAddress.Size = new global::System.Drawing.Size(53, 16);
			this.rbTrainerSpriteImgAddress.TabIndex = 8;
			this.rbTrainerSpriteImgAddress.TabStop = true;
			this.rbTrainerSpriteImgAddress.Text = "画像 :";
			this.rbTrainerSpriteImgAddress.UseVisualStyleBackColor = true;
			this.grpTrainerSpritePreview.Controls.Add(this.picTrainerSprite);
			this.grpTrainerSpritePreview.Location = new global::System.Drawing.Point(18, 46);
			this.grpTrainerSpritePreview.Name = "grpTrainerSpritePreview";
			this.grpTrainerSpritePreview.Size = new global::System.Drawing.Size(80, 86);
			this.grpTrainerSpritePreview.TabIndex = 0;
			this.grpTrainerSpritePreview.TabStop = false;
			this.grpTrainerClass.Controls.Add(this.btnSaveTrainerClass);
			this.grpTrainerClass.Controls.Add(this.lblTrainerClassName);
			this.grpTrainerClass.Controls.Add(this.nudPrizeMoneyRate);
			this.grpTrainerClass.Controls.Add(this.cmbTrainerClassName);
			this.grpTrainerClass.Controls.Add(this.lblPrizeMoneyRate);
			this.grpTrainerClass.Controls.Add(this.txtTrainerClassName);
			this.grpTrainerClass.Controls.Add(this.btnChangeTrainerClassName);
			this.grpTrainerClass.Location = new global::System.Drawing.Point(16, 246);
			this.grpTrainerClass.Name = "grpTrainerClass";
			this.grpTrainerClass.Size = new global::System.Drawing.Size(244, 162);
			this.grpTrainerClass.TabIndex = 9;
			this.grpTrainerClass.TabStop = false;
			this.grpTrainerClass.Text = "肩書き";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(532, 423);
			base.Controls.Add(this.grpTrainerClass);
			base.Controls.Add(this.grpTrainerSprite);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "TrainerSpriteEditor";
			this.Text = "トレーナー画像/肩書き";
			((global::System.ComponentModel.ISupportInitialize)this.nudTrainerSpriteYPosition).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudTrainerSpriteID).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.picTrainerSprite).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nudPrizeMoneyRate).EndInit();
			this.grpTrainerSprite.ResumeLayout(false);
			this.grpTrainerSprite.PerformLayout();
			this.grpImportExport.ResumeLayout(false);
			this.grpImportExport.PerformLayout();
			this.grpTrainerSpritePreview.ResumeLayout(false);
			this.grpTrainerClass.ResumeLayout(false);
			this.grpTrainerClass.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x040006CB RID: 1739
				private Button _btnSaveTrainerSprite;
		private TextBox _txtTrainerSpritePalAddress;
		private NumericUpDown _nudTrainerSpriteID;
		private NumericUpDown _nudTrainerSpriteYPosition;
		private ComboBox _cmbTrainerClassName;
		private Button _btnChangeTrainerClassName;
		private NumericUpDown _nudPrizeMoneyRate;
		private Button _btnSaveTrainerClass;
		private Button _btnExportTrainerSprite;
		private Button _btnImportTrainerSprite;
		private Button _btnChangeTrainerSpriteAddress;
		private TextBox _txtTrainerSpriteImgAddress;
        private global::System.ComponentModel.IContainer components;
	}
}
