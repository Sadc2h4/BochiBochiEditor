using System;
using System.Drawing;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「ランダム作成」の設定を決めてもらう画面（種・割合・建物の数・囲み・道・対象の範囲）
	// 決めた内容は Options と WholeMap で返す。作るのは呼び出し側（MapAssistForm）
	//-------------------------------------------------------------------------------
	public partial class MapAssistRandomForm : Form
	{
		private readonly Random seeds = new Random();

		// 決めた設定
		internal MapAssistRandomOptions Options
		{
			get
			{
				return new MapAssistRandomOptions
				{
					Seed = (int)this.nudRandomSeed.Value,
					WaterPercent = (int)this.nudRandomWater.Value,
					ForestPercent = (int)this.nudRandomForest.Value,
					GrassPercent = (int)this.nudRandomGrass.Value,
					Buildings = (int)this.nudRandomBuildings.Value,
					DecorationPercent = (int)this.nudRandomDecoration.Value,
					ForestBorder = this.chkRandomBorder.Checked,
					Paths = this.chkRandomPaths.Checked,
				};
			}
		}

		// マップ全体に作るか（false なら選んでいる範囲）
		internal bool WholeMap
		{
			get { return this.rdoRandomWhole.Checked; }
		}

		//-------------------------------------------------------------------------------
		// 画面を作る処理。range は左の「マップ」タブで選んでいる範囲（空なら「マップ全体」だけ選べる）
		//-------------------------------------------------------------------------------
		internal MapAssistRandomForm(Rectangle range, MapAssistRandomOptions last)
		{
			this.InitializeComponent();
			Localizer.Apply(this);
			if (last != null)
			{
				this.nudRandomWater.Value = Math.Max(this.nudRandomWater.Minimum, Math.Min(this.nudRandomWater.Maximum, last.WaterPercent));
				this.nudRandomForest.Value = Math.Max(this.nudRandomForest.Minimum, Math.Min(this.nudRandomForest.Maximum, last.ForestPercent));
				this.nudRandomGrass.Value = Math.Max(this.nudRandomGrass.Minimum, Math.Min(this.nudRandomGrass.Maximum, last.GrassPercent));
				this.nudRandomBuildings.Value = Math.Max(this.nudRandomBuildings.Minimum, Math.Min(this.nudRandomBuildings.Maximum, last.Buildings));
				this.nudRandomDecoration.Value = Math.Max(this.nudRandomDecoration.Minimum, Math.Min(this.nudRandomDecoration.Maximum, last.DecorationPercent));
				this.chkRandomBorder.Checked = last.ForestBorder;
				this.chkRandomPaths.Checked = last.Paths;
			}
			this.nudRandomSeed.Value = this.seeds.Next(0, 1000000);
			if (range.IsEmpty)
			{
				this.rdoRandomRange.Enabled = false;
				this.rdoRandomRange.Text = Localizer.T("選んでいる範囲（左の「マップ」タブで範囲を選ぶと使えます）");
				this.rdoRandomWhole.Checked = true;
			}
			else
			{
				this.rdoRandomRange.Text = string.Format(Localizer.T("選んでいる範囲（({0}, {1}) から {2}×{3}）"), range.X, range.Y, range.Width, range.Height);
				this.rdoRandomRange.Checked = true;
			}
		}

		//-------------------------------------------------------------------------------
		// 「種をふる」: 乱数の種を新しくする
		//-------------------------------------------------------------------------------
		private void btnRandomSeed_Click(object sender, EventArgs e)
		{
			this.nudRandomSeed.Value = this.seeds.Next(0, 1000000);
		}

		//-------------------------------------------------------------------------------
		// 「作成する」
		//-------------------------------------------------------------------------------
		private void btnRandomRun_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		//-------------------------------------------------------------------------------
		// 「閉じる」
		//-------------------------------------------------------------------------------
		private void btnRandomCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
