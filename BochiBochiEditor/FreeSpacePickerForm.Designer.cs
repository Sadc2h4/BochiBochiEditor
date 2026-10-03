namespace BochiBochiEditor
{
	partial class FreeSpacePickerForm
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
			this.lblPickerSummary = new System.Windows.Forms.Label();
			this.dgvCandidates = new System.Windows.Forms.DataGridView();
			this.colNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colRunLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colRunRange = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colBytesBefore = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblPickerNote = new System.Windows.Forms.Label();
			this.btnPickerOk = new System.Windows.Forms.Button();
			this.btnPickerCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dgvCandidates)).BeginInit();
			this.SuspendLayout();
			//
			// lblPickerSummary
			//
			this.lblPickerSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.lblPickerSummary.Location = new System.Drawing.Point(10, 10);
			this.lblPickerSummary.Name = "lblPickerSummary";
			this.lblPickerSummary.Size = new System.Drawing.Size(620, 44);
			this.lblPickerSummary.TabIndex = 0;
			this.lblPickerSummary.Text = "書き込み先を選んでください。";
			//
			// dgvCandidates
			//
			this.dgvCandidates.AllowUserToAddRows = false;
			this.dgvCandidates.AllowUserToDeleteRows = false;
			this.dgvCandidates.AllowUserToResizeRows = false;
			this.dgvCandidates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvCandidates.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dgvCandidates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvCandidates.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
				this.colNo,
				this.colAddress,
				this.colRunLength,
				this.colRunRange,
				this.colBytesBefore});
			this.dgvCandidates.Location = new System.Drawing.Point(10, 58);
			this.dgvCandidates.MultiSelect = false;
			this.dgvCandidates.Name = "dgvCandidates";
			this.dgvCandidates.ReadOnly = true;
			this.dgvCandidates.RowHeadersVisible = false;
			this.dgvCandidates.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvCandidates.Size = new System.Drawing.Size(620, 300);
			this.dgvCandidates.TabIndex = 1;
			this.dgvCandidates.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCandidates_CellDoubleClick);
			this.dgvCandidates.SelectionChanged += new System.EventHandler(this.dgvCandidates_SelectionChanged);
			//
			// colNo
			//
			this.colNo.HeaderText = "番号";
			this.colNo.Name = "colNo";
			this.colNo.ReadOnly = true;
			this.colNo.Width = 50;
			//
			// colAddress
			//
			this.colAddress.HeaderText = "書き込み先";
			this.colAddress.Name = "colAddress";
			this.colAddress.ReadOnly = true;
			this.colAddress.Width = 100;
			//
			// colRunLength
			//
			this.colRunLength.HeaderText = "空きの大きさ（バイト）";
			this.colRunLength.Name = "colRunLength";
			this.colRunLength.ReadOnly = true;
			this.colRunLength.Width = 150;
			//
			// colRunRange
			//
			this.colRunRange.HeaderText = "空きの範囲";
			this.colRunRange.Name = "colRunRange";
			this.colRunRange.ReadOnly = true;
			this.colRunRange.Width = 150;
			//
			// colBytesBefore
			//
			this.colBytesBefore.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colBytesBefore.HeaderText = "空きの直前の 8 バイト";
			this.colBytesBefore.MinimumWidth = 150;
			this.colBytesBefore.Name = "colBytesBefore";
			this.colBytesBefore.ReadOnly = true;
			this.colBytesBefore.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			//
			// lblPickerNote
			//
			this.lblPickerNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblPickerNote.Location = new System.Drawing.Point(10, 364);
			this.lblPickerNote.Name = "lblPickerNote";
			this.lblPickerNote.Size = new System.Drawing.Size(620, 40);
			this.lblPickerNote.TabIndex = 2;
			this.lblPickerNote.Text = "0xFF が並んでいる場所を空きとみなしています。小さい空きは、画像などのデータの一部のことがあります。迷ったら大きい空きを選んでください。";
			//
			// btnPickerOk
			//
			this.btnPickerOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPickerOk.Location = new System.Drawing.Point(390, 410);
			this.btnPickerOk.Name = "btnPickerOk";
			this.btnPickerOk.Size = new System.Drawing.Size(130, 30);
			this.btnPickerOk.TabIndex = 3;
			this.btnPickerOk.Text = "この場所に書く";
			this.btnPickerOk.UseVisualStyleBackColor = true;
			this.btnPickerOk.Click += new System.EventHandler(this.btnPickerOk_Click);
			//
			// btnPickerCancel
			//
			this.btnPickerCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPickerCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnPickerCancel.Location = new System.Drawing.Point(530, 410);
			this.btnPickerCancel.Name = "btnPickerCancel";
			this.btnPickerCancel.Size = new System.Drawing.Size(100, 30);
			this.btnPickerCancel.TabIndex = 4;
			this.btnPickerCancel.Text = "キャンセル";
			this.btnPickerCancel.UseVisualStyleBackColor = true;
			//
			// FreeSpacePickerForm
			//
			this.AcceptButton = this.btnPickerOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnPickerCancel;
			this.ClientSize = new System.Drawing.Size(640, 450);
			this.Controls.Add(this.lblPickerSummary);
			this.Controls.Add(this.dgvCandidates);
			this.Controls.Add(this.lblPickerNote);
			this.Controls.Add(this.btnPickerOk);
			this.Controls.Add(this.btnPickerCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(560, 360);
			this.Name = "FreeSpacePickerForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "書き込み先の選択";
			((System.ComponentModel.ISupportInitialize)(this.dgvCandidates)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Label lblPickerSummary;
		private System.Windows.Forms.DataGridView dgvCandidates;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNo;
		private System.Windows.Forms.DataGridViewTextBoxColumn colAddress;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRunLength;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRunRange;
		private System.Windows.Forms.DataGridViewTextBoxColumn colBytesBefore;
		private System.Windows.Forms.Label lblPickerNote;
		private System.Windows.Forms.Button btnPickerOk;
		private System.Windows.Forms.Button btnPickerCancel;
	}
}
