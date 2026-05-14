using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x02000028 RID: 40
	public partial class TrainerDataEditor : Form
	{
		// Token: 0x06000BC6 RID: 3014 RVA: 0x0005826C File Offset: 0x0005646C
		public TrainerDataEditor()
		{
			base.Load += this.TrainerDataEditor_Load;
			base.FormClosing += this.TrainerDataEditor_FormClosing;
			this.TRAINER_DATA_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_DATA_OFFSET");
			this.TRAINER_DATA_LENGTH = RomIniReader.ReadHexOrDecimal("TRAINER_DATA_LENGTH");
			this.TRAINER_ENTRY_COUNT = RomIniReader.ReadHexOrDecimal("TRAINER_ENTRY_COUNT");
			this.TRAINER_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("TRAINER_NAME_LENGTH");
			this.isTrainerDataChanged = false;
			this.isPokemonDataChanged = false;
			this.trainerDataList = new Dictionary<int, TrainerDataEditor.TrainerData>();
			this.currentTrainerIndex = -1;
			this.currentPokemonSlots = new List<TrainerDataEditor.PokemonSlotData>();
			this.currentSelectedSlotIndex = -1;
			this.pokemonNameList = new Dictionary<int, PokemonData>();
			this.itemInfoList = new Dictionary<ushort, ItemData.ItemInfo>();
			this.InitializeComponent();
		}

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x06000BC9 RID: 3017 RVA: 0x0005A21D File Offset: 0x0005841D
		// (set) Token: 0x06000BCA RID: 3018 RVA: 0x0005A228 File Offset: 0x00058428
		internal virtual ListBox lstTrainerIdName
		{
			[CompilerGenerated]
			get
			{
				return this._lstTrainerIdName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstTrainerIdName_SelectedIndexChanged);
				ListBox listBox = this._lstTrainerIdName;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstTrainerIdName = value;
				listBox = this._lstTrainerIdName;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06000BCB RID: 3019 RVA: 0x0005A26B File Offset: 0x0005846B
		// (set) Token: 0x06000BCC RID: 3020 RVA: 0x0005A275 File Offset: 0x00058475
		internal virtual GroupBox grpTrainerData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06000BCD RID: 3021 RVA: 0x0005A27E File Offset: 0x0005847E
		// (set) Token: 0x06000BCE RID: 3022 RVA: 0x0005A288 File Offset: 0x00058488
		internal virtual GroupBox grpTrainerItem
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06000BCF RID: 3023 RVA: 0x0005A291 File Offset: 0x00058491
		// (set) Token: 0x06000BD0 RID: 3024 RVA: 0x0005A29C File Offset: 0x0005849C
		internal virtual NumericUpDown nudTrainerUnknownValue
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerUnknownValue;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerUnknownValue;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerUnknownValue = value;
				numericUpDown = this._nudTrainerUnknownValue;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06000BD1 RID: 3025 RVA: 0x0005A2DF File Offset: 0x000584DF
		// (set) Token: 0x06000BD2 RID: 3026 RVA: 0x0005A2E9 File Offset: 0x000584E9
		internal virtual Label lblTrainerUnknownValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x06000BD3 RID: 3027 RVA: 0x0005A2F2 File Offset: 0x000584F2
		// (set) Token: 0x06000BD4 RID: 3028 RVA: 0x0005A2FC File Offset: 0x000584FC
		internal virtual Label lblPokemonDataNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x06000BD5 RID: 3029 RVA: 0x0005A305 File Offset: 0x00058505
		// (set) Token: 0x06000BD6 RID: 3030 RVA: 0x0005A30F File Offset: 0x0005850F
		internal virtual NumericUpDown nudPokemonDataNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x06000BD7 RID: 3031 RVA: 0x0005A318 File Offset: 0x00058518
		// (set) Token: 0x06000BD8 RID: 3032 RVA: 0x0005A322 File Offset: 0x00058522
		internal virtual TextBox txtPokemonDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x06000BD9 RID: 3033 RVA: 0x0005A32B File Offset: 0x0005852B
		// (set) Token: 0x06000BDA RID: 3034 RVA: 0x0005A335 File Offset: 0x00058535
		internal virtual Label lblPokemonDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x06000BDB RID: 3035 RVA: 0x0005A33E File Offset: 0x0005853E
		// (set) Token: 0x06000BDC RID: 3036 RVA: 0x0005A348 File Offset: 0x00058548
		internal virtual Button btnChangeTrainerName
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeTrainerName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeTrainerName_Click);
				Button button = this._btnChangeTrainerName;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeTrainerName = value;
				button = this._btnChangeTrainerName;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x06000BDD RID: 3037 RVA: 0x0005A38B File Offset: 0x0005858B
		// (set) Token: 0x06000BDE RID: 3038 RVA: 0x0005A395 File Offset: 0x00058595
		internal virtual Label lblTrainerAi
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06000BDF RID: 3039 RVA: 0x0005A39E File Offset: 0x0005859E
		// (set) Token: 0x06000BE0 RID: 3040 RVA: 0x0005A3A8 File Offset: 0x000585A8
		internal virtual CheckBox chkDoubleBattle
		{
			[CompilerGenerated]
			get
			{
				return this._chkDoubleBattle;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerControl_ValueChanged);
				CheckBox checkBox = this._chkDoubleBattle;
				if (checkBox != null)
				{
					checkBox.CheckedChanged -= eventHandler;
				}
				this._chkDoubleBattle = value;
				checkBox = this._chkDoubleBattle;
				if (checkBox != null)
				{
					checkBox.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06000BE1 RID: 3041 RVA: 0x0005A3EB File Offset: 0x000585EB
		// (set) Token: 0x06000BE2 RID: 3042 RVA: 0x0005A3F5 File Offset: 0x000585F5
		internal virtual Label lblIntroMusic
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06000BE3 RID: 3043 RVA: 0x0005A3FE File Offset: 0x000585FE
		// (set) Token: 0x06000BE4 RID: 3044 RVA: 0x0005A408 File Offset: 0x00058608
		internal virtual NumericUpDown nudTrainerAi
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerAi;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerAi;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerAi = value;
				numericUpDown = this._nudTrainerAi;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x06000BE5 RID: 3045 RVA: 0x0005A44B File Offset: 0x0005864B
		// (set) Token: 0x06000BE6 RID: 3046 RVA: 0x0005A458 File Offset: 0x00058658
		internal virtual NumericUpDown nudIntroMusic
		{
			[CompilerGenerated]
			get
			{
				return this._nudIntroMusic;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudIntroMusic;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIntroMusic = value;
				numericUpDown = this._nudIntroMusic;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x06000BE7 RID: 3047 RVA: 0x0005A49B File Offset: 0x0005869B
		// (set) Token: 0x06000BE8 RID: 3048 RVA: 0x0005A4A5 File Offset: 0x000586A5
		internal virtual Label lblTrainerName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06000BE9 RID: 3049 RVA: 0x0005A4AE File Offset: 0x000586AE
		// (set) Token: 0x06000BEA RID: 3050 RVA: 0x0005A4B8 File Offset: 0x000586B8
		internal virtual NumericUpDown nudTrainerSprite
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudTrainerSprite_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerSprite;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerSprite = value;
				numericUpDown = this._nudTrainerSprite;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x06000BEB RID: 3051 RVA: 0x0005A4FB File Offset: 0x000586FB
		// (set) Token: 0x06000BEC RID: 3052 RVA: 0x0005A505 File Offset: 0x00058705
		internal virtual PictureBox picTrainerSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x06000BED RID: 3053 RVA: 0x0005A50E File Offset: 0x0005870E
		// (set) Token: 0x06000BEE RID: 3054 RVA: 0x0005A518 File Offset: 0x00058718
		internal virtual ComboBox cmbTrainerItem1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerItem1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerItemComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerItem1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerItem1 = value;
				comboBox = this._cmbTrainerItem1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06000BEF RID: 3055 RVA: 0x0005A55B File Offset: 0x0005875B
		// (set) Token: 0x06000BF0 RID: 3056 RVA: 0x0005A565 File Offset: 0x00058765
		internal virtual PictureBox picTrainerItem1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06000BF1 RID: 3057 RVA: 0x0005A56E File Offset: 0x0005876E
		// (set) Token: 0x06000BF2 RID: 3058 RVA: 0x0005A578 File Offset: 0x00058778
		internal virtual ComboBox cmbTrainerItem4
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerItem4;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerItemComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerItem4;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerItem4 = value;
				comboBox = this._cmbTrainerItem4;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x06000BF3 RID: 3059 RVA: 0x0005A5BB File Offset: 0x000587BB
		// (set) Token: 0x06000BF4 RID: 3060 RVA: 0x0005A5C8 File Offset: 0x000587C8
		internal virtual ComboBox cmbTrainerItem3
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerItem3;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerItemComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerItem3;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerItem3 = value;
				comboBox = this._cmbTrainerItem3;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x06000BF5 RID: 3061 RVA: 0x0005A60B File Offset: 0x0005880B
		// (set) Token: 0x06000BF6 RID: 3062 RVA: 0x0005A618 File Offset: 0x00058818
		internal virtual ComboBox cmbTrainerItem2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerItem2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.TrainerItemComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerItem2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerItem2 = value;
				comboBox = this._cmbTrainerItem2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x06000BF7 RID: 3063 RVA: 0x0005A65B File Offset: 0x0005885B
		// (set) Token: 0x06000BF8 RID: 3064 RVA: 0x0005A665 File Offset: 0x00058865
		internal virtual PictureBox picTrainerItem4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x06000BF9 RID: 3065 RVA: 0x0005A66E File Offset: 0x0005886E
		// (set) Token: 0x06000BFA RID: 3066 RVA: 0x0005A678 File Offset: 0x00058878
		internal virtual PictureBox picTrainerItem3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06000BFB RID: 3067 RVA: 0x0005A681 File Offset: 0x00058881
		// (set) Token: 0x06000BFC RID: 3068 RVA: 0x0005A68B File Offset: 0x0005888B
		internal virtual PictureBox picTrainerItem2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06000BFD RID: 3069 RVA: 0x0005A694 File Offset: 0x00058894
		// (set) Token: 0x06000BFE RID: 3070 RVA: 0x0005A6A0 File Offset: 0x000588A0
		internal virtual ComboBox cmbTrainerClass
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerClass;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbTrainerClass_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerClass;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerClass = value;
				comboBox = this._cmbTrainerClass;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06000BFF RID: 3071 RVA: 0x0005A6E3 File Offset: 0x000588E3
		// (set) Token: 0x06000C00 RID: 3072 RVA: 0x0005A6ED File Offset: 0x000588ED
		internal virtual Label lblTrainerDataType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x06000C01 RID: 3073 RVA: 0x0005A6F6 File Offset: 0x000588F6
		// (set) Token: 0x06000C02 RID: 3074 RVA: 0x0005A700 File Offset: 0x00058900
		internal virtual Label lblTrainerClass
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x06000C03 RID: 3075 RVA: 0x0005A709 File Offset: 0x00058909
		// (set) Token: 0x06000C04 RID: 3076 RVA: 0x0005A714 File Offset: 0x00058914
		internal virtual ComboBox cmbTrainerDataType
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerDataType;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbTrainerDataType_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerDataType;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerDataType = value;
				comboBox = this._cmbTrainerDataType;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06000C05 RID: 3077 RVA: 0x0005A757 File Offset: 0x00058957
		// (set) Token: 0x06000C06 RID: 3078 RVA: 0x0005A761 File Offset: 0x00058961
		internal virtual GroupBox grpPokemonData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06000C07 RID: 3079 RVA: 0x0005A76A File Offset: 0x0005896A
		// (set) Token: 0x06000C08 RID: 3080 RVA: 0x0005A774 File Offset: 0x00058974
		internal virtual ListBox lstPokemonDataSlot
		{
			[CompilerGenerated]
			get
			{
				return this._lstPokemonDataSlot;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstPokemonDataSlot_SelectedIndexChanged);
				ListBox listBox = this._lstPokemonDataSlot;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstPokemonDataSlot = value;
				listBox = this._lstPokemonDataSlot;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06000C09 RID: 3081 RVA: 0x0005A7B7 File Offset: 0x000589B7
		// (set) Token: 0x06000C0A RID: 3082 RVA: 0x0005A7C4 File Offset: 0x000589C4
		internal virtual Button btnCreatePokemonData
		{
			[CompilerGenerated]
			get
			{
				return this._btnCreatePokemonData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnCreatePokemonData_Click);
				Button button = this._btnCreatePokemonData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnCreatePokemonData = value;
				button = this._btnCreatePokemonData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06000C0B RID: 3083 RVA: 0x0005A807 File Offset: 0x00058A07
		// (set) Token: 0x06000C0C RID: 3084 RVA: 0x0005A814 File Offset: 0x00058A14
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

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06000C0D RID: 3085 RVA: 0x0005A857 File Offset: 0x00058A57
		// (set) Token: 0x06000C0E RID: 3086 RVA: 0x0005A864 File Offset: 0x00058A64
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

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06000C0F RID: 3087 RVA: 0x0005A8A7 File Offset: 0x00058AA7
		// (set) Token: 0x06000C10 RID: 3088 RVA: 0x0005A8B1 File Offset: 0x00058AB1
		internal virtual PictureBox picPokemonIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06000C11 RID: 3089 RVA: 0x0005A8BA File Offset: 0x00058ABA
		// (set) Token: 0x06000C12 RID: 3090 RVA: 0x0005A8C4 File Offset: 0x00058AC4
		internal virtual GroupBox GroupBox2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06000C13 RID: 3091 RVA: 0x0005A8CD File Offset: 0x00058ACD
		// (set) Token: 0x06000C14 RID: 3092 RVA: 0x0005A8D8 File Offset: 0x00058AD8
		internal virtual ComboBox cmbMove1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbMove1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.MoveComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbMove1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbMove1 = value;
				comboBox = this._cmbMove1;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x06000C15 RID: 3093 RVA: 0x0005A91B File Offset: 0x00058B1B
		// (set) Token: 0x06000C16 RID: 3094 RVA: 0x0005A928 File Offset: 0x00058B28
		internal virtual ComboBox cmbMove4
		{
			[CompilerGenerated]
			get
			{
				return this._cmbMove4;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.MoveComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbMove4;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbMove4 = value;
				comboBox = this._cmbMove4;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x06000C17 RID: 3095 RVA: 0x0005A96B File Offset: 0x00058B6B
		// (set) Token: 0x06000C18 RID: 3096 RVA: 0x0005A978 File Offset: 0x00058B78
		internal virtual ComboBox cmbMove3
		{
			[CompilerGenerated]
			get
			{
				return this._cmbMove3;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.MoveComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbMove3;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbMove3 = value;
				comboBox = this._cmbMove3;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x06000C19 RID: 3097 RVA: 0x0005A9BB File Offset: 0x00058BBB
		// (set) Token: 0x06000C1A RID: 3098 RVA: 0x0005A9C8 File Offset: 0x00058BC8
		internal virtual ComboBox cmbMove2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbMove2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.MoveComboBox_SelectedIndexChanged);
				ComboBox comboBox = this._cmbMove2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbMove2 = value;
				comboBox = this._cmbMove2;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06000C1B RID: 3099 RVA: 0x0005AA0B File Offset: 0x00058C0B
		// (set) Token: 0x06000C1C RID: 3100 RVA: 0x0005AA15 File Offset: 0x00058C15
		internal virtual Label lblPokemonIv
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06000C1D RID: 3101 RVA: 0x0005AA1E File Offset: 0x00058C1E
		// (set) Token: 0x06000C1E RID: 3102 RVA: 0x0005AA28 File Offset: 0x00058C28
		internal virtual Label lblPokemonLevel
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06000C1F RID: 3103 RVA: 0x0005AA31 File Offset: 0x00058C31
		// (set) Token: 0x06000C20 RID: 3104 RVA: 0x0005AA3C File Offset: 0x00058C3C
		internal virtual NumericUpDown nudPokemonUnknownValue1
		{
			[CompilerGenerated]
			get
			{
				return this._nudPokemonUnknownValue1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PokemonControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudPokemonUnknownValue1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPokemonUnknownValue1 = value;
				numericUpDown = this._nudPokemonUnknownValue1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06000C21 RID: 3105 RVA: 0x0005AA7F File Offset: 0x00058C7F
		// (set) Token: 0x06000C22 RID: 3106 RVA: 0x0005AA8C File Offset: 0x00058C8C
		internal virtual NumericUpDown nudPokemonIv
		{
			[CompilerGenerated]
			get
			{
				return this._nudPokemonIv;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PokemonControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudPokemonIv;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPokemonIv = value;
				numericUpDown = this._nudPokemonIv;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x06000C23 RID: 3107 RVA: 0x0005AACF File Offset: 0x00058CCF
		// (set) Token: 0x06000C24 RID: 3108 RVA: 0x0005AADC File Offset: 0x00058CDC
		internal virtual NumericUpDown nudPokemonLevel
		{
			[CompilerGenerated]
			get
			{
				return this._nudPokemonLevel;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PokemonControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudPokemonLevel;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPokemonLevel = value;
				numericUpDown = this._nudPokemonLevel;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x06000C25 RID: 3109 RVA: 0x0005AB1F File Offset: 0x00058D1F
		// (set) Token: 0x06000C26 RID: 3110 RVA: 0x0005AB29 File Offset: 0x00058D29
		internal virtual Label lblPokemonUnknownValue1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x06000C27 RID: 3111 RVA: 0x0005AB32 File Offset: 0x00058D32
		// (set) Token: 0x06000C28 RID: 3112 RVA: 0x0005AB3C File Offset: 0x00058D3C
		internal virtual PictureBox picPokemonItem
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x06000C29 RID: 3113 RVA: 0x0005AB45 File Offset: 0x00058D45
		// (set) Token: 0x06000C2A RID: 3114 RVA: 0x0005AB50 File Offset: 0x00058D50
		internal virtual ComboBox cmbPokemonItem
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonItem;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPokemonItem_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPokemonItem;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonItem = value;
				comboBox = this._cmbPokemonItem;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x06000C2B RID: 3115 RVA: 0x0005AB93 File Offset: 0x00058D93
		// (set) Token: 0x06000C2C RID: 3116 RVA: 0x0005AB9D File Offset: 0x00058D9D
		internal virtual Label lblPokemonUnknownValue2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x06000C2D RID: 3117 RVA: 0x0005ABA6 File Offset: 0x00058DA6
		// (set) Token: 0x06000C2E RID: 3118 RVA: 0x0005ABB0 File Offset: 0x00058DB0
		internal virtual NumericUpDown nudPokemonUnknownValue2
		{
			[CompilerGenerated]
			get
			{
				return this._nudPokemonUnknownValue2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PokemonControl_ValueChanged);
				NumericUpDown numericUpDown = this._nudPokemonUnknownValue2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPokemonUnknownValue2 = value;
				numericUpDown = this._nudPokemonUnknownValue2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x06000C2F RID: 3119 RVA: 0x0005ABF3 File Offset: 0x00058DF3
		// (set) Token: 0x06000C30 RID: 3120 RVA: 0x0005AC00 File Offset: 0x00058E00
		internal virtual Button btnChangePokemonData
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokemonData;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokemonData_Click);
				Button button = this._btnChangePokemonData;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokemonData = value;
				button = this._btnChangePokemonData;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x06000C31 RID: 3121 RVA: 0x0005AC43 File Offset: 0x00058E43
		// (set) Token: 0x06000C32 RID: 3122 RVA: 0x0005AC4D File Offset: 0x00058E4D
		internal virtual TextBox txtTrainerName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x06000C33 RID: 3123 RVA: 0x0005AC58 File Offset: 0x00058E58
		private void TrainerDataEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.LoadAllPokemonName();
			this.InitializePokemonComboBox();
			this.LoadItemData();
			this.InitializePokemonItemComboBox();
			this.InitializeTrainerItemComboBoxes();
			this.InitializeMoveComboBoxes();
			this.LoadTrainerClassNames();
			this.nudTrainerSprite.Minimum = 0m;
			this.nudTrainerSprite.Maximum = new decimal(checked(MyProject.Forms.TrainerSpriteEditor.MAX_TRAINER_SPRITE_COUNT - 1));
			this.LoadTrainerSprite(0);
			this.LoadTrainerData();
			this.btnSave.Enabled = false;
			this.isTrainerDataChanged = false;
			bool flag = this.lstTrainerIdName.Items.Count > 0;
			if (flag)
			{
				this.lstTrainerIdName.SelectedIndex = 0;
				this.currentTrainerIndex = 1;
			}
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x0005AD28 File Offset: 0x00058F28
		private void LoadAllPokemonName()
		{
			this.pokemonNameList.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					string pokemonNameFromRom = this.GetPokemonNameFromRom(i);
					PokemonData pokemonData = new PokemonData(i, pokemonNameFromRom);
					this.pokemonNameList.Add(i, pokemonData);
				}
			}
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x0005AD80 File Offset: 0x00058F80
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

		// Token: 0x06000C36 RID: 3126 RVA: 0x0005AE04 File Offset: 0x00059004
		private void InitializePokemonComboBox()
		{
			this.cmbPokemonCode.BeginUpdate();
			this.cmbPokemonCode.Items.Clear();
			this.cmbPokemonCode.Items.Add("なし");
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					bool flag = this.pokemonNameList.ContainsKey(i);
					if (flag)
					{
						this.cmbPokemonCode.Items.Add(this.pokemonNameList[i].Name);
					}
				}
				this.cmbPokemonCode.EndUpdate();
			}
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x0005AEA4 File Offset: 0x000590A4
		private Bitmap LoadPokemonIcon(int pokemonIndex)
		{
			bool flag = pokemonIndex == 0;
			checked
			{
				Bitmap bitmap;
				if (flag)
				{
					bitmap = new Bitmap(32, 64);
				}
				else
				{
					int num = MyProject.Forms.PokemonEditor.ICON_IMAGE_TABLE_OFFSET + pokemonIndex * 4;
					uint num2 = BitConverter.ToUInt32(this.romData, num);
					uint num3 = num2 - 134217728U;
					int num4 = MyProject.Forms.PokemonEditor.ICON_PALETTE_ID_TABLE_OFFSET + pokemonIndex;
					int num5 = (int)this.romData[num4];
					byte[] array = MyProject.Forms.PokedexOrderEditor.LoadIconPalette(num5);
					Color[] array2 = ImageProcessor.LoadPalette(array, true);
					byte[] array3 = new byte[2048];
					Array.Copy(this.romData, (long)(unchecked((ulong)num3)), array3, 0L, Math.Min(unchecked((long)array3.Length), unchecked((long)this.romData.Length) - (long)(unchecked((ulong)num3))));
					Bitmap bitmap2 = ImageProcessor.LoadSprite(ref array3, array2, 32, 64, false);
					bitmap = bitmap2;
				}
				return bitmap;
			}
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x0005AF80 File Offset: 0x00059180
		private void cmbPokemonCode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.cmbPokemonCode.SelectedIndex == 0;
			if (flag)
			{
				this.picPokemonIcon.Image = new Bitmap(32, 64);
			}
			else
			{
				int selectedIndex = this.cmbPokemonCode.SelectedIndex;
				Bitmap bitmap = this.LoadPokemonIcon(selectedIndex);
				this.picPokemonIcon.Image = bitmap;
			}
			this.picPokemonIcon.Refresh();
			bool flag2 = this.currentSelectedSlotIndex >= 0 && this.currentSelectedSlotIndex < this.currentPokemonSlots.Count;
			if (flag2)
			{
				TrainerDataEditor.PokemonSlotData pokemonSlotData = this.currentPokemonSlots[this.currentSelectedSlotIndex];
				bool flag3 = this.cmbPokemonCode.SelectedIndex != (int)pokemonSlotData.PokemonCode;
				if (flag3)
				{
					this.SetPokemonDataChanged();
				}
			}
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x0005B048 File Offset: 0x00059248
		private void LoadItemData()
		{
			ushort num = checked((ushort)(ItemData.TOTAL_ITEM_COUNT - 1));
			for (ushort num2 = 0; num2 <= num; num2 += 1)
			{
				this.itemInfoList.Add(num2, ItemData.GetItemInfo(this.romData, num2));
			}
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x0005B088 File Offset: 0x00059288
		private void InitializePokemonItemComboBox()
		{
			this.cmbPokemonItem.BeginUpdate();
			this.cmbPokemonItem.Items.Clear();
			ushort num = checked((ushort)(ItemData.TOTAL_ITEM_COUNT - 1));
			for (ushort num2 = 0; num2 <= num; num2 += 1)
			{
				bool flag = this.itemInfoList.ContainsKey(num2);
				if (flag)
				{
					this.cmbPokemonItem.Items.Add(this.itemInfoList[num2].Name);
				}
			}
			this.cmbPokemonItem.EndUpdate();
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x0005B10C File Offset: 0x0005930C
		private void cmbPokemonItem_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.cmbPokemonItem.SelectedIndex == -1;
			if (flag)
			{
				this.picPokemonItem.Image = null;
			}
			else
			{
				ushort num = checked((ushort)this.cmbPokemonItem.SelectedIndex);
				ItemData.DisplayItemImage(this.picPokemonItem, this.romData, num);
			}
			bool flag2 = this.currentSelectedSlotIndex >= 0 && this.currentSelectedSlotIndex < this.currentPokemonSlots.Count;
			if (flag2)
			{
				TrainerDataEditor.PokemonSlotData pokemonSlotData = this.currentPokemonSlots[this.currentSelectedSlotIndex];
				bool flag3 = this.cmbPokemonItem.SelectedIndex != (int)pokemonSlotData.ItemCode;
				if (flag3)
				{
					this.SetPokemonDataChanged();
				}
			}
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x0005B1BA File Offset: 0x000593BA
		private void InitializeTrainerItemComboBoxes()
		{
			this.InitializeSingleItemComboBox(this.cmbTrainerItem1);
			this.InitializeSingleItemComboBox(this.cmbTrainerItem2);
			this.InitializeSingleItemComboBox(this.cmbTrainerItem3);
			this.InitializeSingleItemComboBox(this.cmbTrainerItem4);
		}

		// Token: 0x06000C3D RID: 3133 RVA: 0x0005B1F4 File Offset: 0x000593F4
		private void InitializeSingleItemComboBox(ComboBox comboBox)
		{
			comboBox.BeginUpdate();
			comboBox.Items.Clear();
			ushort num = checked((ushort)(ItemData.TOTAL_ITEM_COUNT - 1));
			for (ushort num2 = 0; num2 <= num; num2 += 1)
			{
				bool flag = this.itemInfoList.ContainsKey(num2);
				if (flag)
				{
					comboBox.Items.Add(this.itemInfoList[num2].Name);
				}
			}
			comboBox.EndUpdate();
		}

		// Token: 0x06000C3E RID: 3134 RVA: 0x0005B264 File Offset: 0x00059464
		private void DisplayTrainerItemImage(PictureBox picBox, ComboBox comboBox)
		{
			ushort num = checked((ushort)comboBox.SelectedIndex);
			ItemData.DisplayItemImage(picBox, this.romData, num);
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x0005B288 File Offset: 0x00059488
		private void TrainerItemComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			ValueTuple<PictureBox, int> comboBoxInfo = this.GetComboBoxInfo(comboBox);
			this.DisplayTrainerItemImage(comboBoxInfo.Item1, comboBox);
			bool flag = this.currentTrainerIndex >= 0 && this.trainerDataList.ContainsKey(this.currentTrainerIndex);
			if (flag)
			{
				TrainerDataEditor.TrainerData trainerData = this.trainerDataList[this.currentTrainerIndex];
				bool flag2 = comboBox.SelectedIndex != (int)trainerData.Items[comboBoxInfo.Item2];
				if (flag2)
				{
					this.SetTrainerDataChanged();
				}
			}
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x0005B310 File Offset: 0x00059510
		private ValueTuple<PictureBox, int> GetComboBoxInfo(ComboBox comboBox)
		{
			string name = comboBox.Name;
			ValueTuple<PictureBox, int> valueTuple;
			if (Operators.CompareString(name, "cmbTrainerItem1", false) != 0)
			{
				if (Operators.CompareString(name, "cmbTrainerItem2", false) != 0)
				{
					if (Operators.CompareString(name, "cmbTrainerItem3", false) != 0)
					{
						if (Operators.CompareString(name, "cmbTrainerItem4", false) != 0)
						{
							valueTuple = new ValueTuple<PictureBox, int>(null, -1);
						}
						else
						{
							valueTuple = new ValueTuple<PictureBox, int>(this.picTrainerItem4, 3);
						}
					}
					else
					{
						valueTuple = new ValueTuple<PictureBox, int>(this.picTrainerItem3, 2);
					}
				}
				else
				{
					valueTuple = new ValueTuple<PictureBox, int>(this.picTrainerItem2, 1);
				}
			}
			else
			{
				valueTuple = new ValueTuple<PictureBox, int>(this.picTrainerItem1, 0);
			}
			return valueTuple;
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0005B3B4 File Offset: 0x000595B4
		private void InitializeMoveComboBoxes()
		{
			this.moveNames = MoveData.GetMoveNames(this.romData);
			this.InitializeSingleMoveComboBox(this.cmbMove1);
			this.InitializeSingleMoveComboBox(this.cmbMove2);
			this.InitializeSingleMoveComboBox(this.cmbMove3);
			this.InitializeSingleMoveComboBox(this.cmbMove4);
		}

		// Token: 0x06000C42 RID: 3138 RVA: 0x0005B408 File Offset: 0x00059608
		private void InitializeSingleMoveComboBox(ComboBox comboBox)
		{
			comboBox.BeginUpdate();
			comboBox.Items.Clear();
			checked
			{
				int num = this.moveNames.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					comboBox.Items.Add(this.moveNames[i]);
				}
				comboBox.EndUpdate();
			}
		}

		// Token: 0x06000C43 RID: 3139 RVA: 0x0005B464 File Offset: 0x00059664
		private void LoadTrainerClassNames()
		{
			this.cmbTrainerClass.BeginUpdate();
			this.cmbTrainerClass.Items.Clear();
			checked
			{
				int num = MyProject.Forms.TrainerSpriteEditor.TRAINER_CLASS_NAME_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = MyProject.Forms.TrainerSpriteEditor.TRAINER_CLASS_NAME_TABLE_OFFSET + i * MyProject.Forms.TrainerSpriteEditor.TRAINER_CLASS_NAME_LENGTH;
					string text = TextConverter.BytesToPokemonString(this.romData, num2, MyProject.Forms.TrainerSpriteEditor.TRAINER_CLASS_NAME_LENGTH);
					this.cmbTrainerClass.Items.Add(text);
				}
				this.cmbTrainerClass.EndUpdate();
			}
		}

		// Token: 0x06000C44 RID: 3140 RVA: 0x0005B50C File Offset: 0x0005970C
		private void LoadTrainerSprite(int spriteIndex)
		{
			checked
			{
				int num = MyProject.Forms.TrainerSpriteEditor.TRAINER_SPRITE_TABLE_OFFSET + spriteIndex * 8;
				uint num2 = BitConverter.ToUInt32(this.romData, num) - 134217728U;
				int num3 = MyProject.Forms.TrainerSpriteEditor.TRAINER_PALETTE_TABLE_OFFSET + spriteIndex * 8;
				uint num4 = BitConverter.ToUInt32(this.romData, num3) - 134217728U;
				this.DisplayTrainerSprite(num2, num4);
			}
		}

		// Token: 0x06000C45 RID: 3141 RVA: 0x0005B574 File Offset: 0x00059774
		private void DisplayTrainerSprite(uint imageAddress, uint paletteAddress)
		{
			byte[] array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, imageAddress, false);
			byte[] array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, paletteAddress, true);
			Color[] array3 = ImageProcessor.LoadPalette(array2, true);
			Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, 64, 64, false);
			this.picTrainerSprite.Image = bitmap;
			this.picTrainerSprite.Refresh();
		}

		// Token: 0x06000C46 RID: 3142 RVA: 0x0005B5D0 File Offset: 0x000597D0
		private void nudTrainerSprite_ValueChanged(object sender, EventArgs e)
		{
			int num = Convert.ToInt32(this.nudTrainerSprite.Value);
			this.LoadTrainerSprite(num);
			this.SetTrainerDataChanged();
		}

		// Token: 0x06000C47 RID: 3143 RVA: 0x0005B600 File Offset: 0x00059800
		private void LoadTrainerData()
		{
			this.trainerDataList.Clear();
			this.lstTrainerIdName.Items.Clear();
			checked
			{
				int num = this.TRAINER_ENTRY_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					TrainerDataEditor.TrainerData trainerData = this.ReadTrainerData(i);
					bool flag = trainerData != null;
					if (flag)
					{
						this.trainerDataList.Add(i, trainerData);
						string text = string.Format("{0} - {1}", i.ToString("X4"), trainerData.Name);
						this.lstTrainerIdName.Items.Add(text);
					}
				}
			}
		}

		// Token: 0x06000C48 RID: 3144 RVA: 0x0005B694 File Offset: 0x00059894
		private TrainerDataEditor.TrainerData ReadTrainerData(int index)
		{
			checked
			{
				int num = this.TRAINER_DATA_OFFSET + index * this.TRAINER_DATA_LENGTH;
				TrainerDataEditor.TrainerData trainerData = new TrainerDataEditor.TrainerData();
				trainerData.Index = index;
				trainerData.DataType = this.romData[num + 0];
				trainerData.ClassId = this.romData[num + 1];
				trainerData.IntroMusic = this.romData[num + 2];
				trainerData.SpriteId = this.romData[num + 3];
				byte[] array = new byte[this.TRAINER_NAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num + 4, array, 0, this.TRAINER_NAME_LENGTH);
				trainerData.Name = TextConverter.BytesToPokemonString(array, 0, this.TRAINER_NAME_LENGTH);
				trainerData.Items = new ushort[4];
				trainerData.Items[0] = BitConverter.ToUInt16(this.romData, num + 10);
				trainerData.Items[1] = BitConverter.ToUInt16(this.romData, num + 10 + 2);
				trainerData.Items[2] = BitConverter.ToUInt16(this.romData, num + 10 + 4);
				trainerData.Items[3] = BitConverter.ToUInt16(this.romData, num + 10 + 6);
				trainerData.IsDoubleBattle = (this.romData[num + 18] & 1) > 0;
				trainerData.Ai = this.romData[num + 20];
				trainerData.PokemonCount = this.romData[num + 24];
				trainerData.UnknownValue = BitConverter.ToUInt16(this.romData, num + 26);
				uint num2 = BitConverter.ToUInt32(this.romData, num + 28);
				trainerData.PokemonDataAddress = num2 - 134217728U;
				return trainerData;
			}
		}

		// Token: 0x06000C49 RID: 3145 RVA: 0x0005B824 File Offset: 0x00059A24
		private void DisplayTrainerData(TrainerDataEditor.TrainerData trainer)
		{
			this.cmbTrainerDataType.SelectedIndex = (int)trainer.DataType;
			this.cmbTrainerClass.SelectedIndex = (int)trainer.ClassId;
			this.nudIntroMusic.Value = new decimal((int)trainer.IntroMusic);
			this.nudTrainerSprite.Value = new decimal((int)trainer.SpriteId);
			this.LoadTrainerSprite((int)trainer.SpriteId);
			this.txtTrainerName.Text = trainer.Name;
			this.cmbTrainerItem1.SelectedIndex = (int)trainer.Items[0];
			this.cmbTrainerItem2.SelectedIndex = (int)trainer.Items[1];
			this.cmbTrainerItem3.SelectedIndex = (int)trainer.Items[2];
			this.cmbTrainerItem4.SelectedIndex = (int)trainer.Items[3];
			this.chkDoubleBattle.Checked = trainer.IsDoubleBattle;
			this.nudTrainerAi.Value = new decimal((int)trainer.Ai);
			this.nudPokemonDataNum.Value = new decimal((int)trainer.PokemonCount);
			this.nudTrainerUnknownValue.Value = new decimal((int)trainer.UnknownValue);
			this.txtPokemonDataAddress.Text = trainer.PokemonDataAddress.ToString("X8");
			this.LoadPokemonDataForTrainer(trainer);
		}

		// Token: 0x06000C4A RID: 3146 RVA: 0x0005B971 File Offset: 0x00059B71
		private void SetTrainerDataChanged()
		{
			this.isTrainerDataChanged = true;
			this.btnSave.Enabled = true;
		}

		// Token: 0x06000C4B RID: 3147 RVA: 0x0005B988 File Offset: 0x00059B88
		private void cmbTrainerClass_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.currentTrainerIndex >= 0 && this.trainerDataList.ContainsKey(this.currentTrainerIndex);
			if (flag)
			{
				TrainerDataEditor.TrainerData trainerData = this.trainerDataList[this.currentTrainerIndex];
				bool flag2 = this.cmbTrainerClass.SelectedIndex != (int)trainerData.ClassId;
				if (flag2)
				{
					this.SetTrainerDataChanged();
				}
			}
		}

		// Token: 0x06000C4C RID: 3148 RVA: 0x0005B9EE File Offset: 0x00059BEE
		private void TrainerControl_ValueChanged(object sender, EventArgs e)
		{
			this.SetTrainerDataChanged();
		}

		// Token: 0x06000C4D RID: 3149 RVA: 0x0005B9F8 File Offset: 0x00059BF8
		private void lstTrainerIdName_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.isTrainerDataChanged;
			checked
			{
				if (flag)
				{
					DialogResult dialogResult = MessageBox.Show("変更が保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Cancel)
					{
						this.lstTrainerIdName.SelectedIndexChanged -= this.lstTrainerIdName_SelectedIndexChanged;
						this.lstTrainerIdName.SelectedIndex = this.currentTrainerIndex - 1;
						this.lstTrainerIdName.SelectedIndexChanged += this.lstTrainerIdName_SelectedIndexChanged;
						return;
					}
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult == DialogResult.No)
						{
							TrainerDataEditor.TrainerData trainerData = this.ReadTrainerData(this.currentTrainerIndex);
							this.trainerDataList[this.currentTrainerIndex] = trainerData;
						}
					}
					else
					{
						this.SaveCurrentTrainerData();
					}
				}
				int num = this.lstTrainerIdName.SelectedIndex + 1;
				this.currentTrainerIndex = num;
				TrainerDataEditor.TrainerData trainerData2 = this.trainerDataList[num];
				this.DisplayTrainerData(trainerData2);
				this.isTrainerDataChanged = false;
				this.btnSave.Enabled = false;
			}
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x0005BAF0 File Offset: 0x00059CF0
		private void TrainerDataEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.isTrainerDataChanged;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("変更が保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
						this.SaveCurrentTrainerData();
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x0005BB49 File Offset: 0x00059D49
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveCurrentTrainerData();
		}

		// Token: 0x06000C50 RID: 3152 RVA: 0x0005BB54 File Offset: 0x00059D54
		private void SaveCurrentTrainerData()
		{
			this.lstTrainerIdName.SelectedIndexChanged -= this.lstTrainerIdName_SelectedIndexChanged;
			TrainerDataEditor.TrainerData trainerData = this.trainerDataList[this.currentTrainerIndex];
			checked
			{
				trainerData.DataType = (byte)this.cmbTrainerDataType.SelectedIndex;
				trainerData.ClassId = (byte)this.cmbTrainerClass.SelectedIndex;
				trainerData.IntroMusic = Convert.ToByte(this.nudIntroMusic.Value);
				trainerData.SpriteId = Convert.ToByte(this.nudTrainerSprite.Value);
				trainerData.Name = this.txtTrainerName.Text;
				trainerData.Items[0] = (ushort)this.cmbTrainerItem1.SelectedIndex;
				trainerData.Items[1] = (ushort)this.cmbTrainerItem2.SelectedIndex;
				trainerData.Items[2] = (ushort)this.cmbTrainerItem3.SelectedIndex;
				trainerData.Items[3] = (ushort)this.cmbTrainerItem4.SelectedIndex;
				trainerData.IsDoubleBattle = this.chkDoubleBattle.Checked;
				trainerData.Ai = Convert.ToByte(this.nudTrainerAi.Value);
				trainerData.PokemonCount = Convert.ToByte(this.nudPokemonDataNum.Value);
				trainerData.UnknownValue = Convert.ToUInt16(this.nudTrainerUnknownValue.Value);
				trainerData.PokemonDataAddress = Convert.ToUInt32(this.txtPokemonDataAddress.Text.Trim(), 16);
				this.WriteTrainerDataToRom(trainerData);
				this.WriteAllPokemonDataToRom(trainerData);
				this.UpdateTrainerListDisplayName(this.currentTrainerIndex, trainerData.Name);
				this.isTrainerDataChanged = false;
				this.isPokemonDataChanged = false;
				this.btnSave.Enabled = false;
				this.btnChangePokemonData.Enabled = false;
				this.lstTrainerIdName.SelectedIndexChanged += this.lstTrainerIdName_SelectedIndexChanged;
			}
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x0005BD1C File Offset: 0x00059F1C
		private void WriteTrainerDataToRom(TrainerDataEditor.TrainerData trainer)
		{
			checked
			{
				int num = this.TRAINER_DATA_OFFSET + trainer.Index * this.TRAINER_DATA_LENGTH;
				this.romData[num + 0] = trainer.DataType;
				this.romData[num + 1] = trainer.ClassId;
				this.romData[num + 2] = trainer.IntroMusic;
				this.romData[num + 3] = trainer.SpriteId;
				byte[] array = TextConverter.PokemonStringToBytes(trainer.Name, this.TRAINER_NAME_LENGTH);
				int num2 = this.TRAINER_NAME_LENGTH - 1;
				for (int i = 0; i <= num2; i++)
				{
					this.romData[num + 4 + i] = 0;
				}
				Array.Copy(array, 0, this.romData, num + 4, Math.Min(array.Length, this.TRAINER_NAME_LENGTH));
				BitConverter.GetBytes(trainer.Items[0]).CopyTo(this.romData, num + 10);
				BitConverter.GetBytes(trainer.Items[1]).CopyTo(this.romData, num + 10 + 2);
				BitConverter.GetBytes(trainer.Items[2]).CopyTo(this.romData, num + 10 + 4);
				BitConverter.GetBytes(trainer.Items[3]).CopyTo(this.romData, num + 10 + 6);
				this.romData[num + 18] = (byte)(trainer.IsDoubleBattle ? 1 : 0);
				this.romData[num + 20] = trainer.Ai;
				this.romData[num + 24] = trainer.PokemonCount;
				BitConverter.GetBytes(trainer.UnknownValue).CopyTo(this.romData, num + 26);
				uint num3 = trainer.PokemonDataAddress + 134217728U;
				BitConverter.GetBytes(num3).CopyTo(this.romData, num + 28);
				MainForm.romData = this.romData;
			}
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x0005BED8 File Offset: 0x0005A0D8
		private void UpdateTrainerListDisplayName(int trainerIndex, string trainerName)
		{
			int num = checked(trainerIndex - 1);
			string text = string.Format("{0} - {1}", trainerIndex.ToString("X4"), trainerName);
			this.lstTrainerIdName.Items[num] = text;
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x0005BF18 File Offset: 0x0005A118
		private void btnChangeTrainerName_Click(object sender, EventArgs e)
		{
			string text = this.txtTrainerName.Text;
			int num = checked(this.TRAINER_NAME_LENGTH - 1);
			bool flag = text.Length > num;
			if (flag)
			{
				text = text.Substring(0, num);
				this.txtTrainerName.Text = text;
			}
			TrainerDataEditor.TrainerData trainerData = this.trainerDataList[this.currentTrainerIndex];
			bool flag2 = Operators.CompareString(trainerData.Name, text, false) != 0;
			if (flag2)
			{
				this.SetTrainerDataChanged();
			}
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x0005BF94 File Offset: 0x0005A194
		private void LoadPokemonDataForTrainer(TrainerDataEditor.TrainerData trainer)
		{
			this.currentPokemonSlots.Clear();
			this.lstPokemonDataSlot.Items.Clear();
			int dataType = (int)trainer.DataType;
			int pokemonSlotSize = this.GetPokemonSlotSize(dataType);
			uint pokemonDataAddress = trainer.PokemonDataAddress;
			checked
			{
				int num = (int)(trainer.PokemonCount - 1);
				for (int i = 0; i <= num; i++)
				{
					uint num2 = pokemonDataAddress + (uint)(i * pokemonSlotSize);
					TrainerDataEditor.PokemonSlotData pokemonSlotData = this.ReadPokemonSlotData(num2, dataType, i);
					pokemonSlotData.SlotIndex = i;
					this.currentPokemonSlots.Add(pokemonSlotData);
					this.lstPokemonDataSlot.Items.Add(string.Format("スロット{0}", i + 1));
				}
				this.btnChangePokemonData.Enabled = false;
				bool flag = this.lstPokemonDataSlot.Items.Count > 0;
				if (flag)
				{
					this.lstPokemonDataSlot.SelectedIndex = 0;
				}
			}
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x0005C078 File Offset: 0x0005A278
		private int GetPokemonSlotSize(int dataType)
		{
			int num;
			switch (dataType)
			{
			case 0:
			case 2:
				num = 8;
				break;
			case 1:
			case 3:
				num = 16;
				break;
			default:
				num = 8;
				break;
			}
			return num;
		}

		// Token: 0x06000C56 RID: 3158 RVA: 0x0005C0B4 File Offset: 0x0005A2B4
		private TrainerDataEditor.PokemonSlotData ReadPokemonSlotData(uint address, int dataType, int slotIndex)
		{
			TrainerDataEditor.PokemonSlotData pokemonSlotData = new TrainerDataEditor.PokemonSlotData();
			checked
			{
				int num = (int)address;
				pokemonSlotData.Iv = this.romData[num + 0];
				pokemonSlotData.UnknownValue1 = this.romData[num + 1];
				pokemonSlotData.Level = this.romData[num + 2];
				pokemonSlotData.UnknownValue2 = this.romData[num + 3];
				pokemonSlotData.PokemonCode = BitConverter.ToUInt16(this.romData, num + 4);
				switch (dataType)
				{
				case 0:
					pokemonSlotData.ItemCode = 0;
					Array.Clear(pokemonSlotData.Moves, 0, pokemonSlotData.Moves.Length);
					break;
				case 1:
					pokemonSlotData.ItemCode = 0;
					pokemonSlotData.Moves[0] = BitConverter.ToUInt16(this.romData, num + 6);
					pokemonSlotData.Moves[1] = BitConverter.ToUInt16(this.romData, num + 8);
					pokemonSlotData.Moves[2] = BitConverter.ToUInt16(this.romData, num + 10);
					pokemonSlotData.Moves[3] = BitConverter.ToUInt16(this.romData, num + 12);
					break;
				case 2:
					pokemonSlotData.ItemCode = BitConverter.ToUInt16(this.romData, num + 6);
					Array.Clear(pokemonSlotData.Moves, 0, pokemonSlotData.Moves.Length);
					break;
				case 3:
					pokemonSlotData.ItemCode = BitConverter.ToUInt16(this.romData, num + 6);
					pokemonSlotData.Moves[0] = BitConverter.ToUInt16(this.romData, num + 8);
					pokemonSlotData.Moves[1] = BitConverter.ToUInt16(this.romData, num + 10);
					pokemonSlotData.Moves[2] = BitConverter.ToUInt16(this.romData, num + 12);
					pokemonSlotData.Moves[3] = BitConverter.ToUInt16(this.romData, num + 14);
					break;
				}
				return pokemonSlotData;
			}
		}

		// Token: 0x06000C57 RID: 3159 RVA: 0x0005C274 File Offset: 0x0005A474
		private void DisplayPokemonSlotData(TrainerDataEditor.PokemonSlotData slotData, int dataType)
		{
			this.nudPokemonLevel.Value = new decimal((int)slotData.Level);
			this.nudPokemonIv.Value = new decimal((int)slotData.Iv);
			this.nudPokemonUnknownValue1.Value = new decimal((int)slotData.UnknownValue1);
			this.nudPokemonUnknownValue2.Value = new decimal((int)slotData.UnknownValue2);
			this.cmbPokemonCode.SelectedIndex = (int)slotData.PokemonCode;
			bool flag = dataType == 2 || dataType == 3;
			if (flag)
			{
				this.cmbPokemonItem.Enabled = true;
				this.cmbPokemonItem.SelectedIndex = (int)slotData.ItemCode;
			}
			else
			{
				this.cmbPokemonItem.Enabled = false;
				this.cmbPokemonItem.SelectedIndex = -1;
			}
			bool flag2 = dataType == 1 || dataType == 3;
			if (flag2)
			{
				this.cmbMove1.Enabled = true;
				this.cmbMove2.Enabled = true;
				this.cmbMove3.Enabled = true;
				this.cmbMove4.Enabled = true;
				this.cmbMove1.SelectedIndex = (int)slotData.Moves[0];
				this.cmbMove2.SelectedIndex = (int)slotData.Moves[1];
				this.cmbMove3.SelectedIndex = (int)slotData.Moves[2];
				this.cmbMove4.SelectedIndex = (int)slotData.Moves[3];
			}
			else
			{
				this.cmbMove1.Enabled = false;
				this.cmbMove2.Enabled = false;
				this.cmbMove3.Enabled = false;
				this.cmbMove4.Enabled = false;
				this.cmbMove1.SelectedIndex = -1;
				this.cmbMove2.SelectedIndex = -1;
				this.cmbMove3.SelectedIndex = -1;
				this.cmbMove4.SelectedIndex = -1;
			}
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x0005C440 File Offset: 0x0005A640
		private void lstPokemonDataSlot_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstPokemonDataSlot.SelectedIndex >= 0 && this.lstPokemonDataSlot.SelectedIndex < this.currentPokemonSlots.Count;
			if (flag)
			{
				this.currentSelectedSlotIndex = this.lstPokemonDataSlot.SelectedIndex;
				TrainerDataEditor.PokemonSlotData pokemonSlotData = this.currentPokemonSlots[this.currentSelectedSlotIndex];
				int selectedIndex = this.cmbTrainerDataType.SelectedIndex;
				this.DisplayPokemonSlotData(pokemonSlotData, selectedIndex);
				this.ResetPokemonDataChangeFlag();
			}
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x0005C4BC File Offset: 0x0005A6BC
		private void cmbTrainerDataType_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.SetTrainerDataChanged();
			bool flag = this.currentSelectedSlotIndex >= 0 && this.currentSelectedSlotIndex < this.currentPokemonSlots.Count;
			if (flag)
			{
				TrainerDataEditor.PokemonSlotData pokemonSlotData = this.currentPokemonSlots[this.currentSelectedSlotIndex];
				int selectedIndex = this.cmbTrainerDataType.SelectedIndex;
				this.DisplayPokemonSlotData(pokemonSlotData, selectedIndex);
				this.SetPokemonDataChanged();
			}
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x0005C524 File Offset: 0x0005A724
		private void SetPokemonDataChanged()
		{
			this.isPokemonDataChanged = true;
			this.btnChangePokemonData.Enabled = true;
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x0005C53B File Offset: 0x0005A73B
		private void ResetPokemonDataChangeFlag()
		{
			this.isPokemonDataChanged = false;
			this.btnChangePokemonData.Enabled = false;
		}

		// Token: 0x06000C5C RID: 3164 RVA: 0x0005C552 File Offset: 0x0005A752
		private void PokemonControl_ValueChanged(object sender, EventArgs e)
		{
			this.SetPokemonDataChanged();
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x0005C55C File Offset: 0x0005A75C
		private void MoveComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.currentSelectedSlotIndex >= 0 && this.currentSelectedSlotIndex < this.currentPokemonSlots.Count;
			if (flag)
			{
				TrainerDataEditor.PokemonSlotData pokemonSlotData = this.currentPokemonSlots[this.currentSelectedSlotIndex];
				ComboBox comboBox = (ComboBox)sender;
				int moveIndexFromComboBox = this.GetMoveIndexFromComboBox(comboBox);
				bool flag2 = comboBox.SelectedIndex != (int)pokemonSlotData.Moves[moveIndexFromComboBox];
				if (flag2)
				{
					this.SetPokemonDataChanged();
				}
			}
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x0005C5D4 File Offset: 0x0005A7D4
		private int GetMoveIndexFromComboBox(ComboBox comboBox)
		{
			string name = comboBox.Name;
			int num;
			if (Operators.CompareString(name, "cmbMove1", false) != 0)
			{
				if (Operators.CompareString(name, "cmbMove2", false) != 0)
				{
					if (Operators.CompareString(name, "cmbMove3", false) != 0)
					{
						if (Operators.CompareString(name, "cmbMove4", false) != 0)
						{
							num = -1;
						}
						else
						{
							num = 3;
						}
					}
					else
					{
						num = 2;
					}
				}
				else
				{
					num = 1;
				}
			}
			else
			{
				num = 0;
			}
			return num;
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x0005C63E File Offset: 0x0005A83E
		private void btnChangePokemonData_Click(object sender, EventArgs e)
		{
			this.UpdateCurrentPokemonSlotData();
			this.SetTrainerDataChanged();
			this.ResetPokemonDataChangeFlag();
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x0005C658 File Offset: 0x0005A858
		private void UpdateCurrentPokemonSlotData()
		{
			TrainerDataEditor.PokemonSlotData pokemonSlotData = this.currentPokemonSlots[this.currentSelectedSlotIndex];
			int selectedIndex = this.cmbTrainerDataType.SelectedIndex;
			pokemonSlotData.Level = Convert.ToByte(this.nudPokemonLevel.Value);
			pokemonSlotData.Iv = Convert.ToByte(this.nudPokemonIv.Value);
			pokemonSlotData.UnknownValue1 = Convert.ToByte(this.nudPokemonUnknownValue1.Value);
			pokemonSlotData.UnknownValue2 = Convert.ToByte(this.nudPokemonUnknownValue2.Value);
			checked
			{
				pokemonSlotData.PokemonCode = (ushort)this.cmbPokemonCode.SelectedIndex;
				bool flag = selectedIndex == 2 || selectedIndex == 3;
				if (flag)
				{
					pokemonSlotData.ItemCode = (ushort)this.cmbPokemonItem.SelectedIndex;
				}
				else
				{
					pokemonSlotData.ItemCode = 0;
				}
				bool flag2 = selectedIndex == 1 || selectedIndex == 3;
				if (flag2)
				{
					pokemonSlotData.Moves[0] = (ushort)((this.cmbMove1.SelectedIndex >= 0) ? this.cmbMove1.SelectedIndex : 0);
					pokemonSlotData.Moves[1] = (ushort)((this.cmbMove2.SelectedIndex >= 0) ? this.cmbMove2.SelectedIndex : 0);
					pokemonSlotData.Moves[2] = (ushort)((this.cmbMove3.SelectedIndex >= 0) ? this.cmbMove3.SelectedIndex : 0);
					pokemonSlotData.Moves[3] = (ushort)((this.cmbMove4.SelectedIndex >= 0) ? this.cmbMove4.SelectedIndex : 0);
				}
				else
				{
					Array.Clear(pokemonSlotData.Moves, 0, pokemonSlotData.Moves.Length);
				}
			}
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x0005C7E4 File Offset: 0x0005A9E4
		private void WriteAllPokemonDataToRom(TrainerDataEditor.TrainerData trainer)
		{
			int dataType = (int)trainer.DataType;
			{
				foreach (TrainerDataEditor.PokemonSlotData pokemonSlotData in this.currentPokemonSlots)
				{
					this.WritePokemonSlotDataToRom(pokemonSlotData, dataType, trainer);
				}
			}
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x0005C848 File Offset: 0x0005AA48
		private void WritePokemonSlotDataToRom(TrainerDataEditor.PokemonSlotData slotData, int dataType, TrainerDataEditor.TrainerData trainer)
		{
			int pokemonSlotSize = this.GetPokemonSlotSize(dataType);
			checked
			{
				uint num = trainer.PokemonDataAddress + (uint)(slotData.SlotIndex * pokemonSlotSize);
				int num2 = (int)num;
				this.romData[num2 + 0] = slotData.Iv;
				this.romData[num2 + 1] = slotData.UnknownValue1;
				this.romData[num2 + 2] = slotData.Level;
				this.romData[num2 + 3] = slotData.UnknownValue2;
				BitConverter.GetBytes(slotData.PokemonCode).CopyTo(this.romData, num2 + 4);
				switch (dataType)
				{
				case 0:
				{
					bool flag = pokemonSlotSize >= 8;
					if (flag)
					{
						Array.Clear(this.romData, num2 + 6, 2);
					}
					break;
				}
				case 1:
					BitConverter.GetBytes(slotData.Moves[0]).CopyTo(this.romData, num2 + 6);
					BitConverter.GetBytes(slotData.Moves[1]).CopyTo(this.romData, num2 + 8);
					BitConverter.GetBytes(slotData.Moves[2]).CopyTo(this.romData, num2 + 10);
					BitConverter.GetBytes(slotData.Moves[3]).CopyTo(this.romData, num2 + 12);
					break;
				case 2:
					BitConverter.GetBytes(slotData.ItemCode).CopyTo(this.romData, num2 + 6);
					break;
				case 3:
					BitConverter.GetBytes(slotData.ItemCode).CopyTo(this.romData, num2 + 6);
					BitConverter.GetBytes(slotData.Moves[0]).CopyTo(this.romData, num2 + 8);
					BitConverter.GetBytes(slotData.Moves[1]).CopyTo(this.romData, num2 + 10);
					BitConverter.GetBytes(slotData.Moves[2]).CopyTo(this.romData, num2 + 12);
					BitConverter.GetBytes(slotData.Moves[3]).CopyTo(this.romData, num2 + 14);
					break;
				}
				MainForm.romData = this.romData;
			}
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x0005CA44 File Offset: 0x0005AC44
		private void btnCreatePokemonData_Click(object sender, EventArgs e)
		{
			using (InsertNewPokemonData insertNewPokemonData = new InsertNewPokemonData())
			{
				bool flag = insertNewPokemonData.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					this.CreateNewPokemonData(insertNewPokemonData.NewTrainerDataType, insertNewPokemonData.NewPokemonDataAddress, insertNewPokemonData.NewPokemonDataNum);
				}
			}
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x0005CAA0 File Offset: 0x0005ACA0
		private void CreateNewPokemonData(int newDataType, string newAddress, int newPokemonCount)
		{
			TrainerDataEditor.TrainerData trainerData = this.trainerDataList[this.currentTrainerIndex];
			checked
			{
				trainerData.DataType = (byte)newDataType;
				trainerData.PokemonDataAddress = Convert.ToUInt32(newAddress, 16);
				trainerData.PokemonCount = (byte)newPokemonCount;
				this.currentPokemonSlots.Clear();
				int pokemonSlotSize = this.GetPokemonSlotSize(newDataType);
				int num = newPokemonCount - 1;
				for (int i = 0; i <= num; i++)
				{
					TrainerDataEditor.PokemonSlotData pokemonSlotData = new TrainerDataEditor.PokemonSlotData();
					pokemonSlotData.SlotIndex = i;
					this.currentPokemonSlots.Add(pokemonSlotData);
				}
				this.UpdateUIAfterPokemonDataCreation(trainerData);
				this.SetTrainerDataChanged();
			}
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x0005CB34 File Offset: 0x0005AD34
		private void UpdateUIAfterPokemonDataCreation(TrainerDataEditor.TrainerData trainer)
		{
			this.cmbTrainerDataType.SelectedIndex = (int)trainer.DataType;
			this.nudPokemonDataNum.Value = new decimal((int)trainer.PokemonCount);
			this.txtPokemonDataAddress.Text = trainer.PokemonDataAddress.ToString("X8");
			this.lstPokemonDataSlot.Items.Clear();
			checked
			{
				int num = (int)(trainer.PokemonCount - 1);
				for (int i = 0; i <= num; i++)
				{
					this.lstPokemonDataSlot.Items.Add(string.Format("スロット{0}", i + 1));
				}
				bool flag = this.lstPokemonDataSlot.Items.Count > 0;
				if (flag)
				{
					this.lstPokemonDataSlot.SelectedIndex = 0;
				}
				this.btnChangePokemonData.Enabled = false;
			}
		}

		// Token: 0x040006B0 RID: 1712
		public readonly int TRAINER_DATA_OFFSET;

		// Token: 0x040006B1 RID: 1713
		public readonly int TRAINER_DATA_LENGTH;

		// Token: 0x040006B2 RID: 1714
		public readonly int TRAINER_ENTRY_COUNT;

		// Token: 0x040006B3 RID: 1715
		public readonly int TRAINER_NAME_LENGTH;

		// Token: 0x040006B4 RID: 1716
		public const int OFFSET_DATA_TYPE = 0;

		// Token: 0x040006B5 RID: 1717
		public const int OFFSET_CLASS_ID = 1;

		// Token: 0x040006B6 RID: 1718
		public const int OFFSET_INTRO_MUSIC = 2;

		// Token: 0x040006B7 RID: 1719
		public const int OFFSET_SPRITE_ID = 3;

		// Token: 0x040006B8 RID: 1720
		public const int OFFSET_NAME = 4;

		// Token: 0x040006B9 RID: 1721
		public const int OFFSET_ITEMS = 10;

		// Token: 0x040006BA RID: 1722
		public const int OFFSET_DOUBLE_BATTLE = 18;

		// Token: 0x040006BB RID: 1723
		public const int OFFSET_AI = 20;

		// Token: 0x040006BC RID: 1724
		public const int OFFSET_POKEMON_COUNT = 24;

		// Token: 0x040006BD RID: 1725
		public const int OFFSET_UNKNOWN_VALUE = 26;

		// Token: 0x040006BE RID: 1726
		public const int OFFSET_POKEMON_DATA_ADDRESS = 28;

		// Token: 0x040006BF RID: 1727
		public const int POKEMON_SLOT_SIZE_BASIC = 8;

		// Token: 0x040006C0 RID: 1728
		public const int POKEMON_SLOT_SIZE_WITH_MOVES = 16;

		// Token: 0x040006C1 RID: 1729
		private byte[] romData;

		// Token: 0x040006C2 RID: 1730
		private bool isTrainerDataChanged;

		// Token: 0x040006C3 RID: 1731
		private bool isPokemonDataChanged;

		// Token: 0x040006C4 RID: 1732
		private Dictionary<int, TrainerDataEditor.TrainerData> trainerDataList;

		// Token: 0x040006C5 RID: 1733
		private int currentTrainerIndex;

		// Token: 0x040006C6 RID: 1734
		private List<TrainerDataEditor.PokemonSlotData> currentPokemonSlots;

		// Token: 0x040006C7 RID: 1735
		private int currentSelectedSlotIndex;

		// Token: 0x040006C8 RID: 1736
		private Dictionary<int, PokemonData> pokemonNameList;

		// Token: 0x040006C9 RID: 1737
		private Dictionary<ushort, ItemData.ItemInfo> itemInfoList;

		// Token: 0x040006CA RID: 1738
		private List<string> moveNames;

		// Token: 0x02000067 RID: 103
		public class TrainerData
		{
			// Token: 0x170005F6 RID: 1526
			// (get) Token: 0x06000FF1 RID: 4081 RVA: 0x0006C718 File Offset: 0x0006A918
			// (set) Token: 0x06000FF2 RID: 4082 RVA: 0x0006C722 File Offset: 0x0006A922
			public int Index { get; set; }

			// Token: 0x170005F7 RID: 1527
			// (get) Token: 0x06000FF3 RID: 4083 RVA: 0x0006C72B File Offset: 0x0006A92B
			// (set) Token: 0x06000FF4 RID: 4084 RVA: 0x0006C735 File Offset: 0x0006A935
			public byte DataType { get; set; }

			// Token: 0x170005F8 RID: 1528
			// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x0006C73E File Offset: 0x0006A93E
			// (set) Token: 0x06000FF6 RID: 4086 RVA: 0x0006C748 File Offset: 0x0006A948
			public byte ClassId { get; set; }

			// Token: 0x170005F9 RID: 1529
			// (get) Token: 0x06000FF7 RID: 4087 RVA: 0x0006C751 File Offset: 0x0006A951
			// (set) Token: 0x06000FF8 RID: 4088 RVA: 0x0006C75B File Offset: 0x0006A95B
			public byte IntroMusic { get; set; }

			// Token: 0x170005FA RID: 1530
			// (get) Token: 0x06000FF9 RID: 4089 RVA: 0x0006C764 File Offset: 0x0006A964
			// (set) Token: 0x06000FFA RID: 4090 RVA: 0x0006C76E File Offset: 0x0006A96E
			public byte SpriteId { get; set; }

			// Token: 0x170005FB RID: 1531
			// (get) Token: 0x06000FFB RID: 4091 RVA: 0x0006C777 File Offset: 0x0006A977
			// (set) Token: 0x06000FFC RID: 4092 RVA: 0x0006C781 File Offset: 0x0006A981
			public string Name { get; set; }

			// Token: 0x170005FC RID: 1532
			// (get) Token: 0x06000FFD RID: 4093 RVA: 0x0006C78A File Offset: 0x0006A98A
			// (set) Token: 0x06000FFE RID: 4094 RVA: 0x0006C794 File Offset: 0x0006A994
			public ushort[] Items { get; set; }

			// Token: 0x170005FD RID: 1533
			// (get) Token: 0x06000FFF RID: 4095 RVA: 0x0006C79D File Offset: 0x0006A99D
			// (set) Token: 0x06001000 RID: 4096 RVA: 0x0006C7A7 File Offset: 0x0006A9A7
			public bool IsDoubleBattle { get; set; }

			// Token: 0x170005FE RID: 1534
			// (get) Token: 0x06001001 RID: 4097 RVA: 0x0006C7B0 File Offset: 0x0006A9B0
			// (set) Token: 0x06001002 RID: 4098 RVA: 0x0006C7BA File Offset: 0x0006A9BA
			public byte Ai { get; set; }

			// Token: 0x170005FF RID: 1535
			// (get) Token: 0x06001003 RID: 4099 RVA: 0x0006C7C3 File Offset: 0x0006A9C3
			// (set) Token: 0x06001004 RID: 4100 RVA: 0x0006C7CD File Offset: 0x0006A9CD
			public byte PokemonCount { get; set; }

			// Token: 0x17000600 RID: 1536
			// (get) Token: 0x06001005 RID: 4101 RVA: 0x0006C7D6 File Offset: 0x0006A9D6
			// (set) Token: 0x06001006 RID: 4102 RVA: 0x0006C7E0 File Offset: 0x0006A9E0
			public ushort UnknownValue { get; set; }

			// Token: 0x17000601 RID: 1537
			// (get) Token: 0x06001007 RID: 4103 RVA: 0x0006C7E9 File Offset: 0x0006A9E9
			// (set) Token: 0x06001008 RID: 4104 RVA: 0x0006C7F3 File Offset: 0x0006A9F3
			public uint PokemonDataAddress { get; set; }
		}

		// Token: 0x02000068 RID: 104
		public class PokemonSlotData
		{
			// Token: 0x06001009 RID: 4105 RVA: 0x0006C7FC File Offset: 0x0006A9FC
			public PokemonSlotData()
			{
				this.Moves = new ushort[4];
				this.PokemonCode = 0;
				this.ItemCode = 0;
				this.Level = 0;
				this.Iv = 0;
				this.UnknownValue1 = 0;
				this.UnknownValue2 = 0;
				this.Moves = new ushort[4];
			}

			// Token: 0x17000602 RID: 1538
			// (get) Token: 0x0600100A RID: 4106 RVA: 0x0006C85B File Offset: 0x0006AA5B
			// (set) Token: 0x0600100B RID: 4107 RVA: 0x0006C865 File Offset: 0x0006AA65
			public int SlotIndex { get; set; }

			// Token: 0x17000603 RID: 1539
			// (get) Token: 0x0600100C RID: 4108 RVA: 0x0006C86E File Offset: 0x0006AA6E
			// (set) Token: 0x0600100D RID: 4109 RVA: 0x0006C878 File Offset: 0x0006AA78
			public ushort PokemonCode { get; set; }

			// Token: 0x17000604 RID: 1540
			// (get) Token: 0x0600100E RID: 4110 RVA: 0x0006C881 File Offset: 0x0006AA81
			// (set) Token: 0x0600100F RID: 4111 RVA: 0x0006C88B File Offset: 0x0006AA8B
			public ushort ItemCode { get; set; }

			// Token: 0x17000605 RID: 1541
			// (get) Token: 0x06001010 RID: 4112 RVA: 0x0006C894 File Offset: 0x0006AA94
			// (set) Token: 0x06001011 RID: 4113 RVA: 0x0006C89E File Offset: 0x0006AA9E
			public byte Level { get; set; }

			// Token: 0x17000606 RID: 1542
			// (get) Token: 0x06001012 RID: 4114 RVA: 0x0006C8A7 File Offset: 0x0006AAA7
			// (set) Token: 0x06001013 RID: 4115 RVA: 0x0006C8B1 File Offset: 0x0006AAB1
			public byte Iv { get; set; }

			// Token: 0x17000607 RID: 1543
			// (get) Token: 0x06001014 RID: 4116 RVA: 0x0006C8BA File Offset: 0x0006AABA
			// (set) Token: 0x06001015 RID: 4117 RVA: 0x0006C8C4 File Offset: 0x0006AAC4
			public byte UnknownValue1 { get; set; }

			// Token: 0x17000608 RID: 1544
			// (get) Token: 0x06001016 RID: 4118 RVA: 0x0006C8CD File Offset: 0x0006AACD
			// (set) Token: 0x06001017 RID: 4119 RVA: 0x0006C8D7 File Offset: 0x0006AAD7
			public byte UnknownValue2 { get; set; }

			// Token: 0x17000609 RID: 1545
			// (get) Token: 0x06001018 RID: 4120 RVA: 0x0006C8E0 File Offset: 0x0006AAE0
			// (set) Token: 0x06001019 RID: 4121 RVA: 0x0006C8EA File Offset: 0x0006AAEA
			public ushort[] Moves { get; set; }
		}
	}
}
