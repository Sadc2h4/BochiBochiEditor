namespace BochiBochiEditor
{
	partial class TileImportWizard
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
			this.components = new System.ComponentModel.Container();
			this.lblStep5 = new System.Windows.Forms.Label();
			this.pnlPagePlace = new System.Windows.Forms.Panel();
			this.lblPagePlaceTitle = new System.Windows.Forms.Label();
			this.flpPlaceTools = new System.Windows.Forms.FlowLayoutPanel();
			this.btnAutoPlace = new System.Windows.Forms.Button();
			this.btnClearPlace = new System.Windows.Forms.Button();
			this.chkAllowReplace = new System.Windows.Forms.CheckBox();
			this.lblPlaceInfo = new System.Windows.Forms.Label();
			this.splitPlace = new System.Windows.Forms.SplitContainer();
			this.lblPlaceSource = new System.Windows.Forms.Label();
			this.pnlPlaceSource = new System.Windows.Forms.Panel();
			this.lblPlaceTarget = new System.Windows.Forms.Label();
			this.pnlPlaceTarget = new System.Windows.Forms.Panel();
			this.pnlSteps = new System.Windows.Forms.Panel();
			this.lblStep4 = new System.Windows.Forms.Label();
			this.lblStep3 = new System.Windows.Forms.Label();
			this.lblStep2 = new System.Windows.Forms.Label();
			this.lblStep1 = new System.Windows.Forms.Label();
			this.lblWizardTitle = new System.Windows.Forms.Label();
			this.pnlButtons = new System.Windows.Forms.Panel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnNext = new System.Windows.Forms.Button();
			this.btnBack = new System.Windows.Forms.Button();
			this.pnlContent = new System.Windows.Forms.Panel();
			this.pnlPageImage = new System.Windows.Forms.Panel();
			this.lblImageInfo = new System.Windows.Forms.Label();
			this.grpTransparent = new System.Windows.Forms.GroupBox();
			this.lblTransparentInfo = new System.Windows.Forms.Label();
			this.pnlTransparentSwatch = new System.Windows.Forms.Panel();
			this.grpTarget = new System.Windows.Forms.GroupBox();
			this.lblTargetInfo = new System.Windows.Forms.Label();
			this.rbTargetPrimary = new System.Windows.Forms.RadioButton();
			this.rbTargetSecondary = new System.Windows.Forms.RadioButton();
			this.pnlSourcePreview = new System.Windows.Forms.Panel();
			this.txtImagePath = new System.Windows.Forms.TextBox();
			this.btnPickImage = new System.Windows.Forms.Button();
			this.lblPageImageTitle = new System.Windows.Forms.Label();
			this.pnlPageColor = new System.Windows.Forms.Panel();
			this.lblColorInfo = new System.Windows.Forms.Label();
			this.pnlConvertPreview = new System.Windows.Forms.Panel();
			this.lblPaletteAfter = new System.Windows.Forms.Label();
			this.pnlPaletteAfter = new System.Windows.Forms.Panel();
			this.lblPaletteBefore = new System.Windows.Forms.Label();
			this.pnlPaletteBefore = new System.Windows.Forms.Panel();
			this.lblPaletteWarning = new System.Windows.Forms.Label();
			this.rbPaletteMatch = new System.Windows.Forms.RadioButton();
			this.rbPaletteOverwrite = new System.Windows.Forms.RadioButton();
			this.cmbPaletteSlot = new System.Windows.Forms.ComboBox();
			this.lblPaletteSlot = new System.Windows.Forms.Label();
			this.lblPageColorTitle = new System.Windows.Forms.Label();
			this.pnlPageTiles = new System.Windows.Forms.Panel();
			this.txtIssues = new System.Windows.Forms.TextBox();
			this.pnlBlockMeter = new System.Windows.Forms.Panel();
			this.lblBlockStats = new System.Windows.Forms.Label();
			this.pnlTileMeter = new System.Windows.Forms.Panel();
			this.lblTileStats = new System.Windows.Forms.Label();
			this.grpLayer = new System.Windows.Forms.GroupBox();
			this.pnlBaseBlockPreview = new System.Windows.Forms.Panel();
			this.pnlLayerPreview = new System.Windows.Forms.Panel();
			this.nudBaseBlock = new System.Windows.Forms.NumericUpDown();
			this.lblBaseBlock = new System.Windows.Forms.Label();
			this.rbLayerAbove = new System.Windows.Forms.RadioButton();
			this.rbLayerBelow = new System.Windows.Forms.RadioButton();
			this.rbLayerBottom = new System.Windows.Forms.RadioButton();
			this.chkSkipEmpty = new System.Windows.Forms.CheckBox();
			this.chkDedupeBlocks = new System.Windows.Forms.CheckBox();
			this.chkReuseTiles = new System.Windows.Forms.CheckBox();
			this.chkDedupeFlip = new System.Windows.Forms.CheckBox();
			this.lblPageTilesTitle = new System.Windows.Forms.Label();
			this.pnlPageConfirm = new System.Windows.Forms.Panel();
			this.txtResult = new System.Windows.Forms.TextBox();
			this.btnWrite = new System.Windows.Forms.Button();
			this.chkBackup = new System.Windows.Forms.CheckBox();
			this.txtSummary = new System.Windows.Forms.TextBox();
			this.lblPageConfirmTitle = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.btnPickBaseBlock = new System.Windows.Forms.Button();
			this.tmrBaseBlockInput = new System.Windows.Forms.Timer(this.components);
			this.pnlSteps.SuspendLayout();
			this.pnlButtons.SuspendLayout();
			this.pnlContent.SuspendLayout();
			this.pnlPageImage.SuspendLayout();
			this.grpTransparent.SuspendLayout();
			this.grpTarget.SuspendLayout();
			this.pnlPageColor.SuspendLayout();
			this.pnlPageTiles.SuspendLayout();
			this.grpLayer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudBaseBlock)).BeginInit();
			this.pnlPageConfirm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitPlace)).BeginInit();
			this.splitPlace.Panel1.SuspendLayout();
			this.splitPlace.Panel2.SuspendLayout();
			this.splitPlace.SuspendLayout();
			this.pnlPagePlace.SuspendLayout();
			this.flpPlaceTools.SuspendLayout();
			this.SuspendLayout();
			//
			// pnlSteps
			//
			this.pnlSteps.Controls.Add(this.lblStep5);
			this.pnlSteps.Controls.Add(this.lblStep4);
			this.pnlSteps.Controls.Add(this.lblStep3);
			this.pnlSteps.Controls.Add(this.lblStep2);
			this.pnlSteps.Controls.Add(this.lblStep1);
			this.pnlSteps.Controls.Add(this.lblWizardTitle);
			this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlSteps.Location = new System.Drawing.Point(0, 0);
			this.pnlSteps.Name = "pnlSteps";
			this.pnlSteps.Padding = new System.Windows.Forms.Padding(12);
			this.pnlSteps.Size = new System.Drawing.Size(170, 552);
			this.pnlSteps.TabIndex = 0;
			//
			// lblWizardTitle
			//
			this.lblWizardTitle.Font = new System.Drawing.Font("Yu Gothic UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblWizardTitle.Location = new System.Drawing.Point(12, 14);
			this.lblWizardTitle.Name = "lblWizardTitle";
			this.lblWizardTitle.Size = new System.Drawing.Size(150, 48);
			this.lblWizardTitle.TabIndex = 0;
			this.lblWizardTitle.Text = "マップチップ\r\n取り込み";
			//
			// lblStep1
			//
			this.lblStep1.Location = new System.Drawing.Point(12, 78);
			this.lblStep1.Name = "lblStep1";
			this.lblStep1.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.lblStep1.Size = new System.Drawing.Size(146, 30);
			this.lblStep1.TabIndex = 1;
			this.lblStep1.Text = "1. 画像と書き込み先";
			this.lblStep1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// lblStep2
			//
			this.lblStep2.Location = new System.Drawing.Point(12, 112);
			this.lblStep2.Name = "lblStep2";
			this.lblStep2.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.lblStep2.Size = new System.Drawing.Size(146, 30);
			this.lblStep2.TabIndex = 2;
			this.lblStep2.Text = "2. 色とパレット";
			this.lblStep2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// lblStep3
			//
			this.lblStep3.Location = new System.Drawing.Point(12, 146);
			this.lblStep3.Name = "lblStep3";
			this.lblStep3.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.lblStep3.Size = new System.Drawing.Size(146, 30);
			this.lblStep3.TabIndex = 3;
			this.lblStep3.Text = "3. タイルとブロック";
			this.lblStep3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// lblStep4
			//
			this.lblStep4.Location = new System.Drawing.Point(12, 180);
			this.lblStep4.Name = "lblStep4";
			this.lblStep4.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.lblStep4.Size = new System.Drawing.Size(146, 30);
			this.lblStep4.TabIndex = 4;
			this.lblStep4.Text = "4. 配置";
			this.lblStep4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// pnlButtons
			//
			this.pnlButtons.Controls.Add(this.btnCancel);
			this.pnlButtons.Controls.Add(this.btnNext);
			this.pnlButtons.Controls.Add(this.btnBack);
			this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlButtons.Location = new System.Drawing.Point(0, 552);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Size = new System.Drawing.Size(900, 48);
			this.pnlButtons.TabIndex = 2;
			//
			// btnBack
			//
			this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBack.Location = new System.Drawing.Point(572, 10);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(100, 28);
			this.btnBack.TabIndex = 0;
			this.btnBack.Text = "< 戻る";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			//
			// btnNext
			//
			this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNext.Location = new System.Drawing.Point(678, 10);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new System.Drawing.Size(100, 28);
			this.btnNext.TabIndex = 1;
			this.btnNext.Text = "次へ >";
			this.btnNext.UseVisualStyleBackColor = true;
			this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
			//
			// btnCancel
			//
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(788, 10);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(100, 28);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "キャンセル";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			// pnlContent
			//
			this.pnlContent.Controls.Add(this.pnlPageImage);
			this.pnlContent.Controls.Add(this.pnlPageColor);
			this.pnlContent.Controls.Add(this.pnlPageTiles);
			this.pnlContent.Controls.Add(this.pnlPageConfirm);
			this.pnlContent.Controls.Add(this.pnlPagePlace);
			this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlContent.Location = new System.Drawing.Point(170, 0);
			this.pnlContent.Name = "pnlContent";
			this.pnlContent.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
			this.pnlContent.Size = new System.Drawing.Size(730, 552);
			this.pnlContent.TabIndex = 1;
			//
			// pnlPageImage
			//
			this.pnlPageImage.Controls.Add(this.lblImageInfo);
			this.pnlPageImage.Controls.Add(this.grpTransparent);
			this.pnlPageImage.Controls.Add(this.grpTarget);
			this.pnlPageImage.Controls.Add(this.pnlSourcePreview);
			this.pnlPageImage.Controls.Add(this.txtImagePath);
			this.pnlPageImage.Controls.Add(this.btnPickImage);
			this.pnlPageImage.Controls.Add(this.lblPageImageTitle);
			this.pnlPageImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPageImage.Location = new System.Drawing.Point(16, 12);
			this.pnlPageImage.Name = "pnlPageImage";
			this.pnlPageImage.Size = new System.Drawing.Size(698, 532);
			this.pnlPageImage.TabIndex = 0;
			//
			// lblPageImageTitle
			//
			this.lblPageImageTitle.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblPageImageTitle.Location = new System.Drawing.Point(0, 0);
			this.lblPageImageTitle.Name = "lblPageImageTitle";
			this.lblPageImageTitle.Size = new System.Drawing.Size(520, 30);
			this.lblPageImageTitle.TabIndex = 0;
			this.lblPageImageTitle.Text = "画像と書き込み先を選ぶ";
			//
			// btnPickImage
			//
			this.btnPickImage.Location = new System.Drawing.Point(0, 40);
			this.btnPickImage.Name = "btnPickImage";
			this.btnPickImage.Size = new System.Drawing.Size(120, 28);
			this.btnPickImage.TabIndex = 1;
			this.btnPickImage.Text = "画像を選ぶ…";
			this.btnPickImage.UseVisualStyleBackColor = true;
			this.btnPickImage.Click += new System.EventHandler(this.btnPickImage_Click);
			//
			// txtImagePath
			//
			this.txtImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.txtImagePath.Location = new System.Drawing.Point(128, 44);
			this.txtImagePath.Name = "txtImagePath";
			this.txtImagePath.PlaceholderText = "PNG などの画像ファイル（ドラッグ＆ドロップでも指定できます）";
			this.txtImagePath.ReadOnly = true;
			this.txtImagePath.Size = new System.Drawing.Size(570, 19);
			this.txtImagePath.TabIndex = 2;
			//
			// pnlSourcePreview
			//
			this.pnlSourcePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlSourcePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlSourcePreview.Cursor = System.Windows.Forms.Cursors.Cross;
			this.pnlSourcePreview.Location = new System.Drawing.Point(0, 80);
			this.pnlSourcePreview.Name = "pnlSourcePreview";
			this.pnlSourcePreview.Size = new System.Drawing.Size(400, 420);
			this.pnlSourcePreview.TabIndex = 3;
			this.pnlSourcePreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSourcePreview_Paint);
			this.pnlSourcePreview.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlSourcePreview_MouseClick);
			//
			// grpTarget
			//
			this.grpTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.grpTarget.Controls.Add(this.lblTargetInfo);
			this.grpTarget.Controls.Add(this.rbTargetPrimary);
			this.grpTarget.Controls.Add(this.rbTargetSecondary);
			this.grpTarget.Location = new System.Drawing.Point(414, 80);
			this.grpTarget.Name = "grpTarget";
			this.grpTarget.Size = new System.Drawing.Size(284, 186);
			this.grpTarget.TabIndex = 4;
			this.grpTarget.TabStop = false;
			this.grpTarget.Text = "書き込み先";
			//
			// rbTargetSecondary
			//
			this.rbTargetSecondary.AutoSize = true;
			this.rbTargetSecondary.Checked = true;
			this.rbTargetSecondary.Location = new System.Drawing.Point(14, 26);
			this.rbTargetSecondary.Name = "rbTargetSecondary";
			this.rbTargetSecondary.Size = new System.Drawing.Size(200, 16);
			this.rbTargetSecondary.TabIndex = 0;
			this.rbTargetSecondary.TabStop = true;
			this.rbTargetSecondary.Text = "タイルセット2（このマップ用の部分）";
			this.rbTargetSecondary.UseVisualStyleBackColor = true;
			this.rbTargetSecondary.CheckedChanged += new System.EventHandler(this.Target_CheckedChanged);
			//
			// rbTargetPrimary
			//
			this.rbTargetPrimary.AutoSize = true;
			this.rbTargetPrimary.Location = new System.Drawing.Point(14, 52);
			this.rbTargetPrimary.Name = "rbTargetPrimary";
			this.rbTargetPrimary.Size = new System.Drawing.Size(196, 16);
			this.rbTargetPrimary.TabIndex = 1;
			this.rbTargetPrimary.Text = "タイルセット1（多くのマップで共有）";
			this.rbTargetPrimary.UseVisualStyleBackColor = true;
			this.rbTargetPrimary.CheckedChanged += new System.EventHandler(this.Target_CheckedChanged);
			//
			// lblTargetInfo
			//
			this.lblTargetInfo.Location = new System.Drawing.Point(14, 78);
			this.lblTargetInfo.Name = "lblTargetInfo";
			this.lblTargetInfo.Size = new System.Drawing.Size(260, 100);
			this.lblTargetInfo.TabIndex = 2;
			this.lblTargetInfo.Text = "-";
			//
			// grpTransparent
			//
			this.grpTransparent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.grpTransparent.Controls.Add(this.lblTransparentInfo);
			this.grpTransparent.Controls.Add(this.pnlTransparentSwatch);
			this.grpTransparent.Location = new System.Drawing.Point(414, 276);
			this.grpTransparent.Name = "grpTransparent";
			this.grpTransparent.Size = new System.Drawing.Size(284, 96);
			this.grpTransparent.TabIndex = 5;
			this.grpTransparent.TabStop = false;
			this.grpTransparent.Text = "透明にする色";
			//
			// pnlTransparentSwatch
			//
			this.pnlTransparentSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlTransparentSwatch.Location = new System.Drawing.Point(14, 28);
			this.pnlTransparentSwatch.Name = "pnlTransparentSwatch";
			this.pnlTransparentSwatch.Size = new System.Drawing.Size(32, 32);
			this.pnlTransparentSwatch.TabIndex = 0;
			//
			// lblTransparentInfo
			//
			this.lblTransparentInfo.Location = new System.Drawing.Point(56, 24);
			this.lblTransparentInfo.Name = "lblTransparentInfo";
			this.lblTransparentInfo.Size = new System.Drawing.Size(220, 62);
			this.lblTransparentInfo.TabIndex = 1;
			this.lblTransparentInfo.Text = "左の画像をクリックすると、その色を透明にします（初期値: 左上の色）。";
			//
			// lblImageInfo
			//
			this.lblImageInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblImageInfo.Location = new System.Drawing.Point(414, 384);
			this.lblImageInfo.Name = "lblImageInfo";
			this.lblImageInfo.Size = new System.Drawing.Size(284, 116);
			this.lblImageInfo.TabIndex = 6;
			this.lblImageInfo.Text = "画像を選んでください。";
			//
			// pnlPageColor
			//
			this.pnlPageColor.Controls.Add(this.lblColorInfo);
			this.pnlPageColor.Controls.Add(this.pnlConvertPreview);
			this.pnlPageColor.Controls.Add(this.lblPaletteAfter);
			this.pnlPageColor.Controls.Add(this.pnlPaletteAfter);
			this.pnlPageColor.Controls.Add(this.lblPaletteBefore);
			this.pnlPageColor.Controls.Add(this.pnlPaletteBefore);
			this.pnlPageColor.Controls.Add(this.lblPaletteWarning);
			this.pnlPageColor.Controls.Add(this.rbPaletteMatch);
			this.pnlPageColor.Controls.Add(this.rbPaletteOverwrite);
			this.pnlPageColor.Controls.Add(this.cmbPaletteSlot);
			this.pnlPageColor.Controls.Add(this.lblPaletteSlot);
			this.pnlPageColor.Controls.Add(this.lblPageColorTitle);
			this.pnlPageColor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPageColor.Location = new System.Drawing.Point(16, 12);
			this.pnlPageColor.Name = "pnlPageColor";
			this.pnlPageColor.Size = new System.Drawing.Size(698, 532);
			this.pnlPageColor.TabIndex = 1;
			this.pnlPageColor.Visible = false;
			//
			// lblPageColorTitle
			//
			this.lblPageColorTitle.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblPageColorTitle.Location = new System.Drawing.Point(0, 0);
			this.lblPageColorTitle.Name = "lblPageColorTitle";
			this.lblPageColorTitle.Size = new System.Drawing.Size(520, 30);
			this.lblPageColorTitle.TabIndex = 0;
			this.lblPageColorTitle.Text = "色とパレットを決める";
			//
			// lblPaletteSlot
			//
			this.lblPaletteSlot.AutoSize = true;
			this.lblPaletteSlot.Location = new System.Drawing.Point(0, 48);
			this.lblPaletteSlot.Name = "lblPaletteSlot";
			this.lblPaletteSlot.Size = new System.Drawing.Size(80, 12);
			this.lblPaletteSlot.TabIndex = 1;
			this.lblPaletteSlot.Text = "使うパレット枠 :";
			//
			// cmbPaletteSlot
			//
			this.cmbPaletteSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPaletteSlot.Location = new System.Drawing.Point(100, 44);
			this.cmbPaletteSlot.Name = "cmbPaletteSlot";
			this.cmbPaletteSlot.Size = new System.Drawing.Size(240, 20);
			this.cmbPaletteSlot.TabIndex = 2;
			this.cmbPaletteSlot.SelectedIndexChanged += new System.EventHandler(this.PaletteOption_Changed);
			//
			// rbPaletteOverwrite
			//
			this.rbPaletteOverwrite.AutoSize = true;
			this.rbPaletteOverwrite.Checked = true;
			this.rbPaletteOverwrite.Location = new System.Drawing.Point(0, 80);
			this.rbPaletteOverwrite.Name = "rbPaletteOverwrite";
			this.rbPaletteOverwrite.Size = new System.Drawing.Size(360, 16);
			this.rbPaletteOverwrite.TabIndex = 3;
			this.rbPaletteOverwrite.TabStop = true;
			this.rbPaletteOverwrite.Text = "画像の色でこの枠を上書きする（15 色まで。多い場合は自動で減色）";
			this.rbPaletteOverwrite.UseVisualStyleBackColor = true;
			this.rbPaletteOverwrite.CheckedChanged += new System.EventHandler(this.PaletteOption_Changed);
			//
			// rbPaletteMatch
			//
			this.rbPaletteMatch.AutoSize = true;
			this.rbPaletteMatch.Location = new System.Drawing.Point(0, 104);
			this.rbPaletteMatch.Name = "rbPaletteMatch";
			this.rbPaletteMatch.Size = new System.Drawing.Size(330, 16);
			this.rbPaletteMatch.TabIndex = 4;
			this.rbPaletteMatch.Text = "この枠の今の色に近い色で描く（パレットは変えない）";
			this.rbPaletteMatch.UseVisualStyleBackColor = true;
			this.rbPaletteMatch.CheckedChanged += new System.EventHandler(this.PaletteOption_Changed);
			//
			// lblPaletteWarning
			//
			this.lblPaletteWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblPaletteWarning.ForeColor = System.Drawing.Color.Red;
			this.lblPaletteWarning.Location = new System.Drawing.Point(0, 128);
			this.lblPaletteWarning.Name = "lblPaletteWarning";
			this.lblPaletteWarning.Size = new System.Drawing.Size(698, 36);
			this.lblPaletteWarning.TabIndex = 5;
			//
			// lblPaletteBefore
			//
			this.lblPaletteBefore.AutoSize = true;
			this.lblPaletteBefore.Location = new System.Drawing.Point(0, 172);
			this.lblPaletteBefore.Name = "lblPaletteBefore";
			this.lblPaletteBefore.Size = new System.Drawing.Size(55, 12);
			this.lblPaletteBefore.TabIndex = 6;
			this.lblPaletteBefore.Text = "今の色";
			//
			// pnlPaletteBefore
			//
			this.pnlPaletteBefore.Location = new System.Drawing.Point(100, 166);
			this.pnlPaletteBefore.Name = "pnlPaletteBefore";
			this.pnlPaletteBefore.Size = new System.Drawing.Size(354, 24);
			this.pnlPaletteBefore.TabIndex = 7;
			this.pnlPaletteBefore.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPaletteBefore_Paint);
			//
			// lblPaletteAfter
			//
			this.lblPaletteAfter.AutoSize = true;
			this.lblPaletteAfter.Location = new System.Drawing.Point(0, 204);
			this.lblPaletteAfter.Name = "lblPaletteAfter";
			this.lblPaletteAfter.Size = new System.Drawing.Size(67, 12);
			this.lblPaletteAfter.TabIndex = 8;
			this.lblPaletteAfter.Text = "書き込む色";
			//
			// pnlPaletteAfter
			//
			this.pnlPaletteAfter.Location = new System.Drawing.Point(100, 198);
			this.pnlPaletteAfter.Name = "pnlPaletteAfter";
			this.pnlPaletteAfter.Size = new System.Drawing.Size(354, 24);
			this.pnlPaletteAfter.TabIndex = 9;
			this.pnlPaletteAfter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPaletteAfter_Paint);
			//
			// pnlConvertPreview
			//
			this.pnlConvertPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlConvertPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlConvertPreview.Location = new System.Drawing.Point(0, 236);
			this.pnlConvertPreview.Name = "pnlConvertPreview";
			this.pnlConvertPreview.Size = new System.Drawing.Size(698, 240);
			this.pnlConvertPreview.TabIndex = 10;
			this.pnlConvertPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlConvertPreview_Paint);
			//
			// lblColorInfo
			//
			this.lblColorInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblColorInfo.Location = new System.Drawing.Point(0, 482);
			this.lblColorInfo.Name = "lblColorInfo";
			this.lblColorInfo.Size = new System.Drawing.Size(698, 44);
			this.lblColorInfo.TabIndex = 11;
			this.lblColorInfo.Text = "-";
			//
			// pnlPageTiles
			//
			this.pnlPageTiles.Controls.Add(this.txtIssues);
			this.pnlPageTiles.Controls.Add(this.pnlBlockMeter);
			this.pnlPageTiles.Controls.Add(this.lblBlockStats);
			this.pnlPageTiles.Controls.Add(this.pnlTileMeter);
			this.pnlPageTiles.Controls.Add(this.lblTileStats);
			this.pnlPageTiles.Controls.Add(this.grpLayer);
			this.pnlPageTiles.Controls.Add(this.chkSkipEmpty);
			this.pnlPageTiles.Controls.Add(this.chkDedupeBlocks);
			this.pnlPageTiles.Controls.Add(this.chkReuseTiles);
			this.pnlPageTiles.Controls.Add(this.chkDedupeFlip);
			this.pnlPageTiles.Controls.Add(this.lblPageTilesTitle);
			this.pnlPageTiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPageTiles.Location = new System.Drawing.Point(16, 12);
			this.pnlPageTiles.Name = "pnlPageTiles";
			this.pnlPageTiles.Size = new System.Drawing.Size(698, 532);
			this.pnlPageTiles.TabIndex = 2;
			this.pnlPageTiles.Visible = false;
			//
			// lblPageTilesTitle
			//
			this.lblPageTilesTitle.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblPageTilesTitle.Location = new System.Drawing.Point(0, 0);
			this.lblPageTilesTitle.Name = "lblPageTilesTitle";
			this.lblPageTilesTitle.Size = new System.Drawing.Size(520, 30);
			this.lblPageTilesTitle.TabIndex = 0;
			this.lblPageTilesTitle.Text = "タイルとブロックの作り方";
			//
			// chkDedupeFlip
			//
			this.chkDedupeFlip.AutoSize = true;
			this.chkDedupeFlip.Checked = true;
			this.chkDedupeFlip.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkDedupeFlip.Location = new System.Drawing.Point(0, 44);
			this.chkDedupeFlip.Name = "chkDedupeFlip";
			this.chkDedupeFlip.Size = new System.Drawing.Size(330, 16);
			this.chkDedupeFlip.TabIndex = 1;
			this.chkDedupeFlip.Text = "左右・上下の反転で同じになるタイルは 1 枚にまとめる";
			this.chkDedupeFlip.UseVisualStyleBackColor = true;
			this.chkDedupeFlip.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// chkReuseTiles
			//
			this.chkReuseTiles.AutoSize = true;
			this.chkReuseTiles.Checked = true;
			this.chkReuseTiles.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkReuseTiles.Location = new System.Drawing.Point(0, 68);
			this.chkReuseTiles.Name = "chkReuseTiles";
			this.chkReuseTiles.Size = new System.Drawing.Size(300, 16);
			this.chkReuseTiles.TabIndex = 2;
			this.chkReuseTiles.Text = "今あるタイルと同じ絵なら、新しく作らずにそれを使う";
			this.chkReuseTiles.UseVisualStyleBackColor = true;
			this.chkReuseTiles.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// chkSkipEmpty
			//
			this.chkSkipEmpty.AutoSize = true;
			this.chkSkipEmpty.Checked = true;
			this.chkSkipEmpty.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkSkipEmpty.Location = new System.Drawing.Point(0, 92);
			this.chkSkipEmpty.Name = "chkSkipEmpty";
			this.chkSkipEmpty.Size = new System.Drawing.Size(230, 16);
			this.chkSkipEmpty.TabIndex = 3;
			this.chkSkipEmpty.Text = "全部透明な 16×16 マスはブロックにしない";
			this.chkSkipEmpty.UseVisualStyleBackColor = true;
			this.chkSkipEmpty.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// chkDedupeBlocks
			//
			this.chkDedupeBlocks.AutoSize = true;
			this.chkDedupeBlocks.Checked = true;
			this.chkDedupeBlocks.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkDedupeBlocks.Location = new System.Drawing.Point(0, 116);
			this.chkDedupeBlocks.Name = "chkDedupeBlocks";
			this.chkDedupeBlocks.Size = new System.Drawing.Size(330, 16);
			this.chkDedupeBlocks.TabIndex = 10;
			this.chkDedupeBlocks.Text = "同じ絵の 16×16 マスは 1 ブロックにまとめる（ブロックの空きを節約）";
			this.chkDedupeBlocks.UseVisualStyleBackColor = true;
			this.chkDedupeBlocks.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// grpLayer
			//
			this.grpLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.grpLayer.Controls.Add(this.btnPickBaseBlock);
			this.grpLayer.Controls.Add(this.pnlLayerPreview);
			this.grpLayer.Controls.Add(this.pnlBaseBlockPreview);
			this.grpLayer.Controls.Add(this.nudBaseBlock);
			this.grpLayer.Controls.Add(this.lblBaseBlock);
			this.grpLayer.Controls.Add(this.rbLayerAbove);
			this.grpLayer.Controls.Add(this.rbLayerBelow);
			this.grpLayer.Controls.Add(this.rbLayerBottom);
			this.grpLayer.Location = new System.Drawing.Point(0, 144);
			this.grpLayer.Name = "grpLayer";
			this.grpLayer.Size = new System.Drawing.Size(698, 176);
			this.grpLayer.TabIndex = 4;
			this.grpLayer.TabStop = false;
			this.grpLayer.Text = "重ね方";
			//
			// rbLayerBottom
			//
			this.rbLayerBottom.AutoSize = true;
			this.rbLayerBottom.Checked = true;
			this.rbLayerBottom.Location = new System.Drawing.Point(14, 26);
			this.rbLayerBottom.Name = "rbLayerBottom";
			this.rbLayerBottom.Size = new System.Drawing.Size(300, 16);
			this.rbLayerBottom.TabIndex = 0;
			this.rbLayerBottom.TabStop = true;
			this.rbLayerBottom.Text = "絵だけで置く（透明な部分は背景色になる。床・地面向け）";
			this.rbLayerBottom.UseVisualStyleBackColor = true;
			this.rbLayerBottom.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// rbLayerBelow
			//
			this.rbLayerBelow.AutoSize = true;
			this.rbLayerBelow.Location = new System.Drawing.Point(14, 50);
			this.rbLayerBelow.Name = "rbLayerBelow";
			this.rbLayerBelow.Size = new System.Drawing.Size(390, 16);
			this.rbLayerBelow.TabIndex = 1;
			this.rbLayerBelow.Text = "下地ブロックの上に重ねる・プレイヤーの下に描く（家具・床の模様向け）";
			this.rbLayerBelow.UseVisualStyleBackColor = true;
			this.rbLayerBelow.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// rbLayerAbove
			//
			this.rbLayerAbove.AutoSize = true;
			this.rbLayerAbove.Location = new System.Drawing.Point(14, 74);
			this.rbLayerAbove.Name = "rbLayerAbove";
			this.rbLayerAbove.Size = new System.Drawing.Size(400, 16);
			this.rbLayerAbove.TabIndex = 2;
			this.rbLayerAbove.Text = "下地ブロックの上に重ねる・プレイヤーより手前に描く（木の上部・屋根向け）";
			this.rbLayerAbove.UseVisualStyleBackColor = true;
			this.rbLayerAbove.CheckedChanged += new System.EventHandler(this.TileOption_Changed);
			//
			// lblBaseBlock
			//
			this.lblBaseBlock.AutoSize = true;
			this.lblBaseBlock.Location = new System.Drawing.Point(32, 110);
			this.lblBaseBlock.Name = "lblBaseBlock";
			this.lblBaseBlock.Size = new System.Drawing.Size(120, 12);
			this.lblBaseBlock.TabIndex = 3;
			this.lblBaseBlock.Text = "下地ブロックの番号 (16進) :";
			//
			// nudBaseBlock
			//
			this.nudBaseBlock.Hexadecimal = true;
			this.nudBaseBlock.Location = new System.Drawing.Point(190, 106);
			this.nudBaseBlock.Maximum = new decimal(new int[] { 1023, 0, 0, 0 });
			this.nudBaseBlock.Name = "nudBaseBlock";
			this.nudBaseBlock.Size = new System.Drawing.Size(70, 19);
			this.nudBaseBlock.TabIndex = 4;
			this.nudBaseBlock.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.nudBaseBlock.ValueChanged += new System.EventHandler(this.TileOption_Changed);
			this.nudBaseBlock.TextChanged += new System.EventHandler(this.nudBaseBlock_TextChanged);
			//
			// pnlBaseBlockPreview
			//
			this.pnlBaseBlockPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlBaseBlockPreview.Location = new System.Drawing.Point(270, 98);
			this.pnlBaseBlockPreview.Name = "pnlBaseBlockPreview";
			this.pnlBaseBlockPreview.Size = new System.Drawing.Size(36, 36);
			this.pnlBaseBlockPreview.TabIndex = 5;
			this.pnlBaseBlockPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlBaseBlockPreview_Paint);
			//
			// btnPickBaseBlock
			//
			this.btnPickBaseBlock.Location = new System.Drawing.Point(316, 103);
			this.btnPickBaseBlock.Name = "btnPickBaseBlock";
			this.btnPickBaseBlock.Size = new System.Drawing.Size(118, 26);
			this.btnPickBaseBlock.TabIndex = 7;
			this.btnPickBaseBlock.Text = "一覧から選ぶ…";
			this.btnPickBaseBlock.UseVisualStyleBackColor = true;
			this.btnPickBaseBlock.Click += new System.EventHandler(this.btnPickBaseBlock_Click);
			//
			// tmrBaseBlockInput
			//
			this.tmrBaseBlockInput.Interval = 400;
			this.tmrBaseBlockInput.Tick += new System.EventHandler(this.tmrBaseBlockInput_Tick);
			//
			// pnlLayerPreview
			//
			this.pnlLayerPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlLayerPreview.Location = new System.Drawing.Point(452, 14);
			this.pnlLayerPreview.Name = "pnlLayerPreview";
			this.pnlLayerPreview.Size = new System.Drawing.Size(238, 154);
			this.pnlLayerPreview.TabIndex = 6;
			this.pnlLayerPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlLayerPreview_Paint);
			//
			// lblTileStats
			//
			this.lblTileStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTileStats.Location = new System.Drawing.Point(0, 334);
			this.lblTileStats.Name = "lblTileStats";
			this.lblTileStats.Size = new System.Drawing.Size(698, 18);
			this.lblTileStats.TabIndex = 5;
			this.lblTileStats.Text = "タイル : -";
			//
			// pnlTileMeter
			//
			this.pnlTileMeter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlTileMeter.Location = new System.Drawing.Point(0, 354);
			this.pnlTileMeter.Name = "pnlTileMeter";
			this.pnlTileMeter.Size = new System.Drawing.Size(698, 10);
			this.pnlTileMeter.TabIndex = 6;
			this.pnlTileMeter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlTileMeter_Paint);
			//
			// lblBlockStats
			//
			this.lblBlockStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblBlockStats.Location = new System.Drawing.Point(0, 374);
			this.lblBlockStats.Name = "lblBlockStats";
			this.lblBlockStats.Size = new System.Drawing.Size(698, 18);
			this.lblBlockStats.TabIndex = 7;
			this.lblBlockStats.Text = "ブロック : -";
			//
			// pnlBlockMeter
			//
			this.pnlBlockMeter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlBlockMeter.Location = new System.Drawing.Point(0, 394);
			this.pnlBlockMeter.Name = "pnlBlockMeter";
			this.pnlBlockMeter.Size = new System.Drawing.Size(698, 10);
			this.pnlBlockMeter.TabIndex = 8;
			this.pnlBlockMeter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlBlockMeter_Paint);
			//
			// txtIssues
			//
			this.txtIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.txtIssues.Location = new System.Drawing.Point(0, 418);
			this.txtIssues.Multiline = true;
			this.txtIssues.Name = "txtIssues";
			this.txtIssues.ReadOnly = true;
			this.txtIssues.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtIssues.Size = new System.Drawing.Size(698, 108);
			this.txtIssues.TabIndex = 9;
			//
			// pnlPageConfirm
			//
			this.pnlPageConfirm.Controls.Add(this.txtResult);
			this.pnlPageConfirm.Controls.Add(this.btnWrite);
			this.pnlPageConfirm.Controls.Add(this.chkBackup);
			this.pnlPageConfirm.Controls.Add(this.txtSummary);
			this.pnlPageConfirm.Controls.Add(this.lblPageConfirmTitle);
			this.pnlPageConfirm.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPageConfirm.Location = new System.Drawing.Point(16, 12);
			this.pnlPageConfirm.Name = "pnlPageConfirm";
			this.pnlPageConfirm.Size = new System.Drawing.Size(698, 532);
			this.pnlPageConfirm.TabIndex = 3;
			this.pnlPageConfirm.Visible = false;
			//
			// lblPageConfirmTitle
			//
			this.lblPageConfirmTitle.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblPageConfirmTitle.Location = new System.Drawing.Point(0, 0);
			this.lblPageConfirmTitle.Name = "lblPageConfirmTitle";
			this.lblPageConfirmTitle.Size = new System.Drawing.Size(520, 30);
			this.lblPageConfirmTitle.TabIndex = 0;
			this.lblPageConfirmTitle.Text = "内容を確認して書き込む";
			//
			// txtSummary
			//
			this.txtSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSummary.Location = new System.Drawing.Point(0, 40);
			this.txtSummary.Multiline = true;
			this.txtSummary.Name = "txtSummary";
			this.txtSummary.ReadOnly = true;
			this.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtSummary.Size = new System.Drawing.Size(698, 220);
			this.txtSummary.TabIndex = 1;
			//
			// chkBackup
			//
			this.chkBackup.AutoSize = true;
			this.chkBackup.Checked = true;
			this.chkBackup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkBackup.Location = new System.Drawing.Point(0, 274);
			this.chkBackup.Name = "chkBackup";
			this.chkBackup.Size = new System.Drawing.Size(360, 16);
			this.chkBackup.TabIndex = 2;
			this.chkBackup.Text = "書き込む前に、今の ROM ファイルのバックアップを同じフォルダに作る";
			this.chkBackup.UseVisualStyleBackColor = true;
			//
			// btnWrite
			//
			this.btnWrite.Location = new System.Drawing.Point(0, 302);
			this.btnWrite.Name = "btnWrite";
			this.btnWrite.Size = new System.Drawing.Size(180, 34);
			this.btnWrite.TabIndex = 3;
			this.btnWrite.Text = "ROM に書き込む";
			this.btnWrite.UseVisualStyleBackColor = true;
			this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
			//
			// txtResult
			//
			this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.txtResult.Location = new System.Drawing.Point(0, 350);
			this.txtResult.Multiline = true;
			this.txtResult.Name = "txtResult";
			this.txtResult.ReadOnly = true;
			this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResult.Size = new System.Drawing.Size(698, 176);
			this.txtResult.TabIndex = 4;
			//
			// lblStep5
			//
			this.lblStep5.Location = new System.Drawing.Point(12, 214);
			this.lblStep5.Name = "lblStep5";
			this.lblStep5.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.lblStep5.Size = new System.Drawing.Size(146, 30);
			this.lblStep5.TabIndex = 5;
			this.lblStep5.Text = "5. 確認と書き込み";
			this.lblStep5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// pnlPagePlace
			//
			this.pnlPagePlace.Controls.Add(this.splitPlace);
			this.pnlPagePlace.Controls.Add(this.lblPlaceInfo);
			this.pnlPagePlace.Controls.Add(this.flpPlaceTools);
			this.pnlPagePlace.Controls.Add(this.lblPagePlaceTitle);
			this.pnlPagePlace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPagePlace.Name = "pnlPagePlace";
			this.pnlPagePlace.Size = new System.Drawing.Size(698, 532);
			this.pnlPagePlace.TabIndex = 4;
			this.pnlPagePlace.Visible = false;
			this.lblPagePlaceTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblPagePlaceTitle.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblPagePlaceTitle.Name = "lblPagePlaceTitle";
			this.lblPagePlaceTitle.Size = new System.Drawing.Size(698, 30);
			this.lblPagePlaceTitle.TabIndex = 0;
			this.lblPagePlaceTitle.Text = "ブロックを配置する";
			this.flpPlaceTools.AutoSize = true;
			this.flpPlaceTools.Controls.Add(this.btnAutoPlace);
			this.flpPlaceTools.Controls.Add(this.btnClearPlace);
			this.flpPlaceTools.Controls.Add(this.chkAllowReplace);
			this.flpPlaceTools.Dock = System.Windows.Forms.DockStyle.Top;
			this.flpPlaceTools.Name = "flpPlaceTools";
			this.flpPlaceTools.Padding = new System.Windows.Forms.Padding(0, 2, 0, 4);
			this.flpPlaceTools.TabIndex = 1;
			this.btnAutoPlace.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.btnAutoPlace.Name = "btnAutoPlace";
			this.btnAutoPlace.Size = new System.Drawing.Size(130, 28);
			this.btnAutoPlace.Text = "空きに自動配置";
			this.btnAutoPlace.UseVisualStyleBackColor = true;
			this.btnAutoPlace.Click += new System.EventHandler(this.btnAutoPlace_Click);
			this.btnClearPlace.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.btnClearPlace.Name = "btnClearPlace";
			this.btnClearPlace.Size = new System.Drawing.Size(130, 28);
			this.btnClearPlace.Text = "配置をやり直す";
			this.btnClearPlace.UseVisualStyleBackColor = true;
			this.btnClearPlace.Click += new System.EventHandler(this.btnClearPlace_Click);
			this.chkAllowReplace.AutoSize = true;
			this.chkAllowReplace.Margin = new System.Windows.Forms.Padding(12, 6, 0, 0);
			this.chkAllowReplace.Name = "chkAllowReplace";
			this.chkAllowReplace.Text = "使用中のブロックへの差し替えを許可する";
			this.chkAllowReplace.UseVisualStyleBackColor = true;
			this.lblPlaceInfo.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblPlaceInfo.Name = "lblPlaceInfo";
			this.lblPlaceInfo.Size = new System.Drawing.Size(698, 40);
			this.lblPlaceInfo.TabIndex = 2;
			this.lblPlaceInfo.Text = "-";
			this.splitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitPlace.Name = "splitPlace";
			this.splitPlace.Panel1.Controls.Add(this.pnlPlaceSource);
			this.splitPlace.Panel1.Controls.Add(this.lblPlaceSource);
			this.splitPlace.Panel2.Controls.Add(this.pnlPlaceTarget);
			this.splitPlace.Panel2.Controls.Add(this.lblPlaceTarget);
			this.splitPlace.Size = new System.Drawing.Size(698, 420);
			this.splitPlace.SplitterDistance = 340;
			this.splitPlace.SplitterWidth = 6;
			this.splitPlace.TabIndex = 3;
			this.lblPlaceSource.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblPlaceSource.Name = "lblPlaceSource";
			this.lblPlaceSource.Size = new System.Drawing.Size(340, 20);
			this.lblPlaceSource.Text = "素材のブロック（選んで右へドラッグ）";
			this.pnlPlaceSource.AutoScroll = true;
			this.pnlPlaceSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlPlaceSource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPlaceSource.Name = "pnlPlaceSource";
			this.pnlPlaceSource.TabIndex = 0;
			this.lblPlaceTarget.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblPlaceTarget.Name = "lblPlaceTarget";
			this.lblPlaceTarget.Size = new System.Drawing.Size(352, 20);
			this.lblPlaceTarget.Text = "取り込み先のブロック（緑の枠 = 空き）";
			this.pnlPlaceTarget.AllowDrop = true;
			this.pnlPlaceTarget.AutoScroll = true;
			this.pnlPlaceTarget.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlPlaceTarget.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPlaceTarget.Name = "pnlPlaceTarget";
			this.pnlPlaceTarget.TabIndex = 1;
			//
			// TileImportWizard
			//
			this.AcceptButton = this.btnNext;
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(900, 600);
			this.Controls.Add(this.pnlContent);
			this.Controls.Add(this.pnlSteps);
			this.Controls.Add(this.pnlButtons);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(860, 560);
			this.Name = "TileImportWizard";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "マップチップ取り込み";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.TileImportWizard_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.TileImportWizard_DragEnter);
			this.pnlSteps.ResumeLayout(false);
			this.pnlButtons.ResumeLayout(false);
			this.pnlContent.ResumeLayout(false);
			this.pnlPageImage.ResumeLayout(false);
			this.pnlPageImage.PerformLayout();
			this.grpTransparent.ResumeLayout(false);
			this.grpTarget.ResumeLayout(false);
			this.grpTarget.PerformLayout();
			this.pnlPageColor.ResumeLayout(false);
			this.pnlPageColor.PerformLayout();
			this.pnlPageTiles.ResumeLayout(false);
			this.pnlPageTiles.PerformLayout();
			this.grpLayer.ResumeLayout(false);
			this.grpLayer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudBaseBlock)).EndInit();
			this.pnlPageConfirm.ResumeLayout(false);
			this.flpPlaceTools.ResumeLayout(false);
			this.flpPlaceTools.PerformLayout();
			this.splitPlace.Panel1.ResumeLayout(false);
			this.splitPlace.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitPlace)).EndInit();
			this.splitPlace.ResumeLayout(false);
			this.pnlPagePlace.ResumeLayout(false);
			this.pnlPagePlace.PerformLayout();
			this.pnlPageConfirm.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel pnlSteps;
		private System.Windows.Forms.Label lblWizardTitle;
		private System.Windows.Forms.Label lblStep1;
		private System.Windows.Forms.Label lblStep2;
		private System.Windows.Forms.Label lblStep3;
		private System.Windows.Forms.Label lblStep4;
		private System.Windows.Forms.Panel pnlButtons;
		private System.Windows.Forms.Button btnBack;
		private System.Windows.Forms.Button btnNext;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel pnlContent;
		private System.Windows.Forms.Panel pnlPageImage;
		private System.Windows.Forms.Label lblPageImageTitle;
		private System.Windows.Forms.Button btnPickImage;
		private System.Windows.Forms.TextBox txtImagePath;
		private System.Windows.Forms.Panel pnlSourcePreview;
		private System.Windows.Forms.GroupBox grpTarget;
		private System.Windows.Forms.RadioButton rbTargetSecondary;
		private System.Windows.Forms.RadioButton rbTargetPrimary;
		private System.Windows.Forms.Label lblTargetInfo;
		private System.Windows.Forms.GroupBox grpTransparent;
		private System.Windows.Forms.Panel pnlTransparentSwatch;
		private System.Windows.Forms.Label lblTransparentInfo;
		private System.Windows.Forms.Label lblImageInfo;
		private System.Windows.Forms.Panel pnlPageColor;
		private System.Windows.Forms.Label lblPageColorTitle;
		private System.Windows.Forms.Label lblPaletteSlot;
		private System.Windows.Forms.ComboBox cmbPaletteSlot;
		private System.Windows.Forms.RadioButton rbPaletteOverwrite;
		private System.Windows.Forms.RadioButton rbPaletteMatch;
		private System.Windows.Forms.Label lblPaletteWarning;
		private System.Windows.Forms.Label lblPaletteBefore;
		private System.Windows.Forms.Panel pnlPaletteBefore;
		private System.Windows.Forms.Label lblPaletteAfter;
		private System.Windows.Forms.Panel pnlPaletteAfter;
		private System.Windows.Forms.Panel pnlConvertPreview;
		private System.Windows.Forms.Label lblColorInfo;
		private System.Windows.Forms.Panel pnlPageTiles;
		private System.Windows.Forms.Label lblPageTilesTitle;
		private System.Windows.Forms.CheckBox chkDedupeFlip;
		private System.Windows.Forms.CheckBox chkReuseTiles;
		private System.Windows.Forms.CheckBox chkSkipEmpty;
		private System.Windows.Forms.CheckBox chkDedupeBlocks;
		private System.Windows.Forms.GroupBox grpLayer;
		private System.Windows.Forms.RadioButton rbLayerBottom;
		private System.Windows.Forms.RadioButton rbLayerBelow;
		private System.Windows.Forms.RadioButton rbLayerAbove;
		private System.Windows.Forms.Label lblBaseBlock;
		private System.Windows.Forms.NumericUpDown nudBaseBlock;
		private System.Windows.Forms.Panel pnlBaseBlockPreview;
		private System.Windows.Forms.Panel pnlLayerPreview;
		private System.Windows.Forms.Button btnPickBaseBlock;
		private System.Windows.Forms.Timer tmrBaseBlockInput;
		private System.Windows.Forms.Label lblTileStats;
		private System.Windows.Forms.Panel pnlTileMeter;
		private System.Windows.Forms.Label lblBlockStats;
		private System.Windows.Forms.Panel pnlBlockMeter;
		private System.Windows.Forms.TextBox txtIssues;
		private System.Windows.Forms.Panel pnlPageConfirm;
		private System.Windows.Forms.Label lblPageConfirmTitle;
		private System.Windows.Forms.TextBox txtSummary;
		private System.Windows.Forms.CheckBox chkBackup;
		private System.Windows.Forms.Button btnWrite;
		private System.Windows.Forms.TextBox txtResult;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Label lblStep5;
		private System.Windows.Forms.Panel pnlPagePlace;
		private System.Windows.Forms.Label lblPagePlaceTitle;
		private System.Windows.Forms.FlowLayoutPanel flpPlaceTools;
		private System.Windows.Forms.Button btnAutoPlace;
		private System.Windows.Forms.Button btnClearPlace;
		private System.Windows.Forms.CheckBox chkAllowReplace;
		private System.Windows.Forms.Label lblPlaceInfo;
		private System.Windows.Forms.SplitContainer splitPlace;
		private System.Windows.Forms.Label lblPlaceSource;
		private System.Windows.Forms.Panel pnlPlaceSource;
		private System.Windows.Forms.Label lblPlaceTarget;
		private System.Windows.Forms.Panel pnlPlaceTarget;
	}
}
