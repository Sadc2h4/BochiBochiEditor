using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 読み込んだ ROM の種類を、カートリッジのアイコンとゲーム名で示す札（上部バー用）
	// アイコンはドット絵なので、画面の拡大率に合わせて整数倍（最近傍）で拡大して描く
	//-------------------------------------------------------------------------------
	public class RomBadge : Control
	{
		private Image badgeIcon;
		// 透明な余白を切り落とし、整数倍に拡大して作り置いた絵（大きさが変わったら作り直す）
		private Bitmap scaledIcon;
		private int scaledIconHeight = -1;

		//-------------------------------------------------------------------------------
		// 札を初期化する処理（自分ですべて描くので二重描画にする）
		//-------------------------------------------------------------------------------
		public RomBadge()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
		}

		//-------------------------------------------------------------------------------
		// 札に出すアイコン（null ならアイコン無しで文字だけ）
		//-------------------------------------------------------------------------------
		[System.ComponentModel.DefaultValue(null)]
		public Image BadgeIcon
		{
			get { return this.badgeIcon; }
			set
			{
				this.badgeIcon = value;
				this.ResetScaledIcon();
				this.FitWidth();
				this.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// 文字が変わったら幅を合わせ直す処理
		//-------------------------------------------------------------------------------
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			this.FitWidth();
			this.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 文字の大きさが変わったら幅を合わせ直す処理
		//-------------------------------------------------------------------------------
		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			this.FitWidth();
		}

		//-------------------------------------------------------------------------------
		// 高さが変わったら、アイコンの拡大率を決め直す処理
		//-------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (this.scaledIconHeight != this.Height)
			{
				this.ResetScaledIcon();
				this.FitWidth();
			}
		}

		//-------------------------------------------------------------------------------
		// 作り置いたアイコンの絵を捨てる処理（次に使うときに作り直す）
		//-------------------------------------------------------------------------------
		private void ResetScaledIcon()
		{
			this.scaledIcon?.Dispose();
			this.scaledIcon = null;
			this.scaledIconHeight = -1;
		}

		//-------------------------------------------------------------------------------
		// 札の高さに収まる、いちばん大きい整数倍のアイコンを作る処理
		// 透明な余白は切り落とし、1 ピクセルずつ塗って拡大するので、ドット絵がにじまず上下も欠けない
		//-------------------------------------------------------------------------------
		private Bitmap GetScaledIcon()
		{
			if (this.badgeIcon == null)
			{
				return null;
			}
			if (this.scaledIcon != null && this.scaledIconHeight == this.Height)
			{
				return this.scaledIcon;
			}
			this.ResetScaledIcon();
			using (Bitmap source = new Bitmap(this.badgeIcon))
			{
				// 透明でない部分だけを囲む範囲
				int left = source.Width, top = source.Height, right = -1, bottom = -1;
				for (int y = 0; y < source.Height; y++)
				{
					for (int x = 0; x < source.Width; x++)
					{
						if (source.GetPixel(x, y).A != 0)
						{
							left = Math.Min(left, x);
							top = Math.Min(top, y);
							right = Math.Max(right, x);
							bottom = Math.Max(bottom, y);
						}
					}
				}
				if (right < 0)
				{
					return null;
				}
				int width = right - left + 1;
				int height = bottom - top + 1;
				// 上下に少し余白を残して収まる倍率（画面の拡大率に応じた上限あり、最低 1 倍）
				int room = this.Height - this.LogicalToDeviceUnits(6);
				int limit = Math.Max(1, (int)Math.Round(this.DeviceDpi / 96.0 * 2));
				int scale = Math.Max(1, Math.Min(limit, room / height));
				Bitmap scaled = new Bitmap(width * scale, height * scale);
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						Color color = source.GetPixel(left + x, top + y);
						if (color.A == 0)
						{
							continue;
						}
						for (int dy = 0; dy < scale; dy++)
						{
							for (int dx = 0; dx < scale; dx++)
							{
								scaled.SetPixel(x * scale + dx, y * scale + dy, color);
							}
						}
					}
				}
				this.scaledIcon = scaled;
				this.scaledIconHeight = this.Height;
			}
			return this.scaledIcon;
		}

		//-------------------------------------------------------------------------------
		// アイコンと文字がちょうど収まる幅にする処理（高さはデザイナーで決めたまま）
		//-------------------------------------------------------------------------------
		private void FitWidth()
		{
			int pad = this.LogicalToDeviceUnits(10);
			int gap = this.LogicalToDeviceUnits(6);
			Bitmap icon = this.GetScaledIcon();
			int iconWidth = icon == null ? 0 : icon.Width + gap;
			int textWidth = string.IsNullOrEmpty(this.Text) ? 0 : TextRenderer.MeasureText(this.Text, this.Font).Width;
			this.Width = pad * 2 + iconWidth + textWidth;
		}

		//-------------------------------------------------------------------------------
		// 札を描く処理（角丸の下地、アイコン、ゲーム名の順）
		//-------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(this.Parent != null ? this.Parent.BackColor : UiTheme.Window);
			Rectangle card = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = UiTheme.CreateRoundRect(card, this.LogicalToDeviceUnits(8)))
			using (SolidBrush back = new SolidBrush(UiTheme.Card))
			using (Pen border = new Pen(UiTheme.Border))
			{
				g.FillPath(back, path);
				g.DrawPath(border, path);
			}
			g.SmoothingMode = SmoothingMode.Default;
			int x = this.LogicalToDeviceUnits(10);
			Bitmap icon = this.GetScaledIcon();
			if (icon != null)
			{
				// 作り置いた絵を等倍で置くだけなので、にじまない
				g.DrawImageUnscaled(icon, x, (this.Height - icon.Height) / 2);
				x += icon.Width + this.LogicalToDeviceUnits(6);
			}
			TextRenderer.DrawText(g, this.Text, this.Font, new Rectangle(x, 0, this.Width - x, this.Height), UiTheme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
		}
	}
}
