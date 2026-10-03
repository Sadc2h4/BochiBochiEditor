using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 右ペイン（ツール欄）の並び・表示・分離の状態を settings.ini に保存し、次回起動時に戻す処理
	// 保存する行:
	//   ToolPartOrder    = 右ペインの部品の並び（上から、キーをカンマ区切り）
	//   ToolPartHidden   = 隠している部品のキー
	//   ToolPartFloating = 分離している部品のキー（タブ部分は tabs）
	//   ToolPartWindows  = 部品ごとの分離ウィンドウの位置と大きさ（キー:x;y;幅;高さ を | 区切り）
	//   ToolPaneFloating / ToolPaneWindow = 右ペイン全体の分離と、その窓の位置と大きさ
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		//-------------------------------------------------------------------------------
		// 分離を扱える部品を、キーと一緒に返す処理（タブ部分も含める）
		//-------------------------------------------------------------------------------
		private IEnumerable<KeyValuePair<string, ToolPartDocker>> GetToolDockers()
		{
			foreach (ToolPart part in this.toolParts)
			{
				yield return new KeyValuePair<string, ToolPartDocker>(part.Key, part.Docker);
			}
			if (this.toolTabsDocker != null)
			{
				yield return new KeyValuePair<string, ToolPartDocker>("tabs", this.toolTabsDocker);
			}
		}

		//-------------------------------------------------------------------------------
		// 今の状態を settings.ini に書く処理（アプリを閉じるときに呼ぶ）
		//-------------------------------------------------------------------------------
		private void SaveToolLayout()
		{
			try
			{
				// 分離中の部品は右ペインにいないので、並びの最後に付ける（戻すときは右ペインの一番下になる）
				List<ToolPart> docked = this.GetDockedToolParts();
				IEnumerable<string> order = docked.Select(p => p.Key).Concat(this.toolParts.Where(p => !docked.Contains(p)).Select(p => p.Key));
				AppSettings.Set("ToolPartOrder", string.Join(",", order));
				this.SaveMainWindowBounds();
				AppSettings.Set("ToolPartHidden", string.Join(",", this.toolParts.Where(p => !p.Docker.IsFloating && !p.Holder.Visible).Select(p => p.Key)));
				List<KeyValuePair<string, ToolPartDocker>> dockers = this.GetToolDockers().ToList();
				AppSettings.Set("ToolPartFloating", string.Join(",", dockers.Where(d => d.Value.IsFloating).Select(d => d.Key)));
				AppSettings.Set("ToolPartWindows", string.Join("|", dockers.Where(d => !d.Value.FloatBounds.IsEmpty).Select(d => d.Key + ":" + FormatBounds(d.Value.FloatBounds))));
				if (this.isToolPaneFloating && this.toolFloatForm != null && !this.toolFloatForm.IsDisposed && this.toolFloatForm.WindowState == FormWindowState.Normal)
				{
					this.toolFloatBounds = this.toolFloatForm.Bounds;
				}
				AppSettings.Set("ToolPaneFloating", this.isToolPaneFloating ? "1" : "0");
				AppSettings.Set("ToolPaneWindow", this.toolFloatBounds.IsEmpty ? string.Empty : FormatBounds(this.toolFloatBounds));
			}
			catch (Exception)
			{
				// 保存できなくても閉じる処理は続ける
			}
		}

		//-------------------------------------------------------------------------------
		// settings.ini の状態を戻す処理（画面を表示した直後に呼ぶ。窓の位置は画面の範囲内に収める）
		//-------------------------------------------------------------------------------
		private void RestoreToolLayout()
		{
			try
			{
				// 並び: 保存された順に上から並べ、保存に無い部品（新しく増えた部品など）は最初の順で後ろに付ける
				string[] saved = (AppSettings.Get("ToolPartOrder") ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				List<ToolPart> order = saved.Select(k => this.toolParts.FirstOrDefault(p => p.Key == k)).Where(p => p != null).Distinct().ToList();
				order.AddRange(this.toolParts.Where(p => !order.Contains(p)));
				this.pnlToolPane.SuspendLayout();
				for (int i = order.Count - 1; i >= 0; i--)
				{
					this.pnlToolPane.Controls.SetChildIndex(order[i].Holder, this.pnlToolPane.Controls.Count - 1);
				}
				this.pnlToolPane.Controls.SetChildIndex(this.pnlToolHeader, this.pnlToolPane.Controls.Count - 1);
				HashSet<string> hidden = new HashSet<string>((AppSettings.Get("ToolPartHidden") ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
				foreach (ToolPart part in this.toolParts)
				{
					part.Holder.Visible = !hidden.Contains(part.Key);
				}
				this.pnlToolPane.ResumeLayout(true);

				// 窓の位置と大きさ（分離していない部品も、次に分離するときの位置として覚えておく）
				Dictionary<string, ToolPartDocker> dockers = this.GetToolDockers().ToDictionary(d => d.Key, d => d.Value);
				foreach (string entry in (AppSettings.Get("ToolPartWindows") ?? string.Empty).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
				{
					int colon = entry.IndexOf(':');
					Rectangle bounds;
					if (colon > 0 && dockers.ContainsKey(entry.Substring(0, colon)) && TryParseBounds(entry.Substring(colon + 1), out bounds))
					{
						dockers[entry.Substring(0, colon)].FloatBounds = bounds;
					}
				}
				Rectangle paneBounds;
				if (TryParseBounds(AppSettings.Get("ToolPaneWindow"), out paneBounds))
				{
					this.toolFloatBounds = paneBounds;
				}

				// 分離（隠している部品は分離しない）
				foreach (string key in (AppSettings.Get("ToolPartFloating") ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (dockers.ContainsKey(key) && !hidden.Contains(key) && !dockers[key].IsFloating)
					{
						dockers[key].Float(null);
					}
				}
				if (AppSettings.Get("ToolPaneFloating") == "1")
				{
					this.FloatToolPane(null);
				}
			}
			catch (Exception)
			{
				// 設定が壊れていても、起動は続ける（最初の並びのまま）
			}
		}

		//-------------------------------------------------------------------------------
		// 位置と大きさを「x;y;幅;高さ」の文字にする処理
		//-------------------------------------------------------------------------------
		private static string FormatBounds(Rectangle bounds)
		{
			return string.Join(";", new[] { bounds.X, bounds.Y, bounds.Width, bounds.Height }.Select(v => v.ToString(CultureInfo.InvariantCulture)));
		}

		//-------------------------------------------------------------------------------
		// メイン画面の位置と大きさを settings.ini に書く処理（MainWindow = x;y;幅;高さ、MainWindowMaximized = 1/0）
		// 最大化中は、最大化を解いたときの位置と大きさを書く
		//-------------------------------------------------------------------------------
		private void SaveMainWindowBounds()
		{
			Rectangle bounds = this.WindowState == FormWindowState.Normal ? this.Bounds : this.RestoreBounds;
			if (bounds.Width > 0 && bounds.Height > 0)
			{
				AppSettings.Set("MainWindow", FormatBounds(bounds));
			}
			AppSettings.Set("MainWindowMaximized", this.WindowState == FormWindowState.Maximized ? "1" : "0");
		}

		//-------------------------------------------------------------------------------
		// メイン画面の位置と大きさを settings.ini から戻す処理（画面を出す前に呼ぶ）
		// 記録が無ければ（初回）、作業領域いっぱいより少し小さい大きさで中央に出す。記録は画面の範囲内に収める
		//-------------------------------------------------------------------------------
		private void RestoreMainWindowBounds()
		{
			try
			{
				Rectangle saved;
				if (TryParseBounds(AppSettings.Get("MainWindow"), out saved) && saved.Width >= this.MinimumSize.Width && saved.Height >= this.MinimumSize.Height)
				{
					this.StartPosition = FormStartPosition.Manual;
					this.Bounds = this.ClampToWorkingArea(saved);
				}
				else
				{
					Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
					int margin = this.LogicalToDeviceUnits(24);
					Size size = new Size(Math.Max(this.MinimumSize.Width, area.Width - margin * 2), Math.Max(this.MinimumSize.Height, area.Height - margin * 2));
					this.StartPosition = FormStartPosition.Manual;
					this.Bounds = this.ClampToWorkingArea(new Rectangle(area.Left + (area.Width - size.Width) / 2, area.Top + (area.Height - size.Height) / 2, size.Width, size.Height));
				}
				if (AppSettings.Get("MainWindowMaximized") == "1")
				{
					this.WindowState = FormWindowState.Maximized;
				}
			}
			catch (Exception)
			{
				// 戻せなければ既定の大きさのまま
			}
		}

		//-------------------------------------------------------------------------------
		// 「x;y;幅;高さ」の文字を位置と大きさに戻す処理（読めなければ false）
		//-------------------------------------------------------------------------------
		private static bool TryParseBounds(string text, out Rectangle bounds)
		{
			bounds = Rectangle.Empty;
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			string[] parts = text.Split(';');
			int[] values = new int[4];
			if (parts.Length != 4)
			{
				return false;
			}
			for (int i = 0; i < 4; i++)
			{
				if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out values[i]))
				{
					return false;
				}
			}
			if (values[2] <= 0 || values[3] <= 0)
			{
				return false;
			}
			bounds = new Rectangle(values[0], values[1], values[2], values[3]);
			return true;
		}
	}
}
