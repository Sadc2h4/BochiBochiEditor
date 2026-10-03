using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 画像そのものをボタンとして描くボタン（マップチップ取り込みボタン・上部バーの ROM 操作ボタン用）
	// 画像の縦横比を保って描き、マウスを乗せると明るく、押すと少し沈み、押せないときは暗くくすませる
	// 画像が無いときは、ふつうのボタンと同じように文字で描く
	//-------------------------------------------------------------------------------
	public class ImageButton : Button
	{
		private Image buttonImage;
		private bool hovering;
		private bool pressing;

		//-------------------------------------------------------------------------------
		// ボタンを初期化する処理（自分ですべて描くので、ちらつかないように二重描画にする）
		//-------------------------------------------------------------------------------
		public ImageButton()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
			this.Cursor = Cursors.Hand;
		}

		//-------------------------------------------------------------------------------
		// ボタンとして描く画像（null なら文字で描く）
		//-------------------------------------------------------------------------------
		[System.ComponentModel.DefaultValue(null)]
		public Image ButtonImage
		{
			get { return this.buttonImage; }
			set
			{
				this.buttonImage = value;
				this.AdjustSizeToImage();
				this.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// 高さの上限（論理ピクセル）。幅いっぱいに広げると大きくなりすぎるので、ここで止める
		//-------------------------------------------------------------------------------
		[System.ComponentModel.DefaultValue(52)]
		public int MaximumImageHeight { get; set; } = 52;

		//-------------------------------------------------------------------------------
		// true なら高さを固定して、画像の縦横比に合わせて幅を決める（上部バーのように高さがそろった場所用）
		//-------------------------------------------------------------------------------
		[System.ComponentModel.DefaultValue(false)]
		public bool FitWidthToHeight { get; set; }

		//-------------------------------------------------------------------------------
		// 画像の周りの余白（論理ピクセル）
		//-------------------------------------------------------------------------------
		[System.ComponentModel.DefaultValue(4)]
		public int ImageMargin { get; set; } = 4;

		//-------------------------------------------------------------------------------
		// 幅が変わったら、画像の縦横比に合わせて高さを決め直す処理
		//-------------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.AdjustSizeToImage();
		}

		//-------------------------------------------------------------------------------
		// 画像の縦横比から大きさを決める処理（余白込み）
		// ふつうは幅から高さを決め、上限を超えたら上限で止める。FitWidthToHeight なら高さから幅を決める
		//-------------------------------------------------------------------------------
		private void AdjustSizeToImage()
		{
			if (this.buttonImage == null || this.buttonImage.Width <= 0 || this.buttonImage.Height <= 0)
			{
				return;
			}
			int margin = this.LogicalToDeviceUnits(this.ImageMargin);
			if (this.FitWidthToHeight)
			{
				int width = (this.Height - margin * 2) * this.buttonImage.Width / this.buttonImage.Height + margin * 2;
				if (width > 0 && this.Width != width)
				{
					this.Width = width;
				}
				return;
			}
			if (this.Width <= 0)
			{
				return;
			}
			int byWidth = (this.Width - margin * 2) * this.buttonImage.Height / this.buttonImage.Width;
			int height = Math.Min(byWidth, this.LogicalToDeviceUnits(this.MaximumImageHeight)) + margin * 2;
			if (height > 0 && this.Height != height)
			{
				this.Height = height;
			}
		}

		//-------------------------------------------------------------------------------
		// マウスが乗った・離れた・押した・離したときに、見た目を切り替える処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseEnter(EventArgs e)
		{
			this.hovering = true;
			this.Invalidate();
			base.OnMouseEnter(e);
		}

		//-------------------------------------------------------------------------------
		// マウスが離れたら元の見た目に戻す処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			this.hovering = false;
			this.pressing = false;
			this.Invalidate();
			base.OnMouseLeave(e);
		}

		//-------------------------------------------------------------------------------
		// 押している間は沈んだ見た目にする処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.pressing = true;
				this.Invalidate();
			}
			base.OnMouseDown(e);
		}

		//-------------------------------------------------------------------------------
		// 離したら沈んだ見た目を戻す処理
		//-------------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			this.pressing = false;
			this.Invalidate();
			base.OnMouseUp(e);
		}

		//-------------------------------------------------------------------------------
		// 押せる／押せないが変わったら描き直す処理
		//-------------------------------------------------------------------------------
		protected override void OnEnabledChanged(EventArgs e)
		{
			if (!this.Enabled)
			{
				this.hovering = false;
				this.pressing = false;
			}
			this.Invalidate();
			base.OnEnabledChanged(e);
		}

		//-------------------------------------------------------------------------------
		// ボタンを描く処理（背景は置かれている場所の色で塗り、その上に画像を描く）
		//-------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Color back = this.Parent != null ? this.Parent.BackColor : UiTheme.Surface;
			g.Clear(back);
			if (this.buttonImage == null)
			{
				this.PaintTextFallback(g);
				return;
			}
			Rectangle dest = this.GetImageRectangle();
			if (this.pressing)
			{
				// 押している間は 1px 下げて少し小さく描き、沈んだように見せる
				dest.Inflate(-this.LogicalToDeviceUnits(1), -this.LogicalToDeviceUnits(1));
				dest.Offset(0, this.LogicalToDeviceUnits(1));
			}
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (ImageAttributes attributes = this.CreateStateAttributes())
			{
				g.DrawImage(this.buttonImage, dest, 0, 0, this.buttonImage.Width, this.buttonImage.Height, GraphicsUnit.Pixel, attributes);
			}
			// キーボードで選んだときだけ、画像の周りに強調色の枠を出す
			if (this.Focused && this.ShowFocusCues)
			{
				Rectangle focus = dest;
				focus.Inflate(this.LogicalToDeviceUnits(2), this.LogicalToDeviceUnits(2));
				using (GraphicsPath path = UiTheme.CreateRoundRect(focus, focus.Height / 2))
				using (Pen pen = new Pen(UiTheme.Accent, 2f))
				{
					g.DrawPath(pen, path);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 画像を描く範囲（縦横比を保って、ボタンの中央に収める）を求める処理
		//-------------------------------------------------------------------------------
		private Rectangle GetImageRectangle()
		{
			int margin = this.LogicalToDeviceUnits(this.ImageMargin);
			Rectangle area = Rectangle.Inflate(this.ClientRectangle, -margin, -margin);
			float scale = Math.Min((float)area.Width / this.buttonImage.Width, (float)area.Height / this.buttonImage.Height);
			int width = (int)(this.buttonImage.Width * scale);
			int height = (int)(this.buttonImage.Height * scale);
			return new Rectangle(area.X + (area.Width - width) / 2, area.Y + (area.Height - height) / 2, width, height);
		}

		//-------------------------------------------------------------------------------
		// 今の状態（ふつう・マウスが乗っている・押している・押せない）に合わせた色の変え方を作る処理
		//-------------------------------------------------------------------------------
		private ImageAttributes CreateStateAttributes()
		{
			ImageAttributes attributes = new ImageAttributes();
			ColorMatrix matrix;
			if (!this.Enabled)
			{
				// 押せないときは白黒に近づけて、薄く描く
				matrix = new ColorMatrix(new float[][]
				{
					new float[] { 0.30f, 0.30f, 0.30f, 0, 0 },
					new float[] { 0.45f, 0.45f, 0.45f, 0, 0 },
					new float[] { 0.10f, 0.10f, 0.10f, 0, 0 },
					new float[] { 0, 0, 0, 0.45f, 0 },
					new float[] { 0, 0, 0, 0, 1 }
				});
			}
			else
			{
				// マウスが乗ったら明るく、押している間は少し暗くする
				float light = this.pressing ? -0.06f : (this.hovering ? 0.10f : 0f);
				matrix = new ColorMatrix(new float[][]
				{
					new float[] { 1, 0, 0, 0, 0 },
					new float[] { 0, 1, 0, 0, 0 },
					new float[] { 0, 0, 1, 0, 0 },
					new float[] { 0, 0, 0, 1, 0 },
					new float[] { light, light, light, 0, 1 }
				});
			}
			attributes.SetColorMatrix(matrix);
			return attributes;
		}

		//-------------------------------------------------------------------------------
		// 画像が無いときに、ふつうのボタンと同じ見た目で文字を描く処理
		//-------------------------------------------------------------------------------
		private void PaintTextFallback(Graphics g)
		{
			Rectangle area = Rectangle.Inflate(this.ClientRectangle, -1, -1);
			Color fill = !this.Enabled ? UiTheme.Surface : (this.pressing ? UiTheme.RaisedPressed : (this.hovering ? UiTheme.RaisedHover : UiTheme.Raised));
			using (SolidBrush brush = new SolidBrush(fill))
			using (Pen pen = new Pen(UiTheme.Border))
			{
				g.FillRectangle(brush, area);
				g.DrawRectangle(pen, area.X, area.Y, area.Width - 1, area.Height - 1);
			}
			TextRenderer.DrawText(g, this.Text, this.Font, area, this.Enabled ? UiTheme.Text : UiTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
		}
	}
}
