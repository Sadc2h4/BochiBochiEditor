using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 取り込みウィザードの「配置」ページ
	// 左の素材ブロックを選び、右の取り込み先タイルセットの好きな枠へドラッグして置く
	//-------------------------------------------------------------------------------
	public partial class TileImportWizard
	{
		// 配置ページでの 1 マスの表示サイズ（16px の 2 倍）
		private const int PlaceCell = 32;
		private const int PlaceColumns = 8;
		private const string PlaceDragFormat = "BochiBochiEditor.PlaceBlocks";

		private bool placeEventsWired;
		private readonly Dictionary<int, Bitmap> newBlockImages = new Dictionary<int, Bitmap>();
		private Bitmap targetSheet;

		// 選択中の素材ブロック（plan.Blocks の添字）と、並びの基準にするマス（実際に選んだマス）
		private readonly HashSet<int> placeSelection = new HashSet<int>();
		private readonly Dictionary<int, Point> placeSelectionCells = new Dictionary<int, Point>();
		// 左で選択済みのマスを押したときは、範囲選択ではなくドラッグの準備にする
		private bool placeSourceDragArmed;
		private Point placeSourceDragStart;
		private bool placeSelecting;
		private Point placeAnchorCell;
		private Rectangle placeSelectRect = Rectangle.Empty;

		// ドラッグ中のブロックと、並びを保つためのずれ（マス単位）
		private List<KeyValuePair<int, Point>> placeDragging;
		// ドラッグ中の並びの中で、マウスで掴んだブロックの位置（この位置がカーソルの枠に来る）
		private Point placeGrabOffset;
		// 左で押したマスのブロック（掴んだブロック）
		private int placeGrabbedIndex = -1;
		private int placeHoverId = -1;
		private bool placeTargetDragArmed;
		private Point placeTargetDragStart;
		private string lastPlaceTip;

		//-------------------------------------------------------------------------------
		// 計画を作り直したときに、配置ページの絵の記録を破棄する処理
		//-------------------------------------------------------------------------------
		private void InvalidatePlaceCaches()
		{
			foreach (Bitmap bmp in this.newBlockImages.Values)
			{
				bmp.Dispose();
			}
			this.newBlockImages.Clear();
			this.targetSheet?.Dispose();
			this.targetSheet = null;
			this.placeSelection.Clear();
			this.placeSelectionCells.Clear();
			this.pnlPlaceSource?.Invalidate();
			this.pnlPlaceTarget?.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 素材ブロックを選択に加える処理（cell は並びの基準にする、実際に選んだマス）
		//-------------------------------------------------------------------------------
		private void AddToPlaceSelection(int index, Point cell)
		{
			if (this.placeSelection.Add(index))
			{
				this.placeSelectionCells[index] = cell;
			}
		}

		//-------------------------------------------------------------------------------
		// 素材ブロックを選択から外す処理
		//-------------------------------------------------------------------------------
		private void RemoveFromPlaceSelection(int index)
		{
			this.placeSelection.Remove(index);
			this.placeSelectionCells.Remove(index);
		}

		//-------------------------------------------------------------------------------
		// 選択をすべて解除する処理
		//-------------------------------------------------------------------------------
		private void ClearPlaceSelection()
		{
			this.placeSelection.Clear();
			this.placeSelectionCells.Clear();
		}

		//-------------------------------------------------------------------------------
		// 配置ページを開いたときの準備（イベント登録・スクロール範囲・既存ブロックの絵）
		//-------------------------------------------------------------------------------
		private void PreparePlacePage()
		{
			if (!this.placeEventsWired)
			{
				this.placeEventsWired = true;
				foreach (Control canvas in new Control[] { this.pnlPlaceSource, this.pnlPlaceTarget })
				{
					typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(canvas, true, null);
					UiTheme.MarkCanvas(canvas);
				}
				this.pnlPlaceSource.Paint += this.pnlPlaceSource_Paint;
				this.pnlPlaceSource.MouseDown += this.pnlPlaceSource_MouseDown;
				this.pnlPlaceSource.MouseMove += this.pnlPlaceSource_MouseMove;
				this.pnlPlaceSource.MouseUp += this.pnlPlaceSource_MouseUp;
				this.pnlPlaceSource.Scroll += (s, e) => this.pnlPlaceSource.Invalidate();
				this.pnlPlaceTarget.Paint += this.pnlPlaceTarget_Paint;
				this.pnlPlaceTarget.MouseDown += this.pnlPlaceTarget_MouseDown;
				this.pnlPlaceTarget.MouseMove += this.pnlPlaceTarget_MouseMove;
				this.pnlPlaceTarget.MouseUp += (s, e) => this.placeTargetDragArmed = false;
				this.pnlPlaceTarget.DragEnter += this.pnlPlaceTarget_DragOver;
				this.pnlPlaceTarget.DragOver += this.pnlPlaceTarget_DragOver;
				this.pnlPlaceTarget.DragLeave += this.pnlPlaceTarget_DragLeave;
				this.pnlPlaceTarget.DragDrop += this.pnlPlaceTarget_DragDrop;
				this.pnlPlaceTarget.Scroll += (s, e) => this.pnlPlaceTarget.Invalidate();
				this.toolTip.SetToolTip(this.btnAutoPlace, Localizer.T("まだ置き場所の決まっていないブロックを、空いている枠へ順に置きます"));
				this.toolTip.SetToolTip(this.btnClearPlace, Localizer.T("すべてのブロックの置き場所を外します"));
				this.toolTip.SetToolTip(this.chkAllowReplace, Localizer.T("オンにすると、マップで使われているブロックの枠にも置けます（そのブロックを置いているマップの見た目が変わります）"));
			}
			if (this.plan == null || this.plan.Target == null)
			{
				return;
			}
			this.pnlPlaceSource.AutoScrollMinSize = new Size(this.plan.CellColumns * PlaceCell + 1, this.plan.CellRows * PlaceCell + 1);
			int rows = (this.plan.Target.BlockCount + PlaceColumns - 1) / PlaceColumns;
			this.pnlPlaceTarget.AutoScrollMinSize = new Size(PlaceColumns * PlaceCell + 1, rows * PlaceCell + 1);
			this.BuildTargetSheet();
			this.UpdatePlaceInfo();
			this.pnlPlaceSource.Invalidate();
			this.pnlPlaceTarget.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 取り込み先タイルセットの既存ブロックを 1 枚の絵（1 行 8 ブロック）にまとめる処理
		//-------------------------------------------------------------------------------
		private void BuildTargetSheet()
		{
			if (this.targetSheet != null || this.plan == null || this.plan.Target == null)
			{
				return;
			}
			TileImportEngine.TargetState target = this.plan.Target;
			int rows = Math.Max(1, (target.BlockCount + PlaceColumns - 1) / PlaceColumns);
			this.targetSheet = new Bitmap(PlaceColumns * 16, rows * 16);
			using (Graphics g = Graphics.FromImage(this.targetSheet))
			{
				for (int i = 0; i < target.BlockCount; i++)
				{
					using (Bitmap block = this.host.GetBlockImage(target.BlockStart + i))
					{
						if (block != null)
						{
							g.DrawImage(block, (i % PlaceColumns) * 16, (i / PlaceColumns) * 16, 16, 16);
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 素材ブロック（計画を反映した完成形）の絵を返す処理（作った絵は記録して使い回す）
		//-------------------------------------------------------------------------------
		private Bitmap GetNewBlockImage(int index)
		{
			Bitmap image;
			if (!this.newBlockImages.TryGetValue(index, out image))
			{
				image = TileImportEngine.RenderBlock(this.plan.Blocks[index].Entries, this.plan, this.planOptions);
				this.newBlockImages[index] = image;
			}
			return image;
		}

		//-------------------------------------------------------------------------------
		// 配置の状況（置いた数・未配置・差し替え）と操作の案内を表示する処理
		//-------------------------------------------------------------------------------
		private void UpdatePlaceInfo()
		{
			if (this.plan == null)
			{
				return;
			}
			int total = this.plan.Blocks.Count;
			int unassigned = this.plan.UnassignedCount;
			string status = string.Format(Localizer.T("配置済み {0} / {1} ブロック　未配置 {2}　差し替え {3}"), total - unassigned, total, unassigned, this.plan.ReplacingCount);
			string hint = unassigned > 0
				? Localizer.T("左で選んで（ドラッグで範囲選択）、右の枠へドラッグして置きます。右クリックで置き場所を外せます。空きが足りない場合は、差し替えを許可して使用中の枠へ置いてください。")
				: Localizer.T("すべて置けました。右クリックで置き場所を外したり、右の枠どうしでドラッグして動かしたりできます。");
			this.lblPlaceInfo.Text = status + "\r\n" + hint;
			this.lblPlaceInfo.ForeColor = unassigned > 0 ? UiTheme.Warning : UiTheme.Text;
			this.btnNext.Enabled = this.CanLeavePage(this.pageIndex);
		}

		//-------------------------------------------------------------------------------
		// 「空きに自動配置」ボタンの処理
		//-------------------------------------------------------------------------------
		private void btnAutoPlace_Click(object sender, EventArgs e)
		{
			if (this.plan == null)
			{
				return;
			}
			this.plan.AutoAssign();
			this.RefreshPlaceViews();
		}

		//-------------------------------------------------------------------------------
		// 「配置をやり直す」ボタンの処理（すべての置き場所を外す）
		//-------------------------------------------------------------------------------
		private void btnClearPlace_Click(object sender, EventArgs e)
		{
			if (this.plan == null)
			{
				return;
			}
			this.plan.ClearAssignments();
			this.RefreshPlaceViews();
		}

		//-------------------------------------------------------------------------------
		// 配置が変わったあとに表示を更新する処理
		//-------------------------------------------------------------------------------
		private void RefreshPlaceViews()
		{
			this.UpdatePlaceInfo();
			this.pnlPlaceSource.Invalidate();
			this.pnlPlaceTarget.Invalidate();
		}

		// ---------------------------------------------------------------- 左: 素材 ----

		//-------------------------------------------------------------------------------
		// 素材パネル上の座標から画像のマスを求める処理（範囲外なら false）
		//-------------------------------------------------------------------------------
		private bool TryGetSourceCell(Point location, out Point cell)
		{
			cell = Point.Empty;
			if (this.plan == null)
			{
				return false;
			}
			int x = (location.X - this.pnlPlaceSource.AutoScrollPosition.X) / PlaceCell;
			int y = (location.Y - this.pnlPlaceSource.AutoScrollPosition.Y) / PlaceCell;
			if (x < 0 || y < 0 || x >= this.plan.CellColumns || y >= this.plan.CellRows)
			{
				return false;
			}
			cell = new Point(x, y);
			return true;
		}

		//-------------------------------------------------------------------------------
		// 素材を描く処理（未配置は赤、選択中は強調枠）
		//-------------------------------------------------------------------------------
		private void pnlPlaceSource_Paint(object sender, PaintEventArgs e)
		{
			if (this.plan == null)
			{
				return;
			}
			Graphics g = e.Graphics;
			g.TranslateTransform(this.pnlPlaceSource.AutoScrollPosition.X, this.pnlPlaceSource.AutoScrollPosition.Y);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			for (int y = 0; y < this.plan.CellRows; y++)
			{
				for (int x = 0; x < this.plan.CellColumns; x++)
				{
					Rectangle r = new Rectangle(x * PlaceCell, y * PlaceCell, PlaceCell, PlaceCell);
					int index = this.plan.CellBlockIndex[x, y];
					if (index < 0)
					{
						using (HatchBrush empty = new HatchBrush(HatchStyle.Percent10, Color.FromArgb(60, 64, 90), UiTheme.Canvas))
						{
							g.FillRectangle(empty, r);
						}
						continue;
					}
					g.DrawImage(this.GetNewBlockImage(index), r);
					if (this.plan.Blocks[index].BlockId < 0)
					{
						using (SolidBrush unplaced = new SolidBrush(Color.FromArgb(90, UiTheme.Warning)))
						{
							g.FillRectangle(unplaced, r);
						}
					}
				}
			}
			g.PixelOffsetMode = PixelOffsetMode.Default;
			using (Pen grid = new Pen(Color.FromArgb(50, 255, 255, 255)))
			{
				for (int x = 0; x <= this.plan.CellColumns; x++)
				{
					g.DrawLine(grid, x * PlaceCell, 0, x * PlaceCell, this.plan.CellRows * PlaceCell);
				}
				for (int y = 0; y <= this.plan.CellRows; y++)
				{
					g.DrawLine(grid, 0, y * PlaceCell, this.plan.CellColumns * PlaceCell, y * PlaceCell);
				}
			}
			using (Pen selected = new Pen(Color.FromArgb(255, 214, 64), 2f))
			{
				foreach (int index in this.placeSelection)
				{
					foreach (Point cell in this.plan.Blocks[index].Cells)
					{
						g.DrawRectangle(selected, cell.X * PlaceCell + 1, cell.Y * PlaceCell + 1, PlaceCell - 2, PlaceCell - 2);
					}
				}
			}
			if (this.placeSelecting && !this.placeSelectRect.IsEmpty)
			{
				using (Pen dash = new Pen(UiTheme.Accent, 1f) { DashStyle = DashStyle.Dash })
				{
					Rectangle r = this.placeSelectRect;
					g.DrawRectangle(dash, r.X * PlaceCell, r.Y * PlaceCell, r.Width * PlaceCell - 1, r.Height * PlaceCell - 1);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 素材の選択を始める処理（Ctrl を押しながらで追加・解除）
		//-------------------------------------------------------------------------------
		private void pnlPlaceSource_MouseDown(object sender, MouseEventArgs e)
		{
			Point cell;
			if (e.Button != MouseButtons.Left || !this.TryGetSourceCell(e.Location, out cell))
			{
				return;
			}
			int index = this.plan.CellBlockIndex[cell.X, cell.Y];
			bool additive = (Control.ModifierKeys & Keys.Control) == Keys.Control;
			// 選択済みのマスを押した場合は、選択を保ったままドラッグの準備をする（範囲選択にしない）
			if (!additive && index >= 0 && this.placeSelection.Contains(index))
			{
				this.placeSourceDragArmed = true;
				this.placeSourceDragStart = e.Location;
				this.placeGrabbedIndex = index;
				return;
			}
			if (additive && index >= 0)
			{
				if (this.placeSelection.Contains(index))
				{
					this.RemoveFromPlaceSelection(index);
				}
				else
				{
					this.AddToPlaceSelection(index, cell);
				}
			}
			else
			{
				this.ClearPlaceSelection();
				if (index >= 0)
				{
					this.AddToPlaceSelection(index, cell);
				}
			}
			this.placeSelecting = true;
			this.placeGrabbedIndex = index;
			this.placeAnchorCell = cell;
			this.placeSelectRect = new Rectangle(cell, new Size(1, 1));
			this.pnlPlaceSource.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 素材の上ではドラッグで範囲選択し、パネルの外へ出たら右へのドラッグを始める処理
		//-------------------------------------------------------------------------------
		private void pnlPlaceSource_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.placeSourceDragArmed && (e.Button & MouseButtons.Left) != 0)
			{
				Size drag = SystemInformation.DragSize;
				if (Math.Abs(e.X - this.placeSourceDragStart.X) > drag.Width || Math.Abs(e.Y - this.placeSourceDragStart.Y) > drag.Height)
				{
					this.placeSourceDragArmed = false;
					this.StartPlaceDrag(this.placeSelection.ToList(), this.pnlPlaceSource, false, this.placeGrabbedIndex);
				}
				return;
			}
			if (!this.placeSelecting || (e.Button & MouseButtons.Left) == 0)
			{
				this.UpdateSourceTip(e.Location);
				return;
			}
			if (!this.pnlPlaceSource.ClientRectangle.Contains(e.Location))
			{
				this.placeSelecting = false;
				this.placeSelectRect = Rectangle.Empty;
				this.StartPlaceDrag(this.placeSelection.ToList(), this.pnlPlaceSource, false, this.placeGrabbedIndex);
				return;
			}
			Point cell;
			if (!this.TryGetSourceCell(e.Location, out cell))
			{
				return;
			}
			Rectangle rect = Rectangle.FromLTRB(Math.Min(cell.X, this.placeAnchorCell.X), Math.Min(cell.Y, this.placeAnchorCell.Y), Math.Max(cell.X, this.placeAnchorCell.X) + 1, Math.Max(cell.Y, this.placeAnchorCell.Y) + 1);
			if (rect == this.placeSelectRect)
			{
				return;
			}
			this.placeSelectRect = rect;
			if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
			{
				this.ClearPlaceSelection();
			}
			// 左上から順に見て、各ブロックの基準は範囲内で最初に出てきたマスにする
			for (int y = rect.Top; y < rect.Bottom; y++)
			{
				for (int x = rect.Left; x < rect.Right; x++)
				{
					int index = this.plan.CellBlockIndex[x, y];
					if (index >= 0)
					{
						this.AddToPlaceSelection(index, new Point(x, y));
					}
				}
			}
			this.pnlPlaceSource.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 素材の範囲選択を終える処理
		//-------------------------------------------------------------------------------
		private void pnlPlaceSource_MouseUp(object sender, MouseEventArgs e)
		{
			// 選択済みのマスを押して動かさずに離した場合は、そのブロックだけを選び直す
			if (this.placeSourceDragArmed && (Control.ModifierKeys & Keys.Control) != Keys.Control)
			{
				Point cell;
				if (this.TryGetSourceCell(e.Location, out cell) && this.plan.CellBlockIndex[cell.X, cell.Y] >= 0)
				{
					this.ClearPlaceSelection();
					this.AddToPlaceSelection(this.plan.CellBlockIndex[cell.X, cell.Y], cell);
					this.pnlPlaceTarget.Invalidate();
				}
			}
			this.placeSourceDragArmed = false;
			this.placeSelecting = false;
			this.placeSelectRect = Rectangle.Empty;
			this.pnlPlaceSource.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 素材にマウスを乗せたとき、そのブロックの置き場所をツールチップに出す処理
		//-------------------------------------------------------------------------------
		private void UpdateSourceTip(Point location)
		{
			Point cell;
			string tip = null;
			if (this.TryGetSourceCell(location, out cell))
			{
				int index = this.plan.CellBlockIndex[cell.X, cell.Y];
				if (index >= 0)
				{
					int id = this.plan.Blocks[index].BlockId;
					tip = id >= 0 ? string.Format(Localizer.T("置き場所: 0x{0:X3}"), id) : Localizer.T("まだ置き場所が決まっていません");
				}
			}
			this.SetPlaceTip(this.pnlPlaceSource, tip);
		}

		//-------------------------------------------------------------------------------
		// 選んだブロックのドラッグを始める処理（並びは、実際に選んだマス、または右での位置関係を保つ）
		//-------------------------------------------------------------------------------
		private void StartPlaceDrag(List<int> blocks, Control source, bool fromTarget, int grabbed)
		{
			if (fromTarget)
			{
				// 右から動かす場合は、置いてあるものだけを、右での並びのまま動かす
				blocks = blocks.Where(i => this.plan.Blocks[i].BlockId >= 0).ToList();
			}
			if (blocks.Count == 0)
			{
				return;
			}
			Func<int, Point> position = i =>
			{
				if (fromTarget)
				{
					int offset = this.plan.Blocks[i].BlockId - this.plan.Target.BlockStart;
					return new Point(offset % PlaceColumns, offset / PlaceColumns);
				}
				Point cell;
				return this.placeSelectionCells.TryGetValue(i, out cell) ? cell : this.plan.Blocks[i].Cells[0];
			};
			int minX = blocks.Min(i => position(i).X);
			int minY = blocks.Min(i => position(i).Y);
			// 画像（または右）での並び順（上から、左から）にそろえる
			this.placeDragging = blocks
				.Select(i => new KeyValuePair<int, Point>(i, new Point(position(i).X - minX, position(i).Y - minY)))
				.OrderBy(p => p.Value.Y).ThenBy(p => p.Value.X)
				.ToList();
			// 横に 8 ブロックを超える並びは、右の 8 列に収まらないので、並び順のまま 8 個ずつ折り返す
			if (this.placeDragging.Any(p => p.Value.X >= PlaceColumns))
			{
				this.placeDragging = this.placeDragging.Select((p, n) => new KeyValuePair<int, Point>(p.Key, new Point(n % PlaceColumns, n / PlaceColumns))).ToList();
			}
			// 掴んだブロックがカーソルの枠に来るよう、その位置を覚えておく（無ければ左上）
			KeyValuePair<int, Point> grab = this.placeDragging.FirstOrDefault(p => p.Key == grabbed);
			this.placeGrabOffset = this.placeDragging.Any(p => p.Key == grabbed) ? grab.Value : Point.Empty;
			source.DoDragDrop(new DataObject(PlaceDragFormat, true), DragDropEffects.Move);
			this.placeDragging = null;
			this.placeHoverId = -1;
			this.pnlPlaceTarget.Invalidate();
		}

		// ---------------------------------------------------------------- 右: 取り込み先 ----

		//-------------------------------------------------------------------------------
		// 取り込み先パネル上の座標からブロック番号を求める処理（範囲外なら -1）
		//-------------------------------------------------------------------------------
		private int GetTargetSlot(Point location)
		{
			if (this.plan == null || this.plan.Target == null)
			{
				return -1;
			}
			int x = (location.X - this.pnlPlaceTarget.AutoScrollPosition.X) / PlaceCell;
			int y = (location.Y - this.pnlPlaceTarget.AutoScrollPosition.Y) / PlaceCell;
			if (x < 0 || y < 0 || x >= PlaceColumns)
			{
				return -1;
			}
			int offset = y * PlaceColumns + x;
			return offset < this.plan.Target.BlockCount ? this.plan.Target.BlockStart + offset : -1;
		}

		//-------------------------------------------------------------------------------
		// 指定番号に置かれている素材ブロック（plan.Blocks の添字）を返す処理（無ければ -1）
		//-------------------------------------------------------------------------------
		private int FindPlannedAt(int blockId)
		{
			for (int i = 0; i < this.plan.Blocks.Count; i++)
			{
				if (this.plan.Blocks[i].BlockId == blockId)
				{
					return i;
				}
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// ドラッグ中のブロックを hover の位置へ置いた場合の、各ブロックの置き先を求める処理
		// 置けない場合は理由を返す（null なら置ける）
		//-------------------------------------------------------------------------------
		private string ComputeDropTargets(int hover, out List<KeyValuePair<int, int>> targets)
		{
			targets = new List<KeyValuePair<int, int>>();
			if (hover < 0 || this.placeDragging == null)
			{
				return Localizer.T("ここには置けません");
			}
			TileImportEngine.TargetState target = this.plan.Target;
			int hoverColumn = (hover - target.BlockStart) % PlaceColumns;
			int hoverRow = (hover - target.BlockStart) / PlaceColumns;
			// 掴んだブロックがカーソルの枠に来るように並びの左上を決め、端からはみ出す分は並びを保って寄せる
			int width = this.placeDragging.Max(p => p.Value.X) + 1;
			int originColumn = Math.Max(0, Math.Min(PlaceColumns - width, hoverColumn - this.placeGrabOffset.X));
			int originRow = Math.Max(0, hoverRow - this.placeGrabOffset.Y);
			foreach (KeyValuePair<int, Point> item in this.placeDragging)
			{
				if (originColumn + item.Value.X >= PlaceColumns)
				{
					return Localizer.T("並びが右端からはみ出します");
				}
				int id = target.BlockStart + (originRow + item.Value.Y) * PlaceColumns + originColumn + item.Value.X;
				if (id >= target.BlockStart + target.BlockCount)
				{
					return Localizer.T("並びがタイルセットの末尾からはみ出します");
				}
				if (id == 0)
				{
					return Localizer.T("ブロック 0 は予約されているため置けません");
				}
				if (target.ProtectedBlockIds.Contains(id))
				{
					return Localizer.T("前のブロックの 3 層目として使われている枠のため置けません");
				}
				if (target.BlockMapUsage.ContainsKey(id) && !this.chkAllowReplace.Checked)
				{
					return Localizer.T("マップで使われているブロックの枠です（差し替えるには「使用中のブロックへの差し替えを許可する」をオンにしてください）");
				}
				targets.Add(new KeyValuePair<int, int>(item.Key, id));
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// ドラッグ中に、置き先の候補を表示する処理
		//-------------------------------------------------------------------------------
		private void pnlPlaceTarget_DragOver(object sender, DragEventArgs e)
		{
			if (this.placeDragging == null || !e.Data.GetDataPresent(PlaceDragFormat))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			int hover = this.GetTargetSlot(this.pnlPlaceTarget.PointToClient(new Point(e.X, e.Y)));
			List<KeyValuePair<int, int>> targets;
			string reason = this.ComputeDropTargets(hover, out targets);
			e.Effect = reason == null ? DragDropEffects.Move : DragDropEffects.None;
			if (hover != this.placeHoverId)
			{
				this.placeHoverId = hover;
				this.lblPlaceInfo.Text = reason ?? string.Format(Localizer.T("0x{0:X3} から {1} 個を置きます"), targets.Min(t => t.Value), targets.Count);
				this.pnlPlaceTarget.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// ドラッグが取り込み先の外へ出たら候補表示を消す処理
		//-------------------------------------------------------------------------------
		private void pnlPlaceTarget_DragLeave(object sender, EventArgs e)
		{
			this.placeHoverId = -1;
			this.UpdatePlaceInfo();
			this.pnlPlaceTarget.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// ドロップされた位置へブロックを置く処理（その枠に置いてあった別の素材ブロックは外す）
		//-------------------------------------------------------------------------------
		private void pnlPlaceTarget_DragDrop(object sender, DragEventArgs e)
		{
			int hover = this.GetTargetSlot(this.pnlPlaceTarget.PointToClient(new Point(e.X, e.Y)));
			List<KeyValuePair<int, int>> targets;
			if (this.ComputeDropTargets(hover, out targets) != null)
			{
				return;
			}
			HashSet<int> moving = new HashSet<int>(targets.Select(t => t.Key));
			foreach (KeyValuePair<int, int> t in targets)
			{
				int occupant = this.FindPlannedAt(t.Value);
				if (occupant >= 0 && !moving.Contains(occupant))
				{
					this.plan.Blocks[occupant].BlockId = -1;
				}
			}
			foreach (KeyValuePair<int, int> t in targets)
			{
				this.plan.Blocks[t.Key].BlockId = t.Value;
			}
			this.placeHoverId = -1;
			this.RefreshPlaceViews();
		}

		//-------------------------------------------------------------------------------
		// 取り込み先を描く処理（空き = 緑の点線、置いた素材 = 紫、差し替え = オレンジ）
		//-------------------------------------------------------------------------------
		private void pnlPlaceTarget_Paint(object sender, PaintEventArgs e)
		{
			if (this.plan == null || this.plan.Target == null)
			{
				return;
			}
			TileImportEngine.TargetState target = this.plan.Target;
			Graphics g = e.Graphics;
			g.TranslateTransform(this.pnlPlaceTarget.AutoScrollPosition.X, this.pnlPlaceTarget.AutoScrollPosition.Y);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			if (this.targetSheet != null)
			{
				g.DrawImage(this.targetSheet, new Rectangle(0, 0, this.targetSheet.Width * 2, this.targetSheet.Height * 2));
			}
			Dictionary<int, int> placedAt = new Dictionary<int, int>();
			for (int i = 0; i < this.plan.Blocks.Count; i++)
			{
				if (this.plan.Blocks[i].BlockId >= 0)
				{
					placedAt[this.plan.Blocks[i].BlockId] = i;
				}
			}
			foreach (KeyValuePair<int, int> pair in placedAt)
			{
				g.DrawImage(this.GetNewBlockImage(pair.Value), this.SlotRect(pair.Key));
			}
			g.PixelOffsetMode = PixelOffsetMode.Default;
			HashSet<int> free = new HashSet<int>(target.FreeBlockIds);
			using (Pen freePen = new Pen(Color.FromArgb(160, 90, 200, 120), 1f) { DashStyle = DashStyle.Dot })
			using (Pen placedPen = new Pen(UiTheme.Accent, 2f))
			using (Pen replacePen = new Pen(Color.FromArgb(255, 160, 60), 2f))
			using (Pen selectedPen = new Pen(Color.FromArgb(255, 214, 64), 2f))
			{
				for (int id = target.BlockStart; id < target.BlockStart + target.BlockCount; id++)
				{
					Rectangle r = this.SlotRect(id);
					if (placedAt.ContainsKey(id))
					{
						Pen pen = this.placeSelection.Contains(placedAt[id]) ? selectedPen : (target.BlockMapUsage.ContainsKey(id) ? replacePen : placedPen);
						g.DrawRectangle(pen, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
					}
					else if (free.Contains(id))
					{
						g.DrawRectangle(freePen, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
					}
				}
			}
			// ドラッグ中の置き先候補
			if (this.placeDragging != null && this.placeHoverId >= 0)
			{
				List<KeyValuePair<int, int>> targets;
				bool ok = this.ComputeDropTargets(this.placeHoverId, out targets) == null;
				using (SolidBrush hint = new SolidBrush(Color.FromArgb(90, ok ? UiTheme.Accent : UiTheme.Warning)))
				{
					if (ok)
					{
						foreach (KeyValuePair<int, int> t in targets)
						{
							g.FillRectangle(hint, this.SlotRect(t.Value));
						}
					}
					else
					{
						g.FillRectangle(hint, this.SlotRect(this.placeHoverId));
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ブロック番号から取り込み先パネル上の四角形（スクロール前の座標）を求める処理
		//-------------------------------------------------------------------------------
		private Rectangle SlotRect(int blockId)
		{
			int offset = blockId - this.plan.Target.BlockStart;
			return new Rectangle((offset % PlaceColumns) * PlaceCell, (offset / PlaceColumns) * PlaceCell, PlaceCell, PlaceCell);
		}

		//-------------------------------------------------------------------------------
		// 取り込み先で、置いた素材を右クリックで外す／左ドラッグで別の枠へ動かす準備をする処理
		//-------------------------------------------------------------------------------
		private void pnlPlaceTarget_MouseDown(object sender, MouseEventArgs e)
		{
			int slot = this.GetTargetSlot(e.Location);
			int placed = slot >= 0 ? this.FindPlannedAt(slot) : -1;
			if (placed < 0)
			{
				return;
			}
			if (e.Button == MouseButtons.Right)
			{
				this.plan.Blocks[placed].BlockId = -1;
				this.RefreshPlaceViews();
				return;
			}
			if (e.Button == MouseButtons.Left)
			{
				bool additive = (Control.ModifierKeys & Keys.Control) == Keys.Control;
				if (additive)
				{
					if (this.placeSelection.Contains(placed))
					{
						this.RemoveFromPlaceSelection(placed);
					}
					else
					{
						this.AddToPlaceSelection(placed, this.plan.Blocks[placed].Cells[0]);
					}
				}
				else if (!this.placeSelection.Contains(placed))
				{
					this.ClearPlaceSelection();
					this.AddToPlaceSelection(placed, this.plan.Blocks[placed].Cells[0]);
				}
				this.placeTargetDragArmed = !additive;
				this.placeTargetDragStart = e.Location;
				this.placeGrabbedIndex = placed;
				this.pnlPlaceSource.Invalidate();
				this.pnlPlaceTarget.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// 取り込み先のマウス移動: 置いた素材のドラッグ開始と、枠の説明（ツールチップ）
		//-------------------------------------------------------------------------------
		private void pnlPlaceTarget_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.placeTargetDragArmed && (e.Button & MouseButtons.Left) != 0)
			{
				Size drag = SystemInformation.DragSize;
				if (Math.Abs(e.X - this.placeTargetDragStart.X) > drag.Width || Math.Abs(e.Y - this.placeTargetDragStart.Y) > drag.Height)
				{
					this.placeTargetDragArmed = false;
					this.StartPlaceDrag(this.placeSelection.ToList(), this.pnlPlaceTarget, true, this.placeGrabbedIndex);
				}
				return;
			}
			int slot = this.GetTargetSlot(e.Location);
			string tip = null;
			if (slot >= 0)
			{
				int usage;
				int placed = this.FindPlannedAt(slot);
				string state = this.plan.Target.ProtectedBlockIds.Contains(slot)
					? Localizer.T("使用中（前のブロックの 3 層目）")
					: this.plan.Target.BlockMapUsage.TryGetValue(slot, out usage)
					? string.Format(Localizer.T("マップ {0} 件で使用中"), usage)
					: (this.plan.Target.FreeBlockIds.Contains(slot) ? Localizer.T("空き") : Localizer.T("未使用（中身あり）"));
				tip = string.Format("0x{0:X3}  {1}", slot, state);
				if (placed >= 0)
				{
					Point cell = this.plan.Blocks[placed].Cells[0];
					tip += "\n" + string.Format(Localizer.T("取り込むブロック: 素材の ({0}, {1})　右クリックで外す"), cell.X, cell.Y);
				}
			}
			this.SetPlaceTip(this.pnlPlaceTarget, tip);
		}

		//-------------------------------------------------------------------------------
		// ツールチップの文字が変わったときだけ設定し直す処理（ちらつき防止）
		//-------------------------------------------------------------------------------
		private void SetPlaceTip(Control control, string tip)
		{
			if (tip == this.lastPlaceTip)
			{
				return;
			}
			this.lastPlaceTip = tip;
			this.toolTip.SetToolTip(control, tip);
		}
	}
}
