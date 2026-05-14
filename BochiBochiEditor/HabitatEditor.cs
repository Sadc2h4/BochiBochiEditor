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
	// Token: 0x0200000E RID: 14
	public partial class HabitatEditor : Form
	{
		// Token: 0x060000D3 RID: 211 RVA: 0x00008AB0 File Offset: 0x00006CB0
		public HabitatEditor()
		{
			base.Load += this.HabitatEditor_Load;
			base.FormClosing += this.HabitatEditor_FormClosing;
			this.HABITAT_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("HABITAT_TABLE_OFFSET");
			this.HABITAT_ENTRY_COUNT = RomIniReader.ReadHexOrDecimal("HABITAT_ENTRY_COUNT");
			this.isDataModified = false;
			this.habitatList = new List<HabitatEditor.HabitatData>();
			this.pokemonDataList = new Dictionary<int, PokemonData>();
			this.InitializeComponent();
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00009CF9 File Offset: 0x00007EF9
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x00009D03 File Offset: 0x00007F03
		internal virtual GroupBox grpHabitat
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x00009D0C File Offset: 0x00007F0C
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x00009D18 File Offset: 0x00007F18
		internal virtual ListBox lstHabitat
		{
			[CompilerGenerated]
			get
			{
				return this._lstHabitat;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstHabitat_SelectedIndexChanged);
				ListBox listBox = this._lstHabitat;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstHabitat = value;
				listBox = this._lstHabitat;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000DA RID: 218 RVA: 0x00009D5B File Offset: 0x00007F5B
		// (set) Token: 0x060000DB RID: 219 RVA: 0x00009D65 File Offset: 0x00007F65
		internal virtual GroupBox grpPage
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00009D6E File Offset: 0x00007F6E
		// (set) Token: 0x060000DD RID: 221 RVA: 0x00009D78 File Offset: 0x00007F78
		internal virtual TextBox txtDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00009D81 File Offset: 0x00007F81
		// (set) Token: 0x060000DF RID: 223 RVA: 0x00009D8B File Offset: 0x00007F8B
		internal virtual Label lblDataAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00009D94 File Offset: 0x00007F94
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x00009D9E File Offset: 0x00007F9E
		internal virtual Label lblPageNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x00009DA7 File Offset: 0x00007FA7
		// (set) Token: 0x060000E3 RID: 227 RVA: 0x00009DB4 File Offset: 0x00007FB4
		internal virtual Button btnChangePageNum
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePageNum;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePageNum_Click);
				Button button = this._btnChangePageNum;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePageNum = value;
				button = this._btnChangePageNum;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00009DF7 File Offset: 0x00007FF7
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x00009E04 File Offset: 0x00008004
		internal virtual ListBox lstPage
		{
			[CompilerGenerated]
			get
			{
				return this._lstPage;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstPage_SelectedIndexChanged);
				ListBox listBox = this._lstPage;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstPage = value;
				listBox = this._lstPage;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x00009E47 File Offset: 0x00008047
		// (set) Token: 0x060000E7 RID: 231 RVA: 0x00009E54 File Offset: 0x00008054
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

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x00009E97 File Offset: 0x00008097
		// (set) Token: 0x060000E9 RID: 233 RVA: 0x00009EA1 File Offset: 0x000080A1
		internal virtual ComboBox cmbPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000EA RID: 234 RVA: 0x00009EAA File Offset: 0x000080AA
		// (set) Token: 0x060000EB RID: 235 RVA: 0x00009EB4 File Offset: 0x000080B4
		internal virtual NumericUpDown nudPokemonIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000EC RID: 236 RVA: 0x00009EBD File Offset: 0x000080BD
		// (set) Token: 0x060000ED RID: 237 RVA: 0x00009EC8 File Offset: 0x000080C8
		internal virtual Button btnChangePokemon
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokemon;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokemon_Click);
				Button button = this._btnChangePokemon;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokemon = value;
				button = this._btnChangePokemon;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000EE RID: 238 RVA: 0x00009F0B File Offset: 0x0000810B
		// (set) Token: 0x060000EF RID: 239 RVA: 0x00009F15 File Offset: 0x00008115
		internal virtual PictureBox picPokemon4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x00009F1E File Offset: 0x0000811E
		// (set) Token: 0x060000F1 RID: 241 RVA: 0x00009F28 File Offset: 0x00008128
		internal virtual PictureBox picPokemon3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x00009F31 File Offset: 0x00008131
		// (set) Token: 0x060000F3 RID: 243 RVA: 0x00009F3B File Offset: 0x0000813B
		internal virtual PictureBox picPokemon2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000F4 RID: 244 RVA: 0x00009F44 File Offset: 0x00008144
		// (set) Token: 0x060000F5 RID: 245 RVA: 0x00009F4E File Offset: 0x0000814E
		internal virtual PictureBox picPokemon1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x00009F57 File Offset: 0x00008157
		// (set) Token: 0x060000F7 RID: 247 RVA: 0x00009F61 File Offset: 0x00008161
		internal virtual Label lblPokemonIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x00009F6A File Offset: 0x0000816A
		// (set) Token: 0x060000F9 RID: 249 RVA: 0x00009F74 File Offset: 0x00008174
		internal virtual Label lblPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060000FA RID: 250 RVA: 0x00009F7D File Offset: 0x0000817D
		// (set) Token: 0x060000FB RID: 251 RVA: 0x00009F87 File Offset: 0x00008187
		internal virtual Label lblPageAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060000FC RID: 252 RVA: 0x00009F90 File Offset: 0x00008190
		// (set) Token: 0x060000FD RID: 253 RVA: 0x00009F9A File Offset: 0x0000819A
		internal virtual TextBox txtPageAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060000FE RID: 254 RVA: 0x00009FA3 File Offset: 0x000081A3
		// (set) Token: 0x060000FF RID: 255 RVA: 0x00009FAD File Offset: 0x000081AD
		internal virtual Label lblPokemonNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000100 RID: 256 RVA: 0x00009FB6 File Offset: 0x000081B6
		// (set) Token: 0x06000101 RID: 257 RVA: 0x00009FC0 File Offset: 0x000081C0
		internal virtual Button btnChangePokemonNum
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokemonNum;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokemonNum_Click);
				Button button = this._btnChangePokemonNum;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokemonNum = value;
				button = this._btnChangePokemonNum;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000102 RID: 258 RVA: 0x0000A003 File Offset: 0x00008203
		// (set) Token: 0x06000103 RID: 259 RVA: 0x0000A010 File Offset: 0x00008210
		internal virtual Button btnChangeDataAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeDataAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeDataAddress_Click);
				Button button = this._btnChangeDataAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeDataAddress = value;
				button = this._btnChangeDataAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000104 RID: 260 RVA: 0x0000A053 File Offset: 0x00008253
		// (set) Token: 0x06000105 RID: 261 RVA: 0x0000A060 File Offset: 0x00008260
		internal virtual Button btnChangePageAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePageAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePageAddress_Click);
				Button button = this._btnChangePageAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePageAddress = value;
				button = this._btnChangePageAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000106 RID: 262 RVA: 0x0000A0A3 File Offset: 0x000082A3
		// (set) Token: 0x06000107 RID: 263 RVA: 0x0000A0AD File Offset: 0x000082AD
		internal virtual GroupBox grpPokemon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000108 RID: 264 RVA: 0x0000A0B6 File Offset: 0x000082B6
		// (set) Token: 0x06000109 RID: 265 RVA: 0x0000A0C0 File Offset: 0x000082C0
		internal virtual Button btnCreateNewTable
		{
			[CompilerGenerated]
			get
			{
				return this._btnCreateNewTable;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnCreateNewTable_Click);
				Button button = this._btnCreateNewTable;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnCreateNewTable = value;
				button = this._btnCreateNewTable;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600010A RID: 266 RVA: 0x0000A103 File Offset: 0x00008303
		// (set) Token: 0x0600010B RID: 267 RVA: 0x0000A110 File Offset: 0x00008310
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

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600010C RID: 268 RVA: 0x0000A153 File Offset: 0x00008353
		// (set) Token: 0x0600010D RID: 269 RVA: 0x0000A15D File Offset: 0x0000835D
		internal virtual NumericUpDown nudPageNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600010E RID: 270 RVA: 0x0000A166 File Offset: 0x00008366
		// (set) Token: 0x0600010F RID: 271 RVA: 0x0000A170 File Offset: 0x00008370
		internal virtual NumericUpDown nudPokemonNum
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x0000A179 File Offset: 0x00008379
		private void HabitatEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.LoadAllPokemonData();
			this.InitializePokemonComboBox();
			this.LoadAllHabitatData();
			this.lstHabitat.SelectedIndex = 0;
			this.btnSave.Enabled = false;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x0000A1B8 File Offset: 0x000083B8
		private void LoadAllPokemonData()
		{
			this.pokemonDataList.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					string pokemonNameFromRom = this.GetPokemonNameFromRom(i);
					PokemonData pokemonData = new PokemonData(i, pokemonNameFromRom);
					this.LoadSpriteAddresses(pokemonData);
					this.pokemonDataList.Add(i, pokemonData);
				}
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0000A218 File Offset: 0x00008418
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
					bool flag = this.pokemonDataList.ContainsKey(i);
					if (flag)
					{
						this.cmbPokemonCode.Items.Add(this.pokemonDataList[i].Name);
					}
				}
				this.cmbPokemonCode.EndUpdate();
				this.cmbPokemonCode.SelectedIndex = 0;
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000A2C4 File Offset: 0x000084C4
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

		// Token: 0x06000114 RID: 276 RVA: 0x0000A348 File Offset: 0x00008548
		private void LoadSpriteAddresses(PokemonData pokemonData)
		{
			pokemonData.FrontImageAddress = this.ReadImageAddress(MyProject.Forms.PokemonEditor.FRONT_IMAGE_TABLE_OFFSET, pokemonData.Index);
			pokemonData.NormalPaletteAddress = this.ReadImageAddress(MyProject.Forms.PokemonEditor.NORMAL_PALETTE_TABLE_OFFSET, pokemonData.Index);
		}

		// Token: 0x06000115 RID: 277 RVA: 0x0000A39C File Offset: 0x0000859C
		private uint ReadImageAddress(int tableOffset, int entryIndex)
		{
			checked
			{
				int num = tableOffset + entryIndex * 8;
				uint num2 = BitConverter.ToUInt32(this.romData, num);
				return num2 - 134217728U;
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x0000A3C8 File Offset: 0x000085C8
		private void LoadAllHabitatData()
		{
			this.habitatList.Clear();
			checked
			{
				int num = this.HABITAT_ENTRY_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					HabitatEditor.HabitatData habitatData = new HabitatEditor.HabitatData();
					habitatData.Index = i;
					habitatData.Name = this.lstHabitat.Items[i].ToString();
					int num2 = this.HABITAT_TABLE_OFFSET + i * 8;
					uint num3 = BitConverter.ToUInt32(this.romData, num2);
					habitatData.PageTableAddress = num3 - 134217728U;
					habitatData.PageCount = (int)this.romData[num2 + 4];
					this.LoadPageDataForHabitat(habitatData);
					this.habitatList.Add(habitatData);
				}
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x0000A46C File Offset: 0x0000866C
		private void LoadPageDataForHabitat(HabitatEditor.HabitatData habitat)
		{
			habitat.Pages.Clear();
			checked
			{
				int num = habitat.PageCount - 1;
				for (int i = 0; i <= num; i++)
				{
					HabitatEditor.PageData pageData = new HabitatEditor.PageData();
					pageData.Index = i;
					int num2 = (int)(unchecked((ulong)habitat.PageTableAddress) + (ulong)(unchecked((long)(checked(i * 8)))));
					uint num3 = BitConverter.ToUInt32(this.romData, num2);
					pageData.PokemonListAddress = num3 - 134217728U;
					pageData.PokemonCount = (int)this.romData[num2 + 4];
					this.LoadPokemonIdsForPage(pageData);
					habitat.Pages.Add(pageData);
				}
			}
		}

		// Token: 0x06000118 RID: 280 RVA: 0x0000A4F8 File Offset: 0x000086F8
		private void LoadPokemonIdsForPage(HabitatEditor.PageData page)
		{
			page.PokemonIds.Clear();
			bool flag = page.PokemonCount == 0;
			checked
			{
				if (!flag)
				{
					int num = page.PokemonCount - 1;
					for (int i = 0; i <= num; i++)
					{
						int num2 = (int)(unchecked((ulong)page.PokemonListAddress) + (ulong)(unchecked((long)(checked(i * 2)))));
						ushort num3 = BitConverter.ToUInt16(this.romData, num2);
						page.PokemonIds.Add(num3);
					}
				}
			}
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0000A560 File Offset: 0x00008760
		private void lstHabitat_SelectedIndexChanged(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			HabitatEditor.HabitatData habitatData = this.habitatList[selectedIndex];
			this.txtPageAddress.Text = habitatData.PageTableAddress.ToString("X8");
			this.nudPageNum.Value = new decimal(habitatData.PageCount);
			this.UpdatePageList(habitatData);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x0000A5C4 File Offset: 0x000087C4
		private void UpdatePageList(HabitatEditor.HabitatData habitat)
		{
			this.lstPage.BeginUpdate();
			this.lstPage.Items.Clear();
			int pageCount = habitat.PageCount;
			checked
			{
				for (int i = 1; i <= pageCount; i++)
				{
					this.lstPage.Items.Add(string.Format("ページ {0}", i));
				}
				this.lstPage.EndUpdate();
				this.lstPage.SelectedIndex = 0;
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0000A63C File Offset: 0x0000883C
		private void lstPage_SelectedIndexChanged(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			int selectedIndex2 = this.lstPage.SelectedIndex;
			HabitatEditor.PageData pageData = this.habitatList[selectedIndex].Pages[selectedIndex2];
			this.txtDataAddress.Text = pageData.PokemonListAddress.ToString("X8");
			this.nudPokemonNum.Value = new decimal(pageData.PokemonCount);
			this.DisplayPokemonForPage(pageData);
		}

		// Token: 0x0600011C RID: 284 RVA: 0x0000A6B8 File Offset: 0x000088B8
		private void DisplayPokemonForPage(HabitatEditor.PageData page)
		{
			this.ClearPokemonInfo();
			checked
			{
				int num = page.PokemonCount - 1;
				for (int i = 0; i <= num; i++)
				{
					ushort num2 = page.PokemonIds[i];
					bool flag = this.pokemonDataList.ContainsKey((int)num2) && this.GetPictureBoxByIndex(i) != null;
					if (flag)
					{
						PokemonData pokemonData = this.pokemonDataList[(int)num2];
						ImageProcessor.DisplayGBASprite(this.GetPictureBoxByIndex(i), this.romData, pokemonData.FrontImageAddress, pokemonData.NormalPaletteAddress, 64, 64, true, true);
					}
				}
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x0000A748 File Offset: 0x00008948
		private PictureBox GetPictureBoxByIndex(int index)
		{
			PictureBox pictureBox;
			switch (index)
			{
			case 0:
				pictureBox = this.picPokemon1;
				break;
			case 1:
				pictureBox = this.picPokemon2;
				break;
			case 2:
				pictureBox = this.picPokemon3;
				break;
			case 3:
				pictureBox = this.picPokemon4;
				break;
			default:
				pictureBox = null;
				break;
			}
			return pictureBox;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x0000A79F File Offset: 0x0000899F
		private void ClearPageInfo()
		{
			this.txtDataAddress.Text = "000000";
			this.nudPokemonNum.Value = 0m;
		}

		// Token: 0x0600011F RID: 287 RVA: 0x0000A7C4 File Offset: 0x000089C4
		private void ClearPokemonInfo()
		{
			this.picPokemon1.Image = null;
			this.picPokemon2.Image = null;
			this.picPokemon3.Image = null;
			this.picPokemon4.Image = null;
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000A7FC File Offset: 0x000089FC
		private void btnChangePageAddress_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			HabitatEditor.HabitatData habitatData = this.habitatList[selectedIndex];
			uint num = Convert.ToUInt32(this.txtPageAddress.Text, 16);
			bool flag = num == habitatData.PageTableAddress;
			if (!flag)
			{
				habitatData.PageTableAddress = num;
				this.LoadPageDataForHabitat(habitatData);
				this.UpdatePageList(habitatData);
				this.isDataModified = true;
				this.btnSave.Enabled = true;
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x0000A870 File Offset: 0x00008A70
		private void btnChangePageNum_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			HabitatEditor.HabitatData habitatData = this.habitatList[selectedIndex];
			int num = Convert.ToInt32(this.nudPageNum.Value);
			bool flag = num == habitatData.PageCount;
			if (!flag)
			{
				bool flag2 = num > habitatData.Pages.Count;
				if (flag2)
				{
					MessageBox.Show("ページ数を増やす場合は、新しいテーブルを生成してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					this.nudPageNum.Value = new decimal(habitatData.Pages.Count);
				}
				else
				{
					habitatData.PageCount = num;
					this.UpdatePageList(habitatData);
					this.isDataModified = true;
					this.btnSave.Enabled = true;
				}
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x0000A920 File Offset: 0x00008B20
		private void btnChangeDataAddress_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			int selectedIndex2 = this.lstPage.SelectedIndex;
			HabitatEditor.PageData pageData = this.habitatList[selectedIndex].Pages[selectedIndex2];
			uint num = Convert.ToUInt32(this.txtDataAddress.Text, 16);
			bool flag = num == pageData.PokemonListAddress;
			if (!flag)
			{
				pageData.PokemonListAddress = num;
				this.LoadPokemonIdsForPage(pageData);
				this.DisplayPokemonForPage(pageData);
				this.isDataModified = true;
				this.btnSave.Enabled = true;
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000A9B0 File Offset: 0x00008BB0
		private void btnChangePokemonNum_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			int selectedIndex2 = this.lstPage.SelectedIndex;
			HabitatEditor.PageData pageData = this.habitatList[selectedIndex].Pages[selectedIndex2];
			int num = Convert.ToInt32(this.nudPokemonNum.Value);
			bool flag = num == pageData.PokemonCount;
			if (!flag)
			{
				bool flag2 = num > pageData.PokemonIds.Count;
				if (flag2)
				{
					MessageBox.Show("ポケモン数を増やす場合は、新しいデータを生成してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					this.nudPokemonNum.Value = new decimal(pageData.PokemonIds.Count);
				}
				else
				{
					pageData.PokemonCount = num;
					this.DisplayPokemonForPage(pageData);
					this.isDataModified = true;
					this.btnSave.Enabled = true;
				}
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0000AA7C File Offset: 0x00008C7C
		private void btnChangePokemon_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			int selectedIndex2 = this.lstPage.SelectedIndex;
			HabitatEditor.PageData pageData = this.habitatList[selectedIndex].Pages[selectedIndex2];
			checked
			{
				int num = Convert.ToInt32(this.nudPokemonIndex.Value) - 1;
				bool flag = num < 0 || num >= pageData.PokemonCount;
				if (flag)
				{
					MessageBox.Show("無効なポケモン番号です。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					ushort num2 = (ushort)this.cmbPokemonCode.SelectedIndex;
					ushort num3 = pageData.PokemonIds[num];
					bool flag2 = num2 == num3;
					if (!flag2)
					{
						pageData.PokemonIds[num] = num2;
						this.DisplayPokemonForPage(pageData);
						this.isDataModified = true;
						this.btnSave.Enabled = true;
					}
				}
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0000AB51 File Offset: 0x00008D51
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveAllHabitatData();
			MainForm.romData = this.romData;
			this.isDataModified = false;
			this.btnSave.Enabled = false;
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000AB7C File Offset: 0x00008D7C
		private void SaveAllHabitatData()
		{
			checked
			{
				{
					foreach (HabitatEditor.HabitatData habitatData in this.habitatList)
					{
						int num = this.HABITAT_TABLE_OFFSET + habitatData.Index * 8;
						byte[] bytes = BitConverter.GetBytes(habitatData.PageTableAddress + 134217728U);
						Array.Copy(bytes, 0, this.romData, num, 4);
						this.romData[num + 4] = (byte)habitatData.PageCount;
						int num2 = habitatData.PageCount - 1;
						for (int i = 0; i <= num2; i++)
						{
							int num3 = (int)(unchecked((ulong)habitatData.PageTableAddress) + (ulong)(unchecked((long)(checked(i * 8)))));
							byte[] bytes2 = BitConverter.GetBytes(134217728U);
							Array.Copy(bytes2, 0, this.romData, num3, 4);
							this.romData[num3 + 4] = 0;
							this.romData[num3 + 5] = 0;
							this.romData[num3 + 6] = 0;
							this.romData[num3 + 7] = 0;
						}
						foreach (HabitatEditor.PageData pageData in habitatData.Pages)
						{
							this.SavePageData(pageData);
						}
					}
				}
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0000ACF8 File Offset: 0x00008EF8
		private void SavePageData(HabitatEditor.PageData page)
		{
			HabitatEditor.HabitatData habitatData = null;
			{
				foreach (HabitatEditor.HabitatData habitatData2 in this.habitatList)
				{
					bool flag = habitatData2.Pages.Contains(page);
					if (flag)
					{
						habitatData = habitatData2;
						break;
					}
				}
			}
			bool flag2 = habitatData == null;
			checked
			{
				if (!flag2)
				{
					int num = (int)(unchecked((ulong)habitatData.PageTableAddress) + (ulong)(unchecked((long)(checked(page.Index * 8)))));
					byte[] bytes = BitConverter.GetBytes(page.PokemonListAddress + 134217728U);
					Array.Copy(bytes, 0, this.romData, num, 4);
					this.romData[num + 4] = (byte)page.PokemonCount;
					int num2 = page.PokemonCount - 1;
					for (int i = 0; i <= num2; i++)
					{
						bool flag3 = i < page.PokemonIds.Count;
						if (flag3)
						{
							int num3 = (int)(unchecked((ulong)page.PokemonListAddress) + (ulong)(unchecked((long)(checked(i * 2)))));
							byte[] bytes2 = BitConverter.GetBytes(page.PokemonIds[i]);
							Array.Copy(bytes2, 0, this.romData, num3, 2);
						}
					}
				}
			}
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0000AE24 File Offset: 0x00009024
		private void HabitatEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.isDataModified;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Cancel)
				{
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult == DialogResult.No)
						{
							e.Cancel = false;
						}
					}
					else
					{
						this.SaveAllHabitatData();
						MainForm.romData = this.romData;
						this.isDataModified = false;
						this.btnSave.Enabled = false;
						e.Cancel = false;
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000AEB0 File Offset: 0x000090B0
		private void btnCreateNewTable_Click(object sender, EventArgs e)
		{
			using (InsertNewPokedexTable insertNewPokedexTable = new InsertNewPokedexTable())
			{
				bool flag = insertNewPokedexTable.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					this.CreateNewPageTable(insertNewPokedexTable.NewTableAddress, insertNewPokedexTable.NewPageCount);
				}
			}
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000AF08 File Offset: 0x00009108
		private void btnCreateNewData_Click(object sender, EventArgs e)
		{
			using (InsertNewPokedexData insertNewPokedexData = new InsertNewPokedexData())
			{
				bool flag = insertNewPokedexData.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					this.CreateNewPokemonData(insertNewPokedexData.NewDataAddress, insertNewPokedexData.NewPokemonCount);
				}
			}
		}

		// Token: 0x0600012B RID: 299 RVA: 0x0000AF60 File Offset: 0x00009160
		private void CreateNewPageTable(uint tableAddress, int pageCount)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			HabitatEditor.HabitatData habitatData = this.habitatList[selectedIndex];
			habitatData.Pages.Clear();
			checked
			{
				int num = pageCount - 1;
				for (int i = 0; i <= num; i++)
				{
					HabitatEditor.PageData pageData = new HabitatEditor.PageData();
					pageData.Index = i;
					pageData.PokemonListAddress = 0U;
					pageData.PokemonCount = 0;
					pageData.PokemonIds.Clear();
					habitatData.Pages.Add(pageData);
				}
				habitatData.PageTableAddress = tableAddress;
				habitatData.PageCount = pageCount;
				this.txtPageAddress.Text = tableAddress.ToString("X6");
				this.nudPageNum.Value = new decimal(pageCount);
				this.UpdatePageList(habitatData);
				this.isDataModified = true;
				this.btnSave.Enabled = true;
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0000B034 File Offset: 0x00009234
		private void CreateNewPokemonData(uint dataAddress, int pokemonCount)
		{
			int selectedIndex = this.lstHabitat.SelectedIndex;
			int selectedIndex2 = this.lstPage.SelectedIndex;
			HabitatEditor.PageData pageData = this.habitatList[selectedIndex].Pages[selectedIndex2];
			pageData.PokemonListAddress = dataAddress;
			pageData.PokemonCount = pokemonCount;
			pageData.PokemonIds.Clear();
			checked
			{
				for (int i = 1; i <= pokemonCount; i++)
				{
					pageData.PokemonIds.Add(0);
				}
				this.txtDataAddress.Text = dataAddress.ToString("X6");
				this.nudPokemonNum.Value = new decimal(pokemonCount);
				this.DisplayPokemonForPage(pageData);
				this.isDataModified = true;
				this.btnSave.Enabled = true;
			}
		}

		// Token: 0x04000095 RID: 149
		public readonly int HABITAT_TABLE_OFFSET;

		// Token: 0x04000096 RID: 150
		public readonly int HABITAT_ENTRY_COUNT;

		// Token: 0x04000097 RID: 151
		public const int HABITAT_ENTRY_LENGTH = 8;

		// Token: 0x04000098 RID: 152
		public const int PAGE_ENTRY_LENGTH = 8;

		// Token: 0x04000099 RID: 153
		private byte[] romData;

		// Token: 0x0400009A RID: 154
		private bool isDataModified;

		// Token: 0x0400009B RID: 155
		private List<HabitatEditor.HabitatData> habitatList;

		// Token: 0x0400009C RID: 156
		private Dictionary<int, PokemonData> pokemonDataList;

		// Token: 0x02000034 RID: 52
		public class HabitatData
		{
			// Token: 0x06000EC6 RID: 3782 RVA: 0x0006ABE0 File Offset: 0x00068DE0
			public HabitatData()
			{
				this.Pages = new List<HabitatEditor.PageData>();
			}

			// Token: 0x0400080E RID: 2062
			public int Index;

			// Token: 0x0400080F RID: 2063
			public string Name;

			// Token: 0x04000810 RID: 2064
			public uint PageTableAddress;

			// Token: 0x04000811 RID: 2065
			public int PageCount;

			// Token: 0x04000812 RID: 2066
			public List<HabitatEditor.PageData> Pages;
		}

		// Token: 0x02000035 RID: 53
		public class PageData
		{
			// Token: 0x06000EC7 RID: 3783 RVA: 0x0006ABF4 File Offset: 0x00068DF4
			public PageData()
			{
				this.PokemonIds = new List<ushort>();
			}

			// Token: 0x04000813 RID: 2067
			public int Index;

			// Token: 0x04000814 RID: 2068
			public uint PokemonListAddress;

			// Token: 0x04000815 RID: 2069
			public int PokemonCount;

			// Token: 0x04000816 RID: 2070
			public List<ushort> PokemonIds;
		}
	}
}
