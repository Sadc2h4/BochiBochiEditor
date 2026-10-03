namespace BochiBochiEditor
{
	partial class AddMapBankForm
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
			this.lblAddMapDesc = new System.Windows.Forms.Label();
			this.lblAddMapBank = new System.Windows.Forms.Label();
			this.cmbAddMapBank = new System.Windows.Forms.ComboBox();
			this.btnAddMapOk = new System.Windows.Forms.Button();
			this.btnAddMapCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// lblAddMapDesc
			//
			this.lblAddMapDesc.Location = new System.Drawing.Point(12, 12);
			this.lblAddMapDesc.Name = "lblAddMapDesc";
			this.lblAddMapDesc.Size = new System.Drawing.Size(376, 70);
			this.lblAddMapDesc.TabIndex = 0;
			this.lblAddMapDesc.Text = "選んだバンクの最後に、新しいマップを 1 つ追加します（一覧のいちばん下で、新しいバンクを作ることもできます）。今左の一覧で選んでいるマップをもとにします（見出しの設定・地形データの形・タイルセットを写し、イベントとマップスクリプトは空、接続は無し）。";
			//
			// lblAddMapBank
			//
			this.lblAddMapBank.AutoSize = true;
			this.lblAddMapBank.Location = new System.Drawing.Point(12, 88);
			this.lblAddMapBank.Name = "lblAddMapBank";
			this.lblAddMapBank.Size = new System.Drawing.Size(100, 12);
			this.lblAddMapBank.TabIndex = 1;
			this.lblAddMapBank.Text = "追加するバンク :";
			//
			// cmbAddMapBank
			//
			this.cmbAddMapBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAddMapBank.FormattingEnabled = true;
			this.cmbAddMapBank.Location = new System.Drawing.Point(130, 85);
			this.cmbAddMapBank.Name = "cmbAddMapBank";
			this.cmbAddMapBank.Size = new System.Drawing.Size(200, 20);
			this.cmbAddMapBank.TabIndex = 2;
			//
			// btnAddMapOk
			//
			this.btnAddMapOk.Location = new System.Drawing.Point(178, 122);
			this.btnAddMapOk.Name = "btnAddMapOk";
			this.btnAddMapOk.Size = new System.Drawing.Size(110, 30);
			this.btnAddMapOk.TabIndex = 3;
			this.btnAddMapOk.Text = "追加する";
			this.btnAddMapOk.UseVisualStyleBackColor = true;
			this.btnAddMapOk.Click += new System.EventHandler(this.btnAddMapOk_Click);
			//
			// btnAddMapCancel
			//
			this.btnAddMapCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnAddMapCancel.Location = new System.Drawing.Point(298, 122);
			this.btnAddMapCancel.Name = "btnAddMapCancel";
			this.btnAddMapCancel.Size = new System.Drawing.Size(90, 30);
			this.btnAddMapCancel.TabIndex = 4;
			this.btnAddMapCancel.Text = "キャンセル";
			this.btnAddMapCancel.UseVisualStyleBackColor = true;
			//
			// AddMapBankForm
			//
			this.AcceptButton = this.btnAddMapOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnAddMapCancel;
			this.ClientSize = new System.Drawing.Size(400, 164);
			this.Controls.Add(this.lblAddMapDesc);
			this.Controls.Add(this.lblAddMapBank);
			this.Controls.Add(this.cmbAddMapBank);
			this.Controls.Add(this.btnAddMapOk);
			this.Controls.Add(this.btnAddMapCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddMapBankForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "マップを追加";
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblAddMapDesc;
		private System.Windows.Forms.Label lblAddMapBank;
		private System.Windows.Forms.ComboBox cmbAddMapBank;
		private System.Windows.Forms.Button btnAddMapOk;
		private System.Windows.Forms.Button btnAddMapCancel;
	}
}
