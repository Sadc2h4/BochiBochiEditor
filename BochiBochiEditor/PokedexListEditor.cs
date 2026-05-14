using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x0200001E RID: 30
	public partial class PokedexListEditor : Form
	{
		// Token: 0x17000310 RID: 784
		// (get) Token: 0x0600083E RID: 2110 RVA: 0x000412FD File Offset: 0x0003F4FD
		// (set) Token: 0x0600083F RID: 2111 RVA: 0x00041308 File Offset: 0x0003F508
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

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000840 RID: 2112 RVA: 0x0004134B File Offset: 0x0003F54B
		// (set) Token: 0x06000841 RID: 2113 RVA: 0x00041355 File Offset: 0x0003F555
		internal virtual ListBox lstPokedexListAiueo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000842 RID: 2114 RVA: 0x0004135E File Offset: 0x0003F55E
		// (set) Token: 0x06000843 RID: 2115 RVA: 0x00041368 File Offset: 0x0003F568
		internal virtual GroupBox grpPokedexListAiueo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000844 RID: 2116 RVA: 0x00041371 File Offset: 0x0003F571
		// (set) Token: 0x06000845 RID: 2117 RVA: 0x0004137B File Offset: 0x0003F57B
		internal virtual ComboBox cmbPokedexListAiueo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000846 RID: 2118 RVA: 0x00041384 File Offset: 0x0003F584
		// (set) Token: 0x06000847 RID: 2119 RVA: 0x0004138E File Offset: 0x0003F58E
		internal virtual TextBox txtPokedexListAiueoPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000848 RID: 2120 RVA: 0x00041397 File Offset: 0x0003F597
		// (set) Token: 0x06000849 RID: 2121 RVA: 0x000413A1 File Offset: 0x0003F5A1
		internal virtual PictureBox picPokedexListAiueoIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x0600084A RID: 2122 RVA: 0x000413AA File Offset: 0x0003F5AA
		// (set) Token: 0x0600084B RID: 2123 RVA: 0x000413B4 File Offset: 0x0003F5B4
		internal virtual Button btnChangePokemon1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x0600084C RID: 2124 RVA: 0x000413BD File Offset: 0x0003F5BD
		// (set) Token: 0x0600084D RID: 2125 RVA: 0x000413C7 File Offset: 0x0003F5C7
		internal virtual GroupBox gtpPokedexListType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x0600084E RID: 2126 RVA: 0x000413D0 File Offset: 0x0003F5D0
		// (set) Token: 0x0600084F RID: 2127 RVA: 0x000413DA File Offset: 0x0003F5DA
		internal virtual Button btnChangePokemon2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06000850 RID: 2128 RVA: 0x000413E3 File Offset: 0x0003F5E3
		// (set) Token: 0x06000851 RID: 2129 RVA: 0x000413ED File Offset: 0x0003F5ED
		internal virtual TextBox txtPokedexListTypePokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06000852 RID: 2130 RVA: 0x000413F6 File Offset: 0x0003F5F6
		// (set) Token: 0x06000853 RID: 2131 RVA: 0x00041400 File Offset: 0x0003F600
		internal virtual PictureBox picPokedexListTypeIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06000854 RID: 2132 RVA: 0x00041409 File Offset: 0x0003F609
		// (set) Token: 0x06000855 RID: 2133 RVA: 0x00041413 File Offset: 0x0003F613
		internal virtual ComboBox cmbPokedexListType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06000856 RID: 2134 RVA: 0x0004141C File Offset: 0x0003F61C
		// (set) Token: 0x06000857 RID: 2135 RVA: 0x00041426 File Offset: 0x0003F626
		internal virtual ListBox lstPokedexListType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000858 RID: 2136 RVA: 0x0004142F File Offset: 0x0003F62F
		// (set) Token: 0x06000859 RID: 2137 RVA: 0x00041439 File Offset: 0x0003F639
		internal virtual GroupBox gtpPokedexListLight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x0600085A RID: 2138 RVA: 0x00041442 File Offset: 0x0003F642
		// (set) Token: 0x0600085B RID: 2139 RVA: 0x0004144C File Offset: 0x0003F64C
		internal virtual Button btnChangePokemon3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x0600085C RID: 2140 RVA: 0x00041455 File Offset: 0x0003F655
		// (set) Token: 0x0600085D RID: 2141 RVA: 0x0004145F File Offset: 0x0003F65F
		internal virtual TextBox txtPokedexListLightPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x0600085E RID: 2142 RVA: 0x00041468 File Offset: 0x0003F668
		// (set) Token: 0x0600085F RID: 2143 RVA: 0x00041472 File Offset: 0x0003F672
		internal virtual PictureBox picPokedexListLightIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000860 RID: 2144 RVA: 0x0004147B File Offset: 0x0003F67B
		// (set) Token: 0x06000861 RID: 2145 RVA: 0x00041485 File Offset: 0x0003F685
		internal virtual ComboBox cmbPokedexListLight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000862 RID: 2146 RVA: 0x0004148E File Offset: 0x0003F68E
		// (set) Token: 0x06000863 RID: 2147 RVA: 0x00041498 File Offset: 0x0003F698
		internal virtual ListBox lstPokedexListLight
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000864 RID: 2148 RVA: 0x000414A1 File Offset: 0x0003F6A1
		// (set) Token: 0x06000865 RID: 2149 RVA: 0x000414AB File Offset: 0x0003F6AB
		internal virtual GroupBox gtpPokedexListSmall
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06000866 RID: 2150 RVA: 0x000414B4 File Offset: 0x0003F6B4
		// (set) Token: 0x06000867 RID: 2151 RVA: 0x000414BE File Offset: 0x0003F6BE
		internal virtual Button btnChangePokemon4
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06000868 RID: 2152 RVA: 0x000414C7 File Offset: 0x0003F6C7
		// (set) Token: 0x06000869 RID: 2153 RVA: 0x000414D1 File Offset: 0x0003F6D1
		internal virtual TextBox txtPokedexListSmallPokemonCode
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x0600086A RID: 2154 RVA: 0x000414DA File Offset: 0x0003F6DA
		// (set) Token: 0x0600086B RID: 2155 RVA: 0x000414E4 File Offset: 0x0003F6E4
		internal virtual PictureBox picPokedexListSmallIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x0600086C RID: 2156 RVA: 0x000414ED File Offset: 0x0003F6ED
		// (set) Token: 0x0600086D RID: 2157 RVA: 0x000414F7 File Offset: 0x0003F6F7
		internal virtual ComboBox cmbPokedexListSmall
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x0600086E RID: 2158 RVA: 0x00041500 File Offset: 0x0003F700
		// (set) Token: 0x0600086F RID: 2159 RVA: 0x0004150A File Offset: 0x0003F70A
		internal virtual ListBox lstPokedexListSmall
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00041514 File Offset: 0x0003F714
		public PokedexListEditor()
		{
			base.Load += this.PokedexListEditor_Load;
			base.FormClosing += this.PokedexListEditor_FormClosing;
			this.AIUEO_ORDER_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("AIUEO_ORDER_TABLE_OFFSET");
			this.AIUEO_ORDER_LIST_COUNT = RomIniReader.ReadHexOrDecimal("AIUEO_ORDER_LIST_COUNT");
			this.LIGHT_ORDER_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("LIGHT_ORDER_TABLE_OFFSET");
			this.LIGHT_ORDER_LIST_COUNT = RomIniReader.ReadHexOrDecimal("LIGHT_ORDER_LIST_COUNT");
			this.SMALL_ORDER_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("SMALL_ORDER_TABLE_OFFSET");
			this.SMALL_ORDER_LIST_COUNT = RomIniReader.ReadHexOrDecimal("SMALL_ORDER_LIST_COUNT");
			this.TYPE_ORDER_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("TYPE_ORDER_TABLE_OFFSET");
			this.TYPE_ORDER_LIST_COUNT = RomIniReader.ReadHexOrDecimal("TYPE_ORDER_LIST_COUNT");
			this.ENABLE_POKEMONCODE_INDEXED = RomIniReader.ReadBoolean("ENABLE_POKEMONCODE_INDEXED");
			this.hasUnsavedChanges = false;
			this.pokemonDataList = new Dictionary<int, PokemonData>();
			this.pokedexOrderToPokemonCodeMap = new Dictionary<int, int>();
			this.uiGroups = new Dictionary<string, PokedexListEditor.PokedexListGroup>();
			this.aiueoOrderList = new List<int>();
			this.lightOrderList = new List<int>();
			this.smallOrderList = new List<int>();
			this.typeOrderList = new List<int>();
			this.InitializeComponent();
			this.romData = MainForm.romData;
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00041645 File Offset: 0x0003F845
		private void PokedexListEditor_Load(object sender, EventArgs e)
		{
			this.btnSave.Enabled = false;
			this.hasUnsavedChanges = false;
			this.LoadAllPokemonData();
			this.InitializeUiGroups();
			this.PopulateAllPokemonComboBoxes();
			this.LoadAllSortedLists();
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x00041678 File Offset: 0x0003F878
		private void LoadAllPokemonData()
		{
			this.pokemonDataList.Clear();
			this.pokedexOrderToPokemonCodeMap.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					string pokemonNameFromRom = this.GetPokemonNameFromRom(i, MyProject.Forms.PokemonEditor.POKEMON_NAME_OFFSET, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH);
					PokemonData pokemonData = new PokemonData(i, pokemonNameFromRom);
					int num2 = MyProject.Forms.PokedexOrderEditor.POKEDEX_ORDER_TABLE_OFFSET + (i - 1) * MyProject.Forms.PokedexOrderEditor.POKEDEX_ORDER_ENTRY_LENGTH;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					pokemonData.PokedexOrder = (int)num3;
					bool flag = !this.pokedexOrderToPokemonCodeMap.ContainsKey((int)num3);
					if (flag)
					{
						this.pokedexOrderToPokemonCodeMap.Add((int)num3, i);
					}
					this.pokemonDataList.Add(i, pokemonData);
				}
			}
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x00041764 File Offset: 0x0003F964
		private void InitializeUiGroups()
		{
			this.uiGroups.Clear();
			this.uiGroups.Add("Aiueo", new PokedexListEditor.PokedexListGroup(this.lstPokedexListAiueo, this.cmbPokedexListAiueo, this.picPokedexListAiueoIcon, this.txtPokedexListAiueoPokemonCode, this.btnChangePokemon1));
			this.uiGroups.Add("Type", new PokedexListEditor.PokedexListGroup(this.lstPokedexListType, this.cmbPokedexListType, this.picPokedexListTypeIcon, this.txtPokedexListTypePokemonCode, this.btnChangePokemon2));
			this.uiGroups.Add("Light", new PokedexListEditor.PokedexListGroup(this.lstPokedexListLight, this.cmbPokedexListLight, this.picPokedexListLightIcon, this.txtPokedexListLightPokemonCode, this.btnChangePokemon3));
			this.uiGroups.Add("Small", new PokedexListEditor.PokedexListGroup(this.lstPokedexListSmall, this.cmbPokedexListSmall, this.picPokedexListSmallIcon, this.txtPokedexListSmallPokemonCode, this.btnChangePokemon4));
			{
				foreach (PokedexListEditor.PokedexListGroup pokedexListGroup in this.uiGroups.Values)
				{
					pokedexListGroup.ListBox.SelectedIndexChanged += this.OnPokedexListSelectionChanged;
					pokedexListGroup.ChangeButton.Click += this.OnChangePokemonButtonClicked;
				}
			}
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x000418C8 File Offset: 0x0003FAC8
		private void PopulateAllPokemonComboBoxes()
		{
			string[] array = this.pokemonDataList.Values.OrderBy((PokemonData p) => p.Index).Select((PokemonData p) => p.Name).ToArray<string>();
			{
				foreach (PokedexListEditor.PokedexListGroup pokedexListGroup in this.uiGroups.Values)
				{
					pokedexListGroup.ComboBox.BeginUpdate();
					pokedexListGroup.ComboBox.Items.Clear();
					pokedexListGroup.ComboBox.Items.AddRange(array);
					pokedexListGroup.ComboBox.EndUpdate();
				}
			}
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x000419BC File Offset: 0x0003FBBC
		private void LoadAllSortedLists()
		{
			this.LoadPokedexListAiueo();
			this.LoadPokedexListType();
			this.LoadPokedexListLight();
			this.LoadPokedexListSmall();
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x000419DC File Offset: 0x0003FBDC
		private void LoadPokedexListAiueo()
		{
			ListBox listBox = this.uiGroups["Aiueo"].ListBox;
			listBox.Items.Clear();
			this.aiueoOrderList.Clear();
			listBox.BeginUpdate();
			checked
			{
				int num = this.AIUEO_ORDER_LIST_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.AIUEO_ORDER_TABLE_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					bool enable_POKEMONCODE_INDEXED = this.ENABLE_POKEMONCODE_INDEXED;
					if (enable_POKEMONCODE_INDEXED)
					{
						int num4 = (int)num3;
						this.aiueoOrderList.Add(num4);
						PokemonData pokemonData = this.pokemonDataList[num4];
						listBox.Items.Add(string.Format("No.{0:D3} : {1}", pokemonData.PokedexOrder, pokemonData.Name));
					}
					else
					{
						int num5 = (int)num3;
						this.aiueoOrderList.Add(num5);
						int num6 = this.pokedexOrderToPokemonCodeMap[num5];
						PokemonData pokemonData2 = this.pokemonDataList[num6];
						listBox.Items.Add(string.Format("No.{0:D3} : {1}", num5, pokemonData2.Name));
					}
				}
				listBox.EndUpdate();
				listBox.SelectedIndex = 0;
			}
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x00041B14 File Offset: 0x0003FD14
		private void LoadPokedexListType()
		{
			ListBox listBox = this.uiGroups["Type"].ListBox;
			listBox.Items.Clear();
			this.typeOrderList.Clear();
			listBox.BeginUpdate();
			checked
			{
				int num = this.TYPE_ORDER_LIST_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.TYPE_ORDER_TABLE_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					this.typeOrderList.Add((int)num3);
					PokemonData pokemonData = this.pokemonDataList[(int)num3];
					listBox.Items.Add(string.Format("No.{0:D3} : {1}", pokemonData.PokedexOrder, pokemonData.Name));
				}
				listBox.EndUpdate();
				listBox.SelectedIndex = 0;
			}
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x00041BDC File Offset: 0x0003FDDC
		private void LoadPokedexListLight()
		{
			ListBox listBox = this.uiGroups["Light"].ListBox;
			listBox.Items.Clear();
			this.lightOrderList.Clear();
			listBox.BeginUpdate();
			checked
			{
				int num = this.LIGHT_ORDER_LIST_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.LIGHT_ORDER_TABLE_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					bool enable_POKEMONCODE_INDEXED = this.ENABLE_POKEMONCODE_INDEXED;
					if (enable_POKEMONCODE_INDEXED)
					{
						int num4 = (int)num3;
						this.lightOrderList.Add(num4);
						PokemonData pokemonData = this.pokemonDataList[num4];
						listBox.Items.Add(string.Format("No.{0:D3} : {1}", pokemonData.PokedexOrder, pokemonData.Name));
					}
					else
					{
						int num5 = (int)num3;
						this.lightOrderList.Add(num5);
						int num6 = this.pokedexOrderToPokemonCodeMap[num5];
						PokemonData pokemonData2 = this.pokemonDataList[num6];
						listBox.Items.Add(string.Format("No.{0:D3} : {1}", num5, pokemonData2.Name));
					}
				}
				listBox.EndUpdate();
				listBox.SelectedIndex = 0;
			}
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x00041D14 File Offset: 0x0003FF14
		private void LoadPokedexListSmall()
		{
			ListBox listBox = this.uiGroups["Small"].ListBox;
			listBox.Items.Clear();
			this.smallOrderList.Clear();
			listBox.BeginUpdate();
			checked
			{
				int num = this.SMALL_ORDER_LIST_COUNT - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.SMALL_ORDER_TABLE_OFFSET + i * 2;
					ushort num3 = BitConverter.ToUInt16(this.romData, num2);
					bool enable_POKEMONCODE_INDEXED = this.ENABLE_POKEMONCODE_INDEXED;
					if (enable_POKEMONCODE_INDEXED)
					{
						int num4 = (int)num3;
						this.smallOrderList.Add(num4);
						PokemonData pokemonData = this.pokemonDataList[num4];
						listBox.Items.Add(string.Format("No.{0:D3} : {1}", pokemonData.PokedexOrder, pokemonData.Name));
					}
					else
					{
						int num5 = (int)num3;
						this.smallOrderList.Add(num5);
						int num6 = this.pokedexOrderToPokemonCodeMap[num5];
						PokemonData pokemonData2 = this.pokemonDataList[num6];
						listBox.Items.Add(string.Format("No.{0:D3} : {1}", num5, pokemonData2.Name));
					}
				}
				listBox.EndUpdate();
				listBox.SelectedIndex = 0;
			}
		}

		// Token: 0x0600087A RID: 2170 RVA: 0x00041E4C File Offset: 0x0004004C
		private void OnPokedexListSelectionChanged(object sender, EventArgs e)
		{
			ListBox listBox = sender as ListBox;
			PokedexListEditor.PokedexListGroup pokedexListGroup = this.uiGroups.Values.FirstOrDefault((PokedexListEditor.PokedexListGroup g) => g.ListBox == listBox);
			bool flag = pokedexListGroup == null || listBox.SelectedItem == null;
			if (!flag)
			{
				string text = listBox.SelectedItem.ToString();
				int num = text.IndexOf("No.");
				int num2 = text.IndexOf(':', num);
				string text2 = checked(text.Substring(num + 3, num2 - num - 3)).Trim();
				int num3;
				bool flag2 = !int.TryParse(text2, out num3);
				if (!flag2)
				{
					bool flag3 = this.pokedexOrderToPokemonCodeMap.ContainsKey(num3);
					if (flag3)
					{
						int num4 = this.pokedexOrderToPokemonCodeMap[num3];
						PokemonData pokemonData = this.pokemonDataList[num4];
						this.UpdateSelectedPokemonInfo(pokedexListGroup, pokemonData);
					}
				}
			}
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x00041F38 File Offset: 0x00040138
		private void OnChangePokemonButtonClicked(object sender, EventArgs e)
		{
			Button button = sender as Button;
			bool flag = button == null;
			if (!flag)
			{
				PokedexListEditor.PokedexListGroup pokedexListGroup = this.uiGroups.Values.FirstOrDefault((PokedexListEditor.PokedexListGroup g) => g.ChangeButton == button);
				bool flag2 = pokedexListGroup == null;
				if (!flag2)
				{
					int selectedIndex = pokedexListGroup.ListBox.SelectedIndex;
					bool flag3 = selectedIndex == -1;
					if (!flag3)
					{
						object selectedItem = pokedexListGroup.ComboBox.SelectedItem;
						string selectedPokemonInCombo = ((selectedItem != null) ? selectedItem.ToString() : null);
						bool flag4 = string.IsNullOrEmpty(selectedPokemonInCombo);
						if (!flag4)
						{
							PokemonData pokemonData = this.pokemonDataList.Values.FirstOrDefault((PokemonData p) => Operators.CompareString(p.Name, selectedPokemonInCombo, false) == 0);
							bool flag5 = pokemonData == null;
							if (!flag5)
							{
								this.UpdatePokedexList(pokedexListGroup, selectedIndex, pokemonData);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x00042010 File Offset: 0x00040210
		private void UpdatePokedexList(PokedexListEditor.PokedexListGroup group, int index, PokemonData newPokemon)
		{
			bool flag = group.ListBox == this.lstPokedexListAiueo;
			if (flag)
			{
				int num = this.aiueoOrderList[index];
				int pokedexOrder = newPokemon.PokedexOrder;
				bool flag2 = num != pokedexOrder;
				if (flag2)
				{
					this.aiueoOrderList[index] = pokedexOrder;
					group.ListBox.Items[index] = string.Format("No.{0:D3} : {1}", pokedexOrder, newPokemon.Name);
					this.hasUnsavedChanges = true;
					this.btnSave.Enabled = true;
				}
			}
			else
			{
				bool flag3 = group.ListBox == this.lstPokedexListType;
				if (flag3)
				{
					int num2 = this.typeOrderList[index];
					int index2 = newPokemon.Index;
					bool flag4 = num2 != index2;
					if (flag4)
					{
						this.typeOrderList[index] = index2;
						group.ListBox.Items[index] = string.Format("No.{0:D3} : {1}", newPokemon.PokedexOrder, newPokemon.Name);
						this.hasUnsavedChanges = true;
						this.btnSave.Enabled = true;
					}
				}
				else
				{
					bool flag5 = group.ListBox == this.lstPokedexListLight;
					if (flag5)
					{
						int num3 = this.lightOrderList[index];
						int pokedexOrder2 = newPokemon.PokedexOrder;
						bool flag6 = num3 != pokedexOrder2;
						if (flag6)
						{
							this.lightOrderList[index] = pokedexOrder2;
							group.ListBox.Items[index] = string.Format("No.{0:D3} : {1}", pokedexOrder2, newPokemon.Name);
							this.hasUnsavedChanges = true;
							this.btnSave.Enabled = true;
						}
					}
					else
					{
						bool flag7 = group.ListBox == this.lstPokedexListSmall;
						if (flag7)
						{
							int num4 = this.smallOrderList[index];
							int pokedexOrder3 = newPokemon.PokedexOrder;
							bool flag8 = num4 != pokedexOrder3;
							if (flag8)
							{
								this.smallOrderList[index] = pokedexOrder3;
								group.ListBox.Items[index] = string.Format("No.{0:D3} : {1}", pokedexOrder3, newPokemon.Name);
								this.hasUnsavedChanges = true;
								this.btnSave.Enabled = true;
							}
						}
					}
				}
			}
			this.UpdateSelectedPokemonInfo(group, newPokemon);
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x00042250 File Offset: 0x00040450
		private void UpdateSelectedPokemonInfo(PokedexListEditor.PokedexListGroup group, PokemonData pokemonData)
		{
			group.ComboBox.SelectedItem = pokemonData.Name;
			group.PokemonCodeTextBox.Text = string.Format("ポケモンコード : {0:X4}", pokemonData.Index);
			this.DisplayPokemonIcon(pokemonData.Index, group.IconPictureBox);
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x000422A4 File Offset: 0x000404A4
		private void DisplayPokemonIcon(int pokemonCode, PictureBox pictureBox)
		{
			uint num3;
			Color[] array2;
			byte[] array3;
			checked
			{
				int num = MyProject.Forms.PokemonEditor.ICON_IMAGE_TABLE_OFFSET + pokemonCode * 4;
				uint num2 = BitConverter.ToUInt32(this.romData, num);
				num3 = num2 - 134217728U;
				int num4 = MyProject.Forms.PokemonEditor.ICON_PALETTE_ID_TABLE_OFFSET + pokemonCode;
				int num5 = (int)this.romData[num4];
				byte[] array = this.LoadIconPalette(num5);
				array2 = ImageProcessor.LoadPalette(array, true);
				array3 = new byte[2048];
			}
			Array.Copy(this.romData, (long)((ulong)num3), array3, 0L, (long)array3.Length);
			Bitmap bitmap = ImageProcessor.LoadSprite(ref array3, array2, 32, 32, false);
			pictureBox.Image = bitmap;
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x00042348 File Offset: 0x00040548
		private byte[] LoadIconPalette(int paletteId)
		{
			uint num3;
			byte[] array;
			checked
			{
				int num = MyProject.Forms.PokemonEditor.ICON_PALETTE_TABLE_OFFSET + paletteId * 8;
				uint num2 = BitConverter.ToUInt32(this.romData, num);
				num3 = num2 - 134217728U;
				array = new byte[32];
			}
			Array.Copy(this.romData, (long)((ulong)num3), array, 0L, 32L);
			return array;
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x000423A4 File Offset: 0x000405A4
		private string GetPokemonNameFromRom(int pokemonIndex, int nameOffset, int nameLength)
		{
			checked
			{
				int num = nameOffset + pokemonIndex * nameLength;
				byte[] array = new byte[nameLength - 1 + 1];
				Array.Copy(this.romData, num, array, 0, nameLength);
				return TextConverter.BytesToPokemonString(array, 0, nameLength);
			}
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x000423DF File Offset: 0x000405DF
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveChangesWithoutMessage();
		}

		// Token: 0x06000882 RID: 2178 RVA: 0x000423EC File Offset: 0x000405EC
		private void SaveChangesWithoutMessage()
		{
			checked
			{
				int num = this.aiueoOrderList.Count - 1;
				for (int i = 0; i <= num; i++)
				{
					int num2 = this.AIUEO_ORDER_TABLE_OFFSET + i * 2;
					ushort num3 = (ushort)this.aiueoOrderList[i];
					byte[] bytes = BitConverter.GetBytes(num3);
					MainForm.romData[num2] = bytes[0];
					MainForm.romData[num2 + 1] = bytes[1];
				}
				int num4 = this.typeOrderList.Count - 1;
				for (int j = 0; j <= num4; j++)
				{
					int num5 = this.TYPE_ORDER_TABLE_OFFSET + j * 2;
					ushort num6 = (ushort)this.typeOrderList[j];
					byte[] bytes2 = BitConverter.GetBytes(num6);
					MainForm.romData[num5] = bytes2[0];
					MainForm.romData[num5 + 1] = bytes2[1];
				}
				int num7 = this.lightOrderList.Count - 1;
				for (int k = 0; k <= num7; k++)
				{
					int num8 = this.LIGHT_ORDER_TABLE_OFFSET + k * 2;
					ushort num9 = (ushort)this.lightOrderList[k];
					byte[] bytes3 = BitConverter.GetBytes(num9);
					MainForm.romData[num8] = bytes3[0];
					MainForm.romData[num8 + 1] = bytes3[1];
				}
				int num10 = this.smallOrderList.Count - 1;
				for (int l = 0; l <= num10; l++)
				{
					int num11 = this.SMALL_ORDER_TABLE_OFFSET + l * 2;
					ushort num12 = (ushort)this.smallOrderList[l];
					byte[] bytes4 = BitConverter.GetBytes(num12);
					MainForm.romData[num11] = bytes4[0];
					MainForm.romData[num11 + 1] = bytes4[1];
				}
				this.hasUnsavedChanges = false;
				this.btnSave.Enabled = false;
			}
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00042584 File Offset: 0x00040784
		private void PokedexListEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。変更を保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveChangesWithoutMessage();
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
		}

		// Token: 0x040004A7 RID: 1191
		public readonly int AIUEO_ORDER_TABLE_OFFSET;

		// Token: 0x040004A8 RID: 1192
		public readonly int AIUEO_ORDER_LIST_COUNT;

		// Token: 0x040004A9 RID: 1193
		public readonly int LIGHT_ORDER_TABLE_OFFSET;

		// Token: 0x040004AA RID: 1194
		public readonly int LIGHT_ORDER_LIST_COUNT;

		// Token: 0x040004AB RID: 1195
		public readonly int SMALL_ORDER_TABLE_OFFSET;

		// Token: 0x040004AC RID: 1196
		public readonly int SMALL_ORDER_LIST_COUNT;

		// Token: 0x040004AD RID: 1197
		public readonly int TYPE_ORDER_TABLE_OFFSET;

		// Token: 0x040004AE RID: 1198
		public readonly int TYPE_ORDER_LIST_COUNT;

		// Token: 0x040004AF RID: 1199
		public readonly bool ENABLE_POKEMONCODE_INDEXED;

		// Token: 0x040004B0 RID: 1200
		private byte[] romData;

		// Token: 0x040004B1 RID: 1201
		private bool hasUnsavedChanges;

		// Token: 0x040004B2 RID: 1202
		private Dictionary<int, PokemonData> pokemonDataList;

		// Token: 0x040004B3 RID: 1203
		private Dictionary<int, int> pokedexOrderToPokemonCodeMap;

		// Token: 0x040004B4 RID: 1204
		private Dictionary<string, PokedexListEditor.PokedexListGroup> uiGroups;

		// Token: 0x040004B5 RID: 1205
		private List<int> aiueoOrderList;

		// Token: 0x040004B6 RID: 1206
		private List<int> lightOrderList;

		// Token: 0x040004B7 RID: 1207
		private List<int> smallOrderList;

		// Token: 0x040004B8 RID: 1208
		private List<int> typeOrderList;

		// Token: 0x02000060 RID: 96
		public class PokedexListGroup
		{
			// Token: 0x06000FD7 RID: 4055 RVA: 0x0006C570 File Offset: 0x0006A770
			public PokedexListGroup(ListBox lst, ComboBox cmb, PictureBox pic, TextBox txt, Button btn)
			{
				this.ListBox = lst;
				this.ComboBox = cmb;
				this.IconPictureBox = pic;
				this.PokemonCodeTextBox = txt;
				this.ChangeButton = btn;
			}

			// Token: 0x04000900 RID: 2304
			public readonly ListBox ListBox;

			// Token: 0x04000901 RID: 2305
			public readonly ComboBox ComboBox;

			// Token: 0x04000902 RID: 2306
			public readonly PictureBox IconPictureBox;

			// Token: 0x04000903 RID: 2307
			public readonly TextBox PokemonCodeTextBox;

			// Token: 0x04000904 RID: 2308
			public readonly Button ChangeButton;
		}
	}
}
