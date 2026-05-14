namespace BochiBochiEditor
{
	// Token: 0x02000013 RID: 19
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated]
	public partial class InsertNewLevelMoveList : global::System.Windows.Forms.Form
	{
		// Token: 0x060002D6 RID: 726 RVA: 0x000169D0 File Offset: 0x00014BD0
		[global::System.Diagnostics.DebuggerNonUserCode]
		protected override void Dispose(bool disposing)
		{
			try
			{
				bool flag = disposing && this.components != null;
				if (flag)
				{
					this.components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00016A20 File Offset: 0x00014C20
		[global::System.Diagnostics.DebuggerStepThrough]
		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::BochiBochiEditor.InsertNewLevelMoveList));
			this.txtNewAddress = new global::System.Windows.Forms.TextBox();
			this.lblNewAddress = new global::System.Windows.Forms.Label();
			this.lblNewNumMove = new global::System.Windows.Forms.Label();
			this.nudNewNumMove = new global::System.Windows.Forms.NumericUpDown();
			this.btnNewLevelMoveList = new global::System.Windows.Forms.Button();
			((global::System.ComponentModel.ISupportInitialize)this.nudNewNumMove).BeginInit();
			base.SuspendLayout();
			this.txtNewAddress.Location = new global::System.Drawing.Point(110, 15);
			this.txtNewAddress.Name = "txtNewAddress";
			this.txtNewAddress.Size = new global::System.Drawing.Size(100, 19);
			this.txtNewAddress.TabIndex = 0;
			this.txtNewAddress.Text = "00000000";
			this.lblNewAddress.AutoSize = true;
			this.lblNewAddress.Location = new global::System.Drawing.Point(16, 20);
			this.lblNewAddress.Name = "lblNewAddress";
			this.lblNewAddress.Size = new global::System.Drawing.Size(83, 12);
			this.lblNewAddress.TabIndex = 1;
			this.lblNewAddress.Text = "生成先アドレス :";
			this.lblNewNumMove.AutoSize = true;
			this.lblNewNumMove.Location = new global::System.Drawing.Point(16, 46);
			this.lblNewNumMove.Name = "lblNewNumMove";
			this.lblNewNumMove.Size = new global::System.Drawing.Size(35, 12);
			this.lblNewNumMove.TabIndex = 2;
			this.lblNewNumMove.Text = "技数 :";
			this.nudNewNumMove.Location = new global::System.Drawing.Point(110, 42);
			global::System.Windows.Forms.NumericUpDown nudNewNumMove = this.nudNewNumMove;
			int[] array = new int[4];
			array[0] = 64;
			nudNewNumMove.Maximum = new decimal(array);
			global::System.Windows.Forms.NumericUpDown nudNewNumMove2 = this.nudNewNumMove;
			int[] array2 = new int[4];
			array2[0] = 1;
			nudNewNumMove2.Minimum = new decimal(array2);
			this.nudNewNumMove.Name = "nudNewNumMove";
			this.nudNewNumMove.Size = new global::System.Drawing.Size(48, 19);
			this.nudNewNumMove.TabIndex = 3;
			global::System.Windows.Forms.NumericUpDown nudNewNumMove3 = this.nudNewNumMove;
			int[] array3 = new int[4];
			array3[0] = 1;
			nudNewNumMove3.Value = new decimal(array3);
			this.btnNewLevelMoveList.Location = new global::System.Drawing.Point(228, 40);
			this.btnNewLevelMoveList.Name = "btnNewLevelMoveList";
			this.btnNewLevelMoveList.Size = new global::System.Drawing.Size(76, 23);
			this.btnNewLevelMoveList.TabIndex = 4;
			this.btnNewLevelMoveList.Text = "生成";
			this.btnNewLevelMoveList.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(324, 81);
			base.Controls.Add(this.btnNewLevelMoveList);
			base.Controls.Add(this.nudNewNumMove);
			base.Controls.Add(this.lblNewNumMove);
			base.Controls.Add(this.lblNewAddress);
			base.Controls.Add(this.txtNewAddress);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (global::System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "InsertNewLevelMoveList";
			this.Text = "新しいレベル技リストを生成";
			((global::System.ComponentModel.ISupportInitialize)this.nudNewNumMove).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000185 RID: 389
				private Button _btnNewLevelMoveList;
        private global::System.ComponentModel.IContainer components;
	}
}
