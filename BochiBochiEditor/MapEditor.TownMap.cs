using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 右ペインの「タウンマップ」: 今のマップが地方のどこに当たるかを赤い枠で示す
	// マップ名の番号（ヘッダーの MapNameId）が、タウンマップの場所の表の番号と同じことを使う
	// マスをクリックすると、その場所のマップを左の一覧に絞り込む
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// タウンマップの部分を単独で分離する処理
		private ToolPartDocker townMapDocker;
		// 読み込んだタウンマップ（別の ROM を開いたら読み直す）
		private TownMapData townMap;
		private byte[] townMapRom;
		// 今表示している地方と、今のマップの場所番号（案内文を戻すため）
		private int townMapRegion;
		private string townMapCurrentText = string.Empty;
		// たたんで見出しの 1 行だけにしているか（小さい画面向け。settings.ini の TownMapCollapsed に保存）
		private bool townMapCollapsed;

		//-------------------------------------------------------------------------------
		// タウンマップの部分を初期化する処理
		//-------------------------------------------------------------------------------
		private void InitializeTownMap()
		{
			// 右ペインの縦幅を使うので、保存された設定が無ければたたんだ状態で始める
			this.townMapCollapsed = AppSettings.Get("TownMapCollapsed", "1") == "1";
			this.townMapDocker = new ToolPartDocker(this, this.pnlTownMapPart, this.pnlTownMapHeader, this.lblTownMapTitle, this.btnTownMapFloat,
				() => Localizer.T("タウンマップ　（ドラッグで分離）"),
				() => Localizer.T("タウンマップ　（元の場所へドラッグで戻す）"),
				() => Localizer.T("タウンマップ"),
				this.MapEditor_KeyDown);
			// 分離中は窓いっぱいに広げ、戻したら上から積む並びに戻す
			this.townMapDocker.StateChanged += (sender, e) =>
			{
				this.pnlTownMapPart.Dock = this.townMapDocker.IsFloating ? DockStyle.Fill : DockStyle.Top;
				this.ApplyTownMapCollapsed();
				this.UpdateToolFloatButton();
			};
			this.pnlTownMapPart.SizeChanged += (sender, e) => this.FitTownMapPartHeight();
			// 画像ボタンは幅に合わせて自分の高さを決めるので、それに合わせてたたんだときの高さも決め直す
			this.btnTownMapOpen.SizeChanged += (sender, e) => this.FitTownMapPartHeight();
			this.townMapView.HoverCellChanged += this.TownMapView_HoverCellChanged;
			this.townMapView.CellClicked += this.TownMapView_CellClicked;
			this.mapToolTip.SetToolTip(this.townMapView, Localizer.T("赤い枠が今のマップの場所です。マスをクリックすると、その場所のマップを左の一覧に絞り込みます。"));
			this.ApplyTownMapCollapsed();
		}

		//-------------------------------------------------------------------------------
		// 「▾／▸」ボタン: タウンマップをたたむ／広げる処理（状態は次回起動時のために保存する）
		//-------------------------------------------------------------------------------
		private void btnTownMapCollapse_Click(object sender, EventArgs e)
		{
			this.townMapCollapsed = !this.townMapCollapsed;
			AppSettings.Set("TownMapCollapsed", this.townMapCollapsed ? "1" : "0");
			this.ApplyTownMapCollapsed();
		}

		//-------------------------------------------------------------------------------
		// たたんでいるかどうかを、表示に反映する処理
		// 分離している間は、別ウィンドウの中なので常に広げて表示し、たたむボタンは隠す
		//-------------------------------------------------------------------------------
		private void ApplyTownMapCollapsed()
		{
			bool floating = this.townMapDocker != null && this.townMapDocker.IsFloating;
			bool collapsed = this.townMapCollapsed && !floating;
			this.townMapView.Visible = !collapsed;
			this.lblTownMapInfo.Visible = !collapsed;
			// たたんでいる間は、見出しの代わりに広げるための画像ボタンだけを出す
			this.pnlTownMapHeader.Visible = !collapsed;
			this.btnTownMapOpen.Visible = collapsed;
			this.UpdateTownMapOpenButton();
			this.btnTownMapCollapse.Visible = !floating;
			UiTheme.SetGlyph(this.btnTownMapCollapse, collapsed ? "" : "", collapsed ? "▸" : "▾");
			this.mapToolTip?.SetToolTip(this.btnTownMapCollapse, collapsed ? Localizer.T("タウンマップを広げる") : Localizer.T("タウンマップをたたんで 1 行にする"));
			this.townMapDocker?.UpdateHeader();
			this.FitTownMapPartHeight();
		}

		//-------------------------------------------------------------------------------
		// 広げるための画像ボタンの画像（今の言語のもの）とツールチップを設定する処理
		//-------------------------------------------------------------------------------
		private void UpdateTownMapOpenButton()
		{
			if (this.btnTownMapOpen == null)
			{
				return;
			}
			this.btnTownMapOpen.ButtonImage = this.LoadButtonImage("Open_TownMap");
			string place = string.IsNullOrEmpty(this.townMapCurrentText) ? string.Empty : Environment.NewLine + this.townMapCurrentText;
			this.mapToolTip?.SetToolTip(this.btnTownMapOpen, Localizer.T("タウンマップを広げる") + place);
			this.FitTownMapPartHeight();
		}

		//-------------------------------------------------------------------------------
		// 右ペインに入っている間は、幅に合わせて高さを決める処理（地図の部分の縦横比 4:3 を保つ）
		//-------------------------------------------------------------------------------
		private void FitTownMapPartHeight()
		{
			if (this.townMapDocker == null || this.townMapDocker.IsFloating)
			{
				return;
			}
			Rectangle view = this.townMap != null ? this.townMap.ViewArea : new Rectangle(24, 16, 192, 144);
			int mapHeight = this.pnlTownMapPart.ClientSize.Width * view.Height / view.Width;
			int height = this.townMapCollapsed ? this.btnTownMapOpen.Height : this.pnlTownMapHeader.Height + this.lblTownMapInfo.Height + mapHeight;
			if (this.pnlTownMapPart.Height != height)
			{
				this.pnlTownMapPart.Height = height;
			}
		}

		//-------------------------------------------------------------------------------
		// 今のマップに合わせて、表示する地方と赤い枠を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateTownMap()
		{
			if (this.townMapView == null)
			{
				return;
			}
			if (this.romData == null)
			{
				this.townMap = null;
				this.townMapRom = null;
				this.townMapView.Data = null;
				this.townMapView.MapImage = null;
				this.townMapView.SetMarkedCells(null);
				this.SetTownMapCurrentText(Localizer.T("ROM を読み込むと、タウンマップを表示します。"));
				return;
			}
			if (!ReferenceEquals(this.townMapRom, this.romData))
			{
				this.townMap = TownMapData.Load(this.romData);
				this.townMapRom = this.romData;
				this.townMapRegion = 0;
			}
			if (this.townMap == null)
			{
				this.townMapView.Data = null;
				this.townMapView.MapImage = null;
				this.townMapView.SetMarkedCells(null);
				this.SetTownMapCurrentText(Localizer.T("この ROM では、タウンマップを読み込めませんでした。"));
				return;
			}
			List<Point> cells = null;
			string text;
			if (this.tempHeader == null || this.chkTerrainIdMode.Checked)
			{
				text = Localizer.T("マップを選ぶと、その場所を赤い枠で示します。");
			}
			else
			{
				byte section = this.tempHeader.MapNameId;
				int region;
				if (this.townMap.FindSection(section, out region, out cells))
				{
					this.townMapRegion = region;
					text = string.Format(Localizer.T("{0}: {1}"), this.GetTownMapRegionName(region), this.GetMapSectionName(section));
				}
				else
				{
					cells = null;
					text = string.Format(Localizer.T("{0} はタウンマップに載っていません"), this.GetMapSectionName(section));
				}
			}
			this.townMapView.Data = this.townMap;
			this.townMapView.MapImage = this.townMap.Images[this.townMapRegion] ?? this.townMap.Images[0];
			this.townMapView.SetMarkedCells(cells);
			this.SetTownMapCurrentText(text);
		}

		//-------------------------------------------------------------------------------
		// 下の案内文（今のマップの場所）を設定する処理（マウスを乗せている間は、乗せたマスの名前を優先する）
		//-------------------------------------------------------------------------------
		private void SetTownMapCurrentText(string text)
		{
			bool changed = this.townMapCurrentText != text;
			this.townMapCurrentText = text;
			this.lblTownMapInfo.Text = text;
			// たたんでいる間は、広げるボタンのツールチップに今の場所を出す
			if (changed)
			{
				this.UpdateTownMapOpenButton();
			}
		}

		//-------------------------------------------------------------------------------
		// 地方の名前を返す処理
		//-------------------------------------------------------------------------------
		private string GetTownMapRegionName(int region)
		{
			return this.townMap != null && region >= 0 && region < this.townMap.RegionNames.Length ? Localizer.T(this.townMap.RegionNames[region]) : string.Empty;
		}

		//-------------------------------------------------------------------------------
		// 場所番号の名前を返す処理（マップ名の一覧から引く。無ければ番号だけ）
		//-------------------------------------------------------------------------------
		private string GetMapSectionName(byte section)
		{
			int index = section - MapEditor.MAP_NAME_FIRST_INDEX;
			if (index >= 0 && index < this.cmbMapNameId.Items.Count)
			{
				string item = this.cmbMapNameId.Items[index].ToString();
				int close = item.IndexOf(']');
				return string.Format("[{0:X2}] {1}", section, close >= 0 ? item.Substring(close + 1) : item);
			}
			return string.Format("[{0:X2}]", section);
		}

		//-------------------------------------------------------------------------------
		// マスにある場所番号を集める処理（地上・ダンジョンの順、重複と「場所なし」は除く）
		//-------------------------------------------------------------------------------
		private List<byte> GetTownMapSectionsAt(Point cell)
		{
			List<byte> result = new List<byte>();
			if (this.townMap == null || cell.X < 0)
			{
				return result;
			}
			for (int layer = 0; layer < this.townMap.LayerCount; layer++)
			{
				byte section = this.townMap.GetSection(this.townMapRegion, layer, cell.X, cell.Y);
				if (section != this.townMap.NoneSection && !result.Contains(section))
				{
					result.Add(section);
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// マウスを乗せたマスの場所の名前を、下の案内文に出す処理
		//-------------------------------------------------------------------------------
		private void TownMapView_HoverCellChanged(object sender, Point cell)
		{
			List<byte> sections = this.GetTownMapSectionsAt(cell);
			if (sections.Count == 0)
			{
				this.lblTownMapInfo.Text = this.townMapCurrentText;
				this.townMapView.Cursor = Cursors.Default;
				return;
			}
			this.lblTownMapInfo.Text = string.Join(" / ", sections.Select(s => this.GetMapSectionName(s))) + Localizer.T("（クリックで一覧に絞り込み）");
			this.townMapView.Cursor = Cursors.Hand;
		}

		//-------------------------------------------------------------------------------
		// マスをクリックしたら、その場所のマップを左の一覧に絞り込む処理（検索欄に「場所:58」の形で入れる）
		//-------------------------------------------------------------------------------
		private void TownMapView_CellClicked(object sender, Point cell)
		{
			List<byte> sections = this.GetTownMapSectionsAt(cell);
			if (sections.Count == 0 || this.romData == null)
			{
				return;
			}
			this.txtMapSearch.Text = Localizer.T("場所") + ":" + string.Join(",", sections.Select(s => s.ToString("X2")));
			this.mapSearchTimer.Stop();
			this.ApplyMapSearchFilter();
		}
	}
}
