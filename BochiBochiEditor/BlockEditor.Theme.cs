using System;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ブロックエディターの見た目（マップエディタと同じダークテーマと言語）
	//-------------------------------------------------------------------------------
	public partial class BlockEditor
	{
		//-------------------------------------------------------------------------------
		// 開く前に、ダークテーマと今の言語を当てる処理
		//-------------------------------------------------------------------------------
		internal void ApplyEditorTheme()
		{
			Localizer.Apply(this);
			UiTheme.Apply(this);
		}

		//-------------------------------------------------------------------------------
		// 表示後に、ブロック・タイル・パレットを描く部分の下地を一覧用の暗い色にする処理
		//-------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			foreach (Control canvas in new Control[] { this.pnlBlock, this.pnlPalette, this.pnlPreview, this.pnlData })
			{
				UiTheme.MarkCanvas(canvas);
				canvas.Invalidate();
			}
		}
	}
}
