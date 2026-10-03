namespace BochiBochiEditor
{
	partial class ToolPartSelectForm
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
			this.lblToolPartDesc = new System.Windows.Forms.Label();
			this.lvwToolParts = new System.Windows.Forms.ListView();
			this.colToolPartName = new System.Windows.Forms.ColumnHeader();
			this.colToolPartDesc = new System.Windows.Forms.ColumnHeader();
			this.btnToolPartUp = new System.Windows.Forms.Button();
			this.btnToolPartDown = new System.Windows.Forms.Button();
			this.btnToolPartShowAll = new System.Windows.Forms.Button();
			this.btnToolPartReset = new System.Windows.Forms.Button();
			this.lblToolPartNote = new System.Windows.Forms.Label();
			this.btnToolPartOk = new System.Windows.Forms.Button();
			this.btnToolPartCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// lblToolPartDesc
			//
			this.lblToolPartDesc.Location = new System.Drawing.Point(12, 10);
			this.lblToolPartDesc.Name = "lblToolPartDesc";
			this.lblToolPartDesc.Size = new System.Drawing.Size(636, 36);
			this.lblToolPartDesc.TabIndex = 0;
			this.lblToolPartDesc.Text = "右のツール欄に出す機能を選びます。チェックを入れた機能が、上から順に並びます。\r\nブロック・移動エリア・イベントの一覧は、いつも出ます。";
			//
			// lvwToolParts
			//
			this.lvwToolParts.CheckBoxes = true;
			this.lvwToolParts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.colToolPartName,
			this.colToolPartDesc});
			this.lvwToolParts.FullRowSelect = true;
			this.lvwToolParts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.lvwToolParts.HideSelection = false;
			this.lvwToolParts.Location = new System.Drawing.Point(12, 52);
			this.lvwToolParts.MultiSelect = false;
			this.lvwToolParts.Name = "lvwToolParts";
			this.lvwToolParts.Size = new System.Drawing.Size(520, 190);
			this.lvwToolParts.TabIndex = 1;
			this.lvwToolParts.UseCompatibleStateImageBehavior = false;
			this.lvwToolParts.View = System.Windows.Forms.View.Details;
			this.lvwToolParts.SelectedIndexChanged += new System.EventHandler(this.lvwToolParts_SelectedIndexChanged);
			//
			// colToolPartName
			//
			this.colToolPartName.Width = 170;
			//
			// colToolPartDesc
			//
			this.colToolPartDesc.Width = 330;
			//
			// btnToolPartUp
			//
			this.btnToolPartUp.Location = new System.Drawing.Point(540, 52);
			this.btnToolPartUp.Name = "btnToolPartUp";
			this.btnToolPartUp.Size = new System.Drawing.Size(108, 28);
			this.btnToolPartUp.TabIndex = 2;
			this.btnToolPartUp.Text = "上へ";
			this.btnToolPartUp.UseVisualStyleBackColor = true;
			this.btnToolPartUp.Click += new System.EventHandler(this.btnToolPartUp_Click);
			//
			// btnToolPartDown
			//
			this.btnToolPartDown.Location = new System.Drawing.Point(540, 84);
			this.btnToolPartDown.Name = "btnToolPartDown";
			this.btnToolPartDown.Size = new System.Drawing.Size(108, 28);
			this.btnToolPartDown.TabIndex = 3;
			this.btnToolPartDown.Text = "下へ";
			this.btnToolPartDown.UseVisualStyleBackColor = true;
			this.btnToolPartDown.Click += new System.EventHandler(this.btnToolPartDown_Click);
			//
			// btnToolPartShowAll
			//
			this.btnToolPartShowAll.Location = new System.Drawing.Point(540, 130);
			this.btnToolPartShowAll.Name = "btnToolPartShowAll";
			this.btnToolPartShowAll.Size = new System.Drawing.Size(108, 28);
			this.btnToolPartShowAll.TabIndex = 4;
			this.btnToolPartShowAll.Text = "すべて表示";
			this.btnToolPartShowAll.UseVisualStyleBackColor = true;
			this.btnToolPartShowAll.Click += new System.EventHandler(this.btnToolPartShowAll_Click);
			//
			// btnToolPartReset
			//
			this.btnToolPartReset.Location = new System.Drawing.Point(540, 214);
			this.btnToolPartReset.Name = "btnToolPartReset";
			this.btnToolPartReset.Size = new System.Drawing.Size(108, 28);
			this.btnToolPartReset.TabIndex = 5;
			this.btnToolPartReset.Text = "初めの状態に戻す";
			this.btnToolPartReset.UseVisualStyleBackColor = true;
			this.btnToolPartReset.Click += new System.EventHandler(this.btnToolPartReset_Click);
			//
			// lblToolPartNote
			//
			this.lblToolPartNote.Location = new System.Drawing.Point(12, 250);
			this.lblToolPartNote.Name = "lblToolPartNote";
			this.lblToolPartNote.Size = new System.Drawing.Size(430, 36);
			this.lblToolPartNote.TabIndex = 6;
			this.lblToolPartNote.Text = "別ウィンドウに分けたい機能は、その機能の見出しをドラッグします。";
			//
			// btnToolPartOk
			//
			this.btnToolPartOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnToolPartOk.Location = new System.Drawing.Point(446, 254);
			this.btnToolPartOk.Name = "btnToolPartOk";
			this.btnToolPartOk.Size = new System.Drawing.Size(98, 28);
			this.btnToolPartOk.TabIndex = 7;
			this.btnToolPartOk.Text = "OK";
			this.btnToolPartOk.UseVisualStyleBackColor = true;
			//
			// btnToolPartCancel
			//
			this.btnToolPartCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnToolPartCancel.Location = new System.Drawing.Point(550, 254);
			this.btnToolPartCancel.Name = "btnToolPartCancel";
			this.btnToolPartCancel.Size = new System.Drawing.Size(98, 28);
			this.btnToolPartCancel.TabIndex = 8;
			this.btnToolPartCancel.Text = "キャンセル";
			this.btnToolPartCancel.UseVisualStyleBackColor = true;
			//
			// ToolPartSelectForm
			//
			this.AcceptButton = this.btnToolPartOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnToolPartCancel;
			this.ClientSize = new System.Drawing.Size(660, 294);
			this.Controls.Add(this.lblToolPartDesc);
			this.Controls.Add(this.lvwToolParts);
			this.Controls.Add(this.btnToolPartUp);
			this.Controls.Add(this.btnToolPartDown);
			this.Controls.Add(this.btnToolPartShowAll);
			this.Controls.Add(this.btnToolPartReset);
			this.Controls.Add(this.lblToolPartNote);
			this.Controls.Add(this.btnToolPartOk);
			this.Controls.Add(this.btnToolPartCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolPartSelectForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "表示機能の選択";
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Label lblToolPartDesc;
		private System.Windows.Forms.ListView lvwToolParts;
		private System.Windows.Forms.ColumnHeader colToolPartName;
		private System.Windows.Forms.ColumnHeader colToolPartDesc;
		private System.Windows.Forms.Button btnToolPartUp;
		private System.Windows.Forms.Button btnToolPartDown;
		private System.Windows.Forms.Button btnToolPartShowAll;
		private System.Windows.Forms.Button btnToolPartReset;
		private System.Windows.Forms.Label lblToolPartNote;
		private System.Windows.Forms.Button btnToolPartOk;
		private System.Windows.Forms.Button btnToolPartCancel;
	}
}
