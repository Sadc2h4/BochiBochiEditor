namespace BochiBochiEditor
{
	partial class BlockPickerForm
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
			this.lblPickerHint = new System.Windows.Forms.Label();
			this.chkPickerShowTop = new System.Windows.Forms.CheckBox();
			this.pnlPickerBlocks = new System.Windows.Forms.Panel();
			this.pnlPickerButtons = new System.Windows.Forms.Panel();
			this.lblPickerSelected = new System.Windows.Forms.Label();
			this.btnPickerOK = new System.Windows.Forms.Button();
			this.btnPickerCancel = new System.Windows.Forms.Button();
			this.pnlPickerButtons.SuspendLayout();
			this.SuspendLayout();
			//
			// lblPickerHint
			//
			this.lblPickerHint.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblPickerHint.Location = new System.Drawing.Point(8, 8);
			this.lblPickerHint.Name = "lblPickerHint";
			this.lblPickerHint.Size = new System.Drawing.Size(344, 60);
			this.lblPickerHint.TabIndex = 0;
			this.lblPickerHint.Text = "クリックで選び、ダブルクリックか「決定」で確定します。";
			//
			// chkPickerShowTop
			//
			this.chkPickerShowTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.chkPickerShowTop.Location = new System.Drawing.Point(8, 48);
			this.chkPickerShowTop.Name = "chkPickerShowTop";
			this.chkPickerShowTop.Size = new System.Drawing.Size(344, 24);
			this.chkPickerShowTop.TabIndex = 1;
			this.chkPickerShowTop.Text = "上層も重ねて表示する";
			this.chkPickerShowTop.UseVisualStyleBackColor = true;
			this.chkPickerShowTop.CheckedChanged += new System.EventHandler(this.chkPickerShowTop_CheckedChanged);
			//
			// pnlPickerBlocks
			//
			this.pnlPickerBlocks.AutoScroll = true;
			this.pnlPickerBlocks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlPickerBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPickerBlocks.Location = new System.Drawing.Point(8, 72);
			this.pnlPickerBlocks.Name = "pnlPickerBlocks";
			this.pnlPickerBlocks.Size = new System.Drawing.Size(344, 436);
			this.pnlPickerBlocks.TabIndex = 2;
			this.pnlPickerBlocks.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPickerBlocks_Paint);
			this.pnlPickerBlocks.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlPickerBlocks_MouseClick);
			this.pnlPickerBlocks.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pnlPickerBlocks_MouseDoubleClick);
			this.pnlPickerBlocks.MouseEnter += new System.EventHandler(this.pnlPickerBlocks_MouseEnter);
			this.pnlPickerBlocks.MouseLeave += new System.EventHandler(this.pnlPickerBlocks_MouseLeave);
			this.pnlPickerBlocks.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlPickerBlocks_MouseMove);
			this.pnlPickerBlocks.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.pnlPickerBlocks_MouseWheel);
			this.pnlPickerBlocks.Scroll += new System.Windows.Forms.ScrollEventHandler(this.pnlPickerBlocks_Scroll);
			//
			// pnlPickerButtons
			//
			this.pnlPickerButtons.Controls.Add(this.lblPickerSelected);
			this.pnlPickerButtons.Controls.Add(this.btnPickerOK);
			this.pnlPickerButtons.Controls.Add(this.btnPickerCancel);
			this.pnlPickerButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlPickerButtons.Location = new System.Drawing.Point(8, 508);
			this.pnlPickerButtons.Name = "pnlPickerButtons";
			this.pnlPickerButtons.Size = new System.Drawing.Size(344, 44);
			this.pnlPickerButtons.TabIndex = 3;
			//
			// lblPickerSelected
			//
			this.lblPickerSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.lblPickerSelected.Location = new System.Drawing.Point(0, 8);
			this.lblPickerSelected.Name = "lblPickerSelected";
			this.lblPickerSelected.Size = new System.Drawing.Size(164, 32);
			this.lblPickerSelected.TabIndex = 0;
			this.lblPickerSelected.Text = "選択中: 0x000";
			this.lblPickerSelected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// btnPickerOK
			//
			this.btnPickerOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPickerOK.Location = new System.Drawing.Point(170, 10);
			this.btnPickerOK.Name = "btnPickerOK";
			this.btnPickerOK.Size = new System.Drawing.Size(84, 28);
			this.btnPickerOK.TabIndex = 1;
			this.btnPickerOK.Text = "決定";
			this.btnPickerOK.UseVisualStyleBackColor = true;
			this.btnPickerOK.Click += new System.EventHandler(this.btnPickerOK_Click);
			//
			// btnPickerCancel
			//
			this.btnPickerCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPickerCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnPickerCancel.Location = new System.Drawing.Point(260, 10);
			this.btnPickerCancel.Name = "btnPickerCancel";
			this.btnPickerCancel.Size = new System.Drawing.Size(84, 28);
			this.btnPickerCancel.TabIndex = 2;
			this.btnPickerCancel.Text = "キャンセル";
			this.btnPickerCancel.UseVisualStyleBackColor = true;
			//
			// BlockPickerForm
			//
			this.AcceptButton = this.btnPickerOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnPickerCancel;
			this.ClientSize = new System.Drawing.Size(360, 560);
			this.Controls.Add(this.pnlPickerBlocks);
			this.Controls.Add(this.chkPickerShowTop);
			this.Controls.Add(this.lblPickerHint);
			this.Controls.Add(this.pnlPickerButtons);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(280, 360);
			this.Name = "BlockPickerForm";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "下地ブロックを選ぶ";
			this.pnlPickerButtons.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Label lblPickerHint;
		private System.Windows.Forms.CheckBox chkPickerShowTop;
		private System.Windows.Forms.Panel pnlPickerBlocks;
		private System.Windows.Forms.Panel pnlPickerButtons;
		private System.Windows.Forms.Label lblPickerSelected;
		private System.Windows.Forms.Button btnPickerOK;
		private System.Windows.Forms.Button btnPickerCancel;
	}
}
