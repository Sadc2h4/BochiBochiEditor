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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x02000024 RID: 36
	public partial class PokemonEditor : Form
	{
		// Token: 0x06000942 RID: 2370 RVA: 0x00043EF4 File Offset: 0x000420F4
		public PokemonEditor()
		{
			base.Load += this.PokemonEditor_Load;
			base.FormClosing += this.PokemonEditor_FormClosing;
			this.POKEMON_NAME_OFFSET = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_OFFSET");
			this.POKEMON_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_LENGTH");
			this.TOTAL_POKEMON_COUNT = RomIniReader.ReadHexOrDecimal("TOTAL_POKEMON_COUNT");
			this.FRONT_IMAGE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("FRONT_IMAGE_TABLE_OFFSET");
			this.BACK_IMAGE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("BACK_IMAGE_TABLE_OFFSET");
			this.NORMAL_PALETTE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("NORMAL_PALETTE_TABLE_OFFSET");
			this.SHINY_PALETTE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("SHINY_PALETTE_TABLE_OFFSET");
			this.FRONT_Y_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("FRONT_Y_TABLE_OFFSET");
			this.BACK_Y_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("BACK_Y_TABLE_OFFSET");
			this.SHADOW_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("SHADOW_TABLE_OFFSET");
			this.ICON_IMAGE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ICON_IMAGE_TABLE_OFFSET");
			this.ICON_PALETTE_ID_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_ID_TABLE_OFFSET");
			this.ICON_PALETTE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_TABLE_OFFSET");
			this.ICON_PALETTE_COUNT = RomIniReader.ReadHexOrDecimal("ICON_PALETTE_COUNT");
			this.FOOTPRINT_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("FOOTPRINT_TABLE_OFFSET");
			this.NO_FOOTPRINT_START_INDEX = RomIniReader.ReadHexOrDecimal("NO_FOOTPRINT_START_INDEX");
			this.BASE_STATS_OFFSET = RomIniReader.ReadHexOrDecimal("BASE_STATS_OFFSET");
			this.ENABLE_BASE_STATS_EXPANSION = RomIniReader.ReadBoolean("ENABLE_BASE_STATS_EXPANSION");
			this.BASE_STATS_ENTRY_LENGTH = (this.ENABLE_BASE_STATS_EXPANSION ? 32 : 28);
			this.ABILITY_NAME_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ABILITY_NAME_TABLE_OFFSET");
			this.ABILITY_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("ABILITY_NAME_LENGTH");
			this.TOTAL_ABILITY_COUNT = RomIniReader.ReadHexOrDecimal("TOTAL_ABILITY_COUNT");
			this.TYPE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TYPE_TABLE_OFFSET");
			this.TYPE_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("TYPE_NAME_LENGTH");
			this.TOTAL_TYPE_COUNT = RomIniReader.ReadHexOrDecimal("TOTAL_TYPE_COUNT");
			this.LEVEL_MOVE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("LEVEL_MOVE_TABLE_OFFSET");
			this.ENABLE_MOVE_ID_EXPANSION = RomIniReader.ReadBoolean("ENABLE_MOVE_ID_EXPANSION");
			this.LEVEL_MOVE_ENTRY_LENGTH = (this.ENABLE_MOVE_ID_EXPANSION ? 3 : 2);
			this.TM_HM_LIST_OFFSET = RomIniReader.ReadHexOrDecimal("TM_HM_LIST_OFFSET");
			this.TM_HM_LEARN_OFFSET = RomIniReader.ReadHexOrDecimal("TM_HM_LEARN_OFFSET");
			this.TM_COUNT = RomIniReader.ReadHexOrDecimal("TM_COUNT");
			this.HM_COUNT = RomIniReader.ReadHexOrDecimal("HM_COUNT");
			this.MOVE_TUTOR_LIST_OFFSET = RomIniReader.ReadHexOrDecimal("MOVE_TUTOR_LIST_OFFSET");
			this.MOVE_TUTOR_LEARN_OFFSET = RomIniReader.ReadHexOrDecimal("MOVE_TUTOR_LEARN_OFFSET");
			this.MOVE_TUTOR_COUNT = RomIniReader.ReadHexOrDecimal("MOVE_TUTOR_COUNT");
			this.EVOLUTION_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("EVOLUTION_TABLE_OFFSET");
			this.EVOLUTION_SLOT_LENGTH = RomIniReader.ReadHexOrDecimal("EVOLUTION_SLOT_LENGTH");
			this.EVOLUTION_SLOT_COUNT = RomIniReader.ReadHexOrDecimal("EVOLUTION_SLOT_COUNT");
			this.POKEDEX_DATA_OFFSET = RomIniReader.ReadHexOrDecimal("POKEDEX_DATA_OFFSET");
			this.POKEDEX_DATA_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("POKEDEX_DATA_ENTRY_LENGTH");
			this.POKEDEX_CATEGORY_LENGTH = RomIniReader.ReadHexOrDecimal("POKEDEX_CATEGORY_LENGTH");
			this.ENABLE_CATEGORY_NO_SPACE = RomIniReader.ReadBoolean("ENABLE_CATEGORY_NO_SPACE");
			this.CRY_DATA_TABLE_OFFSET_1 = RomIniReader.ReadHexOrDecimal("CRY_DATA_TABLE_OFFSET_1");
			this.CRY_DATA_TABLE_OFFSET_2 = RomIniReader.ReadHexOrDecimal("CRY_DATA_TABLE_OFFSET_2");
			this.CRY_DATA_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("CRY_DATA_ENTRY_LENGTH");
			this.MAX_CRY_ID = RomIniReader.ReadHexOrDecimal("MAX_CRY_ID");
			this.EXTENDED_CRY_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("EXTENDED_CRY_TABLE_OFFSET");
			this.FIRST_EXTENDED_CRY_POKEMON_INDEX = RomIniReader.ReadHexOrDecimal("FIRST_EXTENDED_CRY_POKEMON_INDEX");
			this.ENABLE_INDEXED_CRY_TABLE = RomIniReader.ReadBoolean("ENABLE_INDEXED_CRY_TABLE");
			this.hasUnsavedChanges = false;
			this.currentPokemonIndex = 1;
			this.pokemonDataList = new Dictionary<int, PokemonData>();
			this.battleBackgroundImage = null;
			this.battleShadowImage = null;
			this.battleBubbleImage = null;
			this.evolutionMethods = new List<EvolutionMethod>();
			this.evolutionSlots = new List<EvolutionSlot>();
			this.isUpdatingEvolutionUI = false;
			this.levelMoves = new List<LevelMove>();
			this.levelMoveAddress = 0U;
			this.tmIds = new List<ushort>();
			this.hmIds = new List<ushort>();
			this.tmHmCount = 0;
			this.tmHmDataLength = 0;
			this.moveTutorIds = new List<ushort>();
			this.moveTutorCount = 0;
			this.moveTutorDataLength = 0;
			this.isUpdatingSizeCompUI = false;
			this.InitializeComponent();
		}

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x06000945 RID: 2373 RVA: 0x0004C5A0 File Offset: 0x0004A7A0
		// (set) Token: 0x06000946 RID: 2374 RVA: 0x0004C5AC File Offset: 0x0004A7AC
		internal virtual Button btnSavePokemon
		{
			[CompilerGenerated]
			get
			{
				return this._btnSavePokemon;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSavePokemon_Click);
				Button button = this._btnSavePokemon;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSavePokemon = value;
				button = this._btnSavePokemon;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x06000947 RID: 2375 RVA: 0x0004C5EF File Offset: 0x0004A7EF
		// (set) Token: 0x06000948 RID: 2376 RVA: 0x0004C5F9 File Offset: 0x0004A7F9
		internal virtual GroupBox grpSelect
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x06000949 RID: 2377 RVA: 0x0004C602 File Offset: 0x0004A802
		// (set) Token: 0x0600094A RID: 2378 RVA: 0x0004C60C File Offset: 0x0004A80C
		internal virtual TextBox txtPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x0600094B RID: 2379 RVA: 0x0004C615 File Offset: 0x0004A815
		// (set) Token: 0x0600094C RID: 2380 RVA: 0x0004C61F File Offset: 0x0004A81F
		internal virtual Label lblPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x0600094D RID: 2381 RVA: 0x0004C628 File Offset: 0x0004A828
		// (set) Token: 0x0600094E RID: 2382 RVA: 0x0004C634 File Offset: 0x0004A834
		internal virtual ComboBox cmbPokemonCode
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonCode;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPokemonCode_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPokemonCode;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonCode = value;
				comboBox = this._cmbPokemonCode;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x0600094F RID: 2383 RVA: 0x0004C677 File Offset: 0x0004A877
		// (set) Token: 0x06000950 RID: 2384 RVA: 0x0004C681 File Offset: 0x0004A881
		internal virtual GroupBox grpPokemonName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x06000951 RID: 2385 RVA: 0x0004C68A File Offset: 0x0004A88A
		// (set) Token: 0x06000952 RID: 2386 RVA: 0x0004C694 File Offset: 0x0004A894
		internal virtual Button btnChangeName
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeName_Click);
				Button button = this._btnChangeName;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeName = value;
				button = this._btnChangeName;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x06000953 RID: 2387 RVA: 0x0004C6D7 File Offset: 0x0004A8D7
		// (set) Token: 0x06000954 RID: 2388 RVA: 0x0004C6E1 File Offset: 0x0004A8E1
		internal virtual TextBox txtPokemonName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000385 RID: 901
		// (get) Token: 0x06000955 RID: 2389 RVA: 0x0004C6EA File Offset: 0x0004A8EA
		// (set) Token: 0x06000956 RID: 2390 RVA: 0x0004C6F4 File Offset: 0x0004A8F4
		internal virtual TabControl tabPokemonInfo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000386 RID: 902
		// (get) Token: 0x06000957 RID: 2391 RVA: 0x0004C6FD File Offset: 0x0004A8FD
		// (set) Token: 0x06000958 RID: 2392 RVA: 0x0004C707 File Offset: 0x0004A907
		internal virtual TabPage tabSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x06000959 RID: 2393 RVA: 0x0004C710 File Offset: 0x0004A910
		// (set) Token: 0x0600095A RID: 2394 RVA: 0x0004C71A File Offset: 0x0004A91A
		internal virtual GroupBox grpPokemonIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x0600095B RID: 2395 RVA: 0x0004C723 File Offset: 0x0004A923
		// (set) Token: 0x0600095C RID: 2396 RVA: 0x0004C730 File Offset: 0x0004A930
		internal virtual Button btnChangePokemonIcon
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokemonIcon;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokemonIcon_Click);
				Button button = this._btnChangePokemonIcon;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokemonIcon = value;
				button = this._btnChangePokemonIcon;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x0600095D RID: 2397 RVA: 0x0004C773 File Offset: 0x0004A973
		// (set) Token: 0x0600095E RID: 2398 RVA: 0x0004C780 File Offset: 0x0004A980
		internal virtual ComboBox cmbPokemonIconPal
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonIconPal;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPokemonIconPal_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPokemonIconPal;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonIconPal = value;
				comboBox = this._cmbPokemonIconPal;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x0600095F RID: 2399 RVA: 0x0004C7C3 File Offset: 0x0004A9C3
		// (set) Token: 0x06000960 RID: 2400 RVA: 0x0004C7CD File Offset: 0x0004A9CD
		internal virtual PictureBox picPokemonIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x06000961 RID: 2401 RVA: 0x0004C7D6 File Offset: 0x0004A9D6
		// (set) Token: 0x06000962 RID: 2402 RVA: 0x0004C7E0 File Offset: 0x0004A9E0
		internal virtual TextBox txtPokemonIconAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x06000963 RID: 2403 RVA: 0x0004C7E9 File Offset: 0x0004A9E9
		// (set) Token: 0x06000964 RID: 2404 RVA: 0x0004C7F3 File Offset: 0x0004A9F3
		internal virtual GroupBox grpPokemonSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x06000965 RID: 2405 RVA: 0x0004C7FC File Offset: 0x0004A9FC
		// (set) Token: 0x06000966 RID: 2406 RVA: 0x0004C808 File Offset: 0x0004AA08
		internal virtual Button btnChangePokemonSprite
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokemonSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokemonSprite_Click);
				Button button = this._btnChangePokemonSprite;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokemonSprite = value;
				button = this._btnChangePokemonSprite;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x06000967 RID: 2407 RVA: 0x0004C84B File Offset: 0x0004AA4B
		// (set) Token: 0x06000968 RID: 2408 RVA: 0x0004C855 File Offset: 0x0004AA55
		internal virtual PictureBox picBackShiny
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x06000969 RID: 2409 RVA: 0x0004C85E File Offset: 0x0004AA5E
		// (set) Token: 0x0600096A RID: 2410 RVA: 0x0004C868 File Offset: 0x0004AA68
		internal virtual PictureBox picFrontShiny
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x0600096B RID: 2411 RVA: 0x0004C871 File Offset: 0x0004AA71
		// (set) Token: 0x0600096C RID: 2412 RVA: 0x0004C87B File Offset: 0x0004AA7B
		internal virtual PictureBox picBackNormal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x0600096D RID: 2413 RVA: 0x0004C884 File Offset: 0x0004AA84
		// (set) Token: 0x0600096E RID: 2414 RVA: 0x0004C88E File Offset: 0x0004AA8E
		internal virtual PictureBox picFrontNormal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x0600096F RID: 2415 RVA: 0x0004C897 File Offset: 0x0004AA97
		// (set) Token: 0x06000970 RID: 2416 RVA: 0x0004C8A1 File Offset: 0x0004AAA1
		internal virtual TextBox txtShinyPalPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06000971 RID: 2417 RVA: 0x0004C8AA File Offset: 0x0004AAAA
		// (set) Token: 0x06000972 RID: 2418 RVA: 0x0004C8B4 File Offset: 0x0004AAB4
		internal virtual TextBox txtNormalPalPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06000973 RID: 2419 RVA: 0x0004C8BD File Offset: 0x0004AABD
		// (set) Token: 0x06000974 RID: 2420 RVA: 0x0004C8C7 File Offset: 0x0004AAC7
		internal virtual TextBox txtBackImgPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06000975 RID: 2421 RVA: 0x0004C8D0 File Offset: 0x0004AAD0
		// (set) Token: 0x06000976 RID: 2422 RVA: 0x0004C8DA File Offset: 0x0004AADA
		internal virtual TextBox txtFrontImgPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06000977 RID: 2423 RVA: 0x0004C8E3 File Offset: 0x0004AAE3
		// (set) Token: 0x06000978 RID: 2424 RVA: 0x0004C8ED File Offset: 0x0004AAED
		internal virtual TabPage tabStats
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06000979 RID: 2425 RVA: 0x0004C8F6 File Offset: 0x0004AAF6
		// (set) Token: 0x0600097A RID: 2426 RVA: 0x0004C900 File Offset: 0x0004AB00
		internal virtual TabPage tabEvolution
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x0600097B RID: 2427 RVA: 0x0004C909 File Offset: 0x0004AB09
		// (set) Token: 0x0600097C RID: 2428 RVA: 0x0004C913 File Offset: 0x0004AB13
		internal virtual TabPage tabLearn
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x0600097D RID: 2429 RVA: 0x0004C91C File Offset: 0x0004AB1C
		// (set) Token: 0x0600097E RID: 2430 RVA: 0x0004C926 File Offset: 0x0004AB26
		internal virtual TabPage tabPokedex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x0600097F RID: 2431 RVA: 0x0004C92F File Offset: 0x0004AB2F
		// (set) Token: 0x06000980 RID: 2432 RVA: 0x0004C939 File Offset: 0x0004AB39
		internal virtual GroupBox grpPokemonFootPrint
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x06000981 RID: 2433 RVA: 0x0004C942 File Offset: 0x0004AB42
		// (set) Token: 0x06000982 RID: 2434 RVA: 0x0004C94C File Offset: 0x0004AB4C
		internal virtual Button btnPokemonFootPrintExport
		{
			[CompilerGenerated]
			get
			{
				return this._btnPokemonFootPrintExport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPokemonFootPrintExport_Click);
				Button button = this._btnPokemonFootPrintExport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPokemonFootPrintExport = value;
				button = this._btnPokemonFootPrintExport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x06000983 RID: 2435 RVA: 0x0004C98F File Offset: 0x0004AB8F
		// (set) Token: 0x06000984 RID: 2436 RVA: 0x0004C99C File Offset: 0x0004AB9C
		internal virtual Button btnPokemonFootPrintImport
		{
			[CompilerGenerated]
			get
			{
				return this._btnPokemonFootPrintImport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPokemonFootPrintImport_Click);
				Button button = this._btnPokemonFootPrintImport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPokemonFootPrintImport = value;
				button = this._btnPokemonFootPrintImport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x06000985 RID: 2437 RVA: 0x0004C9DF File Offset: 0x0004ABDF
		// (set) Token: 0x06000986 RID: 2438 RVA: 0x0004C9EC File Offset: 0x0004ABEC
		internal virtual Button btnChangePokemonFootPrint
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokemonFootPrint;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokemonFootPrint_Click);
				Button button = this._btnChangePokemonFootPrint;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokemonFootPrint = value;
				button = this._btnChangePokemonFootPrint;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x06000987 RID: 2439 RVA: 0x0004CA2F File Offset: 0x0004AC2F
		// (set) Token: 0x06000988 RID: 2440 RVA: 0x0004CA39 File Offset: 0x0004AC39
		internal virtual PictureBox picPokemonFootPrint
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x06000989 RID: 2441 RVA: 0x0004CA42 File Offset: 0x0004AC42
		// (set) Token: 0x0600098A RID: 2442 RVA: 0x0004CA4C File Offset: 0x0004AC4C
		internal virtual TextBox txtPokemonFootPrintPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x0600098B RID: 2443 RVA: 0x0004CA55 File Offset: 0x0004AC55
		// (set) Token: 0x0600098C RID: 2444 RVA: 0x0004CA5F File Offset: 0x0004AC5F
		internal virtual GroupBox grpPokemonSpritePosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x0600098D RID: 2445 RVA: 0x0004CA68 File Offset: 0x0004AC68
		// (set) Token: 0x0600098E RID: 2446 RVA: 0x0004CA72 File Offset: 0x0004AC72
		internal virtual Label lblPlayerPokemonYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x0600098F RID: 2447 RVA: 0x0004CA7B File Offset: 0x0004AC7B
		// (set) Token: 0x06000990 RID: 2448 RVA: 0x0004CA88 File Offset: 0x0004AC88
		internal virtual NumericUpDown nudPlayerPokemonYPosition
		{
			[CompilerGenerated]
			get
			{
				return this._nudPlayerPokemonYPosition;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudPlayerPokemonYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPlayerPokemonYPosition = value;
				numericUpDown = this._nudPlayerPokemonYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06000991 RID: 2449 RVA: 0x0004CACB File Offset: 0x0004ACCB
		// (set) Token: 0x06000992 RID: 2450 RVA: 0x0004CAD5 File Offset: 0x0004ACD5
		internal virtual PictureBox picBattleBackGround
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x06000993 RID: 2451 RVA: 0x0004CADE File Offset: 0x0004ACDE
		// (set) Token: 0x06000994 RID: 2452 RVA: 0x0004CAE8 File Offset: 0x0004ACE8
		internal virtual GroupBox grpBaseStats
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x06000995 RID: 2453 RVA: 0x0004CAF1 File Offset: 0x0004ACF1
		// (set) Token: 0x06000996 RID: 2454 RVA: 0x0004CAFB File Offset: 0x0004ACFB
		internal virtual Label lblBaseStatsSpeed
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06000997 RID: 2455 RVA: 0x0004CB04 File Offset: 0x0004AD04
		// (set) Token: 0x06000998 RID: 2456 RVA: 0x0004CB0E File Offset: 0x0004AD0E
		internal virtual Label lblBaseStatsSpDefense
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06000999 RID: 2457 RVA: 0x0004CB17 File Offset: 0x0004AD17
		// (set) Token: 0x0600099A RID: 2458 RVA: 0x0004CB21 File Offset: 0x0004AD21
		internal virtual Label lblBaseStatsSpAttack
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x0600099B RID: 2459 RVA: 0x0004CB2A File Offset: 0x0004AD2A
		// (set) Token: 0x0600099C RID: 2460 RVA: 0x0004CB34 File Offset: 0x0004AD34
		internal virtual Label lblBaseStatsDefense
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x0600099D RID: 2461 RVA: 0x0004CB3D File Offset: 0x0004AD3D
		// (set) Token: 0x0600099E RID: 2462 RVA: 0x0004CB47 File Offset: 0x0004AD47
		internal virtual Label lblBaseStatsAttack
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x0600099F RID: 2463 RVA: 0x0004CB50 File Offset: 0x0004AD50
		// (set) Token: 0x060009A0 RID: 2464 RVA: 0x0004CB5A File Offset: 0x0004AD5A
		internal virtual Label lblBaseStatsHp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x060009A1 RID: 2465 RVA: 0x0004CB63 File Offset: 0x0004AD63
		// (set) Token: 0x060009A2 RID: 2466 RVA: 0x0004CB6D File Offset: 0x0004AD6D
		internal virtual GroupBox grpGender
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x060009A3 RID: 2467 RVA: 0x0004CB76 File Offset: 0x0004AD76
		// (set) Token: 0x060009A4 RID: 2468 RVA: 0x0004CB80 File Offset: 0x0004AD80
		internal virtual Label lblGender
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x060009A5 RID: 2469 RVA: 0x0004CB89 File Offset: 0x0004AD89
		// (set) Token: 0x060009A6 RID: 2470 RVA: 0x0004CB93 File Offset: 0x0004AD93
		internal virtual GroupBox grpEv
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x060009A7 RID: 2471 RVA: 0x0004CB9C File Offset: 0x0004AD9C
		// (set) Token: 0x060009A8 RID: 2472 RVA: 0x0004CBA6 File Offset: 0x0004ADA6
		internal virtual Label lblEggGroup1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x060009A9 RID: 2473 RVA: 0x0004CBAF File Offset: 0x0004ADAF
		// (set) Token: 0x060009AA RID: 2474 RVA: 0x0004CBB9 File Offset: 0x0004ADB9
		internal virtual Label lblEggStep
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x060009AB RID: 2475 RVA: 0x0004CBC2 File Offset: 0x0004ADC2
		// (set) Token: 0x060009AC RID: 2476 RVA: 0x0004CBCC File Offset: 0x0004ADCC
		internal virtual ComboBox cmbGender
		{
			[CompilerGenerated]
			get
			{
				return this._cmbGender;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbGender_SelectedIndexChanged);
				ComboBox comboBox = this._cmbGender;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbGender = value;
				comboBox = this._cmbGender;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x060009AD RID: 2477 RVA: 0x0004CC0F File Offset: 0x0004AE0F
		// (set) Token: 0x060009AE RID: 2478 RVA: 0x0004CC19 File Offset: 0x0004AE19
		internal virtual Label lblEggGroup2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x060009AF RID: 2479 RVA: 0x0004CC22 File Offset: 0x0004AE22
		// (set) Token: 0x060009B0 RID: 2480 RVA: 0x0004CC2C File Offset: 0x0004AE2C
		internal virtual GroupBox grpEgg
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x060009B1 RID: 2481 RVA: 0x0004CC35 File Offset: 0x0004AE35
		// (set) Token: 0x060009B2 RID: 2482 RVA: 0x0004CC40 File Offset: 0x0004AE40
		internal virtual ComboBox cmbEggGroup2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEggGroup2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EggGroup_SelectedIndexChanged);
				ComboBox comboBox = this._cmbEggGroup2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbEggGroup2 = value;
				comboBox = this._cmbEggGroup2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x060009B3 RID: 2483 RVA: 0x0004CC83 File Offset: 0x0004AE83
		// (set) Token: 0x060009B4 RID: 2484 RVA: 0x0004CC90 File Offset: 0x0004AE90
		internal virtual ComboBox cmbEggGroup1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEggGroup1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EggGroup_SelectedIndexChanged);
				ComboBox comboBox = this._cmbEggGroup1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbEggGroup1 = value;
				comboBox = this._cmbEggGroup1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x060009B5 RID: 2485 RVA: 0x0004CCD3 File Offset: 0x0004AED3
		// (set) Token: 0x060009B6 RID: 2486 RVA: 0x0004CCE0 File Offset: 0x0004AEE0
		internal virtual ComboBox cmbEggStep
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEggStep;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EggStep_SelectedIndexChanged);
				ComboBox comboBox = this._cmbEggStep;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbEggStep = value;
				comboBox = this._cmbEggStep;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x060009B7 RID: 2487 RVA: 0x0004CD23 File Offset: 0x0004AF23
		// (set) Token: 0x060009B8 RID: 2488 RVA: 0x0004CD2D File Offset: 0x0004AF2D
		internal virtual GroupBox grpOther
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x060009B9 RID: 2489 RVA: 0x0004CD36 File Offset: 0x0004AF36
		// (set) Token: 0x060009BA RID: 2490 RVA: 0x0004CD40 File Offset: 0x0004AF40
		internal virtual Label lblCatchRate
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x060009BB RID: 2491 RVA: 0x0004CD49 File Offset: 0x0004AF49
		// (set) Token: 0x060009BC RID: 2492 RVA: 0x0004CD53 File Offset: 0x0004AF53
		internal virtual Label lblGrowthRate
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x060009BD RID: 2493 RVA: 0x0004CD5C File Offset: 0x0004AF5C
		// (set) Token: 0x060009BE RID: 2494 RVA: 0x0004CD66 File Offset: 0x0004AF66
		internal virtual Label lblBaseExp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x060009BF RID: 2495 RVA: 0x0004CD6F File Offset: 0x0004AF6F
		// (set) Token: 0x060009C0 RID: 2496 RVA: 0x0004CD79 File Offset: 0x0004AF79
		internal virtual Label lblBaseHappiness
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x060009C1 RID: 2497 RVA: 0x0004CD82 File Offset: 0x0004AF82
		// (set) Token: 0x060009C2 RID: 2498 RVA: 0x0004CD8C File Offset: 0x0004AF8C
		internal virtual Label lblPokemonDirection
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x060009C3 RID: 2499 RVA: 0x0004CD95 File Offset: 0x0004AF95
		// (set) Token: 0x060009C4 RID: 2500 RVA: 0x0004CD9F File Offset: 0x0004AF9F
		internal virtual Label lblPokemonColor
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x060009C5 RID: 2501 RVA: 0x0004CDA8 File Offset: 0x0004AFA8
		// (set) Token: 0x060009C6 RID: 2502 RVA: 0x0004CDB4 File Offset: 0x0004AFB4
		internal virtual ComboBox cmbPokemonDirection
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonDirection;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				ComboBox comboBox = this._cmbPokemonDirection;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonDirection = value;
				comboBox = this._cmbPokemonDirection;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x060009C7 RID: 2503 RVA: 0x0004CDF7 File Offset: 0x0004AFF7
		// (set) Token: 0x060009C8 RID: 2504 RVA: 0x0004CE04 File Offset: 0x0004B004
		internal virtual ComboBox cmbPokemonColor
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonColor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				ComboBox comboBox = this._cmbPokemonColor;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonColor = value;
				comboBox = this._cmbPokemonColor;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x060009C9 RID: 2505 RVA: 0x0004CE47 File Offset: 0x0004B047
		// (set) Token: 0x060009CA RID: 2506 RVA: 0x0004CE54 File Offset: 0x0004B054
		internal virtual ComboBox cmbGrowthRate
		{
			[CompilerGenerated]
			get
			{
				return this._cmbGrowthRate;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				ComboBox comboBox = this._cmbGrowthRate;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbGrowthRate = value;
				comboBox = this._cmbGrowthRate;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x0004CE97 File Offset: 0x0004B097
		// (set) Token: 0x060009CC RID: 2508 RVA: 0x0004CEA1 File Offset: 0x0004B0A1
		internal virtual Label lblRunRate
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x0004CEAA File Offset: 0x0004B0AA
		// (set) Token: 0x060009CE RID: 2510 RVA: 0x0004CEB4 File Offset: 0x0004B0B4
		internal virtual GroupBox grpAbility
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x0004CEBD File Offset: 0x0004B0BD
		// (set) Token: 0x060009D0 RID: 2512 RVA: 0x0004CEC8 File Offset: 0x0004B0C8
		internal virtual ComboBox cmbAbilityHidden
		{
			[CompilerGenerated]
			get
			{
				return this._cmbAbilityHidden;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.Abilities_SelectedIndexChanged);
				ComboBox comboBox = this._cmbAbilityHidden;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbAbilityHidden = value;
				comboBox = this._cmbAbilityHidden;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x060009D1 RID: 2513 RVA: 0x0004CF0B File Offset: 0x0004B10B
		// (set) Token: 0x060009D2 RID: 2514 RVA: 0x0004CF18 File Offset: 0x0004B118
		internal virtual ComboBox cmbAbility2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbAbility2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.Abilities_SelectedIndexChanged);
				ComboBox comboBox = this._cmbAbility2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbAbility2 = value;
				comboBox = this._cmbAbility2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x060009D3 RID: 2515 RVA: 0x0004CF5B File Offset: 0x0004B15B
		// (set) Token: 0x060009D4 RID: 2516 RVA: 0x0004CF68 File Offset: 0x0004B168
		internal virtual ComboBox cmbAbility1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbAbility1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.Abilities_SelectedIndexChanged);
				ComboBox comboBox = this._cmbAbility1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbAbility1 = value;
				comboBox = this._cmbAbility1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x060009D5 RID: 2517 RVA: 0x0004CFAB File Offset: 0x0004B1AB
		// (set) Token: 0x060009D6 RID: 2518 RVA: 0x0004CFB5 File Offset: 0x0004B1B5
		internal virtual Label lblAbilityHidden
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x060009D7 RID: 2519 RVA: 0x0004CFBE File Offset: 0x0004B1BE
		// (set) Token: 0x060009D8 RID: 2520 RVA: 0x0004CFC8 File Offset: 0x0004B1C8
		internal virtual Label lblAbility2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x060009D9 RID: 2521 RVA: 0x0004CFD1 File Offset: 0x0004B1D1
		// (set) Token: 0x060009DA RID: 2522 RVA: 0x0004CFDB File Offset: 0x0004B1DB
		internal virtual Label lblAbility1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x060009DB RID: 2523 RVA: 0x0004CFE4 File Offset: 0x0004B1E4
		// (set) Token: 0x060009DC RID: 2524 RVA: 0x0004CFEE File Offset: 0x0004B1EE
		internal virtual GroupBox grpHoldItem
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x060009DD RID: 2525 RVA: 0x0004CFF7 File Offset: 0x0004B1F7
		// (set) Token: 0x060009DE RID: 2526 RVA: 0x0004D001 File Offset: 0x0004B201
		internal virtual PictureBox picHoldItem1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x060009DF RID: 2527 RVA: 0x0004D00A File Offset: 0x0004B20A
		// (set) Token: 0x060009E0 RID: 2528 RVA: 0x0004D014 File Offset: 0x0004B214
		internal virtual ComboBox cmbHoldItem1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbHoldItem1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbHoldItem_SelectedIndexChanged);
				ComboBox comboBox = this._cmbHoldItem1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbHoldItem1 = value;
				comboBox = this._cmbHoldItem1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x060009E1 RID: 2529 RVA: 0x0004D057 File Offset: 0x0004B257
		// (set) Token: 0x060009E2 RID: 2530 RVA: 0x0004D061 File Offset: 0x0004B261
		internal virtual Label lblHoldItem2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x060009E3 RID: 2531 RVA: 0x0004D06A File Offset: 0x0004B26A
		// (set) Token: 0x060009E4 RID: 2532 RVA: 0x0004D074 File Offset: 0x0004B274
		internal virtual Label lblHoldItem1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x060009E5 RID: 2533 RVA: 0x0004D07D File Offset: 0x0004B27D
		// (set) Token: 0x060009E6 RID: 2534 RVA: 0x0004D087 File Offset: 0x0004B287
		internal virtual PictureBox picHoldItem2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x060009E7 RID: 2535 RVA: 0x0004D090 File Offset: 0x0004B290
		// (set) Token: 0x060009E8 RID: 2536 RVA: 0x0004D09C File Offset: 0x0004B29C
		internal virtual ComboBox cmbHoldItem2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbHoldItem2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbHoldItem_SelectedIndexChanged);
				ComboBox comboBox = this._cmbHoldItem2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbHoldItem2 = value;
				comboBox = this._cmbHoldItem2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x060009E9 RID: 2537 RVA: 0x0004D0DF File Offset: 0x0004B2DF
		// (set) Token: 0x060009EA RID: 2538 RVA: 0x0004D0E9 File Offset: 0x0004B2E9
		internal virtual GroupBox grpPokemonType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x060009EB RID: 2539 RVA: 0x0004D0F2 File Offset: 0x0004B2F2
		// (set) Token: 0x060009EC RID: 2540 RVA: 0x0004D0FC File Offset: 0x0004B2FC
		internal virtual Label lblType2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x060009ED RID: 2541 RVA: 0x0004D105 File Offset: 0x0004B305
		// (set) Token: 0x060009EE RID: 2542 RVA: 0x0004D110 File Offset: 0x0004B310
		internal virtual ComboBox cmbType1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbType1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.Types_SelectedIndexChanged);
				ComboBox comboBox = this._cmbType1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbType1 = value;
				comboBox = this._cmbType1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x060009EF RID: 2543 RVA: 0x0004D153 File Offset: 0x0004B353
		// (set) Token: 0x060009F0 RID: 2544 RVA: 0x0004D15D File Offset: 0x0004B35D
		internal virtual Label lblType1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x060009F1 RID: 2545 RVA: 0x0004D166 File Offset: 0x0004B366
		// (set) Token: 0x060009F2 RID: 2546 RVA: 0x0004D170 File Offset: 0x0004B370
		internal virtual ComboBox cmbType2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbType2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.Types_SelectedIndexChanged);
				ComboBox comboBox = this._cmbType2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbType2 = value;
				comboBox = this._cmbType2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x060009F3 RID: 2547 RVA: 0x0004D1B3 File Offset: 0x0004B3B3
		// (set) Token: 0x060009F4 RID: 2548 RVA: 0x0004D1BD File Offset: 0x0004B3BD
		internal virtual GroupBox grpMoveTutorList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x060009F5 RID: 2549 RVA: 0x0004D1C6 File Offset: 0x0004B3C6
		// (set) Token: 0x060009F6 RID: 2550 RVA: 0x0004D1D0 File Offset: 0x0004B3D0
		internal virtual CheckedListBox clbMoveTutorList
		{
			[CompilerGenerated]
			get
			{
				return this._clbMoveTutorList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ItemCheckEventHandler itemCheckEventHandler = new ItemCheckEventHandler(this.clbMoveTutorList_ItemCheck);
				CheckedListBox checkedListBox = this._clbMoveTutorList;
				if (checkedListBox != null)
				{
					checkedListBox.ItemCheck -= itemCheckEventHandler;
				}
				this._clbMoveTutorList = value;
				checkedListBox = this._clbMoveTutorList;
				if (checkedListBox != null)
				{
					checkedListBox.ItemCheck += itemCheckEventHandler;
				}
			}
		}

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x060009F7 RID: 2551 RVA: 0x0004D213 File Offset: 0x0004B413
		// (set) Token: 0x060009F8 RID: 2552 RVA: 0x0004D21D File Offset: 0x0004B41D
		internal virtual GroupBox grpTmHmList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x060009F9 RID: 2553 RVA: 0x0004D226 File Offset: 0x0004B426
		// (set) Token: 0x060009FA RID: 2554 RVA: 0x0004D230 File Offset: 0x0004B430
		internal virtual CheckedListBox clbTmHmList
		{
			[CompilerGenerated]
			get
			{
				return this._clbTmHmList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				ItemCheckEventHandler itemCheckEventHandler = new ItemCheckEventHandler(this.clbTmHmList_ItemCheck);
				CheckedListBox checkedListBox = this._clbTmHmList;
				if (checkedListBox != null)
				{
					checkedListBox.ItemCheck -= itemCheckEventHandler;
				}
				this._clbTmHmList = value;
				checkedListBox = this._clbTmHmList;
				if (checkedListBox != null)
				{
					checkedListBox.ItemCheck += itemCheckEventHandler;
				}
			}
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x060009FB RID: 2555 RVA: 0x0004D273 File Offset: 0x0004B473
		// (set) Token: 0x060009FC RID: 2556 RVA: 0x0004D27D File Offset: 0x0004B47D
		internal virtual GroupBox groLevelMoveList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x060009FD RID: 2557 RVA: 0x0004D286 File Offset: 0x0004B486
		// (set) Token: 0x060009FE RID: 2558 RVA: 0x0004D290 File Offset: 0x0004B490
		internal virtual Button btnChangeMove
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeMove;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeMove_Click);
				Button button = this._btnChangeMove;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeMove = value;
				button = this._btnChangeMove;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x060009FF RID: 2559 RVA: 0x0004D2D3 File Offset: 0x0004B4D3
		// (set) Token: 0x06000A00 RID: 2560 RVA: 0x0004D2E0 File Offset: 0x0004B4E0
		internal virtual ListBox lstLevelMoveList
		{
			[CompilerGenerated]
			get
			{
				return this._lstLevelMoveList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstLevelMoveList_SelectedIndexChanged);
				ListBox listBox = this._lstLevelMoveList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstLevelMoveList = value;
				listBox = this._lstLevelMoveList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06000A01 RID: 2561 RVA: 0x0004D323 File Offset: 0x0004B523
		// (set) Token: 0x06000A02 RID: 2562 RVA: 0x0004D32D File Offset: 0x0004B52D
		internal virtual GroupBox grpLevelMoveTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06000A03 RID: 2563 RVA: 0x0004D336 File Offset: 0x0004B536
		// (set) Token: 0x06000A04 RID: 2564 RVA: 0x0004D340 File Offset: 0x0004B540
		internal virtual Button btnLevelMoveTableAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnLevelMoveTableAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnLevelMoveTableAddress_Click);
				Button button = this._btnLevelMoveTableAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnLevelMoveTableAddress = value;
				button = this._btnLevelMoveTableAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x0004D383 File Offset: 0x0004B583
		// (set) Token: 0x06000A06 RID: 2566 RVA: 0x0004D38D File Offset: 0x0004B58D
		internal virtual TextBox lblLevelMoveTableAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06000A07 RID: 2567 RVA: 0x0004D396 File Offset: 0x0004B596
		// (set) Token: 0x06000A08 RID: 2568 RVA: 0x0004D3A0 File Offset: 0x0004B5A0
		internal virtual Button btnChangeLevelMoveNumber
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeLevelMoveNumber;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeLevelMoveNumber_Click);
				Button button = this._btnChangeLevelMoveNumber;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeLevelMoveNumber = value;
				button = this._btnChangeLevelMoveNumber;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06000A09 RID: 2569 RVA: 0x0004D3E3 File Offset: 0x0004B5E3
		// (set) Token: 0x06000A0A RID: 2570 RVA: 0x0004D3ED File Offset: 0x0004B5ED
		internal virtual GroupBox grpEvolutionCondition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x06000A0B RID: 2571 RVA: 0x0004D3F6 File Offset: 0x0004B5F6
		// (set) Token: 0x06000A0C RID: 2572 RVA: 0x0004D400 File Offset: 0x0004B600
		internal virtual ListBox lstEvolutionSlot
		{
			[CompilerGenerated]
			get
			{
				return this._lstEvolutionSlot;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstEvolutionSlot_SelectedIndexChanged);
				ListBox listBox = this._lstEvolutionSlot;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstEvolutionSlot = value;
				listBox = this._lstEvolutionSlot;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x06000A0D RID: 2573 RVA: 0x0004D443 File Offset: 0x0004B643
		// (set) Token: 0x06000A0E RID: 2574 RVA: 0x0004D450 File Offset: 0x0004B650
		internal virtual ComboBox cmbEvolutionMethod
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEvolutionMethod;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbEvolutionMethod_SelectedIndexChanged);
				EventHandler eventHandler2 = new EventHandler(this.EvolutionControls_ValueChanged);
				ComboBox comboBox = this._cmbEvolutionMethod;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
					comboBox.SelectedIndexChanged -= eventHandler2;
				}
				this._cmbEvolutionMethod = value;
				comboBox = this._cmbEvolutionMethod;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
					comboBox.SelectedIndexChanged += eventHandler2;
				}
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x06000A0F RID: 2575 RVA: 0x0004D4AE File Offset: 0x0004B6AE
		// (set) Token: 0x06000A10 RID: 2576 RVA: 0x0004D4B8 File Offset: 0x0004B6B8
		internal virtual Label lblEvolutionMethod
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x06000A11 RID: 2577 RVA: 0x0004D4C1 File Offset: 0x0004B6C1
		// (set) Token: 0x06000A12 RID: 2578 RVA: 0x0004D4CB File Offset: 0x0004B6CB
		internal virtual TextBox txtParameter1Description
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x06000A13 RID: 2579 RVA: 0x0004D4D4 File Offset: 0x0004B6D4
		// (set) Token: 0x06000A14 RID: 2580 RVA: 0x0004D4DE File Offset: 0x0004B6DE
		internal virtual Label lblParameter1Description
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x06000A15 RID: 2581 RVA: 0x0004D4E7 File Offset: 0x0004B6E7
		// (set) Token: 0x06000A16 RID: 2582 RVA: 0x0004D4F1 File Offset: 0x0004B6F1
		internal virtual GroupBox groParamter2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x06000A17 RID: 2583 RVA: 0x0004D4FA File Offset: 0x0004B6FA
		// (set) Token: 0x06000A18 RID: 2584 RVA: 0x0004D504 File Offset: 0x0004B704
		internal virtual GroupBox grpParameter1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06000A19 RID: 2585 RVA: 0x0004D50D File Offset: 0x0004B70D
		// (set) Token: 0x06000A1A RID: 2586 RVA: 0x0004D517 File Offset: 0x0004B717
		internal virtual GroupBox grpEvolveTo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06000A1B RID: 2587 RVA: 0x0004D520 File Offset: 0x0004B720
		// (set) Token: 0x06000A1C RID: 2588 RVA: 0x0004D52C File Offset: 0x0004B72C
		internal virtual ComboBox cmbEvolveTo
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEvolveTo;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EvolutionControls_ValueChanged);
				ComboBox comboBox = this._cmbEvolveTo;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbEvolveTo = value;
				comboBox = this._cmbEvolveTo;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06000A1D RID: 2589 RVA: 0x0004D56F File Offset: 0x0004B76F
		// (set) Token: 0x06000A1E RID: 2590 RVA: 0x0004D579 File Offset: 0x0004B779
		internal virtual PictureBox picEvolveTo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06000A1F RID: 2591 RVA: 0x0004D582 File Offset: 0x0004B782
		// (set) Token: 0x06000A20 RID: 2592 RVA: 0x0004D58C File Offset: 0x0004B78C
		internal virtual GroupBox grpParameterAssist
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06000A21 RID: 2593 RVA: 0x0004D595 File Offset: 0x0004B795
		// (set) Token: 0x06000A22 RID: 2594 RVA: 0x0004D5A0 File Offset: 0x0004B7A0
		internal virtual RadioButton rbParameterAssistMoveName
		{
			[CompilerGenerated]
			get
			{
				return this._rbParameterAssistMoveName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbParameterAssist_CheckedChanged);
				RadioButton radioButton = this._rbParameterAssistMoveName;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbParameterAssistMoveName = value;
				radioButton = this._rbParameterAssistMoveName;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x06000A23 RID: 2595 RVA: 0x0004D5E3 File Offset: 0x0004B7E3
		// (set) Token: 0x06000A24 RID: 2596 RVA: 0x0004D5F0 File Offset: 0x0004B7F0
		internal virtual RadioButton rbParameterAssistItem
		{
			[CompilerGenerated]
			get
			{
				return this._rbParameterAssistItem;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbParameterAssist_CheckedChanged);
				RadioButton radioButton = this._rbParameterAssistItem;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbParameterAssistItem = value;
				radioButton = this._rbParameterAssistItem;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x06000A25 RID: 2597 RVA: 0x0004D633 File Offset: 0x0004B833
		// (set) Token: 0x06000A26 RID: 2598 RVA: 0x0004D640 File Offset: 0x0004B840
		internal virtual RadioButton rbParameterAssistType
		{
			[CompilerGenerated]
			get
			{
				return this._rbParameterAssistType;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbParameterAssist_CheckedChanged);
				RadioButton radioButton = this._rbParameterAssistType;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbParameterAssistType = value;
				radioButton = this._rbParameterAssistType;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06000A27 RID: 2599 RVA: 0x0004D683 File Offset: 0x0004B883
		// (set) Token: 0x06000A28 RID: 2600 RVA: 0x0004D690 File Offset: 0x0004B890
		internal virtual NumericUpDown nudParameter2B
		{
			[CompilerGenerated]
			get
			{
				return this._nudParameter2B;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EvolutionControls_ValueChanged);
				NumericUpDown numericUpDown = this._nudParameter2B;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudParameter2B = value;
				numericUpDown = this._nudParameter2B;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06000A29 RID: 2601 RVA: 0x0004D6D3 File Offset: 0x0004B8D3
		// (set) Token: 0x06000A2A RID: 2602 RVA: 0x0004D6E0 File Offset: 0x0004B8E0
		internal virtual NumericUpDown nudParameter2A
		{
			[CompilerGenerated]
			get
			{
				return this._nudParameter2A;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EvolutionControls_ValueChanged);
				NumericUpDown numericUpDown = this._nudParameter2A;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudParameter2A = value;
				numericUpDown = this._nudParameter2A;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x06000A2B RID: 2603 RVA: 0x0004D723 File Offset: 0x0004B923
		// (set) Token: 0x06000A2C RID: 2604 RVA: 0x0004D730 File Offset: 0x0004B930
		internal virtual NumericUpDown nudParameter1B
		{
			[CompilerGenerated]
			get
			{
				return this._nudParameter1B;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EvolutionControls_ValueChanged);
				NumericUpDown numericUpDown = this._nudParameter1B;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudParameter1B = value;
				numericUpDown = this._nudParameter1B;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x06000A2D RID: 2605 RVA: 0x0004D773 File Offset: 0x0004B973
		// (set) Token: 0x06000A2E RID: 2606 RVA: 0x0004D780 File Offset: 0x0004B980
		internal virtual NumericUpDown nudParameter1A
		{
			[CompilerGenerated]
			get
			{
				return this._nudParameter1A;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EvolutionControls_ValueChanged);
				NumericUpDown numericUpDown = this._nudParameter1A;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudParameter1A = value;
				numericUpDown = this._nudParameter1A;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x06000A2F RID: 2607 RVA: 0x0004D7C3 File Offset: 0x0004B9C3
		// (set) Token: 0x06000A30 RID: 2608 RVA: 0x0004D7CD File Offset: 0x0004B9CD
		internal virtual ComboBox cmbParameterAssistType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06000A31 RID: 2609 RVA: 0x0004D7D6 File Offset: 0x0004B9D6
		// (set) Token: 0x06000A32 RID: 2610 RVA: 0x0004D7E0 File Offset: 0x0004B9E0
		internal virtual ComboBox cmbParameterAssistPokemon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06000A33 RID: 2611 RVA: 0x0004D7E9 File Offset: 0x0004B9E9
		// (set) Token: 0x06000A34 RID: 2612 RVA: 0x0004D7F4 File Offset: 0x0004B9F4
		internal virtual RadioButton rbParameterAssistPokemon
		{
			[CompilerGenerated]
			get
			{
				return this._rbParameterAssistPokemon;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbParameterAssist_CheckedChanged);
				RadioButton radioButton = this._rbParameterAssistPokemon;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbParameterAssistPokemon = value;
				radioButton = this._rbParameterAssistPokemon;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06000A35 RID: 2613 RVA: 0x0004D837 File Offset: 0x0004BA37
		// (set) Token: 0x06000A36 RID: 2614 RVA: 0x0004D841 File Offset: 0x0004BA41
		internal virtual ComboBox cmbParameterAssistMoveName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06000A37 RID: 2615 RVA: 0x0004D84A File Offset: 0x0004BA4A
		// (set) Token: 0x06000A38 RID: 2616 RVA: 0x0004D854 File Offset: 0x0004BA54
		internal virtual ComboBox cmbParameterAssistItem
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06000A39 RID: 2617 RVA: 0x0004D85D File Offset: 0x0004BA5D
		// (set) Token: 0x06000A3A RID: 2618 RVA: 0x0004D868 File Offset: 0x0004BA68
		internal virtual Button btnWriteParameter1
		{
			[CompilerGenerated]
			get
			{
				return this._btnWriteParameter1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnWriteParameter1_Click);
				Button button = this._btnWriteParameter1;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnWriteParameter1 = value;
				button = this._btnWriteParameter1;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x06000A3B RID: 2619 RVA: 0x0004D8AB File Offset: 0x0004BAAB
		// (set) Token: 0x06000A3C RID: 2620 RVA: 0x0004D8B8 File Offset: 0x0004BAB8
		internal virtual Button btnWriteParameter2
		{
			[CompilerGenerated]
			get
			{
				return this._btnWriteParameter2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnWriteParameter2_Click);
				Button button = this._btnWriteParameter2;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnWriteParameter2 = value;
				button = this._btnWriteParameter2;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06000A3D RID: 2621 RVA: 0x0004D8FB File Offset: 0x0004BAFB
		// (set) Token: 0x06000A3E RID: 2622 RVA: 0x0004D905 File Offset: 0x0004BB05
		internal virtual TextBox txtParameter2Description
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06000A3F RID: 2623 RVA: 0x0004D90E File Offset: 0x0004BB0E
		// (set) Token: 0x06000A40 RID: 2624 RVA: 0x0004D918 File Offset: 0x0004BB18
		internal virtual Label lblParameter2Description
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06000A41 RID: 2625 RVA: 0x0004D921 File Offset: 0x0004BB21
		// (set) Token: 0x06000A42 RID: 2626 RVA: 0x0004D92B File Offset: 0x0004BB2B
		internal virtual GroupBox grpHeightWeight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x0004D934 File Offset: 0x0004BB34
		// (set) Token: 0x06000A44 RID: 2628 RVA: 0x0004D940 File Offset: 0x0004BB40
		internal virtual NumericUpDown nudWeight
		{
			[CompilerGenerated]
			get
			{
				return this._nudWeight;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudWeight_ValueChanged);
				NumericUpDown numericUpDown = this._nudWeight;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudWeight = value;
				numericUpDown = this._nudWeight;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06000A45 RID: 2629 RVA: 0x0004D983 File Offset: 0x0004BB83
		// (set) Token: 0x06000A46 RID: 2630 RVA: 0x0004D990 File Offset: 0x0004BB90
		internal virtual NumericUpDown nudHeight
		{
			[CompilerGenerated]
			get
			{
				return this._nudHeight;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudHeight_ValueChanged);
				NumericUpDown numericUpDown = this._nudHeight;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudHeight = value;
				numericUpDown = this._nudHeight;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x0004D9D3 File Offset: 0x0004BBD3
		// (set) Token: 0x06000A48 RID: 2632 RVA: 0x0004D9DD File Offset: 0x0004BBDD
		internal virtual Label lblWeight2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06000A49 RID: 2633 RVA: 0x0004D9E6 File Offset: 0x0004BBE6
		// (set) Token: 0x06000A4A RID: 2634 RVA: 0x0004D9F0 File Offset: 0x0004BBF0
		internal virtual Label lblHeight2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x06000A4B RID: 2635 RVA: 0x0004D9F9 File Offset: 0x0004BBF9
		// (set) Token: 0x06000A4C RID: 2636 RVA: 0x0004DA03 File Offset: 0x0004BC03
		internal virtual Label lblWeight1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x06000A4D RID: 2637 RVA: 0x0004DA0C File Offset: 0x0004BC0C
		// (set) Token: 0x06000A4E RID: 2638 RVA: 0x0004DA16 File Offset: 0x0004BC16
		internal virtual Label lblHeight1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06000A4F RID: 2639 RVA: 0x0004DA1F File Offset: 0x0004BC1F
		// (set) Token: 0x06000A50 RID: 2640 RVA: 0x0004DA29 File Offset: 0x0004BC29
		internal virtual GroupBox grpPokedexCategory
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06000A51 RID: 2641 RVA: 0x0004DA32 File Offset: 0x0004BC32
		// (set) Token: 0x06000A52 RID: 2642 RVA: 0x0004DA3C File Offset: 0x0004BC3C
		internal virtual Button btnChangePokedexCategory
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokedexCategory;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokedexCategory_Click);
				Button button = this._btnChangePokedexCategory;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokedexCategory = value;
				button = this._btnChangePokedexCategory;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06000A53 RID: 2643 RVA: 0x0004DA7F File Offset: 0x0004BC7F
		// (set) Token: 0x06000A54 RID: 2644 RVA: 0x0004DA89 File Offset: 0x0004BC89
		internal virtual TextBox txtPokedexCategory2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06000A55 RID: 2645 RVA: 0x0004DA92 File Offset: 0x0004BC92
		// (set) Token: 0x06000A56 RID: 2646 RVA: 0x0004DA9C File Offset: 0x0004BC9C
		internal virtual TextBox txtPokedexCategory1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06000A57 RID: 2647 RVA: 0x0004DAA5 File Offset: 0x0004BCA5
		// (set) Token: 0x06000A58 RID: 2648 RVA: 0x0004DAAF File Offset: 0x0004BCAF
		internal virtual GroupBox grpPokedexDescription
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x06000A59 RID: 2649 RVA: 0x0004DAB8 File Offset: 0x0004BCB8
		// (set) Token: 0x06000A5A RID: 2650 RVA: 0x0004DAC2 File Offset: 0x0004BCC2
		internal virtual TextBox txtPokedexDescription
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06000A5B RID: 2651 RVA: 0x0004DACB File Offset: 0x0004BCCB
		// (set) Token: 0x06000A5C RID: 2652 RVA: 0x0004DAD8 File Offset: 0x0004BCD8
		internal virtual Button btnChangePokedexDescription
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokedexDescription;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokedexDescription_Click);
				Button button = this._btnChangePokedexDescription;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokedexDescription = value;
				button = this._btnChangePokedexDescription;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06000A5D RID: 2653 RVA: 0x0004DB1B File Offset: 0x0004BD1B
		// (set) Token: 0x06000A5E RID: 2654 RVA: 0x0004DB28 File Offset: 0x0004BD28
		internal virtual Button btnChangePokedexDescriptionAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokedexDescriptionAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokedexDescriptionAddress_Click);
				Button button = this._btnChangePokedexDescriptionAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokedexDescriptionAddress = value;
				button = this._btnChangePokedexDescriptionAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06000A5F RID: 2655 RVA: 0x0004DB6B File Offset: 0x0004BD6B
		// (set) Token: 0x06000A60 RID: 2656 RVA: 0x0004DB75 File Offset: 0x0004BD75
		internal virtual TextBox txtPokedexDescriptionAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06000A61 RID: 2657 RVA: 0x0004DB7E File Offset: 0x0004BD7E
		// (set) Token: 0x06000A62 RID: 2658 RVA: 0x0004DB88 File Offset: 0x0004BD88
		internal virtual GroupBox grpSizeComparison
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06000A63 RID: 2659 RVA: 0x0004DB91 File Offset: 0x0004BD91
		// (set) Token: 0x06000A64 RID: 2660 RVA: 0x0004DB9B File Offset: 0x0004BD9B
		internal virtual Label lblSizeComparison4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06000A65 RID: 2661 RVA: 0x0004DBA4 File Offset: 0x0004BDA4
		// (set) Token: 0x06000A66 RID: 2662 RVA: 0x0004DBAE File Offset: 0x0004BDAE
		internal virtual Label lblSizeComparison3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06000A67 RID: 2663 RVA: 0x0004DBB7 File Offset: 0x0004BDB7
		// (set) Token: 0x06000A68 RID: 2664 RVA: 0x0004DBC1 File Offset: 0x0004BDC1
		internal virtual Label lblSizeComparison2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06000A69 RID: 2665 RVA: 0x0004DBCA File Offset: 0x0004BDCA
		// (set) Token: 0x06000A6A RID: 2666 RVA: 0x0004DBD4 File Offset: 0x0004BDD4
		internal virtual Label lblSizeComparison1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x06000A6B RID: 2667 RVA: 0x0004DBDD File Offset: 0x0004BDDD
		// (set) Token: 0x06000A6C RID: 2668 RVA: 0x0004DBE8 File Offset: 0x0004BDE8
		internal virtual NumericUpDown nudSizeComparison4
		{
			[CompilerGenerated]
			get
			{
				return this._nudSizeComparison4;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.SizeComparison_ValueChanged);
				NumericUpDown numericUpDown = this._nudSizeComparison4;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudSizeComparison4 = value;
				numericUpDown = this._nudSizeComparison4;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x06000A6D RID: 2669 RVA: 0x0004DC2B File Offset: 0x0004BE2B
		// (set) Token: 0x06000A6E RID: 2670 RVA: 0x0004DC38 File Offset: 0x0004BE38
		internal virtual NumericUpDown nudSizeComparison3
		{
			[CompilerGenerated]
			get
			{
				return this._nudSizeComparison3;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.SizeComparison_ValueChanged);
				NumericUpDown numericUpDown = this._nudSizeComparison3;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudSizeComparison3 = value;
				numericUpDown = this._nudSizeComparison3;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x06000A6F RID: 2671 RVA: 0x0004DC7B File Offset: 0x0004BE7B
		// (set) Token: 0x06000A70 RID: 2672 RVA: 0x0004DC88 File Offset: 0x0004BE88
		internal virtual NumericUpDown nudSizeComparison2
		{
			[CompilerGenerated]
			get
			{
				return this._nudSizeComparison2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.SizeComparison_ValueChanged);
				NumericUpDown numericUpDown = this._nudSizeComparison2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudSizeComparison2 = value;
				numericUpDown = this._nudSizeComparison2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06000A71 RID: 2673 RVA: 0x0004DCCB File Offset: 0x0004BECB
		// (set) Token: 0x06000A72 RID: 2674 RVA: 0x0004DCD8 File Offset: 0x0004BED8
		internal virtual NumericUpDown nudSizeComparison1
		{
			[CompilerGenerated]
			get
			{
				return this._nudSizeComparison1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.SizeComparison_ValueChanged);
				NumericUpDown numericUpDown = this._nudSizeComparison1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudSizeComparison1 = value;
				numericUpDown = this._nudSizeComparison1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06000A73 RID: 2675 RVA: 0x0004DD1B File Offset: 0x0004BF1B
		// (set) Token: 0x06000A74 RID: 2676 RVA: 0x0004DD25 File Offset: 0x0004BF25
		internal virtual TabPage tabCry
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06000A75 RID: 2677 RVA: 0x0004DD2E File Offset: 0x0004BF2E
		// (set) Token: 0x06000A76 RID: 2678 RVA: 0x0004DD38 File Offset: 0x0004BF38
		internal virtual GroupBox grpPlayerSide
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06000A77 RID: 2679 RVA: 0x0004DD41 File Offset: 0x0004BF41
		// (set) Token: 0x06000A78 RID: 2680 RVA: 0x0004DD4B File Offset: 0x0004BF4B
		internal virtual Label lblPlayerBubbleXYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06000A79 RID: 2681 RVA: 0x0004DD54 File Offset: 0x0004BF54
		// (set) Token: 0x06000A7A RID: 2682 RVA: 0x0004DD5E File Offset: 0x0004BF5E
		internal virtual GroupBox grpEnemyShadowYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06000A7B RID: 2683 RVA: 0x0004DD67 File Offset: 0x0004BF67
		// (set) Token: 0x06000A7C RID: 2684 RVA: 0x0004DD74 File Offset: 0x0004BF74
		internal virtual NumericUpDown nudEnemyShadowYPosition
		{
			[CompilerGenerated]
			get
			{
				return this._nudEnemyShadowYPosition;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudEnemyShadowYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEnemyShadowYPosition = value;
				numericUpDown = this._nudEnemyShadowYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06000A7D RID: 2685 RVA: 0x0004DDB7 File Offset: 0x0004BFB7
		// (set) Token: 0x06000A7E RID: 2686 RVA: 0x0004DDC1 File Offset: 0x0004BFC1
		internal virtual Label lblEnemyShadowYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06000A7F RID: 2687 RVA: 0x0004DDCA File Offset: 0x0004BFCA
		// (set) Token: 0x06000A80 RID: 2688 RVA: 0x0004DDD4 File Offset: 0x0004BFD4
		internal virtual GroupBox grpEnemySide
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06000A81 RID: 2689 RVA: 0x0004DDDD File Offset: 0x0004BFDD
		// (set) Token: 0x06000A82 RID: 2690 RVA: 0x0004DDE7 File Offset: 0x0004BFE7
		internal virtual Label lblEnemyBubbleXYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06000A83 RID: 2691 RVA: 0x0004DDF0 File Offset: 0x0004BFF0
		// (set) Token: 0x06000A84 RID: 2692 RVA: 0x0004DDFA File Offset: 0x0004BFFA
		internal virtual Label lblEnemyPokemonYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06000A85 RID: 2693 RVA: 0x0004DE03 File Offset: 0x0004C003
		// (set) Token: 0x06000A86 RID: 2694 RVA: 0x0004DE10 File Offset: 0x0004C010
		internal virtual NumericUpDown nudEnemyPokemonYPosition
		{
			[CompilerGenerated]
			get
			{
				return this._nudEnemyPokemonYPosition;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudEnemyPokemonYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEnemyPokemonYPosition = value;
				numericUpDown = this._nudEnemyPokemonYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06000A87 RID: 2695 RVA: 0x0004DE53 File Offset: 0x0004C053
		// (set) Token: 0x06000A88 RID: 2696 RVA: 0x0004DE60 File Offset: 0x0004C060
		internal virtual NumericUpDown nudEnemyBubbleXYPosition1
		{
			[CompilerGenerated]
			get
			{
				return this._nudEnemyBubbleXYPosition1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudEnemyBubbleXYPosition1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEnemyBubbleXYPosition1 = value;
				numericUpDown = this._nudEnemyBubbleXYPosition1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06000A89 RID: 2697 RVA: 0x0004DEA3 File Offset: 0x0004C0A3
		// (set) Token: 0x06000A8A RID: 2698 RVA: 0x0004DEB0 File Offset: 0x0004C0B0
		internal virtual NumericUpDown nudPlayerBubbleXYPosition1
		{
			[CompilerGenerated]
			get
			{
				return this._nudPlayerBubbleXYPosition1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudPlayerBubbleXYPosition1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPlayerBubbleXYPosition1 = value;
				numericUpDown = this._nudPlayerBubbleXYPosition1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x06000A8B RID: 2699 RVA: 0x0004DEF3 File Offset: 0x0004C0F3
		// (set) Token: 0x06000A8C RID: 2700 RVA: 0x0004DEFD File Offset: 0x0004C0FD
		internal virtual GroupBox grpCryDataImportExport
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06000A8D RID: 2701 RVA: 0x0004DF06 File Offset: 0x0004C106
		// (set) Token: 0x06000A8E RID: 2702 RVA: 0x0004DF10 File Offset: 0x0004C110
		internal virtual GroupBox grpGen3CryConversion
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06000A8F RID: 2703 RVA: 0x0004DF19 File Offset: 0x0004C119
		// (set) Token: 0x06000A90 RID: 2704 RVA: 0x0004DF23 File Offset: 0x0004C123
		internal virtual Label lblGen3CryID
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06000A91 RID: 2705 RVA: 0x0004DF2C File Offset: 0x0004C12C
		// (set) Token: 0x06000A92 RID: 2706 RVA: 0x0004DF36 File Offset: 0x0004C136
		internal virtual TextBox txtGen3CryConversion
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06000A93 RID: 2707 RVA: 0x0004DF3F File Offset: 0x0004C13F
		// (set) Token: 0x06000A94 RID: 2708 RVA: 0x0004DF4C File Offset: 0x0004C14C
		internal virtual Button btnChangeGen3CryConversion
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeGen3CryConversion;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeGen3CryConversion_Click);
				Button button = this._btnChangeGen3CryConversion;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeGen3CryConversion = value;
				button = this._btnChangeGen3CryConversion;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06000A95 RID: 2709 RVA: 0x0004DF8F File Offset: 0x0004C18F
		// (set) Token: 0x06000A96 RID: 2710 RVA: 0x0004DF99 File Offset: 0x0004C199
		internal virtual GroupBox grpCryData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06000A97 RID: 2711 RVA: 0x0004DFA2 File Offset: 0x0004C1A2
		// (set) Token: 0x06000A98 RID: 2712 RVA: 0x0004DFAC File Offset: 0x0004C1AC
		internal virtual Button btnPlayCry
		{
			[CompilerGenerated]
			get
			{
				return this._btnPlayCry;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPlayCry_Click);
				Button button = this._btnPlayCry;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPlayCry = value;
				button = this._btnPlayCry;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06000A99 RID: 2713 RVA: 0x0004DFEF File Offset: 0x0004C1EF
		// (set) Token: 0x06000A9A RID: 2714 RVA: 0x0004DFF9 File Offset: 0x0004C1F9
		internal virtual Label lblCrySamples2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x06000A9B RID: 2715 RVA: 0x0004E002 File Offset: 0x0004C202
		// (set) Token: 0x06000A9C RID: 2716 RVA: 0x0004E00C File Offset: 0x0004C20C
		internal virtual Label lblCrySampleRate2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06000A9D RID: 2717 RVA: 0x0004E015 File Offset: 0x0004C215
		// (set) Token: 0x06000A9E RID: 2718 RVA: 0x0004E01F File Offset: 0x0004C21F
		internal virtual Label lblCrySamples1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06000A9F RID: 2719 RVA: 0x0004E028 File Offset: 0x0004C228
		// (set) Token: 0x06000AA0 RID: 2720 RVA: 0x0004E032 File Offset: 0x0004C232
		internal virtual Label lblCrySampleRate1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06000AA1 RID: 2721 RVA: 0x0004E03B File Offset: 0x0004C23B
		// (set) Token: 0x06000AA2 RID: 2722 RVA: 0x0004E045 File Offset: 0x0004C245
		internal virtual Panel pnlCryData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06000AA3 RID: 2723 RVA: 0x0004E04E File Offset: 0x0004C24E
		// (set) Token: 0x06000AA4 RID: 2724 RVA: 0x0004E058 File Offset: 0x0004C258
		internal virtual Button btnChangeCryDataAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeCryDataAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeCryDataAddress_Click);
				Button button = this._btnChangeCryDataAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeCryDataAddress = value;
				button = this._btnChangeCryDataAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06000AA5 RID: 2725 RVA: 0x0004E09B File Offset: 0x0004C29B
		// (set) Token: 0x06000AA6 RID: 2726 RVA: 0x0004E0A5 File Offset: 0x0004C2A5
		internal virtual TextBox txtCryDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06000AA7 RID: 2727 RVA: 0x0004E0AE File Offset: 0x0004C2AE
		// (set) Token: 0x06000AA8 RID: 2728 RVA: 0x0004E0B8 File Offset: 0x0004C2B8
		internal virtual Label lblCryDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06000AA9 RID: 2729 RVA: 0x0004E0C1 File Offset: 0x0004C2C1
		// (set) Token: 0x06000AAA RID: 2730 RVA: 0x0004E0CC File Offset: 0x0004C2CC
		internal virtual Button btnExportCryData
		{
			[CompilerGenerated]
			get
			{
				return this._btnExportCryData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnExportCryData_Click);
				Button button = this._btnExportCryData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnExportCryData = value;
				button = this._btnExportCryData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x06000AAB RID: 2731 RVA: 0x0004E10F File Offset: 0x0004C30F
		// (set) Token: 0x06000AAC RID: 2732 RVA: 0x0004E11C File Offset: 0x0004C31C
		internal virtual Button btnCryDataImportAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnCryDataImportAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnCryDataImportAddress_Click);
				Button button = this._btnCryDataImportAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnCryDataImportAddress = value;
				button = this._btnCryDataImportAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06000AAD RID: 2733 RVA: 0x0004E15F File Offset: 0x0004C35F
		// (set) Token: 0x06000AAE RID: 2734 RVA: 0x0004E169 File Offset: 0x0004C369
		internal virtual TextBox txtCryDataImportAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06000AAF RID: 2735 RVA: 0x0004E172 File Offset: 0x0004C372
		// (set) Token: 0x06000AB0 RID: 2736 RVA: 0x0004E17C File Offset: 0x0004C37C
		internal virtual Label lblCryDataImportAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x06000AB1 RID: 2737 RVA: 0x0004E185 File Offset: 0x0004C385
		// (set) Token: 0x06000AB2 RID: 2738 RVA: 0x0004E18F File Offset: 0x0004C38F
		internal virtual Label lblWarning
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06000AB3 RID: 2739 RVA: 0x0004E198 File Offset: 0x0004C398
		// (set) Token: 0x06000AB4 RID: 2740 RVA: 0x0004E1A2 File Offset: 0x0004C3A2
		internal virtual ComboBox cmbMoveList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06000AB5 RID: 2741 RVA: 0x0004E1AB File Offset: 0x0004C3AB
		// (set) Token: 0x06000AB6 RID: 2742 RVA: 0x0004E1B8 File Offset: 0x0004C3B8
		internal virtual NumericUpDown nudPlayerBubbleXYPosition2
		{
			[CompilerGenerated]
			get
			{
				return this._nudPlayerBubbleXYPosition2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudPlayerBubbleXYPosition2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPlayerBubbleXYPosition2 = value;
				numericUpDown = this._nudPlayerBubbleXYPosition2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06000AB7 RID: 2743 RVA: 0x0004E1FB File Offset: 0x0004C3FB
		// (set) Token: 0x06000AB8 RID: 2744 RVA: 0x0004E208 File Offset: 0x0004C408
		internal virtual NumericUpDown nudEnemyBubbleXYPosition2
		{
			[CompilerGenerated]
			get
			{
				return this._nudEnemyBubbleXYPosition2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.YPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudEnemyBubbleXYPosition2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEnemyBubbleXYPosition2 = value;
				numericUpDown = this._nudEnemyBubbleXYPosition2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06000AB9 RID: 2745 RVA: 0x0004E24B File Offset: 0x0004C44B
		// (set) Token: 0x06000ABA RID: 2746 RVA: 0x0004E258 File Offset: 0x0004C458
		internal virtual CheckBox chkShowBubbleSprite
		{
			[CompilerGenerated]
			get
			{
				return this._chkShowBubbleSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.chkShowBubbleSprite_CheckedChanged);
				CheckBox checkBox = this._chkShowBubbleSprite;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkShowBubbleSprite = value;
				checkBox = this._chkShowBubbleSprite;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x06000ABB RID: 2747 RVA: 0x0004E29B File Offset: 0x0004C49B
		// (set) Token: 0x06000ABC RID: 2748 RVA: 0x0004E2A5 File Offset: 0x0004C4A5
		internal virtual ComboBox cmbExportPokemonSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06000ABD RID: 2749 RVA: 0x0004E2AE File Offset: 0x0004C4AE
		// (set) Token: 0x06000ABE RID: 2750 RVA: 0x0004E2B8 File Offset: 0x0004C4B8
		internal virtual RadioButton rbShinyPal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x06000ABF RID: 2751 RVA: 0x0004E2C1 File Offset: 0x0004C4C1
		// (set) Token: 0x06000AC0 RID: 2752 RVA: 0x0004E2CB File Offset: 0x0004C4CB
		internal virtual RadioButton rbNormalPal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x06000AC1 RID: 2753 RVA: 0x0004E2D4 File Offset: 0x0004C4D4
		// (set) Token: 0x06000AC2 RID: 2754 RVA: 0x0004E2DE File Offset: 0x0004C4DE
		internal virtual RadioButton rbBackImg
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x06000AC3 RID: 2755 RVA: 0x0004E2E7 File Offset: 0x0004C4E7
		// (set) Token: 0x06000AC4 RID: 2756 RVA: 0x0004E2F1 File Offset: 0x0004C4F1
		internal virtual RadioButton rbFrontImg
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06000AC5 RID: 2757 RVA: 0x0004E2FA File Offset: 0x0004C4FA
		// (set) Token: 0x06000AC6 RID: 2758 RVA: 0x0004E304 File Offset: 0x0004C504
		internal virtual TextBox txtImportPokemonSpriteAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06000AC7 RID: 2759 RVA: 0x0004E30D File Offset: 0x0004C50D
		// (set) Token: 0x06000AC8 RID: 2760 RVA: 0x0004E318 File Offset: 0x0004C518
		internal virtual Button btnImportPokemonSprite
		{
			[CompilerGenerated]
			get
			{
				return this._btnImportPokemonSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnImportPokemonSprite_Click);
				Button button = this._btnImportPokemonSprite;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnImportPokemonSprite = value;
				button = this._btnImportPokemonSprite;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x06000AC9 RID: 2761 RVA: 0x0004E35B File Offset: 0x0004C55B
		// (set) Token: 0x06000ACA RID: 2762 RVA: 0x0004E368 File Offset: 0x0004C568
		internal virtual Button btnExportPokemonSprite
		{
			[CompilerGenerated]
			get
			{
				return this._btnExportPokemonSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnExportPokemonSprite_Click);
				Button button = this._btnExportPokemonSprite;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnExportPokemonSprite = value;
				button = this._btnExportPokemonSprite;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06000ACB RID: 2763 RVA: 0x0004E3AB File Offset: 0x0004C5AB
		// (set) Token: 0x06000ACC RID: 2764 RVA: 0x0004E3B8 File Offset: 0x0004C5B8
		internal virtual Button btnPokemonIconExport
		{
			[CompilerGenerated]
			get
			{
				return this._btnPokemonIconExport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPokemonIconExport_Click);
				Button button = this._btnPokemonIconExport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPokemonIconExport = value;
				button = this._btnPokemonIconExport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06000ACD RID: 2765 RVA: 0x0004E3FB File Offset: 0x0004C5FB
		// (set) Token: 0x06000ACE RID: 2766 RVA: 0x0004E408 File Offset: 0x0004C608
		internal virtual Button btnPokemonIconImport
		{
			[CompilerGenerated]
			get
			{
				return this._btnPokemonIconImport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPokemonIconImport_Click);
				Button button = this._btnPokemonIconImport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPokemonIconImport = value;
				button = this._btnPokemonIconImport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06000ACF RID: 2767 RVA: 0x0004E44B File Offset: 0x0004C64B
		// (set) Token: 0x06000AD0 RID: 2768 RVA: 0x0004E455 File Offset: 0x0004C655
		internal virtual PictureBox picPokemonIconPal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06000AD1 RID: 2769 RVA: 0x0004E45E File Offset: 0x0004C65E
		// (set) Token: 0x06000AD2 RID: 2770 RVA: 0x0004E468 File Offset: 0x0004C668
		internal virtual NumericUpDown nudEvHp
		{
			[CompilerGenerated]
			get
			{
				return this._nudEvHp;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EVs_ValueChanged);
				NumericUpDown numericUpDown = this._nudEvHp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEvHp = value;
				numericUpDown = this._nudEvHp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06000AD3 RID: 2771 RVA: 0x0004E4AB File Offset: 0x0004C6AB
		// (set) Token: 0x06000AD4 RID: 2772 RVA: 0x0004E4B8 File Offset: 0x0004C6B8
		internal virtual NumericUpDown nudEvSpeed
		{
			[CompilerGenerated]
			get
			{
				return this._nudEvSpeed;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EVs_ValueChanged);
				NumericUpDown numericUpDown = this._nudEvSpeed;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEvSpeed = value;
				numericUpDown = this._nudEvSpeed;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06000AD5 RID: 2773 RVA: 0x0004E4FB File Offset: 0x0004C6FB
		// (set) Token: 0x06000AD6 RID: 2774 RVA: 0x0004E508 File Offset: 0x0004C708
		internal virtual NumericUpDown nudEvAttack
		{
			[CompilerGenerated]
			get
			{
				return this._nudEvAttack;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EVs_ValueChanged);
				NumericUpDown numericUpDown = this._nudEvAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEvAttack = value;
				numericUpDown = this._nudEvAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x06000AD7 RID: 2775 RVA: 0x0004E54B File Offset: 0x0004C74B
		// (set) Token: 0x06000AD8 RID: 2776 RVA: 0x0004E558 File Offset: 0x0004C758
		internal virtual NumericUpDown nudEvSpDefense
		{
			[CompilerGenerated]
			get
			{
				return this._nudEvSpDefense;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EVs_ValueChanged);
				NumericUpDown numericUpDown = this._nudEvSpDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEvSpDefense = value;
				numericUpDown = this._nudEvSpDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x06000AD9 RID: 2777 RVA: 0x0004E59B File Offset: 0x0004C79B
		// (set) Token: 0x06000ADA RID: 2778 RVA: 0x0004E5A8 File Offset: 0x0004C7A8
		internal virtual NumericUpDown nudEvDefense
		{
			[CompilerGenerated]
			get
			{
				return this._nudEvDefense;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EVs_ValueChanged);
				NumericUpDown numericUpDown = this._nudEvDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEvDefense = value;
				numericUpDown = this._nudEvDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x06000ADB RID: 2779 RVA: 0x0004E5EB File Offset: 0x0004C7EB
		// (set) Token: 0x06000ADC RID: 2780 RVA: 0x0004E5F8 File Offset: 0x0004C7F8
		internal virtual NumericUpDown nudEvSpAttack
		{
			[CompilerGenerated]
			get
			{
				return this._nudEvSpAttack;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.EVs_ValueChanged);
				NumericUpDown numericUpDown = this._nudEvSpAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEvSpAttack = value;
				numericUpDown = this._nudEvSpAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x06000ADD RID: 2781 RVA: 0x0004E63B File Offset: 0x0004C83B
		// (set) Token: 0x06000ADE RID: 2782 RVA: 0x0004E648 File Offset: 0x0004C848
		internal virtual NumericUpDown nudBaseStatsHp
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseStatsHp;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.BaseStats_ValueChanged);
				NumericUpDown numericUpDown = this._nudBaseStatsHp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseStatsHp = value;
				numericUpDown = this._nudBaseStatsHp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x06000ADF RID: 2783 RVA: 0x0004E68B File Offset: 0x0004C88B
		// (set) Token: 0x06000AE0 RID: 2784 RVA: 0x0004E698 File Offset: 0x0004C898
		internal virtual NumericUpDown nudBaseStatsSpeed
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseStatsSpeed;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.BaseStats_ValueChanged);
				NumericUpDown numericUpDown = this._nudBaseStatsSpeed;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseStatsSpeed = value;
				numericUpDown = this._nudBaseStatsSpeed;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x06000AE1 RID: 2785 RVA: 0x0004E6DB File Offset: 0x0004C8DB
		// (set) Token: 0x06000AE2 RID: 2786 RVA: 0x0004E6E8 File Offset: 0x0004C8E8
		internal virtual NumericUpDown nudBaseStatsSpDefense
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseStatsSpDefense;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.BaseStats_ValueChanged);
				NumericUpDown numericUpDown = this._nudBaseStatsSpDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseStatsSpDefense = value;
				numericUpDown = this._nudBaseStatsSpDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06000AE3 RID: 2787 RVA: 0x0004E72B File Offset: 0x0004C92B
		// (set) Token: 0x06000AE4 RID: 2788 RVA: 0x0004E738 File Offset: 0x0004C938
		internal virtual NumericUpDown nudBaseStatsSpAttack
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseStatsSpAttack;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.BaseStats_ValueChanged);
				NumericUpDown numericUpDown = this._nudBaseStatsSpAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseStatsSpAttack = value;
				numericUpDown = this._nudBaseStatsSpAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x06000AE5 RID: 2789 RVA: 0x0004E77B File Offset: 0x0004C97B
		// (set) Token: 0x06000AE6 RID: 2790 RVA: 0x0004E788 File Offset: 0x0004C988
		internal virtual NumericUpDown nudBaseStatsDefense
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseStatsDefense;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.BaseStats_ValueChanged);
				NumericUpDown numericUpDown = this._nudBaseStatsDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseStatsDefense = value;
				numericUpDown = this._nudBaseStatsDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x06000AE7 RID: 2791 RVA: 0x0004E7CB File Offset: 0x0004C9CB
		// (set) Token: 0x06000AE8 RID: 2792 RVA: 0x0004E7D8 File Offset: 0x0004C9D8
		internal virtual NumericUpDown nudBaseStatsAttack
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseStatsAttack;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.BaseStats_ValueChanged);
				NumericUpDown numericUpDown = this._nudBaseStatsAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseStatsAttack = value;
				numericUpDown = this._nudBaseStatsAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x06000AE9 RID: 2793 RVA: 0x0004E81B File Offset: 0x0004CA1B
		// (set) Token: 0x06000AEA RID: 2794 RVA: 0x0004E828 File Offset: 0x0004CA28
		internal virtual NumericUpDown nudBaseHappiness
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseHappiness;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				NumericUpDown numericUpDown = this._nudBaseHappiness;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseHappiness = value;
				numericUpDown = this._nudBaseHappiness;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06000AEB RID: 2795 RVA: 0x0004E86B File Offset: 0x0004CA6B
		// (set) Token: 0x06000AEC RID: 2796 RVA: 0x0004E878 File Offset: 0x0004CA78
		internal virtual NumericUpDown nudCatchRate
		{
			[CompilerGenerated]
			get
			{
				return this._nudCatchRate;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				NumericUpDown numericUpDown = this._nudCatchRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudCatchRate = value;
				numericUpDown = this._nudCatchRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x06000AED RID: 2797 RVA: 0x0004E8BB File Offset: 0x0004CABB
		// (set) Token: 0x06000AEE RID: 2798 RVA: 0x0004E8C8 File Offset: 0x0004CAC8
		internal virtual NumericUpDown nudRunRate
		{
			[CompilerGenerated]
			get
			{
				return this._nudRunRate;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				NumericUpDown numericUpDown = this._nudRunRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudRunRate = value;
				numericUpDown = this._nudRunRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x06000AEF RID: 2799 RVA: 0x0004E90B File Offset: 0x0004CB0B
		// (set) Token: 0x06000AF0 RID: 2800 RVA: 0x0004E918 File Offset: 0x0004CB18
		internal virtual NumericUpDown nudBaseExp
		{
			[CompilerGenerated]
			get
			{
				return this._nudBaseExp;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.OtherData_Changed);
				NumericUpDown numericUpDown = this._nudBaseExp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudBaseExp = value;
				numericUpDown = this._nudBaseExp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x06000AF1 RID: 2801 RVA: 0x0004E95B File Offset: 0x0004CB5B
		// (set) Token: 0x06000AF2 RID: 2802 RVA: 0x0004E965 File Offset: 0x0004CB65
		internal virtual NumericUpDown nudMoveConditionLevel
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06000AF3 RID: 2803 RVA: 0x0004E96E File Offset: 0x0004CB6E
		// (set) Token: 0x06000AF4 RID: 2804 RVA: 0x0004E978 File Offset: 0x0004CB78
		internal virtual PictureBox picSizeComparison
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06000AF5 RID: 2805 RVA: 0x0004E981 File Offset: 0x0004CB81
		// (set) Token: 0x06000AF6 RID: 2806 RVA: 0x0004E98B File Offset: 0x0004CB8B
		internal virtual GroupBox grpSizeComparisonPreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x06000AF7 RID: 2807 RVA: 0x0004E994 File Offset: 0x0004CB94
		// (set) Token: 0x06000AF8 RID: 2808 RVA: 0x0004E99E File Offset: 0x0004CB9E
		internal virtual GroupBox grpTableExpansion
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x06000AF9 RID: 2809 RVA: 0x0004E9A7 File Offset: 0x0004CBA7
		// (set) Token: 0x06000AFA RID: 2810 RVA: 0x0004E9B1 File Offset: 0x0004CBB1
		internal virtual RadioButton rbSettingExpansion
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x06000AFB RID: 2811 RVA: 0x0004E9BA File Offset: 0x0004CBBA
		// (set) Token: 0x06000AFC RID: 2812 RVA: 0x0004E9C4 File Offset: 0x0004CBC4
		internal virtual RadioButton rbSettingNormal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x0004E9D0 File Offset: 0x0004CBD0
		private void PokemonEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.cmbPokemonIconPal.Items.Clear();
			checked
			{
				int num = this.ICON_PALETTE_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					this.cmbPokemonIconPal.Items.Add(string.Format("パレット {0}", i));
				}
				this.cmbPokemonIconPal.SelectedIndex = 0;
				this.UpdateIconPalettePreview();
				this.LoadBattleImages();
				this.nudPlayerPokemonYPosition.ValueChanged += this.UpdateBattleDisplay;
				this.nudEnemyPokemonYPosition.ValueChanged += this.UpdateBattleDisplay;
				this.nudEnemyShadowYPosition.ValueChanged += this.UpdateBattleDisplay;
				this.InitializeGenderComboBox();
				this.InitializeEggStepComboBox();
				this.InitializeEggGroupComboBoxes();
				this.InitializeGrowthRateComboBox();
				this.InitializePokemonColorComboBox();
				this.InitializePokemonDirectionComboBox();
				this.InitializeAbilityComboBoxes();
				this.InitializeItemComboBoxes();
				this.InitializeTypeComboBoxes();
				this.InitializeEvolutionMethodComboBox();
				this.InitializeMoveComboBox();
				this.InitializeTmHmList();
				this.InitializeMoveTutorList();
				this.InitializeWaveformPanel();
				this.LoadAllPokemonData();
				this.InitializePokemonList();
				this.InitializeEvolveToComboBox();
				this.InitializeParameterAssistComboBoxes();
				bool enable_BASE_STATS_EXPANSION = this.ENABLE_BASE_STATS_EXPANSION;
				if (enable_BASE_STATS_EXPANSION)
				{
					this.rbSettingExpansion.Checked = true;
					this.nudBaseExp.Maximum = 65535m;
				}
				else
				{
					this.rbSettingNormal.Checked = true;
				}
				this.currentPokemonIndex = 1;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				this.txtPokemonCode.Text = this.currentPokemonIndex.ToString("X4");
				this.txtPokemonName.Text = pokemonData.Name;
				this.UpdateSpriteAddressUI(pokemonData);
				this.DisplayPokemonSprites(pokemonData);
				this.rbFrontImg.Checked = true;
				this.cmbExportPokemonSprite.SelectedIndex = 0;
				this.txtFrontImgPointer.Enter += this.SpriteTextBox_Enter;
				this.txtBackImgPointer.Enter += this.SpriteTextBox_Enter;
				this.txtNormalPalPointer.Enter += this.SpriteTextBox_Enter;
				this.txtShinyPalPointer.Enter += this.SpriteTextBox_Enter;
				this.UpdateIconAddressUI(pokemonData);
				this.DisplayPokemonIcon(pokemonData);
				this.UpdateFootprintAddressUI(pokemonData);
				bool flag = this.currentPokemonIndex < this.NO_FOOTPRINT_START_INDEX;
				if (flag)
				{
					this.SetFootprintControlsEnabled(true);
					this.DisplayPokemonFootprint(pokemonData);
				}
				else
				{
					this.SetFootprintControlsEnabled(false);
				}
				this.LoadYPositionData(pokemonData);
				this.UpdateBattleDisplay(null, null);
				this.LoadPokemonStats(pokemonData);
				this.LoadEvolutionTable(pokemonData);
				this.lstEvolutionSlot.SelectedIndex = 0;
				this.LoadEvolutionMethods();
				this.rbParameterAssistPokemon.Checked = true;
				this.LoadLevelMoveAddress(pokemonData);
				this.LoadLevelMoves(pokemonData);
				this.LoadTmHmLearnData(pokemonData);
				this.LoadMoveTutorLearnData(pokemonData);
				int pokedexIndex = this.GetPokedexIndex(pokemonData);
				bool flag2 = pokedexIndex == -1;
				if (flag2)
				{
					this.SetPokedexControlsEnabled(false);
				}
				else
				{
					this.SetPokedexControlsEnabled(true);
					this.LoadPokedexCategory(pokemonData);
					this.UpdatePokedexCategoryUI(pokemonData);
					this.LoadPokedexDescriptionAddress(pokemonData);
					this.LoadPokedexDescription(pokemonData);
					this.LoadPokedexData(pokemonData);
					this.UpdateSizeUI(pokemonData);
					this.UpdateSizeComparisonUI(pokemonData);
					this.DisplaySizeComparisonImages(pokemonData);
				}
				this.LoadCryData(pokemonData);
				this.hasUnsavedChanges = false;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x0004ED4C File Offset: 0x0004CF4C
		private void LoadPokemonData(int pokemonIndex)
		{
			bool flag = pokemonIndex == this.currentPokemonIndex;
			if (!flag)
			{
				bool flag2 = this.hasUnsavedChanges;
				if (flag2)
				{
					DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					bool flag3 = dialogResult == DialogResult.Yes;
					if (flag3)
					{
						this.SaveCurrentPokemonChanges();
					}
					else
					{
						bool flag4 = dialogResult == DialogResult.No;
						if (flag4)
						{
							this.ReloadCurrentPokemonData();
						}
						else
						{
							bool flag5 = dialogResult == DialogResult.Cancel;
							if (flag5)
							{
								this.cmbPokemonCode.SelectedIndex = checked(this.currentPokemonIndex - 1);
								return;
							}
						}
					}
				}
				this.currentPokemonIndex = pokemonIndex;
				PokemonData pokemonData = this.pokemonDataList[pokemonIndex];
				this.txtPokemonCode.Text = pokemonIndex.ToString("X4");
				this.txtPokemonName.Text = pokemonData.Name;
				this.txtImportPokemonSpriteAddress.Text = string.Empty;
				this.txtCryDataImportAddress.Text = string.Empty;
				this.UpdateSpriteAddressUI(pokemonData);
				this.DisplayPokemonSprites(pokemonData);
				this.UpdateIconAddressUI(pokemonData);
				this.DisplayPokemonIcon(pokemonData);
				this.UpdateFootprintAddressUI(pokemonData);
				bool flag6 = pokemonIndex < this.NO_FOOTPRINT_START_INDEX;
				if (flag6)
				{
					this.SetFootprintControlsEnabled(true);
					this.DisplayPokemonFootprint(pokemonData);
				}
				else
				{
					this.SetFootprintControlsEnabled(false);
				}
				this.LoadYPositionData(pokemonData);
				this.UpdateBattleDisplay(null, null);
				this.LoadPokemonStats(pokemonData);
				this.LoadEvolutionTable(pokemonData);
				this.lstEvolutionSlot.SelectedIndex = 0;
				this.LoadLevelMoveAddress(pokemonData);
				this.LoadLevelMoves(pokemonData);
				this.LoadTmHmLearnData(pokemonData);
				this.LoadMoveTutorLearnData(pokemonData);
				int pokedexIndex = this.GetPokedexIndex(pokemonData);
				bool flag7 = pokedexIndex == -1;
				if (flag7)
				{
					this.SetPokedexControlsEnabled(false);
				}
				else
				{
					this.SetPokedexControlsEnabled(true);
					this.LoadPokedexCategory(pokemonData);
					this.UpdatePokedexCategoryUI(pokemonData);
					this.LoadPokedexDescriptionAddress(pokemonData);
					this.LoadPokedexDescription(pokemonData);
					this.LoadPokedexData(pokemonData);
					this.UpdateSizeUI(pokemonData);
					this.UpdateSizeComparisonUI(pokemonData);
					this.DisplaySizeComparisonImages(pokemonData);
				}
				this.LoadCryData(pokemonData);
				this.hasUnsavedChanges = false;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x0004EF5C File Offset: 0x0004D15C
		private void ReloadCurrentPokemonData()
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			pokemonData.TemporaryFrontImageData = null;
			pokemonData.TemporaryBackImageData = null;
			pokemonData.TemporaryNormalPaletteData = null;
			pokemonData.TemporaryShinyPaletteData = null;
			pokemonData.TemporaryIconData = null;
			pokemonData.TemporaryFootprintData = null;
			pokemonData.TemporaryLevelMoveData = null;
			pokemonData.TemporaryLevelMoveAddress = 0U;
			pokemonData.Name = this.GetPokemonNameFromRom(this.currentPokemonIndex);
			this.txtPokemonName.Text = pokemonData.Name;
			this.UpdateComboBoxDisplayName(this.currentPokemonIndex);
			this.UpdateEvolveToComboBox();
			this.UpdateParameterAssistPokemonComboBox();
			this.LoadSpriteAddresses(pokemonData);
			this.LoadIconAddress(pokemonData);
			bool flag = this.currentPokemonIndex < this.NO_FOOTPRINT_START_INDEX;
			if (flag)
			{
				this.LoadFootprintAddress(pokemonData);
				this.UpdateFootprintAddressUI(pokemonData);
				this.SetFootprintControlsEnabled(true);
				this.DisplayPokemonFootprint(pokemonData);
			}
			else
			{
				this.SetFootprintControlsEnabled(false);
			}
			this.LoadYPositionData(pokemonData);
			this.UpdateSpriteAddressUI(pokemonData);
			this.UpdateIconAddressUI(pokemonData);
			this.DisplayPokemonSprites(pokemonData);
			this.DisplayPokemonIcon(pokemonData);
			this.LoadPokemonStats(pokemonData);
			this.LoadLevelMoveAddress(pokemonData);
			this.LoadLevelMoves(pokemonData);
			this.LoadTmHmLearnData(pokemonData);
			this.LoadMoveTutorLearnData(pokemonData);
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag2 = pokedexIndex != -1;
			if (flag2)
			{
				this.LoadPokedexCategory(pokemonData);
				this.UpdatePokedexCategoryUI(pokemonData);
				this.LoadPokedexData(pokemonData);
				this.UpdateSizeUI(pokemonData);
				this.LoadPokedexDescriptionAddress(pokemonData);
				this.LoadPokedexDescription(pokemonData);
				this.UpdateSizeComparisonUI(pokemonData);
				this.DisplaySizeComparisonImages(pokemonData);
			}
			else
			{
				this.SetPokedexControlsEnabled(false);
			}
			this.LoadCryDataAddress(pokemonData);
			this.LoadCryData(pokemonData);
			this.hasUnsavedChanges = false;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x0004F119 File Offset: 0x0004D319
		private void UpdateSaveButtonState()
		{
			this.btnSavePokemon.Enabled = this.hasUnsavedChanges;
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x0004F12E File Offset: 0x0004D32E
		private void btnSavePokemon_Click(object sender, EventArgs e)
		{
			this.SaveCurrentPokemonChanges();
		}

		// Token: 0x06000B02 RID: 2818 RVA: 0x0004F138 File Offset: 0x0004D338
		private void SaveCurrentPokemonChanges()
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			this.SavePokemonNameToRom(pokemonData);
			bool flag = pokemonData.TemporaryFrontImageData != null;
			if (flag)
			{
				Array.Copy(pokemonData.TemporaryFrontImageData, 0L, this.romData, (long)((ulong)pokemonData.FrontImageAddress), (long)pokemonData.TemporaryFrontImageData.Length);
				pokemonData.TemporaryFrontImageData = null;
			}
			bool flag2 = pokemonData.TemporaryBackImageData != null;
			if (flag2)
			{
				Array.Copy(pokemonData.TemporaryBackImageData, 0L, this.romData, (long)((ulong)pokemonData.BackImageAddress), (long)pokemonData.TemporaryBackImageData.Length);
				pokemonData.TemporaryBackImageData = null;
			}
			bool flag3 = pokemonData.TemporaryNormalPaletteData != null;
			if (flag3)
			{
				Array.Copy(pokemonData.TemporaryNormalPaletteData, 0L, this.romData, (long)((ulong)pokemonData.NormalPaletteAddress), (long)pokemonData.TemporaryNormalPaletteData.Length);
				pokemonData.TemporaryNormalPaletteData = null;
			}
			bool flag4 = pokemonData.TemporaryShinyPaletteData != null;
			if (flag4)
			{
				Array.Copy(pokemonData.TemporaryShinyPaletteData, 0L, this.romData, (long)((ulong)pokemonData.ShinyPaletteAddress), (long)pokemonData.TemporaryShinyPaletteData.Length);
				pokemonData.TemporaryShinyPaletteData = null;
			}
			this.SaveSpriteAddresses(pokemonData);
			this.SaveIconData(pokemonData);
			bool flag5 = this.currentPokemonIndex < this.NO_FOOTPRINT_START_INDEX;
			if (flag5)
			{
				this.SaveFootprintAddress(pokemonData);
				bool flag6 = pokemonData.TemporaryFootprintData != null;
				if (flag6)
				{
					Array.Copy(pokemonData.TemporaryFootprintData, 0L, this.romData, (long)((ulong)pokemonData.FootprintAddress), 32L);
					pokemonData.TemporaryFootprintData = null;
				}
			}
			this.SaveYPositionData(pokemonData);
			this.SavePokemonStats(pokemonData);
			this.SaveLevelMoveAddress();
			this.SaveLevelMoves();
			this.SaveTmHmLearnData(pokemonData);
			this.SaveMoveTutorLearnData(pokemonData);
			this.SaveEvolutionTable(pokemonData);
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag7 = pokedexIndex != -1;
			if (flag7)
			{
				this.SavePokedexCategory(pokemonData);
				this.SavePokedexData(pokemonData);
				this.SavePokedexDescription(pokemonData);
			}
			bool flag8 = pokemonData.TemporaryCry != null;
			if (flag8)
			{
				CryProcessor.SaveCompressedCryToROM(pokemonData.TemporaryCry, pokemonData.TemporaryCryAddress, this.romData);
				pokemonData.CryDataAddress = pokemonData.TemporaryCryAddress;
				pokemonData.TemporaryCry = null;
				pokemonData.TemporaryCryAddress = 0U;
			}
			this.SaveCryData(pokemonData);
			MainForm.romData = this.romData;
			this.hasUnsavedChanges = false;
			this.UpdateSaveButtonState();
			this.UpdateComboBoxDisplayName(this.currentPokemonIndex);
			this.UpdateEvolveToComboBox();
			this.UpdateParameterAssistPokemonComboBox();
		}

		// Token: 0x06000B03 RID: 2819 RVA: 0x0004F3A0 File Offset: 0x0004D5A0
		private void PokemonEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveCurrentPokemonChanges();
				}
				else
				{
					bool flag3 = dialogResult == DialogResult.No;
					if (!flag3)
					{
						bool flag4 = dialogResult == DialogResult.Cancel;
						if (flag4)
						{
							e.Cancel = true;
						}
					}
				}
			}
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x0004F400 File Offset: 0x0004D600
		private void LoadAllPokemonData()
		{
			this.pokemonDataList.Clear();
			checked
			{
				int num = this.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					string pokemonNameFromRom = this.GetPokemonNameFromRom(i);
					PokemonData pokemonData = new PokemonData(i, pokemonNameFromRom);
					this.LoadSpriteAddresses(pokemonData);
					this.LoadIconAddress(pokemonData);
					bool flag = i < this.NO_FOOTPRINT_START_INDEX;
					if (flag)
					{
						this.LoadFootprintAddress(pokemonData);
					}
					int num2 = MyProject.Forms.PokedexOrderEditor.POKEDEX_ORDER_TABLE_OFFSET + (pokemonData.Index - 1) * MyProject.Forms.PokedexOrderEditor.POKEDEX_ORDER_ENTRY_LENGTH;
					pokemonData.PokedexOrder = (int)BitConverter.ToUInt16(this.romData, num2);
					this.LoadCryDataAddress(pokemonData);
					this.pokemonDataList.Add(i, pokemonData);
				}
			}
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x0004F4C0 File Offset: 0x0004D6C0
		private void InitializePokemonList()
		{
			this.cmbPokemonCode.BeginUpdate();
			this.cmbPokemonCode.Items.Clear();
			checked
			{
				int num = this.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					PokemonData pokemonData = this.pokemonDataList[i];
					this.cmbPokemonCode.Items.Add(pokemonData.Name);
				}
				this.cmbPokemonCode.EndUpdate();
				this.cmbPokemonCode.SelectedIndex = 0;
			}
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x0004F540 File Offset: 0x0004D740
		private string GetPokemonNameFromRom(int pokemonIndex)
		{
			checked
			{
				int num = this.POKEMON_NAME_OFFSET + pokemonIndex * this.POKEMON_NAME_LENGTH;
				byte[] array = new byte[this.POKEMON_NAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num, array, 0, this.POKEMON_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, this.POKEMON_NAME_LENGTH);
			}
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x0004F594 File Offset: 0x0004D794
		private void btnChangeName_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			string text = this.txtPokemonName.Text;
			bool flag = string.IsNullOrEmpty(text);
			if (flag)
			{
				text = pokemonData.OriginalName;
				this.txtPokemonName.Text = text;
			}
			checked
			{
				bool flag2 = text.Length > this.POKEMON_NAME_LENGTH - 1;
				if (flag2)
				{
					text = text.Substring(0, this.POKEMON_NAME_LENGTH - 1);
					this.txtPokemonName.Text = text;
				}
				bool flag3 = Operators.CompareString(pokemonData.Name, text, false) != 0;
				if (flag3)
				{
					pokemonData.Name = text;
					this.UpdateComboBoxDisplayName(this.currentPokemonIndex);
					this.UpdateEvolveToComboBox();
					this.UpdateParameterAssistPokemonComboBox();
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x0004F65C File Offset: 0x0004D85C
		private void SavePokemonNameToRom(PokemonData pokemonData)
		{
			checked
			{
				int num = this.POKEMON_NAME_OFFSET + pokemonData.Index * this.POKEMON_NAME_LENGTH;
				int num2 = this.POKEMON_NAME_LENGTH - 1;
				for (int i = 0; i <= num2; i++)
				{
					this.romData[num + i] = 0;
				}
				byte[] array = TextConverter.PokemonStringToBytes(pokemonData.Name, this.POKEMON_NAME_LENGTH - 1);
				int num3 = array.Length - 1;
				for (int j = 0; j <= num3; j++)
				{
					this.romData[num + j] = array[j];
				}
			}
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x0004F6DC File Offset: 0x0004D8DC
		private void UpdateComboBoxDisplayName(int pokemonIndex)
		{
			int num = checked(pokemonIndex - 1);
			PokemonData pokemonData = this.pokemonDataList[pokemonIndex];
			int selectedIndex = this.cmbPokemonCode.SelectedIndex;
			this.cmbPokemonCode.Items[num] = pokemonData.Name;
			this.cmbPokemonCode.SelectedIndex = selectedIndex;
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x0004F72C File Offset: 0x0004D92C
		private void UpdateEvolveToComboBox()
		{
			int selectedIndex = this.cmbEvolveTo.SelectedIndex;
			this.cmbEvolveTo.BeginUpdate();
			this.cmbEvolveTo.Items.Clear();
			this.cmbEvolveTo.Items.Add("なし");
			checked
			{
				int num = this.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					this.cmbEvolveTo.Items.Add(this.pokemonDataList[i].Name);
				}
				this.cmbEvolveTo.EndUpdate();
				this.cmbEvolveTo.SelectedIndex = selectedIndex;
			}
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x0004F7CC File Offset: 0x0004D9CC
		private void UpdateParameterAssistPokemonComboBox()
		{
			int selectedIndex = this.cmbParameterAssistPokemon.SelectedIndex;
			this.cmbParameterAssistPokemon.BeginUpdate();
			this.cmbParameterAssistPokemon.Items.Clear();
			checked
			{
				int num = this.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					this.cmbParameterAssistPokemon.Items.Add(this.pokemonDataList[i].Name);
				}
				this.cmbParameterAssistPokemon.EndUpdate();
				this.cmbParameterAssistPokemon.SelectedIndex = selectedIndex;
			}
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x0004F854 File Offset: 0x0004DA54
		private void cmbPokemonCode_SelectedIndexChanged(object sender, EventArgs e)
		{
			int num = checked(this.cmbPokemonCode.SelectedIndex + 1);
			this.LoadPokemonData(num);
		}

		// Token: 0x06000B0D RID: 2829 RVA: 0x0004F878 File Offset: 0x0004DA78
		private void LoadSpriteAddresses(PokemonData pokemonData)
		{
			pokemonData.FrontImageAddress = this.ReadImageAddress(this.FRONT_IMAGE_TABLE_OFFSET, pokemonData.Index);
			pokemonData.BackImageAddress = this.ReadImageAddress(this.BACK_IMAGE_TABLE_OFFSET, pokemonData.Index);
			pokemonData.NormalPaletteAddress = this.ReadImageAddress(this.NORMAL_PALETTE_TABLE_OFFSET, pokemonData.Index);
			pokemonData.ShinyPaletteAddress = this.ReadImageAddress(this.SHINY_PALETTE_TABLE_OFFSET, pokemonData.Index);
		}

		// Token: 0x06000B0E RID: 2830 RVA: 0x0004F8EC File Offset: 0x0004DAEC
		private uint ReadImageAddress(int tableOffset, int entryIndex)
		{
			checked
			{
				uint num3 = 0;
				try
				{
					int num = tableOffset + entryIndex * 8;
					uint num2 = BitConverter.ToUInt32(this.romData, num);
					num3 = num2 - 134217728U;
				}
				catch (Exception ex)
				{
					num3 = 0U;
				}
				return num3;
			}
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x0004F93C File Offset: 0x0004DB3C
		private void UpdateSpriteAddressUI(PokemonData pokemonData)
		{
			this.txtFrontImgPointer.Text = pokemonData.FrontImageAddress.ToString("X8");
			this.txtBackImgPointer.Text = pokemonData.BackImageAddress.ToString("X8");
			this.txtNormalPalPointer.Text = pokemonData.NormalPaletteAddress.ToString("X8");
			this.txtShinyPalPointer.Text = pokemonData.ShinyPaletteAddress.ToString("X8");
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x0004F9C8 File Offset: 0x0004DBC8
		private void DisplayPokemonSprites(PokemonData pokemonData)
		{
			this.DisplaySingleGBASprite(this.picFrontNormal, pokemonData.FrontImageAddress, pokemonData.NormalPaletteAddress, pokemonData.TemporaryFrontImageData, pokemonData.TemporaryNormalPaletteData);
			this.DisplaySingleGBASprite(this.picFrontShiny, pokemonData.FrontImageAddress, pokemonData.ShinyPaletteAddress, pokemonData.TemporaryFrontImageData, pokemonData.TemporaryShinyPaletteData);
			this.DisplaySingleGBASprite(this.picBackNormal, pokemonData.BackImageAddress, pokemonData.NormalPaletteAddress, pokemonData.TemporaryBackImageData, pokemonData.TemporaryNormalPaletteData);
			this.DisplaySingleGBASprite(this.picBackShiny, pokemonData.BackImageAddress, pokemonData.ShinyPaletteAddress, pokemonData.TemporaryBackImageData, pokemonData.TemporaryShinyPaletteData);
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x0004FA6C File Offset: 0x0004DC6C
		private void DisplaySingleGBASprite(PictureBox picBox, uint imageAddr, uint palAddr, byte[] tempImageData, byte[] tempPaletteData)
		{
			try
			{
				bool flag = ((ulong)imageAddr == 0UL && tempImageData == null) || ((ulong)palAddr == 0UL && tempPaletteData == null);
				checked
				{
					if (!flag)
					{
						bool flag2 = tempImageData != null;
						byte[] array;
						if (flag2)
						{
							int num = BitConverter.ToInt32(tempImageData, 0) >> 8;
							array = new byte[num - 1 + 1];
							ImageProcessor.LZ77UnComp(tempImageData, array);
						}
						else
						{
							array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, imageAddr, false);
						}
						bool flag3 = tempPaletteData != null;
						byte[] array2;
						if (flag3)
						{
							int num2 = BitConverter.ToInt32(tempPaletteData, 0) >> 8;
							array2 = new byte[num2 - 1 + 1];
							ImageProcessor.LZ77UnComp(tempPaletteData, array2);
						}
						else
						{
							array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, palAddr, true);
						}
						Color[] array3 = ImageProcessor.LoadPalette(array2, true);
						Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, 64, 64, false);
						bool flag4 = picBox.Image != null;
						if (flag4)
						{
							picBox.Image.Dispose();
						}
						picBox.Image = bitmap;
						picBox.Refresh();
					}
				}
			}
			catch (Exception ex)
			{
				picBox.Image = null;
			}
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x0004FB88 File Offset: 0x0004DD88
		private void btnChangePokemonSprite_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			uint frontImageAddress = pokemonData.FrontImageAddress;
			uint backImageAddress = pokemonData.BackImageAddress;
			uint normalPaletteAddress = pokemonData.NormalPaletteAddress;
			uint shinyPaletteAddress = pokemonData.ShinyPaletteAddress;
			uint num = Convert.ToUInt32(this.txtFrontImgPointer.Text, 16);
			uint num2 = Convert.ToUInt32(this.txtBackImgPointer.Text, 16);
			uint num3 = Convert.ToUInt32(this.txtNormalPalPointer.Text, 16);
			uint num4 = Convert.ToUInt32(this.txtShinyPalPointer.Text, 16);
			bool flag = frontImageAddress != num || backImageAddress != num2 || normalPaletteAddress != num3 || shinyPaletteAddress != num4;
			bool flag2 = flag;
			if (flag2)
			{
				pokemonData.TemporaryFrontImageData = null;
				pokemonData.TemporaryBackImageData = null;
				pokemonData.TemporaryNormalPaletteData = null;
				pokemonData.TemporaryShinyPaletteData = null;
				pokemonData.FrontImageAddress = num;
				pokemonData.BackImageAddress = num2;
				pokemonData.NormalPaletteAddress = num3;
				pokemonData.ShinyPaletteAddress = num4;
				this.DisplayPokemonSprites(pokemonData);
				this.UpdateBattleDisplay(null, null);
				EvolutionSlot evolutionSlot = this.evolutionSlots[this.lstEvolutionSlot.SelectedIndex];
				this.DisplayEvolveToPokemonImage((int)evolutionSlot.EvolveToPokemonId);
				this.DisplaySizeComparisonImages(pokemonData);
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x0004FCD0 File Offset: 0x0004DED0
		private void SaveSpriteAddresses(PokemonData pokemonData)
		{
			this.SaveImageAddress(this.FRONT_IMAGE_TABLE_OFFSET, this.currentPokemonIndex, pokemonData.FrontImageAddress);
			this.SaveImageAddress(this.BACK_IMAGE_TABLE_OFFSET, this.currentPokemonIndex, pokemonData.BackImageAddress);
			this.SaveImageAddress(this.NORMAL_PALETTE_TABLE_OFFSET, this.currentPokemonIndex, pokemonData.NormalPaletteAddress);
			this.SaveImageAddress(this.SHINY_PALETTE_TABLE_OFFSET, this.currentPokemonIndex, pokemonData.ShinyPaletteAddress);
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x0004FD44 File Offset: 0x0004DF44
		private void SaveImageAddress(int tableOffset, int entryIndex, uint address)
		{
			checked
			{
				int num = tableOffset + entryIndex * 8;
				uint num2 = address + 134217728U;
				byte[] bytes = BitConverter.GetBytes(num2);
				Array.Copy(bytes, 0, this.romData, num, 4);
			}
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x0004FD78 File Offset: 0x0004DF78
		private void btnExportPokemonSprite_Click(object sender, EventArgs e)
		{
			checked
			{
				using (SaveFileDialog saveFileDialog = new SaveFileDialog())
				{
					saveFileDialog.Filter = "PNG画像|*.png";
					saveFileDialog.Title = "ポケモン画像をエクスポート";
					PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
					string text = this.currentPokemonIndex.ToString("X4");
					string spriteTypeSuffix = this.GetSpriteTypeSuffix(this.cmbExportPokemonSprite.SelectedIndex);
					saveFileDialog.FileName = string.Format("pokemon_{0}{1}.png", text, spriteTypeSuffix);
					bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
					if (flag)
					{
						byte[] array;
						byte[] array2;
						switch (this.cmbExportPokemonSprite.SelectedIndex)
						{
						case 0:
						{
							uint num = pokemonData.FrontImageAddress;
							uint num2 = pokemonData.NormalPaletteAddress;
							bool flag2 = pokemonData.TemporaryFrontImageData != null;
							if (flag2)
							{
								array = pokemonData.TemporaryFrontImageData;
							}
							else
							{
								array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num, false);
							}
							bool flag3 = pokemonData.TemporaryNormalPaletteData != null;
							if (flag3)
							{
								array2 = pokemonData.TemporaryNormalPaletteData;
							}
							else
							{
								array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num2, true);
							}
							break;
						}
						case 1:
						{
							uint num = pokemonData.BackImageAddress;
							uint num2 = pokemonData.NormalPaletteAddress;
							bool flag4 = pokemonData.TemporaryBackImageData != null;
							if (flag4)
							{
								array = pokemonData.TemporaryBackImageData;
							}
							else
							{
								array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num, false);
							}
							bool flag5 = pokemonData.TemporaryNormalPaletteData != null;
							if (flag5)
							{
								array2 = pokemonData.TemporaryNormalPaletteData;
							}
							else
							{
								array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num2, true);
							}
							break;
						}
						case 2:
						{
							uint num = pokemonData.FrontImageAddress;
							uint num2 = pokemonData.ShinyPaletteAddress;
							bool flag6 = pokemonData.TemporaryFrontImageData != null;
							if (flag6)
							{
								array = pokemonData.TemporaryFrontImageData;
							}
							else
							{
								array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num, false);
							}
							bool flag7 = pokemonData.TemporaryShinyPaletteData != null;
							if (flag7)
							{
								array2 = pokemonData.TemporaryShinyPaletteData;
							}
							else
							{
								array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num2, true);
							}
							break;
						}
						default:
						{
							uint num = pokemonData.BackImageAddress;
							uint num2 = pokemonData.ShinyPaletteAddress;
							bool flag8 = pokemonData.TemporaryBackImageData != null;
							if (flag8)
							{
								array = pokemonData.TemporaryBackImageData;
							}
							else
							{
								array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num, false);
							}
							bool flag9 = pokemonData.TemporaryShinyPaletteData != null;
							if (flag9)
							{
								array2 = pokemonData.TemporaryShinyPaletteData;
							}
							else
							{
								array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num2, true);
							}
							break;
						}
						}
						int num3 = BitConverter.ToInt32(array, 0);
						bool flag10 = (num3 & 255) == 16;
						if (flag10)
						{
							int num4 = num3 >> 8;
							byte[] array3 = new byte[num4 - 1 + 1];
							ImageProcessor.LZ77UnComp(array, array3);
							array = array3;
						}
						num3 = BitConverter.ToInt32(array2, 0);
						bool flag11 = (num3 & 255) == 16;
						if (flag11)
						{
							int num5 = num3 >> 8;
							byte[] array4 = new byte[num5 - 1 + 1];
							ImageProcessor.LZ77UnComp(array2, array4);
							array2 = array4;
						}
						Color[] array5 = ImageProcessor.LoadPalette(array2, false);
						ImageProcessor.ExportSpriteTo4bppPng(saveFileDialog.FileName, array, array5, 64, 64);
					}
				}
			}
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x000500A0 File Offset: 0x0004E2A0
		private string GetSpriteTypeSuffix(int selectedIndex)
		{
			string text;
			switch (selectedIndex)
			{
			case 0:
				text = "_front_normal";
				break;
			case 1:
				text = "_back_normal";
				break;
			case 2:
				text = "_front_shiny";
				break;
			default:
				text = "_back_shiny";
				break;
			}
			return text;
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x000500EC File Offset: 0x0004E2EC
		private void SpriteTextBox_Enter(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			string name = textBox.Name;
			if (Operators.CompareString(name, "txtFrontImgPointer", false) != 0)
			{
				if (Operators.CompareString(name, "txtBackImgPointer", false) != 0)
				{
					if (Operators.CompareString(name, "txtNormalPalPointer", false) != 0)
					{
						if (Operators.CompareString(name, "txtShinyPalPointer", false) == 0)
						{
							this.rbShinyPal.Checked = true;
						}
					}
					else
					{
						this.rbNormalPal.Checked = true;
					}
				}
				else
				{
					this.rbBackImg.Checked = true;
				}
			}
			else
			{
				this.rbFrontImg.Checked = true;
			}
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x00050184 File Offset: 0x0004E384
		private void btnImportPokemonSprite_Click(object sender, EventArgs e)
		{
			bool flag = !string.IsNullOrEmpty(this.txtImportPokemonSpriteAddress.Text);
			if (flag)
			{
				string text = this.txtImportPokemonSpriteAddress.Text.Trim();
				uint num = 0;
				bool flag2 = !uint.TryParse(text, NumberStyles.HexNumber, null, out num);
				if (flag2)
				{
					MessageBox.Show("16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					using (OpenFileDialog openFileDialog = new OpenFileDialog())
					{
						openFileDialog.Filter = "PNG画像|*.png";
						openFileDialog.Title = "ポケモン画像をインポート";
						bool flag3 = openFileDialog.ShowDialog() == DialogResult.OK;
						if (flag3)
						{
							using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
							{
								bool flag4 = bitmap.Width != 64 || bitmap.Height != 64;
								if (flag4)
								{
									MessageBox.Show("サイズは64x64である必要があります。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								}
								else
								{
									bool flag5 = bitmap.PixelFormat != PixelFormat.Format4bppIndexed;
									if (flag5)
									{
										MessageBox.Show("4bppインデックスカラーのみ対応しています。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
									}
									else
									{
										PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
										bool flag6 = this.rbFrontImg.Checked || this.rbBackImg.Checked;
										if (flag6)
										{
											byte[] array = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
											byte[] array2 = ImageProcessor.LZ77Comp(array, false);
											bool @checked = this.rbFrontImg.Checked;
											if (@checked)
											{
												pokemonData.TemporaryFrontImageData = array2;
											}
											else
											{
												pokemonData.TemporaryBackImageData = array2;
											}
										}
										else
										{
											bool flag7 = this.rbNormalPal.Checked || this.rbShinyPal.Checked;
											if (flag7)
											{
												byte[] array3 = ImageProcessor.ConvertPaletteToBytes(bitmap.Palette);
												byte[] array4 = ImageProcessor.LZ77Comp(array3, true);
												bool checked2 = this.rbNormalPal.Checked;
												if (checked2)
												{
													pokemonData.TemporaryNormalPaletteData = array4;
												}
												else
												{
													pokemonData.TemporaryShinyPaletteData = array4;
												}
											}
										}
										string text2 = this.txtImportPokemonSpriteAddress.Text.Trim();
										uint num2 = Convert.ToUInt32(this.txtImportPokemonSpriteAddress.Text, 16);
										bool checked3 = this.rbFrontImg.Checked;
										if (checked3)
										{
											this.txtFrontImgPointer.Text = text2;
											pokemonData.FrontImageAddress = num2;
										}
										else
										{
											bool checked4 = this.rbBackImg.Checked;
											if (checked4)
											{
												this.txtBackImgPointer.Text = text2;
												pokemonData.BackImageAddress = num2;
											}
											else
											{
												bool checked5 = this.rbNormalPal.Checked;
												if (checked5)
												{
													this.txtNormalPalPointer.Text = text2;
													pokemonData.NormalPaletteAddress = num2;
												}
												else
												{
													bool checked6 = this.rbShinyPal.Checked;
													if (checked6)
													{
														this.txtShinyPalPointer.Text = text2;
														pokemonData.ShinyPaletteAddress = num2;
													}
												}
											}
										}
										this.DisplayPokemonSprites(pokemonData);
										this.UpdateBattleDisplay(null, null);
										EvolutionSlot evolutionSlot = this.evolutionSlots[this.lstEvolutionSlot.SelectedIndex];
										this.DisplayEvolveToPokemonImage((int)evolutionSlot.EvolveToPokemonId);
										this.DisplaySizeComparisonImages(pokemonData);
										this.hasUnsavedChanges = true;
										this.UpdateSaveButtonState();
										this.DisplayPokemonSprites(pokemonData);
										this.hasUnsavedChanges = true;
										this.UpdateSaveButtonState();
									}
								}
							}
						}
					}
				}
			}
			else
			{
				MessageBox.Show("アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x00050520 File Offset: 0x0004E720
		private void LoadIconAddress(PokemonData pokemonData)
		{
			checked
			{
				int num = this.ICON_IMAGE_TABLE_OFFSET + pokemonData.Index * 4;
				uint num2 = BitConverter.ToUInt32(this.romData, num);
				pokemonData.IconImageAddress = num2 - 134217728U;
				int num3 = this.ICON_PALETTE_ID_TABLE_OFFSET + pokemonData.Index;
				int num4 = (int)this.romData[num3];
				pokemonData.IconPaletteId = Math.Max(0, Math.Min(num4, this.ICON_PALETTE_COUNT - 1));
			}
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x0005058C File Offset: 0x0004E78C
		private void UpdateIconAddressUI(PokemonData pokemonData)
		{
			this.txtPokemonIconAddress.Text = pokemonData.IconImageAddress.ToString("X8");
			this.cmbPokemonIconPal.SelectedIndex = pokemonData.IconPaletteId;
		}

		// Token: 0x06000B1B RID: 2843 RVA: 0x000505CC File Offset: 0x0004E7CC
		private void DisplayPokemonIcon(PokemonData pokemonData)
		{
			bool flag = pokemonData.TemporaryIconData != null;
			if (flag)
			{
				byte[] array = pokemonData.TemporaryIconData;
				this.DisplayTemporaryIcon(array, pokemonData.IconPaletteId);
			}
			else
			{
				byte[] array;
				int num = 0;
				checked
				{
					num = (int)Math.Min(2048L, unchecked((long)this.romData.Length) - (long)(unchecked((ulong)pokemonData.IconImageAddress)));
					array = new byte[num - 1 + 1];
				}
				Array.Copy(this.romData, (long)((ulong)pokemonData.IconImageAddress), array, 0L, (long)num);
				byte[] array2 = this.LoadIconPalette(pokemonData.IconPaletteId);
				Color[] array3 = ImageProcessor.LoadPalette(array2, true);
				Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, 32, 64, false);
				bool flag2 = this.picPokemonIcon.Image != null;
				if (flag2)
				{
					this.picPokemonIcon.Image.Dispose();
				}
				this.picPokemonIcon.Image = bitmap;
				this.picPokemonIcon.Refresh();
			}
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x000506AC File Offset: 0x0004E8AC
		private byte[] LoadIconPalette(int paletteId)
		{
			uint num2 = 0;
			byte[] array;
			checked
			{
				int num = this.ICON_PALETTE_TABLE_OFFSET + paletteId * 8;
				num2 = BitConverter.ToUInt32(this.romData, num);
				num2 -= 134217728U;
				array = new byte[32];
			}
			Array.Copy(this.romData, (long)((ulong)num2), array, 0L, 32L);
			return array;
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x000506FC File Offset: 0x0004E8FC
		private void cmbPokemonIconPal_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdateIconPalettePreview();
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = pokemonData.IconPaletteId != this.cmbPokemonIconPal.SelectedIndex;
				if (flag2)
				{
					pokemonData.IconPaletteId = this.cmbPokemonIconPal.SelectedIndex;
					this.DisplayPokemonIcon(pokemonData);
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00050784 File Offset: 0x0004E984
		private void UpdateIconPalettePreview()
		{
			int selectedIndex = this.cmbPokemonIconPal.SelectedIndex;
			byte[] array = this.LoadIconPalette(selectedIndex);
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
				bool flag = this.picPokemonIconPal.Image != null;
				if (flag)
				{
					this.picPokemonIconPal.Image.Dispose();
				}
				this.picPokemonIconPal.Image = bitmap;
			}
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x00050870 File Offset: 0x0004EA70
		private void btnChangePokemonIcon_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			uint num = Convert.ToUInt32(this.txtPokemonIconAddress.Text, 16);
			bool flag = pokemonData.IconImageAddress != num;
			bool flag2 = flag;
			if (flag2)
			{
				pokemonData.IconImageAddress = num;
				this.DisplayPokemonIcon(pokemonData);
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x000508D8 File Offset: 0x0004EAD8
		private void SaveIconData(PokemonData pokemonData)
		{
			bool flag = pokemonData.TemporaryIconData != null;
			if (flag)
			{
				Array.Copy(pokemonData.TemporaryIconData, 0L, this.romData, (long)((ulong)pokemonData.IconImageAddress), (long)pokemonData.TemporaryIconData.Length);
				pokemonData.TemporaryIconData = null;
			}
			this.SaveIconImageAddress(pokemonData);
			checked
			{
				int num = this.ICON_PALETTE_ID_TABLE_OFFSET + this.currentPokemonIndex;
				this.romData[num] = (byte)pokemonData.IconPaletteId;
			}
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x00050948 File Offset: 0x0004EB48
		private void SaveIconImageAddress(PokemonData pokemonData)
		{
			checked
			{
				int num = this.ICON_IMAGE_TABLE_OFFSET + this.currentPokemonIndex * 4;
				uint num2 = pokemonData.IconImageAddress + 134217728U;
				byte[] bytes = BitConverter.GetBytes(num2);
				Array.Copy(bytes, 0, this.romData, num, 4);
			}
		}

		// Token: 0x06000B22 RID: 2850 RVA: 0x0005098C File Offset: 0x0004EB8C
		private void btnPokemonIconExport_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = "PNG画像|*.png";
				saveFileDialog.Title = "ポケモンアイコンをエクスポート";
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				string text = this.currentPokemonIndex.ToString("X4");
				int selectedIndex = this.cmbPokemonIconPal.SelectedIndex;
				saveFileDialog.FileName = string.Format("icon_{0}_pal{1}.png", text, selectedIndex);
				bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					bool flag2 = pokemonData.TemporaryIconData != null;
					byte[] array;
					if (flag2)
					{
						array = pokemonData.TemporaryIconData;
					}
					else
					{
						int num = 0;
						checked
						{
							num = (int)Math.Min(2048L, unchecked((long)this.romData.Length) - (long)(unchecked((ulong)pokemonData.IconImageAddress)));
							array = new byte[num - 1 + 1];
						}
						Array.Copy(this.romData, (long)((ulong)pokemonData.IconImageAddress), array, 0L, (long)num);
					}
					byte[] array2 = this.LoadIconPalette(pokemonData.IconPaletteId);
					Color[] array3 = ImageProcessor.LoadPalette(array2, false);
					ImageProcessor.ExportSpriteTo4bppPng(saveFileDialog.FileName, array, array3, 32, 64);
				}
			}
		}

		// Token: 0x06000B23 RID: 2851 RVA: 0x00050ACC File Offset: 0x0004ECCC
		private void btnPokemonIconImport_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "PNG画像|*.png";
				openFileDialog.Title = "ポケモンアイコンをインポート";
				bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
					{
						bool flag2 = bitmap.Width != 32 || bitmap.Height != 64;
						if (flag2)
						{
							MessageBox.Show(string.Format("サイズは{0}x{1}である必要があります。", 32, 64), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						}
						else
						{
							bool flag3 = bitmap.PixelFormat != PixelFormat.Format4bppIndexed;
							if (flag3)
							{
								MessageBox.Show("4bppインデックスカラー画像のみ対応しています。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							}
							else
							{
								byte[] array = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
								PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
								pokemonData.TemporaryIconData = array;
								this.DisplayTemporaryIcon(array, pokemonData.IconPaletteId);
								this.hasUnsavedChanges = true;
								this.UpdateSaveButtonState();
							}
						}
					}
				}
			}
		}

		// Token: 0x06000B24 RID: 2852 RVA: 0x00050C08 File Offset: 0x0004EE08
		private void DisplayTemporaryIcon(byte[] iconData, int paletteId)
		{
			byte[] array = this.LoadIconPalette(paletteId);
			Color[] array2 = ImageProcessor.LoadPalette(array, false);
			Bitmap bitmap = ImageProcessor.LoadSprite(ref iconData, array2, 32, 64, false);
			bool flag = this.picPokemonIcon.Image != null;
			if (flag)
			{
				this.picPokemonIcon.Image.Dispose();
			}
			this.picPokemonIcon.Image = bitmap;
			this.picPokemonIcon.Refresh();
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x00050C74 File Offset: 0x0004EE74
		private void LoadFootprintAddress(PokemonData pokemonData)
		{
			bool flag = pokemonData.Index >= this.NO_FOOTPRINT_START_INDEX;
			checked
			{
				if (flag)
				{
					pokemonData.FootprintAddress = 0U;
				}
				else
				{
					int num = this.FOOTPRINT_TABLE_OFFSET + pokemonData.Index * 4;
					uint num2 = BitConverter.ToUInt32(this.romData, num);
					pokemonData.FootprintAddress = num2 - 134217728U;
				}
			}
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x00050CD0 File Offset: 0x0004EED0
		private void UpdateFootprintAddressUI(PokemonData pokemonData)
		{
			bool flag = pokemonData.Index >= this.NO_FOOTPRINT_START_INDEX;
			if (flag)
			{
				this.txtPokemonFootPrintPointer.Text = string.Empty;
				this.SetFootprintControlsEnabled(false);
			}
			else
			{
				this.txtPokemonFootPrintPointer.Text = pokemonData.FootprintAddress.ToString("X8");
				this.SetFootprintControlsEnabled(true);
			}
		}

		// Token: 0x06000B27 RID: 2855 RVA: 0x00050D38 File Offset: 0x0004EF38
		private void DisplayPokemonFootprint(PokemonData pokemonData)
		{
			bool flag = pokemonData.Index >= this.NO_FOOTPRINT_START_INDEX;
			if (flag)
			{
				bool flag2 = this.picPokemonFootPrint.Image != null;
				if (flag2)
				{
					this.picPokemonFootPrint.Image.Dispose();
					this.picPokemonFootPrint.Image = null;
				}
				this.SetFootprintControlsEnabled(false);
			}
			else
			{
				this.SetFootprintControlsEnabled(true);
				byte[] array = new byte[32];
				Array.Copy(this.romData, (long)((ulong)pokemonData.FootprintAddress), array, 0L, 32L);
				using (Bitmap bitmap = ImageProcessor.Decode1BppFootprintSprite(ref array, new Color[]
				{
					Color.Transparent,
					Color.Black
				}))
				{
					this.picPokemonFootPrint.Image = new Bitmap(bitmap);
					this.picPokemonFootPrint.SizeMode = PictureBoxSizeMode.Zoom;
					this.picPokemonFootPrint.Refresh();
				}
			}
		}

		// Token: 0x06000B28 RID: 2856 RVA: 0x00050E3C File Offset: 0x0004F03C
		private void SetFootprintControlsEnabled(bool enabled)
		{
			this.txtPokemonFootPrintPointer.Enabled = enabled;
			this.txtPokemonFootPrintPointer.Text = (enabled ? this.txtPokemonFootPrintPointer.Text : string.Empty);
			this.btnChangePokemonFootPrint.Enabled = enabled;
			this.btnPokemonFootPrintImport.Enabled = enabled;
			this.btnPokemonFootPrintExport.Enabled = enabled;
			bool flag = !enabled;
			if (flag)
			{
				bool flag2 = this.picPokemonFootPrint.Image != null;
				if (flag2)
				{
					this.picPokemonFootPrint.Image.Dispose();
					this.picPokemonFootPrint.Image = null;
				}
			}
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x00050EDC File Offset: 0x0004F0DC
		private void btnChangePokemonFootPrint_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			uint num = Convert.ToUInt32(this.txtPokemonFootPrintPointer.Text, 16);
			bool flag = pokemonData.FootprintAddress != num;
			bool flag2 = flag;
			if (flag2)
			{
				pokemonData.FootprintAddress = num;
				this.DisplayPokemonFootprint(pokemonData);
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B2A RID: 2858 RVA: 0x00050F44 File Offset: 0x0004F144
		private void SaveFootprintAddress(PokemonData pokemonData)
		{
			checked
			{
				int num = this.FOOTPRINT_TABLE_OFFSET + this.currentPokemonIndex * 4;
				uint num2 = pokemonData.FootprintAddress + 134217728U;
				byte[] bytes = BitConverter.GetBytes(num2);
				Array.Copy(bytes, 0, this.romData, num, 4);
			}
		}

		// Token: 0x06000B2B RID: 2859 RVA: 0x00050F88 File Offset: 0x0004F188
		private void btnPokemonFootPrintImport_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "PNG画像|*.png|バイナリファイル|*.bin";
				openFileDialog.Title = "足跡画像を選択";
				bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					string text = Path.GetExtension(openFileDialog.FileName).ToLower();
					byte[] array = null;
					bool flag2 = Operators.CompareString(text, ".bin", false) == 0;
					if (flag2)
					{
						array = File.ReadAllBytes(openFileDialog.FileName);
						bool flag3 = array.Length != 32;
						if (flag3)
						{
							MessageBox.Show(string.Format("ファイルサイズが不正です: {0} bytes (規定値: {1} bytes)", array.Length, 32), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							return;
						}
					}
					else
					{
						using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
						{
							bool flag4 = bitmap.Width != 16 || bitmap.Height != 16;
							if (flag4)
							{
								MessageBox.Show(string.Format("画像サイズは{0}x{1}である必要があります", 16, 16), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								return;
							}
							bool flag5 = bitmap.PixelFormat != PixelFormat.Format1bppIndexed;
							if (flag5)
							{
								MessageBox.Show("1bppのインデックスカラー画像のみ対応しています。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								return;
							}
							bool flag6 = bitmap.Palette.Entries.Length != 2;
							if (flag6)
							{
								MessageBox.Show("パレットは2色である必要があります。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								return;
							}
							array = ImageProcessor.EncodeImageToFootprintData(bitmap);
						}
					}
					PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
					pokemonData.TemporaryFootprintData = array;
					this.DisplayFootprintFromData(array);
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x00051188 File Offset: 0x0004F388
		private void DisplayFootprintFromData(byte[] footprintData)
		{
			using (Bitmap bitmap = ImageProcessor.Decode1BppFootprintSprite(ref footprintData, new Color[]
			{
				Color.Transparent,
				Color.Black
			}))
			{
				this.picPokemonFootPrint.Image = new Bitmap(bitmap);
				this.picPokemonFootPrint.SizeMode = PictureBoxSizeMode.Zoom;
				this.picPokemonFootPrint.Refresh();
			}
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x00051208 File Offset: 0x0004F408
		private void btnPokemonFootPrintExport_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.FileName = string.Format("footprint_{0:X4}", this.currentPokemonIndex);
				saveFileDialog.Filter = "PNG画像|*.png|バイナリファイル|*.bin";
				saveFileDialog.Title = "足跡画像をエクスポート";
				bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					this.ExportFootprintData(saveFileDialog.FileName, pokemonData);
				}
			}
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x000512A0 File Offset: 0x0004F4A0
		private void ExportFootprintData(string filePath, PokemonData pokemonData)
		{
			byte[] array = new byte[32];
			Array.Copy(this.romData, (long)((ulong)pokemonData.FootprintAddress), array, 0L, 32L);
			string text = Path.GetExtension(filePath).ToLower();
			bool flag = Operators.CompareString(text, ".bin", false) == 0;
			if (flag)
			{
				File.WriteAllBytes(filePath, array);
			}
			else
			{
				this.ExportFootprintAsPng(filePath, array);
			}
		}

		// Token: 0x06000B2F RID: 2863 RVA: 0x00051304 File Offset: 0x0004F504
		private void ExportFootprintAsPng(string filePath, byte[] footprintData)
		{
			using (Bitmap bitmap = ImageProcessor.Decode1BppFootprintSprite(ref footprintData, new Color[]
			{
				Color.FromArgb(255, 248, 248, 248),
				Color.Black
			}))
			{
				using (Bitmap bitmap2 = this.ConvertTo1bppBitmapWithCustomWhite(bitmap))
				{
					bitmap2.Save(filePath, ImageFormat.Png);
				}
			}
		}

		// Token: 0x06000B30 RID: 2864 RVA: 0x000513A0 File Offset: 0x0004F5A0
		private Bitmap ConvertTo1bppBitmapWithCustomWhite(Bitmap sourceImage)
		{
			int width = sourceImage.Width;
			int height = sourceImage.Height;
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
			ColorPalette palette = bitmap.Palette;
			palette.Entries[0] = Color.FromArgb(255, 248, 248, 248);
			palette.Entries[1] = Color.Black;
			bitmap.Palette = palette;
			Rectangle rectangle = new Rectangle(0, 0, width, height);
			BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
			int stride = bitmapData.Stride;
			checked
			{
				byte[] array = new byte[stride * height - 1 + 1];
				int num = height - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = i * stride;
					int num3 = 7;
					byte b = 0;
					int num4 = width - 1;
					for (int j = 0; j <= num4; j++)
					{
						Color pixel = sourceImage.GetPixel(j, i);
						int num5 = (int)((pixel.R + pixel.G + pixel.B) / 3);
						bool flag = num5 < 128;
						if (flag)
						{
							b = (byte)((int)b | (1 << num3));
						}
						num3--;
						bool flag2 = num3 < 0;
						if (flag2)
						{
							array[num2] = b;
							num2++;
							num3 = 7;
							b = 0;
						}
					}
					bool flag3 = num3 != 7;
					if (flag3)
					{
						array[num2] = b;
					}
				}
				Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
				return bitmap;
			}
		}

		// Token: 0x06000B31 RID: 2865 RVA: 0x0005151F File Offset: 0x0004F71F
		private void YPosition_ValueChanged(object sender, EventArgs e)
		{
			this.UpdateBattleDisplay(RuntimeHelpers.GetObjectValue(sender), e);
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x00051540 File Offset: 0x0004F740
		private void LoadYPositionData(PokemonData pokemonData)
		{
			int num = checked(this.BACK_Y_TABLE_OFFSET + pokemonData.Index * 4);
			byte b = this.romData[num];
			this.nudPlayerBubbleXYPosition1.Value = new decimal((int)((byte)((uint)b >> 4) & 15));
			this.nudPlayerBubbleXYPosition2.Value = new decimal((int)(b & 15));
			checked
			{
				byte b2 = this.romData[num + 1];
				this.nudPlayerPokemonYPosition.Value = new decimal((b2 >= 128) ? ((int)b2 - 256) : ((int)b2));
				int num2 = this.FRONT_Y_TABLE_OFFSET + pokemonData.Index * 4;
				byte b3 = this.romData[num2];
				this.nudEnemyBubbleXYPosition1.Value = new decimal((int)(unchecked((byte)((uint)b3 >> 4)) & 15));
				this.nudEnemyBubbleXYPosition2.Value = new decimal((int)(b3 & 15));
				byte b4 = this.romData[num2 + 1];
				this.nudEnemyPokemonYPosition.Value = new decimal((b4 >= 128) ? ((int)b4 - 256) : ((int)b4));
				int num3 = this.SHADOW_TABLE_OFFSET + pokemonData.Index;
				byte b5 = this.romData[num3];
				this.nudEnemyShadowYPosition.Value = new decimal((b5 >= 128) ? ((int)b5 - 256) : ((int)b5));
			}
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x00051680 File Offset: 0x0004F880
		private void SaveYPositionData(PokemonData pokemonData)
		{
			checked
			{
				int num = this.BACK_Y_TABLE_OFFSET + pokemonData.Index * 4;
				byte b = (byte)((Convert.ToInt32(this.nudPlayerBubbleXYPosition1.Value) << 4) | Convert.ToInt32(this.nudPlayerBubbleXYPosition2.Value));
				this.romData[num] = b;
				this.romData[num + 1] = (byte)(Convert.ToInt32(this.nudPlayerPokemonYPosition.Value) & 255);
				int num2 = this.FRONT_Y_TABLE_OFFSET + pokemonData.Index * 4;
				byte b2 = (byte)((Convert.ToInt32(this.nudEnemyBubbleXYPosition1.Value) << 4) | Convert.ToInt32(this.nudEnemyBubbleXYPosition2.Value));
				this.romData[num2] = b2;
				this.romData[num2 + 1] = (byte)(Convert.ToInt32(this.nudEnemyPokemonYPosition.Value) & 255);
				int num3 = this.SHADOW_TABLE_OFFSET + pokemonData.Index;
				this.romData[num3] = (byte)(Convert.ToInt32(this.nudEnemyShadowYPosition.Value) & 255);
			}
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x0005177C File Offset: 0x0004F97C
		private void LoadBattleImages()
		{
			this.battleBackgroundImage = (Bitmap)Image.FromFile("img/BattleBackGround.png");
			this.battleShadowImage = (Bitmap)Image.FromFile("img/BattleBackGroundShadow.png");
			this.battleShadowImage.MakeTransparent();
			this.battleBubbleImage = (Bitmap)Image.FromFile("img/BattleBubble.png");
			this.battleBubbleImage.MakeTransparent();
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x000517E4 File Offset: 0x0004F9E4
		private void UpdateBattleDisplay(object sender, EventArgs e)
		{
			Bitmap bitmap = new Bitmap(this.picBattleBackGround.Width, this.picBattleBackGround.Height);
			checked
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.SmoothingMode = SmoothingMode.None;
					graphics.PixelOffsetMode = PixelOffsetMode.Half;
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					bool flag = this.battleBackgroundImage != null;
					if (flag)
					{
						graphics.DrawImage(this.battleBackgroundImage, 0, 0, this.picBattleBackGround.Width, this.picBattleBackGround.Height);
					}
					bool flag2 = this.picBackNormal.Image != null;
					if (flag2)
					{
						int num = Convert.ToInt32(decimal.Add(48m, this.nudPlayerPokemonYPosition.Value));
						graphics.DrawImage(this.picBackNormal.Image, 40, num, this.picBackNormal.Width, this.picBackNormal.Height);
					}
					bool flag3 = decimal.Compare(this.nudEnemyShadowYPosition.Value, 0m) != 0 && this.battleShadowImage != null;
					if (flag3)
					{
						graphics.DrawImage(this.battleShadowImage, 160, 65, 32, 8);
					}
					bool flag4 = this.picFrontNormal.Image != null;
					if (flag4)
					{
						int num2 = Convert.ToInt32(decimal.Add(8m, this.nudEnemyPokemonYPosition.Value));
						bool flag5 = decimal.Compare(this.nudEnemyShadowYPosition.Value, 0m) != 0;
						if (flag5)
						{
							num2 -= Convert.ToInt32(this.nudEnemyShadowYPosition.Value);
						}
						graphics.DrawImage(this.picFrontNormal.Image, 144, num2, this.picFrontNormal.Width, this.picFrontNormal.Height);
					}
					bool flag6 = this.chkShowBubbleSprite.Checked && this.battleBubbleImage != null;
					if (flag6)
					{
						int num3 = Convert.ToInt32(this.nudPlayerBubbleXYPosition1.Value);
						int num4 = Convert.ToInt32(this.nudPlayerBubbleXYPosition2.Value);
						int num5 = (int)Math.Round(unchecked((double)(checked(num3 * 8)) / 2.0 + 56.0));
						int num6 = (int)Math.Round(unchecked(72.0 - (double)(checked(num4 * 8)) / 2.0 + (double)Convert.ToInt32(this.nudPlayerPokemonYPosition.Value)));
						int num7 = Convert.ToInt32(this.nudEnemyBubbleXYPosition1.Value);
						int num8 = Convert.ToInt32(this.nudEnemyBubbleXYPosition2.Value);
						int num9 = (int)Math.Round(unchecked(160.0 - (double)(checked(num7 * 8)) / 2.0));
						int num10 = (int)Math.Round(unchecked(24.0 - (double)(checked(num8 * 8)) / 2.0 + (double)Convert.ToInt32(this.nudEnemyPokemonYPosition.Value) - (double)Convert.ToInt32(this.nudEnemyShadowYPosition.Value)));
						graphics.DrawImage(this.battleBubbleImage, num5, num6);
						graphics.DrawImage(this.battleBubbleImage, num9, num10);
					}
				}
				this.picBattleBackGround.Image = bitmap;
			}
		}

		// Token: 0x06000B36 RID: 2870 RVA: 0x00051B20 File Offset: 0x0004FD20
		private void chkShowBubbleSprite_CheckedChanged(object sender, EventArgs e)
		{
			this.UpdateBattleDisplay(RuntimeHelpers.GetObjectValue(sender), e);
		}

		// Token: 0x06000B37 RID: 2871 RVA: 0x00051B34 File Offset: 0x0004FD34
		private void LoadPokemonStats(PokemonData pokemonData)
		{
			int num = checked(this.BASE_STATS_OFFSET + pokemonData.Index * this.BASE_STATS_ENTRY_LENGTH);
			this.LoadBaseStats(num);
			this.LoadEVs(num);
			this.LoadGenderValue(pokemonData, num);
			this.LoadEggData(pokemonData, num);
			this.LoadOtherData(pokemonData, num);
			this.LoadAbilities(pokemonData, num);
			this.LoadHoldItems(pokemonData, num);
			this.LoadTypes(pokemonData, num);
		}

		// Token: 0x06000B38 RID: 2872 RVA: 0x00051BA0 File Offset: 0x0004FDA0
		private void SavePokemonStats(PokemonData pokemonData)
		{
			int num = checked(this.BASE_STATS_OFFSET + pokemonData.Index * this.BASE_STATS_ENTRY_LENGTH);
			this.SaveBaseStats(num);
			this.SaveEVs(num);
			this.SaveGenderValue(pokemonData, num);
			this.SaveEggData(pokemonData, num);
			this.SaveOtherData(pokemonData, num);
			this.SaveAbilities(pokemonData, num);
			this.SaveHoldItems(pokemonData, num);
			this.SaveTypes(pokemonData, num);
		}

		// Token: 0x06000B39 RID: 2873 RVA: 0x00051C0C File Offset: 0x0004FE0C
		private void LoadBaseStats(int offset)
		{
			checked
			{
				this.nudBaseStatsHp.Value = new decimal((int)this.romData[offset + 0]);
				this.nudBaseStatsAttack.Value = new decimal((int)this.romData[offset + 1]);
				this.nudBaseStatsDefense.Value = new decimal((int)this.romData[offset + 2]);
				this.nudBaseStatsSpAttack.Value = new decimal((int)this.romData[offset + 4]);
				this.nudBaseStatsSpDefense.Value = new decimal((int)this.romData[offset + 5]);
				this.nudBaseStatsSpeed.Value = new decimal((int)this.romData[offset + 3]);
			}
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x00051CBC File Offset: 0x0004FEBC
		private void SaveBaseStats(int offset)
		{
			checked
			{
				this.romData[offset + 0] = Convert.ToByte(this.nudBaseStatsHp.Value);
				this.romData[offset + 1] = Convert.ToByte(this.nudBaseStatsAttack.Value);
				this.romData[offset + 2] = Convert.ToByte(this.nudBaseStatsDefense.Value);
				this.romData[offset + 4] = Convert.ToByte(this.nudBaseStatsSpAttack.Value);
				this.romData[offset + 5] = Convert.ToByte(this.nudBaseStatsSpDefense.Value);
				this.romData[offset + 3] = Convert.ToByte(this.nudBaseStatsSpeed.Value);
			}
		}

		// Token: 0x06000B3B RID: 2875 RVA: 0x00051D66 File Offset: 0x0004FF66
		private void BaseStats_ValueChanged(object sender, EventArgs e)
		{
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x00051D78 File Offset: 0x0004FF78
		private void LoadEVs(int offset)
		{
			int num = checked(offset + 10);
			ushort num2 = BitConverter.ToUInt16(this.romData, num);
			byte[] array = this.DecodeEV(num2);
			this.nudEvHp.Value = new decimal((int)array[0]);
			this.nudEvAttack.Value = new decimal((int)array[1]);
			this.nudEvDefense.Value = new decimal((int)array[2]);
			this.nudEvSpAttack.Value = new decimal((int)array[3]);
			this.nudEvSpDefense.Value = new decimal((int)array[4]);
			this.nudEvSpeed.Value = new decimal((int)array[5]);
		}

		// Token: 0x06000B3D RID: 2877 RVA: 0x00051E18 File Offset: 0x00050018
		private byte[] DecodeEV(ushort evValue)
		{
			byte[] array = new byte[6];
			byte[] bytes = BitConverter.GetBytes(evValue);
			array[0] = (byte)(bytes[0] & 3);
			checked
			{
				array[1] = (byte)((bytes[0] & 12) >> 2);
				array[2] = (byte)((bytes[0] & 48) >> 4);
				array[5] = (byte)((bytes[0] & 192) >> 6);
				array[3] = (byte)(bytes[1] & 3);
				array[4] = (byte)((bytes[1] & 12) >> 2);
				return array;
			}
		}

		// Token: 0x06000B3E RID: 2878 RVA: 0x00051E80 File Offset: 0x00050080
		private ushort EncodeEV(byte[] evs)
		{
			byte b = 0;
			byte b2 = 0;
			b |= (byte)(evs[0] & 3);
			checked
			{
				b = (byte)((int)b | ((int)(evs[1] & 3) << 2));
				b = (byte)((int)b | ((int)(evs[2] & 3) << 4));
				b = (byte)((int)b | ((int)(evs[5] & 3) << 6));
				b2 |= (byte)(evs[3] & 3);
				b2 = (byte)((int)b2 | ((int)(evs[4] & 3) << 2));
				return BitConverter.ToUInt16(new byte[] { b, b2 }, 0);
			}
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x00051EE8 File Offset: 0x000500E8
		private void SaveEVs(int offset)
		{
			ushort num = this.EncodeEV(new byte[]
			{
				Convert.ToByte(this.nudEvHp.Value),
				Convert.ToByte(this.nudEvAttack.Value),
				Convert.ToByte(this.nudEvDefense.Value),
				Convert.ToByte(this.nudEvSpAttack.Value),
				Convert.ToByte(this.nudEvSpDefense.Value),
				Convert.ToByte(this.nudEvSpeed.Value)
			});
			byte[] bytes = BitConverter.GetBytes(num);
			checked
			{
				int num2 = offset + 10;
				this.romData[num2] = bytes[0];
				this.romData[num2 + 1] = bytes[1];
			}
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x00051F9B File Offset: 0x0005019B
		private void EVs_ValueChanged(object sender, EventArgs e)
		{
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x00051FAC File Offset: 0x000501AC
		private void InitializeGenderComboBox()
		{
			this.genderMapping = new Dictionary<byte, string>
			{
				{ 0, "♂のみ" },
				{ 31, "♂:87.5% / ♀:12.5%" },
				{ 63, "♂:75% / ♀:25%" },
				{ 127, "♂:50% / ♀:50%" },
				{ 191, "♂:25% / ♀:75%" },
				{ 223, "♂:12.5% / ♀:87.5%" },
				{ 254, "♀のみ" },
				{ byte.MaxValue, "ふめい" }
			};
			this.cmbGender.DataSource = new BindingSource(this.genderMapping, null);
			this.cmbGender.DisplayMember = "Value";
			this.cmbGender.ValueMember = "Key";
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x0005207C File Offset: 0x0005027C
		private void LoadGenderValue(PokemonData pokemonData, int offset)
		{
			int num = checked(offset + 16);
			byte b = this.romData[num];
			this.cmbGender.SelectedValue = b;
			pokemonData.OriginalGenderValue = b;
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x000520B4 File Offset: 0x000502B4
		private void SaveGenderValue(PokemonData pokemonData, int offset)
		{
			int num = checked(offset + 16);
			this.romData[num] = Conversions.ToByte(this.cmbGender.SelectedValue);
			pokemonData.OriginalGenderValue = Conversions.ToByte(this.cmbGender.SelectedValue);
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x000520F8 File Offset: 0x000502F8
		private void cmbGender_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				object selectedItem = this.cmbGender.SelectedItem;
				byte key = ((selectedItem != null) ? ((KeyValuePair<byte, string>)selectedItem) : default(KeyValuePair<byte, string>)).Key;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = key != pokemonData.OriginalGenderValue;
				if (flag2)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x0005217C File Offset: 0x0005037C
		private void InitializeEggStepComboBox()
		{
			this.eggStepMapping = new Dictionary<byte, string>
			{
				{ 5, "1280歩(サイクル5)" },
				{ 10, "2560歩(サイクル10)" },
				{ 15, "3840歩(サイクル15)" },
				{ 20, "5120歩(サイクル20)" },
				{ 25, "6400歩(サイクル25)" },
				{ 30, "7680歩(サイクル30)" },
				{ 35, "8960歩(サイクル35)" },
				{ 40, "10240歩(サイクル40)" },
				{ 120, "-(サイクル120)" }
			};
			this.cmbEggStep.DataSource = new BindingSource(this.eggStepMapping, null);
			this.cmbEggStep.DisplayMember = "Value";
			this.cmbEggStep.ValueMember = "Key";
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x0005224C File Offset: 0x0005044C
		private void InitializeEggGroupComboBoxes()
		{
			this.eggGroupMapping = new Dictionary<byte, string>
			{
				{ 1, "怪獣" },
				{ 10, "鉱物" },
				{ 7, "植物" },
				{ 2, "水中1" },
				{ 12, "水中2" },
				{ 9, "水中3" },
				{ 14, "ドラゴン" },
				{ 4, "飛行" },
				{ 8, "人型" },
				{ 11, "不定形" },
				{ 3, "虫" },
				{ 6, "妖精" },
				{ 5, "陸上" },
				{ 13, "メタモン" },
				{ 15, "タマゴ未発見" }
			};
			this.cmbEggGroup1.DataSource = new BindingSource(this.eggGroupMapping, null);
			this.cmbEggGroup1.DisplayMember = "Value";
			this.cmbEggGroup1.ValueMember = "Key";
			this.cmbEggGroup2.DataSource = new BindingSource(this.eggGroupMapping, null);
			this.cmbEggGroup2.DisplayMember = "Value";
			this.cmbEggGroup2.ValueMember = "Key";
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x000523A4 File Offset: 0x000505A4
		private void LoadEggData(PokemonData pokemonData, int offset)
		{
			checked
			{
				int num = offset + 17;
				byte b = this.romData[num];
				this.cmbEggStep.SelectedValue = b;
				pokemonData.OriginalEggStepValue = Conversions.ToByte(this.cmbEggStep.SelectedValue);
				int num2 = offset + 20;
				byte b2 = this.romData[num2];
				this.cmbEggGroup1.SelectedValue = b2;
				pokemonData.OriginalEggGroup1Value = Conversions.ToByte(this.cmbEggGroup1.SelectedValue);
				int num3 = offset + 21;
				byte b3 = this.romData[num3];
				this.cmbEggGroup2.SelectedValue = b3;
				pokemonData.OriginalEggGroup2Value = Conversions.ToByte(this.cmbEggGroup2.SelectedValue);
			}
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x0005245C File Offset: 0x0005065C
		private void SaveEggData(PokemonData pokemonData, int offset)
		{
			byte b = Conversions.ToByte(this.cmbEggStep.SelectedValue);
			checked
			{
				this.romData[offset + 17] = b;
				pokemonData.OriginalEggStepValue = Conversions.ToByte(this.cmbEggStep.SelectedValue);
				byte b2 = Conversions.ToByte(this.cmbEggGroup1.SelectedValue);
				this.romData[offset + 20] = b2;
				pokemonData.OriginalEggGroup1Value = Conversions.ToByte(this.cmbEggGroup1.SelectedValue);
				byte b3 = Conversions.ToByte(this.cmbEggGroup2.SelectedValue);
				this.romData[offset + 21] = b3;
				pokemonData.OriginalEggGroup2Value = Conversions.ToByte(this.cmbEggGroup2.SelectedValue);
			}
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x00052508 File Offset: 0x00050708
		private void EggStep_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				object selectedItem = this.cmbEggStep.SelectedItem;
				byte key = ((selectedItem != null) ? ((KeyValuePair<byte, string>)selectedItem) : default(KeyValuePair<byte, string>)).Key;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = key != pokemonData.OriginalEggStepValue;
				if (flag2)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x0005258C File Offset: 0x0005078C
		private void EggGroup_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				ComboBox comboBox = (ComboBox)sender;
				object selectedItem = comboBox.SelectedItem;
				byte key = ((selectedItem != null) ? ((KeyValuePair<byte, string>)selectedItem) : default(KeyValuePair<byte, string>)).Key;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = comboBox == this.cmbEggGroup1;
				byte b;
				if (flag2)
				{
					b = pokemonData.OriginalEggGroup1Value;
				}
				else
				{
					b = pokemonData.OriginalEggGroup2Value;
				}
				bool flag3 = key != b;
				if (flag3)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x00052634 File Offset: 0x00050834
		private void InitializeGrowthRateComboBox()
		{
			this.growthRateMapping = new Dictionary<byte, string>
			{
				{ 1, "60万" },
				{ 4, "80万" },
				{ 0, "100万" },
				{ 3, "105万" },
				{ 5, "125万" },
				{ 2, "164万" }
			};
			this.cmbGrowthRate.DataSource = new BindingSource(this.growthRateMapping, null);
			this.cmbGrowthRate.DisplayMember = "Value";
			this.cmbGrowthRate.ValueMember = "Key";
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x000526D8 File Offset: 0x000508D8
		private void InitializePokemonColorComboBox()
		{
			this.pokemonColorMapping = new Dictionary<byte, string>
			{
				{ 0, "赤" },
				{ 1, "青" },
				{ 2, "黄" },
				{ 3, "緑" },
				{ 4, "黒" },
				{ 5, "茶" },
				{ 6, "紫" },
				{ 7, "灰" },
				{ 8, "白" },
				{ 9, "桃" }
			};
			this.cmbPokemonColor.DataSource = new BindingSource(this.pokemonColorMapping, null);
			this.cmbPokemonColor.DisplayMember = "Value";
			this.cmbPokemonColor.ValueMember = "Key";
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x000527B0 File Offset: 0x000509B0
		private void InitializePokemonDirectionComboBox()
		{
			this.pokemonDirectionMapping = new Dictionary<byte, string>
			{
				{ 1, "左" },
				{ 0, "右" }
			};
			this.cmbPokemonDirection.DataSource = new BindingSource(this.pokemonDirectionMapping, null);
			this.cmbPokemonDirection.DisplayMember = "Value";
			this.cmbPokemonDirection.ValueMember = "Key";
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x00052820 File Offset: 0x00050A20
		private void LoadOtherData(PokemonData pokemonData, int offset)
		{
			checked
			{
				this.nudCatchRate.Value = new decimal((int)this.romData[offset + 8]);
				this.nudBaseHappiness.Value = new decimal((int)this.romData[offset + 18]);
				bool enable_BASE_STATS_EXPANSION = this.ENABLE_BASE_STATS_EXPANSION;
				if (enable_BASE_STATS_EXPANSION)
				{
					this.nudBaseExp.Value = new decimal((int)BitConverter.ToUInt16(this.romData, offset + 30));
				}
				else
				{
					this.nudBaseExp.Value = new decimal((int)this.romData[offset + 9]);
				}
				byte b = this.romData[offset + 19];
				this.cmbGrowthRate.SelectedValue = b;
				pokemonData.OriginalGrowthRateValue = Conversions.ToByte(this.cmbGrowthRate.SelectedValue);
				this.nudRunRate.Value = new decimal((int)this.romData[offset + 24]);
				byte b2 = this.romData[offset + 25];
				byte b3 = (byte)(b2 & 15);
				this.cmbPokemonColor.SelectedValue = b3;
				pokemonData.OriginalPokemonColorValue = Conversions.ToByte(this.cmbPokemonColor.SelectedValue);
				byte b4 = (byte)((b2 & 128) >> 7);
				this.cmbPokemonDirection.SelectedValue = b4;
				pokemonData.OriginalPokemonDirectionValue = Conversions.ToByte(this.cmbPokemonDirection.SelectedValue);
			}
		}

		// Token: 0x06000B4F RID: 2895 RVA: 0x00052974 File Offset: 0x00050B74
		private void SaveOtherData(PokemonData pokemonData, int offset)
		{
			checked
			{
				this.romData[offset + 8] = Convert.ToByte(this.nudCatchRate.Value);
				this.romData[offset + 18] = Convert.ToByte(this.nudBaseHappiness.Value);
				bool enable_BASE_STATS_EXPANSION = this.ENABLE_BASE_STATS_EXPANSION;
				if (enable_BASE_STATS_EXPANSION)
				{
					byte[] bytes = BitConverter.GetBytes(Convert.ToUInt16(this.nudBaseExp.Value));
					this.romData[offset + 30] = bytes[0];
					this.romData[offset + 30 + 1] = bytes[1];
				}
				else
				{
					this.romData[offset + 9] = Convert.ToByte(this.nudBaseExp.Value);
				}
				this.romData[offset + 19] = Conversions.ToByte(this.cmbGrowthRate.SelectedValue);
				pokemonData.OriginalGrowthRateValue = Conversions.ToByte(this.cmbGrowthRate.SelectedValue);
				this.romData[offset + 24] = Convert.ToByte(this.nudRunRate.Value);
				byte b = Conversions.ToByte(this.cmbPokemonColor.SelectedValue);
				byte b2 = Conversions.ToByte(this.cmbPokemonDirection.SelectedValue);
				byte b3 = (byte)(unchecked((byte)(b2 << 7)) | b);
				this.romData[offset + 25] = b3;
				pokemonData.OriginalPokemonColorValue = Conversions.ToByte(this.cmbPokemonColor.SelectedValue);
				pokemonData.OriginalPokemonDirectionValue = Conversions.ToByte(this.cmbPokemonDirection.SelectedValue);
			}
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x00052AC8 File Offset: 0x00050CC8
		private void OtherData_Changed(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = false;
				bool flag3 = sender == this.cmbGrowthRate;
				if (flag3)
				{
					object selectedItem = this.cmbGrowthRate.SelectedItem;
					bool flag4 = ((selectedItem != null) ? ((KeyValuePair<byte, string>)selectedItem) : default(KeyValuePair<byte, string>)).Key != pokemonData.OriginalGrowthRateValue;
					if (flag4)
					{
						flag2 = true;
					}
				}
				else
				{
					bool flag5 = sender == this.cmbPokemonColor;
					if (flag5)
					{
						object selectedItem2 = this.cmbPokemonColor.SelectedItem;
						bool flag6 = ((selectedItem2 != null) ? ((KeyValuePair<byte, string>)selectedItem2) : default(KeyValuePair<byte, string>)).Key != pokemonData.OriginalPokemonColorValue;
						if (flag6)
						{
							flag2 = true;
						}
					}
					else
					{
						bool flag7 = sender == this.cmbPokemonDirection;
						if (flag7)
						{
							object selectedItem3 = this.cmbPokemonDirection.SelectedItem;
							bool flag8 = ((selectedItem3 != null) ? ((KeyValuePair<byte, string>)selectedItem3) : default(KeyValuePair<byte, string>)).Key != pokemonData.OriginalPokemonDirectionValue;
							if (flag8)
							{
								flag2 = true;
							}
						}
						else
						{
							flag2 = true;
						}
					}
				}
				bool flag9 = flag2;
				if (flag9)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x00052C10 File Offset: 0x00050E10
		private string GetAbilityNameFromRom(int abilityId)
		{
			checked
			{
				int num = this.ABILITY_NAME_TABLE_OFFSET + abilityId * this.ABILITY_NAME_LENGTH;
				byte[] array = new byte[this.ABILITY_NAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num, array, 0, this.ABILITY_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, this.ABILITY_NAME_LENGTH);
			}
		}

		// Token: 0x06000B52 RID: 2898 RVA: 0x00052C64 File Offset: 0x00050E64
		private void InitializeAbilityComboBoxes()
		{
			this.cmbAbility1.Items.Clear();
			this.cmbAbility2.Items.Clear();
			this.cmbAbilityHidden.Items.Clear();
			checked
			{
				int num = this.TOTAL_ABILITY_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					string abilityNameFromRom = this.GetAbilityNameFromRom(i);
					this.cmbAbility1.Items.Add(abilityNameFromRom);
					this.cmbAbility2.Items.Add(abilityNameFromRom);
					this.cmbAbilityHidden.Items.Add(abilityNameFromRom);
				}
			}
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x00052CF8 File Offset: 0x00050EF8
		private void LoadAbilities(PokemonData pokemonData, int offset)
		{
			bool enable_BASE_STATS_EXPANSION = this.ENABLE_BASE_STATS_EXPANSION;
			checked
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				if (enable_BASE_STATS_EXPANSION)
				{
					num = (int)BitConverter.ToUInt16(this.romData, offset + 22);
					num2 = (int)BitConverter.ToUInt16(this.romData, offset + 26);
					num3 = (int)BitConverter.ToUInt16(this.romData, offset + 28);
				}
				else
				{
					num = (int)this.romData[offset + 22];
					num2 = (int)this.romData[offset + 23];
					num3 = (int)this.romData[offset + 26];
				}
				bool flag = this.cmbAbility1.Items.Count > num;
				if (flag)
				{
					this.cmbAbility1.SelectedIndex = num;
				}
				pokemonData.OriginalAbility1Id = num;
				bool flag2 = this.cmbAbility2.Items.Count > num2;
				if (flag2)
				{
					this.cmbAbility2.SelectedIndex = num2;
				}
				pokemonData.OriginalAbility2Id = num2;
				bool flag3 = this.cmbAbilityHidden.Items.Count > num3;
				if (flag3)
				{
					this.cmbAbilityHidden.SelectedIndex = num3;
				}
				pokemonData.OriginalAbilityHiddenId = num3;
			}
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x00052DF4 File Offset: 0x00050FF4
		private void SaveAbilities(PokemonData pokemonData, int offset)
		{
			bool enable_BASE_STATS_EXPANSION = this.ENABLE_BASE_STATS_EXPANSION;
			checked
			{
				if (enable_BASE_STATS_EXPANSION)
				{
					byte[] bytes = BitConverter.GetBytes((ushort)this.cmbAbility1.SelectedIndex);
					this.romData[offset + 22] = bytes[0];
					this.romData[offset + 22 + 1] = bytes[1];
					byte[] bytes2 = BitConverter.GetBytes((ushort)this.cmbAbility2.SelectedIndex);
					this.romData[offset + 26] = bytes2[0];
					this.romData[offset + 26 + 1] = bytes2[1];
					byte[] bytes3 = BitConverter.GetBytes((ushort)this.cmbAbilityHidden.SelectedIndex);
					this.romData[offset + 28] = bytes3[0];
					this.romData[offset + 28 + 1] = bytes3[1];
				}
				else
				{
					this.romData[offset + 22] = (byte)this.cmbAbility1.SelectedIndex;
					this.romData[offset + 23] = (byte)this.cmbAbility2.SelectedIndex;
					this.romData[offset + 26] = (byte)this.cmbAbilityHidden.SelectedIndex;
				}
				pokemonData.OriginalAbility1Id = this.cmbAbility1.SelectedIndex;
				pokemonData.OriginalAbility2Id = this.cmbAbility2.SelectedIndex;
				pokemonData.OriginalAbilityHiddenId = this.cmbAbilityHidden.SelectedIndex;
			}
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x00052F20 File Offset: 0x00051120
		private void Abilities_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				ComboBox comboBox = (ComboBox)sender;
				int selectedIndex = comboBox.SelectedIndex;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = false;
				string name = comboBox.Name;
				if (Operators.CompareString(name, "cmbAbility1", false) != 0)
				{
					if (Operators.CompareString(name, "cmbAbility2", false) != 0)
					{
						if (Operators.CompareString(name, "cmbAbilityHidden", false) == 0)
						{
							flag2 = selectedIndex != pokemonData.OriginalAbilityHiddenId;
						}
					}
					else
					{
						flag2 = selectedIndex != pokemonData.OriginalAbility2Id;
					}
				}
				else
				{
					flag2 = selectedIndex != pokemonData.OriginalAbility1Id;
				}
				bool flag3 = flag2;
				if (flag3)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x00052FF0 File Offset: 0x000511F0
		private void InitializeItemComboBoxes()
		{
			this.cmbHoldItem1.BeginUpdate();
			this.cmbHoldItem2.BeginUpdate();
			this.cmbHoldItem1.Items.Clear();
			this.cmbHoldItem2.Items.Clear();
			List<string> itemNames = ItemData.GetItemNames(this.romData);
			{
				foreach (string text in itemNames)
				{
					this.cmbHoldItem1.Items.Add(text);
					this.cmbHoldItem2.Items.Add(text);
				}
			}
			this.cmbHoldItem1.EndUpdate();
			this.cmbHoldItem2.EndUpdate();
			this.cmbHoldItem1.SelectedIndex = 0;
			this.cmbHoldItem2.SelectedIndex = 0;
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x000530D8 File Offset: 0x000512D8
		private void LoadHoldItems(PokemonData pokemonData, int offset)
		{
			checked
			{
				ushort num = BitConverter.ToUInt16(this.romData, offset + 12);
				ushort num2 = BitConverter.ToUInt16(this.romData, offset + 14);
				this.cmbHoldItem1.SelectedIndex = (int)num;
				this.cmbHoldItem2.SelectedIndex = (int)num2;
				pokemonData.OriginalHoldItem1Id = (int)num;
				pokemonData.OriginalHoldItem2Id = (int)num2;
				this.UpdateItemImage(this.cmbHoldItem1, this.picHoldItem1);
				this.UpdateItemImage(this.cmbHoldItem2, this.picHoldItem2);
			}
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x00053158 File Offset: 0x00051358
		private void SaveHoldItems(PokemonData pokemonData, int offset)
		{
			checked
			{
				ushort num = (ushort)this.cmbHoldItem1.SelectedIndex;
				ushort num2 = (ushort)this.cmbHoldItem2.SelectedIndex;
				byte[] bytes = BitConverter.GetBytes(num);
				this.romData[offset + 12] = bytes[0];
				this.romData[offset + 12 + 1] = bytes[1];
				byte[] bytes2 = BitConverter.GetBytes(num2);
				this.romData[offset + 14] = bytes2[0];
				this.romData[offset + 14 + 1] = bytes2[1];
				pokemonData.OriginalHoldItem1Id = (int)num;
				pokemonData.OriginalHoldItem2Id = (int)num2;
			}
		}

		// Token: 0x06000B59 RID: 2905 RVA: 0x000531DC File Offset: 0x000513DC
		private void UpdateItemImage(ComboBox comboBox, PictureBox pictureBox)
		{
			ushort num = checked((ushort)comboBox.SelectedIndex);
			ItemData.DisplayItemImage(pictureBox, this.romData, num);
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x00053200 File Offset: 0x00051400
		private void cmbHoldItem_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				ComboBox comboBox = (ComboBox)sender;
				PictureBox pictureBox = ((comboBox == this.cmbHoldItem1) ? this.picHoldItem1 : this.picHoldItem2);
				this.UpdateItemImage(comboBox, pictureBox);
				int selectedIndex = comboBox.SelectedIndex;
				bool flag2 = false;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				string name = comboBox.Name;
				if (Operators.CompareString(name, "cmbHoldItem1", false) != 0)
				{
					if (Operators.CompareString(name, "cmbHoldItem2", false) == 0)
					{
						flag2 = selectedIndex != pokemonData.OriginalHoldItem2Id;
					}
				}
				else
				{
					flag2 = selectedIndex != pokemonData.OriginalHoldItem1Id;
				}
				bool flag3 = flag2;
				if (flag3)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x000532D4 File Offset: 0x000514D4
		private string GetTypeNameFromRom(int typeId)
		{
			checked
			{
				int num = this.TYPE_TABLE_OFFSET + typeId * this.TYPE_NAME_LENGTH;
				byte[] array = new byte[this.TYPE_NAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num, array, 0, this.TYPE_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, this.TYPE_NAME_LENGTH);
			}
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x00053328 File Offset: 0x00051528
		private void InitializeTypeComboBoxes()
		{
			this.cmbType1.Items.Clear();
			this.cmbType2.Items.Clear();
			checked
			{
				int num = this.TOTAL_TYPE_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					string typeNameFromRom = this.GetTypeNameFromRom(i);
					this.cmbType1.Items.Add(typeNameFromRom);
					this.cmbType2.Items.Add(typeNameFromRom);
				}
				this.cmbType1.SelectedIndex = 0;
				this.cmbType2.SelectedIndex = 0;
			}
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x000533B4 File Offset: 0x000515B4
		private void LoadTypes(PokemonData pokemonData, int offset)
		{
			checked
			{
				byte b = this.romData[offset + 6];
				this.cmbType1.SelectedIndex = (int)b;
				pokemonData.OriginalType1Id = b;
				byte b2 = this.romData[offset + 7];
				this.cmbType2.SelectedIndex = (int)b2;
				pokemonData.OriginalType2Id = b2;
			}
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x00053404 File Offset: 0x00051604
		private void SaveTypes(PokemonData pokemonData, int offset)
		{
			checked
			{
				this.romData[offset + 6] = (byte)this.cmbType1.SelectedIndex;
				pokemonData.OriginalType1Id = (byte)this.cmbType1.SelectedIndex;
				this.romData[offset + 7] = (byte)this.cmbType2.SelectedIndex;
				pokemonData.OriginalType2Id = (byte)this.cmbType2.SelectedIndex;
			}
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x00053464 File Offset: 0x00051664
		private void Types_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = !this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
			if (!flag)
			{
				ComboBox comboBox = (ComboBox)sender;
				int selectedIndex = comboBox.SelectedIndex;
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag2 = false;
				string name = comboBox.Name;
				if (Operators.CompareString(name, "cmbType1", false) != 0)
				{
					if (Operators.CompareString(name, "cmbType2", false) == 0)
					{
						flag2 = selectedIndex != (int)pokemonData.OriginalType2Id;
					}
				}
				else
				{
					flag2 = selectedIndex != (int)pokemonData.OriginalType1Id;
				}
				bool flag3 = flag2;
				if (flag3)
				{
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x00053514 File Offset: 0x00051714
		private void LoadEvolutionMethods()
		{
			this.evolutionMethods.Clear();
			string text = Path.Combine(Application.StartupPath, "txt\\EvolutionCode.txt");
			string[] array = File.ReadAllLines(text, Encoding.UTF8);
			checked
			{
				int num = array.Length - 1;
				for (int i = 0; i <= num; i++)
				{
					string text2 = array[i];
					bool flag = string.IsNullOrEmpty(text2);
					if (!flag)
					{
						string[] array2 = text2.Split(new char[] { ';' });
						EvolutionMethod evolutionMethod = new EvolutionMethod();
						evolutionMethod.Code = i;
						evolutionMethod.MethodName = array2[0].Trim();
						evolutionMethod.Parameter1Description = array2[1].Trim();
						evolutionMethod.Parameter2Description = array2[2].Trim();
						this.evolutionMethods.Add(evolutionMethod);
					}
				}
			}
		}

		// Token: 0x06000B61 RID: 2913 RVA: 0x000535D8 File Offset: 0x000517D8
		private void InitializeEvolutionMethodComboBox()
		{
			this.cmbEvolutionMethod.BeginUpdate();
			this.cmbEvolutionMethod.Items.Clear();
			string[] array = File.ReadAllLines("txt\\EvolutionCode.txt");
			foreach (string text in array)
			{
				string[] array3 = text.Split(new char[] { ';' });
				this.cmbEvolutionMethod.Items.Add(array3[0]);
			}
			this.cmbEvolutionMethod.EndUpdate();
		}

		// Token: 0x06000B62 RID: 2914 RVA: 0x00053660 File Offset: 0x00051860
		private void cmbEvolutionMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			string text = this.cmbEvolutionMethod.SelectedItem.ToString();
			string[] array = File.ReadAllLines("txt\\EvolutionCode.txt");
			foreach (string text2 in array)
			{
				string[] array3 = text2.Split(new char[] { ';' });
				bool flag = Operators.CompareString(array3[0], text, false) == 0;
				if (flag)
				{
					this.txtParameter1Description.Text = array3[1];
					this.txtParameter2Description.Text = array3[2];
					break;
				}
			}
		}

		// Token: 0x06000B63 RID: 2915 RVA: 0x000536F4 File Offset: 0x000518F4
		private void InitializeEvolveToComboBox()
		{
			this.cmbEvolveTo.BeginUpdate();
			this.cmbEvolveTo.Items.Clear();
			this.cmbEvolveTo.Items.Add("なし");
			checked
			{
				int num = this.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					this.cmbEvolveTo.Items.Add(this.pokemonDataList[i].Name);
				}
				this.cmbEvolveTo.EndUpdate();
				this.cmbEvolveTo.SelectedIndex = 0;
			}
		}

		// Token: 0x06000B64 RID: 2916 RVA: 0x00053788 File Offset: 0x00051988
		private void LoadEvolutionTable(PokemonData pokemonData)
		{
			this.evolutionSlots.Clear();
			this.lstEvolutionSlot.Items.Clear();
			checked
			{
				int num = this.EVOLUTION_TABLE_OFFSET + pokemonData.Index * this.EVOLUTION_SLOT_LENGTH * this.EVOLUTION_SLOT_COUNT;
				int num2 = this.EVOLUTION_SLOT_COUNT - 1;
				for (int i = 0; i <= num2; i++)
				{
					int num3 = num + i * this.EVOLUTION_SLOT_LENGTH;
					EvolutionSlot evolutionSlot = new EvolutionSlot();
					evolutionSlot.SlotIndex = i;
					evolutionSlot.EvolutionCode = this.romData[num3];
					evolutionSlot.Parameter1A = this.romData[num3 + 2];
					evolutionSlot.Parameter1B = this.romData[num3 + 3];
					evolutionSlot.EvolveToPokemonId = BitConverter.ToUInt16(this.romData, num3 + 4);
					evolutionSlot.Parameter2A = this.romData[num3 + 6];
					evolutionSlot.Parameter2B = this.romData[num3 + 7];
					this.evolutionSlots.Add(evolutionSlot);
					this.lstEvolutionSlot.Items.Add(evolutionSlot.ToString());
				}
			}
		}

		// Token: 0x06000B65 RID: 2917 RVA: 0x0005389C File Offset: 0x00051A9C
		private void DisplayEvolveToPokemonImage(int pokemonId)
		{
			bool flag = this.pokemonDataList.ContainsKey(pokemonId);
			if (flag)
			{
				PokemonData pokemonData = this.pokemonDataList[pokemonId];
				this.DisplaySingleGBASprite(this.picEvolveTo, pokemonData.FrontImageAddress, pokemonData.NormalPaletteAddress, pokemonData.TemporaryFrontImageData, pokemonData.TemporaryNormalPaletteData);
			}
			else
			{
				this.picEvolveTo.Image = null;
			}
		}

		// Token: 0x06000B66 RID: 2918 RVA: 0x00053900 File Offset: 0x00051B00
		private void lstEvolutionSlot_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.isUpdatingEvolutionUI = true;
			EvolutionSlot evolutionSlot = this.evolutionSlots[this.lstEvolutionSlot.SelectedIndex];
			this.cmbEvolutionMethod.SelectedIndex = (int)evolutionSlot.EvolutionCode;
			this.nudParameter1A.Value = new decimal((int)evolutionSlot.Parameter1A);
			this.nudParameter1B.Value = new decimal((int)evolutionSlot.Parameter1B);
			this.cmbEvolveTo.SelectedIndex = (int)evolutionSlot.EvolveToPokemonId;
			this.nudParameter2A.Value = new decimal((int)evolutionSlot.Parameter2A);
			this.nudParameter2B.Value = new decimal((int)evolutionSlot.Parameter2B);
			this.DisplayEvolveToPokemonImage((int)evolutionSlot.EvolveToPokemonId);
			this.isUpdatingEvolutionUI = false;
		}

		// Token: 0x06000B67 RID: 2919 RVA: 0x000539C0 File Offset: 0x00051BC0
		private void EvolutionControls_ValueChanged(object sender, EventArgs e)
		{
			bool flag = sender == this.cmbEvolveTo;
			if (flag)
			{
				this.DisplayEvolveToPokemonImage(this.cmbEvolveTo.SelectedIndex);
			}
			bool flag2 = this.isUpdatingEvolutionUI;
			checked
			{
				if (!flag2)
				{
					bool flag3 = this.lstEvolutionSlot.SelectedIndex >= 0;
					if (flag3)
					{
						EvolutionSlot evolutionSlot = this.evolutionSlots[this.lstEvolutionSlot.SelectedIndex];
						byte evolutionCode = evolutionSlot.EvolutionCode;
						byte parameter1A = evolutionSlot.Parameter1A;
						byte parameter1B = evolutionSlot.Parameter1B;
						ushort evolveToPokemonId = evolutionSlot.EvolveToPokemonId;
						byte parameter2A = evolutionSlot.Parameter2A;
						byte parameter2B = evolutionSlot.Parameter2B;
						evolutionSlot.EvolutionCode = (byte)this.cmbEvolutionMethod.SelectedIndex;
						evolutionSlot.Parameter1A = Convert.ToByte(this.nudParameter1A.Value);
						evolutionSlot.Parameter1B = Convert.ToByte(this.nudParameter1B.Value);
						evolutionSlot.EvolveToPokemonId = (ushort)this.cmbEvolveTo.SelectedIndex;
						evolutionSlot.Parameter2A = Convert.ToByte(this.nudParameter2A.Value);
						evolutionSlot.Parameter2B = Convert.ToByte(this.nudParameter2B.Value);
						bool flag4 = evolutionCode != evolutionSlot.EvolutionCode || parameter1A != evolutionSlot.Parameter1A || parameter1B != evolutionSlot.Parameter1B || evolveToPokemonId != evolutionSlot.EvolveToPokemonId || parameter2A != evolutionSlot.Parameter2A || parameter2B != evolutionSlot.Parameter2B;
						bool flag5 = flag4;
						if (flag5)
						{
							this.hasUnsavedChanges = true;
							this.UpdateSaveButtonState();
						}
					}
				}
			}
		}

		// Token: 0x06000B68 RID: 2920 RVA: 0x00053B40 File Offset: 0x00051D40
		private void SaveEvolutionTable(PokemonData pokemonData)
		{
			checked
			{
				int num = this.EVOLUTION_TABLE_OFFSET + pokemonData.Index * this.EVOLUTION_SLOT_LENGTH * this.EVOLUTION_SLOT_COUNT;
				{
					foreach (EvolutionSlot evolutionSlot in this.evolutionSlots)
					{
						int num2 = num + evolutionSlot.SlotIndex * this.EVOLUTION_SLOT_LENGTH;
						this.romData[num2] = evolutionSlot.EvolutionCode;
						this.romData[num2 + 2] = evolutionSlot.Parameter1A;
						this.romData[num2 + 3] = evolutionSlot.Parameter1B;
						byte[] bytes = BitConverter.GetBytes(evolutionSlot.EvolveToPokemonId);
						this.romData[num2 + 4] = bytes[0];
						this.romData[num2 + 5] = bytes[1];
						this.romData[num2 + 6] = evolutionSlot.Parameter2A;
						this.romData[num2 + 7] = evolutionSlot.Parameter2B;
					}
				}
			}
		}

		// Token: 0x06000B69 RID: 2921 RVA: 0x00053C3C File Offset: 0x00051E3C
		private void InitializeParameterAssistComboBoxes()
		{
			this.cmbParameterAssistPokemon.BeginUpdate();
			this.cmbParameterAssistPokemon.Items.Clear();
			checked
			{
				int num = this.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					this.cmbParameterAssistPokemon.Items.Add(this.pokemonDataList[i].Name);
				}
				this.cmbParameterAssistPokemon.EndUpdate();
				this.cmbParameterAssistType.BeginUpdate();
				this.cmbParameterAssistType.Items.Clear();
				try
				{
					foreach (object obj in this.cmbType1.Items)
					{
						string text = Conversions.ToString(obj);
						this.cmbParameterAssistType.Items.Add(text);
					}
				}
				finally
				{
				}
				this.cmbParameterAssistType.EndUpdate();
				this.cmbParameterAssistItem.BeginUpdate();
				this.cmbParameterAssistItem.Items.Clear();
				List<string> itemNames = ItemData.GetItemNames(this.romData);
				{
					foreach (string text2 in itemNames)
					{
						this.cmbParameterAssistItem.Items.Add(text2);
					}
				}
				this.cmbParameterAssistItem.EndUpdate();
				this.cmbParameterAssistMoveName.BeginUpdate();
				this.cmbParameterAssistMoveName.Items.Clear();
				List<string> moveNames = MoveData.GetMoveNames(this.romData);
				{
					foreach (string text3 in moveNames)
					{
						this.cmbParameterAssistMoveName.Items.Add(text3);
					}
				}
				this.cmbParameterAssistMoveName.EndUpdate();
				this.cmbParameterAssistPokemon.SelectedIndex = 0;
				this.cmbParameterAssistType.SelectedIndex = 0;
				this.cmbParameterAssistItem.SelectedIndex = 0;
				this.cmbParameterAssistMoveName.SelectedIndex = 0;
			}
		}

		// Token: 0x06000B6A RID: 2922 RVA: 0x00053E80 File Offset: 0x00052080
		private void rbParameterAssist_CheckedChanged(object sender, EventArgs e)
		{
			this.cmbParameterAssistPokemon.Enabled = this.rbParameterAssistPokemon.Checked;
			this.cmbParameterAssistType.Enabled = this.rbParameterAssistType.Checked;
			this.cmbParameterAssistItem.Enabled = this.rbParameterAssistItem.Checked;
			this.cmbParameterAssistMoveName.Enabled = this.rbParameterAssistMoveName.Checked;
		}

		// Token: 0x06000B6B RID: 2923 RVA: 0x00053EEA File Offset: 0x000520EA
		private void btnWriteParameter1_Click(object sender, EventArgs e)
		{
			this.WriteParameter(1);
		}

		// Token: 0x06000B6C RID: 2924 RVA: 0x00053EF5 File Offset: 0x000520F5
		private void btnWriteParameter2_Click(object sender, EventArgs e)
		{
			this.WriteParameter(2);
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x00053F00 File Offset: 0x00052100
		private void WriteParameter(int parameterNumber)
		{
			ushort num = 0;
			bool @checked = this.rbParameterAssistPokemon.Checked;
			checked
			{
				if (@checked)
				{
					num = (ushort)(this.cmbParameterAssistPokemon.SelectedIndex + 1);
				}
				else
				{
					bool checked2 = this.rbParameterAssistType.Checked;
					if (checked2)
					{
						num = (ushort)this.cmbParameterAssistType.SelectedIndex;
					}
					else
					{
						bool checked3 = this.rbParameterAssistItem.Checked;
						if (checked3)
						{
							num = (ushort)this.cmbParameterAssistItem.SelectedIndex;
						}
						else
						{
							bool checked4 = this.rbParameterAssistMoveName.Checked;
							if (checked4)
							{
								num = (ushort)this.cmbParameterAssistMoveName.SelectedIndex;
							}
						}
					}
				}
				byte[] bytes = BitConverter.GetBytes(num);
				byte b = bytes[0];
				byte b2 = bytes[1];
				bool flag = parameterNumber == 1;
				if (flag)
				{
					this.nudParameter1A.Value = new decimal((int)b);
					this.nudParameter1B.Value = new decimal((int)b2);
				}
				else
				{
					this.nudParameter2A.Value = new decimal((int)b);
					this.nudParameter2B.Value = new decimal((int)b2);
				}
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x00054008 File Offset: 0x00052208
		private void InitializeMoveComboBox()
		{
			this.cmbMoveList.BeginUpdate();
			this.cmbMoveList.Items.Clear();
			List<string> moveNames = MoveData.GetMoveNames(this.romData);
			{
				foreach (string text in moveNames)
				{
					this.cmbMoveList.Items.Add(text);
				}
			}
			this.cmbMoveList.EndUpdate();
			this.cmbMoveList.SelectedIndex = 0;
		}

		// Token: 0x06000B6F RID: 2927 RVA: 0x000540A8 File Offset: 0x000522A8
		private void LoadLevelMoveAddress(PokemonData pokemonData)
		{
			checked
			{
				int num = this.LEVEL_MOVE_TABLE_OFFSET + pokemonData.Index * 4;
				uint num2 = BitConverter.ToUInt32(this.romData, num);
				this.levelMoveAddress = num2 - 134217728U;
				this.lblLevelMoveTableAddress.Text = this.levelMoveAddress.ToString("X8");
			}
		}

		// Token: 0x06000B70 RID: 2928 RVA: 0x000540FC File Offset: 0x000522FC
		private void SaveLevelMoveAddress()
		{
			uint num = Convert.ToUInt32(this.lblLevelMoveTableAddress.Text, 16);
			checked
			{
				uint num2 = num + 134217728U;
				byte[] bytes = BitConverter.GetBytes(num2);
				int num3 = this.LEVEL_MOVE_TABLE_OFFSET + this.currentPokemonIndex * 4;
				Array.Copy(bytes, 0, this.romData, num3, 4);
			}
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x0005414C File Offset: 0x0005234C
		private void btnLevelMoveTableAddress_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			bool flag = pokemonData.TemporaryLevelMoveData != null;
			if (flag)
			{
				pokemonData.TemporaryLevelMoveData = null;
				pokemonData.TemporaryLevelMoveAddress = 0U;
			}
			uint num = Convert.ToUInt32(this.lblLevelMoveTableAddress.Text, 16);
			bool flag2 = this.levelMoveAddress != num;
			if (flag2)
			{
				this.levelMoveAddress = num;
				this.LoadLevelMoves(this.pokemonDataList[this.currentPokemonIndex]);
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x000541E0 File Offset: 0x000523E0
		private void LoadLevelMoves(PokemonData pokemonData)
		{
			this.levelMoves.Clear();
			this.lstLevelMoveList.Items.Clear();
			bool flag = pokemonData.TemporaryLevelMoveData != null && pokemonData.TemporaryLevelMoveAddress == this.levelMoveAddress;
			byte[] array = null;
			checked
			{
				int num = (int)this.levelMoveAddress;
				bool flag2 = flag;
				if (flag2)
				{
					array = pokemonData.TemporaryLevelMoveData;
				}
				int num2 = 0;
				int num3 = num;
				int num4 = 0;
				for (;;)
				{
					bool flag3 = this.LEVEL_MOVE_ENTRY_LENGTH == 2;
					if (flag3)
					{
						bool flag4 = flag;
						ushort num5 = 0;
						if (flag4)
						{
							num5 = BitConverter.ToUInt16(array, num2);
							bool flag5 = num5 == ushort.MaxValue;
							if (flag5)
							{
								break;
							}
						}
						else
						{
							num5 = BitConverter.ToUInt16(this.romData, num3 + num2);
							bool flag6 = num5 == ushort.MaxValue;
							if (flag6)
							{
								break;
							}
						}
						bool flag7;
						unchecked
						{
							int num6 = (int)((ushort)((uint)num5 >> 8) / 2);
							int num7 = (int)(num5 & 255) | ((int)((ushort)((uint)num5 >> 8) & 1) << 8);
							LevelMove levelMove = new LevelMove();
							levelMove.Level = num6;
							levelMove.MoveId = num7;
							levelMove.MoveName = ((num7 < this.cmbMoveList.Items.Count) ? this.cmbMoveList.Items[num7].ToString() : string.Format("技{0}", num7));
							this.levelMoves.Add(levelMove);
							this.lstLevelMoveList.Items.Add(levelMove.ToString());
							flag7 = flag;
						}
						if (flag7)
						{
							num2 += 2;
						}
						else
						{
							num3 += 2;
						}
					}
					else
					{
						bool flag8 = this.LEVEL_MOVE_ENTRY_LENGTH == 3;
						if (flag8)
						{
							bool flag9 = flag;
							ushort num5 = 0;
							int num6 = 0;
							if (flag9)
							{
								bool flag10 = array[num2] == 0 && array[num2 + 1] == 0 && array[num2 + 2] == byte.MaxValue;
								if (flag10)
								{
									break;
								}
								num5 = BitConverter.ToUInt16(array, num2);
								num6 = (int)array[num2 + 2];
							}
							else
							{
								bool flag11 = this.romData[num3] == 0 && this.romData[num3 + 1] == 0 && this.romData[num3 + 2] == byte.MaxValue;
								if (flag11)
								{
									break;
								}
								num5 = BitConverter.ToUInt16(this.romData, num3);
								num6 = (int)this.romData[num3 + 2];
							}
							LevelMove levelMove2 = new LevelMove();
							levelMove2.Level = num6;
							levelMove2.MoveId = (int)num5;
							levelMove2.MoveName = (((int)num5 < this.cmbMoveList.Items.Count) ? this.cmbMoveList.Items[(int)num5].ToString() : string.Format("技{0}", num5));
							this.levelMoves.Add(levelMove2);
							this.lstLevelMoveList.Items.Add(levelMove2.ToString());
							bool flag12 = flag;
							if (flag12)
							{
								num2 += 3;
							}
							else
							{
								num3 += 3;
							}
						}
					}
					num4++;
				}
				bool flag13 = this.lstLevelMoveList.Items.Count > 0;
				if (flag13)
				{
					this.lstLevelMoveList.SelectedIndex = 0;
				}
				else
				{
					this.lstLevelMoveList.SelectedIndex = -1;
				}
			}
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x000544FC File Offset: 0x000526FC
		private void lstLevelMoveList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstLevelMoveList.SelectedIndex >= 0 && this.lstLevelMoveList.SelectedIndex < this.levelMoves.Count;
			if (flag)
			{
				LevelMove levelMove = this.levelMoves[this.lstLevelMoveList.SelectedIndex];
				this.nudMoveConditionLevel.Value = new decimal(levelMove.Level);
				this.cmbMoveList.SelectedIndex = levelMove.MoveId;
			}
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x0005457C File Offset: 0x0005277C
		private void btnChangeMove_Click(object sender, EventArgs e)
		{
			int num = Convert.ToInt32(this.nudMoveConditionLevel.Value);
			int selectedIndex = this.cmbMoveList.SelectedIndex;
			LevelMove levelMove = this.levelMoves[this.lstLevelMoveList.SelectedIndex];
			bool flag = levelMove.Level != num || levelMove.MoveId != selectedIndex;
			if (flag)
			{
				levelMove.Level = num;
				levelMove.MoveId = selectedIndex;
				levelMove.MoveName = this.cmbMoveList.Items[selectedIndex].ToString();
				this.lstLevelMoveList.Items[this.lstLevelMoveList.SelectedIndex] = levelMove.ToString();
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x0005463C File Offset: 0x0005283C
		private void SaveLevelMoves()
		{
			checked
			{
				int num = (int)this.levelMoveAddress;
				{
					foreach (LevelMove levelMove in this.levelMoves)
					{
						bool flag = this.LEVEL_MOVE_ENTRY_LENGTH == 2;
						if (flag)
						{
							int num2 = (levelMove.Level * 2) | ((levelMove.MoveId >> 8) & 1);
							ushort num3 = (ushort)((num2 << 8) | (levelMove.MoveId & 255));
							byte[] bytes = BitConverter.GetBytes(num3);
							this.romData[num] = bytes[0];
							this.romData[num + 1] = bytes[1];
							num += 2;
						}
						else
						{
							bool flag2 = this.LEVEL_MOVE_ENTRY_LENGTH == 3;
							if (flag2)
							{
								byte[] bytes2 = BitConverter.GetBytes((ushort)levelMove.MoveId);
								this.romData[num] = bytes2[0];
								this.romData[num + 1] = bytes2[1];
								this.romData[num + 2] = (byte)levelMove.Level;
								num += 3;
							}
						}
					}
				}
				bool flag3 = this.LEVEL_MOVE_ENTRY_LENGTH == 2;
				if (flag3)
				{
					this.romData[num] = byte.MaxValue;
					this.romData[num + 1] = byte.MaxValue;
				}
				else
				{
					bool flag4 = this.LEVEL_MOVE_ENTRY_LENGTH == 3;
					if (flag4)
					{
						this.romData[num] = 0;
						this.romData[num + 1] = 0;
						this.romData[num + 2] = byte.MaxValue;
					}
				}
				PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
				bool flag5 = pokemonData.TemporaryLevelMoveData != null;
				if (flag5)
				{
					pokemonData.TemporaryLevelMoveData = null;
					pokemonData.TemporaryLevelMoveAddress = 0U;
				}
			}
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x000547E0 File Offset: 0x000529E0
		private void btnChangeLevelMoveNumber_Click(object sender, EventArgs e)
		{
			using (InsertNewLevelMoveList insertNewLevelMoveList = new InsertNewLevelMoveList())
			{
				bool flag = insertNewLevelMoveList.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					uint num = Convert.ToUInt32(insertNewLevelMoveList.NewLevelMoveAddress, 16);
					int newLevelMoveNum = insertNewLevelMoveList.NewLevelMoveNum;
					byte[] array = this.GenerateNewLevelMoveData(newLevelMoveNum);
					PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
					pokemonData.TemporaryLevelMoveData = array;
					pokemonData.TemporaryLevelMoveAddress = num;
					this.levelMoveAddress = num;
					this.lblLevelMoveTableAddress.Text = num.ToString("X8");
					this.LoadLevelMoves(pokemonData);
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
				}
			}
		}

		// Token: 0x06000B77 RID: 2935 RVA: 0x0005489C File Offset: 0x00052A9C
		private byte[] GenerateNewLevelMoveData(int numMoves)
		{
			List<byte> list = new List<byte>();
			checked
			{
				int num = numMoves - 1;
				for (int i = 0; i <= num; i++)
				{
					bool flag = this.LEVEL_MOVE_ENTRY_LENGTH == 2;
					if (flag)
					{
						ushort num2 = 513;
						list.AddRange(BitConverter.GetBytes(num2));
					}
					else
					{
						bool flag2 = this.LEVEL_MOVE_ENTRY_LENGTH == 3;
						if (flag2)
						{
							list.AddRange(BitConverter.GetBytes(1));
							list.Add(1);
						}
					}
				}
				bool flag3 = this.LEVEL_MOVE_ENTRY_LENGTH == 2;
				if (flag3)
				{
					list.Add(byte.MaxValue);
					list.Add(byte.MaxValue);
				}
				else
				{
					bool flag4 = this.LEVEL_MOVE_ENTRY_LENGTH == 3;
					if (flag4)
					{
						list.Add(0);
						list.Add(0);
						list.Add(byte.MaxValue);
					}
				}
				return list.ToArray();
			}
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0005496C File Offset: 0x00052B6C
		private void InitializeTmHmList()
		{
			this.tmIds.Clear();
			this.hmIds.Clear();
			checked
			{
				int num = this.TM_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.TM_HM_LIST_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					this.tmIds.Add(num3);
				}
				int num4 = this.HM_COUNT - 1;
				for (int j = 0; j <= num4; j++)
				{
					int num5 = this.TM_HM_LIST_OFFSET + (this.TM_COUNT + j) * 2;
					ushort num6 = BitConverter.ToUInt16(this.romData, num5);
					this.hmIds.Add(num6);
				}
				this.tmHmCount = this.tmIds.Count + this.hmIds.Count;
				this.tmHmDataLength = (this.tmHmCount + 7) / 8;
				this.clbTmHmList.BeginUpdate();
				this.clbTmHmList.Items.Clear();
				int num7 = this.tmIds.Count - 1;
				for (int k = 0; k <= num7; k++)
				{
					string text = (((int)this.tmIds[k] < this.cmbMoveList.Items.Count) ? this.cmbMoveList.Items[(int)this.tmIds[k]].ToString() : string.Format("技{0}", this.tmIds[k]));
					this.clbTmHmList.Items.Add(string.Format("TM{0:00} - {1}", k + 1, text));
				}
				int num8 = this.hmIds.Count - 1;
				for (int l = 0; l <= num8; l++)
				{
					string text2 = (((int)this.hmIds[l] < this.cmbMoveList.Items.Count) ? this.cmbMoveList.Items[(int)this.hmIds[l]].ToString() : string.Format("技{0}", this.hmIds[l]));
					this.clbTmHmList.Items.Add(string.Format("HM{0:00} - {1}", l + 1, text2));
				}
				this.clbTmHmList.EndUpdate();
			}
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x00054BC8 File Offset: 0x00052DC8
		private void LoadTmHmLearnData(PokemonData pokemonData)
		{
			this.clbTmHmList.BeginUpdate();
			checked
			{
				int num = this.clbTmHmList.Items.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					this.clbTmHmList.SetItemChecked(i, false);
				}
				int num2 = this.TM_HM_LEARN_OFFSET + pokemonData.Index * this.tmHmDataLength;
				int num3 = this.tmHmCount - 1;
				for (int j = 0; j <= num3; j++)
				{
					int num4 = j / 8;
					int num5 = j % 8;
					byte b = this.romData[num2 + num4];
					bool flag = ((int)b & (1 << num5)) != 0;
					this.clbTmHmList.SetItemChecked(j, flag);
				}
				this.clbTmHmList.EndUpdate();
			}
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x00054C81 File Offset: 0x00052E81
		private void clbTmHmList_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x00054C94 File Offset: 0x00052E94
		private void SaveTmHmLearnData(PokemonData pokemonData)
		{
			checked
			{
				int num = this.TM_HM_LEARN_OFFSET + pokemonData.Index * this.tmHmDataLength;
				byte[] array = new byte[this.tmHmDataLength - 1 + 1];
				int num2 = this.tmHmCount - 1;
				for (int i = 0; i <= num2; i++)
				{
					int num3 = i / 8;
					int num4 = i % 8;
					bool itemChecked = this.clbTmHmList.GetItemChecked(i);
					if (itemChecked)
					{
						array[num3] = (byte)((int)array[num3] | (1 << num4));
					}
				}
				Array.Copy(array, 0, this.romData, num, this.tmHmDataLength);
			}
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x00054D20 File Offset: 0x00052F20
		private void InitializeMoveTutorList()
		{
			this.moveTutorIds.Clear();
			checked
			{
				int num = this.MOVE_TUTOR_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.MOVE_TUTOR_LIST_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					this.moveTutorIds.Add(num3);
				}
				this.moveTutorCount = this.moveTutorIds.Count;
				this.moveTutorDataLength = (this.moveTutorCount + 7) / 8;
				this.clbMoveTutorList.BeginUpdate();
				this.clbMoveTutorList.Items.Clear();
				int num4 = this.moveTutorIds.Count - 1;
				for (int j = 0; j <= num4; j++)
				{
					string text = (((int)this.moveTutorIds[j] < this.cmbMoveList.Items.Count) ? this.cmbMoveList.Items[(int)this.moveTutorIds[j]].ToString() : string.Format("技{0}", this.moveTutorIds[j]));
					this.clbMoveTutorList.Items.Add(string.Format("{0}", text));
				}
				this.clbMoveTutorList.EndUpdate();
			}
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x00054E64 File Offset: 0x00053064
		private void LoadMoveTutorLearnData(PokemonData pokemonData)
		{
			this.clbMoveTutorList.BeginUpdate();
			checked
			{
				int num = this.clbMoveTutorList.Items.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					this.clbMoveTutorList.SetItemChecked(i, false);
				}
				int num2 = this.MOVE_TUTOR_LEARN_OFFSET + pokemonData.Index * this.moveTutorDataLength;
				int num3 = this.moveTutorCount - 1;
				for (int j = 0; j <= num3; j++)
				{
					int num4 = j / 8;
					int num5 = j % 8;
					byte b = this.romData[num2 + num4];
					bool flag = ((int)b & (1 << num5)) != 0;
					bool flag2 = j < this.clbMoveTutorList.Items.Count;
					if (flag2)
					{
						this.clbMoveTutorList.SetItemChecked(j, flag);
					}
				}
				this.clbMoveTutorList.EndUpdate();
			}
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x00054F3C File Offset: 0x0005313C
		private void SaveMoveTutorLearnData(PokemonData pokemonData)
		{
			checked
			{
				int num = this.MOVE_TUTOR_LEARN_OFFSET + pokemonData.Index * this.moveTutorDataLength;
				byte[] array = new byte[this.moveTutorDataLength - 1 + 1];
				int num2 = this.moveTutorCount - 1;
				for (int i = 0; i <= num2; i++)
				{
					int num3 = i / 8;
					int num4 = i % 8;
					bool itemChecked = this.clbMoveTutorList.GetItemChecked(i);
					if (itemChecked)
					{
						array[num3] = (byte)((int)array[num3] | (1 << num4));
					}
				}
				Array.Copy(array, 0, this.romData, num, this.moveTutorDataLength);
			}
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x00054FC8 File Offset: 0x000531C8
		private void clbMoveTutorList_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x00054FDC File Offset: 0x000531DC
		private int GetPokedexIndex(PokemonData pokemonData)
		{
			bool flag = pokemonData.PokedexOrder <= MyProject.Forms.PokedexOrderEditor.MAX_POKEDEX_COUNT;
			int num = 0;
			if (flag)
			{
				num = pokemonData.PokedexOrder;
			}
			else
			{
				num = -1;
			}
			return num;
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x00055018 File Offset: 0x00053218
		private void SetPokedexControlsEnabled(bool enabled)
		{
			this.grpPokedexCategory.Enabled = enabled;
			this.grpHeightWeight.Enabled = enabled;
			this.grpPokedexDescription.Enabled = enabled;
			this.grpSizeComparison.Enabled = enabled;
			bool flag = !enabled;
			if (flag)
			{
				this.txtPokedexCategory1.Text = string.Empty;
				this.txtPokedexCategory2.Text = string.Empty;
				this.nudHeight.Value = 0m;
				this.nudWeight.Value = 0m;
				this.lblHeight1.Text = "0.0";
				this.lblWeight1.Text = "0.0";
				this.txtPokedexDescriptionAddress.Text = "000000";
				this.txtPokedexDescription.Text = string.Empty;
				this.nudSizeComparison1.Value = 0m;
				this.nudSizeComparison2.Value = 0m;
				this.nudSizeComparison3.Value = 0m;
				this.nudSizeComparison4.Value = 0m;
				bool flag2 = this.picSizeComparison.Image != null;
				if (flag2)
				{
					Image image = this.picSizeComparison.Image;
					this.picSizeComparison.Image = null;
					image.Dispose();
				}
			}
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x00055168 File Offset: 0x00053368
		private void LoadPokedexCategory(PokemonData pokemonData)
		{
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag = pokedexIndex == -1;
			checked
			{
				if (flag)
				{
					pokemonData.PokedexCategory = "";
					pokemonData.OriginalPokedexCategory = "";
				}
				else
				{
					int num = this.POKEDEX_DATA_OFFSET + pokedexIndex * this.POKEDEX_DATA_ENTRY_LENGTH;
					byte[] array = new byte[this.POKEDEX_CATEGORY_LENGTH - 1 + 1];
					Array.Copy(this.romData, num, array, 0, this.POKEDEX_CATEGORY_LENGTH);
					int num2 = array.Length - 1;
					for (int i = 0; i <= num2; i++)
					{
						bool flag2 = array[i] == 0;
						if (flag2)
						{
							array[i] = 0;
						}
					}
					pokemonData.PokedexCategory = TextConverter.BytesToPokemonString(array, 0, this.POKEDEX_CATEGORY_LENGTH);
					pokemonData.OriginalPokedexCategory = pokemonData.PokedexCategory;
				}
			}
		}

		// Token: 0x06000B83 RID: 2947 RVA: 0x00055228 File Offset: 0x00053428
		private void SavePokedexCategory(PokemonData pokemonData)
		{
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag = pokedexIndex == -1;
			checked
			{
				if (!flag)
				{
					int num = this.POKEDEX_DATA_OFFSET + pokedexIndex * this.POKEDEX_DATA_ENTRY_LENGTH;
					int num2 = this.POKEDEX_CATEGORY_LENGTH - 1;
					for (int i = 0; i <= num2; i++)
					{
						this.romData[num + i] = 0;
					}
					byte[] array = TextConverter.ConvertPokedexCategoryToBytes(pokemonData.PokedexCategory);
					bool enable_CATEGORY_NO_SPACE = this.ENABLE_CATEGORY_NO_SPACE;
					if (enable_CATEGORY_NO_SPACE)
					{
						int num3 = 0;
						int num4 = array.Length - 1;
						for (int j = 0; j <= num4; j++)
						{
							bool flag2 = array[j] == 0 || num3 >= this.POKEDEX_CATEGORY_LENGTH - 1;
							if (flag2)
							{
								break;
							}
							this.romData[num + num3] = array[j];
							num3++;
						}
						bool flag3 = num3 < this.POKEDEX_CATEGORY_LENGTH;
						if (flag3)
						{
							this.romData[num + num3] = byte.MaxValue;
						}
					}
					else
					{
						int num5 = Math.Min(this.POKEDEX_CATEGORY_LENGTH, array.Length);
						Array.Copy(array, 0, this.romData, num, num5);
					}
				}
			}
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x00055333 File Offset: 0x00053533
		private void UpdatePokedexCategoryUI(PokemonData pokemonData)
		{
			this.txtPokedexCategory1.Text = pokemonData.PokedexCategory;
			this.txtPokedexCategory2.Text = pokemonData.PokedexCategory;
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x0005535C File Offset: 0x0005355C
		private void btnChangePokedexCategory_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			string text = this.txtPokedexCategory2.Text.Trim();
			bool enable_CATEGORY_NO_SPACE = this.ENABLE_CATEGORY_NO_SPACE;
			if (enable_CATEGORY_NO_SPACE)
			{
				int num = checked(this.POKEDEX_CATEGORY_LENGTH - 1);
				bool flag = text.Length > num;
				if (flag)
				{
					text = text.Substring(0, num);
				}
				this.txtPokedexCategory2.Text = text;
			}
			else
			{
				bool flag2 = text.Length < this.POKEDEX_CATEGORY_LENGTH;
				if (flag2)
				{
					text = text.PadRight(this.POKEDEX_CATEGORY_LENGTH, '\u3000');
					this.txtPokedexCategory2.Text = text;
				}
				else
				{
					text = text.Substring(0, this.POKEDEX_CATEGORY_LENGTH);
					this.txtPokedexCategory2.Text = text;
				}
			}
			bool flag3 = Operators.CompareString(pokemonData.PokedexCategory, text, false) != 0;
			if (flag3)
			{
				pokemonData.PokedexCategory = text;
				this.txtPokedexCategory1.Text = text;
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B86 RID: 2950 RVA: 0x0005545C File Offset: 0x0005365C
		private void LoadPokedexData(PokemonData pokemonData)
		{
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag = pokedexIndex == -1;
			checked
			{
				if (flag)
				{
					pokemonData.Height = 0;
					pokemonData.OriginalHeight = 0;
					pokemonData.Weight = 0;
					pokemonData.OriginalWeight = 0;
					pokemonData.SizeComparison1 = 0;
					pokemonData.SizeComparison2 = 0;
					pokemonData.SizeComparison3 = 0;
					pokemonData.SizeComparison4 = 0;
					pokemonData.OriginalSizeComparison1 = 0;
					pokemonData.OriginalSizeComparison2 = 0;
					pokemonData.OriginalSizeComparison3 = 0;
					pokemonData.OriginalSizeComparison4 = 0;
				}
				else
				{
					int num = this.POKEDEX_DATA_OFFSET + pokedexIndex * this.POKEDEX_DATA_ENTRY_LENGTH;
					pokemonData.Height = BitConverter.ToUInt16(this.romData, num + 6);
					pokemonData.OriginalHeight = pokemonData.Height;
					pokemonData.Weight = BitConverter.ToUInt16(this.romData, num + 8);
					pokemonData.OriginalWeight = pokemonData.Weight;
					pokemonData.SizeComparison1 = BitConverter.ToUInt16(this.romData, num + 18);
					pokemonData.SizeComparison2 = BitConverter.ToInt16(this.romData, num + 20);
					pokemonData.SizeComparison3 = BitConverter.ToUInt16(this.romData, num + 22);
					pokemonData.SizeComparison4 = BitConverter.ToInt16(this.romData, num + 24);
					pokemonData.OriginalSizeComparison1 = pokemonData.SizeComparison1;
					pokemonData.OriginalSizeComparison2 = pokemonData.SizeComparison2;
					pokemonData.OriginalSizeComparison3 = pokemonData.SizeComparison3;
					pokemonData.OriginalSizeComparison4 = pokemonData.SizeComparison4;
				}
			}
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x000555C0 File Offset: 0x000537C0
		private void SavePokedexData(PokemonData pokemonData)
		{
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag = pokedexIndex == -1;
			checked
			{
				if (!flag)
				{
					int num = this.POKEDEX_DATA_OFFSET + pokedexIndex * this.POKEDEX_DATA_ENTRY_LENGTH;
					byte[] bytes = BitConverter.GetBytes(pokemonData.Height);
					this.romData[num + 6] = bytes[0];
					this.romData[num + 6 + 1] = bytes[1];
					byte[] bytes2 = BitConverter.GetBytes(pokemonData.Weight);
					this.romData[num + 8] = bytes2[0];
					this.romData[num + 8 + 1] = bytes2[1];
					byte[] bytes3 = BitConverter.GetBytes(pokemonData.SizeComparison1);
					Array.Copy(bytes3, 0, this.romData, num + 18, 2);
					byte[] bytes4 = BitConverter.GetBytes(pokemonData.SizeComparison2);
					Array.Copy(bytes4, 0, this.romData, num + 20, 2);
					byte[] bytes5 = BitConverter.GetBytes(pokemonData.SizeComparison3);
					Array.Copy(bytes5, 0, this.romData, num + 22, 2);
					byte[] bytes6 = BitConverter.GetBytes(pokemonData.SizeComparison4);
					Array.Copy(bytes6, 0, this.romData, num + 24, 2);
				}
			}
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x000556CC File Offset: 0x000538CC
		private void UpdateSizeUI(PokemonData pokemonData)
		{
			this.nudHeight.Value = new decimal((int)pokemonData.Height);
			this.nudWeight.Value = new decimal((int)pokemonData.Weight);
			this.lblHeight1.Text = ((double)pokemonData.Height / 10.0).ToString("0.0");
			this.lblWeight1.Text = ((double)pokemonData.Weight / 10.0).ToString("0.0");
		}

		// Token: 0x06000B89 RID: 2953 RVA: 0x0005575C File Offset: 0x0005395C
		private void nudHeight_ValueChanged(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			pokemonData.Height = Convert.ToUInt16(this.nudHeight.Value);
			this.lblHeight1.Text = ((double)pokemonData.Height / 10.0).ToString("0.0");
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x000557CC File Offset: 0x000539CC
		private void nudWeight_ValueChanged(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			pokemonData.Weight = Convert.ToUInt16(this.nudWeight.Value);
			this.lblWeight1.Text = ((double)pokemonData.Weight / 10.0).ToString("0.0");
			this.hasUnsavedChanges = true;
			this.UpdateSaveButtonState();
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x0005583C File Offset: 0x00053A3C
		private void btnChangePokedexDescriptionAddress_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			uint num = Convert.ToUInt32(this.txtPokedexDescriptionAddress.Text, 16);
			bool flag = pokemonData.PokedexDescriptionAddress != num;
			if (flag)
			{
				pokemonData.PokedexDescriptionAddress = num;
				this.LoadPokedexDescription(pokemonData);
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x000558A0 File Offset: 0x00053AA0
		private void btnChangePokedexDescription_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			string text = this.txtPokedexDescription.Text;
			bool flag = Operators.CompareString(pokemonData.PokedexDescription, text, false) != 0;
			if (flag)
			{
				pokemonData.PokedexDescription = text;
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x000558F8 File Offset: 0x00053AF8
		private void LoadPokedexDescription(PokemonData pokemonData)
		{
			bool flag = (ulong)pokemonData.PokedexDescriptionAddress == 0UL;
			checked
			{
				if (flag)
				{
					this.txtPokedexDescription.Text = string.Empty;
				}
				else
				{
					List<byte> list = new List<byte>();
					int num = (int)pokemonData.PokedexDescriptionAddress;
					int num2 = 256;
					int num3 = num2 - 1;
					for (int i = 0; i <= num3; i++)
					{
						byte b = this.romData[num + i];
						bool flag2 = b == byte.MaxValue;
						if (flag2)
						{
							break;
						}
						list.Add(b);
					}
					pokemonData.PokedexDescription = TextConverter.BytesToPokemonString(list.ToArray(), 0, list.Count);
					pokemonData.OriginalPokedexDescription = pokemonData.PokedexDescription;
					this.txtPokedexDescription.Text = pokemonData.PokedexDescription;
				}
			}
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x000559B8 File Offset: 0x00053BB8
		private void SavePokedexDescription(PokemonData pokemonData)
		{
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			string text = this.txtPokedexDescription.Text;
			byte[] array = TextConverter.PokemonStringToBytes(text, 256);
			Array.Copy(array, 0L, this.romData, (long)((ulong)pokemonData.PokedexDescriptionAddress), (long)array.Length);
			bool flag = pokedexIndex == -1;
			checked
			{
				if (!flag)
				{
					int num = this.POKEDEX_DATA_OFFSET + pokedexIndex * this.POKEDEX_DATA_ENTRY_LENGTH;
					uint num2 = pokemonData.PokedexDescriptionAddress + 134217728U;
					byte[] bytes = BitConverter.GetBytes(num2);
					Array.Copy(bytes, 0, this.romData, num + 12, 4);
				}
			}
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x00055A48 File Offset: 0x00053C48
		private void LoadPokedexDescriptionAddress(PokemonData pokemonData)
		{
			int pokedexIndex = this.GetPokedexIndex(pokemonData);
			bool flag = pokedexIndex == -1;
			checked
			{
				if (flag)
				{
					pokemonData.PokedexDescriptionAddress = 0U;
					pokemonData.OriginalPokedexDescriptionAddress = 0U;
					this.txtPokedexDescriptionAddress.Text = "00000000";
				}
				else
				{
					int num = this.POKEDEX_DATA_OFFSET + pokedexIndex * this.POKEDEX_DATA_ENTRY_LENGTH + 12;
					uint num2 = BitConverter.ToUInt32(this.romData, num);
					pokemonData.PokedexDescriptionAddress = num2 - 134217728U;
					pokemonData.OriginalPokedexDescriptionAddress = pokemonData.PokedexDescriptionAddress;
					this.txtPokedexDescriptionAddress.Text = pokemonData.PokedexDescriptionAddress.ToString("X8");
				}
			}
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x00055AE8 File Offset: 0x00053CE8
		private void UpdateSizeComparisonUI(PokemonData pokemonData)
		{
			this.isUpdatingSizeCompUI = true;
			try
			{
				this.nudSizeComparison1.Value = new decimal((int)pokemonData.SizeComparison1);
				this.nudSizeComparison2.Value = new decimal((int)pokemonData.SizeComparison2);
				this.nudSizeComparison3.Value = new decimal((int)pokemonData.SizeComparison3);
				this.nudSizeComparison4.Value = new decimal((int)pokemonData.SizeComparison4);
			}
			finally
			{
				this.isUpdatingSizeCompUI = false;
			}
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x00055B78 File Offset: 0x00053D78
		private void DisplaySizeComparisonImages(PokemonData pokemonData)
		{
			Bitmap bitmap = null;
			Bitmap bitmap2 = null;
			checked
			{
				try
				{
					bitmap2 = (Bitmap)Image.FromFile("img/SizeComparisonBackGround.png");
					bitmap = new Bitmap(bitmap2.Width, bitmap2.Height);
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.DrawImage(bitmap2, 0, 0, bitmap.Width, bitmap.Height);
						Color[] array = new Color[16];
						array[0] = Color.FromArgb(0, 0, 0, 0);
						int num = 1;
						do
						{
							array[num] = Color.Black;
							num++;
						}
						while (num <= 15);
						try
						{
							bool flag = pokemonData.TemporaryFrontImageData != null;
							byte[] array2;
							if (flag)
							{
								int num2 = BitConverter.ToInt32(pokemonData.TemporaryFrontImageData, 0) >> 8;
								array2 = new byte[num2 - 1 + 1];
								ImageProcessor.LZ77UnComp(pokemonData.TemporaryFrontImageData, array2);
							}
							else
							{
								array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, pokemonData.FrontImageAddress, false);
							}
							using (Bitmap bitmap3 = ImageProcessor.LoadSprite(ref array2, array, 64, 64, false))
							{
								float num3 = 256f / (float)pokemonData.SizeComparison1;
								int num4 = (int)Math.Round((double)(unchecked(64f * num3)));
								int num5 = (int)Math.Round((double)(unchecked(64f * num3)));
								int sizeComparison = (int)pokemonData.SizeComparison2;
								Rectangle rectangle = new Rectangle(0 + (64 - num4) / 2, 8 + (64 - num5) / 2 + sizeComparison, num4, num5);
								graphics.DrawImage(bitmap3, rectangle);
							}
						}
						catch (Exception ex)
						{
						}
						try
						{
							int num6 = MyProject.Forms.TrainerSpriteEditor.TRAINER_SPRITE_TABLE_OFFSET + 1080;
							uint num7 = BitConverter.ToUInt32(this.romData, num6);
							uint num8 = num7 - 134217728U;
							byte[] array3 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num8, false);
							using (Bitmap bitmap4 = ImageProcessor.LoadSprite(ref array3, array, 64, 64, false))
							{
								float num9 = 256f / (float)pokemonData.SizeComparison3;
								int num10 = (int)Math.Round((double)(unchecked(64f * num9)));
								int num11 = (int)Math.Round((double)(unchecked(64f * num9)));
								int sizeComparison2 = (int)pokemonData.SizeComparison4;
								Rectangle rectangle2 = new Rectangle(40 + (64 - num10) / 2, 8 + (64 - num11) / 2 + sizeComparison2, num10, num11);
								graphics.DrawImage(bitmap4, rectangle2);
							}
						}
						catch (Exception ex2)
						{
						}
					}
					bool flag2 = this.picSizeComparison.Image != null;
					if (flag2)
					{
						this.picSizeComparison.Image.Dispose();
					}
					this.picSizeComparison.Image = bitmap;
				}
				catch (Exception ex3)
				{
					bool flag3 = bitmap != null;
					if (flag3)
					{
						bitmap.Dispose();
					}
					bool flag4 = this.picSizeComparison.Image != null;
					if (flag4)
					{
						this.picSizeComparison.Image.Dispose();
					}
					this.picSizeComparison.Image = null;
				}
				finally
				{
					bool flag5 = bitmap2 != null;
					if (flag5)
					{
						bitmap2.Dispose();
					}
				}
			}
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x00055F24 File Offset: 0x00054124
		private void SizeComparison_ValueChanged(object sender, EventArgs e)
		{
			bool flag = this.isUpdatingSizeCompUI;
			if (!flag)
			{
				bool flag2 = this.pokemonDataList.ContainsKey(this.currentPokemonIndex);
				if (flag2)
				{
					PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
					pokemonData.SizeComparison1 = Convert.ToUInt16(this.nudSizeComparison1.Value);
					pokemonData.SizeComparison2 = Convert.ToInt16(this.nudSizeComparison2.Value);
					pokemonData.SizeComparison3 = Convert.ToUInt16(this.nudSizeComparison3.Value);
					pokemonData.SizeComparison4 = Convert.ToInt16(this.nudSizeComparison4.Value);
					this.hasUnsavedChanges = true;
					this.UpdateSaveButtonState();
					this.DisplaySizeComparisonImages(pokemonData);
				}
			}
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x00055FE0 File Offset: 0x000541E0
		private void LoadCryDataAddress(PokemonData pokemonData)
		{
			bool enable_INDEXED_CRY_TABLE = this.ENABLE_INDEXED_CRY_TABLE;
			checked
			{
				if (enable_INDEXED_CRY_TABLE)
				{
					ushort num = (ushort)pokemonData.Index;
					int num2 = this.CRY_DATA_TABLE_OFFSET_1 + (int)num * this.CRY_DATA_ENTRY_LENGTH;
					uint num3 = BitConverter.ToUInt32(this.romData, num2 + 4);
					bool flag = unchecked((ulong)num3) == 0UL;
					if (flag)
					{
						pokemonData.CryDataAddress = 0U;
					}
					else
					{
						pokemonData.CryDataAddress = num3 - 134217728U;
					}
					pokemonData.OriginalCryDataAddress = pokemonData.CryDataAddress;
				}
				else
				{
					bool flag2 = pokemonData.Index >= 252 && pokemonData.Index <= 276;
					if (flag2)
					{
						pokemonData.CryDataAddress = 0U;
						pokemonData.OriginalCryDataAddress = 0U;
						pokemonData.Gen3CryId = 0;
						pokemonData.OriginalGen3CryId = 0;
					}
					else
					{
						ushort num4 = 0;
						bool flag3 = pokemonData.Index >= this.FIRST_EXTENDED_CRY_POKEMON_INDEX;
						if (flag3)
						{
							int num5 = this.EXTENDED_CRY_TABLE_OFFSET + (pokemonData.Index - this.FIRST_EXTENDED_CRY_POKEMON_INDEX) * 2;
							num4 = BitConverter.ToUInt16(this.romData, num5);
						}
						else
						{
							bool flag4 = pokemonData.Index >= 1;
							if (flag4)
							{
								num4 = (ushort)(pokemonData.Index - 1);
							}
						}
						int num6 = this.CRY_DATA_TABLE_OFFSET_1 + (int)num4 * this.CRY_DATA_ENTRY_LENGTH;
						uint num7 = BitConverter.ToUInt32(this.romData, num6 + 4);
						bool flag5 = unchecked((ulong)num7) == 0UL;
						if (flag5)
						{
							pokemonData.CryDataAddress = 0U;
						}
						else
						{
							pokemonData.CryDataAddress = num7 - 134217728U;
						}
						pokemonData.OriginalCryDataAddress = pokemonData.CryDataAddress;
						pokemonData.Gen3CryId = num4;
						pokemonData.OriginalGen3CryId = num4;
					}
				}
			}
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x00056174 File Offset: 0x00054374
		private void LoadCryData(PokemonData pokemonData)
		{
			bool flag = true;
			bool flag2 = (ulong)pokemonData.CryDataAddress == 0UL;
			if (flag2)
			{
				flag = false;
			}
			bool flag3 = !this.ENABLE_INDEXED_CRY_TABLE;
			if (flag3)
			{
				bool flag4 = pokemonData.Index >= 252 && pokemonData.Index <= 276;
				if (flag4)
				{
					flag = false;
				}
			}
			bool flag5 = flag && !this.ENABLE_INDEXED_CRY_TABLE;
			bool enable_INDEXED_CRY_TABLE = this.ENABLE_INDEXED_CRY_TABLE;
			bool flag6 = !enable_INDEXED_CRY_TABLE && flag && (pokemonData.Index < 1 || pokemonData.Index > 251);
			this.txtCryDataAddress.Enabled = flag;
			this.btnChangeCryDataAddress.Enabled = flag;
			this.btnPlayCry.Enabled = flag;
			this.txtGen3CryConversion.Enabled = flag6;
			this.btnChangeGen3CryConversion.Enabled = flag6;
			this.btnCryDataImportAddress.Enabled = flag;
			this.btnExportCryData.Enabled = flag;
			this.txtCryDataImportAddress.Enabled = flag;
			bool flag7 = flag;
			if (flag7)
			{
				this.txtCryDataAddress.Text = pokemonData.CryDataAddress.ToString("X8");
				bool enable_INDEXED_CRY_TABLE2 = this.ENABLE_INDEXED_CRY_TABLE;
				if (enable_INDEXED_CRY_TABLE2)
				{
					this.txtGen3CryConversion.Text = "0000";
				}
				else
				{
					bool flag8 = flag6;
					if (flag8)
					{
						this.txtGen3CryConversion.Text = pokemonData.Gen3CryId.ToString();
					}
					else
					{
						this.txtGen3CryConversion.Text = "0000";
					}
				}
				bool flag9 = pokemonData.TemporaryCry != null;
				Cry cry;
				if (flag9)
				{
					cry = pokemonData.TemporaryCry;
				}
				else
				{
					cry = CryProcessor.LoadCryFromAddress(pokemonData.CryDataAddress, this.romData);
				}
				bool flag10 = cry != null;
				if (flag10)
				{
					this.lblCrySampleRate2.Text = cry.SampleRate.ToString() + " Hz";
					this.lblCrySamples2.Text = cry.Data.Length.ToString() + " samples";
					this.DisplayCryWaveform(cry);
				}
			}
			else
			{
				this.txtCryDataAddress.Text = "00000000";
				this.txtGen3CryConversion.Text = "0000";
				this.lblCrySampleRate2.Text = "0 Hz";
				this.lblCrySamples2.Text = "0 samples";
				this.DisplayCryWaveform(null);
			}
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x000563E0 File Offset: 0x000545E0
		private void btnChangeCryDataAddress_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			pokemonData.TemporaryCry = null;
			pokemonData.TemporaryCryAddress = 0U;
			uint num = Convert.ToUInt32(this.txtCryDataAddress.Text, 16);
			bool flag = pokemonData.CryDataAddress != num;
			if (flag)
			{
				pokemonData.CryDataAddress = num;
				Cry cry = CryProcessor.LoadCryFromAddress(pokemonData.CryDataAddress, this.romData);
				this.lblCrySampleRate2.Text = cry.SampleRate.ToString() + " Hz";
				this.lblCrySamples2.Text = cry.Data.Length.ToString() + " samples";
				this.DisplayCryWaveform(cry);
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x000564B0 File Offset: 0x000546B0
		private void btnChangeGen3CryConversion_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			ushort num = Convert.ToUInt16(this.txtGen3CryConversion.Text);
			bool flag = pokemonData.Gen3CryId != num;
			if (flag)
			{
				pokemonData.Gen3CryId = num;
				uint num4 = 0;
				checked
				{
					int num2 = this.CRY_DATA_TABLE_OFFSET_1 + (int)num * this.CRY_DATA_ENTRY_LENGTH;
					uint num3 = BitConverter.ToUInt32(this.romData, num2 + 4);
					bool flag2 = unchecked((ulong)num3) == 0UL;
					if (flag2)
					{
						num4 = 0U;
					}
					else
					{
						num4 = num3 - 134217728U;
					}
					pokemonData.CryDataAddress = num4;
					this.txtCryDataAddress.Text = num4.ToString("X8");
				}
				bool flag3 = (ulong)num4 > 0UL;
				if (flag3)
				{
					Cry cry = CryProcessor.LoadCryFromAddress(num4, this.romData);
					this.lblCrySampleRate2.Text = cry.SampleRate.ToString() + " Hz";
					this.lblCrySamples2.Text = cry.Data.Length.ToString() + " samples";
					this.DisplayCryWaveform(cry);
				}
				else
				{
					this.lblCrySampleRate2.Text = "0 Hz";
					this.lblCrySamples2.Text = "0 samples";
					this.DisplayCryWaveform(null);
				}
				this.hasUnsavedChanges = true;
				this.UpdateSaveButtonState();
			}
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x0005660C File Offset: 0x0005480C
		private void SaveCryData(PokemonData pokemonData)
		{
			bool enable_INDEXED_CRY_TABLE = this.ENABLE_INDEXED_CRY_TABLE;
			checked
			{
				ushort num = 0;
				if (enable_INDEXED_CRY_TABLE)
				{
					num = (ushort)pokemonData.Index;
				}
				else
				{
					num = pokemonData.Gen3CryId;
				}
				int num2 = this.CRY_DATA_TABLE_OFFSET_1 + (int)num * this.CRY_DATA_ENTRY_LENGTH;
				bool flag = unchecked((ulong)pokemonData.CryDataAddress) == 0UL;
				uint num3 = 0;
				if (flag)
				{
					num3 = 0U;
				}
				else
				{
					num3 = pokemonData.CryDataAddress + 134217728U;
				}
				byte[] bytes = BitConverter.GetBytes(num3);
				Array.Copy(bytes, 0, this.romData, num2 + 4, 4);
				int num4 = this.CRY_DATA_TABLE_OFFSET_2 + (int)num * this.CRY_DATA_ENTRY_LENGTH;
				Array.Copy(bytes, 0, this.romData, num4 + 4, 4);
				bool flag2 = !this.ENABLE_INDEXED_CRY_TABLE;
				if (flag2)
				{
					bool flag3 = pokemonData.Index >= this.FIRST_EXTENDED_CRY_POKEMON_INDEX;
					if (flag3)
					{
						int num5 = this.EXTENDED_CRY_TABLE_OFFSET + (pokemonData.Index - this.FIRST_EXTENDED_CRY_POKEMON_INDEX) * 2;
						byte[] bytes2 = BitConverter.GetBytes(num);
						Array.Copy(bytes2, 0, this.romData, num5, 2);
					}
				}
			}
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x00056708 File Offset: 0x00054908
		private void btnPlayCry_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			bool flag = pokemonData.TemporaryCry != null;
			if (flag)
			{
				CryProcessor.PlayCry(pokemonData.TemporaryCry);
			}
			else
			{
				CryProcessor.PlayCryFromAddress(pokemonData.CryDataAddress, this.romData);
			}
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x00056758 File Offset: 0x00054958
		private void btnExportCryData_Click(object sender, EventArgs e)
		{
			PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
			string text = this.currentPokemonIndex.ToString("X4");
			bool flag = pokemonData.TemporaryCry != null;
			if (flag)
			{
				CryProcessor.ExportCryToWav(pokemonData.TemporaryCry, string.Format("cry_{0}.wav", text));
			}
			else
			{
				CryProcessor.ExportCryFromAddress(pokemonData.CryDataAddress, this.romData, text);
			}
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x000567C8 File Offset: 0x000549C8
		private void btnCryDataImportAddress_Click(object sender, EventArgs e)
		{
			bool flag = !string.IsNullOrEmpty(this.txtCryDataImportAddress.Text);
			if (flag)
			{
				string text = this.txtCryDataImportAddress.Text.Trim();
				uint num = 0;
				bool flag2 = !uint.TryParse(text, NumberStyles.HexNumber, null, out num);
				if (flag2)
				{
					MessageBox.Show("16進数で入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					PokemonData pokemonData = this.pokemonDataList[this.currentPokemonIndex];
					uint num2 = Convert.ToUInt32(this.txtCryDataImportAddress.Text, 16);
					using (OpenFileDialog openFileDialog = new OpenFileDialog())
					{
						openFileDialog.Filter = "WAVファイル|*.wav";
						openFileDialog.Title = "鳴き声WAVファイルを選択";
						bool flag3 = openFileDialog.ShowDialog() == DialogResult.OK;
						if (flag3)
						{
							Cry cry = CryProcessor.ImportAndCompressWav(openFileDialog.FileName);
							bool flag4 = cry != null;
							if (flag4)
							{
								pokemonData.TemporaryCry = cry;
								pokemonData.TemporaryCryAddress = num2;
								pokemonData.CryDataAddress = num2;
								this.txtCryDataAddress.Text = num2.ToString("X8");
								this.lblCrySampleRate2.Text = cry.SampleRate.ToString() + " Hz";
								this.lblCrySamples2.Text = cry.Data.Length.ToString() + " samples";
								this.DisplayCryWaveform(cry);
								this.hasUnsavedChanges = true;
								this.UpdateSaveButtonState();
							}
						}
					}
				}
			}
			else
			{
				MessageBox.Show("アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		// Token: 0x06000B9B RID: 2971 RVA: 0x00056978 File Offset: 0x00054B78
		private void InitializeWaveformPanel()
		{
			this.pnlCryData.AutoScroll = true;
			this.picWaveform = new PictureBox();
			this.picWaveform.Location = new Point(0, 0);
			this.picWaveform.Size = this.pnlCryData.ClientSize;
			this.picWaveform.BackColor = Color.White;
			this.pnlCryData.Controls.Add(this.picWaveform);
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x000569F0 File Offset: 0x00054BF0
		private void DisplayCryWaveform(Cry cry)
		{
			bool flag = this.picWaveform.Image != null;
			if (flag)
			{
				this.picWaveform.Image.Dispose();
				this.picWaveform.Image = null;
			}
			bool flag2 = cry == null || cry.Data == null || cry.Data.Length == 0;
			checked
			{
				if (flag2)
				{
					this.picWaveform.Size = this.pnlCryData.ClientSize;
					this.picWaveform.Image = new Bitmap(this.picWaveform.Width, this.picWaveform.Height);
					using (Graphics graphics = Graphics.FromImage(this.picWaveform.Image))
					{
						graphics.Clear(Color.White);
					}
				}
				else
				{
					int num = cry.Data.Length;
					int num2 = 128;
					this.picWaveform.Size = new Size(num, num2);
					Bitmap bitmap = new Bitmap(num, num2);
					using (Graphics graphics2 = Graphics.FromImage(bitmap))
					{
						graphics2.Clear(Color.White);
						Pen pen = new Pen(Color.Green, 1f);
						int num3 = (int)Math.Round((double)num2 / 2.0);
						float num4 = (float)num2 / 2f / 128f;
						graphics2.DrawLine(Pens.LightGray, 0, num3, bitmap.Width, num3);
						int num5 = cry.Data.Length - 1;
						for (int i = 1; i <= num5; i++)
						{
							int num6 = i - 1;
							int num7 = (int)Math.Round((double)(unchecked((float)num3 + (float)cry.Data[checked(i - 1)] * num4)));
							int num8 = i;
							int num9 = (int)Math.Round((double)(unchecked((float)num3 + (float)cry.Data[i] * num4)));
							graphics2.DrawLine(pen, num6, num7, num8, num9);
						}
						pen.Dispose();
					}
					this.picWaveform.Image = bitmap;
					this.pnlCryData.AutoScrollPosition = new Point(0, 0);
				}
			}
		}

		// Token: 0x040005F0 RID: 1520
		public const uint GBA_BASE_ADDRESS = 134217728U;

		// Token: 0x040005F1 RID: 1521
		public readonly int POKEMON_NAME_OFFSET;

		// Token: 0x040005F2 RID: 1522
		public readonly int POKEMON_NAME_LENGTH;

		// Token: 0x040005F3 RID: 1523
		public readonly int TOTAL_POKEMON_COUNT;

		// Token: 0x040005F4 RID: 1524
		public const int FIRST_POKEMON_INDEX = 1;

		// Token: 0x040005F5 RID: 1525
		public readonly int FRONT_IMAGE_TABLE_OFFSET;

		// Token: 0x040005F6 RID: 1526
		public readonly int BACK_IMAGE_TABLE_OFFSET;

		// Token: 0x040005F7 RID: 1527
		public readonly int NORMAL_PALETTE_TABLE_OFFSET;

		// Token: 0x040005F8 RID: 1528
		public readonly int SHINY_PALETTE_TABLE_OFFSET;

		// Token: 0x040005F9 RID: 1529
		public const int SPRITE_DATA_ENTRY_LENGTH = 8;

		// Token: 0x040005FA RID: 1530
		public readonly int FRONT_Y_TABLE_OFFSET;

		// Token: 0x040005FB RID: 1531
		public readonly int BACK_Y_TABLE_OFFSET;

		// Token: 0x040005FC RID: 1532
		public const int Y_TABLE_ENTRY_LENGTH = 4;

		// Token: 0x040005FD RID: 1533
		public readonly int SHADOW_TABLE_OFFSET;

		// Token: 0x040005FE RID: 1534
		public const int SHADOW_TABLE_ENTRY_LENGTH = 1;

		// Token: 0x040005FF RID: 1535
		public readonly int ICON_IMAGE_TABLE_OFFSET;

		// Token: 0x04000600 RID: 1536
		public readonly int ICON_PALETTE_ID_TABLE_OFFSET;

		// Token: 0x04000601 RID: 1537
		public readonly int ICON_PALETTE_TABLE_OFFSET;

		// Token: 0x04000602 RID: 1538
		public const int ICON_WIDTH = 32;

		// Token: 0x04000603 RID: 1539
		public const int ICON_HEIGHT = 64;

		// Token: 0x04000604 RID: 1540
		public readonly int ICON_PALETTE_COUNT;

		// Token: 0x04000605 RID: 1541
		public readonly int FOOTPRINT_TABLE_OFFSET;

		// Token: 0x04000606 RID: 1542
		public const int FOOTPRINT_WIDTH = 16;

		// Token: 0x04000607 RID: 1543
		public const int FOOTPRINT_HEIGHT = 16;

		// Token: 0x04000608 RID: 1544
		public const int FOOTPRINT_DATA_SIZE = 32;

		// Token: 0x04000609 RID: 1545
		public readonly int NO_FOOTPRINT_START_INDEX;

		// Token: 0x0400060A RID: 1546
		public readonly int BASE_STATS_OFFSET;

		// Token: 0x0400060B RID: 1547
		public readonly bool ENABLE_BASE_STATS_EXPANSION;

		// Token: 0x0400060C RID: 1548
		public readonly int BASE_STATS_ENTRY_LENGTH;

		// Token: 0x0400060D RID: 1549
		public const int OFFSET_HP = 0;

		// Token: 0x0400060E RID: 1550
		public const int OFFSET_ATTACK = 1;

		// Token: 0x0400060F RID: 1551
		public const int OFFSET_DEFENSE = 2;

		// Token: 0x04000610 RID: 1552
		public const int OFFSET_SPEED = 3;

		// Token: 0x04000611 RID: 1553
		public const int OFFSET_SP_ATTACK = 4;

		// Token: 0x04000612 RID: 1554
		public const int OFFSET_SP_DEFENSE = 5;

		// Token: 0x04000613 RID: 1555
		public const int OFFSET_EV = 10;

		// Token: 0x04000614 RID: 1556
		public const int OFFSET_GENDER = 16;

		// Token: 0x04000615 RID: 1557
		public const int OFFSET_EGG_STEP = 17;

		// Token: 0x04000616 RID: 1558
		public const int OFFSET_EGG_GROUP1 = 20;

		// Token: 0x04000617 RID: 1559
		public const int OFFSET_EGG_GROUP2 = 21;

		// Token: 0x04000618 RID: 1560
		public const int OFFSET_CATCH_RATE = 8;

		// Token: 0x04000619 RID: 1561
		public const int OFFSET_BASE_HAPPINESS = 18;

		// Token: 0x0400061A RID: 1562
		public const int OFFSET_BASE_EXP_NORMAL = 9;

		// Token: 0x0400061B RID: 1563
		public const int OFFSET_BASE_EXP_EXPANDED = 30;

		// Token: 0x0400061C RID: 1564
		public const int OFFSET_GROWTH_RATE = 19;

		// Token: 0x0400061D RID: 1565
		public const int OFFSET_RUN_RATE = 24;

		// Token: 0x0400061E RID: 1566
		public const int OFFSET_COLOR_AND_DIRECTION = 25;

		// Token: 0x0400061F RID: 1567
		public readonly int ABILITY_NAME_TABLE_OFFSET;

		// Token: 0x04000620 RID: 1568
		public readonly int ABILITY_NAME_LENGTH;

		// Token: 0x04000621 RID: 1569
		public readonly int TOTAL_ABILITY_COUNT;

		// Token: 0x04000622 RID: 1570
		public const int OFFSET_ABILITY1 = 22;

		// Token: 0x04000623 RID: 1571
		public const int OFFSET_ABILITY2_NORMAL = 23;

		// Token: 0x04000624 RID: 1572
		public const int OFFSET_ABILITY2_EXPANDED = 26;

		// Token: 0x04000625 RID: 1573
		public const int OFFSET_ABILITY_HIDDEN_NORMAL = 26;

		// Token: 0x04000626 RID: 1574
		public const int OFFSET_ABILITY_HIDDEN_EXPANDED = 28;

		// Token: 0x04000627 RID: 1575
		public const int OFFSET_HOLD_ITEM1 = 12;

		// Token: 0x04000628 RID: 1576
		public const int OFFSET_HOLD_ITEM2 = 14;

		// Token: 0x04000629 RID: 1577
		public readonly int TYPE_TABLE_OFFSET;

		// Token: 0x0400062A RID: 1578
		public readonly int TYPE_NAME_LENGTH;

		// Token: 0x0400062B RID: 1579
		public readonly int TOTAL_TYPE_COUNT;

		// Token: 0x0400062C RID: 1580
		public const int OFFSET_TYPE1 = 6;

		// Token: 0x0400062D RID: 1581
		public const int OFFSET_TYPE2 = 7;

		// Token: 0x0400062E RID: 1582
		public readonly int LEVEL_MOVE_TABLE_OFFSET;

		// Token: 0x0400062F RID: 1583
		public readonly bool ENABLE_MOVE_ID_EXPANSION;

		// Token: 0x04000630 RID: 1584
		public readonly int LEVEL_MOVE_ENTRY_LENGTH;

		// Token: 0x04000631 RID: 1585
		public readonly int TM_HM_LIST_OFFSET;

		// Token: 0x04000632 RID: 1586
		public readonly int TM_HM_LEARN_OFFSET;

		// Token: 0x04000633 RID: 1587
		public readonly int TM_COUNT;

		// Token: 0x04000634 RID: 1588
		public readonly int HM_COUNT;

		// Token: 0x04000635 RID: 1589
		public readonly int MOVE_TUTOR_LIST_OFFSET;

		// Token: 0x04000636 RID: 1590
		public readonly int MOVE_TUTOR_LEARN_OFFSET;

		// Token: 0x04000637 RID: 1591
		public readonly int MOVE_TUTOR_COUNT;

		// Token: 0x04000638 RID: 1592
		public readonly int EVOLUTION_TABLE_OFFSET;

		// Token: 0x04000639 RID: 1593
		public readonly int EVOLUTION_SLOT_LENGTH;

		// Token: 0x0400063A RID: 1594
		public readonly int EVOLUTION_SLOT_COUNT;

		// Token: 0x0400063B RID: 1595
		public readonly int POKEDEX_DATA_OFFSET;

		// Token: 0x0400063C RID: 1596
		public readonly int POKEDEX_DATA_ENTRY_LENGTH;

		// Token: 0x0400063D RID: 1597
		public readonly int POKEDEX_CATEGORY_LENGTH;

		// Token: 0x0400063E RID: 1598
		public readonly bool ENABLE_CATEGORY_NO_SPACE;

		// Token: 0x0400063F RID: 1599
		public const int OFFSET_HEIGHT = 6;

		// Token: 0x04000640 RID: 1600
		public const int OFFSET_WEIGHT = 8;

		// Token: 0x04000641 RID: 1601
		public const int POKEDEX_DESCRIPTION_ADDRESS_OFFSET = 12;

		// Token: 0x04000642 RID: 1602
		public const int POKEDEX_DESCRIPTION_MAX_LENGTH = 256;

		// Token: 0x04000643 RID: 1603
		public const int OFFSET_SIZE_COMP_1 = 18;

		// Token: 0x04000644 RID: 1604
		public const int OFFSET_SIZE_COMP_2 = 20;

		// Token: 0x04000645 RID: 1605
		public const int OFFSET_SIZE_COMP_3 = 22;

		// Token: 0x04000646 RID: 1606
		public const int OFFSET_SIZE_COMP_4 = 24;

		// Token: 0x04000647 RID: 1607
		public readonly int CRY_DATA_TABLE_OFFSET_1;

		// Token: 0x04000648 RID: 1608
		public readonly int CRY_DATA_TABLE_OFFSET_2;

		// Token: 0x04000649 RID: 1609
		public readonly int CRY_DATA_ENTRY_LENGTH;

		// Token: 0x0400064A RID: 1610
		public const int CRY_DATA_ADDRESS_OFFSET = 4;

		// Token: 0x0400064B RID: 1611
		public readonly int MAX_CRY_ID;

		// Token: 0x0400064C RID: 1612
		public readonly int EXTENDED_CRY_TABLE_OFFSET;

		// Token: 0x0400064D RID: 1613
		public readonly int FIRST_EXTENDED_CRY_POKEMON_INDEX;

		// Token: 0x0400064E RID: 1614
		public readonly bool ENABLE_INDEXED_CRY_TABLE;

		// Token: 0x0400064F RID: 1615
		private byte[] romData;

		// Token: 0x04000650 RID: 1616
		private bool hasUnsavedChanges;

		// Token: 0x04000651 RID: 1617
		private int currentPokemonIndex;

		// Token: 0x04000652 RID: 1618
		private Dictionary<int, PokemonData> pokemonDataList;

		// Token: 0x04000653 RID: 1619
		private Bitmap battleBackgroundImage;

		// Token: 0x04000654 RID: 1620
		private Bitmap battleShadowImage;

		// Token: 0x04000655 RID: 1621
		private Bitmap battleBubbleImage;

		// Token: 0x04000656 RID: 1622
		private Dictionary<byte, string> genderMapping;

		// Token: 0x04000657 RID: 1623
		private Dictionary<byte, string> eggStepMapping;

		// Token: 0x04000658 RID: 1624
		private Dictionary<byte, string> eggGroupMapping;

		// Token: 0x04000659 RID: 1625
		private Dictionary<byte, string> growthRateMapping;

		// Token: 0x0400065A RID: 1626
		private Dictionary<byte, string> pokemonColorMapping;

		// Token: 0x0400065B RID: 1627
		private Dictionary<byte, string> pokemonDirectionMapping;

		// Token: 0x0400065C RID: 1628
		private List<EvolutionMethod> evolutionMethods;

		// Token: 0x0400065D RID: 1629
		private List<EvolutionSlot> evolutionSlots;

		// Token: 0x0400065E RID: 1630
		private bool isUpdatingEvolutionUI;

		// Token: 0x0400065F RID: 1631
		private List<LevelMove> levelMoves;

		// Token: 0x04000660 RID: 1632
		private uint levelMoveAddress;

		// Token: 0x04000661 RID: 1633
		private List<ushort> tmIds;

		// Token: 0x04000662 RID: 1634
		private List<ushort> hmIds;

		// Token: 0x04000663 RID: 1635
		private int tmHmCount;

		// Token: 0x04000664 RID: 1636
		private int tmHmDataLength;

		// Token: 0x04000665 RID: 1637
		private List<ushort> moveTutorIds;

		// Token: 0x04000666 RID: 1638
		private int moveTutorCount;

		// Token: 0x04000667 RID: 1639
		private int moveTutorDataLength;

		// Token: 0x04000668 RID: 1640
		private bool isUpdatingSizeCompUI;

		// Token: 0x04000669 RID: 1641
		private PictureBox picWaveform;
	}
}
