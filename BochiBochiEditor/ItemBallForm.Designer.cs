namespace BochiBochiEditor
{
	partial class ItemBallForm
	{
		private System.ComponentModel.IContainer components = null;

		//-------------------------------------------------------------------------------
		// 使用中のリソースを破棄する処理
		//-------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		//-------------------------------------------------------------------------------
		// デザイナーで配置した部品を初期化する処理
		//-------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.lblItemBallDesc = new System.Windows.Forms.Label();
			this.lblItemBallItem = new System.Windows.Forms.Label();
			this.cmbItemBallItem = new System.Windows.Forms.ComboBox();
			this.lblItemBallFlag = new System.Windows.Forms.Label();
			this.txtItemBallFlag = new System.Windows.Forms.TextBox();
			this.lblItemBallFlagNote = new System.Windows.Forms.Label();
			this.btnItemBallOk = new System.Windows.Forms.Button();
			this.btnItemBallCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// lblItemBallDesc
			//
			this.lblItemBallDesc.Location = new System.Drawing.Point(12, 10);
			this.lblItemBallDesc.Name = "lblItemBallDesc";
			this.lblItemBallDesc.Size = new System.Drawing.Size(436, 52);
			this.lblItemBallDesc.TabIndex = 0;
			this.lblItemBallDesc.Text = "拾えるアイテム（モンスターボールの形の落とし物）を置きます。\r\nアイテムを渡すスクリプトは、「編集中のMAPを確定」のときに自動で用意します。";
			//
			// lblItemBallItem
			//
			this.lblItemBallItem.AutoSize = true;
			this.lblItemBallItem.Location = new System.Drawing.Point(12, 72);
			this.lblItemBallItem.Name = "lblItemBallItem";
			this.lblItemBallItem.Size = new System.Drawing.Size(60, 15);
			this.lblItemBallItem.TabIndex = 1;
			this.lblItemBallItem.Text = "アイテム :";
			//
			// cmbItemBallItem
			//
			this.cmbItemBallItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbItemBallItem.FormattingEnabled = true;
			this.cmbItemBallItem.Location = new System.Drawing.Point(110, 68);
			this.cmbItemBallItem.MaxDropDownItems = 20;
			this.cmbItemBallItem.Name = "cmbItemBallItem";
			this.cmbItemBallItem.Size = new System.Drawing.Size(338, 23);
			this.cmbItemBallItem.TabIndex = 2;
			this.cmbItemBallItem.SelectedIndexChanged += new System.EventHandler(this.ItemBallInput_Changed);
			//
			// lblItemBallFlag
			//
			this.lblItemBallFlag.AutoSize = true;
			this.lblItemBallFlag.Location = new System.Drawing.Point(12, 106);
			this.lblItemBallFlag.Name = "lblItemBallFlag";
			this.lblItemBallFlag.Size = new System.Drawing.Size(92, 15);
			this.lblItemBallFlag.TabIndex = 3;
			this.lblItemBallFlag.Text = "フラグ（16 進）:";
			//
			// txtItemBallFlag
			//
			this.txtItemBallFlag.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.txtItemBallFlag.Location = new System.Drawing.Point(110, 102);
			this.txtItemBallFlag.MaxLength = 4;
			this.txtItemBallFlag.Name = "txtItemBallFlag";
			this.txtItemBallFlag.Size = new System.Drawing.Size(80, 23);
			this.txtItemBallFlag.TabIndex = 4;
			this.txtItemBallFlag.TextChanged += new System.EventHandler(this.ItemBallInput_Changed);
			//
			// lblItemBallFlagNote
			//
			this.lblItemBallFlagNote.Location = new System.Drawing.Point(12, 134);
			this.lblItemBallFlagNote.Name = "lblItemBallFlagNote";
			this.lblItemBallFlagNote.Size = new System.Drawing.Size(436, 84);
			this.lblItemBallFlagNote.TabIndex = 5;
			//
			// btnItemBallOk
			//
			this.btnItemBallOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnItemBallOk.Location = new System.Drawing.Point(246, 226);
			this.btnItemBallOk.Name = "btnItemBallOk";
			this.btnItemBallOk.Size = new System.Drawing.Size(98, 28);
			this.btnItemBallOk.TabIndex = 6;
			this.btnItemBallOk.Text = "追加";
			this.btnItemBallOk.UseVisualStyleBackColor = true;
			//
			// btnItemBallCancel
			//
			this.btnItemBallCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnItemBallCancel.Location = new System.Drawing.Point(350, 226);
			this.btnItemBallCancel.Name = "btnItemBallCancel";
			this.btnItemBallCancel.Size = new System.Drawing.Size(98, 28);
			this.btnItemBallCancel.TabIndex = 7;
			this.btnItemBallCancel.Text = "キャンセル";
			this.btnItemBallCancel.UseVisualStyleBackColor = true;
			//
			// ItemBallForm
			//
			this.AcceptButton = this.btnItemBallOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnItemBallCancel;
			this.ClientSize = new System.Drawing.Size(460, 266);
			this.Controls.Add(this.lblItemBallDesc);
			this.Controls.Add(this.lblItemBallItem);
			this.Controls.Add(this.cmbItemBallItem);
			this.Controls.Add(this.lblItemBallFlag);
			this.Controls.Add(this.txtItemBallFlag);
			this.Controls.Add(this.lblItemBallFlagNote);
			this.Controls.Add(this.btnItemBallOk);
			this.Controls.Add(this.btnItemBallCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ItemBallForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "アイテム（落とし物）を追加";
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblItemBallDesc;
		private System.Windows.Forms.Label lblItemBallItem;
		private System.Windows.Forms.ComboBox cmbItemBallItem;
		private System.Windows.Forms.Label lblItemBallFlag;
		private System.Windows.Forms.TextBox txtItemBallFlag;
		private System.Windows.Forms.Label lblItemBallFlagNote;
		private System.Windows.Forms.Button btnItemBallOk;
		private System.Windows.Forms.Button btnItemBallCancel;
	}
}
