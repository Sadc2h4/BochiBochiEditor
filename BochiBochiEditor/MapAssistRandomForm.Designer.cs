namespace BochiBochiEditor
{
	partial class MapAssistRandomForm
	{
		private System.ComponentModel.IContainer components = null;

		//-------------------------------------------------------------------------------
		// 使っている資源を破棄する処理
		//-------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		//-------------------------------------------------------------------------------
		// 画面の部品を作る処理（デザイナー生成）
		//-------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.lblRandomNote = new System.Windows.Forms.Label();
			this.lblRandomSeed = new System.Windows.Forms.Label();
			this.nudRandomSeed = new System.Windows.Forms.NumericUpDown();
			this.btnRandomSeed = new System.Windows.Forms.Button();
			this.lblRandomWater = new System.Windows.Forms.Label();
			this.nudRandomWater = new System.Windows.Forms.NumericUpDown();
			this.lblRandomForest = new System.Windows.Forms.Label();
			this.nudRandomForest = new System.Windows.Forms.NumericUpDown();
			this.lblRandomGrass = new System.Windows.Forms.Label();
			this.nudRandomGrass = new System.Windows.Forms.NumericUpDown();
			this.lblRandomBuildings = new System.Windows.Forms.Label();
			this.nudRandomBuildings = new System.Windows.Forms.NumericUpDown();
			this.lblRandomDecoration = new System.Windows.Forms.Label();
			this.nudRandomDecoration = new System.Windows.Forms.NumericUpDown();
			this.chkRandomBorder = new System.Windows.Forms.CheckBox();
			this.chkRandomPaths = new System.Windows.Forms.CheckBox();
			this.grpRandomTarget = new System.Windows.Forms.GroupBox();
			this.rdoRandomRange = new System.Windows.Forms.RadioButton();
			this.rdoRandomWhole = new System.Windows.Forms.RadioButton();
			this.btnRandomRun = new System.Windows.Forms.Button();
			this.btnRandomCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomSeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomWater)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomForest)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomGrass)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomBuildings)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomDecoration)).BeginInit();
			this.grpRandomTarget.SuspendLayout();
			this.SuspendLayout();
			//
			// lblRandomNote
			//
			this.lblRandomNote.Location = new System.Drawing.Point(12, 10);
			this.lblRandomNote.Name = "lblRandomNote";
			this.lblRandomNote.Size = new System.Drawing.Size(426, 48);
			this.lblRandomNote.TabIndex = 0;
			this.lblRandomNote.Text = "指定したパーツ（地面・森・水・草むら・建物・道・飾り）から、マップのたたきを作ります。同じ種と設定なら同じ結果になります。結果はメインの画面の「戻る」（Ctrl+Z）で元に戻せます。";
			//
			// lblRandomSeed
			//
			this.lblRandomSeed.AutoSize = true;
			this.lblRandomSeed.Location = new System.Drawing.Point(12, 66);
			this.lblRandomSeed.Name = "lblRandomSeed";
			this.lblRandomSeed.Size = new System.Drawing.Size(67, 15);
			this.lblRandomSeed.TabIndex = 1;
			this.lblRandomSeed.Text = "乱数の種 :";
			//
			// nudRandomSeed
			//
			this.nudRandomSeed.Location = new System.Drawing.Point(232, 63);
			this.nudRandomSeed.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
			this.nudRandomSeed.Name = "nudRandomSeed";
			this.nudRandomSeed.Size = new System.Drawing.Size(100, 23);
			this.nudRandomSeed.TabIndex = 2;
			//
			// btnRandomSeed
			//
			this.btnRandomSeed.Location = new System.Drawing.Point(340, 62);
			this.btnRandomSeed.Name = "btnRandomSeed";
			this.btnRandomSeed.Size = new System.Drawing.Size(80, 25);
			this.btnRandomSeed.TabIndex = 3;
			this.btnRandomSeed.Text = "種をふる";
			this.btnRandomSeed.UseVisualStyleBackColor = true;
			this.btnRandomSeed.Click += new System.EventHandler(this.btnRandomSeed_Click);
			//
			// lblRandomForest
			//
			this.lblRandomForest.AutoSize = true;
			this.lblRandomForest.Location = new System.Drawing.Point(12, 98);
			this.lblRandomForest.Name = "lblRandomForest";
			this.lblRandomForest.Size = new System.Drawing.Size(80, 15);
			this.lblRandomForest.TabIndex = 4;
			this.lblRandomForest.Text = "森の割合 (%) :";
			//
			// nudRandomForest
			//
			this.nudRandomForest.Location = new System.Drawing.Point(232, 95);
			this.nudRandomForest.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
			this.nudRandomForest.Name = "nudRandomForest";
			this.nudRandomForest.Size = new System.Drawing.Size(70, 23);
			this.nudRandomForest.TabIndex = 5;
			this.nudRandomForest.Value = new decimal(new int[] { 15, 0, 0, 0 });
			//
			// lblRandomWater
			//
			this.lblRandomWater.AutoSize = true;
			this.lblRandomWater.Location = new System.Drawing.Point(12, 128);
			this.lblRandomWater.Name = "lblRandomWater";
			this.lblRandomWater.Size = new System.Drawing.Size(80, 15);
			this.lblRandomWater.TabIndex = 6;
			this.lblRandomWater.Text = "水の割合 (%) :";
			//
			// nudRandomWater
			//
			this.nudRandomWater.Location = new System.Drawing.Point(232, 125);
			this.nudRandomWater.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
			this.nudRandomWater.Name = "nudRandomWater";
			this.nudRandomWater.Size = new System.Drawing.Size(70, 23);
			this.nudRandomWater.TabIndex = 7;
			this.nudRandomWater.Value = new decimal(new int[] { 10, 0, 0, 0 });
			//
			// lblRandomGrass
			//
			this.lblRandomGrass.AutoSize = true;
			this.lblRandomGrass.Location = new System.Drawing.Point(12, 158);
			this.lblRandomGrass.Name = "lblRandomGrass";
			this.lblRandomGrass.Size = new System.Drawing.Size(100, 15);
			this.lblRandomGrass.TabIndex = 8;
			this.lblRandomGrass.Text = "草むらの割合 (%) :";
			//
			// nudRandomGrass
			//
			this.nudRandomGrass.Location = new System.Drawing.Point(232, 155);
			this.nudRandomGrass.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
			this.nudRandomGrass.Name = "nudRandomGrass";
			this.nudRandomGrass.Size = new System.Drawing.Size(70, 23);
			this.nudRandomGrass.TabIndex = 9;
			this.nudRandomGrass.Value = new decimal(new int[] { 10, 0, 0, 0 });
			//
			// lblRandomBuildings
			//
			this.lblRandomBuildings.AutoSize = true;
			this.lblRandomBuildings.Location = new System.Drawing.Point(12, 188);
			this.lblRandomBuildings.Name = "lblRandomBuildings";
			this.lblRandomBuildings.Size = new System.Drawing.Size(73, 15);
			this.lblRandomBuildings.TabIndex = 10;
			this.lblRandomBuildings.Text = "建物の数 :";
			//
			// nudRandomBuildings
			//
			this.nudRandomBuildings.Location = new System.Drawing.Point(232, 185);
			this.nudRandomBuildings.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
			this.nudRandomBuildings.Name = "nudRandomBuildings";
			this.nudRandomBuildings.Size = new System.Drawing.Size(70, 23);
			this.nudRandomBuildings.TabIndex = 11;
			this.nudRandomBuildings.Value = new decimal(new int[] { 3, 0, 0, 0 });
			//
			// lblRandomDecoration
			//
			this.lblRandomDecoration.AutoSize = true;
			this.lblRandomDecoration.Location = new System.Drawing.Point(12, 218);
			this.lblRandomDecoration.Name = "lblRandomDecoration";
			this.lblRandomDecoration.Size = new System.Drawing.Size(120, 15);
			this.lblRandomDecoration.TabIndex = 12;
			this.lblRandomDecoration.Text = "小物（地面 100 マスに）:";
			//
			// nudRandomDecoration
			//
			this.nudRandomDecoration.Location = new System.Drawing.Point(232, 215);
			this.nudRandomDecoration.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
			this.nudRandomDecoration.Name = "nudRandomDecoration";
			this.nudRandomDecoration.Size = new System.Drawing.Size(70, 23);
			this.nudRandomDecoration.TabIndex = 13;
			this.nudRandomDecoration.Value = new decimal(new int[] { 3, 0, 0, 0 });
			//
			// chkRandomBorder
			//
			this.chkRandomBorder.AutoSize = true;
			this.chkRandomBorder.Checked = true;
			this.chkRandomBorder.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkRandomBorder.Location = new System.Drawing.Point(316, 97);
			this.chkRandomBorder.Name = "chkRandomBorder";
			this.chkRandomBorder.Size = new System.Drawing.Size(140, 19);
			this.chkRandomBorder.TabIndex = 14;
			this.chkRandomBorder.Text = "まわりを森で囲む";
			this.chkRandomBorder.UseVisualStyleBackColor = true;
			//
			// chkRandomPaths
			//
			this.chkRandomPaths.AutoSize = true;
			this.chkRandomPaths.Checked = true;
			this.chkRandomPaths.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkRandomPaths.Location = new System.Drawing.Point(316, 187);
			this.chkRandomPaths.Name = "chkRandomPaths";
			this.chkRandomPaths.Size = new System.Drawing.Size(150, 19);
			this.chkRandomPaths.TabIndex = 15;
			this.chkRandomPaths.Text = "建物から道を引く";
			this.chkRandomPaths.UseVisualStyleBackColor = true;
			//
			// grpRandomTarget
			//
			this.grpRandomTarget.Controls.Add(this.rdoRandomRange);
			this.grpRandomTarget.Controls.Add(this.rdoRandomWhole);
			this.grpRandomTarget.Location = new System.Drawing.Point(12, 250);
			this.grpRandomTarget.Name = "grpRandomTarget";
			this.grpRandomTarget.Size = new System.Drawing.Size(426, 74);
			this.grpRandomTarget.TabIndex = 16;
			this.grpRandomTarget.TabStop = false;
			this.grpRandomTarget.Text = "作る範囲";
			//
			// rdoRandomRange
			//
			this.rdoRandomRange.AutoSize = true;
			this.rdoRandomRange.Location = new System.Drawing.Point(12, 22);
			this.rdoRandomRange.Name = "rdoRandomRange";
			this.rdoRandomRange.Size = new System.Drawing.Size(120, 19);
			this.rdoRandomRange.TabIndex = 0;
			this.rdoRandomRange.Text = "選んでいる範囲";
			this.rdoRandomRange.UseVisualStyleBackColor = true;
			//
			// rdoRandomWhole
			//
			this.rdoRandomWhole.AutoSize = true;
			this.rdoRandomWhole.Location = new System.Drawing.Point(12, 46);
			this.rdoRandomWhole.Name = "rdoRandomWhole";
			this.rdoRandomWhole.Size = new System.Drawing.Size(220, 19);
			this.rdoRandomWhole.TabIndex = 1;
			this.rdoRandomWhole.Text = "マップ全体（今の中身は置き換わります）";
			this.rdoRandomWhole.UseVisualStyleBackColor = true;
			//
			// btnRandomRun
			//
			this.btnRandomRun.Location = new System.Drawing.Point(250, 336);
			this.btnRandomRun.Name = "btnRandomRun";
			this.btnRandomRun.Size = new System.Drawing.Size(100, 30);
			this.btnRandomRun.TabIndex = 17;
			this.btnRandomRun.Text = "作成する";
			this.btnRandomRun.UseVisualStyleBackColor = true;
			this.btnRandomRun.Click += new System.EventHandler(this.btnRandomRun_Click);
			//
			// btnRandomCancel
			//
			this.btnRandomCancel.Location = new System.Drawing.Point(358, 336);
			this.btnRandomCancel.Name = "btnRandomCancel";
			this.btnRandomCancel.Size = new System.Drawing.Size(80, 30);
			this.btnRandomCancel.TabIndex = 18;
			this.btnRandomCancel.Text = "閉じる";
			this.btnRandomCancel.UseVisualStyleBackColor = true;
			this.btnRandomCancel.Click += new System.EventHandler(this.btnRandomCancel_Click);
			//
			// MapAssistRandomForm
			//
			this.AcceptButton = this.btnRandomRun;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnRandomCancel;
			this.ClientSize = new System.Drawing.Size(450, 378);
			this.Controls.Add(this.lblRandomNote);
			this.Controls.Add(this.lblRandomSeed);
			this.Controls.Add(this.nudRandomSeed);
			this.Controls.Add(this.btnRandomSeed);
			this.Controls.Add(this.lblRandomForest);
			this.Controls.Add(this.nudRandomForest);
			this.Controls.Add(this.lblRandomWater);
			this.Controls.Add(this.nudRandomWater);
			this.Controls.Add(this.lblRandomGrass);
			this.Controls.Add(this.nudRandomGrass);
			this.Controls.Add(this.lblRandomBuildings);
			this.Controls.Add(this.nudRandomBuildings);
			this.Controls.Add(this.lblRandomDecoration);
			this.Controls.Add(this.nudRandomDecoration);
			this.Controls.Add(this.chkRandomBorder);
			this.Controls.Add(this.chkRandomPaths);
			this.Controls.Add(this.grpRandomTarget);
			this.Controls.Add(this.btnRandomRun);
			this.Controls.Add(this.btnRandomCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapAssistRandomForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ランダム作成（試作）";
			((System.ComponentModel.ISupportInitialize)(this.nudRandomSeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomWater)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomForest)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomGrass)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomBuildings)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudRandomDecoration)).EndInit();
			this.grpRandomTarget.ResumeLayout(false);
			this.grpRandomTarget.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblRandomNote;
		private System.Windows.Forms.Label lblRandomSeed;
		private System.Windows.Forms.NumericUpDown nudRandomSeed;
		private System.Windows.Forms.Button btnRandomSeed;
		private System.Windows.Forms.Label lblRandomWater;
		private System.Windows.Forms.NumericUpDown nudRandomWater;
		private System.Windows.Forms.Label lblRandomForest;
		private System.Windows.Forms.NumericUpDown nudRandomForest;
		private System.Windows.Forms.Label lblRandomGrass;
		private System.Windows.Forms.NumericUpDown nudRandomGrass;
		private System.Windows.Forms.Label lblRandomBuildings;
		private System.Windows.Forms.NumericUpDown nudRandomBuildings;
		private System.Windows.Forms.Label lblRandomDecoration;
		private System.Windows.Forms.NumericUpDown nudRandomDecoration;
		private System.Windows.Forms.CheckBox chkRandomBorder;
		private System.Windows.Forms.CheckBox chkRandomPaths;
		private System.Windows.Forms.GroupBox grpRandomTarget;
		private System.Windows.Forms.RadioButton rdoRandomRange;
		private System.Windows.Forms.RadioButton rdoRandomWhole;
		private System.Windows.Forms.Button btnRandomRun;
		private System.Windows.Forms.Button btnRandomCancel;
	}
}
