namespace BochiBochiEditor
{
	partial class RomTablesForm
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
			this.lblRomTablesSummary = new System.Windows.Forms.Label();
			this.pnlRomTablesOptions = new System.Windows.Forms.Panel();
			this.lblRomTablesLegend = new System.Windows.Forms.Label();
			this.chkRomTablesIssuesOnly = new System.Windows.Forms.CheckBox();
			this.dgvRomTables = new System.Windows.Forms.DataGridView();
			this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colUsage = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colEntrySize = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNotes = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.pnlRomTablesButtons = new System.Windows.Forms.Panel();
			this.btnRomTablesCopy = new System.Windows.Forms.Button();
			this.btnRomTablesClose = new System.Windows.Forms.Button();
			this.pnlRomTablesOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvRomTables)).BeginInit();
			this.pnlRomTablesButtons.SuspendLayout();
			this.SuspendLayout();
			//
			// lblRomTablesSummary
			//
			this.lblRomTablesSummary.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblRomTablesSummary.Location = new System.Drawing.Point(10, 10);
			this.lblRomTablesSummary.Name = "lblRomTablesSummary";
			this.lblRomTablesSummary.Size = new System.Drawing.Size(1080, 44);
			this.lblRomTablesSummary.TabIndex = 0;
			this.lblRomTablesSummary.Text = "開いている ROM の表の読み込み先";
			//
			// pnlRomTablesOptions
			//
			this.pnlRomTablesOptions.Controls.Add(this.lblRomTablesLegend);
			this.pnlRomTablesOptions.Controls.Add(this.chkRomTablesIssuesOnly);
			this.pnlRomTablesOptions.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlRomTablesOptions.Location = new System.Drawing.Point(10, 54);
			this.pnlRomTablesOptions.Name = "pnlRomTablesOptions";
			this.pnlRomTablesOptions.Size = new System.Drawing.Size(1080, 30);
			this.pnlRomTablesOptions.TabIndex = 1;
			//
			// lblRomTablesLegend
			//
			this.lblRomTablesLegend.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblRomTablesLegend.Location = new System.Drawing.Point(260, 0);
			this.lblRomTablesLegend.Name = "lblRomTablesLegend";
			this.lblRomTablesLegend.Size = new System.Drawing.Size(820, 30);
			this.lblRomTablesLegend.TabIndex = 1;
			this.lblRomTablesLegend.Text = "OK: 問題なし　／　変更あり: 元のゲームから場所や件数が変わっている（多くは改造による正常な変化）　／　要確認: 読み込みがずれている可能性　／　読めない: 場所が ROM の外";
			this.lblRomTablesLegend.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// chkRomTablesIssuesOnly
			//
			this.chkRomTablesIssuesOnly.Dock = System.Windows.Forms.DockStyle.Left;
			this.chkRomTablesIssuesOnly.Location = new System.Drawing.Point(0, 0);
			this.chkRomTablesIssuesOnly.Name = "chkRomTablesIssuesOnly";
			this.chkRomTablesIssuesOnly.Size = new System.Drawing.Size(260, 30);
			this.chkRomTablesIssuesOnly.TabIndex = 0;
			this.chkRomTablesIssuesOnly.Text = "変更・要確認がある表だけ表示";
			this.chkRomTablesIssuesOnly.UseVisualStyleBackColor = true;
			this.chkRomTablesIssuesOnly.CheckedChanged += new System.EventHandler(this.chkRomTablesIssuesOnly_CheckedChanged);
			//
			// dgvRomTables
			//
			this.dgvRomTables.AllowUserToAddRows = false;
			this.dgvRomTables.AllowUserToDeleteRows = false;
			this.dgvRomTables.AllowUserToResizeRows = false;
			this.dgvRomTables.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvRomTables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvRomTables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
				this.colStatus,
				this.colCategory,
				this.colName,
				this.colUsage,
				this.colAddress,
				this.colSource,
				this.colEntrySize,
				this.colCount,
				this.colNotes,
				this.colKey});
			this.dgvRomTables.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvRomTables.Location = new System.Drawing.Point(10, 84);
			this.dgvRomTables.Name = "dgvRomTables";
			this.dgvRomTables.ReadOnly = true;
			this.dgvRomTables.RowHeadersVisible = false;
			this.dgvRomTables.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvRomTables.Size = new System.Drawing.Size(1080, 520);
			this.dgvRomTables.TabIndex = 2;
			//
			// colStatus
			//
			this.colStatus.HeaderText = "状態";
			this.colStatus.Name = "colStatus";
			this.colStatus.ReadOnly = true;
			this.colStatus.Width = 70;
			//
			// colCategory
			//
			this.colCategory.HeaderText = "分類";
			this.colCategory.Name = "colCategory";
			this.colCategory.ReadOnly = true;
			this.colCategory.Width = 80;
			//
			// colName
			//
			this.colName.HeaderText = "表";
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			this.colName.Width = 170;
			//
			// colUsage
			//
			this.colUsage.HeaderText = "使いみち";
			this.colUsage.Name = "colUsage";
			this.colUsage.ReadOnly = true;
			this.colUsage.Width = 190;
			//
			// colAddress
			//
			this.colAddress.HeaderText = "場所";
			this.colAddress.Name = "colAddress";
			this.colAddress.ReadOnly = true;
			this.colAddress.Width = 80;
			//
			// colSource
			//
			this.colSource.HeaderText = "場所の決め方";
			this.colSource.Name = "colSource";
			this.colSource.ReadOnly = true;
			this.colSource.Width = 150;
			//
			// colEntrySize
			//
			this.colEntrySize.HeaderText = "1 項目";
			this.colEntrySize.Name = "colEntrySize";
			this.colEntrySize.ReadOnly = true;
			this.colEntrySize.Width = 70;
			//
			// colCount
			//
			this.colCount.HeaderText = "件数";
			this.colCount.Name = "colCount";
			this.colCount.ReadOnly = true;
			this.colCount.Width = 170;
			//
			// colNotes
			//
			this.colNotes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colNotes.HeaderText = "メモ";
			this.colNotes.MinimumWidth = 200;
			this.colNotes.Name = "colNotes";
			this.colNotes.ReadOnly = true;
			//
			// colKey
			//
			this.colKey.HeaderText = "ini の項目";
			this.colKey.Name = "colKey";
			this.colKey.ReadOnly = true;
			this.colKey.Width = 200;
			//
			// pnlRomTablesButtons
			//
			this.pnlRomTablesButtons.Controls.Add(this.btnRomTablesCopy);
			this.pnlRomTablesButtons.Controls.Add(this.btnRomTablesClose);
			this.pnlRomTablesButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlRomTablesButtons.Location = new System.Drawing.Point(10, 604);
			this.pnlRomTablesButtons.Name = "pnlRomTablesButtons";
			this.pnlRomTablesButtons.Size = new System.Drawing.Size(1080, 46);
			this.pnlRomTablesButtons.TabIndex = 3;
			//
			// btnRomTablesCopy
			//
			this.btnRomTablesCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRomTablesCopy.Location = new System.Drawing.Point(846, 10);
			this.btnRomTablesCopy.Name = "btnRomTablesCopy";
			this.btnRomTablesCopy.Size = new System.Drawing.Size(130, 30);
			this.btnRomTablesCopy.TabIndex = 0;
			this.btnRomTablesCopy.Text = "一覧をコピー";
			this.btnRomTablesCopy.UseVisualStyleBackColor = true;
			this.btnRomTablesCopy.Click += new System.EventHandler(this.btnRomTablesCopy_Click);
			//
			// btnRomTablesClose
			//
			this.btnRomTablesClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRomTablesClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnRomTablesClose.Location = new System.Drawing.Point(984, 10);
			this.btnRomTablesClose.Name = "btnRomTablesClose";
			this.btnRomTablesClose.Size = new System.Drawing.Size(96, 30);
			this.btnRomTablesClose.TabIndex = 1;
			this.btnRomTablesClose.Text = "閉じる";
			this.btnRomTablesClose.UseVisualStyleBackColor = true;
			this.btnRomTablesClose.Click += new System.EventHandler(this.btnRomTablesClose_Click);
			//
			// RomTablesForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnRomTablesClose;
			this.ClientSize = new System.Drawing.Size(1100, 660);
			this.Controls.Add(this.dgvRomTables);
			this.Controls.Add(this.pnlRomTablesOptions);
			this.Controls.Add(this.lblRomTablesSummary);
			this.Controls.Add(this.pnlRomTablesButtons);
			this.MinimumSize = new System.Drawing.Size(700, 400);
			this.Name = "RomTablesForm";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "開いている ROM のテーブル情報";
			this.pnlRomTablesOptions.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvRomTables)).EndInit();
			this.pnlRomTablesButtons.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Label lblRomTablesSummary;
		private System.Windows.Forms.Panel pnlRomTablesOptions;
		private System.Windows.Forms.Label lblRomTablesLegend;
		private System.Windows.Forms.CheckBox chkRomTablesIssuesOnly;
		private System.Windows.Forms.DataGridView dgvRomTables;
		private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
		private System.Windows.Forms.DataGridViewTextBoxColumn colName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colUsage;
		private System.Windows.Forms.DataGridViewTextBoxColumn colAddress;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn colEntrySize;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCount;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNotes;
		private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
		private System.Windows.Forms.Panel pnlRomTablesButtons;
		private System.Windows.Forms.Button btnRomTablesCopy;
		private System.Windows.Forms.Button btnRomTablesClose;
	}
}
