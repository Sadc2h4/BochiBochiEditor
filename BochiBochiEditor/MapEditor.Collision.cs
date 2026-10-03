using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 移動エリア（移動許可）の一覧と、各値の意味の説明
	// 値は「高さ × 4 ＋ 通行（0 = 通れる、それ以外 = 通れない）」でできている（00〜3F の 64 通り）
	// 説明は AdvanceMap の定義（GehDaten）とゲームの仕組みをもとに書いたもの
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 一覧の並び（横 8 × 縦 8）と、元の画像の 1 マスの大きさ
		private const int CollisionColumns = 8;
		private const int CollisionCellPixels = 16;
		// マウスが乗っている移動エリアの値（無ければ -1）
		private int collisionHoverIndex = -1;

		//-------------------------------------------------------------------------------
		// 移動エリアの一覧と説明欄の初期化処理
		//-------------------------------------------------------------------------------
		private void InitializeCollisionPaletteUI()
		{
			this.lblCollisionTitle.Font = new Font(this.Font.FontFamily, this.Font.Size + 1f, FontStyle.Bold);
			this.pnlCollisionPalette.MouseMove += this.pnlCollisionPalette_MouseMove;
			this.pnlCollisionPalette.MouseLeave += this.pnlCollisionPalette_MouseLeave;
			this.pnlCollisionPalette.Resize += (sender, e) => this.pnlCollisionPalette.Invalidate();
			this.UpdateCollisionInfo();
		}

		//-------------------------------------------------------------------------------
		// 一覧の拡大率（元の画像の何倍で描くか。整数倍にして数字をくっきり見せる）を求める処理
		//-------------------------------------------------------------------------------
		private int GetCollisionScale()
		{
			int size = CollisionColumns * CollisionCellPixels;
			int room = Math.Min(this.pnlCollisionPalette.ClientSize.Width, this.pnlCollisionPalette.ClientSize.Height);
			return Math.Max(1, room / size);
		}

		//-------------------------------------------------------------------------------
		// 一覧を描く左上の位置（横は中央にそろえる）を求める処理
		//-------------------------------------------------------------------------------
		private Point GetCollisionOrigin(int cell)
		{
			int width = CollisionColumns * cell;
			return new Point(Math.Max(0, (this.pnlCollisionPalette.ClientSize.Width - width) / 2), 0);
		}

		//-------------------------------------------------------------------------------
		// 一覧上の座標から移動エリアの値を求める処理（一覧の外は -1）
		//-------------------------------------------------------------------------------
		private int HitTestCollision(Point point)
		{
			int cell = CollisionCellPixels * this.GetCollisionScale();
			Point origin = this.GetCollisionOrigin(cell);
			int x = point.X - origin.X;
			int y = point.Y - origin.Y;
			if (x < 0 || y < 0 || x >= CollisionColumns * cell || y >= CollisionColumns * cell)
			{
				return -1;
			}
			return y / cell * CollisionColumns + x / cell;
		}

		//-------------------------------------------------------------------------------
		// 移動エリアの一覧を描く処理（枠の大きさに合わせて整数倍で拡大する）
		//-------------------------------------------------------------------------------
		private void PaintCollisionPalette(Graphics g)
		{
			this.UpdateCollisionInfo();
			if (this.blockPaletteBitmap == null || this.collisionBitmap == null)
			{
				return;
			}
			int scale = this.GetCollisionScale();
			int cell = CollisionCellPixels * scale;
			Point origin = this.GetCollisionOrigin(cell);
			int size = CollisionColumns * cell;
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(this.collisionBitmap, new Rectangle(origin.X, origin.Y, size, size));
			g.PixelOffsetMode = PixelOffsetMode.Default;
			if (this.chkShowGrid.Checked)
			{
				using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
				{
					for (int i = 0; i <= CollisionColumns; i++)
					{
						g.DrawLine(pen, origin.X + i * cell, origin.Y, origin.X + i * cell, origin.Y + size);
						g.DrawLine(pen, origin.X, origin.Y + i * cell, origin.X + size, origin.Y + i * cell);
					}
				}
			}
			if (this.collisionHoverIndex >= 0 && this.collisionHoverIndex != this.selectedCollisionIndex)
			{
				using (Pen pen = new Pen(Color.White, 2f))
				{
					g.DrawRectangle(pen, this.GetCollisionCellRect(this.collisionHoverIndex, origin, cell));
				}
			}
			using (Pen pen = new Pen(Color.FromArgb(255, 214, 64), Math.Max(2f, scale + 1f)))
			{
				g.DrawRectangle(pen, this.GetCollisionCellRect(this.selectedCollisionIndex, origin, cell));
			}
		}

		//-------------------------------------------------------------------------------
		// 移動エリアの 1 マスの枠（内側に 1px 寄せる）を返す処理
		//-------------------------------------------------------------------------------
		private Rectangle GetCollisionCellRect(int index, Point origin, int cell)
		{
			return new Rectangle(origin.X + index % CollisionColumns * cell + 1, origin.Y + index / CollisionColumns * cell + 1, cell - 2, cell - 2);
		}

		//-------------------------------------------------------------------------------
		// クリックした位置の移動エリアを選ぶ処理
		//-------------------------------------------------------------------------------
		private void SelectCollisionAt(Point point)
		{
			int index = this.HitTestCollision(point);
			if (index >= 0)
			{
				this.selectedCollisionIndex = index;
				this.pnlCollisionPalette.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// マウスが乗った移動エリアを枠で示し、説明欄にその意味を出す処理
		//-------------------------------------------------------------------------------
		private void pnlCollisionPalette_MouseMove(object sender, MouseEventArgs e)
		{
			int index = this.HitTestCollision(e.Location);
			if (index != this.collisionHoverIndex)
			{
				this.collisionHoverIndex = index;
				this.pnlCollisionPalette.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// マウスが一覧から出たら、説明欄を選択中の移動エリアに戻す処理
		//-------------------------------------------------------------------------------
		private void pnlCollisionPalette_MouseLeave(object sender, EventArgs e)
		{
			this.collisionHoverIndex = -1;
			this.pnlCollisionPalette.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 説明欄を更新する処理（マウスが乗っていればその値、無ければ選択中の値を説明する）
		//-------------------------------------------------------------------------------
		private void UpdateCollisionInfo()
		{
			if (this.lblCollisionTitle == null)
			{
				return;
			}
			bool hovering = this.collisionHoverIndex >= 0;
			int value = hovering ? this.collisionHoverIndex : this.selectedCollisionIndex;
			string title;
			string detail;
			DescribeCollision(value, out title, out detail);
			string text = string.Format(Localizer.T(hovering ? "カーソル {0:X2} : {1}" : "選択中 {0:X2} : {1}"), value, title);
			if (this.lblCollisionTitle.Text != text)
			{
				this.lblCollisionTitle.Text = text;
				this.lblCollisionDetail.Text = detail;
				this.lblCollisionTitle.ForeColor = hovering ? UiTheme.TextMuted : UiTheme.Text;
			}
		}

		//-------------------------------------------------------------------------------
		// 移動エリアの値の意味（見出しと説明）を返す処理
		// 高さの表記は AdvanceMap に合わせる（水面を高さ 0、ふつうの地面を高さ 2 とする。内部の値は 1 大きい）
		//-------------------------------------------------------------------------------
		internal static void DescribeCollision(int value, out string title, out string detail)
		{
			int elevation = value >> 2;
			bool blocked = (value & 3) != 0;
			string oddNote = (value & 3) >= 2 ? Localizer.T("（ゲームでは通れない扱い。ふつうは 1 つ前の奇数の値を使います）") : string.Empty;
			if (elevation == 0)
			{
				if (!blocked)
				{
					title = Localizer.T("高さの切り替え");
					detail = Localizer.T("はしご・階段・段差の境目などに使います。どの高さからでも出入りでき、ここを通ると高さが切り替わります。");
				}
				else
				{
					title = Localizer.T("通れない");
					detail = Localizer.T("壁・木・建物など。どの高さからも通れません。いちばんよく使う「通れない」です。") + oddNote;
				}
				return;
			}
			if (elevation == 15)
			{
				if (!blocked)
				{
					title = Localizer.T("橋（上からも下からも通れる）");
					detail = Localizer.T("橋に使います。橋の上を歩くときも、橋の下をくぐるとき（なみのりなど）も通れます。");
				}
				else
				{
					title = Localizer.T("通れない・橋の高さ");
					detail = Localizer.T("橋の高さにある障害物です。") + oddNote;
				}
				return;
			}
			int height = elevation - 1;
			if (elevation == 1)
			{
				title = blocked ? Localizer.T("通れない・高さ 0（水面）") : Localizer.T("水面・高さ 0（なみのり）");
				detail = blocked
					? Localizer.T("水面の高さにある障害物（水の中の岩など）です。") + oddNote
					: Localizer.T("海・川・池など、なみのりで進む水の上に使います。");
				return;
			}
			if (elevation == 3)
			{
				title = blocked ? Localizer.T("通れない・高さ 2") : Localizer.T("通れる・高さ 2（ふつうの地面）");
				detail = blocked
					? Localizer.T("ふつうの地面の高さにある障害物（看板・柵など）です。") + oddNote
					: Localizer.T("多くのマップで標準の、歩ける地面です。");
				return;
			}
			title = string.Format(Localizer.T(blocked ? "通れない・高さ {0}" : "通れる・高さ {0}"), height);
			detail = blocked
				? string.Format(Localizer.T("高さ {0} にある障害物です。"), height) + oddNote
				: string.Format(Localizer.T("高さ {0} の歩ける場所です（段差の上・高台など）。同じ高さの場所どうしでしか行き来できず、違う高さへは 00（高さの切り替え）を通って移ります。"), height);
		}
	}
}
