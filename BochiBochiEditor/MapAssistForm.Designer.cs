namespace BochiBochiEditor
{
	partial class MapAssistForm
	{
		private System.ComponentModel.IContainer components = null;

		//-------------------------------------------------------------------------------
		// 使用中のリソースを破棄する処理
		//-------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.DisposeImages();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		//-------------------------------------------------------------------------------
		// デザイナーで配置した部品を初期化する処理
		//-------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.lblAssistTarget = new System.Windows.Forms.Label();
			this.tabAssistSource = new BochiBochiEditor.ThemedTabControl();
			this.tabAssistBlocks = new System.Windows.Forms.TabPage();
			this.pnlAssistBlocks = new BochiBochiEditor.NoFocusScrollPanel();
			this.canvasAssistBlocks = new BochiBochiEditor.MapAssistCanvas();
			this.tabAssistMap = new System.Windows.Forms.TabPage();
			this.pnlAssistMap = new BochiBochiEditor.NoFocusScrollPanel();
			this.canvasAssistMap = new BochiBochiEditor.MapAssistCanvas();
			this.lblAssistSelection = new System.Windows.Forms.Label();
			this.chkAssistMapZoom = new System.Windows.Forms.CheckBox();
			this.grpAssistParts = new System.Windows.Forms.GroupBox();
			this.lstAssistParts = new System.Windows.Forms.ListBox();
			this.btnAssistAddArea = new System.Windows.Forms.Button();
			this.btnAssistAddEdge = new System.Windows.Forms.Button();
			this.btnAssistAddStamp = new System.Windows.Forms.Button();
			this.btnAssistAddRepeat = new System.Windows.Forms.Button();
			this.btnAssistDeletePart = new System.Windows.Forms.Button();
			this.btnAssistAuto = new System.Windows.Forms.Button();
			this.lblAssistPartsNote = new System.Windows.Forms.Label();
			this.grpAssistDetail = new System.Windows.Forms.GroupBox();
			this.lblAssistName = new System.Windows.Forms.Label();
			this.txtAssistName = new System.Windows.Forms.TextBox();
			this.lblAssistRole = new System.Windows.Forms.Label();
			this.cmbAssistRole = new System.Windows.Forms.ComboBox();
			this.lblAssistKindCaption = new System.Windows.Forms.Label();
			this.lblAssistKind = new System.Windows.Forms.Label();
			this.pnlAssistDetail = new BochiBochiEditor.NoFocusScrollPanel();
			this.btnAssistAssign = new System.Windows.Forms.Button();
			this.btnAssistClear = new System.Windows.Forms.Button();
			this.lblAssistHint = new System.Windows.Forms.Label();
			this.lblAssistStatus = new System.Windows.Forms.Label();
			this.btnAssistReload = new System.Windows.Forms.Button();
			this.btnAssistSave = new System.Windows.Forms.Button();
			this.btnAssistClose = new System.Windows.Forms.Button();
			this.grpAssistSupport = new System.Windows.Forms.GroupBox();
			this.lblSupportNote = new System.Windows.Forms.Label();
			this.btnSupportPaint = new System.Windows.Forms.Button();
			this.lblSupportPaintNote = new System.Windows.Forms.Label();
			this.chkSupportRepeat = new System.Windows.Forms.CheckBox();
			this.chkSupportEdge = new System.Windows.Forms.CheckBox();
			this.chkSupportCollision = new System.Windows.Forms.CheckBox();
			this.btnSupportTidy = new System.Windows.Forms.Button();
			this.lblSupportTidyNote = new System.Windows.Forms.Label();
			this.btnSupportRandom = new System.Windows.Forms.Button();
			this.tabAssistSource.SuspendLayout();
			this.tabAssistBlocks.SuspendLayout();
			this.pnlAssistBlocks.SuspendLayout();
			this.tabAssistMap.SuspendLayout();
			this.pnlAssistMap.SuspendLayout();
			this.grpAssistParts.SuspendLayout();
			this.grpAssistDetail.SuspendLayout();
			this.grpAssistSupport.SuspendLayout();
			this.SuspendLayout();
			//
			// lblAssistTarget
			//
			this.lblAssistTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAssistTarget.Location = new System.Drawing.Point(12, 8);
			this.lblAssistTarget.Name = "lblAssistTarget";
			this.lblAssistTarget.Size = new System.Drawing.Size(1336, 34);
			this.lblAssistTarget.TabIndex = 0;
			this.lblAssistTarget.Text = "対象: -";
			//
			// tabAssistSource
			//
			this.tabAssistSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.tabAssistSource.Controls.Add(this.tabAssistBlocks);
			this.tabAssistSource.Controls.Add(this.tabAssistMap);
			this.tabAssistSource.Location = new System.Drawing.Point(12, 46);
			this.tabAssistSource.Name = "tabAssistSource";
			this.tabAssistSource.SelectedIndex = 0;
			this.tabAssistSource.Size = new System.Drawing.Size(440, 592);
			this.tabAssistSource.TabIndex = 1;
			this.tabAssistSource.SelectedIndexChanged += new System.EventHandler(this.tabAssistSource_SelectedIndexChanged);
			//
			// tabAssistBlocks
			//
			this.tabAssistBlocks.Controls.Add(this.pnlAssistBlocks);
			this.tabAssistBlocks.Location = new System.Drawing.Point(4, 24);
			this.tabAssistBlocks.Name = "tabAssistBlocks";
			this.tabAssistBlocks.Size = new System.Drawing.Size(432, 564);
			this.tabAssistBlocks.TabIndex = 0;
			this.tabAssistBlocks.Text = "ブロック一覧";
			//
			// pnlAssistBlocks
			//
			this.pnlAssistBlocks.AutoScroll = true;
			this.pnlAssistBlocks.Controls.Add(this.canvasAssistBlocks);
			this.pnlAssistBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlAssistBlocks.Location = new System.Drawing.Point(0, 0);
			this.pnlAssistBlocks.Name = "pnlAssistBlocks";
			this.pnlAssistBlocks.Size = new System.Drawing.Size(432, 564);
			this.pnlAssistBlocks.TabIndex = 0;
			//
			// canvasAssistBlocks
			//
			this.canvasAssistBlocks.Location = new System.Drawing.Point(0, 0);
			this.canvasAssistBlocks.Name = "canvasAssistBlocks";
			this.canvasAssistBlocks.Size = new System.Drawing.Size(256, 256);
			this.canvasAssistBlocks.TabIndex = 0;
			//
			// tabAssistMap
			//
			this.tabAssistMap.Controls.Add(this.pnlAssistMap);
			this.tabAssistMap.Location = new System.Drawing.Point(4, 24);
			this.tabAssistMap.Name = "tabAssistMap";
			this.tabAssistMap.Size = new System.Drawing.Size(432, 564);
			this.tabAssistMap.TabIndex = 1;
			this.tabAssistMap.Text = "マップ";
			//
			// pnlAssistMap
			//
			this.pnlAssistMap.AutoScroll = true;
			this.pnlAssistMap.Controls.Add(this.canvasAssistMap);
			this.pnlAssistMap.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlAssistMap.Location = new System.Drawing.Point(0, 0);
			this.pnlAssistMap.Name = "pnlAssistMap";
			this.pnlAssistMap.Size = new System.Drawing.Size(432, 564);
			this.pnlAssistMap.TabIndex = 0;
			//
			// canvasAssistMap
			//
			this.canvasAssistMap.Location = new System.Drawing.Point(0, 0);
			this.canvasAssistMap.Name = "canvasAssistMap";
			this.canvasAssistMap.Size = new System.Drawing.Size(256, 256);
			this.canvasAssistMap.TabIndex = 0;
			//
			// lblAssistSelection
			//
			this.lblAssistSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblAssistSelection.Location = new System.Drawing.Point(12, 644);
			this.lblAssistSelection.Name = "lblAssistSelection";
			this.lblAssistSelection.Size = new System.Drawing.Size(300, 18);
			this.lblAssistSelection.TabIndex = 2;
			this.lblAssistSelection.Text = "選択: なし";
			//
			// chkAssistMapZoom
			//
			this.chkAssistMapZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkAssistMapZoom.AutoSize = true;
			this.chkAssistMapZoom.Checked = true;
			this.chkAssistMapZoom.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkAssistMapZoom.Location = new System.Drawing.Point(320, 642);
			this.chkAssistMapZoom.Name = "chkAssistMapZoom";
			this.chkAssistMapZoom.Size = new System.Drawing.Size(126, 19);
			this.chkAssistMapZoom.TabIndex = 3;
			this.chkAssistMapZoom.Text = "マップを 2 倍で表示";
			this.chkAssistMapZoom.UseVisualStyleBackColor = true;
			this.chkAssistMapZoom.CheckedChanged += new System.EventHandler(this.chkAssistMapZoom_CheckedChanged);
			//
			// grpAssistParts
			//
			this.grpAssistParts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.grpAssistParts.Controls.Add(this.lstAssistParts);
			this.grpAssistParts.Controls.Add(this.btnAssistAddArea);
			this.grpAssistParts.Controls.Add(this.btnAssistAddEdge);
			this.grpAssistParts.Controls.Add(this.btnAssistAddStamp);
			this.grpAssistParts.Controls.Add(this.btnAssistAddRepeat);
			this.grpAssistParts.Controls.Add(this.btnAssistDeletePart);
			this.grpAssistParts.Controls.Add(this.btnAssistAuto);
			this.grpAssistParts.Controls.Add(this.lblAssistPartsNote);
			this.grpAssistParts.Location = new System.Drawing.Point(464, 46);
			this.grpAssistParts.Name = "grpAssistParts";
			this.grpAssistParts.Size = new System.Drawing.Size(252, 592);
			this.grpAssistParts.TabIndex = 4;
			this.grpAssistParts.TabStop = false;
			this.grpAssistParts.Text = "パーツ";
			//
			// lstAssistParts
			//
			this.lstAssistParts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lstAssistParts.FormattingEnabled = true;
			this.lstAssistParts.IntegralHeight = false;
			this.lstAssistParts.ItemHeight = 15;
			this.lstAssistParts.Location = new System.Drawing.Point(10, 22);
			this.lstAssistParts.Name = "lstAssistParts";
			this.lstAssistParts.Size = new System.Drawing.Size(232, 330);
			this.lstAssistParts.TabIndex = 0;
			this.lstAssistParts.SelectedIndexChanged += new System.EventHandler(this.lstAssistParts_SelectedIndexChanged);
			//
			// btnAssistAddArea
			//
			this.btnAssistAddArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistAddArea.Location = new System.Drawing.Point(10, 360);
			this.btnAssistAddArea.Name = "btnAssistAddArea";
			this.btnAssistAddArea.Size = new System.Drawing.Size(113, 28);
			this.btnAssistAddArea.TabIndex = 1;
			this.btnAssistAddArea.Text = "＋ 面";
			this.btnAssistAddArea.UseVisualStyleBackColor = true;
			this.btnAssistAddArea.Click += new System.EventHandler(this.btnAssistAddArea_Click);
			//
			// btnAssistAddEdge
			//
			this.btnAssistAddEdge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistAddEdge.Location = new System.Drawing.Point(129, 360);
			this.btnAssistAddEdge.Name = "btnAssistAddEdge";
			this.btnAssistAddEdge.Size = new System.Drawing.Size(113, 28);
			this.btnAssistAddEdge.TabIndex = 2;
			this.btnAssistAddEdge.Text = "＋ 縁つき";
			this.btnAssistAddEdge.UseVisualStyleBackColor = true;
			this.btnAssistAddEdge.Click += new System.EventHandler(this.btnAssistAddEdge_Click);
			//
			// btnAssistAddStamp
			//
			this.btnAssistAddStamp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistAddStamp.Location = new System.Drawing.Point(10, 392);
			this.btnAssistAddStamp.Name = "btnAssistAddStamp";
			this.btnAssistAddStamp.Size = new System.Drawing.Size(113, 28);
			this.btnAssistAddStamp.TabIndex = 3;
			this.btnAssistAddStamp.Text = "＋ 部品";
			this.btnAssistAddStamp.UseVisualStyleBackColor = true;
			this.btnAssistAddStamp.Click += new System.EventHandler(this.btnAssistAddStamp_Click);
			//
			// btnAssistAddRepeat
			//
			this.btnAssistAddRepeat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistAddRepeat.Location = new System.Drawing.Point(129, 392);
			this.btnAssistAddRepeat.Name = "btnAssistAddRepeat";
			this.btnAssistAddRepeat.Size = new System.Drawing.Size(113, 28);
			this.btnAssistAddRepeat.TabIndex = 4;
			this.btnAssistAddRepeat.Text = "＋ くり返し";
			this.btnAssistAddRepeat.UseVisualStyleBackColor = true;
			this.btnAssistAddRepeat.Click += new System.EventHandler(this.btnAssistAddRepeat_Click);
			//
			// btnAssistDeletePart
			//
			this.btnAssistDeletePart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistDeletePart.Location = new System.Drawing.Point(10, 462);
			this.btnAssistDeletePart.Name = "btnAssistDeletePart";
			this.btnAssistDeletePart.Size = new System.Drawing.Size(232, 28);
			this.btnAssistDeletePart.TabIndex = 5;
			this.btnAssistDeletePart.Text = "選んだパーツを削除";
			this.btnAssistDeletePart.UseVisualStyleBackColor = true;
			this.btnAssistDeletePart.Click += new System.EventHandler(this.btnAssistDeletePart_Click);
			//
			// btnAssistAuto
			//
			this.btnAssistAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistAuto.Location = new System.Drawing.Point(10, 426);
			this.btnAssistAuto.Name = "btnAssistAuto";
			this.btnAssistAuto.Size = new System.Drawing.Size(232, 30);
			this.btnAssistAuto.TabIndex = 7;
			this.btnAssistAuto.Text = "マップから候補を作る（自動）";
			this.btnAssistAuto.UseVisualStyleBackColor = true;
			this.btnAssistAuto.Click += new System.EventHandler(this.btnAssistAuto_Click);
			//
			// lblAssistPartsNote
			//
			this.lblAssistPartsNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblAssistPartsNote.Location = new System.Drawing.Point(10, 498);
			this.lblAssistPartsNote.Name = "lblAssistPartsNote";
			this.lblAssistPartsNote.Size = new System.Drawing.Size(232, 86);
			this.lblAssistPartsNote.TabIndex = 6;
			this.lblAssistPartsNote.Text = "面 = 地面・草むらなど。縁つき = 水・道・崖など、境目のブロックがある地形。部品 = 家・大きい木など、決まった並び。くり返し = 森など、同じ並びを広げるもの。";
			//
			// grpAssistDetail
			//
			this.grpAssistDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.grpAssistDetail.Controls.Add(this.lblAssistName);
			this.grpAssistDetail.Controls.Add(this.txtAssistName);
			this.grpAssistDetail.Controls.Add(this.lblAssistRole);
			this.grpAssistDetail.Controls.Add(this.cmbAssistRole);
			this.grpAssistDetail.Controls.Add(this.lblAssistKindCaption);
			this.grpAssistDetail.Controls.Add(this.lblAssistKind);
			this.grpAssistDetail.Controls.Add(this.pnlAssistDetail);
			this.grpAssistDetail.Controls.Add(this.btnAssistAssign);
			this.grpAssistDetail.Controls.Add(this.btnAssistClear);
			this.grpAssistDetail.Controls.Add(this.lblAssistHint);
			this.grpAssistDetail.Location = new System.Drawing.Point(728, 46);
			this.grpAssistDetail.Name = "grpAssistDetail";
			this.grpAssistDetail.Size = new System.Drawing.Size(380, 592);
			this.grpAssistDetail.TabIndex = 5;
			this.grpAssistDetail.TabStop = false;
			this.grpAssistDetail.Text = "選んだパーツの中身";
			//
			// lblAssistName
			//
			this.lblAssistName.AutoSize = true;
			this.lblAssistName.Location = new System.Drawing.Point(12, 27);
			this.lblAssistName.Name = "lblAssistName";
			this.lblAssistName.Size = new System.Drawing.Size(40, 15);
			this.lblAssistName.TabIndex = 0;
			this.lblAssistName.Text = "名前 :";
			//
			// txtAssistName
			//
			this.txtAssistName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAssistName.Location = new System.Drawing.Point(70, 24);
			this.txtAssistName.MaxLength = 40;
			this.txtAssistName.Name = "txtAssistName";
			this.txtAssistName.Size = new System.Drawing.Size(298, 23);
			this.txtAssistName.TabIndex = 1;
			this.txtAssistName.TextChanged += new System.EventHandler(this.txtAssistName_TextChanged);
			//
			// lblAssistRole
			//
			this.lblAssistRole.AutoSize = true;
			this.lblAssistRole.Location = new System.Drawing.Point(12, 57);
			this.lblAssistRole.Name = "lblAssistRole";
			this.lblAssistRole.Size = new System.Drawing.Size(40, 15);
			this.lblAssistRole.TabIndex = 2;
			this.lblAssistRole.Text = "役割 :";
			//
			// cmbAssistRole
			//
			this.cmbAssistRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAssistRole.FormattingEnabled = true;
			this.cmbAssistRole.Location = new System.Drawing.Point(70, 54);
			this.cmbAssistRole.Name = "cmbAssistRole";
			this.cmbAssistRole.Size = new System.Drawing.Size(180, 23);
			this.cmbAssistRole.TabIndex = 3;
			this.cmbAssistRole.SelectedIndexChanged += new System.EventHandler(this.cmbAssistRole_SelectedIndexChanged);
			//
			// lblAssistKindCaption
			//
			this.lblAssistKindCaption.AutoSize = true;
			this.lblAssistKindCaption.Location = new System.Drawing.Point(12, 86);
			this.lblAssistKindCaption.Name = "lblAssistKindCaption";
			this.lblAssistKindCaption.Size = new System.Drawing.Size(40, 15);
			this.lblAssistKindCaption.TabIndex = 4;
			this.lblAssistKindCaption.Text = "種類 :";
			//
			// lblAssistKind
			//
			this.lblAssistKind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAssistKind.Location = new System.Drawing.Point(70, 86);
			this.lblAssistKind.Name = "lblAssistKind";
			this.lblAssistKind.Size = new System.Drawing.Size(298, 15);
			this.lblAssistKind.TabIndex = 5;
			this.lblAssistKind.Text = "-";
			//
			// pnlAssistDetail
			//
			this.pnlAssistDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlAssistDetail.AutoScroll = true;
			this.pnlAssistDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlAssistDetail.Location = new System.Drawing.Point(12, 110);
			this.pnlAssistDetail.Name = "pnlAssistDetail";
			this.pnlAssistDetail.Size = new System.Drawing.Size(356, 322);
			this.pnlAssistDetail.TabIndex = 6;
			this.pnlAssistDetail.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlAssistDetail_Paint);
			this.pnlAssistDetail.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlAssistDetail_MouseDown);
			this.pnlAssistDetail.Resize += new System.EventHandler(this.pnlAssistDetail_Resize);
			//
			// btnAssistAssign
			//
			this.btnAssistAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistAssign.Location = new System.Drawing.Point(12, 440);
			this.btnAssistAssign.Name = "btnAssistAssign";
			this.btnAssistAssign.Size = new System.Drawing.Size(210, 30);
			this.btnAssistAssign.TabIndex = 7;
			this.btnAssistAssign.Text = "← 左で選んだ所を入れる";
			this.btnAssistAssign.UseVisualStyleBackColor = true;
			this.btnAssistAssign.Click += new System.EventHandler(this.btnAssistAssign_Click);
			//
			// btnAssistClear
			//
			this.btnAssistClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAssistClear.Location = new System.Drawing.Point(230, 440);
			this.btnAssistClear.Name = "btnAssistClear";
			this.btnAssistClear.Size = new System.Drawing.Size(138, 30);
			this.btnAssistClear.TabIndex = 8;
			this.btnAssistClear.Text = "選んだ枠を空にする";
			this.btnAssistClear.UseVisualStyleBackColor = true;
			this.btnAssistClear.Click += new System.EventHandler(this.btnAssistClear_Click);
			//
			// lblAssistHint
			//
			this.lblAssistHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAssistHint.Location = new System.Drawing.Point(12, 478);
			this.lblAssistHint.Name = "lblAssistHint";
			this.lblAssistHint.Size = new System.Drawing.Size(356, 106);
			this.lblAssistHint.TabIndex = 9;
			//
			// lblAssistStatus
			//
			this.lblAssistStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAssistStatus.Location = new System.Drawing.Point(12, 668);
			this.lblAssistStatus.Name = "lblAssistStatus";
			this.lblAssistStatus.Size = new System.Drawing.Size(940, 34);
			this.lblAssistStatus.TabIndex = 6;
			//
			// btnAssistReload
			//
			this.btnAssistReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAssistReload.Location = new System.Drawing.Point(968, 666);
			this.btnAssistReload.Name = "btnAssistReload";
			this.btnAssistReload.Size = new System.Drawing.Size(160, 32);
			this.btnAssistReload.TabIndex = 7;
			this.btnAssistReload.Text = "今のマップを読み直す";
			this.btnAssistReload.UseVisualStyleBackColor = true;
			this.btnAssistReload.Click += new System.EventHandler(this.btnAssistReload_Click);
			//
			// btnAssistSave
			//
			this.btnAssistSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAssistSave.Location = new System.Drawing.Point(1136, 666);
			this.btnAssistSave.Name = "btnAssistSave";
			this.btnAssistSave.Size = new System.Drawing.Size(104, 32);
			this.btnAssistSave.TabIndex = 8;
			this.btnAssistSave.Text = "指定を保存";
			this.btnAssistSave.UseVisualStyleBackColor = true;
			this.btnAssistSave.Click += new System.EventHandler(this.btnAssistSave_Click);
			//
			// btnAssistClose
			//
			this.btnAssistClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAssistClose.Location = new System.Drawing.Point(1248, 666);
			this.btnAssistClose.Name = "btnAssistClose";
			this.btnAssistClose.Size = new System.Drawing.Size(100, 32);
			this.btnAssistClose.TabIndex = 9;
			this.btnAssistClose.Text = "閉じる";
			this.btnAssistClose.UseVisualStyleBackColor = true;
			this.btnAssistClose.Click += new System.EventHandler(this.btnAssistClose_Click);
			//
			// grpAssistSupport
			//
			this.grpAssistSupport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.grpAssistSupport.Controls.Add(this.lblSupportNote);
			this.grpAssistSupport.Controls.Add(this.btnSupportPaint);
			this.grpAssistSupport.Controls.Add(this.lblSupportPaintNote);
			this.grpAssistSupport.Controls.Add(this.chkSupportRepeat);
			this.grpAssistSupport.Controls.Add(this.chkSupportEdge);
			this.grpAssistSupport.Controls.Add(this.chkSupportCollision);
			this.grpAssistSupport.Controls.Add(this.btnSupportTidy);
			this.grpAssistSupport.Controls.Add(this.lblSupportTidyNote);
			this.grpAssistSupport.Controls.Add(this.btnSupportRandom);
			this.grpAssistSupport.Location = new System.Drawing.Point(1120, 46);
			this.grpAssistSupport.Name = "grpAssistSupport";
			this.grpAssistSupport.Size = new System.Drawing.Size(228, 592);
			this.grpAssistSupport.TabIndex = 10;
			this.grpAssistSupport.TabStop = false;
			this.grpAssistSupport.Text = "サポート作成・ランダム作成（試作）";
			//
			// lblSupportNote
			//
			this.lblSupportNote.Location = new System.Drawing.Point(12, 22);
			this.lblSupportNote.Name = "lblSupportNote";
			this.lblSupportNote.Size = new System.Drawing.Size(204, 92);
			this.lblSupportNote.TabIndex = 0;
			this.lblSupportNote.Text = "左の「マップ」タブで範囲を選んでから使います（整えるときは、選んでいなければマップ全体）。変更はメインの画面の「戻る」（Ctrl+Z）で元に戻せます。";
			//
			// btnSupportPaint
			//
			this.btnSupportPaint.Location = new System.Drawing.Point(12, 118);
			this.btnSupportPaint.Name = "btnSupportPaint";
			this.btnSupportPaint.Size = new System.Drawing.Size(204, 30);
			this.btnSupportPaint.TabIndex = 1;
			this.btnSupportPaint.Text = "選んだパーツで範囲を塗る";
			this.btnSupportPaint.UseVisualStyleBackColor = true;
			this.btnSupportPaint.Click += new System.EventHandler(this.btnSupportPaint_Click);
			//
			// lblSupportPaintNote
			//
			this.lblSupportPaintNote.Location = new System.Drawing.Point(12, 152);
			this.lblSupportPaintNote.Name = "lblSupportPaintNote";
			this.lblSupportPaintNote.Size = new System.Drawing.Size(204, 80);
			this.lblSupportPaintNote.TabIndex = 2;
			this.lblSupportPaintNote.Text = "面 = 右の欄で選んだブロック（選んでいなければ先頭）、縁つき = 中央で塗って縁を付ける、部品 = 範囲の左上に置く、くり返し = 範囲に並べる。";
			//
			// chkSupportRepeat
			//
			this.chkSupportRepeat.AutoSize = true;
			this.chkSupportRepeat.Checked = true;
			this.chkSupportRepeat.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkSupportRepeat.Location = new System.Drawing.Point(12, 240);
			this.chkSupportRepeat.Name = "chkSupportRepeat";
			this.chkSupportRepeat.Size = new System.Drawing.Size(170, 19);
			this.chkSupportRepeat.TabIndex = 3;
			this.chkSupportRepeat.Text = "くり返し（森・模様）を整える";
			this.chkSupportRepeat.UseVisualStyleBackColor = true;
			//
			// chkSupportEdge
			//
			this.chkSupportEdge.AutoSize = true;
			this.chkSupportEdge.Checked = true;
			this.chkSupportEdge.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkSupportEdge.Location = new System.Drawing.Point(12, 264);
			this.chkSupportEdge.Name = "chkSupportEdge";
			this.chkSupportEdge.Size = new System.Drawing.Size(132, 19);
			this.chkSupportEdge.TabIndex = 4;
			this.chkSupportEdge.Text = "縁つきの縁を整える";
			this.chkSupportEdge.UseVisualStyleBackColor = true;
			//
			// chkSupportCollision
			//
			this.chkSupportCollision.AutoSize = true;
			this.chkSupportCollision.Checked = true;
			this.chkSupportCollision.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkSupportCollision.Location = new System.Drawing.Point(12, 288);
			this.chkSupportCollision.Name = "chkSupportCollision";
			this.chkSupportCollision.Size = new System.Drawing.Size(122, 19);
			this.chkSupportCollision.TabIndex = 5;
			this.chkSupportCollision.Text = "移動エリアを付ける";
			this.chkSupportCollision.UseVisualStyleBackColor = true;
			//
			// btnSupportTidy
			//
			this.btnSupportTidy.Location = new System.Drawing.Point(12, 316);
			this.btnSupportTidy.Name = "btnSupportTidy";
			this.btnSupportTidy.Size = new System.Drawing.Size(204, 30);
			this.btnSupportTidy.TabIndex = 6;
			this.btnSupportTidy.Text = "範囲を整える";
			this.btnSupportTidy.UseVisualStyleBackColor = true;
			this.btnSupportTidy.Click += new System.EventHandler(this.btnSupportTidy_Click);
			//
			// lblSupportTidyNote
			//
			this.lblSupportTidyNote.Location = new System.Drawing.Point(12, 352);
			this.lblSupportTidyNote.Name = "lblSupportTidyNote";
			this.lblSupportTidyNote.Size = new System.Drawing.Size(204, 160);
			this.lblSupportTidyNote.TabIndex = 7;
			this.lblSupportTidyNote.Text = "整える: パーツのブロックがつながった所ごとに、縁つきは辺・角のブロックへ置き換え、くり返しは並びのずれを直します（上端・下端は塗るときに付きます）。移動エリアは、パーツで決めた値（無ければ ROM のマップでいちばん多い値）を付けます。ROM へは、今までどおり「確定」で書き込みます。";
			//
			// btnSupportRandom
			//
			this.btnSupportRandom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSupportRandom.Location = new System.Drawing.Point(12, 550);
			this.btnSupportRandom.Name = "btnSupportRandom";
			this.btnSupportRandom.Size = new System.Drawing.Size(204, 30);
			this.btnSupportRandom.TabIndex = 8;
			this.btnSupportRandom.Text = "ランダム作成…（ゼロから作る）";
			this.btnSupportRandom.UseVisualStyleBackColor = true;
			this.btnSupportRandom.Click += new System.EventHandler(this.btnSupportRandom_Click);
			//
			// MapAssistForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(1360, 710);
			this.Controls.Add(this.lblAssistTarget);
			this.Controls.Add(this.tabAssistSource);
			this.Controls.Add(this.lblAssistSelection);
			this.Controls.Add(this.chkAssistMapZoom);
			this.Controls.Add(this.grpAssistParts);
			this.Controls.Add(this.grpAssistDetail);
			this.Controls.Add(this.grpAssistSupport);
			this.Controls.Add(this.lblAssistStatus);
			this.Controls.Add(this.btnAssistReload);
			this.Controls.Add(this.btnAssistSave);
			this.Controls.Add(this.btnAssistClose);
			this.MinimumSize = new System.Drawing.Size(1240, 560);
			this.Name = "MapAssistForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "マップ作成補助";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapAssistForm_FormClosing);
			this.tabAssistSource.ResumeLayout(false);
			this.tabAssistBlocks.ResumeLayout(false);
			this.pnlAssistBlocks.ResumeLayout(false);
			this.tabAssistMap.ResumeLayout(false);
			this.pnlAssistMap.ResumeLayout(false);
			this.grpAssistParts.ResumeLayout(false);
			this.grpAssistDetail.ResumeLayout(false);
			this.grpAssistDetail.PerformLayout();
			this.grpAssistSupport.ResumeLayout(false);
			this.grpAssistSupport.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblAssistTarget;
		private BochiBochiEditor.ThemedTabControl tabAssistSource;
		private System.Windows.Forms.TabPage tabAssistBlocks;
		private BochiBochiEditor.NoFocusScrollPanel pnlAssistBlocks;
		private BochiBochiEditor.MapAssistCanvas canvasAssistBlocks;
		private System.Windows.Forms.TabPage tabAssistMap;
		private BochiBochiEditor.NoFocusScrollPanel pnlAssistMap;
		private BochiBochiEditor.MapAssistCanvas canvasAssistMap;
		private System.Windows.Forms.Label lblAssistSelection;
		private System.Windows.Forms.CheckBox chkAssistMapZoom;
		private System.Windows.Forms.GroupBox grpAssistParts;
		private System.Windows.Forms.ListBox lstAssistParts;
		private System.Windows.Forms.Button btnAssistAddArea;
		private System.Windows.Forms.Button btnAssistAddEdge;
		private System.Windows.Forms.Button btnAssistAddStamp;
		private System.Windows.Forms.Button btnAssistAddRepeat;
		private System.Windows.Forms.Button btnAssistDeletePart;
		private System.Windows.Forms.Button btnAssistAuto;
		private System.Windows.Forms.Label lblAssistPartsNote;
		private System.Windows.Forms.GroupBox grpAssistDetail;
		private System.Windows.Forms.Label lblAssistName;
		private System.Windows.Forms.TextBox txtAssistName;
		private System.Windows.Forms.Label lblAssistRole;
		private System.Windows.Forms.ComboBox cmbAssistRole;
		private System.Windows.Forms.Label lblAssistKindCaption;
		private System.Windows.Forms.Label lblAssistKind;
		private BochiBochiEditor.NoFocusScrollPanel pnlAssistDetail;
		private System.Windows.Forms.Button btnAssistAssign;
		private System.Windows.Forms.Button btnAssistClear;
		private System.Windows.Forms.Label lblAssistHint;
		private System.Windows.Forms.Label lblAssistStatus;
		private System.Windows.Forms.Button btnAssistReload;
		private System.Windows.Forms.Button btnAssistSave;
		private System.Windows.Forms.Button btnAssistClose;
		private System.Windows.Forms.GroupBox grpAssistSupport;
		private System.Windows.Forms.Label lblSupportNote;
		private System.Windows.Forms.Button btnSupportPaint;
		private System.Windows.Forms.Label lblSupportPaintNote;
		private System.Windows.Forms.CheckBox chkSupportRepeat;
		private System.Windows.Forms.CheckBox chkSupportEdge;
		private System.Windows.Forms.CheckBox chkSupportCollision;
		private System.Windows.Forms.Button btnSupportTidy;
		private System.Windows.Forms.Label lblSupportTidyNote;
		private System.Windows.Forms.Button btnSupportRandom;
	}
}
