using System;
using System.Globalization;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップに重ねて描く「移動エリアの数字」と「イベントの印」の濃さを、マップ下のスライダーで変える処理
	// 移動エリアとイベントで別々の値を持ち、settings.ini に残す
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		private const string CollisionOpacitySetting = "CollisionOverlayOpacity";
		private const string EventOpacitySetting = "EventOverlayOpacity";
		private const int DefaultOverlayOpacity = 60;

		private int collisionOverlayOpacity = DefaultOverlayOpacity;
		private int eventOverlayOpacity = DefaultOverlayOpacity;
		private bool updatingOverlayOpacity;

		//-------------------------------------------------------------------------------
		// 移動エリアの数字を描くときの濃さ（0〜1）
		//-------------------------------------------------------------------------------
		private float CollisionOverlayAlpha
		{
			get { return this.collisionOverlayOpacity / 100f; }
		}

		//-------------------------------------------------------------------------------
		// イベントの印を描くときの濃さ（0〜1）
		//-------------------------------------------------------------------------------
		private float EventOverlayAlpha
		{
			get { return this.eventOverlayOpacity / 100f; }
		}

		//-------------------------------------------------------------------------------
		// 保存してある濃さを読み、スライダーの動きとタブの切り替えをつなぐ処理
		//-------------------------------------------------------------------------------
		private void InitializeOverlayOpacity()
		{
			this.collisionOverlayOpacity = this.ReadOverlayOpacity(CollisionOpacitySetting);
			this.eventOverlayOpacity = this.ReadOverlayOpacity(EventOpacitySetting);
			this.trkOverlayOpacity.BackColor = UiTheme.Surface;
			this.trkOverlayOpacity.ValueChanged += this.trkOverlayOpacity_ValueChanged;
			this.trkOverlayOpacity.MouseUp += this.trkOverlayOpacity_Commit;
			this.trkOverlayOpacity.KeyUp += this.trkOverlayOpacity_Commit;
			this.tabEditorMode.SelectedIndexChanged += this.tabEditorMode_OverlayOpacityChanged;
			this.mapToolTip.SetToolTip(this.trkOverlayOpacity, Localizer.T("マップに重ねる印の濃さ: 右へ動かすと濃く、左へ動かすと薄くなります（移動エリアとイベントで別々に覚えます）"));
			this.UpdateOverlayOpacityBar();
		}

		//-------------------------------------------------------------------------------
		// settings.ini から濃さ（10〜100）を読む処理（無い・読めないときは既定値）
		//-------------------------------------------------------------------------------
		private int ReadOverlayOpacity(string name)
		{
			int value;
			if (!int.TryParse(AppSettings.Get(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
			{
				return DefaultOverlayOpacity;
			}
			return Math.Max(this.trkOverlayOpacity.Minimum, Math.Min(this.trkOverlayOpacity.Maximum, value));
		}

		//-------------------------------------------------------------------------------
		// 移動エリア・イベントのタブのときだけスライダーを出し、そのタブの濃さを映す処理
		//-------------------------------------------------------------------------------
		private void UpdateOverlayOpacityBar()
		{
			bool collision = this.tabEditorMode.SelectedTab == this.tabCollision;
			bool visible = collision || this.tabEditorMode.SelectedTab == this.tabEvent;
			this.lblOverlayOpacity.Visible = visible;
			this.trkOverlayOpacity.Visible = visible;
			if (!visible)
			{
				return;
			}
			int value = collision ? this.collisionOverlayOpacity : this.eventOverlayOpacity;
			this.updatingOverlayOpacity = true;
			this.trkOverlayOpacity.Value = value;
			this.updatingOverlayOpacity = false;
			this.lblOverlayOpacity.Text = string.Format(collision ? Localizer.T("移動エリアの濃さ {0}%") : Localizer.T("イベントの濃さ {0}%"), value);
		}

		//-------------------------------------------------------------------------------
		// タブを切り替えたときの処理
		//-------------------------------------------------------------------------------
		private void tabEditorMode_OverlayOpacityChanged(object sender, EventArgs e)
		{
			this.UpdateOverlayOpacityBar();
		}

		//-------------------------------------------------------------------------------
		// スライダーを動かしたときに、今のタブの濃さを変えてマップを描き直す処理
		//-------------------------------------------------------------------------------
		private void trkOverlayOpacity_ValueChanged(object sender, EventArgs e)
		{
			if (this.updatingOverlayOpacity)
			{
				return;
			}
			if (this.tabEditorMode.SelectedTab == this.tabCollision)
			{
				this.collisionOverlayOpacity = this.trkOverlayOpacity.Value;
			}
			else if (this.tabEditorMode.SelectedTab == this.tabEvent)
			{
				this.eventOverlayOpacity = this.trkOverlayOpacity.Value;
			}
			this.UpdateOverlayOpacityBar();
			this.pnlMapCanvas.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// スライダーから手を離したときに、濃さを settings.ini に残す処理
		//-------------------------------------------------------------------------------
		private void trkOverlayOpacity_Commit(object sender, EventArgs e)
		{
			AppSettings.Set(CollisionOpacitySetting, this.collisionOverlayOpacity.ToString(CultureInfo.InvariantCulture));
			AppSettings.Set(EventOpacitySetting, this.eventOverlayOpacity.ToString(CultureInfo.InvariantCulture));
		}
	}
}
