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
	// Token: 0x02000012 RID: 18
	public partial class InGameTradeEditor : Form
	{
		// Token: 0x06000216 RID: 534 RVA: 0x00011444 File Offset: 0x0000F644
		public InGameTradeEditor()
		{
			base.Load += this.InGameTradeEditor_Load;
			base.FormClosing += this.InGameTradeEditor_FormClosing;
			this.IN_GAME_TRADE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("IN_GAME_TRADE_TABLE_OFFSET");
			this.IN_GAME_TRADE_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("IN_GAME_TRADE_ENTRY_LENGTH");
			this.IN_GAME_TRADE_TOTAL_COUNT = RomIniReader.ReadHexOrDecimal("IN_GAME_TRADE_TOTAL_COUNT");
			this.IN_GAME_TRADE_NICKNAME_LENGTH = RomIniReader.ReadHexOrDecimal("IN_GAME_TRADE_NICKNAME_LENGTH");
			this.PERSONALITY_TEXT_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("PERSONALITY_TEXT_TABLE_OFFSET");
			this.PERSONALITY_TEXT_COUNT = RomIniReader.ReadHexOrDecimal("PERSONALITY_TEXT_COUNT");
			this.IN_GAME_TRADE_TRAINER_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("IN_GAME_TRADE_TRAINER_NAME_LENGTH");
			this.hasUnsavedChanges = false;
			this.pokemonIconList = new Dictionary<int, PokemonData>();
			this.currentTradeData = new InGameTradeEditor.TradeData();
			this.InitializeComponent();
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x06000219 RID: 537 RVA: 0x0001433D File Offset: 0x0001253D
		// (set) Token: 0x0600021A RID: 538 RVA: 0x00014348 File Offset: 0x00012548
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

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600021B RID: 539 RVA: 0x0001438B File Offset: 0x0001258B
		// (set) Token: 0x0600021C RID: 540 RVA: 0x00014398 File Offset: 0x00012598
		internal virtual ListBox lstInGameTradeList
		{
			[CompilerGenerated]
			get
			{
				return this._lstInGameTradeList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstInGameTradeList_SelectedIndexChanged);
				ListBox listBox = this._lstInGameTradeList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstInGameTradeList = value;
				listBox = this._lstInGameTradeList;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x0600021D RID: 541 RVA: 0x000143DB File Offset: 0x000125DB
		// (set) Token: 0x0600021E RID: 542 RVA: 0x000143E8 File Offset: 0x000125E8
		internal virtual ComboBox cmbPokemonToGive
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonToGive;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPokemonToGive_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPokemonToGive;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonToGive = value;
				comboBox = this._cmbPokemonToGive;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x0600021F RID: 543 RVA: 0x0001442B File Offset: 0x0001262B
		// (set) Token: 0x06000220 RID: 544 RVA: 0x00014435 File Offset: 0x00012635
		internal virtual GroupBox grpPokemonToGive
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000221 RID: 545 RVA: 0x0001443E File Offset: 0x0001263E
		// (set) Token: 0x06000222 RID: 546 RVA: 0x00014448 File Offset: 0x00012648
		internal virtual PictureBox picPokemonToGive
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000223 RID: 547 RVA: 0x00014451 File Offset: 0x00012651
		// (set) Token: 0x06000224 RID: 548 RVA: 0x0001445B File Offset: 0x0001265B
		internal virtual GroupBox grpPokemonToReceive
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000225 RID: 549 RVA: 0x00014464 File Offset: 0x00012664
		// (set) Token: 0x06000226 RID: 550 RVA: 0x0001446E File Offset: 0x0001266E
		internal virtual PictureBox picPokemonToReceive
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000227 RID: 551 RVA: 0x00014477 File Offset: 0x00012677
		// (set) Token: 0x06000228 RID: 552 RVA: 0x00014484 File Offset: 0x00012684
		internal virtual ComboBox cmbPokemonToReceive
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonToReceive;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPokemonToReceive_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPokemonToReceive;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonToReceive = value;
				comboBox = this._cmbPokemonToReceive;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000229 RID: 553 RVA: 0x000144C7 File Offset: 0x000126C7
		// (set) Token: 0x0600022A RID: 554 RVA: 0x000144D1 File Offset: 0x000126D1
		internal virtual GroupBox grpPokemonToReceiveDetail
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x0600022B RID: 555 RVA: 0x000144DA File Offset: 0x000126DA
		// (set) Token: 0x0600022C RID: 556 RVA: 0x000144E4 File Offset: 0x000126E4
		internal virtual GroupBox grpPersonalityValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x0600022D RID: 557 RVA: 0x000144ED File Offset: 0x000126ED
		// (set) Token: 0x0600022E RID: 558 RVA: 0x000144F7 File Offset: 0x000126F7
		internal virtual TextBox txtPersonalityValueDecimal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x0600022F RID: 559 RVA: 0x00014500 File Offset: 0x00012700
		// (set) Token: 0x06000230 RID: 560 RVA: 0x0001450A File Offset: 0x0001270A
		internal virtual Label lblPersonalityValueHexa
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000231 RID: 561 RVA: 0x00014513 File Offset: 0x00012713
		// (set) Token: 0x06000232 RID: 562 RVA: 0x0001451D File Offset: 0x0001271D
		internal virtual TextBox txtPersonalityValueHexa
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x06000233 RID: 563 RVA: 0x00014526 File Offset: 0x00012726
		// (set) Token: 0x06000234 RID: 564 RVA: 0x00014530 File Offset: 0x00012730
		internal virtual Label lblPersonalityValueDecimal
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000235 RID: 565 RVA: 0x00014539 File Offset: 0x00012739
		// (set) Token: 0x06000236 RID: 566 RVA: 0x00014543 File Offset: 0x00012743
		internal virtual TextBox txtNickNameReadOnly
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000237 RID: 567 RVA: 0x0001454C File Offset: 0x0001274C
		// (set) Token: 0x06000238 RID: 568 RVA: 0x00014558 File Offset: 0x00012758
		internal virtual Button btnChangeNickName
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeNickName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeNickName_Click);
				Button button = this._btnChangeNickName;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeNickName = value;
				button = this._btnChangeNickName;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000239 RID: 569 RVA: 0x0001459B File Offset: 0x0001279B
		// (set) Token: 0x0600023A RID: 570 RVA: 0x000145A5 File Offset: 0x000127A5
		internal virtual TextBox txtNickNameInput
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x0600023B RID: 571 RVA: 0x000145AE File Offset: 0x000127AE
		// (set) Token: 0x0600023C RID: 572 RVA: 0x000145B8 File Offset: 0x000127B8
		internal virtual Label lblNickName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x0600023D RID: 573 RVA: 0x000145C1 File Offset: 0x000127C1
		// (set) Token: 0x0600023E RID: 574 RVA: 0x000145CB File Offset: 0x000127CB
		internal virtual Label lblHeldItem
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x0600023F RID: 575 RVA: 0x000145D4 File Offset: 0x000127D4
		// (set) Token: 0x06000240 RID: 576 RVA: 0x000145E0 File Offset: 0x000127E0
		internal virtual ComboBox cmbHeldItem
		{
			[CompilerGenerated]
			get
			{
				return this._cmbHeldItem;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbHeldItem_SelectedIndexChanged);
				ComboBox comboBox = this._cmbHeldItem;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbHeldItem = value;
				comboBox = this._cmbHeldItem;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000241 RID: 577 RVA: 0x00014623 File Offset: 0x00012823
		// (set) Token: 0x06000242 RID: 578 RVA: 0x0001462D File Offset: 0x0001282D
		internal virtual PictureBox picHeldItem
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000243 RID: 579 RVA: 0x00014636 File Offset: 0x00012836
		// (set) Token: 0x06000244 RID: 580 RVA: 0x00014640 File Offset: 0x00012840
		internal virtual GroupBox grpPersonalityValueCalc
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000245 RID: 581 RVA: 0x00014649 File Offset: 0x00012849
		// (set) Token: 0x06000246 RID: 582 RVA: 0x00014653 File Offset: 0x00012853
		internal virtual RadioButton rbPersonalityValueGenderFemale
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000247 RID: 583 RVA: 0x0001465C File Offset: 0x0001285C
		// (set) Token: 0x06000248 RID: 584 RVA: 0x00014666 File Offset: 0x00012866
		internal virtual RadioButton rbPersonalityValueGenderMale
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000249 RID: 585 RVA: 0x0001466F File Offset: 0x0001286F
		// (set) Token: 0x0600024A RID: 586 RVA: 0x00014679 File Offset: 0x00012879
		internal virtual ComboBox cmbPersonalityValueAbility
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600024B RID: 587 RVA: 0x00014682 File Offset: 0x00012882
		// (set) Token: 0x0600024C RID: 588 RVA: 0x0001468C File Offset: 0x0001288C
		internal virtual ComboBox cmbPersonalityValuePersonality
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x0600024D RID: 589 RVA: 0x00014695 File Offset: 0x00012895
		// (set) Token: 0x0600024E RID: 590 RVA: 0x0001469F File Offset: 0x0001289F
		internal virtual Label lblPersonalityValuePersonality
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x0600024F RID: 591 RVA: 0x000146A8 File Offset: 0x000128A8
		// (set) Token: 0x06000250 RID: 592 RVA: 0x000146B2 File Offset: 0x000128B2
		internal virtual Label lblPersonalityValueGender
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000251 RID: 593 RVA: 0x000146BB File Offset: 0x000128BB
		// (set) Token: 0x06000252 RID: 594 RVA: 0x000146C5 File Offset: 0x000128C5
		internal virtual Label lblPersonalityValueAbility
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000253 RID: 595 RVA: 0x000146CE File Offset: 0x000128CE
		// (set) Token: 0x06000254 RID: 596 RVA: 0x000146D8 File Offset: 0x000128D8
		internal virtual Button btnChangePersonalityValue
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePersonalityValue;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePersonalityValue_Click);
				Button button = this._btnChangePersonalityValue;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePersonalityValue = value;
				button = this._btnChangePersonalityValue;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000255 RID: 597 RVA: 0x0001471B File Offset: 0x0001291B
		// (set) Token: 0x06000256 RID: 598 RVA: 0x00014725 File Offset: 0x00012925
		internal virtual GroupBox grpIndividualValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000257 RID: 599 RVA: 0x0001472E File Offset: 0x0001292E
		// (set) Token: 0x06000258 RID: 600 RVA: 0x00014738 File Offset: 0x00012938
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
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
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

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000259 RID: 601 RVA: 0x0001477B File Offset: 0x0001297B
		// (set) Token: 0x0600025A RID: 602 RVA: 0x00014785 File Offset: 0x00012985
		internal virtual Label lblHeldItemMail
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x0600025B RID: 603 RVA: 0x0001478E File Offset: 0x0001298E
		// (set) Token: 0x0600025C RID: 604 RVA: 0x00014798 File Offset: 0x00012998
		internal virtual NumericUpDown nudIndividualValueHp
		{
			[CompilerGenerated]
			get
			{
				return this._nudIndividualValueHp;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudIndividualValueHp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIndividualValueHp = value;
				numericUpDown = this._nudIndividualValueHp;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x0600025D RID: 605 RVA: 0x000147DB File Offset: 0x000129DB
		// (set) Token: 0x0600025E RID: 606 RVA: 0x000147E8 File Offset: 0x000129E8
		internal virtual NumericUpDown nudIndividualValueSpeed
		{
			[CompilerGenerated]
			get
			{
				return this._nudIndividualValueSpeed;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudIndividualValueSpeed;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIndividualValueSpeed = value;
				numericUpDown = this._nudIndividualValueSpeed;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x0600025F RID: 607 RVA: 0x0001482B File Offset: 0x00012A2B
		// (set) Token: 0x06000260 RID: 608 RVA: 0x00014838 File Offset: 0x00012A38
		internal virtual NumericUpDown nudIndividualValueSpDefense
		{
			[CompilerGenerated]
			get
			{
				return this._nudIndividualValueSpDefense;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudIndividualValueSpDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIndividualValueSpDefense = value;
				numericUpDown = this._nudIndividualValueSpDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000261 RID: 609 RVA: 0x0001487B File Offset: 0x00012A7B
		// (set) Token: 0x06000262 RID: 610 RVA: 0x00014888 File Offset: 0x00012A88
		internal virtual NumericUpDown nudIndividualValueSpAttack
		{
			[CompilerGenerated]
			get
			{
				return this._nudIndividualValueSpAttack;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudIndividualValueSpAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIndividualValueSpAttack = value;
				numericUpDown = this._nudIndividualValueSpAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000263 RID: 611 RVA: 0x000148CB File Offset: 0x00012ACB
		// (set) Token: 0x06000264 RID: 612 RVA: 0x000148D8 File Offset: 0x00012AD8
		internal virtual NumericUpDown nudIndividualValueDefense
		{
			[CompilerGenerated]
			get
			{
				return this._nudIndividualValueDefense;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudIndividualValueDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIndividualValueDefense = value;
				numericUpDown = this._nudIndividualValueDefense;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000265 RID: 613 RVA: 0x0001491B File Offset: 0x00012B1B
		// (set) Token: 0x06000266 RID: 614 RVA: 0x00014928 File Offset: 0x00012B28
		internal virtual NumericUpDown nudIndividualValueAttack
		{
			[CompilerGenerated]
			get
			{
				return this._nudIndividualValueAttack;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudIndividualValueAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudIndividualValueAttack = value;
				numericUpDown = this._nudIndividualValueAttack;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000267 RID: 615 RVA: 0x0001496B File Offset: 0x00012B6B
		// (set) Token: 0x06000268 RID: 616 RVA: 0x00014975 File Offset: 0x00012B75
		internal virtual Label lblIndividualValueSpeed
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000269 RID: 617 RVA: 0x0001497E File Offset: 0x00012B7E
		// (set) Token: 0x0600026A RID: 618 RVA: 0x00014988 File Offset: 0x00012B88
		internal virtual Label lblIndividualValueSpDefense
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x0600026B RID: 619 RVA: 0x00014991 File Offset: 0x00012B91
		// (set) Token: 0x0600026C RID: 620 RVA: 0x0001499B File Offset: 0x00012B9B
		internal virtual Label lblIndividualValueSpAttack
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x0600026D RID: 621 RVA: 0x000149A4 File Offset: 0x00012BA4
		// (set) Token: 0x0600026E RID: 622 RVA: 0x000149AE File Offset: 0x00012BAE
		internal virtual Label lblIndividualValueDefense
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x0600026F RID: 623 RVA: 0x000149B7 File Offset: 0x00012BB7
		// (set) Token: 0x06000270 RID: 624 RVA: 0x000149C1 File Offset: 0x00012BC1
		internal virtual Label lblIndividualValueAttack
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000271 RID: 625 RVA: 0x000149CA File Offset: 0x00012BCA
		// (set) Token: 0x06000272 RID: 626 RVA: 0x000149D4 File Offset: 0x00012BD4
		internal virtual Label lblIndividualValueHp
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000273 RID: 627 RVA: 0x000149DD File Offset: 0x00012BDD
		// (set) Token: 0x06000274 RID: 628 RVA: 0x000149E7 File Offset: 0x00012BE7
		internal virtual GroupBox grpTrainerInfomation
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000275 RID: 629 RVA: 0x000149F0 File Offset: 0x00012BF0
		// (set) Token: 0x06000276 RID: 630 RVA: 0x000149FA File Offset: 0x00012BFA
		internal virtual GroupBox grpConditionsSheen
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000277 RID: 631 RVA: 0x00014A03 File Offset: 0x00012C03
		// (set) Token: 0x06000278 RID: 632 RVA: 0x00014A10 File Offset: 0x00012C10
		internal virtual NumericUpDown nudConditionsCool
		{
			[CompilerGenerated]
			get
			{
				return this._nudConditionsCool;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudConditionsCool;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudConditionsCool = value;
				numericUpDown = this._nudConditionsCool;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000279 RID: 633 RVA: 0x00014A53 File Offset: 0x00012C53
		// (set) Token: 0x0600027A RID: 634 RVA: 0x00014A5D File Offset: 0x00012C5D
		internal virtual Label lblConditionsCool
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x0600027B RID: 635 RVA: 0x00014A66 File Offset: 0x00012C66
		// (set) Token: 0x0600027C RID: 636 RVA: 0x00014A70 File Offset: 0x00012C70
		internal virtual NumericUpDown nudSheen
		{
			[CompilerGenerated]
			get
			{
				return this._nudSheen;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudSheen;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudSheen = value;
				numericUpDown = this._nudSheen;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x0600027D RID: 637 RVA: 0x00014AB3 File Offset: 0x00012CB3
		// (set) Token: 0x0600027E RID: 638 RVA: 0x00014ABD File Offset: 0x00012CBD
		internal virtual Label lblConditionsBeauty
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x0600027F RID: 639 RVA: 0x00014AC6 File Offset: 0x00012CC6
		// (set) Token: 0x06000280 RID: 640 RVA: 0x00014AD0 File Offset: 0x00012CD0
		internal virtual NumericUpDown nudConditionsTough
		{
			[CompilerGenerated]
			get
			{
				return this._nudConditionsTough;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudConditionsTough;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudConditionsTough = value;
				numericUpDown = this._nudConditionsTough;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000281 RID: 641 RVA: 0x00014B13 File Offset: 0x00012D13
		// (set) Token: 0x06000282 RID: 642 RVA: 0x00014B1D File Offset: 0x00012D1D
		internal virtual Label lblConditionsCute
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000283 RID: 643 RVA: 0x00014B26 File Offset: 0x00012D26
		// (set) Token: 0x06000284 RID: 644 RVA: 0x00014B30 File Offset: 0x00012D30
		internal virtual NumericUpDown nudConditionsSmart
		{
			[CompilerGenerated]
			get
			{
				return this._nudConditionsSmart;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudConditionsSmart;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudConditionsSmart = value;
				numericUpDown = this._nudConditionsSmart;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000285 RID: 645 RVA: 0x00014B73 File Offset: 0x00012D73
		// (set) Token: 0x06000286 RID: 646 RVA: 0x00014B7D File Offset: 0x00012D7D
		internal virtual Label lblConditionsSmart
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x06000287 RID: 647 RVA: 0x00014B86 File Offset: 0x00012D86
		// (set) Token: 0x06000288 RID: 648 RVA: 0x00014B90 File Offset: 0x00012D90
		internal virtual NumericUpDown nudConditionsCute
		{
			[CompilerGenerated]
			get
			{
				return this._nudConditionsCute;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudConditionsCute;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudConditionsCute = value;
				numericUpDown = this._nudConditionsCute;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x06000289 RID: 649 RVA: 0x00014BD3 File Offset: 0x00012DD3
		// (set) Token: 0x0600028A RID: 650 RVA: 0x00014BDD File Offset: 0x00012DDD
		internal virtual Label lblConditionsTough
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x0600028B RID: 651 RVA: 0x00014BE6 File Offset: 0x00012DE6
		// (set) Token: 0x0600028C RID: 652 RVA: 0x00014BF0 File Offset: 0x00012DF0
		internal virtual NumericUpDown nudConditionsBeauty
		{
			[CompilerGenerated]
			get
			{
				return this._nudConditionsBeauty;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_MailId_IVs_Conditions_Sheen_ValueChanged);
				NumericUpDown numericUpDown = this._nudConditionsBeauty;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudConditionsBeauty = value;
				numericUpDown = this._nudConditionsBeauty;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x0600028D RID: 653 RVA: 0x00014C33 File Offset: 0x00012E33
		// (set) Token: 0x0600028E RID: 654 RVA: 0x00014C3D File Offset: 0x00012E3D
		internal virtual Label lblSheen
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x0600028F RID: 655 RVA: 0x00014C46 File Offset: 0x00012E46
		// (set) Token: 0x06000290 RID: 656 RVA: 0x00014C50 File Offset: 0x00012E50
		internal virtual Label lblTrainerName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000291 RID: 657 RVA: 0x00014C59 File Offset: 0x00012E59
		// (set) Token: 0x06000292 RID: 658 RVA: 0x00014C64 File Offset: 0x00012E64
		internal virtual NumericUpDown nudTrainerId2
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerId2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudTrainerId_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerId2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerId2 = value;
				numericUpDown = this._nudTrainerId2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000293 RID: 659 RVA: 0x00014CA7 File Offset: 0x00012EA7
		// (set) Token: 0x06000294 RID: 660 RVA: 0x00014CB4 File Offset: 0x00012EB4
		internal virtual NumericUpDown nudTrainerId1
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerId1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudTrainerId_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerId1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerId1 = value;
				numericUpDown = this._nudTrainerId1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000295 RID: 661 RVA: 0x00014CF7 File Offset: 0x00012EF7
		// (set) Token: 0x06000296 RID: 662 RVA: 0x00014D01 File Offset: 0x00012F01
		internal virtual TextBox txtTrainerNameReadOnly
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x06000297 RID: 663 RVA: 0x00014D0A File Offset: 0x00012F0A
		// (set) Token: 0x06000298 RID: 664 RVA: 0x00014D14 File Offset: 0x00012F14
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

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x06000299 RID: 665 RVA: 0x00014D57 File Offset: 0x00012F57
		// (set) Token: 0x0600029A RID: 666 RVA: 0x00014D61 File Offset: 0x00012F61
		internal virtual TextBox txtTrainerNameInput
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x0600029B RID: 667 RVA: 0x00014D6A File Offset: 0x00012F6A
		// (set) Token: 0x0600029C RID: 668 RVA: 0x00014D74 File Offset: 0x00012F74
		internal virtual Label lblTrainerGender
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x0600029D RID: 669 RVA: 0x00014D7D File Offset: 0x00012F7D
		// (set) Token: 0x0600029E RID: 670 RVA: 0x00014D87 File Offset: 0x00012F87
		internal virtual Label lblTrainerId2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x0600029F RID: 671 RVA: 0x00014D90 File Offset: 0x00012F90
		// (set) Token: 0x060002A0 RID: 672 RVA: 0x00014D9A File Offset: 0x00012F9A
		internal virtual Label lblTrainerId1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060002A1 RID: 673 RVA: 0x00014DA3 File Offset: 0x00012FA3
		// (set) Token: 0x060002A2 RID: 674 RVA: 0x00014DB0 File Offset: 0x00012FB0
		internal virtual RadioButton rbTrainerGenderFemale
		{
			[CompilerGenerated]
			get
			{
				return this._rbTrainerGenderFemale;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbTrainerGender_CheckedChanged);
				RadioButton radioButton = this._rbTrainerGenderFemale;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbTrainerGenderFemale = value;
				radioButton = this._rbTrainerGenderFemale;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x00014DF3 File Offset: 0x00012FF3
		// (set) Token: 0x060002A4 RID: 676 RVA: 0x00014E00 File Offset: 0x00013000
		internal virtual RadioButton rbTrainerGenderMale
		{
			[CompilerGenerated]
			get
			{
				return this._rbTrainerGenderMale;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.rbTrainerGender_CheckedChanged);
				RadioButton radioButton = this._rbTrainerGenderMale;
				if (radioButton != null)
				{
					radioButton.CheckedChanged -= eventHandler;
				}
				this._rbTrainerGenderMale = value;
				radioButton = this._rbTrainerGenderMale;
				if (radioButton != null)
				{
					radioButton.CheckedChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060002A5 RID: 677 RVA: 0x00014E43 File Offset: 0x00013043
		// (set) Token: 0x060002A6 RID: 678 RVA: 0x00014E4D File Offset: 0x0001304D
		internal virtual GroupBox grpUnknownValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060002A7 RID: 679 RVA: 0x00014E56 File Offset: 0x00013056
		// (set) Token: 0x060002A8 RID: 680 RVA: 0x00014E60 File Offset: 0x00013060
		internal virtual NumericUpDown nudUnknownValue2
		{
			[CompilerGenerated]
			get
			{
				return this._nudUnknownValue2;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudUnknownValue_ValueChanged);
				NumericUpDown numericUpDown = this._nudUnknownValue2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudUnknownValue2 = value;
				numericUpDown = this._nudUnknownValue2;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060002A9 RID: 681 RVA: 0x00014EA3 File Offset: 0x000130A3
		// (set) Token: 0x060002AA RID: 682 RVA: 0x00014EB0 File Offset: 0x000130B0
		internal virtual NumericUpDown nudUnknownValue1
		{
			[CompilerGenerated]
			get
			{
				return this._nudUnknownValue1;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudUnknownValue_ValueChanged);
				NumericUpDown numericUpDown = this._nudUnknownValue1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudUnknownValue1 = value;
				numericUpDown = this._nudUnknownValue1;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060002AB RID: 683 RVA: 0x00014EF3 File Offset: 0x000130F3
		// (set) Token: 0x060002AC RID: 684 RVA: 0x00014EFD File Offset: 0x000130FD
		internal virtual Label lblUnknownValue2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060002AD RID: 685 RVA: 0x00014F06 File Offset: 0x00013106
		// (set) Token: 0x060002AE RID: 686 RVA: 0x00014F10 File Offset: 0x00013110
		internal virtual Label lblUnknownValue1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060002AF RID: 687 RVA: 0x00014F19 File Offset: 0x00013119
		// (set) Token: 0x060002B0 RID: 688 RVA: 0x00014F23 File Offset: 0x00013123
		internal virtual RadioButton rbPersonalityValueGenderUnknown
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x00014F2C File Offset: 0x0001312C
		private void InGameTradeEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.LoadAllPokemonIconData();
			this.InitializePokemonLists();
			this.InitializeItemList();
			this.InitializePersonalityList();
			this.InitializeAbilityList();
			this.InitializeTradeList();
			this.lstInGameTradeList.SelectedIndex = 0;
			this.UpdateDisplay();
			this.ResetChangeFlag();
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x00014F8A File Offset: 0x0001318A
		private void UpdateDisplay()
		{
			this.UpdateTradeData();
			this.UpdatePokemonIcons();
			this.UpdateHeldItemIcon();
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x00014FA4 File Offset: 0x000131A4
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

		// Token: 0x060002B4 RID: 692 RVA: 0x00015028 File Offset: 0x00013228
		private void LoadAllPokemonIconData()
		{
			this.pokemonIconList.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					PokemonData pokemonData = new PokemonData(i, this.GetPokemonNameFromRom(i));
					int num2 = MyProject.Forms.PokemonEditor.ICON_IMAGE_TABLE_OFFSET + i * 4;
					pokemonData.IconImageAddress = BitConverter.ToUInt32(this.romData, num2) - 134217728U;
					int num3 = MyProject.Forms.PokemonEditor.ICON_PALETTE_ID_TABLE_OFFSET + i;
					pokemonData.IconPaletteId = Math.Max(0, Math.Min((int)this.romData[num3], MyProject.Forms.PokemonEditor.ICON_PALETTE_COUNT - 1));
					this.pokemonIconList.Add(i, pokemonData);
				}
			}
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x000150F0 File Offset: 0x000132F0
		private void InitializePokemonLists()
		{
			this.cmbPokemonToGive.BeginUpdate();
			this.cmbPokemonToReceive.BeginUpdate();
			this.cmbPokemonToGive.Items.Clear();
			this.cmbPokemonToReceive.Items.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					string pokemonNameFromRom = this.GetPokemonNameFromRom(i);
					this.cmbPokemonToGive.Items.Add(pokemonNameFromRom);
					this.cmbPokemonToReceive.Items.Add(pokemonNameFromRom);
				}
				this.cmbPokemonToGive.EndUpdate();
				this.cmbPokemonToReceive.EndUpdate();
			}
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0001519C File Offset: 0x0001339C
		private void InitializeItemList()
		{
			this.cmbHeldItem.BeginUpdate();
			this.cmbHeldItem.Items.Clear();
			List<string> itemNames = ItemData.GetItemNames(this.romData);
			{
				foreach (string text in itemNames)
				{
					this.cmbHeldItem.Items.Add(text);
				}
			}
			this.cmbHeldItem.EndUpdate();
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x00015230 File Offset: 0x00013430
		private void InitializePersonalityList()
		{
			this.cmbPersonalityValuePersonality.BeginUpdate();
			this.cmbPersonalityValuePersonality.Items.Clear();
			List<string> list = this.LoadPersonalityTexts();
			{
				foreach (string text in list)
				{
					this.cmbPersonalityValuePersonality.Items.Add(text);
				}
			}
			this.cmbPersonalityValuePersonality.EndUpdate();
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x000152C0 File Offset: 0x000134C0
		private void InitializeTradeList()
		{
			this.lstInGameTradeList.BeginUpdate();
			this.lstInGameTradeList.Items.Clear();
			checked
			{
				int num = this.IN_GAME_TRADE_TOTAL_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					this.lstInGameTradeList.Items.Add(string.Format("交換データ{0:X}", i));
				}
				this.lstInGameTradeList.EndUpdate();
			}
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x00015330 File Offset: 0x00013530
		private void DisplayPokemonIcon(PictureBox pictureBox, PokemonData pokemonData)
		{
			int num;
			byte[] array;
			checked
			{
				num = (int)Math.Min(2048L, unchecked((long)this.romData.Length) - (long)(unchecked((ulong)pokemonData.IconImageAddress)));
				array = new byte[num - 1 + 1];
			}
			Array.Copy(this.romData, (long)((ulong)pokemonData.IconImageAddress), array, 0L, (long)num);
			byte[] array2 = this.LoadIconPalette(pokemonData.IconPaletteId);
			Color[] array3 = ImageProcessor.LoadPalette(array2, true);
			Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, 32, 64, false);
			bool flag = pictureBox.Image != null;
			if (flag)
			{
				pictureBox.Image.Dispose();
			}
			pictureBox.Image = bitmap;
		}

		// Token: 0x060002BA RID: 698 RVA: 0x000153C8 File Offset: 0x000135C8
		private byte[] LoadIconPalette(int paletteId)
		{
			uint num2;
			byte[] array;
			checked
			{
				int num = MyProject.Forms.PokemonEditor.ICON_PALETTE_TABLE_OFFSET + paletteId * 8;
				num2 = BitConverter.ToUInt32(this.romData, num) - 134217728U;
				array = new byte[32];
			}
			Array.Copy(this.romData, (long)((ulong)num2), array, 0L, 32L);
			return array;
		}

		// Token: 0x060002BB RID: 699 RVA: 0x00015420 File Offset: 0x00013620
		private void UpdatePokemonIcons()
		{
			bool flag = this.cmbPokemonToGive.SelectedIndex >= 0;
			checked
			{
				if (flag)
				{
					int num = this.cmbPokemonToGive.SelectedIndex + 1;
					this.DisplayPokemonIcon(this.picPokemonToGive, this.pokemonIconList[num]);
				}
				bool flag2 = this.cmbPokemonToReceive.SelectedIndex >= 0;
				if (flag2)
				{
					int num2 = this.cmbPokemonToReceive.SelectedIndex + 1;
					this.DisplayPokemonIcon(this.picPokemonToReceive, this.pokemonIconList[num2]);
				}
			}
		}

		// Token: 0x060002BC RID: 700 RVA: 0x000154AC File Offset: 0x000136AC
		private void cmbPokemonToGive_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdatePokemonIcons();
			bool flag = this.cmbPokemonToGive.SelectedIndex != (int)this.currentTradeData.GivePokemonIndex;
			if (flag)
			{
				this.SetChangeFlag();
			}
		}

		// Token: 0x060002BD RID: 701 RVA: 0x000154E8 File Offset: 0x000136E8
		private void cmbPokemonToReceive_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdatePokemonIcons();
			bool flag = this.cmbPokemonToReceive.SelectedIndex != (int)this.currentTradeData.ReceivePokemonIndex;
			if (flag)
			{
				this.SetChangeFlag();
			}
		}

		// Token: 0x060002BE RID: 702 RVA: 0x00015524 File Offset: 0x00013724
		private void UpdateHeldItemIcon()
		{
			ushort num = checked((ushort)this.cmbHeldItem.SelectedIndex);
			ItemData.DisplayItemImage(this.picHeldItem, this.romData, num);
		}

		// Token: 0x060002BF RID: 703 RVA: 0x00015554 File Offset: 0x00013754
		private void cmbHeldItem_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdateHeldItemIcon();
			bool flag = this.cmbHeldItem.SelectedIndex != (int)this.currentTradeData.HeldItemId;
			if (flag)
			{
				this.SetChangeFlag();
			}
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00015590 File Offset: 0x00013790
		private void btnChangeNickName_Click(object sender, EventArgs e)
		{
			string text = this.txtNickNameInput.Text;
			bool flag = text.Length > 5;
			if (flag)
			{
				text = text.Substring(0, 5);
				this.txtNickNameInput.Text = text;
			}
			bool flag2 = Operators.CompareString(text, this.currentTradeData.NickName, false) == 0;
			if (!flag2)
			{
				this.currentTradeData.NickName = text;
				this.txtNickNameReadOnly.Text = text;
				this.SetChangeFlag();
			}
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0001560C File Offset: 0x0001380C
		private void nud_MailId_IVs_Conditions_Sheen_ValueChanged(object sender, EventArgs e)
		{
			bool flag = sender == this.nudHeldItemMail;
			if (flag)
			{
				this.currentTradeData.MailId = Convert.ToByte(this.nudHeldItemMail.Value);
			}
			else
			{
				bool flag2 = sender == this.nudIndividualValueHp;
				if (flag2)
				{
					this.currentTradeData.IVs[0] = Convert.ToByte(this.nudIndividualValueHp.Value);
				}
				else
				{
					bool flag3 = sender == this.nudIndividualValueAttack;
					if (flag3)
					{
						this.currentTradeData.IVs[1] = Convert.ToByte(this.nudIndividualValueAttack.Value);
					}
					else
					{
						bool flag4 = sender == this.nudIndividualValueDefense;
						if (flag4)
						{
							this.currentTradeData.IVs[2] = Convert.ToByte(this.nudIndividualValueDefense.Value);
						}
						else
						{
							bool flag5 = sender == this.nudIndividualValueSpeed;
							if (flag5)
							{
								this.currentTradeData.IVs[3] = Convert.ToByte(this.nudIndividualValueSpeed.Value);
							}
							else
							{
								bool flag6 = sender == this.nudIndividualValueSpAttack;
								if (flag6)
								{
									this.currentTradeData.IVs[4] = Convert.ToByte(this.nudIndividualValueSpAttack.Value);
								}
								else
								{
									bool flag7 = sender == this.nudIndividualValueSpDefense;
									if (flag7)
									{
										this.currentTradeData.IVs[5] = Convert.ToByte(this.nudIndividualValueSpDefense.Value);
									}
									else
									{
										bool flag8 = sender == this.nudConditionsCool;
										if (flag8)
										{
											this.currentTradeData.Conditions[0] = Convert.ToByte(this.nudConditionsCool.Value);
										}
										else
										{
											bool flag9 = sender == this.nudConditionsBeauty;
											if (flag9)
											{
												this.currentTradeData.Conditions[1] = Convert.ToByte(this.nudConditionsBeauty.Value);
											}
											else
											{
												bool flag10 = sender == this.nudConditionsCute;
												if (flag10)
												{
													this.currentTradeData.Conditions[2] = Convert.ToByte(this.nudConditionsCute.Value);
												}
												else
												{
													bool flag11 = sender == this.nudConditionsSmart;
													if (flag11)
													{
														this.currentTradeData.Conditions[3] = Convert.ToByte(this.nudConditionsSmart.Value);
													}
													else
													{
														bool flag12 = sender == this.nudConditionsTough;
														if (flag12)
														{
															this.currentTradeData.Conditions[4] = Convert.ToByte(this.nudConditionsTough.Value);
														}
														else
														{
															bool flag13 = sender == this.nudSheen;
															if (flag13)
															{
																this.currentTradeData.Sheen = Convert.ToByte(this.nudSheen.Value);
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
					}
				}
			}
			this.SetChangeFlag();
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x00015898 File Offset: 0x00013A98
		private void btnChangePersonalityValue_Click(object sender, EventArgs e)
		{
			string text = this.txtPersonalityValueDecimal.Text.Trim();
			uint num;
			bool flag = uint.TryParse(text, out num);
			checked
			{
				if (flag)
				{
					bool flag2 = num != this.currentTradeData.PersonalityValue;
					if (flag2)
					{
						this.currentTradeData.PersonalityValue = num;
						this.txtPersonalityValueHexa.Text = num.ToString("X8");
						int num2 = (int)(unchecked((ulong)num % (ulong)((long)this.PERSONALITY_TEXT_COUNT)));
						this.cmbPersonalityValuePersonality.SelectedIndex = num2;
						this.CalculateGender();
						this.CalculateAbility();
						this.SetChangeFlag();
					}
				}
				else
				{
					MessageBox.Show("有効な数値を入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00015948 File Offset: 0x00013B48
		private List<string> LoadPersonalityTexts()
		{
			List<string> list = new List<string>();
			checked
			{
				int num = this.PERSONALITY_TEXT_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.PERSONALITY_TEXT_TABLE_OFFSET + i * 4;
					uint num3 = BitConverter.ToUInt32(this.romData, num2);
					int num4 = (int)(num3 - 134217728U);
					List<byte> list2 = new List<byte>();
					int num5 = 0;
					while (num4 + num5 < this.romData.Length && this.romData[num4 + num5] != 255)
					{
						list2.Add(this.romData[num4 + num5]);
						num5++;
					}
					string text = TextConverter.BytesToPokemonString(list2.ToArray(), 0, list2.Count);
					list.Add(text);
				}
				return list;
			}
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00015A18 File Offset: 0x00013C18
		private void InitializeAbilityList()
		{
			this.cmbPersonalityValueAbility.BeginUpdate();
			this.cmbPersonalityValueAbility.Items.Clear();
			List<string> list = this.LoadAbilityNames();
			{
				foreach (string text in list)
				{
					this.cmbPersonalityValueAbility.Items.Add(text);
				}
			}
			this.cmbPersonalityValueAbility.EndUpdate();
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00015AA8 File Offset: 0x00013CA8
		private List<string> LoadAbilityNames()
		{
			List<string> list = new List<string>();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_ABILITY_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = MyProject.Forms.PokemonEditor.ABILITY_NAME_TABLE_OFFSET + i * MyProject.Forms.PokemonEditor.ABILITY_NAME_LENGTH;
					byte[] array = new byte[MyProject.Forms.PokemonEditor.ABILITY_NAME_LENGTH - 1 + 1];
					Array.Copy(this.romData, num2, array, 0, MyProject.Forms.PokemonEditor.ABILITY_NAME_LENGTH);
					string text = TextConverter.BytesToPokemonString(array, 0, MyProject.Forms.PokemonEditor.ABILITY_NAME_LENGTH);
					list.Add(text);
				}
				return list;
			}
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x00015B64 File Offset: 0x00013D64
		private void CalculateGender()
		{
			checked
			{
				int num = (int)(this.currentTradeData.ReceivePokemonIndex + 1);
				int num2 = MyProject.Forms.PokemonEditor.BASE_STATS_OFFSET + num * MyProject.Forms.PokemonEditor.BASE_STATS_ENTRY_LENGTH;
				byte b = this.romData[num2 + 16];
				if (b != 0)
				{
					if (b != 254)
					{
						if (b != 255)
						{
							byte b2 = (byte)(unchecked((ulong)this.currentTradeData.PersonalityValue) & 255UL);
							bool flag = b2 < b;
							if (flag)
							{
								this.rbPersonalityValueGenderFemale.Checked = true;
							}
							else
							{
								this.rbPersonalityValueGenderMale.Checked = true;
							}
						}
						else
						{
							this.rbPersonalityValueGenderUnknown.Checked = true;
						}
					}
					else
					{
						this.rbPersonalityValueGenderFemale.Checked = true;
					}
				}
				else
				{
					this.rbPersonalityValueGenderMale.Checked = true;
				}
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00015C38 File Offset: 0x00013E38
		private void CalculateAbility()
		{
			int num4;
			int num5;
			byte b;
			bool flag;
			checked
			{
				int num = (int)(this.currentTradeData.ReceivePokemonIndex + 1);
				int num2 = MyProject.Forms.PokemonEditor.BASE_STATS_OFFSET + num * MyProject.Forms.PokemonEditor.BASE_STATS_ENTRY_LENGTH;
				bool enable_BASE_STATS_EXPANSION = MyProject.Forms.PokemonEditor.ENABLE_BASE_STATS_EXPANSION;
				int num3;
				if (enable_BASE_STATS_EXPANSION)
				{
					num3 = (int)BitConverter.ToUInt16(this.romData, num2 + 22);
					num4 = (int)BitConverter.ToUInt16(this.romData, num2 + 26);
				}
				else
				{
					num3 = (int)this.romData[num2 + 22];
					num4 = (int)this.romData[num2 + 23];
				}
				num5 = num3;
				b = 0;
				flag = num4 != 0;
			}
			if (flag)
			{
				bool flag2 = ((ulong)this.currentTradeData.PersonalityValue & 1UL) == 1UL;
				if (flag2)
				{
					num5 = num4;
					b = 1;
				}
			}
			this.currentTradeData.AbilityValue = b;
			this.cmbPersonalityValueAbility.SelectedIndex = num5;
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x00015D18 File Offset: 0x00013F18
		private void GetAbilityFromData()
		{
			checked
			{
				int num = this.IN_GAME_TRADE_TABLE_OFFSET + (int)this.currentTradeData.TradeIndex * this.IN_GAME_TRADE_ENTRY_LENGTH;
				this.currentTradeData.AbilityValue = this.romData[num + 20];
				int num2 = (int)(this.currentTradeData.ReceivePokemonIndex + 1);
				int num3 = MyProject.Forms.PokemonEditor.BASE_STATS_OFFSET + num2 * MyProject.Forms.PokemonEditor.BASE_STATS_ENTRY_LENGTH;
				bool enable_BASE_STATS_EXPANSION = MyProject.Forms.PokemonEditor.ENABLE_BASE_STATS_EXPANSION;
				int num4;
				int num5;
				if (enable_BASE_STATS_EXPANSION)
				{
					num4 = (int)BitConverter.ToUInt16(this.romData, num3 + 22);
					num5 = (int)BitConverter.ToUInt16(this.romData, num3 + 26);
				}
				else
				{
					num4 = (int)this.romData[num3 + 22];
					num5 = (int)this.romData[num3 + 23];
				}
				bool flag = this.currentTradeData.AbilityValue == 0;
				if (flag)
				{
					bool flag2 = num4 < this.cmbPersonalityValueAbility.Items.Count;
					if (flag2)
					{
						this.cmbPersonalityValueAbility.SelectedIndex = num4;
					}
				}
				else
				{
					bool flag3 = num5 != 0 && num5 < this.cmbPersonalityValueAbility.Items.Count;
					if (flag3)
					{
						this.cmbPersonalityValueAbility.SelectedIndex = num5;
					}
					else
					{
						bool flag4 = num4 < this.cmbPersonalityValueAbility.Items.Count;
						if (flag4)
						{
							this.cmbPersonalityValueAbility.SelectedIndex = num4;
						}
					}
				}
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x00015E78 File Offset: 0x00014078
		private void btnChangeTrainerName_Click(object sender, EventArgs e)
		{
			string text = this.txtTrainerNameInput.Text;
			int num = checked(this.IN_GAME_TRADE_TRAINER_NAME_LENGTH - 1);
			bool flag = text.Length > num;
			if (flag)
			{
				text = text.Substring(0, num);
				this.txtTrainerNameInput.Text = text;
			}
			bool flag2 = Operators.CompareString(text, this.currentTradeData.TrainerName, false) == 0;
			if (!flag2)
			{
				this.currentTradeData.TrainerName = text;
				this.txtTrainerNameReadOnly.Text = text;
				this.SetChangeFlag();
			}
		}

		// Token: 0x060002CA RID: 714 RVA: 0x00015EFC File Offset: 0x000140FC
		private void nudTrainerId_ValueChanged(object sender, EventArgs e)
		{
			bool flag = sender == this.nudTrainerId1;
			if (flag)
			{
				this.currentTradeData.TrainerId1 = Convert.ToUInt16(this.nudTrainerId1.Value);
			}
			else
			{
				bool flag2 = sender == this.nudTrainerId2;
				if (flag2)
				{
					this.currentTradeData.TrainerId2 = Convert.ToUInt16(this.nudTrainerId2.Value);
				}
			}
			this.SetChangeFlag();
		}

		// Token: 0x060002CB RID: 715 RVA: 0x00015F68 File Offset: 0x00014168
		private void UpdateTrainerGender()
		{
			checked
			{
				int num = this.IN_GAME_TRADE_TABLE_OFFSET + (int)this.currentTradeData.TradeIndex * this.IN_GAME_TRADE_ENTRY_LENGTH;
				this.currentTradeData.TrainerGender = this.romData[num + 54];
				byte trainerGender = this.currentTradeData.TrainerGender;
				if (trainerGender != 0)
				{
					if (trainerGender == 1)
					{
						this.rbTrainerGenderFemale.Checked = true;
					}
				}
				else
				{
					this.rbTrainerGenderMale.Checked = true;
				}
			}
		}

		// Token: 0x060002CC RID: 716 RVA: 0x00015FE0 File Offset: 0x000141E0
		private void rbTrainerGender_CheckedChanged(object sender, EventArgs e)
		{
			bool @checked = this.rbTrainerGenderMale.Checked;
			if (@checked)
			{
				this.currentTradeData.TrainerGender = 0;
			}
			else
			{
				bool checked2 = this.rbTrainerGenderFemale.Checked;
				if (checked2)
				{
					this.currentTradeData.TrainerGender = 1;
				}
			}
			this.SetChangeFlag();
		}

		// Token: 0x060002CD RID: 717 RVA: 0x00016034 File Offset: 0x00014234
		private void nudUnknownValue_ValueChanged(object sender, EventArgs e)
		{
			bool flag = sender == this.nudUnknownValue1;
			if (flag)
			{
				this.currentTradeData.UnknownValue1 = Convert.ToByte(this.nudUnknownValue1.Value);
			}
			else
			{
				bool flag2 = sender == this.nudUnknownValue2;
				if (flag2)
				{
					this.currentTradeData.UnknownValue2 = Convert.ToByte(this.nudUnknownValue2.Value);
				}
			}
			this.SetChangeFlag();
		}

		// Token: 0x060002CE RID: 718 RVA: 0x000160A0 File Offset: 0x000142A0
		private void lstInGameTradeList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.lstInGameTradeList.SelectedIndex == (int)this.currentTradeData.TradeIndex;
			if (!flag)
			{
				bool flag2 = this.hasUnsavedChanges;
				if (flag2)
				{
					DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Cancel)
					{
						this.lstInGameTradeList.SelectedIndex = (int)this.currentTradeData.TradeIndex;
						return;
					}
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult == DialogResult.No)
						{
							this.UpdateDisplay();
						}
					}
					else
					{
						this.SaveTradeChanges();
					}
				}
				this.currentTradeData.TradeIndex = checked((byte)this.lstInGameTradeList.SelectedIndex);
				this.UpdateDisplay();
				this.ResetChangeFlag();
			}
		}

		// Token: 0x060002CF RID: 719 RVA: 0x00016158 File Offset: 0x00014358
		private void UpdateTradeData()
		{
			checked
			{
				int num = this.IN_GAME_TRADE_TABLE_OFFSET + (int)this.currentTradeData.TradeIndex * this.IN_GAME_TRADE_ENTRY_LENGTH;
				ushort num2 = BitConverter.ToUInt16(this.romData, num + 56);
				ushort num3 = (ushort)(num2 - 1);
				this.cmbPokemonToGive.SelectedIndex = (int)num3;
				this.currentTradeData.GivePokemonIndex = (ushort)this.cmbPokemonToGive.SelectedIndex;
				ushort num4 = BitConverter.ToUInt16(this.romData, num + 12);
				ushort num5 = (ushort)(num4 - 1);
				this.cmbPokemonToReceive.SelectedIndex = (int)num5;
				this.currentTradeData.ReceivePokemonIndex = (ushort)this.cmbPokemonToReceive.SelectedIndex;
				ushort num6 = BitConverter.ToUInt16(this.romData, num + 40);
				this.cmbHeldItem.SelectedIndex = (int)num6;
				this.currentTradeData.HeldItemId = (ushort)this.cmbHeldItem.SelectedIndex;
				byte[] array = new byte[this.IN_GAME_TRADE_NICKNAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num + 0, array, 0, this.IN_GAME_TRADE_NICKNAME_LENGTH);
				string text = TextConverter.BytesToPokemonString(array, 0, this.IN_GAME_TRADE_NICKNAME_LENGTH);
				this.txtNickNameReadOnly.Text = text;
				this.txtNickNameInput.Text = text;
				this.currentTradeData.NickName = text;
				this.currentTradeData.MailId = this.romData[num + 42];
				this.nudHeldItemMail.Value = new decimal((int)this.currentTradeData.MailId);
				byte b = 0;
				do
				{
					this.currentTradeData.IVs[(int)b] = this.romData[num + 14 + (int)b];
					unchecked
					{
						b += 1;
					}
				}
				while (b <= 5);
				this.nudIndividualValueHp.Value = new decimal((int)this.currentTradeData.IVs[0]);
				this.nudIndividualValueAttack.Value = new decimal((int)this.currentTradeData.IVs[1]);
				this.nudIndividualValueDefense.Value = new decimal((int)this.currentTradeData.IVs[2]);
				this.nudIndividualValueSpeed.Value = new decimal((int)this.currentTradeData.IVs[3]);
				this.nudIndividualValueSpAttack.Value = new decimal((int)this.currentTradeData.IVs[4]);
				this.nudIndividualValueSpDefense.Value = new decimal((int)this.currentTradeData.IVs[5]);
				byte b2 = 0;
				do
				{
					this.currentTradeData.Conditions[(int)b2] = this.romData[num + 28 + (int)b2];
					unchecked
					{
						b2 += 1;
					}
				}
				while (b2 <= 4);
				this.nudConditionsCool.Value = new decimal((int)this.currentTradeData.Conditions[0]);
				this.nudConditionsBeauty.Value = new decimal((int)this.currentTradeData.Conditions[1]);
				this.nudConditionsCute.Value = new decimal((int)this.currentTradeData.Conditions[2]);
				this.nudConditionsSmart.Value = new decimal((int)this.currentTradeData.Conditions[3]);
				this.nudConditionsTough.Value = new decimal((int)this.currentTradeData.Conditions[4]);
				this.currentTradeData.Sheen = this.romData[num + 55];
				this.nudSheen.Value = new decimal((int)this.currentTradeData.Sheen);
				int num7 = num + 36;
				this.currentTradeData.PersonalityValue = BitConverter.ToUInt32(this.romData, num7);
				this.txtPersonalityValueHexa.Text = this.currentTradeData.PersonalityValue.ToString("X8");
				this.txtPersonalityValueDecimal.Text = this.currentTradeData.PersonalityValue.ToString();
				int num8 = (int)(unchecked((ulong)this.currentTradeData.PersonalityValue % (ulong)((long)this.PERSONALITY_TEXT_COUNT)));
				this.cmbPersonalityValuePersonality.SelectedIndex = num8;
				this.CalculateGender();
				this.GetAbilityFromData();
				byte[] array2 = new byte[this.IN_GAME_TRADE_TRAINER_NAME_LENGTH - 1 + 1];
				Array.Copy(this.romData, num + 43, array2, 0, this.IN_GAME_TRADE_TRAINER_NAME_LENGTH);
				string text2 = TextConverter.BytesToPokemonString(array2, 0, this.IN_GAME_TRADE_TRAINER_NAME_LENGTH);
				this.txtTrainerNameReadOnly.Text = text2;
				this.txtTrainerNameInput.Text = text2;
				this.currentTradeData.TrainerName = text2;
				this.currentTradeData.TrainerId1 = BitConverter.ToUInt16(this.romData, num + 24);
				this.nudTrainerId1.Value = new decimal((int)this.currentTradeData.TrainerId1);
				this.currentTradeData.TrainerId2 = BitConverter.ToUInt16(this.romData, num + 26);
				this.nudTrainerId2.Value = new decimal((int)this.currentTradeData.TrainerId2);
				this.UpdateTrainerGender();
				this.currentTradeData.UnknownValue1 = this.romData[num + 58];
				this.nudUnknownValue1.Value = new decimal((int)this.currentTradeData.UnknownValue1);
				this.currentTradeData.UnknownValue2 = this.romData[num + 59];
				this.nudUnknownValue2.Value = new decimal((int)this.currentTradeData.UnknownValue2);
			}
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0001666C File Offset: 0x0001486C
		private void SaveTradeChanges()
		{
			checked
			{
				int num = this.IN_GAME_TRADE_TABLE_OFFSET + (int)this.currentTradeData.TradeIndex * this.IN_GAME_TRADE_ENTRY_LENGTH;
				ushort num2 = (ushort)(this.cmbPokemonToGive.SelectedIndex + 1);
				byte[] bytes = BitConverter.GetBytes(num2);
				Array.Copy(bytes, 0, this.romData, num + 56, 2);
				ushort num3 = (ushort)(this.cmbPokemonToReceive.SelectedIndex + 1);
				byte[] bytes2 = BitConverter.GetBytes(num3);
				Array.Copy(bytes2, 0, this.romData, num + 12, 2);
				ushort num4 = (ushort)this.cmbHeldItem.SelectedIndex;
				byte[] bytes3 = BitConverter.GetBytes(num4);
				Array.Copy(bytes3, 0, this.romData, num + 40, 2);
				int num5 = this.IN_GAME_TRADE_NICKNAME_LENGTH - 1;
				for (int i = 0; i <= num5; i++)
				{
					this.romData[num + 0 + i] = 0;
				}
				byte[] array = TextConverter.PokemonStringToBytes(this.currentTradeData.NickName, 5);
				Array.Copy(array, 0, this.romData, num + 0, Math.Min(array.Length, this.IN_GAME_TRADE_NICKNAME_LENGTH));
				this.romData[num + 42] = this.currentTradeData.MailId;
				int num6 = 0;
				do
				{
					this.romData[num + 14 + num6] = this.currentTradeData.IVs[num6];
					num6++;
				}
				while (num6 <= 5);
				int num7 = 0;
				do
				{
					this.romData[num + 28 + num7] = this.currentTradeData.Conditions[num7];
					num7++;
				}
				while (num7 <= 4);
				this.romData[num + 55] = this.currentTradeData.Sheen;
				int num8 = num + 36;
				byte[] bytes4 = BitConverter.GetBytes(this.currentTradeData.PersonalityValue);
				Array.Copy(bytes4, 0, this.romData, num8, 4);
				this.romData[num + 20] = this.currentTradeData.AbilityValue;
				int num9 = this.IN_GAME_TRADE_TRAINER_NAME_LENGTH - 1;
				for (int j = 0; j <= num9; j++)
				{
					this.romData[num + 43 + j] = 0;
				}
				byte[] array2 = TextConverter.PokemonStringToBytes(this.currentTradeData.TrainerName, this.IN_GAME_TRADE_TRAINER_NAME_LENGTH - 1);
				Array.Copy(array2, 0, this.romData, num + 43, Math.Min(array2.Length, this.IN_GAME_TRADE_TRAINER_NAME_LENGTH));
				byte[] bytes5 = BitConverter.GetBytes(this.currentTradeData.TrainerId1);
				Array.Copy(bytes5, 0, this.romData, num + 24, 2);
				byte[] bytes6 = BitConverter.GetBytes(this.currentTradeData.TrainerId2);
				Array.Copy(bytes6, 0, this.romData, num + 26, 2);
				this.romData[num + 54] = this.currentTradeData.TrainerGender;
				this.romData[num + 58] = this.currentTradeData.UnknownValue1;
				this.romData[num + 59] = this.currentTradeData.UnknownValue2;
				MainForm.romData = this.romData;
				this.ResetChangeFlag();
			}
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x00016937 File Offset: 0x00014B37
		private void SetChangeFlag()
		{
			this.hasUnsavedChanges = true;
			this.btnSave.Enabled = true;
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x0001694E File Offset: 0x00014B4E
		private void ResetChangeFlag()
		{
			this.hasUnsavedChanges = false;
			this.btnSave.Enabled = false;
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x00016965 File Offset: 0x00014B65
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveTradeChanges();
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x00016970 File Offset: 0x00014B70
		private void InGameTradeEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Cancel)
				{
					if (dialogResult == DialogResult.Yes)
					{
						this.SaveTradeChanges();
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0400016A RID: 362
		public readonly int IN_GAME_TRADE_TABLE_OFFSET;

		// Token: 0x0400016B RID: 363
		public readonly int IN_GAME_TRADE_ENTRY_LENGTH;

		// Token: 0x0400016C RID: 364
		public readonly int IN_GAME_TRADE_TOTAL_COUNT;

		// Token: 0x0400016D RID: 365
		public const int IN_GAME_TRADE_GIVE_POKEMON_OFFSET = 56;

		// Token: 0x0400016E RID: 366
		public const int IN_GAME_TRADE_RECEIVE_POKEMON_OFFSET = 12;

		// Token: 0x0400016F RID: 367
		public const int IN_GAME_TRADE_HELD_ITEM_OFFSET = 40;

		// Token: 0x04000170 RID: 368
		public const int IN_GAME_TRADE_NICKNAME_OFFSET = 0;

		// Token: 0x04000171 RID: 369
		public readonly int IN_GAME_TRADE_NICKNAME_LENGTH;

		// Token: 0x04000172 RID: 370
		public const int IN_GAME_TRADE_MAIL_OFFSET = 42;

		// Token: 0x04000173 RID: 371
		public const int IN_GAME_TRADE_IV_OFFSET = 14;

		// Token: 0x04000174 RID: 372
		public const int IN_GAME_TRADE_CONDITION_OFFSET = 28;

		// Token: 0x04000175 RID: 373
		public const int IN_GAME_TRADE_SHEEN_OFFSET = 55;

		// Token: 0x04000176 RID: 374
		public const int IN_GAME_TRADE_PERSONALITY_VALUE_OFFSET = 36;

		// Token: 0x04000177 RID: 375
		public readonly int PERSONALITY_TEXT_TABLE_OFFSET;

		// Token: 0x04000178 RID: 376
		public readonly int PERSONALITY_TEXT_COUNT;

		// Token: 0x04000179 RID: 377
		public const int IN_GAME_TRADE_Ability_VALUE_OFFSET = 20;

		// Token: 0x0400017A RID: 378
		public const int IN_GAME_TRADE_TRAINER_NAME_OFFSET = 43;

		// Token: 0x0400017B RID: 379
		public readonly int IN_GAME_TRADE_TRAINER_NAME_LENGTH;

		// Token: 0x0400017C RID: 380
		public const int IN_GAME_TRADE_TRAINER_ID1_OFFSET = 24;

		// Token: 0x0400017D RID: 381
		public const int IN_GAME_TRADE_TRAINER_ID2_OFFSET = 26;

		// Token: 0x0400017E RID: 382
		public const int IN_GAME_TRADE_TRAINER_GENDER_OFFSET = 54;

		// Token: 0x0400017F RID: 383
		public const int IN_GAME_TRADE_UNKNOWN_VALUE1_OFFSET = 58;

		// Token: 0x04000180 RID: 384
		public const int IN_GAME_TRADE_UNKNOWN_VALUE2_OFFSET = 59;

		// Token: 0x04000181 RID: 385
		private byte[] romData;

		// Token: 0x04000182 RID: 386
		private bool hasUnsavedChanges;

		// Token: 0x04000183 RID: 387
		private Dictionary<int, PokemonData> pokemonIconList;

		// Token: 0x04000184 RID: 388
		private InGameTradeEditor.TradeData currentTradeData;

		// Token: 0x0200003F RID: 63
		public class TradeData
		{
			// Token: 0x06000F32 RID: 3890 RVA: 0x0006B7CB File Offset: 0x000699CB
			public TradeData()
			{
				this.IVs = new byte[6];
				this.Conditions = new byte[5];
			}

			// Token: 0x170005C1 RID: 1473
			// (get) Token: 0x06000F33 RID: 3891 RVA: 0x0006B7EE File Offset: 0x000699EE
			// (set) Token: 0x06000F34 RID: 3892 RVA: 0x0006B7F8 File Offset: 0x000699F8
			public byte TradeIndex { get; set; }

			// Token: 0x170005C2 RID: 1474
			// (get) Token: 0x06000F35 RID: 3893 RVA: 0x0006B801 File Offset: 0x00069A01
			// (set) Token: 0x06000F36 RID: 3894 RVA: 0x0006B80B File Offset: 0x00069A0B
			public ushort GivePokemonIndex { get; set; }

			// Token: 0x170005C3 RID: 1475
			// (get) Token: 0x06000F37 RID: 3895 RVA: 0x0006B814 File Offset: 0x00069A14
			// (set) Token: 0x06000F38 RID: 3896 RVA: 0x0006B81E File Offset: 0x00069A1E
			public ushort ReceivePokemonIndex { get; set; }

			// Token: 0x170005C4 RID: 1476
			// (get) Token: 0x06000F39 RID: 3897 RVA: 0x0006B827 File Offset: 0x00069A27
			// (set) Token: 0x06000F3A RID: 3898 RVA: 0x0006B831 File Offset: 0x00069A31
			public ushort HeldItemId { get; set; }

			// Token: 0x170005C5 RID: 1477
			// (get) Token: 0x06000F3B RID: 3899 RVA: 0x0006B83A File Offset: 0x00069A3A
			// (set) Token: 0x06000F3C RID: 3900 RVA: 0x0006B844 File Offset: 0x00069A44
			public string NickName { get; set; }

			// Token: 0x170005C6 RID: 1478
			// (get) Token: 0x06000F3D RID: 3901 RVA: 0x0006B84D File Offset: 0x00069A4D
			// (set) Token: 0x06000F3E RID: 3902 RVA: 0x0006B857 File Offset: 0x00069A57
			public byte MailId { get; set; }

			// Token: 0x170005C7 RID: 1479
			// (get) Token: 0x06000F3F RID: 3903 RVA: 0x0006B860 File Offset: 0x00069A60
			// (set) Token: 0x06000F40 RID: 3904 RVA: 0x0006B86A File Offset: 0x00069A6A
			public byte[] IVs { get; set; }

			// Token: 0x170005C8 RID: 1480
			// (get) Token: 0x06000F41 RID: 3905 RVA: 0x0006B873 File Offset: 0x00069A73
			// (set) Token: 0x06000F42 RID: 3906 RVA: 0x0006B87D File Offset: 0x00069A7D
			public byte[] Conditions { get; set; }

			// Token: 0x170005C9 RID: 1481
			// (get) Token: 0x06000F43 RID: 3907 RVA: 0x0006B886 File Offset: 0x00069A86
			// (set) Token: 0x06000F44 RID: 3908 RVA: 0x0006B890 File Offset: 0x00069A90
			public byte Sheen { get; set; }

			// Token: 0x170005CA RID: 1482
			// (get) Token: 0x06000F45 RID: 3909 RVA: 0x0006B899 File Offset: 0x00069A99
			// (set) Token: 0x06000F46 RID: 3910 RVA: 0x0006B8A3 File Offset: 0x00069AA3
			public uint PersonalityValue { get; set; }

			// Token: 0x170005CB RID: 1483
			// (get) Token: 0x06000F47 RID: 3911 RVA: 0x0006B8AC File Offset: 0x00069AAC
			// (set) Token: 0x06000F48 RID: 3912 RVA: 0x0006B8B6 File Offset: 0x00069AB6
			public byte AbilityValue { get; set; }

			// Token: 0x170005CC RID: 1484
			// (get) Token: 0x06000F49 RID: 3913 RVA: 0x0006B8BF File Offset: 0x00069ABF
			// (set) Token: 0x06000F4A RID: 3914 RVA: 0x0006B8C9 File Offset: 0x00069AC9
			public string TrainerName { get; set; }

			// Token: 0x170005CD RID: 1485
			// (get) Token: 0x06000F4B RID: 3915 RVA: 0x0006B8D2 File Offset: 0x00069AD2
			// (set) Token: 0x06000F4C RID: 3916 RVA: 0x0006B8DC File Offset: 0x00069ADC
			public ushort TrainerId1 { get; set; }

			// Token: 0x170005CE RID: 1486
			// (get) Token: 0x06000F4D RID: 3917 RVA: 0x0006B8E5 File Offset: 0x00069AE5
			// (set) Token: 0x06000F4E RID: 3918 RVA: 0x0006B8EF File Offset: 0x00069AEF
			public ushort TrainerId2 { get; set; }

			// Token: 0x170005CF RID: 1487
			// (get) Token: 0x06000F4F RID: 3919 RVA: 0x0006B8F8 File Offset: 0x00069AF8
			// (set) Token: 0x06000F50 RID: 3920 RVA: 0x0006B902 File Offset: 0x00069B02
			public byte TrainerGender { get; set; }

			// Token: 0x170005D0 RID: 1488
			// (get) Token: 0x06000F51 RID: 3921 RVA: 0x0006B90B File Offset: 0x00069B0B
			// (set) Token: 0x06000F52 RID: 3922 RVA: 0x0006B915 File Offset: 0x00069B15
			public byte UnknownValue1 { get; set; }

			// Token: 0x170005D1 RID: 1489
			// (get) Token: 0x06000F53 RID: 3923 RVA: 0x0006B91E File Offset: 0x00069B1E
			// (set) Token: 0x06000F54 RID: 3924 RVA: 0x0006B928 File Offset: 0x00069B28
			public byte UnknownValue2 { get; set; }
		}
	}
}
