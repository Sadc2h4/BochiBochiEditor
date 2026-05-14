using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x0200001D RID: 29
	public partial class OverWorldEditor : Form
	{
		// Token: 0x06000783 RID: 1923 RVA: 0x00039E1C File Offset: 0x0003801C
		public OverWorldEditor()
		{
			base.Load += this.OverWorldEditor_Load;
			base.FormClosing += this.OverWorldEditor_FormClosing;
			this.OVERWORLD_DATA_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("OVERWORLD_DATA_TABLE_OFFSET");
			this.OVERWORLD_PALETTE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("OVERWORLD_PALETTE_TABLE_OFFSET");
			this.OVERWORLD_FONT_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("OVERWORLD_FONT_TABLE_OFFSET");
			this.OVERWORLD_DATA_MAX_COUNT_OFFSET = RomIniReader.ReadHexOrDecimal("OVERWORLD_DATA_MAX_COUNT_OFFSET");
			this.romData = MainForm.romData;
			this.hasUnsavedChanges = false;
			this.currentIndex = -1;
			this.overworldDataList = new List<OverWorldEditor.OverWorldData>();
			this.paletteTableDataList = new List<OverWorldEditor.PaletteTableData>();
			this.spriteFrameDataList = new List<OverWorldEditor.SpriteFrameData>();
			this.frameLimits = new Dictionary<int, int>();
			this.temporaryOverWorldDataAddress = -1;
			this.temporaryFrameData = new Dictionary<int, byte[]>();
			this.dataSizeMapping = new Dictionary<string, ValueTuple<int, int, int>>
			{
				{
					"16x32",
					new ValueTuple<int, int, int>(16, 32, 256)
				},
				{
					"32x32",
					new ValueTuple<int, int, int>(32, 32, 512)
				},
				{
					"16x16",
					new ValueTuple<int, int, int>(16, 16, 128)
				},
				{
					"64x64",
					new ValueTuple<int, int, int>(64, 64, 2048)
				},
				{
					"128x64",
					new ValueTuple<int, int, int>(128, 64, 4096)
				},
				{
					"32x16",
					new ValueTuple<int, int, int>(32, 16, 256)
				}
			};
			this.dataFootPrintMapping = new Dictionary<string, byte>
			{
				{ "無効", 0 },
				{ "通常", 1 },
				{ "自転車", 2 }
			};
			this.InitializeComponent();
		}

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000786 RID: 1926 RVA: 0x0003C99B File Offset: 0x0003AB9B
		// (set) Token: 0x06000787 RID: 1927 RVA: 0x0003C9A5 File Offset: 0x0003ABA5
		internal virtual Label lblDataTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000788 RID: 1928 RVA: 0x0003C9AE File Offset: 0x0003ABAE
		// (set) Token: 0x06000789 RID: 1929 RVA: 0x0003C9B8 File Offset: 0x0003ABB8
		internal virtual TextBox txtDataTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x0600078A RID: 1930 RVA: 0x0003C9C1 File Offset: 0x0003ABC1
		// (set) Token: 0x0600078B RID: 1931 RVA: 0x0003C9CC File Offset: 0x0003ABCC
		internal virtual ListBox lstOverWorldDataList
		{
			[CompilerGenerated]
			get
			{
				return this._lstOverWorldDataList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstOverWorldDataList_SelectedIndexChanged);
				ListBox listBox = this._lstOverWorldDataList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstOverWorldDataList = value;
				listBox = this._lstOverWorldDataList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x0600078C RID: 1932 RVA: 0x0003CA0F File Offset: 0x0003AC0F
		// (set) Token: 0x0600078D RID: 1933 RVA: 0x0003CA19 File Offset: 0x0003AC19
		internal virtual Label lblPaletteTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x0600078E RID: 1934 RVA: 0x0003CA22 File Offset: 0x0003AC22
		// (set) Token: 0x0600078F RID: 1935 RVA: 0x0003CA2C File Offset: 0x0003AC2C
		internal virtual TextBox txtPaletteTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000790 RID: 1936 RVA: 0x0003CA35 File Offset: 0x0003AC35
		// (set) Token: 0x06000791 RID: 1937 RVA: 0x0003CA3F File Offset: 0x0003AC3F
		internal virtual GroupBox grpData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000792 RID: 1938 RVA: 0x0003CA48 File Offset: 0x0003AC48
		// (set) Token: 0x06000793 RID: 1939 RVA: 0x0003CA52 File Offset: 0x0003AC52
		internal virtual Label lblDataPaletteId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000794 RID: 1940 RVA: 0x0003CA5B File Offset: 0x0003AC5B
		// (set) Token: 0x06000795 RID: 1941 RVA: 0x0003CA65 File Offset: 0x0003AC65
		internal virtual Label lblFontTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000796 RID: 1942 RVA: 0x0003CA6E File Offset: 0x0003AC6E
		// (set) Token: 0x06000797 RID: 1943 RVA: 0x0003CA78 File Offset: 0x0003AC78
		internal virtual TextBox txtFontTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000798 RID: 1944 RVA: 0x0003CA81 File Offset: 0x0003AC81
		// (set) Token: 0x06000799 RID: 1945 RVA: 0x0003CA8B File Offset: 0x0003AC8B
		internal virtual Label lblDataNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x0600079A RID: 1946 RVA: 0x0003CA94 File Offset: 0x0003AC94
		// (set) Token: 0x0600079B RID: 1947 RVA: 0x0003CA9E File Offset: 0x0003AC9E
		internal virtual NumericUpDown nudDataMaxCount
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x0600079C RID: 1948 RVA: 0x0003CAA7 File Offset: 0x0003ACA7
		// (set) Token: 0x0600079D RID: 1949 RVA: 0x0003CAB1 File Offset: 0x0003ACB1
		internal virtual Label lblDataSizeA
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x0600079E RID: 1950 RVA: 0x0003CABA File Offset: 0x0003ACBA
		// (set) Token: 0x0600079F RID: 1951 RVA: 0x0003CAC4 File Offset: 0x0003ACC4
		internal virtual ComboBox cmbDataSizeA
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x060007A0 RID: 1952 RVA: 0x0003CACD File Offset: 0x0003ACCD
		// (set) Token: 0x060007A1 RID: 1953 RVA: 0x0003CAD7 File Offset: 0x0003ACD7
		internal virtual GroupBox grpDataUnknownValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x060007A2 RID: 1954 RVA: 0x0003CAE0 File Offset: 0x0003ACE0
		// (set) Token: 0x060007A3 RID: 1955 RVA: 0x0003CAEA File Offset: 0x0003ACEA
		internal virtual CheckBox chkDataUnknownValue3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x060007A4 RID: 1956 RVA: 0x0003CAF3 File Offset: 0x0003ACF3
		// (set) Token: 0x060007A5 RID: 1957 RVA: 0x0003CAFD File Offset: 0x0003ACFD
		internal virtual CheckBox chkDataUnknownValue2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x060007A6 RID: 1958 RVA: 0x0003CB06 File Offset: 0x0003AD06
		// (set) Token: 0x060007A7 RID: 1959 RVA: 0x0003CB10 File Offset: 0x0003AD10
		internal virtual CheckBox chkDataUnknownValue1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x060007A8 RID: 1960 RVA: 0x0003CB19 File Offset: 0x0003AD19
		// (set) Token: 0x060007A9 RID: 1961 RVA: 0x0003CB23 File Offset: 0x0003AD23
		internal virtual NumericUpDown nudDataPaletteSlot
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x060007AA RID: 1962 RVA: 0x0003CB2C File Offset: 0x0003AD2C
		// (set) Token: 0x060007AB RID: 1963 RVA: 0x0003CB36 File Offset: 0x0003AD36
		internal virtual NumericUpDown nudDataLength
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x060007AC RID: 1964 RVA: 0x0003CB3F File Offset: 0x0003AD3F
		// (set) Token: 0x060007AD RID: 1965 RVA: 0x0003CB49 File Offset: 0x0003AD49
		internal virtual Label lblDataLength
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x060007AE RID: 1966 RVA: 0x0003CB52 File Offset: 0x0003AD52
		// (set) Token: 0x060007AF RID: 1967 RVA: 0x0003CB5C File Offset: 0x0003AD5C
		internal virtual Label lblDataPaletteSlot
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x060007B0 RID: 1968 RVA: 0x0003CB65 File Offset: 0x0003AD65
		// (set) Token: 0x060007B1 RID: 1969 RVA: 0x0003CB6F File Offset: 0x0003AD6F
		internal virtual TextBox txtDataPadding
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x060007B2 RID: 1970 RVA: 0x0003CB78 File Offset: 0x0003AD78
		// (set) Token: 0x060007B3 RID: 1971 RVA: 0x0003CB82 File Offset: 0x0003AD82
		internal virtual Label lblDataPadding
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x060007B4 RID: 1972 RVA: 0x0003CB8B File Offset: 0x0003AD8B
		// (set) Token: 0x060007B5 RID: 1973 RVA: 0x0003CB95 File Offset: 0x0003AD95
		internal virtual ComboBox cmbDataFootPrint
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x060007B6 RID: 1974 RVA: 0x0003CB9E File Offset: 0x0003AD9E
		// (set) Token: 0x060007B7 RID: 1975 RVA: 0x0003CBA8 File Offset: 0x0003ADA8
		internal virtual Label lblDataFootPrintUnUsedValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x060007B8 RID: 1976 RVA: 0x0003CBB1 File Offset: 0x0003ADB1
		// (set) Token: 0x060007B9 RID: 1977 RVA: 0x0003CBBC File Offset: 0x0003ADBC
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

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x060007BA RID: 1978 RVA: 0x0003CBFF File Offset: 0x0003ADFF
		// (set) Token: 0x060007BB RID: 1979 RVA: 0x0003CC09 File Offset: 0x0003AE09
		internal virtual TextBox txtDataMemoryAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x060007BC RID: 1980 RVA: 0x0003CC12 File Offset: 0x0003AE12
		// (set) Token: 0x060007BD RID: 1981 RVA: 0x0003CC1C File Offset: 0x0003AE1C
		internal virtual TextBox txtDataSpriteTable
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x060007BE RID: 1982 RVA: 0x0003CC25 File Offset: 0x0003AE25
		// (set) Token: 0x060007BF RID: 1983 RVA: 0x0003CC2F File Offset: 0x0003AE2F
		internal virtual TextBox txtDataAnimationAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x060007C0 RID: 1984 RVA: 0x0003CC38 File Offset: 0x0003AE38
		// (set) Token: 0x060007C1 RID: 1985 RVA: 0x0003CC42 File Offset: 0x0003AE42
		internal virtual TextBox txtDataSizeAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x060007C2 RID: 1986 RVA: 0x0003CC4B File Offset: 0x0003AE4B
		// (set) Token: 0x060007C3 RID: 1987 RVA: 0x0003CC55 File Offset: 0x0003AE55
		internal virtual TextBox txtDataLoadAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x060007C4 RID: 1988 RVA: 0x0003CC5E File Offset: 0x0003AE5E
		// (set) Token: 0x060007C5 RID: 1989 RVA: 0x0003CC68 File Offset: 0x0003AE68
		internal virtual Label lblDataMemoryAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x060007C6 RID: 1990 RVA: 0x0003CC71 File Offset: 0x0003AE71
		// (set) Token: 0x060007C7 RID: 1991 RVA: 0x0003CC7B File Offset: 0x0003AE7B
		internal virtual Label lblDataSpriteTable
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x060007C8 RID: 1992 RVA: 0x0003CC84 File Offset: 0x0003AE84
		// (set) Token: 0x060007C9 RID: 1993 RVA: 0x0003CC8E File Offset: 0x0003AE8E
		internal virtual Label lblDataAnimationAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x060007CA RID: 1994 RVA: 0x0003CC97 File Offset: 0x0003AE97
		// (set) Token: 0x060007CB RID: 1995 RVA: 0x0003CCA1 File Offset: 0x0003AEA1
		internal virtual Label lblDataSizeAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x060007CC RID: 1996 RVA: 0x0003CCAA File Offset: 0x0003AEAA
		// (set) Token: 0x060007CD RID: 1997 RVA: 0x0003CCB4 File Offset: 0x0003AEB4
		internal virtual Label lblDataLoadAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x060007CE RID: 1998 RVA: 0x0003CCBD File Offset: 0x0003AEBD
		// (set) Token: 0x060007CF RID: 1999 RVA: 0x0003CCC7 File Offset: 0x0003AEC7
		internal virtual GroupBox grpSpritePreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x060007D0 RID: 2000 RVA: 0x0003CCD0 File Offset: 0x0003AED0
		// (set) Token: 0x060007D1 RID: 2001 RVA: 0x0003CCDA File Offset: 0x0003AEDA
		internal virtual TextBox txtSpriteFrameAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x060007D2 RID: 2002 RVA: 0x0003CCE3 File Offset: 0x0003AEE3
		// (set) Token: 0x060007D3 RID: 2003 RVA: 0x0003CCF0 File Offset: 0x0003AEF0
		internal virtual Button btnSpriteSheetExport
		{
			[CompilerGenerated]
			get
			{
				return this._btnSpriteSheetExport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSpriteSheetExport_Click);
				Button button = this._btnSpriteSheetExport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSpriteSheetExport = value;
				button = this._btnSpriteSheetExport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x060007D4 RID: 2004 RVA: 0x0003CD33 File Offset: 0x0003AF33
		// (set) Token: 0x060007D5 RID: 2005 RVA: 0x0003CD3D File Offset: 0x0003AF3D
		internal virtual TextBox txtCreateNewSpriteSheetAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x060007D6 RID: 2006 RVA: 0x0003CD46 File Offset: 0x0003AF46
		// (set) Token: 0x060007D7 RID: 2007 RVA: 0x0003CD50 File Offset: 0x0003AF50
		internal virtual GroupBox grpFont
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x060007D8 RID: 2008 RVA: 0x0003CD59 File Offset: 0x0003AF59
		// (set) Token: 0x060007D9 RID: 2009 RVA: 0x0003CD63 File Offset: 0x0003AF63
		internal virtual TextBox txtCreatePaletteId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x060007DA RID: 2010 RVA: 0x0003CD6C File Offset: 0x0003AF6C
		// (set) Token: 0x060007DB RID: 2011 RVA: 0x0003CD78 File Offset: 0x0003AF78
		internal virtual ComboBox cmbPaletteId
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPaletteId;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPaletteId_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPaletteId;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPaletteId = value;
				comboBox = this._cmbPaletteId;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x060007DC RID: 2012 RVA: 0x0003CDBB File Offset: 0x0003AFBB
		// (set) Token: 0x060007DD RID: 2013 RVA: 0x0003CDC5 File Offset: 0x0003AFC5
		internal virtual Label lblPaletteId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x060007DE RID: 2014 RVA: 0x0003CDCE File Offset: 0x0003AFCE
		// (set) Token: 0x060007DF RID: 2015 RVA: 0x0003CDD8 File Offset: 0x0003AFD8
		internal virtual Label lblFontId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x060007E0 RID: 2016 RVA: 0x0003CDE1 File Offset: 0x0003AFE1
		// (set) Token: 0x060007E1 RID: 2017 RVA: 0x0003CDEC File Offset: 0x0003AFEC
		internal virtual Button btnReload
		{
			[CompilerGenerated]
			get
			{
				return this._btnReload;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnReload_Click);
				Button button = this._btnReload;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnReload = value;
				button = this._btnReload;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x060007E2 RID: 2018 RVA: 0x0003CE2F File Offset: 0x0003B02F
		// (set) Token: 0x060007E3 RID: 2019 RVA: 0x0003CE3C File Offset: 0x0003B03C
		internal virtual Button btnCreateNewSpriteTable
		{
			[CompilerGenerated]
			get
			{
				return this._btnCreateNewSpriteTable;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnCreateNewSpriteTable_Click);
				Button button = this._btnCreateNewSpriteTable;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnCreateNewSpriteTable = value;
				button = this._btnCreateNewSpriteTable;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x060007E4 RID: 2020 RVA: 0x0003CE7F File Offset: 0x0003B07F
		// (set) Token: 0x060007E5 RID: 2021 RVA: 0x0003CE89 File Offset: 0x0003B089
		internal virtual NumericUpDown nudSpriteFramePage
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x060007E6 RID: 2022 RVA: 0x0003CE92 File Offset: 0x0003B092
		// (set) Token: 0x060007E7 RID: 2023 RVA: 0x0003CE9C File Offset: 0x0003B09C
		internal virtual Label lblSpriteFrameLimit
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x060007E8 RID: 2024 RVA: 0x0003CEA5 File Offset: 0x0003B0A5
		// (set) Token: 0x060007E9 RID: 2025 RVA: 0x0003CEB0 File Offset: 0x0003B0B0
		internal virtual Button btnSpriteFrameRight
		{
			[CompilerGenerated]
			get
			{
				return this._btnSpriteFrameRight;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSpriteFrameRight_Click);
				Button button = this._btnSpriteFrameRight;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSpriteFrameRight = value;
				button = this._btnSpriteFrameRight;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x060007EA RID: 2026 RVA: 0x0003CEF3 File Offset: 0x0003B0F3
		// (set) Token: 0x060007EB RID: 2027 RVA: 0x0003CF00 File Offset: 0x0003B100
		internal virtual Button btnSpriteFrameLeft
		{
			[CompilerGenerated]
			get
			{
				return this._btnSpriteFrameLeft;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSpriteFrameLeft_Click);
				Button button = this._btnSpriteFrameLeft;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSpriteFrameLeft = value;
				button = this._btnSpriteFrameLeft;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x060007EC RID: 2028 RVA: 0x0003CF43 File Offset: 0x0003B143
		// (set) Token: 0x060007ED RID: 2029 RVA: 0x0003CF50 File Offset: 0x0003B150
		internal virtual Button btnCreateNewPalette
		{
			[CompilerGenerated]
			get
			{
				return this._btnCreateNewPalette;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnCreateNewPalette_Click);
				Button button = this._btnCreateNewPalette;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnCreateNewPalette = value;
				button = this._btnCreateNewPalette;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x060007EE RID: 2030 RVA: 0x0003CF93 File Offset: 0x0003B193
		// (set) Token: 0x060007EF RID: 2031 RVA: 0x0003CF9D File Offset: 0x0003B19D
		internal virtual PictureBox picSpriteFramePreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x060007F0 RID: 2032 RVA: 0x0003CFA6 File Offset: 0x0003B1A6
		// (set) Token: 0x060007F1 RID: 2033 RVA: 0x0003CFB0 File Offset: 0x0003B1B0
		internal virtual Label lblCreatePaletteAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x060007F2 RID: 2034 RVA: 0x0003CFB9 File Offset: 0x0003B1B9
		// (set) Token: 0x060007F3 RID: 2035 RVA: 0x0003CFC3 File Offset: 0x0003B1C3
		internal virtual Label lblCreateNewSpriteSheetAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x060007F4 RID: 2036 RVA: 0x0003CFCC File Offset: 0x0003B1CC
		// (set) Token: 0x060007F5 RID: 2037 RVA: 0x0003CFD6 File Offset: 0x0003B1D6
		internal virtual GroupBox grpPaletteIdList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x060007F6 RID: 2038 RVA: 0x0003CFDF File Offset: 0x0003B1DF
		// (set) Token: 0x060007F7 RID: 2039 RVA: 0x0003CFE9 File Offset: 0x0003B1E9
		internal virtual NumericUpDown nudUnUsedValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x060007F8 RID: 2040 RVA: 0x0003CFF2 File Offset: 0x0003B1F2
		// (set) Token: 0x060007F9 RID: 2041 RVA: 0x0003CFFC File Offset: 0x0003B1FC
		internal virtual GroupBox grpCreateNewPaletteId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x060007FA RID: 2042 RVA: 0x0003D005 File Offset: 0x0003B205
		// (set) Token: 0x060007FB RID: 2043 RVA: 0x0003D00F File Offset: 0x0003B20F
		internal virtual TextBox txtCreatePaletteAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x060007FC RID: 2044 RVA: 0x0003D018 File Offset: 0x0003B218
		// (set) Token: 0x060007FD RID: 2045 RVA: 0x0003D022 File Offset: 0x0003B222
		internal virtual TextBox txtPaletteIdAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x060007FE RID: 2046 RVA: 0x0003D02B File Offset: 0x0003B22B
		// (set) Token: 0x060007FF RID: 2047 RVA: 0x0003D035 File Offset: 0x0003B235
		internal virtual Label lblCreatePaletteId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000800 RID: 2048 RVA: 0x0003D03E File Offset: 0x0003B23E
		// (set) Token: 0x06000801 RID: 2049 RVA: 0x0003D048 File Offset: 0x0003B248
		internal virtual GroupBox grpCreateNewSpriteTable
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000802 RID: 2050 RVA: 0x0003D051 File Offset: 0x0003B251
		// (set) Token: 0x06000803 RID: 2051 RVA: 0x0003D05C File Offset: 0x0003B25C
		internal virtual Button btnSpriteSheetImport
		{
			[CompilerGenerated]
			get
			{
				return this._btnSpriteSheetImport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSpriteSheetImport_Click);
				Button button = this._btnSpriteSheetImport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSpriteSheetImport = value;
				button = this._btnSpriteSheetImport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000804 RID: 2052 RVA: 0x0003D09F File Offset: 0x0003B29F
		// (set) Token: 0x06000805 RID: 2053 RVA: 0x0003D0A9 File Offset: 0x0003B2A9
		internal virtual NumericUpDown nudSpriteFrameLimit
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000806 RID: 2054 RVA: 0x0003D0B2 File Offset: 0x0003B2B2
		// (set) Token: 0x06000807 RID: 2055 RVA: 0x0003D0BC File Offset: 0x0003B2BC
		internal virtual Label lblDataSizeB
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000808 RID: 2056 RVA: 0x0003D0C5 File Offset: 0x0003B2C5
		// (set) Token: 0x06000809 RID: 2057 RVA: 0x0003D0CF File Offset: 0x0003B2CF
		internal virtual ComboBox cmbDataSizeB
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x0600080A RID: 2058 RVA: 0x0003D0D8 File Offset: 0x0003B2D8
		// (set) Token: 0x0600080B RID: 2059 RVA: 0x0003D0E2 File Offset: 0x0003B2E2
		internal virtual PictureBox picPaletteIdPreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x0600080C RID: 2060 RVA: 0x0003D0EB File Offset: 0x0003B2EB
		// (set) Token: 0x0600080D RID: 2061 RVA: 0x0003D0F5 File Offset: 0x0003B2F5
		internal virtual ComboBox cmbDataPaletteId2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x0600080E RID: 2062 RVA: 0x0003D0FE File Offset: 0x0003B2FE
		// (set) Token: 0x0600080F RID: 2063 RVA: 0x0003D108 File Offset: 0x0003B308
		internal virtual ComboBox cmbDataPaletteId1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06000810 RID: 2064 RVA: 0x0003D111 File Offset: 0x0003B311
		// (set) Token: 0x06000811 RID: 2065 RVA: 0x0003D11C File Offset: 0x0003B31C
		internal virtual Button btnCreateNewData
		{
			[CompilerGenerated]
			get
			{
				return this._btnCreateNewData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnCreateNewData_Click);
				Button button = this._btnCreateNewData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnCreateNewData = value;
				button = this._btnCreateNewData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06000812 RID: 2066 RVA: 0x0003D15F File Offset: 0x0003B35F
		// (set) Token: 0x06000813 RID: 2067 RVA: 0x0003D169 File Offset: 0x0003B369
		internal virtual Label lblCreateNewData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x0003D172 File Offset: 0x0003B372
		// (set) Token: 0x06000815 RID: 2069 RVA: 0x0003D17C File Offset: 0x0003B37C
		internal virtual TextBox txtCreateNewData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x0003D185 File Offset: 0x0003B385
		// (set) Token: 0x06000817 RID: 2071 RVA: 0x0003D190 File Offset: 0x0003B390
		internal virtual ComboBox cmbFontId
		{
			[CompilerGenerated]
			get
			{
				return this._cmbFontId;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbFontId_SelectedIndexChanged);
				ComboBox comboBox = this._cmbFontId;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbFontId = value;
				comboBox = this._cmbFontId;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x06000818 RID: 2072 RVA: 0x0003D1D4 File Offset: 0x0003B3D4
		private void OverWorldEditor_Load(object sender, EventArgs e)
		{
			this.txtDataTableAddress.Text = this.OVERWORLD_DATA_TABLE_OFFSET.ToString("X8");
			this.txtPaletteTableAddress.Text = this.OVERWORLD_PALETTE_TABLE_OFFSET.ToString("X8");
			this.txtFontTableAddress.Text = this.OVERWORLD_FONT_TABLE_OFFSET.ToString("X8");
			this.nudDataMaxCount.Value = new decimal((int)this.romData[this.OVERWORLD_DATA_MAX_COUNT_OFFSET]);
			this.InitializeComboBoxes();
			this.LoadPaletteTable();
			this.LoadDataInfoList();
			this.LoadFrameLimits();
			this.picSpriteFramePreview.SizeMode = PictureBoxSizeMode.Zoom;
			this.picSpriteFramePreview.BackColor = Color.White;
			this.lstOverWorldDataList.SelectedIndex = 0;
			this.SetChangeFlag(false);
		}

		// Token: 0x06000819 RID: 2073 RVA: 0x0003D2AC File Offset: 0x0003B4AC
		private void InitializeComboBoxes()
		{
			this.cmbDataSizeA.Items.Clear();
			this.cmbDataSizeB.Items.Clear();
			{
				foreach (string text in this.dataSizeMapping.Keys)
				{
					int item = this.dataSizeMapping[text].Item3;
					string text2 = string.Format("{0} (0x{1:X})", text, item);
					this.cmbDataSizeA.Items.Add(text2);
					this.cmbDataSizeB.Items.Add(text2);
				}
			}
			this.cmbDataFootPrint.Items.Clear();
			this.cmbDataFootPrint.Items.AddRange(this.dataFootPrintMapping.Keys.ToArray<string>());
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x0003D3A0 File Offset: 0x0003B5A0
		private void LoadPaletteTable()
		{
			this.paletteTableDataList.Clear();
			this.cmbPaletteId.Items.Clear();
			this.cmbDataPaletteId1.Items.Clear();
			this.cmbDataPaletteId2.Items.Clear();
			int num = Convert.ToInt32(this.txtPaletteTableAddress.Text, 16);
			int num2 = 0;
			checked
			{
				for (;;)
				{
					int num3 = num + num2 * 8;
					bool flag = true;
					int num4 = 0;
					do
					{
						bool flag2 = this.romData[num3 + num4] > 0;
						if (flag2)
						{
							goto Block_1;
						}
						num4++;
					}
					while (num4 <= 7);
					IL_0089:
					bool flag3 = flag;
					if (flag3)
					{
						break;
					}
					uint num5 = BitConverter.ToUInt32(this.romData, num3);
					ushort num6 = BitConverter.ToUInt16(this.romData, num3 + 4);
					this.paletteTableDataList.Add(new OverWorldEditor.PaletteTableData
					{
						Address = num5 - 134217728U,
						PaletteID = num6
					});
					num2++;
					continue;
					Block_1:
					flag = false;
					goto IL_0089;
				}
				bool flag4 = !this.paletteTableDataList.Any((OverWorldEditor.PaletteTableData x) => x.PaletteID == 4607);
				if (flag4)
				{
					this.paletteTableDataList.Add(new OverWorldEditor.PaletteTableData
					{
						Address = 0U,
						PaletteID = 4607
					});
				}
				this.paletteTableDataList = this.paletteTableDataList.OrderBy((OverWorldEditor.PaletteTableData x) => x.PaletteID).ToList<OverWorldEditor.PaletteTableData>();
				{
					foreach (OverWorldEditor.PaletteTableData paletteTableData in this.paletteTableDataList)
					{
						this.cmbPaletteId.Items.Add(string.Format("{0:X4}", paletteTableData.PaletteID));
						this.cmbDataPaletteId1.Items.Add(string.Format("{0:X4}", paletteTableData.PaletteID));
						this.cmbDataPaletteId2.Items.Add(string.Format("{0:X4}", paletteTableData.PaletteID));
					}
				}
				this.cmbPaletteId.SelectedIndex = 0;
				this.RestoreComboSelectionFromText(this.cmbDataPaletteId1);
				this.RestoreComboSelectionFromText(this.cmbDataPaletteId2);
			}
		}

		// Token: 0x0600081B RID: 2075 RVA: 0x0003D614 File Offset: 0x0003B814
		private void RestoreComboSelectionFromText(ComboBox combo)
		{
			int num = combo.FindStringExact(combo.Text);
			combo.SelectedIndex = num;
		}

		// Token: 0x0600081C RID: 2076 RVA: 0x0003D638 File Offset: 0x0003B838
		private void LoadDataInfoList()
		{
			this.lstOverWorldDataList.BeginUpdate();
			this.lstOverWorldDataList.Items.Clear();
			this.overworldDataList.Clear();
			int num = Convert.ToInt32(this.txtDataTableAddress.Text, 16);
			checked
			{
				int num2 = Convert.ToInt32(this.nudDataMaxCount.Value) + 1;
				int num3 = num2 - 1;
				for (int i = 0; i <= num3; i++)
				{
					this.lstOverWorldDataList.Items.Add(string.Format("スロット {0:000}", i));
					int num4 = num + i * 4;
					uint num5 = BitConverter.ToUInt32(this.romData, num4);
					bool flag = unchecked((ulong)num5) == 0UL;
					int num6 = 0;
					if (flag)
					{
						num6 = -1;
					}
					else
					{
						num6 = (int)(num5 - 134217728U);
					}
					this.overworldDataList.Add(new OverWorldEditor.OverWorldData
					{
						Index = i,
						DataOffset = num6
					});
				}
				this.lstOverWorldDataList.EndUpdate();
			}
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x0003D730 File Offset: 0x0003B930
		private void LoadFrameLimits()
		{
			this.frameLimits.Clear();
			string text = Path.Combine(Application.StartupPath, "ini", "OverWorldSpriteFrameLimit.ini");
			bool flag = File.Exists(text);
			if (flag)
			{
				string[] array = File.ReadAllLines(text);
				foreach (string text2 in array)
				{
					string[] array3 = text2.Split(new char[] { '=' });
					bool flag2 = array3.Length == 2;
					if (flag2)
					{
						int num = 0;
						int num2 = 0;
						bool flag3 = int.TryParse(array3[0].Trim(), out num) && int.TryParse(array3[1].Trim(), out num2);
						if (flag3)
						{
							this.frameLimits[num] = num2;
						}
					}
				}
			}
		}

		// Token: 0x0600081E RID: 2078 RVA: 0x0003D7F8 File Offset: 0x0003B9F8
		private void LoadSpriteFrameTable()
		{
			this.spriteFrameDataList.Clear();
			OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
			bool flag = (ulong)overWorldData.SpriteTableAddress == 0UL;
			checked
			{
				if (flag)
				{
					this.ClearSpritePreview();
					this.grpSpritePreview.Enabled = false;
				}
				else
				{
					bool flag2 = !this.grpSpritePreview.Enabled;
					if (flag2)
					{
						this.grpSpritePreview.Enabled = true;
					}
					bool flag3 = this.temporarySpriteTableData != null;
					int num = 0;
					if (flag3)
					{
						num = this.temporarySpriteTableData.FrameCount;
					}
					else
					{
						num = this.frameLimits[overWorldData.Index];
					}
					int num2 = num - 1;
					for (int i = 0; i <= num2; i++)
					{
						OverWorldEditor.SpriteFrameData spriteFrameData = new OverWorldEditor.SpriteFrameData();
						bool flag4 = this.temporarySpriteTableData != null;
						if (flag4)
						{
							spriteFrameData.ImageAddress = (uint)(unchecked((ulong)this.temporarySpriteTableData.Address) + (ulong)(unchecked((long)(checked(this.temporarySpriteTableData.FrameCount * 8)))) + (ulong)(unchecked((long)(checked(i * this.temporarySpriteTableData.ImageSize)))));
							spriteFrameData.ImageSize = (ushort)this.temporarySpriteTableData.ImageSize;
						}
						else
						{
							int num3 = (int)(unchecked((ulong)overWorldData.SpriteTableAddress) + (ulong)(unchecked((long)(checked(i * 8)))));
							spriteFrameData.ImageAddress = BitConverter.ToUInt32(this.romData, num3) - 134217728U;
							spriteFrameData.ImageSize = BitConverter.ToUInt16(this.romData, num3 + 4);
						}
						spriteFrameData.Unknown = 0;
						spriteFrameData.FrameIndex = i;
						spriteFrameData.FrameLimit = num;
						this.spriteFrameDataList.Add(spriteFrameData);
					}
					this.UpdateFrameNavigationUI();
					this.LoadCurrentFrame();
				}
			}
		}

		// Token: 0x0600081F RID: 2079 RVA: 0x0003D99C File Offset: 0x0003BB9C
		private void UpdateFrameNavigationUI()
		{
			OverWorldEditor.SpriteFrameData currentFrameData = this.GetCurrentFrameData();
			checked
			{
				this.nudSpriteFramePage.Maximum = new decimal(currentFrameData.FrameLimit - 1);
				this.nudSpriteFramePage.Value = new decimal(currentFrameData.FrameIndex);
				this.btnSpriteFrameLeft.Enabled = currentFrameData.FrameIndex > 0;
				this.btnSpriteFrameRight.Enabled = currentFrameData.FrameIndex < currentFrameData.FrameLimit - 1;
				this.txtSpriteFrameAddress.Text = currentFrameData.ImageAddress.ToString("X8");
			}
		}

		// Token: 0x06000820 RID: 2080 RVA: 0x0003DA34 File Offset: 0x0003BC34
		private OverWorldEditor.SpriteFrameData GetCurrentFrameData()
		{
			int frameIdx = Convert.ToInt32(this.nudSpriteFramePage.Value);
			return this.spriteFrameDataList.FirstOrDefault((OverWorldEditor.SpriteFrameData f) => f.FrameIndex == frameIdx);
		}

		// Token: 0x06000821 RID: 2081 RVA: 0x0003DA7C File Offset: 0x0003BC7C
		private void LoadCurrentFrame()
		{
			OverWorldEditor.SpriteFrameData currentFrameData = this.GetCurrentFrameData();
			OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
			byte[] paletteDataByID = this.GetPaletteDataByID(overWorldData.PaletteId1);
			Color[] array = ImageProcessor.LoadPalette(paletteDataByID, false);
			bool flag = this.temporarySpriteTableData != null && this.temporarySpriteTableData.FrameData.ContainsKey(currentFrameData.FrameIndex);
			byte[] array2;
			if (flag)
			{
				array2 = this.temporarySpriteTableData.FrameData[currentFrameData.FrameIndex];
			}
			else
			{
				bool flag2 = this.temporaryFrameData.ContainsKey(currentFrameData.FrameIndex);
				if (flag2)
				{
					array2 = this.temporaryFrameData[currentFrameData.FrameIndex];
				}
				else
				{
					int num = 0;
					checked
					{
						num = (int)Math.Min((long)(unchecked((ulong)currentFrameData.ImageSize)), unchecked((long)this.romData.Length) - (long)(unchecked((ulong)currentFrameData.ImageAddress)));
						array2 = new byte[num - 1 + 1];
					}
					Array.Copy(this.romData, (long)((ulong)currentFrameData.ImageAddress), array2, 0L, (long)num);
				}
			}
			int width = (int)overWorldData.Width;
			int height = (int)overWorldData.Height;
			Bitmap bitmap = ImageProcessor.LoadSprite(ref array2, array, width, height, true);
			checked
			{
				int num2 = bitmap.Width * 2;
				int num3 = bitmap.Height * 2;
				Bitmap bitmap2 = new Bitmap(num2, num3);
				using (Graphics graphics = Graphics.FromImage(bitmap2))
				{
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					graphics.PixelOffsetMode = PixelOffsetMode.Half;
					graphics.DrawImage(bitmap, 0, 0, num2, num3);
				}
				Bitmap bitmap3 = new Bitmap(this.picSpriteFramePreview.Width, this.picSpriteFramePreview.Height);
				using (Graphics graphics2 = Graphics.FromImage(bitmap3))
				{
					graphics2.Clear(Color.White);
					int num4 = (bitmap3.Width - bitmap2.Width) / 2;
					int num5 = (bitmap3.Height - bitmap2.Height) / 2;
					graphics2.DrawImage(bitmap2, num4, num5);
				}
				this.picSpriteFramePreview.Image = bitmap3;
				this.picSpriteFramePreview.SizeMode = PictureBoxSizeMode.Zoom;
			}
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x0003DC9C File Offset: 0x0003BE9C
		public byte[] GetPaletteDataByID(ushort paletteID)
		{
			OverWorldEditor.PaletteTableData paletteTableData = this.paletteTableDataList.FirstOrDefault((OverWorldEditor.PaletteTableData x) => x.PaletteID == paletteID);
			bool flag = paletteTableData != null;
			byte[] array2;
			if (flag)
			{
				byte[] array = new byte[32];
				Array.Copy(this.romData, (long)((ulong)paletteTableData.Address), array, 0L, 32L);
				array2 = array;
			}
			else
			{
				array2 = null;
			}
			return array2;
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x0003DD08 File Offset: 0x0003BF08
		private void lstOverWorldDataList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstOverWorldDataList.SelectedIndex == this.currentIndex;
			if (!flag)
			{
				bool flag2 = this.currentIndex >= 0 && this.overworldDataList[this.currentIndex].DataOffset != -1 && this.hasUnsavedChanges;
				if (flag2)
				{
					DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Cancel)
					{
						this.lstOverWorldDataList.SelectedIndex = this.currentIndex;
						return;
					}
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult == DialogResult.No)
						{
							this.ReloadData(this.currentIndex);
							this.SetChangeFlag(false);
						}
					}
					else
					{
						this.SaveData(this.currentIndex);
						this.SetChangeFlag(false);
					}
				}
				else
				{
					bool flag3 = this.hasUnsavedChanges;
					if (flag3)
					{
						this.ReloadData(this.currentIndex);
						this.SetChangeFlag(false);
					}
				}
				this.txtCreateNewData.Text = "";
				this.txtCreateNewSpriteSheetAddress.Text = "";
				this.nudSpriteFrameLimit.Value = this.nudSpriteFrameLimit.Minimum;
				this.cmbDataSizeB.SelectedIndex = 0;
				this.txtCreatePaletteId.Text = "";
				this.txtCreatePaletteAddress.Text = "";
				int selectedIndex = this.lstOverWorldDataList.SelectedIndex;
				OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[selectedIndex];
				this.currentIndex = selectedIndex;
				this.nudSpriteFramePage.Value = 0m;
				bool flag4 = overWorldData.DataOffset == -1;
				if (flag4)
				{
					this.ClearDataControls();
					this.ModifyDataControls(false);
					this.temporaryOverWorldDataAddress = -1;
					this.temporaryFrameData.Clear();
					this.temporarySpriteTableData = null;
					this.SetChangeFlag(false);
				}
				else
				{
					this.ModifyDataControls(true);
					this.LoadAndDisplayData(overWorldData);
					this.SetChangeFlag(false);
				}
			}
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x0003DEF0 File Offset: 0x0003C0F0
		private void ReloadData(int index)
		{
			int num = Convert.ToInt32(this.txtDataTableAddress.Text, 16);
			int num2 = checked(num + index * 4);
			uint num3 = BitConverter.ToUInt32(this.romData, num2);
			bool flag = (ulong)num3 == 0UL;
			int num4 = 0;
			if (flag)
			{
				num4 = -1;
			}
			else
			{
				num4 = checked((int)(num3 - 134217728U));
			}
			this.overworldDataList[index] = new OverWorldEditor.OverWorldData
			{
				Index = index,
				DataOffset = num4
			};
			this.temporaryFrameData.Clear();
			this.temporarySpriteTableData = null;
			this.LoadFrameLimits();
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x0003DF7C File Offset: 0x0003C17C
		private void SaveData(int index)
		{
			OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[index];
			bool flag = overWorldData.DataOffset == -1 && this.temporaryOverWorldDataAddress != -1;
			bool flag4;
			checked
			{
				if (flag)
				{
					int num = Convert.ToInt32(this.txtDataTableAddress.Text, 16);
					int num2 = num + index * 4;
					uint num3 = (uint)(unchecked((long)this.temporaryOverWorldDataAddress) + 134217728L);
					BitConverter.GetBytes(num3).CopyTo(this.romData, num2);
					overWorldData.DataOffset = this.temporaryOverWorldDataAddress;
					this.temporaryOverWorldDataAddress = -1;
				}
				else
				{
					bool flag2 = this.temporaryOverWorldDataAddress != -1;
					if (flag2)
					{
						overWorldData.DataOffset = this.temporaryOverWorldDataAddress;
						int num4 = Convert.ToInt32(this.txtDataTableAddress.Text, 16);
						int num5 = num4 + index * 4;
						uint num6 = (uint)(unchecked((long)this.temporaryOverWorldDataAddress) + 134217728L);
						BitConverter.GetBytes(num6).CopyTo(this.romData, num5);
						this.temporaryOverWorldDataAddress = -1;
					}
				}
				overWorldData.PaletteId1 = Convert.ToUInt16(this.cmbDataPaletteId1.SelectedItem.ToString(), 16);
				overWorldData.PaletteId2 = Convert.ToUInt16(this.cmbDataPaletteId2.SelectedItem.ToString(), 16);
				overWorldData.DataLength = Convert.ToUInt16(this.nudDataLength.Value);
				string text = this.cmbDataSizeA.SelectedItem.ToString().Split(new char[] { ' ' })[0];
				ValueTuple<int, int, int> valueTuple = this.dataSizeMapping[text];
				overWorldData.Width = (ushort)valueTuple.Item1;
				overWorldData.Height = (ushort)valueTuple.Item2;
				overWorldData.PaletteSlot = Convert.ToInt32(this.nudDataPaletteSlot.Value);
				overWorldData.UnknownValue1 = this.chkDataUnknownValue1.Checked;
				overWorldData.UnknownValue2 = this.chkDataUnknownValue2.Checked;
				overWorldData.UnknownValue3 = this.chkDataUnknownValue3.Checked;
				string text2 = this.cmbDataFootPrint.SelectedItem.ToString();
				overWorldData.FootPrint = this.dataFootPrintMapping[text2];
				overWorldData.UnUsedValue = Convert.ToUInt16(this.nudUnUsedValue.Value);
				overWorldData.LoadAddress = Convert.ToUInt32(this.txtDataLoadAddress.Text, 16);
				overWorldData.SizeAddress = Convert.ToUInt32(this.txtDataSizeAddress.Text, 16);
				overWorldData.AnimationAddress = Convert.ToUInt32(this.txtDataAnimationAddress.Text, 16);
				overWorldData.SpriteTableAddress = Convert.ToUInt32(this.txtDataSpriteTable.Text, 16);
				overWorldData.MemoryAddress = Convert.ToUInt32(this.txtDataMemoryAddress.Text, 16);
				BitConverter.GetBytes(overWorldData.Padding).CopyTo(this.romData, overWorldData.DataOffset + 0);
				BitConverter.GetBytes(overWorldData.PaletteId1).CopyTo(this.romData, overWorldData.DataOffset + 2);
				BitConverter.GetBytes(overWorldData.PaletteId2).CopyTo(this.romData, overWorldData.DataOffset + 4);
				BitConverter.GetBytes(overWorldData.DataLength).CopyTo(this.romData, overWorldData.DataOffset + 6);
				BitConverter.GetBytes(overWorldData.Width).CopyTo(this.romData, overWorldData.DataOffset + 8);
				BitConverter.GetBytes(overWorldData.Height).CopyTo(this.romData, overWorldData.DataOffset + 10);
				byte b = (byte)(overWorldData.PaletteSlot & 15);
				bool unknownValue = overWorldData.UnknownValue1;
				if (unknownValue)
				{
					b |= 16;
				}
				bool unknownValue2 = overWorldData.UnknownValue2;
				if (unknownValue2)
				{
					b |= 64;
				}
				bool unknownValue3 = overWorldData.UnknownValue3;
				if (unknownValue3)
				{
					b |= 128;
				}
				this.romData[overWorldData.DataOffset + 12] = b;
				this.romData[overWorldData.DataOffset + 13] = overWorldData.FootPrint;
				BitConverter.GetBytes(overWorldData.UnUsedValue).CopyTo(this.romData, overWorldData.DataOffset + 14);
				BitConverter.GetBytes(overWorldData.LoadAddress + 134217728U).CopyTo(this.romData, overWorldData.DataOffset + 16);
				BitConverter.GetBytes(overWorldData.SizeAddress + 134217728U).CopyTo(this.romData, overWorldData.DataOffset + 20);
				BitConverter.GetBytes(overWorldData.AnimationAddress + 134217728U).CopyTo(this.romData, overWorldData.DataOffset + 24);
				BitConverter.GetBytes(overWorldData.SpriteTableAddress + 134217728U).CopyTo(this.romData, overWorldData.DataOffset + 28);
				BitConverter.GetBytes(overWorldData.MemoryAddress + 134217728U).CopyTo(this.romData, overWorldData.DataOffset + 32);
				bool flag3 = this.temporarySpriteTableData != null;
				if (flag3)
				{
					this.UpdateFrameLimitFile(index, this.temporarySpriteTableData.FrameCount);
					int num7 = (int)this.temporarySpriteTableData.Address;
					int num8 = num7 + this.temporarySpriteTableData.FrameCount * 8;
					int num9 = this.temporarySpriteTableData.FrameCount - 1;
					for (int i = 0; i <= num9; i++)
					{
						int num10 = num7 + i * 8;
						uint num11 = (uint)(num8 + i * this.temporarySpriteTableData.ImageSize);
						byte[] bytes = BitConverter.GetBytes(num11 + 134217728U);
						Array.Copy(bytes, 0, this.romData, num10, 4);
						byte[] bytes2 = BitConverter.GetBytes((ushort)this.temporarySpriteTableData.ImageSize);
						Array.Copy(bytes2, 0, this.romData, num10 + 4, 2);
						this.romData[num10 + 6] = 0;
						this.romData[num10 + 7] = 0;
					}
					int num12 = this.temporarySpriteTableData.FrameCount - 1;
					for (int j = 0; j <= num12; j++)
					{
						int num13 = num8 + j * this.temporarySpriteTableData.ImageSize;
						byte[] array = this.temporarySpriteTableData.FrameData[j];
						Array.Copy(array, 0, this.romData, num13, array.Length);
					}
					this.temporarySpriteTableData = null;
				}
				flag4 = this.temporaryFrameData.Count > 0;
			}
			if (flag4)
			{
				{
					foreach (KeyValuePair<int, byte[]> keyValuePair in this.temporaryFrameData)
					{
						int frameIndex = keyValuePair.Key;
						byte[] value = keyValuePair.Value;
						OverWorldEditor.SpriteFrameData spriteFrameData = this.spriteFrameDataList.FirstOrDefault((OverWorldEditor.SpriteFrameData f) => f.FrameIndex == frameIndex);
						bool flag5 = spriteFrameData != null;
						if (flag5)
						{
							Array.Copy(value, 0L, this.romData, (long)((ulong)spriteFrameData.ImageAddress), (long)value.Length);
						}
					}
				}
			}
			int num14 = Convert.ToInt32(this.txtFontTableAddress.Text, 16);
			int num15 = overWorldData.Index / 2;
			checked
			{
				byte b2 = this.romData[num14 + num15];
				bool flag6 = overWorldData.Index % 2 == 0;
				if (flag6)
				{
					b2 = (byte)((int)(b2 & 240) | (overWorldData.FontId & 15));
				}
				else
				{
					b2 = (byte)((int)(b2 & 15) | ((overWorldData.FontId & 15) << 4));
				}
				this.romData[num14 + num15] = b2;
				MainForm.romData = this.romData;
			}
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x0003E6C0 File Offset: 0x0003C8C0
		private void UpdateFrameLimitFile(int index, int frameCount)
		{
			string text = Path.Combine(Application.StartupPath, "ini", "OverWorldSpriteFrameLimit.ini");
			List<string> list = new List<string>();
			list.AddRange(File.ReadAllLines(text));
			string text2 = string.Format("{0} = {1}", index, frameCount);
			int num = -1;
			checked
			{
				int num2 = list.Count - 1;
				for (int i = 0; i <= num2; i++)
				{
					bool flag = list[i].StartsWith(string.Format("{0} =", index));
					if (flag)
					{
						num = i;
						break;
					}
				}
				list[num] = text2;
				File.WriteAllLines(text, list);
				this.LoadFrameLimits();
			}
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x0003E76C File Offset: 0x0003C96C
		private void OverWorldEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Cancel)
				{
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult != DialogResult.No)
						{
						}
					}
					else
					{
						this.SaveData(this.currentIndex);
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x0003E7CC File Offset: 0x0003C9CC
		private void LoadAndDisplayData(OverWorldEditor.OverWorldData data)
		{
			bool flag = data.DataOffset == -1;
			if (flag)
			{
				this.ClearDataControls();
				this.ModifyDataControls(false);
			}
			else
			{
				byte b2;
				bool flag2;
				checked
				{
					data.Padding = BitConverter.ToUInt16(this.romData, data.DataOffset + 0);
					data.PaletteId1 = BitConverter.ToUInt16(this.romData, data.DataOffset + 2);
					data.PaletteId2 = BitConverter.ToUInt16(this.romData, data.DataOffset + 4);
					data.DataLength = BitConverter.ToUInt16(this.romData, data.DataOffset + 6);
					data.Width = BitConverter.ToUInt16(this.romData, data.DataOffset + 8);
					data.Height = BitConverter.ToUInt16(this.romData, data.DataOffset + 10);
					byte b = this.romData[data.DataOffset + 12];
					data.PaletteSlot = (int)(b & 15);
					data.UnknownValue1 = (b & 16) > 0;
					data.UnknownValue2 = (b & 64) > 0;
					data.UnknownValue3 = (b & 128) > 0;
					data.FootPrint = this.romData[data.DataOffset + 13];
					data.UnUsedValue = BitConverter.ToUInt16(this.romData, data.DataOffset + 14);
					data.LoadAddress = BitConverter.ToUInt32(this.romData, data.DataOffset + 16) - 134217728U;
					data.SizeAddress = BitConverter.ToUInt32(this.romData, data.DataOffset + 20) - 134217728U;
					data.AnimationAddress = BitConverter.ToUInt32(this.romData, data.DataOffset + 24) - 134217728U;
					data.SpriteTableAddress = BitConverter.ToUInt32(this.romData, data.DataOffset + 28) - 134217728U;
					data.MemoryAddress = BitConverter.ToUInt32(this.romData, data.DataOffset + 32) - 134217728U;
					this.txtDataPadding.Text = data.Padding.ToString("X4");
					this.SetComboBoxValue(this.cmbDataPaletteId1, data.PaletteId1.ToString("X4"));
					this.SetComboBoxValue(this.cmbDataPaletteId2, data.PaletteId2.ToString("X4"));
					this.nudDataLength.Value = new decimal((int)data.DataLength);
					this.UpdateSizeComboBox(data.Width, data.Height);
					this.nudDataPaletteSlot.Value = new decimal(data.PaletteSlot);
					this.chkDataUnknownValue1.Checked = data.UnknownValue1;
					this.chkDataUnknownValue2.Checked = data.UnknownValue2;
					this.chkDataUnknownValue3.Checked = data.UnknownValue3;
					this.UpdateFootPrintComboBox(data.FootPrint);
					this.nudUnUsedValue.Value = new decimal((int)data.UnUsedValue);
					this.txtDataLoadAddress.Text = data.LoadAddress.ToString("X8");
					this.txtDataSizeAddress.Text = data.SizeAddress.ToString("X8");
					this.txtDataAnimationAddress.Text = data.AnimationAddress.ToString("X8");
					this.txtDataSpriteTable.Text = data.SpriteTableAddress.ToString("X8");
					this.txtDataMemoryAddress.Text = data.MemoryAddress.ToString("X8");
					int num = Convert.ToInt32(this.txtFontTableAddress.Text, 16);
					int num2 = data.Index / 2;
					b2 = this.romData[num + num2];
					flag2 = data.Index % 2 == 0;
				}
				if (flag2)
				{
					data.FontId = (int)(b2 & 15);
				}
				else
				{
					data.FontId = (int)((byte)((uint)b2 >> 4) & 15);
				}
				this.cmbFontId.SelectedIndex = data.FontId;
				this.LoadSpriteFrameTable();
				this.ModifyDataControls(true);
			}
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x0003EBD0 File Offset: 0x0003CDD0
		private void ClearDataControls()
		{
			this.txtDataPadding.Text = "0000";
			this.cmbDataPaletteId1.SelectedIndex = -1;
			this.cmbDataPaletteId1.Text = "0000";
			this.cmbDataPaletteId2.SelectedIndex = -1;
			this.cmbDataPaletteId2.Text = "0000";
			this.nudDataLength.Value = 0m;
			this.cmbDataSizeA.SelectedIndex = -1;
			this.nudDataPaletteSlot.Value = 0m;
			this.chkDataUnknownValue1.Checked = false;
			this.chkDataUnknownValue2.Checked = false;
			this.chkDataUnknownValue3.Checked = false;
			this.cmbDataFootPrint.SelectedIndex = -1;
			this.nudUnUsedValue.Value = 0m;
			this.txtDataLoadAddress.Text = "00000000";
			this.txtDataSizeAddress.Text = "00000000";
			this.txtDataAnimationAddress.Text = "00000000";
			this.txtDataSpriteTable.Text = "00000000";
			this.txtDataMemoryAddress.Text = "00000000";
			this.cmbFontId.SelectedIndex = 0;
			this.ClearSpritePreview();
		}

		// Token: 0x0600082A RID: 2090 RVA: 0x0003ED08 File Offset: 0x0003CF08
		private void ClearSpritePreview()
		{
			this.picSpriteFramePreview.Image = null;
			this.txtSpriteFrameAddress.Text = "00000000";
			this.nudSpriteFramePage.Value = 0m;
			this.nudSpriteFramePage.Enabled = false;
			this.btnSpriteFrameLeft.Enabled = false;
			this.btnSpriteFrameRight.Enabled = false;
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x0003ED6C File Offset: 0x0003CF6C
		private void ModifyDataControls(bool flag)
		{
			this.btnReload.Enabled = flag;
			this.grpFont.Enabled = flag;
			this.grpCreateNewSpriteTable.Enabled = flag;
			try
			{
				foreach (object obj in this.grpData.Controls)
				{
					Control control = (Control)obj;
					bool flag2 = control == this.btnCreateNewData || control == this.txtCreateNewData || control == this.lblCreateNewData;
					if (flag2)
					{
						control.Enabled = true;
					}
					else
					{
						bool flag3 = control == this.cmbDataSizeA;
						if (flag3)
						{
							control.Enabled = false;
						}
						else
						{
							control.Enabled = flag;
						}
					}
				}
			}
			finally
			{
			}
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x0003EE44 File Offset: 0x0003D044
		private void SetComboBoxValue(ComboBox comboBox, string value)
		{
			int num = comboBox.FindStringExact(value);
			bool flag = num >= 0;
			if (flag)
			{
				comboBox.SelectedIndex = num;
			}
			else
			{
				comboBox.SelectedIndex = -1;
				comboBox.Text = value;
			}
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x0003EE84 File Offset: 0x0003D084
		private void UpdateSizeComboBox(ushort width, ushort height)
		{
			checked
			{
				int num = this.cmbDataSizeA.Items.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					string text = this.cmbDataSizeA.Items[i].ToString().Split(new char[] { ' ' })[0];
					bool flag = this.dataSizeMapping.ContainsKey(text) && this.dataSizeMapping[text].Item1 == (int)width && this.dataSizeMapping[text].Item2 == (int)height;
					if (flag)
					{
						this.cmbDataSizeA.SelectedIndex = i;
						break;
					}
				}
				this.cmbDataSizeB.SelectedIndex = 0;
			}
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x0003EF40 File Offset: 0x0003D140
		private void UpdateFootPrintComboBox(byte footPrintValue)
		{
			this.cmbDataFootPrint.SelectedItem = this.dataFootPrintMapping.FirstOrDefault((KeyValuePair<string, byte> x) => x.Value == footPrintValue).Key;
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x0003EF88 File Offset: 0x0003D188
		private void cmbPaletteId_SelectedIndexChanged(object sender, EventArgs e)
		{
			OverWorldEditor.PaletteTableData paletteTableData = this.paletteTableDataList[this.cmbPaletteId.SelectedIndex];
			this.txtPaletteIdAddress.Text = paletteTableData.Address.ToString("X8");
			this.DisplayPalettePreview(paletteTableData.Address);
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x0003EFDC File Offset: 0x0003D1DC
		private void DisplayPalettePreview(uint paletteAddress)
		{
			bool flag = (ulong)paletteAddress == 0UL;
			if (flag)
			{
				this.picPaletteIdPreview.Image = null;
			}
			else
			{
				byte[] array = new byte[32];
				Array.Copy(this.romData, (long)((ulong)paletteAddress), array, 0L, 32L);
				Color[] array2 = ImageProcessor.LoadPalette(array, false);
				Bitmap bitmap = new Bitmap(64, 16);
				checked
				{
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						int num = 0;
						do
						{
							int num2 = num % 8 * 8;
							int num3 = num / 8 * 8;
							using (SolidBrush solidBrush = new SolidBrush(array2[num]))
							{
								graphics.FillRectangle(solidBrush, num2, num3, 8, 8);
							}
							num++;
						}
						while (num <= 15);
					}
					this.picPaletteIdPreview.Image = bitmap;
					this.picPaletteIdPreview.SizeMode = PictureBoxSizeMode.StretchImage;
				}
			}
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x0003F0D4 File Offset: 0x0003D2D4
		private void btnSpriteFrameLeft_Click(object sender, EventArgs e)
		{
			bool flag = decimal.Compare(this.nudSpriteFramePage.Value, 0m) > 0;
			if (flag)
			{
				NumericUpDown nudSpriteFramePage;
				(nudSpriteFramePage = this.nudSpriteFramePage).Value = decimal.Subtract(nudSpriteFramePage.Value, 1m);
				this.UpdateFrameNavigationUI();
				this.LoadCurrentFrame();
			}
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x0003F12C File Offset: 0x0003D32C
		private void btnSpriteFrameRight_Click(object sender, EventArgs e)
		{
			OverWorldEditor.SpriteFrameData currentFrameData = this.GetCurrentFrameData();
			bool flag = currentFrameData != null && decimal.Compare(this.nudSpriteFramePage.Value, new decimal(checked(currentFrameData.FrameLimit - 1))) < 0;
			if (flag)
			{
				NumericUpDown nudSpriteFramePage;
				(nudSpriteFramePage = this.nudSpriteFramePage).Value = decimal.Add(nudSpriteFramePage.Value, 1m);
				this.UpdateFrameNavigationUI();
				this.LoadCurrentFrame();
			}
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x0003F19C File Offset: 0x0003D39C
		private void btnReload_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstOverWorldDataList.SelectedIndex;
			OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[selectedIndex];
			bool flag = false;
			try
			{
				ushort num = Convert.ToUInt16(this.cmbDataPaletteId1.Text, 16);
				bool flag2 = overWorldData.PaletteId1 != num;
				if (flag2)
				{
					overWorldData.PaletteId1 = num;
					flag = true;
					this.LoadCurrentFrame();
				}
				ushort num2 = Convert.ToUInt16(this.cmbDataPaletteId2.Text, 16);
				bool flag3 = overWorldData.PaletteId2 != num2;
				if (flag3)
				{
					overWorldData.PaletteId2 = num2;
					flag = true;
				}
				ushort num3 = Convert.ToUInt16(this.nudDataLength.Value);
				bool flag4 = overWorldData.DataLength != num3;
				if (flag4)
				{
					overWorldData.DataLength = num3;
					flag = true;
				}
				int num4 = Convert.ToInt32(this.nudDataPaletteSlot.Value);
				bool flag5 = overWorldData.PaletteSlot != num4;
				if (flag5)
				{
					overWorldData.PaletteSlot = num4;
					flag = true;
				}
				bool flag6 = overWorldData.UnknownValue1 != this.chkDataUnknownValue1.Checked;
				if (flag6)
				{
					overWorldData.UnknownValue1 = this.chkDataUnknownValue1.Checked;
					flag = true;
				}
				bool flag7 = overWorldData.UnknownValue2 != this.chkDataUnknownValue2.Checked;
				if (flag7)
				{
					overWorldData.UnknownValue2 = this.chkDataUnknownValue2.Checked;
					flag = true;
				}
				bool flag8 = overWorldData.UnknownValue3 != this.chkDataUnknownValue3.Checked;
				if (flag8)
				{
					overWorldData.UnknownValue3 = this.chkDataUnknownValue3.Checked;
					flag = true;
				}
				bool flag9 = this.cmbDataFootPrint.SelectedIndex >= 0;
				if (flag9)
				{
					string text = this.cmbDataFootPrint.SelectedItem.ToString();
					bool flag10 = this.dataFootPrintMapping.ContainsKey(text);
					if (flag10)
					{
						byte b = this.dataFootPrintMapping[text];
						bool flag11 = overWorldData.FootPrint != b;
						if (flag11)
						{
							overWorldData.FootPrint = b;
							flag = true;
						}
					}
				}
				ushort num5 = Convert.ToUInt16(this.nudUnUsedValue.Value);
				bool flag12 = overWorldData.UnUsedValue != num5;
				if (flag12)
				{
					overWorldData.UnUsedValue = num5;
					flag = true;
				}
				uint num6 = Convert.ToUInt32(this.txtDataLoadAddress.Text, 16);
				bool flag13 = overWorldData.LoadAddress != num6;
				if (flag13)
				{
					overWorldData.LoadAddress = num6;
					flag = true;
				}
				uint num7 = Convert.ToUInt32(this.txtDataSizeAddress.Text, 16);
				bool flag14 = overWorldData.SizeAddress != num7;
				if (flag14)
				{
					overWorldData.SizeAddress = num7;
					flag = true;
				}
				uint num8 = Convert.ToUInt32(this.txtDataAnimationAddress.Text, 16);
				bool flag15 = overWorldData.AnimationAddress != num8;
				if (flag15)
				{
					overWorldData.AnimationAddress = num8;
					flag = true;
				}
				uint num9 = Convert.ToUInt32(this.txtDataMemoryAddress.Text, 16);
				bool flag16 = overWorldData.MemoryAddress != num9;
				if (flag16)
				{
					overWorldData.MemoryAddress = num9;
					flag = true;
				}
				bool flag17 = flag;
				if (flag17)
				{
					this.SetChangeFlag(true);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("入力値の形式が正しくありません。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x0003F4F0 File Offset: 0x0003D6F0
		private void SetChangeFlag(bool flag)
		{
			this.hasUnsavedChanges = flag;
			this.btnSave.Enabled = flag;
			bool flag2 = !flag;
			if (flag2)
			{
				this.temporaryOverWorldDataAddress = -1;
				this.temporaryFrameData.Clear();
				this.temporarySpriteTableData = null;
			}
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x0003F536 File Offset: 0x0003D736
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveData(this.currentIndex);
			this.SetChangeFlag(false);
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x0003F550 File Offset: 0x0003D750
		private void btnSpriteSheetExport_Click(object sender, EventArgs e)
		{
			checked
			{
				using (SaveFileDialog saveFileDialog = new SaveFileDialog())
				{
					saveFileDialog.Filter = "PNG Image (*.png)|*.png";
					saveFileDialog.Title = "画像をエクスポート";
					saveFileDialog.FileName = string.Format("OverWorld_{0:000}.png", this.currentIndex);
					bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
					if (flag)
					{
						OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
						int width = (int)overWorldData.Width;
						int height = (int)overWorldData.Height;
						int count = this.spriteFrameDataList.Count;
						byte[] paletteDataByID = this.GetPaletteDataByID(overWorldData.PaletteId1);
						Color[] array = ImageProcessor.LoadPalette(paletteDataByID, false);
						int num = width / 8;
						int num2 = height / 8;
						int num3 = num * num2;
						int num4 = num3 * 32;
						int num5 = width * count;
						int num6 = height;
						int num7 = num5 / 8;
						int num8 = num7 * num2;
						int num9 = num8 * 32;
						byte[] array2 = new byte[num9 - 1 + 1];
						List<byte[]> list = new List<byte[]>();
						int num10 = count - 1;
						for (int i = 0; i <= num10; i++)
						{
							OverWorldEditor.SpriteFrameData spriteFrameData = this.spriteFrameDataList[i];
							bool flag2 = this.temporarySpriteTableData != null && this.temporarySpriteTableData.FrameData.ContainsKey(i);
							byte[] array3;
							if (flag2)
							{
								array3 = this.temporarySpriteTableData.FrameData[i];
							}
							else
							{
								bool flag3 = this.temporaryFrameData.ContainsKey(i);
								if (flag3)
								{
									array3 = this.temporaryFrameData[i];
								}
								else
								{
									array3 = new byte[num4 - 1 + 1];
									unchecked
									{
										Array.Copy(this.romData, (long)((ulong)spriteFrameData.ImageAddress), array3, 0L, (long)num4);
									}
								}
							}
							list.Add(array3);
						}
						int num11 = num2 - 1;
						for (int j = 0; j <= num11; j++)
						{
							int num12 = num7 - 1;
							for (int k = 0; k <= num12; k++)
							{
								int num13 = k / num;
								int num14 = k % num;
								int num15 = j * num + num14;
								int num16 = num15 * 32;
								byte[] array4 = list[num13];
								int num17 = j * num7 + k;
								int num18 = num17 * 32;
								Array.Copy(array4, num16, array2, num18, 32);
							}
						}
						ImageProcessor.ExportSpriteTo4bppPng(saveFileDialog.FileName, array2, array, num5, num6);
					}
				}
			}
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x0003F7B4 File Offset: 0x0003D9B4
		private void btnSpriteSheetImport_Click(object sender, EventArgs e)
		{
			checked
			{
				using (OpenFileDialog openFileDialog = new OpenFileDialog())
				{
					openFileDialog.Filter = "PNG Image (*.png)|*.png";
					openFileDialog.Title = "画像をインポート";
					bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
					if (flag)
					{
						using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
						{
							bool flag2 = bitmap.PixelFormat != PixelFormat.Format4bppIndexed;
							if (flag2)
							{
								MessageBox.Show("画像は16色 (4bpp) インデックスカラーのPNGである必要があります。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							}
							else
							{
								OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
								List<OverWorldEditor.SpriteFrameData> list = this.spriteFrameDataList;
								int width = (int)overWorldData.Width;
								int height = (int)overWorldData.Height;
								int count = list.Count;
								int num = width * count;
								int num2 = height;
								bool flag3 = bitmap.Width != num || bitmap.Height != num2;
								if (flag3)
								{
									MessageBox.Show(string.Format("無効な画像サイズです。{0}x{1}である必要があります。", num, num2), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								}
								else
								{
									byte[] array = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
									int num3 = width / 8;
									int num4 = height / 8;
									int num5 = num3 * num4;
									int num6 = num5 * 32;
									int num7 = num / 8;
									this.temporaryFrameData.Clear();
									int num8 = count - 1;
									for (int i = 0; i <= num8; i++)
									{
										byte[] array2 = new byte[num6 - 1 + 1];
										int num9 = i;
										int num10 = num4 - 1;
										for (int j = 0; j <= num10; j++)
										{
											int num11 = num3 - 1;
											for (int k = 0; k <= num11; k++)
											{
												int num12 = num9 * num3 + k;
												int num13 = j * num7 + num12;
												int num14 = num13 * 32;
												int num15 = j * num3 + k;
												int num16 = num15 * 32;
												Array.Copy(array, num14, array2, num16, 32);
											}
										}
										bool flag4 = this.temporarySpriteTableData != null;
										if (flag4)
										{
											this.temporarySpriteTableData.FrameData[i] = array2;
										}
										else
										{
											this.temporaryFrameData[i] = array2;
										}
									}
									this.SetChangeFlag(true);
									this.LoadCurrentFrame();
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x0003FA20 File Offset: 0x0003DC20
		private void btnCreateNewSpriteTable_Click(object sender, EventArgs e)
		{
			bool flag = string.IsNullOrWhiteSpace(this.txtCreateNewSpriteSheetAddress.Text);
			checked
			{
				if (flag)
				{
					MessageBox.Show("画像テーブルの生成先アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					this.temporaryFrameData.Clear();
					uint num = 0;
					try
					{
						num = Convert.ToUInt32(this.txtCreateNewSpriteSheetAddress.Text, 16);
					}
					catch (Exception ex)
					{
						MessageBox.Show("16進数アドレスの形式が正しくありません。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
					int num2 = Convert.ToInt32(this.nudSpriteFrameLimit.Value);
					string text = this.cmbDataSizeB.SelectedItem.ToString().Split(new char[] { ' ' })[0];
					int item = this.dataSizeMapping[text].Item3;
					this.temporarySpriteTableData = new OverWorldEditor.SpriteTableData
					{
						Address = num,
						FrameCount = num2,
						ImageSize = item,
						FrameData = new Dictionary<int, byte[]>()
					};
					int num3 = num2 - 1;
					for (int i = 0; i <= num3; i++)
					{
						this.temporarySpriteTableData.FrameData[i] = new byte[item - 1 + 1];
					}
					OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
					overWorldData.SpriteTableAddress = num;
					ValueTuple<int, int, int> valueTuple = this.dataSizeMapping[text];
					overWorldData.Width = (ushort)valueTuple.Item1;
					overWorldData.Height = (ushort)valueTuple.Item2;
					this.txtDataSpriteTable.Text = num.ToString("X8");
					this.UpdateSizeComboBox(overWorldData.Width, overWorldData.Height);
					this.LoadSpriteFrameTable();
					this.SetChangeFlag(true);
				}
			}
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0003FBE4 File Offset: 0x0003DDE4
		private void btnCreateNewData_Click(object sender, EventArgs e)
		{
			OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
			bool flag = overWorldData.DataOffset == -1;
			try
			{
				bool flag2 = string.IsNullOrWhiteSpace(this.txtCreateNewData.Text);
				if (flag2)
				{
					MessageBox.Show("データの生成先アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				this.temporaryOverWorldDataAddress = checked((int)Convert.ToUInt32(this.txtCreateNewData.Text, 16));
			}
			catch (Exception ex)
			{
				MessageBox.Show("16進数アドレスの形式が正しくありません。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			bool flag3 = flag;
			if (flag3)
			{
				overWorldData.DataOffset = this.temporaryOverWorldDataAddress;
				overWorldData.Padding = ushort.MaxValue;
				overWorldData.PaletteId1 = Convert.ToUInt16((this.cmbDataPaletteId1.Items.Count > 0) ? this.cmbDataPaletteId1.Items[0].ToString() : "0000", 16);
				overWorldData.PaletteId2 = Convert.ToUInt16((this.cmbDataPaletteId2.Items.Count > 0) ? this.cmbDataPaletteId2.Items[0].ToString() : "0000", 16);
				overWorldData.FootPrint = (byte)((this.cmbDataFootPrint.Items.Count > 0) ? this.dataFootPrintMapping[this.cmbDataFootPrint.Items[0].ToString()] : 0);
				overWorldData.Width = 16;
				overWorldData.Height = 32;
				overWorldData.SpriteTableAddress = 0U;
			}
			else
			{
				ushort width = overWorldData.Width;
				ushort height = overWorldData.Height;
				uint spriteTableAddress = overWorldData.SpriteTableAddress;
				overWorldData.DataOffset = this.temporaryOverWorldDataAddress;
				overWorldData.Padding = ushort.MaxValue;
				overWorldData.PaletteId1 = Convert.ToUInt16((this.cmbDataPaletteId1.Items.Count > 0) ? this.cmbDataPaletteId1.Items[0].ToString() : "0000", 16);
				overWorldData.PaletteId2 = Convert.ToUInt16((this.cmbDataPaletteId2.Items.Count > 0) ? this.cmbDataPaletteId2.Items[0].ToString() : "0000", 16);
				overWorldData.FootPrint = (byte)((this.cmbDataFootPrint.Items.Count > 0) ? this.dataFootPrintMapping[this.cmbDataFootPrint.Items[0].ToString()] : 0);
				overWorldData.Width = width;
				overWorldData.Height = height;
				overWorldData.SpriteTableAddress = spriteTableAddress;
			}
			overWorldData.DataLength = 0;
			overWorldData.PaletteSlot = 0;
			overWorldData.UnUsedValue = 0;
			overWorldData.UnknownValue1 = false;
			overWorldData.UnknownValue2 = false;
			overWorldData.UnknownValue3 = false;
			overWorldData.LoadAddress = 0U;
			overWorldData.SizeAddress = 0U;
			overWorldData.AnimationAddress = 0U;
			overWorldData.MemoryAddress = 0U;
			overWorldData.FontId = 0;
			this.txtDataPadding.Text = overWorldData.Padding.ToString("X4");
			this.SetComboBoxValue(this.cmbDataPaletteId1, overWorldData.PaletteId1.ToString("X4"));
			this.SetComboBoxValue(this.cmbDataPaletteId2, overWorldData.PaletteId2.ToString("X4"));
			this.nudDataLength.Value = new decimal((int)overWorldData.DataLength);
			this.UpdateSizeComboBox(overWorldData.Width, overWorldData.Height);
			this.nudDataPaletteSlot.Value = new decimal(overWorldData.PaletteSlot);
			this.chkDataUnknownValue1.Checked = overWorldData.UnknownValue1;
			this.chkDataUnknownValue2.Checked = overWorldData.UnknownValue2;
			this.chkDataUnknownValue3.Checked = overWorldData.UnknownValue3;
			this.UpdateFootPrintComboBox(overWorldData.FootPrint);
			this.nudUnUsedValue.Value = new decimal((int)overWorldData.UnUsedValue);
			this.txtDataLoadAddress.Text = overWorldData.LoadAddress.ToString("X8");
			this.txtDataSizeAddress.Text = overWorldData.SizeAddress.ToString("X8");
			this.txtDataAnimationAddress.Text = overWorldData.AnimationAddress.ToString("X8");
			this.txtDataSpriteTable.Text = overWorldData.SpriteTableAddress.ToString("X8");
			this.txtDataMemoryAddress.Text = overWorldData.MemoryAddress.ToString("X8");
			this.cmbFontId.SelectedIndex = overWorldData.FontId;
			this.ModifyDataControls(true);
			this.LoadSpriteFrameTable();
			this.SetChangeFlag(true);
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x000400B4 File Offset: 0x0003E2B4
		private void cmbFontId_SelectedIndexChanged(object sender, EventArgs e)
		{
			OverWorldEditor.OverWorldData overWorldData = this.overworldDataList[this.currentIndex];
			bool flag = overWorldData.FontId != this.cmbFontId.SelectedIndex;
			if (flag)
			{
				overWorldData.FontId = this.cmbFontId.SelectedIndex;
				this.SetChangeFlag(true);
			}
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x0004010C File Offset: 0x0003E30C
		private void btnCreateNewPalette_Click(object sender, EventArgs e)
		{
			ushort newPaletteId = 0;
			bool flag = string.IsNullOrWhiteSpace(this.txtCreatePaletteId.Text);
			if (flag)
			{
				MessageBox.Show("パレットIDを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				bool flag2 = string.IsNullOrWhiteSpace(this.txtCreatePaletteAddress.Text);
				if (flag2)
				{
					MessageBox.Show("パレットアドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					try
					{
						newPaletteId = Convert.ToUInt16(this.txtCreatePaletteId.Text, 16);
					}
					catch (Exception ex)
					{
						MessageBox.Show("パレットIDは16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
					bool flag3 = this.paletteTableDataList.Any((OverWorldEditor.PaletteTableData x) => x.PaletteID == newPaletteId);
					if (flag3)
					{
						MessageBox.Show("同じパレットIDが既に存在します。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					else
					{
						using (OpenFileDialog openFileDialog = new OpenFileDialog())
						{
							openFileDialog.Filter = "PNG Image (*.png)|*.png";
							openFileDialog.Title = "16色4bppのPNG画像を選択";
							bool flag4 = openFileDialog.ShowDialog() == DialogResult.OK;
							if (flag4)
							{
								using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
								{
									bool flag5 = bitmap.PixelFormat != PixelFormat.Format4bppIndexed;
									if (flag5)
									{
										MessageBox.Show("16色4bppのPNG画像のみ対応しています。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
									}
									else
									{
										byte[] array = ImageProcessor.ConvertPaletteToBytes(bitmap.Palette);
										uint num = Convert.ToUInt32(this.txtCreatePaletteAddress.Text, 16);
										Array.Copy(array, 0L, this.romData, (long)((ulong)num), (long)array.Length);
										OverWorldEditor.PaletteTableData paletteTableData = new OverWorldEditor.PaletteTableData
										{
											Address = num,
											PaletteID = newPaletteId
										};
										int num2 = Convert.ToInt32(this.txtPaletteTableAddress.Text, 16);
										int num3 = 0;
										checked
										{
											int num4 = 0;
											for (;;)
											{
												num4 = num2 + num3 * 8;
												bool flag6 = true;
												int num5 = 0;
												do
												{
													bool flag7 = this.romData[num4 + num5] > 0;
													if (flag7)
													{
														goto Block_13;
													}
													num5++;
												}
												while (num5 <= 7);
												IL_01F5:
												bool flag8 = flag6;
												if (flag8)
												{
													break;
												}
												num3++;
												continue;
												Block_13:
												flag6 = false;
												goto IL_01F5;
											}
											int num6 = num4;
											Array.Copy(this.romData, num6, this.romData, num6 + 8, 8);
											byte[] bytes = BitConverter.GetBytes(paletteTableData.Address + 134217728U);
											Array.Copy(bytes, 0, this.romData, num6, 4);
											byte[] bytes2 = BitConverter.GetBytes(paletteTableData.PaletteID);
											Array.Copy(bytes2, 0, this.romData, num6 + 4, 2);
											this.romData[num6 + 6] = 0;
											this.romData[num6 + 7] = 0;
											this.LoadPaletteTable();
											this.cmbPaletteId.SelectedItem = newPaletteId.ToString("X4");
											MessageBox.Show("パレットを追加しました。", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0400046F RID: 1135
		public readonly int OVERWORLD_DATA_TABLE_OFFSET;

		// Token: 0x04000470 RID: 1136
		public readonly int OVERWORLD_PALETTE_TABLE_OFFSET;

		// Token: 0x04000471 RID: 1137
		public readonly int OVERWORLD_FONT_TABLE_OFFSET;

		// Token: 0x04000472 RID: 1138
		public readonly int OVERWORLD_DATA_MAX_COUNT_OFFSET;

		// Token: 0x04000473 RID: 1139
		public const int OFFSET_PADDING = 0;

		// Token: 0x04000474 RID: 1140
		public const int OFFSET_PALETTE_ID1 = 2;

		// Token: 0x04000475 RID: 1141
		public const int OFFSET_PALETTE_ID2 = 4;

		// Token: 0x04000476 RID: 1142
		public const int OFFSET_DATA_LENGTH = 6;

		// Token: 0x04000477 RID: 1143
		public const int OFFSET_WIDTH = 8;

		// Token: 0x04000478 RID: 1144
		public const int OFFSET_HEIGHT = 10;

		// Token: 0x04000479 RID: 1145
		public const int OFFSET_PALETTE_SLOT_AND_UNKNOWN = 12;

		// Token: 0x0400047A RID: 1146
		public const int OFFSET_FOOT_PRINT = 13;

		// Token: 0x0400047B RID: 1147
		public const int OFFSET_UNUSED_VALUE = 14;

		// Token: 0x0400047C RID: 1148
		public const int OFFSET_LOAD_ADDRESS = 16;

		// Token: 0x0400047D RID: 1149
		public const int OFFSET_SIZE_ADDRESS = 20;

		// Token: 0x0400047E RID: 1150
		public const int OFFSET_ANIMATION_ADDRESS = 24;

		// Token: 0x0400047F RID: 1151
		public const int OFFSET_SPRITE_TABLE_ADDRESS = 28;

		// Token: 0x04000480 RID: 1152
		public const int OFFSET_MEMORY_ADDRESS = 32;

		// Token: 0x04000481 RID: 1153
		private byte[] romData;

		// Token: 0x04000482 RID: 1154
		private bool hasUnsavedChanges;

		// Token: 0x04000483 RID: 1155
		private int currentIndex;

		// Token: 0x04000484 RID: 1156
		private List<OverWorldEditor.OverWorldData> overworldDataList;

		// Token: 0x04000485 RID: 1157
		private List<OverWorldEditor.PaletteTableData> paletteTableDataList;

		// Token: 0x04000486 RID: 1158
		private List<OverWorldEditor.SpriteFrameData> spriteFrameDataList;

		// Token: 0x04000487 RID: 1159
		private Dictionary<int, int> frameLimits;

		// Token: 0x04000488 RID: 1160
		private int temporaryOverWorldDataAddress;

		// Token: 0x04000489 RID: 1161
		private Dictionary<int, byte[]> temporaryFrameData;

		// Token: 0x0400048A RID: 1162
		private OverWorldEditor.SpriteTableData temporarySpriteTableData;

		// Token: 0x0400048B RID: 1163
		private Dictionary<string, ValueTuple<int, int, int>> dataSizeMapping;

		// Token: 0x0400048C RID: 1164
		private Dictionary<string, byte> dataFootPrintMapping;

		// Token: 0x02000056 RID: 86
		public class OverWorldData
		{
			// Token: 0x170005D2 RID: 1490
			// (get) Token: 0x06000F88 RID: 3976 RVA: 0x0006C1D4 File Offset: 0x0006A3D4
			// (set) Token: 0x06000F89 RID: 3977 RVA: 0x0006C1DE File Offset: 0x0006A3DE
			public int Index { get; set; }

			// Token: 0x170005D3 RID: 1491
			// (get) Token: 0x06000F8A RID: 3978 RVA: 0x0006C1E7 File Offset: 0x0006A3E7
			// (set) Token: 0x06000F8B RID: 3979 RVA: 0x0006C1F1 File Offset: 0x0006A3F1
			public int DataOffset { get; set; }

			// Token: 0x170005D4 RID: 1492
			// (get) Token: 0x06000F8C RID: 3980 RVA: 0x0006C1FA File Offset: 0x0006A3FA
			// (set) Token: 0x06000F8D RID: 3981 RVA: 0x0006C204 File Offset: 0x0006A404
			public ushort Padding { get; set; }

			// Token: 0x170005D5 RID: 1493
			// (get) Token: 0x06000F8E RID: 3982 RVA: 0x0006C20D File Offset: 0x0006A40D
			// (set) Token: 0x06000F8F RID: 3983 RVA: 0x0006C217 File Offset: 0x0006A417
			public ushort PaletteId1 { get; set; }

			// Token: 0x170005D6 RID: 1494
			// (get) Token: 0x06000F90 RID: 3984 RVA: 0x0006C220 File Offset: 0x0006A420
			// (set) Token: 0x06000F91 RID: 3985 RVA: 0x0006C22A File Offset: 0x0006A42A
			public ushort PaletteId2 { get; set; }

			// Token: 0x170005D7 RID: 1495
			// (get) Token: 0x06000F92 RID: 3986 RVA: 0x0006C233 File Offset: 0x0006A433
			// (set) Token: 0x06000F93 RID: 3987 RVA: 0x0006C23D File Offset: 0x0006A43D
			public ushort DataLength { get; set; }

			// Token: 0x170005D8 RID: 1496
			// (get) Token: 0x06000F94 RID: 3988 RVA: 0x0006C246 File Offset: 0x0006A446
			// (set) Token: 0x06000F95 RID: 3989 RVA: 0x0006C250 File Offset: 0x0006A450
			public ushort Width { get; set; }

			// Token: 0x170005D9 RID: 1497
			// (get) Token: 0x06000F96 RID: 3990 RVA: 0x0006C259 File Offset: 0x0006A459
			// (set) Token: 0x06000F97 RID: 3991 RVA: 0x0006C263 File Offset: 0x0006A463
			public ushort Height { get; set; }

			// Token: 0x170005DA RID: 1498
			// (get) Token: 0x06000F98 RID: 3992 RVA: 0x0006C26C File Offset: 0x0006A46C
			// (set) Token: 0x06000F99 RID: 3993 RVA: 0x0006C276 File Offset: 0x0006A476
			public int PaletteSlot { get; set; }

			// Token: 0x170005DB RID: 1499
			// (get) Token: 0x06000F9A RID: 3994 RVA: 0x0006C27F File Offset: 0x0006A47F
			// (set) Token: 0x06000F9B RID: 3995 RVA: 0x0006C289 File Offset: 0x0006A489
			public bool UnknownValue1 { get; set; }

			// Token: 0x170005DC RID: 1500
			// (get) Token: 0x06000F9C RID: 3996 RVA: 0x0006C292 File Offset: 0x0006A492
			// (set) Token: 0x06000F9D RID: 3997 RVA: 0x0006C29C File Offset: 0x0006A49C
			public bool UnknownValue2 { get; set; }

			// Token: 0x170005DD RID: 1501
			// (get) Token: 0x06000F9E RID: 3998 RVA: 0x0006C2A5 File Offset: 0x0006A4A5
			// (set) Token: 0x06000F9F RID: 3999 RVA: 0x0006C2AF File Offset: 0x0006A4AF
			public bool UnknownValue3 { get; set; }

			// Token: 0x170005DE RID: 1502
			// (get) Token: 0x06000FA0 RID: 4000 RVA: 0x0006C2B8 File Offset: 0x0006A4B8
			// (set) Token: 0x06000FA1 RID: 4001 RVA: 0x0006C2C2 File Offset: 0x0006A4C2
			public byte FootPrint { get; set; }

			// Token: 0x170005DF RID: 1503
			// (get) Token: 0x06000FA2 RID: 4002 RVA: 0x0006C2CB File Offset: 0x0006A4CB
			// (set) Token: 0x06000FA3 RID: 4003 RVA: 0x0006C2D5 File Offset: 0x0006A4D5
			public ushort UnUsedValue { get; set; }

			// Token: 0x170005E0 RID: 1504
			// (get) Token: 0x06000FA4 RID: 4004 RVA: 0x0006C2DE File Offset: 0x0006A4DE
			// (set) Token: 0x06000FA5 RID: 4005 RVA: 0x0006C2E8 File Offset: 0x0006A4E8
			public uint LoadAddress { get; set; }

			// Token: 0x170005E1 RID: 1505
			// (get) Token: 0x06000FA6 RID: 4006 RVA: 0x0006C2F1 File Offset: 0x0006A4F1
			// (set) Token: 0x06000FA7 RID: 4007 RVA: 0x0006C2FB File Offset: 0x0006A4FB
			public uint SizeAddress { get; set; }

			// Token: 0x170005E2 RID: 1506
			// (get) Token: 0x06000FA8 RID: 4008 RVA: 0x0006C304 File Offset: 0x0006A504
			// (set) Token: 0x06000FA9 RID: 4009 RVA: 0x0006C30E File Offset: 0x0006A50E
			public uint AnimationAddress { get; set; }

			// Token: 0x170005E3 RID: 1507
			// (get) Token: 0x06000FAA RID: 4010 RVA: 0x0006C317 File Offset: 0x0006A517
			// (set) Token: 0x06000FAB RID: 4011 RVA: 0x0006C321 File Offset: 0x0006A521
			public uint SpriteTableAddress { get; set; }

			// Token: 0x170005E4 RID: 1508
			// (get) Token: 0x06000FAC RID: 4012 RVA: 0x0006C32A File Offset: 0x0006A52A
			// (set) Token: 0x06000FAD RID: 4013 RVA: 0x0006C334 File Offset: 0x0006A534
			public uint MemoryAddress { get; set; }

			// Token: 0x170005E5 RID: 1509
			// (get) Token: 0x06000FAE RID: 4014 RVA: 0x0006C33D File Offset: 0x0006A53D
			// (set) Token: 0x06000FAF RID: 4015 RVA: 0x0006C347 File Offset: 0x0006A547
			public int FontId { get; set; }
		}

		// Token: 0x02000057 RID: 87
		public class PaletteTableData
		{
			// Token: 0x170005E6 RID: 1510
			// (get) Token: 0x06000FB1 RID: 4017 RVA: 0x0006C358 File Offset: 0x0006A558
			// (set) Token: 0x06000FB2 RID: 4018 RVA: 0x0006C362 File Offset: 0x0006A562
			public uint Address { get; set; }

			// Token: 0x170005E7 RID: 1511
			// (get) Token: 0x06000FB3 RID: 4019 RVA: 0x0006C36B File Offset: 0x0006A56B
			// (set) Token: 0x06000FB4 RID: 4020 RVA: 0x0006C375 File Offset: 0x0006A575
			public ushort PaletteID { get; set; }
		}

		// Token: 0x02000058 RID: 88
		public class SpriteFrameData
		{
			// Token: 0x170005E8 RID: 1512
			// (get) Token: 0x06000FB6 RID: 4022 RVA: 0x0006C386 File Offset: 0x0006A586
			// (set) Token: 0x06000FB7 RID: 4023 RVA: 0x0006C390 File Offset: 0x0006A590
			public uint ImageAddress { get; set; }

			// Token: 0x170005E9 RID: 1513
			// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x0006C399 File Offset: 0x0006A599
			// (set) Token: 0x06000FB9 RID: 4025 RVA: 0x0006C3A3 File Offset: 0x0006A5A3
			public ushort ImageSize { get; set; }

			// Token: 0x170005EA RID: 1514
			// (get) Token: 0x06000FBA RID: 4026 RVA: 0x0006C3AC File Offset: 0x0006A5AC
			// (set) Token: 0x06000FBB RID: 4027 RVA: 0x0006C3B6 File Offset: 0x0006A5B6
			public ushort Unknown { get; set; }

			// Token: 0x170005EB RID: 1515
			// (get) Token: 0x06000FBC RID: 4028 RVA: 0x0006C3BF File Offset: 0x0006A5BF
			// (set) Token: 0x06000FBD RID: 4029 RVA: 0x0006C3C9 File Offset: 0x0006A5C9
			public int FrameIndex { get; set; }

			// Token: 0x170005EC RID: 1516
			// (get) Token: 0x06000FBE RID: 4030 RVA: 0x0006C3D2 File Offset: 0x0006A5D2
			// (set) Token: 0x06000FBF RID: 4031 RVA: 0x0006C3DC File Offset: 0x0006A5DC
			public int FrameLimit { get; set; }
		}

		// Token: 0x02000059 RID: 89
		public class SpriteTableData
		{
			// Token: 0x170005ED RID: 1517
			// (get) Token: 0x06000FC1 RID: 4033 RVA: 0x0006C3ED File Offset: 0x0006A5ED
			// (set) Token: 0x06000FC2 RID: 4034 RVA: 0x0006C3F7 File Offset: 0x0006A5F7
			public uint Address { get; set; }

			// Token: 0x170005EE RID: 1518
			// (get) Token: 0x06000FC3 RID: 4035 RVA: 0x0006C400 File Offset: 0x0006A600
			// (set) Token: 0x06000FC4 RID: 4036 RVA: 0x0006C40A File Offset: 0x0006A60A
			public int FrameCount { get; set; }

			// Token: 0x170005EF RID: 1519
			// (get) Token: 0x06000FC5 RID: 4037 RVA: 0x0006C413 File Offset: 0x0006A613
			// (set) Token: 0x06000FC6 RID: 4038 RVA: 0x0006C41D File Offset: 0x0006A61D
			public int ImageSize { get; set; }

			// Token: 0x170005F0 RID: 1520
			// (get) Token: 0x06000FC7 RID: 4039 RVA: 0x0006C426 File Offset: 0x0006A626
			// (set) Token: 0x06000FC8 RID: 4040 RVA: 0x0006C430 File Offset: 0x0006A630
			public Dictionary<int, byte[]> FrameData { get; set; }
		}
	}
}
