using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x0200000F RID: 15
	public partial class HeldItemMailEditor : Form
	{
		// Token: 0x0600012D RID: 301 RVA: 0x0000B0F4 File Offset: 0x000092F4
		public HeldItemMailEditor()
		{
			base.Load += this.HeldItemMailEditor_Load;
			base.FormClosing += this.HeldItemMailEditor_FormClosing;
			this.HELD_ITEM_MAIL_OFFSET = RomIniReader.ReadHexOrDecimal("HELD_ITEM_MAIL_OFFSET");
			this.HELD_ITEM_MAIL_COUNT = RomIniReader.ReadHexOrDecimal("HELD_ITEM_MAIL_COUNT");
			this.HELD_ITEM_MAIL_LENGTH = RomIniReader.ReadHexOrDecimal("HELD_ITEM_MAIL_LENGTH");
			this.EASYCHAT_GROUP_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("EASYCHAT_GROUP_TABLE_OFFSET");
			this.EASYCHAT_GROUP_COUNT = RomIniReader.ReadHexOrDecimal("EASYCHAT_GROUP_COUNT");
			this.EASYCHAT_GROUP_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("EASYCHAT_GROUP_ENTRY_LENGTH");
			this.hasUnsavedChanges = false;
			this.currentMailId = 0;
			this.InitializeComponent();
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000130 RID: 304 RVA: 0x0000E4A5 File Offset: 0x0000C6A5
		// (set) Token: 0x06000131 RID: 305 RVA: 0x0000E4B0 File Offset: 0x0000C6B0
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

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000132 RID: 306 RVA: 0x0000E4F3 File Offset: 0x0000C6F3
		// (set) Token: 0x06000133 RID: 307 RVA: 0x0000E500 File Offset: 0x0000C700
		internal virtual NumericUpDown nudHeldItemMail
		{
			[CompilerGenerated]
			get
			{
				return this._nudHeldItemMail;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudHeldItemMail_ValueChanged);
				NumericUpDown numericUpDown = this._nudHeldItemMail;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudHeldItemMail = value;
				numericUpDown = this._nudHeldItemMail;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000134 RID: 308 RVA: 0x0000E543 File Offset: 0x0000C743
		// (set) Token: 0x06000135 RID: 309 RVA: 0x0000E54D File Offset: 0x0000C74D
		internal virtual Label lblHeldItemMail
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000136 RID: 310 RVA: 0x0000E556 File Offset: 0x0000C756
		// (set) Token: 0x06000137 RID: 311 RVA: 0x0000E560 File Offset: 0x0000C760
		internal virtual GroupBox grpEasyChatPreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000138 RID: 312 RVA: 0x0000E569 File Offset: 0x0000C769
		// (set) Token: 0x06000139 RID: 313 RVA: 0x0000E574 File Offset: 0x0000C774
		internal virtual TextBox txtEasyChatPreview9
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview9;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview9;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview9 = value;
				textBox = this._txtEasyChatPreview9;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600013A RID: 314 RVA: 0x0000E5B7 File Offset: 0x0000C7B7
		// (set) Token: 0x0600013B RID: 315 RVA: 0x0000E5C4 File Offset: 0x0000C7C4
		internal virtual TextBox txtEasyChatPreview6
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview6;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview6;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview6 = value;
				textBox = this._txtEasyChatPreview6;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600013C RID: 316 RVA: 0x0000E607 File Offset: 0x0000C807
		// (set) Token: 0x0600013D RID: 317 RVA: 0x0000E614 File Offset: 0x0000C814
		internal virtual TextBox txtEasyChatPreview3
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview3;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview3;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview3 = value;
				textBox = this._txtEasyChatPreview3;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600013E RID: 318 RVA: 0x0000E657 File Offset: 0x0000C857
		// (set) Token: 0x0600013F RID: 319 RVA: 0x0000E664 File Offset: 0x0000C864
		internal virtual TextBox txtEasyChatPreview8
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview8;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview8;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview8 = value;
				textBox = this._txtEasyChatPreview8;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000140 RID: 320 RVA: 0x0000E6A7 File Offset: 0x0000C8A7
		// (set) Token: 0x06000141 RID: 321 RVA: 0x0000E6B4 File Offset: 0x0000C8B4
		internal virtual TextBox txtEasyChatPreview5
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview5;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview5;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview5 = value;
				textBox = this._txtEasyChatPreview5;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000142 RID: 322 RVA: 0x0000E6F7 File Offset: 0x0000C8F7
		// (set) Token: 0x06000143 RID: 323 RVA: 0x0000E704 File Offset: 0x0000C904
		internal virtual TextBox txtEasyChatPreview2
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview2;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview2 = value;
				textBox = this._txtEasyChatPreview2;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000E747 File Offset: 0x0000C947
		// (set) Token: 0x06000145 RID: 325 RVA: 0x0000E754 File Offset: 0x0000C954
		internal virtual TextBox txtEasyChatPreview7
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview7;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview7;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview7 = value;
				textBox = this._txtEasyChatPreview7;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000146 RID: 326 RVA: 0x0000E797 File Offset: 0x0000C997
		// (set) Token: 0x06000147 RID: 327 RVA: 0x0000E7A4 File Offset: 0x0000C9A4
		internal virtual TextBox txtEasyChatPreview4
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview4;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview4;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview4 = value;
				textBox = this._txtEasyChatPreview4;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000148 RID: 328 RVA: 0x0000E7E7 File Offset: 0x0000C9E7
		// (set) Token: 0x06000149 RID: 329 RVA: 0x0000E7F4 File Offset: 0x0000C9F4
		internal virtual TextBox txtEasyChatPreview1
		{
			[CompilerGenerated]
			get
			{
				return this._txtEasyChatPreview1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.PreviewTextBox_Enter);
				TextBox textBox = this._txtEasyChatPreview1;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtEasyChatPreview1 = value;
				textBox = this._txtEasyChatPreview1;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x0600014A RID: 330 RVA: 0x0000E837 File Offset: 0x0000CA37
		// (set) Token: 0x0600014B RID: 331 RVA: 0x0000E841 File Offset: 0x0000CA41
		internal virtual RadioButton rbEasyChatPreview9
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x0600014C RID: 332 RVA: 0x0000E84A File Offset: 0x0000CA4A
		// (set) Token: 0x0600014D RID: 333 RVA: 0x0000E854 File Offset: 0x0000CA54
		internal virtual RadioButton rbEasyChatPreview6
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x0600014E RID: 334 RVA: 0x0000E85D File Offset: 0x0000CA5D
		// (set) Token: 0x0600014F RID: 335 RVA: 0x0000E867 File Offset: 0x0000CA67
		internal virtual RadioButton rbEasyChatPreview3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000150 RID: 336 RVA: 0x0000E870 File Offset: 0x0000CA70
		// (set) Token: 0x06000151 RID: 337 RVA: 0x0000E87A File Offset: 0x0000CA7A
		internal virtual RadioButton rbEasyChatPreview8
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000152 RID: 338 RVA: 0x0000E883 File Offset: 0x0000CA83
		// (set) Token: 0x06000153 RID: 339 RVA: 0x0000E88D File Offset: 0x0000CA8D
		internal virtual RadioButton rbEasyChatPreview5
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000154 RID: 340 RVA: 0x0000E896 File Offset: 0x0000CA96
		// (set) Token: 0x06000155 RID: 341 RVA: 0x0000E8A0 File Offset: 0x0000CAA0
		internal virtual RadioButton rbEasyChatPreview2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000156 RID: 342 RVA: 0x0000E8A9 File Offset: 0x0000CAA9
		// (set) Token: 0x06000157 RID: 343 RVA: 0x0000E8B3 File Offset: 0x0000CAB3
		internal virtual RadioButton rbEasyChatPreview7
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000158 RID: 344 RVA: 0x0000E8BC File Offset: 0x0000CABC
		// (set) Token: 0x06000159 RID: 345 RVA: 0x0000E8C6 File Offset: 0x0000CAC6
		internal virtual RadioButton rbEasyChatPreview4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x0600015A RID: 346 RVA: 0x0000E8CF File Offset: 0x0000CACF
		// (set) Token: 0x0600015B RID: 347 RVA: 0x0000E8D9 File Offset: 0x0000CAD9
		internal virtual RadioButton rbEasyChatPreview1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600015C RID: 348 RVA: 0x0000E8E2 File Offset: 0x0000CAE2
		// (set) Token: 0x0600015D RID: 349 RVA: 0x0000E8EC File Offset: 0x0000CAEC
		internal virtual ComboBox cmbEasyChatListPokemon1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListPokemon1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListPokemon1;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListPokemon1 = value;
				comboBox = this._cmbEasyChatListPokemon1;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600015E RID: 350 RVA: 0x0000E92F File Offset: 0x0000CB2F
		// (set) Token: 0x0600015F RID: 351 RVA: 0x0000E939 File Offset: 0x0000CB39
		internal virtual RadioButton rbEasyChatListPokemon1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000160 RID: 352 RVA: 0x0000E942 File Offset: 0x0000CB42
		// (set) Token: 0x06000161 RID: 353 RVA: 0x0000E94C File Offset: 0x0000CB4C
		internal virtual Label lblEasyChatListPokemon1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000162 RID: 354 RVA: 0x0000E955 File Offset: 0x0000CB55
		// (set) Token: 0x06000163 RID: 355 RVA: 0x0000E95F File Offset: 0x0000CB5F
		internal virtual GroupBox grpEasyChatList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000164 RID: 356 RVA: 0x0000E968 File Offset: 0x0000CB68
		// (set) Token: 0x06000165 RID: 357 RVA: 0x0000E974 File Offset: 0x0000CB74
		internal virtual Button btnAssign
		{
			[CompilerGenerated]
			get
			{
				return this._btnAssign;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnAssign_Click);
				Button button = this._btnAssign;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnAssign = value;
				button = this._btnAssign;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000166 RID: 358 RVA: 0x0000E9B7 File Offset: 0x0000CBB7
		// (set) Token: 0x06000167 RID: 359 RVA: 0x0000E9C1 File Offset: 0x0000CBC1
		internal virtual Label lblEasyChatListPokemon2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000168 RID: 360 RVA: 0x0000E9CA File Offset: 0x0000CBCA
		// (set) Token: 0x06000169 RID: 361 RVA: 0x0000E9D4 File Offset: 0x0000CBD4
		internal virtual ComboBox cmbEasyChatListPokemon2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListPokemon2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListPokemon2;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListPokemon2 = value;
				comboBox = this._cmbEasyChatListPokemon2;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600016A RID: 362 RVA: 0x0000EA17 File Offset: 0x0000CC17
		// (set) Token: 0x0600016B RID: 363 RVA: 0x0000EA21 File Offset: 0x0000CC21
		internal virtual RadioButton rbEasyChatListPokemon2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600016C RID: 364 RVA: 0x0000EA2A File Offset: 0x0000CC2A
		// (set) Token: 0x0600016D RID: 365 RVA: 0x0000EA34 File Offset: 0x0000CC34
		internal virtual Label lblEasyChatListMove2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600016E RID: 366 RVA: 0x0000EA3D File Offset: 0x0000CC3D
		// (set) Token: 0x0600016F RID: 367 RVA: 0x0000EA48 File Offset: 0x0000CC48
		internal virtual ComboBox cmbEasyChatListMove2
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListMove2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListMove2;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListMove2 = value;
				comboBox = this._cmbEasyChatListMove2;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000170 RID: 368 RVA: 0x0000EA8B File Offset: 0x0000CC8B
		// (set) Token: 0x06000171 RID: 369 RVA: 0x0000EA95 File Offset: 0x0000CC95
		internal virtual Label lblEasyChatListMove1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000172 RID: 370 RVA: 0x0000EA9E File Offset: 0x0000CC9E
		// (set) Token: 0x06000173 RID: 371 RVA: 0x0000EAA8 File Offset: 0x0000CCA8
		internal virtual RadioButton rbEasyChatListMove2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000174 RID: 372 RVA: 0x0000EAB1 File Offset: 0x0000CCB1
		// (set) Token: 0x06000175 RID: 373 RVA: 0x0000EABC File Offset: 0x0000CCBC
		internal virtual ComboBox cmbEasyChatListMove1
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListMove1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListMove1;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListMove1 = value;
				comboBox = this._cmbEasyChatListMove1;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000176 RID: 374 RVA: 0x0000EAFF File Offset: 0x0000CCFF
		// (set) Token: 0x06000177 RID: 375 RVA: 0x0000EB09 File Offset: 0x0000CD09
		internal virtual RadioButton rbEasyChatListMove1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000178 RID: 376 RVA: 0x0000EB12 File Offset: 0x0000CD12
		// (set) Token: 0x06000179 RID: 377 RVA: 0x0000EB1C File Offset: 0x0000CD1C
		internal virtual Label lblEasyChatListBattle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600017A RID: 378 RVA: 0x0000EB25 File Offset: 0x0000CD25
		// (set) Token: 0x0600017B RID: 379 RVA: 0x0000EB2F File Offset: 0x0000CD2F
		internal virtual Label lblEasyChatListTrainer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600017C RID: 380 RVA: 0x0000EB38 File Offset: 0x0000CD38
		// (set) Token: 0x0600017D RID: 381 RVA: 0x0000EB44 File Offset: 0x0000CD44
		internal virtual ComboBox cmbEasyChatListBattle
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListBattle;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListBattle;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListBattle = value;
				comboBox = this._cmbEasyChatListBattle;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600017E RID: 382 RVA: 0x0000EB87 File Offset: 0x0000CD87
		// (set) Token: 0x0600017F RID: 383 RVA: 0x0000EB94 File Offset: 0x0000CD94
		internal virtual ComboBox cmbEasyChatListTrainer
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListTrainer;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListTrainer;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListTrainer = value;
				comboBox = this._cmbEasyChatListTrainer;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000180 RID: 384 RVA: 0x0000EBD7 File Offset: 0x0000CDD7
		// (set) Token: 0x06000181 RID: 385 RVA: 0x0000EBE1 File Offset: 0x0000CDE1
		internal virtual Label lblEasyChatListStatus
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000182 RID: 386 RVA: 0x0000EBEA File Offset: 0x0000CDEA
		// (set) Token: 0x06000183 RID: 387 RVA: 0x0000EBF4 File Offset: 0x0000CDF4
		internal virtual RadioButton rbEasyChatListBattle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000184 RID: 388 RVA: 0x0000EBFD File Offset: 0x0000CDFD
		// (set) Token: 0x06000185 RID: 389 RVA: 0x0000EC08 File Offset: 0x0000CE08
		internal virtual ComboBox cmbEasyChatListStatus
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListStatus;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListStatus;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListStatus = value;
				comboBox = this._cmbEasyChatListStatus;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000186 RID: 390 RVA: 0x0000EC4B File Offset: 0x0000CE4B
		// (set) Token: 0x06000187 RID: 391 RVA: 0x0000EC55 File Offset: 0x0000CE55
		internal virtual RadioButton rbEasyChatListTrainer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000188 RID: 392 RVA: 0x0000EC5E File Offset: 0x0000CE5E
		// (set) Token: 0x06000189 RID: 393 RVA: 0x0000EC68 File Offset: 0x0000CE68
		internal virtual RadioButton rbEasyChatListStatus
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x0600018A RID: 394 RVA: 0x0000EC71 File Offset: 0x0000CE71
		// (set) Token: 0x0600018B RID: 395 RVA: 0x0000EC7B File Offset: 0x0000CE7B
		internal virtual Label lblEasyChatListAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x0600018C RID: 396 RVA: 0x0000EC84 File Offset: 0x0000CE84
		// (set) Token: 0x0600018D RID: 397 RVA: 0x0000EC8E File Offset: 0x0000CE8E
		internal virtual Label lblEasyChatListCondition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600018E RID: 398 RVA: 0x0000EC97 File Offset: 0x0000CE97
		// (set) Token: 0x0600018F RID: 399 RVA: 0x0000ECA1 File Offset: 0x0000CEA1
		internal virtual Label lblEasyChatListSpeech
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000190 RID: 400 RVA: 0x0000ECAA File Offset: 0x0000CEAA
		// (set) Token: 0x06000191 RID: 401 RVA: 0x0000ECB4 File Offset: 0x0000CEB4
		internal virtual ComboBox cmbEasyChatListAction
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListAction;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListAction;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListAction = value;
				comboBox = this._cmbEasyChatListAction;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000192 RID: 402 RVA: 0x0000ECF7 File Offset: 0x0000CEF7
		// (set) Token: 0x06000193 RID: 403 RVA: 0x0000ED01 File Offset: 0x0000CF01
		internal virtual Label lblEasyChatListEnding
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000194 RID: 404 RVA: 0x0000ED0A File Offset: 0x0000CF0A
		// (set) Token: 0x06000195 RID: 405 RVA: 0x0000ED14 File Offset: 0x0000CF14
		internal virtual ComboBox cmbEasyChatListCondition
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListCondition;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListCondition;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListCondition = value;
				comboBox = this._cmbEasyChatListCondition;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000196 RID: 406 RVA: 0x0000ED57 File Offset: 0x0000CF57
		// (set) Token: 0x06000197 RID: 407 RVA: 0x0000ED61 File Offset: 0x0000CF61
		internal virtual Label lblEasyChatListPeople
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000198 RID: 408 RVA: 0x0000ED6A File Offset: 0x0000CF6A
		// (set) Token: 0x06000199 RID: 409 RVA: 0x0000ED74 File Offset: 0x0000CF74
		internal virtual ComboBox cmbEasyChatListEnding
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListEnding;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListEnding;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListEnding = value;
				comboBox = this._cmbEasyChatListEnding;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x0600019A RID: 410 RVA: 0x0000EDB7 File Offset: 0x0000CFB7
		// (set) Token: 0x0600019B RID: 411 RVA: 0x0000EDC4 File Offset: 0x0000CFC4
		internal virtual ComboBox cmbEasyChatListSpeech
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListSpeech;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListSpeech;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListSpeech = value;
				comboBox = this._cmbEasyChatListSpeech;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x0600019C RID: 412 RVA: 0x0000EE07 File Offset: 0x0000D007
		// (set) Token: 0x0600019D RID: 413 RVA: 0x0000EE11 File Offset: 0x0000D011
		internal virtual Label lblEasyChatListFeeling
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x0600019E RID: 414 RVA: 0x0000EE1A File Offset: 0x0000D01A
		// (set) Token: 0x0600019F RID: 415 RVA: 0x0000EE24 File Offset: 0x0000D024
		internal virtual RadioButton rbEasyChatListAction
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060001A0 RID: 416 RVA: 0x0000EE2D File Offset: 0x0000D02D
		// (set) Token: 0x060001A1 RID: 417 RVA: 0x0000EE38 File Offset: 0x0000D038
		internal virtual ComboBox cmbEasyChatListPeople
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListPeople;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListPeople;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListPeople = value;
				comboBox = this._cmbEasyChatListPeople;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x0000EE7B File Offset: 0x0000D07B
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x0000EE85 File Offset: 0x0000D085
		internal virtual RadioButton rbEasyChatListCondition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x0000EE8E File Offset: 0x0000D08E
		// (set) Token: 0x060001A5 RID: 421 RVA: 0x0000EE98 File Offset: 0x0000D098
		internal virtual Label lblEasyChatListVoice
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060001A6 RID: 422 RVA: 0x0000EEA1 File Offset: 0x0000D0A1
		// (set) Token: 0x060001A7 RID: 423 RVA: 0x0000EEAB File Offset: 0x0000D0AB
		internal virtual RadioButton rbEasyChatListSpeech
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060001A8 RID: 424 RVA: 0x0000EEB4 File Offset: 0x0000D0B4
		// (set) Token: 0x060001A9 RID: 425 RVA: 0x0000EEC0 File Offset: 0x0000D0C0
		internal virtual ComboBox cmbEasyChatListFeeling
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListFeeling;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListFeeling;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListFeeling = value;
				comboBox = this._cmbEasyChatListFeeling;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060001AA RID: 426 RVA: 0x0000EF03 File Offset: 0x0000D103
		// (set) Token: 0x060001AB RID: 427 RVA: 0x0000EF0D File Offset: 0x0000D10D
		internal virtual Label lblEasyChatListGreeting
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060001AC RID: 428 RVA: 0x0000EF16 File Offset: 0x0000D116
		// (set) Token: 0x060001AD RID: 429 RVA: 0x0000EF20 File Offset: 0x0000D120
		internal virtual RadioButton rbEasyChatListEnding
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060001AE RID: 430 RVA: 0x0000EF29 File Offset: 0x0000D129
		// (set) Token: 0x060001AF RID: 431 RVA: 0x0000EF34 File Offset: 0x0000D134
		internal virtual ComboBox cmbEasyChatListVoice
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListVoice;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListVoice;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListVoice = value;
				comboBox = this._cmbEasyChatListVoice;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x0000EF77 File Offset: 0x0000D177
		// (set) Token: 0x060001B1 RID: 433 RVA: 0x0000EF81 File Offset: 0x0000D181
		internal virtual RadioButton rbEasyChatListFeeling
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060001B2 RID: 434 RVA: 0x0000EF8A File Offset: 0x0000D18A
		// (set) Token: 0x060001B3 RID: 435 RVA: 0x0000EF94 File Offset: 0x0000D194
		internal virtual RadioButton rbEasyChatListPeople
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060001B4 RID: 436 RVA: 0x0000EF9D File Offset: 0x0000D19D
		// (set) Token: 0x060001B5 RID: 437 RVA: 0x0000EFA7 File Offset: 0x0000D1A7
		internal virtual RadioButton rbEasyChatListVoice
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060001B6 RID: 438 RVA: 0x0000EFB0 File Offset: 0x0000D1B0
		// (set) Token: 0x060001B7 RID: 439 RVA: 0x0000EFBC File Offset: 0x0000D1BC
		internal virtual ComboBox cmbEasyChatListGreeting
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListGreeting;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListGreeting;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListGreeting = value;
				comboBox = this._cmbEasyChatListGreeting;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060001B8 RID: 440 RVA: 0x0000EFFF File Offset: 0x0000D1FF
		// (set) Token: 0x060001B9 RID: 441 RVA: 0x0000F009 File Offset: 0x0000D209
		internal virtual RadioButton rbEasyChatListGreeting
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060001BA RID: 442 RVA: 0x0000F012 File Offset: 0x0000D212
		// (set) Token: 0x060001BB RID: 443 RVA: 0x0000F01C File Offset: 0x0000D21C
		internal virtual NumericUpDown nudEasyChatCount
		{
			[CompilerGenerated]
			get
			{
				return this._nudEasyChatCount;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudEasyChatCount_ValueChanged);
				NumericUpDown numericUpDown = this._nudEasyChatCount;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudEasyChatCount = value;
				numericUpDown = this._nudEasyChatCount;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060001BC RID: 444 RVA: 0x0000F05F File Offset: 0x0000D25F
		// (set) Token: 0x060001BD RID: 445 RVA: 0x0000F069 File Offset: 0x0000D269
		internal virtual Label lblEasyChatCount
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060001BE RID: 446 RVA: 0x0000F072 File Offset: 0x0000D272
		// (set) Token: 0x060001BF RID: 447 RVA: 0x0000F07C File Offset: 0x0000D27C
		internal virtual Label lblEasyChatListTrendySaying
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x0000F085 File Offset: 0x0000D285
		// (set) Token: 0x060001C1 RID: 449 RVA: 0x0000F08F File Offset: 0x0000D28F
		internal virtual Label lblEasyChatListMisc
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060001C2 RID: 450 RVA: 0x0000F098 File Offset: 0x0000D298
		// (set) Token: 0x060001C3 RID: 451 RVA: 0x0000F0A2 File Offset: 0x0000D2A2
		internal virtual Label lblEasyChatListAdjective
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060001C4 RID: 452 RVA: 0x0000F0AB File Offset: 0x0000D2AB
		// (set) Token: 0x060001C5 RID: 453 RVA: 0x0000F0B8 File Offset: 0x0000D2B8
		internal virtual ComboBox cmbEasyChatListTrendySaying
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListTrendySaying;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListTrendySaying;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListTrendySaying = value;
				comboBox = this._cmbEasyChatListTrendySaying;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x0000F0FB File Offset: 0x0000D2FB
		// (set) Token: 0x060001C7 RID: 455 RVA: 0x0000F105 File Offset: 0x0000D305
		internal virtual Label lblEasyChatListHobby
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x0000F10E File Offset: 0x0000D30E
		// (set) Token: 0x060001C9 RID: 457 RVA: 0x0000F118 File Offset: 0x0000D318
		internal virtual ComboBox cmbEasyChatListAdjective
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListAdjective;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListAdjective;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListAdjective = value;
				comboBox = this._cmbEasyChatListAdjective;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x060001CA RID: 458 RVA: 0x0000F15B File Offset: 0x0000D35B
		// (set) Token: 0x060001CB RID: 459 RVA: 0x0000F168 File Offset: 0x0000D368
		internal virtual ComboBox cmbEasyChatListMisc
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListMisc;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListMisc;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListMisc = value;
				comboBox = this._cmbEasyChatListMisc;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x060001CC RID: 460 RVA: 0x0000F1AB File Offset: 0x0000D3AB
		// (set) Token: 0x060001CD RID: 461 RVA: 0x0000F1B5 File Offset: 0x0000D3B5
		internal virtual Label lblEasyChatListEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x060001CE RID: 462 RVA: 0x0000F1BE File Offset: 0x0000D3BE
		// (set) Token: 0x060001CF RID: 463 RVA: 0x0000F1C8 File Offset: 0x0000D3C8
		internal virtual ComboBox cmbEasyChatListHobby
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListHobby;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListHobby;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListHobby = value;
				comboBox = this._cmbEasyChatListHobby;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x060001D0 RID: 464 RVA: 0x0000F20B File Offset: 0x0000D40B
		// (set) Token: 0x060001D1 RID: 465 RVA: 0x0000F215 File Offset: 0x0000D415
		internal virtual RadioButton rbEasyChatListTrendySaying
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x060001D2 RID: 466 RVA: 0x0000F21E File Offset: 0x0000D41E
		// (set) Token: 0x060001D3 RID: 467 RVA: 0x0000F228 File Offset: 0x0000D428
		internal virtual Label lblEasyChatListTime
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x0000F231 File Offset: 0x0000D431
		// (set) Token: 0x060001D5 RID: 469 RVA: 0x0000F23B File Offset: 0x0000D43B
		internal virtual RadioButton rbEasyChatListMisc
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x060001D6 RID: 470 RVA: 0x0000F244 File Offset: 0x0000D444
		// (set) Token: 0x060001D7 RID: 471 RVA: 0x0000F250 File Offset: 0x0000D450
		internal virtual ComboBox cmbEasyChatListEvent
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListEvent;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListEvent;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListEvent = value;
				comboBox = this._cmbEasyChatListEvent;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x060001D8 RID: 472 RVA: 0x0000F293 File Offset: 0x0000D493
		// (set) Token: 0x060001D9 RID: 473 RVA: 0x0000F29D File Offset: 0x0000D49D
		internal virtual Label lblEasyChatListLifeStyle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x060001DA RID: 474 RVA: 0x0000F2A6 File Offset: 0x0000D4A6
		// (set) Token: 0x060001DB RID: 475 RVA: 0x0000F2B0 File Offset: 0x0000D4B0
		internal virtual RadioButton rbEasyChatListAdjective
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x060001DC RID: 476 RVA: 0x0000F2B9 File Offset: 0x0000D4B9
		// (set) Token: 0x060001DD RID: 477 RVA: 0x0000F2C4 File Offset: 0x0000D4C4
		internal virtual ComboBox cmbEasyChatListTime
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListTime;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListTime;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListTime = value;
				comboBox = this._cmbEasyChatListTime;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x060001DE RID: 478 RVA: 0x0000F307 File Offset: 0x0000D507
		// (set) Token: 0x060001DF RID: 479 RVA: 0x0000F311 File Offset: 0x0000D511
		internal virtual RadioButton rbEasyChatListEvent
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x060001E0 RID: 480 RVA: 0x0000F31A File Offset: 0x0000D51A
		// (set) Token: 0x060001E1 RID: 481 RVA: 0x0000F324 File Offset: 0x0000D524
		internal virtual RadioButton rbEasyChatListHobby
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x060001E2 RID: 482 RVA: 0x0000F32D File Offset: 0x0000D52D
		// (set) Token: 0x060001E3 RID: 483 RVA: 0x0000F337 File Offset: 0x0000D537
		internal virtual RadioButton rbEasyChatListTime
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x060001E4 RID: 484 RVA: 0x0000F340 File Offset: 0x0000D540
		// (set) Token: 0x060001E5 RID: 485 RVA: 0x0000F34C File Offset: 0x0000D54C
		internal virtual ComboBox cmbEasyChatListLifeStyle
		{
			[CompilerGenerated]
			get
			{
				return this._cmbEasyChatListLifeStyle;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.ListComboBox_Enter);
				ComboBox comboBox = this._cmbEasyChatListLifeStyle;
				if (comboBox != null)
				{
					comboBox.Enter -= eventHandler;
				}
				this._cmbEasyChatListLifeStyle = value;
				comboBox = this._cmbEasyChatListLifeStyle;
				if (comboBox != null)
				{
					comboBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x0000F38F File Offset: 0x0000D58F
		// (set) Token: 0x060001E7 RID: 487 RVA: 0x0000F399 File Offset: 0x0000D599
		internal virtual RadioButton rbEasyChatListLifeStyle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000F3A4 File Offset: 0x0000D5A4
		private void HeldItemMailEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.LoadEasyChatGroupTable();
			this.LoadPokemonNames();
			this.LoadMoveNames();
			this.LoadEasyChatLists();
			this.InitializeEasyChatGroupComboBoxMap();
			this.LoadHeldItemMail();
			this.nudHeldItemMail.Maximum = new decimal(checked(this.HELD_ITEM_MAIL_COUNT - 1));
			this.rbEasyChatListPokemon1.Checked = true;
			this.SetUnsavedChanges(false);
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000F415 File Offset: 0x0000D615
		private void SetUnsavedChanges(bool value)
		{
			this.hasUnsavedChanges = value;
			this.btnSave.Enabled = value;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000F42C File Offset: 0x0000D62C
		private DialogResult CheckForUnsavedChanges()
		{
			bool flag = this.hasUnsavedChanges;
			DialogResult dialogResult2;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("変更が保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				dialogResult2 = dialogResult;
			}
			else
			{
				dialogResult2 = DialogResult.OK;
			}
			return dialogResult2;
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000F464 File Offset: 0x0000D664
		private void SaveHeldItemMail(int mailId)
		{
			checked
			{
				int num = this.HELD_ITEM_MAIL_OFFSET + mailId * this.HELD_ITEM_MAIL_LENGTH;
				TextBox[] previewTextBoxes = this.GetPreviewTextBoxes();
				byte[] array = new byte[this.HELD_ITEM_MAIL_LENGTH - 1 + 1];
				int num2 = Convert.ToInt32(this.nudEasyChatCount.Value);
				using (MemoryStream memoryStream = new MemoryStream(array))
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
					{
						int num3 = 0;
						do
						{
							bool flag = num3 < num2;
							if (flag)
							{
								string text = previewTextBoxes[num3].Text;
								bool flag2 = string.IsNullOrEmpty(text);
								if (flag2)
								{
									binaryWriter.Write(0);
								}
								else
								{
									ushort num4 = this.FindEasyChatWordValue(text);
									binaryWriter.Write(num4);
								}
							}
							else
							{
								bool flag3 = num3 == num2;
								if (flag3)
								{
									binaryWriter.Write(ushort.MaxValue);
								}
								else
								{
									binaryWriter.Write(0);
								}
							}
							num3++;
						}
						while (num3 <= 9);
					}
				}
				Array.Copy(array, 0, this.romData, num, this.HELD_ITEM_MAIL_LENGTH);
				MainForm.romData = this.romData;
				this.SetUnsavedChanges(false);
			}
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000F5A0 File Offset: 0x0000D7A0
		private void btnSave_Click(object sender, EventArgs e)
		{
			int num = Convert.ToInt32(this.nudHeldItemMail.Value);
			this.SaveHeldItemMail(num);
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000F5C8 File Offset: 0x0000D7C8
		private ushort FindEasyChatWordValue(string text)
		{
			bool flag = string.IsNullOrEmpty(text);
			ushort num;
			if (flag)
			{
				num = 0;
			}
			else
			{
				bool flag2 = this.easyChatGroupToComboBox == null;
				if (flag2)
				{
					this.InitializeEasyChatGroupComboBoxMap();
				}
				{
					foreach (KeyValuePair<int, ComboBox> keyValuePair in this.easyChatGroupToComboBox)
					{
						int key = keyValuePair.Key;
						ComboBox value = keyValuePair.Value;
						ushort num2 = this.FindEasyChatItemValue(value, text, key);
						bool flag3 = num2 != ushort.MaxValue;
						if (flag3)
						{
							return num2;
						}
					}
				}
				num = 0;
			}
			return num;
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000F680 File Offset: 0x0000D880
		private ushort FindEasyChatItemValue(ComboBox comboBox, string text, int group)
		{
			try
			{
				foreach (object obj in comboBox.Items)
				{
					object objectValue = RuntimeHelpers.GetObjectValue(obj);
					HeldItemMailEditor.EasyChatItem easyChatItem = objectValue as HeldItemMailEditor.EasyChatItem;
					bool flag = easyChatItem != null && Operators.CompareString(easyChatItem.Text, text, false) == 0;
					if (flag)
					{
						int num = easyChatItem.Index;
						bool flag2 = Operators.CompareString(comboBox.Name, "cmbEasyChatListTrainer", false) == 0 || Operators.CompareString(comboBox.Name, "cmbEasyChatListEvent", false) == 0;
						if (flag2)
						{
							num = comboBox.Items.IndexOf(RuntimeHelpers.GetObjectValue(objectValue));
						}
						return checked((ushort)((group << 9) | (num & 511)));
					}
				}
			}
			finally
			{
			}
			return ushort.MaxValue;
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000F770 File Offset: 0x0000D970
		private void nudHeldItemMail_ValueChanged(object sender, EventArgs e)
		{
			DialogResult dialogResult = this.CheckForUnsavedChanges();
			bool flag = dialogResult == DialogResult.Cancel;
			if (flag)
			{
				this.nudHeldItemMail.ValueChanged -= this.nudHeldItemMail_ValueChanged;
				this.nudHeldItemMail.Value = new decimal(this.currentMailId);
				this.nudHeldItemMail.ValueChanged += this.nudHeldItemMail_ValueChanged;
			}
			else
			{
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveHeldItemMail(this.currentMailId);
					bool flag3 = this.hasUnsavedChanges;
					if (flag3)
					{
						this.nudHeldItemMail.ValueChanged -= this.nudHeldItemMail_ValueChanged;
						this.nudHeldItemMail.Value = new decimal(this.currentMailId);
						this.nudHeldItemMail.ValueChanged += this.nudHeldItemMail_ValueChanged;
						return;
					}
				}
				else
				{
					bool flag4 = dialogResult == DialogResult.No;
					if (flag4)
					{
						this.SetUnsavedChanges(false);
					}
				}
				this.LoadHeldItemMail();
			}
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000F860 File Offset: 0x0000DA60
		private void nudEasyChatCount_ValueChanged(object sender, EventArgs e)
		{
			this.UpdatePreviewEnabledState(Convert.ToInt32(this.nudEasyChatCount.Value));
			this.SetUnsavedChanges(true);
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x0000F884 File Offset: 0x0000DA84
		private void HeldItemMailEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			DialogResult dialogResult = this.CheckForUnsavedChanges();
			bool flag = dialogResult == DialogResult.Cancel;
			if (flag)
			{
				e.Cancel = true;
			}
			else
			{
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveHeldItemMail(Convert.ToInt32(this.nudHeldItemMail.Value));
					bool flag3 = this.hasUnsavedChanges;
					if (flag3)
					{
						e.Cancel = true;
					}
				}
				else
				{
					bool flag4 = dialogResult == DialogResult.No;
					if (flag4)
					{
						this.SetUnsavedChanges(false);
					}
				}
			}
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000F8F8 File Offset: 0x0000DAF8
		private void LoadEasyChatGroupTable()
		{
			checked
			{
				this.easyChatGroupOffsets = new int[this.EASYCHAT_GROUP_COUNT - 1 + 1];
				this.easyChatGroupCounts = new int[this.EASYCHAT_GROUP_COUNT - 1 + 1];
				int num = this.EASYCHAT_GROUP_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.EASYCHAT_GROUP_TABLE_OFFSET + i * this.EASYCHAT_GROUP_ENTRY_LENGTH;
					int num3 = BitConverter.ToInt32(this.romData, num2);
					this.easyChatGroupOffsets[i] = (int)(unchecked((long)num3) - 134217728L);
					this.easyChatGroupCounts[i] = (int)this.romData[num2 + 4];
				}
			}
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000F986 File Offset: 0x0000DB86
		private void LoadPokemonNames()
		{
			this.LoadTwoByteIndexComboBox(this.cmbEasyChatListPokemon1, 21, (int idx) => this.GetPokemonName(MyProject.Forms.PokemonEditor.POKEMON_NAME_OFFSET, idx));
			this.LoadTwoByteIndexComboBox(this.cmbEasyChatListPokemon2, 0, (int idx) => this.GetPokemonName(MyProject.Forms.PokemonEditor.POKEMON_NAME_OFFSET, idx));
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000F9C0 File Offset: 0x0000DBC0
		private void LoadTwoByteIndexComboBox(ComboBox comboBox, int groupIndex, Func<int, string> nameResolver)
		{
			int num = this.easyChatGroupOffsets[groupIndex];
			int num2 = this.easyChatGroupCounts[groupIndex];
			comboBox.BeginUpdate();
			comboBox.Items.Clear();
			checked
			{
				int num3 = num2 - 1;
				for (int i = 0; i <= num3; i++)
				{
					int num4 = (int)BitConverter.ToInt16(this.romData, num + i * 2);
					string text = nameResolver(num4);
					HeldItemMailEditor.EasyChatItem easyChatItem = new HeldItemMailEditor.EasyChatItem
					{
						Text = text,
						Index = num4
					};
					comboBox.Items.Add(easyChatItem);
				}
				comboBox.EndUpdate();
				comboBox.SelectedIndex = 0;
			}
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000FA58 File Offset: 0x0000DC58
		private string GetPokemonName(int nameOffset, int index)
		{
			checked
			{
				byte[] array = new byte[MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH - 1 + 1];
				int num = nameOffset + index * MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH;
				Array.Copy(this.romData, num, array, 0, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH);
			}
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000FACB File Offset: 0x0000DCCB
		private void LoadMoveNames()
		{
			this.LoadTwoByteIndexComboBox(this.cmbEasyChatListMove1, 18, (int idx) => this.GetMoveName(idx));
			this.LoadTwoByteIndexComboBox(this.cmbEasyChatListMove2, 19, (int idx) => this.GetMoveName(idx));
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000FB04 File Offset: 0x0000DD04
		private string GetMoveName(int index)
		{
			checked
			{
				byte[] array = new byte[MoveData.MOVE_NAME_LENGTH - 1 + 1];
				int num = MoveData.MOVE_NAME_TABLE_OFFSET + index * MoveData.MOVE_NAME_LENGTH;
				Array.Copy(this.romData, num, array, 0, MoveData.MOVE_NAME_LENGTH);
				return TextConverter.BytesToPokemonString(array, 0, MoveData.MOVE_NAME_LENGTH);
			}
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000FB54 File Offset: 0x0000DD54
		private void LoadEasyChatLists()
		{
			this.LoadEasyChatComboBox(this.cmbEasyChatListTrainer, 1);
			this.LoadEasyChatComboBox(this.cmbEasyChatListStatus, 2);
			this.LoadEasyChatComboBox(this.cmbEasyChatListBattle, 3);
			this.LoadEasyChatComboBox(this.cmbEasyChatListGreeting, 4);
			this.LoadEasyChatComboBox(this.cmbEasyChatListPeople, 5);
			this.LoadEasyChatComboBox(this.cmbEasyChatListVoice, 6);
			this.LoadEasyChatComboBox(this.cmbEasyChatListSpeech, 7);
			this.LoadEasyChatComboBox(this.cmbEasyChatListEnding, 8);
			this.LoadEasyChatComboBox(this.cmbEasyChatListFeeling, 9);
			this.LoadEasyChatComboBox(this.cmbEasyChatListCondition, 10);
			this.LoadEasyChatComboBox(this.cmbEasyChatListAction, 11);
			this.LoadEasyChatComboBox(this.cmbEasyChatListLifeStyle, 12);
			this.LoadEasyChatComboBox(this.cmbEasyChatListHobby, 13);
			this.LoadEasyChatComboBox(this.cmbEasyChatListTime, 14);
			this.LoadEasyChatComboBox(this.cmbEasyChatListMisc, 15);
			this.LoadEasyChatComboBox(this.cmbEasyChatListAdjective, 16);
			this.LoadEasyChatComboBox(this.cmbEasyChatListEvent, 17);
			this.LoadEasyChatComboBox(this.cmbEasyChatListTrendySaying, 20);
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x0000FC68 File Offset: 0x0000DE68
		private void LoadEasyChatComboBox(ComboBox comboBox, int groupIndex)
		{
			int num = this.easyChatGroupOffsets[groupIndex];
			int num2 = this.easyChatGroupCounts[groupIndex];
			comboBox.BeginUpdate();
			comboBox.Items.Clear();
			checked
			{
				int num3 = num2 - 1;
				for (int i = 0; i <= num3; i++)
				{
					int num4 = num + i * 12;
					int num5 = BitConverter.ToInt32(this.romData, num4);
					int num6 = (int)(unchecked((long)num5) - 134217728L);
					byte b = this.romData[num4 + 4];
					string text = TextConverter.BytesToPokemonString(this.romData, num6, 12);
					bool flag = Operators.CompareString(comboBox.Name, "cmbEasyChatListTrainer", false) == 0 || Operators.CompareString(comboBox.Name, "cmbEasyChatListEvent", false) == 0;
					if (flag)
					{
						b = (byte)i;
					}
					HeldItemMailEditor.EasyChatItem easyChatItem = new HeldItemMailEditor.EasyChatItem
					{
						Text = text,
						Index = (int)b
					};
					comboBox.Items.Add(easyChatItem);
				}
				comboBox.EndUpdate();
				comboBox.SelectedIndex = 0;
			}
		}

		// Token: 0x060001FA RID: 506 RVA: 0x0000FD68 File Offset: 0x0000DF68
		private void LoadHeldItemMail()
		{
			int num = Convert.ToInt32(this.nudHeldItemMail.Value);
			this.currentMailId = num;
			checked
			{
				int num2 = this.HELD_ITEM_MAIL_OFFSET + num * this.HELD_ITEM_MAIL_LENGTH;
				this.ResetPreviewArea();
				byte[] array = new byte[this.HELD_ITEM_MAIL_LENGTH - 1 + 1];
				Array.Copy(this.romData, num2, array, 0, this.HELD_ITEM_MAIL_LENGTH);
				int num3 = 0;
				int num4 = 0;
				do
				{
					ushort num5 = BitConverter.ToUInt16(array, num4 * 2);
					bool flag = num5 == ushort.MaxValue;
					if (flag)
					{
						break;
					}
					num3 = num4 + 1;
					num4++;
				}
				while (num4 <= 9);
				bool flag2 = num3 == 0;
				if (flag2)
				{
					num3 = 1;
				}
				this.nudEasyChatCount.ValueChanged -= this.nudEasyChatCount_ValueChanged;
				this.nudEasyChatCount.Value = new decimal(num3);
				this.nudEasyChatCount.ValueChanged += this.nudEasyChatCount_ValueChanged;
				this.UpdatePreviewEnabledState(num3);
				int num6 = num3 - 1;
				for (int i = 0; i <= num6; i++)
				{
					ushort num7 = BitConverter.ToUInt16(array, i * 2);
					bool flag3 = num7 != ushort.MaxValue && num7 > 0;
					if (flag3)
					{
						int num8 = (int)(unchecked((ushort)((uint)num7 >> 9)) & 127);
						int num9 = (int)(num7 & 511);
						string easyChatWordText = this.GetEasyChatWordText(num8, num9);
						this.SetPreviewText(i, easyChatWordText);
					}
				}
				this.rbEasyChatPreview1.Checked = true;
				this.SetUnsavedChanges(false);
			}
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000FED0 File Offset: 0x0000E0D0
		private void ResetPreviewArea()
		{
			TextBox[] previewTextBoxes = this.GetPreviewTextBoxes();
			RadioButton[] previewRadioButtons = this.GetPreviewRadioButtons();
			checked
			{
				int num = previewTextBoxes.Length - 1;
				for (int i = 0; i <= num; i++)
				{
					previewTextBoxes[i].Text = "";
					previewTextBoxes[i].Enabled = false;
					previewRadioButtons[i].Enabled = false;
					previewRadioButtons[i].Checked = false;
				}
			}
		}

		// Token: 0x060001FC RID: 508 RVA: 0x0000FF2C File Offset: 0x0000E12C
		private void InitializeEasyChatGroupComboBoxMap()
		{
			this.easyChatGroupToComboBox = new Dictionary<int, ComboBox>
			{
				{ 0, this.cmbEasyChatListPokemon2 },
				{ 1, this.cmbEasyChatListTrainer },
				{ 2, this.cmbEasyChatListStatus },
				{ 3, this.cmbEasyChatListBattle },
				{ 4, this.cmbEasyChatListGreeting },
				{ 5, this.cmbEasyChatListPeople },
				{ 6, this.cmbEasyChatListVoice },
				{ 7, this.cmbEasyChatListSpeech },
				{ 8, this.cmbEasyChatListEnding },
				{ 9, this.cmbEasyChatListFeeling },
				{ 10, this.cmbEasyChatListCondition },
				{ 11, this.cmbEasyChatListAction },
				{ 12, this.cmbEasyChatListLifeStyle },
				{ 13, this.cmbEasyChatListHobby },
				{ 14, this.cmbEasyChatListTime },
				{ 15, this.cmbEasyChatListMisc },
				{ 16, this.cmbEasyChatListAdjective },
				{ 17, this.cmbEasyChatListEvent },
				{ 18, this.cmbEasyChatListMove1 },
				{ 19, this.cmbEasyChatListMove2 },
				{ 20, this.cmbEasyChatListTrendySaying },
				{ 21, this.cmbEasyChatListPokemon1 }
			};
		}

		// Token: 0x060001FD RID: 509 RVA: 0x00010088 File Offset: 0x0000E288
		private void PreviewTextBox_Enter(object sender, EventArgs e)
		{
			string name = ((TextBox)sender).Name;
			string text = name.Replace("txt", "rb");
			RadioButton radioButton = (RadioButton)base.Controls.Find(text, true).FirstOrDefault<Control>();
			bool flag = radioButton != null;
			if (flag)
			{
				radioButton.Checked = true;
			}
		}

		// Token: 0x060001FE RID: 510 RVA: 0x000100E0 File Offset: 0x0000E2E0
		private void ListComboBox_Enter(object sender, EventArgs e)
		{
			string name = ((ComboBox)sender).Name;
			string text = name.Replace("cmb", "rb");
			RadioButton radioButton = (RadioButton)base.Controls.Find(text, true).FirstOrDefault<Control>();
			bool flag = radioButton != null;
			if (flag)
			{
				radioButton.Checked = true;
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x00010138 File Offset: 0x0000E338
		private void UpdatePreviewEnabledState(int count)
		{
			TextBox[] previewTextBoxes = this.GetPreviewTextBoxes();
			RadioButton[] previewRadioButtons = this.GetPreviewRadioButtons();
			checked
			{
				int num = previewTextBoxes.Length - 1;
				for (int i = 0; i <= num; i++)
				{
					bool flag = i < count;
					previewRadioButtons[i].Enabled = flag;
					previewTextBoxes[i].Enabled = flag;
					bool flag2 = !flag;
					if (flag2)
					{
						previewTextBoxes[i].Text = "";
					}
				}
				bool flag3 = !this.rbEasyChatPreview1.Enabled;
				if (!flag3)
				{
					bool flag4 = !previewRadioButtons.Any((RadioButton rb) => rb.Checked && rb.Enabled);
					if (flag4)
					{
						this.rbEasyChatPreview1.Checked = true;
					}
				}
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x000101F8 File Offset: 0x0000E3F8
		private string GetEasyChatWordText(int group, int index)
		{
			ComboBox comboBox = null;
			bool flag = this.easyChatGroupToComboBox != null && this.easyChatGroupToComboBox.TryGetValue(group, out comboBox);
			string text;
			if (flag)
			{
				text = this.FindEasyChatItemText(comboBox, index);
			}
			else
			{
				text = "????";
			}
			return text;
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0001023C File Offset: 0x0000E43C
		private string FindEasyChatItemText(ComboBox comboBox, int index)
		{
			try
			{
				foreach (object obj in comboBox.Items)
				{
					object objectValue = RuntimeHelpers.GetObjectValue(obj);
					HeldItemMailEditor.EasyChatItem easyChatItem = objectValue as HeldItemMailEditor.EasyChatItem;
					bool flag = easyChatItem != null && easyChatItem.Index == index;
					if (flag)
					{
						return easyChatItem.Text;
					}
				}
			}
			finally
			{
			}
			return "????";
		}

		// Token: 0x06000202 RID: 514 RVA: 0x000102C8 File Offset: 0x0000E4C8
		private void SetPreviewText(int position, string text)
		{
			TextBox[] previewTextBoxes = this.GetPreviewTextBoxes();
			RadioButton[] previewRadioButtons = this.GetPreviewRadioButtons();
			previewTextBoxes[position].Text = text;
			previewTextBoxes[position].Enabled = true;
			previewRadioButtons[position].Enabled = true;
		}

		// Token: 0x06000203 RID: 515 RVA: 0x00010304 File Offset: 0x0000E504
		private void btnAssign_Click(object sender, EventArgs e)
		{
			RadioButton radioButton = this.grpEasyChatList.Controls.OfType<RadioButton>().FirstOrDefault((RadioButton rb) => rb.Checked);
			string text = radioButton.Name.Replace("rb", "cmb");
			ComboBox comboBox = (ComboBox)base.Controls.Find(text, true).FirstOrDefault<Control>();
			bool flag = comboBox.SelectedItem is HeldItemMailEditor.EasyChatItem;
			string text2;
			if (flag)
			{
				text2 = ((HeldItemMailEditor.EasyChatItem)comboBox.SelectedItem).Text;
			}
			else
			{
				text2 = comboBox.SelectedItem.ToString();
			}
			RadioButton radioButton2 = this.grpEasyChatPreview.Controls.OfType<RadioButton>().FirstOrDefault((RadioButton rb) => rb.Checked && rb.Enabled);
			string text3 = radioButton2.Name.Replace("rb", "txt");
			TextBox textBox = (TextBox)base.Controls.Find(text3, true).FirstOrDefault<Control>();
			string text4 = textBox.Text;
			bool flag2 = Operators.CompareString(text4, text2, false) != 0;
			if (flag2)
			{
				textBox.Text = text2;
				this.SetUnsavedChanges(true);
			}
		}

		// Token: 0x06000204 RID: 516 RVA: 0x00010454 File Offset: 0x0000E654
		private TextBox[] GetPreviewTextBoxes()
		{
			return new TextBox[] { this.txtEasyChatPreview1, this.txtEasyChatPreview2, this.txtEasyChatPreview3, this.txtEasyChatPreview4, this.txtEasyChatPreview5, this.txtEasyChatPreview6, this.txtEasyChatPreview7, this.txtEasyChatPreview8, this.txtEasyChatPreview9 };
		}

		// Token: 0x06000205 RID: 517 RVA: 0x000104C0 File Offset: 0x0000E6C0
		private RadioButton[] GetPreviewRadioButtons()
		{
			return new RadioButton[] { this.rbEasyChatPreview1, this.rbEasyChatPreview2, this.rbEasyChatPreview3, this.rbEasyChatPreview4, this.rbEasyChatPreview5, this.rbEasyChatPreview6, this.rbEasyChatPreview7, this.rbEasyChatPreview8, this.rbEasyChatPreview9 };
		}

		// Token: 0x040000FA RID: 250
		public readonly int HELD_ITEM_MAIL_OFFSET;

		// Token: 0x040000FB RID: 251
		public readonly int HELD_ITEM_MAIL_COUNT;

		// Token: 0x040000FC RID: 252
		public readonly int HELD_ITEM_MAIL_LENGTH;

		// Token: 0x040000FD RID: 253
		public readonly int EASYCHAT_GROUP_TABLE_OFFSET;

		// Token: 0x040000FE RID: 254
		public readonly int EASYCHAT_GROUP_COUNT;

		// Token: 0x040000FF RID: 255
		public readonly int EASYCHAT_GROUP_ENTRY_LENGTH;

		// Token: 0x04000100 RID: 256
		public const int EASYCHAT_ENTRY_LENGTH = 12;

		// Token: 0x04000101 RID: 257
		private byte[] romData;

		// Token: 0x04000102 RID: 258
		private bool hasUnsavedChanges;

		// Token: 0x04000103 RID: 259
		private int currentMailId;

		// Token: 0x04000104 RID: 260
		private int[] easyChatGroupOffsets;

		// Token: 0x04000105 RID: 261
		private int[] easyChatGroupCounts;

		// Token: 0x04000106 RID: 262
		private Dictionary<int, ComboBox> easyChatGroupToComboBox;

		// Token: 0x04000107 RID: 263
		private const int GROUP_POKEMON2 = 0;

		// Token: 0x04000108 RID: 264
		private const int GROUP_TRAINER = 1;

		// Token: 0x04000109 RID: 265
		private const int GROUP_STATUS = 2;

		// Token: 0x0400010A RID: 266
		private const int GROUP_BATTLE = 3;

		// Token: 0x0400010B RID: 267
		private const int GROUP_GREETING = 4;

		// Token: 0x0400010C RID: 268
		private const int GROUP_PEOPLE = 5;

		// Token: 0x0400010D RID: 269
		private const int GROUP_VOICE = 6;

		// Token: 0x0400010E RID: 270
		private const int GROUP_SPEECH = 7;

		// Token: 0x0400010F RID: 271
		private const int GROUP_ENDING = 8;

		// Token: 0x04000110 RID: 272
		private const int GROUP_FEELING = 9;

		// Token: 0x04000111 RID: 273
		private const int GROUP_CONDITION = 10;

		// Token: 0x04000112 RID: 274
		private const int GROUP_ACTION = 11;

		// Token: 0x04000113 RID: 275
		private const int GROUP_LIFESTYLE = 12;

		// Token: 0x04000114 RID: 276
		private const int GROUP_HOBBY = 13;

		// Token: 0x04000115 RID: 277
		private const int GROUP_TIME = 14;

		// Token: 0x04000116 RID: 278
		private const int GROUP_MISC = 15;

		// Token: 0x04000117 RID: 279
		private const int GROUP_ADJECTIVE = 16;

		// Token: 0x04000118 RID: 280
		private const int GROUP_EVENT = 17;

		// Token: 0x04000119 RID: 281
		private const int GROUP_MOVE1 = 18;

		// Token: 0x0400011A RID: 282
		private const int GROUP_MOVE2 = 19;

		// Token: 0x0400011B RID: 283
		private const int GROUP_TRENDY_SAYING = 20;

		// Token: 0x0400011C RID: 284
		private const int GROUP_POKEMON1 = 21;

		// Token: 0x02000036 RID: 54
		private class EasyChatItem
		{
			// Token: 0x17000595 RID: 1429
			// (get) Token: 0x06000EC9 RID: 3785 RVA: 0x0006AC10 File Offset: 0x00068E10
			// (set) Token: 0x06000ECA RID: 3786 RVA: 0x0006AC1A File Offset: 0x00068E1A
			public string Text { get; set; }

			// Token: 0x17000596 RID: 1430
			// (get) Token: 0x06000ECB RID: 3787 RVA: 0x0006AC23 File Offset: 0x00068E23
			// (set) Token: 0x06000ECC RID: 3788 RVA: 0x0006AC2D File Offset: 0x00068E2D
			public int Index { get; set; }

			// Token: 0x06000ECD RID: 3789 RVA: 0x0006AC38 File Offset: 0x00068E38
			public override string ToString()
			{
				return this.Text;
			}
		}
	}
}
