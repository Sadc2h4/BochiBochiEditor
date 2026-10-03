using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「マップ作成補助」の画面（1 段階目: パーツの指定。2 段階目「サポート作成」は MapAssistForm.Support.cs）
	// 今のマップのタイルセットについて、「どのブロックが何のパーツか」を手で指定して保存する。
	// 左でブロック（ブロック一覧、またはマップ上の範囲）を選び、右のパーツの枠へ入れる。
	// パーツの指定は ROM も編集中のマップも変えない。保存先は resource\mapassist の下（MapAssistData.cs）
	//-------------------------------------------------------------------------------
	public partial class MapAssistForm : Form
	{
		// 右の「中身」の欄に並べる枠 1 つ（縁つきの 13 個の枠、面のブロック 1 個、部品の本体など）
		private sealed class SlotBox
		{
			public string Slot;
			// 面のブロックのときだけ、何番目のブロックか（それ以外は -1）
			public int Member = -1;
			public Rectangle Bounds;
			public MapAssistGrid Grid;
			// 空のときに出す文字（矢印など）
			public string Label;
			public int CellSize;
		}

		// パーツの一覧の 1 行
		private sealed class PartItem
		{
			public MapAssistPart Part;

			public override string ToString()
			{
				return string.Format("{0}　［{1}／{2}］", this.Part.Name, KindName(this.Part.Kind), RoleName(this.Part.Role));
			}
		}

		private MapAssistContext context;
		private MapAssistSet set;
		private Bitmap mapImage;
		private bool dirty;
		private bool updating;
		private string selectedSlot;
		private int selectedMember = -1;
		private readonly List<SlotBox> slotBoxes = new List<SlotBox>();
		private readonly Dictionary<int, Color> blockMarkers = new Dictionary<int, Color>();

		// 「今のマップを読み直す」が押されたとき（マップエディタが今の内容を渡し直す）
		internal event EventHandler ReloadRequested;

		//-------------------------------------------------------------------------------
		// デザイナー用（引数なし）の初期化処理
		//-------------------------------------------------------------------------------
		public MapAssistForm()
		{
			this.InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		// 今のマップの材料を受け取って初期化する処理（マップを選んでいなければ null）
		//-------------------------------------------------------------------------------
		internal MapAssistForm(MapAssistContext context) : this()
		{
			Localizer.Apply(this);
			foreach (MapAssistRole role in Enum.GetValues(typeof(MapAssistRole)))
			{
				this.cmbAssistRole.Items.Add(RoleName(role));
			}
			// 描き直しのちらつきを抑える
			typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this.pnlAssistDetail, true);
			this.canvasAssistBlocks.SelectionChanged += (sender, e) => this.UpdateSelectionLabel();
			this.canvasAssistMap.SelectionChanged += (sender, e) => this.UpdateSelectionLabel();
			this.canvasAssistBlocks.MarkerProvider = this.GetBlockMarker;
			this.SetContext(context);
		}

		// 保存していない変更があるか
		internal bool IsDirty
		{
			get { return this.dirty; }
		}

		// 今の組で使えるパーツ（検証用）
		internal IList<MapAssistPart> Parts
		{
			get { return this.set != null ? (IList<MapAssistPart>)this.set.Parts : new List<MapAssistPart>(); }
		}

		//-------------------------------------------------------------------------------
		// 役割の名前（画面用）
		//-------------------------------------------------------------------------------
		internal static string RoleName(MapAssistRole role)
		{
			switch (role)
			{
				case MapAssistRole.Ground: return Localizer.T("地面");
				case MapAssistRole.Grass: return Localizer.T("草むら");
				case MapAssistRole.Path: return Localizer.T("道");
				case MapAssistRole.Water: return Localizer.T("水");
				case MapAssistRole.Tree: return Localizer.T("木・森");
				case MapAssistRole.Cliff: return Localizer.T("崖・段差");
				case MapAssistRole.Fence: return Localizer.T("柵");
				case MapAssistRole.Building: return Localizer.T("建物");
				case MapAssistRole.Decoration: return Localizer.T("飾り");
				default: return Localizer.T("その他");
			}
		}

		//-------------------------------------------------------------------------------
		// 種類の名前（画面用）
		//-------------------------------------------------------------------------------
		internal static string KindName(MapAssistKind kind)
		{
			switch (kind)
			{
				case MapAssistKind.Area: return Localizer.T("面");
				case MapAssistKind.Edge: return Localizer.T("縁つき");
				case MapAssistKind.Stamp: return Localizer.T("部品");
				default: return Localizer.T("くり返し");
			}
		}

		//-------------------------------------------------------------------------------
		// 役割ごとの印の色（ブロック一覧で、指定済みのブロックの左上に付ける）
		//-------------------------------------------------------------------------------
		private static Color RoleColor(MapAssistRole role)
		{
			switch (role)
			{
				case MapAssistRole.Ground: return Color.FromArgb(222, 184, 120);
				case MapAssistRole.Grass: return Color.FromArgb(120, 220, 110);
				case MapAssistRole.Path: return Color.FromArgb(255, 170, 70);
				case MapAssistRole.Water: return Color.FromArgb(80, 160, 255);
				case MapAssistRole.Tree: return Color.FromArgb(30, 140, 70);
				case MapAssistRole.Cliff: return Color.FromArgb(150, 100, 70);
				case MapAssistRole.Fence: return Color.FromArgb(190, 190, 200);
				case MapAssistRole.Building: return Color.FromArgb(240, 90, 90);
				case MapAssistRole.Decoration: return Color.FromArgb(240, 140, 220);
				default: return Color.White;
			}
		}

		//-------------------------------------------------------------------------------
		// 今のマップの材料を入れ替える処理（マップを切り替えたとき・読み直したとき）
		// タイルセットの組が変わるときは、保存していない指定を保存してから、新しい組の指定を読み込む
		//-------------------------------------------------------------------------------
		internal void SetContext(MapAssistContext next)
		{
			bool samePair = this.context != null && next != null && this.set != null
				&& this.context.Primary.SamePlace(next.Primary) && this.context.Secondary.SamePlace(next.Secondary);
			string note = null;
			if (!samePair && this.dirty && this.set != null)
			{
				note = this.SaveDefinitions();
			}
			if (this.context != null && this.context != next)
			{
				this.context.Dispose();
			}
			this.context = next;
			if (!samePair)
			{
				this.set = next != null ? MapAssistSet.Load(next.Primary, next.Secondary) : null;
				this.dirty = false;
			}
			else
			{
				// 同じ組のまま読み直したとき: ブロックを編集した後の識別子に合わせる（保存のときに書く）
				this.set.Primary = next.Primary;
				this.set.Secondary = next.Secondary;
			}
			this.RebuildImages();
			this.RebuildPartList(samePair ? this.lstAssistParts.SelectedIndex : 0);
			this.UpdateTargetLabel();
			this.UpdateSelectionLabel();
			bool ready = this.context != null;
			this.grpAssistParts.Enabled = ready;
			this.grpAssistDetail.Enabled = ready;
			this.btnAssistSave.Enabled = ready;
			this.grpAssistSupport.Enabled = ready;
			this.lblAssistStatus.Text = note ?? (ready ? string.Empty : Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。"));
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧とマップの絵を作り直す処理
		//-------------------------------------------------------------------------------
		private void RebuildImages()
		{
			if (this.mapImage != null)
			{
				this.mapImage.Dispose();
				this.mapImage = null;
			}
			if (this.context == null || this.context.BlockSheet == null)
			{
				this.canvasAssistBlocks.SetImage(null, 2);
				this.canvasAssistMap.SetImage(null, 1);
				return;
			}
			this.canvasAssistBlocks.SetImage(this.context.BlockSheet, 2);
			if (this.context.MapWidth > 0 && this.context.MapHeight > 0)
			{
				// マップの絵は、ブロック一覧の絵からマスごとに写して作る
				this.mapImage = new Bitmap(this.context.MapWidth * 16, this.context.MapHeight * 16);
				using (Graphics g = Graphics.FromImage(this.mapImage))
				{
					g.Clear(UiTheme.Canvas);
					for (int y = 0; y < this.context.MapHeight; y++)
					{
						for (int x = 0; x < this.context.MapWidth; x++)
						{
							int id = this.context.MapBlocks[y * this.context.MapWidth + x];
							Rectangle source = new Rectangle(id % MapAssistContext.SheetColumns * 16, id / MapAssistContext.SheetColumns * 16, 16, 16);
							if (source.Bottom <= this.context.BlockSheet.Height)
							{
								g.DrawImage(this.context.BlockSheet, new Rectangle(x * 16, y * 16, 16, 16), source, GraphicsUnit.Pixel);
							}
						}
					}
				}
			}
			this.canvasAssistMap.SetImage(this.mapImage, this.chkAssistMapZoom.Checked ? 2 : 1);
		}

		//-------------------------------------------------------------------------------
		// 持っている絵を破棄する処理（画面を閉じるとき）
		//-------------------------------------------------------------------------------
		private void DisposeImages()
		{
			if (this.mapImage != null)
			{
				this.mapImage.Dispose();
				this.mapImage = null;
			}
			if (this.context != null)
			{
				this.context.Dispose();
				this.context = null;
			}
		}

		//-------------------------------------------------------------------------------
		// 上の案内（対象のマップ・タイルセット・指定ファイル）を書き直す処理
		//-------------------------------------------------------------------------------
		private void UpdateTargetLabel()
		{
			if (this.context == null || this.set == null)
			{
				this.lblAssistTarget.Text = Localizer.T("対象: -");
				return;
			}
			string none = Localizer.T("（まだありません）");
			this.lblAssistTarget.Text = string.Format(Localizer.T("対象: {0}　タイルセット1 = 0x{1:X6}（{2} ブロック）／タイルセット2 = 0x{3:X6}（{4} ブロック）{5}指定ファイル（resource\\mapassist）: タイルセット1 = {6}、タイルセット2 = {7}"),
				this.context.MapLabel, this.context.Primary.HeaderOffset, this.context.Primary.BlockCount, this.context.Secondary.HeaderOffset, this.context.Secondary.BlockCount,
				Environment.NewLine, this.set.PrimaryFileName ?? none, this.set.SecondaryFileName ?? none);
		}

		//-------------------------------------------------------------------------------
		// 左で選んでいる範囲の案内を書き直す処理
		//-------------------------------------------------------------------------------
		private void UpdateSelectionLabel()
		{
			bool map = this.tabAssistSource.SelectedTab == this.tabAssistMap;
			Rectangle r = map ? this.canvasAssistMap.Selection : this.canvasAssistBlocks.Selection;
			if (r.IsEmpty)
			{
				this.lblAssistSelection.Text = Localizer.T("選択: なし（クリックかドラッグで選びます）");
			}
			else if (map)
			{
				this.lblAssistSelection.Text = string.Format(Localizer.T("選択: マップの ({0}, {1}) から 幅 {2} × 高さ {3}"), r.X, r.Y, r.Width, r.Height);
			}
			else
			{
				int first = r.Y * MapAssistContext.SheetColumns + r.X;
				this.lblAssistSelection.Text = r.Width == 1 && r.Height == 1
					? string.Format(Localizer.T("選択: ブロック 0x{0:X3}"), first)
					: string.Format(Localizer.T("選択: ブロック 0x{0:X3} から 幅 {1} × 高さ {2}"), first, r.Width, r.Height);
			}
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧のマスに付ける印の色を返す処理（どのパーツにも入っていなければ null）
		//-------------------------------------------------------------------------------
		private Color? GetBlockMarker(int x, int y)
		{
			Color color;
			return this.blockMarkers.TryGetValue(y * MapAssistContext.SheetColumns + x, out color) ? color : (Color?)null;
		}

		//-------------------------------------------------------------------------------
		// 印の表（ブロックの通し番号 → 役割の色）を作り直す処理
		//-------------------------------------------------------------------------------
		private void RebuildMarkers()
		{
			this.blockMarkers.Clear();
			if (this.context != null && this.set != null)
			{
				foreach (MapAssistPart part in this.set.Parts)
				{
					foreach (MapAssistCell cell in part.AllCells())
					{
						int id = this.context.ToGlobal(cell);
						if (id >= 0 && !this.blockMarkers.ContainsKey(id))
						{
							this.blockMarkers[id] = RoleColor(part.Role);
						}
					}
				}
			}
			this.canvasAssistBlocks.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// パーツの一覧を作り直し、指定の行を選ぶ処理
		//-------------------------------------------------------------------------------
		private void RebuildPartList(int selectIndex)
		{
			this.updating = true;
			try
			{
				this.lstAssistParts.BeginUpdate();
				this.lstAssistParts.Items.Clear();
				if (this.set != null)
				{
					foreach (MapAssistPart part in this.set.Parts)
					{
						this.lstAssistParts.Items.Add(new PartItem { Part = part });
					}
				}
				this.lstAssistParts.EndUpdate();
				this.lstAssistParts.SelectedIndex = this.lstAssistParts.Items.Count == 0 ? -1 : Math.Max(0, Math.Min(selectIndex, this.lstAssistParts.Items.Count - 1));
			}
			finally
			{
				this.updating = false;
			}
			this.RebuildMarkers();
			this.ShowSelectedPart();
		}

		// 選んでいるパーツ（無ければ null）
		private MapAssistPart SelectedPart
		{
			get
			{
				PartItem item = this.lstAssistParts.SelectedItem as PartItem;
				return item != null ? item.Part : null;
			}
		}

		//-------------------------------------------------------------------------------
		// 選んでいるパーツの名前・役割・中身を右の欄に出す処理
		//-------------------------------------------------------------------------------
		private void ShowSelectedPart()
		{
			MapAssistPart part = this.SelectedPart;
			this.updating = true;
			try
			{
				this.txtAssistName.Text = part != null ? part.Name : string.Empty;
				this.cmbAssistRole.SelectedIndex = part != null ? (int)part.Role : -1;
				this.lblAssistKind.Text = part != null ? KindName(part.Kind) : "-";
			}
			finally
			{
				this.updating = false;
			}
			bool has = part != null;
			this.txtAssistName.Enabled = has;
			this.cmbAssistRole.Enabled = has;
			this.btnAssistAssign.Enabled = has;
			this.btnAssistClear.Enabled = has;
			this.btnAssistDeletePart.Enabled = has;
			this.selectedMember = -1;
			// 初めに選んでおく枠: 縁つきは中央、くり返しは本体
			this.selectedSlot = part == null ? null : (part.Kind == MapAssistKind.Edge ? "C" : (part.Kind == MapAssistKind.Area ? MapAssistPart.SlotBlocks : MapAssistPart.SlotBody));
			this.lblAssistHint.Text = part == null ? Localizer.T("左下の「＋ 面」などでパーツを作り、左で選んだブロックを入れていきます。") : HintFor(part.Kind);
			this.btnAssistClear.Text = part != null && part.Kind == MapAssistKind.Area ? Localizer.T("選んだブロックを外す") : Localizer.T("選んだ枠を空にする");
			this.LayoutDetail();
		}

		//-------------------------------------------------------------------------------
		// 種類ごとの使い方の説明を返す処理
		//-------------------------------------------------------------------------------
		private static string HintFor(MapAssistKind kind)
		{
			switch (kind)
			{
				case MapAssistKind.Area:
					return Localizer.T("面: 地面・草むらなど、同じ役割で置き換えられるブロックの集まりです。左でブロックを選び（ドラッグでまとめて選べます）、「左で選んだ所を入れる」を押します。");
				case MapAssistKind.Edge:
					return Localizer.T("縁つき: 周りとの境目に専用のブロックがある地形です。左の 3×3 は外側の縁（中央・辺・角）、右の 2×2 は内側の角（地形がへこむ所。矢印は外側がある向き）です。枠をクリックしてからブロックを入れます。ブロック一覧で 3×3 を選んで入れると、外側の 9 個をまとめて入れられます（2×2 なら内側の角）。");
				case MapAssistKind.Stamp:
					return Localizer.T("部品: 家・大きい木・看板など、決まった並びをそのまま使うものです。左の「マップ」で範囲をドラッグして入れると、移動エリア（通れる・通れない）も一緒に覚えます。");
				default:
					return Localizer.T("くり返し: 森のように、同じ並びをくり返して広げるものです。「くり返す単位」に 1 単位（例: 木 1 本ぶん）、「上端」「下端」に、いちばん上・下の行だけで使う並びを入れます。3 つの幅は同じにします。");
			}
		}

		//-------------------------------------------------------------------------------
		// 右の「中身」の欄に並べる枠の位置を決める処理（種類ごとに並べ方が違う）
		//-------------------------------------------------------------------------------
		private void LayoutDetail()
		{
			this.slotBoxes.Clear();
			MapAssistPart part = this.SelectedPart;
			int bottom = 0;
			if (part != null)
			{
				int margin = this.LogicalToDeviceUnits(10);
				int width = Math.Max(this.LogicalToDeviceUnits(120), this.pnlAssistDetail.ClientSize.Width - margin * 2);
				if (part.Kind == MapAssistKind.Area)
				{
					int box = this.LogicalToDeviceUnits(36);
					int pitch = this.LogicalToDeviceUnits(40);
					int perRow = Math.Max(1, width / pitch);
					MapAssistGrid blocks = part.GetSlot(MapAssistPart.SlotBlocks);
					int count = blocks != null ? blocks.Cells.Length : 0;
					for (int i = 0; i < count; i++)
					{
						MapAssistGrid one = new MapAssistGrid(1, 1);
						one.Cells[0] = blocks.Cells[i];
						this.slotBoxes.Add(new SlotBox { Slot = MapAssistPart.SlotBlocks, Member = i, Grid = one, CellSize = box, Bounds = new Rectangle(margin + i % perRow * pitch, margin + i / perRow * pitch, box, box) });
					}
					bottom = margin + ((Math.Max(1, count) - 1) / perRow + 1) * pitch;
				}
				else if (part.Kind == MapAssistKind.Edge)
				{
					int box = this.LogicalToDeviceUnits(40);
					int pitch = this.LogicalToDeviceUnits(44);
					int top = this.LogicalToDeviceUnits(30);
					string[] outerLabels = { "↖", "↑", "↗", "←", "中", "→", "↙", "↓", "↘" };
					for (int i = 0; i < 9; i++)
					{
						string name = MapAssistPart.EdgeOuterSlots[i];
						this.slotBoxes.Add(new SlotBox { Slot = name, Grid = part.GetSlot(name), Label = outerLabels[i], CellSize = box, Bounds = new Rectangle(margin + i % 3 * pitch, top + i / 3 * pitch, box, box) });
					}
					int innerLeft = margin + 3 * pitch + this.LogicalToDeviceUnits(28);
					string[] innerLabels = { "↖", "↗", "↙", "↘" };
					for (int i = 0; i < 4; i++)
					{
						string name = MapAssistPart.EdgeInnerSlots[i];
						this.slotBoxes.Add(new SlotBox { Slot = name, Grid = part.GetSlot(name), Label = innerLabels[i], CellSize = box, Bounds = new Rectangle(innerLeft + i % 2 * pitch, top + i / 2 * pitch, box, box) });
					}
					bottom = top + 3 * pitch;
				}
				else
				{
					// 部品・くり返し: 枠を縦に並べる。並びが大きいときは、幅に収まるように 1 マスを小さくする
					int y = this.LogicalToDeviceUnits(26);
					foreach (string name in MapAssistPart.SlotNamesOf(part.Kind))
					{
						MapAssistGrid grid = part.GetSlot(name);
						int cell = this.LogicalToDeviceUnits(32);
						Size size = new Size(this.LogicalToDeviceUnits(96), cell);
						if (grid != null)
						{
							cell = Math.Max(this.LogicalToDeviceUnits(6), Math.Min(cell, width / grid.Width));
							size = new Size(grid.Width * cell, grid.Height * cell);
						}
						this.slotBoxes.Add(new SlotBox { Slot = name, Grid = grid, Label = Localizer.T("（空）"), CellSize = cell, Bounds = new Rectangle(margin, y, size.Width, size.Height) });
						y += size.Height + this.LogicalToDeviceUnits(30);
					}
					bottom = y;
				}
			}
			this.pnlAssistDetail.AutoScrollMinSize = new Size(0, bottom + this.LogicalToDeviceUnits(8));
			this.pnlAssistDetail.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 枠の上に出す見出しを返す処理（部品・くり返しの枠だけ）
		//-------------------------------------------------------------------------------
		private static string SlotCaption(MapAssistKind kind, string slot)
		{
			if (kind == MapAssistKind.Stamp)
			{
				return Localizer.T("並び");
			}
			if (slot == MapAssistPart.SlotTop)
			{
				return Localizer.T("上端");
			}
			return slot == MapAssistPart.SlotBottom ? Localizer.T("下端") : Localizer.T("くり返す単位");
		}

		//-------------------------------------------------------------------------------
		// 右の「中身」の欄を描く処理
		//-------------------------------------------------------------------------------
		private void pnlAssistDetail_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(UiTheme.Input);
			MapAssistPart part = this.SelectedPart;
			if (part == null || this.context == null)
			{
				return;
			}
			g.TranslateTransform(this.pnlAssistDetail.AutoScrollPosition.X, this.pnlAssistDetail.AutoScrollPosition.Y);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			int margin = this.LogicalToDeviceUnits(10);
			if (part.Kind == MapAssistKind.Edge)
			{
				TextRenderer.DrawText(g, Localizer.T("外側の縁（中央・辺・角）"), this.Font, new Point(margin, this.LogicalToDeviceUnits(8)), UiTheme.TextMuted);
				SlotBox firstInner = this.slotBoxes.FirstOrDefault(b => b.Slot == MapAssistPart.EdgeInnerSlots[0]);
				if (firstInner != null)
				{
					TextRenderer.DrawText(g, Localizer.T("内側の角"), this.Font, new Point(firstInner.Bounds.X, this.LogicalToDeviceUnits(8)), UiTheme.TextMuted);
				}
			}
			else if (part.Kind == MapAssistKind.Area && this.slotBoxes.Count == 0)
			{
				TextRenderer.DrawText(g, Localizer.T("まだブロックが入っていません。"), this.Font, new Point(margin, margin), UiTheme.TextMuted);
			}
			foreach (SlotBox box in this.slotBoxes)
			{
				if (part.Kind == MapAssistKind.Stamp || part.Kind == MapAssistKind.Repeat)
				{
					string caption = SlotCaption(part.Kind, box.Slot);
					if (box.Grid != null)
					{
						caption += string.Format("（{0}×{1}）", box.Grid.Width, box.Grid.Height);
					}
					TextRenderer.DrawText(g, caption, this.Font, new Point(box.Bounds.X, box.Bounds.Y - this.LogicalToDeviceUnits(18)), UiTheme.TextMuted);
				}
				using (SolidBrush back = new SolidBrush(UiTheme.Canvas))
				{
					g.FillRectangle(back, box.Bounds);
				}
				if (box.Grid != null)
				{
					int cell = box.Bounds.Width / box.Grid.Width;
					for (int y = 0; y < box.Grid.Height; y++)
					{
						for (int x = 0; x < box.Grid.Width; x++)
						{
							this.DrawBlock(g, box.Grid[x, y], new Rectangle(box.Bounds.X + x * cell, box.Bounds.Y + y * cell, cell, cell));
						}
					}
				}
				else
				{
					TextRenderer.DrawText(g, box.Label ?? string.Empty, this.Font, box.Bounds, UiTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				}
				bool selected = part.Kind == MapAssistKind.Area ? box.Member == this.selectedMember : box.Slot == this.selectedSlot;
				using (Pen pen = new Pen(selected ? UiTheme.Accent : UiTheme.Border, selected ? 3f : 1f))
				{
					g.DrawRectangle(pen, box.Bounds.X - 1, box.Bounds.Y - 1, box.Bounds.Width + 1, box.Bounds.Height + 1);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マス 1 つ（ブロック）を描く処理（空きは斜線、今のタイルセットに無いブロックは「?」）
		//-------------------------------------------------------------------------------
		private void DrawBlock(Graphics g, MapAssistCell cell, Rectangle target)
		{
			if (cell.IsEmpty)
			{
				using (Pen pen = new Pen(UiTheme.Border))
				{
					g.DrawLine(pen, target.Left, target.Top, target.Right - 1, target.Bottom - 1);
				}
				return;
			}
			int id = this.context.ToGlobal(cell);
			Rectangle source = new Rectangle(id % MapAssistContext.SheetColumns * 16, id / MapAssistContext.SheetColumns * 16, 16, 16);
			if (id < 0 || source.Bottom > this.context.BlockSheet.Height)
			{
				TextRenderer.DrawText(g, "?", this.Font, target, UiTheme.Warning, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				return;
			}
			g.DrawImage(this.context.BlockSheet, target, source, GraphicsUnit.Pixel);
		}

		//-------------------------------------------------------------------------------
		// 「中身」の欄でクリックした枠を選ぶ処理
		//-------------------------------------------------------------------------------
		private void pnlAssistDetail_MouseDown(object sender, MouseEventArgs e)
		{
			Point p = new Point(e.X - this.pnlAssistDetail.AutoScrollPosition.X, e.Y - this.pnlAssistDetail.AutoScrollPosition.Y);
			foreach (SlotBox box in this.slotBoxes)
			{
				if (Rectangle.Inflate(box.Bounds, 2, 2).Contains(p))
				{
					this.selectedSlot = box.Slot;
					this.selectedMember = box.Member;
					this.pnlAssistDetail.Invalidate();
					return;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 欄の大きさが変わったら、枠を並べ直す処理
		//-------------------------------------------------------------------------------
		private void pnlAssistDetail_Resize(object sender, EventArgs e)
		{
			this.LayoutDetail();
		}

		//-------------------------------------------------------------------------------
		// 左で選んでいる範囲を、マスの並びにする処理（選んでいない・データのあるブロックが 1 つも無いときは null）
		// ブロック一覧から選んだときの移動エリアは、今のマップでそのブロックにいちばん多く付いている値（無ければ未定）
		//-------------------------------------------------------------------------------
		private MapAssistGrid GetSelectionGrid()
		{
			if (this.context == null)
			{
				return null;
			}
			bool map = this.tabAssistSource.SelectedTab == this.tabAssistMap;
			Rectangle r = map ? this.canvasAssistMap.Selection : this.canvasAssistBlocks.Selection;
			if (r.IsEmpty)
			{
				return null;
			}
			MapAssistGrid grid = new MapAssistGrid(r.Width, r.Height);
			for (int y = 0; y < r.Height; y++)
			{
				for (int x = 0; x < r.Width; x++)
				{
					if (map)
					{
						int index = (r.Y + y) * this.context.MapWidth + r.X + x;
						grid[x, y] = this.context.ToCell(this.context.MapBlocks[index], this.context.MapCollisions[index]);
					}
					else
					{
						int id = (r.Y + y) * MapAssistContext.SheetColumns + r.X + x;
						grid[x, y] = this.context.ToCell(id, this.context.MostCommonCollision(id));
					}
				}
			}
			return grid.IsBlank ? null : grid;
		}

		//-------------------------------------------------------------------------------
		// 新しいパーツの名前を決める処理（「面 1」「面 2」…のうち、まだ無いもの）
		//-------------------------------------------------------------------------------
		private string NewPartName(MapAssistKind kind)
		{
			for (int i = 1; ; i++)
			{
				string name = string.Format("{0} {1}", KindName(kind), i);
				if (!this.set.Parts.Any(p => p.Name == name))
				{
					return name;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// パーツを 1 つ足して選ぶ処理（左で何か選んでいれば、そのまま入れる）
		//-------------------------------------------------------------------------------
		internal MapAssistPart AddPart(MapAssistKind kind)
		{
			if (this.set == null)
			{
				return null;
			}
			MapAssistRole role = kind == MapAssistKind.Area ? MapAssistRole.Ground : (kind == MapAssistKind.Edge ? MapAssistRole.Water : (kind == MapAssistKind.Stamp ? MapAssistRole.Building : MapAssistRole.Tree));
			MapAssistPart part = new MapAssistPart { Name = this.NewPartName(kind), Kind = kind, Role = role };
			this.set.Parts.Add(part);
			this.dirty = true;
			this.RebuildPartList(this.set.Parts.Count - 1);
			if (this.GetSelectionGrid() != null)
			{
				// 入れられない選び方（縁つきで 3×3 でも 1 個でもない、など）のときは、空のパーツのままにする
				string error = this.AssignSelection();
				this.lblAssistStatus.Text = error ?? string.Format(Localizer.T("「{0}」を作り、左で選んでいた所を入れました。"), part.Name);
			}
			else
			{
				this.lblAssistStatus.Text = string.Format(Localizer.T("「{0}」を作りました。左でブロックを選んで入れてください。"), part.Name);
			}
			return part;
		}

		//-------------------------------------------------------------------------------
		// 左で選んでいる所を、選んでいるパーツ（の枠）へ入れる処理（成功なら null、入れられなければ理由）
		//-------------------------------------------------------------------------------
		internal string AssignSelection()
		{
			MapAssistPart part = this.SelectedPart;
			if (part == null)
			{
				return Localizer.T("先に、入れる先のパーツを選んでください。");
			}
			MapAssistGrid selection = this.GetSelectionGrid();
			if (selection == null)
			{
				return Localizer.T("左でブロック（またはマップの範囲）を選んでください。データの無いブロックだけを選んでいるときも入れられません。");
			}
			switch (part.Kind)
			{
				case MapAssistKind.Area:
				{
					// まだ入っていないブロックだけを、後ろへ足す
					MapAssistGrid old = part.GetSlot(MapAssistPart.SlotBlocks);
					List<MapAssistCell> cells = old != null ? old.Cells.Where(c => !c.IsEmpty).ToList() : new List<MapAssistCell>();
					foreach (MapAssistCell cell in selection.Cells)
					{
						if (!cell.IsEmpty && !cells.Any(c => c.Tileset == cell.Tileset && c.Block == cell.Block))
						{
							cells.Add(cell);
						}
					}
					MapAssistGrid merged = new MapAssistGrid(cells.Count, 1);
					cells.CopyTo(merged.Cells);
					part.Slots[MapAssistPart.SlotBlocks] = merged;
					break;
				}
				case MapAssistKind.Edge:
				{
					if (selection.Width == 3 && selection.Height == 3)
					{
						// 3×3 は、外側の 9 個（左上・上・右上／左・中央・右／左下・下・右下）にそのまま入れる
						for (int i = 0; i < 9; i++)
						{
							SetSingleSlot(part, MapAssistPart.EdgeOuterSlots[i], selection.Cells[i]);
						}
					}
					else if (selection.Width == 2 && selection.Height == 2)
					{
						for (int i = 0; i < 4; i++)
						{
							SetSingleSlot(part, MapAssistPart.EdgeInnerSlots[i], selection.Cells[i]);
						}
					}
					else if (selection.Cells.Length == 1)
					{
						string slot = this.selectedSlot ?? "C";
						SetSingleSlot(part, slot, selection.Cells[0]);
						// 続けて入れやすいように、次の枠へ進む
						string[] names = MapAssistPart.SlotNamesOf(MapAssistKind.Edge);
						this.selectedSlot = names[(Array.IndexOf(names, slot) + 1) % names.Length];
					}
					else
					{
						return Localizer.T("縁つきには、ブロックを 1 個ずつ（選んだ枠へ）、または 3×3（外側の 9 個）か 2×2（内側の角）で入れてください。");
					}
					break;
				}
				case MapAssistKind.Stamp:
					part.Slots[MapAssistPart.SlotBody] = selection;
					break;
				default:
				{
					string slot = this.selectedSlot ?? MapAssistPart.SlotBody;
					foreach (string other in MapAssistPart.SlotNamesOf(MapAssistKind.Repeat))
					{
						MapAssistGrid grid = part.GetSlot(other);
						if (other != slot && grid != null && grid.Width != selection.Width)
						{
							return string.Format(Localizer.T("幅が合いません。ほかの枠は幅 {0} なので、同じ幅（今の選択は {1}）で選び直してください。"), grid.Width, selection.Width);
						}
					}
					part.Slots[slot] = selection;
					break;
				}
			}
			this.AfterPartChanged();
			return null;
		}

		//-------------------------------------------------------------------------------
		// 縁つきの枠 1 つにブロックを入れる処理（空きのマスなら、その枠を空にする）
		//-------------------------------------------------------------------------------
		private static void SetSingleSlot(MapAssistPart part, string slot, MapAssistCell cell)
		{
			if (cell.IsEmpty)
			{
				part.Slots.Remove(slot);
				return;
			}
			MapAssistGrid grid = new MapAssistGrid(1, 1);
			grid.Cells[0] = cell;
			part.Slots[slot] = grid;
		}

		//-------------------------------------------------------------------------------
		// 選んでいる枠を空にする処理（面は、選んだブロックを外す。選んでいなければ最後の 1 個）
		//-------------------------------------------------------------------------------
		internal void ClearSelectedSlot()
		{
			MapAssistPart part = this.SelectedPart;
			if (part == null)
			{
				return;
			}
			if (part.Kind == MapAssistKind.Area)
			{
				MapAssistGrid old = part.GetSlot(MapAssistPart.SlotBlocks);
				if (old == null)
				{
					return;
				}
				List<MapAssistCell> cells = old.Cells.ToList();
				cells.RemoveAt(this.selectedMember >= 0 && this.selectedMember < cells.Count ? this.selectedMember : cells.Count - 1);
				if (cells.Count == 0)
				{
					part.Slots.Remove(MapAssistPart.SlotBlocks);
				}
				else
				{
					MapAssistGrid rest = new MapAssistGrid(cells.Count, 1);
					cells.CopyTo(rest.Cells);
					part.Slots[MapAssistPart.SlotBlocks] = rest;
				}
				this.selectedMember = -1;
			}
			else if (this.selectedSlot != null)
			{
				part.Slots.Remove(this.selectedSlot);
			}
			this.AfterPartChanged();
		}

		//-------------------------------------------------------------------------------
		// パーツの中身を変えた後の更新（未保存の印・一覧の文字・ブロック一覧の印・中身の欄）
		//-------------------------------------------------------------------------------
		private void AfterPartChanged()
		{
			this.dirty = true;
			int index = this.lstAssistParts.SelectedIndex;
			if (index >= 0)
			{
				this.updating = true;
				this.lstAssistParts.Items[index] = this.lstAssistParts.Items[index];
				this.updating = false;
			}
			this.RebuildMarkers();
			this.LayoutDetail();
		}

		//-------------------------------------------------------------------------------
		// 一覧の index 番目のパーツを選ぶ処理（検証用）
		//-------------------------------------------------------------------------------
		internal void SelectPartIndex(int index)
		{
			this.lstAssistParts.SelectedIndex = index;
		}

		//-------------------------------------------------------------------------------
		// 「中身」の欄の枠を名前で選ぶ処理（検証用）
		//-------------------------------------------------------------------------------
		internal void SelectSlot(string slot)
		{
			this.selectedSlot = slot;
			this.pnlAssistDetail.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 左のタブ（ブロック一覧／マップ）と、選ぶ範囲を設定する処理（検証用）
		//-------------------------------------------------------------------------------
		internal void SelectSource(bool map, Rectangle cells)
		{
			this.tabAssistSource.SelectedTab = map ? this.tabAssistMap : this.tabAssistBlocks;
			(map ? this.canvasAssistMap : this.canvasAssistBlocks).SetSelection(cells);
		}

		//-------------------------------------------------------------------------------
		// 指定を保存する処理（結果の案内文を返す）
		//-------------------------------------------------------------------------------
		internal string SaveDefinitions()
		{
			if (this.set == null)
			{
				return string.Empty;
			}
			try
			{
				List<string> written = this.set.Save();
				this.dirty = false;
				this.UpdateTargetLabel();
				return written.Count == 0
					? Localizer.T("保存するパーツがありません。")
					: string.Format(Localizer.T("保存しました: {0}"), string.Join(Localizer.T("、"), written));
			}
			catch (Exception ex)
			{
				return string.Format(Localizer.T("保存できませんでした: {0}"), ex.Message);
			}
		}

		//-------------------------------------------------------------------------------
		// 「＋ 面」
		//-------------------------------------------------------------------------------
		private void btnAssistAddArea_Click(object sender, EventArgs e)
		{
			this.AddPart(MapAssistKind.Area);
		}

		//-------------------------------------------------------------------------------
		// 「＋ 縁つき」
		//-------------------------------------------------------------------------------
		private void btnAssistAddEdge_Click(object sender, EventArgs e)
		{
			this.AddPart(MapAssistKind.Edge);
		}

		//-------------------------------------------------------------------------------
		// 「＋ 部品」
		//-------------------------------------------------------------------------------
		private void btnAssistAddStamp_Click(object sender, EventArgs e)
		{
			this.AddPart(MapAssistKind.Stamp);
		}

		//-------------------------------------------------------------------------------
		// 「＋ くり返し」
		//-------------------------------------------------------------------------------
		private void btnAssistAddRepeat_Click(object sender, EventArgs e)
		{
			this.AddPart(MapAssistKind.Repeat);
		}

		//-------------------------------------------------------------------------------
		// 自動で作った候補のパーツか（名前が「自動: 」で始まるもの。名前を変えたパーツは、利用者が直したものとして扱う）
		//-------------------------------------------------------------------------------
		private static bool IsAutoPart(MapAssistPart part)
		{
			return part.Name.StartsWith(MapAssistAnalyzer.AutoPrefix, StringComparison.Ordinal) || part.Name.StartsWith(Localizer.T(MapAssistAnalyzer.AutoPrefix), StringComparison.Ordinal);
		}

		//-------------------------------------------------------------------------------
		// マップでの使われ方と挙動の値から、パーツの候補を作って一覧に足す処理（結果の案内文を返す）
		// 前に作った候補（名前が「自動: 」のままのパーツ）は作り直す。利用者が指定したパーツは残す
		//-------------------------------------------------------------------------------
		internal string CreateAutoCandidates()
		{
			if (this.context == null || this.set == null || this.context.AutoInputProvider == null)
			{
				return Localizer.T("マップが選ばれていません。メインの画面の左の一覧からマップを選んでください。");
			}
			MapAssistAutoInput input = this.context.AutoInputProvider();
			// 利用者が指定したパーツのブロックは、面の候補から外す
			HashSet<int> reserved = new HashSet<int>();
			foreach (MapAssistPart part in this.set.Parts.Where(p => !IsAutoPart(p)))
			{
				foreach (MapAssistCell cell in part.AllCells())
				{
					reserved.Add(this.context.ToGlobal(cell));
				}
			}
			List<MapAssistPart> candidates = MapAssistAnalyzer.Analyze(this.context, input, reserved, Localizer.T);
			this.set.Parts.RemoveAll(IsAutoPart);
			int first = this.set.Parts.Count;
			this.set.Parts.AddRange(candidates);
			this.dirty = true;
			this.RebuildPartList(first);
			return string.Format(Localizer.T("候補を {0} 個作りました（面 {1}・縁つき {2}・部品 {3}・くり返し {4}）。調べたマップは {5} 枚（うちタイルセット2 も同じもの {6} 枚）です。名前が「{7}」で始まるパーツが候補なので、中身を確かめて直してください。名前を変えたパーツは、候補を作り直しても残ります。"),
				candidates.Count, candidates.Count(p => p.Kind == MapAssistKind.Area), candidates.Count(p => p.Kind == MapAssistKind.Edge), candidates.Count(p => p.Kind == MapAssistKind.Stamp),
				candidates.Count(p => p.Kind == MapAssistKind.Repeat), input.Samples.Count, input.Samples.Count(s => s.SamePair), Localizer.T(MapAssistAnalyzer.AutoPrefix).Trim());
		}

		//-------------------------------------------------------------------------------
		// 「マップから候補を作る（自動）」: 前に作った候補があれば、作り直してよいか確かめてから作る
		//-------------------------------------------------------------------------------
		private void btnAssistAuto_Click(object sender, EventArgs e)
		{
			if (this.set == null)
			{
				return;
			}
			int old = this.set.Parts.Count(IsAutoPart);
			if (old > 0 && MessageBox.Show(this, string.Format(Localizer.T("前に作った候補（名前が「{0}」で始まるパーツ {1} 個）を作り直します。よろしいですか？（名前を変えたパーツと、自分で作ったパーツは残ります）"), Localizer.T(MapAssistAnalyzer.AutoPrefix).Trim(), old),
				this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			this.Cursor = Cursors.WaitCursor;
			try
			{
				this.lblAssistStatus.Text = this.CreateAutoCandidates();
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		//-------------------------------------------------------------------------------
		// 「選んだパーツを削除」
		//-------------------------------------------------------------------------------
		private void btnAssistDeletePart_Click(object sender, EventArgs e)
		{
			MapAssistPart part = this.SelectedPart;
			if (part == null)
			{
				return;
			}
			if (MessageBox.Show(this, string.Format(Localizer.T("パーツ「{0}」を削除しますか？"), part.Name), this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
			{
				return;
			}
			int index = this.lstAssistParts.SelectedIndex;
			this.set.Parts.Remove(part);
			this.dirty = true;
			this.RebuildPartList(index);
		}

		//-------------------------------------------------------------------------------
		// 「左で選んだ所を入れる」
		//-------------------------------------------------------------------------------
		private void btnAssistAssign_Click(object sender, EventArgs e)
		{
			MapAssistPart part = this.SelectedPart;
			this.lblAssistStatus.Text = this.AssignSelection() ?? (part != null ? string.Format(Localizer.T("「{0}」に入れました。"), part.Name) : string.Empty);
		}

		//-------------------------------------------------------------------------------
		// 「選んだ枠を空にする」
		//-------------------------------------------------------------------------------
		private void btnAssistClear_Click(object sender, EventArgs e)
		{
			this.ClearSelectedSlot();
		}

		//-------------------------------------------------------------------------------
		// 一覧で別のパーツを選んだら、中身を出し直す処理
		//-------------------------------------------------------------------------------
		private void lstAssistParts_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this.updating)
			{
				this.ShowSelectedPart();
			}
		}

		//-------------------------------------------------------------------------------
		// 名前を変えたら、パーツと一覧に反映する処理
		//-------------------------------------------------------------------------------
		private void txtAssistName_TextChanged(object sender, EventArgs e)
		{
			MapAssistPart part = this.SelectedPart;
			if (this.updating || part == null)
			{
				return;
			}
			part.Name = this.txtAssistName.Text.Trim();
			this.dirty = true;
			this.updating = true;
			this.lstAssistParts.Items[this.lstAssistParts.SelectedIndex] = this.lstAssistParts.SelectedItem;
			this.updating = false;
		}

		//-------------------------------------------------------------------------------
		// 役割を変えたら、パーツ・一覧・ブロック一覧の印に反映する処理
		//-------------------------------------------------------------------------------
		private void cmbAssistRole_SelectedIndexChanged(object sender, EventArgs e)
		{
			MapAssistPart part = this.SelectedPart;
			if (this.updating || part == null || this.cmbAssistRole.SelectedIndex < 0)
			{
				return;
			}
			part.Role = (MapAssistRole)this.cmbAssistRole.SelectedIndex;
			this.dirty = true;
			this.updating = true;
			this.lstAssistParts.Items[this.lstAssistParts.SelectedIndex] = this.lstAssistParts.SelectedItem;
			this.updating = false;
			this.RebuildMarkers();
		}

		//-------------------------------------------------------------------------------
		// 左のタブ（ブロック一覧／マップ）を切り替えたら、選択の案内を書き直す処理
		//-------------------------------------------------------------------------------
		private void tabAssistSource_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdateSelectionLabel();
		}

		//-------------------------------------------------------------------------------
		// 「マップを 2 倍で表示」を切り替えたら、マップの絵の倍率を変える処理
		//-------------------------------------------------------------------------------
		private void chkAssistMapZoom_CheckedChanged(object sender, EventArgs e)
		{
			this.canvasAssistMap.SetImage(this.mapImage, this.chkAssistMapZoom.Checked ? 2 : 1);
			this.UpdateSelectionLabel();
		}

		//-------------------------------------------------------------------------------
		// 「今のマップを読み直す」: マップエディタに、今の内容を渡し直してもらう処理
		//-------------------------------------------------------------------------------
		private void btnAssistReload_Click(object sender, EventArgs e)
		{
			this.ReloadRequested?.Invoke(this, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------------
		// 「指定を保存」
		//-------------------------------------------------------------------------------
		private void btnAssistSave_Click(object sender, EventArgs e)
		{
			this.lblAssistStatus.Text = this.SaveDefinitions();
		}

		//-------------------------------------------------------------------------------
		// 「閉じる」
		//-------------------------------------------------------------------------------
		private void btnAssistClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		//-------------------------------------------------------------------------------
		// 閉じる前に、保存していない指定をどうするか確かめる処理
		// 利用者が閉じたときは聞く。エディタごと閉じるときなどは、聞かずに保存する
		//-------------------------------------------------------------------------------
		private void MapAssistForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!this.dirty || this.set == null)
			{
				return;
			}
			if (e.CloseReason != CloseReason.UserClosing)
			{
				this.SaveDefinitions();
				return;
			}
			DialogResult answer = MessageBox.Show(this, Localizer.T("パーツの指定に、保存していない変更があります。保存しますか？"), this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (answer == DialogResult.Cancel)
			{
				e.Cancel = true;
			}
			else if (answer == DialogResult.Yes)
			{
				this.SaveDefinitions();
			}
		}
	}
}
