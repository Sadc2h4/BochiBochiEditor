using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x0200000A RID: 10
	public partial class BlockEditor : Form
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00003679 File Offset: 0x00001879
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00003683 File Offset: 0x00001883
		internal virtual GroupBox grpBlock
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600001F RID: 31 RVA: 0x0000368C File Offset: 0x0000188C
		// (set) Token: 0x06000020 RID: 32 RVA: 0x00003698 File Offset: 0x00001898
		internal virtual VScrollBar vsbBlock
		{
			[CompilerGenerated]
			get
			{
				return this._vsbBlock;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ScrollEventHandler scrollEventHandler = new ScrollEventHandler(this.vsbBlock_Scroll);
				VScrollBar vscrollBar = this._vsbBlock;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll -= scrollEventHandler;
				}
				this._vsbBlock = value;
				vscrollBar = this._vsbBlock;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll += scrollEventHandler;
				}
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000021 RID: 33 RVA: 0x000036DB File Offset: 0x000018DB
		// (set) Token: 0x06000022 RID: 34 RVA: 0x000036E8 File Offset: 0x000018E8
		internal virtual Panel pnlBlock
		{
			[CompilerGenerated]
			get
			{
				return this._pnlBlock;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlBlock_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlBlock_MouseDown);
				MouseEventHandler mouseEventHandler2 = new MouseEventHandler(this.pnlBlock_MouseWheel);
				MouseEventHandler mouseEventHandler3 = new MouseEventHandler(this.pnlBlock_MouseMove);
				Panel panel = this._pnlBlock;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
					panel.MouseWheel -= mouseEventHandler2;
					panel.MouseMove -= mouseEventHandler3;
				}
				this._pnlBlock = value;
				panel = this._pnlBlock;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
					panel.MouseWheel += mouseEventHandler2;
					panel.MouseMove += mouseEventHandler3;
				}
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00003788 File Offset: 0x00001988
		// (set) Token: 0x06000024 RID: 36 RVA: 0x00003792 File Offset: 0x00001992
		internal virtual GroupBox grpPalette
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000025 RID: 37 RVA: 0x0000379B File Offset: 0x0000199B
		// (set) Token: 0x06000026 RID: 38 RVA: 0x000037A8 File Offset: 0x000019A8
		internal virtual VScrollBar vsrPalette
		{
			[CompilerGenerated]
			get
			{
				return this._vsrPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ScrollEventHandler scrollEventHandler = new ScrollEventHandler(this.vsrPalette_Scroll);
				VScrollBar vscrollBar = this._vsrPalette;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll -= scrollEventHandler;
				}
				this._vsrPalette = value;
				vscrollBar = this._vsrPalette;
				if (vscrollBar != null)
				{
					vscrollBar.Scroll += scrollEventHandler;
				}
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000027 RID: 39 RVA: 0x000037EB File Offset: 0x000019EB
		// (set) Token: 0x06000028 RID: 40 RVA: 0x000037F8 File Offset: 0x000019F8
		internal virtual Panel pnlPalette
		{
			[CompilerGenerated]
			get
			{
				return this._pnlPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlPalette_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlPalette_MouseDown);
				MouseEventHandler mouseEventHandler2 = new MouseEventHandler(this.pnlPalette_MouseMove);
				MouseEventHandler mouseEventHandler3 = new MouseEventHandler(this.pnlPalette_MouseUp);
				MouseEventHandler mouseEventHandler4 = new MouseEventHandler(this.pnlPalette_MouseWheel);
				Panel panel = this._pnlPalette;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
					panel.MouseMove -= mouseEventHandler2;
					panel.MouseUp -= mouseEventHandler3;
					panel.MouseWheel -= mouseEventHandler4;
				}
				this._pnlPalette = value;
				panel = this._pnlPalette;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
					panel.MouseMove += mouseEventHandler2;
					panel.MouseUp += mouseEventHandler3;
					panel.MouseWheel += mouseEventHandler4;
				}
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000029 RID: 41 RVA: 0x000038B8 File Offset: 0x00001AB8
		// (set) Token: 0x0600002A RID: 42 RVA: 0x000038C2 File Offset: 0x00001AC2
		internal virtual CheckBox chkYFlip
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600002B RID: 43 RVA: 0x000038CB File Offset: 0x00001ACB
		// (set) Token: 0x0600002C RID: 44 RVA: 0x000038D5 File Offset: 0x00001AD5
		internal virtual CheckBox chkXFlip
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600002D RID: 45 RVA: 0x000038DE File Offset: 0x00001ADE
		// (set) Token: 0x0600002E RID: 46 RVA: 0x000038E8 File Offset: 0x00001AE8
		internal virtual Panel pnlPreview
		{
			[CompilerGenerated]
			get
			{
				return this._pnlPreview;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlPreview_Paint);
				Panel panel = this._pnlPreview;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
				}
				this._pnlPreview = value;
				panel = this._pnlPreview;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
				}
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600002F RID: 47 RVA: 0x0000392B File Offset: 0x00001B2B
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00003938 File Offset: 0x00001B38
		internal virtual ComboBox cmbPalette
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPalette_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPalette;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPalette = value;
				comboBox = this._cmbPalette;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000031 RID: 49 RVA: 0x0000397B File Offset: 0x00001B7B
		// (set) Token: 0x06000032 RID: 50 RVA: 0x00003985 File Offset: 0x00001B85
		internal virtual GroupBox grpData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000033 RID: 51 RVA: 0x0000398E File Offset: 0x00001B8E
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00003998 File Offset: 0x00001B98
		internal virtual Label lblData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000035 RID: 53 RVA: 0x000039A1 File Offset: 0x00001BA1
		// (set) Token: 0x06000036 RID: 54 RVA: 0x000039AC File Offset: 0x00001BAC
		internal virtual Panel pnlData
		{
			[CompilerGenerated]
			get
			{
				return this._pnlData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				PaintEventHandler paintEventHandler = new PaintEventHandler(this.pnlData_Paint);
				MouseEventHandler mouseEventHandler = new MouseEventHandler(this.pnlData_MouseDown);
				MouseEventHandler mouseEventHandler2 = new MouseEventHandler(this.pnlData_MouseMove);
				MouseEventHandler mouseEventHandler3 = new MouseEventHandler(this.pnlData_MouseUp);
				Panel panel = this._pnlData;
				if (panel != null)
				{
					panel.Paint -= paintEventHandler;
					panel.MouseDown -= mouseEventHandler;
					panel.MouseMove -= mouseEventHandler2;
					panel.MouseUp -= mouseEventHandler3;
				}
				this._pnlData = value;
				panel = this._pnlData;
				if (panel != null)
				{
					panel.Paint += paintEventHandler;
					panel.MouseDown += mouseEventHandler;
					panel.MouseMove += mouseEventHandler2;
					panel.MouseUp += mouseEventHandler3;
				}
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00003A4C File Offset: 0x00001C4C
		// (set) Token: 0x06000038 RID: 56 RVA: 0x00003A56 File Offset: 0x00001C56
		internal virtual ComboBox cmbTileAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00003A5F File Offset: 0x00001C5F
		// (set) Token: 0x0600003A RID: 58 RVA: 0x00003A69 File Offset: 0x00001C69
		internal virtual Label lbltileAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00003A72 File Offset: 0x00001C72
		// (set) Token: 0x0600003C RID: 60 RVA: 0x00003A7C File Offset: 0x00001C7C
		internal virtual TextBox txtTileAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00003A85 File Offset: 0x00001C85
		// (set) Token: 0x0600003E RID: 62 RVA: 0x00003A8F File Offset: 0x00001C8F
		internal virtual NumericUpDown nudTileAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00003A98 File Offset: 0x00001C98
		// (set) Token: 0x06000040 RID: 64 RVA: 0x00003AA2 File Offset: 0x00001CA2
		internal virtual ComboBox cmbAttribute
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00003AAB File Offset: 0x00001CAB
		// (set) Token: 0x06000042 RID: 66 RVA: 0x00003AB5 File Offset: 0x00001CB5
		internal virtual Label lblAttribute
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000043 RID: 67 RVA: 0x00003ABE File Offset: 0x00001CBE
		// (set) Token: 0x06000044 RID: 68 RVA: 0x00003AC8 File Offset: 0x00001CC8
		internal virtual TextBox txtAttribute
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000045 RID: 69 RVA: 0x00003AD1 File Offset: 0x00001CD1
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00003ADB File Offset: 0x00001CDB
		internal virtual NumericUpDown nudAttribute
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000047 RID: 71 RVA: 0x00003AE4 File Offset: 0x00001CE4
		// (set) Token: 0x06000048 RID: 72 RVA: 0x00003AF0 File Offset: 0x00001CF0
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

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00003B33 File Offset: 0x00001D33
		// (set) Token: 0x0600004A RID: 74 RVA: 0x00003B3D File Offset: 0x00001D3D
		internal virtual ComboBox cmbLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00003B46 File Offset: 0x00001D46
		// (set) Token: 0x0600004C RID: 76 RVA: 0x00003B50 File Offset: 0x00001D50
		internal virtual ComboBox cmbUnknown
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00003B59 File Offset: 0x00001D59
		// (set) Token: 0x0600004E RID: 78 RVA: 0x00003B63 File Offset: 0x00001D63
		internal virtual Label lblLayer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600004F RID: 79 RVA: 0x00003B6C File Offset: 0x00001D6C
		// (set) Token: 0x06000050 RID: 80 RVA: 0x00003B76 File Offset: 0x00001D76
		internal virtual Label lblUnknown
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000051 RID: 81 RVA: 0x00003B7F File Offset: 0x00001D7F
		// (set) Token: 0x06000052 RID: 82 RVA: 0x00003B89 File Offset: 0x00001D89
		internal virtual TextBox txtUnknown
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000053 RID: 83 RVA: 0x00003B92 File Offset: 0x00001D92
		// (set) Token: 0x06000054 RID: 84 RVA: 0x00003B9C File Offset: 0x00001D9C
		internal virtual NumericUpDown nudUnknown
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00003BA5 File Offset: 0x00001DA5
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00003BAF File Offset: 0x00001DAF
		internal virtual CheckBox chkWildWater
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00003BB8 File Offset: 0x00001DB8
		// (set) Token: 0x06000058 RID: 88 RVA: 0x00003BC2 File Offset: 0x00001DC2
		internal virtual CheckBox chkWildGrass
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00003BCB File Offset: 0x00001DCB
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00003BD5 File Offset: 0x00001DD5
		internal virtual Label lblWild
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00003BDE File Offset: 0x00001DDE
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00003BE8 File Offset: 0x00001DE8
		internal virtual Label lblBLockInfo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003BF1 File Offset: 0x00001DF1
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00003BFB File Offset: 0x00001DFB
		internal virtual Label lblBlockIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00003C04 File Offset: 0x00001E04
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00003C1C File Offset: 0x00001E1C
		private int SelectedBlockIndex
		{
			get
			{
				return this.currentBlockIndex;
			}
			set
			{
				checked
				{
					bool flag = value > 0 && this.IsTripleLayer(value - 1);
					if (flag)
					{
						value--;
					}
					bool flag2 = this.currentBlockIndex == value;
					if (!flag2)
					{
						bool flag3 = !this.ConfirmSaveBeforeSwitch();
						if (!flag3)
						{
							this.currentBlockIndex = value;
							this.BackupCurrentBlock();
							this.LoadBlockBehaviorToUI();
							this.UpdatePnlDataSize();
							this.pnlBlock.Invalidate();
							this.pnlData.Invalidate();
						}
					}
				}
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003C98 File Offset: 0x00001E98
		public BlockEditor(byte[] rom, Bitmap bmp, bool grid, uint ts1Addr, uint ts2Addr, int ts1Count, int total, byte[] imgBytes, Color[] pals, uint ts1BlockImg, uint ts2BlockImg)
		{
			base.Load += this.BlockEditor_Load;
			base.FormClosing += this.BlockEditor_FormClosing;
			this.isSelectingPalette = false;
			this.isSelectingData = false;
			this.hasPalSelection = false;
			this.cachedPaletteIndex = -1;
			this.hasUnsavedChanges = false;
			this.isUpdatingUI = false;
			this.currentBlockIndex = 0;
			this.InitializeComponent();
			this.romData = rom;
			this.blockPaletteBitmap = bmp;
			this.showGrid = grid;
			this.ts1BehaviorAddr = ts1Addr;
			this.ts2BehaviorAddr = ts2Addr;
			this.ts1BlockCount = ts1Count;
			this.totalBlocks = total;
			this.imageBytes = imgBytes;
			this.palettes = pals;
			this.ts1BlockImageAddr = ts1BlockImg;
			this.ts2BlockImageAddr = ts2BlockImg;
			this.EnableDoubleBuffering(this.pnlBlock);
			this.EnableDoubleBuffering(this.pnlPalette);
			this.EnableDoubleBuffering(this.pnlData);
			this.EnableDoubleBuffering(this.pnlPreview);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003D94 File Offset: 0x00001F94
		public BlockEditor()
		{
			base.Load += this.BlockEditor_Load;
			base.FormClosing += this.BlockEditor_FormClosing;
			this.isSelectingPalette = false;
			this.isSelectingData = false;
			this.hasPalSelection = false;
			this.cachedPaletteIndex = -1;
			this.hasUnsavedChanges = false;
			this.isUpdatingUI = false;
			this.currentBlockIndex = 0;
			this.InitializeComponent();
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003E08 File Offset: 0x00002008
		private void EnableDoubleBuffering(Control ctrl)
		{
			typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, ctrl, new object[] { true });
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003E44 File Offset: 0x00002044
		private void BlockEditor_Load(object sender, EventArgs e)
		{
			this.InitializeControllers();
			bool flag = this.blockPaletteBitmap != null;
			checked
			{
				if (flag)
				{
					int num = this.blockPaletteBitmap.Height * 2;
					int height = this.pnlBlock.ClientSize.Height;
					this.vsbBlock.SmallChange = 32;
					this.vsbBlock.LargeChange = height;
					this.vsbBlock.Maximum = ((num > height) ? (num - 1) : 0);
					this.vsbBlock.Enabled = num > height;
				}
				this.isUpdatingUI = true;
				this.cmbPalette.Items.Clear();
				int num2 = 0;
				do
				{
					this.cmbPalette.Items.Add(string.Format("パレット {0}", num2));
					num2++;
				}
				while (num2 <= 12);
				this.cmbPalette.SelectedIndex = 0;
				this.isUpdatingUI = false;
				bool flag2 = this.imageBytes != null;
				if (flag2)
				{
					int num3 = this.imageBytes.Length / 32;
					int num4 = 16;
					int num5 = (int)Math.Ceiling((double)num3 / (double)num4) * 8 * 2;
					this.vsrPalette.SmallChange = 16;
					this.vsrPalette.LargeChange = this.pnlPalette.ClientSize.Height;
					this.vsrPalette.Maximum = Math.Max(0, num5 - 1);
					this.vsrPalette.Enabled = num5 > this.pnlPalette.ClientSize.Height;
				}
				this.nudTileAction.ValueChanged += this.OnDataChanged;
				this.nudAttribute.ValueChanged += this.OnDataChanged;
				this.nudUnknown.ValueChanged += this.OnDataChanged;
				this.cmbLayer.SelectedIndexChanged += this.OnDataChanged;
				this.chkWildGrass.CheckedChanged += this.OnDataChanged;
				this.chkWildWater.CheckedChanged += this.OnDataChanged;
				this.chkXFlip.CheckedChanged += this.OnFlipChanged;
				this.chkYFlip.CheckedChanged += this.OnFlipChanged;
				this.SelectedBlockIndex = 0;
				this.UpdatePnlDataSize();
				this.BackupCurrentBlock();
				this.SetUnsavedState(false);
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x000040A4 File Offset: 0x000022A4
		private void InitializeControllers()
		{
			this.tileActionController = new BlockEditor.LinkedDataController(this.nudTileAction, this.txtTileAction, this.cmbTileAction);
			this.attributeController = new BlockEditor.LinkedDataController(this.nudAttribute, this.txtAttribute, this.cmbAttribute);
			this.unknownController = new BlockEditor.LinkedDataController(this.nudUnknown, this.txtUnknown, this.cmbUnknown);
			this.layerController = new BlockEditor.ComboBoxOnlyController(this.cmbLayer);
			Encoding utf = Encoding.UTF8;
			this.tileActionController.LoadData("txt\\BlockTileAction.txt", utf);
			this.attributeController.LoadData("txt\\BlockAttribute.txt", utf);
			this.unknownController.LoadData("txt\\BlockUnknown.txt", utf);
			this.layerController.LoadData("txt\\BlockLayer.txt", utf);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00004168 File Offset: 0x00002368
		private uint GetAddress(int blockId, uint ts1Addr, uint ts2Addr, int entrySize)
		{
			bool flag = blockId < this.ts1BlockCount;
			uint num;
			if (flag)
			{
				num = (((ulong)ts1Addr == 0UL) ? 0U : checked(ts1Addr + (uint)(blockId * entrySize)));
			}
			else
			{
				num = (((ulong)ts2Addr == 0UL) ? 0U : checked(ts2Addr + (uint)((blockId - this.ts1BlockCount) * entrySize)));
			}
			return num;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x000041B0 File Offset: 0x000023B0
		private uint GetBlockImageAddress(int blockId)
		{
			return this.GetAddress(blockId, this.ts1BlockImageAddr, this.ts2BlockImageAddr, 16);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000041D8 File Offset: 0x000023D8
		private uint GetBehaviorAddress(int blockId)
		{
			return this.GetAddress(blockId, this.ts1BehaviorAddr, this.ts2BehaviorAddr, 4);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00004200 File Offset: 0x00002400
		private void BackupCurrentBlock()
		{
			bool flag = this.SelectedBlockIndex < 0;
			if (!flag)
			{
				uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex);
				bool flag2 = (ulong)blockImageAddress > 0UL;
				if (flag2)
				{
					this.backupBlockData = new byte[16];
					Array.Copy(this.romData, checked((int)blockImageAddress), this.backupBlockData, 0, 16);
				}
				uint behaviorAddress = this.GetBehaviorAddress(this.SelectedBlockIndex);
				bool flag3 = (ulong)behaviorAddress > 0UL;
				checked
				{
					if (flag3)
					{
						this.backupBehaviorData = new byte[4];
						Array.Copy(this.romData, (int)behaviorAddress, this.backupBehaviorData, 0, 4);
					}
					bool flag4 = this.SelectedBlockIndex + 1 < this.totalBlocks;
					if (flag4)
					{
						uint blockImageAddress2 = this.GetBlockImageAddress(this.SelectedBlockIndex + 1);
						bool flag5 = unchecked((ulong)blockImageAddress2) > 0UL;
						if (flag5)
						{
							this.backupNextBlockData = new byte[16];
							Array.Copy(this.romData, (int)blockImageAddress2, this.backupNextBlockData, 0, 16);
						}
						else
						{
							this.backupNextBlockData = null;
						}
						uint behaviorAddress2 = this.GetBehaviorAddress(this.SelectedBlockIndex + 1);
						bool flag6 = unchecked((ulong)behaviorAddress2) > 0UL;
						if (flag6)
						{
							this.backupNextBehaviorData = new byte[4];
							Array.Copy(this.romData, (int)behaviorAddress2, this.backupNextBehaviorData, 0, 4);
						}
						else
						{
							this.backupNextBehaviorData = null;
						}
					}
					else
					{
						this.backupNextBlockData = null;
						this.backupNextBehaviorData = null;
					}
				}
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00004360 File Offset: 0x00002560
		private void RestoreCurrentBlock()
		{
			bool flag = this.SelectedBlockIndex < 0;
			checked
			{
				if (!flag)
				{
					bool flag2 = this.backupBlockData != null;
					if (flag2)
					{
						uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex);
						bool flag3 = unchecked((ulong)blockImageAddress) > 0UL;
						if (flag3)
						{
							Array.Copy(this.backupBlockData, 0, this.romData, (int)blockImageAddress, 16);
						}
					}
					bool flag4 = this.backupBehaviorData != null;
					if (flag4)
					{
						uint behaviorAddress = this.GetBehaviorAddress(this.SelectedBlockIndex);
						bool flag5 = unchecked((ulong)behaviorAddress) > 0UL;
						if (flag5)
						{
							Array.Copy(this.backupBehaviorData, 0, this.romData, (int)behaviorAddress, 4);
						}
					}
					bool flag6 = this.SelectedBlockIndex + 1 < this.totalBlocks;
					if (flag6)
					{
						bool flag7 = this.backupNextBlockData != null;
						if (flag7)
						{
							uint blockImageAddress2 = this.GetBlockImageAddress(this.SelectedBlockIndex + 1);
							bool flag8 = unchecked((ulong)blockImageAddress2) > 0UL;
							if (flag8)
							{
								Array.Copy(this.backupNextBlockData, 0, this.romData, (int)blockImageAddress2, 16);
							}
						}
						bool flag9 = this.backupNextBehaviorData != null;
						if (flag9)
						{
							uint behaviorAddress2 = this.GetBehaviorAddress(this.SelectedBlockIndex + 1);
							bool flag10 = unchecked((ulong)behaviorAddress2) > 0UL;
							if (flag10)
							{
								Array.Copy(this.backupNextBehaviorData, 0, this.romData, (int)behaviorAddress2, 4);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x000044A8 File Offset: 0x000026A8
		private void SetPixelToBuffer(byte[] pixels, int stride, int x, int y, Color c)
		{
			checked
			{
				int num = y * stride + x * 4;
				bool flag = num >= 0 && num + 3 < pixels.Length;
				if (flag)
				{
					pixels[num] = c.B;
					pixels[num + 1] = c.G;
					pixels[num + 2] = c.R;
					pixels[num + 3] = byte.MaxValue;
				}
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00004500 File Offset: 0x00002700
		private void RenderTile(byte[] pixels, int stride, int tileIndex, int palIndex, bool flipX, bool flipY, int drawOffsetX, int drawOffsetY)
		{
			checked
			{
				bool flag = tileIndex * 32 + 31 >= this.imageBytes.Length;
				if (!flag)
				{
					int num = palIndex * 16;
					int num2 = 0;
					do
					{
						int num3 = 0;
						do
						{
							int num4 = tileIndex * 32 + num2 * 4 + num3 / 2;
							byte b = this.imageBytes[num4];
							int num5 = (int)(b & 15);
							bool flag2 = num5 > 0;
							if (flag2)
							{
								int num6 = (flipX ? (7 - num3) : num3) + drawOffsetX;
								int num7 = (flipY ? (7 - num2) : num2) + drawOffsetY;
								this.SetPixelToBuffer(pixels, stride, num6, num7, this.palettes[num + num5]);
							}
							int num8 = (int)(unchecked((byte)((uint)b >> 4)) & 15);
							bool flag3 = num8 > 0;
							if (flag3)
							{
								int num9 = (flipX ? (7 - (num3 + 1)) : (num3 + 1)) + drawOffsetX;
								int num10 = (flipY ? (7 - num2) : num2) + drawOffsetY;
								this.SetPixelToBuffer(pixels, stride, num9, num10, this.palettes[num + num8]);
							}
							num3 += 2;
						}
						while (num3 <= 7);
						num2++;
					}
					while (num2 <= 7);
				}
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00004608 File Offset: 0x00002808
		private void RenderBlockLayer(byte[] pixels, int stride, int romOffset, int drawOffsetX, int drawOffsetY)
		{
			int num = 0;
			checked
			{
				do
				{
					ushort num2 = BitConverter.ToUInt16(this.romData, romOffset + num * 2);
					int num3 = (int)(num2 & 1023);
					bool flag = (num2 & 1024) > 0;
					bool flag2 = (num2 & 2048) > 0;
					int num4 = (int)(unchecked((ushort)((uint)num2 >> 12)) & 15);
					int num5 = num % 2 * 8 + drawOffsetX;
					int num6 = num / 2 * 8 + drawOffsetY;
					this.RenderTile(pixels, stride, num3, num4, flag, flag2, num5, num6);
					num++;
				}
				while (num <= 3);
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00004684 File Offset: 0x00002884
		private Bitmap CreateBlockBitmap(int blockId)
		{
			Bitmap bitmap = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int stride = bitmapData.Stride;
			byte[] array = new byte[checked(stride * bitmap.Height - 1 + 1)];
			uint blockImageAddress = this.GetBlockImageAddress(blockId);
			bool flag = (ulong)blockImageAddress > 0UL;
			checked
			{
				if (flag)
				{
					this.RenderBlockLayer(array, stride, (int)blockImageAddress, 0, 0);
					this.RenderBlockLayer(array, stride, (int)blockImageAddress + 8, 0, 0);
					bool flag2 = this.IsTripleLayer(blockId) && blockId + 1 < this.totalBlocks;
					if (flag2)
					{
						uint blockImageAddress2 = this.GetBlockImageAddress(blockId + 1);
						bool flag3 = unchecked((ulong)blockImageAddress2) > 0UL;
						if (flag3)
						{
							this.RenderBlockLayer(array, stride, (int)blockImageAddress2, 0, 0);
						}
					}
				}
				Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
				bitmap.UnlockBits(bitmapData);
				return bitmap;
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000477C File Offset: 0x0000297C
		private Rectangle GetSelectionRect(int x1, int y1, int x2, int y2)
		{
			Rectangle rectangle = checked(new Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x1 - x2) + 1, Math.Abs(y1 - y2) + 1));
			return rectangle;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000047B8 File Offset: 0x000029B8
		private void pnlBlock_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.blockPaletteBitmap == null;
			checked
			{
				if (!flag)
				{
					Graphics graphics = e.Graphics;
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					graphics.PixelOffsetMode = PixelOffsetMode.Half;
					graphics.TranslateTransform(0f, (float)(0 - this.vsbBlock.Value));
					graphics.ScaleTransform(2f, 2f);
					graphics.DrawImage(this.blockPaletteBitmap, 0, 0);
					int num = this.blockPaletteBitmap.Width / 16;
					int num2 = this.SelectedBlockIndex % num;
					int num3 = this.SelectedBlockIndex / num;
					uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex);
					bool flag2 = unchecked((ulong)blockImageAddress) > 0UL;
					if (flag2)
					{
						using (SolidBrush solidBrush = new SolidBrush(this.pnlBlock.BackColor))
						{
							graphics.FillRectangle(solidBrush, num2 * 16, num3 * 16, 16, 16);
						}
						using (Bitmap bitmap = this.CreateBlockBitmap(this.SelectedBlockIndex))
						{
							graphics.DrawImage(bitmap, num2 * 16, num3 * 16);
						}
					}
					bool flag3 = this.showGrid;
					if (flag3)
					{
						using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
						{
							int width = this.blockPaletteBitmap.Width;
							for (int i = 0; i <= width; i += 16)
							{
								graphics.DrawLine(pen, i, 0, i, this.blockPaletteBitmap.Height);
							}
							int height = this.blockPaletteBitmap.Height;
							for (int j = 0; j <= height; j += 16)
							{
								graphics.DrawLine(pen, 0, j, this.blockPaletteBitmap.Width, j);
							}
						}
					}
					int num4 = (this.IsTripleLayer(this.SelectedBlockIndex) ? 32 : 16);
					using (Pen pen2 = new Pen(Color.Red, 1f))
					{
						graphics.DrawRectangle(pen2, num2 * 16, num3 * 16, num4 - 1, 15);
					}
				}
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004A08 File Offset: 0x00002C08
		private void pnlBlock_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = this.blockPaletteBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = this.blockPaletteBitmap.Width / 16;
					int num2 = this.blockPaletteBitmap.Height / 16;
					int num3 = e.X / 32;
					int num4 = (e.Y + this.vsbBlock.Value) / 32;
					bool flag2 = num3 >= 0 && num3 < num && num4 >= 0 && num4 < num2;
					if (flag2)
					{
						int num5 = num4 * num + num3;
						bool flag3 = num5 < this.totalBlocks;
						if (flag3)
						{
							this.SelectedBlockIndex = num5;
						}
					}
				}
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00004AA0 File Offset: 0x00002CA0
		private void pnlData_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.SelectedBlockIndex < 0;
			if (!flag)
			{
				uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex);
				bool flag2 = (ulong)blockImageAddress == 0UL;
				checked
				{
					if (!flag2)
					{
						Graphics graphics = e.Graphics;
						graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
						graphics.PixelOffsetMode = PixelOffsetMode.Half;
						graphics.ScaleTransform(2f, 2f);
						bool flag3 = this.IsCurrentBlockTripleLayer();
						int num = (flag3 ? 3 : 2);
						using (Bitmap bitmap = new Bitmap(16 * num, 16, PixelFormat.Format32bppArgb))
						{
							BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int stride = bitmapData.Stride;
							byte[] array = new byte[stride * bitmap.Height - 1 + 1];
							this.RenderBlockLayer(array, stride, (int)blockImageAddress, 0, 0);
							this.RenderBlockLayer(array, stride, (int)blockImageAddress + 8, 16, 0);
							bool flag4 = flag3;
							if (flag4)
							{
								uint blockImageAddress2 = this.GetBlockImageAddress(this.SelectedBlockIndex + 1);
								bool flag5 = unchecked((ulong)blockImageAddress2) > 0UL;
								if (flag5)
								{
									this.RenderBlockLayer(array, stride, (int)blockImageAddress2, 32, 0);
								}
							}
							Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
							bitmap.UnlockBits(bitmapData);
							graphics.DrawImage(bitmap, 0, 0);
						}
						using (Pen pen = new Pen(Color.FromArgb(100, 128, 128, 128)))
						{
							graphics.DrawLine(pen, 8, 0, 8, 16);
							graphics.DrawLine(pen, 16, 0, 16, 16);
							graphics.DrawLine(pen, 24, 0, 24, 16);
							graphics.DrawLine(pen, 32, 0, 32, 16);
							bool flag6 = flag3;
							if (flag6)
							{
								graphics.DrawLine(pen, 40, 0, 40, 16);
								graphics.DrawLine(pen, 48, 0, 48, 16);
							}
							graphics.DrawLine(pen, 0, 8, 16 * num, 8);
						}
						bool flag7 = this.isSelectingData;
						if (flag7)
						{
							Rectangle selectionRect = this.GetSelectionRect(this.selStartX, this.selStartY, this.selEndX, this.selEndY);
							using (Pen pen2 = new Pen(Color.Red, 1f))
							{
								graphics.DrawRectangle(pen2, selectionRect.X * 8, selectionRect.Y * 8, selectionRect.Width * 8, selectionRect.Height * 8);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00004D4C File Offset: 0x00002F4C
		private void pnlData_MouseDown(object sender, MouseEventArgs e)
		{
			int num = e.X / 2 / 8;
			int num2 = e.Y / 2 / 8;
			int num3 = (this.IsCurrentBlockTripleLayer() ? 5 : 3);
			bool flag = e.Button == MouseButtons.Left;
			if (flag)
			{
				bool flag2 = this.selectedTiles == null;
				if (!flag2)
				{
					uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex);
					bool flag3 = (ulong)blockImageAddress == 0UL;
					checked
					{
						if (!flag3)
						{
							int num4 = (int)blockImageAddress;
							bool flag4 = false;
							int num5 = this.selectedTiles.GetLength(1) - 1;
							for (int i = 0; i <= num5; i++)
							{
								int num6 = this.selectedTiles.GetLength(0) - 1;
								for (int j = 0; j <= num6; j++)
								{
									int num7 = num + j;
									int num8 = num2 + i;
									bool flag5 = num7 > num3 || num8 > 1;
									if (!flag5)
									{
										bool flag6 = num7 >= 4;
										bool flag7 = num7 >= 2 && num7 <= 3;
										int num9 = num7 % 2;
										bool flag8 = flag6;
										int num10;
										if (flag8)
										{
											uint blockImageAddress2 = this.GetBlockImageAddress(this.SelectedBlockIndex + 1);
											bool flag9 = unchecked((ulong)blockImageAddress2) == 0UL;
											if (flag9)
											{
												goto IL_01A9;
											}
											num10 = (int)blockImageAddress2 + (num8 * 2 + num9) * 2;
										}
										else
										{
											num10 = num4 + (flag7 ? 8 : 0) + (num8 * 2 + num9) * 2;
										}
										byte[] bytes = BitConverter.GetBytes(this.selectedTiles[j, i]);
										bool flag10 = this.romData[num10] != bytes[0] || this.romData[num10 + 1] != bytes[1];
										if (flag10)
										{
											this.romData[num10] = bytes[0];
											this.romData[num10 + 1] = bytes[1];
											flag4 = true;
										}
									}
									IL_01A9:;
								}
							}
							bool flag11 = flag4;
							if (flag11)
							{
								this.SetUnsavedState(true);
								this.pnlData.Invalidate();
								this.pnlBlock.Invalidate();
							}
						}
					}
				}
			}
			else
			{
				bool flag12 = e.Button == MouseButtons.Right;
				if (flag12)
				{
					this.isSelectingData = true;
					this.selStartX = Math.Max(0, Math.Min(num3, num));
					this.selStartY = Math.Max(0, Math.Min(1, num2));
					this.selEndX = this.selStartX;
					this.selEndY = this.selStartY;
					this.pnlData.Invalidate();
				}
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00004FB4 File Offset: 0x000031B4
		private void pnlData_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = this.isSelectingData;
			if (flag)
			{
				int num = (this.IsCurrentBlockTripleLayer() ? 5 : 3);
				this.selEndX = Math.Max(0, Math.Min(num, e.X / 2 / 8));
				this.selEndY = Math.Max(0, Math.Min(1, e.Y / 2 / 8));
				this.pnlData.Invalidate();
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00005024 File Offset: 0x00003224
		private void pnlData_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.isSelectingData && e.Button == MouseButtons.Right;
			checked
			{
				if (flag)
				{
					int num = (this.IsCurrentBlockTripleLayer() ? 5 : 3);
					Rectangle selectionRect = this.GetSelectionRect(this.selStartX, this.selStartY, this.selEndX, this.selEndY);
					this.selectedTiles = new ushort[selectionRect.Width - 1 + 1, selectionRect.Height - 1 + 1];
					uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex);
					bool flag2 = unchecked((ulong)blockImageAddress) > 0UL;
					if (flag2)
					{
						int num2 = (int)blockImageAddress;
						int num3 = selectionRect.Height - 1;
						for (int i = 0; i <= num3; i++)
						{
							int num4 = selectionRect.Width - 1;
							for (int j = 0; j <= num4; j++)
							{
								int num5 = selectionRect.X + j;
								int num6 = selectionRect.Y + i;
								bool flag3 = num5 > num || num6 > 1;
								if (!flag3)
								{
									bool flag4 = num5 >= 4;
									bool flag5 = num5 >= 2 && num5 <= 3;
									int num7 = num5 % 2;
									bool flag6 = flag4;
									int num8;
									if (flag6)
									{
										uint blockImageAddress2 = this.GetBlockImageAddress(this.SelectedBlockIndex + 1);
										bool flag7 = unchecked((ulong)blockImageAddress2) == 0UL;
										if (flag7)
										{
											goto IL_017B;
										}
										num8 = (int)blockImageAddress2 + (num6 * 2 + num7) * 2;
									}
									else
									{
										num8 = num2 + (flag5 ? 8 : 0) + (num6 * 2 + num7) * 2;
									}
									this.selectedTiles[j, i] = BitConverter.ToUInt16(this.romData, num8);
								}
								IL_017B:;
							}
						}
					}
					this.isUpdatingUI = true;
					bool flag8 = selectionRect.Width == 1 && selectionRect.Height == 1;
					if (flag8)
					{
						this.chkXFlip.Enabled = true;
						this.chkYFlip.Enabled = true;
						ushort num9 = this.selectedTiles[0, 0];
						this.chkXFlip.Checked = (num9 & 1024) > 0;
						this.chkYFlip.Checked = (num9 & 2048) > 0;
						int num10 = (int)(unchecked((ushort)((uint)num9 >> 12)) & 15);
						bool flag9 = num10 >= 0 && num10 < this.cmbPalette.Items.Count;
						if (flag9)
						{
							this.cmbPalette.SelectedIndex = num10;
						}
						int num11 = (int)(num9 & 1023);
						int num12 = 16;
						this.hasPalSelection = true;
						int num13 = num11 % num12;
						int num14 = num11 / num12;
						this.palSelRect = new Rectangle(num13, num14, 1, 1);
						bool enabled = this.vsrPalette.Enabled;
						if (enabled)
						{
							int num15 = num14 * 8 * 2;
							int height = this.pnlPalette.ClientSize.Height;
							bool flag10 = num15 < this.vsrPalette.Value || num15 + 16 > this.vsrPalette.Value + height;
							if (flag10)
							{
								int num16 = num15 - height / 2 + 8;
								int num17 = Math.Max(0, this.vsrPalette.Maximum - this.vsrPalette.LargeChange + 1);
								this.vsrPalette.Value = Math.Max(0, Math.Min(num16, num17));
							}
						}
						this.pnlPalette.Invalidate();
					}
					else
					{
						this.chkXFlip.Enabled = false;
						this.chkYFlip.Enabled = false;
						this.chkXFlip.Checked = false;
						this.chkYFlip.Checked = false;
					}
					this.isUpdatingUI = false;
					this.pnlPreview.Invalidate();
					this.isSelectingData = false;
					this.pnlData.Invalidate();
				}
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x000053D0 File Offset: 0x000035D0
		private void pnlPreview_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.selectedTiles == null;
			checked
			{
				if (!flag)
				{
					Graphics graphics = e.Graphics;
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					graphics.PixelOffsetMode = PixelOffsetMode.Half;
					graphics.ScaleTransform(2f, 2f);
					int length = this.selectedTiles.GetLength(0);
					int length2 = this.selectedTiles.GetLength(1);
					using (Bitmap bitmap = new Bitmap(length * 8, length2 * 8, PixelFormat.Format32bppArgb))
					{
						BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int stride = bitmapData.Stride;
						byte[] array = new byte[stride * bitmap.Height - 1 + 1];
						int num = length2 - 1;
						for (int i = 0; i <= num; i++)
						{
							int num2 = length - 1;
							for (int j = 0; j <= num2; j++)
							{
								ushort num3 = this.selectedTiles[j, i];
								int num4 = (int)(num3 & 1023);
								bool flag2 = (num3 & 1024) > 0;
								bool flag3 = (num3 & 2048) > 0;
								int num5 = (int)(unchecked((ushort)((uint)num3 >> 12)) & 15);
								this.RenderTile(array, stride, num4, num5, flag2, flag3, j * 8, i * 8);
							}
						}
						Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
						bitmap.UnlockBits(bitmapData);
						graphics.DrawImage(bitmap, 0, 0);
					}
				}
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00005558 File Offset: 0x00003758
		private void pnlPalette_Paint(object sender, PaintEventArgs e)
		{
			bool flag = this.imageBytes == null || this.cmbPalette.SelectedIndex < 0;
			if (!flag)
			{
				bool flag2 = this.paletteCacheBitmap == null || this.cachedPaletteIndex != this.cmbPalette.SelectedIndex;
				if (flag2)
				{
					this.UpdatePaletteCache();
				}
				Graphics graphics = e.Graphics;
				graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
				graphics.PixelOffsetMode = PixelOffsetMode.Half;
				graphics.ScaleTransform(2f, 2f);
				graphics.TranslateTransform(0f, -(float)this.vsrPalette.Value / 2f);
				bool flag3 = this.paletteCacheBitmap != null;
				if (flag3)
				{
					graphics.DrawImage(this.paletteCacheBitmap, 0, 0);
				}
				bool flag4 = this.isSelectingPalette;
				checked
				{
					if (flag4)
					{
						Rectangle selectionRect = this.GetSelectionRect(this.selStartX, this.selStartY, this.selEndX, this.selEndY);
						using (Pen pen = new Pen(Color.Red, 1f))
						{
							graphics.DrawRectangle(pen, selectionRect.X * 8, selectionRect.Y * 8, selectionRect.Width * 8, selectionRect.Height * 8);
						}
					}
					else
					{
						bool flag5 = this.hasPalSelection;
						if (flag5)
						{
							using (Pen pen2 = new Pen(Color.Red, 1f))
							{
								graphics.DrawRectangle(pen2, this.palSelRect.X * 8, this.palSelRect.Y * 8, this.palSelRect.Width * 8, this.palSelRect.Height * 8);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00005720 File Offset: 0x00003920
		private void pnlPalette_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = e.Button == MouseButtons.Right;
			if (flag)
			{
				this.isSelectingPalette = true;
				this.selStartX = e.X / 2 / 8;
				this.selStartY = checked(e.Y + this.vsrPalette.Value) / 2 / 8;
				this.selEndX = this.selStartX;
				this.selEndY = this.selStartY;
				this.pnlPalette.Invalidate();
			}
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00005798 File Offset: 0x00003998
		private void pnlPalette_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = this.isSelectingPalette;
			checked
			{
				if (flag)
				{
					int num = 16;
					int num2 = this.imageBytes.Length / 32 / num;
					this.selEndX = Math.Max(0, Math.Min(num - 1, e.X / 2 / 8));
					this.selEndY = Math.Max(0, Math.Min(num2 - 1, (e.Y + this.vsrPalette.Value) / 2 / 8));
					this.pnlPalette.Invalidate();
				}
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00005818 File Offset: 0x00003A18
		private void pnlPalette_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.isSelectingPalette && e.Button == MouseButtons.Right;
			checked
			{
				if (flag)
				{
					Rectangle selectionRect = this.GetSelectionRect(this.selStartX, this.selStartY, this.selEndX, this.selEndY);
					this.selectedTiles = new ushort[selectionRect.Width - 1 + 1, selectionRect.Height - 1 + 1];
					int selectedIndex = this.cmbPalette.SelectedIndex;
					int num = selectionRect.Height - 1;
					for (int i = 0; i <= num; i++)
					{
						int num2 = selectionRect.Width - 1;
						for (int j = 0; j <= num2; j++)
						{
							int num3 = selectionRect.X + j;
							int num4 = selectionRect.Y + i;
							int num5 = num4 * 16 + num3;
							this.selectedTiles[j, i] = (ushort)((num5 & 1023) | ((selectedIndex & 15) << 12));
						}
					}
					this.isUpdatingUI = true;
					bool flag2 = selectionRect.Width == 1 && selectionRect.Height == 1;
					if (flag2)
					{
						this.chkXFlip.Enabled = true;
						this.chkYFlip.Enabled = true;
						this.chkXFlip.Checked = false;
						this.chkYFlip.Checked = false;
					}
					else
					{
						this.chkXFlip.Enabled = false;
						this.chkYFlip.Enabled = false;
						this.chkXFlip.Checked = false;
						this.chkYFlip.Checked = false;
					}
					this.isUpdatingUI = false;
					this.hasPalSelection = true;
					this.palSelRect = selectionRect;
					this.pnlPreview.Invalidate();
					this.isSelectingPalette = false;
					this.pnlPalette.Invalidate();
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x000059D0 File Offset: 0x00003BD0
		private void UpdatePaletteCache()
		{
			bool flag = this.imageBytes == null || this.cmbPalette.SelectedIndex < 0;
			checked
			{
				if (!flag)
				{
					int selectedIndex = this.cmbPalette.SelectedIndex;
					bool flag2 = this.paletteCacheBitmap != null && this.cachedPaletteIndex == selectedIndex;
					if (!flag2)
					{
						bool flag3 = this.paletteCacheBitmap != null;
						if (flag3)
						{
							this.paletteCacheBitmap.Dispose();
						}
						int num = this.imageBytes.Length / 32;
						int num2 = 16;
						int num3 = (int)Math.Ceiling((double)num / (double)num2);
						int num4 = 128;
						int num5 = num3 * 8;
						bool flag4 = num4 <= 0 || num5 <= 0;
						if (!flag4)
						{
							this.paletteCacheBitmap = new Bitmap(num4, num5, PixelFormat.Format32bppArgb);
							BitmapData bitmapData = this.paletteCacheBitmap.LockBits(new Rectangle(0, 0, num4, num5), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int stride = bitmapData.Stride;
							byte[] array = new byte[stride * num5 - 1 + 1];
							int num6 = selectedIndex * 16;
							int num7 = num - 1;
							for (int i = 0; i <= num7; i++)
							{
								int num8 = i % num2 * 8;
								int num9 = i / num2 * 8;
								int num10 = i * 32;
								int num11 = 0;
								do
								{
									int num12 = 0;
									do
									{
										int num13 = num10 + num11 * 4 + num12 / 2;
										bool flag5 = num13 >= this.imageBytes.Length;
										if (!flag5)
										{
											byte b = this.imageBytes[num13];
											int num14 = (int)(b & 15);
											bool flag6 = num14 > 0;
											if (flag6)
											{
												this.SetPixelToBuffer(array, stride, num8 + num12, num9 + num11, this.palettes[num6 + num14]);
											}
											int num15 = (int)(unchecked((byte)((uint)b >> 4)) & 15);
											bool flag7 = num15 > 0;
											if (flag7)
											{
												this.SetPixelToBuffer(array, stride, num8 + num12 + 1, num9 + num11, this.palettes[num6 + num15]);
											}
										}
										num12 += 2;
									}
									while (num12 <= 7);
									num11++;
								}
								while (num11 <= 7);
							}
							Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
							this.paletteCacheBitmap.UnlockBits(bitmapData);
							this.cachedPaletteIndex = selectedIndex;
						}
					}
				}
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00005BFC File Offset: 0x00003DFC
		private void cmbPalette_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.isUpdatingUI;
			if (flag)
			{
				this.pnlPalette.Invalidate();
			}
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00005C24 File Offset: 0x00003E24
		private void OnFlipChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingUI || this.selectedTiles == null || this.selectedTiles.Length != 1;
			checked
			{
				if (!flag)
				{
					ushort num = this.selectedTiles[0, 0];
					num = (ushort)((int)num & -1025);
					num = (ushort)((int)num & -2049);
					bool @checked = this.chkXFlip.Checked;
					if (@checked)
					{
						num |= 1024;
					}
					bool checked2 = this.chkYFlip.Checked;
					if (checked2)
					{
						num |= 2048;
					}
					this.selectedTiles[0, 0] = num;
					this.pnlPreview.Invalidate();
				}
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00005CC8 File Offset: 0x00003EC8
		private void LoadBlockBehaviorToUI()
		{
			this.lblBlockIndex.Text = string.Format("番号 : {0:D4} (0x{1:X4})", this.SelectedBlockIndex, this.SelectedBlockIndex);
			uint behaviorAddress = this.GetBehaviorAddress(this.SelectedBlockIndex);
			this.isUpdatingUI = true;
			bool flag = (ulong)behaviorAddress == 0UL;
			if (flag)
			{
				this.nudTileAction.Value = 0m;
				this.nudAttribute.Value = 0m;
				this.nudUnknown.Value = 0m;
				this.cmbLayer.SelectedIndex = -1;
				this.chkWildGrass.Checked = false;
				this.chkWildWater.Checked = false;
			}
			else
			{
				BlockEditor.BehaviorData behaviorData = default(BlockEditor.BehaviorData);
				behaviorData.Load(this.romData, behaviorAddress);
				bool flag2 = decimal.Compare(new decimal((int)behaviorData.TileAction), this.nudTileAction.Maximum) <= 0;
				if (flag2)
				{
					this.nudTileAction.Value = new decimal((int)behaviorData.TileAction);
				}
				bool flag3 = decimal.Compare(new decimal((int)behaviorData.Attribute), this.nudAttribute.Maximum) <= 0;
				if (flag3)
				{
					this.nudAttribute.Value = new decimal((int)behaviorData.Attribute);
				}
				bool flag4 = decimal.Compare(new decimal((int)behaviorData.Unknown), this.nudUnknown.Maximum) <= 0;
				if (flag4)
				{
					this.nudUnknown.Value = new decimal((int)behaviorData.Unknown);
				}
				this.SelectComboBoxByValue(this.cmbLayer, string.Format("[{0:X2}]", behaviorData.LayerVal));
				this.chkWildGrass.Checked = behaviorData.IsWildGrass;
				this.chkWildWater.Checked = behaviorData.IsWildWater;
			}
			this.isUpdatingUI = false;
			this.SetUnsavedState(false);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00005EA8 File Offset: 0x000040A8
		private void SelectComboBoxByValue(ComboBox cmb, string prefix)
		{
			checked
			{
				int num = cmb.Items.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					bool flag = cmb.Items[i].ToString().StartsWith(prefix);
					if (flag)
					{
						cmb.SelectedIndex = i;
						return;
					}
				}
				cmb.SelectedIndex = -1;
			}
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00005F00 File Offset: 0x00004100
		private void SaveCurrentBlockBehavior()
		{
			bool flag = this.isUpdatingUI || this.romData == null;
			if (!flag)
			{
				uint behaviorAddress = this.GetBehaviorAddress(this.SelectedBlockIndex);
				bool flag2 = (ulong)behaviorAddress == 0UL;
				checked
				{
					if (!flag2)
					{
						BlockEditor.BehaviorData behaviorData = default(BlockEditor.BehaviorData);
						behaviorData.TileAction = Convert.ToByte(this.nudTileAction.Value);
						behaviorData.Attribute = Convert.ToByte(this.nudAttribute.Value);
						behaviorData.Unknown = Convert.ToByte(this.nudUnknown.Value);
						behaviorData.IsWildGrass = this.chkWildGrass.Checked;
						behaviorData.IsWildWater = this.chkWildWater.Checked;
						bool flag3 = this.cmbLayer.SelectedItem != null;
						if (flag3)
						{
							string text = this.cmbLayer.SelectedItem.ToString();
							bool flag4 = text.Length >= 4 && text.StartsWith("[");
							if (flag4)
							{
								behaviorData.LayerVal = Convert.ToByte(text.Substring(1, 2), 16);
							}
						}
						behaviorData.Save(this.romData, behaviorAddress);
						bool flag5 = behaviorData.LayerVal == 48 && this.SelectedBlockIndex + 1 < this.totalBlocks;
						if (flag5)
						{
							uint behaviorAddress2 = this.GetBehaviorAddress(this.SelectedBlockIndex + 1);
							bool flag6 = unchecked((ulong)behaviorAddress2) > 0UL;
							if (flag6)
							{
								BlockEditor.BehaviorData behaviorData2 = default(BlockEditor.BehaviorData);
								behaviorData2.LayerVal = 0;
								behaviorData2.TileAction = 0;
								behaviorData2.Attribute = 0;
								behaviorData2.Unknown = 0;
								behaviorData2.IsWildGrass = false;
								behaviorData2.IsWildWater = false;
								behaviorData2.Save(this.romData, behaviorAddress2);
							}
							uint blockImageAddress = this.GetBlockImageAddress(this.SelectedBlockIndex + 1);
							bool flag7 = unchecked((ulong)blockImageAddress) > 0UL;
							if (flag7)
							{
								int num = 8;
								do
								{
									this.romData[(int)blockImageAddress + num] = 0;
									num++;
								}
								while (num <= 15);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000060F0 File Offset: 0x000042F0
		private void UpdateBlockPaletteBitmapCache(int blockId)
		{
			bool flag = this.blockPaletteBitmap == null || blockId < 0 || blockId >= this.totalBlocks;
			if (!flag)
			{
				uint blockImageAddress = this.GetBlockImageAddress(blockId);
				bool flag2 = (ulong)blockImageAddress == 0UL;
				checked
				{
					if (!flag2)
					{
						int num = this.blockPaletteBitmap.Width / 16;
						int num2 = blockId % num;
						int num3 = blockId / num;
						using (Graphics graphics = Graphics.FromImage(this.blockPaletteBitmap))
						{
							graphics.CompositingMode = CompositingMode.SourceCopy;
							graphics.FillRectangle(Brushes.Transparent, num2 * 16, num3 * 16, 16, 16);
							graphics.CompositingMode = CompositingMode.SourceOver;
							bool flag3 = blockId > 0 && this.IsTripleLayer(blockId - 1);
							if (!flag3)
							{
								using (Bitmap bitmap = this.CreateBlockBitmap(blockId))
								{
									graphics.DrawImage(bitmap, num2 * 16, num3 * 16);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00006200 File Offset: 0x00004400
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveCurrentBlockBehavior();
			this.UpdateBlockPaletteBitmapCache(this.SelectedBlockIndex);
			checked
			{
				bool flag = this.IsTripleLayer(this.SelectedBlockIndex) && this.SelectedBlockIndex + 1 < this.totalBlocks;
				if (flag)
				{
					this.UpdateBlockPaletteBitmapCache(this.SelectedBlockIndex + 1);
				}
				this.BackupCurrentBlock();
				this.SetUnsavedState(false);
				this.UpdatePnlDataSize();
				this.pnlData.Invalidate();
				this.pnlBlock.Invalidate();
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00006288 File Offset: 0x00004488
		private bool ConfirmSaveBeforeSwitch()
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
							this.RestoreCurrentBlock();
							this.SetUnsavedState(false);
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

		// Token: 0x06000084 RID: 132 RVA: 0x000062FC File Offset: 0x000044FC
		private void OnDataChanged(object sender, EventArgs e)
		{
			bool flag = !this.isUpdatingUI;
			if (flag)
			{
				bool flag2 = sender == this.cmbLayer && this.cmbLayer.SelectedItem != null;
				if (flag2)
				{
					string text = this.cmbLayer.SelectedItem.ToString();
					bool flag3 = text.Length >= 4 && text.StartsWith("[");
					if (flag3)
					{
						byte b = Convert.ToByte(text.Substring(1, 2), 16);
						bool flag4 = b == 48 && checked(this.SelectedBlockIndex + 1) >= this.totalBlocks;
						if (flag4)
						{
							MessageBox.Show("次のブロックが存在しないため、トリプルレイヤーに設定できません。", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							this.LoadBlockBehaviorToUI();
							return;
						}
					}
				}
				this.SetUnsavedState(true);
				bool flag5 = sender == this.cmbLayer;
				if (flag5)
				{
					this.UpdatePnlDataSize();
					this.pnlData.Invalidate();
					this.pnlBlock.Invalidate();
				}
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000063F2 File Offset: 0x000045F2
		private void SetUnsavedState(bool changed)
		{
			this.hasUnsavedChanges = changed;
			this.btnSave.Enabled = changed;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x0000640C File Offset: 0x0000460C
		private void BlockEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = !this.ConfirmSaveBeforeSwitch();
			if (flag)
			{
				e.Cancel = true;
			}
			else
			{
				base.DialogResult = DialogResult.OK;
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x0000643C File Offset: 0x0000463C
		private void vsbBlock_Scroll(object sender, ScrollEventArgs e)
		{
			this.pnlBlock.Invalidate();
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000644B File Offset: 0x0000464B
		private void vsrPalette_Scroll(object sender, ScrollEventArgs e)
		{
			this.pnlPalette.Invalidate();
		}

		// Token: 0x06000089 RID: 137 RVA: 0x0000645C File Offset: 0x0000465C
		private void pnlBlock_MouseWheel(object sender, MouseEventArgs e)
		{
			bool flag = !this.vsbBlock.Enabled;
			checked
			{
				if (!flag)
				{
					int num = Math.Max(0, this.vsbBlock.Maximum - this.vsbBlock.LargeChange + 1);
					int num2 = Math.Min(Math.Max(0, this.vsbBlock.Value - e.Delta / 120 * this.vsbBlock.SmallChange), num);
					bool flag2 = this.vsbBlock.Value != num2;
					if (flag2)
					{
						this.vsbBlock.Value = num2;
						this.pnlBlock.Invalidate();
					}
					((HandledMouseEventArgs)e).Handled = true;
				}
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x0000650C File Offset: 0x0000470C
		private void pnlPalette_MouseWheel(object sender, MouseEventArgs e)
		{
			bool flag = !this.vsrPalette.Enabled;
			if (!flag)
			{
				int num = checked(Math.Min(Math.Max(0, this.vsrPalette.Value - e.Delta / 120 * this.vsrPalette.SmallChange), Math.Max(0, this.vsrPalette.Maximum - this.vsrPalette.LargeChange + 1)));
				bool flag2 = this.vsrPalette.Value != num;
				if (flag2)
				{
					this.vsrPalette.Value = num;
					this.pnlPalette.Invalidate();
				}
				((HandledMouseEventArgs)e).Handled = true;
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000065BC File Offset: 0x000047BC
		private void pnlBlock_MouseMove(object sender, MouseEventArgs e)
		{
			bool flag = this.blockPaletteBitmap == null;
			checked
			{
				if (!flag)
				{
					int num = this.blockPaletteBitmap.Width / 16;
					int num2 = this.blockPaletteBitmap.Height / 16;
					int num3 = e.X / 32;
					int num4 = (e.Y + this.vsbBlock.Value) / 32;
					bool flag2 = num3 >= 0 && num3 < num && num4 >= 0 && num4 < num2;
					if (flag2)
					{
						int num5 = num4 * num + num3;
						bool flag3 = num5 < this.totalBlocks;
						if (flag3)
						{
							this.lblBLockInfo.Text = string.Format("{0}, {1:D4} (0x{2:X4})", (num5 < this.ts1BlockCount) ? "タイルセット1" : "タイルセット2", num5, num5);
						}
						else
						{
							this.lblBLockInfo.Text = "";
						}
					}
					else
					{
						this.lblBLockInfo.Text = "";
					}
				}
			}
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000066B4 File Offset: 0x000048B4
		private bool IsTripleLayer(int blockId)
		{
			bool flag = blockId < 0 || blockId >= this.totalBlocks;
			bool flag2;
			if (flag)
			{
				flag2 = false;
			}
			else
			{
				uint behaviorAddress = this.GetBehaviorAddress(blockId);
				bool flag3 = (ulong)behaviorAddress == 0UL;
				if (flag3)
				{
					flag2 = false;
				}
				else
				{
					byte b = this.romData[checked((int)behaviorAddress + 3)];
					flag2 = (b & 252) == 48;
				}
			}
			return flag2;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00006710 File Offset: 0x00004910
		private bool IsCurrentBlockTripleLayer()
		{
			bool flag = this.cmbLayer.SelectedItem != null;
			if (flag)
			{
				string text = this.cmbLayer.SelectedItem.ToString();
				bool flag2 = text.Length >= 4 && text.StartsWith("[");
				if (flag2)
				{
					return Convert.ToByte(text.Substring(1, 2), 16) == 48;
				}
			}
			return this.IsTripleLayer(this.SelectedBlockIndex);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00006788 File Offset: 0x00004988
		private void UpdatePnlDataSize()
		{
			bool flag = this.IsCurrentBlockTripleLayer();
			if (flag)
			{
				this.pnlData.Width = 96;
			}
			else
			{
				this.pnlData.Width = 64;
			}
		}

		// Token: 0x0400002F RID: 47
		private byte[] romData;

		// Token: 0x04000030 RID: 48
		private Bitmap blockPaletteBitmap;

		// Token: 0x04000031 RID: 49
		private bool showGrid;

		// Token: 0x04000032 RID: 50
		private uint ts1BehaviorAddr;

		// Token: 0x04000033 RID: 51
		private uint ts2BehaviorAddr;

		// Token: 0x04000034 RID: 52
		private int ts1BlockCount;

		// Token: 0x04000035 RID: 53
		private int totalBlocks;

		// Token: 0x04000036 RID: 54
		private byte[] imageBytes;

		// Token: 0x04000037 RID: 55
		private Color[] palettes;

		// Token: 0x04000038 RID: 56
		private uint ts1BlockImageAddr;

		// Token: 0x04000039 RID: 57
		private uint ts2BlockImageAddr;

		// Token: 0x0400003A RID: 58
		private ushort[,] selectedTiles;

		// Token: 0x0400003B RID: 59
		private int selStartX;

		// Token: 0x0400003C RID: 60
		private int selStartY;

		// Token: 0x0400003D RID: 61
		private int selEndX;

		// Token: 0x0400003E RID: 62
		private int selEndY;

		// Token: 0x0400003F RID: 63
		private bool isSelectingPalette;

		// Token: 0x04000040 RID: 64
		private bool isSelectingData;

		// Token: 0x04000041 RID: 65
		private bool hasPalSelection;

		// Token: 0x04000042 RID: 66
		private Rectangle palSelRect;

		// Token: 0x04000043 RID: 67
		private byte[] backupBlockData;

		// Token: 0x04000044 RID: 68
		private byte[] backupBehaviorData;

		// Token: 0x04000045 RID: 69
		private byte[] backupNextBlockData;

		// Token: 0x04000046 RID: 70
		private byte[] backupNextBehaviorData;

		// Token: 0x04000047 RID: 71
		private Bitmap paletteCacheBitmap;

		// Token: 0x04000048 RID: 72
		private int cachedPaletteIndex;

		// Token: 0x04000049 RID: 73
		public const int BEHAVIOR_SIZE = 4;

		// Token: 0x0400004A RID: 74
		public const int OFFSET_TILE_ACTION = 0;

		// Token: 0x0400004B RID: 75
		public const int OFFSET_ATTRIBUTE = 1;

		// Token: 0x0400004C RID: 76
		public const int OFFSET_UNKNOWN = 2;

		// Token: 0x0400004D RID: 77
		public const int OFFSET_LAYER_AND_WILD = 3;

		// Token: 0x0400004E RID: 78
		public const int TRIPLE_LAYER_VALUE = 48;

		// Token: 0x0400004F RID: 79
		private bool hasUnsavedChanges;

		// Token: 0x04000050 RID: 80
		private bool isUpdatingUI;

		// Token: 0x04000051 RID: 81
		private int currentBlockIndex;

		// Token: 0x04000052 RID: 82
		private const int ZOOM = 2;

		// Token: 0x04000053 RID: 83
		private BlockEditor.LinkedDataController tileActionController;

		// Token: 0x04000054 RID: 84
		private BlockEditor.LinkedDataController attributeController;

		// Token: 0x04000055 RID: 85
		private BlockEditor.LinkedDataController unknownController;

		// Token: 0x04000056 RID: 86
		private BlockEditor.ComboBoxOnlyController layerController;

		// Token: 0x0200002F RID: 47
		private struct BehaviorData
		{
			// Token: 0x06000EAE RID: 3758 RVA: 0x0006A6A8 File Offset: 0x000688A8
			public void Load(byte[] rom, uint addr)
			{
				checked
				{
					this.TileAction = rom[(int)addr + 0];
					this.Attribute = rom[(int)addr + 1];
					this.Unknown = rom[(int)addr + 2];
					byte b = rom[(int)addr + 3];
					this.LayerVal = (byte)(b & 252);
					this.IsWildGrass = (b & 1) > 0;
					this.IsWildWater = (b & 2) > 0;
				}
			}

			// Token: 0x06000EAF RID: 3759 RVA: 0x0006A708 File Offset: 0x00068908
			public void Save(byte[] rom, uint addr)
			{
				checked
				{
					rom[(int)addr + 0] = this.TileAction;
					rom[(int)addr + 1] = this.Attribute;
					rom[(int)addr + 2] = this.Unknown;
					byte b = 0;
					bool isWildGrass = this.IsWildGrass;
					if (isWildGrass)
					{
						b |= 1;
					}
					bool isWildWater = this.IsWildWater;
					if (isWildWater)
					{
						b |= 2;
					}
					rom[(int)addr + 3] = (byte)(this.LayerVal | b);
				}
			}

			// Token: 0x040007FE RID: 2046
			public byte TileAction;

			// Token: 0x040007FF RID: 2047
			public byte Attribute;

			// Token: 0x04000800 RID: 2048
			public byte Unknown;

			// Token: 0x04000801 RID: 2049
			public byte LayerVal;

			// Token: 0x04000802 RID: 2050
			public bool IsWildGrass;

			// Token: 0x04000803 RID: 2051
			public bool IsWildWater;
		}

		// Token: 0x02000030 RID: 48
		public class BlockInfoItem
		{
			// Token: 0x17000590 RID: 1424
			// (get) Token: 0x06000EB0 RID: 3760 RVA: 0x0006A768 File Offset: 0x00068968
			// (set) Token: 0x06000EB1 RID: 3761 RVA: 0x0006A772 File Offset: 0x00068972
			public int ByteValue { get; set; }

			// Token: 0x17000591 RID: 1425
			// (get) Token: 0x06000EB2 RID: 3762 RVA: 0x0006A77B File Offset: 0x0006897B
			// (set) Token: 0x06000EB3 RID: 3763 RVA: 0x0006A785 File Offset: 0x00068985
			public string DisplayText { get; set; }

			// Token: 0x06000EB4 RID: 3764 RVA: 0x0006A790 File Offset: 0x00068990
			public BlockInfoItem(string line)
			{
				int num = line.IndexOf("]");
				this.ByteValue = Convert.ToInt32(line.Substring(1, checked(num - 1)), 16);
				this.DisplayText = line;
			}

			// Token: 0x06000EB5 RID: 3765 RVA: 0x0006A7D4 File Offset: 0x000689D4
			public override string ToString()
			{
				return this.DisplayText;
			}
		}

		// Token: 0x02000031 RID: 49
		public class LinkedDataController
		{
			// Token: 0x06000EB6 RID: 3766 RVA: 0x0006A7EC File Offset: 0x000689EC
			public LinkedDataController(NumericUpDown n, TextBox t, ComboBox c)
			{
				this.isUpdating = false;
				this.nud = n;
				this.txt = t;
				this.cmb = c;
				this.nud.ValueChanged += this.OnNudChanged;
				this.txt.TextChanged += this.OnTxtChanged;
				this.cmb.SelectedIndexChanged += this.OnCmbChanged;
			}

			// Token: 0x06000EB7 RID: 3767 RVA: 0x0006A868 File Offset: 0x00068A68
			public void LoadData(string filePath, Encoding encoding)
			{
				this.cmb.Items.Clear();
				bool flag = File.Exists(filePath);
				if (flag)
				{
					foreach (string text in File.ReadAllLines(filePath, encoding))
					{
						bool flag2 = !string.IsNullOrWhiteSpace(text);
						if (flag2)
						{
							this.cmb.Items.Add(new BlockEditor.BlockInfoItem(text));
						}
					}
					this.nud.Maximum = new decimal(Math.Max(0, checked(this.cmb.Items.Count - 1)));
					bool flag3 = this.cmb.Items.Count > 0;
					if (flag3)
					{
						this.cmb.SelectedIndex = 0;
					}
				}
			}

			// Token: 0x06000EB8 RID: 3768 RVA: 0x0006A92C File Offset: 0x00068B2C
			private void SyncControls(int val)
			{
				this.isUpdating = true;
				this.nud.Value = new decimal(val);
				this.txt.Text = val.ToString("X2");
				checked
				{
					int num = this.cmb.Items.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						bool flag = ((BlockEditor.BlockInfoItem)this.cmb.Items[i]).ByteValue == val;
						if (flag)
						{
							this.cmb.SelectedIndex = i;
							break;
						}
					}
					this.isUpdating = false;
				}
			}

			// Token: 0x06000EB9 RID: 3769 RVA: 0x0006A9C4 File Offset: 0x00068BC4
			private void OnNudChanged(object sender, EventArgs e)
			{
				bool flag = !this.isUpdating;
				if (flag)
				{
					this.SyncControls(Convert.ToInt32(this.nud.Value));
				}
			}

			// Token: 0x06000EBA RID: 3770 RVA: 0x0006A9F8 File Offset: 0x00068BF8
			private void OnTxtChanged(object sender, EventArgs e)
			{
				bool flag = this.isUpdating;
				if (!flag)
				{
					int num;
					bool flag2 = int.TryParse(this.txt.Text, NumberStyles.HexNumber, null, out num);
					if (flag2)
					{
						bool flag3 = decimal.Compare(new decimal(num), this.nud.Minimum) >= 0 && decimal.Compare(new decimal(num), this.nud.Maximum) <= 0;
						if (flag3)
						{
							this.SyncControls(num);
						}
					}
				}
			}

			// Token: 0x06000EBB RID: 3771 RVA: 0x0006AA74 File Offset: 0x00068C74
			private void OnCmbChanged(object sender, EventArgs e)
			{
				bool flag = !this.isUpdating && this.cmb.SelectedIndex >= 0;
				if (flag)
				{
					this.SyncControls(((BlockEditor.BlockInfoItem)this.cmb.SelectedItem).ByteValue);
				}
			}

			// Token: 0x04000806 RID: 2054
			private bool isUpdating;

			// Token: 0x04000807 RID: 2055
			private NumericUpDown nud;

			// Token: 0x04000808 RID: 2056
			private TextBox txt;

			// Token: 0x04000809 RID: 2057
			private ComboBox cmb;
		}

		// Token: 0x02000032 RID: 50
		public class ComboBoxOnlyController
		{
			// Token: 0x06000EBC RID: 3772 RVA: 0x0006AAC0 File Offset: 0x00068CC0
			public ComboBoxOnlyController(ComboBox c)
			{
				this.cmb = c;
			}

			// Token: 0x06000EBD RID: 3773 RVA: 0x0006AAD4 File Offset: 0x00068CD4
			public void LoadData(string filePath, Encoding encoding)
			{
				this.cmb.Items.Clear();
				bool flag = File.Exists(filePath);
				if (flag)
				{
					foreach (string text in File.ReadAllLines(filePath, encoding))
					{
						bool flag2 = !string.IsNullOrWhiteSpace(text);
						if (flag2)
						{
							this.cmb.Items.Add(new BlockEditor.BlockInfoItem(text));
						}
					}
					bool flag3 = this.cmb.Items.Count > 0;
					if (flag3)
					{
						this.cmb.SelectedIndex = 0;
					}
				}
			}

			// Token: 0x0400080A RID: 2058
			private ComboBox cmb;
		}
	}
}
