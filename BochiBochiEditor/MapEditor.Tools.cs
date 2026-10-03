using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップ編集ツール（ペン／塗りつぶし／矩形）、マップ検索、ステータスバーの処理群
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		//-------------------------------------------------------------------------------
		// マップ上で使う描画ツールの種類
		//-------------------------------------------------------------------------------
		private enum MapPaintTool
		{
			Pen,
			Fill,
			Rectangle,
			// 自動ペン: 選んだブロックが入っているパーツ（マップ作成補助）として塗る（MapEditor.PartBrush.cs）
			Parts
		}

		private MapPaintTool currentPaintTool = MapPaintTool.Pen;

		// 矩形ツールでドラッグ中かどうかと、その始点・終点（マップのセル座標）
		private bool isDraggingToolRect;
		private Point toolRectStart;
		private Point toolRectEnd;

		// マップ検索の入力を少し待ってから絞り込むためのタイマー
		private System.Windows.Forms.Timer mapSearchTimer;

		//-------------------------------------------------------------------------------
		// 編集ツール・検索・ステータスバーのイベントと初期状態を設定する処理
		//-------------------------------------------------------------------------------
		private void InitializeMapTools()
		{
			this.btnToolPen.Tag = MapPaintTool.Pen;
			this.btnToolFill.Tag = MapPaintTool.Fill;
			this.btnToolRect.Tag = MapPaintTool.Rectangle;
			this.btnToolParts.Tag = MapPaintTool.Parts;
			this.mapToolTip.SetToolTip(this.btnToolParts, Localizer.T("自動ペン：選んだブロックでなぞると、岸・段差・木の端が自動で付き、建物はまるごと置ける (P)\n自動の対象でないブロックは、ペンと同じように塗る"));
			this.InitializeMapPartBrush();
			this.InitializeOverlayOpacity();
			this.mapToolTip.SetToolTip(this.btnToolPen, Localizer.T("ペン：ドラッグで塗る (B)\n右クリックでスポイト"));
			this.mapToolTip.SetToolTip(this.btnToolFill, Localizer.T("塗りつぶし：同じブロックがつながった範囲を塗る (G)"));
			this.mapToolTip.SetToolTip(this.btnToolRect, Localizer.T("矩形：ドラッグした四角形を塗る (R)"));

			this.pnlMapCanvas.MouseMove += this.pnlMapCanvas_ToolMouseMove;
			this.pnlMapCanvas.MouseUp += this.pnlMapCanvas_ToolMouseUp;
			this.pnlMapCanvas.MouseLeave += this.pnlMapCanvas_ToolMouseLeave;
			this.pnlMapCanvas.MouseWheel += this.pnlMapCanvas_StatusMouseWheel;
			this.pnlMapCanvas.Paint += this.pnlMapCanvas_ToolPaint;
			this.tabEditorMode.SelectedIndexChanged += this.tabEditorMode_ToolStateChanged;
			this.tvwMapSelector.AfterSelect += this.tvwMapSelector_StatusAfterSelect;

			this.mapSearchTimer = new System.Windows.Forms.Timer { Interval = 250 };
			this.mapSearchTimer.Tick += this.MapSearchTimer_Tick;
			this.txtMapSearch.TextChanged += this.txtMapSearch_TextChanged;
			this.txtMapSearch.KeyDown += this.txtMapSearch_KeyDown;
			this.mapToolTip.SetToolTip(this.txtMapSearch, Localizer.T("マップ名や (バンク, 番号) で絞り込み。ひらがな・カタカナは区別しません。「地形:9」で地形データの番号が同じマップ（形を使い回しているマップ）を、「場所:58」でタウンマップの場所が同じマップを探せます。Esc で解除"));

			this.statusMain.Renderer = new UiTheme.FlatToolStripRenderer();
			this.UpdateMapToolButtons();
			this.UpdateStatusBar();
			this.UpdateStatusCursor(new Point(-1, -1));
			this.InitializeMapSafety();
		}

		//-------------------------------------------------------------------------------
		// ツールボタン押下で描画ツールを切り替える処理
		//-------------------------------------------------------------------------------
		private void MapToolButton_Click(object sender, EventArgs e)
		{
			if (sender is Button button && button.Tag is MapPaintTool tool)
			{
				this.SetPaintTool(tool);
			}
		}

		//-------------------------------------------------------------------------------
		// 描画ツールを切り替えてボタン表示とステータスを更新する処理
		//-------------------------------------------------------------------------------
		private void SetPaintTool(MapPaintTool tool)
		{
			this.currentPaintTool = tool;
			this.isDraggingToolRect = false;
			if (tool == MapPaintTool.Parts)
			{
				// ブロックのモードに切り替え、パーツを読み直す
				if (this.tabEditorMode.SelectedTab != this.tabBlock)
				{
					this.tabEditorMode.SelectedTab = this.tabBlock;
				}
				this.RefreshPartBrush();
			}
			this.UpdatePartBrushBarVisible();
			this.UpdateMapToolButtons();
			this.UpdateStatusBar();
			this.pnlMapCanvas.Invalidate();
			// 自動ペンの対象のブロックの色づけを、道具に合わせて出す／消す
			this.pnlTilesetPalette.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 編集モード切替時にツールボタンの有効状態を合わせる処理
		//-------------------------------------------------------------------------------
		private void tabEditorMode_ToolStateChanged(object sender, EventArgs e)
		{
			this.isDraggingToolRect = false;
			this.UpdatePartBrushBarVisible();
			this.UpdateMapToolButtons();
			this.UpdateStatusBar();
		}

		//-------------------------------------------------------------------------------
		// ツールボタンの選択表示と有効／無効を更新する処理（イベントモードでは使えない）
		//-------------------------------------------------------------------------------
		private void UpdateMapToolButtons()
		{
			bool paintMode = this.IsPaintEditMode();
			foreach (Button button in new Button[] { this.btnToolPen, this.btnToolFill, this.btnToolRect, this.btnToolParts })
			{
				// パーツはブロックのモードだけ
				bool usable = paintMode && ((MapPaintTool)button.Tag != MapPaintTool.Parts || this.tabEditorMode.SelectedTab == this.tabBlock);
				button.Enabled = usable;
				UiTheme.SetToggleButtonState(button, usable && (MapPaintTool)button.Tag == this.currentPaintTool);
			}
		}

		//-------------------------------------------------------------------------------
		// 現在の編集モードがブロックまたは移動エリア（塗る系のモード）か判定する処理
		//-------------------------------------------------------------------------------
		private bool IsPaintEditMode()
		{
			return this.tabEditorMode.SelectedTab == this.tabBlock || this.tabEditorMode.SelectedTab == this.tabCollision;
		}

		//-------------------------------------------------------------------------------
		// マップキャンバス上の座標をマップのセル座標へ変換する処理
		//-------------------------------------------------------------------------------
		private Point CanvasToMapCell(Point location)
		{
			int zoom = this.GetMapZoomScale();
			int scrollX = this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0;
			int scrollY = this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0;
			int x = (location.X + scrollX) / zoom / 16 - this.primaryMapOffsetX;
			int y = (location.Y + scrollY) / zoom / 16 - this.primaryMapOffsetY;
			return new Point(x, y);
		}

		//-------------------------------------------------------------------------------
		// マップのセル座標がマップ内にあるか判定する処理
		//-------------------------------------------------------------------------------
		private bool IsInsideMap(int x, int y)
		{
			return this.mapMatrix != null && x >= 0 && y >= 0 && x < this.mapMatrix.GetLength(0) && y < this.mapMatrix.GetLength(1);
		}

		//-------------------------------------------------------------------------------
		// 塗りつぶし／矩形ツールの左クリック開始を処理する処理（ペンのときは false を返して従来処理へ）
		//-------------------------------------------------------------------------------
		private bool TryHandleMapToolMouseDown(MouseEventArgs e, int mapX, int mapY, bool isBlockEdit)
		{
			if (e.Button != MouseButtons.Left || this.currentPaintTool == MapPaintTool.Pen || this.mapMatrix == null)
			{
				return false;
			}
			if (this.currentPaintTool == MapPaintTool.Parts)
			{
				// 移動エリアのモードと、今のブロックが自動の対象でないときは、ペンとして動かす
				if (!isBlockEdit || this.CurrentPartBrushTarget() == null)
				{
					return false;
				}
				if (this.IsInsideMap(mapX, mapY))
				{
					this.SetPartBrushPointer(e.Location);
					this.BeginPartBrushStroke(mapX, mapY);
				}
				return true;
			}
			if (!this.IsInsideMap(mapX, mapY))
			{
				return true;
			}
			if (this.currentPaintTool == MapPaintTool.Fill)
			{
				this.FloodFillMap(mapX, mapY, isBlockEdit);
				return true;
			}
			this.isDraggingToolRect = true;
			this.toolRectStart = new Point(mapX, mapY);
			this.toolRectEnd = this.toolRectStart;
			this.pnlMapCanvas.Invalidate();
			return true;
		}

		//-------------------------------------------------------------------------------
		// 矩形ツールのドラッグ中に範囲を更新し、ステータスバーの座標を更新する処理
		//-------------------------------------------------------------------------------
		private void pnlMapCanvas_ToolMouseMove(object sender, MouseEventArgs e)
		{
			Point cell = this.CanvasToMapCell(e.Location);
			if (this.currentPaintTool == MapPaintTool.Parts)
			{
				this.SetPartBrushPointer(e.Location);
				if (this.isPaintingParts && e.Button == MouseButtons.Left)
				{
					this.PaintPartsLineTo(cell);
				}
				this.MovePartBrushHover(cell);
			}
			if (this.isDraggingToolRect)
			{
				Point clamped = new Point(
					Math.Max(0, Math.Min(this.mapMatrix.GetLength(0) - 1, cell.X)),
					Math.Max(0, Math.Min(this.mapMatrix.GetLength(1) - 1, cell.Y)));
				if (clamped != this.toolRectEnd)
				{
					this.toolRectEnd = clamped;
					this.pnlMapCanvas.Invalidate();
				}
			}
			this.UpdateStatusCursor(cell);
		}

		//-------------------------------------------------------------------------------
		// 矩形ツールのドラッグを離したときに範囲を塗る処理
		//-------------------------------------------------------------------------------
		private void pnlMapCanvas_ToolMouseUp(object sender, MouseEventArgs e)
		{
			if (this.isPaintingParts && e.Button == MouseButtons.Left)
			{
				this.EndPartBrushStroke();
				return;
			}
			if (!this.isDraggingToolRect || e.Button != MouseButtons.Left)
			{
				return;
			}
			this.isDraggingToolRect = false;
			Rectangle area = this.GetToolRectArea();
			bool isBlockEdit = this.tabEditorMode.SelectedTab == this.tabBlock;
			this.FillMapRectangle(area, isBlockEdit);
			this.pnlMapCanvas.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// キャンバスからマウスが離れたらステータスバーの座標表示を消す処理
		//-------------------------------------------------------------------------------
		private void pnlMapCanvas_ToolMouseLeave(object sender, EventArgs e)
		{
			this.UpdateStatusCursor(new Point(-1, -1));
			if (this.partBrushHover.X >= 0)
			{
				this.MovePartBrushHover(new Point(-1, -1));
			}
		}

		//-------------------------------------------------------------------------------
		// 矩形ツールの始点・終点から塗る範囲（セル単位）を求める処理
		//-------------------------------------------------------------------------------
		private Rectangle GetToolRectArea()
		{
			int left = Math.Min(this.toolRectStart.X, this.toolRectEnd.X);
			int top = Math.Min(this.toolRectStart.Y, this.toolRectEnd.Y);
			int right = Math.Max(this.toolRectStart.X, this.toolRectEnd.X);
			int bottom = Math.Max(this.toolRectStart.Y, this.toolRectEnd.Y);
			return new Rectangle(left, top, right - left + 1, bottom - top + 1);
		}

		//-------------------------------------------------------------------------------
		// 矩形ツールでドラッグ中の範囲をキャンバス上に描く処理
		//-------------------------------------------------------------------------------
		private void pnlMapCanvas_ToolPaint(object sender, PaintEventArgs e)
		{
			// マップの描画（DrawMapToGraphics）が、スクロールと倍率の変換を掛けたままにする。
			// ここから先はキャンバスのピクセルで計算した位置に描くので、変換を外す（外さないと、2 倍表示やスクロール中に枠がカーソルから離れて大きく出る）
			e.Graphics.ResetTransform();
			if (this.currentPaintTool == MapPaintTool.Parts && this.tabEditorMode.SelectedTab == this.tabBlock)
			{
				this.DrawPartBrushPreview(e.Graphics);
				return;
			}
			if (!this.isDraggingToolRect || this.mapMatrix == null)
			{
				return;
			}
			Rectangle area = this.GetToolRectArea();
			int zoom = this.GetMapZoomScale();
			int cell = 16 * zoom;
			int scrollX = this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0;
			int scrollY = this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0;
			Rectangle r = new Rectangle((area.X + this.primaryMapOffsetX) * cell - scrollX, (area.Y + this.primaryMapOffsetY) * cell - scrollY, area.Width * cell, area.Height * cell);
			using (SolidBrush fill = new SolidBrush(Color.FromArgb(60, UiTheme.Accent)))
			using (Pen pen = new Pen(UiTheme.Accent, 2f) { DashStyle = DashStyle.Dash })
			{
				e.Graphics.FillRectangle(fill, r);
				e.Graphics.DrawRectangle(pen, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
			}
			string size = string.Format("{0}×{1}", area.Width, area.Height);
			TextRenderer.DrawText(e.Graphics, size, this.Font, new Point(r.X + 4, r.Y + 4), Color.White, UiTheme.Accent);
		}

		//-------------------------------------------------------------------------------
		// 選択中ブロック（範囲）をタイル状に繰り返したときの、指定セルに置くブロック番号を返す処理
		//-------------------------------------------------------------------------------
		private int GetPatternBlockId(int mapX, int mapY, Point origin)
		{
			int w = Math.Max(1, this.selectedBlockRect.Width);
			int h = Math.Max(1, this.selectedBlockRect.Height);
			int dx = ((mapX - origin.X) % w + w) % w;
			int dy = ((mapY - origin.Y) % h + h) % h;
			return (this.selectedBlockRect.Y + dy) * PaletteColumns + (this.selectedBlockRect.X + dx);
		}

		//-------------------------------------------------------------------------------
		// 1 セルへブロックまたは移動エリアを書き込み、履歴に記録する処理（変更があれば true）
		//-------------------------------------------------------------------------------
		private bool WriteMapCell(Graphics layer, int x, int y, int blockId, bool isBlockEdit)
		{
			if (isBlockEdit)
			{
				// 3 層ブロックの 2 枠目は単独で置けないため、従来のペンと同じく飛ばす
				if (blockId < 0 || blockId >= this.totalBlocks || (blockId > 0 && this.IsTripleLayerBlock(blockId - 1)))
				{
					return false;
				}
				if (this.mapMatrix[x, y].BlockIndex == blockId)
				{
					return false;
				}
				this.RecordMapEditAction(x, y, this.mapMatrix[x, y].BlockIndex, blockId, this.mapMatrix[x, y].Collision, this.mapMatrix[x, y].Collision, true);
				this.mapMatrix[x, y].BlockIndex = blockId;
				layer.DrawImage(this.blockPaletteBitmap, new Rectangle(x * 16, y * 16, 16, 16), new Rectangle(blockId % PaletteColumns * 16, blockId / PaletteColumns * 16, 16, 16), GraphicsUnit.Pixel);
				return true;
			}
			if (this.mapMatrix[x, y].Collision == this.selectedCollisionIndex)
			{
				return false;
			}
			this.RecordMapEditAction(x, y, this.mapMatrix[x, y].BlockIndex, this.mapMatrix[x, y].BlockIndex, this.mapMatrix[x, y].Collision, this.selectedCollisionIndex, false);
			this.mapMatrix[x, y].Collision = this.selectedCollisionIndex;
			return true;
		}

		//-------------------------------------------------------------------------------
		// 指定範囲を選択中のブロック（または移動エリア）で塗り、1 回分の履歴にまとめる処理
		//-------------------------------------------------------------------------------
		private void FillMapRectangle(Rectangle area, bool isBlockEdit)
		{
			if (this.mapMatrix == null || (isBlockEdit && (this.primaryMapLayerBitmap == null || this.blockPaletteBitmap == null)))
			{
				return;
			}
			this.BeginMapEditStroke();
			bool changed = false;
			using (Graphics layer = isBlockEdit ? Graphics.FromImage(this.primaryMapLayerBitmap) : null)
			{
				if (layer != null)
				{
					layer.CompositingMode = CompositingMode.SourceCopy;
				}
				for (int y = area.Top; y < area.Bottom; y++)
				{
					for (int x = area.Left; x < area.Right; x++)
					{
						if (this.IsInsideMap(x, y))
						{
							changed |= this.WriteMapCell(layer, x, y, this.GetPatternBlockId(x, y, area.Location), isBlockEdit);
						}
					}
				}
			}
			this.FinishToolEdit(changed, isBlockEdit);
		}

		//-------------------------------------------------------------------------------
		// クリックしたセルとつながった同じ値の範囲を塗りつぶす処理（上下左右のつながり）
		//-------------------------------------------------------------------------------
		private void FloodFillMap(int startX, int startY, bool isBlockEdit)
		{
			if (this.mapMatrix == null || (isBlockEdit && (this.primaryMapLayerBitmap == null || this.blockPaletteBitmap == null)))
			{
				return;
			}
			int width = this.mapMatrix.GetLength(0);
			int height = this.mapMatrix.GetLength(1);
			int target = isBlockEdit ? this.mapMatrix[startX, startY].BlockIndex : this.mapMatrix[startX, startY].Collision;
			bool[,] visited = new bool[width, height];
			Stack<Point> stack = new Stack<Point>();
			List<Point> region = new List<Point>();
			stack.Push(new Point(startX, startY));
			visited[startX, startY] = true;
			while (stack.Count > 0)
			{
				Point p = stack.Pop();
				region.Add(p);
				foreach (Point n in new Point[] { new Point(p.X + 1, p.Y), new Point(p.X - 1, p.Y), new Point(p.X, p.Y + 1), new Point(p.X, p.Y - 1) })
				{
					if (n.X < 0 || n.Y < 0 || n.X >= width || n.Y >= height || visited[n.X, n.Y])
					{
						continue;
					}
					int value = isBlockEdit ? this.mapMatrix[n.X, n.Y].BlockIndex : this.mapMatrix[n.X, n.Y].Collision;
					if (value == target)
					{
						visited[n.X, n.Y] = true;
						stack.Push(n);
					}
				}
			}

			this.BeginMapEditStroke();
			bool changed = false;
			Point origin = new Point(startX, startY);
			using (Graphics layer = isBlockEdit ? Graphics.FromImage(this.primaryMapLayerBitmap) : null)
			{
				if (layer != null)
				{
					layer.CompositingMode = CompositingMode.SourceCopy;
				}
				foreach (Point p in region)
				{
					changed |= this.WriteMapCell(layer, p.X, p.Y, this.GetPatternBlockId(p.X, p.Y, origin), isBlockEdit);
				}
			}
			this.FinishToolEdit(changed, isBlockEdit);
		}

		//-------------------------------------------------------------------------------
		// ツールでの編集後に履歴を確定し、表示と未保存状態を更新する処理
		//-------------------------------------------------------------------------------
		private void FinishToolEdit(bool changed, bool isBlockEdit)
		{
			this.EndMapEditStroke();
			if (!changed)
			{
				return;
			}
			if (isBlockEdit)
			{
				this.UpdateMapRender();
			}
			this.pnlMapCanvas.Invalidate();
			this.SetUnsavedChanges(true);
		}

		//-------------------------------------------------------------------------------
		// マップ検索の入力が変わったら少し待ってから絞り込む処理
		//-------------------------------------------------------------------------------
		private void txtMapSearch_TextChanged(object sender, EventArgs e)
		{
			this.mapSearchTimer.Stop();
			this.mapSearchTimer.Start();
		}

		//-------------------------------------------------------------------------------
		// 検索欄で Esc を押したら検索を解除し、Enter なら最初の候補を選ぶ処理
		//-------------------------------------------------------------------------------
		private void txtMapSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				this.txtMapSearch.Clear();
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Enter)
			{
				this.mapSearchTimer.Stop();
				this.ApplyMapSearchFilter();
				TreeNode first = this.FindFirstMapNode(this.tvwMapSelector.Nodes);
				if (first != null)
				{
					this.tvwMapSelector.SelectedNode = first;
					this.tvwMapSelector.Focus();
				}
				e.SuppressKeyPress = true;
			}
		}

		//-------------------------------------------------------------------------------
		// 絞り込み待ちのタイマーが切れたら検索を実行する処理
		//-------------------------------------------------------------------------------
		private void MapSearchTimer_Tick(object sender, EventArgs e)
		{
			this.mapSearchTimer.Stop();
			this.ApplyMapSearchFilter();
		}

		//-------------------------------------------------------------------------------
		// マップ一覧を作り直し、検索語に一致しないマップを外す処理
		//-------------------------------------------------------------------------------
		private void ApplyMapSearchFilter()
		{
			if (this.romData == null || this.chkTerrainIdMode.Checked)
			{
				return;
			}
			TreeNode selected = this.tvwMapSelector.SelectedNode;
			object selectedTag = (selected != null) ? selected.Tag : null;
			bool wasUpdating = this.isUpdatingUI;
			this.isUpdatingUI = true;
			try
			{
				this.RefreshMapTree();
				string keyword = NormalizeSearchText(this.txtMapSearch.Text);
				// 「地形:9」（英語は「layout:9」）なら、地形データの番号が同じマップ（形を使い回しているマップ）だけに絞り込む
				int terrainFilter = ParseTerrainSearch(this.txtMapSearch.Text);
				// 「場所:58」「場所:66,7E」（英語は「place:58」）なら、マップ名の番号（タウンマップの場所）が一致するマップだけに絞り込む
				HashSet<int> placeFilter = ParsePlaceSearch(this.txtMapSearch.Text);
				if (keyword.Length == 0)
				{
					this.RestoreMapTreeSelection(selectedTag);
					return;
				}
				this.tvwMapSelector.BeginUpdate();
				for (int i = this.tvwMapSelector.Nodes.Count - 1; i >= 0; i--)
				{
					TreeNode group = this.tvwMapSelector.Nodes[i];
					bool groupMatches = terrainFilter < 0 && placeFilter == null && NormalizeSearchText(group.Text).Contains(keyword);
					for (int j = group.Nodes.Count - 1; j >= 0; j--)
					{
						bool matches = placeFilter != null
							? (group.Nodes[j].Tag is MapEditor.MapHeader placeHeader && placeFilter.Contains(placeHeader.MapNameId))
							: terrainFilter >= 0
								? (group.Nodes[j].Tag is MapEditor.MapHeader header && header.TerrainId == terrainFilter)
								: (groupMatches || NormalizeSearchText(group.Nodes[j].Text).Contains(keyword));
						if (!matches)
						{
							group.Nodes.RemoveAt(j);
						}
					}
					if (group.Nodes.Count == 0)
					{
						this.tvwMapSelector.Nodes.RemoveAt(i);
					}
					else
					{
						group.Expand();
					}
				}
				this.AssignMapThumbnailKeys(this.tvwMapSelector.Nodes);
				this.tvwMapSelector.EndUpdate();
				this.RestoreMapTreeSelection(selectedTag);
			}
			finally
			{
				this.isUpdatingUI = wasUpdating;
			}
		}

		//-------------------------------------------------------------------------------
		// 検索語が「地形:番号」「layout:番号」の形なら、その番号を返す処理（違えば -1）
		//-------------------------------------------------------------------------------
		private static int ParseTerrainSearch(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return -1;
			}
			System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text.Normalize(NormalizationForm.FormKC).Trim(), @"^(地形|layout)\s*:\s*(\d{1,5})$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			return match.Success ? int.Parse(match.Groups[2].Value) : -1;
		}

		//-------------------------------------------------------------------------------
		// 検索語が「場所:58」「place:66,7E」の形なら、その番号（16 進）の集まりを返す処理（違えば null）
		//-------------------------------------------------------------------------------
		private static HashSet<int> ParsePlaceSearch(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text.Normalize(NormalizationForm.FormKC).Trim(), @"^(場所|place)\s*:\s*([0-9a-f]{1,2}(\s*,\s*[0-9a-f]{1,2})*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			if (!match.Success)
			{
				return null;
			}
			HashSet<int> result = new HashSet<int>();
			foreach (string part in match.Groups[2].Value.Split(','))
			{
				result.Add(Convert.ToInt32(part.Trim(), 16));
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 作り直したツリーで、元の選択マップを選択表示だけ戻す処理（読み込みはやり直さない）
		//-------------------------------------------------------------------------------
		private void RestoreMapTreeSelection(object selectedTag)
		{
			if (selectedTag == null)
			{
				return;
			}
			foreach (TreeNode group in this.tvwMapSelector.Nodes)
			{
				foreach (TreeNode node in group.Nodes)
				{
					if (ReferenceEquals(node.Tag, selectedTag))
					{
						this.tvwMapSelector.BeforeSelect -= this.tvwMapSelect_BeforeSelect;
						this.tvwMapSelector.AfterSelect -= this.tvwMapSelector_AfterSelect;
						this.tvwMapSelector.AfterSelect -= this.tvwMapSelector_StatusAfterSelect;
						try
						{
							this.tvwMapSelector.SelectedNode = node;
							this.UpdateMapSelectorSelectionStyle(node);
							node.EnsureVisible();
						}
						finally
						{
							this.tvwMapSelector.BeforeSelect += this.tvwMapSelect_BeforeSelect;
							this.tvwMapSelector.AfterSelect += this.tvwMapSelector_AfterSelect;
							this.tvwMapSelector.AfterSelect += this.tvwMapSelector_StatusAfterSelect;
						}
						return;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ツリーの中で最初に見つかるマップ（子ノード）を返す処理
		//-------------------------------------------------------------------------------
		private TreeNode FindFirstMapNode(TreeNodeCollection nodes)
		{
			foreach (TreeNode group in nodes)
			{
				if (group.Nodes.Count > 0)
				{
					return group.Nodes[0];
				}
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 検索用に文字列を正規化する処理（全角半角・大小文字・カタカナ／ひらがな・区切り記号を揃える）
		//-------------------------------------------------------------------------------
		private static string NormalizeSearchText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			string normalized = text.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
			StringBuilder sb = new StringBuilder(normalized.Length);
			foreach (char c in normalized)
			{
				if (c >= 'ァ' && c <= 'ヶ')
				{
					sb.Append((char)(c - 0x60));
				}
				else if (c == ' ' || c == '(' || c == ')')
				{
					continue;
				}
				else if (c == '.' || c == '-' || c == '_')
				{
					sb.Append(',');
				}
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString().Trim();
		}

		//-------------------------------------------------------------------------------
		// マップを選んだらステータスバーのマップ名を更新する処理
		//-------------------------------------------------------------------------------
		private void tvwMapSelector_StatusAfterSelect(object sender, TreeViewEventArgs e)
		{
			this.UpdateStatusBar();
			this.RunMapSafetyChecks();
		}

		//-------------------------------------------------------------------------------
		// ホイールでマップの倍率が変わったらステータスバーを更新する処理
		//-------------------------------------------------------------------------------
		private void pnlMapCanvas_StatusMouseWheel(object sender, MouseEventArgs e)
		{
			this.UpdateStatusBar();
		}

		//-------------------------------------------------------------------------------
		// ステータスバー全体（ROM・マップ・倍率・ツール・未保存）を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateStatusBar()
		{
			if (this.statusMain == null)
			{
				return;
			}
			this.lblStatusRom.Text = (this.romData == null) ? Localizer.T("ROM未選択") : System.IO.Path.GetFileName(this.loadedRomPath ?? string.Empty);
			string mapText = (this.tempHeader != null && this.romData != null && !this.chkTerrainIdMode.Checked) ? string.Format("({0}, {1}) {2}", this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this)) : string.Empty;
			this.lblStatusMap.Text = string.IsNullOrEmpty(mapText) ? Localizer.T("マップ未選択") : mapText;
			this.lblStatusZoom.Text = string.Format(Localizer.T("表示 {0}00%"), this.GetMapZoomScale());
			string mode = (this.tabEditorMode.SelectedTab != null) ? this.tabEditorMode.SelectedTab.Text : string.Empty;
			string tool = !this.IsPaintEditMode() ? string.Empty : (this.currentPaintTool == MapPaintTool.Pen ? Localizer.T(" / ペン") : (this.currentPaintTool == MapPaintTool.Fill ? Localizer.T(" / 塗りつぶし") : (this.currentPaintTool == MapPaintTool.Rectangle ? Localizer.T(" / 矩形") : Localizer.T(" / 自動ペン"))));
			this.lblStatusTool.Text = mode + tool;
			this.lblStatusDirty.Text = this.hasUnsavedChanges ? Localizer.T("● 未確定の変更あり") : Localizer.T("確定済み");
			this.lblStatusDirty.ForeColor = this.hasUnsavedChanges ? UiTheme.Warning : UiTheme.TextMuted;
			// 「マップの設定」タブの、地形データの表との一致の表示と、右ペインのタウンマップも合わせて更新する
			this.UpdateTerrainSyncStatus();
			this.UpdateTownMap();
		}

		//-------------------------------------------------------------------------------
		// ステータスバーのカーソル位置（セル座標とブロック番号）を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateStatusCursor(Point cell)
		{
			if (this.statusMain == null)
			{
				return;
			}
			if (!this.IsInsideMap(cell.X, cell.Y))
			{
				this.lblStatusCursor.Text = "X: -  Y: -";
				return;
			}
			int block = this.mapMatrix[cell.X, cell.Y].BlockIndex;
			int collision = this.mapMatrix[cell.X, cell.Y].Collision;
			this.lblStatusCursor.Text = string.Format(Localizer.T("X: {0}  Y: {1}   ブロック 0x{2:X3}   移動エリア 0x{3:X2}"), cell.X, cell.Y, block, collision);
		}
	}
}
