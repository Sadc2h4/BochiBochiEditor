using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「自動ペン」（マップの道具「自動ペン (P)」）
	// 使う人は、いつものように「ブロック」の一覧でブロックを選んで、マップをなぞるだけ。
	// 選んだブロックが、マップ作成補助のパーツ（面・縁つき・部品・くり返し）のどれに入っているかを内部で調べ、そのパーツとして塗る。
	//   縁つき（水・崖）… なぞった所を中央で塗り、まわりの縁を付け直す（塗りながら岸ができる）
	//   くり返し（森）  … 模様の単位にそろえて並べ、上端・下端も付け直す
	//   部品（建物）    … まるごと 1 つ置く（選んだブロックがクリックした所に来る）
	//   面（地面）      … 選んだブロックで塗る（移動エリアも付く）
	// どのパーツにも入っていないブロックや、ブロックをまとめて選んでいるときは、ペンと同じ動きになる。
	// パーツの指定がまだ無いタイルセットでは、道具を選んだときに ROM のマップから候補を自動で作って保存する（画面には出さない）。
	// 画面に出すのは、道具を選んでいる間の細い帯（今のブロックがどう描かれるかの 1 行と、筆の大きさ）だけ。
	// 1 回のドラッグが 1 回の「戻る」になる。ROM へは今までどおり「確定」で書く
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 選んだブロックから引いた、塗り方（どのパーツか・面なら何番目のブロックか・部品なら選んだブロックが部品の中のどこか）
		private sealed class PartBrushTarget
		{
			public MapAssistPart Part;
			public int Member = -1;
			public Point Anchor = Point.Empty;
		}

		// 選べる筆の大きさ（マス）
		private static readonly int[] PartBrushSizes = { 1, 2, 3, 5 };

		// 道具を選んでいる間に出す細い帯の部品
		private Label lblPartBrushHint;
		private FlowLayoutPanel flpPartBrushSizes;
		private readonly List<Button> partBrushSizeButtons = new List<Button>();
		private int partBrushSize = 1;

		// 今のタイルセットの組のパーツと、塗るときの材料、ブロックの番号から塗り方を引く表
		private MapAssistSet partBrushSet;
		private MapAssistContext partBrushContext;
		private int[] partBrushCollisionTable;
		private MapAssistAutoInput partBrushSamples;
		private readonly Dictionary<int, PartBrushTarget> partBrushLookup = new Dictionary<int, PartBrushTarget>();
		// 候補を自動で作ろうとしたタイルセットの組（何度も作り直さないため）
		private readonly HashSet<string> partBrushAutoTried = new HashSet<string>();
		private ImageButton btnPartBrushGroups;
		// ブロックの選択に関係なく使うパーツ（検証用）
		private PartBrushTarget partBrushForced;

		// ドラッグ中か・このドラッグの塗り方・直前に塗ったマス・このドラッグで変わったか・カーソルのあるマス
		private bool isPaintingParts;
		private PartBrushTarget partBrushStroke;
		private Rectangle partBrushLastRange = Rectangle.Empty;
		// 直前になぞったマス（マウスを速く動かしたときに、間のマスも塗るため）
		private Point partBrushLastCell = new Point(-1, -1);
		private bool partBrushStrokeChanged;
		private Point partBrushHover = new Point(-1, -1);
		// カーソルが、マスの中の右半分（X = 1）・下半分（Y = 1）にあるか。偶数の大きさの筆を、カーソルが中心になる側へ寄せるのに使う
		private Point partBrushHalf = new Point(1, 1);
		// 最後に枠を描いた範囲（マス）。動いたときに、ここを描き直して消す
		private Rectangle partBrushHoverRect = Rectangle.Empty;

		// 今のブロックで使うパーツ（無ければ null。検証用にも使う）
		internal MapAssistPart SelectedBrushPart
		{
			get
			{
				PartBrushTarget target = this.CurrentPartBrushTarget();
				return target != null ? target.Part : null;
			}
		}

		internal int PartBrushPartCount
		{
			get { return this.partBrushSet != null ? this.partBrushSet.Parts.Count : 0; }
		}

		//-------------------------------------------------------------------------------
		// 道具を選んでいる間に出す細い帯（案内の 1 行と筆の大きさ）を作る処理（道具の初期化のときに呼ぶ）
		//-------------------------------------------------------------------------------
		private void InitializeMapPartBrush()
		{
			if (this.pnlPartBrushBar == null)
			{
				return;
			}
			Control host = this.pnlPartBrushBar;
			host.SuspendLayout();
			this.lblPartBrushHint = new Label
			{
				Dock = DockStyle.Fill,
				AutoSize = false,
				AutoEllipsis = true,
				TextAlign = ContentAlignment.MiddleLeft,
				Padding = new Padding(30, 0, 4, 0),
				Name = "lblPartBrushHint",
			};
			this.flpPartBrushSizes = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Padding = new Padding(0, 0, 30, 0), Name = "flpPartBrushSizes" };
			Label sizeLabel = new Label { AutoSize = true, Text = "筆の大きさ :", Margin = new Padding(2, 9, 4, 0), Name = "lblPartBrushSize" };
			this.flpPartBrushSizes.Controls.Add(sizeLabel);
			foreach (int size in PartBrushSizes)
			{
				int value = size;
				Button button = new Button
				{
					Text = size.ToString(),
					Size = new Size(host.LogicalToDeviceUnits(28), host.LogicalToDeviceUnits(24)),
					Margin = new Padding(0, 4, 2, 0),
					Tag = size,
					Name = "btnPartBrushSize" + size,
					UseVisualStyleBackColor = false,
				};
				button.Click += (sender, e) => this.SetPartBrushSize(value);
				this.mapToolTip.SetToolTip(button, string.Format(Localizer.T("{0}×{0} マスの筆でなぞります（建物などの置く物には使いません）"), size));
				this.partBrushSizeButtons.Add(button);
				this.flpPartBrushSizes.Controls.Add(button);
			}
			// 画像ボタン「タイル自動化設定を開く」（img\Automated Tile Placement_JP.png / _EN.png）: どのブロックが水・崖・木・建物かの指定（マップ作成補助）を開く
			this.btnPartBrushGroups = new ImageButton
			{
				FitWidthToHeight = true,
				ImageMargin = 0,
				Height = host.LogicalToDeviceUnits(32),
				Width = host.LogicalToDeviceUnits(320),
				Margin = new Padding(12, 0, 0, 0),
				Text = "タイル自動化設定を開く",
				Name = "btnPartBrushGroups",
			};
			this.btnPartBrushGroups.Click += (sender, e) => this.ShowMapAssist();
			this.flpPartBrushSizes.Controls.Add(this.btnPartBrushGroups);
			this.UpdatePartBrushGroupsButton();
			// 後から足したものほど先に場所を取るので、案内（残り全部）→ 筆の大きさの順に入れる
			host.Controls.Add(this.lblPartBrushHint);
			host.Controls.Add(this.flpPartBrushSizes);
			host.ResumeLayout(true);
			Localizer.Apply(host);
			Localizer.LanguageChanged += (sender, e) =>
			{
				this.UpdatePartBrushHint();
				this.UpdatePartBrushGroupsButton();
			};
			this.SetPartBrushSize(1);
			this.UpdatePartBrushHint();
			this.UpdatePartBrushBarVisible();
		}

		//-------------------------------------------------------------------------------
		// 画像ボタン「タイル自動化設定を開く」の画像（今の言語のもの）とツールチップを設定する処理
		//-------------------------------------------------------------------------------
		private void UpdatePartBrushGroupsButton()
		{
			if (this.btnPartBrushGroups == null)
			{
				return;
			}
			this.btnPartBrushGroups.ButtonImage = this.LoadButtonImage("Automated Tile Placement");
			this.mapToolTip.SetToolTip(this.btnPartBrushGroups, Localizer.T("タイル自動化設定を開く: どのブロックが水・崖・木・建物かの指定（マップ作成補助）を見る・直す。\n岸や崖が正しく付かないときや、自作のマップチップを使うときは、ここで指定します"));
		}

		//-------------------------------------------------------------------------------
		// 細い帯を、自動ペンをブロックのモードで使っている間だけ出す処理
		//-------------------------------------------------------------------------------
		private void UpdatePartBrushBarVisible()
		{
			if (this.pnlPartBrushBar == null)
			{
				return;
			}
			bool visible = this.currentPaintTool == MapPaintTool.Parts && this.tabEditorMode.SelectedTab == this.tabBlock;
			if (this.pnlPartBrushBar.Visible != visible)
			{
				this.pnlPartBrushBar.Visible = visible;
			}
		}

		//-------------------------------------------------------------------------------
		// 筆の大きさを決める処理（ボタンの選択の表示も合わせる）
		//-------------------------------------------------------------------------------
		internal void SetPartBrushSize(int size)
		{
			this.partBrushSize = Math.Max(1, Math.Min(7, size));
			foreach (Button button in this.partBrushSizeButtons)
			{
				UiTheme.SetToggleButtonState(button, (int)button.Tag == this.partBrushSize);
			}
			if (this.pnlMapCanvas != null)
			{
				this.pnlMapCanvas.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// 今のタイルセットの組のパーツを読み直す処理（マップを替えたとき・補助の画面で保存したとき・道具を選んだときに呼ぶ）
		// 自動ペンを選んでいて、パーツの指定がまだ無いタイルセットなら、候補を自動で作って保存する
		//-------------------------------------------------------------------------------
		internal void RefreshPartBrush()
		{
			if (this.lblPartBrushHint == null)
			{
				return;
			}
			if (this.partBrushContext != null)
			{
				this.partBrushContext.Dispose();
				this.partBrushContext = null;
			}
			this.partBrushSet = null;
			this.partBrushCollisionTable = null;
			this.partBrushSamples = null;
			this.partBrushForced = null;
			MapAssistContext context = this.BuildMapAssistContext();
			if (context != null)
			{
				this.partBrushContext = context;
				this.partBrushSet = MapAssistSet.Load(context.Primary, context.Secondary);
				// 指定ファイルが無い側（タイルセット 1・2）があれば、その分の候補を作る
				if ((this.partBrushSet.PrimaryFileName == null || this.partBrushSet.SecondaryFileName == null) && this.currentPaintTool == MapPaintTool.Parts)
				{
					this.CreatePartBrushCandidates();
				}
			}
			this.BuildPartBrushLookup();
			this.UpdatePartBrushHint();
			this.pnlTilesetPalette.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// ROM のマップでの使われ方からパーツの候補を作って保存する処理（指定ファイルが無い側のタイルセットの分だけ。同じ組では 1 回だけ）
		// タイルセット 1 の指定ファイルがあるとき（同梱の指定など）は、タイルセット 2 のブロックを使う候補（建物など）だけを足す。
		// 作れた数を返す。補助の画面を開いている間は作らない（同じファイルを 2 か所から書かない）
		//-------------------------------------------------------------------------------
		internal int CreatePartBrushCandidates()
		{
			if (this.partBrushContext == null || this.partBrushSet == null || this.partBrushContext.AutoInputProvider == null)
			{
				return 0;
			}
			if (this.mapAssistForm != null && !this.mapAssistForm.IsDisposed)
			{
				return 0;
			}
			string key = this.partBrushContext.Primary.Fingerprint + "/" + this.partBrushContext.Secondary.Fingerprint;
			if (!this.partBrushAutoTried.Add(key))
			{
				return 0;
			}
			bool needPrimary = this.partBrushSet.PrimaryFileName == null;
			bool needSecondary = this.partBrushSet.SecondaryFileName == null;
			Cursor old = this.Cursor;
			this.Cursor = Cursors.WaitCursor;
			try
			{
				MapAssistAutoInput input = this.partBrushContext.AutoInputProvider();
				// もう指定されているブロックは、候補から外す
				HashSet<int> reserved = new HashSet<int>();
				foreach (MapAssistPart part in this.partBrushSet.Parts)
				{
					foreach (MapAssistCell cell in part.AllCells())
					{
						reserved.Add(this.partBrushContext.ToGlobal(cell));
					}
				}
				List<MapAssistPart> candidates = MapAssistAnalyzer.Analyze(this.partBrushContext, input, reserved, Localizer.T)
					.Where(p => p.UsesTileset(2) ? needSecondary : needPrimary).ToList();
				if (candidates.Count == 0)
				{
					return 0;
				}
				this.partBrushSet.Parts.AddRange(candidates);
				this.partBrushSamples = input;
				try
				{
					this.partBrushSet.Save();
				}
				catch (Exception ex)
				{
					// 保存できなくても、この回はそのまま使える（次に開いたときに作り直す）
					WriteErrorLog("自動ペンのパーツの保存", ex);
				}
				return candidates.Count;
			}
			catch (Exception ex)
			{
				WriteErrorLog("自動ペンのパーツの作成", ex);
				return 0;
			}
			finally
			{
				this.Cursor = old;
			}
		}

		//-------------------------------------------------------------------------------
		// ブロックの番号から塗り方を引く表を作る処理
		// 同じブロックがいくつかのパーツにあるときは、縁つき → くり返し → 部品 → 面 の順で先のものを使う
		//-------------------------------------------------------------------------------
		private void BuildPartBrushLookup()
		{
			this.partBrushLookup.Clear();
			if (this.partBrushSet == null || this.partBrushContext == null)
			{
				return;
			}
			MapAssistContext context = this.partBrushContext;
			foreach (MapAssistKind kind in new[] { MapAssistKind.Edge, MapAssistKind.Repeat, MapAssistKind.Stamp, MapAssistKind.Area })
			{
				foreach (MapAssistPart part in this.partBrushSet.Parts.Where(q => q.Kind == kind))
				{
					if (kind == MapAssistKind.Stamp || kind == MapAssistKind.Area)
					{
						MapAssistGrid grid = part.GetSlot(kind == MapAssistKind.Stamp ? MapAssistPart.SlotBody : MapAssistPart.SlotBlocks);
						if (grid == null)
						{
							continue;
						}
						for (int i = 0; i < grid.Cells.Length; i++)
						{
							int id = context.ToGlobal(grid.Cells[i]);
							if (id >= 0 && !this.partBrushLookup.ContainsKey(id))
							{
								this.partBrushLookup[id] = kind == MapAssistKind.Stamp
									? new PartBrushTarget { Part = part, Anchor = new Point(i % grid.Width, i / grid.Width) }
									: new PartBrushTarget { Part = part, Member = i };
							}
						}
						continue;
					}
					// 縁つきは、中央のブロックが入っているものだけ（無いと塗れない）
					if (kind == MapAssistKind.Edge && (part.GetSlot("C") == null || context.ToGlobal(part.GetSlot("C").Cells[0]) < 0))
					{
						continue;
					}
					if (kind == MapAssistKind.Repeat && part.GetSlot(MapAssistPart.SlotBody) == null)
					{
						continue;
					}
					foreach (MapAssistCell cell in part.AllCells())
					{
						int id = context.ToGlobal(cell);
						if (id >= 0 && !this.partBrushLookup.ContainsKey(id))
						{
							this.partBrushLookup[id] = new PartBrushTarget { Part = part };
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 自動ペンを選んでいる間、「ブロック」の一覧で、自動の対象のブロックに薄い紅色を重ねる処理（一覧の描画から呼ぶ）
		// どのブロックを選べば自動で描けるかを、選ぶ前に見分けられるようにする
		//-------------------------------------------------------------------------------
		private void DrawPartBrushPaletteMask(Graphics g, PaletteLayout layout, int scrollX, int scrollY)
		{
			if (this.currentPaintTool != MapPaintTool.Parts || this.tabEditorMode.SelectedTab != this.tabBlock || this.partBrushLookup.Count == 0)
			{
				return;
			}
			RectangleF clip = g.VisibleClipBounds;
			using (SolidBrush mask = new SolidBrush(Color.FromArgb(64, 255, 40, 72)))
			{
				foreach (int id in this.partBrushLookup.Keys)
				{
					foreach (Rectangle cell in this.GetPaletteDisplayRects(new Rectangle(id % PaletteColumns, id / PaletteColumns, 1, 1), layout, scrollX, scrollY))
					{
						if (clip.IntersectsWith(cell))
						{
							g.FillRectangle(mask, cell);
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 「ブロック」の一覧で選んでいるブロックの番号を返す処理（まとめて選んでいるときは -1）
		//-------------------------------------------------------------------------------
		private int SelectedSingleBlock()
		{
			Rectangle rect = this.selectedBlockRect;
			if (this.blockPaletteBitmap == null || rect.Height != 1 || rect.Width < 1 || rect.Width > 2)
			{
				return -1;
			}
			int id = rect.Y * PaletteColumns + rect.X;
			// 3 層のブロックは、2 つで 1 つとして選ばれる
			if (rect.Width == 2 && !this.IsTripleLayerBlock(id))
			{
				return -1;
			}
			return id;
		}

		//-------------------------------------------------------------------------------
		// 今選んでいるブロックの塗り方を返す処理（自動の対象でなければ null = ペンと同じに塗る）
		//-------------------------------------------------------------------------------
		private PartBrushTarget CurrentPartBrushTarget()
		{
			if (this.partBrushForced != null)
			{
				return this.partBrushForced;
			}
			int id = this.SelectedSingleBlock();
			PartBrushTarget target;
			return id >= 0 && this.partBrushLookup.TryGetValue(id, out target) ? target : null;
		}

		//-------------------------------------------------------------------------------
		// 案内の 1 行を、今選んでいるブロックの塗り方にする処理（ブロックを選び直したときにも呼ぶ）
		//-------------------------------------------------------------------------------
		internal void UpdatePartBrushHint()
		{
			if (this.lblPartBrushHint == null || this.currentPaintTool != MapPaintTool.Parts)
			{
				return;
			}
			PartBrushTarget target = this.CurrentPartBrushTarget();
			string text;
			if (this.partBrushSet == null)
			{
				text = Localizer.T("自動ペン: マップを選ぶと使えます。");
			}
			else if (this.partBrushLookup.Count == 0)
			{
				text = Localizer.T("自動ペン: このタイルセットでは自動で描ける物を見つけられませんでした。ペンと同じように塗ります。");
			}
			else if (target == null)
			{
				text = this.SelectedSingleBlock() < 0
					? Localizer.T("自動ペン: ブロックをまとめて選んでいる間は、ペンと同じようにそのまま貼ります。")
					: Localizer.T("自動ペン: このブロックは自動の対象ではないので、ペンと同じように塗ります。右の一覧で紅くなっているブロックを選ぶと自動になります。");
			}
			else
			{
				string name = PartBrushDisplayName(target.Part);
				switch (target.Part.Kind)
				{
					case MapAssistKind.Edge:
						text = string.Format(Localizer.T("自動ペン: 「{0}」として描きます。なぞると、まわりの縁（岸・段差）が自動で付きます。"), name);
						break;
					case MapAssistKind.Repeat:
						text = MapAssistTrees.IsStackedTree(target.Part)
							? string.Format(Localizer.T("自動ペン: 「{0}」を 1 本ずつ置きます。なぞると森になり、木の重なりと森の端は自動で付きます。"), name)
							: string.Format(Localizer.T("自動ペン: 「{0}」として並べます。なぞると、上の端・下の端が自動で付きます。"), name);
						break;
					case MapAssistKind.Stamp:
						text = string.Format(Localizer.T("自動ペン: 「{0}」をまるごと 1 つ置きます（クリック）。"), name);
						break;
					default:
						text = string.Format(Localizer.T("自動ペン: 「{0}」として塗ります（移動エリアも付きます）。"), name);
						break;
				}
			}
			if (this.lblPartBrushHint.Text != text)
			{
				this.lblPartBrushHint.Text = text;
			}
			this.pnlMapCanvas.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 画面に出すパーツの名前を返す処理（候補に付く「自動: 」は外す）
		//-------------------------------------------------------------------------------
		private static string PartBrushDisplayName(MapAssistPart part)
		{
			string name = part.Name ?? string.Empty;
			foreach (string prefix in new[] { MapAssistAnalyzer.AutoPrefix, Localizer.T(MapAssistAnalyzer.AutoPrefix) })
			{
				if (name.StartsWith(prefix, StringComparison.Ordinal) && name.Length > prefix.Length)
				{
					return name.Substring(prefix.Length);
				}
			}
			return name;
		}

		//-------------------------------------------------------------------------------
		// 番号でパーツを選ぶ処理（検証用。ブロックの選択に関係なく、そのパーツで塗る。-1 で元に戻す）
		//-------------------------------------------------------------------------------
		internal void SelectPartBrush(int index)
		{
			this.partBrushForced = this.partBrushSet != null && index >= 0 && index < this.partBrushSet.Parts.Count ? new PartBrushTarget { Part = this.partBrushSet.Parts[index] } : null;
			this.UpdatePartBrushHint();
		}

		//-------------------------------------------------------------------------------
		// ブロックごとに付ける移動エリアの表を返す処理（ROM の全マップの集計は、同じ材料の間は 1 回だけ）
		//-------------------------------------------------------------------------------
		private int[] PartBrushCollisionTable()
		{
			if (this.partBrushCollisionTable == null && this.partBrushContext != null && this.partBrushSet != null)
			{
				MapAssistAutoInput input = this.partBrushContext.AutoInputProvider != null ? this.partBrushContext.AutoInputProvider() : new MapAssistAutoInput();
				this.partBrushSamples = input;
				this.partBrushCollisionTable = MapAssistSupport.BuildCollisionTable(this.partBrushContext, this.partBrushSet.Parts, MapAssistAnalyzer.CollisionTable(this.partBrushContext, input));
			}
			return this.partBrushCollisionTable;
		}

		//-------------------------------------------------------------------------------
		// キャンバスの上のカーソルの位置から、マスの中のどちら側にあるかを覚える処理（マウスの処理から、塗る前・枠を出す前に呼ぶ）
		//-------------------------------------------------------------------------------
		private void SetPartBrushPointer(Point location)
		{
			int zoom = this.GetMapZoomScale();
			int scrollX = this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0;
			int scrollY = this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0;
			int px = (location.X + scrollX) / zoom;
			int py = (location.Y + scrollY) / zoom;
			this.partBrushHalf = new Point(px % 16 >= 8 ? 1 : 0, py % 16 >= 8 ? 1 : 0);
		}

		//-------------------------------------------------------------------------------
		// 筆の範囲（マス）を返す処理。いつもカーソルが中心に来るようにする
		// 奇数の大きさはカーソルのマスが中心。偶数の大きさは、カーソルがマスの左半分にあれば左へ、右半分にあれば右へ寄せる（上下も同じ）
		//-------------------------------------------------------------------------------
		private Rectangle PartBrushRect(int x, int y)
		{
			int size = this.partBrushSize;
			if (size % 2 == 1)
			{
				return new Rectangle(x - size / 2, y - size / 2, size, size);
			}
			return new Rectangle(x - size / 2 + this.partBrushHalf.X, y - size / 2 + this.partBrushHalf.Y, size, size);
		}

		//-------------------------------------------------------------------------------
		// その塗り方で、カーソルのマスから塗る範囲（マス）を返す処理
		//   部品     … 選んだブロックがカーソルの所に来る位置に、部品の形
		//   くり返し … 筆の範囲を、模様の単位（本体の大きさ）の区切りまで広げる（木が半分にならないように）
		//   ほか     … 筆の範囲
		//-------------------------------------------------------------------------------
		private Rectangle PartBrushRange(PartBrushTarget target, int x, int y)
		{
			MapAssistGrid body = target != null ? target.Part.GetSlot(MapAssistPart.SlotBody) : null;
			if (target != null && target.Part.Kind == MapAssistKind.Stamp)
			{
				return body != null ? new Rectangle(x - target.Anchor.X, y - target.Anchor.Y, body.Width, body.Height) : new Rectangle(x, y, 1, 1);
			}
			Rectangle range = this.PartBrushRect(x, y);
			if (target != null && target.Part.Kind == MapAssistKind.Repeat && body != null && !MapAssistTrees.IsStackedTree(target.Part))
			{
				int left = (int)Math.Floor((double)range.Left / body.Width) * body.Width;
				int top = (int)Math.Floor((double)range.Top / body.Height) * body.Height;
				int right = (int)Math.Ceiling((double)range.Right / body.Width) * body.Width;
				int bottom = (int)Math.Ceiling((double)range.Bottom / body.Height) * body.Height;
				range = Rectangle.FromLTRB(left, top, right, bottom);
			}
			return range;
		}

		//-------------------------------------------------------------------------------
		// カーソルの所に出す枠の範囲を返す処理（ドラッグ中はそのドラッグの塗り方、ほかは今のブロックの塗り方）
		//-------------------------------------------------------------------------------
		private Rectangle PartBrushPreviewRect(int x, int y)
		{
			PartBrushTarget target = this.isPaintingParts && this.partBrushStroke != null ? this.partBrushStroke : this.CurrentPartBrushTarget();
			Rectangle range = this.PartBrushRange(target, x, y);
			if (target != null && MapAssistTrees.IsStackedTree(target.Part) && this.mapMatrix != null && this.partBrushContext != null)
			{
				// 重なって並ぶ木は、置く木（先端の段・幅 2 マス）が筆の範囲より大きい。木の絵が出る範囲を枠にする
				// （枠の範囲だけを描き直すので、絵が枠からはみ出すと、動いた後に薄い絵が残る）
				foreach (KeyValuePair<Point, int> cell in MapAssistTrees.Preview(this.partBrushContext, target.Part, this.mapMatrix.GetLength(0), this.mapMatrix.GetLength(1), (tx, ty) => this.mapMatrix[tx, ty].BlockIndex, range))
				{
					range = Rectangle.Union(range, new Rectangle(cell.Key.X, cell.Key.Y, 1, 1));
				}
			}
			return range;
		}

		//-------------------------------------------------------------------------------
		// ドラッグを始める処理（1 回の「戻る」の始まり）。今のブロックが自動の対象でなければ何もしない
		//-------------------------------------------------------------------------------
		private void BeginPartBrushStroke(int x, int y)
		{
			this.partBrushStroke = this.CurrentPartBrushTarget();
			if (this.partBrushStroke == null)
			{
				return;
			}
			this.isPaintingParts = true;
			this.partBrushStrokeChanged = false;
			this.partBrushLastRange = Rectangle.Empty;
			this.partBrushLastCell = new Point(x, y);
			this.BeginMapEditStroke();
			this.PaintPartsAt(x, y, true);
		}

		//-------------------------------------------------------------------------------
		// ドラッグ中に、直前のマスから今のマスまでを線でつないで塗る処理（マウスを速く動かすと、通知がマスを飛ばすため）
		//-------------------------------------------------------------------------------
		private void PaintPartsLineTo(Point cell)
		{
			Point from = this.partBrushLastCell.X >= 0 ? this.partBrushLastCell : cell;
			int steps = Math.Max(Math.Abs(cell.X - from.X), Math.Abs(cell.Y - from.Y));
			for (int i = steps > 0 ? 1 : 0; i <= steps; i++)
			{
				int x = steps == 0 ? cell.X : from.X + (int)Math.Round((cell.X - from.X) * (double)i / steps);
				int y = steps == 0 ? cell.Y : from.Y + (int)Math.Round((cell.Y - from.Y) * (double)i / steps);
				if (this.IsInsideMap(x, y))
				{
					this.PaintPartsAt(x, y, false);
				}
			}
			this.partBrushLastCell = cell;
		}

		//-------------------------------------------------------------------------------
		// ドラッグを終える処理（履歴を確定する）
		//-------------------------------------------------------------------------------
		private void EndPartBrushStroke()
		{
			if (!this.isPaintingParts)
			{
				return;
			}
			this.isPaintingParts = false;
			this.partBrushStroke = null;
			this.FinishToolEdit(this.partBrushStrokeChanged, true);
			this.pnlMapCanvas.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// マスにパーツを塗る処理（firstPoint はドラッグの最初の点か。部品は最初の点だけに置く）
		// 今のマップの写しをパーツで書き換え、変わったマスだけを履歴に入れて表示に反映する
		//-------------------------------------------------------------------------------
		internal void PaintPartsAt(int x, int y, bool firstPoint)
		{
			PartBrushTarget target = this.partBrushStroke ?? this.CurrentPartBrushTarget();
			MapAssistPart part = target != null ? target.Part : null;
			if (part == null || this.mapMatrix == null || this.partBrushContext == null || this.partBrushSet == null || this.primaryMapLayerBitmap == null || this.blockPaletteBitmap == null)
			{
				return;
			}
			// 前と同じ範囲なら塗り直さない
			Rectangle whole = this.PartBrushRange(target, x, y);
			if (!firstPoint && this.partBrushLastRange == whole)
			{
				return;
			}
			this.partBrushLastRange = whole;
			if (part.Kind == MapAssistKind.Stamp && !firstPoint)
			{
				return;
			}
			int width = this.mapMatrix.GetLength(0);
			int height = this.mapMatrix.GetLength(1);
			MapAssistWork work = new MapAssistWork { Width = width, Height = height, Blocks = new int[width * height], Collisions = new int[width * height] };
			for (int yy = 0; yy < height; yy++)
			{
				for (int xx = 0; xx < width; xx++)
				{
					work.Blocks[yy * width + xx] = this.mapMatrix[xx, yy].BlockIndex;
					work.Collisions[yy * width + xx] = this.mapMatrix[xx, yy].Collision;
				}
			}
			int[] table = this.PartBrushCollisionTable();
			if (table == null)
			{
				return;
			}
			bool tidy = true;
			MapAssistSupportReport report = new MapAssistSupportReport();
			Rectangle range = part.Kind == MapAssistKind.Stamp ? new Rectangle(whole.X, whole.Y, 1, 1) : Rectangle.Intersect(whole, new Rectangle(0, 0, width, height));
			if (range.Width <= 0 || range.Height <= 0)
			{
				return;
			}
			switch (part.Kind)
			{
				case MapAssistKind.Repeat:
					if (MapAssistTrees.IsStackedTree(part))
					{
						// 重なって並ぶ木: なぞった所に木を 1 本ずつ置く
						MapAssistTrees.Paint(this.partBrushContext, work, part, range, table, report);
					}
					else
					{
						this.PaintRepeatBrush(work, part, range, table, tidy, report);
					}
					break;
				case MapAssistKind.Edge when !tidy:
				{
					MapAssistGrid center = part.GetSlot("C");
					if (center == null)
					{
						return;
					}
					MapAssistSupport.FillRect(this.partBrushContext, work, range, (cx, cy) => center.Cells[0], table, report);
					break;
				}
				default:
					if (MapAssistSupport.Paint(this.partBrushContext, work, this.partBrushSet.Parts, part, range, target.Member, table, this.partBrushSamples, report) != null)
					{
						return;
					}
					break;
			}
			this.ApplyPartBrushWork(work);
		}

		//-------------------------------------------------------------------------------
		// くり返し（森）をブラシで塗る処理
		// 範囲を本体で埋め（模様の位置はマップの座標でそろえる）、tidy なら、まわりも含めてこのパーツのマスの上端・下端を付け直す
		//-------------------------------------------------------------------------------
		private void PaintRepeatBrush(MapAssistWork work, MapAssistPart part, Rectangle range, int[] table, bool tidy, MapAssistSupportReport report)
		{
			MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
			if (body == null)
			{
				return;
			}
			MapAssistGrid top = MapAssistSupport.SameWidth(part.GetSlot(MapAssistPart.SlotTop), body);
			MapAssistGrid bottom = MapAssistSupport.SameWidth(part.GetSlot(MapAssistPart.SlotBottom), body);
			MapAssistContext context = this.partBrushContext;
			MapAssistSupport.FillRect(context, work, range, (x, y) => body[x % body.Width, y % body.Height], table, report);
			if (!tidy)
			{
				return;
			}
			int topRows = top != null ? top.Height : 0;
			int bottomRows = bottom != null ? bottom.Height : 0;
			if (topRows + bottomRows == 0)
			{
				return;
			}
			HashSet<int> members = new HashSet<int>(part.AllCells().Select(c => context.ToGlobal(c)).Where(id => id >= 0));
			Rectangle around = Rectangle.Intersect(Rectangle.Inflate(range, 1, topRows + bottomRows + 1), new Rectangle(0, 0, work.Width, work.Height));
			Func<int, int, bool> inside = (x, y) => x >= 0 && y >= 0 && x < work.Width && y < work.Height && members.Contains(work.Blocks[y * work.Width + x]);
			Dictionary<int, MapAssistCell> changes = new Dictionary<int, MapAssistCell>();
			for (int y = around.Top; y < around.Bottom; y++)
			{
				for (int x = around.Left; x < around.Right; x++)
				{
					if (!inside(x, y))
					{
						continue;
					}
					int above = 0;
					while (above < topRows && inside(x, y - above - 1))
					{
						above++;
					}
					int below = 0;
					while (below < bottomRows && inside(x, y + below + 1))
					{
						below++;
					}
					// マップの外は続いているとみなす
					if (y - above - 1 < 0)
					{
						above = topRows;
					}
					if (y + below + 1 >= work.Height)
					{
						below = bottomRows;
					}
					changes[y * work.Width + x] = MapAssistSupport.RepeatCell(body, top, bottom, x, y, 0, 0, above, below);
				}
			}
			foreach (KeyValuePair<int, MapAssistCell> change in changes)
			{
				int id = context.ToGlobal(change.Value);
				if (id < 0)
				{
					continue;
				}
				work.Blocks[change.Key] = id;
				int collision = change.Value.Collision >= 0 ? change.Value.Collision : table[id];
				if (collision >= 0)
				{
					work.Collisions[change.Key] = collision;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 写しと今のマップの違うマスを、履歴に入れてマップと表示に反映する処理
		//-------------------------------------------------------------------------------
		private void ApplyPartBrushWork(MapAssistWork work)
		{
			bool changed = false;
			using (Graphics layer = Graphics.FromImage(this.primaryMapLayerBitmap))
			{
				layer.CompositingMode = CompositingMode.SourceCopy;
				for (int y = 0; y < work.Height; y++)
				{
					for (int x = 0; x < work.Width; x++)
					{
						int i = y * work.Width + x;
						int block = work.Blocks[i];
						int collision = work.Collisions[i];
						MapCell cell = this.mapMatrix[x, y];
						if (block != cell.BlockIndex && block >= 0 && block < this.totalBlocks)
						{
							this.RecordMapEditAction(x, y, cell.BlockIndex, block, cell.Collision, cell.Collision, true);
							this.mapMatrix[x, y].BlockIndex = block;
							layer.DrawImage(this.blockPaletteBitmap, new Rectangle(x * 16, y * 16, 16, 16), new Rectangle(block % PaletteColumns * 16, block / PaletteColumns * 16, 16, 16), GraphicsUnit.Pixel);
							changed = true;
						}
						if (collision != cell.Collision && collision >= 0)
						{
							this.RecordMapEditAction(x, y, this.mapMatrix[x, y].BlockIndex, this.mapMatrix[x, y].BlockIndex, cell.Collision, collision, false);
							this.mapMatrix[x, y].Collision = collision;
							changed = true;
						}
					}
				}
			}
			if (changed)
			{
				this.partBrushStrokeChanged = true;
				this.UpdateMapRender();
				this.pnlMapCanvas.Invalidate();
				this.SetUnsavedChanges(true);
			}
		}

		//-------------------------------------------------------------------------------
		// マスの範囲を、キャンバスの上の位置（ピクセル）にする処理
		//-------------------------------------------------------------------------------
		private Rectangle MapCellsToCanvas(Rectangle area)
		{
			int zoom = this.GetMapZoomScale();
			int cell = 16 * zoom;
			int scrollX = this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0;
			int scrollY = this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0;
			return new Rectangle((area.X + this.primaryMapOffsetX) * cell - scrollX, (area.Y + this.primaryMapOffsetY) * cell - scrollY, area.Width * cell, area.Height * cell);
		}

		//-------------------------------------------------------------------------------
		// カーソルが動いて枠の位置が変わったときに、前と今の枠の所だけを描き直す処理（キャンバス全体を描き直すと追いつかないため）
		//-------------------------------------------------------------------------------
		private void MovePartBrushHover(Point cell)
		{
			Rectangle next = this.mapMatrix != null && this.IsInsideMap(cell.X, cell.Y) ? this.PartBrushPreviewRect(cell.X, cell.Y) : Rectangle.Empty;
			if (cell == this.partBrushHover && next == this.partBrushHoverRect)
			{
				return;
			}
			Rectangle old = this.partBrushHoverRect;
			this.partBrushHover = cell;
			this.partBrushHoverRect = next;
			foreach (Rectangle area in new[] { old, next })
			{
				if (!area.IsEmpty)
				{
					this.pnlMapCanvas.Invalidate(Rectangle.Inflate(this.MapCellsToCanvas(area), 4, 4));
				}
			}
		}

		//-------------------------------------------------------------------------------
		// カーソルの所に塗るはずのブロック（マスと、ブロックの番号）を並べる処理。縁や端は付く前の、中身だけ
		//-------------------------------------------------------------------------------
		private IEnumerable<KeyValuePair<Point, int>> PartBrushGhostCells(PartBrushTarget target, int x, int y)
		{
			MapAssistContext context = this.partBrushContext;
			if (target == null || context == null)
			{
				yield break;
			}
			MapAssistPart part = target.Part;
			Rectangle range = this.PartBrushRange(target, x, y);
			if (MapAssistTrees.IsStackedTree(part))
			{
				// 重なって並ぶ木: 置くはずの木（先端・上・下）をそのまま出す
				foreach (KeyValuePair<Point, int> tree in MapAssistTrees.Preview(context, part, this.mapMatrix.GetLength(0), this.mapMatrix.GetLength(1), (tx, ty) => this.mapMatrix[tx, ty].BlockIndex, range))
				{
					if (tree.Value < this.totalBlocks)
					{
						yield return tree;
					}
				}
				yield break;
			}
			MapAssistGrid body = part.GetSlot(MapAssistPart.SlotBody);
			MapAssistGrid blocks = part.GetSlot(MapAssistPart.SlotBlocks);
			MapAssistGrid center = part.GetSlot("C");
			for (int yy = range.Top; yy < range.Bottom; yy++)
			{
				for (int xx = range.Left; xx < range.Right; xx++)
				{
					if (!this.IsInsideMap(xx, yy))
					{
						continue;
					}
					MapAssistCell cell;
					switch (part.Kind)
					{
						case MapAssistKind.Stamp:
							if (body == null)
							{
								yield break;
							}
							cell = body[xx - range.Left, yy - range.Top];
							break;
						case MapAssistKind.Repeat:
							if (body == null)
							{
								yield break;
							}
							cell = body[xx % body.Width, yy % body.Height];
							break;
						case MapAssistKind.Edge:
							if (center == null)
							{
								yield break;
							}
							cell = center.Cells[0];
							break;
						default:
							if (blocks == null)
							{
								yield break;
							}
							cell = target.Member >= 0 && target.Member < blocks.Cells.Length ? blocks.Cells[target.Member] : blocks.Cells.FirstOrDefault(c => context.ToGlobal(c) >= 0);
							break;
					}
					int id = context.ToGlobal(cell);
					if (id >= 0 && id < this.totalBlocks)
					{
						yield return new KeyValuePair<Point, int>(new Point(xx, yy), id);
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// カーソルの所に、置く物の透けた絵と筆の枠を描く処理（自動ペンで、今のブロックが自動の対象のとき）
		// ドラッグ中は枠だけにする（塗った結果が見えるように）
		//-------------------------------------------------------------------------------
		private void DrawPartBrushPreview(Graphics g)
		{
			PartBrushTarget target = this.isPaintingParts && this.partBrushStroke != null ? this.partBrushStroke : this.CurrentPartBrushTarget();
			if (this.mapMatrix == null || !this.IsInsideMap(this.partBrushHover.X, this.partBrushHover.Y) || target == null)
			{
				return;
			}
			if (!this.isPaintingParts && this.blockPaletteBitmap != null)
			{
				using (System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes())
				{
					attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.7f });
					InterpolationMode oldMode = g.InterpolationMode;
					PixelOffsetMode oldOffset = g.PixelOffsetMode;
					g.InterpolationMode = InterpolationMode.NearestNeighbor;
					g.PixelOffsetMode = PixelOffsetMode.Half;
					foreach (KeyValuePair<Point, int> cell in this.PartBrushGhostCells(target, this.partBrushHover.X, this.partBrushHover.Y))
					{
						Rectangle at = this.MapCellsToCanvas(new Rectangle(cell.Key.X, cell.Key.Y, 1, 1));
						g.DrawImage(this.blockPaletteBitmap, at, cell.Value % PaletteColumns * 16, cell.Value / PaletteColumns * 16, 16, 16, GraphicsUnit.Pixel, attributes);
					}
					g.InterpolationMode = oldMode;
					g.PixelOffsetMode = oldOffset;
				}
			}
			Rectangle r = this.MapCellsToCanvas(this.PartBrushPreviewRect(this.partBrushHover.X, this.partBrushHover.Y));
			using (Pen pen = new Pen(Color.White, 2f) { DashStyle = DashStyle.Dash })
			using (Pen shadow = new Pen(Color.Black, 1f))
			{
				g.DrawRectangle(shadow, Rectangle.Inflate(r, 1, 1));
				g.DrawRectangle(pen, r);
			}
		}
	}
}
