namespace BochiBochiEditor
{
	partial class MapResizeForm
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
			this.lblResizeDesc = new System.Windows.Forms.Label();
			this.lblResizeCurrent = new System.Windows.Forms.Label();
			this.lblResizeWidth = new System.Windows.Forms.Label();
			this.nudResizeWidth = new System.Windows.Forms.NumericUpDown();
			this.lblResizeHeight = new System.Windows.Forms.Label();
			this.nudResizeHeight = new System.Windows.Forms.NumericUpDown();
			this.lblResizeShiftX = new System.Windows.Forms.Label();
			this.nudResizeShiftX = new System.Windows.Forms.NumericUpDown();
			this.lblResizeShiftY = new System.Windows.Forms.Label();
			this.nudResizeShiftY = new System.Windows.Forms.NumericUpDown();
			this.lblResizeBorderWidth = new System.Windows.Forms.Label();
			this.nudResizeBorderWidth = new System.Windows.Forms.NumericUpDown();
			this.lblResizeBorderHeight = new System.Windows.Forms.Label();
			this.nudResizeBorderHeight = new System.Windows.Forms.NumericUpDown();
			this.lblResizeFill = new System.Windows.Forms.Label();
			this.nudResizeFillBlock = new System.Windows.Forms.NumericUpDown();
			this.picResizeFillBlock = new System.Windows.Forms.PictureBox();
			this.lblResizeFillNote = new System.Windows.Forms.Label();
			this.lblResizeInfo = new System.Windows.Forms.Label();
			this.btnResizeOk = new System.Windows.Forms.Button();
			this.btnResizeCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeShiftX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeShiftY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeBorderWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeBorderHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeFillBlock)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picResizeFillBlock)).BeginInit();
			this.SuspendLayout();
			//
			// lblResizeDesc
			//
			this.lblResizeDesc.Location = new System.Drawing.Point(12, 12);
			this.lblResizeDesc.Name = "lblResizeDesc";
			this.lblResizeDesc.Size = new System.Drawing.Size(436, 66);
			this.lblResizeDesc.TabIndex = 0;
			this.lblResizeDesc.Text = "マップの大きさを変えます。今の内容は左上をそろえて残し（左側・上側に足すと、その分だけ右・下へずれます）、広がった部分は下で選んだブロックで埋めます。小さくすると、はみ出した部分は無くなります。新しい並びは空き領域に作り、「編集中のMAPを確定」を押すまで元のマップは変わりません。";
			//
			// lblResizeCurrent
			//
			this.lblResizeCurrent.AutoSize = true;
			this.lblResizeCurrent.Location = new System.Drawing.Point(12, 86);
			this.lblResizeCurrent.Name = "lblResizeCurrent";
			this.lblResizeCurrent.Size = new System.Drawing.Size(120, 12);
			this.lblResizeCurrent.TabIndex = 1;
			this.lblResizeCurrent.Text = "今の大きさ : -";
			//
			// lblResizeWidth
			//
			this.lblResizeWidth.AutoSize = true;
			this.lblResizeWidth.Location = new System.Drawing.Point(12, 116);
			this.lblResizeWidth.Name = "lblResizeWidth";
			this.lblResizeWidth.Size = new System.Drawing.Size(60, 12);
			this.lblResizeWidth.TabIndex = 2;
			this.lblResizeWidth.Text = "新しい幅 :";
			//
			// nudResizeWidth
			//
			this.nudResizeWidth.Location = new System.Drawing.Point(140, 113);
			this.nudResizeWidth.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			this.nudResizeWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeWidth.Name = "nudResizeWidth";
			this.nudResizeWidth.Size = new System.Drawing.Size(70, 19);
			this.nudResizeWidth.TabIndex = 3;
			this.nudResizeWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeWidth.ValueChanged += new System.EventHandler(this.ResizeValue_Changed);
			//
			// lblResizeHeight
			//
			this.lblResizeHeight.AutoSize = true;
			this.lblResizeHeight.Location = new System.Drawing.Point(240, 116);
			this.lblResizeHeight.Name = "lblResizeHeight";
			this.lblResizeHeight.Size = new System.Drawing.Size(70, 12);
			this.lblResizeHeight.TabIndex = 4;
			this.lblResizeHeight.Text = "新しい高さ :";
			//
			// nudResizeHeight
			//
			this.nudResizeHeight.Location = new System.Drawing.Point(368, 113);
			this.nudResizeHeight.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			this.nudResizeHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeHeight.Name = "nudResizeHeight";
			this.nudResizeHeight.Size = new System.Drawing.Size(70, 19);
			this.nudResizeHeight.TabIndex = 5;
			this.nudResizeHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeHeight.ValueChanged += new System.EventHandler(this.ResizeValue_Changed);
			//
			// lblResizeShiftX
			//
			this.lblResizeShiftX.AutoSize = true;
			this.lblResizeShiftX.Location = new System.Drawing.Point(12, 146);
			this.lblResizeShiftX.Name = "lblResizeShiftX";
			this.lblResizeShiftX.Size = new System.Drawing.Size(100, 12);
			this.lblResizeShiftX.TabIndex = 20;
			this.lblResizeShiftX.Text = "左側に足す列数 :";
			//
			// nudResizeShiftX
			//
			this.nudResizeShiftX.Location = new System.Drawing.Point(140, 143);
			this.nudResizeShiftX.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			this.nudResizeShiftX.Minimum = new decimal(new int[] { 255, 0, 0, -2147483648 });
			this.nudResizeShiftX.Name = "nudResizeShiftX";
			this.nudResizeShiftX.Size = new System.Drawing.Size(70, 19);
			this.nudResizeShiftX.TabIndex = 21;
			this.nudResizeShiftX.ValueChanged += new System.EventHandler(this.ResizeShift_Changed);
			//
			// lblResizeShiftY
			//
			this.lblResizeShiftY.AutoSize = true;
			this.lblResizeShiftY.Location = new System.Drawing.Point(240, 146);
			this.lblResizeShiftY.Name = "lblResizeShiftY";
			this.lblResizeShiftY.Size = new System.Drawing.Size(100, 12);
			this.lblResizeShiftY.TabIndex = 22;
			this.lblResizeShiftY.Text = "上側に足す行数 :";
			//
			// nudResizeShiftY
			//
			this.nudResizeShiftY.Location = new System.Drawing.Point(368, 143);
			this.nudResizeShiftY.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			this.nudResizeShiftY.Minimum = new decimal(new int[] { 255, 0, 0, -2147483648 });
			this.nudResizeShiftY.Name = "nudResizeShiftY";
			this.nudResizeShiftY.Size = new System.Drawing.Size(70, 19);
			this.nudResizeShiftY.TabIndex = 23;
			this.nudResizeShiftY.ValueChanged += new System.EventHandler(this.ResizeShift_Changed);
			//
			// lblResizeBorderWidth
			//
			this.lblResizeBorderWidth.AutoSize = true;
			this.lblResizeBorderWidth.Location = new System.Drawing.Point(12, 176);
			this.lblResizeBorderWidth.Name = "lblResizeBorderWidth";
			this.lblResizeBorderWidth.Size = new System.Drawing.Size(90, 12);
			this.lblResizeBorderWidth.TabIndex = 6;
			this.lblResizeBorderWidth.Text = "ボーダーの幅 :";
			//
			// nudResizeBorderWidth
			//
			this.nudResizeBorderWidth.Location = new System.Drawing.Point(140, 173);
			this.nudResizeBorderWidth.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			this.nudResizeBorderWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeBorderWidth.Name = "nudResizeBorderWidth";
			this.nudResizeBorderWidth.Size = new System.Drawing.Size(70, 19);
			this.nudResizeBorderWidth.TabIndex = 7;
			this.nudResizeBorderWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeBorderWidth.ValueChanged += new System.EventHandler(this.ResizeValue_Changed);
			//
			// lblResizeBorderHeight
			//
			this.lblResizeBorderHeight.AutoSize = true;
			this.lblResizeBorderHeight.Location = new System.Drawing.Point(240, 176);
			this.lblResizeBorderHeight.Name = "lblResizeBorderHeight";
			this.lblResizeBorderHeight.Size = new System.Drawing.Size(100, 12);
			this.lblResizeBorderHeight.TabIndex = 8;
			this.lblResizeBorderHeight.Text = "ボーダーの高さ :";
			//
			// nudResizeBorderHeight
			//
			this.nudResizeBorderHeight.Location = new System.Drawing.Point(368, 173);
			this.nudResizeBorderHeight.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			this.nudResizeBorderHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeBorderHeight.Name = "nudResizeBorderHeight";
			this.nudResizeBorderHeight.Size = new System.Drawing.Size(70, 19);
			this.nudResizeBorderHeight.TabIndex = 9;
			this.nudResizeBorderHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudResizeBorderHeight.ValueChanged += new System.EventHandler(this.ResizeValue_Changed);
			//
			// lblResizeFill
			//
			this.lblResizeFill.AutoSize = true;
			this.lblResizeFill.Location = new System.Drawing.Point(12, 214);
			this.lblResizeFill.Name = "lblResizeFill";
			this.lblResizeFill.Size = new System.Drawing.Size(120, 12);
			this.lblResizeFill.TabIndex = 10;
			this.lblResizeFill.Text = "埋めるブロック（16進）:";
			//
			// nudResizeFillBlock
			//
			this.nudResizeFillBlock.Hexadecimal = true;
			this.nudResizeFillBlock.Location = new System.Drawing.Point(140, 211);
			this.nudResizeFillBlock.Maximum = new decimal(new int[] { 1023, 0, 0, 0 });
			this.nudResizeFillBlock.Name = "nudResizeFillBlock";
			this.nudResizeFillBlock.Size = new System.Drawing.Size(70, 19);
			this.nudResizeFillBlock.TabIndex = 11;
			this.nudResizeFillBlock.ValueChanged += new System.EventHandler(this.ResizeValue_Changed);
			//
			// picResizeFillBlock
			//
			this.picResizeFillBlock.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picResizeFillBlock.Location = new System.Drawing.Point(220, 202);
			this.picResizeFillBlock.Name = "picResizeFillBlock";
			this.picResizeFillBlock.Size = new System.Drawing.Size(36, 36);
			this.picResizeFillBlock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.picResizeFillBlock.TabIndex = 12;
			this.picResizeFillBlock.TabStop = false;
			//
			// lblResizeFillNote
			//
			this.lblResizeFillNote.Location = new System.Drawing.Point(266, 200);
			this.lblResizeFillNote.Name = "lblResizeFillNote";
			this.lblResizeFillNote.Size = new System.Drawing.Size(186, 44);
			this.lblResizeFillNote.TabIndex = 13;
			this.lblResizeFillNote.Text = "初めは、今いちばん多いブロックです。番号はブロック一覧で確かめられます。";
			//
			// lblResizeInfo
			//
			this.lblResizeInfo.Location = new System.Drawing.Point(12, 250);
			this.lblResizeInfo.Name = "lblResizeInfo";
			this.lblResizeInfo.Size = new System.Drawing.Size(436, 124);
			this.lblResizeInfo.TabIndex = 14;
			//
			// btnResizeOk
			//
			this.btnResizeOk.Location = new System.Drawing.Point(222, 382);
			this.btnResizeOk.Name = "btnResizeOk";
			this.btnResizeOk.Size = new System.Drawing.Size(126, 30);
			this.btnResizeOk.TabIndex = 15;
			this.btnResizeOk.Text = "大きさを変える";
			this.btnResizeOk.UseVisualStyleBackColor = true;
			this.btnResizeOk.Click += new System.EventHandler(this.btnResizeOk_Click);
			//
			// btnResizeCancel
			//
			this.btnResizeCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnResizeCancel.Location = new System.Drawing.Point(358, 382);
			this.btnResizeCancel.Name = "btnResizeCancel";
			this.btnResizeCancel.Size = new System.Drawing.Size(90, 30);
			this.btnResizeCancel.TabIndex = 16;
			this.btnResizeCancel.Text = "キャンセル";
			this.btnResizeCancel.UseVisualStyleBackColor = true;
			//
			// MapResizeForm
			//
			this.AcceptButton = this.btnResizeOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnResizeCancel;
			this.ClientSize = new System.Drawing.Size(460, 424);
			this.Controls.Add(this.lblResizeDesc);
			this.Controls.Add(this.lblResizeCurrent);
			this.Controls.Add(this.lblResizeWidth);
			this.Controls.Add(this.nudResizeWidth);
			this.Controls.Add(this.lblResizeHeight);
			this.Controls.Add(this.nudResizeHeight);
			this.Controls.Add(this.lblResizeShiftX);
			this.Controls.Add(this.nudResizeShiftX);
			this.Controls.Add(this.lblResizeShiftY);
			this.Controls.Add(this.nudResizeShiftY);
			this.Controls.Add(this.lblResizeBorderWidth);
			this.Controls.Add(this.nudResizeBorderWidth);
			this.Controls.Add(this.lblResizeBorderHeight);
			this.Controls.Add(this.nudResizeBorderHeight);
			this.Controls.Add(this.lblResizeFill);
			this.Controls.Add(this.nudResizeFillBlock);
			this.Controls.Add(this.picResizeFillBlock);
			this.Controls.Add(this.lblResizeFillNote);
			this.Controls.Add(this.lblResizeInfo);
			this.Controls.Add(this.btnResizeOk);
			this.Controls.Add(this.btnResizeCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapResizeForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "マップの大きさを変える";
			((System.ComponentModel.ISupportInitialize)(this.nudResizeWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeShiftX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeShiftY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeBorderWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeBorderHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudResizeFillBlock)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picResizeFillBlock)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblResizeDesc;
		private System.Windows.Forms.Label lblResizeCurrent;
		private System.Windows.Forms.Label lblResizeWidth;
		private System.Windows.Forms.NumericUpDown nudResizeWidth;
		private System.Windows.Forms.Label lblResizeHeight;
		private System.Windows.Forms.NumericUpDown nudResizeHeight;
		private System.Windows.Forms.Label lblResizeShiftX;
		private System.Windows.Forms.NumericUpDown nudResizeShiftX;
		private System.Windows.Forms.Label lblResizeShiftY;
		private System.Windows.Forms.NumericUpDown nudResizeShiftY;
		private System.Windows.Forms.Label lblResizeBorderWidth;
		private System.Windows.Forms.NumericUpDown nudResizeBorderWidth;
		private System.Windows.Forms.Label lblResizeBorderHeight;
		private System.Windows.Forms.NumericUpDown nudResizeBorderHeight;
		private System.Windows.Forms.Label lblResizeFill;
		private System.Windows.Forms.NumericUpDown nudResizeFillBlock;
		private System.Windows.Forms.PictureBox picResizeFillBlock;
		private System.Windows.Forms.Label lblResizeFillNote;
		private System.Windows.Forms.Label lblResizeInfo;
		private System.Windows.Forms.Button btnResizeOk;
		private System.Windows.Forms.Button btnResizeCancel;
	}
}
