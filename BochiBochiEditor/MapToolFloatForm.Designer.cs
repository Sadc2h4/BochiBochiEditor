namespace BochiBochiEditor
{
	partial class MapToolFloatForm
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
			this.pnlContentHost = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			//
			// pnlContentHost
			//
			this.pnlContentHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlContentHost.Location = new System.Drawing.Point(0, 0);
			this.pnlContentHost.Name = "pnlContentHost";
			this.pnlContentHost.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.pnlContentHost.Size = new System.Drawing.Size(360, 720);
			this.pnlContentHost.TabIndex = 0;
			//
			// MapToolFloatForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(360, 720);
			this.Controls.Add(this.pnlContentHost);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 360);
			this.Name = "MapToolFloatForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "マップ編集ツール";
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel pnlContentHost;
	}
}
