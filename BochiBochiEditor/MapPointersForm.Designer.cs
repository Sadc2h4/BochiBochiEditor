namespace BochiBochiEditor
{
	partial class MapPointersForm
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
			this.lblMapPointersSummary = new System.Windows.Forms.Label();
			this.lblMapPointersResult = new System.Windows.Forms.Label();
			this.dgvMapPointers = new System.Windows.Forms.DataGridView();
			this.colGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colBytes = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.chkPointersOnlyEvents = new System.Windows.Forms.CheckBox();
			this.btnCopyLocation = new System.Windows.Forms.Button();
			this.btnCopyValue = new System.Windows.Forms.Button();
			this.btnCopyBytes = new System.Windows.Forms.Button();
			this.btnCopyRow = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.mapPointersContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuCopyLocation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuCopyValue = new System.Windows.Forms.ToolStripMenuItem();
			this.menuCopyBytes = new System.Windows.Forms.ToolStripMenuItem();
			this.menuCopyRow = new System.Windows.Forms.ToolStripMenuItem();
			this.grpFindReferences = new System.Windows.Forms.GroupBox();
			this.lblFindAddress = new System.Windows.Forms.Label();
			this.txtFindAddress = new System.Windows.Forms.TextBox();
			this.lblFindBytes = new System.Windows.Forms.Label();
			this.btnFindReferences = new System.Windows.Forms.Button();
			this.btnFindSelected = new System.Windows.Forms.Button();
			this.btnCopyFindBytes = new System.Windows.Forms.Button();
			this.dgvReferences = new System.Windows.Forms.DataGridView();
			this.colReferenceLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colReferenceDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblFindResult = new System.Windows.Forms.Label();
			this.referencesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuCopyReferenceLocation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuCopyReferenceRow = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.dgvMapPointers)).BeginInit();
			this.mapPointersContextMenu.SuspendLayout();
			this.grpFindReferences.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvReferences)).BeginInit();
			this.referencesContextMenu.SuspendLayout();
			this.SuspendLayout();
			//
			// lblMapPointersSummary
			//
			this.lblMapPointersSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.lblMapPointersSummary.Location = new System.Drawing.Point(10, 10);
			this.lblMapPointersSummary.Name = "lblMapPointersSummary";
			this.lblMapPointersSummary.Size = new System.Drawing.Size(740, 44);
			this.lblMapPointersSummary.TabIndex = 0;
			this.lblMapPointersSummary.Text = "このマップのポインタ一覧";
			//
			// lblMapPointersResult
			//
			this.lblMapPointersResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.lblMapPointersResult.AutoEllipsis = true;
			this.lblMapPointersResult.Location = new System.Drawing.Point(10, 54);
			this.lblMapPointersResult.Name = "lblMapPointersResult";
			this.lblMapPointersResult.Size = new System.Drawing.Size(740, 26);
			this.lblMapPointersResult.TabIndex = 1;
			this.lblMapPointersResult.Text = "薄い文字の値はポインタではありません。";
			//
			// dgvMapPointers
			//
			this.dgvMapPointers.AllowUserToAddRows = false;
			this.dgvMapPointers.AllowUserToDeleteRows = false;
			this.dgvMapPointers.AllowUserToResizeRows = false;
			this.dgvMapPointers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.dgvMapPointers.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvMapPointers.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dgvMapPointers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMapPointers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
				this.colGroup,
				this.colName,
				this.colLocation,
				this.colValue,
				this.colBytes});
			this.dgvMapPointers.ContextMenuStrip = this.mapPointersContextMenu;
			this.dgvMapPointers.Location = new System.Drawing.Point(10, 80);
			this.dgvMapPointers.MultiSelect = true;
			this.dgvMapPointers.Name = "dgvMapPointers";
			this.dgvMapPointers.ReadOnly = true;
			this.dgvMapPointers.RowHeadersVisible = false;
			this.dgvMapPointers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvMapPointers.Size = new System.Drawing.Size(740, 280);
			this.dgvMapPointers.TabIndex = 2;
			this.dgvMapPointers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMapPointers_CellDoubleClick);
			this.dgvMapPointers.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvMapPointers_CellMouseDown);
			//
			// colGroup
			//
			this.colGroup.HeaderText = "分類";
			this.colGroup.Name = "colGroup";
			this.colGroup.ReadOnly = true;
			this.colGroup.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colGroup.Width = 100;
			//
			// colName
			//
			this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colName.HeaderText = "項目";
			this.colName.MinimumWidth = 200;
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			this.colName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			//
			// colLocation
			//
			this.colLocation.HeaderText = "置き場所";
			this.colLocation.Name = "colLocation";
			this.colLocation.ReadOnly = true;
			this.colLocation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colLocation.Width = 90;
			//
			// colValue
			//
			this.colValue.HeaderText = "値";
			this.colValue.Name = "colValue";
			this.colValue.ReadOnly = true;
			this.colValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colValue.Width = 90;
			//
			// colBytes
			//
			this.colBytes.HeaderText = "バイト列（リトルエンディアン）";
			this.colBytes.Name = "colBytes";
			this.colBytes.ReadOnly = true;
			this.colBytes.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colBytes.Width = 180;
			//
			// chkPointersOnlyEvents
			//
			this.chkPointersOnlyEvents.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
			this.chkPointersOnlyEvents.AutoSize = true;
			this.chkPointersOnlyEvents.Location = new System.Drawing.Point(10, 372);
			this.chkPointersOnlyEvents.Name = "chkPointersOnlyEvents";
			this.chkPointersOnlyEvents.Size = new System.Drawing.Size(260, 16);
			this.chkPointersOnlyEvents.TabIndex = 3;
			this.chkPointersOnlyEvents.Text = "イベントとマップスクリプトだけ";
			this.chkPointersOnlyEvents.UseVisualStyleBackColor = true;
			this.chkPointersOnlyEvents.CheckedChanged += new System.EventHandler(this.chkPointersOnlyEvents_CheckedChanged);
			//
			// btnCopyLocation
			//
			this.btnCopyLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
			this.btnCopyLocation.Location = new System.Drawing.Point(10, 400);
			this.btnCopyLocation.Name = "btnCopyLocation";
			this.btnCopyLocation.Size = new System.Drawing.Size(134, 32);
			this.btnCopyLocation.TabIndex = 4;
			this.btnCopyLocation.Text = "置き場所をコピー";
			this.btnCopyLocation.UseVisualStyleBackColor = true;
			this.btnCopyLocation.Click += new System.EventHandler(this.btnCopyLocation_Click);
			//
			// btnCopyValue
			//
			this.btnCopyValue.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
			this.btnCopyValue.Location = new System.Drawing.Point(152, 400);
			this.btnCopyValue.Name = "btnCopyValue";
			this.btnCopyValue.Size = new System.Drawing.Size(110, 32);
			this.btnCopyValue.TabIndex = 5;
			this.btnCopyValue.Text = "値をコピー";
			this.btnCopyValue.UseVisualStyleBackColor = true;
			this.btnCopyValue.Click += new System.EventHandler(this.btnCopyValue_Click);
			//
			// btnCopyBytes
			//
			this.btnCopyBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
			this.btnCopyBytes.Location = new System.Drawing.Point(270, 400);
			this.btnCopyBytes.Name = "btnCopyBytes";
			this.btnCopyBytes.Size = new System.Drawing.Size(134, 32);
			this.btnCopyBytes.TabIndex = 6;
			this.btnCopyBytes.Text = "バイト列をコピー";
			this.btnCopyBytes.UseVisualStyleBackColor = true;
			this.btnCopyBytes.Click += new System.EventHandler(this.btnCopyBytes_Click);
			//
			// btnCopyRow
			//
			this.btnCopyRow.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
			this.btnCopyRow.Location = new System.Drawing.Point(412, 400);
			this.btnCopyRow.Name = "btnCopyRow";
			this.btnCopyRow.Size = new System.Drawing.Size(110, 32);
			this.btnCopyRow.TabIndex = 7;
			this.btnCopyRow.Text = "行をコピー";
			this.btnCopyRow.UseVisualStyleBackColor = true;
			this.btnCopyRow.Click += new System.EventHandler(this.btnCopyRow_Click);
			//
			// btnClose
			//
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = new System.Drawing.Point(654, 400);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(96, 32);
			this.btnClose.TabIndex = 8;
			this.btnClose.Text = "閉じる";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			//
			// mapPointersContextMenu
			//
			this.mapPointersContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.menuCopyLocation,
				this.menuCopyValue,
				this.menuCopyBytes,
				this.menuCopyRow});
			this.mapPointersContextMenu.Name = "mapPointersContextMenu";
			//
			// menuCopyLocation
			//
			this.menuCopyLocation.Name = "menuCopyLocation";
			this.menuCopyLocation.Text = "置き場所をコピー";
			this.menuCopyLocation.Click += new System.EventHandler(this.btnCopyLocation_Click);
			//
			// menuCopyValue
			//
			this.menuCopyValue.Name = "menuCopyValue";
			this.menuCopyValue.Text = "値をコピー";
			this.menuCopyValue.Click += new System.EventHandler(this.btnCopyValue_Click);
			//
			// menuCopyBytes
			//
			this.menuCopyBytes.Name = "menuCopyBytes";
			this.menuCopyBytes.Text = "バイト列をコピー";
			this.menuCopyBytes.Click += new System.EventHandler(this.btnCopyBytes_Click);
			//
			// menuCopyRow
			//
			this.menuCopyRow.Name = "menuCopyRow";
			this.menuCopyRow.Text = "行をコピー";
			this.menuCopyRow.Click += new System.EventHandler(this.btnCopyRow_Click);
			//
			// grpFindReferences
			//
			this.grpFindReferences.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.grpFindReferences.Controls.Add(this.lblFindAddress);
			this.grpFindReferences.Controls.Add(this.txtFindAddress);
			this.grpFindReferences.Controls.Add(this.lblFindBytes);
			this.grpFindReferences.Controls.Add(this.btnFindReferences);
			this.grpFindReferences.Controls.Add(this.btnFindSelected);
			this.grpFindReferences.Controls.Add(this.btnCopyFindBytes);
			this.grpFindReferences.Controls.Add(this.dgvReferences);
			this.grpFindReferences.Controls.Add(this.lblFindResult);
			this.grpFindReferences.Location = new System.Drawing.Point(10, 440);
			this.grpFindReferences.Name = "grpFindReferences";
			this.grpFindReferences.Size = new System.Drawing.Size(740, 250);
			this.grpFindReferences.TabIndex = 9;
			this.grpFindReferences.TabStop = false;
			this.grpFindReferences.Text = "アドレスの参照元を探す";
			//
			// lblFindAddress
			//
			this.lblFindAddress.AutoSize = true;
			this.lblFindAddress.Location = new System.Drawing.Point(10, 28);
			this.lblFindAddress.Name = "lblFindAddress";
			this.lblFindAddress.TabIndex = 0;
			this.lblFindAddress.Text = "アドレス :";
			//
			// txtFindAddress
			//
			this.txtFindAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.txtFindAddress.Location = new System.Drawing.Point(104, 24);
			this.txtFindAddress.Name = "txtFindAddress";
			this.txtFindAddress.PlaceholderText = "08123456 / 123456 / 56 34 12 08";
			this.txtFindAddress.Size = new System.Drawing.Size(326, 19);
			this.txtFindAddress.TabIndex = 1;
			this.txtFindAddress.TextChanged += new System.EventHandler(this.txtFindAddress_TextChanged);
			//
			// lblFindBytes
			//
			this.lblFindBytes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.lblFindBytes.Location = new System.Drawing.Point(10, 64);
			this.lblFindBytes.Name = "lblFindBytes";
			this.lblFindBytes.Size = new System.Drawing.Size(420, 22);
			this.lblFindBytes.TabIndex = 4;
			this.lblFindBytes.Text = "バイト列 : -";
			//
			// btnFindReferences
			//
			this.btnFindReferences.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.btnFindReferences.Location = new System.Drawing.Point(440, 20);
			this.btnFindReferences.Name = "btnFindReferences";
			this.btnFindReferences.Size = new System.Drawing.Size(140, 30);
			this.btnFindReferences.TabIndex = 2;
			this.btnFindReferences.Text = "参照元を探す";
			this.btnFindReferences.UseVisualStyleBackColor = true;
			this.btnFindReferences.Click += new System.EventHandler(this.btnFindReferences_Click);
			//
			// btnFindSelected
			//
			this.btnFindSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.btnFindSelected.Location = new System.Drawing.Point(440, 57);
			this.btnFindSelected.Name = "btnFindSelected";
			this.btnFindSelected.Size = new System.Drawing.Size(290, 30);
			this.btnFindSelected.TabIndex = 5;
			this.btnFindSelected.Text = "選んだ行の値で探す";
			this.btnFindSelected.UseVisualStyleBackColor = true;
			this.btnFindSelected.Click += new System.EventHandler(this.btnFindSelected_Click);
			//
			// btnCopyFindBytes
			//
			this.btnCopyFindBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			this.btnCopyFindBytes.Location = new System.Drawing.Point(588, 20);
			this.btnCopyFindBytes.Name = "btnCopyFindBytes";
			this.btnCopyFindBytes.Size = new System.Drawing.Size(142, 30);
			this.btnCopyFindBytes.TabIndex = 3;
			this.btnCopyFindBytes.Text = "バイト列をコピー";
			this.btnCopyFindBytes.UseVisualStyleBackColor = true;
			this.btnCopyFindBytes.Click += new System.EventHandler(this.btnCopyFindBytes_Click);
			//
			// dgvReferences
			//
			this.dgvReferences.AllowUserToAddRows = false;
			this.dgvReferences.AllowUserToDeleteRows = false;
			this.dgvReferences.AllowUserToResizeRows = false;
			this.dgvReferences.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.dgvReferences.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvReferences.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dgvReferences.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvReferences.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
				this.colReferenceLocation,
				this.colReferenceDescription});
			this.dgvReferences.ContextMenuStrip = this.referencesContextMenu;
			this.dgvReferences.Location = new System.Drawing.Point(10, 96);
			this.dgvReferences.MultiSelect = true;
			this.dgvReferences.Name = "dgvReferences";
			this.dgvReferences.ReadOnly = true;
			this.dgvReferences.RowHeadersVisible = false;
			this.dgvReferences.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvReferences.Size = new System.Drawing.Size(720, 120);
			this.dgvReferences.TabIndex = 6;
			this.dgvReferences.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvReferences_CellDoubleClick);
			this.dgvReferences.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvMapPointers_CellMouseDown);
			//
			// colReferenceLocation
			//
			this.colReferenceLocation.HeaderText = "置き場所";
			this.colReferenceLocation.Name = "colReferenceLocation";
			this.colReferenceLocation.ReadOnly = true;
			this.colReferenceLocation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colReferenceLocation.Width = 100;
			//
			// colReferenceDescription
			//
			this.colReferenceDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colReferenceDescription.HeaderText = "説明";
			this.colReferenceDescription.MinimumWidth = 200;
			this.colReferenceDescription.Name = "colReferenceDescription";
			this.colReferenceDescription.ReadOnly = true;
			this.colReferenceDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			//
			// lblFindResult
			//
			this.lblFindResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			this.lblFindResult.AutoEllipsis = true;
			this.lblFindResult.Location = new System.Drawing.Point(10, 222);
			this.lblFindResult.Name = "lblFindResult";
			this.lblFindResult.Size = new System.Drawing.Size(720, 22);
			this.lblFindResult.TabIndex = 7;
			//
			// referencesContextMenu
			//
			this.referencesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.menuCopyReferenceLocation,
				this.menuCopyReferenceRow});
			this.referencesContextMenu.Name = "referencesContextMenu";
			//
			// menuCopyReferenceLocation
			//
			this.menuCopyReferenceLocation.Name = "menuCopyReferenceLocation";
			this.menuCopyReferenceLocation.Text = "置き場所をコピー";
			this.menuCopyReferenceLocation.Click += new System.EventHandler(this.menuCopyReferenceLocation_Click);
			//
			// menuCopyReferenceRow
			//
			this.menuCopyReferenceRow.Name = "menuCopyReferenceRow";
			this.menuCopyReferenceRow.Text = "行をコピー";
			this.menuCopyReferenceRow.Click += new System.EventHandler(this.menuCopyReferenceRow_Click);
			//
			// MapPointersForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AcceptButton = this.btnFindReferences;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(760, 700);
			this.Controls.Add(this.lblMapPointersSummary);
			this.Controls.Add(this.lblMapPointersResult);
			this.Controls.Add(this.dgvMapPointers);
			this.Controls.Add(this.chkPointersOnlyEvents);
			this.Controls.Add(this.btnCopyLocation);
			this.Controls.Add(this.btnCopyValue);
			this.Controls.Add(this.btnCopyBytes);
			this.Controls.Add(this.btnCopyRow);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.grpFindReferences);
			this.MinimumSize = new System.Drawing.Size(776, 650);
			this.Name = "MapPointersForm";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "このマップのポインタ一覧";
			((System.ComponentModel.ISupportInitialize)(this.dgvMapPointers)).EndInit();
			this.mapPointersContextMenu.ResumeLayout(false);
			this.grpFindReferences.ResumeLayout(false);
			this.grpFindReferences.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvReferences)).EndInit();
			this.referencesContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label lblMapPointersSummary;
		private System.Windows.Forms.Label lblMapPointersResult;
		private System.Windows.Forms.DataGridView dgvMapPointers;
		private System.Windows.Forms.DataGridViewTextBoxColumn colGroup;
		private System.Windows.Forms.DataGridViewTextBoxColumn colName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colLocation;
		private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
		private System.Windows.Forms.DataGridViewTextBoxColumn colBytes;
		private System.Windows.Forms.CheckBox chkPointersOnlyEvents;
		private System.Windows.Forms.Button btnCopyLocation;
		private System.Windows.Forms.Button btnCopyValue;
		private System.Windows.Forms.Button btnCopyBytes;
		private System.Windows.Forms.Button btnCopyRow;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.ContextMenuStrip mapPointersContextMenu;
		private System.Windows.Forms.ToolStripMenuItem menuCopyLocation;
		private System.Windows.Forms.ToolStripMenuItem menuCopyValue;
		private System.Windows.Forms.ToolStripMenuItem menuCopyBytes;
		private System.Windows.Forms.ToolStripMenuItem menuCopyRow;
		private System.Windows.Forms.GroupBox grpFindReferences;
		private System.Windows.Forms.Label lblFindAddress;
		private System.Windows.Forms.TextBox txtFindAddress;
		private System.Windows.Forms.Label lblFindBytes;
		private System.Windows.Forms.Button btnFindReferences;
		private System.Windows.Forms.Button btnFindSelected;
		private System.Windows.Forms.Button btnCopyFindBytes;
		private System.Windows.Forms.DataGridView dgvReferences;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReferenceLocation;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReferenceDescription;
		private System.Windows.Forms.Label lblFindResult;
		private System.Windows.Forms.ContextMenuStrip referencesContextMenu;
		private System.Windows.Forms.ToolStripMenuItem menuCopyReferenceLocation;
		private System.Windows.Forms.ToolStripMenuItem menuCopyReferenceRow;
	}
}
