using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x0200002A RID: 42
	public partial class WildPokemonEditor : Form
	{
		// Token: 0x06000CBB RID: 3259 RVA: 0x0005F144 File Offset: 0x0005D344
		public WildPokemonEditor()
		{
			base.Load += this.WildPokemonEditor_Load;
			base.FormClosing += this.WildPokemonEditor_FormClosing;
			this.ENABLE_DNS_WILD_ENCOUNTER = RomIniReader.ReadBoolean("ENABLE_DNS_WILD_ENCOUNTER");
			this.WILD_ENCOUNTER_TABLE_MORNING_OFFSET = (this.ENABLE_DNS_WILD_ENCOUNTER ? RomIniReader.ReadHexOrDecimal("WILD_ENCOUNTER_TABLE_MORNING_OFFSET") : 0);
			this.WILD_ENCOUNTER_TABLE_DAY_OFFSET = RomIniReader.ReadHexOrDecimal("WILD_ENCOUNTER_TABLE_DAY_OFFSET");
			this.WILD_ENCOUNTER_TABLE_EVENING_OFFSET = (this.ENABLE_DNS_WILD_ENCOUNTER ? RomIniReader.ReadHexOrDecimal("WILD_ENCOUNTER_TABLE_EVENING_OFFSET") : 0);
			this.WILD_ENCOUNTER_TABLE_NIGHT_OFFSET = (this.ENABLE_DNS_WILD_ENCOUNTER ? RomIniReader.ReadHexOrDecimal("WILD_ENCOUNTER_TABLE_NIGHT_OFFSET") : 0);
			this.SLOT_COUNTS = new int[] { 12, 5, 5, 10 };
			this.AREA_POINTER_OFFSETS = new int[] { 4, 8, 12, 16 };
			this.hasUnsavedChanges = false;
			this.previousSelectedIndex = -1;
			this.isLoadingEntry = false;
			this.pokemonIconList = new Dictionary<int, PokemonData>();
			this.previousComboSelections = new Dictionary<ComboBox, int>();
			this.areaControls = new List<WildPokemonEditor.AreaControlSet>();
			this.loadedEncounterTables = new Dictionary<int, List<WildPokemonEditor.WildEncounterEntry>>();
			this.InitializeComponent();
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x06000CBE RID: 3262 RVA: 0x00066661 File Offset: 0x00064861
		// (set) Token: 0x06000CBF RID: 3263 RVA: 0x0006666C File Offset: 0x0006486C
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

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x06000CC0 RID: 3264 RVA: 0x000666AF File Offset: 0x000648AF
		// (set) Token: 0x06000CC1 RID: 3265 RVA: 0x000666B9 File Offset: 0x000648B9
		internal virtual GroupBox grpSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x06000CC2 RID: 3266 RVA: 0x000666C2 File Offset: 0x000648C2
		// (set) Token: 0x06000CC3 RID: 3267 RVA: 0x000666CC File Offset: 0x000648CC
		internal virtual Label lblMapBankSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x06000CC4 RID: 3268 RVA: 0x000666D5 File Offset: 0x000648D5
		// (set) Token: 0x06000CC5 RID: 3269 RVA: 0x000666DF File Offset: 0x000648DF
		internal virtual NumericUpDown nudMapBankSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x06000CC6 RID: 3270 RVA: 0x000666E8 File Offset: 0x000648E8
		// (set) Token: 0x06000CC7 RID: 3271 RVA: 0x000666F2 File Offset: 0x000648F2
		internal virtual GroupBox grpTimeSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06000CC8 RID: 3272 RVA: 0x000666FB File Offset: 0x000648FB
		// (set) Token: 0x06000CC9 RID: 3273 RVA: 0x00066705 File Offset: 0x00064905
		internal virtual RadioButton rbNightSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x06000CCA RID: 3274 RVA: 0x0006670E File Offset: 0x0006490E
		// (set) Token: 0x06000CCB RID: 3275 RVA: 0x00066718 File Offset: 0x00064918
		internal virtual RadioButton rbEveningSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06000CCC RID: 3276 RVA: 0x00066721 File Offset: 0x00064921
		// (set) Token: 0x06000CCD RID: 3277 RVA: 0x0006672B File Offset: 0x0006492B
		internal virtual RadioButton rbDaySearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x06000CCE RID: 3278 RVA: 0x00066734 File Offset: 0x00064934
		// (set) Token: 0x06000CCF RID: 3279 RVA: 0x0006673E File Offset: 0x0006493E
		internal virtual RadioButton rbMorningSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x06000CD0 RID: 3280 RVA: 0x00066747 File Offset: 0x00064947
		// (set) Token: 0x06000CD1 RID: 3281 RVA: 0x00066751 File Offset: 0x00064951
		internal virtual Label lblMapNumberSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x06000CD2 RID: 3282 RVA: 0x0006675A File Offset: 0x0006495A
		// (set) Token: 0x06000CD3 RID: 3283 RVA: 0x00066764 File Offset: 0x00064964
		internal virtual NumericUpDown nudMapNumberSearch
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x06000CD4 RID: 3284 RVA: 0x0006676D File Offset: 0x0006496D
		// (set) Token: 0x06000CD5 RID: 3285 RVA: 0x00066778 File Offset: 0x00064978
		internal virtual ListBox lstResult
		{
			[CompilerGenerated]
			get
			{
				return this._lstResult;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstResult_SelectedIndexChanged);
				ListBox listBox = this._lstResult;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstResult = value;
				listBox = this._lstResult;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06000CD6 RID: 3286 RVA: 0x000667BB File Offset: 0x000649BB
		// (set) Token: 0x06000CD7 RID: 3287 RVA: 0x000667C8 File Offset: 0x000649C8
		internal virtual Button btnSearch
		{
			[CompilerGenerated]
			get
			{
				return this._btnSearch;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSearch_Click);
				Button button = this._btnSearch;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSearch = value;
				button = this._btnSearch;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06000CD8 RID: 3288 RVA: 0x0006680B File Offset: 0x00064A0B
		// (set) Token: 0x06000CD9 RID: 3289 RVA: 0x00066818 File Offset: 0x00064A18
		internal virtual TabControl tabAreaData
		{
			[CompilerGenerated]
			get
			{
				return this._tabAreaData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.tabAreaData_SelectedIndexChanged);
				TabControl tabControl = this._tabAreaData;
				if (tabControl != null)
				{
					tabControl.SelectedIndexChanged -= eventHandler;
				}
				this._tabAreaData = value;
				tabControl = this._tabAreaData;
				if (tabControl != null)
				{
					tabControl.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06000CDA RID: 3290 RVA: 0x0006685B File Offset: 0x00064A5B
		// (set) Token: 0x06000CDB RID: 3291 RVA: 0x00066865 File Offset: 0x00064A65
		internal virtual TabPage tabField
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x06000CDC RID: 3292 RVA: 0x0006686E File Offset: 0x00064A6E
		// (set) Token: 0x06000CDD RID: 3293 RVA: 0x00066878 File Offset: 0x00064A78
		internal virtual TabPage tabWater
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x06000CDE RID: 3294 RVA: 0x00066881 File Offset: 0x00064A81
		// (set) Token: 0x06000CDF RID: 3295 RVA: 0x0006688B File Offset: 0x00064A8B
		internal virtual TabPage tabRock
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x06000CE0 RID: 3296 RVA: 0x00066894 File Offset: 0x00064A94
		// (set) Token: 0x06000CE1 RID: 3297 RVA: 0x0006689E File Offset: 0x00064A9E
		internal virtual TabPage tabFishing
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x06000CE2 RID: 3298 RVA: 0x000668A7 File Offset: 0x00064AA7
		// (set) Token: 0x06000CE3 RID: 3299 RVA: 0x000668B1 File Offset: 0x00064AB1
		internal virtual NumericUpDown nudMaxLvField1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x06000CE4 RID: 3300 RVA: 0x000668BA File Offset: 0x00064ABA
		// (set) Token: 0x06000CE5 RID: 3301 RVA: 0x000668C4 File Offset: 0x00064AC4
		internal virtual NumericUpDown nudMinLvField1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x06000CE6 RID: 3302 RVA: 0x000668CD File Offset: 0x00064ACD
		// (set) Token: 0x06000CE7 RID: 3303 RVA: 0x000668D7 File Offset: 0x00064AD7
		internal virtual Label lblPercentageField1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06000CE8 RID: 3304 RVA: 0x000668E0 File Offset: 0x00064AE0
		// (set) Token: 0x06000CE9 RID: 3305 RVA: 0x000668EA File Offset: 0x00064AEA
		internal virtual PictureBox picIconField1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x06000CEA RID: 3306 RVA: 0x000668F3 File Offset: 0x00064AF3
		// (set) Token: 0x06000CEB RID: 3307 RVA: 0x000668FD File Offset: 0x00064AFD
		internal virtual ComboBox cmbPokemonField1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x06000CEC RID: 3308 RVA: 0x00066906 File Offset: 0x00064B06
		// (set) Token: 0x06000CED RID: 3309 RVA: 0x00066910 File Offset: 0x00064B10
		internal virtual NumericUpDown nudMaxLvField12
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x06000CEE RID: 3310 RVA: 0x00066919 File Offset: 0x00064B19
		// (set) Token: 0x06000CEF RID: 3311 RVA: 0x00066923 File Offset: 0x00064B23
		internal virtual NumericUpDown nudMaxLvField11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x06000CF0 RID: 3312 RVA: 0x0006692C File Offset: 0x00064B2C
		// (set) Token: 0x06000CF1 RID: 3313 RVA: 0x00066936 File Offset: 0x00064B36
		internal virtual NumericUpDown nudMaxLvField10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x06000CF2 RID: 3314 RVA: 0x0006693F File Offset: 0x00064B3F
		// (set) Token: 0x06000CF3 RID: 3315 RVA: 0x00066949 File Offset: 0x00064B49
		internal virtual NumericUpDown nudMaxLvField9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x06000CF4 RID: 3316 RVA: 0x00066952 File Offset: 0x00064B52
		// (set) Token: 0x06000CF5 RID: 3317 RVA: 0x0006695C File Offset: 0x00064B5C
		internal virtual NumericUpDown nudMaxLvField8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x06000CF6 RID: 3318 RVA: 0x00066965 File Offset: 0x00064B65
		// (set) Token: 0x06000CF7 RID: 3319 RVA: 0x0006696F File Offset: 0x00064B6F
		internal virtual NumericUpDown nudMaxLvField7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06000CF8 RID: 3320 RVA: 0x00066978 File Offset: 0x00064B78
		// (set) Token: 0x06000CF9 RID: 3321 RVA: 0x00066982 File Offset: 0x00064B82
		internal virtual NumericUpDown nudMaxLvField6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06000CFA RID: 3322 RVA: 0x0006698B File Offset: 0x00064B8B
		// (set) Token: 0x06000CFB RID: 3323 RVA: 0x00066995 File Offset: 0x00064B95
		internal virtual NumericUpDown nudMaxLvField5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x06000CFC RID: 3324 RVA: 0x0006699E File Offset: 0x00064B9E
		// (set) Token: 0x06000CFD RID: 3325 RVA: 0x000669A8 File Offset: 0x00064BA8
		internal virtual NumericUpDown nudMaxLvField4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x06000CFE RID: 3326 RVA: 0x000669B1 File Offset: 0x00064BB1
		// (set) Token: 0x06000CFF RID: 3327 RVA: 0x000669BB File Offset: 0x00064BBB
		internal virtual NumericUpDown nudMaxLvField3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x06000D00 RID: 3328 RVA: 0x000669C4 File Offset: 0x00064BC4
		// (set) Token: 0x06000D01 RID: 3329 RVA: 0x000669CE File Offset: 0x00064BCE
		internal virtual NumericUpDown nudMaxLvField2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x06000D02 RID: 3330 RVA: 0x000669D7 File Offset: 0x00064BD7
		// (set) Token: 0x06000D03 RID: 3331 RVA: 0x000669E1 File Offset: 0x00064BE1
		internal virtual NumericUpDown nudMinLvField12
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x06000D04 RID: 3332 RVA: 0x000669EA File Offset: 0x00064BEA
		// (set) Token: 0x06000D05 RID: 3333 RVA: 0x000669F4 File Offset: 0x00064BF4
		internal virtual NumericUpDown nudMinLvField11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06000D06 RID: 3334 RVA: 0x000669FD File Offset: 0x00064BFD
		// (set) Token: 0x06000D07 RID: 3335 RVA: 0x00066A07 File Offset: 0x00064C07
		internal virtual NumericUpDown nudMinLvField10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x06000D08 RID: 3336 RVA: 0x00066A10 File Offset: 0x00064C10
		// (set) Token: 0x06000D09 RID: 3337 RVA: 0x00066A1A File Offset: 0x00064C1A
		internal virtual NumericUpDown nudMinLvField9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D7 RID: 1239
		// (get) Token: 0x06000D0A RID: 3338 RVA: 0x00066A23 File Offset: 0x00064C23
		// (set) Token: 0x06000D0B RID: 3339 RVA: 0x00066A2D File Offset: 0x00064C2D
		internal virtual NumericUpDown nudMinLvField8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x06000D0C RID: 3340 RVA: 0x00066A36 File Offset: 0x00064C36
		// (set) Token: 0x06000D0D RID: 3341 RVA: 0x00066A40 File Offset: 0x00064C40
		internal virtual NumericUpDown nudMinLvField7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x06000D0E RID: 3342 RVA: 0x00066A49 File Offset: 0x00064C49
		// (set) Token: 0x06000D0F RID: 3343 RVA: 0x00066A53 File Offset: 0x00064C53
		internal virtual NumericUpDown nudMinLvField6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x06000D10 RID: 3344 RVA: 0x00066A5C File Offset: 0x00064C5C
		// (set) Token: 0x06000D11 RID: 3345 RVA: 0x00066A66 File Offset: 0x00064C66
		internal virtual NumericUpDown nudMinLvField5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x06000D12 RID: 3346 RVA: 0x00066A6F File Offset: 0x00064C6F
		// (set) Token: 0x06000D13 RID: 3347 RVA: 0x00066A79 File Offset: 0x00064C79
		internal virtual NumericUpDown nudMinLvField4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x06000D14 RID: 3348 RVA: 0x00066A82 File Offset: 0x00064C82
		// (set) Token: 0x06000D15 RID: 3349 RVA: 0x00066A8C File Offset: 0x00064C8C
		internal virtual NumericUpDown nudMinLvField3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x06000D16 RID: 3350 RVA: 0x00066A95 File Offset: 0x00064C95
		// (set) Token: 0x06000D17 RID: 3351 RVA: 0x00066A9F File Offset: 0x00064C9F
		internal virtual NumericUpDown nudMinLvField2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x06000D18 RID: 3352 RVA: 0x00066AA8 File Offset: 0x00064CA8
		// (set) Token: 0x06000D19 RID: 3353 RVA: 0x00066AB2 File Offset: 0x00064CB2
		internal virtual Label lblPercentageField12
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x06000D1A RID: 3354 RVA: 0x00066ABB File Offset: 0x00064CBB
		// (set) Token: 0x06000D1B RID: 3355 RVA: 0x00066AC5 File Offset: 0x00064CC5
		internal virtual Label lblPercentageField11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x06000D1C RID: 3356 RVA: 0x00066ACE File Offset: 0x00064CCE
		// (set) Token: 0x06000D1D RID: 3357 RVA: 0x00066AD8 File Offset: 0x00064CD8
		internal virtual Label lblPercentageField10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x06000D1E RID: 3358 RVA: 0x00066AE1 File Offset: 0x00064CE1
		// (set) Token: 0x06000D1F RID: 3359 RVA: 0x00066AEB File Offset: 0x00064CEB
		internal virtual Label lblPercentageField9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x06000D20 RID: 3360 RVA: 0x00066AF4 File Offset: 0x00064CF4
		// (set) Token: 0x06000D21 RID: 3361 RVA: 0x00066AFE File Offset: 0x00064CFE
		internal virtual Label lblPercentageField8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x06000D22 RID: 3362 RVA: 0x00066B07 File Offset: 0x00064D07
		// (set) Token: 0x06000D23 RID: 3363 RVA: 0x00066B11 File Offset: 0x00064D11
		internal virtual Label lblPercentageField7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x06000D24 RID: 3364 RVA: 0x00066B1A File Offset: 0x00064D1A
		// (set) Token: 0x06000D25 RID: 3365 RVA: 0x00066B24 File Offset: 0x00064D24
		internal virtual Label lblPercentageField6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x06000D26 RID: 3366 RVA: 0x00066B2D File Offset: 0x00064D2D
		// (set) Token: 0x06000D27 RID: 3367 RVA: 0x00066B37 File Offset: 0x00064D37
		internal virtual Label lblPercentageField5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x06000D28 RID: 3368 RVA: 0x00066B40 File Offset: 0x00064D40
		// (set) Token: 0x06000D29 RID: 3369 RVA: 0x00066B4A File Offset: 0x00064D4A
		internal virtual Label lblPercentageField4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x06000D2A RID: 3370 RVA: 0x00066B53 File Offset: 0x00064D53
		// (set) Token: 0x06000D2B RID: 3371 RVA: 0x00066B5D File Offset: 0x00064D5D
		internal virtual Label lblPercentageField3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x06000D2C RID: 3372 RVA: 0x00066B66 File Offset: 0x00064D66
		// (set) Token: 0x06000D2D RID: 3373 RVA: 0x00066B70 File Offset: 0x00064D70
		internal virtual Label lblPercentageField2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x06000D2E RID: 3374 RVA: 0x00066B79 File Offset: 0x00064D79
		// (set) Token: 0x06000D2F RID: 3375 RVA: 0x00066B83 File Offset: 0x00064D83
		internal virtual PictureBox picIconField12
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x06000D30 RID: 3376 RVA: 0x00066B8C File Offset: 0x00064D8C
		// (set) Token: 0x06000D31 RID: 3377 RVA: 0x00066B96 File Offset: 0x00064D96
		internal virtual ComboBox cmbPokemonField12
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x06000D32 RID: 3378 RVA: 0x00066B9F File Offset: 0x00064D9F
		// (set) Token: 0x06000D33 RID: 3379 RVA: 0x00066BA9 File Offset: 0x00064DA9
		internal virtual PictureBox picIconField11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x06000D34 RID: 3380 RVA: 0x00066BB2 File Offset: 0x00064DB2
		// (set) Token: 0x06000D35 RID: 3381 RVA: 0x00066BBC File Offset: 0x00064DBC
		internal virtual ComboBox cmbPokemonField11
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x06000D36 RID: 3382 RVA: 0x00066BC5 File Offset: 0x00064DC5
		// (set) Token: 0x06000D37 RID: 3383 RVA: 0x00066BCF File Offset: 0x00064DCF
		internal virtual PictureBox picIconField10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x06000D38 RID: 3384 RVA: 0x00066BD8 File Offset: 0x00064DD8
		// (set) Token: 0x06000D39 RID: 3385 RVA: 0x00066BE2 File Offset: 0x00064DE2
		internal virtual ComboBox cmbPokemonField10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x06000D3A RID: 3386 RVA: 0x00066BEB File Offset: 0x00064DEB
		// (set) Token: 0x06000D3B RID: 3387 RVA: 0x00066BF5 File Offset: 0x00064DF5
		internal virtual PictureBox picIconField9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x06000D3C RID: 3388 RVA: 0x00066BFE File Offset: 0x00064DFE
		// (set) Token: 0x06000D3D RID: 3389 RVA: 0x00066C08 File Offset: 0x00064E08
		internal virtual ComboBox cmbPokemonField9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x06000D3E RID: 3390 RVA: 0x00066C11 File Offset: 0x00064E11
		// (set) Token: 0x06000D3F RID: 3391 RVA: 0x00066C1B File Offset: 0x00064E1B
		internal virtual PictureBox picIconField8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x06000D40 RID: 3392 RVA: 0x00066C24 File Offset: 0x00064E24
		// (set) Token: 0x06000D41 RID: 3393 RVA: 0x00066C2E File Offset: 0x00064E2E
		internal virtual ComboBox cmbPokemonField8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x06000D42 RID: 3394 RVA: 0x00066C37 File Offset: 0x00064E37
		// (set) Token: 0x06000D43 RID: 3395 RVA: 0x00066C41 File Offset: 0x00064E41
		internal virtual PictureBox picIconField7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x06000D44 RID: 3396 RVA: 0x00066C4A File Offset: 0x00064E4A
		// (set) Token: 0x06000D45 RID: 3397 RVA: 0x00066C54 File Offset: 0x00064E54
		internal virtual ComboBox cmbPokemonField7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x06000D46 RID: 3398 RVA: 0x00066C5D File Offset: 0x00064E5D
		// (set) Token: 0x06000D47 RID: 3399 RVA: 0x00066C67 File Offset: 0x00064E67
		internal virtual PictureBox picIconField6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x06000D48 RID: 3400 RVA: 0x00066C70 File Offset: 0x00064E70
		// (set) Token: 0x06000D49 RID: 3401 RVA: 0x00066C7A File Offset: 0x00064E7A
		internal virtual ComboBox cmbPokemonField6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x06000D4A RID: 3402 RVA: 0x00066C83 File Offset: 0x00064E83
		// (set) Token: 0x06000D4B RID: 3403 RVA: 0x00066C8D File Offset: 0x00064E8D
		internal virtual PictureBox picIconField5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x06000D4C RID: 3404 RVA: 0x00066C96 File Offset: 0x00064E96
		// (set) Token: 0x06000D4D RID: 3405 RVA: 0x00066CA0 File Offset: 0x00064EA0
		internal virtual ComboBox cmbPokemonField5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x06000D4E RID: 3406 RVA: 0x00066CA9 File Offset: 0x00064EA9
		// (set) Token: 0x06000D4F RID: 3407 RVA: 0x00066CB3 File Offset: 0x00064EB3
		internal virtual PictureBox picIconField4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x06000D50 RID: 3408 RVA: 0x00066CBC File Offset: 0x00064EBC
		// (set) Token: 0x06000D51 RID: 3409 RVA: 0x00066CC6 File Offset: 0x00064EC6
		internal virtual ComboBox cmbPokemonField4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x06000D52 RID: 3410 RVA: 0x00066CCF File Offset: 0x00064ECF
		// (set) Token: 0x06000D53 RID: 3411 RVA: 0x00066CD9 File Offset: 0x00064ED9
		internal virtual PictureBox picIconField3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x06000D54 RID: 3412 RVA: 0x00066CE2 File Offset: 0x00064EE2
		// (set) Token: 0x06000D55 RID: 3413 RVA: 0x00066CEC File Offset: 0x00064EEC
		internal virtual ComboBox cmbPokemonField3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x06000D56 RID: 3414 RVA: 0x00066CF5 File Offset: 0x00064EF5
		// (set) Token: 0x06000D57 RID: 3415 RVA: 0x00066CFF File Offset: 0x00064EFF
		internal virtual PictureBox picIconField2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x06000D58 RID: 3416 RVA: 0x00066D08 File Offset: 0x00064F08
		// (set) Token: 0x06000D59 RID: 3417 RVA: 0x00066D12 File Offset: 0x00064F12
		internal virtual ComboBox cmbPokemonField2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x06000D5A RID: 3418 RVA: 0x00066D1B File Offset: 0x00064F1B
		// (set) Token: 0x06000D5B RID: 3419 RVA: 0x00066D28 File Offset: 0x00064F28
		internal virtual NumericUpDown nudEncounterRate
		{
			[CompilerGenerated]
			get
			{
				return this._nudEncounterRate;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudEncounterRate_ValueChanged);
				NumericUpDown numericUpDown = this._nudEncounterRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEncounterRate = value;
				numericUpDown = this._nudEncounterRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x06000D5C RID: 3420 RVA: 0x00066D6B File Offset: 0x00064F6B
		// (set) Token: 0x06000D5D RID: 3421 RVA: 0x00066D75 File Offset: 0x00064F75
		internal virtual Label lblEncoutnerRate
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x06000D5E RID: 3422 RVA: 0x00066D7E File Offset: 0x00064F7E
		// (set) Token: 0x06000D5F RID: 3423 RVA: 0x00066D88 File Offset: 0x00064F88
		internal virtual GroupBox grpNewAreaData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x06000D60 RID: 3424 RVA: 0x00066D91 File Offset: 0x00064F91
		// (set) Token: 0x06000D61 RID: 3425 RVA: 0x00066D9C File Offset: 0x00064F9C
		internal virtual Button btnNewMapEntry
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewMapEntry;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewMapEntry_Click);
				Button button = this._btnNewMapEntry;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewMapEntry = value;
				button = this._btnNewMapEntry;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x06000D62 RID: 3426 RVA: 0x00066DDF File Offset: 0x00064FDF
		// (set) Token: 0x06000D63 RID: 3427 RVA: 0x00066DE9 File Offset: 0x00064FE9
		internal virtual Label lblBorder1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x06000D64 RID: 3428 RVA: 0x00066DF2 File Offset: 0x00064FF2
		// (set) Token: 0x06000D65 RID: 3429 RVA: 0x00066DFC File Offset: 0x00064FFC
		internal virtual NumericUpDown nudMaxLvWater5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x06000D66 RID: 3430 RVA: 0x00066E05 File Offset: 0x00065005
		// (set) Token: 0x06000D67 RID: 3431 RVA: 0x00066E0F File Offset: 0x0006500F
		internal virtual NumericUpDown nudMaxLvWater4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x06000D68 RID: 3432 RVA: 0x00066E18 File Offset: 0x00065018
		// (set) Token: 0x06000D69 RID: 3433 RVA: 0x00066E22 File Offset: 0x00065022
		internal virtual NumericUpDown nudMaxLvWater3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x06000D6A RID: 3434 RVA: 0x00066E2B File Offset: 0x0006502B
		// (set) Token: 0x06000D6B RID: 3435 RVA: 0x00066E35 File Offset: 0x00065035
		internal virtual NumericUpDown nudMaxLvWater2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x06000D6C RID: 3436 RVA: 0x00066E3E File Offset: 0x0006503E
		// (set) Token: 0x06000D6D RID: 3437 RVA: 0x00066E48 File Offset: 0x00065048
		internal virtual NumericUpDown nudMaxLvWater1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x06000D6E RID: 3438 RVA: 0x00066E51 File Offset: 0x00065051
		// (set) Token: 0x06000D6F RID: 3439 RVA: 0x00066E5B File Offset: 0x0006505B
		internal virtual NumericUpDown nudMinLvWater5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x06000D70 RID: 3440 RVA: 0x00066E64 File Offset: 0x00065064
		// (set) Token: 0x06000D71 RID: 3441 RVA: 0x00066E6E File Offset: 0x0006506E
		internal virtual NumericUpDown nudMinLvWater4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x06000D72 RID: 3442 RVA: 0x00066E77 File Offset: 0x00065077
		// (set) Token: 0x06000D73 RID: 3443 RVA: 0x00066E81 File Offset: 0x00065081
		internal virtual NumericUpDown nudMinLvWater3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x06000D74 RID: 3444 RVA: 0x00066E8A File Offset: 0x0006508A
		// (set) Token: 0x06000D75 RID: 3445 RVA: 0x00066E94 File Offset: 0x00065094
		internal virtual NumericUpDown nudMinLvWater2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x06000D76 RID: 3446 RVA: 0x00066E9D File Offset: 0x0006509D
		// (set) Token: 0x06000D77 RID: 3447 RVA: 0x00066EA7 File Offset: 0x000650A7
		internal virtual NumericUpDown nudMinLvWater1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x06000D78 RID: 3448 RVA: 0x00066EB0 File Offset: 0x000650B0
		// (set) Token: 0x06000D79 RID: 3449 RVA: 0x00066EBA File Offset: 0x000650BA
		internal virtual Label lblPercentageWater5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x06000D7A RID: 3450 RVA: 0x00066EC3 File Offset: 0x000650C3
		// (set) Token: 0x06000D7B RID: 3451 RVA: 0x00066ECD File Offset: 0x000650CD
		internal virtual Label lblPercentageWater4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000510 RID: 1296
		// (get) Token: 0x06000D7C RID: 3452 RVA: 0x00066ED6 File Offset: 0x000650D6
		// (set) Token: 0x06000D7D RID: 3453 RVA: 0x00066EE0 File Offset: 0x000650E0
		internal virtual Label lblPercentageWater3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000511 RID: 1297
		// (get) Token: 0x06000D7E RID: 3454 RVA: 0x00066EE9 File Offset: 0x000650E9
		// (set) Token: 0x06000D7F RID: 3455 RVA: 0x00066EF3 File Offset: 0x000650F3
		internal virtual Label lblPercentageWater2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x06000D80 RID: 3456 RVA: 0x00066EFC File Offset: 0x000650FC
		// (set) Token: 0x06000D81 RID: 3457 RVA: 0x00066F06 File Offset: 0x00065106
		internal virtual Label lblPercentageWater1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x06000D82 RID: 3458 RVA: 0x00066F0F File Offset: 0x0006510F
		// (set) Token: 0x06000D83 RID: 3459 RVA: 0x00066F19 File Offset: 0x00065119
		internal virtual PictureBox picIconWater5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x06000D84 RID: 3460 RVA: 0x00066F22 File Offset: 0x00065122
		// (set) Token: 0x06000D85 RID: 3461 RVA: 0x00066F2C File Offset: 0x0006512C
		internal virtual ComboBox cmbPokemonWater5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x06000D86 RID: 3462 RVA: 0x00066F35 File Offset: 0x00065135
		// (set) Token: 0x06000D87 RID: 3463 RVA: 0x00066F3F File Offset: 0x0006513F
		internal virtual PictureBox picIconWater4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x06000D88 RID: 3464 RVA: 0x00066F48 File Offset: 0x00065148
		// (set) Token: 0x06000D89 RID: 3465 RVA: 0x00066F52 File Offset: 0x00065152
		internal virtual ComboBox cmbPokemonWater4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x06000D8A RID: 3466 RVA: 0x00066F5B File Offset: 0x0006515B
		// (set) Token: 0x06000D8B RID: 3467 RVA: 0x00066F65 File Offset: 0x00065165
		internal virtual PictureBox picIconWater3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x06000D8C RID: 3468 RVA: 0x00066F6E File Offset: 0x0006516E
		// (set) Token: 0x06000D8D RID: 3469 RVA: 0x00066F78 File Offset: 0x00065178
		internal virtual ComboBox cmbPokemonWater3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x06000D8E RID: 3470 RVA: 0x00066F81 File Offset: 0x00065181
		// (set) Token: 0x06000D8F RID: 3471 RVA: 0x00066F8B File Offset: 0x0006518B
		internal virtual PictureBox picIconWater2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x06000D90 RID: 3472 RVA: 0x00066F94 File Offset: 0x00065194
		// (set) Token: 0x06000D91 RID: 3473 RVA: 0x00066F9E File Offset: 0x0006519E
		internal virtual ComboBox cmbPokemonWater2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x06000D92 RID: 3474 RVA: 0x00066FA7 File Offset: 0x000651A7
		// (set) Token: 0x06000D93 RID: 3475 RVA: 0x00066FB1 File Offset: 0x000651B1
		internal virtual PictureBox picIconWater1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x06000D94 RID: 3476 RVA: 0x00066FBA File Offset: 0x000651BA
		// (set) Token: 0x06000D95 RID: 3477 RVA: 0x00066FC4 File Offset: 0x000651C4
		internal virtual ComboBox cmbPokemonWater1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x00066FCD File Offset: 0x000651CD
		// (set) Token: 0x06000D97 RID: 3479 RVA: 0x00066FD7 File Offset: 0x000651D7
		internal virtual NumericUpDown nudMaxLvRock5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x06000D98 RID: 3480 RVA: 0x00066FE0 File Offset: 0x000651E0
		// (set) Token: 0x06000D99 RID: 3481 RVA: 0x00066FEA File Offset: 0x000651EA
		internal virtual NumericUpDown nudMaxLvRock4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x06000D9A RID: 3482 RVA: 0x00066FF3 File Offset: 0x000651F3
		// (set) Token: 0x06000D9B RID: 3483 RVA: 0x00066FFD File Offset: 0x000651FD
		internal virtual NumericUpDown nudMaxLvRock3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x06000D9C RID: 3484 RVA: 0x00067006 File Offset: 0x00065206
		// (set) Token: 0x06000D9D RID: 3485 RVA: 0x00067010 File Offset: 0x00065210
		internal virtual NumericUpDown nudMaxLvRock2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x06000D9E RID: 3486 RVA: 0x00067019 File Offset: 0x00065219
		// (set) Token: 0x06000D9F RID: 3487 RVA: 0x00067023 File Offset: 0x00065223
		internal virtual NumericUpDown nudMaxLvRock1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x06000DA0 RID: 3488 RVA: 0x0006702C File Offset: 0x0006522C
		// (set) Token: 0x06000DA1 RID: 3489 RVA: 0x00067036 File Offset: 0x00065236
		internal virtual NumericUpDown nudMinLvRock5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x06000DA2 RID: 3490 RVA: 0x0006703F File Offset: 0x0006523F
		// (set) Token: 0x06000DA3 RID: 3491 RVA: 0x00067049 File Offset: 0x00065249
		internal virtual NumericUpDown nudMinLvRock4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x06000DA4 RID: 3492 RVA: 0x00067052 File Offset: 0x00065252
		// (set) Token: 0x06000DA5 RID: 3493 RVA: 0x0006705C File Offset: 0x0006525C
		internal virtual NumericUpDown nudMinLvRock3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000525 RID: 1317
		// (get) Token: 0x06000DA6 RID: 3494 RVA: 0x00067065 File Offset: 0x00065265
		// (set) Token: 0x06000DA7 RID: 3495 RVA: 0x0006706F File Offset: 0x0006526F
		internal virtual NumericUpDown nudMinLvRock2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x06000DA8 RID: 3496 RVA: 0x00067078 File Offset: 0x00065278
		// (set) Token: 0x06000DA9 RID: 3497 RVA: 0x00067082 File Offset: 0x00065282
		internal virtual NumericUpDown nudMinLvRock1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x06000DAA RID: 3498 RVA: 0x0006708B File Offset: 0x0006528B
		// (set) Token: 0x06000DAB RID: 3499 RVA: 0x00067095 File Offset: 0x00065295
		internal virtual Label lblPercentageRock5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x06000DAC RID: 3500 RVA: 0x0006709E File Offset: 0x0006529E
		// (set) Token: 0x06000DAD RID: 3501 RVA: 0x000670A8 File Offset: 0x000652A8
		internal virtual Label lblPercentageRock4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x06000DAE RID: 3502 RVA: 0x000670B1 File Offset: 0x000652B1
		// (set) Token: 0x06000DAF RID: 3503 RVA: 0x000670BB File Offset: 0x000652BB
		internal virtual Label lblPercentageRock3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x06000DB0 RID: 3504 RVA: 0x000670C4 File Offset: 0x000652C4
		// (set) Token: 0x06000DB1 RID: 3505 RVA: 0x000670CE File Offset: 0x000652CE
		internal virtual Label lblPercentageRock2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x06000DB2 RID: 3506 RVA: 0x000670D7 File Offset: 0x000652D7
		// (set) Token: 0x06000DB3 RID: 3507 RVA: 0x000670E1 File Offset: 0x000652E1
		internal virtual Label lblPercentageRock1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x06000DB4 RID: 3508 RVA: 0x000670EA File Offset: 0x000652EA
		// (set) Token: 0x06000DB5 RID: 3509 RVA: 0x000670F4 File Offset: 0x000652F4
		internal virtual PictureBox picIconRock5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x06000DB6 RID: 3510 RVA: 0x000670FD File Offset: 0x000652FD
		// (set) Token: 0x06000DB7 RID: 3511 RVA: 0x00067107 File Offset: 0x00065307
		internal virtual ComboBox cmbPokemonRock5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x06000DB8 RID: 3512 RVA: 0x00067110 File Offset: 0x00065310
		// (set) Token: 0x06000DB9 RID: 3513 RVA: 0x0006711A File Offset: 0x0006531A
		internal virtual PictureBox picIconRock4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x06000DBA RID: 3514 RVA: 0x00067123 File Offset: 0x00065323
		// (set) Token: 0x06000DBB RID: 3515 RVA: 0x0006712D File Offset: 0x0006532D
		internal virtual ComboBox cmbPokemonRock4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x06000DBC RID: 3516 RVA: 0x00067136 File Offset: 0x00065336
		// (set) Token: 0x06000DBD RID: 3517 RVA: 0x00067140 File Offset: 0x00065340
		internal virtual PictureBox picIconRock3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x06000DBE RID: 3518 RVA: 0x00067149 File Offset: 0x00065349
		// (set) Token: 0x06000DBF RID: 3519 RVA: 0x00067153 File Offset: 0x00065353
		internal virtual ComboBox cmbPokemonRock3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x0006715C File Offset: 0x0006535C
		// (set) Token: 0x06000DC1 RID: 3521 RVA: 0x00067166 File Offset: 0x00065366
		internal virtual PictureBox picIconRock2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x06000DC2 RID: 3522 RVA: 0x0006716F File Offset: 0x0006536F
		// (set) Token: 0x06000DC3 RID: 3523 RVA: 0x00067179 File Offset: 0x00065379
		internal virtual ComboBox cmbPokemonRock2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x06000DC4 RID: 3524 RVA: 0x00067182 File Offset: 0x00065382
		// (set) Token: 0x06000DC5 RID: 3525 RVA: 0x0006718C File Offset: 0x0006538C
		internal virtual PictureBox picIconRock1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x06000DC6 RID: 3526 RVA: 0x00067195 File Offset: 0x00065395
		// (set) Token: 0x06000DC7 RID: 3527 RVA: 0x0006719F File Offset: 0x0006539F
		internal virtual ComboBox cmbPokemonRock1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x06000DC8 RID: 3528 RVA: 0x000671A8 File Offset: 0x000653A8
		// (set) Token: 0x06000DC9 RID: 3529 RVA: 0x000671B2 File Offset: 0x000653B2
		internal virtual NumericUpDown nudMaxLvFishing10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x06000DCA RID: 3530 RVA: 0x000671BB File Offset: 0x000653BB
		// (set) Token: 0x06000DCB RID: 3531 RVA: 0x000671C5 File Offset: 0x000653C5
		internal virtual NumericUpDown nudMaxLvFishing9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x06000DCC RID: 3532 RVA: 0x000671CE File Offset: 0x000653CE
		// (set) Token: 0x06000DCD RID: 3533 RVA: 0x000671D8 File Offset: 0x000653D8
		internal virtual NumericUpDown nudMaxLvFishing8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x06000DCE RID: 3534 RVA: 0x000671E1 File Offset: 0x000653E1
		// (set) Token: 0x06000DCF RID: 3535 RVA: 0x000671EB File Offset: 0x000653EB
		internal virtual NumericUpDown nudMaxLvFishing7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x06000DD0 RID: 3536 RVA: 0x000671F4 File Offset: 0x000653F4
		// (set) Token: 0x06000DD1 RID: 3537 RVA: 0x000671FE File Offset: 0x000653FE
		internal virtual NumericUpDown nudMaxLvFishing6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x06000DD2 RID: 3538 RVA: 0x00067207 File Offset: 0x00065407
		// (set) Token: 0x06000DD3 RID: 3539 RVA: 0x00067211 File Offset: 0x00065411
		internal virtual NumericUpDown nudMaxLvFishing5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x06000DD4 RID: 3540 RVA: 0x0006721A File Offset: 0x0006541A
		// (set) Token: 0x06000DD5 RID: 3541 RVA: 0x00067224 File Offset: 0x00065424
		internal virtual NumericUpDown nudMaxLvFishing4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x06000DD6 RID: 3542 RVA: 0x0006722D File Offset: 0x0006542D
		// (set) Token: 0x06000DD7 RID: 3543 RVA: 0x00067237 File Offset: 0x00065437
		internal virtual NumericUpDown nudMaxLvFishing3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x06000DD8 RID: 3544 RVA: 0x00067240 File Offset: 0x00065440
		// (set) Token: 0x06000DD9 RID: 3545 RVA: 0x0006724A File Offset: 0x0006544A
		internal virtual NumericUpDown nudMaxLvFishing2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x06000DDA RID: 3546 RVA: 0x00067253 File Offset: 0x00065453
		// (set) Token: 0x06000DDB RID: 3547 RVA: 0x0006725D File Offset: 0x0006545D
		internal virtual NumericUpDown nudMaxLvFishing1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x06000DDC RID: 3548 RVA: 0x00067266 File Offset: 0x00065466
		// (set) Token: 0x06000DDD RID: 3549 RVA: 0x00067270 File Offset: 0x00065470
		internal virtual NumericUpDown nudMinLvFishing10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x06000DDE RID: 3550 RVA: 0x00067279 File Offset: 0x00065479
		// (set) Token: 0x06000DDF RID: 3551 RVA: 0x00067283 File Offset: 0x00065483
		internal virtual NumericUpDown nudMinLvFishing9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x06000DE0 RID: 3552 RVA: 0x0006728C File Offset: 0x0006548C
		// (set) Token: 0x06000DE1 RID: 3553 RVA: 0x00067296 File Offset: 0x00065496
		internal virtual NumericUpDown nudMinLvFishing8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x06000DE2 RID: 3554 RVA: 0x0006729F File Offset: 0x0006549F
		// (set) Token: 0x06000DE3 RID: 3555 RVA: 0x000672A9 File Offset: 0x000654A9
		internal virtual NumericUpDown nudMinLvFishing7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x06000DE4 RID: 3556 RVA: 0x000672B2 File Offset: 0x000654B2
		// (set) Token: 0x06000DE5 RID: 3557 RVA: 0x000672BC File Offset: 0x000654BC
		internal virtual NumericUpDown nudMinLvFishing6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x06000DE6 RID: 3558 RVA: 0x000672C5 File Offset: 0x000654C5
		// (set) Token: 0x06000DE7 RID: 3559 RVA: 0x000672CF File Offset: 0x000654CF
		internal virtual NumericUpDown nudMinLvFishing5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x06000DE8 RID: 3560 RVA: 0x000672D8 File Offset: 0x000654D8
		// (set) Token: 0x06000DE9 RID: 3561 RVA: 0x000672E2 File Offset: 0x000654E2
		internal virtual NumericUpDown nudMinLvFishing4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x06000DEA RID: 3562 RVA: 0x000672EB File Offset: 0x000654EB
		// (set) Token: 0x06000DEB RID: 3563 RVA: 0x000672F5 File Offset: 0x000654F5
		internal virtual NumericUpDown nudMinLvFishing3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x06000DEC RID: 3564 RVA: 0x000672FE File Offset: 0x000654FE
		// (set) Token: 0x06000DED RID: 3565 RVA: 0x00067308 File Offset: 0x00065508
		internal virtual NumericUpDown nudMinLvFishing2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x06000DEE RID: 3566 RVA: 0x00067311 File Offset: 0x00065511
		// (set) Token: 0x06000DEF RID: 3567 RVA: 0x0006731B File Offset: 0x0006551B
		internal virtual NumericUpDown nudMinLvFishing1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x06000DF0 RID: 3568 RVA: 0x00067324 File Offset: 0x00065524
		// (set) Token: 0x06000DF1 RID: 3569 RVA: 0x0006732E File Offset: 0x0006552E
		internal virtual Label lblPercentageFishing10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x06000DF2 RID: 3570 RVA: 0x00067337 File Offset: 0x00065537
		// (set) Token: 0x06000DF3 RID: 3571 RVA: 0x00067341 File Offset: 0x00065541
		internal virtual Label lblPercentageFishing9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x06000DF4 RID: 3572 RVA: 0x0006734A File Offset: 0x0006554A
		// (set) Token: 0x06000DF5 RID: 3573 RVA: 0x00067354 File Offset: 0x00065554
		internal virtual Label lblPercentageFishing8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x06000DF6 RID: 3574 RVA: 0x0006735D File Offset: 0x0006555D
		// (set) Token: 0x06000DF7 RID: 3575 RVA: 0x00067367 File Offset: 0x00065567
		internal virtual Label lblPercentageFishing7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x06000DF8 RID: 3576 RVA: 0x00067370 File Offset: 0x00065570
		// (set) Token: 0x06000DF9 RID: 3577 RVA: 0x0006737A File Offset: 0x0006557A
		internal virtual Label lblPercentageFishing6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x00067383 File Offset: 0x00065583
		// (set) Token: 0x06000DFB RID: 3579 RVA: 0x0006738D File Offset: 0x0006558D
		internal virtual Label lblPercentageFishing5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x06000DFC RID: 3580 RVA: 0x00067396 File Offset: 0x00065596
		// (set) Token: 0x06000DFD RID: 3581 RVA: 0x000673A0 File Offset: 0x000655A0
		internal virtual Label lblPercentageFishing4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x06000DFE RID: 3582 RVA: 0x000673A9 File Offset: 0x000655A9
		// (set) Token: 0x06000DFF RID: 3583 RVA: 0x000673B3 File Offset: 0x000655B3
		internal virtual Label lblPercentageFishing3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x06000E00 RID: 3584 RVA: 0x000673BC File Offset: 0x000655BC
		// (set) Token: 0x06000E01 RID: 3585 RVA: 0x000673C6 File Offset: 0x000655C6
		internal virtual Label lblPercentageFishing2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x06000E02 RID: 3586 RVA: 0x000673CF File Offset: 0x000655CF
		// (set) Token: 0x06000E03 RID: 3587 RVA: 0x000673D9 File Offset: 0x000655D9
		internal virtual Label lblPercentageFishing1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06000E04 RID: 3588 RVA: 0x000673E2 File Offset: 0x000655E2
		// (set) Token: 0x06000E05 RID: 3589 RVA: 0x000673EC File Offset: 0x000655EC
		internal virtual PictureBox picIconFishing10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x06000E06 RID: 3590 RVA: 0x000673F5 File Offset: 0x000655F5
		// (set) Token: 0x06000E07 RID: 3591 RVA: 0x000673FF File Offset: 0x000655FF
		internal virtual ComboBox cmbPokemonFishing10
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x06000E08 RID: 3592 RVA: 0x00067408 File Offset: 0x00065608
		// (set) Token: 0x06000E09 RID: 3593 RVA: 0x00067412 File Offset: 0x00065612
		internal virtual PictureBox picIconFishing9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x06000E0A RID: 3594 RVA: 0x0006741B File Offset: 0x0006561B
		// (set) Token: 0x06000E0B RID: 3595 RVA: 0x00067425 File Offset: 0x00065625
		internal virtual ComboBox cmbPokemonFishing9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000558 RID: 1368
		// (get) Token: 0x06000E0C RID: 3596 RVA: 0x0006742E File Offset: 0x0006562E
		// (set) Token: 0x06000E0D RID: 3597 RVA: 0x00067438 File Offset: 0x00065638
		internal virtual PictureBox picIconFishing8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x06000E0E RID: 3598 RVA: 0x00067441 File Offset: 0x00065641
		// (set) Token: 0x06000E0F RID: 3599 RVA: 0x0006744B File Offset: 0x0006564B
		internal virtual ComboBox cmbPokemonFishing8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x06000E10 RID: 3600 RVA: 0x00067454 File Offset: 0x00065654
		// (set) Token: 0x06000E11 RID: 3601 RVA: 0x0006745E File Offset: 0x0006565E
		internal virtual PictureBox picIconFishing7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x06000E12 RID: 3602 RVA: 0x00067467 File Offset: 0x00065667
		// (set) Token: 0x06000E13 RID: 3603 RVA: 0x00067471 File Offset: 0x00065671
		internal virtual ComboBox cmbPokemonFishing7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x06000E14 RID: 3604 RVA: 0x0006747A File Offset: 0x0006567A
		// (set) Token: 0x06000E15 RID: 3605 RVA: 0x00067484 File Offset: 0x00065684
		internal virtual PictureBox picIconFishing6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x06000E16 RID: 3606 RVA: 0x0006748D File Offset: 0x0006568D
		// (set) Token: 0x06000E17 RID: 3607 RVA: 0x00067497 File Offset: 0x00065697
		internal virtual ComboBox cmbPokemonFishing6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x06000E18 RID: 3608 RVA: 0x000674A0 File Offset: 0x000656A0
		// (set) Token: 0x06000E19 RID: 3609 RVA: 0x000674AA File Offset: 0x000656AA
		internal virtual PictureBox picIconFishing5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x06000E1A RID: 3610 RVA: 0x000674B3 File Offset: 0x000656B3
		// (set) Token: 0x06000E1B RID: 3611 RVA: 0x000674BD File Offset: 0x000656BD
		internal virtual ComboBox cmbPokemonFishing5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x06000E1C RID: 3612 RVA: 0x000674C6 File Offset: 0x000656C6
		// (set) Token: 0x06000E1D RID: 3613 RVA: 0x000674D0 File Offset: 0x000656D0
		internal virtual PictureBox picIconFishing4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000561 RID: 1377
		// (get) Token: 0x06000E1E RID: 3614 RVA: 0x000674D9 File Offset: 0x000656D9
		// (set) Token: 0x06000E1F RID: 3615 RVA: 0x000674E3 File Offset: 0x000656E3
		internal virtual ComboBox cmbPokemonFishing4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x06000E20 RID: 3616 RVA: 0x000674EC File Offset: 0x000656EC
		// (set) Token: 0x06000E21 RID: 3617 RVA: 0x000674F6 File Offset: 0x000656F6
		internal virtual PictureBox picIconFishing3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x06000E22 RID: 3618 RVA: 0x000674FF File Offset: 0x000656FF
		// (set) Token: 0x06000E23 RID: 3619 RVA: 0x00067509 File Offset: 0x00065709
		internal virtual ComboBox cmbPokemonFishing3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x06000E24 RID: 3620 RVA: 0x00067512 File Offset: 0x00065712
		// (set) Token: 0x06000E25 RID: 3621 RVA: 0x0006751C File Offset: 0x0006571C
		internal virtual PictureBox picIconFishing2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x06000E26 RID: 3622 RVA: 0x00067525 File Offset: 0x00065725
		// (set) Token: 0x06000E27 RID: 3623 RVA: 0x0006752F File Offset: 0x0006572F
		internal virtual ComboBox cmbPokemonFishing2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x06000E28 RID: 3624 RVA: 0x00067538 File Offset: 0x00065738
		// (set) Token: 0x06000E29 RID: 3625 RVA: 0x00067542 File Offset: 0x00065742
		internal virtual PictureBox picIconFishing1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x06000E2A RID: 3626 RVA: 0x0006754B File Offset: 0x0006574B
		// (set) Token: 0x06000E2B RID: 3627 RVA: 0x00067555 File Offset: 0x00065755
		internal virtual ComboBox cmbPokemonFishing1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x06000E2C RID: 3628 RVA: 0x0006755E File Offset: 0x0006575E
		// (set) Token: 0x06000E2D RID: 3629 RVA: 0x00067568 File Offset: 0x00065768
		internal virtual Label lblNewAreaAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x06000E2E RID: 3630 RVA: 0x00067571 File Offset: 0x00065771
		// (set) Token: 0x06000E2F RID: 3631 RVA: 0x0006757B File Offset: 0x0006577B
		internal virtual ComboBox cmbNewArea
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x06000E30 RID: 3632 RVA: 0x00067584 File Offset: 0x00065784
		// (set) Token: 0x06000E31 RID: 3633 RVA: 0x0006758E File Offset: 0x0006578E
		internal virtual TextBox txtNewAreaAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x06000E32 RID: 3634 RVA: 0x00067597 File Offset: 0x00065797
		// (set) Token: 0x06000E33 RID: 3635 RVA: 0x000675A4 File Offset: 0x000657A4
		internal virtual Button btnNewAreaData
		{
			[CompilerGenerated]
			get
			{
				return this._btnNewAreaData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnNewAreaData_Click);
				Button button = this._btnNewAreaData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnNewAreaData = value;
				button = this._btnNewAreaData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x06000E34 RID: 3636 RVA: 0x000675E7 File Offset: 0x000657E7
		// (set) Token: 0x06000E35 RID: 3637 RVA: 0x000675F1 File Offset: 0x000657F1
		internal virtual Label lblNewArea
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x06000E36 RID: 3638 RVA: 0x000675FA File Offset: 0x000657FA
		// (set) Token: 0x06000E37 RID: 3639 RVA: 0x00067604 File Offset: 0x00065804
		internal virtual GroupBox grpLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700056E RID: 1390
		// (get) Token: 0x06000E38 RID: 3640 RVA: 0x0006760D File Offset: 0x0006580D
		// (set) Token: 0x06000E39 RID: 3641 RVA: 0x00067617 File Offset: 0x00065817
		internal virtual GroupBox grpTimeLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x06000E3A RID: 3642 RVA: 0x00067620 File Offset: 0x00065820
		// (set) Token: 0x06000E3B RID: 3643 RVA: 0x0006762A File Offset: 0x0006582A
		internal virtual RadioButton rbNightLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x06000E3C RID: 3644 RVA: 0x00067633 File Offset: 0x00065833
		// (set) Token: 0x06000E3D RID: 3645 RVA: 0x0006763D File Offset: 0x0006583D
		internal virtual RadioButton rbEveningLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06000E3E RID: 3646 RVA: 0x00067646 File Offset: 0x00065846
		// (set) Token: 0x06000E3F RID: 3647 RVA: 0x00067650 File Offset: 0x00065850
		internal virtual RadioButton rbDayLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000572 RID: 1394
		// (get) Token: 0x06000E40 RID: 3648 RVA: 0x00067659 File Offset: 0x00065859
		// (set) Token: 0x06000E41 RID: 3649 RVA: 0x00067663 File Offset: 0x00065863
		internal virtual RadioButton rbMorningLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000573 RID: 1395
		// (get) Token: 0x06000E42 RID: 3650 RVA: 0x0006766C File Offset: 0x0006586C
		// (set) Token: 0x06000E43 RID: 3651 RVA: 0x00067676 File Offset: 0x00065876
		internal virtual Label lblTableIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06000E44 RID: 3652 RVA: 0x0006767F File Offset: 0x0006587F
		// (set) Token: 0x06000E45 RID: 3653 RVA: 0x00067689 File Offset: 0x00065889
		internal virtual NumericUpDown nudTableIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x06000E46 RID: 3654 RVA: 0x00067692 File Offset: 0x00065892
		// (set) Token: 0x06000E47 RID: 3655 RVA: 0x0006769C File Offset: 0x0006589C
		internal virtual Label lblMapNumberLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x06000E48 RID: 3656 RVA: 0x000676A5 File Offset: 0x000658A5
		// (set) Token: 0x06000E49 RID: 3657 RVA: 0x000676AF File Offset: 0x000658AF
		internal virtual NumericUpDown nudMapNumberLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x06000E4A RID: 3658 RVA: 0x000676B8 File Offset: 0x000658B8
		// (set) Token: 0x06000E4B RID: 3659 RVA: 0x000676C2 File Offset: 0x000658C2
		internal virtual Label lblMapBankLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000578 RID: 1400
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x000676CB File Offset: 0x000658CB
		// (set) Token: 0x06000E4D RID: 3661 RVA: 0x000676D5 File Offset: 0x000658D5
		internal virtual NumericUpDown nudMapBankLoad
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000579 RID: 1401
		// (get) Token: 0x06000E4E RID: 3662 RVA: 0x000676DE File Offset: 0x000658DE
		// (set) Token: 0x06000E4F RID: 3663 RVA: 0x000676E8 File Offset: 0x000658E8
		internal virtual Label lblBorder2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x000676F1 File Offset: 0x000658F1
		private void WildPokemonEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.InitializeControlSets();
			this.LoadAllPokemonIconData();
			this.InitializePokemonComboBoxes();
			this.LoadAllWildData();
			this.CheckAvailableTables();
			this.ResetAllAreaTabs();
			this.SetUnsavedChanges(false);
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00067734 File Offset: 0x00065934
		private void InitializeControlSets()
		{
			this.areaControls.Add(new WildPokemonEditor.AreaControlSet
			{
				Combos = new ComboBox[]
				{
					this.cmbPokemonField1, this.cmbPokemonField2, this.cmbPokemonField3, this.cmbPokemonField4, this.cmbPokemonField5, this.cmbPokemonField6, this.cmbPokemonField7, this.cmbPokemonField8, this.cmbPokemonField9, this.cmbPokemonField10,
					this.cmbPokemonField11, this.cmbPokemonField12
				},
				Icons = new PictureBox[]
				{
					this.picIconField1, this.picIconField2, this.picIconField3, this.picIconField4, this.picIconField5, this.picIconField6, this.picIconField7, this.picIconField8, this.picIconField9, this.picIconField10,
					this.picIconField11, this.picIconField12
				},
				MinLvs = new NumericUpDown[]
				{
					this.nudMinLvField1, this.nudMinLvField2, this.nudMinLvField3, this.nudMinLvField4, this.nudMinLvField5, this.nudMinLvField6, this.nudMinLvField7, this.nudMinLvField8, this.nudMinLvField9, this.nudMinLvField10,
					this.nudMinLvField11, this.nudMinLvField12
				},
				MaxLvs = new NumericUpDown[]
				{
					this.nudMaxLvField1, this.nudMaxLvField2, this.nudMaxLvField3, this.nudMaxLvField4, this.nudMaxLvField5, this.nudMaxLvField6, this.nudMaxLvField7, this.nudMaxLvField8, this.nudMaxLvField9, this.nudMaxLvField10,
					this.nudMaxLvField11, this.nudMaxLvField12
				}
			});
			this.areaControls.Add(new WildPokemonEditor.AreaControlSet
			{
				Combos = new ComboBox[] { this.cmbPokemonWater1, this.cmbPokemonWater2, this.cmbPokemonWater3, this.cmbPokemonWater4, this.cmbPokemonWater5 },
				Icons = new PictureBox[] { this.picIconWater1, this.picIconWater2, this.picIconWater3, this.picIconWater4, this.picIconWater5 },
				MinLvs = new NumericUpDown[] { this.nudMinLvWater1, this.nudMinLvWater2, this.nudMinLvWater3, this.nudMinLvWater4, this.nudMinLvWater5 },
				MaxLvs = new NumericUpDown[] { this.nudMaxLvWater1, this.nudMaxLvWater2, this.nudMaxLvWater3, this.nudMaxLvWater4, this.nudMaxLvWater5 }
			});
			this.areaControls.Add(new WildPokemonEditor.AreaControlSet
			{
				Combos = new ComboBox[] { this.cmbPokemonRock1, this.cmbPokemonRock2, this.cmbPokemonRock3, this.cmbPokemonRock4, this.cmbPokemonRock5 },
				Icons = new PictureBox[] { this.picIconRock1, this.picIconRock2, this.picIconRock3, this.picIconRock4, this.picIconRock5 },
				MinLvs = new NumericUpDown[] { this.nudMinLvRock1, this.nudMinLvRock2, this.nudMinLvRock3, this.nudMinLvRock4, this.nudMinLvRock5 },
				MaxLvs = new NumericUpDown[] { this.nudMaxLvRock1, this.nudMaxLvRock2, this.nudMaxLvRock3, this.nudMaxLvRock4, this.nudMaxLvRock5 }
			});
			this.areaControls.Add(new WildPokemonEditor.AreaControlSet
			{
				Combos = new ComboBox[] { this.cmbPokemonFishing1, this.cmbPokemonFishing2, this.cmbPokemonFishing3, this.cmbPokemonFishing4, this.cmbPokemonFishing5, this.cmbPokemonFishing6, this.cmbPokemonFishing7, this.cmbPokemonFishing8, this.cmbPokemonFishing9, this.cmbPokemonFishing10 },
				Icons = new PictureBox[] { this.picIconFishing1, this.picIconFishing2, this.picIconFishing3, this.picIconFishing4, this.picIconFishing5, this.picIconFishing6, this.picIconFishing7, this.picIconFishing8, this.picIconFishing9, this.picIconFishing10 },
				MinLvs = new NumericUpDown[] { this.nudMinLvFishing1, this.nudMinLvFishing2, this.nudMinLvFishing3, this.nudMinLvFishing4, this.nudMinLvFishing5, this.nudMinLvFishing6, this.nudMinLvFishing7, this.nudMinLvFishing8, this.nudMinLvFishing9, this.nudMinLvFishing10 },
				MaxLvs = new NumericUpDown[] { this.nudMaxLvFishing1, this.nudMaxLvFishing2, this.nudMaxLvFishing3, this.nudMaxLvFishing4, this.nudMaxLvFishing5, this.nudMaxLvFishing6, this.nudMaxLvFishing7, this.nudMaxLvFishing8, this.nudMaxLvFishing9, this.nudMaxLvFishing10 }
			});
			{
				foreach (WildPokemonEditor.AreaControlSet areaControlSet in this.areaControls)
				{
					try
					{
						foreach (NumericUpDown numericUpDown in areaControlSet.MinLvs.Concat(areaControlSet.MaxLvs))
						{
							numericUpDown.ValueChanged += delegate(object a0, EventArgs a1)
							{
								this.SetUnsavedChanges(true);
							};
						}
					}
					finally
					{
					}
				}
			}
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x00067D98 File Offset: 0x00065F98
		private void LoadAllPokemonIconData()
		{
			this.pokemonIconList.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					PokemonData pokemonData = new PokemonData(i, this.GetPokemonNameFromRom(i));
					pokemonData.IconImageAddress = BitConverter.ToUInt32(this.romData, MyProject.Forms.PokemonEditor.ICON_IMAGE_TABLE_OFFSET + i * 4) - 134217728U;
					int num2 = (int)this.romData[MyProject.Forms.PokemonEditor.ICON_PALETTE_ID_TABLE_OFFSET + i];
					pokemonData.IconPaletteId = Math.Max(0, Math.Min(num2, MyProject.Forms.PokemonEditor.ICON_PALETTE_COUNT - 1));
					this.pokemonIconList[i] = pokemonData;
				}
			}
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x00067E5C File Offset: 0x0006605C
		private void InitializePokemonComboBoxes()
		{
			List<string> list = new List<string> { "なし" };
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					list.Add(this.GetPokemonNameFromRom(i));
				}
				string[] array = list.ToArray();
				{
					foreach (WildPokemonEditor.AreaControlSet areaControlSet in this.areaControls)
					{
						foreach (ComboBox comboBox in areaControlSet.Combos)
						{
							comboBox.BeginUpdate();
							comboBox.Items.Clear();
							comboBox.Items.AddRange(array);
							comboBox.EndUpdate();
							comboBox.SelectedIndex = 0;
							this.previousComboSelections[comboBox] = 0;
							comboBox.SelectedIndexChanged += this.OnPokemonSelectionChanged;
						}
					}
				}
			}
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x00067F80 File Offset: 0x00066180
		private string GetPokemonNameFromRom(int pokemonIndex)
		{
			checked
			{
				int num = MyProject.Forms.PokemonEditor.POKEMON_NAME_OFFSET + pokemonIndex * MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH;
				byte[] array = new byte[MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num, array, 0, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH);
			}
		}

		// Token: 0x06000E55 RID: 3669 RVA: 0x00068004 File Offset: 0x00066204
		private int ComboIndexToPokemonIndex(int comboIndex)
		{
			bool flag = comboIndex <= 0;
			int num;
			if (flag)
			{
				num = 0;
			}
			else
			{
				num = checked(comboIndex - 1 + 1);
			}
			return num;
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x0006802C File Offset: 0x0006622C
		private int PokemonIndexToComboIndex(int pokemonIndex)
		{
			bool flag = pokemonIndex == 0;
			int num;
			if (flag)
			{
				num = 0;
			}
			else
			{
				num = checked(pokemonIndex - 1 + 1);
			}
			return num;
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x00068050 File Offset: 0x00066250
		private void OnPokemonSelectionChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			bool flag = this.previousComboSelections.ContainsKey(comboBox) && this.previousComboSelections[comboBox] == comboBox.SelectedIndex;
			if (!flag)
			{
				this.previousComboSelections[comboBox] = comboBox.SelectedIndex;
				PictureBox pictureBox = null;
				{
					foreach (WildPokemonEditor.AreaControlSet areaControlSet in this.areaControls)
					{
						int num = Array.IndexOf<ComboBox>(areaControlSet.Combos, comboBox);
						bool flag2 = num >= 0;
						if (flag2)
						{
							pictureBox = areaControlSet.Icons[num];
							break;
						}
					}
				}
				bool flag3 = pictureBox == null;
				if (flag3)
				{
					this.SetUnsavedChanges(true);
				}
				else
				{
					bool flag4 = comboBox.SelectedIndex == 0;
					if (flag4)
					{
						bool flag5 = pictureBox.Image != null;
						if (flag5)
						{
							pictureBox.Image.Dispose();
							pictureBox.Image = null;
						}
					}
					else
					{
						int num2 = this.ComboIndexToPokemonIndex(comboBox.SelectedIndex);
						bool flag6 = this.pokemonIconList.ContainsKey(num2);
						if (flag6)
						{
							this.DisplayPokemonIcon(pictureBox, this.pokemonIconList[num2]);
						}
						else
						{
							bool flag7 = pictureBox.Image != null;
							if (flag7)
							{
								pictureBox.Image.Dispose();
								pictureBox.Image = null;
							}
						}
					}
					this.SetUnsavedChanges(true);
				}
			}
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x000681D4 File Offset: 0x000663D4
		private void DisplayPokemonIcon(PictureBox pic, PokemonData pokemonData)
		{
			checked
			{
				int num = Math.Min(2048, this.romData.Length - (int)pokemonData.IconImageAddress);
				bool flag = num <= 0;
				if (flag)
				{
					bool flag2 = pic.Image != null;
					if (flag2)
					{
						pic.Image.Dispose();
						pic.Image = null;
					}
				}
				else
				{
					byte[] array = new byte[num - 1 + 1];
					unchecked
					{
						Array.Copy(this.romData, (long)((ulong)pokemonData.IconImageAddress), array, 0L, (long)num);
					}
					int num2 = MyProject.Forms.PokemonEditor.ICON_PALETTE_TABLE_OFFSET + pokemonData.IconPaletteId * 8;
					uint num3 = BitConverter.ToUInt32(this.romData, num2) - 134217728U;
					byte[] array2 = new byte[32];
					Array.Copy(this.romData, (int)num3, array2, 0, 32);
					Bitmap bitmap = ImageProcessor.LoadSprite(ref array, ImageProcessor.LoadPalette(array2, true), 32, 64, false);
					bool flag3 = pic.Image != null;
					if (flag3)
					{
						pic.Image.Dispose();
					}
					pic.Image = bitmap;
				}
			}
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x000682DC File Offset: 0x000664DC
		private void LoadAllWildData()
		{
			this.loadedEncounterTables.Clear();
			int[] array = new int[] { this.WILD_ENCOUNTER_TABLE_MORNING_OFFSET, this.WILD_ENCOUNTER_TABLE_DAY_OFFSET, this.WILD_ENCOUNTER_TABLE_EVENING_OFFSET, this.WILD_ENCOUNTER_TABLE_NIGHT_OFFSET };
			int num = 0;
			checked
			{
				do
				{
					int num2 = array[num];
					List<WildPokemonEditor.WildEncounterEntry> list = new List<WildPokemonEditor.WildEncounterEntry>();
					bool flag = num2 == 0;
					if (flag)
					{
						this.loadedEncounterTables[num] = list;
					}
					else
					{
						int num3 = 0;
						int num4 = num2;
						for (;;)
						{
							bool flag2 = this.romData[num4] == byte.MaxValue && this.romData[num4 + 1] == byte.MaxValue;
							if (flag2)
							{
								break;
							}
							WildPokemonEditor.WildEncounterEntry wildEncounterEntry = new WildPokemonEditor.WildEncounterEntry();
							wildEncounterEntry.TableIndex = num3;
							wildEncounterEntry.OriginalEntryAddress = num4;
							wildEncounterEntry.MapBank = this.romData[num4 + 0];
							wildEncounterEntry.MapNumber = this.romData[num4 + 1];
							int num5 = 0;
							do
							{
								int num6 = num4 + this.AREA_POINTER_OFFSETS[num5];
								int num7 = BitConverter.ToInt32(this.romData, num6);
								bool flag3 = num7 != 0;
								if (flag3)
								{
									int num8 = (int)(unchecked((long)num7) - 134217728L);
									wildEncounterEntry.Areas[num5].IsActive = true;
									wildEncounterEntry.Areas[num5].OriginalHeaderAddress = num8;
									wildEncounterEntry.Areas[num5].EncounterRate = this.romData[num8 + 0];
									int num9 = BitConverter.ToInt32(this.romData, num8 + 4);
									int num10 = (int)(unchecked((long)num9) - 134217728L);
									wildEncounterEntry.Areas[num5].OriginalDataAddress = num10;
									wildEncounterEntry.Areas[num5].Slots.Clear();
									int num11 = this.SLOT_COUNTS[num5];
									int num12 = num11 - 1;
									for (int i = 0; i <= num12; i++)
									{
										int num13 = num10 + i * 4;
										byte b = this.romData[num13 + 0];
										byte b2 = this.romData[num13 + 1];
										ushort num14 = BitConverter.ToUInt16(this.romData, num13 + 2);
										wildEncounterEntry.Areas[num5].Slots.Add(new WildPokemonEditor.WildPokemonSlot(b, b2, num14));
									}
								}
								else
								{
									this.InitializeEmptySlots(wildEncounterEntry.Areas[num5], num5);
								}
								num5++;
							}
							while (num5 <= 3);
							list.Add(wildEncounterEntry);
							num3++;
							num4 += 20;
						}
						this.loadedEncounterTables[num] = list;
					}
					num++;
				}
				while (num <= 3);
			}
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x0006855C File Offset: 0x0006675C
		private void InitializeEmptySlots(WildPokemonEditor.WildArea area, int typeIdx)
		{
			area.IsActive = false;
			area.EncounterRate = 0;
			area.Slots.Clear();
			int num = this.SLOT_COUNTS[typeIdx];
			checked
			{
				int num2 = num - 1;
				for (int i = 0; i <= num2; i++)
				{
					area.Slots.Add(new WildPokemonEditor.WildPokemonSlot(0, 0, 0));
				}
			}
		}

		// Token: 0x06000E5B RID: 3675 RVA: 0x000685B4 File Offset: 0x000667B4
		private void btnSearch_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveIfNeeded();
			if (!flag)
			{
				this.lstResult.Items.Clear();
				this.ResetAllAreaTabs();
				this.previousSelectedIndex = -1;
				int selectedSearchTimeIndex = this.GetSelectedSearchTimeIndex();
				byte b = Convert.ToByte(this.nudMapBankSearch.Value);
				byte b2 = Convert.ToByte(this.nudMapNumberSearch.Value);
				bool flag2 = false;
				this.lstResult.BeginUpdate();
				{
					foreach (WildPokemonEditor.WildEncounterEntry wildEncounterEntry in this.loadedEncounterTables[selectedSearchTimeIndex])
					{
						bool flag3 = wildEncounterEntry.MapBank == b && wildEncounterEntry.MapNumber == b2;
						if (flag3)
						{
							this.lstResult.Items.Add(string.Format("マップ({0}, {1}) [{2}]", wildEncounterEntry.MapBank, wildEncounterEntry.MapNumber, wildEncounterEntry.TableIndex));
							flag2 = true;
						}
					}
				}
				this.lstResult.EndUpdate();
				bool flag4 = !flag2;
				if (flag4)
				{
					MessageBox.Show("該当するマップが見つかりませんでした。", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x00068704 File Offset: 0x00066904
		private int GetSelectedSearchTimeIndex()
		{
			bool @checked = this.rbMorningSearch.Checked;
			int num;
			if (@checked)
			{
				num = 0;
			}
			else
			{
				bool checked2 = this.rbDaySearch.Checked;
				if (checked2)
				{
					num = 1;
				}
				else
				{
					bool checked3 = this.rbEveningSearch.Checked;
					if (checked3)
					{
						num = 2;
					}
					else
					{
						bool checked4 = this.rbNightSearch.Checked;
						if (checked4)
						{
							num = 3;
						}
						else
						{
							num = 1;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x00068768 File Offset: 0x00066968
		private int GetSelectedLoadTimeIndex()
		{
			bool @checked = this.rbMorningLoad.Checked;
			int num;
			if (@checked)
			{
				num = 0;
			}
			else
			{
				bool checked2 = this.rbDayLoad.Checked;
				if (checked2)
				{
					num = 1;
				}
				else
				{
					bool checked3 = this.rbEveningLoad.Checked;
					if (checked3)
					{
						num = 2;
					}
					else
					{
						bool checked4 = this.rbNightLoad.Checked;
						if (checked4)
						{
							num = 3;
						}
						else
						{
							num = 1;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06000E5E RID: 3678 RVA: 0x000687CC File Offset: 0x000669CC
		private void lstResult_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstResult.SelectedIndex == -1;
			if (!flag)
			{
				bool flag2 = this.lstResult.SelectedIndex == this.previousSelectedIndex;
				if (!flag2)
				{
					bool flag3 = this.hasUnsavedChanges;
					if (flag3)
					{
						DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
						if (dialogResult == DialogResult.Cancel)
						{
							this.isLoadingEntry = true;
							this.lstResult.SelectedIndex = this.previousSelectedIndex;
							this.isLoadingEntry = false;
							return;
						}
						if (dialogResult != DialogResult.Yes)
						{
							if (dialogResult == DialogResult.No)
							{
								this.RevertCurrentEntry();
								this.SetUnsavedChanges(false);
								this.LoadSelectedEntry();
							}
						}
						else
						{
							this.UpdateCurrentEntryObject();
							this.WriteToRomData();
							this.SetUnsavedChanges(false);
							this.LoadSelectedEntry();
						}
					}
					else
					{
						this.LoadSelectedEntry();
					}
					this.previousSelectedIndex = this.lstResult.SelectedIndex;
				}
			}
		}

		// Token: 0x06000E5F RID: 3679 RVA: 0x000688BC File Offset: 0x00066ABC
		private void LoadSelectedEntry()
		{
			bool flag = this.lstResult.SelectedIndex == -1 || this.lstResult.SelectedItem == null;
			if (!flag)
			{
				string text = this.lstResult.SelectedItem.ToString();
				int num = text.LastIndexOf('[');
				int num2 = text.LastIndexOf(']');
				bool flag2 = num == -1 || num2 == -1;
				if (!flag2)
				{
					string text2 = checked(text.Substring(num + 1, num2 - num - 1));
					int tableIndex;
					bool flag3 = !int.TryParse(text2, out tableIndex);
					if (!flag3)
					{
						int selectedSearchTimeIndex = this.GetSelectedSearchTimeIndex();
						bool flag4 = !this.loadedEncounterTables.ContainsKey(selectedSearchTimeIndex);
						if (!flag4)
						{
							WildPokemonEditor.WildEncounterEntry wildEncounterEntry = this.loadedEncounterTables[selectedSearchTimeIndex].FirstOrDefault((WildPokemonEditor.WildEncounterEntry x) => x.TableIndex == tableIndex);
							bool flag5 = wildEncounterEntry == null;
							if (!flag5)
							{
								this.isLoadingEntry = true;
								try
								{
									this.LoadMapEntry(wildEncounterEntry, selectedSearchTimeIndex);
								}
								finally
								{
									this.isLoadingEntry = false;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000E60 RID: 3680 RVA: 0x000689D8 File Offset: 0x00066BD8
		private void LoadMapEntry(WildPokemonEditor.WildEncounterEntry entry, int timeIndex)
		{
			this.nudMapBankLoad.Value = new decimal((int)entry.MapBank);
			this.nudMapNumberLoad.Value = new decimal((int)entry.MapNumber);
			this.nudTableIndex.Value = new decimal(entry.TableIndex);
			this.rbMorningLoad.Checked = timeIndex == 0;
			this.rbDayLoad.Checked = timeIndex == 1;
			this.rbEveningLoad.Checked = timeIndex == 2;
			this.rbNightLoad.Checked = timeIndex == 3;
			int num = 0;
			checked
			{
				do
				{
					WildPokemonEditor.WildArea wildArea = entry.Areas[num];
					WildPokemonEditor.AreaControlSet areaControlSet = this.areaControls[num];
					this.ResetAreaControls(areaControlSet, wildArea.IsActive);
					bool isActive = wildArea.IsActive;
					if (isActive)
					{
						areaControlSet.CurrentRate = (int)wildArea.EncounterRate;
						int num2 = Math.Min(wildArea.Slots.Count, areaControlSet.Combos.Length);
						int num3 = num2 - 1;
						for (int i = 0; i <= num3; i++)
						{
							WildPokemonEditor.WildPokemonSlot wildPokemonSlot = wildArea.Slots[i];
							areaControlSet.MinLvs[i].Value = Math.Max(areaControlSet.MinLvs[i].Minimum, Math.Min(areaControlSet.MinLvs[i].Maximum, new decimal((int)wildPokemonSlot.MinLevel)));
							areaControlSet.MaxLvs[i].Value = Math.Max(areaControlSet.MaxLvs[i].Minimum, Math.Min(areaControlSet.MaxLvs[i].Maximum, new decimal((int)wildPokemonSlot.MaxLevel)));
							int num4 = this.PokemonIndexToComboIndex((int)wildPokemonSlot.PokemonID);
							bool flag = num4 >= 0 && num4 <= areaControlSet.Combos[i].Items.Count - 1;
							if (flag)
							{
								areaControlSet.Combos[i].SelectedIndex = num4;
							}
							else
							{
								areaControlSet.Combos[i].SelectedIndex = 0;
							}
							bool flag2 = num4 > 0;
							if (flag2)
							{
								int num5 = this.ComboIndexToPokemonIndex(num4);
								bool flag3 = this.pokemonIconList.ContainsKey(num5);
								if (flag3)
								{
									this.DisplayPokemonIcon(areaControlSet.Icons[i], this.pokemonIconList[num5]);
								}
								else
								{
									bool flag4 = areaControlSet.Icons[i].Image != null;
									if (flag4)
									{
										areaControlSet.Icons[i].Image.Dispose();
										areaControlSet.Icons[i].Image = null;
									}
								}
							}
							else
							{
								bool flag5 = areaControlSet.Icons[i].Image != null;
								if (flag5)
								{
									areaControlSet.Icons[i].Image.Dispose();
									areaControlSet.Icons[i].Image = null;
								}
							}
						}
					}
					num++;
				}
				while (num <= 3);
				this.UpdateEncounterRateDisplay();
				this.NewAreaControls(true);
				this.SetUnsavedChanges(false);
			}
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x00068CC0 File Offset: 0x00066EC0
		private void UpdateEncounterRateDisplay()
		{
			this.nudEncounterRate.ValueChanged -= this.nudEncounterRate_ValueChanged;
			int selectedIndex = this.tabAreaData.SelectedIndex;
			bool flag = selectedIndex >= 0 && selectedIndex < this.areaControls.Count;
			if (flag)
			{
				this.nudEncounterRate.Value = new decimal(this.areaControls[selectedIndex].CurrentRate);
				this.nudEncounterRate.Enabled = this.areaControls[selectedIndex].IsEnabled;
			}
			else
			{
				this.nudEncounterRate.Value = 0m;
				this.nudEncounterRate.Enabled = false;
			}
			this.nudEncounterRate.ValueChanged += this.nudEncounterRate_ValueChanged;
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x00068D85 File Offset: 0x00066F85
		private void tabAreaData_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdateEncounterRateDisplay();
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x00068D90 File Offset: 0x00066F90
		private void nudEncounterRate_ValueChanged(object sender, EventArgs e)
		{
			int selectedIndex = this.tabAreaData.SelectedIndex;
			bool flag = selectedIndex >= 0 && selectedIndex < this.areaControls.Count;
			if (flag)
			{
				this.areaControls[selectedIndex].CurrentRate = Convert.ToInt32(this.nudEncounterRate.Value);
				this.SetUnsavedChanges(true);
			}
		}

		// Token: 0x06000E64 RID: 3684 RVA: 0x00068DF0 File Offset: 0x00066FF0
		private void ToggleAreaControls(WildPokemonEditor.AreaControlSet controls, bool isEnabled)
		{
			foreach (ComboBox comboBox in controls.Combos)
			{
				comboBox.Enabled = isEnabled;
			}
			foreach (NumericUpDown numericUpDown in controls.MinLvs)
			{
				numericUpDown.Enabled = isEnabled;
			}
			foreach (NumericUpDown numericUpDown2 in controls.MaxLvs)
			{
				numericUpDown2.Enabled = isEnabled;
			}
		}

		// Token: 0x06000E65 RID: 3685 RVA: 0x00068E88 File Offset: 0x00067088
		private void ResetAllAreaTabs()
		{
			this.nudMapBankLoad.Value = 0m;
			this.nudMapNumberLoad.Value = 0m;
			this.nudTableIndex.Value = 0m;
			this.rbMorningLoad.Checked = false;
			this.rbDayLoad.Checked = false;
			this.rbEveningLoad.Checked = false;
			this.rbNightLoad.Checked = false;
			int num = 0;
			checked
			{
				do
				{
					this.ResetAreaControls(this.areaControls[num], false);
					num++;
				}
				while (num <= 3);
				this.nudEncounterRate.Value = 0m;
				this.nudEncounterRate.Enabled = false;
				this.NewAreaControls(false);
				this.SetUnsavedChanges(false);
			}
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x00068F4C File Offset: 0x0006714C
		private void ResetAreaControls(WildPokemonEditor.AreaControlSet controls, bool isEnabled)
		{
			controls.IsEnabled = isEnabled;
			controls.CurrentRate = 0;
			this.ToggleAreaControls(controls, isEnabled);
			foreach (NumericUpDown numericUpDown in controls.MinLvs)
			{
				numericUpDown.Value = numericUpDown.Minimum;
			}
			foreach (NumericUpDown numericUpDown2 in controls.MaxLvs)
			{
				numericUpDown2.Value = numericUpDown2.Minimum;
			}
			foreach (ComboBox comboBox in controls.Combos)
			{
				comboBox.SelectedIndex = 0;
			}
			foreach (PictureBox pictureBox in controls.Icons)
			{
				bool flag = pictureBox.Image != null;
				if (flag)
				{
					pictureBox.Image.Dispose();
					pictureBox.Image = null;
				}
			}
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x0006905C File Offset: 0x0006725C
		private void NewAreaControls(bool value)
		{
			if (value)
			{
				this.cmbNewArea.SelectedIndex = 0;
				this.txtNewAreaAddress.Text = "";
				this.cmbNewArea.Enabled = true;
				this.txtNewAreaAddress.Enabled = true;
				this.btnNewAreaData.Enabled = true;
			}
			else
			{
				this.cmbNewArea.SelectedIndex = -1;
				this.txtNewAreaAddress.Text = "";
				this.cmbNewArea.Enabled = false;
				this.txtNewAreaAddress.Enabled = false;
				this.btnNewAreaData.Enabled = false;
			}
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x00069100 File Offset: 0x00067300
		private void SetUnsavedChanges(bool value)
		{
			bool flag = this.isLoadingEntry;
			if (!flag)
			{
				this.hasUnsavedChanges = value;
				this.btnSave.Enabled = value;
			}
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x0006912E File Offset: 0x0006732E
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.UpdateCurrentEntryObject();
			this.WriteToRomData();
			this.SetUnsavedChanges(false);
		}

		// Token: 0x06000E6A RID: 3690 RVA: 0x00069148 File Offset: 0x00067348
		private void UpdateCurrentEntryObject()
		{
			int num = (this.rbDayLoad.Checked ? 1 : (this.rbEveningLoad.Checked ? 2 : (this.rbNightLoad.Checked ? 3 : 0)));
			int tableIdx = Convert.ToInt32(this.nudTableIndex.Value);
			bool flag = !this.loadedEncounterTables.ContainsKey(num);
			checked
			{
				if (!flag)
				{
					WildPokemonEditor.WildEncounterEntry wildEncounterEntry = this.loadedEncounterTables[num].FirstOrDefault((WildPokemonEditor.WildEncounterEntry x) => x.TableIndex == tableIdx);
					bool flag2 = wildEncounterEntry == null;
					if (!flag2)
					{
						int num2 = 0;
						do
						{
							WildPokemonEditor.WildArea wildArea = wildEncounterEntry.Areas[num2];
							WildPokemonEditor.AreaControlSet areaControlSet = this.areaControls[num2];
							wildArea.IsActive = areaControlSet.IsEnabled;
							wildArea.EncounterRate = (byte)areaControlSet.CurrentRate;
							bool isActive = wildArea.IsActive;
							if (isActive)
							{
								int num3 = this.SLOT_COUNTS[num2];
								int num4 = num3 - 1;
								for (int i = 0; i <= num4; i++)
								{
									bool flag3 = i < wildArea.Slots.Count;
									if (flag3)
									{
										WildPokemonEditor.WildPokemonSlot wildPokemonSlot = wildArea.Slots[i];
										wildPokemonSlot.MinLevel = Convert.ToByte(areaControlSet.MinLvs[i].Value);
										wildPokemonSlot.MaxLevel = Convert.ToByte(areaControlSet.MaxLvs[i].Value);
										int selectedIndex = areaControlSet.Combos[i].SelectedIndex;
										int num5 = this.ComboIndexToPokemonIndex(selectedIndex);
										wildPokemonSlot.PokemonID = (ushort)((num5 > 0) ? num5 : 0);
									}
								}
							}
							num2++;
						}
						while (num2 <= 3);
					}
				}
			}
		}

		// Token: 0x06000E6B RID: 3691 RVA: 0x000692FC File Offset: 0x000674FC
		private void WriteToRomData()
		{
			int num = (this.rbDayLoad.Checked ? 1 : (this.rbEveningLoad.Checked ? 2 : (this.rbNightLoad.Checked ? 3 : 0)));
			int tableIdx = Convert.ToInt32(this.nudTableIndex.Value);
			bool flag = !this.loadedEncounterTables.ContainsKey(num);
			checked
			{
				if (!flag)
				{
					WildPokemonEditor.WildEncounterEntry wildEncounterEntry = this.loadedEncounterTables[num].FirstOrDefault((WildPokemonEditor.WildEncounterEntry x) => x.TableIndex == tableIdx);
					bool flag2 = wildEncounterEntry == null;
					if (!flag2)
					{
						this.romData[wildEncounterEntry.OriginalEntryAddress + 0] = wildEncounterEntry.MapBank;
						this.romData[wildEncounterEntry.OriginalEntryAddress + 1] = wildEncounterEntry.MapNumber;
						int num2 = 0;
						do
						{
							this.romData[wildEncounterEntry.OriginalEntryAddress + 1 + 1 + num2] = 0;
							num2++;
						}
						while (num2 <= 1);
						int num3 = 0;
						do
						{
							WildPokemonEditor.WildArea wildArea = wildEncounterEntry.Areas[num3];
							int num4 = wildEncounterEntry.OriginalEntryAddress + this.AREA_POINTER_OFFSETS[num3];
							bool isActive = wildArea.IsActive;
							if (isActive)
							{
								this.romData[wildArea.OriginalHeaderAddress + 0] = wildArea.EncounterRate;
								int num5 = 0;
								do
								{
									this.romData[wildArea.OriginalHeaderAddress + 0 + 1 + num5] = 0;
									num5++;
								}
								while (num5 <= 2);
								uint num6 = 134217728U + (uint)wildArea.OriginalDataAddress;
								byte[] bytes = BitConverter.GetBytes(num6);
								Array.Copy(bytes, 0, this.romData, wildArea.OriginalHeaderAddress + 4, 4);
								int num7 = wildArea.Slots.Count - 1;
								for (int i = 0; i <= num7; i++)
								{
									int num8 = wildArea.OriginalDataAddress + i * 4;
									WildPokemonEditor.WildPokemonSlot wildPokemonSlot = wildArea.Slots[i];
									this.romData[num8 + 0] = wildPokemonSlot.MinLevel;
									this.romData[num8 + 1] = wildPokemonSlot.MaxLevel;
									byte[] bytes2 = BitConverter.GetBytes(wildPokemonSlot.PokemonID);
									Array.Copy(bytes2, 0, this.romData, num8 + 2, 2);
								}
								uint num9 = 134217728U + (uint)wildArea.OriginalHeaderAddress;
								byte[] bytes3 = BitConverter.GetBytes(num9);
								Array.Copy(bytes3, 0, this.romData, num4, 4);
							}
							else
							{
								byte[] array = new byte[4];
								Array.Copy(array, 0, this.romData, num4, 4);
							}
							num3++;
						}
						while (num3 <= 3);
						List<WildPokemonEditor.WildEncounterEntry> list = this.loadedEncounterTables[num];
						bool flag3 = wildEncounterEntry.TableIndex == list.Count - 1;
						if (flag3)
						{
							int num10 = wildEncounterEntry.OriginalEntryAddress + 20;
							this.romData[num10] = byte.MaxValue;
							this.romData[num10 + 1] = byte.MaxValue;
							int num11 = 2;
							do
							{
								this.romData[num10 + num11] = 0;
								num11++;
							}
							while (num11 <= 19);
						}
						MainForm.romData = this.romData;
						this.SetUnsavedChanges(false);
					}
				}
			}
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x000695E8 File Offset: 0x000677E8
		private void WildPokemonEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = !this.ConfirmSaveIfNeeded();
			if (flag)
			{
				e.Cancel = true;
			}
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x00069610 File Offset: 0x00067810
		private void btnNewAreaData_Click(object sender, EventArgs e)
		{
			bool flag = string.IsNullOrWhiteSpace(this.txtNewAreaAddress.Text);
			checked
			{
				if (flag)
				{
					MessageBox.Show("アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					string text = this.txtNewAreaAddress.Text.Trim();
					uint num;
					bool flag2 = !uint.TryParse(text, NumberStyles.HexNumber, null, out num);
					if (flag2)
					{
						MessageBox.Show("16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					else
					{
						int selectedLoadTimeIndex = this.GetSelectedLoadTimeIndex();
						int tableIdx = Convert.ToInt32(this.nudTableIndex.Value);
						bool flag3 = !this.loadedEncounterTables.ContainsKey(selectedLoadTimeIndex);
						if (!flag3)
						{
							WildPokemonEditor.WildEncounterEntry wildEncounterEntry = this.loadedEncounterTables[selectedLoadTimeIndex].FirstOrDefault((WildPokemonEditor.WildEncounterEntry x) => x.TableIndex == tableIdx);
							bool flag4 = wildEncounterEntry == null;
							if (!flag4)
							{
								int selectedIndex = this.cmbNewArea.SelectedIndex;
								bool flag5 = selectedIndex < 0 || selectedIndex > 3;
								if (!flag5)
								{
									int num2 = Convert.ToInt32(this.txtNewAreaAddress.Text.Trim(), 16);
									int num3 = num2 + 8;
									WildPokemonEditor.WildArea wildArea = wildEncounterEntry.Areas[selectedIndex];
									wildArea.IsActive = true;
									wildArea.EncounterRate = 0;
									wildArea.OriginalHeaderAddress = num2;
									wildArea.OriginalDataAddress = num3;
									wildArea.Slots.Clear();
									int num4 = this.SLOT_COUNTS[selectedIndex];
									int num5 = num4 - 1;
									for (int i = 0; i <= num5; i++)
									{
										wildArea.Slots.Add(new WildPokemonEditor.WildPokemonSlot(0, 0, 0));
									}
									this.LoadMapEntry(wildEncounterEntry, selectedLoadTimeIndex);
									this.tabAreaData.SelectedIndex = selectedIndex;
									this.SetUnsavedChanges(true);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x000697D8 File Offset: 0x000679D8
		private void btnNewMapEntry_Click(object sender, EventArgs e)
		{
			bool flag = !this.ConfirmSaveIfNeeded();
			checked
			{
				if (!flag)
				{
					int selectedSearchTimeIndex = this.GetSelectedSearchTimeIndex();
					bool flag2 = !this.loadedEncounterTables.ContainsKey(selectedSearchTimeIndex);
					if (flag2)
					{
						this.loadedEncounterTables[selectedSearchTimeIndex] = new List<WildPokemonEditor.WildEncounterEntry>();
					}
					List<WildPokemonEditor.WildEncounterEntry> list = this.loadedEncounterTables[selectedSearchTimeIndex];
					WildPokemonEditor.WildEncounterEntry wildEncounterEntry = new WildPokemonEditor.WildEncounterEntry();
					wildEncounterEntry.TableIndex = list.Count;
					wildEncounterEntry.MapBank = Convert.ToByte(this.nudMapBankSearch.Value);
					wildEncounterEntry.MapNumber = Convert.ToByte(this.nudMapNumberSearch.Value);
					bool flag3 = list.Count > 0;
					int num;
					if (flag3)
					{
						num = list.Last<WildPokemonEditor.WildEncounterEntry>().OriginalEntryAddress + 20;
					}
					else
					{
						switch (selectedSearchTimeIndex)
						{
						case 0:
							num = this.WILD_ENCOUNTER_TABLE_MORNING_OFFSET;
							break;
						case 1:
							num = this.WILD_ENCOUNTER_TABLE_DAY_OFFSET;
							break;
						case 2:
							num = this.WILD_ENCOUNTER_TABLE_EVENING_OFFSET;
							break;
						case 3:
							num = this.WILD_ENCOUNTER_TABLE_NIGHT_OFFSET;
							break;
						default:
							num = 0;
							break;
						}
					}
					wildEncounterEntry.OriginalEntryAddress = num;
					int num2 = 0;
					do
					{
						this.InitializeEmptySlots(wildEncounterEntry.Areas[num2], num2);
						num2++;
					}
					while (num2 <= 3);
					list.Add(wildEncounterEntry);
					string text = string.Format("マップ({0}, {1}) [{2}]", wildEncounterEntry.MapBank, wildEncounterEntry.MapNumber, wildEncounterEntry.TableIndex);
					this.lstResult.Items.Add(text);
					this.previousSelectedIndex = -1;
					this.lstResult.SelectedIndex = this.lstResult.Items.Count - 1;
					this.SetUnsavedChanges(true);
				}
			}
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x00069980 File Offset: 0x00067B80
		private void RevertCurrentEntry()
		{
			int selectedLoadTimeIndex = this.GetSelectedLoadTimeIndex();
			int tableIdx = Convert.ToInt32(this.nudTableIndex.Value);
			bool flag = !this.loadedEncounterTables.ContainsKey(selectedLoadTimeIndex);
			checked
			{
				if (!flag)
				{
					List<WildPokemonEditor.WildEncounterEntry> list = this.loadedEncounterTables[selectedLoadTimeIndex];
					WildPokemonEditor.WildEncounterEntry wildEncounterEntry = list.FirstOrDefault((WildPokemonEditor.WildEncounterEntry x) => x.TableIndex == tableIdx);
					bool flag2 = wildEncounterEntry == null;
					if (!flag2)
					{
						int originalEntryAddress = wildEncounterEntry.OriginalEntryAddress;
						bool flag3 = this.romData[originalEntryAddress] == byte.MaxValue && this.romData[originalEntryAddress + 1] == byte.MaxValue;
						if (flag3)
						{
							int num = this.lstResult.Items.Count - 1;
							for (int i = num; i >= 0; i += -1)
							{
								bool flag4 = this.lstResult.Items[i].ToString().Contains(string.Format("[{0}]", wildEncounterEntry.TableIndex));
								if (flag4)
								{
									this.isLoadingEntry = true;
									this.lstResult.Items.RemoveAt(i);
									this.lstResult.SelectedIndex = -1;
									this.previousSelectedIndex = -1;
									this.isLoadingEntry = false;
									this.ResetAllAreaTabs();
									break;
								}
							}
							list.Remove(wildEncounterEntry);
						}
						else
						{
							wildEncounterEntry.MapBank = this.romData[originalEntryAddress + 0];
							wildEncounterEntry.MapNumber = this.romData[originalEntryAddress + 1];
							int num2 = 0;
							do
							{
								int num3 = originalEntryAddress + this.AREA_POINTER_OFFSETS[num2];
								int num4 = BitConverter.ToInt32(this.romData, num3);
								bool flag5 = num4 != 0;
								if (flag5)
								{
									int num5 = (int)(unchecked((long)num4) - 134217728L);
									wildEncounterEntry.Areas[num2].IsActive = true;
									wildEncounterEntry.Areas[num2].OriginalHeaderAddress = num5;
									wildEncounterEntry.Areas[num2].EncounterRate = this.romData[num5 + 0];
									int num6 = BitConverter.ToInt32(this.romData, num5 + 4);
									int num7 = (int)(unchecked((long)num6) - 134217728L);
									wildEncounterEntry.Areas[num2].OriginalDataAddress = num7;
									wildEncounterEntry.Areas[num2].Slots.Clear();
									int num8 = this.SLOT_COUNTS[num2];
									int num9 = num8 - 1;
									for (int j = 0; j <= num9; j++)
									{
										int num10 = num7 + j * 4;
										byte b = this.romData[num10 + 0];
										byte b2 = this.romData[num10 + 1];
										ushort num11 = BitConverter.ToUInt16(this.romData, num10 + 2);
										wildEncounterEntry.Areas[num2].Slots.Add(new WildPokemonEditor.WildPokemonSlot(b, b2, num11));
									}
								}
								else
								{
									this.InitializeEmptySlots(wildEncounterEntry.Areas[num2], num2);
								}
								num2++;
							}
							while (num2 <= 3);
						}
					}
				}
			}
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x00069C54 File Offset: 0x00067E54
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
							this.RevertCurrentEntry();
							this.SetUnsavedChanges(false);
							flag2 = true;
						}
					}
					else
					{
						this.UpdateCurrentEntryObject();
						this.WriteToRomData();
						this.SetUnsavedChanges(false);
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

		// Token: 0x06000E71 RID: 3697 RVA: 0x00069CD4 File Offset: 0x00067ED4
		private void CheckAvailableTables()
		{
			this.rbMorningSearch.Enabled = this.loadedEncounterTables.ContainsKey(0) && this.loadedEncounterTables[0].Count > 0;
			this.rbDaySearch.Enabled = this.loadedEncounterTables.ContainsKey(1) && this.loadedEncounterTables[1].Count > 0;
			this.rbEveningSearch.Enabled = this.loadedEncounterTables.ContainsKey(2) && this.loadedEncounterTables[2].Count > 0;
			this.rbNightSearch.Enabled = this.loadedEncounterTables.ContainsKey(3) && this.loadedEncounterTables[3].Count > 0;
			bool enabled = this.rbMorningSearch.Enabled;
			if (enabled)
			{
				this.rbMorningSearch.Checked = true;
			}
			else
			{
				bool enabled2 = this.rbDaySearch.Enabled;
				if (enabled2)
				{
					this.rbDaySearch.Checked = true;
				}
				else
				{
					bool enabled3 = this.rbEveningSearch.Enabled;
					if (enabled3)
					{
						this.rbEveningSearch.Checked = true;
					}
					else
					{
						bool enabled4 = this.rbNightSearch.Enabled;
						if (enabled4)
						{
							this.rbNightSearch.Checked = true;
						}
					}
				}
			}
		}

		// Token: 0x040007C9 RID: 1993
		private readonly bool ENABLE_DNS_WILD_ENCOUNTER;

		// Token: 0x040007CA RID: 1994
		private readonly int WILD_ENCOUNTER_TABLE_MORNING_OFFSET;

		// Token: 0x040007CB RID: 1995
		private readonly int WILD_ENCOUNTER_TABLE_DAY_OFFSET;

		// Token: 0x040007CC RID: 1996
		private readonly int WILD_ENCOUNTER_TABLE_EVENING_OFFSET;

		// Token: 0x040007CD RID: 1997
		private readonly int WILD_ENCOUNTER_TABLE_NIGHT_OFFSET;

		// Token: 0x040007CE RID: 1998
		private const int WILD_ENCOUNTER_ENTRY_LENGTH = 20;

		// Token: 0x040007CF RID: 1999
		private const int OFFSET_MAPBANK = 0;

		// Token: 0x040007D0 RID: 2000
		private const int OFFSET_MAPNUMBER = 1;

		// Token: 0x040007D1 RID: 2001
		private const int AREA_HEADER_LENGTH = 8;

		// Token: 0x040007D2 RID: 2002
		private const int OFFSET_ENCOUNTER_RATE = 0;

		// Token: 0x040007D3 RID: 2003
		private const int OFFSET_POKEMON_DATA_ADDRESS = 4;

		// Token: 0x040007D4 RID: 2004
		private const int OFFSET_POKE_MIN_LEVEL = 0;

		// Token: 0x040007D5 RID: 2005
		private const int OFFSET_POKE_MAX_LEVEL = 1;

		// Token: 0x040007D6 RID: 2006
		private const int OFFSET_POKE_CODE = 2;

		// Token: 0x040007D7 RID: 2007
		private readonly int[] SLOT_COUNTS;

		// Token: 0x040007D8 RID: 2008
		private readonly int[] AREA_POINTER_OFFSETS;

		// Token: 0x040007D9 RID: 2009
		private byte[] romData;

		// Token: 0x040007DA RID: 2010
		private bool hasUnsavedChanges;

		// Token: 0x040007DB RID: 2011
		private int previousSelectedIndex;

		// Token: 0x040007DC RID: 2012
		private bool isLoadingEntry;

		// Token: 0x040007DD RID: 2013
		private Dictionary<int, PokemonData> pokemonIconList;

		// Token: 0x040007DE RID: 2014
		private Dictionary<ComboBox, int> previousComboSelections;

		// Token: 0x040007DF RID: 2015
		private List<WildPokemonEditor.AreaControlSet> areaControls;

		// Token: 0x040007E0 RID: 2016
		private Dictionary<int, List<WildPokemonEditor.WildEncounterEntry>> loadedEncounterTables;

		// Token: 0x02000069 RID: 105
		private class AreaControlSet
		{
			// Token: 0x0600101A RID: 4122 RVA: 0x0006C8F3 File Offset: 0x0006AAF3
			public AreaControlSet()
			{
				this.IsEnabled = false;
			}

			// Token: 0x1700060A RID: 1546
			// (get) Token: 0x0600101B RID: 4123 RVA: 0x0006C904 File Offset: 0x0006AB04
			// (set) Token: 0x0600101C RID: 4124 RVA: 0x0006C90E File Offset: 0x0006AB0E
			public ComboBox[] Combos { get; set; }

			// Token: 0x1700060B RID: 1547
			// (get) Token: 0x0600101D RID: 4125 RVA: 0x0006C917 File Offset: 0x0006AB17
			// (set) Token: 0x0600101E RID: 4126 RVA: 0x0006C921 File Offset: 0x0006AB21
			public PictureBox[] Icons { get; set; }

			// Token: 0x1700060C RID: 1548
			// (get) Token: 0x0600101F RID: 4127 RVA: 0x0006C92A File Offset: 0x0006AB2A
			// (set) Token: 0x06001020 RID: 4128 RVA: 0x0006C934 File Offset: 0x0006AB34
			public NumericUpDown[] MinLvs { get; set; }

			// Token: 0x1700060D RID: 1549
			// (get) Token: 0x06001021 RID: 4129 RVA: 0x0006C93D File Offset: 0x0006AB3D
			// (set) Token: 0x06001022 RID: 4130 RVA: 0x0006C947 File Offset: 0x0006AB47
			public NumericUpDown[] MaxLvs { get; set; }

			// Token: 0x1700060E RID: 1550
			// (get) Token: 0x06001023 RID: 4131 RVA: 0x0006C950 File Offset: 0x0006AB50
			// (set) Token: 0x06001024 RID: 4132 RVA: 0x0006C95A File Offset: 0x0006AB5A
			public int CurrentRate { get; set; }

			// Token: 0x1700060F RID: 1551
			// (get) Token: 0x06001025 RID: 4133 RVA: 0x0006C963 File Offset: 0x0006AB63
			// (set) Token: 0x06001026 RID: 4134 RVA: 0x0006C96D File Offset: 0x0006AB6D
			public bool IsEnabled { get; set; }
		}

		// Token: 0x0200006A RID: 106
		private class WildPokemonSlot
		{
			// Token: 0x17000610 RID: 1552
			// (get) Token: 0x06001027 RID: 4135 RVA: 0x0006C976 File Offset: 0x0006AB76
			// (set) Token: 0x06001028 RID: 4136 RVA: 0x0006C980 File Offset: 0x0006AB80
			public byte MinLevel { get; set; }

			// Token: 0x17000611 RID: 1553
			// (get) Token: 0x06001029 RID: 4137 RVA: 0x0006C989 File Offset: 0x0006AB89
			// (set) Token: 0x0600102A RID: 4138 RVA: 0x0006C993 File Offset: 0x0006AB93
			public byte MaxLevel { get; set; }

			// Token: 0x17000612 RID: 1554
			// (get) Token: 0x0600102B RID: 4139 RVA: 0x0006C99C File Offset: 0x0006AB9C
			// (set) Token: 0x0600102C RID: 4140 RVA: 0x0006C9A6 File Offset: 0x0006ABA6
			public ushort PokemonID { get; set; }

			// Token: 0x0600102D RID: 4141 RVA: 0x0006C9AF File Offset: 0x0006ABAF
			public WildPokemonSlot(byte min, byte max, ushort id)
			{
				this.MinLevel = min;
				this.MaxLevel = max;
				this.PokemonID = id;
			}
		}

		// Token: 0x0200006B RID: 107
		private class WildArea
		{
			// Token: 0x17000613 RID: 1555
			// (get) Token: 0x0600102E RID: 4142 RVA: 0x0006C9D1 File Offset: 0x0006ABD1
			// (set) Token: 0x0600102F RID: 4143 RVA: 0x0006C9DB File Offset: 0x0006ABDB
			public int Type { get; set; }

			// Token: 0x17000614 RID: 1556
			// (get) Token: 0x06001030 RID: 4144 RVA: 0x0006C9E4 File Offset: 0x0006ABE4
			// (set) Token: 0x06001031 RID: 4145 RVA: 0x0006C9EE File Offset: 0x0006ABEE
			public byte EncounterRate { get; set; }

			// Token: 0x17000615 RID: 1557
			// (get) Token: 0x06001032 RID: 4146 RVA: 0x0006C9F7 File Offset: 0x0006ABF7
			// (set) Token: 0x06001033 RID: 4147 RVA: 0x0006CA01 File Offset: 0x0006AC01
			public List<WildPokemonEditor.WildPokemonSlot> Slots { get; set; }

			// Token: 0x17000616 RID: 1558
			// (get) Token: 0x06001034 RID: 4148 RVA: 0x0006CA0A File Offset: 0x0006AC0A
			// (set) Token: 0x06001035 RID: 4149 RVA: 0x0006CA14 File Offset: 0x0006AC14
			public bool IsActive { get; set; }

			// Token: 0x17000617 RID: 1559
			// (get) Token: 0x06001036 RID: 4150 RVA: 0x0006CA1D File Offset: 0x0006AC1D
			// (set) Token: 0x06001037 RID: 4151 RVA: 0x0006CA27 File Offset: 0x0006AC27
			public int OriginalHeaderAddress { get; set; }

			// Token: 0x17000618 RID: 1560
			// (get) Token: 0x06001038 RID: 4152 RVA: 0x0006CA30 File Offset: 0x0006AC30
			// (set) Token: 0x06001039 RID: 4153 RVA: 0x0006CA3A File Offset: 0x0006AC3A
			public int OriginalDataAddress { get; set; }

			// Token: 0x0600103A RID: 4154 RVA: 0x0006CA43 File Offset: 0x0006AC43
			public WildArea(int typeIdx)
			{
				this.Slots = new List<WildPokemonEditor.WildPokemonSlot>();
				this.IsActive = false;
				this.OriginalHeaderAddress = 0;
				this.OriginalDataAddress = 0;
				this.Type = typeIdx;
			}
		}

		// Token: 0x0200006C RID: 108
		private class WildEncounterEntry
		{
			// Token: 0x17000619 RID: 1561
			// (get) Token: 0x0600103B RID: 4155 RVA: 0x0006CA79 File Offset: 0x0006AC79
			// (set) Token: 0x0600103C RID: 4156 RVA: 0x0006CA83 File Offset: 0x0006AC83
			public int TableIndex { get; set; }

			// Token: 0x1700061A RID: 1562
			// (get) Token: 0x0600103D RID: 4157 RVA: 0x0006CA8C File Offset: 0x0006AC8C
			// (set) Token: 0x0600103E RID: 4158 RVA: 0x0006CA96 File Offset: 0x0006AC96
			public byte MapBank { get; set; }

			// Token: 0x1700061B RID: 1563
			// (get) Token: 0x0600103F RID: 4159 RVA: 0x0006CA9F File Offset: 0x0006AC9F
			// (set) Token: 0x06001040 RID: 4160 RVA: 0x0006CAA9 File Offset: 0x0006ACA9
			public byte MapNumber { get; set; }

			// Token: 0x1700061C RID: 1564
			// (get) Token: 0x06001041 RID: 4161 RVA: 0x0006CAB2 File Offset: 0x0006ACB2
			// (set) Token: 0x06001042 RID: 4162 RVA: 0x0006CABC File Offset: 0x0006ACBC
			public WildPokemonEditor.WildArea[] Areas { get; set; }

			// Token: 0x1700061D RID: 1565
			// (get) Token: 0x06001043 RID: 4163 RVA: 0x0006CAC5 File Offset: 0x0006ACC5
			// (set) Token: 0x06001044 RID: 4164 RVA: 0x0006CACF File Offset: 0x0006ACCF
			public int OriginalEntryAddress { get; set; }

			// Token: 0x06001045 RID: 4165 RVA: 0x0006CAD8 File Offset: 0x0006ACD8
			public WildEncounterEntry()
			{
				this.OriginalEntryAddress = 0;
				this.Areas = new WildPokemonEditor.WildArea[]
				{
					new WildPokemonEditor.WildArea(0),
					new WildPokemonEditor.WildArea(1),
					new WildPokemonEditor.WildArea(2),
					new WildPokemonEditor.WildArea(3)
				};
			}
		}
	}
}
