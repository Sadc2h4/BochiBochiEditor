using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「重ね方」の見本（取り込む画像のブロックで、見た目とプレイヤーとの重なりを描く）
	//-------------------------------------------------------------------------------
	public partial class TileImportWizard
	{
		//-------------------------------------------------------------------------------
		// 見本に使うブロックを選ぶ処理（透明な部分を含むブロックを優先する。違いが一番わかりやすいため）
		//-------------------------------------------------------------------------------
		private int PickLayerSampleBlock()
		{
			if (this.plan == null || this.planOptions == null || this.plan.Blocks.Count == 0)
			{
				return -1;
			}
			int artMask = 1 << this.format.ArtLayer(this.planOptions.Layer);
			int best = 0;
			int bestScore = -1;
			for (int i = 0; i < this.plan.Blocks.Count && i < 64; i++)
			{
				// 絵の層だけを描いて、透明なピクセルの数を数える
				using (Bitmap art = TileImportEngine.RenderBlock(this.plan.Blocks[i].Entries, this.plan, this.planOptions, artMask))
				{
					int transparent = 0;
					for (int y = 0; y < 16; y++)
					{
						for (int x = 0; x < 16; x++)
						{
							if (art.GetPixel(x, y).A == 0)
							{
								transparent++;
							}
						}
					}
					// 全部透明・全部不透明のブロックは違いが出ないので後回しにする
					int score = (transparent > 0 && transparent < 256) ? Math.Min(transparent, 256 - transparent) : 0;
					if (score > bestScore)
					{
						bestScore = score;
						best = i;
					}
				}
			}
			return best;
		}

		//-------------------------------------------------------------------------------
		// 「重ね方」の見本を描く処理（左: 見た目、右: プレイヤーとの重なり）
		//-------------------------------------------------------------------------------
		private void pnlLayerPreview_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle area = this.pnlLayerPreview.ClientRectangle;
			int index = this.PickLayerSampleBlock();
			if (index < 0)
			{
				TextRenderer.DrawText(g, Localizer.T("画像を選ぶと見本が出ます"), this.Font, area, UiTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				return;
			}
			TileImportEngine.LayerStyle style = this.planOptions.Layer;
			ushort[] entries = this.plan.Blocks[index].Entries;
			// 枠の大きさに合わせて拡大する（プレイヤーの頭がマスの上にはみ出す分、マスの 1.5 倍の高さと、下に説明 1 行分が要る）
			int captionHeight = this.Font.Height + 4;
			int noteHeight = this.Font.Height + 4;
			int byHeight = (int)((area.Height - captionHeight - noteHeight) / 1.5);
			int byWidth = area.Width / 2 - 16;
			int cell = Math.Max(16, Math.Min(byHeight, byWidth) / 16 * 16);
			// 左右の見本は同じ高さにそろえる（頭がはみ出す分＝マスの半分だけ、両方とも下げて描く）
			int top = area.Y + captionHeight + cell / 2;
			Rectangle left = new Rectangle(area.X + 8, top, cell, cell);
			Rectangle right = new Rectangle(area.X + area.Width / 2 + 8, top, cell, cell);
			TextRenderer.DrawText(g, Localizer.T("見た目"), this.Font, new Point(area.X + 4, area.Y), UiTheme.TextMuted);
			TextRenderer.DrawText(g, Localizer.T("プレイヤーが重なったとき"), this.Font, new Point(area.X + area.Width / 2 + 4, area.Y), UiTheme.TextMuted);

			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;

			// 左: 完成したブロック。絵だけの場合は、透明な部分がゲームの背景色で見える
			this.FillBackdrop(g, left, style);
			using (Bitmap full = TileImportEngine.RenderBlock(entries, this.plan, this.planOptions))
			{
				g.DrawImage(full, left);
			}

			// 右: そのマスの上に立つプレイヤー（頭はマスの上にはみ出す）
			this.FillBackdrop(g, right, style);
			// プレイヤーより手前に描くときは、絵の層より下 → プレイヤー → 絵の層の順。それ以外は全層 → プレイヤー
			bool aboveTop = style == TileImportEngine.LayerStyle.OverBaseAbovePlayer;
			int artLayer = this.format.ArtLayer(style);
			int underMask = aboveTop ? (1 << artLayer) - 1 : -1;
			using (Bitmap under = TileImportEngine.RenderBlock(entries, this.plan, this.planOptions, underMask))
			{
				g.DrawImage(under, right);
			}
			// プレイヤーはそのマスの上に立つ（足元をマスの下端にそろえる）
			DrawSamplePlayer(g, right);
			if (aboveTop)
			{
				// プレイヤーより手前に描く層（木の上部・屋根など）は、プレイヤーの上に重なる
				using (Bitmap over = TileImportEngine.RenderBlock(entries, this.plan, this.planOptions, 1 << artLayer))
				{
					g.DrawImage(over, right);
				}
			}
			g.PixelOffsetMode = PixelOffsetMode.Default;

			string note = style == TileImportEngine.LayerStyle.ArtOnBottom
				? Localizer.T("透明な部分は背景色")
				: (aboveTop ? Localizer.T("絵がプレイヤーを隠す（奥に見える）") : Localizer.T("プレイヤーが絵の上（手前に見える）"));
			// 説明は見本の下に 1 行で出す
			TextRenderer.DrawText(g, note, this.Font, new Rectangle(area.X + 4, left.Bottom + 4, area.Width - 8, noteHeight), UiTheme.Text, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
		}

		//-------------------------------------------------------------------------------
		// 見本の下地を塗る処理（絵だけの場合はゲームの背景色、それ以外は下地ブロックが描かれるので市松模様）
		//-------------------------------------------------------------------------------
		private void FillBackdrop(Graphics g, Rectangle rect, TileImportEngine.LayerStyle style)
		{
			if (style == TileImportEngine.LayerStyle.ArtOnBottom && this.plan.Target != null)
			{
				Color backdrop = this.plan.Target.AllPalettes[0];
				using (SolidBrush brush = new SolidBrush(backdrop.A == 0 ? Color.Black : Color.FromArgb(255, backdrop)))
				{
					g.FillRectangle(brush, rect);
				}
				return;
			}
			using (HatchBrush checker = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.FromArgb(60, 64, 90), Color.FromArgb(44, 48, 70)))
			{
				g.FillRectangle(checker, rect);
			}
		}

		// 見本に使うプレイヤーの画像（img\TrainerOW.png。読めなければ null）
		private static Bitmap samplePlayerImage;
		private static bool samplePlayerLoaded;

		//-------------------------------------------------------------------------------
		// 見本用のプレイヤー画像を読み込む処理（1 回だけ読む）
		//-------------------------------------------------------------------------------
		private static Bitmap GetSamplePlayerImage()
		{
			if (!samplePlayerLoaded)
			{
				samplePlayerLoaded = true;
				try
				{
					string path = AppAssetLocator.FindRequiredFile(System.IO.Path.Combine("img", "TrainerOW.png"));
					using (Bitmap loaded = new Bitmap(path))
					{
						samplePlayerImage = new Bitmap(loaded);
					}
				}
				catch (Exception)
				{
					samplePlayerImage = null;
				}
			}
			return samplePlayerImage;
		}

		//-------------------------------------------------------------------------------
		// マスの上に立つプレイヤーを描く処理（足元をマスの下端にそろえ、頭はマスの上にはみ出す）
		// img\TrainerOW.png があればそれを使い、無ければ簡単な人の形で代わりに描く
		//-------------------------------------------------------------------------------
		private static void DrawSamplePlayer(Graphics g, Rectangle tile)
		{
			Bitmap sprite = GetSamplePlayerImage();
			// ブロック 16px に対する倍率と同じ倍率でプレイヤーを描く
			float scale = tile.Width / 16f;
			if (sprite != null)
			{
				int w = (int)(sprite.Width * scale);
				int h = (int)(sprite.Height * scale);
				Rectangle dest = new Rectangle(tile.X + (tile.Width - w) / 2, tile.Bottom - h, w, h);
				InterpolationMode previous = g.InterpolationMode;
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.DrawImage(sprite, dest);
				g.InterpolationMode = previous;
				return;
			}
			Rectangle r = new Rectangle(tile.X + tile.Width / 4, tile.Bottom - tile.Height * 9 / 8, tile.Width / 2, tile.Height * 9 / 8);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			int rw = r.Width;
			int rh = r.Height;
			using (SolidBrush cap = new SolidBrush(Color.FromArgb(220, 60, 60)))
			using (SolidBrush skin = new SolidBrush(Color.FromArgb(250, 210, 170)))
			using (SolidBrush body = new SolidBrush(Color.FromArgb(70, 110, 210)))
			using (SolidBrush legs = new SolidBrush(Color.FromArgb(50, 50, 70)))
			using (Pen outline = new Pen(Color.FromArgb(30, 30, 40), 1.5f))
			{
				Rectangle head = new Rectangle(r.X + rw / 8, r.Y, rw * 3 / 4, rh * 3 / 8);
				Rectangle torso = new Rectangle(r.X + rw / 8, r.Y + rh * 3 / 8, rw * 3 / 4, rh * 3 / 8);
				Rectangle leg = new Rectangle(r.X + rw / 4, r.Y + rh * 3 / 4, rw / 2, rh / 4);
				g.FillRectangle(legs, leg);
				g.FillRectangle(body, torso);
				g.DrawRectangle(outline, torso);
				g.FillEllipse(skin, head);
				g.FillPie(cap, head.X, head.Y, head.Width, head.Height, 180, 180);
				g.DrawEllipse(outline, head);
			}
			g.SmoothingMode = SmoothingMode.Default;
		}
	}
}
