using System;
using System.Drawing;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「大きさを変える…」で、新しい大きさと、広がった部分を埋めるブロックを決めてもらう画面
	// 入力が変わるたびに、必要な空き領域や注意（はみ出すイベント・形を使い回しているマップなど）を下に出す
	//-------------------------------------------------------------------------------
	public partial class MapResizeForm : Form
	{
		// 入力の内容（幅・高さ・ボーダーの幅・高さ）から、下に出す説明と、実行してよいかを返す処理
		internal delegate string DescribeHandler(int width, int height, int borderWidth, int borderHeight, int shiftX, int shiftY, out bool allowed);

		private readonly DescribeHandler describe;
		private readonly Func<int, Bitmap> blockImage;
		private bool ready;
		// ずらすマス数の前の値（変えた分だけ幅・高さも変えるため）
		private int lastShiftX;
		private int lastShiftY;

		// 決めた内容
		internal int NewWidth { get { return (int)this.nudResizeWidth.Value; } }
		internal int NewHeight { get { return (int)this.nudResizeHeight.Value; } }
		internal int NewBorderWidth { get { return (int)this.nudResizeBorderWidth.Value; } }
		internal int NewBorderHeight { get { return (int)this.nudResizeBorderHeight.Value; } }
		internal int FillBlock { get { return (int)this.nudResizeFillBlock.Value; } }
		// 中身をずらすマス数（左側に足す列数・上側に足す行数。負なら左・上を削る）
		internal int ShiftX { get { return (int)this.nudResizeShiftX.Value; } }
		internal int ShiftY { get { return (int)this.nudResizeShiftY.Value; } }

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public MapResizeForm()
		{
			this.InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// 今の大きさ・ボーダーを変えられるか・初めの埋めるブロック・ブロックの総数と、説明／ブロックの絵を作る処理を受け取って初期化する処理
		//-------------------------------------------------------------------------------
		internal MapResizeForm(int width, int height, int borderWidth, int borderHeight, bool borderEditable, int fillBlock, int blockCount,
			Func<int, Bitmap> blockImage, DescribeHandler describe) : this()
		{
			Localizer.Apply(this);
			this.describe = describe;
			this.blockImage = blockImage;
			this.lblResizeCurrent.Text = string.Format(Localizer.T("今の大きさ : 幅 {0} × 高さ {1}"), width, height);
			this.nudResizeWidth.Value = Math.Max(this.nudResizeWidth.Minimum, Math.Min(this.nudResizeWidth.Maximum, width));
			this.nudResizeHeight.Value = Math.Max(this.nudResizeHeight.Minimum, Math.Min(this.nudResizeHeight.Maximum, height));
			this.nudResizeBorderWidth.Value = Math.Max(this.nudResizeBorderWidth.Minimum, Math.Min(this.nudResizeBorderWidth.Maximum, borderWidth));
			this.nudResizeBorderHeight.Value = Math.Max(this.nudResizeBorderHeight.Minimum, Math.Min(this.nudResizeBorderHeight.Maximum, borderHeight));
			// ボーダーの大きさを持たないゲーム（エメラルド）では、ボーダーの欄を押せなくする
			this.nudResizeBorderWidth.Enabled = borderEditable;
			this.nudResizeBorderHeight.Enabled = borderEditable;
			this.nudResizeFillBlock.Maximum = Math.Max(0, Math.Min(1023, blockCount - 1));
			this.nudResizeFillBlock.Value = Math.Max(0, Math.Min((int)this.nudResizeFillBlock.Maximum, fillBlock));
			this.ready = true;
			this.UpdateInfo();
		}

		//-------------------------------------------------------------------------------
		// 入力が変わったら、説明とブロックの絵を書き直す処理
		//-------------------------------------------------------------------------------
		private void ResizeValue_Changed(object sender, EventArgs e)
		{
			if (this.ready)
			{
				this.UpdateInfo();
			}
		}

		//-------------------------------------------------------------------------------
		// 説明（必要な空き領域・注意）と、埋めるブロックの絵を出し、実行ボタンの押せる／押せないを合わせる処理
		//-------------------------------------------------------------------------------
		private void UpdateInfo()
		{
			bool allowed = true;
			string text = this.describe != null ? this.describe(this.NewWidth, this.NewHeight, this.NewBorderWidth, this.NewBorderHeight, this.ShiftX, this.ShiftY, out allowed) : string.Empty;
			this.lblResizeInfo.Text = text;
			this.btnResizeOk.Enabled = allowed;
			Image old = this.picResizeFillBlock.Image;
			this.picResizeFillBlock.Image = this.blockImage != null ? this.blockImage(this.FillBlock) : null;
			if (old != null)
			{
				old.Dispose();
			}
		}

		//-------------------------------------------------------------------------------
		// 左側に足す列数・上側に足す行数が変わったら、その分だけ新しい幅・高さも変える処理（足した分の大きさが自然に増えるように）
		//-------------------------------------------------------------------------------
		private void ResizeShift_Changed(object sender, EventArgs e)
		{
			if (!this.ready)
			{
				return;
			}
			int deltaX = this.ShiftX - this.lastShiftX;
			int deltaY = this.ShiftY - this.lastShiftY;
			this.lastShiftX = this.ShiftX;
			this.lastShiftY = this.ShiftY;
			this.ready = false;
			try
			{
				this.nudResizeWidth.Value = Math.Max(this.nudResizeWidth.Minimum, Math.Min(this.nudResizeWidth.Maximum, this.nudResizeWidth.Value + deltaX));
				this.nudResizeHeight.Value = Math.Max(this.nudResizeHeight.Minimum, Math.Min(this.nudResizeHeight.Maximum, this.nudResizeHeight.Value + deltaY));
			}
			finally
			{
				this.ready = true;
			}
			this.UpdateInfo();
		}

		//-------------------------------------------------------------------------------
		// 「大きさを変える」: 決めた内容を返して閉じる処理
		//-------------------------------------------------------------------------------
		private void btnResizeOk_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
