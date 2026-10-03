using System.Windows.Forms;

namespace BochiBochiEditor
{
	public partial class MapEditor
	{
		//-------------------------------------------------------------------------------
		// 表示専用のゲームで編集・保存の入口を止め、案内する処理
		//-------------------------------------------------------------------------------
		private bool BlockIfReadOnly()
		{
			if (!this.IsRomReadOnly) return false;
			MessageBox.Show(this, Localizer.T("このゲームは表示のみ対応しています。編集と保存は今後対応します。"),
				Localizer.T("表示専用"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			return true;
		}

		//-------------------------------------------------------------------------------
		// ゲームが表示専用のときだけ、ROM を表示専用として扱う判定
		//-------------------------------------------------------------------------------
		private bool IsRomReadOnly
		{
			get
			{
				return GameProfile.Current.IsReadOnly;
			}
		}

		//-------------------------------------------------------------------------------
		// このゲームでまだ使えない機能（ブロック編集・取り込み・新規作成など）の入口を止め、案内する処理
		//-------------------------------------------------------------------------------
		private bool BlockIfLimited()
		{
			if (this.BlockIfReadOnly()) return true;
			if (!GameProfile.Current.LimitedEditing) return false;
			MessageBox.Show(this, Localizer.T("この機能は、このゲームではまだ使えません（マップの確定と ROM の保存は使えます）。"),
				Localizer.T("未対応の機能"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			return true;
		}
	}
}
