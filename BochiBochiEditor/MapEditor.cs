using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x0200001B RID: 27
	public partial class MapEditor : Form, IMessageFilter
	{
		// Token: 0x06000463 RID: 1123 RVA: 0x000205C8 File Offset: 0x0001E7C8
		public MapEditor()
		{
			base.Load += this.MapEditor_Load;
			base.Shown += this.MapEditor_Shown;
			base.FormClosing += this.MapEditor_FormClosing;
			Application.AddMessageFilter(this);
			this.hasUnsavedChanges = false;
			this.isUpdatingUI = false;
			this.isSwitchingMode = false;
			this.mapHeaders = new List<MapEditor.MapHeader>();
			this.tileset2BlockLimits = new Dictionary<int, int>();
			this.undoStack = new Stack<List<MapEditor.MapEditAction>>();
			this.redoStack = new Stack<List<MapEditor.MapEditAction>>();
			this.mapZoomScale = 1;
			this.totalBlocks = 0;
			this.primaryMapOffsetX = 0;
			this.primaryMapOffsetY = 0;
			this.cachedConnBank = -1;
			this.cachedConnNumber = -1;
			this.selectedBlockRect = new Rectangle(0, 0, 1, 1);
			this.selectedCollisionIndex = 0;
			this.currentStroke = null;
			this.isDraggingEvent = false;
			this.MAP_NAME_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("MAP_NAME_TABLE_OFFSET");
			this.MAP_NAME_FIRST_INDEX = RomIniReader.ReadHexOrDecimal("MAP_NAME_FIRST_INDEX");
			this.MAP_NAME_COUNT = RomIniReader.ReadHexOrDecimal("MAP_NAME_COUNT");
			this.MAP_BANK_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("MAP_BANK_TABLE_OFFSET");
			this.TILESET_INDEX_START_OFFSET = RomIniReader.ReadHexOrDecimal("TILESET_INDEX_START_OFFSET");
			this.MAP_TERRAIN_ID_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("MAP_TERRAIN_ID_TABLE_OFFSET");
			this.MAP_TERRAIN_ID_COUNT = RomIniReader.ReadHexOrDecimal("MAP_TERRAIN_ID_COUNT");
			this.InitializeComponent();
			this.KeyPreview = true;
			this.KeyDown += this.MapEditor_KeyDown;
			this.ConfigureResizableMapEditorUI();
			AppIconHelper.Apply(this);
		}

		//-------------------------------------------------------------------------------
		// マップエディタをリサイズ可能な表示に初期化する処理
		//-------------------------------------------------------------------------------
		private void ConfigureResizableMapEditorUI()
		{
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.MaximizeBox = true;
			base.MinimumSize = new Size(960, 560);
			this.tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.grpMapSelector.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			this.grpEditMapScript.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			this.grpEditMapConnection.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			this.chkMapZoom2x.Visible = false;
			this.chkMapZoom2x.Enabled = false;
			this.BuildMapToolWindow();
			this.BuildMapEditModeSwitcher();
			this.ConfigureEditorModeTabs();
			this.ConfigureNewEventAutoFreeSpaceUI();
			this.ConfigureMapSelectorSelectionStyle();
			this.ApplyMapEditorButtonIcons();
			this.tabMapEdit.Resize += this.tabMapEdit_Resize;
			base.Resize += this.MapEditor_Resize;
			this.ResizeMapEditLayout();
		}

		//-------------------------------------------------------------------------------
		// マップ編集用のミニコンソールウィンドウを作成する処理
		//-------------------------------------------------------------------------------
		private void BuildMapToolWindow()
		{
			bool flag = this.mapToolWindow != null;
			if (flag)
			{
				return;
			}
			this.mapToolTip = new ToolTip
			{
				AutoPopDelay = 5000,
				InitialDelay = 350,
				ReshowDelay = 100
			};
			this.mapToolHostForm = new Form
			{
				Text = "マップ編集ツール",
				Size = new Size(700, 760),
				MinimumSize = new Size(360, 360),
				StartPosition = FormStartPosition.Manual,
				FormBorderStyle = FormBorderStyle.SizableToolWindow,
				ShowInTaskbar = false,
				MinimizeBox = false,
				MaximizeBox = false,
				ControlBox = false
			};
			this.mapToolHostForm.Resize += this.MapToolHostForm_Resize;
			this.mapToolWindow = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(8),
				BorderStyle = BorderStyle.FixedSingle,
				BackColor = Color.FromArgb(225, 242, 232)
			};
			this.mapToolWindow.MouseDown += this.MapToolWindow_MouseDown;
			this.mapToolWindow.MouseMove += this.MapToolWindow_MouseMove;
			this.mapToolWindow.MouseUp += this.MapToolWindow_MouseUp;
			this.mapToolGrip = new Panel
			{
				Dock = DockStyle.Left,
				Width = 22,
				Cursor = Cursors.SizeAll,
				BackColor = Color.FromArgb(196, 224, 207)
			};
			this.mapToolGrip.MouseDown += this.MapToolWindow_MouseDown;
			this.mapToolGrip.MouseMove += this.MapToolWindow_MouseMove;
			this.mapToolGrip.MouseUp += this.MapToolWindow_MouseUp;
			this.mapToolTip.SetToolTip(this.mapToolGrip, "ドラッグして移動");
			this.mapToolContentPanel = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(8, 0, 0, 0),
				BackColor = Color.FromArgb(225, 242, 232)
			};
			this.mapToolContentPanel.MouseDown += this.MapToolWindow_MouseDown;
			this.mapToolContentPanel.MouseMove += this.MapToolWindow_MouseMove;
			this.mapToolContentPanel.MouseUp += this.MapToolWindow_MouseUp;
			this.MoveControlToMapToolWindow(this.btnLoadMapEmerge, new Point(8, 8), new Size(80, 26));
			this.MoveControlToMapToolWindow(this.btnLoadMapDive, new Point(96, 8), new Size(80, 26));
			this.MoveControlToMapToolWindow(this.chkPlayTileAnimation, new Point(188, 11), new Size(132, 22));
			this.MoveControlToMapToolWindow(this.pnlShowEvent, new Point(8, 42), new Size(620, 24));
			this.MoveControlToMapToolWindow(this.btnUndo, new Point(8, 76), new Size(42, 38));
			this.MoveControlToMapToolWindow(this.btnRedo, new Point(56, 76), new Size(42, 38));
			this.MoveControlToMapToolWindow(this.btnOpenScriptEditor, new Point(112, 74), new Size(90, 42));
			this.MoveControlToMapToolWindow(this.btnOpenBlockEditor, new Point(210, 74), new Size(90, 42));
			this.MoveControlToMapToolWindow(this.grpBorderDataPreview, new Point(516, 70), new Size(130, 132));
			this.MoveControlToMapToolWindow(this.pnlBlockIndex, new Point(8, 124), new Size(360, 24));
			this.MoveControlToMapToolWindow(this.tabEditorMode, new Point(8, 208), new Size(640, 442));
			this.tabEditorMode.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.ConfigureEditorModeTabs();
			this.ResizeMapToolWindowFrame();
			this.ResizeMapToolWindowContent();
			this.mapToolWindow.Controls.Add(this.mapToolContentPanel);
			this.mapToolWindow.Controls.Add(this.mapToolGrip);
			this.mapToolHostForm.Controls.Add(this.mapToolWindow);
			this.mapToolWindow.BringToFront();
		}

		//-------------------------------------------------------------------------------
		// マップ編集用のミニコンソールウィンドウを表示する処理
		//-------------------------------------------------------------------------------
		private void ShowMapToolWindow()
		{
			bool flag = this.mapToolHostForm == null || this.mapToolHostForm.IsDisposed;
			if (flag)
			{
				return;
			}
			bool flag2 = !this.mapToolHostPositionInitialized;
			if (flag2)
			{
				Point point = this.tabMapEdit.PointToScreen(new Point(Math.Max(16, this.tabMapEdit.ClientSize.Width - this.mapToolHostForm.Width - 16), 16));
				this.mapToolHostForm.Location = point;
				this.mapToolHostPositionInitialized = true;
			}
			bool flag3 = !this.mapToolHostForm.Visible;
			if (flag3)
			{
				this.mapToolHostForm.Show(this);
			}
			this.mapToolHostForm.BringToFront();
			this.ResizeMapToolWindowContent();
		}

		//-------------------------------------------------------------------------------
		// マップ画面上の編集モード切替ボタンを作成する処理
		//-------------------------------------------------------------------------------
		private void BuildMapEditModeSwitcher()
		{
			bool flag = this.mapEditModeSwitcher != null;
			if (flag)
			{
				return;
			}
			this.mapEditModeSwitcher = new Panel
			{
				Height = 26
			};
			this.btnMapEditModeBlock = this.CreateMapEditModeButton("ブロック", this.tabBlock);
			this.btnMapEditModeCollision = this.CreateMapEditModeButton("移動エリア", this.tabCollision);
			this.btnMapEditModeEvent = this.CreateMapEditModeButton("イベント", this.tabEvent);
			this.mapEditModeSwitcher.Controls.Add(this.btnMapEditModeBlock);
			this.mapEditModeSwitcher.Controls.Add(this.btnMapEditModeCollision);
			this.mapEditModeSwitcher.Controls.Add(this.btnMapEditModeEvent);
			this.tabMapEdit.Controls.Add(this.mapEditModeSwitcher);
			this.mapEditModeSwitcher.BringToFront();
			this.ResizeMapEditModeSwitcher();
			this.UpdateMapEditModeSwitcher();
		}

		//-------------------------------------------------------------------------------
		// マップ画面上の編集モード切替ボタンを作成する処理
		//-------------------------------------------------------------------------------
		private Button CreateMapEditModeButton(string text, TabPage targetTab)
		{
			Button button = new Button
			{
				Text = text,
				Tag = targetTab,
				Height = 24,
				FlatStyle = FlatStyle.Standard,
				UseVisualStyleBackColor = false
			};
			button.Click += this.MapEditModeButton_Click;
			return button;
		}

		//-------------------------------------------------------------------------------
		// 編集モードタブを3等分幅に設定する処理
		//-------------------------------------------------------------------------------
		private void ConfigureEditorModeTabs()
		{
			bool flag = this.tabEditorMode == null;
			if (flag)
			{
				return;
			}
			this.tabEditorMode.SizeMode = TabSizeMode.Fixed;
			this.tabEditorMode.Multiline = false;
			this.ResizeEditorModeTabHeaders();
		}

		private void ConfigureMapSelectorSelectionStyle()
		{
			bool flag = this.tvwMapSelector == null;
			if (flag)
			{
				return;
			}
			this.tvwMapSelector.HideSelection = false;
			this.tvwMapSelector.FullRowSelect = true;
			this.tvwMapSelector.ShowLines = false;
		}

		//-------------------------------------------------------------------------------
		// 編集モードタブの見出し幅をツールウィンドウ幅に合わせる処理
		//-------------------------------------------------------------------------------
		private void ResizeEditorModeTabHeaders()
		{
			bool flag = this.tabEditorMode == null || this.tabEditorMode.TabPages.Count == 0;
			if (flag)
			{
				return;
			}
			int width = Math.Max(72, checked((this.tabEditorMode.Width - 10) / this.tabEditorMode.TabPages.Count));
			this.tabEditorMode.ItemSize = new Size(width, 22);
		}

		//-------------------------------------------------------------------------------
		// マップ画面上の編集モード切替ボタンを再配置する処理
		//-------------------------------------------------------------------------------
		private void ResizeMapEditModeSwitcher()
		{
			bool flag = this.mapEditModeSwitcher == null || this.pnlMapCanvas == null;
			if (flag)
			{
				return;
			}
			int x = this.pnlMapCanvas.Left + 18;
			int y = Math.Max(8, this.pnlMapCanvas.Top - 32);
			int width = Math.Min(360, Math.Max(240, this.pnlMapCanvas.Width - 36));
			bool flag2 = this.btnLoadMapUp != null && this.btnLoadMapUp.Left > x + 240;
			if (flag2)
			{
				width = Math.Min(width, this.btnLoadMapUp.Left - x - 8);
			}
			this.mapEditModeSwitcher.Location = new Point(x, y);
			this.mapEditModeSwitcher.Size = new Size(width, 26);
			Button[] array = new Button[] { this.btnMapEditModeBlock, this.btnMapEditModeCollision, this.btnMapEditModeEvent };
			int num = this.mapEditModeSwitcher.Width / array.Length;
			int num2 = 0;
			checked
			{
				for (int i = 0; i < array.Length; i++)
				{
					Button button = array[i];
					int num3 = (i == array.Length - 1) ? (this.mapEditModeSwitcher.Width - num2) : num;
					button.Location = new Point(num2, 0);
					button.Size = new Size(num3, 24);
					num2 += num3;
				}
			}
			this.mapEditModeSwitcher.BringToFront();
		}

		//-------------------------------------------------------------------------------
		// マップ画面上の編集モード切替ボタン押下を処理する処理
		//-------------------------------------------------------------------------------
		private void MapEditModeButton_Click(object sender, EventArgs e)
		{
			Button button = sender as Button;
			TabPage tabPage = ((button != null) ? button.Tag : null) as TabPage;
			this.SetEditorMode(tabPage);
		}

		//-------------------------------------------------------------------------------
		// 編集モードを共通的に切り替える処理
		//-------------------------------------------------------------------------------
		private void SetEditorMode(TabPage tabPage)
		{
			bool flag = tabPage == null || this.tabEditorMode == null;
			if (flag)
			{
				return;
			}
			bool flag2 = this.tabEditorMode.SelectedTab != tabPage;
			if (flag2)
			{
				this.tabEditorMode.SelectedTab = tabPage;
			}
			this.UpdateMapEditModeSwitcher();
			this.pnlMapCanvas?.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// マップ画面上の編集モード切替ボタンの選択表示を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateMapEditModeSwitcher()
		{
			bool flag = this.mapEditModeSwitcher == null || this.tabEditorMode == null;
			if (flag)
			{
				return;
			}
			Button[] array = new Button[] { this.btnMapEditModeBlock, this.btnMapEditModeCollision, this.btnMapEditModeEvent };
			foreach (Button button in array)
			{
				bool flag2 = button != null && button.Tag == this.tabEditorMode.SelectedTab;
				if (button != null)
				{
					button.BackColor = flag2 ? Color.FromArgb(209, 233, 219) : SystemColors.Control;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 既存の操作部品をミニコンソールウィンドウへ移動する処理
		//-------------------------------------------------------------------------------
		private void MoveControlToMapToolWindow(Control control, Point location, Size size)
		{
			control.Parent?.Controls.Remove(control);
			control.Location = location;
			control.Size = size;
			control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			this.mapToolContentPanel.Controls.Add(control);
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウの表示高さを調整する処理
		//-------------------------------------------------------------------------------
		private void ResizeMapToolWindowFrame()
		{
			bool flag = this.mapToolWindow == null || this.mapToolHostForm == null;
			if (flag)
			{
				return;
			}
			this.mapToolWindow.Size = this.mapToolHostForm.ClientSize;
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウ内の部品サイズを調整する処理
		//-------------------------------------------------------------------------------
		private void ResizeMapToolWindowContent()
		{
			bool flag = this.mapToolContentPanel == null;
			if (flag)
			{
				return;
			}
			int contentWidth = Math.Max(320, this.mapToolContentPanel.ClientSize.Width);
			int contentHeight = Math.Max(260, this.mapToolContentPanel.ClientSize.Height);
			this.btnLoadMapEmerge.Location = new Point(8, 8);
			this.btnLoadMapDive.Location = new Point(96, 8);
			this.chkPlayTileAnimation.Location = new Point(188, 11);
			this.pnlShowEvent.Location = new Point(8, 42);
			this.pnlShowEvent.Size = new Size(Math.Max(260, contentWidth - 24), 24);
			bool flag2 = contentWidth >= 520;
			this.grpBorderDataPreview.Visible = flag2;
			this.grpBorderDataPreview.Location = new Point(Math.Max(8, contentWidth - 140), 70);
			this.grpBorderDataPreview.Size = new Size(130, 132);
			this.pnlBlockIndex.Location = new Point(8, 124);
			this.pnlBlockIndex.Size = new Size(Math.Max(220, Math.Min(360, contentWidth - 16)), 24);
			this.tabEditorMode.Location = new Point(8, 208);
			this.tabEditorMode.Size = new Size(Math.Max(260, contentWidth - 16), Math.Max(120, contentHeight - 230));
			this.ResizeEditorModeTabHeaders();
			this.tabBlock.Size = new Size(this.tabEditorMode.Width - 8, this.tabEditorMode.Height - 26);
			this.tabCollision.Size = this.tabBlock.Size;
			this.tabEvent.Size = this.tabBlock.Size;
			this.hsbTilesetScroll.Location = new Point(6, this.tabBlock.Height - 26);
			int paletteWidth = (this.blockPaletteBitmap == null) ? 128 : Math.Min(this.blockPaletteBitmap.Width, Math.Max(128, this.tabBlock.Width - 30));
			this.hsbTilesetScroll.Width = paletteWidth;
			this.pnlTilesetPalette.Size = new Size(paletteWidth, Math.Max(128, this.tabBlock.Height - 36));
			this.vsbTilesetScroll.Location = new Point(this.pnlTilesetPalette.Right + 2, 8);
			this.vsbTilesetScroll.Height = this.pnlTilesetPalette.Height;
			this.pnlCollisionPalette.Size = new Size(128, 128);
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウのリサイズ時に内容を再配置する処理
		//-------------------------------------------------------------------------------
		private void MapToolHostForm_Resize(object sender, EventArgs e)
		{
			this.ResizeMapToolWindowFrame();
			this.ResizeMapToolWindowContent();
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウのドラッグ開始を処理する処理
		//-------------------------------------------------------------------------------
		private void MapToolWindow_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = e.Button != MouseButtons.Left || this.mapToolHostForm == null || this.mapToolWindow == null;
			if (flag)
			{
				return;
			}
			this.mapToolDragging = true;
			Control control = sender as Control ?? this.mapToolWindow;
			Point point = control.PointToScreen(e.Location);
			this.mapToolDragOffset = new Point(point.X - this.mapToolHostForm.Left, point.Y - this.mapToolHostForm.Top);
			control.Capture = true;
			this.mapToolHostForm.BringToFront();
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウのドラッグ移動を処理する処理
		//-------------------------------------------------------------------------------
		private void MapToolWindow_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = !this.mapToolDragging || this.mapToolHostForm == null || this.mapToolWindow == null;
			if (flag)
			{
				return;
			}
			Control control = sender as Control ?? this.mapToolWindow;
			Point point = control.PointToScreen(e.Location);
			this.mapToolHostForm.Location = new Point(point.X - this.mapToolDragOffset.X, point.Y - this.mapToolDragOffset.Y);
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウのドラッグ終了を処理する処理
		//-------------------------------------------------------------------------------
		private void MapToolWindow_MouseUp(object sender, MouseEventArgs e)
		{
			this.mapToolDragging = false;
			bool flag = this.mapToolWindow != null;
			if (flag)
			{
				this.mapToolWindow.Capture = false;
				this.mapToolGrip.Capture = false;
				this.mapToolContentPanel.Capture = false;
			}
		}

		//-------------------------------------------------------------------------------
		// ミニコンソールウィンドウをマップタブ内へ収める処理
		//-------------------------------------------------------------------------------
		private void ClampMapToolWindow()
		{
		}

		//-------------------------------------------------------------------------------
		// マップ編集ボタンへアイコンを設定する処理
		//-------------------------------------------------------------------------------
		private void ApplyMapEditorButtonIcons()
		{
			this.ApplyButtonIcon(this.btnUndo, "undo_icon.png", "戻る");
			this.ApplyButtonIcon(this.btnRedo, "redo_icon.png", "進む");
		}

		//-------------------------------------------------------------------------------
		// 指定ボタンへボタン用アイコンを設定する処理
		//-------------------------------------------------------------------------------
		private void ApplyButtonIcon(Button button, string iconFileName, string fallbackText)
		{
			Image image = this.LoadButtonIcon(iconFileName);
			bool flag = image == null;
			if (flag)
			{
				button.Text = fallbackText;
				return;
			}
			button.Image = image;
			button.Text = string.Empty;
			button.ImageAlign = ContentAlignment.MiddleCenter;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 1;
			this.mapToolTip?.SetToolTip(button, fallbackText);
		}

		//-------------------------------------------------------------------------------
		// ボタン用アイコンを読み込む処理
		//-------------------------------------------------------------------------------
		private Image LoadButtonIcon(string iconFileName)
		{
			try
			{
				string text = AppAssetLocator.FindRequiredFile(Path.Combine("ボタン用アイコン", iconFileName));
				using (Image image = Image.FromFile(text))
				{
					return new Bitmap(image, new Size(22, 22));
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// マップエディタのリサイズ時にマップ編集画面を再配置する処理
		//-------------------------------------------------------------------------------
		private void MapEditor_Resize(object sender, EventArgs e)
		{
			this.ResizeMapEditLayout();
		}

		//-------------------------------------------------------------------------------
		// マップタブのリサイズ時にマップ編集画面を再配置する処理
		//-------------------------------------------------------------------------------
		private void tabMapEdit_Resize(object sender, EventArgs e)
		{
			this.ResizeMapEditLayout();
		}

		//-------------------------------------------------------------------------------
		// マップ編集画面内のキャンバスと周辺UIを表示領域に合わせて再配置する処理
		//-------------------------------------------------------------------------------
		private void ResizeMapEditLayout()
		{
			bool flag = this.tabMapEdit == null || this.pnlMapCanvas == null;
			if (flag)
			{
				return;
			}
			int num = this.tabMapEdit.ClientSize.Width;
			int num2 = this.tabMapEdit.ClientSize.Height;
			int num4 = Math.Max(220, num2 - 104);
			int num5 = Math.Max(280, num - 88);
			this.vsbMapDataPreview.Location = new Point(34 + num5 + 2, 43);
			this.vsbMapDataPreview.Height = num4;
			this.pnlMapCanvas.Location = new Point(34, 43);
			this.pnlMapCanvas.Size = new Size(num5, num4);
			this.hsbMapDataPreview.Location = new Point(34, 43 + num4 + 2);
			this.hsbMapDataPreview.Width = num5;
			this.btnLoadMapLeft.Location = new Point(5, Math.Max(43, 43 + num4 / 2 - this.btnLoadMapLeft.Height / 2));
			this.btnLoadMapRight.Location = new Point(Math.Min(num - 31, this.vsbMapDataPreview.Right + 5), Math.Max(43, 43 + num4 / 2 - this.btnLoadMapRight.Height / 2));
			this.btnLoadMapUp.Location = new Point(34 + num5 / 2 - this.btnLoadMapUp.Width / 2, 11);
			this.btnLoadMapDown.Location = new Point(34 + num5 / 2 - this.btnLoadMapDown.Width / 2, num2 - 35);
			this.btnMapScreenShot.Location = new Point(14, num2 - 35);
			this.chkShowGrid.Location = new Point(this.btnMapScreenShot.Right + 18, num2 - 29);
			this.pnlMapPosition.Location = new Point(Math.Max(288, num - 360), num2 - 36);
			this.ResizeMapEditModeSwitcher();
			this.ResizeMapToolWindowFrame();
			this.ResizeMapToolWindowContent();
			this.ClampMapToolWindow();
			this.UpdateMapScrollBars();
			this.pnlMapCanvas.Invalidate();
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000466 RID: 1126 RVA: 0x0002B93D File Offset: 0x00029B3D
		// (set) Token: 0x06000467 RID: 1127 RVA: 0x0002B948 File Offset: 0x00029B48
		internal virtual TreeView tvwMapSelector
		{
			[CompilerGenerated]
			get
			{
				return this._tvwMapSelector;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				TreeViewEventHandler treeViewEventHandler = new TreeViewEventHandler(this.tvwMapSelector_AfterSelect);
				TreeViewCancelEventHandler treeViewCancelEventHandler = new TreeViewCancelEventHandler(this.tvwMapSelect_BeforeSelect);
				TreeView treeView = this._tvwMapSelector;
				if (treeView != null)
				{
					treeView.AfterSelect -= treeViewEventHandler;
					treeView.BeforeSelect -= treeViewCancelEventHandler;
				}
				this._tvwMapSelector = value;
				treeView = this._tvwMapSelector;
				if (treeView != null)
				{
					treeView.AfterSelect += treeViewEventHandler;
					treeView.BeforeSelect += treeViewCancelEventHandler;
				}
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000468 RID: 1128 RVA: 0x0002B9A6 File Offset: 0x00029BA6
		// (set) Token: 0x06000469 RID: 1129 RVA: 0x0002B9B0 File Offset: 0x00029BB0
		internal virtual Button btnSave
		{
			[CompilerGenerated]
			get
			{
				return this._btnSave;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSave_Click);
				Button button = this._btnSave;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSave = value;
				button = this._btnSave;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x0600046A RID: 1130 RVA: 0x0002B9F3 File Offset: 0x00029BF3
		// (set) Token: 0x0600046B RID: 1131 RVA: 0x0002B9FD File Offset: 0x00029BFD
		internal virtual GroupBox grpMapSelector
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x0600046C RID: 1132 RVA: 0x0002BA06 File Offset: 0x00029C06
		// (set) Token: 0x0600046D RID: 1133 RVA: 0x0002BA10 File Offset: 0x00029C10
		internal virtual RadioButton rbMapSortName
		{
			[CompilerGenerated]
			get
			{
				return this._rbMapSortName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbMapSort_CheckedChanged);
				RadioButton radioButton = this._rbMapSortName;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbMapSortName = value;
				radioButton = this._rbMapSortName;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x0600046E RID: 1134 RVA: 0x0002BA53 File Offset: 0x00029C53
		// (set) Token: 0x0600046F RID: 1135 RVA: 0x0002BA60 File Offset: 0x00029C60
		internal virtual RadioButton rbMapSortIndex
		{
			[CompilerGenerated]
			get
			{
				return this._rbMapSortIndex;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbMapSort_CheckedChanged);
				RadioButton radioButton = this._rbMapSortIndex;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbMapSortIndex = value;
				radioButton = this._rbMapSortIndex;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000470 RID: 1136 RVA: 0x0002BAA3 File Offset: 0x00029CA3
		// (set) Token: 0x06000471 RID: 1137 RVA: 0x0002BAAD File Offset: 0x00029CAD
		internal virtual GroupBox grpMapHeader
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000472 RID: 1138 RVA: 0x0002BAB6 File Offset: 0x00029CB6
		// (set) Token: 0x06000473 RID: 1139 RVA: 0x0002BAC0 File Offset: 0x00029CC0
		internal virtual GroupBox grpMapFooter
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000474 RID: 1140 RVA: 0x0002BAC9 File Offset: 0x00029CC9
		// (set) Token: 0x06000475 RID: 1141 RVA: 0x0002BAD3 File Offset: 0x00029CD3
		internal virtual NumericUpDown nudMusicCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000476 RID: 1142 RVA: 0x0002BADC File Offset: 0x00029CDC
		// (set) Token: 0x06000477 RID: 1143 RVA: 0x0002BAE6 File Offset: 0x00029CE6
		internal virtual ComboBox cmbWeather
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000478 RID: 1144 RVA: 0x0002BAEF File Offset: 0x00029CEF
		// (set) Token: 0x06000479 RID: 1145 RVA: 0x0002BAF9 File Offset: 0x00029CF9
		internal virtual Label lblSight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x0600047A RID: 1146 RVA: 0x0002BB02 File Offset: 0x00029D02
		// (set) Token: 0x0600047B RID: 1147 RVA: 0x0002BB0C File Offset: 0x00029D0C
		internal virtual Label lblMapNameId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x0600047C RID: 1148 RVA: 0x0002BB15 File Offset: 0x00029D15
		// (set) Token: 0x0600047D RID: 1149 RVA: 0x0002BB1F File Offset: 0x00029D1F
		internal virtual Label lblTerrainId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x0600047E RID: 1150 RVA: 0x0002BB28 File Offset: 0x00029D28
		// (set) Token: 0x0600047F RID: 1151 RVA: 0x0002BB32 File Offset: 0x00029D32
		internal virtual Label lblMusicCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000480 RID: 1152 RVA: 0x0002BB3B File Offset: 0x00029D3B
		// (set) Token: 0x06000481 RID: 1153 RVA: 0x0002BB45 File Offset: 0x00029D45
		internal virtual GroupBox grpMapHeaderAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000482 RID: 1154 RVA: 0x0002BB4E File Offset: 0x00029D4E
		// (set) Token: 0x06000483 RID: 1155 RVA: 0x0002BB58 File Offset: 0x00029D58
		internal virtual TextBox txtAddressMapConnection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000484 RID: 1156 RVA: 0x0002BB61 File Offset: 0x00029D61
		// (set) Token: 0x06000485 RID: 1157 RVA: 0x0002BB6B File Offset: 0x00029D6B
		internal virtual TextBox txtAddressMapScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000486 RID: 1158 RVA: 0x0002BB74 File Offset: 0x00029D74
		// (set) Token: 0x06000487 RID: 1159 RVA: 0x0002BB7E File Offset: 0x00029D7E
		internal virtual TextBox txtAddressEventScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x0002BB87 File Offset: 0x00029D87
		// (set) Token: 0x06000489 RID: 1161 RVA: 0x0002BB91 File Offset: 0x00029D91
		internal virtual TextBox txtAddressMapFooter
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x0600048A RID: 1162 RVA: 0x0002BB9A File Offset: 0x00029D9A
		// (set) Token: 0x0600048B RID: 1163 RVA: 0x0002BBA4 File Offset: 0x00029DA4
		internal virtual Label lblAddressMapConnection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x0600048C RID: 1164 RVA: 0x0002BBAD File Offset: 0x00029DAD
		// (set) Token: 0x0600048D RID: 1165 RVA: 0x0002BBB7 File Offset: 0x00029DB7
		internal virtual Label lblAddressMapScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x0002BBC0 File Offset: 0x00029DC0
		// (set) Token: 0x0600048F RID: 1167 RVA: 0x0002BBCA File Offset: 0x00029DCA
		internal virtual Label lblAddressEventScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x0002BBD3 File Offset: 0x00029DD3
		// (set) Token: 0x06000491 RID: 1169 RVA: 0x0002BBDD File Offset: 0x00029DDD
		internal virtual Label lblAddressMapFooter
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x0002BBE6 File Offset: 0x00029DE6
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x0002BBF0 File Offset: 0x00029DF0
		internal virtual Label lblTerrainType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000494 RID: 1172 RVA: 0x0002BBF9 File Offset: 0x00029DF9
		// (set) Token: 0x06000495 RID: 1173 RVA: 0x0002BC03 File Offset: 0x00029E03
		internal virtual NumericUpDown nudTerrainId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000496 RID: 1174 RVA: 0x0002BC0C File Offset: 0x00029E0C
		// (set) Token: 0x06000497 RID: 1175 RVA: 0x0002BC16 File Offset: 0x00029E16
		internal virtual Label lblWeather
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000498 RID: 1176 RVA: 0x0002BC1F File Offset: 0x00029E1F
		// (set) Token: 0x06000499 RID: 1177 RVA: 0x0002BC29 File Offset: 0x00029E29
		internal virtual ComboBox cmbTerrainType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x0600049A RID: 1178 RVA: 0x0002BC32 File Offset: 0x00029E32
		// (set) Token: 0x0600049B RID: 1179 RVA: 0x0002BC3C File Offset: 0x00029E3C
		internal virtual ComboBox cmbSight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x0600049C RID: 1180 RVA: 0x0002BC45 File Offset: 0x00029E45
		// (set) Token: 0x0600049D RID: 1181 RVA: 0x0002BC4F File Offset: 0x00029E4F
		internal virtual Label lblBicycle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x0600049E RID: 1182 RVA: 0x0002BC58 File Offset: 0x00029E58
		// (set) Token: 0x0600049F RID: 1183 RVA: 0x0002BC62 File Offset: 0x00029E62
		internal virtual ComboBox cmbBicycle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x060004A0 RID: 1184 RVA: 0x0002BC6B File Offset: 0x00029E6B
		// (set) Token: 0x060004A1 RID: 1185 RVA: 0x0002BC75 File Offset: 0x00029E75
		internal virtual Label lblMapNameType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x060004A2 RID: 1186 RVA: 0x0002BC7E File Offset: 0x00029E7E
		// (set) Token: 0x060004A3 RID: 1187 RVA: 0x0002BC88 File Offset: 0x00029E88
		internal virtual ComboBox cmbMapNameId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x060004A4 RID: 1188 RVA: 0x0002BC91 File Offset: 0x00029E91
		// (set) Token: 0x060004A5 RID: 1189 RVA: 0x0002BC9B File Offset: 0x00029E9B
		internal virtual ComboBox cmbMapNameType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x0002BCA4 File Offset: 0x00029EA4
		// (set) Token: 0x060004A7 RID: 1191 RVA: 0x0002BCAE File Offset: 0x00029EAE
		internal virtual Label lblBattleType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x060004A8 RID: 1192 RVA: 0x0002BCB7 File Offset: 0x00029EB7
		// (set) Token: 0x060004A9 RID: 1193 RVA: 0x0002BCC1 File Offset: 0x00029EC1
		internal virtual Label lblLevel
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x060004AA RID: 1194 RVA: 0x0002BCCA File Offset: 0x00029ECA
		// (set) Token: 0x060004AB RID: 1195 RVA: 0x0002BCD4 File Offset: 0x00029ED4
		internal virtual NumericUpDown nudLevel
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x060004AC RID: 1196 RVA: 0x0002BCDD File Offset: 0x00029EDD
		// (set) Token: 0x060004AD RID: 1197 RVA: 0x0002BCE7 File Offset: 0x00029EE7
		internal virtual ComboBox cmbBattleType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x0002BCF0 File Offset: 0x00029EF0
		// (set) Token: 0x060004AF RID: 1199 RVA: 0x0002BCFA File Offset: 0x00029EFA
		internal virtual TabControl tabMain
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x060004B0 RID: 1200 RVA: 0x0002BD03 File Offset: 0x00029F03
		// (set) Token: 0x060004B1 RID: 1201 RVA: 0x0002BD0D File Offset: 0x00029F0D
		internal virtual TabPage tabMapInfo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x060004B2 RID: 1202 RVA: 0x0002BD16 File Offset: 0x00029F16
		// (set) Token: 0x060004B3 RID: 1203 RVA: 0x0002BD20 File Offset: 0x00029F20
		internal virtual TabPage tabMapEdit
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x060004B4 RID: 1204 RVA: 0x0002BD29 File Offset: 0x00029F29
		// (set) Token: 0x060004B5 RID: 1205 RVA: 0x0002BD33 File Offset: 0x00029F33
		internal virtual Label lblMapDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x060004B6 RID: 1206 RVA: 0x0002BD3C File Offset: 0x00029F3C
		// (set) Token: 0x060004B7 RID: 1207 RVA: 0x0002BD46 File Offset: 0x00029F46
		internal virtual TextBox txtMapDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x060004B8 RID: 1208 RVA: 0x0002BD4F File Offset: 0x00029F4F
		// (set) Token: 0x060004B9 RID: 1209 RVA: 0x0002BD59 File Offset: 0x00029F59
		internal virtual NumericUpDown nudMapHeight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x060004BA RID: 1210 RVA: 0x0002BD62 File Offset: 0x00029F62
		// (set) Token: 0x060004BB RID: 1211 RVA: 0x0002BD6C File Offset: 0x00029F6C
		internal virtual NumericUpDown nudMapWidth
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x060004BC RID: 1212 RVA: 0x0002BD75 File Offset: 0x00029F75
		// (set) Token: 0x060004BD RID: 1213 RVA: 0x0002BD7F File Offset: 0x00029F7F
		internal virtual Label lblMapHeight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x060004BE RID: 1214 RVA: 0x0002BD88 File Offset: 0x00029F88
		// (set) Token: 0x060004BF RID: 1215 RVA: 0x0002BD92 File Offset: 0x00029F92
		internal virtual Label lblMapWidth
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x060004C0 RID: 1216 RVA: 0x0002BD9B File Offset: 0x00029F9B
		// (set) Token: 0x060004C1 RID: 1217 RVA: 0x0002BDA8 File Offset: 0x00029FA8
		internal virtual Button btnUpdateMapHeaderAddresses
		{
			[CompilerGenerated]
			get
			{
				return this._btnUpdateMapHeaderAddresses;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeAddressMapHeader_Click);
				Button button = this._btnUpdateMapHeaderAddresses;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnUpdateMapHeaderAddresses = value;
				button = this._btnUpdateMapHeaderAddresses;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060004C2 RID: 1218 RVA: 0x0002BDEB File Offset: 0x00029FEB
		// (set) Token: 0x060004C3 RID: 1219 RVA: 0x0002BDF5 File Offset: 0x00029FF5
		internal virtual NumericUpDown nudBorderHeight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0002BDFE File Offset: 0x00029FFE
		// (set) Token: 0x060004C5 RID: 1221 RVA: 0x0002BE08 File Offset: 0x0002A008
		internal virtual NumericUpDown nudBorderWidth
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x0002BE11 File Offset: 0x0002A011
		// (set) Token: 0x060004C7 RID: 1223 RVA: 0x0002BE1B File Offset: 0x0002A01B
		internal virtual Label lblBorderHeight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060004C8 RID: 1224 RVA: 0x0002BE24 File Offset: 0x0002A024
		// (set) Token: 0x060004C9 RID: 1225 RVA: 0x0002BE2E File Offset: 0x0002A02E
		internal virtual Label lblBorderWidth
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060004CA RID: 1226 RVA: 0x0002BE37 File Offset: 0x0002A037
		// (set) Token: 0x060004CB RID: 1227 RVA: 0x0002BE41 File Offset: 0x0002A041
		internal virtual Label lblBorderDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060004CC RID: 1228 RVA: 0x0002BE4A File Offset: 0x0002A04A
		// (set) Token: 0x060004CD RID: 1229 RVA: 0x0002BE54 File Offset: 0x0002A054
		internal virtual TextBox txtBorderDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060004CE RID: 1230 RVA: 0x0002BE5D File Offset: 0x0002A05D
		// (set) Token: 0x060004CF RID: 1231 RVA: 0x0002BE67 File Offset: 0x0002A067
		internal virtual GroupBox grpTileset1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060004D0 RID: 1232 RVA: 0x0002BE70 File Offset: 0x0002A070
		// (set) Token: 0x060004D1 RID: 1233 RVA: 0x0002BE7A File Offset: 0x0002A07A
		internal virtual RadioButton rbTileset1Address
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0002BE83 File Offset: 0x0002A083
		// (set) Token: 0x060004D3 RID: 1235 RVA: 0x0002BE8D File Offset: 0x0002A08D
		internal virtual TextBox txtTileset1Address
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060004D4 RID: 1236 RVA: 0x0002BE96 File Offset: 0x0002A096
		// (set) Token: 0x060004D5 RID: 1237 RVA: 0x0002BEA0 File Offset: 0x0002A0A0
		internal virtual RadioButton rbTileset1Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060004D6 RID: 1238 RVA: 0x0002BEA9 File Offset: 0x0002A0A9
		// (set) Token: 0x060004D7 RID: 1239 RVA: 0x0002BEB3 File Offset: 0x0002A0B3
		internal virtual GroupBox grpTileset2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060004D8 RID: 1240 RVA: 0x0002BEBC File Offset: 0x0002A0BC
		// (set) Token: 0x060004D9 RID: 1241 RVA: 0x0002BEC6 File Offset: 0x0002A0C6
		internal virtual NumericUpDown nudTileset2Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x060004DA RID: 1242 RVA: 0x0002BECF File Offset: 0x0002A0CF
		// (set) Token: 0x060004DB RID: 1243 RVA: 0x0002BED9 File Offset: 0x0002A0D9
		internal virtual RadioButton rbTileset2Address
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x060004DC RID: 1244 RVA: 0x0002BEE2 File Offset: 0x0002A0E2
		// (set) Token: 0x060004DD RID: 1245 RVA: 0x0002BEEC File Offset: 0x0002A0EC
		internal virtual TextBox txtTileset2Address
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x060004DE RID: 1246 RVA: 0x0002BEF5 File Offset: 0x0002A0F5
		// (set) Token: 0x060004DF RID: 1247 RVA: 0x0002BEFF File Offset: 0x0002A0FF
		internal virtual RadioButton rbTileset2Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x0002BF08 File Offset: 0x0002A108
		// (set) Token: 0x060004E1 RID: 1249 RVA: 0x0002BF12 File Offset: 0x0002A112
		internal virtual NumericUpDown nudTileset1Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x060004E2 RID: 1250 RVA: 0x0002BF1B File Offset: 0x0002A11B
		// (set) Token: 0x060004E3 RID: 1251 RVA: 0x0002BF28 File Offset: 0x0002A128
		internal virtual Button btnChangeMapFooterData
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeMapFooterData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeMapFooterData_Click);
				Button button = this._btnChangeMapFooterData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeMapFooterData = value;
				button = this._btnChangeMapFooterData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x060004E4 RID: 1252 RVA: 0x0002BF6B File Offset: 0x0002A16B
		// (set) Token: 0x060004E5 RID: 1253 RVA: 0x0002BF75 File Offset: 0x0002A175
		internal virtual GroupBox grpTilesetDetail
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x060004E6 RID: 1254 RVA: 0x0002BF7E File Offset: 0x0002A17E
		// (set) Token: 0x060004E7 RID: 1255 RVA: 0x0002BF88 File Offset: 0x0002A188
		internal virtual GroupBox grpTileset2Detail
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x060004E8 RID: 1256 RVA: 0x0002BF91 File Offset: 0x0002A191
		// (set) Token: 0x060004E9 RID: 1257 RVA: 0x0002BF9B File Offset: 0x0002A19B
		internal virtual TextBox txtAddressTileset2BlockBehavior
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x060004EA RID: 1258 RVA: 0x0002BFA4 File Offset: 0x0002A1A4
		// (set) Token: 0x060004EB RID: 1259 RVA: 0x0002BFAE File Offset: 0x0002A1AE
		internal virtual TextBox txtAddressTileset2Animation
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x0002BFB7 File Offset: 0x0002A1B7
		// (set) Token: 0x060004ED RID: 1261 RVA: 0x0002BFC1 File Offset: 0x0002A1C1
		internal virtual TextBox txtAddressTileset2BlockImage
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x060004EE RID: 1262 RVA: 0x0002BFCA File Offset: 0x0002A1CA
		// (set) Token: 0x060004EF RID: 1263 RVA: 0x0002BFD4 File Offset: 0x0002A1D4
		internal virtual TextBox txtAddressTileset2Palette
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x060004F0 RID: 1264 RVA: 0x0002BFDD File Offset: 0x0002A1DD
		// (set) Token: 0x060004F1 RID: 1265 RVA: 0x0002BFE7 File Offset: 0x0002A1E7
		internal virtual TextBox txtAddressTileset2Image
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x060004F2 RID: 1266 RVA: 0x0002BFF0 File Offset: 0x0002A1F0
		// (set) Token: 0x060004F3 RID: 1267 RVA: 0x0002BFFA File Offset: 0x0002A1FA
		internal virtual Label lblAddressTileset2BlockBehavior
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x060004F4 RID: 1268 RVA: 0x0002C003 File Offset: 0x0002A203
		// (set) Token: 0x060004F5 RID: 1269 RVA: 0x0002C00D File Offset: 0x0002A20D
		internal virtual Label lblAddressTileset2Animation
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x060004F6 RID: 1270 RVA: 0x0002C016 File Offset: 0x0002A216
		// (set) Token: 0x060004F7 RID: 1271 RVA: 0x0002C020 File Offset: 0x0002A220
		internal virtual Label lblAddressTileset2BlockImage
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x060004F8 RID: 1272 RVA: 0x0002C029 File Offset: 0x0002A229
		// (set) Token: 0x060004F9 RID: 1273 RVA: 0x0002C033 File Offset: 0x0002A233
		internal virtual Label lblAddressTileset2Palette
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x060004FA RID: 1274 RVA: 0x0002C03C File Offset: 0x0002A23C
		// (set) Token: 0x060004FB RID: 1275 RVA: 0x0002C046 File Offset: 0x0002A246
		internal virtual Label lblAddressTileset2Image
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x060004FC RID: 1276 RVA: 0x0002C04F File Offset: 0x0002A24F
		// (set) Token: 0x060004FD RID: 1277 RVA: 0x0002C059 File Offset: 0x0002A259
		internal virtual ComboBox cmbTileset2PaletteType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x060004FE RID: 1278 RVA: 0x0002C062 File Offset: 0x0002A262
		// (set) Token: 0x060004FF RID: 1279 RVA: 0x0002C06C File Offset: 0x0002A26C
		internal virtual ComboBox cmbTileset2ImageCompressType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000500 RID: 1280 RVA: 0x0002C075 File Offset: 0x0002A275
		// (set) Token: 0x06000501 RID: 1281 RVA: 0x0002C07F File Offset: 0x0002A27F
		internal virtual Label lblTileset2PaletteType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000502 RID: 1282 RVA: 0x0002C088 File Offset: 0x0002A288
		// (set) Token: 0x06000503 RID: 1283 RVA: 0x0002C092 File Offset: 0x0002A292
		internal virtual Label lblTileset2ImageCompressType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000504 RID: 1284 RVA: 0x0002C09B File Offset: 0x0002A29B
		// (set) Token: 0x06000505 RID: 1285 RVA: 0x0002C0A5 File Offset: 0x0002A2A5
		internal virtual GroupBox grpTileset1Detail
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000506 RID: 1286 RVA: 0x0002C0AE File Offset: 0x0002A2AE
		// (set) Token: 0x06000507 RID: 1287 RVA: 0x0002C0B8 File Offset: 0x0002A2B8
		internal virtual TextBox txtAddressTileset1BlockBehavior
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06000508 RID: 1288 RVA: 0x0002C0C1 File Offset: 0x0002A2C1
		// (set) Token: 0x06000509 RID: 1289 RVA: 0x0002C0CB File Offset: 0x0002A2CB
		internal virtual TextBox txtAddressTileset1Animation
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x0600050A RID: 1290 RVA: 0x0002C0D4 File Offset: 0x0002A2D4
		// (set) Token: 0x0600050B RID: 1291 RVA: 0x0002C0DE File Offset: 0x0002A2DE
		internal virtual TextBox txtAddressTileset1BlockImage
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x0600050C RID: 1292 RVA: 0x0002C0E7 File Offset: 0x0002A2E7
		// (set) Token: 0x0600050D RID: 1293 RVA: 0x0002C0F1 File Offset: 0x0002A2F1
		internal virtual TextBox txtAddressTileset1Palette
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x0600050E RID: 1294 RVA: 0x0002C0FA File Offset: 0x0002A2FA
		// (set) Token: 0x0600050F RID: 1295 RVA: 0x0002C104 File Offset: 0x0002A304
		internal virtual TextBox txtAddressTileset1Image
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000510 RID: 1296 RVA: 0x0002C10D File Offset: 0x0002A30D
		// (set) Token: 0x06000511 RID: 1297 RVA: 0x0002C117 File Offset: 0x0002A317
		internal virtual Label lblAddressTileset1BlockBehavior
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000512 RID: 1298 RVA: 0x0002C120 File Offset: 0x0002A320
		// (set) Token: 0x06000513 RID: 1299 RVA: 0x0002C12A File Offset: 0x0002A32A
		internal virtual Label lblAddressTileset1Animation
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000514 RID: 1300 RVA: 0x0002C133 File Offset: 0x0002A333
		// (set) Token: 0x06000515 RID: 1301 RVA: 0x0002C13D File Offset: 0x0002A33D
		internal virtual Label lblAddressTileset1BlockImage
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000516 RID: 1302 RVA: 0x0002C146 File Offset: 0x0002A346
		// (set) Token: 0x06000517 RID: 1303 RVA: 0x0002C150 File Offset: 0x0002A350
		internal virtual Label lblAddressTileset1Palette
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000518 RID: 1304 RVA: 0x0002C159 File Offset: 0x0002A359
		// (set) Token: 0x06000519 RID: 1305 RVA: 0x0002C163 File Offset: 0x0002A363
		internal virtual Label lblAddressTileset1Image
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x0600051A RID: 1306 RVA: 0x0002C16C File Offset: 0x0002A36C
		// (set) Token: 0x0600051B RID: 1307 RVA: 0x0002C176 File Offset: 0x0002A376
		internal virtual ComboBox cmbTileset1PaletteType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x0600051C RID: 1308 RVA: 0x0002C17F File Offset: 0x0002A37F
		// (set) Token: 0x0600051D RID: 1309 RVA: 0x0002C189 File Offset: 0x0002A389
		internal virtual ComboBox cmbTileset1ImageCompressType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x0600051E RID: 1310 RVA: 0x0002C192 File Offset: 0x0002A392
		// (set) Token: 0x0600051F RID: 1311 RVA: 0x0002C19C File Offset: 0x0002A39C
		internal virtual Label lblTileset1PaletteType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000520 RID: 1312 RVA: 0x0002C1A5 File Offset: 0x0002A3A5
		// (set) Token: 0x06000521 RID: 1313 RVA: 0x0002C1AF File Offset: 0x0002A3AF
		internal virtual Label lblTileset1ImageCompressType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000522 RID: 1314 RVA: 0x0002C1B8 File Offset: 0x0002A3B8
		// (set) Token: 0x06000523 RID: 1315 RVA: 0x0002C1C4 File Offset: 0x0002A3C4
		internal virtual Button btnChangeTilesetData
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeTilesetData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeTilesetData_Click);
				Button button = this._btnChangeTilesetData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeTilesetData = value;
				button = this._btnChangeTilesetData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000524 RID: 1316 RVA: 0x0002C207 File Offset: 0x0002A407
		// (set) Token: 0x06000525 RID: 1317 RVA: 0x0002C214 File Offset: 0x0002A414
		internal virtual Panel pnlTilesetPalette
		{
			[CompilerGenerated]
			get
			{
				return this._pnlTilesetPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlTilesetPalette_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlTilesetPalette_MouseDown);
				MouseEventHandler mouseEventHandler2 = new MouseEventHandler(this.pnlTilesetPalette_MouseMove);
				MouseEventHandler mouseEventHandler3 = new MouseEventHandler(this.pnlTilesetPalette_MouseUp);
				Panel panel = this._pnlTilesetPalette;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
					panel.MouseMove -= mouseEventHandler2;
					panel.MouseUp -= mouseEventHandler3;
				}
				this._pnlTilesetPalette = value;
				panel = this._pnlTilesetPalette;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
					panel.MouseMove += mouseEventHandler2;
					panel.MouseUp += mouseEventHandler3;
				}
			}
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000526 RID: 1318 RVA: 0x0002C2B4 File Offset: 0x0002A4B4
		// (set) Token: 0x06000527 RID: 1319 RVA: 0x0002C2C0 File Offset: 0x0002A4C0
		internal virtual VScrollBar vsbTilesetScroll
		{
			[CompilerGenerated]
			get
			{
				return this._vsbTilesetScroll;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ScrollEventHandler scrollEventHandler = new ScrollEventHandler(this.ScrollHandlers);
				VScrollBar vscrollBar = this._vsbTilesetScroll;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll -= scrollEventHandler;
				}
				this._vsbTilesetScroll = value;
				vscrollBar = this._vsbTilesetScroll;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll += scrollEventHandler;
				}
			}
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06000528 RID: 1320 RVA: 0x0002C303 File Offset: 0x0002A503
		// (set) Token: 0x06000529 RID: 1321 RVA: 0x0002C310 File Offset: 0x0002A510
		internal virtual Panel pnlBorderDataPreview
		{
			[CompilerGenerated]
			get
			{
				return this._pnlBorderDataPreview;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlBorderDataPreview_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlBorderDataPreview_MouseDown);
				MouseEventHandler mouseEventHandler2 = new MouseEventHandler(this.pnlBorderDataPreview_MouseMove);
				MouseEventHandler mouseEventHandler3 = new MouseEventHandler(this.MouseUpHandlers);
				Panel panel = this._pnlBorderDataPreview;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
					panel.MouseMove -= mouseEventHandler2;
					panel.MouseUp -= mouseEventHandler3;
				}
				this._pnlBorderDataPreview = value;
				panel = this._pnlBorderDataPreview;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
					panel.MouseMove += mouseEventHandler2;
					panel.MouseUp += mouseEventHandler3;
				}
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x0600052A RID: 1322 RVA: 0x0002C3B0 File Offset: 0x0002A5B0
		// (set) Token: 0x0600052B RID: 1323 RVA: 0x0002C3BC File Offset: 0x0002A5BC
		internal virtual Panel pnlMapCanvas
		{
			[CompilerGenerated]
			get
			{
				return this._pnlMapCanvas;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlMapCanvas_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlMapCanvas_MouseDown);
				MouseEventHandler mouseEventHandler2 = new MouseEventHandler(this.pnlMapCanvas_MouseMove);
				EventHandler eventHandler = new EventHandler(this.pnlMapCanvas_MouseLeave);
				MouseEventHandler mouseEventHandler3 = new MouseEventHandler(this.MouseUpHandlers);
				EventHandler eventHandler2 = new EventHandler(this.pnlMapCanvas_Resize);
				EventHandler eventHandler3 = new EventHandler(this.pnlMapCanvas_MouseEnter);
				MouseEventHandler mouseEventHandler4 = new MouseEventHandler(this.pnlMapCanvas_MouseWheel);
				Panel panel = this._pnlMapCanvas;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
					panel.MouseMove -= mouseEventHandler2;
					panel.MouseLeave -= eventHandler;
					panel.MouseUp -= mouseEventHandler3;
					panel.Resize -= eventHandler2;
					panel.MouseEnter -= eventHandler3;
					panel.MouseWheel -= mouseEventHandler4;
				}
				this._pnlMapCanvas = value;
				panel = this._pnlMapCanvas;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
					panel.MouseMove += mouseEventHandler2;
					panel.MouseLeave += eventHandler;
					panel.MouseUp += mouseEventHandler3;
					panel.Resize += eventHandler2;
					panel.MouseEnter += eventHandler3;
					panel.MouseWheel += mouseEventHandler4;
				}
			}
		}

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x0600052C RID: 1324 RVA: 0x0002C4DC File Offset: 0x0002A6DC
		// (set) Token: 0x0600052D RID: 1325 RVA: 0x0002C4E8 File Offset: 0x0002A6E8
		internal virtual HScrollBar hsbMapDataPreview
		{
			[CompilerGenerated]
			get
			{
				return this._hsbMapDataPreview;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ScrollEventHandler scrollEventHandler = new ScrollEventHandler(this.ScrollHandlers);
				HScrollBar hscrollBar = this._hsbMapDataPreview;
				if (hscrollBar != null)
				{
					hscrollBar.Scroll -= scrollEventHandler;
				}
				this._hsbMapDataPreview = value;
				hscrollBar = this._hsbMapDataPreview;
				if (hscrollBar != null)
				{
					hscrollBar.Scroll += scrollEventHandler;
				}
			}
		}

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x0600052E RID: 1326 RVA: 0x0002C52B File Offset: 0x0002A72B
		// (set) Token: 0x0600052F RID: 1327 RVA: 0x0002C535 File Offset: 0x0002A735
		internal virtual GroupBox grpBorderDataPreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06000530 RID: 1328 RVA: 0x0002C53E File Offset: 0x0002A73E
		// (set) Token: 0x06000531 RID: 1329 RVA: 0x0002C548 File Offset: 0x0002A748
		internal virtual VScrollBar vsbMapDataPreview
		{
			[CompilerGenerated]
			get
			{
				return this._vsbMapDataPreview;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ScrollEventHandler scrollEventHandler = new ScrollEventHandler(this.ScrollHandlers);
				VScrollBar vscrollBar = this._vsbMapDataPreview;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll -= scrollEventHandler;
				}
				this._vsbMapDataPreview = value;
				vscrollBar = this._vsbMapDataPreview;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll += scrollEventHandler;
				}
			}
		}

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06000532 RID: 1330 RVA: 0x0002C58B File Offset: 0x0002A78B
		// (set) Token: 0x06000533 RID: 1331 RVA: 0x0002C595 File Offset: 0x0002A795
		internal virtual CheckBox chkShowWarp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000534 RID: 1332 RVA: 0x0002C59E File Offset: 0x0002A79E
		// (set) Token: 0x06000535 RID: 1333 RVA: 0x0002C5A8 File Offset: 0x0002A7A8
		internal virtual CheckBox chkShowTrapScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x0002C5B1 File Offset: 0x0002A7B1
		// (set) Token: 0x06000537 RID: 1335 RVA: 0x0002C5BB File Offset: 0x0002A7BB
		internal virtual CheckBox chkShowSign
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x0002C5C4 File Offset: 0x0002A7C4
		// (set) Token: 0x06000539 RID: 1337 RVA: 0x0002C5CE File Offset: 0x0002A7CE
		internal virtual CheckBox chkShowOverWorld
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x0600053A RID: 1338 RVA: 0x0002C5D7 File Offset: 0x0002A7D7
		// (set) Token: 0x0600053B RID: 1339 RVA: 0x0002C5E1 File Offset: 0x0002A7E1
		internal virtual Button btnLoadMapRight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x0600053C RID: 1340 RVA: 0x0002C5EA File Offset: 0x0002A7EA
		// (set) Token: 0x0600053D RID: 1341 RVA: 0x0002C5F4 File Offset: 0x0002A7F4
		internal virtual Button btnLoadMapLeft
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x0600053E RID: 1342 RVA: 0x0002C5FD File Offset: 0x0002A7FD
		// (set) Token: 0x0600053F RID: 1343 RVA: 0x0002C607 File Offset: 0x0002A807
		internal virtual Button btnLoadMapDown
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000540 RID: 1344 RVA: 0x0002C610 File Offset: 0x0002A810
		// (set) Token: 0x06000541 RID: 1345 RVA: 0x0002C61A File Offset: 0x0002A81A
		internal virtual Button btnLoadMapUp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000542 RID: 1346 RVA: 0x0002C623 File Offset: 0x0002A823
		// (set) Token: 0x06000543 RID: 1347 RVA: 0x0002C62D File Offset: 0x0002A82D
		internal virtual Button btnLoadMapDive
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06000544 RID: 1348 RVA: 0x0002C636 File Offset: 0x0002A836
		// (set) Token: 0x06000545 RID: 1349 RVA: 0x0002C640 File Offset: 0x0002A840
		internal virtual Button btnLoadMapEmerge
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x0002C649 File Offset: 0x0002A849
		// (set) Token: 0x06000547 RID: 1351 RVA: 0x0002C654 File Offset: 0x0002A854
		internal virtual CheckBox chkShowGrid
		{
			[CompilerGenerated]
			get
			{
				return this._chkShowGrid;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.chkShowGrid_CheckedChanged);
				CheckBox checkBox = this._chkShowGrid;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkShowGrid = value;
				checkBox = this._chkShowGrid;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000548 RID: 1352 RVA: 0x0002C697 File Offset: 0x0002A897
		// (set) Token: 0x06000549 RID: 1353 RVA: 0x0002C6A1 File Offset: 0x0002A8A1
		internal virtual Button btnLoadMapUpMoveLeft
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x0002C6AA File Offset: 0x0002A8AA
		// (set) Token: 0x0600054B RID: 1355 RVA: 0x0002C6B4 File Offset: 0x0002A8B4
		internal virtual Button btnLoadMapUpMoveRight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x0600054C RID: 1356 RVA: 0x0002C6BD File Offset: 0x0002A8BD
		// (set) Token: 0x0600054D RID: 1357 RVA: 0x0002C6C8 File Offset: 0x0002A8C8
		internal virtual TabControl tabEditorMode
		{
			[CompilerGenerated]
			get
			{
				return this._tabEditorMode;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.tabEditorMode_SelectedIndexChanged);
				TabControl tabControl = this._tabEditorMode;
				if (tabControl != null)
				{
					tabControl.SelectedIndexChanged -= eventHandler;
				}
				this._tabEditorMode = value;
				tabControl = this._tabEditorMode;
				if (tabControl != null)
				{
					tabControl.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x0600054E RID: 1358 RVA: 0x0002C70B File Offset: 0x0002A90B
		// (set) Token: 0x0600054F RID: 1359 RVA: 0x0002C715 File Offset: 0x0002A915
		internal virtual TabPage tabBlock
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000550 RID: 1360 RVA: 0x0002C71E File Offset: 0x0002A91E
		// (set) Token: 0x06000551 RID: 1361 RVA: 0x0002C728 File Offset: 0x0002A928
		internal virtual TabPage tabCollision
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000552 RID: 1362 RVA: 0x0002C731 File Offset: 0x0002A931
		// (set) Token: 0x06000553 RID: 1363 RVA: 0x0002C73B File Offset: 0x0002A93B
		internal virtual Label lblBlockIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000554 RID: 1364 RVA: 0x0002C744 File Offset: 0x0002A944
		// (set) Token: 0x06000555 RID: 1365 RVA: 0x0002C74E File Offset: 0x0002A94E
		internal virtual Label lblMapPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000556 RID: 1366 RVA: 0x0002C757 File Offset: 0x0002A957
		// (set) Token: 0x06000557 RID: 1367 RVA: 0x0002C761 File Offset: 0x0002A961
		internal virtual Label lblMapPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000558 RID: 1368 RVA: 0x0002C76A File Offset: 0x0002A96A
		// (set) Token: 0x06000559 RID: 1369 RVA: 0x0002C774 File Offset: 0x0002A974
		internal virtual Panel pnlCollisionPalette
		{
			[CompilerGenerated]
			get
			{
				return this._pnlCollisionPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlCollisionPalette_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlCollisionPalette_MouseDown);
				Panel panel = this._pnlCollisionPalette;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
				}
				this._pnlCollisionPalette = value;
				panel = this._pnlCollisionPalette;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
				}
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x0600055A RID: 1370 RVA: 0x0002C7D2 File Offset: 0x0002A9D2
		// (set) Token: 0x0600055B RID: 1371 RVA: 0x0002C7DC File Offset: 0x0002A9DC
		internal virtual CheckBox chkSyncTerrainId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x0600055C RID: 1372 RVA: 0x0002C7E5 File Offset: 0x0002A9E5
		// (set) Token: 0x0600055D RID: 1373 RVA: 0x0002C7EF File Offset: 0x0002A9EF
		internal virtual GroupBox grpEditMapConnection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x0600055E RID: 1374 RVA: 0x0002C7F8 File Offset: 0x0002A9F8
		// (set) Token: 0x0600055F RID: 1375 RVA: 0x0002C802 File Offset: 0x0002AA02
		internal virtual Label lblConnectedMapIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000560 RID: 1376 RVA: 0x0002C80B File Offset: 0x0002AA0B
		// (set) Token: 0x06000561 RID: 1377 RVA: 0x0002C815 File Offset: 0x0002AA15
		internal virtual NumericUpDown nudConnectedMapIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000562 RID: 1378 RVA: 0x0002C81E File Offset: 0x0002AA1E
		// (set) Token: 0x06000563 RID: 1379 RVA: 0x0002C828 File Offset: 0x0002AA28
		internal virtual Label lblConnectedMapShift
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000564 RID: 1380 RVA: 0x0002C831 File Offset: 0x0002AA31
		// (set) Token: 0x06000565 RID: 1381 RVA: 0x0002C83B File Offset: 0x0002AA3B
		internal virtual Label lblConnectedMapDirection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000566 RID: 1382 RVA: 0x0002C844 File Offset: 0x0002AA44
		// (set) Token: 0x06000567 RID: 1383 RVA: 0x0002C84E File Offset: 0x0002AA4E
		internal virtual ComboBox cmbConnectedMapDirection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000568 RID: 1384 RVA: 0x0002C857 File Offset: 0x0002AA57
		// (set) Token: 0x06000569 RID: 1385 RVA: 0x0002C861 File Offset: 0x0002AA61
		internal virtual Label lblConnectedMapNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x0600056A RID: 1386 RVA: 0x0002C86A File Offset: 0x0002AA6A
		// (set) Token: 0x0600056B RID: 1387 RVA: 0x0002C874 File Offset: 0x0002AA74
		internal virtual NumericUpDown nudConnectedMapNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x0600056C RID: 1388 RVA: 0x0002C87D File Offset: 0x0002AA7D
		// (set) Token: 0x0600056D RID: 1389 RVA: 0x0002C887 File Offset: 0x0002AA87
		internal virtual Label lblConnectedMapBank
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x0600056E RID: 1390 RVA: 0x0002C890 File Offset: 0x0002AA90
		// (set) Token: 0x0600056F RID: 1391 RVA: 0x0002C89A File Offset: 0x0002AA9A
		internal virtual NumericUpDown nudConnectedMapBank
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000570 RID: 1392 RVA: 0x0002C8A3 File Offset: 0x0002AAA3
		// (set) Token: 0x06000571 RID: 1393 RVA: 0x0002C8AD File Offset: 0x0002AAAD
		internal virtual NumericUpDown nudConnectedMapShift
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000572 RID: 1394 RVA: 0x0002C8B6 File Offset: 0x0002AAB6
		// (set) Token: 0x06000573 RID: 1395 RVA: 0x0002C8C0 File Offset: 0x0002AAC0
		internal virtual CheckBox chkShowConnectedMap
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000574 RID: 1396 RVA: 0x0002C8C9 File Offset: 0x0002AAC9
		// (set) Token: 0x06000575 RID: 1397 RVA: 0x0002C8D3 File Offset: 0x0002AAD3
		internal virtual TabPage tabNew
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000576 RID: 1398 RVA: 0x0002C8DC File Offset: 0x0002AADC
		// (set) Token: 0x06000577 RID: 1399 RVA: 0x0002C8E8 File Offset: 0x0002AAE8
		internal virtual Button btnMapScreenShot
		{
			[CompilerGenerated]
			get
			{
				return this._btnMapScreenShot;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMapScreenShot_Click);
				Button button = this._btnMapScreenShot;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMapScreenShot = value;
				button = this._btnMapScreenShot;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		internal virtual Button btnUndo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		internal virtual Button btnRedo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000578 RID: 1400 RVA: 0x0002C92B File Offset: 0x0002AB2B
		// (set) Token: 0x06000579 RID: 1401 RVA: 0x0002C935 File Offset: 0x0002AB35
		internal virtual Label lblCurrentMap
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x0600057A RID: 1402 RVA: 0x0002C93E File Offset: 0x0002AB3E
		// (set) Token: 0x0600057B RID: 1403 RVA: 0x0002C948 File Offset: 0x0002AB48
		internal virtual CheckBox chkPlayTileAnimation
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x0600057C RID: 1404 RVA: 0x0002C951 File Offset: 0x0002AB51
		// (set) Token: 0x0600057D RID: 1405 RVA: 0x0002C95C File Offset: 0x0002AB5C
		internal virtual CheckBox chkMapZoom2x
		{
			[CompilerGenerated]
			get
			{
				return this._chkMapZoom2x;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.chkMapZoom2x_CheckedChanged);
				CheckBox checkBox = this._chkMapZoom2x;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkMapZoom2x = value;
				checkBox = this._chkMapZoom2x;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x0600057E RID: 1406 RVA: 0x0002C99F File Offset: 0x0002AB9F
		// (set) Token: 0x0600057F RID: 1407 RVA: 0x0002C9A9 File Offset: 0x0002ABA9
		internal virtual TabPage tabEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000580 RID: 1408 RVA: 0x0002C9B2 File Offset: 0x0002ABB2
		// (set) Token: 0x06000581 RID: 1409 RVA: 0x0002C9BC File Offset: 0x0002ABBC
		internal virtual NumericUpDown nudEventNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000582 RID: 1410 RVA: 0x0002C9C5 File Offset: 0x0002ABC5
		// (set) Token: 0x06000583 RID: 1411 RVA: 0x0002C9CF File Offset: 0x0002ABCF
		internal virtual ComboBox cmbEventType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000584 RID: 1412 RVA: 0x0002C9D8 File Offset: 0x0002ABD8
		// (set) Token: 0x06000585 RID: 1413 RVA: 0x0002C9E4 File Offset: 0x0002ABE4
		internal virtual Button btnOpenBlockEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnOpenBlockEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnOpenBlockEditor_Click);
				Button button = this._btnOpenBlockEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnOpenBlockEditor = value;
				button = this._btnOpenBlockEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000586 RID: 1414 RVA: 0x0002CA27 File Offset: 0x0002AC27
		// (set) Token: 0x06000587 RID: 1415 RVA: 0x0002CA31 File Offset: 0x0002AC31
		internal virtual GroupBox grpPersonEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x06000588 RID: 1416 RVA: 0x0002CA3A File Offset: 0x0002AC3A
		// (set) Token: 0x06000589 RID: 1417 RVA: 0x0002CA44 File Offset: 0x0002AC44
		internal virtual NumericUpDown nudPersonNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x0600058A RID: 1418 RVA: 0x0002CA4D File Offset: 0x0002AC4D
		// (set) Token: 0x0600058B RID: 1419 RVA: 0x0002CA57 File Offset: 0x0002AC57
		internal virtual Label lblPersonNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x0600058C RID: 1420 RVA: 0x0002CA60 File Offset: 0x0002AC60
		// (set) Token: 0x0600058D RID: 1421 RVA: 0x0002CA6A File Offset: 0x0002AC6A
		internal virtual NumericUpDown nudPersonUnknownB2Upper
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x0600058E RID: 1422 RVA: 0x0002CA73 File Offset: 0x0002AC73
		// (set) Token: 0x0600058F RID: 1423 RVA: 0x0002CA7D File Offset: 0x0002AC7D
		internal virtual NumericUpDown nudPersonUnknownB2Lower
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000590 RID: 1424 RVA: 0x0002CA86 File Offset: 0x0002AC86
		// (set) Token: 0x06000591 RID: 1425 RVA: 0x0002CA90 File Offset: 0x0002AC90
		internal virtual NumericUpDown nudPersonSpriteNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000592 RID: 1426 RVA: 0x0002CA99 File Offset: 0x0002AC99
		// (set) Token: 0x06000593 RID: 1427 RVA: 0x0002CAA3 File Offset: 0x0002ACA3
		internal virtual Label lblPersonUnknownB2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000594 RID: 1428 RVA: 0x0002CAAC File Offset: 0x0002ACAC
		// (set) Token: 0x06000595 RID: 1429 RVA: 0x0002CAB6 File Offset: 0x0002ACB6
		internal virtual Label lblPersonSpriteNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000596 RID: 1430 RVA: 0x0002CABF File Offset: 0x0002ACBF
		// (set) Token: 0x06000597 RID: 1431 RVA: 0x0002CAC9 File Offset: 0x0002ACC9
		internal virtual NumericUpDown nudPersonPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000598 RID: 1432 RVA: 0x0002CAD2 File Offset: 0x0002ACD2
		// (set) Token: 0x06000599 RID: 1433 RVA: 0x0002CADC File Offset: 0x0002ACDC
		internal virtual ComboBox cmbPersonAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x0600059A RID: 1434 RVA: 0x0002CAE5 File Offset: 0x0002ACE5
		// (set) Token: 0x0600059B RID: 1435 RVA: 0x0002CAEF File Offset: 0x0002ACEF
		internal virtual Label lblPersonLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x0600059C RID: 1436 RVA: 0x0002CAF8 File Offset: 0x0002ACF8
		// (set) Token: 0x0600059D RID: 1437 RVA: 0x0002CB02 File Offset: 0x0002AD02
		internal virtual Label lblPersonPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x0600059E RID: 1438 RVA: 0x0002CB0B File Offset: 0x0002AD0B
		// (set) Token: 0x0600059F RID: 1439 RVA: 0x0002CB15 File Offset: 0x0002AD15
		internal virtual NumericUpDown nudPersonPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x060005A0 RID: 1440 RVA: 0x0002CB1E File Offset: 0x0002AD1E
		// (set) Token: 0x060005A1 RID: 1441 RVA: 0x0002CB28 File Offset: 0x0002AD28
		internal virtual Label lblPersonPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x060005A2 RID: 1442 RVA: 0x0002CB31 File Offset: 0x0002AD31
		// (set) Token: 0x060005A3 RID: 1443 RVA: 0x0002CB3B File Offset: 0x0002AD3B
		internal virtual ComboBox cmbPersonLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x0002CB44 File Offset: 0x0002AD44
		// (set) Token: 0x060005A5 RID: 1445 RVA: 0x0002CB4E File Offset: 0x0002AD4E
		internal virtual NumericUpDown nudPersonUnknownB13
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x060005A6 RID: 1446 RVA: 0x0002CB57 File Offset: 0x0002AD57
		// (set) Token: 0x060005A7 RID: 1447 RVA: 0x0002CB61 File Offset: 0x0002AD61
		internal virtual NumericUpDown nudPersonTrainer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x060005A8 RID: 1448 RVA: 0x0002CB6A File Offset: 0x0002AD6A
		// (set) Token: 0x060005A9 RID: 1449 RVA: 0x0002CB74 File Offset: 0x0002AD74
		internal virtual NumericUpDown nudPersonUnknownB11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x060005AA RID: 1450 RVA: 0x0002CB7D File Offset: 0x0002AD7D
		// (set) Token: 0x060005AB RID: 1451 RVA: 0x0002CB87 File Offset: 0x0002AD87
		internal virtual NumericUpDown nudPersonMovementRangeY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x060005AC RID: 1452 RVA: 0x0002CB90 File Offset: 0x0002AD90
		// (set) Token: 0x060005AD RID: 1453 RVA: 0x0002CB9A File Offset: 0x0002AD9A
		internal virtual Label lblPersonUnknownB13
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x060005AE RID: 1454 RVA: 0x0002CBA3 File Offset: 0x0002ADA3
		// (set) Token: 0x060005AF RID: 1455 RVA: 0x0002CBAD File Offset: 0x0002ADAD
		internal virtual Label lblPersonTrainer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060005B0 RID: 1456 RVA: 0x0002CBB6 File Offset: 0x0002ADB6
		// (set) Token: 0x060005B1 RID: 1457 RVA: 0x0002CBC0 File Offset: 0x0002ADC0
		internal virtual Label lblPersonUnknownB11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x060005B2 RID: 1458 RVA: 0x0002CBC9 File Offset: 0x0002ADC9
		// (set) Token: 0x060005B3 RID: 1459 RVA: 0x0002CBD3 File Offset: 0x0002ADD3
		internal virtual Label lblPersonMovementRange
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060005B4 RID: 1460 RVA: 0x0002CBDC File Offset: 0x0002ADDC
		// (set) Token: 0x060005B5 RID: 1461 RVA: 0x0002CBE6 File Offset: 0x0002ADE6
		internal virtual Label lblPersonSight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060005B6 RID: 1462 RVA: 0x0002CBEF File Offset: 0x0002ADEF
		// (set) Token: 0x060005B7 RID: 1463 RVA: 0x0002CBF9 File Offset: 0x0002ADF9
		internal virtual NumericUpDown nudPersonSight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x060005B8 RID: 1464 RVA: 0x0002CC02 File Offset: 0x0002AE02
		// (set) Token: 0x060005B9 RID: 1465 RVA: 0x0002CC0C File Offset: 0x0002AE0C
		internal virtual TextBox txtPersonScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x060005BA RID: 1466 RVA: 0x0002CC15 File Offset: 0x0002AE15
		// (set) Token: 0x060005BB RID: 1467 RVA: 0x0002CC1F File Offset: 0x0002AE1F
		internal virtual Label lblPersonScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x060005BC RID: 1468 RVA: 0x0002CC28 File Offset: 0x0002AE28
		// (set) Token: 0x060005BD RID: 1469 RVA: 0x0002CC32 File Offset: 0x0002AE32
		internal virtual Label lblPersonUnknownB22
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x060005BE RID: 1470 RVA: 0x0002CC3B File Offset: 0x0002AE3B
		// (set) Token: 0x060005BF RID: 1471 RVA: 0x0002CC45 File Offset: 0x0002AE45
		internal virtual NumericUpDown nudPersonUnknownB22
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x060005C0 RID: 1472 RVA: 0x0002CC4E File Offset: 0x0002AE4E
		// (set) Token: 0x060005C1 RID: 1473 RVA: 0x0002CC58 File Offset: 0x0002AE58
		internal virtual Label lblPersonFlag
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x060005C2 RID: 1474 RVA: 0x0002CC61 File Offset: 0x0002AE61
		// (set) Token: 0x060005C3 RID: 1475 RVA: 0x0002CC6B File Offset: 0x0002AE6B
		internal virtual Panel pnlCurrentMap
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x060005C4 RID: 1476 RVA: 0x0002CC74 File Offset: 0x0002AE74
		// (set) Token: 0x060005C5 RID: 1477 RVA: 0x0002CC7E File Offset: 0x0002AE7E
		internal virtual Panel pnlMapPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x060005C6 RID: 1478 RVA: 0x0002CC87 File Offset: 0x0002AE87
		// (set) Token: 0x060005C7 RID: 1479 RVA: 0x0002CC91 File Offset: 0x0002AE91
		internal virtual Panel pnlBlockIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x060005C8 RID: 1480 RVA: 0x0002CC9A File Offset: 0x0002AE9A
		// (set) Token: 0x060005C9 RID: 1481 RVA: 0x0002CCA4 File Offset: 0x0002AEA4
		internal virtual GroupBox grpWarpEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x060005CA RID: 1482 RVA: 0x0002CCAD File Offset: 0x0002AEAD
		// (set) Token: 0x060005CB RID: 1483 RVA: 0x0002CCB7 File Offset: 0x0002AEB7
		internal virtual NumericUpDown nudWarpToMapNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x060005CC RID: 1484 RVA: 0x0002CCC0 File Offset: 0x0002AEC0
		// (set) Token: 0x060005CD RID: 1485 RVA: 0x0002CCCA File Offset: 0x0002AECA
		internal virtual Label lblWarpToMapNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x060005CE RID: 1486 RVA: 0x0002CCD3 File Offset: 0x0002AED3
		// (set) Token: 0x060005CF RID: 1487 RVA: 0x0002CCDD File Offset: 0x0002AEDD
		internal virtual NumericUpDown nudWarpToMapBank
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x060005D0 RID: 1488 RVA: 0x0002CCE6 File Offset: 0x0002AEE6
		// (set) Token: 0x060005D1 RID: 1489 RVA: 0x0002CCF0 File Offset: 0x0002AEF0
		internal virtual Label lblWarpToMapBank
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x060005D2 RID: 1490 RVA: 0x0002CCF9 File Offset: 0x0002AEF9
		// (set) Token: 0x060005D3 RID: 1491 RVA: 0x0002CD03 File Offset: 0x0002AF03
		internal virtual NumericUpDown nudWarpToNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x060005D4 RID: 1492 RVA: 0x0002CD0C File Offset: 0x0002AF0C
		// (set) Token: 0x060005D5 RID: 1493 RVA: 0x0002CD16 File Offset: 0x0002AF16
		internal virtual Label lblWarpToNo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x060005D6 RID: 1494 RVA: 0x0002CD1F File Offset: 0x0002AF1F
		// (set) Token: 0x060005D7 RID: 1495 RVA: 0x0002CD29 File Offset: 0x0002AF29
		internal virtual ComboBox cmbWarpLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x060005D8 RID: 1496 RVA: 0x0002CD32 File Offset: 0x0002AF32
		// (set) Token: 0x060005D9 RID: 1497 RVA: 0x0002CD3C File Offset: 0x0002AF3C
		internal virtual Label lblWarpLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x0002CD45 File Offset: 0x0002AF45
		// (set) Token: 0x060005DB RID: 1499 RVA: 0x0002CD4F File Offset: 0x0002AF4F
		internal virtual Label lblWarpPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x0002CD58 File Offset: 0x0002AF58
		// (set) Token: 0x060005DD RID: 1501 RVA: 0x0002CD62 File Offset: 0x0002AF62
		internal virtual NumericUpDown nudWarpPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x0002CD6B File Offset: 0x0002AF6B
		// (set) Token: 0x060005DF RID: 1503 RVA: 0x0002CD75 File Offset: 0x0002AF75
		internal virtual Label lblWarpPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x060005E0 RID: 1504 RVA: 0x0002CD7E File Offset: 0x0002AF7E
		// (set) Token: 0x060005E1 RID: 1505 RVA: 0x0002CD88 File Offset: 0x0002AF88
		internal virtual NumericUpDown nudWarpPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x060005E2 RID: 1506 RVA: 0x0002CD91 File Offset: 0x0002AF91
		// (set) Token: 0x060005E3 RID: 1507 RVA: 0x0002CD9C File Offset: 0x0002AF9C
		internal virtual Button btnWarpGoTo
		{
			[CompilerGenerated]
			get
			{
				return this._btnWarpGoTo;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnWarpGoTo_Click);
				Button button = this._btnWarpGoTo;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnWarpGoTo = value;
				button = this._btnWarpGoTo;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x060005E4 RID: 1508 RVA: 0x0002CDDF File Offset: 0x0002AFDF
		// (set) Token: 0x060005E5 RID: 1509 RVA: 0x0002CDE9 File Offset: 0x0002AFE9
		internal virtual GroupBox grpTrapScriptEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x0002CDF2 File Offset: 0x0002AFF2
		// (set) Token: 0x060005E7 RID: 1511 RVA: 0x0002CDFC File Offset: 0x0002AFFC
		internal virtual NumericUpDown nudTrapScriptUnknownB5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x060005E8 RID: 1512 RVA: 0x0002CE05 File Offset: 0x0002B005
		// (set) Token: 0x060005E9 RID: 1513 RVA: 0x0002CE0F File Offset: 0x0002B00F
		internal virtual Label lblTrapScriptUnknownB5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x060005EA RID: 1514 RVA: 0x0002CE18 File Offset: 0x0002B018
		// (set) Token: 0x060005EB RID: 1515 RVA: 0x0002CE22 File Offset: 0x0002B022
		internal virtual ComboBox cmbTrapScriptLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x060005EC RID: 1516 RVA: 0x0002CE2B File Offset: 0x0002B02B
		// (set) Token: 0x060005ED RID: 1517 RVA: 0x0002CE35 File Offset: 0x0002B035
		internal virtual Label lblTrapScriptLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x060005EE RID: 1518 RVA: 0x0002CE3E File Offset: 0x0002B03E
		// (set) Token: 0x060005EF RID: 1519 RVA: 0x0002CE48 File Offset: 0x0002B048
		internal virtual Label lblTrapScriptPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x060005F0 RID: 1520 RVA: 0x0002CE51 File Offset: 0x0002B051
		// (set) Token: 0x060005F1 RID: 1521 RVA: 0x0002CE5B File Offset: 0x0002B05B
		internal virtual NumericUpDown nudTrapScriptPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x060005F2 RID: 1522 RVA: 0x0002CE64 File Offset: 0x0002B064
		// (set) Token: 0x060005F3 RID: 1523 RVA: 0x0002CE6E File Offset: 0x0002B06E
		internal virtual Label lblTrapScriptPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x060005F4 RID: 1524 RVA: 0x0002CE77 File Offset: 0x0002B077
		// (set) Token: 0x060005F5 RID: 1525 RVA: 0x0002CE81 File Offset: 0x0002B081
		internal virtual NumericUpDown nudTrapScriptPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x060005F6 RID: 1526 RVA: 0x0002CE8A File Offset: 0x0002B08A
		// (set) Token: 0x060005F7 RID: 1527 RVA: 0x0002CE94 File Offset: 0x0002B094
		internal virtual Button btnOpenScriptEditor
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x060005F8 RID: 1528 RVA: 0x0002CE9D File Offset: 0x0002B09D
		// (set) Token: 0x060005F9 RID: 1529 RVA: 0x0002CEA7 File Offset: 0x0002B0A7
		internal virtual TextBox txtTrapScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x060005FA RID: 1530 RVA: 0x0002CEB0 File Offset: 0x0002B0B0
		// (set) Token: 0x060005FB RID: 1531 RVA: 0x0002CEBA File Offset: 0x0002B0BA
		internal virtual Label lblTrapScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x060005FC RID: 1532 RVA: 0x0002CEC3 File Offset: 0x0002B0C3
		// (set) Token: 0x060005FD RID: 1533 RVA: 0x0002CECD File Offset: 0x0002B0CD
		internal virtual Label lblTrapScriptVarValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x0002CED6 File Offset: 0x0002B0D6
		// (set) Token: 0x060005FF RID: 1535 RVA: 0x0002CEE0 File Offset: 0x0002B0E0
		internal virtual Label lblTrapScriptUnknownB10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000600 RID: 1536 RVA: 0x0002CEE9 File Offset: 0x0002B0E9
		// (set) Token: 0x06000601 RID: 1537 RVA: 0x0002CEF3 File Offset: 0x0002B0F3
		internal virtual NumericUpDown nudTrapScriptUnknownB10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000602 RID: 1538 RVA: 0x0002CEFC File Offset: 0x0002B0FC
		// (set) Token: 0x06000603 RID: 1539 RVA: 0x0002CF06 File Offset: 0x0002B106
		internal virtual Label lblTrapScriptVarNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000604 RID: 1540 RVA: 0x0002CF0F File Offset: 0x0002B10F
		// (set) Token: 0x06000605 RID: 1541 RVA: 0x0002CF19 File Offset: 0x0002B119
		internal virtual GroupBox grpSignEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000606 RID: 1542 RVA: 0x0002CF22 File Offset: 0x0002B122
		// (set) Token: 0x06000607 RID: 1543 RVA: 0x0002CF2C File Offset: 0x0002B12C
		internal virtual ComboBox cmbSignLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000608 RID: 1544 RVA: 0x0002CF35 File Offset: 0x0002B135
		// (set) Token: 0x06000609 RID: 1545 RVA: 0x0002CF3F File Offset: 0x0002B13F
		internal virtual Label lblSignLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x0600060A RID: 1546 RVA: 0x0002CF48 File Offset: 0x0002B148
		// (set) Token: 0x0600060B RID: 1547 RVA: 0x0002CF52 File Offset: 0x0002B152
		internal virtual Label lblSignPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x0600060C RID: 1548 RVA: 0x0002CF5B File Offset: 0x0002B15B
		// (set) Token: 0x0600060D RID: 1549 RVA: 0x0002CF65 File Offset: 0x0002B165
		internal virtual NumericUpDown nudSignPositionY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x0600060E RID: 1550 RVA: 0x0002CF6E File Offset: 0x0002B16E
		// (set) Token: 0x0600060F RID: 1551 RVA: 0x0002CF78 File Offset: 0x0002B178
		internal virtual Label lblSignPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000610 RID: 1552 RVA: 0x0002CF81 File Offset: 0x0002B181
		// (set) Token: 0x06000611 RID: 1553 RVA: 0x0002CF8B File Offset: 0x0002B18B
		internal virtual NumericUpDown nudSignPositionX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x0002CF94 File Offset: 0x0002B194
		// (set) Token: 0x06000613 RID: 1555 RVA: 0x0002CF9E File Offset: 0x0002B19E
		internal virtual ComboBox cmbSignType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000614 RID: 1556 RVA: 0x0002CFA7 File Offset: 0x0002B1A7
		// (set) Token: 0x06000615 RID: 1557 RVA: 0x0002CFB1 File Offset: 0x0002B1B1
		internal virtual Label lblSignType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000616 RID: 1558 RVA: 0x0002CFBA File Offset: 0x0002B1BA
		// (set) Token: 0x06000617 RID: 1559 RVA: 0x0002CFC4 File Offset: 0x0002B1C4
		internal virtual TextBox txtSignScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000618 RID: 1560 RVA: 0x0002CFCD File Offset: 0x0002B1CD
		// (set) Token: 0x06000619 RID: 1561 RVA: 0x0002CFD7 File Offset: 0x0002B1D7
		internal virtual Label lblSignScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x0600061A RID: 1562 RVA: 0x0002CFE0 File Offset: 0x0002B1E0
		// (set) Token: 0x0600061B RID: 1563 RVA: 0x0002CFEA File Offset: 0x0002B1EA
		internal virtual Label lblSignUnknownB6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x0600061C RID: 1564 RVA: 0x0002CFF3 File Offset: 0x0002B1F3
		// (set) Token: 0x0600061D RID: 1565 RVA: 0x0002CFFD File Offset: 0x0002B1FD
		internal virtual NumericUpDown nudSignUnknownB6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x0600061E RID: 1566 RVA: 0x0002D006 File Offset: 0x0002B206
		// (set) Token: 0x0600061F RID: 1567 RVA: 0x0002D010 File Offset: 0x0002B210
		internal virtual NumericUpDown nudPersonMovementRangeX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000620 RID: 1568 RVA: 0x0002D019 File Offset: 0x0002B219
		// (set) Token: 0x06000621 RID: 1569 RVA: 0x0002D023 File Offset: 0x0002B223
		internal virtual TextBox txtPersonFlag
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000622 RID: 1570 RVA: 0x0002D02C File Offset: 0x0002B22C
		// (set) Token: 0x06000623 RID: 1571 RVA: 0x0002D036 File Offset: 0x0002B236
		internal virtual TextBox txtTrapScriptVarNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000624 RID: 1572 RVA: 0x0002D03F File Offset: 0x0002B23F
		// (set) Token: 0x06000625 RID: 1573 RVA: 0x0002D049 File Offset: 0x0002B249
		internal virtual TextBox txtTrapScriptVarValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000626 RID: 1574 RVA: 0x0002D052 File Offset: 0x0002B252
		// (set) Token: 0x06000627 RID: 1575 RVA: 0x0002D05C File Offset: 0x0002B25C
		internal virtual HScrollBar hsbTilesetScroll
		{
			[CompilerGenerated]
			get
			{
				return this._hsbTilesetScroll;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ScrollEventHandler scrollEventHandler = new ScrollEventHandler(this.ScrollHandlers);
				HScrollBar hscrollBar = this._hsbTilesetScroll;
				if (hscrollBar != null)
				{
					hscrollBar.Scroll -= scrollEventHandler;
				}
				this._hsbTilesetScroll = value;
				hscrollBar = this._hsbTilesetScroll;
				if (hscrollBar != null)
				{
					hscrollBar.Scroll += scrollEventHandler;
				}
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000628 RID: 1576 RVA: 0x0002D09F File Offset: 0x0002B29F
		// (set) Token: 0x06000629 RID: 1577 RVA: 0x0002D0AC File Offset: 0x0002B2AC
		internal virtual CheckBox chkTerrainIdMode
		{
			[CompilerGenerated]
			get
			{
				return this._chkTerrainIdMode;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.chkLoadTerrainIdTable_CheckedChanged);
				CheckBox checkBox = this._chkTerrainIdMode;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkTerrainIdMode = value;
				checkBox = this._chkTerrainIdMode;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x0600062A RID: 1578 RVA: 0x0002D0EF File Offset: 0x0002B2EF
		// (set) Token: 0x0600062B RID: 1579 RVA: 0x0002D0F9 File Offset: 0x0002B2F9
		internal virtual Panel pnlShowEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x0002D102 File Offset: 0x0002B302
		// (set) Token: 0x0600062D RID: 1581 RVA: 0x0002D10C File Offset: 0x0002B30C
		internal virtual GroupBox grpEditMapScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x0002D115 File Offset: 0x0002B315
		// (set) Token: 0x0600062F RID: 1583 RVA: 0x0002D11F File Offset: 0x0002B31F
		internal virtual ComboBox cmbMapScriptType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000630 RID: 1584 RVA: 0x0002D128 File Offset: 0x0002B328
		// (set) Token: 0x06000631 RID: 1585 RVA: 0x0002D132 File Offset: 0x0002B332
		internal virtual TextBox txtMapScriptListAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000632 RID: 1586 RVA: 0x0002D13B File Offset: 0x0002B33B
		// (set) Token: 0x06000633 RID: 1587 RVA: 0x0002D145 File Offset: 0x0002B345
		internal virtual Label lblMapScriptType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000634 RID: 1588 RVA: 0x0002D14E File Offset: 0x0002B34E
		// (set) Token: 0x06000635 RID: 1589 RVA: 0x0002D158 File Offset: 0x0002B358
		internal virtual Label lblMapScriptListAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000636 RID: 1590 RVA: 0x0002D161 File Offset: 0x0002B361
		// (set) Token: 0x06000637 RID: 1591 RVA: 0x0002D16B File Offset: 0x0002B36B
		internal virtual NumericUpDown nudMapScriptListIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000638 RID: 1592 RVA: 0x0002D174 File Offset: 0x0002B374
		// (set) Token: 0x06000639 RID: 1593 RVA: 0x0002D17E File Offset: 0x0002B37E
		internal virtual TextBox txtMapScriptVar
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x0600063A RID: 1594 RVA: 0x0002D187 File Offset: 0x0002B387
		// (set) Token: 0x0600063B RID: 1595 RVA: 0x0002D191 File Offset: 0x0002B391
		internal virtual Label lblMapScriptVar
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x0600063C RID: 1596 RVA: 0x0002D19A File Offset: 0x0002B39A
		// (set) Token: 0x0600063D RID: 1597 RVA: 0x0002D1A4 File Offset: 0x0002B3A4
		internal virtual TextBox txtMapScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x0600063E RID: 1598 RVA: 0x0002D1AD File Offset: 0x0002B3AD
		// (set) Token: 0x0600063F RID: 1599 RVA: 0x0002D1B7 File Offset: 0x0002B3B7
		internal virtual Label lblMapScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06000640 RID: 1600 RVA: 0x0002D1C0 File Offset: 0x0002B3C0
		// (set) Token: 0x06000641 RID: 1601 RVA: 0x0002D1CA File Offset: 0x0002B3CA
		internal virtual TextBox txtMapScriptValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06000642 RID: 1602 RVA: 0x0002D1D3 File Offset: 0x0002B3D3
		// (set) Token: 0x06000643 RID: 1603 RVA: 0x0002D1DD File Offset: 0x0002B3DD
		internal virtual Label lblMapScriptValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000644 RID: 1604 RVA: 0x0002D1E6 File Offset: 0x0002B3E6
		// (set) Token: 0x06000645 RID: 1605 RVA: 0x0002D1F0 File Offset: 0x0002B3F0
		internal virtual Button btnChangeMapScriptData
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeMapScriptData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeMapScriptData_Click);
				Button button = this._btnChangeMapScriptData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeMapScriptData = value;
				button = this._btnChangeMapScriptData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000646 RID: 1606 RVA: 0x0002D233 File Offset: 0x0002B433
		// (set) Token: 0x06000647 RID: 1607 RVA: 0x0002D23D File Offset: 0x0002B43D
		internal virtual GroupBox grpNewTileset
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000648 RID: 1608 RVA: 0x0002D246 File Offset: 0x0002B446
		// (set) Token: 0x06000649 RID: 1609 RVA: 0x0002D250 File Offset: 0x0002B450
		internal virtual ComboBox cmbNewTilesetType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700027E RID: 638
		// (get) Token: 0x0600064A RID: 1610 RVA: 0x0002D259 File Offset: 0x0002B459
		// (set) Token: 0x0600064B RID: 1611 RVA: 0x0002D263 File Offset: 0x0002B463
		internal virtual Label lblNewTilesetCompress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x0600064C RID: 1612 RVA: 0x0002D26C File Offset: 0x0002B46C
		// (set) Token: 0x0600064D RID: 1613 RVA: 0x0002D276 File Offset: 0x0002B476
		internal virtual Label lblNewTilesetType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x0600064E RID: 1614 RVA: 0x0002D27F File Offset: 0x0002B47F
		// (set) Token: 0x0600064F RID: 1615 RVA: 0x0002D289 File Offset: 0x0002B489
		internal virtual ComboBox cmbNewTilesetCompress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000650 RID: 1616 RVA: 0x0002D292 File Offset: 0x0002B492
		// (set) Token: 0x06000651 RID: 1617 RVA: 0x0002D29C File Offset: 0x0002B49C
		internal virtual Label lblNewTilesetAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000652 RID: 1618 RVA: 0x0002D2A5 File Offset: 0x0002B4A5
		// (set) Token: 0x06000653 RID: 1619 RVA: 0x0002D2AF File Offset: 0x0002B4AF
		internal virtual NumericUpDown nudNewTilesetBlockCount
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06000654 RID: 1620 RVA: 0x0002D2B8 File Offset: 0x0002B4B8
		// (set) Token: 0x06000655 RID: 1621 RVA: 0x0002D2C2 File Offset: 0x0002B4C2
		internal virtual Label lblNewTilesetBlockCount
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06000656 RID: 1622 RVA: 0x0002D2CB File Offset: 0x0002B4CB
		// (set) Token: 0x06000657 RID: 1623 RVA: 0x0002D2D8 File Offset: 0x0002B4D8
		internal virtual Button btnSaveNewTileset
		{
			[CompilerGenerated]
			get
			{
				return this._btnSaveNewTileset;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSaveNewTileset_Click);
				Button button = this._btnSaveNewTileset;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSaveNewTileset = value;
				button = this._btnSaveNewTileset;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06000658 RID: 1624 RVA: 0x0002D31B File Offset: 0x0002B51B
		// (set) Token: 0x06000659 RID: 1625 RVA: 0x0002D325 File Offset: 0x0002B525
		internal virtual TextBox txtNewTilesetAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x0600065A RID: 1626 RVA: 0x0002D32E File Offset: 0x0002B52E
		// (set) Token: 0x0600065B RID: 1627 RVA: 0x0002D338 File Offset: 0x0002B538
		internal virtual GroupBox grpNewPalette
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x0600065C RID: 1628 RVA: 0x0002D341 File Offset: 0x0002B541
		// (set) Token: 0x0600065D RID: 1629 RVA: 0x0002D34C File Offset: 0x0002B54C
		internal virtual Button btnSaveNewPalette
		{
			[CompilerGenerated]
			get
			{
				return this._btnSaveNewPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSaveNewPalette_Click);
				Button button = this._btnSaveNewPalette;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSaveNewPalette = value;
				button = this._btnSaveNewPalette;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x0600065E RID: 1630 RVA: 0x0002D38F File Offset: 0x0002B58F
		// (set) Token: 0x0600065F RID: 1631 RVA: 0x0002D399 File Offset: 0x0002B599
		internal virtual NumericUpDown nudNewPaletteIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000660 RID: 1632 RVA: 0x0002D3A2 File Offset: 0x0002B5A2
		// (set) Token: 0x06000661 RID: 1633 RVA: 0x0002D3AC File Offset: 0x0002B5AC
		internal virtual NumericUpDown nudNewPaletteTilesetIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000662 RID: 1634 RVA: 0x0002D3B5 File Offset: 0x0002B5B5
		// (set) Token: 0x06000663 RID: 1635 RVA: 0x0002D3BF File Offset: 0x0002B5BF
		internal virtual Label lblNewPaletteIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000664 RID: 1636 RVA: 0x0002D3C8 File Offset: 0x0002B5C8
		// (set) Token: 0x06000665 RID: 1637 RVA: 0x0002D3D2 File Offset: 0x0002B5D2
		internal virtual Label lblNewPaletteTilesetIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x0002D3DB File Offset: 0x0002B5DB
		// (set) Token: 0x06000667 RID: 1639 RVA: 0x0002D3E5 File Offset: 0x0002B5E5
		internal virtual GroupBox grpPalettePreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000668 RID: 1640 RVA: 0x0002D3EE File Offset: 0x0002B5EE
		// (set) Token: 0x06000669 RID: 1641 RVA: 0x0002D3F8 File Offset: 0x0002B5F8
		internal virtual Panel pnlPalettePreview
		{
			[CompilerGenerated]
			get
			{
				return this._pnlPalettePreview;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlPalettePreview_Paint);
				Panel panel = this._pnlPalettePreview;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
				}
				this._pnlPalettePreview = value;
				panel = this._pnlPalettePreview;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
				}
			}
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x0600066A RID: 1642 RVA: 0x0002D43B File Offset: 0x0002B63B
		// (set) Token: 0x0600066B RID: 1643 RVA: 0x0002D445 File Offset: 0x0002B645
		internal virtual GroupBox grpNewMapFooter
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x0600066C RID: 1644 RVA: 0x0002D44E File Offset: 0x0002B64E
		// (set) Token: 0x0600066D RID: 1645 RVA: 0x0002D458 File Offset: 0x0002B658
		internal virtual NumericUpDown nudNewMapFooterMapSizeX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x0600066E RID: 1646 RVA: 0x0002D461 File Offset: 0x0002B661
		// (set) Token: 0x0600066F RID: 1647 RVA: 0x0002D46B File Offset: 0x0002B66B
		internal virtual Label lblNewMapFooterMapSizeX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06000670 RID: 1648 RVA: 0x0002D474 File Offset: 0x0002B674
		// (set) Token: 0x06000671 RID: 1649 RVA: 0x0002D47E File Offset: 0x0002B67E
		internal virtual NumericUpDown nudNewMapFooterTileset2Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000672 RID: 1650 RVA: 0x0002D487 File Offset: 0x0002B687
		// (set) Token: 0x06000673 RID: 1651 RVA: 0x0002D491 File Offset: 0x0002B691
		internal virtual NumericUpDown nudNewMapFooterTileset1Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000674 RID: 1652 RVA: 0x0002D49A File Offset: 0x0002B69A
		// (set) Token: 0x06000675 RID: 1653 RVA: 0x0002D4A4 File Offset: 0x0002B6A4
		internal virtual Label lblNewMapFooterTileset2Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000676 RID: 1654 RVA: 0x0002D4AD File Offset: 0x0002B6AD
		// (set) Token: 0x06000677 RID: 1655 RVA: 0x0002D4B7 File Offset: 0x0002B6B7
		internal virtual Label lblNewMapFooterTileset1Index
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06000678 RID: 1656 RVA: 0x0002D4C0 File Offset: 0x0002B6C0
		// (set) Token: 0x06000679 RID: 1657 RVA: 0x0002D4CA File Offset: 0x0002B6CA
		internal virtual NumericUpDown nudNewMapFooterBorderSizeY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x0600067A RID: 1658 RVA: 0x0002D4D3 File Offset: 0x0002B6D3
		// (set) Token: 0x0600067B RID: 1659 RVA: 0x0002D4DD File Offset: 0x0002B6DD
		internal virtual NumericUpDown nudNewMapFooterMapSizeY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x0600067C RID: 1660 RVA: 0x0002D4E6 File Offset: 0x0002B6E6
		// (set) Token: 0x0600067D RID: 1661 RVA: 0x0002D4F0 File Offset: 0x0002B6F0
		internal virtual NumericUpDown nudNewMapFooterBorderSizeX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x0600067E RID: 1662 RVA: 0x0002D4F9 File Offset: 0x0002B6F9
		// (set) Token: 0x0600067F RID: 1663 RVA: 0x0002D503 File Offset: 0x0002B703
		internal virtual Label lblNewMapFooterBorderSizeY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06000680 RID: 1664 RVA: 0x0002D50C File Offset: 0x0002B70C
		// (set) Token: 0x06000681 RID: 1665 RVA: 0x0002D516 File Offset: 0x0002B716
		internal virtual Label lblNewMapFooterMapSizeY
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06000682 RID: 1666 RVA: 0x0002D51F File Offset: 0x0002B71F
		// (set) Token: 0x06000683 RID: 1667 RVA: 0x0002D529 File Offset: 0x0002B729
		internal virtual Label lblNewMapFooterBorderSizeX
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06000684 RID: 1668 RVA: 0x0002D532 File Offset: 0x0002B732
		// (set) Token: 0x06000685 RID: 1669 RVA: 0x0002D53C File Offset: 0x0002B73C
		internal virtual Button btnNewMapFooter
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewMapFooter;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewMapFooter_Click);
				Button button = this._btnNewMapFooter;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewMapFooter = value;
				button = this._btnNewMapFooter;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06000686 RID: 1670 RVA: 0x0002D57F File Offset: 0x0002B77F
		// (set) Token: 0x06000687 RID: 1671 RVA: 0x0002D589 File Offset: 0x0002B789
		internal virtual TextBox txtNewMapFooterAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000688 RID: 1672 RVA: 0x0002D592 File Offset: 0x0002B792
		// (set) Token: 0x06000689 RID: 1673 RVA: 0x0002D59C File Offset: 0x0002B79C
		internal virtual Label lblNewMapFooterAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x0600068A RID: 1674 RVA: 0x0002D5A5 File Offset: 0x0002B7A5
		// (set) Token: 0x0600068B RID: 1675 RVA: 0x0002D5AF File Offset: 0x0002B7AF
		internal virtual GroupBox grpNewEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x0600068C RID: 1676 RVA: 0x0002D5B8 File Offset: 0x0002B7B8
		// (set) Token: 0x0600068D RID: 1677 RVA: 0x0002D5C2 File Offset: 0x0002B7C2
		internal virtual NumericUpDown nudNewEventWarp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x0600068E RID: 1678 RVA: 0x0002D5CB File Offset: 0x0002B7CB
		// (set) Token: 0x0600068F RID: 1679 RVA: 0x0002D5D5 File Offset: 0x0002B7D5
		internal virtual NumericUpDown nudNewEventSign
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000690 RID: 1680 RVA: 0x0002D5DE File Offset: 0x0002B7DE
		// (set) Token: 0x06000691 RID: 1681 RVA: 0x0002D5E8 File Offset: 0x0002B7E8
		internal virtual NumericUpDown nudNewEventTrap
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000692 RID: 1682 RVA: 0x0002D5F1 File Offset: 0x0002B7F1
		// (set) Token: 0x06000693 RID: 1683 RVA: 0x0002D5FB File Offset: 0x0002B7FB
		internal virtual NumericUpDown nudNewEventPerson
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06000694 RID: 1684 RVA: 0x0002D604 File Offset: 0x0002B804
		// (set) Token: 0x06000695 RID: 1685 RVA: 0x0002D60E File Offset: 0x0002B80E
		internal virtual Label lblNewEventWarp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06000696 RID: 1686 RVA: 0x0002D617 File Offset: 0x0002B817
		// (set) Token: 0x06000697 RID: 1687 RVA: 0x0002D621 File Offset: 0x0002B821
		internal virtual Label lblNewEventSign
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000698 RID: 1688 RVA: 0x0002D62A File Offset: 0x0002B82A
		// (set) Token: 0x06000699 RID: 1689 RVA: 0x0002D634 File Offset: 0x0002B834
		internal virtual Label lblNewEventTrap
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x0600069A RID: 1690 RVA: 0x0002D63D File Offset: 0x0002B83D
		// (set) Token: 0x0600069B RID: 1691 RVA: 0x0002D647 File Offset: 0x0002B847
		internal virtual Label lblNewEventPerson
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x0600069C RID: 1692 RVA: 0x0002D650 File Offset: 0x0002B850
		// (set) Token: 0x0600069D RID: 1693 RVA: 0x0002D65A File Offset: 0x0002B85A
		internal virtual TextBox txtNewEventAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x0600069E RID: 1694 RVA: 0x0002D663 File Offset: 0x0002B863
		// (set) Token: 0x0600069F RID: 1695 RVA: 0x0002D66D File Offset: 0x0002B86D
		internal virtual Label lblNewEventAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x060006A0 RID: 1696 RVA: 0x0002D676 File Offset: 0x0002B876
		// (set) Token: 0x060006A1 RID: 1697 RVA: 0x0002D680 File Offset: 0x0002B880
		internal virtual Button btnNewEvent
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewEvent;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewEvent_Click);
				Button button = this._btnNewEvent;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewEvent = value;
				button = this._btnNewEvent;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x060006A2 RID: 1698 RVA: 0x0002D6C3 File Offset: 0x0002B8C3
		// (set) Token: 0x060006A3 RID: 1699 RVA: 0x0002D6CD File Offset: 0x0002B8CD
		internal virtual GroupBox grpNewMapScript
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x060006A4 RID: 1700 RVA: 0x0002D6D6 File Offset: 0x0002B8D6
		// (set) Token: 0x060006A5 RID: 1701 RVA: 0x0002D6E0 File Offset: 0x0002B8E0
		internal virtual CheckBox chkNewMapScriptType01
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x060006A6 RID: 1702 RVA: 0x0002D6E9 File Offset: 0x0002B8E9
		// (set) Token: 0x060006A7 RID: 1703 RVA: 0x0002D6F3 File Offset: 0x0002B8F3
		internal virtual CheckBox chkNewMapScriptType07
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x060006A8 RID: 1704 RVA: 0x0002D6FC File Offset: 0x0002B8FC
		// (set) Token: 0x060006A9 RID: 1705 RVA: 0x0002D706 File Offset: 0x0002B906
		internal virtual CheckBox chkNewMapScriptType06
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x060006AA RID: 1706 RVA: 0x0002D70F File Offset: 0x0002B90F
		// (set) Token: 0x060006AB RID: 1707 RVA: 0x0002D719 File Offset: 0x0002B919
		internal virtual CheckBox chkNewMapScriptType05
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x060006AC RID: 1708 RVA: 0x0002D722 File Offset: 0x0002B922
		// (set) Token: 0x060006AD RID: 1709 RVA: 0x0002D72C File Offset: 0x0002B92C
		internal virtual CheckBox chkNewMapScriptType04
		{
			[CompilerGenerated]
			get
			{
				return this._chkNewMapScriptType04;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.chkNewMapScriptType04_CheckedChanged);
				CheckBox checkBox = this._chkNewMapScriptType04;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkNewMapScriptType04 = value;
				checkBox = this._chkNewMapScriptType04;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x060006AE RID: 1710 RVA: 0x0002D76F File Offset: 0x0002B96F
		// (set) Token: 0x060006AF RID: 1711 RVA: 0x0002D779 File Offset: 0x0002B979
		internal virtual CheckBox chkNewMapScriptType03
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x060006B0 RID: 1712 RVA: 0x0002D782 File Offset: 0x0002B982
		// (set) Token: 0x060006B1 RID: 1713 RVA: 0x0002D78C File Offset: 0x0002B98C
		internal virtual CheckBox chkNewMapScriptType02
		{
			[CompilerGenerated]
			get
			{
				return this._chkNewMapScriptType02;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.chkNewMapScriptType02_CheckedChanged);
				CheckBox checkBox = this._chkNewMapScriptType02;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkNewMapScriptType02 = value;
				checkBox = this._chkNewMapScriptType02;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x060006B2 RID: 1714 RVA: 0x0002D7CF File Offset: 0x0002B9CF
		// (set) Token: 0x060006B3 RID: 1715 RVA: 0x0002D7D9 File Offset: 0x0002B9D9
		internal virtual TextBox txtNewMapScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x060006B4 RID: 1716 RVA: 0x0002D7E2 File Offset: 0x0002B9E2
		// (set) Token: 0x060006B5 RID: 1717 RVA: 0x0002D7EC File Offset: 0x0002B9EC
		internal virtual Label lblNewMapScriptAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x060006B6 RID: 1718 RVA: 0x0002D7F5 File Offset: 0x0002B9F5
		// (set) Token: 0x060006B7 RID: 1719 RVA: 0x0002D800 File Offset: 0x0002BA00
		internal virtual Button btnNewMapScript
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewMapScript;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewMapScript_Click);
				Button button = this._btnNewMapScript;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewMapScript = value;
				button = this._btnNewMapScript;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x060006B8 RID: 1720 RVA: 0x0002D843 File Offset: 0x0002BA43
		// (set) Token: 0x060006B9 RID: 1721 RVA: 0x0002D84D File Offset: 0x0002BA4D
		internal virtual Label lblNewMapScriptType04
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x060006BA RID: 1722 RVA: 0x0002D856 File Offset: 0x0002BA56
		// (set) Token: 0x060006BB RID: 1723 RVA: 0x0002D860 File Offset: 0x0002BA60
		internal virtual NumericUpDown nudNewMapScriptType04
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x060006BC RID: 1724 RVA: 0x0002D869 File Offset: 0x0002BA69
		// (set) Token: 0x060006BD RID: 1725 RVA: 0x0002D873 File Offset: 0x0002BA73
		internal virtual Label lblNewMapScriptType02
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x060006BE RID: 1726 RVA: 0x0002D87C File Offset: 0x0002BA7C
		// (set) Token: 0x060006BF RID: 1727 RVA: 0x0002D886 File Offset: 0x0002BA86
		internal virtual NumericUpDown nudNewMapScriptType02
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x060006C0 RID: 1728 RVA: 0x0002D88F File Offset: 0x0002BA8F
		// (set) Token: 0x060006C1 RID: 1729 RVA: 0x0002D899 File Offset: 0x0002BA99
		internal virtual GroupBox grpNewMapConnection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x060006C2 RID: 1730 RVA: 0x0002D8A2 File Offset: 0x0002BAA2
		// (set) Token: 0x060006C3 RID: 1731 RVA: 0x0002D8AC File Offset: 0x0002BAAC
		internal virtual TextBox txtNewMapConnectionAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x060006C4 RID: 1732 RVA: 0x0002D8B5 File Offset: 0x0002BAB5
		// (set) Token: 0x060006C5 RID: 1733 RVA: 0x0002D8BF File Offset: 0x0002BABF
		internal virtual Label lblNewMapConnectionAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x0002D8C8 File Offset: 0x0002BAC8
		// (set) Token: 0x060006C7 RID: 1735 RVA: 0x0002D8D4 File Offset: 0x0002BAD4
		internal virtual Button btnNewMapConnection
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewMapConnection;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewMapConnection_Click);
				Button button = this._btnNewMapConnection;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewMapConnection = value;
				button = this._btnNewMapConnection;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x060006C8 RID: 1736 RVA: 0x0002D917 File Offset: 0x0002BB17
		// (set) Token: 0x060006C9 RID: 1737 RVA: 0x0002D921 File Offset: 0x0002BB21
		internal virtual Label lblNewMapConnectionCount
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x060006CA RID: 1738 RVA: 0x0002D92A File Offset: 0x0002BB2A
		// (set) Token: 0x060006CB RID: 1739 RVA: 0x0002D934 File Offset: 0x0002BB34
		internal virtual NumericUpDown nudNewMapConnectionCount
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x0002D93D File Offset: 0x0002BB3D
		// (set) Token: 0x060006CD RID: 1741 RVA: 0x0002D947 File Offset: 0x0002BB47
		internal virtual GroupBox grpNewMapName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x060006CE RID: 1742 RVA: 0x0002D950 File Offset: 0x0002BB50
		// (set) Token: 0x060006CF RID: 1743 RVA: 0x0002D95A File Offset: 0x0002BB5A
		internal virtual Label lblNewMapNameOld
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x060006D0 RID: 1744 RVA: 0x0002D963 File Offset: 0x0002BB63
		// (set) Token: 0x060006D1 RID: 1745 RVA: 0x0002D96D File Offset: 0x0002BB6D
		internal virtual ComboBox cmbNewMapName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x060006D2 RID: 1746 RVA: 0x0002D976 File Offset: 0x0002BB76
		// (set) Token: 0x060006D3 RID: 1747 RVA: 0x0002D980 File Offset: 0x0002BB80
		internal virtual Label lblNewMapNameNew
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x060006D4 RID: 1748 RVA: 0x0002D989 File Offset: 0x0002BB89
		// (set) Token: 0x060006D5 RID: 1749 RVA: 0x0002D993 File Offset: 0x0002BB93
		internal virtual TextBox txtNewMapAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x0002D99C File Offset: 0x0002BB9C
		// (set) Token: 0x060006D7 RID: 1751 RVA: 0x0002D9A6 File Offset: 0x0002BBA6
		internal virtual Label lblNewMapAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x060006D8 RID: 1752 RVA: 0x0002D9AF File Offset: 0x0002BBAF
		// (set) Token: 0x060006D9 RID: 1753 RVA: 0x0002D9BC File Offset: 0x0002BBBC
		internal virtual Button btnNewMapName
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewMapName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewMapName_Click);
				Button button = this._btnNewMapName;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewMapName = value;
				button = this._btnNewMapName;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x060006DA RID: 1754 RVA: 0x0002D9FF File Offset: 0x0002BBFF
		// (set) Token: 0x060006DB RID: 1755 RVA: 0x0002DA09 File Offset: 0x0002BC09
		internal virtual TextBox txtNewMapNameNew
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x060006DC RID: 1756 RVA: 0x0002DA14 File Offset: 0x0002BC14
		private void MapEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.InitializeUIHelpers();
			this.InitializeResources();
			this.InitializeComboBoxes();
			this.InitializeEventHandlers();
			this.InitializeNumericUpDowns();
			this.ReadAllMapHeaders();
			this.RefreshMapTree();
			this.LoadTileset2BlockLimits();
			this.ResetEditorState();
			this.ResetNewTabControls();
			this.SetUnsavedChanges(false);
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x0002DA7C File Offset: 0x0002BC7C
		private void MapEditor_Shown(object sender, EventArgs e)
		{
			this.ShowMapToolWindow();
		}

		// Token: 0x060006DD RID: 1757 RVA: 0x0002DA7C File Offset: 0x0002BC7C
		private void InitializeUIHelpers()
		{
			this.tileset1UI = new MapEditor.TilesetUIContainer
			{
				RbIndex = this.rbTileset1Index,
				RbAddress = this.rbTileset1Address,
				NudIndex = this.nudTileset1Index,
				TxtAddress = this.txtTileset1Address,
				CmbCompress = this.cmbTileset1ImageCompressType,
				CmbPaletteType = this.cmbTileset1PaletteType,
				TxtImageAddress = this.txtAddressTileset1Image,
				TxtPaletteAddress = this.txtAddressTileset1Palette,
				TxtBlockImageAddress = this.txtAddressTileset1BlockImage,
				TxtAnimationAddress = this.txtAddressTileset1Animation,
				TxtBehaviorAddress = this.txtAddressTileset1BlockBehavior
			};
			this.tileset2UI = new MapEditor.TilesetUIContainer
			{
				RbIndex = this.rbTileset2Index,
				RbAddress = this.rbTileset2Address,
				NudIndex = this.nudTileset2Index,
				TxtAddress = this.txtTileset2Address,
				CmbCompress = this.cmbTileset2ImageCompressType,
				CmbPaletteType = this.cmbTileset2PaletteType,
				TxtImageAddress = this.txtAddressTileset2Image,
				TxtPaletteAddress = this.txtAddressTileset2Palette,
				TxtBlockImageAddress = this.txtAddressTileset2BlockImage,
				TxtAnimationAddress = this.txtAddressTileset2Animation,
				TxtBehaviorAddress = this.txtAddressTileset2BlockBehavior
			};
			this.EnableDoubleBuffering(this.pnlTilesetPalette);
			this.EnableDoubleBuffering(this.pnlMapCanvas);
			this.EnableDoubleBuffering(this.pnlBorderDataPreview);
			this.EnableDoubleBuffering(this.pnlCollisionPalette);
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x0002DBDC File Offset: 0x0002BDDC
		private void InitializeResources()
		{
			string text = this.FindOptionalAsset("img", "MapCollision.png");
			bool flag = !string.IsNullOrEmpty(text) && File.Exists(text);
			if (flag)
			{
				this.collisionBitmap = new Bitmap(text);
			}
			string text2 = this.FindOptionalAsset("img", "EventIcon.png");
			bool flag2 = !string.IsNullOrEmpty(text2) && File.Exists(text2);
			if (flag2)
			{
				this.eventIconBitmap = new Bitmap(text2);
			}
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x0002DC40 File Offset: 0x0002BE40
		private void InitializeComboBoxes()
		{
			this.LoadFileToComboBox(this.FindRequiredAsset("txt", "MapTerrainType.txt"), this.cmbTerrainType);
			this.LoadFileToComboBox(this.FindRequiredAsset("txt", "MapWeatherType.txt"), this.cmbWeather);
			this.LoadFileToComboBox(this.FindRequiredAsset("txt", "MapBattleType.txt"), this.cmbBattleType);
			this.LoadFileToComboBox(this.FindRequiredAsset("txt", "MapNameType.txt"), this.cmbMapNameType);
			this.LoadFileToComboBox(this.FindRequiredAsset("txt", "MapScriptType.txt"), this.cmbMapScriptType);
			this.cmbSight.Items.Clear();
			this.cmbSight.Items.AddRange(new object[] { "[00]通常", "[01]暗闇, 解除可能", "[02]暗闇, 解除不可" });
			this.cmbBicycle.Items.Clear();
			this.cmbBicycle.Items.AddRange(new object[] { "[00]乗れない", "[01]乗れる" });
			this.cmbConnectedMapDirection.Items.Clear();
			this.cmbConnectedMapDirection.Items.AddRange(new object[] { "[00]無し", "[01]下", "[02]上", "[03]左", "[04]右", "[05]潜水", "[06]浮上" });
			string[] array = new string[] { "[00]未圧縮形式", "[01]圧縮形式" };
			string[] array2 = new string[] { "[00]パレット0-6", "[01]パレット7-12" };
			foreach (ComboBox comboBox in new ComboBox[] { this.cmbTileset1ImageCompressType, this.cmbTileset2ImageCompressType, this.cmbNewTilesetCompress })
			{
				comboBox.Items.Clear();
				comboBox.Items.AddRange(array);
			}
			foreach (ComboBox comboBox2 in new ComboBox[] { this.cmbTileset1PaletteType, this.cmbTileset2PaletteType })
			{
				comboBox2.Items.Clear();
				comboBox2.Items.AddRange(array2);
			}
			string[] array5 = new string[] { "[00]1, 128x320(固定) ", "[01]2, 128x192(可変)" };
			this.cmbNewTilesetType.Items.Clear();
			this.cmbNewTilesetType.Items.AddRange(array5);
			this.cmbMapNameId.BeginUpdate();
			this.cmbNewMapName.BeginUpdate();
			this.cmbMapNameId.Items.Clear();
			this.cmbNewMapName.Items.Clear();
			checked
			{
				int num = this.MAP_NAME_COUNT - 1;
				for (int k = 0; k <= num; k++)
				{
					int num2 = this.MAP_NAME_TABLE_OFFSET + k * 4;
					int num3 = (int)(BitConverter.ToUInt32(this.romData, num2) - 134217728U);
					string text2 = TextConverter.BytesToPokemonString(this.romData, num3, 16);
					this.cmbMapNameId.Items.Add(string.Format("[{0:X2}]{1}", this.MAP_NAME_FIRST_INDEX + k, text2));
					this.cmbNewMapName.Items.Add(text2);
				}
				this.cmbMapNameId.EndUpdate();
				this.cmbNewMapName.EndUpdate();
				this.cmbEventType.Items.Clear();
				this.cmbEventType.Items.AddRange(new object[] { "歩行グラフィック", "看板", "踏むスクリプト", "ワープ" });
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", "EventObjectLayer.txt"), this.cmbPersonLayer);
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", "EventObjectLayer.txt"), this.cmbWarpLayer);
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", "EventObjectLayer.txt"), this.cmbTrapScriptLayer);
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", "EventObjectLayer.txt"), this.cmbSignLayer);
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", "EventObjectAction.txt"), this.cmbPersonAction);
				this.LoadFileToComboBox(this.FindRequiredAsset("txt", "EventSignType.txt"), this.cmbSignType);
			}
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x0002E084 File Offset: 0x0002C284
		private void InitializeEventHandlers()
		{
			this.cmbTerrainType.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.cmbWeather.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.cmbSight.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.cmbBicycle.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.cmbBattleType.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.cmbMapNameId.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.cmbMapNameType.SelectedIndexChanged += this.OnMapHeaderControlChanged;
			this.nudTerrainId.ValueChanged += this.OnMapHeaderControlChanged;
			this.nudLevel.ValueChanged += this.OnMapHeaderControlChanged;
			this.nudMusicCode.ValueChanged += this.OnMapHeaderControlChanged;
			this.btnUndo.Click += this.btnUndo_Click;
			this.btnRedo.Click += this.btnRedo_Click;
			this.SetupTilesetHandlers(this.tileset1UI);
			this.SetupTilesetHandlers(this.tileset2UI);
			this.pnlMapCanvas.MouseMove += this.pnlMapCanvas_MouseMove;
			this.pnlMapCanvas.MouseLeave += this.pnlMapCanvas_MouseLeave;
			this.chkShowConnectedMap.CheckedChanged += this.OnConnectedMapUIChanged;
			this.nudConnectedMapIndex.ValueChanged += this.OnConnectedMapUIChanged;
			this.cmbConnectedMapDirection.SelectedIndexChanged += this.OnConnectedMapDataChanged;
			this.nudConnectedMapShift.ValueChanged += this.OnConnectedMapDataChanged;
			this.nudConnectedMapBank.ValueChanged += this.OnConnectedMapDataChanged;
			this.nudConnectedMapNumber.ValueChanged += this.OnConnectedMapDataChanged;
			this.chkShowOverWorld.CheckedChanged += delegate(object a0, EventArgs a1)
			{
				this.RefreshMapCanvas();
			};
			this.chkShowSign.CheckedChanged += delegate(object a0, EventArgs a1)
			{
				this.RefreshMapCanvas();
			};
			this.chkShowTrapScript.CheckedChanged += delegate(object a0, EventArgs a1)
			{
				this.RefreshMapCanvas();
			};
			this.chkShowWarp.CheckedChanged += delegate(object a0, EventArgs a1)
			{
				this.RefreshMapCanvas();
			};
			this.chkTerrainIdMode.CheckedChanged += this.chkLoadTerrainIdTable_CheckedChanged;
			this.SetupEventHandlers();
			this.SetupEventScriptPointerContextMenus();
			this.SetupMapLoadButtons();
			this.cmbNewTilesetType.SelectedIndexChanged += this.cmbNewTilesetType_SelectedIndexChanged;
			this.nudNewPaletteTilesetIndex.ValueChanged += this.nudNewPalette_ValueChanged;
			this.nudNewPaletteIndex.ValueChanged += this.nudNewPalette_ValueChanged;
			this.pnlPalettePreview.Paint += this.pnlPalettePreview_Paint;
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x0002E342 File Offset: 0x0002C542
		private void InitializeNumericUpDowns()
		{
			this.nudTerrainId.Maximum = new decimal(this.MAP_TERRAIN_ID_COUNT);
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0002E35C File Offset: 0x0002C55C
		private void SetupTilesetHandlers(MapEditor.TilesetUIContainer ui)
		{
			ui.RbIndex.CheckedChanged += delegate(object a0, EventArgs a1)
			{
				this.UpdateTilesetInputMode(ui);
			};
			ui.RbAddress.CheckedChanged += delegate(object a0, EventArgs a1)
			{
				this.UpdateTilesetInputMode(ui);
			};
			ui.NudIndex.ValueChanged += delegate(object a0, EventArgs a1)
			{
				this.SyncTilesetAddressFromIndex(ui, true);
			};
			ui.TxtAddress.TextChanged += delegate(object a0, EventArgs a1)
			{
				this.SyncTilesetAddressFromIndex(ui, false);
			};
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0002E3F4 File Offset: 0x0002C5F4
		private void LoadFileToComboBox(string filePath, ComboBox targetComboBox)
		{
			targetComboBox.Items.Clear();
			bool flag = File.Exists(filePath);
			if (flag)
			{
				targetComboBox.Items.AddRange(File.ReadAllLines(filePath, Encoding.UTF8));
			}
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0002E430 File Offset: 0x0002C630
		private string FindRequiredAsset(string folderName, string fileName)
		{
			return AppAssetLocator.FindRequiredFile(Path.Combine(folderName, fileName));
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0002E430 File Offset: 0x0002C630
		private string FindOptionalAsset(string folderName, string fileName)
		{
			try
			{
				return this.FindRequiredAsset(folderName, fileName);
			}
			catch (FileNotFoundException)
			{
			}
			return Path.Combine(AppContext.BaseDirectory, folderName, fileName);
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0002E430 File Offset: 0x0002C630
		private string FindWritableAssetPath(string folderName, string fileName)
		{
			string text = this.FindOptionalAsset(folderName, fileName);
			bool flag = File.Exists(text);
			if (flag)
			{
				return text;
			}
			string text2 = Path.Combine(AppContext.BaseDirectory, folderName);
			Directory.CreateDirectory(text2);
			return Path.Combine(text2, fileName);
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x0002E430 File Offset: 0x0002C630
		private void LoadTileset2BlockLimits()
		{
			this.tileset2BlockLimits.Clear();
			string text = this.FindOptionalAsset("ini", "Tileset2BlockLimit.ini");
			bool flag = !File.Exists(text);
			if (!flag)
			{
				foreach (string text2 in File.ReadAllLines(text))
				{
					string[] array2 = text2.Split(new char[] { '=' });
					int num = 0;
					int num2 = 0;
					bool flag2 = array2.Length == 2 && int.TryParse(array2[0].Trim(), out num) && int.TryParse(array2[1].Trim(), out num2);
					if (flag2)
					{
						this.tileset2BlockLimits[num] = num2;
					}
				}
			}
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x0002E4E8 File Offset: 0x0002C6E8
		private void ReadAllMapHeaders()
		{
			this.mapHeaders.Clear();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			string text = this.FindOptionalAsset("ini", "MapBankLimit.ini");
			bool flag = !File.Exists(text);
			checked
			{
				if (!flag)
				{
					foreach (string text2 in File.ReadAllLines(text))
					{ string trimText2 = text2.Trim();
						bool flag2 = string.IsNullOrEmpty(text2);
						if (!flag2)
						{
							string[] array2 = text2.Split(new char[] { '=' });
							bool flag3 = array2.Length == 2;
							if (flag3)
							{
								dictionary[int.Parse(array2[0].Trim())] = int.Parse(array2[1].Trim());
							}
						}
					}
					{
						foreach (KeyValuePair<int, int> keyValuePair in dictionary)
						{
							int key = keyValuePair.Key;
							int num = this.MAP_BANK_TABLE_OFFSET + key * 4;
							uint num2 = BitConverter.ToUInt32(this.romData, num);
							bool flag4 = unchecked((ulong)num2) == 0UL;
							if (!flag4)
							{
								int num3 = (int)(num2 - 134217728U);
								int num4 = keyValuePair.Value - 1;
								for (int j = 0; j <= num4; j++)
								{
									int num5 = num3 + j * 4;
									uint num6 = BitConverter.ToUInt32(this.romData, num5);
									bool flag5 = unchecked((ulong)num6) == 0UL;
									if (!flag5)
									{
										int num7 = (int)(num6 - 134217728U);
										MapEditor.MapHeader mapHeader = this.ReadMapHeader(key, j, num7);
										bool flag6 = mapHeader != null;
										if (flag6)
										{
											this.ReadConnections(mapHeader);
											this.ReadEvents(mapHeader);
											this.ReadMapScripts(mapHeader);
											this.mapHeaders.Add(mapHeader);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x0002E6E0 File Offset: 0x0002C8E0
		private MapEditor.MapHeader ReadMapHeader(int bank, int number, int offset)
		{
			checked
			{
				MapEditor.MapHeader mapHeader = new MapEditor.MapHeader
				{
					Bank = bank,
					Number = number,
					FooterAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 0)),
					EventScriptAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 4)),
					MapScriptAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 8)),
					ConnectionAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 12)),
					MusicCode = BitConverter.ToUInt16(this.romData, offset + 16),
					TerrainId = BitConverter.ToUInt16(this.romData, offset + 18),
					MapNameId = this.romData[offset + 20],
					Sight = this.romData[offset + 21],
					Weather = this.romData[offset + 22],
					TerrainType = this.romData[offset + 23],
					Bicycle = this.romData[offset + 24],
					MapNameType = this.romData[offset + 25],
					BattleType = this.romData[offset + 27]
				};
				byte b = this.romData[offset + 26];
				mapHeader.Level = ((b > 127) ? ((sbyte)((int)b - 256)) : ((sbyte)b));
				return mapHeader;
			}
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x0002E834 File Offset: 0x0002CA34
		private MapEditor.MapFooter ReadMapFooter(int offset)
		{
			return checked(new MapEditor.MapFooter
			{
				MapWidth = this.romData[offset + 0],
				MapHeight = this.romData[offset + 4],
				BorderDataAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 8)),
				MapDataAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 12)),
				Tileset1Address = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 16)),
				Tileset2Address = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 20)),
				BorderWidth = this.romData[offset + 24],
				BorderHeight = this.romData[offset + 25]
			});
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0002E8F8 File Offset: 0x0002CAF8
		private MapEditor.TilesetHeader ReadTilesetHeader(int offset)
		{
			MapEditor.TilesetHeader tilesetHeader = new MapEditor.TilesetHeader();
			bool flag = offset == 0;
			checked
			{
				MapEditor.TilesetHeader tilesetHeader2;
				if (flag)
				{
					tilesetHeader2 = tilesetHeader;
				}
				else
				{
					tilesetHeader.ImageCompressType = this.romData[offset + 0];
					tilesetHeader.PaletteType = this.romData[offset + 1];
					tilesetHeader.ImageAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 4));
					tilesetHeader.PaletteAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 8));
					tilesetHeader.BlockImageAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 12));
					tilesetHeader.AnimationAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 16));
					tilesetHeader.BlockBehaviorAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, offset + 20));
					tilesetHeader2 = tilesetHeader;
				}
				return tilesetHeader2;
			}
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x0002E9C8 File Offset: 0x0002CBC8
		private void ReadConnections(MapEditor.MapHeader header)
		{
			header.Connections = new List<MapEditor.ConnectedMap>();
			bool flag = (ulong)header.ConnectionAddress > 0UL;
			checked
			{
				if (flag)
				{
					byte b = this.romData[(int)(unchecked((ulong)header.ConnectionAddress) + 0UL)];
					uint num = BitConverter.ToUInt32(this.romData, (int)(unchecked((ulong)header.ConnectionAddress) + 4UL));
					int num2 = (int)this.PointerToOffset(num);
					bool flag2 = num2 != 0;
					if (flag2)
					{
						int num3 = (int)(b - 1);
						for (int i = 0; i <= num3; i++)
						{
							int num4 = num2 + i * 12;
							header.Connections.Add(new MapEditor.ConnectedMap
							{
								Direction = this.romData[num4 + 0],
								Shift = BitConverter.ToInt32(this.romData, num4 + 4),
								Bank = this.romData[num4 + 8],
								Number = this.romData[num4 + 9]
							});
						}
					}
				}
			}
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x0002EAB0 File Offset: 0x0002CCB0
		private void ReadEvents(MapEditor.MapHeader header)
		{
			header.Persons = new List<MapEditor.PersonEvent>();
			header.Warps = new List<MapEditor.WarpEvent>();
			header.Traps = new List<MapEditor.TrapEvent>();
			header.Signs = new List<MapEditor.SignEvent>();
			bool flag = (ulong)header.EventScriptAddress == 0UL;
			checked
			{
				if (!flag)
				{
					int num = (int)header.EventScriptAddress;
					byte b = this.romData[num + 0];
					byte b2 = this.romData[num + 1];
					byte b3 = this.romData[num + 2];
					byte b4 = this.romData[num + 3];
					int num2 = (int)this.PointerToOffset(BitConverter.ToUInt32(this.romData, num + 4));
					bool flag2 = num2 != 0;
					if (flag2)
					{
						int num3 = (int)(b - 1);
						for (int i = 0; i <= num3; i++)
						{
							int num4 = num2 + i * 24;
							byte b5 = this.romData[num4 + 10];
							header.Persons.Add(new MapEditor.PersonEvent
							{
								No = this.romData[num4 + 0],
								SpriteNo = this.romData[num4 + 1],
								UnknownB2Upper = this.romData[num4 + 2],
								UnknownB2Lower = this.romData[num4 + 3],
								X = BitConverter.ToUInt16(this.romData, num4 + 4),
								Y = BitConverter.ToUInt16(this.romData, num4 + 6),
								Layer = this.romData[num4 + 8],
								Action = this.romData[num4 + 9],
								MovementRangeX = (byte)(b5 & 15),
								MovementRangeY = (byte)(unchecked((byte)((uint)b5 >> 4)) & 15),
								UnknownB11 = this.romData[num4 + 11],
								Trainer = this.romData[num4 + 12],
								UnknownB13 = this.romData[num4 + 13],
								Sight = BitConverter.ToUInt16(this.romData, num4 + 14),
								ScriptAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, num4 + 16)),
								Flag = BitConverter.ToUInt16(this.romData, num4 + 20),
								UnknownB22 = BitConverter.ToUInt16(this.romData, num4 + 22)
							});
						}
					}
					int num5 = (int)this.PointerToOffset(BitConverter.ToUInt32(this.romData, num + 8));
					bool flag3 = num5 != 0;
					if (flag3)
					{
						int num6 = (int)(b2 - 1);
						for (int j = 0; j <= num6; j++)
						{
							int num7 = num5 + j * 8;
							header.Warps.Add(new MapEditor.WarpEvent
							{
								X = BitConverter.ToUInt16(this.romData, num7 + 0),
								Y = BitConverter.ToUInt16(this.romData, num7 + 2),
								Layer = this.romData[num7 + 4],
								WarpToNo = this.romData[num7 + 5],
								MapBank = this.romData[num7 + 7],
								MapNumber = this.romData[num7 + 6]
							});
						}
					}
					int num8 = (int)this.PointerToOffset(BitConverter.ToUInt32(this.romData, num + 12));
					bool flag4 = num8 != 0;
					if (flag4)
					{
						int num9 = (int)(b3 - 1);
						for (int k = 0; k <= num9; k++)
						{
							int num10 = num8 + k * 16;
							header.Traps.Add(new MapEditor.TrapEvent
							{
								X = BitConverter.ToUInt16(this.romData, num10 + 0),
								Y = BitConverter.ToUInt16(this.romData, num10 + 2),
								Layer = this.romData[num10 + 4],
								UnknownB5 = this.romData[num10 + 5],
								VarNumber = BitConverter.ToUInt16(this.romData, num10 + 6),
								VarValue = BitConverter.ToUInt16(this.romData, num10 + 8),
								UnknownB10 = BitConverter.ToUInt16(this.romData, num10 + 10),
								ScriptAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, num10 + 12))
							});
						}
					}
					int num11 = (int)this.PointerToOffset(BitConverter.ToUInt32(this.romData, num + 16));
					bool flag5 = num11 != 0;
					if (flag5)
					{
						int num12 = (int)(b4 - 1);
						for (int l = 0; l <= num12; l++)
						{
							int num13 = num11 + l * 12;
							header.Signs.Add(new MapEditor.SignEvent
							{
								X = BitConverter.ToUInt16(this.romData, num13 + 0),
								Y = BitConverter.ToUInt16(this.romData, num13 + 2),
								Layer = this.romData[num13 + 4],
								SignType = this.romData[num13 + 5],
								UnknownB6 = BitConverter.ToUInt16(this.romData, num13 + 6),
								ScriptAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, num13 + 8))
							});
						}
					}
				}
			}
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x0002EFA4 File Offset: 0x0002D1A4
		private void ReadMapScripts(MapEditor.MapHeader header)
		{
			header.MapScripts = new List<MapEditor.MapScriptEvent>();
			bool flag = (ulong)header.MapScriptAddress == 0UL;
			checked
			{
				if (!flag)
				{
					int num = (int)header.MapScriptAddress;
					for (;;)
					{
						byte b = this.romData[num];
						bool flag2 = b == 0;
						if (flag2)
						{
							break;
						}
						MapEditor.MapScriptEvent mapScriptEvent = new MapEditor.MapScriptEvent
						{
							Type = b
						};
						num++;
						uint num2 = BitConverter.ToUInt32(this.romData, num);
						mapScriptEvent.Pointer = this.PointerToOffset(num2);
						num += 4;
						bool flag3 = b == 2 || b == 4;
						if (flag3)
						{
							mapScriptEvent.ListEntries = new List<MapEditor.MapScriptListEntry>();
							bool flag4 = unchecked((ulong)mapScriptEvent.Pointer) > 0UL;
							if (flag4)
							{
								int num3 = (int)mapScriptEvent.Pointer;
								for (;;)
								{
									ushort num4 = BitConverter.ToUInt16(this.romData, num3);
									bool flag5 = num4 == 0;
									if (flag5)
									{
										break;
									}
									MapEditor.MapScriptListEntry mapScriptListEntry = new MapEditor.MapScriptListEntry
									{
										VarNumber = num4,
										VarValue = BitConverter.ToUInt16(this.romData, num3 + 2),
										ScriptAddress = this.PointerToOffset(BitConverter.ToUInt32(this.romData, num3 + 4))
									};
									mapScriptEvent.ListEntries.Add(mapScriptListEntry);
									num3 += 8;
								}
							}
						}
						header.MapScripts.Add(mapScriptEvent);
					}
				}
			}
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x0002F0F4 File Offset: 0x0002D2F4
		private MapEditor.MapCell[,] ReadMapDataMatrix(MapEditor.MapFooter footer)
		{
			bool flag = (ulong)footer.MapDataAddress == 0UL || footer.MapWidth == 0 || footer.MapHeight == 0;
			checked
			{
				MapEditor.MapCell[,] array;
				if (flag)
				{
					array = null;
				}
				else
				{
					int mapWidth = (int)footer.MapWidth;
					int mapHeight = (int)footer.MapHeight;
					int num = mapWidth * mapHeight * 2;
					byte[] array2 = new byte[num - 1 + 1];
					Array.Copy(this.romData, (int)footer.MapDataAddress, array2, 0, num);
					MapEditor.MapCell[,] array3 = new MapEditor.MapCell[mapWidth - 1 + 1, mapHeight - 1 + 1];
					int num2 = 0;
					int num3 = mapHeight - 1;
					for (int i = 0; i <= num3; i++)
					{
						int num4 = mapWidth - 1;
						for (int j = 0; j <= num4; j++)
						{
							ushort num5 = BitConverter.ToUInt16(array2, num2);
							array3[j, i].BlockIndex = (int)(num5 & 1023);
							array3[j, i].Collision = (num5 & 64512) >> 10;
							num2 += 2;
						}
					}
					array = array3;
				}
				return array;
			}
		}

		// Token: 0x060006ED RID: 1773 RVA: 0x0002F1F0 File Offset: 0x0002D3F0
		private int[,] ReadBorderDataMatrix(MapEditor.MapFooter footer)
		{
			bool flag = (ulong)footer.BorderDataAddress == 0UL || footer.BorderWidth == 0 || footer.BorderHeight == 0;
			checked
			{
				int[,] array;
				if (flag)
				{
					array = null;
				}
				else
				{
					int borderWidth = (int)footer.BorderWidth;
					int borderHeight = (int)footer.BorderHeight;
					int num = borderWidth * borderHeight * 2;
					byte[] array2 = new byte[num - 1 + 1];
					Array.Copy(this.romData, (int)footer.BorderDataAddress, array2, 0, num);
					int[,] array3 = new int[borderWidth - 1 + 1, borderHeight - 1 + 1];
					int num2 = 0;
					int num3 = borderHeight - 1;
					for (int i = 0; i <= num3; i++)
					{
						int num4 = borderWidth - 1;
						for (int j = 0; j <= num4; j++)
						{
							array3[j, i] = (int)(BitConverter.ToUInt16(array2, num2) & 1023);
							num2 += 2;
						}
					}
					array = array3;
				}
				return array;
			}
		}

		// Token: 0x060006EE RID: 1774 RVA: 0x0002F2C8 File Offset: 0x0002D4C8
		private MapEditor.MapCell[,] GetConnectedMapMatrix(int bank, int number, ref MapEditor.MapFooter outFooter)
		{
			MapEditor.MapHeader mapHeader = this.mapHeaders.FirstOrDefault((MapEditor.MapHeader h) => h.Bank == bank && h.Number == number);
			bool flag = mapHeader == null || (ulong)mapHeader.FooterAddress == 0UL;
			MapEditor.MapCell[,] array;
			if (flag)
			{
				array = null;
			}
			else
			{
				MapEditor.MapFooter mapFooter = this.ReadMapFooter(checked((int)mapHeader.FooterAddress));
				bool flag2 = (ulong)mapFooter.MapDataAddress == 0UL || mapFooter.MapWidth == 0 || mapFooter.MapHeight == 0;
				if (flag2)
				{
					array = null;
				}
				else
				{
					outFooter = mapFooter;
					array = this.ReadMapDataMatrix(mapFooter);
				}
			}
			return array;
		}

		// Token: 0x060006EF RID: 1775 RVA: 0x0002F360 File Offset: 0x0002D560
		private void WriteMapHeaderToRom()
		{
			checked
			{
				int num = this.MAP_BANK_TABLE_OFFSET + this.tempHeader.Bank * 4;
				int num2 = (int)(BitConverter.ToUInt32(this.romData, num) - 134217728U);
				int num3 = (int)(BitConverter.ToUInt32(this.romData, num2 + this.tempHeader.Number * 4) - 134217728U);
				this.WritePointerToRom(num3 + 0, this.tempHeader.FooterAddress);
				this.WritePointerToRom(num3 + 4, this.tempHeader.EventScriptAddress);
				this.WritePointerToRom(num3 + 8, this.tempHeader.MapScriptAddress);
				this.WritePointerToRom(num3 + 12, this.tempHeader.ConnectionAddress);
				Array.Copy(BitConverter.GetBytes(this.tempHeader.MusicCode), 0, this.romData, num3 + 16, 2);
				Array.Copy(BitConverter.GetBytes(this.tempHeader.TerrainId), 0, this.romData, num3 + 18, 2);
				this.romData[num3 + 20] = this.tempHeader.MapNameId;
				this.romData[num3 + 21] = this.tempHeader.Sight;
				this.romData[num3 + 22] = this.tempHeader.Weather;
				this.romData[num3 + 23] = this.tempHeader.TerrainType;
				this.romData[num3 + 24] = this.tempHeader.Bicycle;
				this.romData[num3 + 25] = this.tempHeader.MapNameType;
				this.romData[num3 + 26] = ((this.tempHeader.Level < 0) ? ((byte)(256 + (int)this.tempHeader.Level)) : ((byte)this.tempHeader.Level));
				this.romData[num3 + 27] = this.tempHeader.BattleType;
			}
		}

		// Token: 0x060006F0 RID: 1776 RVA: 0x0002F528 File Offset: 0x0002D728
		private void WriteConnectionsToRom()
		{
			bool flag = (ulong)this.tempHeader.ConnectionAddress == 0UL || this.tempHeader.Connections == null;
			checked
			{
				if (!flag)
				{
					int num = (int)this.tempHeader.ConnectionAddress;
					uint num2 = BitConverter.ToUInt32(this.romData, num + 4);
					Array.Clear(this.romData, num, 8);
					this.romData[num + 0] = (byte)this.tempHeader.Connections.Count;
					Array.Copy(BitConverter.GetBytes(num2), 0, this.romData, num + 4, 4);
					int num3 = (int)this.PointerToOffset(num2);
					bool flag2 = num3 != 0;
					if (flag2)
					{
						int num4 = this.tempHeader.Connections.Count - 1;
						for (int i = 0; i <= num4; i++)
						{
							int num5 = num3 + i * 12;
							MapEditor.ConnectedMap connectedMap = this.tempHeader.Connections[i];
							Array.Clear(this.romData, num5, 12);
							this.romData[num5 + 0] = connectedMap.Direction;
							Array.Copy(BitConverter.GetBytes(connectedMap.Shift), 0, this.romData, num5 + 4, 4);
							this.romData[num5 + 8] = connectedMap.Bank;
							this.romData[num5 + 9] = connectedMap.Number;
						}
					}
				}
			}
		}

		// Token: 0x060006F1 RID: 1777 RVA: 0x0002F680 File Offset: 0x0002D880
		private void WriteEventsToRom()
		{
			bool flag = (ulong)this.tempHeader.EventScriptAddress == 0UL;
			checked
			{
				if (!flag)
				{
					int num = (int)this.tempHeader.EventScriptAddress;
					int num2 = ((this.tempHeader.Persons != null) ? this.tempHeader.Persons.Count : 0);
					int num3 = ((this.tempHeader.Warps != null) ? this.tempHeader.Warps.Count : 0);
					int num4 = ((this.tempHeader.Traps != null) ? this.tempHeader.Traps.Count : 0);
					int num5 = ((this.tempHeader.Signs != null) ? this.tempHeader.Signs.Count : 0);
					this.romData[num + 0] = (byte)num2;
					this.romData[num + 1] = (byte)num3;
					this.romData[num + 2] = (byte)num4;
					this.romData[num + 3] = (byte)num5;
					uint num6 = BitConverter.ToUInt32(this.romData, num + 4);
					uint num7 = BitConverter.ToUInt32(this.romData, num + 8);
					uint num8 = BitConverter.ToUInt32(this.romData, num + 12);
					uint num9 = BitConverter.ToUInt32(this.romData, num + 16);
					bool flag2 = num2 > 0 && unchecked((ulong)this.PointerToOffset(num6)) > 0UL;
					if (flag2)
					{
						int num10 = (int)this.PointerToOffset(num6);
						int num11 = num2 - 1;
						for (int i = 0; i <= num11; i++)
						{
							int num12 = num10 + i * 24;
							MapEditor.PersonEvent personEvent = this.tempHeader.Persons[i];
							this.romData[num12 + 0] = personEvent.No;
							this.romData[num12 + 1] = personEvent.SpriteNo;
							this.romData[num12 + 2] = personEvent.UnknownB2Upper;
							this.romData[num12 + 3] = personEvent.UnknownB2Lower;
							Array.Copy(BitConverter.GetBytes(personEvent.X), 0, this.romData, num12 + 4, 2);
							Array.Copy(BitConverter.GetBytes(personEvent.Y), 0, this.romData, num12 + 6, 2);
							this.romData[num12 + 8] = personEvent.Layer;
							this.romData[num12 + 9] = personEvent.Action;
							this.romData[num12 + 10] = (byte)((int)(personEvent.MovementRangeX & 15) | ((int)(personEvent.MovementRangeY & 15) << 4));
							this.romData[num12 + 11] = personEvent.UnknownB11;
							this.romData[num12 + 12] = personEvent.Trainer;
							this.romData[num12 + 13] = personEvent.UnknownB13;
							Array.Copy(BitConverter.GetBytes(personEvent.Sight), 0, this.romData, num12 + 14, 2);
							this.WritePointerToRom(num12 + 16, personEvent.ScriptAddress);
							Array.Copy(BitConverter.GetBytes(personEvent.Flag), 0, this.romData, num12 + 20, 2);
							Array.Copy(BitConverter.GetBytes(personEvent.UnknownB22), 0, this.romData, num12 + 22, 2);
						}
					}
					bool flag3 = num3 > 0 && unchecked((ulong)this.PointerToOffset(num7)) > 0UL;
					if (flag3)
					{
						int num13 = (int)this.PointerToOffset(num7);
						int num14 = num3 - 1;
						for (int j = 0; j <= num14; j++)
						{
							int num15 = num13 + j * 8;
							MapEditor.WarpEvent warpEvent = this.tempHeader.Warps[j];
							Array.Copy(BitConverter.GetBytes(warpEvent.X), 0, this.romData, num15 + 0, 2);
							Array.Copy(BitConverter.GetBytes(warpEvent.Y), 0, this.romData, num15 + 2, 2);
							this.romData[num15 + 4] = warpEvent.Layer;
							this.romData[num15 + 5] = warpEvent.WarpToNo;
							this.romData[num15 + 7] = warpEvent.MapBank;
							this.romData[num15 + 6] = warpEvent.MapNumber;
						}
					}
					bool flag4 = num4 > 0 && unchecked((ulong)this.PointerToOffset(num8)) > 0UL;
					if (flag4)
					{
						int num16 = (int)this.PointerToOffset(num8);
						int num17 = num4 - 1;
						for (int k = 0; k <= num17; k++)
						{
							int num18 = num16 + k * 16;
							MapEditor.TrapEvent trapEvent = this.tempHeader.Traps[k];
							Array.Copy(BitConverter.GetBytes(trapEvent.X), 0, this.romData, num18 + 0, 2);
							Array.Copy(BitConverter.GetBytes(trapEvent.Y), 0, this.romData, num18 + 2, 2);
							this.romData[num18 + 4] = trapEvent.Layer;
							this.romData[num18 + 5] = trapEvent.UnknownB5;
							Array.Copy(BitConverter.GetBytes(trapEvent.VarNumber), 0, this.romData, num18 + 6, 2);
							Array.Copy(BitConverter.GetBytes(trapEvent.VarValue), 0, this.romData, num18 + 8, 2);
							Array.Copy(BitConverter.GetBytes(trapEvent.UnknownB10), 0, this.romData, num18 + 10, 2);
							this.WritePointerToRom(num18 + 12, trapEvent.ScriptAddress);
						}
					}
					bool flag5 = num5 > 0 && unchecked((ulong)this.PointerToOffset(num9)) > 0UL;
					if (flag5)
					{
						int num19 = (int)this.PointerToOffset(num9);
						int num20 = num5 - 1;
						for (int l = 0; l <= num20; l++)
						{
							int num21 = num19 + l * 12;
							MapEditor.SignEvent signEvent = this.tempHeader.Signs[l];
							Array.Copy(BitConverter.GetBytes(signEvent.X), 0, this.romData, num21 + 0, 2);
							Array.Copy(BitConverter.GetBytes(signEvent.Y), 0, this.romData, num21 + 2, 2);
							this.romData[num21 + 4] = signEvent.Layer;
							this.romData[num21 + 5] = signEvent.SignType;
							Array.Copy(BitConverter.GetBytes(signEvent.UnknownB6), 0, this.romData, num21 + 6, 2);
							this.WritePointerToRom(num21 + 8, signEvent.ScriptAddress);
						}
					}
				}
			}
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x0002FC98 File Offset: 0x0002DE98
		private void WriteMapScriptsToRom()
		{
			bool flag = (ulong)this.tempHeader.MapScriptAddress == 0UL || this.tempHeader.MapScripts == null;
			checked
			{
				if (!flag)
				{
					int num = (int)this.tempHeader.MapScriptAddress;
					{
						foreach (MapEditor.MapScriptEvent mapScriptEvent in this.tempHeader.MapScripts)
						{
							bool flag2 = mapScriptEvent.Type == 0;
							if (!flag2)
							{
								this.romData[num] = mapScriptEvent.Type;
								num++;
								this.WritePointerToRom(num, mapScriptEvent.Pointer);
								num += 4;
								bool flag3 = (mapScriptEvent.Type == 2 || mapScriptEvent.Type == 4) && unchecked((ulong)mapScriptEvent.Pointer) != 0UL && mapScriptEvent.ListEntries != null;
								if (flag3)
								{
									int num2 = (int)mapScriptEvent.Pointer;
									foreach (MapEditor.MapScriptListEntry mapScriptListEntry in mapScriptEvent.ListEntries)
									{
										Array.Copy(BitConverter.GetBytes(mapScriptListEntry.VarNumber), 0, this.romData, num2, 2);
										Array.Copy(BitConverter.GetBytes(mapScriptListEntry.VarValue), 0, this.romData, num2 + 2, 2);
										this.WritePointerToRom(num2 + 4, mapScriptListEntry.ScriptAddress);
										num2 += 8;
									}
									this.romData[num2] = 0;
									this.romData[num2 + 1] = 0;
								}
							}
						}
					}
					this.romData[num] = 0;
				}
			}
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x0002FE6C File Offset: 0x0002E06C
		private void WriteFooterToRom()
		{
			bool flag = (ulong)this.tempHeader.FooterAddress == 0UL || this.tempFooter == null;
			checked
			{
				if (!flag)
				{
					int num = (int)this.tempHeader.FooterAddress;
					this.romData[num + 0] = this.tempFooter.MapWidth;
					this.romData[num + 4] = this.tempFooter.MapHeight;
					this.WritePointerToRom(num + 8, this.tempFooter.BorderDataAddress);
					this.WritePointerToRom(num + 12, this.tempFooter.MapDataAddress);
					this.WritePointerToRom(num + 16, this.tempFooter.Tileset1Address);
					this.WritePointerToRom(num + 20, this.tempFooter.Tileset2Address);
					this.romData[num + 24] = this.tempFooter.BorderWidth;
					this.romData[num + 25] = this.tempFooter.BorderHeight;
				}
			}
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0002FF58 File Offset: 0x0002E158
		private void WriteTilesetToRom(int slot)
		{
			MapEditor.TilesetHeader tilesetHeader = ((slot == 1) ? this.tempTileset1 : this.tempTileset2);
			uint num = ((slot == 1) ? this.tempFooter.Tileset1Address : this.tempFooter.Tileset2Address);
			bool flag = (ulong)num == 0UL || tilesetHeader == null;
			checked
			{
				if (!flag)
				{
					int num2 = (int)num;
					this.romData[num2 + 0] = tilesetHeader.ImageCompressType;
					this.romData[num2 + 1] = tilesetHeader.PaletteType;
					this.WritePointerToRom(num2 + 4, tilesetHeader.ImageAddress);
					this.WritePointerToRom(num2 + 8, tilesetHeader.PaletteAddress);
					this.WritePointerToRom(num2 + 12, tilesetHeader.BlockImageAddress);
					this.WritePointerToRom(num2 + 16, tilesetHeader.AnimationAddress);
					this.WritePointerToRom(num2 + 20, tilesetHeader.BlockBehaviorAddress);
				}
			}
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x00030020 File Offset: 0x0002E220
		private void WriteMapDataMatrixToRom()
		{
			bool flag = this.mapMatrix == null || (ulong)this.tempFooter.MapDataAddress == 0UL;
			checked
			{
				if (!flag)
				{
					int num = (int)this.tempFooter.MapDataAddress;
					int length = this.mapMatrix.GetLength(0);
					int length2 = this.mapMatrix.GetLength(1);
					int num2 = length2 - 1;
					for (int i = 0; i <= num2; i++)
					{
						int num3 = length - 1;
						for (int j = 0; j <= num3; j++)
						{
							MapEditor.MapCell mapCell = this.mapMatrix[j, i];
							ushort num4 = (ushort)((mapCell.BlockIndex & 1023) | (mapCell.Collision << 10));
							byte[] bytes = BitConverter.GetBytes(num4);
							int num5 = num + (i * length + j) * 2;
							this.romData[num5] = bytes[0];
							this.romData[num5 + 1] = bytes[1];
						}
					}
				}
			}
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00030104 File Offset: 0x0002E304
		private void WriteBorderDataMatrixToRom()
		{
			bool flag = this.borderMatrix == null || (ulong)this.tempFooter.BorderDataAddress == 0UL;
			checked
			{
				if (!flag)
				{
					int num = (int)this.tempFooter.BorderDataAddress;
					int length = this.borderMatrix.GetLength(0);
					int length2 = this.borderMatrix.GetLength(1);
					int num2 = length2 - 1;
					for (int i = 0; i <= num2; i++)
					{
						int num3 = length - 1;
						for (int j = 0; j <= num3; j++)
						{
							byte[] bytes = BitConverter.GetBytes((ushort)(this.borderMatrix[j, i] & 1023));
							int num4 = num + (i * length + j) * 2;
							this.romData[num4] = bytes[0];
							this.romData[num4 + 1] = bytes[1];
						}
					}
				}
			}
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x000301CF File Offset: 0x0002E3CF
		private void WritePointerToRom(int offset, uint address)
		{
			Array.Copy(BitConverter.GetBytes(((ulong)address != 0UL) ? checked(address + 134217728U) : 0U), 0, this.romData, offset, 4);
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x000301F4 File Offset: 0x0002E3F4
		private void RefreshMapTree()
		{
			this.tvwMapSelector.BeginUpdate();
			this.tvwMapSelector.Nodes.Clear();
			bool @checked = this.rbMapSortIndex.Checked;
			checked
			{
				if (@checked)
				{
					IOrderedEnumerable<IGrouping<int, MapEditor.MapHeader>> orderedEnumerable = this.mapHeaders.GroupBy((MapEditor.MapHeader h) => h.Bank).OrderBy((IGrouping<int, MapEditor.MapHeader> g) => g.Key);
					try
					{
						foreach (IGrouping<int, MapEditor.MapHeader> grouping in orderedEnumerable)
						{
							TreeNode treeNode = new TreeNode(string.Format("バンク {0}", grouping.Key))
							{
								Tag = new
								{
									Bank = grouping.Key
								}
							};
							try
							{
								foreach (MapEditor.MapHeader mapHeader in grouping)
								{
									treeNode.Nodes.Add(new TreeNode(string.Format("({0}, {1}) {2}", mapHeader.Bank, mapHeader.Number, mapHeader.GetMapName(this)))
									{
										Tag = mapHeader
									});
								}
							}
							finally
							{
							}
							this.tvwMapSelector.Nodes.Add(treeNode);
						}
					}
					finally
					{
					}
				}
				else
				{
					bool checked2 = this.rbMapSortName.Checked;
					if (checked2)
					{
						int num = this.MAP_NAME_COUNT - 1;
						for (int i = 0; i <= num; i++)
						{
							int currentMapId = this.MAP_NAME_FIRST_INDEX + i;
							List<MapEditor.MapHeader> list = this.mapHeaders.Where((MapEditor.MapHeader h) => (int)h.MapNameId == currentMapId).ToList<MapEditor.MapHeader>();
							TreeNode treeNode2 = new TreeNode(this.GetMapNameLabelById((byte)currentMapId))
							{
								Tag = new
								{
									MapNameId = currentMapId
								}
							};
							{
								foreach (MapEditor.MapHeader mapHeader2 in list)
								{
									treeNode2.Nodes.Add(new TreeNode(string.Format("({0}, {1}) {2}", mapHeader2.Bank, mapHeader2.Number, mapHeader2.GetMapName(this)))
									{
										Tag = mapHeader2
									});
								}
							}
							this.tvwMapSelector.Nodes.Add(treeNode2);
						}
					}
				}
				this.tvwMapSelector.EndUpdate();
			}
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x000304E0 File Offset: 0x0002E6E0
		private string GetMapNameLabelById(byte mapNameId)
		{
			string text = string.Format("[{0:X2}]", mapNameId);
			try
			{
				foreach (object obj in this.cmbMapNameId.Items)
				{
					object objectValue = RuntimeHelpers.GetObjectValue(obj);
					bool flag = objectValue.ToString().StartsWith(text);
					if (flag)
					{
						return objectValue.ToString();
					}
				}
			}
			finally
			{
			}
			return text;
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x00030574 File Offset: 0x0002E774
		private void RefreshEditorView(MapEditor.ViewUpdateLevel level)
		{
			bool flag = this.isUpdatingUI;
			if (!flag)
			{
				this.isUpdatingUI = true;
				try
				{
					switch (level)
					{
					case MapEditor.ViewUpdateLevel.HeaderOnly:
						this.BindHeaderToUI();
						break;
					case MapEditor.ViewUpdateLevel.FooterAndGraphics:
					{
						this.BindHeaderToUI();
						bool flag2 = (ulong)this.tempHeader.FooterAddress > 0UL;
						if (flag2)
						{
							this.LoadFooterAndContent(this.tempHeader.FooterAddress);
						}
						else
						{
							this.ClearFooterAndGraphics();
						}
						break;
					}
					case MapEditor.ViewUpdateLevel.GraphicsOnly:
						this.UpdateAllGraphics();
						break;
					}
					this.RefreshMapCanvas();
				}
				finally
				{
					this.isUpdatingUI = false;
				}
			}
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x00030628 File Offset: 0x0002E828
		private void ClearFooterAndGraphics()
		{
			this.ResetControlsInContainer(this.grpMapFooter);
			this.ResetControlsInContainer(this.grpTilesetDetail);
			this.ClearMapContent();
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x0003064C File Offset: 0x0002E84C
		private void UpdateAllGraphics()
		{
			this.GenerateTilesetPalette();
			this.UpdatePrimaryMapLayer();
			this.UpdateMapRender();
			this.UpdateBorderRender();
			this.UpdateMapScrollBars();
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x00030672 File Offset: 0x0002E872
		private void RefreshMapCanvas()
		{
			this.pnlMapCanvas.Invalidate();
			this.pnlTilesetPalette.Invalidate();
			this.pnlBorderDataPreview.Invalidate();
			this.pnlCollisionPalette.Invalidate();
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x000306A8 File Offset: 0x0002E8A8
		private void BindHeaderToUI()
		{
			this.lblCurrentMap.Text = string.Format("現在マップ : ({0}, {1}) {2}", this.tempHeader.Bank, this.tempHeader.Number, this.tempHeader.GetMapName(this));
			this.SetMapHeaderControlsEnabled(true);
			this.txtAddressMapFooter.Text = string.Format("{0:X8}", this.tempHeader.FooterAddress);
			this.txtAddressEventScript.Text = string.Format("{0:X8}", this.tempHeader.EventScriptAddress);
			this.txtAddressMapScript.Text = string.Format("{0:X8}", this.tempHeader.MapScriptAddress);
			this.txtAddressMapConnection.Text = string.Format("{0:X8}", this.tempHeader.ConnectionAddress);
			this.nudMusicCode.Value = new decimal((int)this.tempHeader.MusicCode);
			this.nudTerrainId.Value = new decimal((int)this.tempHeader.TerrainId);
			this.nudLevel.Value = new decimal((int)this.tempHeader.Level);
			this.SelectComboBoxByValue(this.cmbMapNameId, string.Format("[{0:X2}]", this.tempHeader.MapNameId));
			this.SelectComboBoxByValue(this.cmbSight, string.Format("[{0:X2}]", this.tempHeader.Sight));
			this.SelectComboBoxByValue(this.cmbWeather, string.Format("[{0:X2}]", this.tempHeader.Weather));
			this.SelectComboBoxByValue(this.cmbTerrainType, string.Format("[{0:X2}]", this.tempHeader.TerrainType));
			this.SelectComboBoxByValue(this.cmbBicycle, string.Format("[{0:X2}]", this.tempHeader.Bicycle));
			this.SelectComboBoxByValue(this.cmbMapNameType, string.Format("[{0:X2}]", this.tempHeader.MapNameType));
			this.SelectComboBoxByValue(this.cmbBattleType, string.Format("[{0:X2}]", this.tempHeader.BattleType));
			bool flag = (ulong)this.tempHeader.FooterAddress > 0UL;
			this.grpMapHeader.Enabled = true;
			this.grpMapFooter.Enabled = flag;
			this.grpTilesetDetail.Enabled = flag;
			this.RefreshConnectionUI();
			this.RefreshEventUI();
			bool flag2 = (ulong)this.tempHeader.MapScriptAddress == 0UL;
			if (flag2)
			{
				this.ResetControlsInContainer(this.grpEditMapScript);
				this.grpEditMapScript.Enabled = false;
			}
			else
			{
				this.grpEditMapScript.Enabled = true;
				this.RefreshMapScriptUI();
			}
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0003098C File Offset: 0x0002EB8C
		private void LoadFooterAndContent(uint footerAddr)
		{
			this.originalFooter = this.ReadMapFooter(checked((int)footerAddr));
			this.tempFooter = this.originalFooter.Clone();
			this.nudMapWidth.Value = new decimal((int)this.tempFooter.MapWidth);
			this.nudMapHeight.Value = new decimal((int)this.tempFooter.MapHeight);
			this.txtBorderDataAddress.Text = string.Format("{0:X8}", this.tempFooter.BorderDataAddress);
			this.txtMapDataAddress.Text = string.Format("{0:X8}", this.tempFooter.MapDataAddress);
			this.txtTileset1Address.Text = string.Format("{0:X8}", this.tempFooter.Tileset1Address);
			this.txtTileset2Address.Text = string.Format("{0:X8}", this.tempFooter.Tileset2Address);
			this.nudBorderWidth.Value = new decimal((int)this.tempFooter.BorderWidth);
			this.nudBorderHeight.Value = new decimal((int)this.tempFooter.BorderHeight);
			this.LoadTileset(1, this.tempFooter.Tileset1Address);
			this.LoadTileset(2, this.tempFooter.Tileset2Address);
			this.mapMatrix = this.ReadMapDataMatrix(this.tempFooter);
			this.borderMatrix = this.ReadBorderDataMatrix(this.tempFooter);
			this.ClearMapEditHistory();
			this.UpdateAllGraphics();
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x00030B14 File Offset: 0x0002ED14
		private void LoadTileset(int slot, uint addr)
		{
			MapEditor.TilesetHeader tilesetHeader = this.ReadTilesetHeader(checked((int)addr));
			MapEditor.TilesetUIContainer tilesetUIContainer = ((slot == 1) ? this.tileset1UI : this.tileset2UI);
			bool flag = slot == 1;
			if (flag)
			{
				this.originalTileset1 = tilesetHeader;
				this.tempTileset1 = tilesetHeader.Clone();
			}
			else
			{
				this.originalTileset2 = tilesetHeader;
				this.tempTileset2 = tilesetHeader.Clone();
			}
			tilesetUIContainer.IsUpdating = true;
			tilesetUIContainer.TxtAddress.Text = string.Format("{0:X8}", addr);
			tilesetUIContainer.NudIndex.Value = new decimal(this.AddressToTilesetIndex(addr));
			tilesetUIContainer.RbIndex.Checked = true;
			this.SelectComboBoxByValue(tilesetUIContainer.CmbCompress, string.Format("[{0:X2}]", tilesetHeader.ImageCompressType));
			this.SelectComboBoxByValue(tilesetUIContainer.CmbPaletteType, string.Format("[{0:X2}]", tilesetHeader.PaletteType));
			tilesetUIContainer.TxtImageAddress.Text = string.Format("{0:X8}", tilesetHeader.ImageAddress);
			tilesetUIContainer.TxtPaletteAddress.Text = string.Format("{0:X8}", tilesetHeader.PaletteAddress);
			tilesetUIContainer.TxtBlockImageAddress.Text = string.Format("{0:X8}", tilesetHeader.BlockImageAddress);
			tilesetUIContainer.TxtAnimationAddress.Text = string.Format("{0:X8}", tilesetHeader.AnimationAddress);
			tilesetUIContainer.TxtBehaviorAddress.Text = string.Format("{0:X8}", tilesetHeader.BlockBehaviorAddress);
			tilesetUIContainer.NudIndex.Enabled = true;
			tilesetUIContainer.TxtAddress.Enabled = false;
			tilesetUIContainer.IsUpdating = false;
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x00030CC4 File Offset: 0x0002EEC4
		private void ClearMapContent()
		{
			this.mapMatrix = null;
			this.borderMatrix = null;
			this.ClearMapEditHistory();
			bool flag = this.mapBitmap != null;
			if (flag)
			{
				this.mapBitmap.Dispose();
				this.mapBitmap = null;
			}
			bool flag2 = this.borderBitmap != null;
			if (flag2)
			{
				this.borderBitmap.Dispose();
				this.borderBitmap = null;
			}
			bool flag3 = this.blockPaletteBitmap != null;
			if (flag3)
			{
				this.blockPaletteBitmap.Dispose();
				this.blockPaletteBitmap = null;
			}
			bool flag4 = this.primaryMapLayerBitmap != null;
			if (flag4)
			{
				this.primaryMapLayerBitmap.Dispose();
				this.primaryMapLayerBitmap = null;
			}
			bool flag5 = this.connectedMapLayerBitmap != null;
			if (flag5)
			{
				this.connectedMapLayerBitmap.Dispose();
				this.connectedMapLayerBitmap = null;
			}
			this.cachedConnBank = -1;
			this.cachedConnNumber = -1;
			this.cachedConnMatrix = null;
			this.cachedConnFooter = null;
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x00030D9E File Offset: 0x0002EF9E
		private void ResetEditorState()
		{
			this.tempHeader = null;
			this.originalHeader = null;
			this.ClearAllControls();
			this.lblCurrentMap.Text = "現在マップ :";
			this.ClearMapContent();
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x00030DD0 File Offset: 0x0002EFD0
		private void ClearAllControls()
		{
			this.isUpdatingUI = true;
			this.ResetControlsInContainer(this.grpMapHeader);
			this.ResetControlsInContainer(this.grpMapFooter);
			this.ResetControlsInContainer(this.grpTilesetDetail);
			this.ResetControlsInContainer(this.grpEditMapConnection);
			this.ResetControlsInContainer(this.grpEditMapScript);
			this.grpMapHeader.Enabled = false;
			this.grpMapFooter.Enabled = false;
			this.grpTilesetDetail.Enabled = false;
			this.grpEditMapConnection.Enabled = false;
			this.grpEditMapScript.Enabled = false;
			this.chkShowConnectedMap.Checked = false;
			this.chkShowConnectedMap.Enabled = false;
			this.btnOpenBlockEditor.Enabled = false;
			this.nudEventNo.Enabled = false;
			this.ResetAndDisableGroup(this.grpPersonEvent);
			this.ResetAndDisableGroup(this.grpWarpEvent);
			this.ResetAndDisableGroup(this.grpTrapScriptEvent);
			this.ResetAndDisableGroup(this.grpSignEvent);
			this.grpPersonEvent.Visible = false;
			this.grpWarpEvent.Visible = false;
			this.grpTrapScriptEvent.Visible = false;
			this.grpSignEvent.Visible = false;
			this.ClearMapContent();
			this.RefreshMapCanvas();
			this.UpdateBlockIndexLabel();
			this.UpdateLoadMapButtons();
			this.isUpdatingUI = false;
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x00030F28 File Offset: 0x0002F128
		private void SetMapHeaderControlsEnabled(bool enabled)
		{
			try
			{
				foreach (object obj in this.grpMapHeader.Controls)
				{
					Control control = (Control)obj;
					bool flag = control != this.grpMapHeaderAddress;
					if (flag)
					{
						control.Enabled = enabled;
					}
				}
			}
			finally
			{
			}
			try
			{
				foreach (object obj2 in this.grpMapHeaderAddress.Controls)
				{
					Control control2 = (Control)obj2;
					bool flag2 = control2 != this.txtAddressMapFooter && control2 != this.lblAddressMapFooter && control2 != this.btnUpdateMapHeaderAddresses;
					if (flag2)
					{
						control2.Enabled = enabled;
					}
				}
			}
			finally
			{
			}
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x0003102C File Offset: 0x0002F22C
		private void UpdateMapScrollBars()
		{
			bool flag = this.mapBitmap == null;
			checked
			{
				if (flag)
				{
					this.hsbMapDataPreview.Enabled = false;
					this.vsbMapDataPreview.Enabled = false;
				}
				else
				{
					int num = this.GetMapZoomScale();
					this.hsbMapDataPreview.SmallChange = 16 * num;
					this.vsbMapDataPreview.SmallChange = 16 * num;
					bool flag2 = this.mapBitmap.Width * num > this.pnlMapCanvas.Width;
					if (flag2)
					{
						this.hsbMapDataPreview.Enabled = true;
						this.hsbMapDataPreview.Minimum = 0;
						this.hsbMapDataPreview.LargeChange = this.pnlMapCanvas.Width;
						this.hsbMapDataPreview.Maximum = this.mapBitmap.Width * num - 1;
						bool flag3 = this.hsbMapDataPreview.Value > this.hsbMapDataPreview.Maximum - this.hsbMapDataPreview.LargeChange + 1;
						if (flag3)
						{
							this.hsbMapDataPreview.Value = Math.Max(0, this.hsbMapDataPreview.Maximum - this.hsbMapDataPreview.LargeChange + 1);
						}
					}
					else
					{
						this.hsbMapDataPreview.Enabled = false;
						this.hsbMapDataPreview.Value = 0;
					}
					bool flag4 = this.mapBitmap.Height * num > this.pnlMapCanvas.Height;
					if (flag4)
					{
						this.vsbMapDataPreview.Enabled = true;
						this.vsbMapDataPreview.Minimum = 0;
						this.vsbMapDataPreview.LargeChange = this.pnlMapCanvas.Height;
						this.vsbMapDataPreview.Maximum = this.mapBitmap.Height * num - 1;
						bool flag5 = this.vsbMapDataPreview.Value > this.vsbMapDataPreview.Maximum - this.vsbMapDataPreview.LargeChange + 1;
						if (flag5)
						{
							this.vsbMapDataPreview.Value = Math.Max(0, this.vsbMapDataPreview.Maximum - this.vsbMapDataPreview.LargeChange + 1);
						}
					}
					else
					{
						this.vsbMapDataPreview.Enabled = false;
						this.vsbMapDataPreview.Value = 0;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マップ表示倍率を取得する処理
		//-------------------------------------------------------------------------------
		private int GetMapZoomScale()
		{
			return Math.Min(8, Math.Max(1, this.mapZoomScale));
		}

		//-------------------------------------------------------------------------------
		// マウス位置を基準にマップ表示倍率を変更する処理
		//-------------------------------------------------------------------------------
		private void SetMapZoomScale(int zoomScale, Point focalPoint)
		{
			bool flag = this.mapBitmap == null;
			if (!flag)
			{
				int mapZoomScale = this.GetMapZoomScale();
				int num = Math.Min(8, Math.Max(1, zoomScale));
				bool flag2 = mapZoomScale == num;
				if (!flag2)
				{
					int num2 = this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0;
					int num3 = this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0;
					int num4 = (focalPoint.X + num2) / mapZoomScale;
					int num5 = (focalPoint.Y + num3) / mapZoomScale;
					this.mapZoomScale = num;
					this.UpdateMapScrollBars();
					this.SetScrollBarValue(this.hsbMapDataPreview, num4 * num - focalPoint.X);
					this.SetScrollBarValue(this.vsbMapDataPreview, num5 * num - focalPoint.Y);
					this.pnlMapCanvas.Invalidate();
				}
			}
		}

		//-------------------------------------------------------------------------------
		// スクロールバーの有効範囲内に値を設定する処理
		//-------------------------------------------------------------------------------
		private void SetScrollBarValue(ScrollBar scrollBar, int value)
		{
			bool flag = scrollBar == null || !scrollBar.Enabled;
			if (flag)
			{
				return;
			}
			int val = Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
			scrollBar.Value = Math.Min(Math.Max(0, value), val);
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x00031264 File Offset: 0x0002F464
		private void UpdateBlockIndexLabel()
		{
			bool flag = this.blockPaletteBitmap == null;
			if (flag)
			{
				this.lblBlockIndex.Text = "ブロックID :";
			}
			else
			{
				int columns = Math.Max(1, this.blockPaletteBitmap.Width / 16);
				int num = checked(this.selectedBlockRect.Y * columns + this.selectedBlockRect.X);
				this.lblBlockIndex.Text = string.Format("ブロックID : {0:D4} (0x{1:X4})", num, num);
			}
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x000312CF File Offset: 0x0002F4CF
		private void ResetMapPositionLabels()
		{
			this.lblMapPositionX.Text = "X :";
			this.lblMapPositionY.Text = "Y :";
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x000312F4 File Offset: 0x0002F4F4
		private void SelectComboBoxByValue(ComboBox cmb, string valuePrefix)
		{
			checked
			{
				int num = cmb.Items.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					bool flag = cmb.Items[i].ToString().StartsWith(valuePrefix);
					if (flag)
					{
						cmb.SelectedIndex = i;
						return;
					}
				}
				cmb.SelectedIndex = -1;
			}
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x0003134C File Offset: 0x0002F54C
		private byte GetByteFromCombo(ComboBox cmb)
		{
			bool flag = cmb.SelectedIndex == -1;
			byte b;
			if (flag)
			{
				b = 0;
			}
			else
			{
				string text = cmb.SelectedItem.ToString();
				bool flag2 = text.StartsWith("[") && text.Length >= 4;
				if (flag2)
				{
					b = Convert.ToByte(text.Substring(1, 2), 16);
				}
				else
				{
					b = 0;
				}
			}
			return b;
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x000313B0 File Offset: 0x0002F5B0
		private string FormatHexTo8Digits(string input)
		{
			uint num = 0U;
			return this.TryParseHex(input, ref num) ? string.Format("{0:X8}", num) : (string.IsNullOrWhiteSpace(input) ? "00000000" : input.Trim().PadLeft(8, '0'));
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x000313E0 File Offset: 0x0002F5E0
		private uint ParseHex8(string text)
		{
			uint num = 0;
			this.TryParseHex(text, ref num);
			return num;
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x00031400 File Offset: 0x0002F600
		private ushort ParseHex4(string text)
		{
			bool flag = string.IsNullOrWhiteSpace(text);
			ushort num = 0;
			if (flag)
			{
				num = 0;
			}
			else
			{
				ushort num2 = 0;
				bool flag2 = ushort.TryParse(text.Trim(), NumberStyles.HexNumber, null, out num2);
				if (flag2)
				{
					num = num2;
				}
				else
				{
					num = 0;
				}
			}
			return num;
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x0003143C File Offset: 0x0002F63C
		private bool TryParseHex(string input, ref uint result)
		{
			bool flag = string.IsNullOrWhiteSpace(input);
			bool flag2;
			if (flag)
			{
				result = 0U;
				flag2 = true;
			}
			else
			{
				string text = input.Trim();
				bool flag3 = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
				if (flag3)
				{
					text = text.Substring(2);
				}
				uint num = 0U;
				flag2 = uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num);
				bool flag4 = flag2;
				if (flag4)
				{
					result = this.NormalizeRomAddress(num);
				}
			}
			return flag2;
		}

		private uint NormalizeRomAddress(uint address)
		{
			return (address >= 134217728U) ? checked(address - 134217728U) : address;
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x00031474 File Offset: 0x0002F674
		private void ResetControlsInContainer(Control container)
		{
			try
			{
				foreach (object obj in container.Controls)
				{
					Control control = (Control)obj;
					bool flag = control is TextBox;
					if (flag)
					{
						control.Text = "";
					}
					else
					{
						bool flag2 = control is NumericUpDown;
						if (flag2)
						{
							((NumericUpDown)control).Value = 0m;
						}
						else
						{
							bool flag3 = control is ComboBox;
							if (flag3)
							{
								((ComboBox)control).SelectedIndex = -1;
							}
							else
							{
								bool flag4 = control is CheckBox;
								if (flag4)
								{
									((CheckBox)control).Checked = false;
								}
								else
								{
									bool hasChildren = control.HasChildren;
									if (hasChildren)
									{
										this.ResetControlsInContainer(control);
									}
								}
							}
						}
					}
				}
			}
			finally
			{
			}
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x00031568 File Offset: 0x0002F768
		private void ResetAndDisableGroup(GroupBox grp)
		{
			grp.Enabled = false;
			try
			{
				foreach (object obj in grp.Controls)
				{
					Control control = (Control)obj;
					bool flag = control is NumericUpDown;
					if (flag)
					{
						((NumericUpDown)control).Value = 0m;
					}
					else
					{
						bool flag2 = control is TextBox;
						if (flag2)
						{
							bool flag3 = control.Name.Contains("Flag") || control.Name.Contains("VarNumber") || control.Name.Contains("VarValue");
							if (flag3)
							{
								control.Text = "0000";
							}
							else
							{
								bool flag4 = control.Name.Contains("Address");
								if (flag4)
								{
									control.Text = "00000000";
								}
								else
								{
									control.Text = "0";
								}
							}
						}
						else
						{
							bool flag5 = control is ComboBox;
							if (flag5)
							{
								((ComboBox)control).SelectedIndex = -1;
							}
							else
							{
								bool flag6 = control is CheckBox;
								if (flag6)
								{
									((CheckBox)control).Checked = false;
								}
							}
						}
					}
				}
			}
			finally
			{
			}
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x000316D0 File Offset: 0x0002F8D0
		private void SetUnsavedChanges(bool changed)
		{
			this.hasUnsavedChanges = changed;
			this.btnSave.Enabled = changed;
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x000316E8 File Offset: 0x0002F8E8
		private bool ConfirmSaveIfNeeded()
		{
			bool flag = !this.hasUnsavedChanges;
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Cancel)
				{
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult != DialogResult.No)
						{
							flag2 = true;
						}
						else
						{
							this.SetUnsavedChanges(false);
							flag2 = true;
						}
					}
					else
					{
						this.btnSave_Click(null, null);
						flag2 = true;
					}
				}
				else
				{
					flag2 = false;
				}
			}
			return flag2;
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x00031754 File Offset: 0x0002F954
		private Bitmap CreateTilesetBitmap(MapEditor.TilesetHeader ts1, MapEditor.TilesetHeader ts2, int ts2Index, ref int outTotalBlocks)
		{
			bool flag = ts1 == null || ts2 == null;
			checked
			{
				Bitmap bitmap;
				if (flag)
				{
					bitmap = null;
				}
				else
				{
					byte[] array = this.LoadTilesetRawImage(ts1);
					byte[] array2 = this.LoadTilesetRawImage(ts2);
					byte[] array3 = new byte[array.Length + array2.Length - 1 + 1];
					Array.Copy(array, 0, array3, 0, array.Length);
					Array.Copy(array2, 0, array3, array.Length, array2.Length);
					Color[] array4 = this.LoadAllPalettes(ts1, ts2);
					int num = (this.tileset2BlockLimits.ContainsKey(ts2Index) ? this.tileset2BlockLimits[ts2Index] : 384);
					byte[] array5 = this.LoadBlockData(ts1, 640);
					byte[] array6 = this.LoadBlockData(ts2, num);
					int num2 = array5.Length / 16;
					int num3 = array6.Length / 16;
					outTotalBlocks = num2 + num3;
					int num4 = 128;
					int num5 = (int)Math.Round(unchecked(Math.Ceiling((double)outTotalBlocks / 8.0) * 16.0));
					bool flag2 = num5 <= 0;
					if (flag2)
					{
						num5 = 16;
					}
					Bitmap bitmap2 = new Bitmap(num4, num5);
					using (Graphics graphics = Graphics.FromImage(bitmap2))
					{
						this.DrawBlockBatch(graphics, array5, array3, array4, 0);
						this.DrawBlockBatch(graphics, array6, array3, array4, num2);
					}
					bitmap = bitmap2;
				}
				return bitmap;
			}
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x000318B0 File Offset: 0x0002FAB0
		private byte[] LoadTilesetRawImage(MapEditor.TilesetHeader ts)
		{
			bool flag = (ulong)ts.ImageAddress == 0UL;
			byte[] array;
			if (flag)
			{
				array = new byte[0];
			}
			else
			{
				bool flag2 = ts.ImageCompressType == 1;
				if (flag2)
				{
					array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, ts.ImageAddress, false);
				}
				else
				{
					byte[] array2 = new byte[32768];
					Array.Copy(this.romData, checked((int)ts.ImageAddress), array2, 0, 32768);
					array = array2;
				}
			}
			return array;
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x00031924 File Offset: 0x0002FB24
		private Color[] LoadAllPalettes(MapEditor.TilesetHeader ts1, MapEditor.TilesetHeader ts2)
		{
			Color[] array = new Color[208];
			bool flag = (ulong)ts1.PaletteAddress > 0UL;
			if (flag)
			{
				byte[] array2 = new byte[224];
				Array.Copy(this.romData, (long)((ulong)ts1.PaletteAddress), array2, 0L, (long)array2.Length);
				int num = 0;
				checked
				{
					do
					{
						byte[] array3 = new byte[32];
						Array.Copy(array2, num * 32, array3, 0, 32);
						Array.Copy(ImageProcessor.LoadPalette(array3, true), 0, array, num * 16, 16);
						num++;
					}
					while (num <= 6);
				}
			}
			bool flag2 = (ulong)ts2.PaletteAddress > 0UL;
			if (flag2)
			{
				int num2 = 224;
				int num3 = 6;
				byte[] array4 = new byte[checked(num3 * 32 - 1 + 1)];
				Array.Copy(this.romData, (long)((ulong)(checked(ts2.PaletteAddress + (uint)num2))), array4, 0L, (long)array4.Length);
				checked
				{
					int num4 = num3 - 1;
					for (int i = 0; i <= num4; i++)
					{
						byte[] array5 = new byte[32];
						Array.Copy(array4, i * 32, array5, 0, 32);
						Array.Copy(ImageProcessor.LoadPalette(array5, true), 0, array, (7 + i) * 16, 16);
					}
				}
			}
			return array;
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x00031A54 File Offset: 0x0002FC54
		private byte[] LoadBlockData(MapEditor.TilesetHeader ts, int blockCount)
		{
			bool flag = (ulong)ts.BlockImageAddress == 0UL || blockCount <= 0;
			checked
			{
				byte[] array;
				if (flag)
				{
					array = new byte[0];
				}
				else
				{
					int num = blockCount * 16;
					byte[] array2 = new byte[num - 1 + 1];
					Array.Copy(this.romData, (int)ts.BlockImageAddress, array2, 0, num);
					array = array2;
				}
				return array;
			}
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x00031AB0 File Offset: 0x0002FCB0
		private void DrawBlockBatch(Graphics g, byte[] blockData, byte[] imageBytes, Color[] palettes, int startIndex)
		{
			int num = blockData.Length / 16;
			int num2 = 8;
			checked
			{
				int num3 = num - 1;
				for (int i = 0; i <= num3; i++)
				{
					int num4 = startIndex + i;
					int num5 = num4 % num2 * 16;
					int num6 = num4 / num2 * 16;
					bool flag = num4 > 0 && this.IsTripleLayerBlock(num4 - 1);
					if (!flag)
					{
						int num7 = i * 16;
						int num8 = 0;
						do
						{
							int num9 = num7 + num8 * 16 / 2;
							int num10 = 0;
							do
							{
								int num11 = num9 + num10 * 2;
								int num12 = num5 + num10 % 2 * 8;
								int num13 = num6 + num10 / 2 * 8;
								this.DrawTileLayer(g, blockData, num11, num12, num13, imageBytes, palettes);
								num10++;
							}
							while (num10 <= 3);
							num8++;
						}
						while (num8 <= 1);
						bool flag2 = this.IsTripleLayerBlock(num4) && i + 1 < num;
						if (flag2)
						{
							int num14 = (i + 1) * 16;
							int num15 = num14;
							int num16 = 0;
							do
							{
								int num17 = num15 + num16 * 2;
								int num18 = num5 + num16 % 2 * 8;
								int num19 = num6 + num16 / 2 * 8;
								this.DrawTileLayer(g, blockData, num17, num18, num19, imageBytes, palettes);
								num16++;
							}
							while (num16 <= 3);
						}
					}
				}
			}
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x00031BD8 File Offset: 0x0002FDD8
		private void DrawTileLayer(Graphics g, byte[] blockData, int dataOffset, int x, int y, byte[] imageBytes, Color[] palettes)
		{
			ushort num = BitConverter.ToUInt16(blockData, dataOffset);
			int num2 = (int)(num & 1023);
			bool flag = (num & 1024) > 0;
			bool flag2 = (num & 2048) > 0;
			int num3 = (int)((ushort)((uint)num >> 12) & 15);
			checked
			{
				int num4 = num2 * 32;
				bool flag3 = num4 + 32 - 1 >= imageBytes.Length;
				if (!flag3)
				{
					using (Bitmap bitmap = new Bitmap(8, 8))
					{
						BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, 8, 8), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int stride = bitmapData.Stride;
						byte[] array = new byte[stride * 8 - 1 + 1];
						int num5 = num3 * 16;
						int num6 = 0;
						do
						{
							int num7 = 0;
							do
							{
								int num8 = num4 + num6 * 4 + num7 / 2;
								byte b = imageBytes[num8];
								int num9 = (int)(b & 15);
								int num10 = (flag ? (7 - num7) : num7);
								int num11 = (flag2 ? (7 - num6) : num6);
								bool flag4 = num9 > 0;
								if (flag4)
								{
									this.SetPixelData(array, stride, num10, num11, palettes[num5 + num9]);
								}
								int num12 = (int)(unchecked((byte)((uint)b >> 4)) & 15);
								int num13 = (flag ? (7 - (num7 + 1)) : (num7 + 1));
								bool flag5 = num12 > 0;
								if (flag5)
								{
									this.SetPixelData(array, stride, num13, num11, palettes[num5 + num12]);
								}
								num7 += 2;
							}
							while (num7 <= 7);
							num6++;
						}
						while (num6 <= 7);
						Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
						bitmap.UnlockBits(bitmapData);
						g.DrawImage(bitmap, x, y);
					}
				}
			}
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x00031D90 File Offset: 0x0002FF90
		private void SetPixelData(byte[] pixels, int stride, int x, int y, Color color)
		{
			checked
			{
				int num = y * stride + x * 4;
				pixels[num] = color.B;
				pixels[num + 1] = color.G;
				pixels[num + 2] = color.R;
				pixels[num + 3] = byte.MaxValue;
			}
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x00031DD4 File Offset: 0x0002FFD4
		private void GenerateTilesetPalette()
		{
			bool flag = this.tempTileset1 == null || this.tempTileset2 == null;
			if (!flag)
			{
				bool flag2 = this.blockPaletteBitmap != null;
				if (flag2)
				{
					this.blockPaletteBitmap.Dispose();
				}
				this.blockPaletteBitmap = this.CreateTilesetBitmap(this.tempTileset1, this.tempTileset2, Convert.ToInt32(this.nudTileset2Index.Value), ref this.totalBlocks);
				this.vsbTilesetScroll.Minimum = 0;
				this.vsbTilesetScroll.Maximum = Math.Max(0, (this.blockPaletteBitmap != null) ? this.blockPaletteBitmap.Height : 0);
				this.vsbTilesetScroll.SmallChange = 16;
				this.vsbTilesetScroll.LargeChange = this.pnlTilesetPalette.Height;
				this.vsbTilesetScroll.Value = 0;
				this.hsbTilesetScroll.Minimum = 0;
				this.hsbTilesetScroll.Maximum = Math.Max(0, (this.blockPaletteBitmap != null) ? this.blockPaletteBitmap.Width : 0);
				this.hsbTilesetScroll.SmallChange = 16;
				this.hsbTilesetScroll.LargeChange = this.pnlTilesetPalette.Width;
				this.hsbTilesetScroll.Value = 0;
				this.UpdateBlockIndexLabel();
				this.btnOpenBlockEditor.Enabled = this.blockPaletteBitmap != null;
			}
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x00031F30 File Offset: 0x00030130
		private Bitmap GeneratePaletteBitmapForMap(MapEditor.MapFooter footer)
		{
			bool flag = footer == null;
			checked
			{
				Bitmap bitmap;
				if (flag)
				{
					bitmap = null;
				}
				else
				{
					MapEditor.TilesetHeader tilesetHeader = this.ReadTilesetHeader((int)footer.Tileset1Address);
					MapEditor.TilesetHeader tilesetHeader2 = this.ReadTilesetHeader((int)footer.Tileset2Address);
					int num = this.AddressToTilesetIndex(footer.Tileset2Address);
					int num2 = 0;
					bitmap = this.CreateTilesetBitmap(tilesetHeader, tilesetHeader2, num, ref num2);
				}
				return bitmap;
			}
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00031F84 File Offset: 0x00030184
		private void UpdatePrimaryMapLayer()
		{
			bool flag = this.mapMatrix == null || this.blockPaletteBitmap == null;
			checked
			{
				if (flag)
				{
					bool flag2 = this.primaryMapLayerBitmap != null;
					if (flag2)
					{
						this.primaryMapLayerBitmap.Dispose();
						this.primaryMapLayerBitmap = null;
					}
				}
				else
				{
					int length = this.mapMatrix.GetLength(0);
					int length2 = this.mapMatrix.GetLength(1);
					bool flag3 = this.primaryMapLayerBitmap != null;
					if (flag3)
					{
						this.primaryMapLayerBitmap.Dispose();
					}
					this.primaryMapLayerBitmap = new Bitmap(length * 16, length2 * 16);
					using (Graphics graphics = Graphics.FromImage(this.primaryMapLayerBitmap))
					{
						int num = 8;
						int num2 = length2 - 1;
						for (int i = 0; i <= num2; i++)
						{
							int num3 = length - 1;
							for (int j = 0; j <= num3; j++)
							{
								int blockIndex = this.mapMatrix[j, i].BlockIndex;
								int num4 = blockIndex % num * 16;
								int num5 = blockIndex / num * 16;
								bool flag4 = num5 < this.blockPaletteBitmap.Height;
								if (flag4)
								{
									graphics.DrawImage(this.blockPaletteBitmap, new Rectangle(j * 16, i * 16, 16, 16), new Rectangle(num4, num5, 16, 16), GraphicsUnit.Pixel);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x000320EC File Offset: 0x000302EC
		private void UpdateBorderRender()
		{
			bool flag = this.borderMatrix == null || this.blockPaletteBitmap == null;
			checked
			{
				if (flag)
				{
					bool flag2 = this.borderBitmap != null;
					if (flag2)
					{
						this.borderBitmap.Dispose();
						this.borderBitmap = null;
					}
				}
				else
				{
					int length = this.borderMatrix.GetLength(0);
					int length2 = this.borderMatrix.GetLength(1);
					this.borderBitmap = new Bitmap(length * 16, length2 * 16);
					using (Graphics graphics = Graphics.FromImage(this.borderBitmap))
					{
						int num = 8;
						int num2 = length2 - 1;
						for (int i = 0; i <= num2; i++)
						{
							int num3 = length - 1;
							for (int j = 0; j <= num3; j++)
							{
								int num4 = this.borderMatrix[j, i];
								int num5 = num4 % num * 16;
								int num6 = num4 / num * 16;
								bool flag3 = num6 < this.blockPaletteBitmap.Height;
								if (flag3)
								{
									graphics.DrawImage(this.blockPaletteBitmap, new Rectangle(j * 16, i * 16, 16, 16), new Rectangle(num5, num6, 16, 16), GraphicsUnit.Pixel);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x00032234 File Offset: 0x00030434
		private void UpdateMapRender()
		{
			bool flag = this.mapMatrix == null || this.primaryMapLayerBitmap == null;
			checked
			{
				if (!flag)
				{
					int length = this.mapMatrix.GetLength(0);
					int length2 = this.mapMatrix.GetLength(1);
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					this.primaryMapOffsetX = 0;
					this.primaryMapOffsetY = 0;
					bool flag2 = this.chkShowConnectedMap.Checked && this.chkShowConnectedMap.Enabled && this.cmbConnectedMapDirection.SelectedIndex > 0 && this.cmbConnectedMapDirection.SelectedIndex < 5;
					bool flag3 = flag2;
					if (flag3)
					{
						byte byteFromCombo = this.GetByteFromCombo(this.cmbConnectedMapDirection);
						int num5 = Convert.ToInt32(this.nudConnectedMapBank.Value);
						int num6 = Convert.ToInt32(this.nudConnectedMapNumber.Value);
						int num7 = Convert.ToInt32(this.nudConnectedMapShift.Value);
						this.UpdateConnectedMapLayer(num5, num6);
						bool flag4 = this.connectedMapLayerBitmap != null && this.cachedConnMatrix != null;
						if (flag4)
						{
							num = this.cachedConnMatrix.GetLength(0);
							num2 = this.cachedConnMatrix.GetLength(1);
							switch (byteFromCombo)
							{
							case 1:
							{
								num3 = num7;
								num4 = length2;
								bool flag5 = num7 < 0;
								if (flag5)
								{
									this.primaryMapOffsetX = Math.Abs(num7);
									num3 = 0;
								}
								break;
							}
							case 2:
							{
								num3 = num7;
								this.primaryMapOffsetY = num2;
								num4 = 0;
								bool flag6 = num7 < 0;
								if (flag6)
								{
									this.primaryMapOffsetX = Math.Abs(num7);
									num3 = 0;
								}
								break;
							}
							case 3:
							{
								this.primaryMapOffsetX = num;
								num3 = 0;
								num4 = num7;
								bool flag7 = num7 < 0;
								if (flag7)
								{
									this.primaryMapOffsetY = Math.Abs(num7);
									num4 = 0;
								}
								break;
							}
							case 4:
							{
								num3 = length;
								num4 = num7;
								bool flag8 = num7 < 0;
								if (flag8)
								{
									this.primaryMapOffsetY = Math.Abs(num7);
									num4 = 0;
								}
								break;
							}
							}
						}
					}
					int num8 = length + this.primaryMapOffsetX;
					int num9 = length2 + this.primaryMapOffsetY;
					bool flag9 = flag2 && this.connectedMapLayerBitmap != null;
					if (flag9)
					{
						num8 = Math.Max(num8, num3 + num);
						num9 = Math.Max(num9, num4 + num2);
					}
					bool flag10 = this.mapBitmap != null;
					if (flag10)
					{
						this.mapBitmap.Dispose();
					}
					this.mapBitmap = new Bitmap(num8 * 16, num9 * 16);
					using (Graphics graphics = Graphics.FromImage(this.mapBitmap))
					{
						bool flag11 = flag2 && this.connectedMapLayerBitmap != null;
						if (flag11)
						{
							graphics.DrawImageUnscaled(this.connectedMapLayerBitmap, num3 * 16, num4 * 16);
						}
						graphics.DrawImageUnscaled(this.primaryMapLayerBitmap, this.primaryMapOffsetX * 16, this.primaryMapOffsetY * 16);
					}
				}
			}
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x0003251C File Offset: 0x0003071C
		private void UpdateConnectedMapLayer(int bank, int number)
		{
			bool flag = bank == this.cachedConnBank && number == this.cachedConnNumber && this.connectedMapLayerBitmap != null;
			checked
			{
				if (!flag)
				{
					bool flag2 = this.connectedMapLayerBitmap != null;
					if (flag2)
					{
						this.connectedMapLayerBitmap.Dispose();
						this.connectedMapLayerBitmap = null;
					}
					this.cachedConnMatrix = null;
					this.cachedConnFooter = null;
					this.cachedConnBank = bank;
					this.cachedConnNumber = number;
					this.cachedConnMatrix = this.GetConnectedMapMatrix(bank, number, ref this.cachedConnFooter);
					bool flag3 = this.cachedConnMatrix != null && this.cachedConnFooter != null;
					if (flag3)
					{
						int length = this.cachedConnMatrix.GetLength(0);
						int length2 = this.cachedConnMatrix.GetLength(1);
						this.connectedMapLayerBitmap = new Bitmap(length * 16, length2 * 16);
						using (Bitmap bitmap = this.GeneratePaletteBitmapForMap(this.cachedConnFooter))
						{
							bool flag4 = bitmap != null;
							if (flag4)
							{
								using (Graphics graphics = Graphics.FromImage(this.connectedMapLayerBitmap))
								{
									float[][] array = new float[5][];
									array[0] = new float[] { 0.299f, 0.299f, 0.299f, 0f, 0f };
									array[1] = new float[] { 0.587f, 0.587f, 0.587f, 0f, 0f };
									array[2] = new float[] { 0.114f, 0.114f, 0.114f, 0f, 0f };
									int num = 3;
									float[] array2 = new float[5];
									array2[3] = 1f;
									array[num] = array2;
									array[4] = new float[] { 0f, 0f, 0f, 0f, 1f };
									ColorMatrix colorMatrix = new ColorMatrix(array);
									ImageAttributes imageAttributes = new ImageAttributes();
									imageAttributes.SetColorMatrix(colorMatrix);
									int num2 = 8;
									int num3 = length2 - 1;
									for (int i = 0; i <= num3; i++)
									{
										int num4 = length - 1;
										for (int j = 0; j <= num4; j++)
										{
											int blockIndex = this.cachedConnMatrix[j, i].BlockIndex;
											int num5 = blockIndex % num2 * 16;
											int num6 = blockIndex / num2 * 16;
											bool flag5 = num6 < bitmap.Height;
											if (flag5)
											{
												graphics.DrawImage(bitmap, new Rectangle(j * 16, i * 16, 16, 16), num5, num6, 16, 16, GraphicsUnit.Pixel, imageAttributes);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x00032788 File Offset: 0x00030988
		private void DrawMapToGraphics(Graphics g, int zoom, int scrollX, int scrollY, int width, int height)
		{
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			checked
			{
				g.TranslateTransform((float)(0 - scrollX), (float)(0 - scrollY));
				g.ScaleTransform((float)zoom, (float)zoom);
				int num = 0;
				int num2 = 0;
				g.DrawImage(this.mapBitmap, num, num2);
				bool flag = this.tabEditorMode.SelectedTab == this.tabCollision && this.mapMatrix != null && this.collisionBitmap != null;
				if (flag)
				{
					ImageAttributes imageAttributes = new ImageAttributes();
					imageAttributes.SetColorMatrix(new ColorMatrix
					{
						Matrix33 = 0.6f
					});
					int num3 = this.mapMatrix.GetLength(1) - 1;
					for (int i = 0; i <= num3; i++)
					{
						int num4 = this.mapMatrix.GetLength(0) - 1;
						for (int j = 0; j <= num4; j++)
						{
							int collision = this.mapMatrix[j, i].Collision;
							Rectangle rectangle = new Rectangle((j + this.primaryMapOffsetX) * 16 + num, (i + this.primaryMapOffsetY) * 16 + num2, 16, 16);
							g.DrawImage(this.collisionBitmap, rectangle, collision % 8 * 16, collision / 8 * 16, 16, 16, GraphicsUnit.Pixel, imageAttributes);
						}
					}
				}
				bool @checked = this.chkShowGrid.Checked;
				if (@checked)
				{
					using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
					{
						int width2 = this.mapBitmap.Width;
						for (int k = 0; k <= width2; k += 16)
						{
							int num5 = k;
							bool flag2 = num5 * zoom - scrollX >= 0 && num5 * zoom - scrollX <= width;
							if (flag2)
							{
								g.DrawLine(pen, num5, 0, num5, this.mapBitmap.Height);
							}
						}
						int height2 = this.mapBitmap.Height;
						for (int l = 0; l <= height2; l += 16)
						{
							int num6 = l;
							bool flag3 = num6 * zoom - scrollY >= 0 && num6 * zoom - scrollY <= height;
							if (flag3)
							{
								g.DrawLine(pen, 0, num6, this.mapBitmap.Width, num6);
							}
						}
					}
				}
				bool flag4 = this.tempHeader != null && this.tabEditorMode.SelectedTab == this.tabEvent;
				if (flag4)
				{
					this.DrawEventsOnMap(g, num, num2);
				}
			}
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x00032A0C File Offset: 0x00030C0C
		private void DrawEventsOnMap(Graphics g, int drawX, int drawY)
		{
			string text = ((this.cmbEventType.SelectedItem != null) ? this.cmbEventType.SelectedItem.ToString() : "");
			int num = (this.nudEventNo.Enabled ? Convert.ToInt32(this.nudEventNo.Value) : (-1));
			checked
			{
				using (ImageAttributes imageAttributes = new ImageAttributes())
				{
					imageAttributes.SetColorMatrix(new ColorMatrix
					{
						Matrix33 = 0.6f
					});
					bool flag = this.chkShowWarp.Checked && this.tempHeader.Warps != null && this.eventIconBitmap != null;
					if (flag)
					{
						int num2 = this.tempHeader.Warps.Count - 1;
						for (int i = 0; i <= num2; i++)
						{
							MapEditor.WarpEvent warpEvent = this.tempHeader.Warps[i];
							int num3 = (int)warpEvent.X;
							int num4 = (int)warpEvent.Y;
							bool flag2 = Operators.CompareString(text, "ワープ", false) == 0 && i == num && num >= 0;
							if (flag2)
							{
								num3 = Convert.ToInt32(this.nudWarpPositionX.Value);
								num4 = Convert.ToInt32(this.nudWarpPositionY.Value);
							}
							int num5 = (num3 + this.primaryMapOffsetX) * 16 + drawX;
							int num6 = (num4 + this.primaryMapOffsetY) * 16 + drawY;
							g.DrawImage(this.eventIconBitmap, new Rectangle(num5, num6, 16, 16), 32, 0, 16, 16, GraphicsUnit.Pixel, imageAttributes);
							bool flag3 = Operators.CompareString(text, "ワープ", false) == 0 && i == num && num >= 0;
							if (flag3)
							{
								using (Pen pen = new Pen(Color.HotPink, 2f))
								{
									g.DrawRectangle(pen, num5, num6, 15, 15);
								}
							}
						}
					}
					bool flag4 = this.chkShowTrapScript.Checked && this.tempHeader.Traps != null && this.eventIconBitmap != null;
					if (flag4)
					{
						int num7 = this.tempHeader.Traps.Count - 1;
						for (int j = 0; j <= num7; j++)
						{
							MapEditor.TrapEvent trapEvent = this.tempHeader.Traps[j];
							int num8 = (int)trapEvent.X;
							int num9 = (int)trapEvent.Y;
							bool flag5 = Operators.CompareString(text, "踏むスクリプト", false) == 0 && j == num && num >= 0;
							if (flag5)
							{
								num8 = Convert.ToInt32(this.nudTrapScriptPositionX.Value);
								num9 = Convert.ToInt32(this.nudTrapScriptPositionY.Value);
							}
							int num10 = (num8 + this.primaryMapOffsetX) * 16 + drawX;
							int num11 = (num9 + this.primaryMapOffsetY) * 16 + drawY;
							g.DrawImage(this.eventIconBitmap, new Rectangle(num10, num11, 16, 16), 16, 0, 16, 16, GraphicsUnit.Pixel, imageAttributes);
							bool flag6 = Operators.CompareString(text, "踏むスクリプト", false) == 0 && j == num && num >= 0;
							if (flag6)
							{
								using (Pen pen2 = new Pen(Color.HotPink, 2f))
								{
									g.DrawRectangle(pen2, num10, num11, 15, 15);
								}
							}
						}
					}
					bool flag7 = this.chkShowSign.Checked && this.tempHeader.Signs != null && this.eventIconBitmap != null;
					if (flag7)
					{
						int num12 = this.tempHeader.Signs.Count - 1;
						for (int k = 0; k <= num12; k++)
						{
							MapEditor.SignEvent signEvent = this.tempHeader.Signs[k];
							int num13 = (int)signEvent.X;
							int num14 = (int)signEvent.Y;
							bool flag8 = Operators.CompareString(text, "看板", false) == 0 && k == num && num >= 0;
							if (flag8)
							{
								num13 = Convert.ToInt32(this.nudSignPositionX.Value);
								num14 = Convert.ToInt32(this.nudSignPositionY.Value);
							}
							int num15 = (num13 + this.primaryMapOffsetX) * 16 + drawX;
							int num16 = (num14 + this.primaryMapOffsetY) * 16 + drawY;
							g.DrawImage(this.eventIconBitmap, new Rectangle(num15, num16, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, imageAttributes);
							bool flag9 = Operators.CompareString(text, "看板", false) == 0 && k == num && num >= 0;
							if (flag9)
							{
								using (Pen pen3 = new Pen(Color.HotPink, 2f))
								{
									g.DrawRectangle(pen3, num15, num16, 15, 15);
								}
							}
						}
					}
				}
				bool flag10 = this.chkShowOverWorld.Checked && this.tempHeader.Persons != null;
				if (flag10)
				{
					int num17 = this.tempHeader.Persons.Count - 1;
					for (int l = 0; l <= num17; l++)
					{
						MapEditor.PersonEvent personEvent = this.tempHeader.Persons[l];
						int num18 = (int)personEvent.X;
						int num19 = (int)personEvent.Y;
						int num20 = (int)personEvent.SpriteNo;
						bool flag11 = Operators.CompareString(text, "歩行グラフィック", false) == 0 && l == num && num >= 0;
						if (flag11)
						{
							num18 = Convert.ToInt32(this.nudPersonPositionX.Value);
							num19 = Convert.ToInt32(this.nudPersonPositionY.Value);
							num20 = Convert.ToInt32(this.nudPersonSpriteNo.Value);
						}
						int num21 = (num18 + this.primaryMapOffsetX) * 16 + drawX;
						int num22 = (num19 + this.primaryMapOffsetY) * 16 + drawY;
						using (Bitmap overWorldSpriteImage = this.GetOverWorldSpriteImage(num20))
						{
							bool flag12 = overWorldSpriteImage != null;
							if (flag12)
							{
								int num23 = num21 + 8 - overWorldSpriteImage.Width / 2;
								int num24 = num22 + 16 - overWorldSpriteImage.Height;
								g.DrawImage(overWorldSpriteImage, num23, num24);
							}
						}
						bool flag13 = Operators.CompareString(text, "歩行グラフィック", false) == 0 && l == num && num >= 0;
						if (flag13)
						{
							using (Pen pen4 = new Pen(Color.HotPink, 2f))
							{
								g.DrawRectangle(pen4, num21, num22, 15, 15);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x00033100 File Offset: 0x00031300
		private Bitmap GetOverWorldSpriteImage(int spriteNo)
		{
			checked
			{
				Bitmap bitmap;
				try
				{
					int num = MyProject.Forms.OverWorldEditor.OVERWORLD_DATA_TABLE_OFFSET + spriteNo * 4;
					uint num2 = BitConverter.ToUInt32(this.romData, num);
					bool flag = unchecked((ulong)num2) == 0UL;
					if (flag)
					{
						bitmap = null;
					}
					else
					{
						int num3 = (int)(num2 - 134217728U);
						ushort num4 = BitConverter.ToUInt16(this.romData, num3 + 2);
						ushort num5 = BitConverter.ToUInt16(this.romData, num3 + 8);
						ushort num6 = BitConverter.ToUInt16(this.romData, num3 + 10);
						uint num7 = BitConverter.ToUInt32(this.romData, num3 + 28) - 134217728U;
						bool flag2 = num5 == 0 || num6 == 0 || unchecked((ulong)num7) <= 0UL;
						if (flag2)
						{
							bitmap = null;
						}
						else
						{
							uint num8 = BitConverter.ToUInt32(this.romData, (int)num7) - 134217728U;
							ushort num9 = BitConverter.ToUInt16(this.romData, (int)num7 + 4);
							bool flag3 = unchecked((ulong)num8) <= 0UL;
							if (flag3)
							{
								bitmap = null;
							}
							else
							{
								uint num10 = 0U;
								int num11 = 0;
								int num12 = 0;
								for (;;)
								{
									num12 = MyProject.Forms.OverWorldEditor.OVERWORLD_PALETTE_TABLE_OFFSET + num11 * 8;
									bool flag4 = this.romData[num12] == 0 && this.romData[num12 + 1] == 0 && this.romData[num12 + 2] == 0 && this.romData[num12 + 3] == 0;
									if (flag4)
									{
										break;
									}
									ushort num13 = BitConverter.ToUInt16(this.romData, num12 + 4);
									bool flag5 = num13 == num4;
									if (flag5)
									{
										goto Block_11;
									}
									num11++;
								}
								goto IL_018B;
								Block_11:
								num10 = BitConverter.ToUInt32(this.romData, num12) - 134217728U;
								IL_018B:
								bool flag6 = unchecked((ulong)num10) == 0UL;
								if (flag6)
								{
									bitmap = null;
								}
								else
								{
									byte[] array = new byte[32];
									Array.Copy(this.romData, (int)num10, array, 0, 32);
									Color[] array2 = ImageProcessor.LoadPalette(array, false);
									array2[0] = Color.Transparent;
									byte[] array3 = new byte[(int)(num9 - 1 + 1)];
									Array.Copy(this.romData, (int)num8, array3, 0, (int)num9);
									bitmap = ImageProcessor.LoadSprite(ref array3, array2, (int)num5, (int)num6, true);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					bitmap = null;
				}
				return bitmap;
			}
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x00033340 File Offset: 0x00031540
		private void pnlTilesetPalette_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.blockPaletteBitmap == null;
			checked
			{
				if (!flag)
				{
					Graphics graphics = e.Graphics;
					int value = this.hsbTilesetScroll.Value;
					int value2 = this.vsbTilesetScroll.Value;
					Rectangle rectangle = new Rectangle(value, value2, this.pnlTilesetPalette.Width, this.pnlTilesetPalette.Height);
					bool flag2 = rectangle.X + rectangle.Width > this.blockPaletteBitmap.Width;
					if (flag2)
					{
						rectangle.Width = this.blockPaletteBitmap.Width - rectangle.X;
					}
					bool flag3 = rectangle.Y + rectangle.Height > this.blockPaletteBitmap.Height;
					if (flag3)
					{
						rectangle.Height = this.blockPaletteBitmap.Height - rectangle.Y;
					}
					bool flag4 = rectangle.Width > 0 && rectangle.Height > 0;
					if (flag4)
					{
						graphics.DrawImage(this.blockPaletteBitmap, 0, 0, rectangle, GraphicsUnit.Pixel);
					}
					bool @checked = this.chkShowGrid.Checked;
					if (@checked)
					{
						using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
						{
							int width = this.blockPaletteBitmap.Width;
							for (int i = 0; i <= width; i += 16)
							{
								int num = i - value;
								bool flag5 = num >= 0 && num <= this.pnlTilesetPalette.Width;
								if (flag5)
								{
									graphics.DrawLine(pen, num, 0, num, this.pnlTilesetPalette.Height);
								}
							}
							int height = this.blockPaletteBitmap.Height;
							for (int j = 0; j <= height; j += 16)
							{
								int num2 = j - value2;
								bool flag6 = num2 >= 0 && num2 <= this.pnlTilesetPalette.Height;
								if (flag6)
								{
									graphics.DrawLine(pen, 0, num2, this.pnlTilesetPalette.Width, num2);
								}
							}
						}
					}
					int num3 = this.selectedBlockRect.X * 16 - value;
					int num4 = this.selectedBlockRect.Y * 16 - value2;
					using (Pen pen2 = new Pen(Color.Red, 2f))
					{
						graphics.DrawRectangle(pen2, num3 + 1, num4 + 1, this.selectedBlockRect.Width * 16 - 2, this.selectedBlockRect.Height * 16 - 2);
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マップチップ選択パレット上の座標を有効なブロック位置へ変換する処理
		//-------------------------------------------------------------------------------
		private bool TryGetTilesetPaletteCell(Point point, out int cellX, out int cellY, out int blockId)
		{
			cellX = 0;
			cellY = 0;
			blockId = -1;
			bool flag = this.blockPaletteBitmap == null || this.totalBlocks <= 0;
			if (flag)
			{
				return false;
			}
			int bitmapX = point.X + this.hsbTilesetScroll.Value;
			int bitmapY = point.Y + this.vsbTilesetScroll.Value;
			bool flag2 = bitmapX < 0 || bitmapY < 0 || bitmapX >= this.blockPaletteBitmap.Width || bitmapY >= this.blockPaletteBitmap.Height;
			if (flag2)
			{
				return false;
			}
			cellX = bitmapX / 16;
			cellY = bitmapY / 16;
			int columns = Math.Max(1, this.blockPaletteBitmap.Width / 16);
			blockId = cellY * columns + cellX;
			return blockId >= 0 && blockId < this.totalBlocks;
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x000335DC File Offset: 0x000317DC
		private void pnlTilesetPalette_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = e.Button != MouseButtons.Left;
			checked
			{
				if (!flag)
				{
					bool flag2 = !this.TryGetTilesetPaletteCell(e.Location, out int num, out int num2, out int num3);
					if (flag2)
					{
						this.isSelectingBlocks = false;
						return;
					}
					this.isSelectingBlocks = true;
					int columns = Math.Max(1, this.blockPaletteBitmap.Width / 16);
					bool flag3 = num3 > 0 && this.IsTripleLayerBlock(num3 - 1);
					if (flag3)
					{
						num3--;
						num = num3 % columns;
						num2 = num3 / columns;
					}
					this.selectionAnchor = new Point(num, num2);
					this.selectedBlockRect = new Rectangle(num, num2, this.IsTripleLayerBlock(num3) ? 2 : 1, 1);
					this.UpdateBlockIndexLabel();
					this.pnlTilesetPalette.Invalidate();
				}
			}
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x000336A8 File Offset: 0x000318A8
		private void pnlTilesetPalette_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = !this.isSelectingBlocks;
			checked
			{
				if (!flag)
				{
					bool flag2 = !this.TryGetTilesetPaletteCell(e.Location, out int num, out int num2, out int blockId);
					if (flag2)
					{
						return;
					}
					int num3 = Math.Max(0, Math.Min(this.selectionAnchor.X, num));
					int num4 = Math.Max(0, Math.Min(this.selectionAnchor.Y, num2));
					int num5 = Math.Max(this.selectionAnchor.X, num);
					int num6 = Math.Max(this.selectionAnchor.Y, num2);
					int num7 = num5 - num3 + 1;
					int columns = Math.Max(1, this.blockPaletteBitmap.Width / 16);
					int num8 = num4 * columns + num5;
					bool flag3 = this.IsTripleLayerBlock(num8);
					if (flag3)
					{
						num7++;
					}
					this.selectedBlockRect = new Rectangle(num3, num4, num7, num6 - num4 + 1);
					this.UpdateBlockIndexLabel();
					this.pnlTilesetPalette.Invalidate();
				}
			}
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x0003379A File Offset: 0x0003199A
		private void pnlTilesetPalette_MouseUp(object sender, MouseEventArgs e)
		{
			this.isSelectingBlocks = false;
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x000337A4 File Offset: 0x000319A4
		private void pnlMapCanvas_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.mapBitmap == null;
			if (!flag)
			{
				int num = this.GetMapZoomScale();
				int num2 = (this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0);
				int num3 = (this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0);
				this.DrawMapToGraphics(e.Graphics, num, num2, num3, this.pnlMapCanvas.Width, this.pnlMapCanvas.Height);
			}
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x00033834 File Offset: 0x00031A34
		private void pnlMapCanvas_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = this.mapMatrix == null;
			checked
			{
				if (!flag)
				{
					int num = this.GetMapZoomScale();
					int num2 = (e.X + (this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0)) / num;
					int num3 = (e.Y + (this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0)) / num;
					int num4 = num2 / 16 - this.primaryMapOffsetX;
					int num5 = num3 / 16 - this.primaryMapOffsetY;
					bool flag2 = this.tabEditorMode.SelectedTab == this.tabBlock;
					if (flag2)
					{
						this.HandleBlockModeMouseDown(e, num4, num5);
					}
					else
					{
						bool flag3 = this.tabEditorMode.SelectedTab == this.tabCollision;
						if (flag3)
						{
							this.HandleCollisionModeMouseDown(e, num4, num5);
						}
						else
						{
							bool flag4 = this.tabEditorMode.SelectedTab == this.tabEvent;
							if (flag4)
							{
								this.HandleEventModeMouseDown(e, num4, num5);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x00033940 File Offset: 0x00031B40
		private void HandleBlockModeMouseDown(MouseEventArgs e, int mapX, int mapY)
		{
			bool flag = e.Button == MouseButtons.Left;
			checked
			{
				if (flag)
				{
					this.isPaintingMap = true;
					this.BeginMapEditStroke();
					this.PasteSelectedBlocksToMap(e.X, e.Y);
				}
				else
				{
					bool flag2 = e.Button == MouseButtons.Right;
					if (flag2)
					{
						bool flag3 = mapX < 0 || mapX >= this.mapMatrix.GetLength(0) || mapY < 0 || mapY >= this.mapMatrix.GetLength(1);
						if (!flag3)
						{
							int num = this.mapMatrix[mapX, mapY].BlockIndex;
							bool flag4 = num > 0 && this.IsTripleLayerBlock(num - 1);
							if (flag4)
							{
								num--;
							}
							int num2 = 8;
							int num3 = num / num2;
							this.selectedBlockRect = new Rectangle(num % num2, num3, this.IsTripleLayerBlock(num) ? 2 : 1, 1);
							this.selectionAnchor = new Point(this.selectedBlockRect.X, this.selectedBlockRect.Y);
							int num4 = num3 * 16;
							bool flag5 = num4 < this.vsbTilesetScroll.Value;
							if (flag5)
							{
								this.vsbTilesetScroll.Value = Math.Max(this.vsbTilesetScroll.Minimum, num4);
							}
							else
							{
								bool flag6 = num4 + 16 > this.vsbTilesetScroll.Value + this.pnlTilesetPalette.Height;
								if (flag6)
								{
									this.vsbTilesetScroll.Value = Math.Min(Math.Max(0, this.vsbTilesetScroll.Maximum - this.vsbTilesetScroll.LargeChange + 1), num4 + 16 - this.pnlTilesetPalette.Height);
								}
							}
							this.UpdateBlockIndexLabel();
							this.pnlTilesetPalette.Invalidate();
						}
					}
				}
			}
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x00033AF4 File Offset: 0x00031CF4
		private void HandleCollisionModeMouseDown(MouseEventArgs e, int mapX, int mapY)
		{
			bool flag = e.Button == MouseButtons.Left;
			if (flag)
			{
				this.isPaintingCollision = true;
				this.BeginMapEditStroke();
				this.PasteSelectedCollisionToMap(e.X, e.Y);
			}
			else
			{
				bool flag2 = e.Button == MouseButtons.Right;
				if (flag2)
				{
					bool flag3 = mapX >= 0 && mapX < this.mapMatrix.GetLength(0) && mapY >= 0 && mapY < this.mapMatrix.GetLength(1);
					if (flag3)
					{
						this.selectedCollisionIndex = this.mapMatrix[mapX, mapY].Collision;
						this.pnlCollisionPalette.Invalidate();
					}
				}
			}
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x00033B98 File Offset: 0x00031D98
		private void HandleEventModeMouseDown(MouseEventArgs e, int mapX, int mapY)
		{
			bool flag = e.Button != MouseButtons.Left;
			checked
			{
				if (!flag)
				{
					string text = ((this.cmbEventType.SelectedItem != null) ? this.cmbEventType.SelectedItem.ToString() : "");
					int num = (this.nudEventNo.Enabled ? Convert.ToInt32(this.nudEventNo.Value) : (-1));
					List<Tuple<string, int>> list = new List<Tuple<string, int>>();
					bool flag2 = this.chkShowOverWorld.Checked && this.tempHeader.Persons != null;
					if (flag2)
					{
						int num2 = this.tempHeader.Persons.Count - 1;
						for (int i = 0; i <= num2; i++)
						{
							bool flag3 = (int)this.tempHeader.Persons[i].X == mapX && (int)this.tempHeader.Persons[i].Y == mapY;
							if (flag3)
							{
								list.Add(new Tuple<string, int>("歩行グラフィック", i));
							}
						}
					}
					bool flag4 = this.chkShowSign.Checked && this.tempHeader.Signs != null;
					if (flag4)
					{
						int num3 = this.tempHeader.Signs.Count - 1;
						for (int j = 0; j <= num3; j++)
						{
							bool flag5 = (int)this.tempHeader.Signs[j].X == mapX && (int)this.tempHeader.Signs[j].Y == mapY;
							if (flag5)
							{
								list.Add(new Tuple<string, int>("看板", j));
							}
						}
					}
					bool flag6 = this.chkShowTrapScript.Checked && this.tempHeader.Traps != null;
					if (flag6)
					{
						int num4 = this.tempHeader.Traps.Count - 1;
						for (int k = 0; k <= num4; k++)
						{
							bool flag7 = (int)this.tempHeader.Traps[k].X == mapX && (int)this.tempHeader.Traps[k].Y == mapY;
							if (flag7)
							{
								list.Add(new Tuple<string, int>("踏むスクリプト", k));
							}
						}
					}
					bool flag8 = this.chkShowWarp.Checked && this.tempHeader.Warps != null;
					if (flag8)
					{
						int num5 = this.tempHeader.Warps.Count - 1;
						for (int l = 0; l <= num5; l++)
						{
							bool flag9 = (int)this.tempHeader.Warps[l].X == mapX && (int)this.tempHeader.Warps[l].Y == mapY;
							if (flag9)
							{
								list.Add(new Tuple<string, int>("ワープ", l));
							}
						}
					}
					bool flag10 = list.Count > 0;
					if (flag10)
					{
						int num6 = 0;
						int num7 = list.Count - 1;
						for (int m = 0; m <= num7; m++)
						{
							bool flag11 = Operators.CompareString(list[m].Item1, text, false) == 0 && list[m].Item2 == num;
							if (flag11)
							{
								num6 = (m + 1) % list.Count;
								break;
							}
						}
						string item = list[num6].Item1;
						int item2 = list[num6].Item2;
						bool flag12 = this.cmbEventType.SelectedItem == null || Operators.CompareString(this.cmbEventType.SelectedItem.ToString(), item, false) != 0;
						if (flag12)
						{
							this.cmbEventType.SelectedItem = item;
						}
						bool flag13 = decimal.Compare(this.nudEventNo.Value, new decimal(item2)) != 0;
						if (flag13)
						{
							this.nudEventNo.Value = new decimal(item2);
						}
						this.isDraggingEvent = true;
					}
					else
					{
						int num8 = -1;
						int num9 = -1;
						bool flag14 = num >= 0;
						if (flag14)
						{
							if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
							{
								if (Operators.CompareString(text, "ワープ", false) != 0)
								{
									if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
									{
										if (Operators.CompareString(text, "看板", false) == 0)
										{
											bool enabled = this.grpSignEvent.Enabled;
											if (enabled)
											{
												num8 = Convert.ToInt32(this.nudSignPositionX.Value);
												num9 = Convert.ToInt32(this.nudSignPositionY.Value);
											}
										}
									}
									else
									{
										bool enabled2 = this.grpTrapScriptEvent.Enabled;
										if (enabled2)
										{
											num8 = Convert.ToInt32(this.nudTrapScriptPositionX.Value);
											num9 = Convert.ToInt32(this.nudTrapScriptPositionY.Value);
										}
									}
								}
								else
								{
									bool enabled3 = this.grpWarpEvent.Enabled;
									if (enabled3)
									{
										num8 = Convert.ToInt32(this.nudWarpPositionX.Value);
										num9 = Convert.ToInt32(this.nudWarpPositionY.Value);
									}
								}
							}
							else
							{
								bool enabled4 = this.grpPersonEvent.Enabled;
								if (enabled4)
								{
									num8 = Convert.ToInt32(this.nudPersonPositionX.Value);
									num9 = Convert.ToInt32(this.nudPersonPositionY.Value);
								}
							}
						}
						bool flag15 = num8 >= 0 && num9 >= 0 && mapX == num8 && mapY == num9;
						if (flag15)
						{
							this.isDraggingEvent = true;
						}
					}
				}
			}
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x00034100 File Offset: 0x00032300
		private void pnlMapCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = this.mapMatrix == null;
			checked
			{
				if (flag)
				{
					this.ResetMapPositionLabels();
				}
				else
				{
					int num = this.GetMapZoomScale();
					int num2 = (e.X + (this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0)) / num / 16 - this.primaryMapOffsetX;
					int num3 = (e.Y + (this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0)) / num / 16 - this.primaryMapOffsetY;
					bool flag2 = num2 >= 0 && num2 < this.mapMatrix.GetLength(0) && num3 >= 0 && num3 < this.mapMatrix.GetLength(1);
					if (flag2)
					{
						this.lblMapPositionX.Text = string.Format("X : {0:D4} (0x{1:X4})", num2, num2);
						this.lblMapPositionY.Text = string.Format("Y : {0:D4} (0x{1:X4})", num3, num3);
					}
					else
					{
						this.ResetMapPositionLabels();
					}
					bool flag3 = this.isPaintingMap;
					if (flag3)
					{
						this.PasteSelectedBlocksToMap(e.X, e.Y);
					}
					else
					{
						bool flag4 = this.isPaintingCollision;
						if (flag4)
						{
							this.PasteSelectedCollisionToMap(e.X, e.Y);
						}
						else
						{
							bool flag5 = this.isDraggingEvent && this.tabEditorMode.SelectedTab == this.tabEvent && this.cmbEventType.SelectedItem != null;
							if (flag5)
							{
								bool flag6 = num2 >= 0 && num2 < this.mapMatrix.GetLength(0) && num3 >= 0 && num3 < this.mapMatrix.GetLength(1);
								if (flag6)
								{
									string text = this.cmbEventType.SelectedItem.ToString();
									if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
									{
										if (Operators.CompareString(text, "ワープ", false) != 0)
										{
											if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
											{
												if (Operators.CompareString(text, "看板", false) == 0)
												{
													bool enabled = this.grpSignEvent.Enabled;
													if (enabled)
													{
														bool flag7 = decimal.Compare(this.nudSignPositionX.Value, new decimal(num2)) != 0;
														if (flag7)
														{
															this.nudSignPositionX.Value = new decimal(num2);
														}
														bool flag8 = decimal.Compare(this.nudSignPositionY.Value, new decimal(num3)) != 0;
														if (flag8)
														{
															this.nudSignPositionY.Value = new decimal(num3);
														}
													}
												}
											}
											else
											{
												bool enabled2 = this.grpTrapScriptEvent.Enabled;
												if (enabled2)
												{
													bool flag9 = decimal.Compare(this.nudTrapScriptPositionX.Value, new decimal(num2)) != 0;
													if (flag9)
													{
														this.nudTrapScriptPositionX.Value = new decimal(num2);
													}
													bool flag10 = decimal.Compare(this.nudTrapScriptPositionY.Value, new decimal(num3)) != 0;
													if (flag10)
													{
														this.nudTrapScriptPositionY.Value = new decimal(num3);
													}
												}
											}
										}
										else
										{
											bool enabled3 = this.grpWarpEvent.Enabled;
											if (enabled3)
											{
												bool flag11 = decimal.Compare(this.nudWarpPositionX.Value, new decimal(num2)) != 0;
												if (flag11)
												{
													this.nudWarpPositionX.Value = new decimal(num2);
												}
												bool flag12 = decimal.Compare(this.nudWarpPositionY.Value, new decimal(num3)) != 0;
												if (flag12)
												{
													this.nudWarpPositionY.Value = new decimal(num3);
												}
											}
										}
									}
									else
									{
										bool enabled4 = this.grpPersonEvent.Enabled;
										if (enabled4)
										{
											bool flag13 = decimal.Compare(this.nudPersonPositionX.Value, new decimal(num2)) != 0;
											if (flag13)
											{
												this.nudPersonPositionX.Value = new decimal(num2);
											}
											bool flag14 = decimal.Compare(this.nudPersonPositionY.Value, new decimal(num3)) != 0;
											if (flag14)
											{
												this.nudPersonPositionY.Value = new decimal(num3);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x0003450F File Offset: 0x0003270F
		private void pnlMapCanvas_MouseLeave(object sender, EventArgs e)
		{
			this.ResetMapPositionLabels();
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x0003451C File Offset: 0x0003271C
		private void PasteSelectedBlocksToMap(int mouseX, int mouseY)
		{
			bool flag = this.mapMatrix == null || this.primaryMapLayerBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = this.GetMapZoomScale();
					int num2 = (mouseX + (this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0)) / num / 16 - this.primaryMapOffsetX;
					int num3 = (mouseY + (this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0)) / num / 16 - this.primaryMapOffsetY;
					int num4 = 8;
					bool flag2 = false;
					using (Graphics graphics = Graphics.FromImage(this.primaryMapLayerBitmap))
					{
						graphics.CompositingMode = CompositingMode.SourceCopy;
						int num5 = this.selectedBlockRect.Height - 1;
						for (int i = 0; i <= num5; i++)
						{
							int num6 = this.selectedBlockRect.Width - 1;
							for (int j = 0; j <= num6; j++)
							{
								int num7 = num2 + j;
								int num8 = num3 + i;
								bool flag3 = num7 < 0 || num7 >= this.mapMatrix.GetLength(0) || num8 < 0 || num8 >= this.mapMatrix.GetLength(1);
								if (!flag3)
								{
									int num9 = (this.selectedBlockRect.Y + i) * num4 + (this.selectedBlockRect.X + j);
									bool flag4 = num9 >= this.totalBlocks;
									if (!flag4)
									{
										bool flag5 = num9 > 0 && this.IsTripleLayerBlock(num9 - 1);
										if (!flag5)
										{
											bool flag6 = this.mapMatrix[num7, num8].BlockIndex != num9;
											if (flag6)
											{
												this.RecordMapEditAction(num7, num8, this.mapMatrix[num7, num8].BlockIndex, num9, this.mapMatrix[num7, num8].Collision, this.mapMatrix[num7, num8].Collision, true);
												this.mapMatrix[num7, num8].BlockIndex = num9;
												flag2 = true;
												graphics.DrawImage(this.blockPaletteBitmap, new Rectangle(num7 * 16, num8 * 16, 16, 16), new Rectangle(num9 % num4 * 16, num9 / num4 * 16, 16, 16), GraphicsUnit.Pixel);
											}
										}
									}
								}
							}
						}
					}
					bool flag7 = flag2;
					if (flag7)
					{
						this.UpdateMapRender();
						this.pnlMapCanvas.Invalidate();
						this.SetUnsavedChanges(true);
					}
				}
			}
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x00034778 File Offset: 0x00032978
		private void PasteSelectedCollisionToMap(int mouseX, int mouseY)
		{
			bool flag = this.mapMatrix == null;
			checked
			{
				if (!flag)
				{
					int num = this.GetMapZoomScale();
					int num2 = (mouseX + (this.hsbMapDataPreview.Enabled ? this.hsbMapDataPreview.Value : 0)) / num / 16 - this.primaryMapOffsetX;
					int num3 = (mouseY + (this.vsbMapDataPreview.Enabled ? this.vsbMapDataPreview.Value : 0)) / num / 16 - this.primaryMapOffsetY;
					bool flag2 = num2 < 0 || num2 >= this.mapMatrix.GetLength(0) || num3 < 0 || num3 >= this.mapMatrix.GetLength(1);
					if (!flag2)
					{
						bool flag3 = this.mapMatrix[num2, num3].Collision != this.selectedCollisionIndex;
						if (flag3)
						{
							this.RecordMapEditAction(num2, num3, this.mapMatrix[num2, num3].BlockIndex, this.mapMatrix[num2, num3].BlockIndex, this.mapMatrix[num2, num3].Collision, this.selectedCollisionIndex, false);
							this.mapMatrix[num2, num3].Collision = this.selectedCollisionIndex;
							this.pnlMapCanvas.Invalidate();
							this.SetUnsavedChanges(true);
						}
					}
				}
			}
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x00034884 File Offset: 0x00032A84
		private void pnlBorderDataPreview_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.borderBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = (this.pnlBorderDataPreview.Width - this.borderBitmap.Width) / 2;
					int num2 = (this.pnlBorderDataPreview.Height - this.borderBitmap.Height) / 2;
					e.Graphics.DrawImage(this.borderBitmap, num, num2);
					bool @checked = this.chkShowGrid.Checked;
					if (@checked)
					{
						using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
						{
							int width = this.borderBitmap.Width;
							for (int i = 0; i <= width; i += 16)
							{
								e.Graphics.DrawLine(pen, i + num, num2, i + num, num2 + this.borderBitmap.Height);
							}
							int height = this.borderBitmap.Height;
							for (int j = 0; j <= height; j += 16)
							{
								e.Graphics.DrawLine(pen, num, j + num2, num + this.borderBitmap.Width, j + num2);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x000349C4 File Offset: 0x00032BC4
		private void pnlBorderDataPreview_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = this.borderMatrix == null || this.borderBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = (this.pnlBorderDataPreview.Width - this.borderBitmap.Width) / 2;
					int num2 = (this.pnlBorderDataPreview.Height - this.borderBitmap.Height) / 2;
					bool flag2 = this.tabEditorMode.SelectedTab == this.tabBlock;
					if (flag2)
					{
						bool flag3 = e.Button == MouseButtons.Left;
						if (flag3)
						{
							this.isPaintingBorder = true;
							this.PasteSelectedBlocksToBorder(e.X, e.Y, num, num2);
						}
						else
						{
							bool flag4 = e.Button == MouseButtons.Right;
							if (flag4)
							{
								int num3 = e.X - num;
								int num4 = e.Y - num2;
								bool flag5 = num3 < 0 || num3 >= this.borderBitmap.Width || num4 < 0 || num4 >= this.borderBitmap.Height;
								if (!flag5)
								{
									int num5 = num3 / 16;
									int num6 = num4 / 16;
									bool flag6 = num5 >= this.borderMatrix.GetLength(0) || num6 >= this.borderMatrix.GetLength(1);
									if (!flag6)
									{
										int num7 = this.borderMatrix[num5, num6];
										bool flag7 = num7 > 0 && this.IsTripleLayerBlock(num7 - 1);
										if (flag7)
										{
											num7--;
										}
										int num8 = 8;
										int num9 = num7 / num8;
										this.selectedBlockRect = new Rectangle(num7 % num8, num9, this.IsTripleLayerBlock(num7) ? 2 : 1, 1);
										this.selectionAnchor = new Point(this.selectedBlockRect.X, this.selectedBlockRect.Y);
										int num10 = num9 * 16;
										bool flag8 = num10 < this.vsbTilesetScroll.Value;
										if (flag8)
										{
											this.vsbTilesetScroll.Value = Math.Max(this.vsbTilesetScroll.Minimum, num10);
										}
										else
										{
											bool flag9 = num10 + 16 > this.vsbTilesetScroll.Value + this.pnlTilesetPalette.Height;
											if (flag9)
											{
												this.vsbTilesetScroll.Value = Math.Min(Math.Max(0, this.vsbTilesetScroll.Maximum - this.vsbTilesetScroll.LargeChange + 1), num10 + 16 - this.pnlTilesetPalette.Height);
											}
										}
										this.UpdateBlockIndexLabel();
										this.pnlTilesetPalette.Invalidate();
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x00034C48 File Offset: 0x00032E48
		private void pnlBorderDataPreview_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = this.isPaintingBorder;
			checked
			{
				if (flag)
				{
					int num = (this.pnlBorderDataPreview.Width - this.borderBitmap.Width) / 2;
					int num2 = (this.pnlBorderDataPreview.Height - this.borderBitmap.Height) / 2;
					this.PasteSelectedBlocksToBorder(e.X, e.Y, num, num2);
				}
			}
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x00034CAC File Offset: 0x00032EAC
		private void PasteSelectedBlocksToBorder(int mouseX, int mouseY, int drawX, int drawY)
		{
			bool flag = this.borderMatrix == null || this.borderBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = (mouseX - drawX) / 16;
					int num2 = (mouseY - drawY) / 16;
					int num3 = 8;
					bool flag2 = false;
					using (Graphics graphics = Graphics.FromImage(this.borderBitmap))
					{
						graphics.CompositingMode = CompositingMode.SourceCopy;
						int num4 = this.selectedBlockRect.Height - 1;
						for (int i = 0; i <= num4; i++)
						{
							int num5 = this.selectedBlockRect.Width - 1;
							for (int j = 0; j <= num5; j++)
							{
								int num6 = num + j;
								int num7 = num2 + i;
								bool flag3 = num6 < 0 || num6 >= this.borderMatrix.GetLength(0) || num7 < 0 || num7 >= this.borderMatrix.GetLength(1);
								if (!flag3)
								{
									int num8 = (this.selectedBlockRect.Y + i) * num3 + (this.selectedBlockRect.X + j);
									bool flag4 = num8 >= this.totalBlocks;
									if (!flag4)
									{
										bool flag5 = num8 > 0 && this.IsTripleLayerBlock(num8 - 1);
										if (!flag5)
										{
											bool flag6 = this.borderMatrix[num6, num7] != num8;
											if (flag6)
											{
												this.borderMatrix[num6, num7] = num8;
												flag2 = true;
												graphics.DrawImage(this.blockPaletteBitmap, new Rectangle(num6 * 16, num7 * 16, 16, 16), new Rectangle(num8 % num3 * 16, num8 / num3 * 16, 16, 16), GraphicsUnit.Pixel);
											}
										}
									}
								}
							}
						}
					}
					bool flag7 = flag2;
					if (flag7)
					{
						this.pnlBorderDataPreview.Invalidate();
						this.SetUnsavedChanges(true);
					}
				}
			}
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x00034E9C File Offset: 0x0003309C
		private void pnlCollisionPalette_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.blockPaletteBitmap == null || this.collisionBitmap == null;
			checked
			{
				if (!flag)
				{
					e.Graphics.DrawImage(this.collisionBitmap, 0, 0);
					bool @checked = this.chkShowGrid.Checked;
					if (@checked)
					{
						using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
						{
							int num = 0;
							do
							{
								e.Graphics.DrawLine(pen, num, 0, num, 128);
								e.Graphics.DrawLine(pen, 0, num, 128, num);
								num += 16;
							}
							while (num <= 128);
						}
					}
					using (Pen pen2 = new Pen(Color.Red, 2f))
					{
						e.Graphics.DrawRectangle(pen2, this.selectedCollisionIndex % 8 * 16 + 1, this.selectedCollisionIndex / 8 * 16 + 1, 14, 14);
					}
				}
			}
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x00034FBC File Offset: 0x000331BC
		private void pnlCollisionPalette_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = e.Button == MouseButtons.Left;
			checked
			{
				if (flag)
				{
					int num = e.X / 16;
					int num2 = e.Y / 16;
					bool flag2 = num >= 0 && (double)num < 8.0 && num2 >= 0 && (double)num2 < 8.0;
					if (flag2)
					{
						this.selectedCollisionIndex = (int)Math.Round(unchecked((double)num + (double)(checked(num2 * 128)) / 16.0));
						this.pnlCollisionPalette.Invalidate();
					}
				}
			}
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x00035049 File Offset: 0x00033249
		private void MouseUpHandlers(object sender, MouseEventArgs e)
		{
			this.EndMapEditStroke();
			this.isPaintingMap = false;
			this.isPaintingBorder = false;
			this.isPaintingCollision = false;
			this.isDraggingEvent = false;
		}

		//-------------------------------------------------------------------------------
		// マップ編集の1操作分の履歴記録を開始する処理
		//-------------------------------------------------------------------------------
		private void BeginMapEditStroke()
		{
			this.currentStroke = new List<MapEditor.MapEditAction>();
		}

		//-------------------------------------------------------------------------------
		// マップ編集の1操作分の履歴記録を確定する処理
		//-------------------------------------------------------------------------------
		private void EndMapEditStroke()
		{
			bool flag = this.currentStroke != null && this.currentStroke.Count > 0;
			if (flag)
			{
				this.undoStack.Push(this.currentStroke);
				this.redoStack.Clear();
				this.UpdateUndoRedoButtons();
			}
			this.currentStroke = null;
		}

		//-------------------------------------------------------------------------------
		// マップ編集履歴を追加または同一セルの変更履歴を更新する処理
		//-------------------------------------------------------------------------------
		private void RecordMapEditAction(int mapX, int mapY, int oldBlockIndex, int newBlockIndex, int oldCollision, int newCollision, bool isBlockEdit)
		{
			bool flag = this.currentStroke == null;
			if (flag)
			{
				this.BeginMapEditStroke();
			}
			int num = this.currentStroke.FindIndex((MapEditor.MapEditAction action) => action.MapX == mapX && action.MapY == mapY && action.IsBlockEdit == isBlockEdit);
			bool flag2 = num >= 0;
			if (flag2)
			{
				MapEditor.MapEditAction mapEditAction = this.currentStroke[num];
				mapEditAction.NewBlockIndex = newBlockIndex;
				mapEditAction.NewCollision = newCollision;
				this.currentStroke[num] = mapEditAction;
			}
			else
			{
				this.currentStroke.Add(new MapEditor.MapEditAction
				{
					MapX = mapX,
					MapY = mapY,
					OldBlockIndex = oldBlockIndex,
					NewBlockIndex = newBlockIndex,
					OldCollision = oldCollision,
					NewCollision = newCollision,
					IsBlockEdit = isBlockEdit
				});
			}
		}

		//-------------------------------------------------------------------------------
		// マップ編集履歴を消去してボタン状態を更新する処理
		//-------------------------------------------------------------------------------
		private void ClearMapEditHistory()
		{
			this.undoStack.Clear();
			this.redoStack.Clear();
			this.currentStroke = null;
			this.UpdateUndoRedoButtons();
		}

		//-------------------------------------------------------------------------------
		// Undo/Redoボタンの有効状態を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateUndoRedoButtons()
		{
			bool flag = this.btnUndo != null;
			if (flag)
			{
				this.btnUndo.Enabled = this.undoStack.Count > 0;
			}
			bool flag2 = this.btnRedo != null;
			if (flag2)
			{
				this.btnRedo.Enabled = this.redoStack.Count > 0;
			}
		}

		//-------------------------------------------------------------------------------
		// マップ編集履歴をマップデータに適用する処理
		//-------------------------------------------------------------------------------
		private void ApplyMapEditActions(List<MapEditor.MapEditAction> actions, bool useNewValues)
		{
			bool flag = this.mapMatrix == null || actions == null;
			checked
			{
				if (!flag)
				{
					foreach (MapEditor.MapEditAction mapEditAction in actions)
					{
						bool flag2 = mapEditAction.MapX < 0 || mapEditAction.MapX >= this.mapMatrix.GetLength(0) || mapEditAction.MapY < 0 || mapEditAction.MapY >= this.mapMatrix.GetLength(1);
						if (!flag2)
						{
							bool flag3 = mapEditAction.IsBlockEdit;
							if (flag3)
							{
								this.mapMatrix[mapEditAction.MapX, mapEditAction.MapY].BlockIndex = (useNewValues ? mapEditAction.NewBlockIndex : mapEditAction.OldBlockIndex);
							}
							else
							{
								this.mapMatrix[mapEditAction.MapX, mapEditAction.MapY].Collision = (useNewValues ? mapEditAction.NewCollision : mapEditAction.OldCollision);
							}
						}
					}
					this.UpdatePrimaryMapLayer();
					this.UpdateMapRender();
					this.pnlMapCanvas.Invalidate();
					this.SetUnsavedChanges(true);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// マップ編集を1操作分元に戻す処理
		//-------------------------------------------------------------------------------
		private void ExecuteUndo()
		{
			this.EndMapEditStroke();
			bool flag = this.undoStack.Count == 0;
			if (!flag)
			{
				List<MapEditor.MapEditAction> list = this.undoStack.Pop();
				this.ApplyMapEditActions(list, false);
				this.redoStack.Push(list);
				this.UpdateUndoRedoButtons();
			}
		}

		//-------------------------------------------------------------------------------
		// 元に戻したマップ編集を1操作分やり直す処理
		//-------------------------------------------------------------------------------
		private void ExecuteRedo()
		{
			this.EndMapEditStroke();
			bool flag = this.redoStack.Count == 0;
			if (!flag)
			{
				List<MapEditor.MapEditAction> list = this.redoStack.Pop();
				this.ApplyMapEditActions(list, true);
				this.undoStack.Push(list);
				this.UpdateUndoRedoButtons();
			}
		}

		//-------------------------------------------------------------------------------
		// 戻るボタン押下時にUndoを実行する処理
		//-------------------------------------------------------------------------------
		private void btnUndo_Click(object sender, EventArgs e)
		{
			this.ExecuteUndo();
		}

		//-------------------------------------------------------------------------------
		// 進むボタン押下時にRedoを実行する処理
		//-------------------------------------------------------------------------------
		private void btnRedo_Click(object sender, EventArgs e)
		{
			this.ExecuteRedo();
		}

		//-------------------------------------------------------------------------------
		// KeyDownイベントによるUndo/Redoショートカットを処理する処理
		//-------------------------------------------------------------------------------
		private void MapEditor_KeyDown(object sender, KeyEventArgs e)
		{
			bool flag = this.TryProcessUndoRedoShortcut(e.KeyData);
			if (flag)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x00035068 File Offset: 0x00033268
		private void ScrollHandlers(object sender, ScrollEventArgs e)
		{
			bool flag = sender == this.vsbTilesetScroll || sender == this.hsbTilesetScroll;
			if (flag)
			{
				this.pnlTilesetPalette.Invalidate();
			}
			else
			{
				this.pnlMapCanvas.Invalidate();
			}
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x000350AC File Offset: 0x000332AC
		private void tvwMapSelector_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TabPage tabPage = (this.tabEditorMode != null) ? this.tabEditorMode.SelectedTab : null;
			this.UpdateMapSelectorSelectionStyle(e.Node);
			bool @checked = this.chkTerrainIdMode.Checked;
			if (@checked)
			{
				this.LoadTerrainIdMode(e.Node);
			}
			else
			{
				this.LoadNormalMode(e.Node);
			}
			this.ResetNewTabControls();
			this.SetEditorMode(tabPage ?? this.tabBlock);
		}

		private void UpdateMapSelectorSelectionStyle(TreeNode selectedNode)
		{
			bool flag = this.highlightedMapSelectorNode != null && this.highlightedMapSelectorNode.TreeView != null;
			if (flag)
			{
				this.highlightedMapSelectorNode.BackColor = Color.Empty;
				this.highlightedMapSelectorNode.ForeColor = Color.Empty;
			}
			this.highlightedMapSelectorNode = selectedNode;
			bool flag2 = selectedNode != null;
			if (flag2)
			{
				selectedNode.BackColor = Color.FromArgb(0, 120, 215);
				selectedNode.ForeColor = Color.White;
				selectedNode.EnsureVisible();
			}
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x000350F0 File Offset: 0x000332F0
		private void LoadTerrainIdMode(TreeNode node)
		{
			uint num = 0U;
			bool flag = ((node != null) ? node.Tag : null) != null && node.Tag is uint;
			if (flag)
			{
				num = Conversions.ToUInteger(node.Tag);
			}
			this.ResetEditorState();
			bool flag2 = (ulong)num == 0UL;
			if (!flag2)
			{
				this.tempHeader = new MapEditor.MapHeader
				{
					Bank = -1,
					Number = -1,
					FooterAddress = num,
					EventScriptAddress = 0U,
					MapScriptAddress = 0U,
					ConnectionAddress = 0U,
					MusicCode = 0,
					TerrainId = 0,
					MapNameId = 0,
					Sight = 0,
					Weather = 0,
					TerrainType = 0,
					Bicycle = 0,
					MapNameType = 0,
					Level = 0,
					BattleType = 0,
					Connections = new List<MapEditor.ConnectedMap>(),
					Persons = new List<MapEditor.PersonEvent>(),
					Warps = new List<MapEditor.WarpEvent>(),
					Traps = new List<MapEditor.TrapEvent>(),
					Signs = new List<MapEditor.SignEvent>()
				};
				this.originalHeader = this.tempHeader.Clone();
				this.isUpdatingUI = true;
				this.grpMapHeader.Enabled = true;
				this.grpMapHeaderAddress.Enabled = true;
				this.SetMapHeaderControlsEnabled(false);
				this.txtAddressMapFooter.Text = string.Format("{0:X8}", num);
				this.lblCurrentMap.Text = string.Format("現在マップ : マップ地形ID {0:D4}", checked(node.Index + 1));
				this.grpMapFooter.Enabled = true;
				this.grpTilesetDetail.Enabled = true;
				this.LoadFooterAndContent(num);
				this.selectedBlockRect = new Rectangle(0, 0, 1, 1);
				this.selectedCollisionIndex = 0;
				this.tabEditorMode.SelectedTab = this.tabBlock;
				this.chkShowConnectedMap.Checked = false;
				this.cmbEventType.SelectedIndex = 0;
				this.RefreshEventUI();
				this.isUpdatingUI = false;
				this.RefreshMapCanvas();
				this.SetUnsavedChanges(false);
			}
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x000352F4 File Offset: 0x000334F4
		private void LoadNormalMode(TreeNode node)
		{
			MapEditor.MapHeader mapHeader = ((node != null) ? node.Tag : null) as MapEditor.MapHeader;
			bool flag = mapHeader == null;
			if (flag)
			{
				this.ResetEditorState();
			}
			else
			{
				this.tempHeader = mapHeader.Clone();
				this.originalHeader = mapHeader.Clone();
				this.selectedBlockRect = new Rectangle(0, 0, 1, 1);
				this.selectedCollisionIndex = 0;
				this.tabEditorMode.SelectedTab = this.tabBlock;
				this.chkShowConnectedMap.Checked = false;
				this.cmbEventType.SelectedIndex = 0;
				this.RefreshEditorView(MapEditor.ViewUpdateLevel.FooterAndGraphics);
				this.SetUnsavedChanges(false);
				bool flag2 = this.tempHeader != null && (ulong)this.tempHeader.MapScriptAddress > 0UL;
				checked
				{
					if (flag2)
					{
						byte b = 0;
						bool flag3 = this.tempHeader.MapScripts != null && this.tempHeader.MapScripts.Count > 0;
						if (flag3)
						{
							b = this.tempHeader.MapScripts.Min((MapEditor.MapScriptEvent ms) => ms.Type);
						}
						int num = this.cmbMapScriptType.Items.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							string text = this.cmbMapScriptType.Items[i].ToString();
							bool flag4 = text.StartsWith(string.Format("[{0:X2}]", b));
							if (flag4)
							{
								this.cmbMapScriptType.SelectedIndex = i;
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x00035488 File Offset: 0x00033688
		private void tvwMapSelect_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			bool flag = this.isUpdatingUI;
			if (!flag)
			{
				bool flag2 = !this.ConfirmSaveIfNeeded();
				if (flag2)
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x000354B8 File Offset: 0x000336B8
		private void rbMapSort_CheckedChanged(object sender, EventArgs e)
		{
			bool @checked = ((RadioButton)sender).Checked;
			if (@checked)
			{
				bool flag = !this.ConfirmSaveIfNeeded();
				if (!flag)
				{
					this.RefreshMapTree();
					this.ResetEditorState();
				}
			}
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x000354F4 File Offset: 0x000336F4
		private void btnChangeAddressMapHeader_Click(object sender, EventArgs e)
		{
			bool @checked = this.chkTerrainIdMode.Checked;
			if (@checked)
			{
				this.txtAddressMapFooter.Text = this.FormatHexTo8Digits(this.txtAddressMapFooter.Text);
				uint num = 0;
				bool flag = !this.TryParseHex(this.txtAddressMapFooter.Text, ref num);
				if (flag)
				{
					MessageBox.Show("アドレスは16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					uint footerAddress = this.tempHeader.FooterAddress;
					this.tempHeader.FooterAddress = num;
					bool flag2 = num != footerAddress;
					if (flag2)
					{
						this.isUpdatingUI = true;
						this.LoadFooterAndContent(num);
						this.isUpdatingUI = false;
						this.SetUnsavedChanges(true);
						this.RefreshMapCanvas();
					}
				}
			}
			else
			{
				this.txtAddressMapFooter.Text = this.FormatHexTo8Digits(this.txtAddressMapFooter.Text);
				this.txtAddressEventScript.Text = this.FormatHexTo8Digits(this.txtAddressEventScript.Text);
				this.txtAddressMapScript.Text = this.FormatHexTo8Digits(this.txtAddressMapScript.Text);
				this.txtAddressMapConnection.Text = this.FormatHexTo8Digits(this.txtAddressMapConnection.Text);
				uint num2 = 0;
				uint num3 = 0;
				uint num4 = 0;
				uint num5 = 0;
				bool flag3 = !this.TryParseHex(this.txtAddressMapFooter.Text, ref num2) || !this.TryParseHex(this.txtAddressEventScript.Text, ref num3) || !this.TryParseHex(this.txtAddressMapScript.Text, ref num4) || !this.TryParseHex(this.txtAddressMapConnection.Text, ref num5);
				if (flag3)
				{
					MessageBox.Show("アドレスは16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					uint footerAddress2 = this.tempHeader.FooterAddress;
					uint eventScriptAddress = this.tempHeader.EventScriptAddress;
					uint mapScriptAddress = this.tempHeader.MapScriptAddress;
					uint connectionAddress = this.tempHeader.ConnectionAddress;
					this.tempHeader.FooterAddress = num2;
					this.tempHeader.EventScriptAddress = num3;
					this.tempHeader.MapScriptAddress = num4;
					this.tempHeader.ConnectionAddress = num5;
					bool flag4 = num5 != connectionAddress;
					if (flag4)
					{
						this.ReadConnections(this.tempHeader);
						this.RefreshConnectionUI();
					}
					bool flag5 = num3 != eventScriptAddress;
					if (flag5)
					{
						this.tempHeader.Persons = new List<MapEditor.PersonEvent>();
						this.tempHeader.Warps = new List<MapEditor.WarpEvent>();
						this.tempHeader.Traps = new List<MapEditor.TrapEvent>();
						this.tempHeader.Signs = new List<MapEditor.SignEvent>();
						bool flag6 = (ulong)num3 > 0UL;
						if (flag6)
						{
							this.ReadEvents(this.tempHeader);
						}
						this.RefreshEventUI();
						this.pnlMapCanvas.Invalidate();
					}
					bool flag7 = num4 != mapScriptAddress;
					if (flag7)
					{
						this.tempHeader.MapScripts = new List<MapEditor.MapScriptEvent>();
						bool flag8 = (ulong)num4 > 0UL;
						checked
						{
							if (flag8)
							{
								this.ReadMapScripts(this.tempHeader);
								this.grpEditMapScript.Enabled = true;
								byte b = 0;
								bool flag9 = this.tempHeader.MapScripts.Count > 0;
								if (flag9)
								{
									b = this.tempHeader.MapScripts.Min((MapEditor.MapScriptEvent ms) => ms.Type);
								}
								int num6 = this.cmbMapScriptType.Items.Count - 1;
								for (int i = 0; i <= num6; i++)
								{
									bool flag10 = this.cmbMapScriptType.Items[i].ToString().StartsWith(string.Format("[{0:X2}]", b));
									if (flag10)
									{
										this.cmbMapScriptType.SelectedIndex = i;
										break;
									}
								}
							}
							else
							{
								this.ResetControlsInContainer(this.grpEditMapScript);
								this.grpEditMapScript.Enabled = false;
							}
							this.RefreshMapScriptUI();
						}
					}
					bool flag11 = num2 != footerAddress2;
					if (flag11)
					{
						this.RefreshEditorView(MapEditor.ViewUpdateLevel.FooterAndGraphics);
					}
					this.CheckForMapHeaderChanges();
				}
			}
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x000358FC File Offset: 0x00033AFC
		private void btnChangeMapFooterData_Click(object sender, EventArgs e)
		{
			bool flag = this.tempFooter == null;
			if (!flag)
			{
				this.txtBorderDataAddress.Text = this.FormatHexTo8Digits(this.txtBorderDataAddress.Text);
				this.txtMapDataAddress.Text = this.FormatHexTo8Digits(this.txtMapDataAddress.Text);
				uint num = 0;
				uint num2 = 0;
				uint num3 = 0;
				uint num4 = 0;
				bool flag2 = !this.TryParseHex(this.txtBorderDataAddress.Text, ref num) || !this.TryParseHex(this.txtMapDataAddress.Text, ref num2) || !this.TryParseHex(this.txtTileset1Address.Text, ref num3) || !this.TryParseHex(this.txtTileset2Address.Text, ref num4);
				if (flag2)
				{
					MessageBox.Show("アドレスは16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					bool flag3 = this.tempFooter.BorderDataAddress != num || this.tempFooter.MapDataAddress != num2 || this.tempFooter.Tileset1Address != num3 || this.tempFooter.Tileset2Address != num4 || this.tempFooter.MapWidth != Convert.ToByte(this.nudMapWidth.Value) || this.tempFooter.MapHeight != Convert.ToByte(this.nudMapHeight.Value) || this.tempFooter.BorderWidth != Convert.ToByte(this.nudBorderWidth.Value) || this.tempFooter.BorderHeight != Convert.ToByte(this.nudBorderHeight.Value);
					bool flag4 = !flag3;
					if (!flag4)
					{
						uint tileset1Address = this.tempFooter.Tileset1Address;
						uint tileset2Address = this.tempFooter.Tileset2Address;
						this.tempFooter.BorderDataAddress = num;
						this.tempFooter.MapDataAddress = num2;
						this.tempFooter.Tileset1Address = num3;
						this.tempFooter.Tileset2Address = num4;
						this.tempFooter.MapWidth = Convert.ToByte(this.nudMapWidth.Value);
						this.tempFooter.MapHeight = Convert.ToByte(this.nudMapHeight.Value);
						this.tempFooter.BorderWidth = Convert.ToByte(this.nudBorderWidth.Value);
						this.tempFooter.BorderHeight = Convert.ToByte(this.nudBorderHeight.Value);
						this.isUpdatingUI = true;
						bool flag5 = num3 != tileset1Address;
						if (flag5)
						{
							this.LoadTileset(1, num3);
						}
						bool flag6 = num4 != tileset2Address;
						if (flag6)
						{
							this.LoadTileset(2, num4);
						}
						this.mapMatrix = this.ReadMapDataMatrix(this.tempFooter);
						this.borderMatrix = this.ReadBorderDataMatrix(this.tempFooter);
						this.ClearMapEditHistory();
						this.isUpdatingUI = false;
						this.SetUnsavedChanges(true);
						this.RefreshEditorView(MapEditor.ViewUpdateLevel.GraphicsOnly);
					}
				}
			}
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x00035BC4 File Offset: 0x00033DC4
		private void btnChangeTilesetData_Click(object sender, EventArgs e)
		{
			bool flag = this.tempTileset1 == null || this.tempTileset2 == null;
			if (!flag)
			{
				bool flag2 = this.ProcessTilesetChanges(this.tileset1UI, this.tempTileset1);
				bool flag3 = this.ProcessTilesetChanges(this.tileset2UI, this.tempTileset2);
				bool flag4 = flag2 || flag3;
				if (flag4)
				{
					this.isUpdatingUI = true;
					this.UpdateAllGraphics();
					this.isUpdatingUI = false;
					this.SetUnsavedChanges(true);
					this.RefreshMapCanvas();
				}
			}
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x00035C44 File Offset: 0x00033E44
		private bool ProcessTilesetChanges(MapEditor.TilesetUIContainer ui, MapEditor.TilesetHeader tempTS)
		{
			bool flag = false;
			uint num = 0;
			uint num2 = 0;
			uint num3 = 0;
			uint num4 = 0;
			uint num5 = 0;
			bool flag2 = !this.TryParseHex(ui.TxtImageAddress.Text, ref num) || !this.TryParseHex(ui.TxtPaletteAddress.Text, ref num2) || !this.TryParseHex(ui.TxtBlockImageAddress.Text, ref num3) || !this.TryParseHex(ui.TxtAnimationAddress.Text, ref num4) || !this.TryParseHex(ui.TxtBehaviorAddress.Text, ref num5);
			bool flag3;
			if (flag2)
			{
				MessageBox.Show("アドレスは16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				flag3 = false;
			}
			else
			{
				bool flag4 = tempTS.ImageCompressType != this.GetByteFromCombo(ui.CmbCompress);
				if (flag4)
				{
					tempTS.ImageCompressType = this.GetByteFromCombo(ui.CmbCompress);
					flag = true;
				}
				bool flag5 = tempTS.PaletteType != this.GetByteFromCombo(ui.CmbPaletteType);
				if (flag5)
				{
					tempTS.PaletteType = this.GetByteFromCombo(ui.CmbPaletteType);
					flag = true;
				}
				bool flag6 = tempTS.ImageAddress != num;
				if (flag6)
				{
					tempTS.ImageAddress = num;
					flag = true;
				}
				bool flag7 = tempTS.PaletteAddress != num2;
				if (flag7)
				{
					tempTS.PaletteAddress = num2;
					flag = true;
				}
				bool flag8 = tempTS.BlockImageAddress != num3;
				if (flag8)
				{
					tempTS.BlockImageAddress = num3;
					flag = true;
				}
				bool flag9 = tempTS.AnimationAddress != num4;
				if (flag9)
				{
					tempTS.AnimationAddress = num4;
					flag = true;
				}
				bool flag10 = tempTS.BlockBehaviorAddress != num5;
				if (flag10)
				{
					tempTS.BlockBehaviorAddress = num5;
					flag = true;
				}
				flag3 = flag;
			}
			return flag3;
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x00035DDC File Offset: 0x00033FDC
		private void SyncHeaderFromUI()
		{
			this.tempHeader.MusicCode = Convert.ToUInt16(this.nudMusicCode.Value);
			this.tempHeader.TerrainId = Convert.ToUInt16(this.nudTerrainId.Value);
			this.tempHeader.Level = Convert.ToSByte(this.nudLevel.Value);
			this.tempHeader.MapNameId = this.GetByteFromCombo(this.cmbMapNameId);
			this.tempHeader.Sight = this.GetByteFromCombo(this.cmbSight);
			this.tempHeader.Weather = this.GetByteFromCombo(this.cmbWeather);
			this.tempHeader.TerrainType = this.GetByteFromCombo(this.cmbTerrainType);
			this.tempHeader.Bicycle = this.GetByteFromCombo(this.cmbBicycle);
			this.tempHeader.MapNameType = this.GetByteFromCombo(this.cmbMapNameType);
			this.tempHeader.BattleType = this.GetByteFromCombo(this.cmbBattleType);
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x00035EDC File Offset: 0x000340DC
		private void CheckForMapHeaderChanges()
		{
			bool flag = this.tempHeader.FooterAddress != this.originalHeader.FooterAddress || this.tempHeader.EventScriptAddress != this.originalHeader.EventScriptAddress || this.tempHeader.MapScriptAddress != this.originalHeader.MapScriptAddress || this.tempHeader.ConnectionAddress != this.originalHeader.ConnectionAddress || decimal.Compare(this.nudMusicCode.Value, new decimal((int)this.originalHeader.MusicCode)) != 0 || decimal.Compare(this.nudTerrainId.Value, new decimal((int)this.originalHeader.TerrainId)) != 0 || decimal.Compare(this.nudLevel.Value, new decimal((int)this.originalHeader.Level)) != 0 || this.GetByteFromCombo(this.cmbMapNameId) != this.originalHeader.MapNameId || this.GetByteFromCombo(this.cmbSight) != this.originalHeader.Sight || this.GetByteFromCombo(this.cmbWeather) != this.originalHeader.Weather || this.GetByteFromCombo(this.cmbTerrainType) != this.originalHeader.TerrainType || this.GetByteFromCombo(this.cmbBicycle) != this.originalHeader.Bicycle || this.GetByteFromCombo(this.cmbMapNameType) != this.originalHeader.MapNameType || this.GetByteFromCombo(this.cmbBattleType) != this.originalHeader.BattleType;
			bool flag2 = !flag && this.tempHeader.Connections != null && this.originalHeader.Connections != null;
			checked
			{
				if (flag2)
				{
					bool flag3 = this.tempHeader.Connections.Count != this.originalHeader.Connections.Count;
					if (flag3)
					{
						flag = true;
					}
					else
					{
						int num = this.tempHeader.Connections.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							MapEditor.ConnectedMap connectedMap = this.tempHeader.Connections[i];
							MapEditor.ConnectedMap connectedMap2 = this.originalHeader.Connections[i];
							bool flag4 = connectedMap.Direction != connectedMap2.Direction || connectedMap.Shift != connectedMap2.Shift || connectedMap.Bank != connectedMap2.Bank || connectedMap.Number != connectedMap2.Number;
							if (flag4)
							{
								flag = true;
								break;
							}
						}
					}
				}
				this.SetUnsavedChanges(flag);
			}
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x00036188 File Offset: 0x00034388
		private void SyncTilesetAddressFromIndex(MapEditor.TilesetUIContainer ui, bool fromIndex)
		{
			bool flag = ui.IsUpdating || this.isUpdatingUI;
			checked
			{
				if (!flag)
				{
					ui.IsUpdating = true;
					if (fromIndex)
					{
						ui.TxtAddress.Text = string.Format("{0:X8}", (uint)(this.TILESET_INDEX_START_OFFSET + Convert.ToInt32(ui.NudIndex.Value) * 24));
					}
					else
					{
						uint num = 0;
						bool flag2 = this.TryParseHex(ui.TxtAddress.Text, ref num);
						if (flag2)
						{
							ui.NudIndex.Value = new decimal(unchecked((ulong)num < (ulong)((long)this.TILESET_INDEX_START_OFFSET)) ? 0 : ((int)((unchecked((ulong)num) - (ulong)(unchecked((long)this.TILESET_INDEX_START_OFFSET))) / 24UL)));
						}
					}
					ui.IsUpdating = false;
				}
			}
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x00036248 File Offset: 0x00034448
		private int AddressToTilesetIndex(uint address)
		{
			bool flag = (ulong)address < (ulong)((long)this.TILESET_INDEX_START_OFFSET);
			checked
			{
				int num = 0;
				if (flag)
				{
					num = 0;
				}
				else
				{
					num = (int)((unchecked((ulong)address) - (ulong)(unchecked((long)this.TILESET_INDEX_START_OFFSET))) / 24UL);
				}
				return num;
			}
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x0003627C File Offset: 0x0003447C
		private void UpdateTilesetInputMode(MapEditor.TilesetUIContainer ui)
		{
			bool flag = ui.IsUpdating || this.isUpdatingUI;
			if (!flag)
			{
				ui.NudIndex.Enabled = ui.RbIndex.Checked;
				ui.TxtAddress.Enabled = ui.RbAddress.Checked;
			}
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x000362D0 File Offset: 0x000344D0
		private void RefreshConnectionUI()
		{
			this.isUpdatingUI = true;
			bool flag = this.tempHeader != null && this.tempHeader.Connections != null && this.tempHeader.Connections.Count > 0;
			this.chkShowConnectedMap.Checked = flag && this.chkShowConnectedMap.Checked;
			this.chkShowConnectedMap.Enabled = flag;
			this.grpEditMapConnection.Enabled = flag;
			bool flag2 = this.chkShowConnectedMap.Checked && flag;
			this.nudConnectedMapIndex.Enabled = flag2;
			this.cmbConnectedMapDirection.Enabled = flag2;
			this.nudConnectedMapShift.Enabled = flag2;
			this.nudConnectedMapBank.Enabled = flag2;
			this.nudConnectedMapNumber.Enabled = flag2;
			bool flag3 = !flag2;
			if (flag3)
			{
				this.nudConnectedMapIndex.Minimum = 0m;
				this.nudConnectedMapIndex.Maximum = 0m;
				this.nudConnectedMapIndex.Value = 0m;
				this.cmbConnectedMapDirection.SelectedIndex = -1;
				this.nudConnectedMapShift.Value = 0m;
				this.nudConnectedMapBank.Value = 0m;
				this.nudConnectedMapNumber.Value = 0m;
			}
			else
			{
				int num = Math.Max(0, checked(this.tempHeader.Connections.Count - 1));
				this.nudConnectedMapIndex.Maximum = new decimal(num);
				bool flag4 = decimal.Compare(this.nudConnectedMapIndex.Value, new decimal(num)) > 0;
				if (flag4)
				{
					this.nudConnectedMapIndex.Value = new decimal(num);
				}
				int num2 = Convert.ToInt32(this.nudConnectedMapIndex.Value);
				bool flag5 = num2 >= 0 && num2 < this.tempHeader.Connections.Count;
				if (flag5)
				{
					MapEditor.ConnectedMap connectedMap = this.tempHeader.Connections[num2];
					this.SelectComboBoxByValue(this.cmbConnectedMapDirection, string.Format("[{0:X2}]", connectedMap.Direction));
					this.nudConnectedMapShift.Value = new decimal(connectedMap.Shift);
					this.nudConnectedMapBank.Value = new decimal((int)connectedMap.Bank);
					this.nudConnectedMapNumber.Value = new decimal((int)connectedMap.Number);
					this.UpdateShiftControlState();
				}
			}
			this.isUpdatingUI = false;
			this.UpdateLoadMapButtons();
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00036548 File Offset: 0x00034748
		private void SyncConnectionFromUI()
		{
			bool flag;
			if (!this.isUpdatingUI)
			{
				MapEditor.MapHeader mapHeader = this.tempHeader;
				if (((mapHeader != null) ? mapHeader.Connections : null) != null)
				{
					flag = this.tempHeader.Connections.Count == 0;
					goto IL_0033;
				}
			}
			flag = true;
			IL_0033:
			bool flag2 = flag;
			if (!flag2)
			{
				int num = Convert.ToInt32(this.nudConnectedMapIndex.Value);
				bool flag3 = num >= 0 && num < this.tempHeader.Connections.Count;
				if (flag3)
				{
					MapEditor.ConnectedMap connectedMap = this.tempHeader.Connections[num];
					connectedMap.Direction = this.GetByteFromCombo(this.cmbConnectedMapDirection);
					connectedMap.Shift = Convert.ToInt32(this.nudConnectedMapShift.Value);
					connectedMap.Bank = Convert.ToByte(this.nudConnectedMapBank.Value);
					connectedMap.Number = Convert.ToByte(this.nudConnectedMapNumber.Value);
					this.SetUnsavedChanges(true);
				}
			}
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x00036630 File Offset: 0x00034830
		private void OnConnectedMapUIChanged(object sender, EventArgs e)
		{
			bool flag = !this.isUpdatingUI;
			if (flag)
			{
				this.RefreshConnectionUI();
				this.UpdateMapRender();
				this.UpdateMapScrollBars();
				this.pnlMapCanvas.Invalidate();
			}
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x00036670 File Offset: 0x00034870
		private void OnConnectedMapDataChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI;
			if (!flag)
			{
				bool flag2 = sender == this.cmbConnectedMapDirection;
				if (flag2)
				{
					this.isUpdatingUI = true;
					this.UpdateShiftControlState();
					this.isUpdatingUI = false;
				}
				this.SyncConnectionFromUI();
				this.CheckForMapHeaderChanges();
				this.UpdateMapRender();
				this.UpdateMapScrollBars();
				this.pnlMapCanvas.Invalidate();
				this.UpdateLoadMapButtons();
			}
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x000366E0 File Offset: 0x000348E0
		private void UpdateShiftControlState()
		{
			bool flag = this.cmbConnectedMapDirection.SelectedIndex != -1;
			if (flag)
			{
				string text = this.cmbConnectedMapDirection.SelectedItem.ToString();
				bool flag2 = text.StartsWith("[00]");
				if (flag2)
				{
					this.nudConnectedMapShift.Value = 0m;
					this.nudConnectedMapShift.Enabled = false;
					this.nudConnectedMapBank.Value = 0m;
					this.nudConnectedMapBank.Enabled = false;
					this.nudConnectedMapNumber.Value = 0m;
					this.nudConnectedMapNumber.Enabled = false;
				}
				else
				{
					bool flag3 = text.StartsWith("[05]") || text.StartsWith("[06]");
					if (flag3)
					{
						this.nudConnectedMapShift.Value = 0m;
						this.nudConnectedMapShift.Enabled = false;
						this.nudConnectedMapBank.Enabled = true;
						this.nudConnectedMapNumber.Enabled = true;
					}
					else
					{
						this.nudConnectedMapShift.Enabled = true;
						this.nudConnectedMapBank.Enabled = true;
						this.nudConnectedMapNumber.Enabled = true;
					}
				}
			}
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x0003680C File Offset: 0x00034A0C
		private void UpdateLoadMapButtons()
		{
			this.btnLoadMapDown.Enabled = false;
			this.btnLoadMapUp.Enabled = false;
			this.btnLoadMapLeft.Enabled = false;
			this.btnLoadMapRight.Enabled = false;
			this.btnLoadMapDive.Enabled = false;
			this.btnLoadMapEmerge.Enabled = false;
			MapEditor.MapHeader mapHeader = this.tempHeader;
			bool flag = ((mapHeader != null) ? mapHeader.Connections : null) == null;
			if (!flag)
			{
				{
					foreach (MapEditor.ConnectedMap connectedMap in this.tempHeader.Connections)
					{
						switch (connectedMap.Direction)
						{
						case 1:
							this.btnLoadMapDown.Enabled = true;
							break;
						case 2:
							this.btnLoadMapUp.Enabled = true;
							break;
						case 3:
							this.btnLoadMapLeft.Enabled = true;
							break;
						case 4:
							this.btnLoadMapRight.Enabled = true;
							break;
						case 5:
							this.btnLoadMapDive.Enabled = true;
							break;
						case 6:
							this.btnLoadMapEmerge.Enabled = true;
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00036964 File Offset: 0x00034B64
		private void SetupMapLoadButtons()
		{
			this.btnLoadMapDown.Tag = 1;
			this.btnLoadMapUp.Tag = 2;
			this.btnLoadMapLeft.Tag = 3;
			this.btnLoadMapRight.Tag = 4;
			this.btnLoadMapDive.Tag = 5;
			this.btnLoadMapEmerge.Tag = 6;
			this.btnLoadMapDown.Click += this.LoadMapButtonClick;
			this.btnLoadMapUp.Click += this.LoadMapButtonClick;
			this.btnLoadMapLeft.Click += this.LoadMapButtonClick;
			this.btnLoadMapRight.Click += this.LoadMapButtonClick;
			this.btnLoadMapDive.Click += this.LoadMapButtonClick;
			this.btnLoadMapEmerge.Click += this.LoadMapButtonClick;
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00036A70 File Offset: 0x00034C70
		private void LoadMapButtonClick(object sender, EventArgs e)
		{
			byte direction = Conversions.ToByte(((Button)sender).Tag);
			MapEditor.MapHeader mapHeader = this.tempHeader;
			bool flag = ((mapHeader != null) ? mapHeader.Connections : null) == null;
			if (!flag)
			{
				MapEditor.ConnectedMap connectedMap = this.tempHeader.Connections.FirstOrDefault((MapEditor.ConnectedMap c) => c.Direction == direction);
				bool flag2 = connectedMap != null;
				if (flag2)
				{
					try
					{
						foreach (object obj in this.tvwMapSelector.Nodes)
						{
							TreeNode treeNode = (TreeNode)obj;
							try
							{
								foreach (object obj2 in treeNode.Nodes)
								{
									TreeNode treeNode2 = (TreeNode)obj2;
									MapEditor.MapHeader mapHeader2 = treeNode2.Tag as MapEditor.MapHeader;
									bool flag3 = mapHeader2 != null && mapHeader2.Bank == (int)connectedMap.Bank && mapHeader2.Number == (int)connectedMap.Number;
									if (flag3)
									{
										this.tvwMapSelector.SelectedNode = treeNode2;
										return;
									}
								}
							}
							finally
							{
							}
						}
					}
					finally
					{
					}
				}
			}
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x00036BE0 File Offset: 0x00034DE0
		private void SetupEventHandlers()
		{
			Control[] array = new Control[]
			{
				this.nudPersonNo, this.nudPersonSpriteNo, this.nudPersonUnknownB2Upper, this.nudPersonUnknownB2Lower, this.nudPersonPositionX, this.nudPersonPositionY, this.cmbPersonLayer, this.cmbPersonAction, this.nudPersonMovementRangeX, this.nudPersonMovementRangeY,
				this.nudPersonUnknownB11, this.nudPersonTrainer, this.nudPersonUnknownB13, this.nudPersonSight, this.txtPersonScript, this.txtPersonFlag, this.nudPersonUnknownB22, this.nudWarpPositionX, this.nudWarpPositionY, this.cmbWarpLayer,
				this.nudWarpToNo, this.nudWarpToMapBank, this.nudWarpToMapNumber, this.nudTrapScriptPositionX, this.nudTrapScriptPositionY, this.cmbTrapScriptLayer, this.nudTrapScriptUnknownB5, this.txtTrapScriptVarNumber, this.txtTrapScriptVarValue, this.nudTrapScriptUnknownB10,
				this.txtTrapScriptAddress, this.nudSignPositionX, this.nudSignPositionY, this.cmbSignLayer, this.cmbSignType, this.nudSignUnknownB6, this.txtSignScriptAddress
			};
			foreach (Control control in array)
			{
				bool flag = control is NumericUpDown;
				if (flag)
				{
					((NumericUpDown)control).ValueChanged += this.OnEventDataChanged;
				}
				else
				{
					bool flag2 = control is ComboBox;
					if (flag2)
					{
						((ComboBox)control).SelectedIndexChanged += this.OnEventDataChanged;
					}
					else
					{
						bool flag3 = control is TextBox;
						if (flag3)
						{
							((TextBox)control).TextChanged += this.OnEventDataChanged;
						}
					}
				}
			}
			this.cmbEventType.SelectedIndexChanged += this.OnEventTypeChanged;
			this.nudEventNo.ValueChanged += this.OnEventTypeOrIndexChanged;
			this.cmbMapScriptType.SelectedIndexChanged += this.cmbMapScriptType_SelectedIndexChanged;
			this.nudMapScriptListIndex.ValueChanged += this.nudMapScriptListIndex_ValueChanged;
		}

		private void SetupEventScriptPointerContextMenus()
		{
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("ポインタのコピー");
			toolStripMenuItem.Click += this.CopyEventScriptPointerMenuItem_Click;
			contextMenuStrip.Items.Add(toolStripMenuItem);
			contextMenuStrip.Opening += delegate(object sender, CancelEventArgs e)
			{
				uint num = 0U;
				string text = "";
				toolStripMenuItem.Enabled = this.TryGetSelectedEventScriptPointerOffset(ref num, ref text);
			};
			this.eventScriptPointerContextMenu = contextMenuStrip;
			this.RegisterEventScriptPointerContextTarget(this.txtPersonScript, this.lblPersonScriptAddress);
			this.RegisterEventScriptPointerContextTarget(this.txtTrapScriptAddress, this.lblTrapScriptAddress);
			this.RegisterEventScriptPointerContextTarget(this.txtSignScriptAddress, this.lblSignScriptAddress);
		}

		private void RegisterEventScriptPointerContextTarget(TextBox textBox, Label label)
		{
			textBox.ContextMenuStrip = this.eventScriptPointerContextMenu;
			label.Cursor = Cursors.Hand;
			label.MouseUp += this.EventScriptPointerContextTarget_MouseUp;
		}

		private void EventScriptPointerContextTarget_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = e.Button != MouseButtons.Right || this.eventScriptPointerContextMenu == null;
			if (flag)
			{
				return;
			}
			Control control = sender as Control;
			bool flag2 = control == null;
			if (!flag2)
			{
				this.eventScriptPointerContextMenu.Show(control, e.Location);
			}
		}

		private void CopyEventScriptPointerMenuItem_Click(object sender, EventArgs e)
		{
			uint num = 0U;
			string text = "";
			bool flag = !this.TryGetSelectedEventScriptPointerOffset(ref num, ref text);
			if (flag)
			{
				MessageBox.Show(text, "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				string text2 = string.Format("{0:X8}", checked(num + 134217728U));
				Clipboard.SetText(text2);
				MessageBox.Show(string.Format("ポインタアドレス {0} をコピーしました。", text2), "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}

		private bool TryGetSelectedEventScriptPointerOffset(ref uint pointerOffset, ref string errorMessage)
		{
			pointerOffset = 0U;
			errorMessage = "";
			bool flag = this.tempHeader == null || (ulong)this.tempHeader.EventScriptAddress == 0UL;
			bool flag2;
			if (flag)
			{
				errorMessage = "イベントデータが読み込まれていません。";
				flag2 = false;
			}
			else
			{
				string text = ((this.cmbEventType.SelectedItem != null) ? this.cmbEventType.SelectedItem.ToString() : "");
				bool flag3 = Operators.CompareString(text, "歩行グラフィック", false) == 0;
				if (flag3)
				{
					flag2 = this.TryGetEventScriptPointerOffset(4, (this.tempHeader.Persons != null) ? this.tempHeader.Persons.Count : 0, 24, 16, "NPCイベント", ref pointerOffset, ref errorMessage);
				}
				else
				{
					bool flag4 = Operators.CompareString(text, "踏むスクリプト", false) == 0;
					if (flag4)
					{
						flag2 = this.TryGetEventScriptPointerOffset(12, (this.tempHeader.Traps != null) ? this.tempHeader.Traps.Count : 0, 16, 12, "踏むスクリプトイベント", ref pointerOffset, ref errorMessage);
					}
					else
					{
						bool flag5 = Operators.CompareString(text, "看板", false) == 0;
						if (flag5)
						{
							flag2 = this.TryGetEventScriptPointerOffset(16, (this.tempHeader.Signs != null) ? this.tempHeader.Signs.Count : 0, 12, 8, "看板イベント", ref pointerOffset, ref errorMessage);
						}
						else
						{
							errorMessage = "スクリプトを持つイベントを選択してください。";
							flag2 = false;
						}
					}
				}
			}
			return flag2;
		}

		private bool TryGetEventScriptPointerOffset(int eventHeaderPointerOffset, int eventCount, int eventSize, int scriptPointerOffset, string eventName, ref uint pointerOffset, ref string errorMessage)
		{
			pointerOffset = 0U;
			errorMessage = "";
			bool flag = !this.nudEventNo.Enabled || eventCount == 0;
			bool flag2;
			if (flag)
			{
				errorMessage = eventName + "がありません。";
				flag2 = false;
			}
			else
			{
				int num = Convert.ToInt32(this.nudEventNo.Value);
				bool flag3 = num < 0 || num >= eventCount;
				if (flag3)
				{
					errorMessage = eventName + "番号がイベント数の範囲外です。";
					flag2 = false;
				}
				else
				{
					uint eventScriptAddress = this.tempHeader.EventScriptAddress;
					bool flag4 = !this.IsRomRange(eventScriptAddress, eventHeaderPointerOffset + 4);
					if (flag4)
					{
						errorMessage = "イベントヘッダの" + eventName + "配列ポインタを読み取れません。";
						flag2 = false;
					}
					else
					{
						uint ptr = BitConverter.ToUInt32(this.romData, checked((int)eventScriptAddress + eventHeaderPointerOffset));
						uint num2 = this.PointerToOffset(ptr);
						bool flag5 = (ulong)num2 == 0UL;
						if (flag5)
						{
							errorMessage = eventName + "配列のポインタが設定されていません。";
							flag2 = false;
						}
						else
						{
							uint num3 = checked(num2 + (uint)(num * eventSize + scriptPointerOffset));
							bool flag6 = !this.IsRomRange(num3, 4);
							if (flag6)
							{
								errorMessage = "スクリプトポインタの位置がROM範囲外です。";
								flag2 = false;
							}
							else
							{
								pointerOffset = num3;
								flag2 = true;
							}
						}
					}
				}
			}
			return flag2;
		}

		private bool IsRomRange(uint offset, int length)
		{
			return this.romData != null && length >= 0 && unchecked((ulong)offset) + (ulong)length <= (ulong)((long)this.romData.Length);
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00036E5C File Offset: 0x0003505C
		private void RefreshEventUI()
		{
			bool flag = this.isUpdatingUI || this.tempHeader == null;
			if (!flag)
			{
				this.isUpdatingUI = true;
				string text = ((this.cmbEventType.SelectedItem != null) ? this.cmbEventType.SelectedItem.ToString() : "");
				this.grpPersonEvent.Visible = Operators.CompareString(text, "歩行グラフィック", false) == 0;
				this.grpWarpEvent.Visible = Operators.CompareString(text, "ワープ", false) == 0;
				this.grpTrapScriptEvent.Visible = Operators.CompareString(text, "踏むスクリプト", false) == 0;
				this.grpSignEvent.Visible = Operators.CompareString(text, "看板", false) == 0;
				int num = 0;
				if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
				{
					if (Operators.CompareString(text, "ワープ", false) != 0)
					{
						if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
						{
							if (Operators.CompareString(text, "看板", false) == 0)
							{
								num = ((this.tempHeader.Signs != null) ? this.tempHeader.Signs.Count : 0);
							}
						}
						else
						{
							num = ((this.tempHeader.Traps != null) ? this.tempHeader.Traps.Count : 0);
						}
					}
					else
					{
						num = ((this.tempHeader.Warps != null) ? this.tempHeader.Warps.Count : 0);
					}
				}
				else
				{
					num = ((this.tempHeader.Persons != null) ? this.tempHeader.Persons.Count : 0);
				}
				bool flag2 = num == 0;
				if (flag2)
				{
					this.nudEventNo.Enabled = false;
					this.nudEventNo.Minimum = 0m;
					this.nudEventNo.Maximum = 0m;
					this.nudEventNo.Value = 0m;
					if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
					{
						if (Operators.CompareString(text, "ワープ", false) != 0)
						{
							if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
							{
								if (Operators.CompareString(text, "看板", false) == 0)
								{
									this.ResetAndDisableGroup(this.grpSignEvent);
								}
							}
							else
							{
								this.ResetAndDisableGroup(this.grpTrapScriptEvent);
							}
						}
						else
						{
							this.ResetAndDisableGroup(this.grpWarpEvent);
						}
					}
					else
					{
						this.ResetAndDisableGroup(this.grpPersonEvent);
					}
					this.isUpdatingUI = false;
				}
				else
				{
					this.nudEventNo.Enabled = true;
					if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
					{
						if (Operators.CompareString(text, "ワープ", false) != 0)
						{
							if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
							{
								if (Operators.CompareString(text, "看板", false) == 0)
								{
									this.grpSignEvent.Enabled = true;
								}
							}
							else
							{
								this.grpTrapScriptEvent.Enabled = true;
							}
						}
						else
						{
							this.grpWarpEvent.Enabled = true;
						}
					}
					else
					{
						this.grpPersonEvent.Enabled = true;
					}
					this.nudEventNo.Minimum = 0m;
					this.nudEventNo.Maximum = new decimal(checked(num - 1));
					bool flag3 = decimal.Compare(this.nudEventNo.Value, this.nudEventNo.Maximum) > 0;
					if (flag3)
					{
						this.nudEventNo.Value = this.nudEventNo.Maximum;
					}
					int num2 = Convert.ToInt32(this.nudEventNo.Value);
					if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
					{
						if (Operators.CompareString(text, "ワープ", false) != 0)
						{
							if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
							{
								if (Operators.CompareString(text, "看板", false) == 0)
								{
									MapEditor.SignEvent signEvent = this.tempHeader.Signs[num2];
									this.nudSignPositionX.Value = new decimal((int)signEvent.X);
									this.nudSignPositionY.Value = new decimal((int)signEvent.Y);
									this.SelectComboBoxByValue(this.cmbSignLayer, string.Format("[{0:X2}]", signEvent.Layer));
									this.SelectComboBoxByValue(this.cmbSignType, string.Format("[{0:X2}]", signEvent.SignType));
									this.nudSignUnknownB6.Value = new decimal((int)signEvent.UnknownB6);
									this.txtSignScriptAddress.Text = string.Format("{0:X8}", signEvent.ScriptAddress);
								}
							}
							else
							{
								MapEditor.TrapEvent trapEvent = this.tempHeader.Traps[num2];
								this.nudTrapScriptPositionX.Value = new decimal((int)trapEvent.X);
								this.nudTrapScriptPositionY.Value = new decimal((int)trapEvent.Y);
								this.SelectComboBoxByValue(this.cmbTrapScriptLayer, string.Format("[{0:X2}]", trapEvent.Layer));
								this.nudTrapScriptUnknownB5.Value = new decimal((int)trapEvent.UnknownB5);
								this.txtTrapScriptVarNumber.Text = string.Format("{0:X4}", trapEvent.VarNumber);
								this.txtTrapScriptVarValue.Text = string.Format("{0:X4}", trapEvent.VarValue);
								this.nudTrapScriptUnknownB10.Value = new decimal((int)trapEvent.UnknownB10);
								this.txtTrapScriptAddress.Text = string.Format("{0:X8}", trapEvent.ScriptAddress);
							}
						}
						else
						{
							MapEditor.WarpEvent warpEvent = this.tempHeader.Warps[num2];
							this.nudWarpPositionX.Value = new decimal((int)warpEvent.X);
							this.nudWarpPositionY.Value = new decimal((int)warpEvent.Y);
							this.SelectComboBoxByValue(this.cmbWarpLayer, string.Format("[{0:X2}]", warpEvent.Layer));
							this.nudWarpToNo.Value = new decimal((int)warpEvent.WarpToNo);
							this.nudWarpToMapBank.Value = new decimal((int)warpEvent.MapBank);
							this.nudWarpToMapNumber.Value = new decimal((int)warpEvent.MapNumber);
						}
					}
					else
					{
						MapEditor.PersonEvent personEvent = this.tempHeader.Persons[num2];
						this.nudPersonNo.Value = new decimal((int)personEvent.No);
						this.nudPersonSpriteNo.Value = new decimal((int)personEvent.SpriteNo);
						this.nudPersonUnknownB2Upper.Value = new decimal((int)personEvent.UnknownB2Upper);
						this.nudPersonUnknownB2Lower.Value = new decimal((int)personEvent.UnknownB2Lower);
						this.nudPersonPositionX.Value = new decimal((int)personEvent.X);
						this.nudPersonPositionY.Value = new decimal((int)personEvent.Y);
						this.SelectComboBoxByValue(this.cmbPersonLayer, string.Format("[{0:X2}]", personEvent.Layer));
						this.SelectComboBoxByValue(this.cmbPersonAction, string.Format("[{0:X2}]", personEvent.Action));
						this.nudPersonMovementRangeX.Value = new decimal((int)personEvent.MovementRangeX);
						this.nudPersonMovementRangeY.Value = new decimal((int)personEvent.MovementRangeY);
						this.nudPersonUnknownB11.Value = new decimal((int)personEvent.UnknownB11);
						this.nudPersonTrainer.Value = new decimal((int)personEvent.Trainer);
						this.nudPersonUnknownB13.Value = new decimal((int)personEvent.UnknownB13);
						this.nudPersonSight.Value = new decimal((int)personEvent.Sight);
						this.txtPersonScript.Text = string.Format("{0:X8}", personEvent.ScriptAddress);
						this.txtPersonFlag.Text = string.Format("{0:X4}", personEvent.Flag);
						this.nudPersonUnknownB22.Value = new decimal((int)personEvent.UnknownB22);
					}
					this.isUpdatingUI = false;
					this.pnlMapCanvas.Invalidate();
				}
			}
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00037674 File Offset: 0x00035874
		private void SyncEventFromUI()
		{
			bool flag = this.isUpdatingUI || this.tempHeader == null;
			if (!flag)
			{
				string text = ((this.cmbEventType.SelectedItem != null) ? this.cmbEventType.SelectedItem.ToString() : "");
				int num = Convert.ToInt32(this.nudEventNo.Value);
				if (Operators.CompareString(text, "歩行グラフィック", false) != 0)
				{
					if (Operators.CompareString(text, "ワープ", false) != 0)
					{
						if (Operators.CompareString(text, "踏むスクリプト", false) != 0)
						{
							if (Operators.CompareString(text, "看板", false) == 0)
							{
								bool flag2 = this.tempHeader.Signs != null && num < this.tempHeader.Signs.Count;
								if (flag2)
								{
									MapEditor.SignEvent signEvent = this.tempHeader.Signs[num];
									signEvent.X = Convert.ToUInt16(this.nudSignPositionX.Value);
									signEvent.Y = Convert.ToUInt16(this.nudSignPositionY.Value);
									signEvent.Layer = this.GetByteFromCombo(this.cmbSignLayer);
									signEvent.SignType = this.GetByteFromCombo(this.cmbSignType);
									signEvent.UnknownB6 = Convert.ToUInt16(this.nudSignUnknownB6.Value);
									signEvent.ScriptAddress = this.ParseHex8(this.txtSignScriptAddress.Text);
									this.SetUnsavedChanges(true);
								}
							}
						}
						else
						{
							bool flag3 = this.tempHeader.Traps != null && num < this.tempHeader.Traps.Count;
							if (flag3)
							{
								MapEditor.TrapEvent trapEvent = this.tempHeader.Traps[num];
								trapEvent.X = Convert.ToUInt16(this.nudTrapScriptPositionX.Value);
								trapEvent.Y = Convert.ToUInt16(this.nudTrapScriptPositionY.Value);
								trapEvent.Layer = this.GetByteFromCombo(this.cmbTrapScriptLayer);
								trapEvent.UnknownB5 = Convert.ToByte(this.nudTrapScriptUnknownB5.Value);
								trapEvent.VarNumber = this.ParseHex4(this.txtTrapScriptVarNumber.Text);
								trapEvent.VarValue = this.ParseHex4(this.txtTrapScriptVarValue.Text);
								trapEvent.UnknownB10 = Convert.ToUInt16(this.nudTrapScriptUnknownB10.Value);
								trapEvent.ScriptAddress = this.ParseHex8(this.txtTrapScriptAddress.Text);
								this.SetUnsavedChanges(true);
							}
						}
					}
					else
					{
						bool flag4 = this.tempHeader.Warps != null && num < this.tempHeader.Warps.Count;
						if (flag4)
						{
							MapEditor.WarpEvent warpEvent = this.tempHeader.Warps[num];
							warpEvent.X = Convert.ToUInt16(this.nudWarpPositionX.Value);
							warpEvent.Y = Convert.ToUInt16(this.nudWarpPositionY.Value);
							warpEvent.Layer = this.GetByteFromCombo(this.cmbWarpLayer);
							warpEvent.WarpToNo = Convert.ToByte(this.nudWarpToNo.Value);
							warpEvent.MapBank = Convert.ToByte(this.nudWarpToMapBank.Value);
							warpEvent.MapNumber = Convert.ToByte(this.nudWarpToMapNumber.Value);
							this.SetUnsavedChanges(true);
						}
					}
				}
				else
				{
					bool flag5 = this.tempHeader.Persons != null && num < this.tempHeader.Persons.Count;
					if (flag5)
					{
						MapEditor.PersonEvent personEvent = this.tempHeader.Persons[num];
						personEvent.No = Convert.ToByte(this.nudPersonNo.Value);
						personEvent.SpriteNo = Convert.ToByte(this.nudPersonSpriteNo.Value);
						personEvent.UnknownB2Upper = Convert.ToByte(this.nudPersonUnknownB2Upper.Value);
						personEvent.UnknownB2Lower = Convert.ToByte(this.nudPersonUnknownB2Lower.Value);
						personEvent.X = Convert.ToUInt16(this.nudPersonPositionX.Value);
						personEvent.Y = Convert.ToUInt16(this.nudPersonPositionY.Value);
						personEvent.Layer = this.GetByteFromCombo(this.cmbPersonLayer);
						personEvent.Action = this.GetByteFromCombo(this.cmbPersonAction);
						personEvent.MovementRangeX = Convert.ToByte(this.nudPersonMovementRangeX.Value);
						personEvent.MovementRangeY = Convert.ToByte(this.nudPersonMovementRangeY.Value);
						personEvent.UnknownB11 = Convert.ToByte(this.nudPersonUnknownB11.Value);
						personEvent.Trainer = Convert.ToByte(this.nudPersonTrainer.Value);
						personEvent.UnknownB13 = Convert.ToByte(this.nudPersonUnknownB13.Value);
						personEvent.Sight = Convert.ToUInt16(this.nudPersonSight.Value);
						personEvent.ScriptAddress = this.ParseHex8(this.txtPersonScript.Text);
						personEvent.Flag = this.ParseHex4(this.txtPersonFlag.Text);
						personEvent.UnknownB22 = Convert.ToUInt16(this.nudPersonUnknownB22.Value);
						this.SetUnsavedChanges(true);
					}
				}
			}
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x00037B95 File Offset: 0x00035D95
		private void OnEventTypeOrIndexChanged(object sender, EventArgs e)
		{
			this.RefreshEventUI();
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x00037BA0 File Offset: 0x00035DA0
		private void OnEventTypeChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI;
			if (!flag)
			{
				this.isUpdatingUI = true;
				this.nudEventNo.Value = 0m;
				this.isUpdatingUI = false;
				this.RefreshEventUI();
			}
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x00037BE0 File Offset: 0x00035DE0
		private void OnEventDataChanged(object sender, EventArgs e)
		{
			this.SyncEventFromUI();
			this.pnlMapCanvas.Invalidate();
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x00037BF8 File Offset: 0x00035DF8
		private void OnMapHeaderControlChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI || this.tempHeader == null;
			if (!flag)
			{
				this.SyncHeaderFromUI();
				this.CheckForMapHeaderChanges();
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00037C30 File Offset: 0x00035E30
		private void cmbMapScriptType_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI || this.tempHeader == null;
			if (!flag)
			{
				this.RefreshMapScriptUI();
			}
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00037C60 File Offset: 0x00035E60
		private void nudMapScriptListIndex_ValueChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI || this.tempHeader == null;
			if (!flag)
			{
				this.RefreshMapScriptListUI();
			}
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00037C90 File Offset: 0x00035E90
		private void RefreshMapScriptUI()
		{
			this.isUpdatingUI = true;
			string text = ((this.cmbMapScriptType.SelectedItem != null) ? this.cmbMapScriptType.SelectedItem.ToString() : "");
			byte typeByte = 0;
			bool flag = text.StartsWith("[") && text.Length >= 4;
			if (flag)
			{
				typeByte = Convert.ToByte(text.Substring(1, 2), 16);
			}
			bool flag2 = typeByte == 0;
			if (flag2)
			{
				this.txtMapScriptListAddress.Text = "";
				this.txtMapScriptListAddress.Enabled = false;
				this.txtMapScriptAddress.Text = "";
				this.txtMapScriptAddress.Enabled = false;
				this.txtMapScriptVar.Text = "";
				this.txtMapScriptVar.Enabled = false;
				this.txtMapScriptValue.Text = "";
				this.txtMapScriptValue.Enabled = false;
				this.nudMapScriptListIndex.Value = 0m;
				this.nudMapScriptListIndex.Enabled = false;
				this.isUpdatingUI = false;
			}
			else
			{
				List<MapEditor.MapScriptEvent> mapScripts = this.tempHeader.MapScripts;
				MapEditor.MapScriptEvent mapScriptEvent = ((mapScripts != null) ? mapScripts.FirstOrDefault((MapEditor.MapScriptEvent x) => x.Type == typeByte) : null);
				bool flag3 = mapScriptEvent == null;
				if (flag3)
				{
					this.txtMapScriptListAddress.Text = "";
					this.txtMapScriptListAddress.Enabled = false;
					this.txtMapScriptAddress.Text = "";
					this.txtMapScriptAddress.Enabled = false;
					this.txtMapScriptVar.Text = "";
					this.txtMapScriptVar.Enabled = false;
					this.txtMapScriptValue.Text = "";
					this.txtMapScriptValue.Enabled = false;
					this.nudMapScriptListIndex.Value = 0m;
					this.nudMapScriptListIndex.Enabled = false;
				}
				else
				{
					bool flag4 = typeByte == 2 || typeByte == 4;
					if (flag4)
					{
						this.txtMapScriptListAddress.Text = string.Format("{0:X8}", mapScriptEvent.Pointer);
						this.txtMapScriptListAddress.Enabled = true;
						int num = ((mapScriptEvent.ListEntries != null) ? mapScriptEvent.ListEntries.Count : 0);
						bool flag5 = num > 0;
						if (flag5)
						{
							this.nudMapScriptListIndex.Enabled = true;
							this.nudMapScriptListIndex.Maximum = new decimal(checked(num - 1));
							this.nudMapScriptListIndex.Value = 0m;
							this.txtMapScriptVar.Enabled = true;
							this.txtMapScriptValue.Enabled = true;
							this.txtMapScriptAddress.Enabled = true;
						}
						else
						{
							this.nudMapScriptListIndex.Enabled = false;
							this.nudMapScriptListIndex.Value = 0m;
							this.txtMapScriptVar.Text = "";
							this.txtMapScriptVar.Enabled = false;
							this.txtMapScriptValue.Text = "";
							this.txtMapScriptValue.Enabled = false;
							this.txtMapScriptAddress.Text = "";
							this.txtMapScriptAddress.Enabled = false;
						}
					}
					else
					{
						this.txtMapScriptListAddress.Text = "";
						this.txtMapScriptListAddress.Enabled = false;
						this.nudMapScriptListIndex.Value = 0m;
						this.nudMapScriptListIndex.Enabled = false;
						this.txtMapScriptVar.Text = "";
						this.txtMapScriptVar.Enabled = false;
						this.txtMapScriptValue.Text = "";
						this.txtMapScriptValue.Enabled = false;
						this.txtMapScriptAddress.Text = string.Format("{0:X8}", mapScriptEvent.Pointer);
						this.txtMapScriptAddress.Enabled = true;
					}
				}
				this.isUpdatingUI = false;
				this.RefreshMapScriptListUI();
			}
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x00038094 File Offset: 0x00036294
		private void RefreshMapScriptListUI()
		{
			bool flag = this.isUpdatingUI || this.tempHeader == null;
			if (!flag)
			{
				this.isUpdatingUI = true;
				byte typeByte = this.GetByteFromCombo(this.cmbMapScriptType);
				bool flag2 = typeByte == 2 || typeByte == 4;
				if (flag2)
				{
					List<MapEditor.MapScriptEvent> mapScripts = this.tempHeader.MapScripts;
					MapEditor.MapScriptEvent mapScriptEvent = ((mapScripts != null) ? mapScripts.FirstOrDefault((MapEditor.MapScriptEvent x) => x.Type == typeByte) : null);
					bool flag3 = mapScriptEvent != null && mapScriptEvent.ListEntries != null && mapScriptEvent.ListEntries.Count > 0;
					if (flag3)
					{
						int num = Convert.ToInt32(this.nudMapScriptListIndex.Value);
						bool flag4 = num >= 0 && num < mapScriptEvent.ListEntries.Count;
						if (flag4)
						{
							MapEditor.MapScriptListEntry mapScriptListEntry = mapScriptEvent.ListEntries[num];
							this.txtMapScriptVar.Text = string.Format("{0:X4}", mapScriptListEntry.VarNumber);
							this.txtMapScriptValue.Text = string.Format("{0:X4}", mapScriptListEntry.VarValue);
							this.txtMapScriptAddress.Text = string.Format("{0:X8}", mapScriptListEntry.ScriptAddress);
						}
					}
				}
				this.isUpdatingUI = false;
			}
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x000381F4 File Offset: 0x000363F4
		private void btnChangeMapScriptData_Click(object sender, EventArgs e)
		{
			bool flag = this.tempHeader == null;
			if (!flag)
			{
				byte typeByte = this.GetByteFromCombo(this.cmbMapScriptType);
				bool flag2 = typeByte == 0;
				if (!flag2)
				{
					List<MapEditor.MapScriptEvent> mapScripts = this.tempHeader.MapScripts;
					MapEditor.MapScriptEvent mapScriptEvent = ((mapScripts != null) ? mapScripts.FirstOrDefault((MapEditor.MapScriptEvent x) => x.Type == typeByte) : null);
					bool flag3 = mapScriptEvent == null;
					if (!flag3)
					{
						bool flag4 = false;
						bool flag5 = typeByte == 2 || typeByte == 4;
						if (flag5)
						{
							this.txtMapScriptListAddress.Text = this.FormatHexTo8Digits(this.txtMapScriptListAddress.Text);
							uint num = 0;
							bool flag6 = this.TryParseHex(this.txtMapScriptListAddress.Text, ref num) && mapScriptEvent.Pointer != num;
							if (flag6)
							{
								mapScriptEvent.Pointer = num;
								flag4 = true;
							}
							bool flag7 = mapScriptEvent.ListEntries != null && mapScriptEvent.ListEntries.Count > 0;
							if (flag7)
							{
								int num2 = Convert.ToInt32(this.nudMapScriptListIndex.Value);
								bool flag8 = num2 >= 0 && num2 < mapScriptEvent.ListEntries.Count;
								if (flag8)
								{
									MapEditor.MapScriptListEntry mapScriptListEntry = mapScriptEvent.ListEntries[num2];
									ushort num3 = this.ParseHex4(this.txtMapScriptVar.Text);
									ushort num4 = this.ParseHex4(this.txtMapScriptValue.Text);
									uint num5 = this.ParseHex8(this.txtMapScriptAddress.Text);
									this.txtMapScriptVar.Text = string.Format("{0:X4}", num3);
									this.txtMapScriptValue.Text = string.Format("{0:X4}", num4);
									this.txtMapScriptAddress.Text = this.FormatHexTo8Digits(this.txtMapScriptAddress.Text);
									bool flag9 = mapScriptListEntry.VarNumber != num3 || mapScriptListEntry.VarValue != num4 || mapScriptListEntry.ScriptAddress != num5;
									if (flag9)
									{
										mapScriptListEntry.VarNumber = num3;
										mapScriptListEntry.VarValue = num4;
										mapScriptListEntry.ScriptAddress = num5;
										flag4 = true;
									}
								}
							}
						}
						else
						{
							this.txtMapScriptAddress.Text = this.FormatHexTo8Digits(this.txtMapScriptAddress.Text);
							uint num6 = 0;
							bool flag10 = this.TryParseHex(this.txtMapScriptAddress.Text, ref num6) && mapScriptEvent.Pointer != num6;
							if (flag10)
							{
								mapScriptEvent.Pointer = num6;
								flag4 = true;
							}
						}
						bool flag11 = flag4;
						if (flag11)
						{
							this.SetUnsavedChanges(true);
						}
					}
				}
			}
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00038494 File Offset: 0x00036694
		private void btnSave_Click(object sender, EventArgs e)
		{
			bool @checked = this.chkTerrainIdMode.Checked;
			if (@checked)
			{
				this.SaveTerrainIdMode();
			}
			else
			{
				this.SaveNormalMode();
			}
			MainForm.romData = this.romData;
			this.originalHeader = this.tempHeader.Clone();
			this.SetUnsavedChanges(false);
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x000384EC File Offset: 0x000366EC
		private void SaveTerrainIdMode()
		{
			TreeNode selectedNode = this.tvwMapSelector.SelectedNode;
			bool flag = selectedNode != null;
			if (flag)
			{
				int index = selectedNode.Index;
				int num = checked(this.MAP_TERRAIN_ID_TABLE_OFFSET + index * 4);
				this.WritePointerToRom(num, this.tempHeader.FooterAddress);
				selectedNode.Tag = this.tempHeader.FooterAddress;
			}
			bool flag2 = this.tempFooter != null && (ulong)this.tempHeader.FooterAddress > 0UL;
			if (flag2)
			{
				this.WriteFooterToRom();
				this.WriteMapDataMatrixToRom();
				this.WriteBorderDataMatrixToRom();
				this.WriteTilesetToRom(1);
				this.WriteTilesetToRom(2);
			}
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x00038594 File Offset: 0x00036794
		private void SaveNormalMode()
		{
			this.SyncHeaderFromUI();
			this.WriteMapHeaderToRom();
			this.WriteConnectionsToRom();
			this.WriteEventsToRom();
			this.WriteMapScriptsToRom();
			this.ClearPendingRepointedEventData();
			MapEditor.MapHeader mapHeader = this.mapHeaders.FirstOrDefault((MapEditor.MapHeader h) => h.Bank == this.tempHeader.Bank && h.Number == this.tempHeader.Number);
			bool flag = mapHeader != null;
			if (flag)
			{
				mapHeader.CopyFrom(this.tempHeader);
			}
			bool flag2 = this.tempFooter != null && (ulong)this.tempHeader.FooterAddress > 0UL;
			if (flag2)
			{
				this.WriteFooterToRom();
				this.WriteMapDataMatrixToRom();
				this.WriteBorderDataMatrixToRom();
				this.WriteTilesetToRom(1);
				this.WriteTilesetToRom(2);
			}
			bool flag3 = this.chkSyncTerrainId.Checked && this.tempHeader != null;
			checked
			{
				if (flag3)
				{
					int terrainId = (int)this.tempHeader.TerrainId;
					int num = 1;
					bool flag4 = terrainId >= num && terrainId <= this.MAP_TERRAIN_ID_COUNT;
					if (flag4)
					{
						int num2 = terrainId - num;
						int num3 = this.MAP_TERRAIN_ID_TABLE_OFFSET + num2 * 4;
						this.WritePointerToRom(num3, this.tempHeader.FooterAddress);
					}
				}
			}
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x000386B0 File Offset: 0x000368B0
		private void btnMapScreenShot_Click(object sender, EventArgs e)
		{
			bool flag = this.mapBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = this.GetMapZoomScale();
					int num2 = this.mapBitmap.Width * num;
					int num3 = this.mapBitmap.Height * num;
					using (Bitmap bitmap = new Bitmap(num2, num3))
					{
						using (Graphics graphics = Graphics.FromImage(bitmap))
						{
							graphics.Clear(Color.White);
							this.DrawMapToGraphics(graphics, num, 0, 0, num2, num3);
						}
						using (SaveFileDialog saveFileDialog = new SaveFileDialog())
						{
							saveFileDialog.Filter = "PNG Image|*.png";
							saveFileDialog.Title = "マップ画像を保存";
							saveFileDialog.FileName = string.Format("map_{0}_{1}.png", this.tempHeader.Bank, this.tempHeader.Number);
							bool flag2 = saveFileDialog.ShowDialog() == DialogResult.OK;
							if (flag2)
							{
								bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x00038800 File Offset: 0x00036A00
		private void btnWarpGoTo_Click(object sender, EventArgs e)
		{
			int num = Convert.ToInt32(this.nudWarpToMapBank.Value);
			int num2 = Convert.ToInt32(this.nudWarpToMapNumber.Value);
			int num3 = Convert.ToInt32(this.nudWarpToNo.Value);
			TreeNode treeNode = null;
			try
			{
				foreach (object obj in this.tvwMapSelector.Nodes)
				{
					TreeNode treeNode2 = (TreeNode)obj;
					try
					{
						foreach (object obj2 in treeNode2.Nodes)
						{
							TreeNode treeNode3 = (TreeNode)obj2;
							MapEditor.MapHeader mapHeader = treeNode3.Tag as MapEditor.MapHeader;
							bool flag = mapHeader != null && mapHeader.Bank == num && mapHeader.Number == num2;
							if (flag)
							{
								treeNode = treeNode3;
								break;
							}
						}
					}
					finally
					{
					}
					bool flag2 = treeNode != null;
					if (flag2)
					{
						break;
					}
				}
			}
			finally
			{
			}
			this.tvwMapSelector.SelectedNode = treeNode;
			this.tabEditorMode.SelectedTab = this.tabEvent;
			this.cmbEventType.SelectedItem = "ワープ";
			bool flag3 = decimal.Compare(this.nudEventNo.Maximum, new decimal(num3)) >= 0;
			if (flag3)
			{
				this.nudEventNo.Value = new decimal(num3);
			}
			else
			{
				bool flag4 = decimal.Compare(this.nudEventNo.Maximum, 0m) >= 0;
				if (flag4)
				{
					this.nudEventNo.Value = this.nudEventNo.Maximum;
				}
			}
			this.pnlMapCanvas.Invalidate();
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x000389E0 File Offset: 0x00036BE0
		private void chkLoadTerrainIdTable_CheckedChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI | this.isSwitchingMode;
			checked
			{
				if (!flag)
				{
					bool flag2 = this.hasUnsavedChanges;
					if (flag2)
					{
						bool @checked = this.chkTerrainIdMode.Checked;
						bool flag3 = !@checked;
						this.isSwitchingMode = true;
						this.chkTerrainIdMode.Checked = flag3;
						DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
						if (dialogResult != DialogResult.Cancel)
						{
							if (dialogResult != DialogResult.Yes)
							{
								if (dialogResult == DialogResult.No)
								{
									this.SetUnsavedChanges(false);
									this.chkTerrainIdMode.Checked = @checked;
								}
							}
							else
							{
								this.btnSave_Click(null, null);
								this.chkTerrainIdMode.Checked = @checked;
							}
						}
						this.isSwitchingMode = false;
					}
					else
					{
						bool checked2 = this.chkTerrainIdMode.Checked;
						if (checked2)
						{
							this.isUpdatingUI = true;
							this.rbMapSortIndex.Checked = true;
							this.rbMapSortIndex.Enabled = false;
							this.rbMapSortName.Enabled = false;
							this.tvwMapSelector.BeginUpdate();
							this.tvwMapSelector.Nodes.Clear();
							int num = this.MAP_TERRAIN_ID_COUNT - 1;
							for (int i = 0; i <= num; i++)
							{
								int num2 = this.MAP_TERRAIN_ID_TABLE_OFFSET + i * 4;
								uint num3 = BitConverter.ToUInt32(this.romData, num2);
								uint num4 = this.PointerToOffset(num3);
								string text = string.Format("マップ地形ID {0:D4}", i + 1);
								TreeNode treeNode = new TreeNode(text)
								{
									Tag = num4
								};
								this.tvwMapSelector.Nodes.Add(treeNode);
							}
							this.tvwMapSelector.EndUpdate();
							this.isUpdatingUI = false;
							this.ResetEditorState();
						}
						else
						{
							this.isUpdatingUI = true;
							this.rbMapSortIndex.Enabled = true;
							this.rbMapSortName.Enabled = true;
							this.RefreshMapTree();
							this.isUpdatingUI = false;
							this.ResetEditorState();
						}
					}
				}
			}
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x00038BDB File Offset: 0x00036DDB
		private void chkShowGrid_CheckedChanged(object sender, EventArgs e)
		{
			this.RefreshMapCanvas();
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x00038BE5 File Offset: 0x00036DE5
		private void chkMapZoom2x_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateMapScrollBars();
			this.pnlMapCanvas.Invalidate();
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00038BFB File Offset: 0x00036DFB
		private void tabEditorMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdateMapEditModeSwitcher();
			this.pnlMapCanvas.Invalidate();
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00038C0A File Offset: 0x00036E0A
		private void pnlMapCanvas_Resize(object sender, EventArgs e)
		{
			this.UpdateMapScrollBars();
			this.pnlMapCanvas.Invalidate();
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x00038C20 File Offset: 0x00036E20
		private void pnlMapCanvas_MouseEnter(object sender, EventArgs e)
		{
			this.pnlMapCanvas.Focus();
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00038C30 File Offset: 0x00036E30
		private void pnlMapCanvas_MouseWheel(object sender, MouseEventArgs e)
		{
			int num = this.GetMapZoomScale() + ((e.Delta > 0) ? 1 : -1);
			this.SetMapZoomScale(num, e.Location);
			((HandledMouseEventArgs)e).Handled = true;
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00038CF4 File Offset: 0x00036EF4
		private void MapEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = !this.ConfirmSaveIfNeeded();
			if (flag)
			{
				e.Cancel = true;
			}
			else
			{
				Application.RemoveMessageFilter(this);
				bool flag2 = this.mapToolHostForm != null && !this.mapToolHostForm.IsDisposed;
				if (flag2)
				{
					this.mapToolHostForm.Close();
					this.mapToolHostForm.Dispose();
					this.mapToolHostForm = null;
				}
			}
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00038D18 File Offset: 0x00036F18
		private uint PointerToOffset(uint ptr)
		{
			return (ptr < 134217728U) ? 0U : checked(ptr - 134217728U);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x00038D3C File Offset: 0x00036F3C
		private void EnableDoubleBuffering(Control ctrl)
		{
			typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, ctrl, new object[] { true });
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x00038D78 File Offset: 0x00036F78
		private void btnOpenBlockEditor_Click(object sender, EventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult == DialogResult.Cancel)
				{
					return;
				}
				if (dialogResult != DialogResult.Yes)
				{
					if (dialogResult == DialogResult.No)
					{
						this.SetUnsavedChanges(false);
						bool flag2 = this.originalHeader != null;
						if (flag2)
						{
							this.tempHeader = this.originalHeader.Clone();
							this.RefreshEditorView(MapEditor.ViewUpdateLevel.FooterAndGraphics);
						}
					}
				}
				else
				{
					this.btnSave_Click(null, null);
				}
			}
			byte[] array = this.LoadTilesetRawImage(this.tempTileset1).Concat(this.LoadTilesetRawImage(this.tempTileset2)).ToArray<byte>();
			Color[] array2 = this.LoadAllPalettes(this.tempTileset1, this.tempTileset2);
			using (BlockEditor blockEditor = new BlockEditor(this.romData, this.blockPaletteBitmap, this.chkShowGrid.Checked, this.tempTileset1.BlockBehaviorAddress, this.tempTileset2.BlockBehaviorAddress, 640, this.totalBlocks, array, array2, this.tempTileset1.BlockImageAddress, this.tempTileset2.BlockImageAddress))
			{
				blockEditor.ShowDialog(this);
				this.RefreshEditorView(MapEditor.ViewUpdateLevel.GraphicsOnly);
			}
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00038EC0 File Offset: 0x000370C0
		private void ResetNewTabControls()
		{
			this.isUpdatingUI = true;
			try
			{
				this.ResetControlsInContainer(this.tabNew);
				this.cmbNewTilesetType.SelectedIndex = 0;
				this.cmbNewTilesetCompress.SelectedIndex = 0;
				this.nudNewTilesetBlockCount.Value = 640m;
				this.nudNewPaletteIndex.Maximum = 12m;
				this.nudNewMapScriptType02.Enabled = false;
				this.nudNewMapScriptType04.Enabled = false;
				this.cmbNewMapName.SelectedIndex = 0;
				this.SyncNewEventCountsFromCurrentMap();
				bool flag = this.chkNewEventAutoFindFreeSpace != null;
				if (flag)
				{
					this.chkNewEventAutoFindFreeSpace.Checked = true;
					this.UpdateNewEventAddressInputState();
				}
				this.pnlPalettePreview.Invalidate();
			}
			finally
			{
				this.isUpdatingUI = false;
			}
		}

		private void ConfigureNewEventAutoFreeSpaceUI()
		{
			bool flag = this.grpNewEvent == null || this.txtNewEventAddress == null || this.btnNewEvent == null;
			if (flag)
			{
				return;
			}
			bool flag2 = this.chkNewEventAutoFindFreeSpace == null;
			if (flag2)
			{
				this.chkNewEventAutoFindFreeSpace = new CheckBox();
				this.chkNewEventAutoFindFreeSpace.AutoSize = true;
				this.chkNewEventAutoFindFreeSpace.Name = "chkNewEventAutoFindFreeSpace";
				this.chkNewEventAutoFindFreeSpace.Text = "自動で空き容量を探す";
				this.chkNewEventAutoFindFreeSpace.CheckedChanged += this.chkNewEventAutoFindFreeSpace_CheckedChanged;
				this.grpNewEvent.Controls.Add(this.chkNewEventAutoFindFreeSpace);
			}
			this.lblNewEventAddress.Location = new Point(14, 120);
			this.txtNewEventAddress.Location = new Point(118, 116);
			this.chkNewEventAutoFindFreeSpace.Location = new Point(14, 140);
			this.btnNewEvent.Location = new Point(14, 164);
			this.grpNewEvent.Size = new Size(this.grpNewEvent.Width, Math.Max(this.grpNewEvent.Height, 202));
			this.chkNewEventAutoFindFreeSpace.Checked = true;
			this.UpdateNewEventAddressInputState();
		}

		private void chkNewEventAutoFindFreeSpace_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateNewEventAddressInputState();
		}

		private void UpdateNewEventAddressInputState()
		{
			bool flag = this.chkNewEventAutoFindFreeSpace != null && this.chkNewEventAutoFindFreeSpace.Checked;
			this.txtNewEventAddress.Enabled = !flag;
		}

		private bool ResolveNewEventAddress(NewDataGenerator.EventGenerator eventGenerator, ref uint address)
		{
			bool flag = this.chkNewEventAutoFindFreeSpace != null && this.chkNewEventAutoFindFreeSpace.Checked;
			if (flag)
			{
				int num = this.CalculateNewEventDataLength(eventGenerator);
				bool flag2 = !this.TryFindFreeSpaceForNewEvent(num, ref address);
				if (flag2)
				{
					MessageBox.Show(string.Format("空き領域が見つかりませんでした。\r\n必要バイト数: {0}", num), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return false;
				}
				this.txtNewEventAddress.Text = string.Format("{0:X8}", address);
				return true;
			}
			return this.ValidateAddressOnNewTab(this.txtNewEventAddress, ref address);
		}

		private int CalculateNewEventDataLength(NewDataGenerator.EventGenerator eventGenerator)
		{
			checked
			{
				return 20 + eventGenerator.PersonCount * 24 + eventGenerator.WarpCount * 8 + eventGenerator.TrapCount * 16 + eventGenerator.SignCount * 12;
			}
		}

		private bool TryFindFreeSpaceForNewEvent(int length, ref uint address)
		{
			bool flag = this.romData == null || length <= 0;
			if (flag)
			{
				return false;
			}
			uint startAddress = this.NormalizeRomAddress((uint)RomIniReader.ReadHexOrDecimal("FREE_SPACE_FINDER_OFFSET"));
			return this.TryFindAlignedFreeSpace(this.romData, startAddress, length, ref address);
		}

		private bool TryFindAlignedFreeSpace(byte[] rom, uint startAddress, int length, ref uint address)
		{
			bool flag = rom == null || length <= 0 || startAddress >= rom.Length;
			if (flag)
			{
				return false;
			}
			int num = this.AlignToFourBytes((int)startAddress);
			checked
			{
				while (num + length <= rom.Length)
				{
					bool flag2 = true;
					for (int i = 0; i < length; i++)
					{
						bool flag3 = rom[num + i] != byte.MaxValue;
						if (flag3)
						{
							flag2 = false;
							num = this.AlignToFourBytes(num + i + 1);
							break;
						}
					}
					if (flag2)
					{
						address = (uint)num;
						return true;
					}
				}
			}
			return false;
		}

		private int AlignToFourBytes(int value)
		{
			return checked(value + 3) & -4;
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00038F78 File Offset: 0x00037178
		private bool ConfirmSaveOnNewTab()
		{
			bool flag = !this.hasUnsavedChanges;
			bool flag2;
			if (flag)
			{
				flag2 = true;
			}
			else
			{
				DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Cancel)
				{
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult != DialogResult.No)
						{
							flag2 = false;
						}
						else
						{
							this.SetUnsavedChanges(false);
							bool flag3 = this.originalHeader != null;
							if (flag3)
							{
								this.tempHeader = this.originalHeader.Clone();
								this.RefreshEditorView(MapEditor.ViewUpdateLevel.FooterAndGraphics);
							}
							flag2 = true;
						}
					}
					else
					{
						this.btnSave_Click(null, null);
						flag2 = true;
					}
				}
				else
				{
					flag2 = false;
				}
			}
			return flag2;
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x0003900C File Offset: 0x0003720C
		private bool ValidateAddressOnNewTab(TextBox txtBox, ref uint address)
		{
			string text = txtBox.Text.Trim();
			bool flag = string.IsNullOrWhiteSpace(text);
			bool flag2;
			if (flag)
			{
				MessageBox.Show("書き込み先アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				flag2 = false;
			}
			else
			{
				bool flag3 = !this.TryParseHex(text, ref address);
				if (flag3)
				{
					MessageBox.Show("アドレスは16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					flag2 = false;
				}
				else
				{
					bool flag4 = !this.IsRomRange(address, 1);
					if (flag4)
					{
						MessageBox.Show("アドレスがROM範囲外です。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						flag2 = false;
					}
					else
					{
						txtBox.Text = string.Format("{0:X8}", address);
						flag2 = true;
					}
				}
			}
			return flag2;
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00039074 File Offset: 0x00037274
		private void OnDataGenerated(uint headerAddress, bool showMessage = true)
		{
			MainForm.romData = this.romData;
			Clipboard.SetText(string.Format("{0:X8}", headerAddress));
			bool flag = showMessage;
			if (flag)
			{
				MessageBox.Show(string.Format("アドレス {0:X8} をコピーしました。", headerAddress), "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}

		//-------------------------------------------------------------------------------
		// 新規イベント作成欄の個数を現在マップに合わせる処理
		//-------------------------------------------------------------------------------
		private void SyncNewEventCountsFromCurrentMap()
		{
			bool flag = this.tempHeader == null;
			if (flag)
			{
				return;
			}
			this.SetNumericValueWithinRange(this.nudNewEventPerson, (this.tempHeader.Persons != null) ? this.tempHeader.Persons.Count : 0);
			this.SetNumericValueWithinRange(this.nudNewEventWarp, (this.tempHeader.Warps != null) ? this.tempHeader.Warps.Count : 0);
			this.SetNumericValueWithinRange(this.nudNewEventTrap, (this.tempHeader.Traps != null) ? this.tempHeader.Traps.Count : 0);
			this.SetNumericValueWithinRange(this.nudNewEventSign, (this.tempHeader.Signs != null) ? this.tempHeader.Signs.Count : 0);
		}

		//-------------------------------------------------------------------------------
		// NumericUpDownへ範囲内の値を設定する処理
		//-------------------------------------------------------------------------------
		private void SetNumericValueWithinRange(NumericUpDown nud, int value)
		{
			decimal d = new decimal(value);
			bool flag = d < nud.Minimum;
			if (flag)
			{
				d = nud.Minimum;
			}
			bool flag2 = d > nud.Maximum;
			if (flag2)
			{
				d = nud.Maximum;
			}
			nud.Value = d;
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x000390C4 File Offset: 0x000372C4
		private void cmbNewTilesetType_SelectedIndexChanged(object sender, EventArgs e)
		{
			object selectedItem = this.cmbNewTilesetType.SelectedItem;
			string text = ((selectedItem != null) ? selectedItem.ToString() : null);
			bool flag = string.IsNullOrEmpty(text);
			if (!flag)
			{
				bool flag2 = text.StartsWith("[00]");
				if (flag2)
				{
					this.nudNewTilesetBlockCount.Maximum = 640m;
					this.nudNewTilesetBlockCount.Value = 640m;
					this.nudNewTilesetBlockCount.Enabled = false;
				}
				else
				{
					bool flag3 = text.StartsWith("[01]");
					if (flag3)
					{
						this.nudNewTilesetBlockCount.Maximum = 384m;
						this.nudNewTilesetBlockCount.Value = 384m;
						this.nudNewTilesetBlockCount.Enabled = true;
					}
				}
			}
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00039194 File Offset: 0x00037394
		private void btnSaveNewTileset_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			if (!flag)
			{
				uint num = 0;
				bool flag2 = !this.ValidateAddressOnNewTab(this.txtNewTilesetAddress, ref num);
				if (!flag2)
				{
					string text = this.cmbNewTilesetType.SelectedItem.ToString();
					bool flag3 = text.StartsWith("[00]");
					using (OpenFileDialog openFileDialog = new OpenFileDialog())
					{
						openFileDialog.Filter = "PNG Image|*.png";
						openFileDialog.Title = "タイルセット画像を選択";
						bool flag4 = openFileDialog.ShowDialog() != DialogResult.OK;
						if (!flag4)
						{
							using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
							{
								bool flag5 = bitmap.PixelFormat != PixelFormat.Format4bppIndexed;
								if (flag5)
								{
									MessageBox.Show("4bpp (16色) の画像を選択してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								}
								else
								{
									bool flag6 = flag3;
									if (flag6)
									{
										bool flag7 = bitmap.Width != 128 || bitmap.Height != 320;
										if (flag7)
										{
											MessageBox.Show("タイルセット1の画像サイズは128x320である必要があります。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
											return;
										}
									}
									else
									{
										bool flag8 = bitmap.Width != 128 || bitmap.Height > 192;
										if (flag8)
										{
											MessageBox.Show("タイルセット2の画像サイズは128x192以下である必要があります。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
											return;
										}
									}
									byte[] array = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
									bool flag9 = this.cmbNewTilesetCompress.SelectedItem.ToString().StartsWith("[01]");
									bool flag10 = flag9;
									if (flag10)
									{
										array = ImageProcessor.LZ77Comp(array, false);
									}
									NewDataGenerator.TilesetGenerator tilesetGenerator = new NewDataGenerator.TilesetGenerator
									{
										ImageBytes = array,
										PaletteType = this.GetByteFromCombo(this.cmbNewTilesetType),
										CompressType = this.GetByteFromCombo(this.cmbNewTilesetCompress),
										BlockCount = Convert.ToInt32(this.nudNewTilesetBlockCount.Value),
										TilesetIndexStartOffset = this.TILESET_INDEX_START_OFFSET
									};
									bool flag11 = tilesetGenerator.GenerateData(this.romData, num);
									if (flag11)
									{
										bool flag12 = !flag3 && tilesetGenerator.BlockCount < 384;
										if (flag12)
										{
											string text2 = this.FindWritableAssetPath("ini", "Tileset2BlockLimit.ini");
											bool flag13 = !this.tileset2BlockLimits.ContainsKey(tilesetGenerator.OutTilesetIndex);
											if (flag13)
											{
												using (StreamWriter streamWriter = new StreamWriter(text2, true, Encoding.UTF8))
												{
													streamWriter.WriteLine(string.Format("{0}={1}", tilesetGenerator.OutTilesetIndex, tilesetGenerator.BlockCount));
												}
												this.tileset2BlockLimits[tilesetGenerator.OutTilesetIndex] = tilesetGenerator.BlockCount;
											}
										}
										MainForm.romData = this.romData;
										Clipboard.SetText(tilesetGenerator.OutTilesetIndex.ToString());
										MessageBox.Show(string.Format("タイルセット番号 {0} をコピーしました。", tilesetGenerator.OutTilesetIndex), "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00039504 File Offset: 0x00037704
		private void pnlPalettePreview_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(Color.DarkGray);
			int num = Convert.ToInt32(this.nudNewPaletteTilesetIndex.Value);
			int num2 = Convert.ToInt32(this.nudNewPaletteIndex.Value);
			Color[] array = this.LoadPaletteForTileset(num, num2);
			int num3 = 0;
			checked
			{
				do
				{
					using (SolidBrush solidBrush = new SolidBrush(array[num3]))
					{
						e.Graphics.FillRectangle(solidBrush, num3 * 12, 0, 12, 12);
					}
					e.Graphics.DrawRectangle(Pens.Silver, num3 * 12, 0, 11, 11);
					num3++;
				}
				while (num3 <= 15);
			}
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x000395BC File Offset: 0x000377BC
		private Color[] LoadPaletteForTileset(int tilesetIndex, int paletteSlot)
		{
			Color[] array = new Color[16];
			int num = 0;
			checked
			{
				do
				{
					array[num] = Color.Black;
					num++;
				}
				while (num <= 15);
				int num2 = this.TILESET_INDEX_START_OFFSET + tilesetIndex * 24;
				uint num3 = BitConverter.ToUInt32(this.romData, num2 + 8);
				int num4 = (int)(num3 - 134217728U);
				byte[] array2 = new byte[32];
				Array.Copy(this.romData, num4 + paletteSlot * 32, array2, 0, 32);
				return ImageProcessor.LoadPalette(array2, false);
			}
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x00039640 File Offset: 0x00037840
		private void nudNewPalette_ValueChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI;
			if (!flag)
			{
				this.pnlPalettePreview.Invalidate();
			}
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00039668 File Offset: 0x00037868
		private void btnSaveNewPalette_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			if (!flag)
			{
				using (OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = "PNG画像|*.png",
					Title = "4bpp 16色パレット画像を選択"
				})
				{
					bool flag2 = openFileDialog.ShowDialog() != DialogResult.OK;
					if (!flag2)
					{
						using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
						{
							bool flag3 = bitmap.PixelFormat != PixelFormat.Format4bppIndexed;
							if (flag3)
							{
								MessageBox.Show("4bpp (16色) のPNGを選択してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							}
							else
							{
								NewDataGenerator.PaletteGenerator paletteGenerator = new NewDataGenerator.PaletteGenerator
								{
									TilesetIndex = Convert.ToInt32(this.nudNewPaletteTilesetIndex.Value),
									PaletteIndex = Convert.ToInt32(this.nudNewPaletteIndex.Value),
									SourcePalette = bitmap.Palette
								};
								bool flag4 = paletteGenerator.GenerateData(this.romData, 0U);
								if (flag4)
								{
									MainForm.romData = this.romData;
									this.pnlPalettePreview.Invalidate();
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x000397A0 File Offset: 0x000379A0
		private void btnNewMapFooter_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			if (!flag)
			{
				uint num = 0;
				bool flag2 = !this.ValidateAddressOnNewTab(this.txtNewMapFooterAddress, ref num);
				if (!flag2)
				{
					byte b = Convert.ToByte(this.nudNewMapFooterMapSizeX.Value);
					byte b2 = Convert.ToByte(this.nudNewMapFooterMapSizeY.Value);
					byte b3 = Convert.ToByte(this.nudNewMapFooterBorderSizeX.Value);
					byte b4 = Convert.ToByte(this.nudNewMapFooterBorderSizeY.Value);
					int num2 = Convert.ToInt32(this.nudNewMapFooterTileset1Index.Value);
					int num3 = Convert.ToInt32(this.nudNewMapFooterTileset2Index.Value);
					NewDataGenerator.MapFooterGenerator mapFooterGenerator = new NewDataGenerator.MapFooterGenerator();
					mapFooterGenerator.MapWidth = b;
					mapFooterGenerator.MapHeight = b2;
					mapFooterGenerator.BorderWidth = b3;
					mapFooterGenerator.BorderHeight = b4;
					mapFooterGenerator.Tileset1Index = num2;
					mapFooterGenerator.Tileset2Index = num3;
					mapFooterGenerator.TilesetIndexStartOffset = this.TILESET_INDEX_START_OFFSET;
					bool flag3 = mapFooterGenerator.GenerateData(this.romData, num);
					if (flag3)
					{
						this.OnDataGenerated(mapFooterGenerator.HeaderAddress);
					}
				}
			}
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x000398B8 File Offset: 0x00037AB8
		private void btnNewEvent_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			if (!flag)
			{
				uint num = 0;
				byte b = Convert.ToByte(this.nudNewEventPerson.Value);
				byte b2 = Convert.ToByte(this.nudNewEventSign.Value);
				byte b3 = Convert.ToByte(this.nudNewEventTrap.Value);
				byte b4 = Convert.ToByte(this.nudNewEventWarp.Value);
				NewDataGenerator.EventGenerator eventGenerator = new NewDataGenerator.EventGenerator
				{
					PersonCount = b,
					SignCount = b2,
					TrapCount = b3,
					WarpCount = b4
				};
				bool flag2 = !this.ResolveNewEventAddress(eventGenerator, ref num);
				if (!flag2)
				{
					bool flag3 = eventGenerator.GenerateData(this.romData, num);
					if (flag3)
					{
						this.OnDataGenerated(eventGenerator.HeaderAddress, false);
						this.ApplyGeneratedEventDataToCurrentMap(eventGenerator, false);
					}
					else
					{
						MessageBox.Show("イベントデータを書き込む領域がROM範囲外です。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 作成したイベントデータを現在マップへ適用する処理
		//-------------------------------------------------------------------------------
		private void ApplyGeneratedEventDataToCurrentMap(NewDataGenerator.EventGenerator eventGenerator, bool showConfirmation = true)
		{
			bool flag = this.tempHeader == null;
			if (flag)
			{
				return;
			}
			bool flag2 = showConfirmation;
			if (flag2)
			{
				DialogResult dialogResult = MessageBox.Show("作成したイベントデータを現在マップに設定しますか？\r\n既存イベントは新しい領域へ可能な限りコピーします。", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				bool flag3 = dialogResult != DialogResult.Yes;
				if (flag3)
				{
					return;
				}
			}
			uint eventScriptAddress = this.tempHeader.EventScriptAddress;
			this.tempHeader.EventScriptAddress = eventGenerator.HeaderAddress;
			bool flag4 = unchecked((ulong)eventScriptAddress) > 0UL && eventScriptAddress != eventGenerator.HeaderAddress;
			if (flag4)
			{
				this.pendingEventDataClearAddress = eventScriptAddress;
				this.pendingEventDataClearReplacementAddress = eventGenerator.HeaderAddress;
			}
			this.tempHeader.Persons = this.ResizeEventList<MapEditor.PersonEvent>(this.tempHeader.Persons, eventGenerator.PersonCount, (MapEditor.PersonEvent x) => x.Clone(), () => new MapEditor.PersonEvent());
			this.tempHeader.Warps = this.ResizeEventList<MapEditor.WarpEvent>(this.tempHeader.Warps, eventGenerator.WarpCount, (MapEditor.WarpEvent x) => x.Clone(), () => new MapEditor.WarpEvent());
			this.tempHeader.Traps = this.ResizeEventList<MapEditor.TrapEvent>(this.tempHeader.Traps, eventGenerator.TrapCount, (MapEditor.TrapEvent x) => x.Clone(), () => new MapEditor.TrapEvent());
			this.tempHeader.Signs = this.ResizeEventList<MapEditor.SignEvent>(this.tempHeader.Signs, eventGenerator.SignCount, (MapEditor.SignEvent x) => x.Clone(), () => new MapEditor.SignEvent());
			this.RefreshEditorView(MapEditor.ViewUpdateLevel.HeaderOnly);
			this.SetUnsavedChanges(true);
			this.tabMain.SelectedTab = this.tabMapEdit;
			this.SetEditorMode(this.tabEvent);
		}

		private void ClearPendingRepointedEventData()
		{
			bool flag = this.pendingEventDataClearAddress == 0U || this.pendingEventDataClearReplacementAddress == 0U;
			if (flag)
			{
				return;
			}
			bool flag2 = this.tempHeader == null || this.tempHeader.EventScriptAddress != this.pendingEventDataClearReplacementAddress;
			if (flag2)
			{
				this.pendingEventDataClearAddress = 0U;
				this.pendingEventDataClearReplacementAddress = 0U;
				return;
			}
			this.FillOldEventDefinitionDataWithFreeSpace(this.pendingEventDataClearAddress, this.pendingEventDataClearReplacementAddress);
			this.pendingEventDataClearAddress = 0U;
			this.pendingEventDataClearReplacementAddress = 0U;
		}

		private void FillOldEventDefinitionDataWithFreeSpace(uint oldHeaderAddress, uint newHeaderAddress)
		{
			bool flag = oldHeaderAddress == 0U || oldHeaderAddress == newHeaderAddress || !this.IsRomRange(oldHeaderAddress, 20);
			if (flag)
			{
				return;
			}
			List<Tuple<int, int>> list = this.GetEventDefinitionRanges(oldHeaderAddress);
			List<Tuple<int, int>> list2 = this.GetEventDefinitionRanges(newHeaderAddress);
			foreach (Tuple<int, int> tuple in list)
			{
				bool flag2 = this.DoesRangeOverlapAny(tuple, list2);
				if (!flag2)
				{
					for (int i = 0; i < tuple.Item2; i++)
					{
						this.romData[tuple.Item1 + i] = byte.MaxValue;
					}
				}
			}
		}

		private List<Tuple<int, int>> GetEventDefinitionRanges(uint headerAddress)
		{
			List<Tuple<int, int>> list = new List<Tuple<int, int>>();
			bool flag = !this.IsRomRange(headerAddress, 20);
			if (flag)
			{
				return list;
			}
			int num = (int)headerAddress;
			this.AddRomRange(list, num, 20);
			int personCount = this.romData[num + 0];
			int warpCount = this.romData[num + 1];
			int trapCount = this.romData[num + 2];
			int signCount = this.romData[num + 3];
			this.AddEventArrayRange(list, BitConverter.ToUInt32(this.romData, num + 4), personCount, 24);
			this.AddEventArrayRange(list, BitConverter.ToUInt32(this.romData, num + 8), warpCount, 8);
			this.AddEventArrayRange(list, BitConverter.ToUInt32(this.romData, num + 12), trapCount, 16);
			this.AddEventArrayRange(list, BitConverter.ToUInt32(this.romData, num + 16), signCount, 12);
			return list;
		}

		private void AddEventArrayRange(List<Tuple<int, int>> ranges, uint pointer, int count, int entrySize)
		{
			bool flag = count <= 0;
			if (flag)
			{
				return;
			}
			uint num = this.PointerToOffset(pointer);
			int length = checked(count * entrySize);
			bool flag2 = num == 0U || !this.IsRomRange(num, length);
			if (!flag2)
			{
				this.AddRomRange(ranges, (int)num, length);
			}
		}

		private void AddRomRange(List<Tuple<int, int>> ranges, int start, int length)
		{
			bool flag = length <= 0 || !this.IsRomRange((uint)start, length);
			if (!flag)
			{
				ranges.Add(Tuple.Create(start, length));
			}
		}

		private bool DoesRangeOverlapAny(Tuple<int, int> range, List<Tuple<int, int>> ranges)
		{
			foreach (Tuple<int, int> tuple in ranges)
			{
				bool flag = this.DoRangesOverlap(range, tuple);
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		private bool DoRangesOverlap(Tuple<int, int> left, Tuple<int, int> right)
		{
			return left.Item1 < checked(right.Item1 + right.Item2) && right.Item1 < checked(left.Item1 + left.Item2);
		}

		//-------------------------------------------------------------------------------
		// イベントリストを指定個数へ調整し、既存分をコピーする処理
		//-------------------------------------------------------------------------------
		private List<T> ResizeEventList<T>(List<T> source, int count, Func<T, T> cloneItem, Func<T> createItem)
		{
			List<T> list = new List<T>();
			checked
			{
				for (int i = 0; i < count; i++)
				{
					bool flag = source != null && i < source.Count;
					if (flag)
					{
						list.Add(cloneItem(source[i]));
					}
					else
					{
						list.Add(createItem());
					}
				}
			}
			return list;
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00039985 File Offset: 0x00037B85
		private void chkNewMapScriptType02_CheckedChanged(object sender, EventArgs e)
		{
			this.nudNewMapScriptType02.Enabled = this.chkNewMapScriptType02.Checked;
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x0003999F File Offset: 0x00037B9F
		private void chkNewMapScriptType04_CheckedChanged(object sender, EventArgs e)
		{
			this.nudNewMapScriptType04.Enabled = this.chkNewMapScriptType04.Checked;
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x000399BC File Offset: 0x00037BBC
		private void btnNewMapScript_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			if (!flag)
			{
				uint num = 0;
				bool flag2 = !this.ValidateAddressOnNewTab(this.txtNewMapScriptAddress, ref num);
				if (!flag2)
				{
					NewDataGenerator.MapScriptGenerator mapScriptGenerator = new NewDataGenerator.MapScriptGenerator
					{
						HasType01 = this.chkNewMapScriptType01.Checked,
						HasType02 = this.chkNewMapScriptType02.Checked,
						HasType03 = this.chkNewMapScriptType03.Checked,
						HasType04 = this.chkNewMapScriptType04.Checked,
						HasType05 = this.chkNewMapScriptType05.Checked,
						HasType06 = this.chkNewMapScriptType06.Checked,
						HasType07 = this.chkNewMapScriptType07.Checked,
						Type02Count = Convert.ToInt32(this.nudNewMapScriptType02.Value),
						Type04Count = Convert.ToInt32(this.nudNewMapScriptType04.Value)
					};
					bool flag3 = mapScriptGenerator.GenerateData(this.romData, num);
					if (flag3)
					{
						this.OnDataGenerated(mapScriptGenerator.HeaderAddress);
					}
				}
			}
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00039AC8 File Offset: 0x00037CC8
		private void btnNewMapConnection_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			if (!flag)
			{
				uint num = 0;
				bool flag2 = !this.ValidateAddressOnNewTab(this.txtNewMapConnectionAddress, ref num);
				if (!flag2)
				{
					byte b = Convert.ToByte(this.nudNewMapConnectionCount.Value);
					NewDataGenerator.MapConnectionGenerator mapConnectionGenerator = new NewDataGenerator.MapConnectionGenerator
					{
						ConnectionCount = b
					};
					bool flag3 = mapConnectionGenerator.GenerateData(this.romData, num);
					if (flag3)
					{
						this.OnDataGenerated(mapConnectionGenerator.HeaderAddress);
					}
				}
			}
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00039B40 File Offset: 0x00037D40
		private void btnNewMapName_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveOnNewTab();
			checked
			{
				if (!flag)
				{
					uint num = 0;
					bool flag2 = !this.ValidateAddressOnNewTab(this.txtNewMapAddress, ref num);
					if (!flag2)
					{
						int selectedIndex = this.cmbNewMapName.SelectedIndex;
						string text = this.txtNewMapNameNew.Text.Trim();
						bool flag3 = string.IsNullOrEmpty(text);
						if (flag3)
						{
							MessageBox.Show("新しいマップ名を入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						}
						else
						{
							byte[] array = TextConverter.PokemonStringToBytes(text, 11);
							Array.Copy(array, 0, this.romData, (int)num, array.Length);
							int num2 = this.MAP_NAME_TABLE_OFFSET + selectedIndex * 4;
							uint num3 = num + 134217728U;
							Array.Copy(BitConverter.GetBytes(num3), 0, this.romData, num2, 4);
							MainForm.romData = this.romData;
							int num4 = this.MAP_NAME_FIRST_INDEX + selectedIndex;
							string text2 = string.Format("[{0:X2}]{1}", num4, text);
							this.cmbMapNameId.Items[selectedIndex] = text2;
							this.cmbNewMapName.Items[selectedIndex] = text;
							this.RefreshMapTree();
						}
					}
				}
			}
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00039C60 File Offset: 0x00037E60
		private bool IsTripleLayerBlock(int blockId)
		{
			bool flag = this.tempTileset1 == null || this.tempTileset2 == null || blockId < 0 || blockId >= this.totalBlocks;
			checked
			{
				bool flag2;
				if (flag)
				{
					flag2 = false;
				}
				else
				{
					int num = 640;
					bool flag3 = blockId < num;
					uint num2 = 0;
					if (flag3)
					{
						num2 = this.tempTileset1.BlockBehaviorAddress + (uint)(blockId * 4);
					}
					else
					{
						num2 = this.tempTileset2.BlockBehaviorAddress + (uint)((blockId - num) * 4);
					}
					bool flag4 = unchecked((ulong)num2 == 0UL || (ulong)num2 >= (ulong)((long)this.romData.Length));
					if (flag4)
					{
						flag2 = false;
					}
					else
					{
						byte b = this.romData[(int)num2 + 3];
						flag2 = (b & 252) == 48;
					}
				}
				return flag2;
			}
		}

		//-------------------------------------------------------------------------------
		// ショートカットキーによるUndo/Redoを処理する処理
		//-------------------------------------------------------------------------------
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool flag = this.TryProcessUndoRedoShortcut(keyData);
			if (flag)
			{
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		//-------------------------------------------------------------------------------
		// マップエディタ内のCtrl+Z/Ctrl+Yを先取りしてUndo/Redoを実行する処理
		//-------------------------------------------------------------------------------
		public bool PreFilterMessage(ref Message m)
		{
			bool flag = m.Msg != 256 && m.Msg != 260;
			if (flag)
			{
				return false;
			}
			bool flag2 = !base.ContainsFocus;
			if (flag2)
			{
				return false;
			}
			Keys keyData = (Keys)((int)m.WParam) | Control.ModifierKeys;
			return this.TryProcessUndoRedoShortcut(keyData);
		}

		//-------------------------------------------------------------------------------
		// Ctrl+Z/Ctrl+Yの判定とUndo/Redo実行を共通化する処理
		//-------------------------------------------------------------------------------
		private bool TryProcessUndoRedoShortcut(Keys keyData)
		{
			Keys keys = keyData & Keys.KeyCode;
			Keys keys2 = keyData & Keys.Modifiers;
			bool flag = (keys2 & Keys.Control) == Keys.Control;
			if (!flag)
			{
				return false;
			}
			bool flag2 = keys == Keys.Z && (this.undoStack.Count > 0 || (this.currentStroke != null && this.currentStroke.Count > 0));
			if (flag2)
			{
				this.ExecuteUndo();
				return true;
			}
			bool flag3 = keys == Keys.Y && this.redoStack.Count > 0;
			if (flag3)
			{
				this.ExecuteRedo();
				return true;
			}
			return false;
		}

		// Token: 0x04000382 RID: 898
		public const int MAP_HEADER_ENTRY_LENGTH = 28;

		// Token: 0x04000383 RID: 899
		public const int OFFSET_MAP_FOOTER_ADDRESS = 0;

		// Token: 0x04000384 RID: 900
		public const int OFFSET_EVENT_SCRIPT_ADDRESS = 4;

		// Token: 0x04000385 RID: 901
		public const int OFFSET_MAP_SCRIPT_ADDRESS = 8;

		// Token: 0x04000386 RID: 902
		public const int OFFSET_MAP_CONNECTION_ADDRESS = 12;

		// Token: 0x04000387 RID: 903
		public const int OFFSET_MUSIC_CODE = 16;

		// Token: 0x04000388 RID: 904
		public const int OFFSET_MAP_TERRAIN_ID = 18;

		// Token: 0x04000389 RID: 905
		public const int OFFSET_MAP_NAME_ID = 20;

		// Token: 0x0400038A RID: 906
		public const int OFFSET_SIGHT = 21;

		// Token: 0x0400038B RID: 907
		public const int OFFSET_WEATHER = 22;

		// Token: 0x0400038C RID: 908
		public const int OFFSET_TERRAIN_TYPE = 23;

		// Token: 0x0400038D RID: 909
		public const int OFFSET_BICYCLE = 24;

		// Token: 0x0400038E RID: 910
		public const int OFFSET_MAP_NAME_TYPE = 25;

		// Token: 0x0400038F RID: 911
		public const int OFFSET_LEVEL = 26;

		// Token: 0x04000390 RID: 912
		public const int OFFSET_BATTLE_TYPE = 27;

		// Token: 0x04000391 RID: 913
		public const int MAP_FOOTER_ENTRY_LENGTH = 28;

		// Token: 0x04000392 RID: 914
		public const int OFFSET_MAP_WIDTH = 0;

		// Token: 0x04000393 RID: 915
		public const int OFFSET_MAP_HEIGHT = 4;

		// Token: 0x04000394 RID: 916
		public const int OFFSET_BORDER_DATA_ADDRESS = 8;

		// Token: 0x04000395 RID: 917
		public const int OFFSET_MAP_DATA_ADDRESS = 12;

		// Token: 0x04000396 RID: 918
		public const int OFFSET_TILESET1_ADDRESS = 16;

		// Token: 0x04000397 RID: 919
		public const int OFFSET_TILESET2_ADDRESS = 20;

		// Token: 0x04000398 RID: 920
		public const int OFFSET_BORDER_WIDTH = 24;

		// Token: 0x04000399 RID: 921
		public const int OFFSET_BORDER_HEIGHT = 25;

		// Token: 0x0400039A RID: 922
		public const int TILESET_ENTRY_LENGTH = 24;

		// Token: 0x0400039B RID: 923
		public const int OFFSET_IMAGE_COMPRESS_TYPE = 0;

		// Token: 0x0400039C RID: 924
		public const int OFFSET_PALETTE_TYPE = 1;

		// Token: 0x0400039D RID: 925
		public const int OFFSET_IMAGE_ADDRESS = 4;

		// Token: 0x0400039E RID: 926
		public const int OFFSET_PALETTE_ADDRESS = 8;

		// Token: 0x0400039F RID: 927
		public const int OFFSET_BLOCK_IMAGE_ADDRESS = 12;

		// Token: 0x040003A0 RID: 928
		public const int OFFSET_ANIMATION_ADDRESS = 16;

		// Token: 0x040003A1 RID: 929
		public const int OFFSET_BLOCK_BEHAVIOR_ADDRESS = 20;

		// Token: 0x040003A2 RID: 930
		public const int TILESET_IMAGE_WIDTH = 128;

		// Token: 0x040003A3 RID: 931
		public const int TILESET1_IMAGE_HEIGHT = 320;

		// Token: 0x040003A4 RID: 932
		public const int TILESET2_IMAGE_HEIGHT_MAX = 192;

		// Token: 0x040003A5 RID: 933
		public const int BYTES_PER_TILE = 32;

		// Token: 0x040003A6 RID: 934
		public const int COLORS_PER_PALETTE = 16;

		// Token: 0x040003A7 RID: 935
		public const int TOTAL_PALETTES = 13;

		// Token: 0x040003A8 RID: 936
		public const int PALETTE_BYTE_SIZE = 32;

		// Token: 0x040003A9 RID: 937
		public const int TILESET2_PALETTE_INDEX_START = 7;

		// Token: 0x040003AA RID: 938
		public const int BLOCK_IMAGE_ENTRY_LENGTH = 16;

		// Token: 0x040003AB RID: 939
		public const int TOTAL_BLOCKS_TILESET1 = 640;

		// Token: 0x040003AC RID: 940
		public const int TOTAL_BLOCKS_TILESET2_MAX = 384;

		// Token: 0x040003AD RID: 941
		public const int LAYER_ENTRY_LENGTH = 2;

		// Token: 0x040003AE RID: 942
		public const int BLOCK_SIZE_PX = 16;

		// Token: 0x040003AF RID: 943
		public const int TILE_SIZE_PX = 8;

		// Token: 0x040003B0 RID: 944
		public const int MAP_DATA_ENTRY_LENGTH = 2;

		// Token: 0x040003B1 RID: 945
		public const int MASK_BLOCK_ID = 1023;

		// Token: 0x040003B2 RID: 946
		public const int MASK_COLLISION = 64512;

		// Token: 0x040003B3 RID: 947
		public const int SHIFT_COLLISION = 10;

		// Token: 0x040003B4 RID: 948
		public const int CONNECTION_HEADER_LENGTH = 8;

		// Token: 0x040003B5 RID: 949
		public const int OFFSET_CONNECTION_COUNT = 0;

		// Token: 0x040003B6 RID: 950
		public const int OFFSET_CONNECTION_DATA_ADDRESS = 4;

		// Token: 0x040003B7 RID: 951
		public const int CONNECTION_DATA_LENGTH = 12;

		// Token: 0x040003B8 RID: 952
		public const int OFFSET_CONNECTION_DIRECTION = 0;

		// Token: 0x040003B9 RID: 953
		public const int OFFSET_CONNECTION_SHIFT = 4;

		// Token: 0x040003BA RID: 954
		public const int OFFSET_CONNECTION_BANK = 8;

		// Token: 0x040003BB RID: 955
		public const int OFFSET_CONNECTION_NUMBER = 9;

		// Token: 0x040003BC RID: 956
		public const int EVENT_HEADER_LENGTH = 20;

		// Token: 0x040003BD RID: 957
		public const int OFFSET_EVENT_PERSON_COUNT = 0;

		// Token: 0x040003BE RID: 958
		public const int OFFSET_EVENT_WARP_COUNT = 1;

		// Token: 0x040003BF RID: 959
		public const int OFFSET_EVENT_TRAP_COUNT = 2;

		// Token: 0x040003C0 RID: 960
		public const int OFFSET_EVENT_SIGN_COUNT = 3;

		// Token: 0x040003C1 RID: 961
		public const int OFFSET_EVENT_PERSON_ADDRESS = 4;

		// Token: 0x040003C2 RID: 962
		public const int OFFSET_EVENT_WARP_ADDRESS = 8;

		// Token: 0x040003C3 RID: 963
		public const int OFFSET_EVENT_TRAP_ADDRESS = 12;

		// Token: 0x040003C4 RID: 964
		public const int OFFSET_EVENT_SIGN_ADDRESS = 16;

		// Token: 0x040003C5 RID: 965
		public const int EVENT_PERSON_LENGTH = 24;

		// Token: 0x040003C6 RID: 966
		public const int EVENT_WARP_LENGTH = 8;

		// Token: 0x040003C7 RID: 967
		public const int EVENT_TRAP_LENGTH = 16;

		// Token: 0x040003C8 RID: 968
		public const int EVENT_SIGN_LENGTH = 12;

		// Token: 0x040003C9 RID: 969
		public const int OFFSET_PERSON_NO = 0;

		// Token: 0x040003CA RID: 970
		public const int OFFSET_PERSON_SPRITE_NO = 1;

		// Token: 0x040003CB RID: 971
		public const int OFFSET_PERSON_UNKNOWN_B2_UPPER = 2;

		// Token: 0x040003CC RID: 972
		public const int OFFSET_PERSON_UNKNOWN_B2_LOWER = 3;

		// Token: 0x040003CD RID: 973
		public const int OFFSET_PERSON_X = 4;

		// Token: 0x040003CE RID: 974
		public const int OFFSET_PERSON_Y = 6;

		// Token: 0x040003CF RID: 975
		public const int OFFSET_PERSON_LAYER = 8;

		// Token: 0x040003D0 RID: 976
		public const int OFFSET_PERSON_ACTION = 9;

		// Token: 0x040003D1 RID: 977
		public const int OFFSET_PERSON_MOVEMENT_RANGE = 10;

		// Token: 0x040003D2 RID: 978
		public const int OFFSET_PERSON_UNKNOWN_B11 = 11;

		// Token: 0x040003D3 RID: 979
		public const int OFFSET_PERSON_TRAINER = 12;

		// Token: 0x040003D4 RID: 980
		public const int OFFSET_PERSON_UNKNOWN_B13 = 13;

		// Token: 0x040003D5 RID: 981
		public const int OFFSET_PERSON_SIGHT = 14;

		// Token: 0x040003D6 RID: 982
		public const int OFFSET_PERSON_SCRIPT_ADDRESS = 16;

		// Token: 0x040003D7 RID: 983
		public const int OFFSET_PERSON_FLAG = 20;

		// Token: 0x040003D8 RID: 984
		public const int OFFSET_PERSON_UNKNOWN_B22 = 22;

		// Token: 0x040003D9 RID: 985
		public const int OFFSET_WARP_X = 0;

		// Token: 0x040003DA RID: 986
		public const int OFFSET_WARP_Y = 2;

		// Token: 0x040003DB RID: 987
		public const int OFFSET_WARP_LAYER = 4;

		// Token: 0x040003DC RID: 988
		public const int OFFSET_WARP_TO_NO = 5;

		// Token: 0x040003DD RID: 989
		public const int OFFSET_WARP_MAP_BANK = 7;

		// Token: 0x040003DE RID: 990
		public const int OFFSET_WARP_MAP_NUMBER = 6;

		// Token: 0x040003DF RID: 991
		public const int OFFSET_TRAP_X = 0;

		// Token: 0x040003E0 RID: 992
		public const int OFFSET_TRAP_Y = 2;

		// Token: 0x040003E1 RID: 993
		public const int OFFSET_TRAP_LAYER = 4;

		// Token: 0x040003E2 RID: 994
		public const int OFFSET_TRAP_UNKNOWN_B5 = 5;

		// Token: 0x040003E3 RID: 995
		public const int OFFSET_TRAP_VAR_NUMBER = 6;

		// Token: 0x040003E4 RID: 996
		public const int OFFSET_TRAP_VAR_VALUE = 8;

		// Token: 0x040003E5 RID: 997
		public const int OFFSET_TRAP_UNKNOWN_B10 = 10;

		// Token: 0x040003E6 RID: 998
		public const int OFFSET_TRAP_SCRIPT_ADDRESS = 12;

		// Token: 0x040003E7 RID: 999
		public const int OFFSET_SIGN_X = 0;

		// Token: 0x040003E8 RID: 1000
		public const int OFFSET_SIGN_Y = 2;

		// Token: 0x040003E9 RID: 1001
		public const int OFFSET_SIGN_LAYER = 4;

		// Token: 0x040003EA RID: 1002
		public const int OFFSET_SIGN_TYPE = 5;

		// Token: 0x040003EB RID: 1003
		public const int OFFSET_SIGN_UNKNOWN_B6 = 6;

		// Token: 0x040003EC RID: 1004
		public const int OFFSET_SIGN_SCRIPT_ADDRESS = 8;

		// Token: 0x040003ED RID: 1005
		private const int TILESET_IMAGE_BYTE_SIZE = 32768;

		// Token: 0x040003EE RID: 1006
		private const byte MAX_BYTE = 255;

		// Token: 0x040003EF RID: 1007
		private const float HALF_OPACITY = 0.6f;

		// Token: 0x040003F0 RID: 1008
		private const float GRAYSCALE_MATRIXX00 = 0.299f;

		// Token: 0x040003F1 RID: 1009
		private const float GRAYSCALE_MATRIXX01 = 0.587f;

		// Token: 0x040003F2 RID: 1010
		private const float GRAYSCALE_MATRIXX02 = 0.114f;

		// Token: 0x040003F3 RID: 1011
		private byte[] romData;

		// Token: 0x040003F4 RID: 1012
		private bool hasUnsavedChanges;

		// Token: 0x040003F5 RID: 1013
		private bool isUpdatingUI;

		// Token: 0x040003F6 RID: 1014
		private bool isSwitchingMode;

		// Token: 0x040003F7 RID: 1015
		private List<MapEditor.MapHeader> mapHeaders;

		// Token: 0x040003F8 RID: 1016
		private MapEditor.MapHeader originalHeader;

		// Token: 0x040003F9 RID: 1017
		private MapEditor.MapHeader tempHeader;

		// Token: 0x040003FA RID: 1018
		private MapEditor.MapFooter originalFooter;

		// Token: 0x040003FB RID: 1019
		private MapEditor.MapFooter tempFooter;

		// Token: 0x040003FC RID: 1020
		private MapEditor.TilesetHeader originalTileset1;

		// Token: 0x040003FD RID: 1021
		private MapEditor.TilesetHeader tempTileset1;

		// Token: 0x040003FE RID: 1022
		private MapEditor.TilesetHeader originalTileset2;

		// Token: 0x040003FF RID: 1023
		private MapEditor.TilesetHeader tempTileset2;

		// Token: 0x04000400 RID: 1024
		private MapEditor.TilesetUIContainer tileset1UI;

		// Token: 0x04000401 RID: 1025
		private MapEditor.TilesetUIContainer tileset2UI;

		// Token: 0x04000402 RID: 1026
		private Dictionary<int, int> tileset2BlockLimits;

		// Token: 0x04000403 RID: 1027
		private Bitmap blockPaletteBitmap;

		// Token: 0x04000404 RID: 1028
		private Bitmap mapBitmap;

		// Token: 0x04000405 RID: 1029
		private Bitmap borderBitmap;

		// Token: 0x04000406 RID: 1030
		private Bitmap collisionBitmap;

		// Token: 0x04000407 RID: 1031
		private MapEditor.MapCell[,] mapMatrix;

		// Token: 0x04000408 RID: 1032
		private int[,] borderMatrix;

		// Token: 0x04000409 RID: 1033
		private int totalBlocks;

		// Token: 0x0400040A RID: 1034
		private int primaryMapOffsetX;

		// Token: 0x0400040B RID: 1035
		private int primaryMapOffsetY;

		// Token: 0x0400040C RID: 1036
		private Bitmap primaryMapLayerBitmap;

		// Token: 0x0400040D RID: 1037
		private Bitmap connectedMapLayerBitmap;

		// Token: 0x0400040E RID: 1038
		private int cachedConnBank;

		// Token: 0x0400040F RID: 1039
		private int cachedConnNumber;

		// Token: 0x04000410 RID: 1040
		private MapEditor.MapCell[,] cachedConnMatrix;

		// Token: 0x04000411 RID: 1041
		private MapEditor.MapFooter cachedConnFooter;

		// Token: 0x04000412 RID: 1042
		private Rectangle selectedBlockRect;

		// Token: 0x04000413 RID: 1043
		private Point selectionAnchor;

		// Token: 0x04000414 RID: 1044
		private int selectedCollisionIndex;

		private Stack<List<MapEditor.MapEditAction>> undoStack;

		private Stack<List<MapEditor.MapEditAction>> redoStack;

		private List<MapEditor.MapEditAction> currentStroke;

		private int mapZoomScale;

		private Form mapToolHostForm;

		private Panel mapToolWindow;

		private Panel mapToolGrip;

		private Panel mapToolContentPanel;

		private ToolTip mapToolTip;

		private bool mapToolDragging;

		private Point mapToolDragOffset;

		private bool mapToolHostPositionInitialized;

		private ContextMenuStrip eventScriptPointerContextMenu;

		private Panel mapEditModeSwitcher;

		private Button btnMapEditModeBlock;

		private Button btnMapEditModeCollision;

		private Button btnMapEditModeEvent;

		private CheckBox chkNewEventAutoFindFreeSpace;

		private TreeNode highlightedMapSelectorNode;

		private uint pendingEventDataClearAddress;

		private uint pendingEventDataClearReplacementAddress;

		// Token: 0x04000415 RID: 1045
		private bool isSelectingBlocks;

		// Token: 0x04000416 RID: 1046
		private bool isPaintingMap;

		// Token: 0x04000417 RID: 1047
		private bool isPaintingBorder;

		// Token: 0x04000418 RID: 1048
		private bool isPaintingCollision;

		// Token: 0x04000419 RID: 1049
		private bool isDraggingEvent;

		// Token: 0x0400041A RID: 1050
		private Bitmap eventIconBitmap;

		// Token: 0x0400041B RID: 1051
		public readonly int MAP_NAME_TABLE_OFFSET;

		// Token: 0x0400041C RID: 1052
		public readonly int MAP_NAME_FIRST_INDEX;

		// Token: 0x0400041D RID: 1053
		public readonly int MAP_NAME_COUNT;

		// Token: 0x0400041E RID: 1054
		public readonly int MAP_BANK_TABLE_OFFSET;

		// Token: 0x0400041F RID: 1055
		public readonly int TILESET_INDEX_START_OFFSET;

		// Token: 0x04000420 RID: 1056
		public readonly int MAP_TERRAIN_ID_TABLE_OFFSET;

		// Token: 0x04000421 RID: 1057
		public readonly int MAP_TERRAIN_ID_COUNT;

		// Token: 0x02000041 RID: 65
		private enum ViewUpdateLevel
		{
			// Token: 0x04000865 RID: 2149
			None,
			// Token: 0x04000866 RID: 2150
			HeaderOnly,
			// Token: 0x04000867 RID: 2151
			FooterAndGraphics,
			// Token: 0x04000868 RID: 2152
			GraphicsOnly
		}

		// Token: 0x02000042 RID: 66
		private struct MapCell
		{
			// Token: 0x04000869 RID: 2153
			public int BlockIndex;

			// Token: 0x0400086A RID: 2154
			public int Collision;
		}

		private struct MapEditAction
		{
			public int MapX;

			public int MapY;

			public int OldBlockIndex;

			public int NewBlockIndex;

			public int OldCollision;

			public int NewCollision;

			public bool IsBlockEdit;
		}

		// Token: 0x02000043 RID: 67
		public class MapHeader
		{
			// Token: 0x06000F56 RID: 3926 RVA: 0x0006B93C File Offset: 0x00069B3C
			public MapEditor.MapHeader Clone()
			{
				MapEditor.MapHeader mapHeader = (MapEditor.MapHeader)base.MemberwiseClone();
				bool flag = this.Connections != null;
				if (flag)
				{
					mapHeader.Connections = new List<MapEditor.ConnectedMap>(this.Connections.Select((MapEditor.ConnectedMap c) => c.Clone()));
				}
				bool flag2 = this.Persons != null;
				if (flag2)
				{
					mapHeader.Persons = new List<MapEditor.PersonEvent>(this.Persons.Select((MapEditor.PersonEvent c) => c.Clone()));
				}
				bool flag3 = this.Warps != null;
				if (flag3)
				{
					mapHeader.Warps = new List<MapEditor.WarpEvent>(this.Warps.Select((MapEditor.WarpEvent c) => c.Clone()));
				}
				bool flag4 = this.Traps != null;
				if (flag4)
				{
					mapHeader.Traps = new List<MapEditor.TrapEvent>(this.Traps.Select((MapEditor.TrapEvent c) => c.Clone()));
				}
				bool flag5 = this.Signs != null;
				if (flag5)
				{
					mapHeader.Signs = new List<MapEditor.SignEvent>(this.Signs.Select((MapEditor.SignEvent c) => c.Clone()));
				}
				bool flag6 = this.MapScripts != null;
				if (flag6)
				{
					mapHeader.MapScripts = new List<MapEditor.MapScriptEvent>(this.MapScripts.Select((MapEditor.MapScriptEvent c) => c.Clone()));
				}
				return mapHeader;
			}

			// Token: 0x06000F57 RID: 3927 RVA: 0x0006BB10 File Offset: 0x00069D10
			public void CopyFrom(MapEditor.MapHeader source)
			{
				this.FooterAddress = source.FooterAddress;
				this.EventScriptAddress = source.EventScriptAddress;
				this.MapScriptAddress = source.MapScriptAddress;
				this.ConnectionAddress = source.ConnectionAddress;
				this.MusicCode = source.MusicCode;
				this.TerrainId = source.TerrainId;
				this.MapNameId = source.MapNameId;
				this.Sight = source.Sight;
				this.Weather = source.Weather;
				this.TerrainType = source.TerrainType;
				this.Bicycle = source.Bicycle;
				this.MapNameType = source.MapNameType;
				this.Level = source.Level;
				this.BattleType = source.BattleType;
				this.Connections = ((source.Connections != null) ? new List<MapEditor.ConnectedMap>(source.Connections.Select((MapEditor.ConnectedMap c) => c.Clone())) : new List<MapEditor.ConnectedMap>());
				this.Persons = ((source.Persons != null) ? new List<MapEditor.PersonEvent>(source.Persons.Select((MapEditor.PersonEvent c) => c.Clone())) : new List<MapEditor.PersonEvent>());
				this.Warps = ((source.Warps != null) ? new List<MapEditor.WarpEvent>(source.Warps.Select((MapEditor.WarpEvent c) => c.Clone())) : new List<MapEditor.WarpEvent>());
				this.Traps = ((source.Traps != null) ? new List<MapEditor.TrapEvent>(source.Traps.Select((MapEditor.TrapEvent c) => c.Clone())) : new List<MapEditor.TrapEvent>());
				this.Signs = ((source.Signs != null) ? new List<MapEditor.SignEvent>(source.Signs.Select((MapEditor.SignEvent c) => c.Clone())) : new List<MapEditor.SignEvent>());
				this.MapScripts = ((source.MapScripts != null) ? new List<MapEditor.MapScriptEvent>(source.MapScripts.Select((MapEditor.MapScriptEvent c) => c.Clone())) : new List<MapEditor.MapScriptEvent>());
			}

			// Token: 0x06000F58 RID: 3928 RVA: 0x0006BD7C File Offset: 0x00069F7C
			public string GetMapName(MapEditor editor)
			{
				int num = checked((int)this.MapNameId - editor.MAP_NAME_FIRST_INDEX);
				bool flag = num >= 0 && num < editor.cmbMapNameId.Items.Count;
				string text;
				if (flag)
				{
					text = editor.cmbMapNameId.Items[num].ToString().Split(new char[] { ']' })[1];
				}
				else
				{
					text = string.Format("[{0:X2}]", this.MapNameId);
				}
				return text;
			}

			// Token: 0x0400086B RID: 2155
			public int Bank;

			// Token: 0x0400086C RID: 2156
			public int Number;

			// Token: 0x0400086D RID: 2157
			public uint FooterAddress;

			// Token: 0x0400086E RID: 2158
			public uint EventScriptAddress;

			// Token: 0x0400086F RID: 2159
			public uint MapScriptAddress;

			// Token: 0x04000870 RID: 2160
			public uint ConnectionAddress;

			// Token: 0x04000871 RID: 2161
			public ushort MusicCode;

			// Token: 0x04000872 RID: 2162
			public ushort TerrainId;

			// Token: 0x04000873 RID: 2163
			public byte MapNameId;

			// Token: 0x04000874 RID: 2164
			public byte Sight;

			// Token: 0x04000875 RID: 2165
			public byte Weather;

			// Token: 0x04000876 RID: 2166
			public byte TerrainType;

			// Token: 0x04000877 RID: 2167
			public byte Bicycle;

			// Token: 0x04000878 RID: 2168
			public byte MapNameType;

			// Token: 0x04000879 RID: 2169
			public byte BattleType;

			// Token: 0x0400087A RID: 2170
			public sbyte Level;

			// Token: 0x0400087B RID: 2171
			public List<MapEditor.ConnectedMap> Connections;

			// Token: 0x0400087C RID: 2172
			public List<MapEditor.PersonEvent> Persons;

			// Token: 0x0400087D RID: 2173
			public List<MapEditor.WarpEvent> Warps;

			// Token: 0x0400087E RID: 2174
			public List<MapEditor.TrapEvent> Traps;

			// Token: 0x0400087F RID: 2175
			public List<MapEditor.SignEvent> Signs;

			// Token: 0x04000880 RID: 2176
			public List<MapEditor.MapScriptEvent> MapScripts;
		}

		// Token: 0x02000044 RID: 68
		public class MapFooter
		{
			// Token: 0x06000F5A RID: 3930 RVA: 0x0006BE04 File Offset: 0x0006A004
			public MapEditor.MapFooter Clone()
			{
				return (MapEditor.MapFooter)base.MemberwiseClone();
			}

			// Token: 0x04000881 RID: 2177
			public byte MapWidth;

			// Token: 0x04000882 RID: 2178
			public byte MapHeight;

			// Token: 0x04000883 RID: 2179
			public byte BorderWidth;

			// Token: 0x04000884 RID: 2180
			public byte BorderHeight;

			// Token: 0x04000885 RID: 2181
			public uint BorderDataAddress;

			// Token: 0x04000886 RID: 2182
			public uint MapDataAddress;

			// Token: 0x04000887 RID: 2183
			public uint Tileset1Address;

			// Token: 0x04000888 RID: 2184
			public uint Tileset2Address;
		}

		// Token: 0x02000045 RID: 69
		public class TilesetHeader
		{
			// Token: 0x06000F5C RID: 3932 RVA: 0x0006BE2C File Offset: 0x0006A02C
			public MapEditor.TilesetHeader Clone()
			{
				return (MapEditor.TilesetHeader)base.MemberwiseClone();
			}

			// Token: 0x04000889 RID: 2185
			public byte ImageCompressType;

			// Token: 0x0400088A RID: 2186
			public byte PaletteType;

			// Token: 0x0400088B RID: 2187
			public uint ImageAddress;

			// Token: 0x0400088C RID: 2188
			public uint PaletteAddress;

			// Token: 0x0400088D RID: 2189
			public uint BlockImageAddress;

			// Token: 0x0400088E RID: 2190
			public uint AnimationAddress;

			// Token: 0x0400088F RID: 2191
			public uint BlockBehaviorAddress;
		}

		// Token: 0x02000046 RID: 70
		private class TilesetUIContainer
		{
			// Token: 0x06000F5D RID: 3933 RVA: 0x0006BE49 File Offset: 0x0006A049
			public TilesetUIContainer()
			{
				this.IsUpdating = false;
			}

			// Token: 0x04000890 RID: 2192
			public RadioButton RbIndex;

			// Token: 0x04000891 RID: 2193
			public RadioButton RbAddress;

			// Token: 0x04000892 RID: 2194
			public NumericUpDown NudIndex;

			// Token: 0x04000893 RID: 2195
			public TextBox TxtAddress;

			// Token: 0x04000894 RID: 2196
			public ComboBox CmbCompress;

			// Token: 0x04000895 RID: 2197
			public ComboBox CmbPaletteType;

			// Token: 0x04000896 RID: 2198
			public TextBox TxtImageAddress;

			// Token: 0x04000897 RID: 2199
			public TextBox TxtPaletteAddress;

			// Token: 0x04000898 RID: 2200
			public TextBox TxtBlockImageAddress;

			// Token: 0x04000899 RID: 2201
			public TextBox TxtAnimationAddress;

			// Token: 0x0400089A RID: 2202
			public TextBox TxtBehaviorAddress;

			// Token: 0x0400089B RID: 2203
			public bool IsUpdating;
		}

		// Token: 0x02000047 RID: 71
		public class ConnectedMap
		{
			// Token: 0x06000F5F RID: 3935 RVA: 0x0006BE64 File Offset: 0x0006A064
			public MapEditor.ConnectedMap Clone()
			{
				return (MapEditor.ConnectedMap)base.MemberwiseClone();
			}

			// Token: 0x0400089C RID: 2204
			public byte Direction;

			// Token: 0x0400089D RID: 2205
			public byte Bank;

			// Token: 0x0400089E RID: 2206
			public byte Number;

			// Token: 0x0400089F RID: 2207
			public int Shift;
		}

		// Token: 0x02000048 RID: 72
		public class PersonEvent
		{
			// Token: 0x06000F61 RID: 3937 RVA: 0x0006BE8C File Offset: 0x0006A08C
			public MapEditor.PersonEvent Clone()
			{
				return (MapEditor.PersonEvent)base.MemberwiseClone();
			}

			// Token: 0x040008A0 RID: 2208
			public byte No;

			// Token: 0x040008A1 RID: 2209
			public byte SpriteNo;

			// Token: 0x040008A2 RID: 2210
			public byte UnknownB2Upper;

			// Token: 0x040008A3 RID: 2211
			public byte UnknownB2Lower;

			// Token: 0x040008A4 RID: 2212
			public byte Layer;

			// Token: 0x040008A5 RID: 2213
			public byte Action;

			// Token: 0x040008A6 RID: 2214
			public byte MovementRangeX;

			// Token: 0x040008A7 RID: 2215
			public byte MovementRangeY;

			// Token: 0x040008A8 RID: 2216
			public byte UnknownB11;

			// Token: 0x040008A9 RID: 2217
			public byte Trainer;

			// Token: 0x040008AA RID: 2218
			public byte UnknownB13;

			// Token: 0x040008AB RID: 2219
			public ushort X;

			// Token: 0x040008AC RID: 2220
			public ushort Y;

			// Token: 0x040008AD RID: 2221
			public ushort Sight;

			// Token: 0x040008AE RID: 2222
			public ushort Flag;

			// Token: 0x040008AF RID: 2223
			public ushort UnknownB22;

			// Token: 0x040008B0 RID: 2224
			public uint ScriptAddress;
		}

		// Token: 0x02000049 RID: 73
		public class WarpEvent
		{
			// Token: 0x06000F63 RID: 3939 RVA: 0x0006BEB4 File Offset: 0x0006A0B4
			public MapEditor.WarpEvent Clone()
			{
				return (MapEditor.WarpEvent)base.MemberwiseClone();
			}

			// Token: 0x040008B1 RID: 2225
			public byte Layer;

			// Token: 0x040008B2 RID: 2226
			public byte WarpToNo;

			// Token: 0x040008B3 RID: 2227
			public byte MapBank;

			// Token: 0x040008B4 RID: 2228
			public byte MapNumber;

			// Token: 0x040008B5 RID: 2229
			public ushort X;

			// Token: 0x040008B6 RID: 2230
			public ushort Y;
		}

		// Token: 0x0200004A RID: 74
		public class TrapEvent
		{
			// Token: 0x06000F65 RID: 3941 RVA: 0x0006BEDC File Offset: 0x0006A0DC
			public MapEditor.TrapEvent Clone()
			{
				return (MapEditor.TrapEvent)base.MemberwiseClone();
			}

			// Token: 0x040008B7 RID: 2231
			public byte Layer;

			// Token: 0x040008B8 RID: 2232
			public byte UnknownB5;

			// Token: 0x040008B9 RID: 2233
			public ushort X;

			// Token: 0x040008BA RID: 2234
			public ushort Y;

			// Token: 0x040008BB RID: 2235
			public ushort VarNumber;

			// Token: 0x040008BC RID: 2236
			public ushort VarValue;

			// Token: 0x040008BD RID: 2237
			public ushort UnknownB10;

			// Token: 0x040008BE RID: 2238
			public uint ScriptAddress;
		}

		// Token: 0x0200004B RID: 75
		public class SignEvent
		{
			// Token: 0x06000F67 RID: 3943 RVA: 0x0006BF04 File Offset: 0x0006A104
			public MapEditor.SignEvent Clone()
			{
				return (MapEditor.SignEvent)base.MemberwiseClone();
			}

			// Token: 0x040008BF RID: 2239
			public byte Layer;

			// Token: 0x040008C0 RID: 2240
			public byte SignType;

			// Token: 0x040008C1 RID: 2241
			public ushort X;

			// Token: 0x040008C2 RID: 2242
			public ushort Y;

			// Token: 0x040008C3 RID: 2243
			public ushort UnknownB6;

			// Token: 0x040008C4 RID: 2244
			public uint ScriptAddress;
		}

		// Token: 0x0200004C RID: 76
		public class MapScriptEvent
		{
			// Token: 0x06000F69 RID: 3945 RVA: 0x0006BF2C File Offset: 0x0006A12C
			public MapEditor.MapScriptEvent Clone()
			{
				MapEditor.MapScriptEvent mapScriptEvent = (MapEditor.MapScriptEvent)base.MemberwiseClone();
				bool flag = this.ListEntries != null;
				if (flag)
				{
					mapScriptEvent.ListEntries = new List<MapEditor.MapScriptListEntry>(this.ListEntries.Select((MapEditor.MapScriptListEntry x) => x.Clone()));
				}
				return mapScriptEvent;
			}

			// Token: 0x040008C5 RID: 2245
			public byte Type;

			// Token: 0x040008C6 RID: 2246
			public uint Pointer;

			// Token: 0x040008C7 RID: 2247
			public List<MapEditor.MapScriptListEntry> ListEntries;
		}

		// Token: 0x0200004D RID: 77
		public class MapScriptListEntry
		{
			// Token: 0x06000F6B RID: 3947 RVA: 0x0006BF9C File Offset: 0x0006A19C
			public MapEditor.MapScriptListEntry Clone()
			{
				return (MapEditor.MapScriptListEntry)base.MemberwiseClone();
			}

			// Token: 0x040008C8 RID: 2248
			public ushort VarNumber;

			// Token: 0x040008C9 RID: 2249
			public ushort VarValue;

			// Token: 0x040008CA RID: 2250
			public uint ScriptAddress;
		}
	}
}
