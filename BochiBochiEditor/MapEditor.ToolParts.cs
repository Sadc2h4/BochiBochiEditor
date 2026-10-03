using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 右ペイン（ツール欄）の部品ごとの表示／非表示・並べ替え・分離
	// 見出しの「≡」ボタンのメニューから操作する。右ペインに入っている間は部品ごとの見出しを出さず（縦幅を使わない）、
	// 分離している間だけ、その窓に見出しを出して、見出しのドラッグやボタンで元へ戻せるようにする
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 右ペインの部品 1 つ分
		private sealed class ToolPart
		{
			public string Key;
			public Func<string> Title;
			// 「表示機能の選択」の画面に出す説明
			public Func<string> Description;
			// 右ペインに入れる入れ物（分離・並べ替えはこれを動かす。タウンマップは部品そのもの）
			public Control Holder;
			// 中身（ROM 未読込のときに無効にする部分）
			public Control Content;
			// 分離中だけ出す見出し（タウンマップは自前の見出しがあるので null）
			public Control Header;
			public ToolPartDocker Docker;
		}

		//-------------------------------------------------------------------------------
		// 部品を包む入れ物。右ペインに入っている間は、並べ直すたびに見えている中身の高さの合計へ自分の高さを合わせる
		// （起動時の DPI 拡大は入れ物と中身を別々に拡大するので、決めた高さを後から合わせ直す必要がある）
		//-------------------------------------------------------------------------------
		private sealed class ToolPartHolder : Panel
		{
			public bool FitToContent { get; set; } = true;

			protected override void OnLayout(LayoutEventArgs levent)
			{
				base.OnLayout(levent);
				if (!this.FitToContent)
				{
					return;
				}
				int height = this.Padding.Vertical;
				foreach (Control control in this.Controls)
				{
					if (control.Visible)
					{
						height += control.Height;
					}
				}
				if (height > 0 && this.Height != height)
				{
					this.Height = height;
				}
			}
		}

		// 部品の一覧（初期の並び順＝上から）
		private readonly List<ToolPart> toolParts = new List<ToolPart>();
		// 画像ボタン「表示機能の選択」を見出しのすぐ下へ置き直している最中か（置き直しがまた並べ直しを呼ぶため）
		private bool placingFeatureButton;

		//-------------------------------------------------------------------------------
		// 部品を登録し、見出しにメニューのボタンを置く処理（タウンマップの初期化のあとに呼ぶ）
		//-------------------------------------------------------------------------------
		private void InitializeToolParts()
		{
			this.AddWrappedToolPart("actions", () => Localizer.T("操作ボタン"), () => Localizer.T("戻る・やり直し、スクリプト編集、ブロック編集のボタン"), this.flpToolActions);
			this.AddWrappedToolPart("import", () => Localizer.T("マップチップ取り込み"), () => Localizer.T("画像からマップチップ（タイルとブロック）を取り込む"), this.btnImportChips);
			this.AddWrappedToolPart("mapmove", () => Localizer.T("マップ移動"), () => Localizer.T("浮上・潜水の先のマップへ移る、タイルアニメを再生する"), this.flpToolMapMove);
			this.toolParts.Add(new ToolPart { Key = "townmap", Title = () => Localizer.T("タウンマップ"), Description = () => Localizer.T("今のマップがタウンマップのどこかを見る、大きさを比べる"), Holder = this.pnlTownMapPart, Content = this.townMapView, Docker = this.townMapDocker });
			this.AddWrappedToolPart("showevent", () => Localizer.T("イベント表示の切り替え"), () => Localizer.T("人物・看板・踏むスクリプト・ワープを、マップに出す／隠す"), this.pnlShowEvent);
			this.AddWrappedToolPart("info", () => Localizer.T("選択ブロック・ボーダー"), () => Localizer.T("選んでいるブロックの番号と絵、マップの外側（ボーダー）の編集"), this.pnlToolInfo);

			// 画像ボタン「表示機能の選択」は、どの並びにしても見出しのすぐ下に置く
			this.pnlToolPane.Layout += (sender, e) => this.PlaceFeatureSelectButton();
			this.UpdateFeatureSelectButton();
			this.PlaceFeatureSelectButton();
		}

		//-------------------------------------------------------------------------------
		// 部品を入れ物で包んで登録する処理（入れ物に、分離中だけ出す見出しを付ける）
		//-------------------------------------------------------------------------------
		private void AddWrappedToolPart(string key, Func<string> title, Func<string> description, Control content)
		{
			int index = this.pnlToolPane.Controls.GetChildIndex(content);
			ToolPartHolder holder = new ToolPartHolder { Dock = DockStyle.Top, Name = "pnlToolPart_" + key };
			Panel header = new Panel { Dock = DockStyle.Top, Height = this.pnlToolPane.LogicalToDeviceUnits(24), Visible = false, Cursor = Cursors.SizeAll };
			Label titleLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0), AutoEllipsis = true };
			Button floatButton = new Button { Dock = DockStyle.Right, Width = this.pnlToolPane.LogicalToDeviceUnits(28), UseVisualStyleBackColor = true };
			header.Controls.Add(titleLabel);
			header.Controls.Add(floatButton);

			this.pnlToolPane.SuspendLayout();
			this.pnlToolPane.Controls.Remove(content);
			content.Dock = DockStyle.Top;
			holder.Controls.Add(content);
			holder.Controls.Add(header);
			holder.Height = content.Height;
			this.pnlToolPane.Controls.Add(holder);
			this.pnlToolPane.Controls.SetChildIndex(holder, index);
			this.pnlToolPane.ResumeLayout(true);

			ToolPart part = new ToolPart { Key = key, Title = title, Description = description, Holder = holder, Content = content, Header = header };
			part.Docker = new ToolPartDocker(this, holder, header, titleLabel, floatButton,
				() => title(),
				() => title() + Localizer.T("　（元の場所へドラッグで戻す）"),
				() => title(),
				this.MapEditor_KeyDown);
			// 分離中は見出しを出して窓いっぱいに広げ、戻したら見出しを隠して中身の高さに合わせる
			part.Docker.StateChanged += (sender, e) =>
			{
				header.Visible = part.Docker.IsFloating;
				holder.FitToContent = !part.Docker.IsFloating;
				holder.Dock = part.Docker.IsFloating ? DockStyle.Fill : DockStyle.Top;
				holder.PerformLayout();
				this.UpdateToolFloatButton();
			};
			this.toolParts.Add(part);
		}

		//-------------------------------------------------------------------------------
		// 右ペインに入っている部品を、上から順に返す処理（Dock=Top は並び順の大きいものほど上に来る）
		//-------------------------------------------------------------------------------
		private List<ToolPart> GetDockedToolParts()
		{
			return this.toolParts
				.Where(p => !p.Docker.IsFloating && p.Holder.Parent == this.pnlToolPane)
				.OrderByDescending(p => this.pnlToolPane.Controls.GetChildIndex(p.Holder))
				.ToList();
		}

		//-------------------------------------------------------------------------------
		// 画像ボタン「表示機能の選択」の画像（今の言語のもの）とツールチップを設定する処理
		//-------------------------------------------------------------------------------
		private void UpdateFeatureSelectButton()
		{
			if (this.btnFeatureSelect == null)
			{
				return;
			}
			this.btnFeatureSelect.ButtonImage = this.LoadButtonImage("Feature_Selection");
			this.mapToolTip?.SetToolTip(this.btnFeatureSelect, Localizer.T("表示機能の選択: 右のツール欄に出す機能（マップチップ取り込み・タウンマップ・イベント表示の切り替えなど）を選ぶ・並べ替える"));
		}

		//-------------------------------------------------------------------------------
		// 画像ボタン「表示機能の選択」を、見出しのすぐ下へ置き直す処理（Dock=Top は並び順の大きいものほど上に来る）
		//-------------------------------------------------------------------------------
		private void PlaceFeatureSelectButton()
		{
			if (this.placingFeatureButton || this.btnFeatureSelect == null || this.btnFeatureSelect.Parent != this.pnlToolPane || this.pnlToolHeader.Parent != this.pnlToolPane)
			{
				return;
			}
			Control.ControlCollection controls = this.pnlToolPane.Controls;
			int header = controls.GetChildIndex(this.pnlToolHeader);
			int button = controls.GetChildIndex(this.btnFeatureSelect);
			if (header == controls.Count - 1 && button == controls.Count - 2)
			{
				return;
			}
			this.placingFeatureButton = true;
			try
			{
				controls.SetChildIndex(this.btnFeatureSelect, controls.Count - 1);
				controls.SetChildIndex(this.pnlToolHeader, controls.Count - 1);
			}
			finally
			{
				this.placingFeatureButton = false;
			}
		}

		//-------------------------------------------------------------------------------
		// 「表示機能の選択」: 選択画面を開き、決めた内容（出す機能と並び）を右のツール欄に反映する処理
		//-------------------------------------------------------------------------------
		private void btnFeatureSelect_Click(object sender, EventArgs e)
		{
			List<ToolPart> docked = this.GetDockedToolParts();
			// 右ペインに入っている部品を上から、そのあとに分離中の部品
			List<ToolPartSelectForm.Entry> entries = docked.Concat(this.toolParts.Where(q => !docked.Contains(q)))
				.Select(q => new ToolPartSelectForm.Entry
				{
					Key = q.Key,
					Title = q.Title() + (q.Docker.IsFloating ? Localizer.T("（分離中）") : string.Empty),
					Description = q.Description != null ? q.Description() : string.Empty,
					Visible = q.Docker.IsFloating || q.Holder.Visible,
				}).ToList();
			using (ToolPartSelectForm form = new ToolPartSelectForm(entries))
			{
				AppIconHelper.Apply(form);
				UiTheme.Apply(form);
				if (form.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				if (form.ResetRequested)
				{
					this.ResetToolParts();
					return;
				}
				List<ToolPartSelectForm.Entry> result = form.GetResult();
				this.ApplyToolPartSelection(result.Select(r => r.Key).ToList(), new HashSet<string>(result.Where(r => r.Visible).Select(r => r.Key)));
			}
		}

		//-------------------------------------------------------------------------------
		// 出す機能と並び（上から、キーの順）を右のツール欄に反映する処理
		// 隠す機能が分離中なら先に右ペインへ戻す。分離中のまま出す機能は、窓をそのままにする
		//-------------------------------------------------------------------------------
		internal void ApplyToolPartSelection(IList<string> order, ISet<string> visibleKeys)
		{
			List<ToolPart> ordered = order.Select(k => this.toolParts.FirstOrDefault(q => q.Key == k)).Where(q => q != null).Distinct().ToList();
			ordered.AddRange(this.toolParts.Where(q => !ordered.Contains(q)));
			this.pnlToolPane.SuspendLayout();
			foreach (ToolPart part in ordered)
			{
				bool visible = visibleKeys.Contains(part.Key);
				if (!visible && part.Docker.IsFloating)
				{
					part.Docker.Dock();
				}
				if (!part.Docker.IsFloating)
				{
					part.Holder.Visible = visible;
				}
			}
			// 下の部品から順に、見出しのすぐ下へ入れていくと、上からの並びになる
			for (int i = ordered.Count - 1; i >= 0; i--)
			{
				if (ordered[i].Holder.Parent == this.pnlToolPane)
				{
					this.pnlToolPane.Controls.SetChildIndex(ordered[i].Holder, this.pnlToolPane.Controls.Count - 1);
				}
			}
			this.pnlToolPane.Controls.SetChildIndex(this.pnlToolHeader, this.pnlToolPane.Controls.Count - 1);
			this.PlaceFeatureSelectButton();
			this.pnlToolPane.ResumeLayout(true);
			this.SaveToolLayout();
		}

		//-------------------------------------------------------------------------------
		// 部品を出す／隠す処理（分離中に隠すときは、先に右ペインへ戻す）
		//-------------------------------------------------------------------------------
		private void SetToolPartVisible(ToolPart part, bool visible)
		{
			if (!visible && part.Docker.IsFloating)
			{
				part.Docker.Dock();
			}
			part.Holder.Visible = visible;
		}

		//-------------------------------------------------------------------------------
		// すべての部品を、右ペインに戻して表示し、最初の並び順に戻す処理
		//-------------------------------------------------------------------------------
		private void ResetToolParts()
		{
			// タブ部分と右ペイン全体の分離も戻す
			if (this.toolTabsDocker != null && this.toolTabsDocker.IsFloating)
			{
				this.toolTabsDocker.Dock();
			}
			if (this.isToolPaneFloating)
			{
				this.DockToolPane();
			}
			this.pnlToolPane.SuspendLayout();
			foreach (ToolPart part in this.toolParts)
			{
				if (part.Docker.IsFloating)
				{
					part.Docker.Dock();
				}
				part.Holder.Visible = true;
			}
			// 下の部品から順に、見出しのすぐ下へ入れていくと、最初の並び（登録順）になる
			for (int i = this.toolParts.Count - 1; i >= 0; i--)
			{
				this.pnlToolPane.Controls.SetChildIndex(this.toolParts[i].Holder, this.pnlToolPane.Controls.Count - 1);
			}
			this.pnlToolPane.Controls.SetChildIndex(this.pnlToolHeader, this.pnlToolPane.Controls.Count - 1);
			this.PlaceFeatureSelectButton();
			this.pnlToolPane.ResumeLayout(true);
		}
	}
}
