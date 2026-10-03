using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップ作成補助」の画面の、2 段階目「サポート作成」の部分（右の枠）
	// 今のマップの写しを MapAssistSupport で書き換え、変わったマスだけをマップエディタへ渡す（MapAssistContext.ApplyHandler）。
	// 渡した後は、マップエディタから今の内容を渡し直してもらい、選んでいた範囲を選び直す
	//-------------------------------------------------------------------------------
	public partial class MapAssistForm
	{
		// ブロックごとに付ける移動エリアの表（ROM の全マップから数えた分。マップの材料が変わったら作り直す）
		private int[] supportCollisionsFromMaps;
		private MapAssistContext supportCollisionsContext;
		// ROM の全マップの並び（縁を整えるときに、岸のブロックを見分けるのに使う）
		private MapAssistAutoInput supportSamples;

		//-------------------------------------------------------------------------------
		// ブロックごとに付ける移動エリアの表を返す処理（ROM の全マップの集計は、同じ材料の間は 1 回だけ作る）
		//-------------------------------------------------------------------------------
		private int[] SupportCollisionTable()
		{
			if (this.supportCollisionsContext != this.context || this.supportCollisionsFromMaps == null)
			{
				MapAssistAutoInput input = this.context.AutoInputProvider != null ? this.context.AutoInputProvider() : new MapAssistAutoInput();
				this.supportCollisionsFromMaps = MapAssistAnalyzer.CollisionTable(this.context, input);
				this.supportSamples = input;
				this.supportCollisionsContext = this.context;
			}
			return MapAssistSupport.BuildCollisionTable(this.context, this.set.Parts, this.supportCollisionsFromMaps);
		}

		//-------------------------------------------------------------------------------
		// 選んだパーツで、マップの範囲を塗る処理（結果の案内文を返す）
		//-------------------------------------------------------------------------------
		internal string SupportPaint(Rectangle range)
		{
			if (this.context == null || this.set == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			MapAssistPart part = this.SelectedPart;
			if (part == null)
			{
				return Localizer.T("先に、塗るのに使うパーツを真ん中の一覧で選んでください。");
			}
			if (range.IsEmpty)
			{
				return Localizer.T("左の「マップ」タブで、塗る範囲をドラッグで選んでください（部品は、置く場所の左上を選びます）。");
			}
			MapAssistWork work = MapAssistWork.FromContext(this.context);
			if (work == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			MapAssistSupportReport report = new MapAssistSupportReport();
			int member = part.Kind == MapAssistKind.Area ? this.selectedMember : -1;
			int[] table = this.SupportCollisionTable();
			string error = MapAssistSupport.Paint(this.context, work, this.set.Parts, part, range, member, table, this.supportSamples, report);
			if (error != null)
			{
				return error;
			}
			return this.ApplySupportWork(work, range, string.Format(Localizer.T("「{0}」で塗りました（{1} マス。縁を直したマス {2}）。"), part.Name, report.Painted, report.EdgeFixed));
		}

		//-------------------------------------------------------------------------------
		// マップの範囲を整える処理（範囲が空ならマップ全体。結果の案内文を返す）
		//-------------------------------------------------------------------------------
		internal string SupportTidy(Rectangle range, bool edges, bool repeats, bool collisions)
		{
			if (this.context == null || this.set == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			if (!edges && !repeats && !collisions)
			{
				return Localizer.T("整える内容（チェック）を 1 つ以上選んでください。");
			}
			MapAssistWork work = MapAssistWork.FromContext(this.context);
			if (work == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			if (edges && !this.set.Parts.Any(p => p.Kind == MapAssistKind.Edge) && !repeats && !collisions)
			{
				return Localizer.T("縁つきのパーツがありません。先に「＋ 縁つき」か「マップから候補を作る（自動）」でパーツを作ってください。");
			}
			Rectangle target = range.IsEmpty ? new Rectangle(0, 0, work.Width, work.Height) : range;
			MapAssistSupportReport report = new MapAssistSupportReport();
			int[] table = this.SupportCollisionTable();
			MapAssistSupport.Tidy(this.context, work, this.set.Parts, target, edges, repeats, collisions, table, this.supportSamples, report);
			string summary = string.Format(Localizer.T("整えました（{0}）: 縁 {1} マス、くり返し {2} マス、移動エリア {3} マス。"),
				range.IsEmpty ? Localizer.T("マップ全体") : string.Format(Localizer.T("({0}, {1}) から {2}×{3}"), range.X, range.Y, range.Width, range.Height),
				report.EdgeFixed, report.RepeatFixed, report.CollisionSet);
			if (report.EdgeSkipped > 0)
			{
				summary += string.Format(Localizer.T("幅 1 マスの所や、枠が空のために直せなかった縁が {0} マスあります。"), report.EdgeSkipped);
			}
			return this.ApplySupportWork(work, range, summary);
		}

		//-------------------------------------------------------------------------------
		// 書き換えた写しを、編集中のマップへ入れる処理（変わる所が無ければ入れない）
		// 入れた後は今のマップを読み直し、選んでいた範囲とマップのタブを元に戻す
		//-------------------------------------------------------------------------------
		private string ApplySupportWork(MapAssistWork work, Rectangle range, string summary)
		{
			int changed = work.CountChanges(this.context);
			if (changed == 0)
			{
				return summary + Localizer.T("変わるマスはありませんでした。");
			}
			if (this.context.ApplyHandler == null)
			{
				return Localizer.T("マップへ入れられませんでした（マップエディタとつながっていません）。");
			}
			string error = this.context.ApplyHandler(work.Blocks, work.Collisions);
			if (error != null)
			{
				return error;
			}
			this.ReloadRequested?.Invoke(this, EventArgs.Empty);
			this.tabAssistSource.SelectedTab = this.tabAssistMap;
			if (!range.IsEmpty)
			{
				this.canvasAssistMap.SetSelection(range);
			}
			return summary + string.Format(Localizer.T("変わったマスは {0} です。メインの画面の「戻る」（Ctrl+Z）で元に戻せます。"), changed);
		}

		// 前回の「ランダム作成」の設定（次に開くときの初期値。種は毎回ふり直す）
		private MapAssistRandomOptions lastRandomOptions;

		//-------------------------------------------------------------------------------
		// マップのたたきをランダムに作る処理（範囲が空ならマップ全体。結果の案内文を返す）
		//-------------------------------------------------------------------------------
		internal string SupportRandom(Rectangle range, MapAssistRandomOptions options)
		{
			if (this.context == null || this.set == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			MapAssistWork work = MapAssistWork.FromContext(this.context);
			if (work == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			Rectangle target = range.IsEmpty ? new Rectangle(0, 0, work.Width, work.Height) : range;
			MapAssistSupportReport report = new MapAssistSupportReport();
			int[] table = this.SupportCollisionTable();
			string summary;
			string error = MapAssistRandom.Generate(this.context, work, this.set.Parts, target, options, table, this.supportSamples, report, Localizer.T, out summary);
			if (error != null)
			{
				return error;
			}
			this.lastRandomOptions = options;
			return this.ApplySupportWork(work, range, summary);
		}

		//-------------------------------------------------------------------------------
		// 「ランダム作成…」: 設定の画面を出してから作る
		//-------------------------------------------------------------------------------
		private void btnSupportRandom_Click(object sender, EventArgs e)
		{
			if (this.context == null || this.set == null)
			{
				this.lblAssistStatus.Text = Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
				return;
			}
			Rectangle range = this.canvasAssistMap.Selection;
			using (MapAssistRandomForm form = new MapAssistRandomForm(range, this.lastRandomOptions))
			{
				AppIconHelper.Apply(form);
				UiTheme.Apply(form);
				if (form.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				this.Cursor = Cursors.WaitCursor;
				try
				{
					this.lblAssistStatus.Text = this.SupportRandom(form.WholeMap ? Rectangle.Empty : range, form.Options);
				}
				finally
				{
					this.Cursor = Cursors.Default;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 「選んだパーツで範囲を塗る」
		//-------------------------------------------------------------------------------
		private void btnSupportPaint_Click(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				this.lblAssistStatus.Text = this.SupportPaint(this.canvasAssistMap.Selection);
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		//-------------------------------------------------------------------------------
		// 「範囲を整える」
		//-------------------------------------------------------------------------------
		private void btnSupportTidy_Click(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				this.lblAssistStatus.Text = this.SupportTidy(this.canvasAssistMap.Selection, this.chkSupportEdge.Checked, this.chkSupportRepeat.Checked, this.chkSupportCollision.Checked);
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}
	}
}
