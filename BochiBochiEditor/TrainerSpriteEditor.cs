using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000029 RID: 41
	public partial class TrainerSpriteEditor : Form
	{
		// Token: 0x06000C66 RID: 3174 RVA: 0x0005CC08 File Offset: 0x0005AE08
		public TrainerSpriteEditor()
		{
			base.Load += this.TrainerSpriteEditor_Load;
			base.FormClosing += this.TrainerSpriteEditor_FormClosing;
			this.TRAINER_SPRITE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_SPRITE_TABLE_OFFSET");
			this.TRAINER_PALETTE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_PALETTE_TABLE_OFFSET");
			this.TRAINER_Y_POSITION_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_Y_POSITION_TABLE_OFFSET");
			this.TRAINER_ANIMATION_POINTER_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_ANIMATION_POINTER_TABLE_OFFSET");
			this.TRAINER_ANIMATION_DATA_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_ANIMATION_DATA_TABLE_OFFSET");
			this.MAX_TRAINER_SPRITE_COUNT = RomIniReader.ReadHexOrDecimal("MAX_TRAINER_SPRITE_COUNT");
			this.TRAINER_CLASS_NAME_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_TABLE_OFFSET");
			this.TRAINER_CLASS_NAME_LENGTH = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_LENGTH");
			this.TRAINER_CLASS_NAME_COUNT = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_COUNT");
			this.PRIZE_MONEY_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("PRIZE_MONEY_TABLE_OFFSET");
			this.PRIZE_MONEY_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("PRIZE_MONEY_ENTRY_LENGTH");
			this.PRIZE_MONEY_COUNT = RomIniReader.ReadHexOrDecimal("PRIZE_MONEY_COUNT");
			this.currentIndex = 0;
			this.isTrainerSpriteModified = false;
			this.isTrainerClassModified = false;
			this.temporaryImageData = null;
			this.temporaryPaletteData = null;
			this.currentTrainerClassIndex = 0;
			this.originalTrainerClassName = "";
			this.originalPrizeMoneyRate = 0;
			this.InitializeComponent();
		}

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x06000C69 RID: 3177 RVA: 0x0005DDC8 File Offset: 0x0005BFC8
		// (set) Token: 0x06000C6A RID: 3178 RVA: 0x0005DDD4 File Offset: 0x0005BFD4
		internal virtual Button btnSaveTrainerSprite
		{
			[CompilerGenerated]
			get
			{
				return this._btnSaveTrainerSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSaveTrainerSprite_Click);
				Button button = this._btnSaveTrainerSprite;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSaveTrainerSprite = value;
				button = this._btnSaveTrainerSprite;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x06000C6B RID: 3179 RVA: 0x0005DE17 File Offset: 0x0005C017
		// (set) Token: 0x06000C6C RID: 3180 RVA: 0x0005DE24 File Offset: 0x0005C024
		internal virtual TextBox txtTrainerSpritePalAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtTrainerSpritePalAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtTrainerSpritePalAddress_Enter);
				TextBox textBox = this._txtTrainerSpritePalAddress;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtTrainerSpritePalAddress = value;
				textBox = this._txtTrainerSpritePalAddress;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x06000C6D RID: 3181 RVA: 0x0005DE67 File Offset: 0x0005C067
		// (set) Token: 0x06000C6E RID: 3182 RVA: 0x0005DE74 File Offset: 0x0005C074
		internal virtual NumericUpDown nudTrainerSpriteID
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerSpriteID;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudTrainerSpriteID_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerSpriteID;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerSpriteID = value;
				numericUpDown = this._nudTrainerSpriteID;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x0005DEB7 File Offset: 0x0005C0B7
		// (set) Token: 0x06000C70 RID: 3184 RVA: 0x0005DEC1 File Offset: 0x0005C0C1
		internal virtual PictureBox picTrainerSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x06000C71 RID: 3185 RVA: 0x0005DECA File Offset: 0x0005C0CA
		// (set) Token: 0x06000C72 RID: 3186 RVA: 0x0005DED4 File Offset: 0x0005C0D4
		internal virtual NumericUpDown nudTrainerSpriteYPosition
		{
			[CompilerGenerated]
			get
			{
				return this._nudTrainerSpriteYPosition;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudTrainerSpriteYPosition_ValueChanged);
				NumericUpDown numericUpDown = this._nudTrainerSpriteYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudTrainerSpriteYPosition = value;
				numericUpDown = this._nudTrainerSpriteYPosition;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x06000C73 RID: 3187 RVA: 0x0005DF17 File Offset: 0x0005C117
		// (set) Token: 0x06000C74 RID: 3188 RVA: 0x0005DF21 File Offset: 0x0005C121
		internal virtual Label lblTrainerSpriteAnimationPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x06000C75 RID: 3189 RVA: 0x0005DF2A File Offset: 0x0005C12A
		// (set) Token: 0x06000C76 RID: 3190 RVA: 0x0005DF34 File Offset: 0x0005C134
		internal virtual Label lblTrainerSpriteAnimationData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x06000C77 RID: 3191 RVA: 0x0005DF3D File Offset: 0x0005C13D
		// (set) Token: 0x06000C78 RID: 3192 RVA: 0x0005DF47 File Offset: 0x0005C147
		internal virtual TextBox txtTrainerSpriteAnimationPointer
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x06000C79 RID: 3193 RVA: 0x0005DF50 File Offset: 0x0005C150
		// (set) Token: 0x06000C7A RID: 3194 RVA: 0x0005DF5A File Offset: 0x0005C15A
		internal virtual TextBox txtTrainerSpriteAnimationData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x06000C7B RID: 3195 RVA: 0x0005DF63 File Offset: 0x0005C163
		// (set) Token: 0x06000C7C RID: 3196 RVA: 0x0005DF6D File Offset: 0x0005C16D
		internal virtual Label lblTrainerSpriteYPosition
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x06000C7D RID: 3197 RVA: 0x0005DF76 File Offset: 0x0005C176
		// (set) Token: 0x06000C7E RID: 3198 RVA: 0x0005DF80 File Offset: 0x0005C180
		internal virtual TextBox txtTrainerClassName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x06000C7F RID: 3199 RVA: 0x0005DF89 File Offset: 0x0005C189
		// (set) Token: 0x06000C80 RID: 3200 RVA: 0x0005DF94 File Offset: 0x0005C194
		internal virtual ComboBox cmbTrainerClassName
		{
			[CompilerGenerated]
			get
			{
				return this._cmbTrainerClassName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbTrainerClassName_SelectedIndexChanged);
				ComboBox comboBox = this._cmbTrainerClassName;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbTrainerClassName = value;
				comboBox = this._cmbTrainerClassName;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x06000C81 RID: 3201 RVA: 0x0005DFD7 File Offset: 0x0005C1D7
		// (set) Token: 0x06000C82 RID: 3202 RVA: 0x0005DFE1 File Offset: 0x0005C1E1
		internal virtual Label lblTrainerClassName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x06000C83 RID: 3203 RVA: 0x0005DFEA File Offset: 0x0005C1EA
		// (set) Token: 0x06000C84 RID: 3204 RVA: 0x0005DFF4 File Offset: 0x0005C1F4
		internal virtual Button btnChangeTrainerClassName
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeTrainerClassName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeTrainerClassName_Click);
				Button button = this._btnChangeTrainerClassName;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeTrainerClassName = value;
				button = this._btnChangeTrainerClassName;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x06000C85 RID: 3205 RVA: 0x0005E037 File Offset: 0x0005C237
		// (set) Token: 0x06000C86 RID: 3206 RVA: 0x0005E044 File Offset: 0x0005C244
		internal virtual NumericUpDown nudPrizeMoneyRate
		{
			[CompilerGenerated]
			get
			{
				return this._nudPrizeMoneyRate;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudPrizeMoneyRate_ValueChanged);
				NumericUpDown numericUpDown = this._nudPrizeMoneyRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudPrizeMoneyRate = value;
				numericUpDown = this._nudPrizeMoneyRate;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06000C87 RID: 3207 RVA: 0x0005E087 File Offset: 0x0005C287
		// (set) Token: 0x06000C88 RID: 3208 RVA: 0x0005E091 File Offset: 0x0005C291
		internal virtual Label lblPrizeMoneyRate
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x06000C89 RID: 3209 RVA: 0x0005E09A File Offset: 0x0005C29A
		// (set) Token: 0x06000C8A RID: 3210 RVA: 0x0005E0A4 File Offset: 0x0005C2A4
		internal virtual Button btnSaveTrainerClass
		{
			[CompilerGenerated]
			get
			{
				return this._btnSaveTrainerClass;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSaveTrainerClass_Click);
				Button button = this._btnSaveTrainerClass;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSaveTrainerClass = value;
				button = this._btnSaveTrainerClass;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x06000C8B RID: 3211 RVA: 0x0005E0E7 File Offset: 0x0005C2E7
		// (set) Token: 0x06000C8C RID: 3212 RVA: 0x0005E0F1 File Offset: 0x0005C2F1
		internal virtual GroupBox grpTrainerSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x06000C8D RID: 3213 RVA: 0x0005E0FA File Offset: 0x0005C2FA
		// (set) Token: 0x06000C8E RID: 3214 RVA: 0x0005E104 File Offset: 0x0005C304
		internal virtual RadioButton rbTrainerSpritePalAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x06000C8F RID: 3215 RVA: 0x0005E10D File Offset: 0x0005C30D
		// (set) Token: 0x06000C90 RID: 3216 RVA: 0x0005E117 File Offset: 0x0005C317
		internal virtual RadioButton rbTrainerSpriteImgAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x06000C91 RID: 3217 RVA: 0x0005E120 File Offset: 0x0005C320
		// (set) Token: 0x06000C92 RID: 3218 RVA: 0x0005E12A File Offset: 0x0005C32A
		internal virtual GroupBox grpTrainerSpritePreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x06000C93 RID: 3219 RVA: 0x0005E133 File Offset: 0x0005C333
		// (set) Token: 0x06000C94 RID: 3220 RVA: 0x0005E13D File Offset: 0x0005C33D
		internal virtual GroupBox grpImportExport
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06000C95 RID: 3221 RVA: 0x0005E146 File Offset: 0x0005C346
		// (set) Token: 0x06000C96 RID: 3222 RVA: 0x0005E150 File Offset: 0x0005C350
		internal virtual Button btnExportTrainerSprite
		{
			[CompilerGenerated]
			get
			{
				return this._btnExportTrainerSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnExportTrainerSprite_Click);
				Button button = this._btnExportTrainerSprite;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnExportTrainerSprite = value;
				button = this._btnExportTrainerSprite;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x06000C97 RID: 3223 RVA: 0x0005E193 File Offset: 0x0005C393
		// (set) Token: 0x06000C98 RID: 3224 RVA: 0x0005E19D File Offset: 0x0005C39D
		internal virtual TextBox txtImportTrainerSpriteAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06000C99 RID: 3225 RVA: 0x0005E1A6 File Offset: 0x0005C3A6
		// (set) Token: 0x06000C9A RID: 3226 RVA: 0x0005E1B0 File Offset: 0x0005C3B0
		internal virtual Button btnImportTrainerSprite
		{
			[CompilerGenerated]
			get
			{
				return this._btnImportTrainerSprite;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnImportTrainerSprite_Click);
				Button button = this._btnImportTrainerSprite;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnImportTrainerSprite = value;
				button = this._btnImportTrainerSprite;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x06000C9B RID: 3227 RVA: 0x0005E1F3 File Offset: 0x0005C3F3
		// (set) Token: 0x06000C9C RID: 3228 RVA: 0x0005E1FD File Offset: 0x0005C3FD
		internal virtual GroupBox grpTrainerClass
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x06000C9D RID: 3229 RVA: 0x0005E206 File Offset: 0x0005C406
		// (set) Token: 0x06000C9E RID: 3230 RVA: 0x0005E210 File Offset: 0x0005C410
		internal virtual Button btnChangeTrainerSpriteAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeTrainerSpriteAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeTrainerSpriteAddress_Click);
				Button button = this._btnChangeTrainerSpriteAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeTrainerSpriteAddress = value;
				button = this._btnChangeTrainerSpriteAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x06000C9F RID: 3231 RVA: 0x0005E253 File Offset: 0x0005C453
		// (set) Token: 0x06000CA0 RID: 3232 RVA: 0x0005E260 File Offset: 0x0005C460
		internal virtual TextBox txtTrainerSpriteImgAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtTrainerSpriteImgAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtTrainerSpriteImgAddress_Enter);
				TextBox textBox = this._txtTrainerSpriteImgAddress;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtTrainerSpriteImgAddress = value;
				textBox = this._txtTrainerSpriteImgAddress;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x06000CA1 RID: 3233 RVA: 0x0005E2A4 File Offset: 0x0005C4A4
		private void TrainerSpriteEditor_Load(object sender, EventArgs e)
		{
			this.nudTrainerSpriteID.Minimum = 0m;
			this.nudTrainerSpriteID.Maximum = new decimal(checked(this.MAX_TRAINER_SPRITE_COUNT - 1));
			this.LoadTrainerData(0);
			this.isTrainerSpriteModified = false;
			this.LoadTrainerClassData();
			this.isTrainerClassModified = false;
			this.rbTrainerSpriteImgAddress.Checked = true;
		}

		// Token: 0x06000CA2 RID: 3234 RVA: 0x0005E308 File Offset: 0x0005C508
		private void nudTrainerSpriteID_ValueChanged(object sender, EventArgs e)
		{
			bool flag = this.isTrainerSpriteModified;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveTrainerSprite();
				}
				else
				{
					bool flag3 = dialogResult == DialogResult.Cancel;
					if (flag3)
					{
						this.nudTrainerSpriteID.Value = new decimal(this.currentIndex);
						return;
					}
					this.temporaryImageData = null;
					this.temporaryPaletteData = null;
				}
			}
			this.txtImportTrainerSpriteAddress.Text = "";
			this.LoadTrainerData(Convert.ToInt32(this.nudTrainerSpriteID.Value));
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x0005E3A0 File Offset: 0x0005C5A0
		private void LoadTrainerData(int index)
		{
			this.currentIndex = index;
			checked
			{
				int num = this.TRAINER_SPRITE_TABLE_OFFSET + index * 8;
				this.originalImageAddress = BitConverter.ToUInt32(MainForm.romData, num) - 134217728U;
				this.txtTrainerSpriteImgAddress.Text = string.Format("{0:X8}", this.originalImageAddress);
				int num2 = this.TRAINER_PALETTE_TABLE_OFFSET + index * 8;
				this.originalPaletteAddress = BitConverter.ToUInt32(MainForm.romData, num2) - 134217728U;
				this.txtTrainerSpritePalAddress.Text = string.Format("{0:X8}", this.originalPaletteAddress);
				int num3 = this.TRAINER_Y_POSITION_TABLE_OFFSET + index * 4;
				this.originalYPosition = MainForm.romData[num3 + 1];
				this.nudTrainerSpriteYPosition.Value = new decimal((int)this.originalYPosition);
				int num4 = this.TRAINER_ANIMATION_POINTER_TABLE_OFFSET + index * 4;
				this.txtTrainerSpriteAnimationPointer.Text = string.Format("{0:X8}", BitConverter.ToUInt32(MainForm.romData, num4) - 134217728U);
				int num5 = this.TRAINER_ANIMATION_DATA_TABLE_OFFSET + index * 4;
				this.txtTrainerSpriteAnimationData.Text = string.Format("{0:X8}", BitConverter.ToUInt32(MainForm.romData, num5) - 134217728U);
				this.DisplayTrainerSprite();
				this.isTrainerSpriteModified = false;
				this.btnSaveTrainerSprite.Enabled = false;
			}
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x0005E4F8 File Offset: 0x0005C6F8
		private void DisplayTrainerSprite()
		{
			try
			{
				uint num = Convert.ToUInt32(this.txtTrainerSpriteImgAddress.Text, 16);
				uint num2 = Convert.ToUInt32(this.txtTrainerSpritePalAddress.Text, 16);
				byte[] currentImageData = this.GetCurrentImageData();
				byte[] currentPaletteData = this.GetCurrentPaletteData();
				Color[] array = ImageProcessor.LoadPalette(currentPaletteData, true);
				Bitmap bitmap = ImageProcessor.LoadSprite(ref currentImageData, array, 64, 64, false);
				bool flag = this.picTrainerSprite.Image != null;
				if (flag)
				{
					this.picTrainerSprite.Image.Dispose();
				}
				this.picTrainerSprite.Image = bitmap;
				this.picTrainerSprite.Refresh();
			}
			catch (Exception ex)
			{
				this.picTrainerSprite.Image = null;
			}
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x0005E5C8 File Offset: 0x0005C7C8
		private void txtTrainerSpriteImgAddress_Enter(object sender, EventArgs e)
		{
			this.rbTrainerSpriteImgAddress.Checked = true;
		}

		// Token: 0x06000CA6 RID: 3238 RVA: 0x0005E5D8 File Offset: 0x0005C7D8
		private void txtTrainerSpritePalAddress_Enter(object sender, EventArgs e)
		{
			this.rbTrainerSpritePalAddress.Checked = true;
		}

		// Token: 0x06000CA7 RID: 3239 RVA: 0x0005E5E8 File Offset: 0x0005C7E8
		private void btnChangeTrainerSpriteAddress_Click(object sender, EventArgs e)
		{
			this.temporaryImageData = null;
			this.temporaryPaletteData = null;
			this.DisplayTrainerSprite();
			this.CheckForChanges();
		}

		// Token: 0x06000CA8 RID: 3240 RVA: 0x0005E608 File Offset: 0x0005C808
		private void btnImportTrainerSprite_Click(object sender, EventArgs e)
		{
			string text = this.txtImportTrainerSpriteAddress.Text.Trim();
			bool flag = string.IsNullOrEmpty(text);
			if (flag)
			{
				MessageBox.Show("アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				uint num;
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
						openFileDialog.Title = "トレーナー画像をインポート";
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
										bool @checked = this.rbTrainerSpriteImgAddress.Checked;
										if (@checked)
										{
											byte[] array = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
											this.temporaryImageData = ImageProcessor.LZ77Comp(array, false);
											this.txtTrainerSpriteImgAddress.Text = text;
											this.originalImageAddress = num;
										}
										else
										{
											bool checked2 = this.rbTrainerSpritePalAddress.Checked;
											if (checked2)
											{
												byte[] array2 = ImageProcessor.ConvertPaletteToBytes(bitmap.Palette);
												this.temporaryPaletteData = ImageProcessor.LZ77Comp(array2, true);
												this.txtTrainerSpritePalAddress.Text = text;
												this.originalPaletteAddress = num;
											}
										}
										this.DisplayTrainerSprite();
										this.isTrainerSpriteModified = true;
										this.btnSaveTrainerSprite.Enabled = true;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000CA9 RID: 3241 RVA: 0x0005E81C File Offset: 0x0005CA1C
		private void btnExportTrainerSprite_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = "PNG画像|*.png";
				saveFileDialog.Title = "トレーナー画像をエクスポート";
				saveFileDialog.FileName = string.Format("trainersprite_{0:X4}.png", this.currentIndex);
				bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					byte[] currentImageData = this.GetCurrentImageData();
					byte[] currentPaletteData = this.GetCurrentPaletteData();
					Color[] array = ImageProcessor.LoadPalette(currentPaletteData, false);
					ImageProcessor.ExportSpriteTo4bppPng(saveFileDialog.FileName, currentImageData, array, 64, 64);
				}
			}
		}

		// Token: 0x06000CAA RID: 3242 RVA: 0x0005E8BC File Offset: 0x0005CABC
		private byte[] GetCurrentImageData()
		{
			bool flag = this.temporaryImageData != null;
			byte[] array;
			if (flag)
			{
				array = this.DecompressIfNeeded(this.temporaryImageData);
			}
			else
			{
				uint num = Convert.ToUInt32(this.txtTrainerSpriteImgAddress.Text, 16);
				array = ImageProcessor.LoadCompressedImagePaletteFromROM(MainForm.romData, num, false);
			}
			return array;
		}

		// Token: 0x06000CAB RID: 3243 RVA: 0x0005E90C File Offset: 0x0005CB0C
		private byte[] GetCurrentPaletteData()
		{
			bool flag = this.temporaryPaletteData != null;
			byte[] array;
			if (flag)
			{
				array = this.DecompressIfNeeded(this.temporaryPaletteData);
			}
			else
			{
				uint num = Convert.ToUInt32(this.txtTrainerSpritePalAddress.Text, 16);
				array = ImageProcessor.LoadCompressedImagePaletteFromROM(MainForm.romData, num, true);
			}
			return array;
		}

		// Token: 0x06000CAC RID: 3244 RVA: 0x0005E95C File Offset: 0x0005CB5C
		private byte[] DecompressIfNeeded(byte[] compressedData)
		{
			int num = BitConverter.ToInt32(compressedData, 0);
			bool flag = (num & 255) == 16;
			byte[] array2;
			if (flag)
			{
				int num2 = num >> 8;
				byte[] array = new byte[checked(num2 - 1 + 1)];
				ImageProcessor.LZ77UnComp(compressedData, array);
				array2 = array;
			}
			else
			{
				array2 = compressedData;
			}
			return array2;
		}

		// Token: 0x06000CAD RID: 3245 RVA: 0x0005E9A5 File Offset: 0x0005CBA5
		private void nudTrainerSpriteYPosition_ValueChanged(object sender, EventArgs e)
		{
			this.CheckForChanges();
		}

		// Token: 0x06000CAE RID: 3246 RVA: 0x0005E9B0 File Offset: 0x0005CBB0
		private void CheckForChanges()
		{
			uint num = Convert.ToUInt32(this.txtTrainerSpriteImgAddress.Text, 16);
			uint num2 = Convert.ToUInt32(this.txtTrainerSpritePalAddress.Text, 16);
			byte b = Convert.ToByte(this.nudTrainerSpriteYPosition.Value);
			this.isTrainerSpriteModified = num != this.originalImageAddress || num2 != this.originalPaletteAddress || b != this.originalYPosition;
			this.btnSaveTrainerSprite.Enabled = this.isTrainerSpriteModified;
		}

		// Token: 0x06000CAF RID: 3247 RVA: 0x0005EA2E File Offset: 0x0005CC2E
		private void btnSaveTrainerSprite_Click(object sender, EventArgs e)
		{
			this.SaveTrainerSprite();
		}

		// Token: 0x06000CB0 RID: 3248 RVA: 0x0005EA38 File Offset: 0x0005CC38
		private void SaveTrainerSprite()
		{
			uint num = Convert.ToUInt32(this.txtTrainerSpriteImgAddress.Text, 16);
			uint num2 = Convert.ToUInt32(this.txtTrainerSpritePalAddress.Text, 16);
			byte b = Convert.ToByte(this.nudTrainerSpriteYPosition.Value);
			bool flag = this.temporaryImageData != null;
			if (flag)
			{
				Array.Copy(this.temporaryImageData, 0L, MainForm.romData, (long)((ulong)num), (long)this.temporaryImageData.Length);
				this.temporaryImageData = null;
			}
			bool flag2 = this.temporaryPaletteData != null;
			if (flag2)
			{
				Array.Copy(this.temporaryPaletteData, 0L, MainForm.romData, (long)((ulong)num2), (long)this.temporaryPaletteData.Length);
				this.temporaryPaletteData = null;
			}
			checked
			{
				int num3 = this.TRAINER_SPRITE_TABLE_OFFSET + this.currentIndex * 8;
				byte[] bytes = BitConverter.GetBytes(num + 134217728U);
				Array.Copy(bytes, 0, MainForm.romData, num3, 4);
				int num4 = this.TRAINER_PALETTE_TABLE_OFFSET + this.currentIndex * 8;
				byte[] bytes2 = BitConverter.GetBytes(num2 + 134217728U);
				Array.Copy(bytes2, 0, MainForm.romData, num4, 4);
				int num5 = this.TRAINER_Y_POSITION_TABLE_OFFSET + this.currentIndex * 4;
				MainForm.romData[num5 + 1] = b;
				this.originalImageAddress = num;
				this.originalPaletteAddress = num2;
				this.originalYPosition = b;
				this.isTrainerSpriteModified = false;
				this.btnSaveTrainerSprite.Enabled = false;
			}
		}

		// Token: 0x06000CB1 RID: 3249 RVA: 0x0005EB8C File Offset: 0x0005CD8C
		private void LoadTrainerClassData()
		{
			this.cmbTrainerClassName.Items.Clear();
			checked
			{
				int num = this.TRAINER_CLASS_NAME_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.TRAINER_CLASS_NAME_TABLE_OFFSET + i * this.TRAINER_CLASS_NAME_LENGTH;
					string text = TextConverter.BytesToPokemonString(MainForm.romData, num2, this.TRAINER_CLASS_NAME_LENGTH);
					this.cmbTrainerClassName.Items.Add(text);
				}
				this.cmbTrainerClassName.SelectedIndex = 0;
				this.LoadSelectedTrainerClassData(0);
			}
		}

		// Token: 0x06000CB2 RID: 3250 RVA: 0x0005EC0C File Offset: 0x0005CE0C
		private void cmbTrainerClassName_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.cmbTrainerClassName.SelectedIndex == this.currentTrainerClassIndex;
			if (!flag)
			{
				bool flag2 = this.isTrainerClassModified;
				if (flag2)
				{
					DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					bool flag3 = dialogResult == DialogResult.Yes;
					if (flag3)
					{
						this.SaveTrainerClassData();
					}
					else
					{
						bool flag4 = dialogResult == DialogResult.No;
						if (flag4)
						{
							this.isTrainerClassModified = false;
							this.btnSaveTrainerClass.Enabled = false;
							this.cmbTrainerClassName.Items[this.currentTrainerClassIndex] = this.originalTrainerClassName;
						}
						else
						{
							bool flag5 = dialogResult == DialogResult.Cancel;
							if (flag5)
							{
								this.cmbTrainerClassName.SelectedIndex = this.currentTrainerClassIndex;
								return;
							}
						}
					}
				}
				this.currentTrainerClassIndex = this.cmbTrainerClassName.SelectedIndex;
				this.LoadSelectedTrainerClassData(this.currentTrainerClassIndex);
			}
		}

		// Token: 0x06000CB3 RID: 3251 RVA: 0x0005ECE4 File Offset: 0x0005CEE4
		private void LoadSelectedTrainerClassData(int index)
		{
			string text = this.cmbTrainerClassName.Items[index].ToString();
			checked
			{
				int num = this.TRAINER_CLASS_NAME_TABLE_OFFSET + index * this.TRAINER_CLASS_NAME_LENGTH;
				this.originalTrainerClassName = TextConverter.BytesToPokemonString(MainForm.romData, num, this.TRAINER_CLASS_NAME_LENGTH);
				bool flag = Operators.CompareString(text, this.originalTrainerClassName, false) != 0;
				if (flag)
				{
					this.txtTrainerClassName.Text = text;
				}
				else
				{
					this.txtTrainerClassName.Text = this.originalTrainerClassName;
				}
				int num2 = this.FindPrizeMoneyEntry(index);
				bool flag2 = num2 == -1;
				if (flag2)
				{
					num2 = this.FindPrizeMoneyEntry(255);
				}
				bool flag3 = num2 != -1;
				if (flag3)
				{
					int num3 = this.PRIZE_MONEY_TABLE_OFFSET + num2 * this.PRIZE_MONEY_ENTRY_LENGTH;
					this.originalPrizeMoneyRate = MainForm.romData[num3 + 1];
					this.nudPrizeMoneyRate.Value = new decimal((int)this.originalPrizeMoneyRate);
				}
				else
				{
					this.originalPrizeMoneyRate = 0;
					this.nudPrizeMoneyRate.Value = 0m;
				}
				this.isTrainerClassModified = false;
				this.btnSaveTrainerClass.Enabled = false;
			}
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x0005EE00 File Offset: 0x0005D000
		private int FindPrizeMoneyEntry(int trainerClassIndex)
		{
			checked
			{
				int num = this.PRIZE_MONEY_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.PRIZE_MONEY_TABLE_OFFSET + i * this.PRIZE_MONEY_ENTRY_LENGTH;
					bool flag = (int)MainForm.romData[num2] == trainerClassIndex;
					if (flag)
					{
						return i;
					}
				}
				return -1;
			}
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x0005EE4C File Offset: 0x0005D04C
		private void btnChangeTrainerClassName_Click(object sender, EventArgs e)
		{
			string text = this.txtTrainerClassName.Text.Trim();
			checked
			{
				bool flag = text.Length > this.TRAINER_CLASS_NAME_LENGTH - 1;
				if (flag)
				{
					text = text.Substring(0, this.TRAINER_CLASS_NAME_LENGTH - 1);
					this.txtTrainerClassName.Text = text;
				}
				bool flag2 = Operators.CompareString(text, this.cmbTrainerClassName.Items[this.currentTrainerClassIndex].ToString(), false) == 0;
				if (!flag2)
				{
					this.cmbTrainerClassName.Items[this.currentTrainerClassIndex] = text;
					this.cmbTrainerClassName.Refresh();
					this.isTrainerClassModified = true;
					this.btnSaveTrainerClass.Enabled = true;
				}
			}
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x0005EF01 File Offset: 0x0005D101
		private void nudPrizeMoneyRate_ValueChanged(object sender, EventArgs e)
		{
			this.CheckTrainerClassChanges();
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x0005EF0C File Offset: 0x0005D10C
		private void CheckTrainerClassChanges()
		{
			string text = this.cmbTrainerClassName.Items[this.currentTrainerClassIndex].ToString();
			byte b = Convert.ToByte(this.nudPrizeMoneyRate.Value);
			this.isTrainerClassModified = Operators.CompareString(text, this.originalTrainerClassName, false) != 0 || b != this.originalPrizeMoneyRate;
			this.btnSaveTrainerClass.Enabled = this.isTrainerClassModified;
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0005EF7D File Offset: 0x0005D17D
		private void btnSaveTrainerClass_Click(object sender, EventArgs e)
		{
			this.SaveTrainerClassData();
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x0005EF88 File Offset: 0x0005D188
		private void SaveTrainerClassData()
		{
			string text = this.cmbTrainerClassName.Items[this.currentTrainerClassIndex].ToString();
			checked
			{
				int num = this.TRAINER_CLASS_NAME_TABLE_OFFSET + this.currentTrainerClassIndex * this.TRAINER_CLASS_NAME_LENGTH;
				int num2 = this.TRAINER_CLASS_NAME_LENGTH - 1;
				for (int i = 0; i <= num2; i++)
				{
					MainForm.romData[num + i] = 0;
				}
				byte[] array = TextConverter.PokemonStringToBytes(text, this.TRAINER_CLASS_NAME_LENGTH - 1);
				Array.Copy(array, 0, MainForm.romData, num, Math.Min(array.Length, this.TRAINER_CLASS_NAME_LENGTH));
				int num3 = this.FindPrizeMoneyEntry(this.currentTrainerClassIndex);
				bool flag = num3 == -1;
				if (flag)
				{
					num3 = this.FindPrizeMoneyEntry(255);
				}
				bool flag2 = num3 != -1;
				if (flag2)
				{
					int num4 = this.PRIZE_MONEY_TABLE_OFFSET + num3 * this.PRIZE_MONEY_ENTRY_LENGTH;
					MainForm.romData[num4 + 1] = Convert.ToByte(this.nudPrizeMoneyRate.Value);
				}
				this.originalTrainerClassName = text;
				this.originalPrizeMoneyRate = Convert.ToByte(this.nudPrizeMoneyRate.Value);
				this.isTrainerClassModified = false;
				this.btnSaveTrainerClass.Enabled = false;
			}
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x0005F0A8 File Offset: 0x0005D2A8
		private void TrainerSpriteEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.isTrainerSpriteModified;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveTrainerSprite();
				}
				else
				{
					bool flag3 = dialogResult == DialogResult.Cancel;
					if (flag3)
					{
						e.Cancel = true;
					}
				}
			}
			bool flag4 = this.isTrainerClassModified;
			if (flag4)
			{
				DialogResult dialogResult2 = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag5 = dialogResult2 == DialogResult.Yes;
				if (flag5)
				{
					this.SaveTrainerClassData();
				}
				else
				{
					bool flag6 = dialogResult2 == DialogResult.Cancel;
					if (flag6)
					{
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x040006E8 RID: 1768
		public readonly int TRAINER_SPRITE_TABLE_OFFSET;

		// Token: 0x040006E9 RID: 1769
		public readonly int TRAINER_PALETTE_TABLE_OFFSET;

		// Token: 0x040006EA RID: 1770
		public readonly int TRAINER_Y_POSITION_TABLE_OFFSET;

		// Token: 0x040006EB RID: 1771
		public readonly int TRAINER_ANIMATION_POINTER_TABLE_OFFSET;

		// Token: 0x040006EC RID: 1772
		public readonly int TRAINER_ANIMATION_DATA_TABLE_OFFSET;

		// Token: 0x040006ED RID: 1773
		public readonly int MAX_TRAINER_SPRITE_COUNT;

		// Token: 0x040006EE RID: 1774
		public readonly int TRAINER_CLASS_NAME_TABLE_OFFSET;

		// Token: 0x040006EF RID: 1775
		public readonly int TRAINER_CLASS_NAME_LENGTH;

		// Token: 0x040006F0 RID: 1776
		public readonly int TRAINER_CLASS_NAME_COUNT;

		// Token: 0x040006F1 RID: 1777
		public readonly int PRIZE_MONEY_TABLE_OFFSET;

		// Token: 0x040006F2 RID: 1778
		public readonly int PRIZE_MONEY_ENTRY_LENGTH;

		// Token: 0x040006F3 RID: 1779
		public readonly int PRIZE_MONEY_COUNT;

		// Token: 0x040006F4 RID: 1780
		private int currentIndex;

		// Token: 0x040006F5 RID: 1781
		private bool isTrainerSpriteModified;

		// Token: 0x040006F6 RID: 1782
		private bool isTrainerClassModified;

		// Token: 0x040006F7 RID: 1783
		private uint originalImageAddress;

		// Token: 0x040006F8 RID: 1784
		private uint originalPaletteAddress;

		// Token: 0x040006F9 RID: 1785
		private byte originalYPosition;

		// Token: 0x040006FA RID: 1786
		private byte[] temporaryImageData;

		// Token: 0x040006FB RID: 1787
		private byte[] temporaryPaletteData;

		// Token: 0x040006FC RID: 1788
		private int currentTrainerClassIndex;

		// Token: 0x040006FD RID: 1789
		private string originalTrainerClassName;

		// Token: 0x040006FE RID: 1790
		private byte originalPrizeMoneyRate;
	}
}
