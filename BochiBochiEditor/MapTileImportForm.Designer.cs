namespace BochiBochiEditor
{
	partial class MapTileImportForm
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
			if (disposing)
			{
				this.DisposeSheets();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		//-------------------------------------------------------------------------------
		// デザイナーで配置した部品を初期化する処理
		//-------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.lblFolder = new System.Windows.Forms.Label();
			this.txtFolder = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.lblSummary = new System.Windows.Forms.Label();
			this.grpMode = new System.Windows.Forms.GroupBox();
			this.rbModeAll = new System.Windows.Forms.RadioButton();
			this.rbModePart = new System.Windows.Forms.RadioButton();
			this.grpAll = new System.Windows.Forms.GroupBox();
			this.chkPrimary = new System.Windows.Forms.CheckBox();
			this.chkSecondary = new System.Windows.Forms.CheckBox();
			this.lblAllNote = new System.Windows.Forms.Label();
			this.grpPart = new System.Windows.Forms.GroupBox();
			this.rbPartPrimary = new System.Windows.Forms.RadioButton();
			this.rbPartSecondary = new System.Windows.Forms.RadioButton();
			this.pnlSheetHost = new System.Windows.Forms.Panel();
			this.pnlSheet = new System.Windows.Forms.Panel();
			this.lblSelection = new System.Windows.Forms.Label();
			this.lblPartNote = new System.Windows.Forms.Label();
			this.btnRun = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.grpMode.SuspendLayout();
			this.grpAll.SuspendLayout();
			this.grpPart.SuspendLayout();
			this.pnlSheetHost.SuspendLayout();
			this.SuspendLayout();
			//
			// lblFolder
			//
			this.lblFolder.AutoSize = true;
			this.lblFolder.Location = new System.Drawing.Point(12, 15);
			this.lblFolder.Name = "lblFolder";
			this.lblFolder.Size = new System.Drawing.Size(115, 12);
			this.lblFolder.TabIndex = 0;
			this.lblFolder.Text = "書き出したフォルダ :";
			//
			// txtFolder
			//
			this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFolder.Location = new System.Drawing.Point(130, 12);
			this.txtFolder.Name = "txtFolder";
			this.txtFolder.ReadOnly = true;
			this.txtFolder.Size = new System.Drawing.Size(410, 19);
			this.txtFolder.TabIndex = 1;
			//
			// btnBrowse
			//
			this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowse.Location = new System.Drawing.Point(548, 10);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(80, 24);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "選ぶ…";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			//
			// lblSummary
			//
			this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSummary.Location = new System.Drawing.Point(12, 40);
			this.lblSummary.Name = "lblSummary";
			this.lblSummary.Size = new System.Drawing.Size(616, 54);
			this.lblSummary.TabIndex = 3;
			this.lblSummary.Text = "「マップタイルをエクスポート」で作ったフォルダを選んでください。";
			//
			// grpMode
			//
			this.grpMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.grpMode.Controls.Add(this.rbModeAll);
			this.grpMode.Controls.Add(this.rbModePart);
			this.grpMode.Location = new System.Drawing.Point(12, 98);
			this.grpMode.Name = "grpMode";
			this.grpMode.Size = new System.Drawing.Size(616, 70);
			this.grpMode.TabIndex = 4;
			this.grpMode.TabStop = false;
			this.grpMode.Text = "取り込み方";
			//
			// rbModeAll
			//
			this.rbModeAll.AutoSize = true;
			this.rbModeAll.Checked = true;
			this.rbModeAll.Location = new System.Drawing.Point(14, 20);
			this.rbModeAll.Name = "rbModeAll";
			this.rbModeAll.Size = new System.Drawing.Size(400, 16);
			this.rbModeAll.TabIndex = 0;
			this.rbModeAll.TabStop = true;
			this.rbModeAll.Text = "まとめて取り込む（タイルセットを丸ごと、新しいタイルセットとして作る）";
			this.rbModeAll.UseVisualStyleBackColor = true;
			this.rbModeAll.CheckedChanged += new System.EventHandler(this.rbMode_CheckedChanged);
			//
			// rbModePart
			//
			this.rbModePart.AutoSize = true;
			this.rbModePart.Location = new System.Drawing.Point(14, 44);
			this.rbModePart.Name = "rbModePart";
			this.rbModePart.Size = new System.Drawing.Size(400, 16);
			this.rbModePart.TabIndex = 1;
			this.rbModePart.Text = "一部だけ取り込む（ブロックを選び、今のマップのタイルセットへマップチップ取り込み）";
			this.rbModePart.UseVisualStyleBackColor = true;
			this.rbModePart.CheckedChanged += new System.EventHandler(this.rbMode_CheckedChanged);
			//
			// grpAll
			//
			this.grpAll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.grpAll.Controls.Add(this.chkPrimary);
			this.grpAll.Controls.Add(this.chkSecondary);
			this.grpAll.Controls.Add(this.lblAllNote);
			this.grpAll.Location = new System.Drawing.Point(12, 174);
			this.grpAll.Name = "grpAll";
			this.grpAll.Size = new System.Drawing.Size(616, 316);
			this.grpAll.TabIndex = 5;
			this.grpAll.TabStop = false;
			this.grpAll.Text = "まとめて取り込むタイルセット";
			//
			// chkPrimary
			//
			this.chkPrimary.AutoSize = true;
			this.chkPrimary.Location = new System.Drawing.Point(14, 22);
			this.chkPrimary.Name = "chkPrimary";
			this.chkPrimary.Size = new System.Drawing.Size(300, 16);
			this.chkPrimary.TabIndex = 0;
			this.chkPrimary.Text = "タイルセット1（多くのマップで共通に使うもの）";
			this.chkPrimary.UseVisualStyleBackColor = true;
			this.chkPrimary.CheckedChanged += new System.EventHandler(this.chkTileset_CheckedChanged);
			//
			// chkSecondary
			//
			this.chkSecondary.AutoSize = true;
			this.chkSecondary.Checked = true;
			this.chkSecondary.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkSecondary.Location = new System.Drawing.Point(14, 46);
			this.chkSecondary.Name = "chkSecondary";
			this.chkSecondary.Size = new System.Drawing.Size(300, 16);
			this.chkSecondary.TabIndex = 1;
			this.chkSecondary.Text = "タイルセット2（その町・道路などに固有のもの）";
			this.chkSecondary.UseVisualStyleBackColor = true;
			this.chkSecondary.CheckedChanged += new System.EventHandler(this.chkTileset_CheckedChanged);
			//
			// lblAllNote
			//
			this.lblAllNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAllNote.Location = new System.Drawing.Point(14, 72);
			this.lblAllNote.Name = "lblAllNote";
			this.lblAllNote.Size = new System.Drawing.Size(588, 150);
			this.lblAllNote.TabIndex = 2;
			this.lblAllNote.Text = "説明";
			//
			// grpPart
			//
			this.grpPart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.grpPart.Controls.Add(this.rbPartPrimary);
			this.grpPart.Controls.Add(this.rbPartSecondary);
			this.grpPart.Controls.Add(this.pnlSheetHost);
			this.grpPart.Controls.Add(this.lblSelection);
			this.grpPart.Controls.Add(this.lblPartNote);
			this.grpPart.Location = new System.Drawing.Point(12, 174);
			this.grpPart.Name = "grpPart";
			this.grpPart.Size = new System.Drawing.Size(616, 316);
			this.grpPart.TabIndex = 6;
			this.grpPart.TabStop = false;
			this.grpPart.Text = "取り込むブロックを選ぶ（ドラッグで範囲を選択）";
			this.grpPart.Visible = false;
			//
			// rbPartPrimary
			//
			this.rbPartPrimary.AutoSize = true;
			this.rbPartPrimary.Location = new System.Drawing.Point(14, 20);
			this.rbPartPrimary.Name = "rbPartPrimary";
			this.rbPartPrimary.Size = new System.Drawing.Size(150, 16);
			this.rbPartPrimary.TabIndex = 0;
			this.rbPartPrimary.Text = "タイルセット1 のブロック";
			this.rbPartPrimary.UseVisualStyleBackColor = true;
			this.rbPartPrimary.CheckedChanged += new System.EventHandler(this.rbPartSheet_CheckedChanged);
			//
			// rbPartSecondary
			//
			this.rbPartSecondary.AutoSize = true;
			this.rbPartSecondary.Checked = true;
			this.rbPartSecondary.Location = new System.Drawing.Point(190, 20);
			this.rbPartSecondary.Name = "rbPartSecondary";
			this.rbPartSecondary.Size = new System.Drawing.Size(150, 16);
			this.rbPartSecondary.TabIndex = 1;
			this.rbPartSecondary.TabStop = true;
			this.rbPartSecondary.Text = "タイルセット2 のブロック";
			this.rbPartSecondary.UseVisualStyleBackColor = true;
			this.rbPartSecondary.CheckedChanged += new System.EventHandler(this.rbPartSheet_CheckedChanged);
			//
			// pnlSheetHost
			//
			this.pnlSheetHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlSheetHost.AutoScroll = true;
			this.pnlSheetHost.Controls.Add(this.pnlSheet);
			this.pnlSheetHost.Location = new System.Drawing.Point(14, 42);
			this.pnlSheetHost.Name = "pnlSheetHost";
			this.pnlSheetHost.Size = new System.Drawing.Size(300, 264);
			this.pnlSheetHost.TabIndex = 2;
			//
			// pnlSheet
			//
			this.pnlSheet.Location = new System.Drawing.Point(0, 0);
			this.pnlSheet.Name = "pnlSheet";
			this.pnlSheet.Size = new System.Drawing.Size(256, 256);
			this.pnlSheet.TabIndex = 0;
			this.pnlSheet.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSheet_Paint);
			this.pnlSheet.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlSheet_MouseDown);
			this.pnlSheet.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlSheet_MouseMove);
			this.pnlSheet.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlSheet_MouseUp);
			//
			// lblSelection
			//
			this.lblSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSelection.Location = new System.Drawing.Point(326, 44);
			this.lblSelection.Name = "lblSelection";
			this.lblSelection.Size = new System.Drawing.Size(276, 40);
			this.lblSelection.TabIndex = 3;
			this.lblSelection.Text = "選んでいるブロック : なし";
			//
			// lblPartNote
			//
			this.lblPartNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblPartNote.Location = new System.Drawing.Point(326, 90);
			this.lblPartNote.Name = "lblPartNote";
			this.lblPartNote.Size = new System.Drawing.Size(276, 200);
			this.lblPartNote.TabIndex = 4;
			this.lblPartNote.Text = "説明";
			//
			// btnRun
			//
			this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRun.Location = new System.Drawing.Point(338, 500);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(190, 30);
			this.btnRun.TabIndex = 7;
			this.btnRun.Text = "取り込む";
			this.btnRun.UseVisualStyleBackColor = true;
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			//
			// btnClose
			//
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = new System.Drawing.Point(538, 500);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(90, 30);
			this.btnClose.TabIndex = 8;
			this.btnClose.Text = "閉じる";
			this.btnClose.UseVisualStyleBackColor = true;
			//
			// MapTileImportForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(640, 540);
			this.Controls.Add(this.lblFolder);
			this.Controls.Add(this.txtFolder);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.lblSummary);
			this.Controls.Add(this.grpMode);
			this.Controls.Add(this.grpAll);
			this.Controls.Add(this.grpPart);
			this.Controls.Add(this.btnRun);
			this.Controls.Add(this.btnClose);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 500);
			this.Name = "MapTileImportForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "マップタイルの取り込み";
			this.grpMode.ResumeLayout(false);
			this.grpMode.PerformLayout();
			this.grpAll.ResumeLayout(false);
			this.grpAll.PerformLayout();
			this.grpPart.ResumeLayout(false);
			this.grpPart.PerformLayout();
			this.pnlSheetHost.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblFolder;
		private System.Windows.Forms.TextBox txtFolder;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label lblSummary;
		private System.Windows.Forms.GroupBox grpMode;
		private System.Windows.Forms.RadioButton rbModeAll;
		private System.Windows.Forms.RadioButton rbModePart;
		private System.Windows.Forms.GroupBox grpAll;
		private System.Windows.Forms.CheckBox chkPrimary;
		private System.Windows.Forms.CheckBox chkSecondary;
		private System.Windows.Forms.Label lblAllNote;
		private System.Windows.Forms.GroupBox grpPart;
		private System.Windows.Forms.RadioButton rbPartPrimary;
		private System.Windows.Forms.RadioButton rbPartSecondary;
		private System.Windows.Forms.Panel pnlSheetHost;
		private System.Windows.Forms.Panel pnlSheet;
		private System.Windows.Forms.Label lblSelection;
		private System.Windows.Forms.Label lblPartNote;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.Button btnClose;
	}
}
