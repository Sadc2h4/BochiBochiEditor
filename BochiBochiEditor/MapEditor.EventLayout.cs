using System;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	public partial class MapEditor
	{
		//-------------------------------------------------------------------------------
		// イベント欄の右端の入力部品を、枠の幅に合わせて伸縮させる処理
		//-------------------------------------------------------------------------------
		private void InitializeEventTabLayout()
		{
			GroupBox[] groups = { this.grpPersonEvent, this.grpWarpEvent, this.grpSignEvent, this.grpTrapScriptEvent };
			int minimumWidth = this.grpPersonEvent.Width;
			int margin = this.grpPersonEvent.Left;
			int gap = this.nudEventNo.Left - this.cmbEventType.Right;
			foreach (GroupBox group in groups)
			{
				int right = 0;
				foreach (Control control in group.Controls)
				{
					if (!(control is Label)) right = Math.Max(right, control.Right);
				}
				foreach (Control control in group.Controls)
				{
					control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
					if (!(control is Label) && control.Right >= right - 2) control.Anchor |= AnchorStyles.Right;
				}
				group.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			}
			// タブ直下の部品は、上限幅を守るためリサイズ時にまとめて調整する。
			this.cmbEventType.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			this.nudEventNo.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			this.btnEventPointers.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			this.btnEventAdd.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			this.btnEventDelete.Anchor = AnchorStyles.Top | AnchorStyles.Left;

			//-------------------------------------------------------------------------------
			// 設計時の幅を下限、DPI 換算した 420 を上限にしてイベント欄の幅を揃える処理
			//-------------------------------------------------------------------------------
			void UpdateEventTabWidths()
			{
				int width = Math.Min(this.LogicalToDeviceUnits(420), Math.Max(minimumWidth, this.tabEvent.ClientSize.Width - margin * 2));
				foreach (GroupBox group in groups)
				{
					if (group.Width != width) group.Width = width;
				}
				int numberLeft = this.grpPersonEvent.Left + width - this.nudEventNo.Width;
				int typeWidth = width - this.nudEventNo.Width - gap;
				if (this.nudEventNo.Left != numberLeft) this.nudEventNo.Left = numberLeft;
				if (this.cmbEventType.Width != typeWidth) this.cmbEventType.Width = typeWidth;
				if (this.btnEventPointers.Width != width) this.btnEventPointers.Width = width;
				// 追加・削除のボタンは、幅を半分ずつにする
				int half = (width - gap) / 2;
				if (this.btnEventAdd.Width != half) this.btnEventAdd.Width = half;
				if (this.btnEventDelete.Left != this.btnEventAdd.Right + gap) this.btnEventDelete.Left = this.btnEventAdd.Right + gap;
				if (this.btnEventDelete.Width != width - half - gap) this.btnEventDelete.Width = width - half - gap;
				int top = this.btnEventPointers.Bottom + this.LogicalToDeviceUnits(4);
				foreach (GroupBox group in groups)
				{
					if (group.Top != top) group.Top = top;
				}
			}

			this.tabEvent.ClientSizeChanged += (sender, e) => UpdateEventTabWidths();
			this.btnEventPointers.SizeChanged += (sender, e) => UpdateEventTabWidths();
			UpdateEventTabWidths();
		}
	}
}
