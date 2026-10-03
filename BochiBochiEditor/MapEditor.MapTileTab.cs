using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// メインタブ「マップタイル」: 今のマップのタイルセットの書き出し（エクスポート）と、書き出したタイルの取り込み（インポート）
	// 左に書き出す内容の情報・ボタン・使い方、右にプレビュー（マップ全体／タイルセット 1・2 のブロック一覧）を出す
	// 書き出し・取り込みの本体は MapEditor.TileTransfer.cs
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// ブロック一覧のプレビューの倍率
		private const int TilePreviewBlockZoom = 2;

		//-------------------------------------------------------------------------------
		// 「マップタイル」タブを開いたとき・言語を変えたときに、表示を作り直すよう登録する処理
		//-------------------------------------------------------------------------------
		private void InitializeMapTileTab()
		{
			this.tabMain.SelectedIndexChanged += (sender, e) => this.RefreshMapTileTabIfShown();
			this.pnlTilePreviewHost.BackColor = UiTheme.Canvas;
			this.UpdateMapTileButtonImages();
		}

		//-------------------------------------------------------------------------------
		// 書き出し・取り込みボタンの画像を、今の言語のもの（img\MapTile_Export_JP.png など）に切り替える処理
		//-------------------------------------------------------------------------------
		private void UpdateMapTileButtonImages()
		{
			this.btnExportMapTiles.ButtonImage = this.LoadButtonImage("MapTile_Export");
			this.btnImportMapTiles.ButtonImage = this.LoadButtonImage("MapTile_Import");
		}

		//-------------------------------------------------------------------------------
		// 「マップタイル」タブを開いているときだけ、表示を作り直す処理（マップを選んだとき・ROM を開いたときなど）
		//-------------------------------------------------------------------------------
		private void RefreshMapTileTabIfShown()
		{
			if (this.tabMain.SelectedTab == this.tabMapTile)
			{
				this.RefreshMapTileTab();
			}
		}

		//-------------------------------------------------------------------------------
		// 書き出す内容の情報とプレビューを、今のマップに合わせて作り直す処理
		//-------------------------------------------------------------------------------
		private void RefreshMapTileTab()
		{
			this.lblTileExportInfo.Text = this.BuildMapTileExportInfo();
			this.RefreshMapTilePreview();
		}

		//-------------------------------------------------------------------------------
		// 書き出す内容の説明文を作る処理（対象のマップ・タイルセットごとの枚数・書き出すファイル）
		//-------------------------------------------------------------------------------
		private string BuildMapTileExportInfo()
		{
			if (this.romData == null || this.tempHeader == null || this.tempHeader.FooterAddress == 0)
			{
				return Localizer.T("左の一覧からマップを選ぶと、書き出す内容をここに表示します。");
			}
			StringBuilder sb = new StringBuilder();
			GameProfile profile = GameProfile.Current;
			MapFooter footer = this.ReadMapFooter((int)this.tempHeader.FooterAddress);
			sb.AppendLine(string.Format(Localizer.T("対象: ({0}, {1}) {2}（{3}×{4} ブロック）"), this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this), footer.MapWidth, footer.MapHeight));
			sb.AppendLine(string.Format(Localizer.T("ゲーム: {0}（{1}）"), Localizer.T(profile.DisplayName), profile.Code));
			uint[] headerAddresses = new uint[] { footer.Tileset1Address, footer.Tileset2Address };
			for (int t = 0; t < 2; t++)
			{
				string error;
				TilesetHeader header = this.ReadTilesetHeader((int)headerAddresses[t]);
				MapTilePackage.TilesetPart part = this.ReadTilesetPart(header, headerAddresses[t], t == 1, out error);
				if (part == null)
				{
					sb.AppendLine(error);
					continue;
				}
				int firstPalette = t == 0 ? 0 : profile.PrimaryPaletteCount;
				int lastPalette = t == 0 ? profile.PrimaryPaletteCount - 1 : 12;
				sb.AppendLine(string.Format(Localizer.T("タイルセット{0}: 番号 {1}・タイル {2} 枚・ブロック {3} 個・パレット {4}〜{5}・{6}"),
					t + 1, part.Index, part.TileCount, part.BlockCount, firstPalette, lastPalette, part.Compressed ? Localizer.T("圧縮あり") : Localizer.T("圧縮なし")));
			}
			sb.AppendLine(Localizer.T("書き出すもの: タイル画像（16 色 PNG）・パレット・ブロック一覧の絵・マップ全体の絵・map_info.txt"));
			if (this.hasUnsavedChanges)
			{
				sb.AppendLine(Localizer.T("※ 確定していない変更があります。書き出す前に「編集中のMAPを確定」を押してください。"));
			}
			return sb.ToString().TrimEnd();
		}

		//-------------------------------------------------------------------------------
		// プレビューを描き直す処理（マップ全体は等倍、ブロック一覧は 2 倍。確定済みの ROM の中身から描く）
		//-------------------------------------------------------------------------------
		private void RefreshMapTilePreview()
		{
			Image old = this.picTilePreview.Image;
			this.picTilePreview.Image = null;
			old?.Dispose();
			if (this.romData == null || this.tempHeader == null || this.tempHeader.FooterAddress == 0)
			{
				return;
			}
			MapFooter footer = this.ReadMapFooter((int)this.tempHeader.FooterAddress);
			MapThumbnailRenderer.Settings settings = MapThumbnailRenderer.Settings.FromCurrentGame();
			if (this.rbTilePreviewMap.Checked)
			{
				this.picTilePreview.Image = MapThumbnailRenderer.RenderFullMap(this.romData, this.tempHeader.FooterAddress, settings);
				return;
			}
			bool secondary = this.rbTilePreviewTileset2.Checked;
			uint headerAddress = secondary ? footer.Tileset2Address : footer.Tileset1Address;
			TilesetHeader header = this.ReadTilesetHeader((int)headerAddress);
			int count = secondary ? this.GetSecondaryBlockCount(this.GetTilesetIndexFromHeaderAddress(headerAddress), header) : this.GetPrimaryBlockCount(header);
			int first = secondary ? GameProfile.Current.PrimaryBlockCount : 0;
			using (Bitmap sheet = MapThumbnailRenderer.RenderBlockSheet(this.romData, footer.Tileset1Address, footer.Tileset2Address, settings, first, count, TileTransferSheetColumns))
			{
				if (sheet == null)
				{
					return;
				}
				Bitmap zoomed = new Bitmap(sheet.Width * TilePreviewBlockZoom, sheet.Height * TilePreviewBlockZoom, PixelFormat.Format32bppArgb);
				using (Graphics g = Graphics.FromImage(zoomed))
				{
					g.InterpolationMode = InterpolationMode.NearestNeighbor;
					g.PixelOffsetMode = PixelOffsetMode.Half;
					g.DrawImage(sheet, new Rectangle(0, 0, zoomed.Width, zoomed.Height));
				}
				this.picTilePreview.Image = zoomed;
			}
		}

		//-------------------------------------------------------------------------------
		// プレビューの種類（マップ全体／タイルセット 1・2 のブロック）が変わったときの処理
		//-------------------------------------------------------------------------------
		private void rbTilePreview_CheckedChanged(object sender, EventArgs e)
		{
			if (((RadioButton)sender).Checked)
			{
				this.RefreshMapTilePreview();
			}
		}
	}
}
