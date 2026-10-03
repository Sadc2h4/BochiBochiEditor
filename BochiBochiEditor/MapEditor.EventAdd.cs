using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// イベント（人・看板・踏むスクリプト・ワープ）の追加と削除
	// 「イベント」のモードで、マップの上を右クリックして「ここに…を追加」、または右の欄の「＋ 追加」「削除」で行う。
	// 追加・削除は編集中の内容（tempHeader の一覧）を変えるだけで、ROM へは今までどおり「編集中のMAPを確定」で書く。
	// 確定のとき、イベントの数が ROM にある入れ物より増えていれば、空き領域に新しい入れ物を作って付け替える
	// （データ作成タブの「イベントを作成」と同じ仕組み。古い入れ物は、ほかのマップが使っていなければ空きに戻す）。
	// 追加・削除は「戻る」「進む」で取り消し・やり直しができる
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// イベントの種類（種類の欄に並ぶ名前の、翻訳前のもの）
		internal const string EventKindPerson = "歩行グラフィック";
		internal const string EventKindSign = "看板";
		internal const string EventKindTrap = "踏むスクリプト";
		internal const string EventKindWarp = "ワープ";
		private static readonly string[] EventKinds = { EventKindPerson, EventKindSign, EventKindTrap, EventKindWarp };

		// 1 つのマップに置ける数（人はゲームが同時に扱える数、ほかは数を 1 バイトで持つため）
		private const int MaxPersonEvents = 64;
		private const int MaxOtherEvents = 255;

		// 「戻る」用の記録 1 つ分（どの種類の何番目に、どのイベントを足したか／消したか）
		internal sealed class EventListChange
		{
			public string Kind;
			public int Index;
			public object Event;
			public bool Added;
		}

		private ContextMenuStrip eventContextMenu;

		//-------------------------------------------------------------------------------
		// 追加・削除のボタンのヒントと、右クリックのメニューを用意する処理（画面の初期化のときに呼ぶ）
		//-------------------------------------------------------------------------------
		private void InitializeEventAddDelete()
		{
			this.eventContextMenu = new ContextMenuStrip();
			this.mapToolTip.SetToolTip(this.btnEventAdd, Localizer.T("上で選んでいる種類のイベントを、見えているマップの中央に 1 つ追加します（追加した後、ドラッグで動かせます）。\nマップの上を右クリックすると、その場所に追加できます"));
			this.mapToolTip.SetToolTip(this.btnEventDelete, Localizer.T("選んでいるイベントを削除します (Delete)。「戻る」で取り消せます"));
		}

		//-------------------------------------------------------------------------------
		// 種類の名前から、編集中のイベントの一覧を返す処理（create = 無ければ空の一覧を作るか）
		//-------------------------------------------------------------------------------
		private IList EventListOf(string kind, bool create)
		{
			if (this.tempHeader == null)
			{
				return null;
			}
			switch (kind)
			{
				case EventKindPerson:
					if (this.tempHeader.Persons == null && create) this.tempHeader.Persons = new List<PersonEvent>();
					return this.tempHeader.Persons;
				case EventKindSign:
					if (this.tempHeader.Signs == null && create) this.tempHeader.Signs = new List<SignEvent>();
					return this.tempHeader.Signs;
				case EventKindTrap:
					if (this.tempHeader.Traps == null && create) this.tempHeader.Traps = new List<TrapEvent>();
					return this.tempHeader.Traps;
				case EventKindWarp:
					if (this.tempHeader.Warps == null && create) this.tempHeader.Warps = new List<WarpEvent>();
					return this.tempHeader.Warps;
				default:
					return null;
			}
		}

		//-------------------------------------------------------------------------------
		// 種類の欄で今選んでいる種類を、翻訳前の名前で返す処理（選んでいなければ null）
		//-------------------------------------------------------------------------------
		private string SelectedEventKind()
		{
			string text = this.cmbEventType.SelectedItem != null ? this.cmbEventType.SelectedItem.ToString() : null;
			return EventKinds.FirstOrDefault(k => k == text || Localizer.T(k) == text);
		}

		//-------------------------------------------------------------------------------
		// 種類と番号で、編集欄のイベントを選ぶ処理
		//-------------------------------------------------------------------------------
		private void SelectEvent(string kind, int index)
		{
			int typeIndex = this.cmbEventType.Items.IndexOf(Localizer.T(kind));
			if (typeIndex < 0)
			{
				typeIndex = this.cmbEventType.Items.IndexOf(kind);
			}
			if (typeIndex >= 0 && this.cmbEventType.SelectedIndex != typeIndex)
			{
				this.cmbEventType.SelectedIndex = typeIndex;
			}
			this.RefreshEventUI();
			if (this.nudEventNo.Enabled && index >= this.nudEventNo.Minimum && index <= this.nudEventNo.Maximum)
			{
				this.nudEventNo.Value = index;
			}
		}

		//-------------------------------------------------------------------------------
		// イベントの一覧が変わった後に、編集欄・マップの表示・「変更あり」の印を合わせる処理
		//-------------------------------------------------------------------------------
		private void AfterEventListChanged()
		{
			this.RefreshEventUI();
			this.UpdateMapRender();
			this.pnlMapCanvas.Invalidate();
			this.SetUnsavedChanges(true);
		}

		//-------------------------------------------------------------------------------
		// 新しいイベントを、初めの値で作る処理
		//   人           … 見た目 0 番、下向きで動かない、階層は今ある人と同じ（無ければ地上）、スクリプトなし。番号は今ある番号の次
		//   看板         … スクリプトの看板、スクリプトなし
		//   踏むスクリプト … スクリプトなし
		//   ワープ       … 行き先は今のマップの 0 番（置いた後で直す）
		//-------------------------------------------------------------------------------
		private object CreateDefaultEvent(string kind, int x, int y)
		{
			ushort px = (ushort)Math.Max(0, x);
			ushort py = (ushort)Math.Max(0, y);
			switch (kind)
			{
				case EventKindPerson:
				{
					List<PersonEvent> persons = this.tempHeader.Persons ?? new List<PersonEvent>();
					int number = persons.Count == 0 ? 1 : persons.Max(q => (int)q.No) + 1;
					return new PersonEvent
					{
						No = (byte)Math.Min(255, number),
						SpriteNo = 0,
						X = px,
						Y = py,
						Layer = persons.Count > 0 ? persons[0].Layer : (byte)3,
						Action = 8,
					};
				}
				case EventKindSign:
					return new SignEvent { X = px, Y = py, Layer = this.tempHeader.Signs != null && this.tempHeader.Signs.Count > 0 ? this.tempHeader.Signs[0].Layer : (byte)0, SignType = 0 };
				case EventKindTrap:
					return new TrapEvent { X = px, Y = py, Layer = this.tempHeader.Traps != null && this.tempHeader.Traps.Count > 0 ? this.tempHeader.Traps[0].Layer : (byte)3 };
				case EventKindWarp:
					return new WarpEvent
					{
						X = px,
						Y = py,
						Layer = this.tempHeader.Warps != null && this.tempHeader.Warps.Count > 0 ? this.tempHeader.Warps[0].Layer : (byte)0,
						WarpToNo = 0,
						MapBank = (byte)this.tempHeader.Bank,
						MapNumber = (byte)this.tempHeader.Number,
					};
				default:
					return null;
			}
		}

		//-------------------------------------------------------------------------------
		// マスにイベントを 1 つ追加する処理（足した番号を返す。足せなければ -1 で、理由を message に入れる）
		//-------------------------------------------------------------------------------
		internal int AddEventAt(string kind, int x, int y, out string message)
		{
			message = null;
			if (this.romData == null || this.tempHeader == null || this.mapMatrix == null)
			{
				message = Localizer.T("先にマップを選んでください。");
				return -1;
			}
			if (this.tempHeader.LoadIncomplete)
			{
				message = Localizer.T("このマップは読み込み時に一部のデータを読めなかったので、イベントを追加できません。");
				return -1;
			}
			IList list = this.EventListOf(kind, true);
			if (list == null)
			{
				return -1;
			}
			int limit = kind == EventKindPerson ? MaxPersonEvents : MaxOtherEvents;
			if (list.Count >= limit)
			{
				message = string.Format(Localizer.T("この種類のイベントは、1 つのマップに {0} 個までです。"), limit);
				return -1;
			}
			x = Math.Max(0, Math.Min(this.mapMatrix.GetLength(0) - 1, x));
			y = Math.Max(0, Math.Min(this.mapMatrix.GetLength(1) - 1, y));
			object created = this.CreateDefaultEvent(kind, x, y);
			EventListChange change = new EventListChange { Kind = kind, Index = list.Count, Event = created, Added = true };
			this.ApplyEventListChange(change, true);
			this.PushEventListChange(change);
			this.SelectEvent(kind, change.Index);
			return change.Index;
		}

		//-------------------------------------------------------------------------------
		// イベントを 1 つ削除する処理（消せたら true）
		//-------------------------------------------------------------------------------
		internal bool DeleteEvent(string kind, int index)
		{
			IList list = this.EventListOf(kind, false);
			if (list == null || index < 0 || index >= list.Count)
			{
				return false;
			}
			EventListChange change = new EventListChange { Kind = kind, Index = index, Event = list[index], Added = false };
			this.ApplyEventListChange(change, true);
			this.PushEventListChange(change);
			if (list.Count > 0)
			{
				this.SelectEvent(kind, Math.Min(index, list.Count - 1));
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// 追加・削除の記録を、「戻る」の履歴に 1 回分として積む処理
		//-------------------------------------------------------------------------------
		private void PushEventListChange(EventListChange change)
		{
			this.EndMapEditStroke();
			this.BeginMapEditStroke();
			this.currentStroke.Add(new MapEditAction { MapX = -1, MapY = -1, EventChange = change });
			this.EndMapEditStroke();
		}

		//-------------------------------------------------------------------------------
		// 追加・削除の記録を一覧に反映する処理（forward = 行うとき・やり直すとき。false = 取り消すとき）
		//-------------------------------------------------------------------------------
		private void ApplyEventListChange(EventListChange change, bool forward)
		{
			IList list = this.EventListOf(change.Kind, true);
			if (list == null)
			{
				return;
			}
			bool insert = change.Added == forward;
			if (insert)
			{
				if (!list.Contains(change.Event))
				{
					list.Insert(Math.Max(0, Math.Min(list.Count, change.Index)), change.Event);
				}
			}
			else
			{
				list.Remove(change.Event);
			}
			this.AfterEventListChanged();
		}

		//-------------------------------------------------------------------------------
		// マスにあるイベント（種類と番号）を集める処理
		//-------------------------------------------------------------------------------
		private List<KeyValuePair<string, int>> EventsAt(int x, int y)
		{
			List<KeyValuePair<string, int>> found = new List<KeyValuePair<string, int>>();
			if (this.tempHeader == null)
			{
				return found;
			}
			if (this.tempHeader.Persons != null)
			{
				for (int i = 0; i < this.tempHeader.Persons.Count; i++)
				{
					if (this.tempHeader.Persons[i].X == x && this.tempHeader.Persons[i].Y == y) found.Add(new KeyValuePair<string, int>(EventKindPerson, i));
				}
			}
			if (this.tempHeader.Signs != null)
			{
				for (int i = 0; i < this.tempHeader.Signs.Count; i++)
				{
					if (this.tempHeader.Signs[i].X == x && this.tempHeader.Signs[i].Y == y) found.Add(new KeyValuePair<string, int>(EventKindSign, i));
				}
			}
			if (this.tempHeader.Traps != null)
			{
				for (int i = 0; i < this.tempHeader.Traps.Count; i++)
				{
					if (this.tempHeader.Traps[i].X == x && this.tempHeader.Traps[i].Y == y) found.Add(new KeyValuePair<string, int>(EventKindTrap, i));
				}
			}
			if (this.tempHeader.Warps != null)
			{
				for (int i = 0; i < this.tempHeader.Warps.Count; i++)
				{
					if (this.tempHeader.Warps[i].X == x && this.tempHeader.Warps[i].Y == y) found.Add(new KeyValuePair<string, int>(EventKindWarp, i));
				}
			}
			return found;
		}

		//-------------------------------------------------------------------------------
		// 「イベント」のモードでマップを右クリックしたときのメニューを出す処理
		// その場所への追加（4 種類）と、その場所にあるイベントの削除を並べる
		//-------------------------------------------------------------------------------
		private void ShowEventContextMenu(Point canvasLocation, int mapX, int mapY)
		{
			if (this.tempHeader == null || !this.IsInsideMap(mapX, mapY))
			{
				return;
			}
			this.eventContextMenu.Items.Clear();
			foreach (string kind in EventKinds)
			{
				string target = kind;
				ToolStripMenuItem add = new ToolStripMenuItem(string.Format(Localizer.T("ここに「{0}」を追加"), Localizer.T(kind)));
				add.Click += (sender, e) => this.AddEventWithMessage(target, mapX, mapY);
				this.eventContextMenu.Items.Add(add);
			}
			ToolStripMenuItem addItem = new ToolStripMenuItem(Localizer.T("ここにアイテム（落とし物）を追加…"));
			addItem.Click += (sender, e) => this.AddItemBallWithDialog(mapX, mapY);
			this.eventContextMenu.Items.Add(addItem);
			List<KeyValuePair<string, int>> here = this.EventsAt(mapX, mapY);
			if (here.Count > 0)
			{
				this.eventContextMenu.Items.Add(new ToolStripSeparator());
				foreach (KeyValuePair<string, int> item in here)
				{
					KeyValuePair<string, int> target = item;
					ToolStripMenuItem select = new ToolStripMenuItem(string.Format(Localizer.T("「{0}」{1} 番を選ぶ"), Localizer.T(item.Key), item.Value));
					select.Click += (sender, e) => this.SelectEvent(target.Key, target.Value);
					ToolStripMenuItem delete = new ToolStripMenuItem(string.Format(Localizer.T("「{0}」{1} 番を削除"), Localizer.T(item.Key), item.Value));
					delete.Click += (sender, e) => this.DeleteEvent(target.Key, target.Value);
					this.eventContextMenu.Items.Add(select);
					this.eventContextMenu.Items.Add(delete);
				}
			}
			this.eventContextMenu.Show(this.pnlMapCanvas, canvasLocation);
		}

		//-------------------------------------------------------------------------------
		// イベントを追加し、足せなかったときは理由を出す処理
		//-------------------------------------------------------------------------------
		private void AddEventWithMessage(string kind, int x, int y)
		{
			string message;
			if (this.AddEventAt(kind, x, y, out message) < 0 && message != null)
			{
				MessageBox.Show(this, message, Localizer.T("イベントの追加"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		//-------------------------------------------------------------------------------
		// 「＋ 追加」: 選んでいる種類のイベントを、見えているマップの中央に追加する処理
		//-------------------------------------------------------------------------------
		private void btnEventAdd_Click(object sender, EventArgs e)
		{
			string kind = this.SelectedEventKind() ?? EventKindPerson;
			Point center = this.CanvasToMapCell(new Point(this.pnlMapCanvas.ClientSize.Width / 2, this.pnlMapCanvas.ClientSize.Height / 2));
			if (this.mapMatrix != null)
			{
				// マップが画面より小さいときは、マップの中央
				if (center.X >= this.mapMatrix.GetLength(0)) center.X = this.mapMatrix.GetLength(0) / 2;
				if (center.Y >= this.mapMatrix.GetLength(1)) center.Y = this.mapMatrix.GetLength(1) / 2;
			}
			this.AddEventWithMessage(kind, center.X, center.Y);
		}

		//-------------------------------------------------------------------------------
		// 「削除」・Delete キー: 選んでいるイベントを削除する処理
		//-------------------------------------------------------------------------------
		private void btnEventDelete_Click(object sender, EventArgs e)
		{
			this.DeleteSelectedEvent();
		}

		//-------------------------------------------------------------------------------
		// 編集欄で選んでいるイベントを削除する処理（消せたら true）
		//-------------------------------------------------------------------------------
		internal bool DeleteSelectedEvent()
		{
			string kind = this.SelectedEventKind();
			if (kind == null || !this.nudEventNo.Enabled)
			{
				return false;
			}
			return this.DeleteEvent(kind, Convert.ToInt32(this.nudEventNo.Value));
		}

		//-------------------------------------------------------------------------------
		// 確定の前に、イベントの入れ物を確かめる処理（書いてよければ true）
		// どれかの種類の数が、ROM にある入れ物の数より増えていたら、空き領域に新しい入れ物（見出し 20 バイト + 4 種類の表）を作り、
		// このマップの付け先をそこへ替える。古い入れ物は、確定の最後に、ほかのマップが使っていなければ空きに戻す。
		// 空き領域が足りないときは、案内を出して false（何も書かない）
		//-------------------------------------------------------------------------------
		internal bool EnsureEventTableRoom()
		{
			if (this.tempHeader == null || this.romData == null)
			{
				return true;
			}
			// 追加した落とし物のスクリプトを先に用意する
			if (!this.WritePendingItemBallScripts())
			{
				return false;
			}
			int persons = this.tempHeader.Persons != null ? this.tempHeader.Persons.Count : 0;
			int warps = this.tempHeader.Warps != null ? this.tempHeader.Warps.Count : 0;
			int traps = this.tempHeader.Traps != null ? this.tempHeader.Traps.Count : 0;
			int signs = this.tempHeader.Signs != null ? this.tempHeader.Signs.Count : 0;
			uint current = this.tempHeader.EventScriptAddress;
			if (current == 0U)
			{
				if (persons + warps + traps + signs == 0)
				{
					return true;
				}
			}
			else if (this.IsRomRange(current, 20)
				&& persons <= this.romData[current] && warps <= this.romData[current + 1] && traps <= this.romData[current + 2] && signs <= this.romData[current + 3])
			{
				return true;
			}
			NewDataGenerator.EventGenerator generator = new NewDataGenerator.EventGenerator
			{
				PersonCount = (byte)persons,
				WarpCount = (byte)warps,
				TrapCount = (byte)traps,
				SignCount = (byte)signs,
			};
			int length = this.CalculateNewEventDataLength(generator);
			uint address = 0U;
			if (!this.TryFindFreeSpaceForNewEvent(length, ref address) || !generator.GenerateData(this.romData, address))
			{
				MessageBox.Show(this, string.Format(Localizer.T("イベントを増やすための空き領域（{0} バイト）が ROM に見つかりませんでした。確定を中止しました（ROM は変えていません）。"), length),
					Localizer.T("イベントの追加"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			this.tempHeader.EventScriptAddress = generator.HeaderAddress;
			this.txtAddressEventScript.Text = string.Format("{0:X8}", generator.HeaderAddress);
			if (current != 0U && current != generator.HeaderAddress)
			{
				this.pendingEventDataClearAddress = current;
				this.pendingEventDataClearReplacementAddress = generator.HeaderAddress;
			}
			return true;
		}
	}
}
