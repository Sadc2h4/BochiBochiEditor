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
	// Token: 0x0200000D RID: 13
	public partial class EggMoveEditor : Form
	{
		// Token: 0x0600009C RID: 156 RVA: 0x00007300 File Offset: 0x00005500
		public EggMoveEditor()
		{
			base.Load += this.EggMoveEditor_Load;
			base.FormClosing += this.EggMoveEditor_FormClosing;
			this.EGG_MOVE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("EGG_MOVE_TABLE_OFFSET");
			this.ENABLE_EGG_MOVE_TABLE_SIZE_CALCULATE = RomIniReader.ReadBoolean("EGG_MOVE_TABLE_CALCULATE_SIZE");
			this.EGG_MOVE_TABLE_SIZE_OFFSET = RomIniReader.ReadHexOrDecimal("EGG_MOVE_TABLE_SIZE_OFFSET");
			this.isDataModified = false;
			this.pokemonDataList = new Dictionary<int, PokemonData>();
			this.currentEggMoveTable = new List<ushort>();
			this.InitializeComponent();
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600009F RID: 159 RVA: 0x00007C22 File Offset: 0x00005E22
		// (set) Token: 0x060000A0 RID: 160 RVA: 0x00007C2C File Offset: 0x00005E2C
		internal virtual ListBox lstEggMoveTable
		{
			[CompilerGenerated]
			get
			{
				return this._lstEggMoveTable;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.lstEggMoveTable_SelectedIndexChanged);
				ListBox listBox = this._lstEggMoveTable;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstEggMoveTable = value;
				listBox = this._lstEggMoveTable;
				if (listBox != null)
				{
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00007C6F File Offset: 0x00005E6F
		// (set) Token: 0x060000A2 RID: 162 RVA: 0x00007C7C File Offset: 0x00005E7C
		internal virtual Button btnPokemonInsert
		{
			[CompilerGenerated]
			get
			{
				return this._btnPokemonInsert;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPokemonInsert_Click);
				Button button = this._btnPokemonInsert;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPokemonInsert = value;
				button = this._btnPokemonInsert;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x00007CBF File Offset: 0x00005EBF
		// (set) Token: 0x060000A4 RID: 164 RVA: 0x00007CCC File Offset: 0x00005ECC
		internal virtual Button btnPokemonReplace
		{
			[CompilerGenerated]
			get
			{
				return this._btnPokemonReplace;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnPokemonReplace_Click);
				Button button = this._btnPokemonReplace;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnPokemonReplace = value;
				button = this._btnPokemonReplace;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x00007D0F File Offset: 0x00005F0F
		// (set) Token: 0x060000A6 RID: 166 RVA: 0x00007D1C File Offset: 0x00005F1C
		internal virtual ComboBox cmbPokemonList
		{
			[CompilerGenerated]
			get
			{
				return this._cmbPokemonList;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbPokemonList_SelectedIndexChanged);
				ComboBox comboBox = this._cmbPokemonList;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbPokemonList = value;
				comboBox = this._cmbPokemonList;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x00007D5F File Offset: 0x00005F5F
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x00007D6C File Offset: 0x00005F6C
		internal virtual Button btnDeletePokemon
		{
			[CompilerGenerated]
			get
			{
				return this._btnDeletePokemon;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnDeletePokemon_Click);
				Button button = this._btnDeletePokemon;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnDeletePokemon = value;
				button = this._btnDeletePokemon;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00007DAF File Offset: 0x00005FAF
		// (set) Token: 0x060000AA RID: 170 RVA: 0x00007DBC File Offset: 0x00005FBC
		internal virtual Button btnMoveInsert
		{
			[CompilerGenerated]
			get
			{
				return this._btnMoveInsert;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMoveInsert_Click);
				Button button = this._btnMoveInsert;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMoveInsert = value;
				button = this._btnMoveInsert;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00007DFF File Offset: 0x00005FFF
		// (set) Token: 0x060000AC RID: 172 RVA: 0x00007E0C File Offset: 0x0000600C
		internal virtual Button btnMoveReplace
		{
			[CompilerGenerated]
			get
			{
				return this._btnMoveReplace;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMoveReplace_Click);
				Button button = this._btnMoveReplace;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMoveReplace = value;
				button = this._btnMoveReplace;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00007E4F File Offset: 0x0000604F
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00007E59 File Offset: 0x00006059
		internal virtual ComboBox cmbMoveList
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00007E62 File Offset: 0x00006062
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00007E6C File Offset: 0x0000606C
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

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00007EAF File Offset: 0x000060AF
		// (set) Token: 0x060000B2 RID: 178 RVA: 0x00007EB9 File Offset: 0x000060B9
		internal virtual PictureBox picPokemon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00007EC2 File Offset: 0x000060C2
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x00007ECC File Offset: 0x000060CC
		internal virtual Label lblwarning
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00007ED5 File Offset: 0x000060D5
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x00007EDF File Offset: 0x000060DF
		internal virtual GroupBox grpControlPokemon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00007EE8 File Offset: 0x000060E8
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x00007EF2 File Offset: 0x000060F2
		internal virtual GroupBox grpControlMove
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x00007EFB File Offset: 0x000060FB
		// (set) Token: 0x060000BA RID: 186 RVA: 0x00007F08 File Offset: 0x00006108
		internal virtual Button btnDeleteMove
		{
			[CompilerGenerated]
			get
			{
				return this._btnDeleteMove;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnDeleteMove_Click);
				Button button = this._btnDeleteMove;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnDeleteMove = value;
				button = this._btnDeleteMove;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00007F4C File Offset: 0x0000614C
		private void EggMoveEditor_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.LoadAllPokemonData();
			this.InitializePokemonComboBox();
			this.InitializeMoveComboBox();
			this.LoadAndDisplayEggMoveTable();
			this.btnSave.Enabled = false;
			this.lstEggMoveTable.SelectedIndex = 0;
			this.cmbPokemonList.SelectedIndex = 0;
			EggMoveEditor.EggMoveListItem eggMoveListItem = (EggMoveEditor.EggMoveListItem)this.lstEggMoveTable.SelectedItem;
			bool isPokemon = eggMoveListItem.IsPokemon;
			if (isPokemon)
			{
				this.SetControlsEnabled(true, false);
			}
			else
			{
				this.SetControlsEnabled(false, true);
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00007FDA File Offset: 0x000061DA
		private void MarkDataModified()
		{
			this.isDataModified = true;
			this.btnSave.Enabled = true;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00007FF1 File Offset: 0x000061F1
		private void MarkDataSaved()
		{
			this.isDataModified = false;
			this.btnSave.Enabled = false;
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00008008 File Offset: 0x00006208
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

		// Token: 0x060000BF RID: 191 RVA: 0x00008068 File Offset: 0x00006268
		private void InitializePokemonComboBox()
		{
			this.cmbPokemonList.BeginUpdate();
			this.cmbPokemonList.Items.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					bool flag = this.pokemonDataList.ContainsKey(i);
					if (flag)
					{
						this.cmbPokemonList.Items.Add(this.pokemonDataList[i].Name);
					}
				}
				this.cmbPokemonList.EndUpdate();
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000080F4 File Offset: 0x000062F4
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

		// Token: 0x060000C1 RID: 193 RVA: 0x00008194 File Offset: 0x00006394
		private void cmbPokemonList_SelectedIndexChanged(object sender, EventArgs e)
		{
			int num = checked(this.cmbPokemonList.SelectedIndex + 1);
			PokemonData pokemonData = this.pokemonDataList[num];
			ImageProcessor.DisplayGBASprite(this.picPokemon, this.romData, pokemonData.FrontImageAddress, pokemonData.NormalPaletteAddress, 64, 64, true, true);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x000081E4 File Offset: 0x000063E4
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

		// Token: 0x060000C3 RID: 195 RVA: 0x00008268 File Offset: 0x00006468
		private void LoadSpriteAddresses(PokemonData pokemonData)
		{
			pokemonData.FrontImageAddress = this.ReadImageAddress(MyProject.Forms.PokemonEditor.FRONT_IMAGE_TABLE_OFFSET, pokemonData.Index);
			pokemonData.NormalPaletteAddress = this.ReadImageAddress(MyProject.Forms.PokemonEditor.NORMAL_PALETTE_TABLE_OFFSET, pokemonData.Index);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x000082BC File Offset: 0x000064BC
		private uint ReadImageAddress(int tableOffset, int entryIndex)
		{
			checked
			{
				int num = tableOffset + entryIndex * 8;
				uint num2 = BitConverter.ToUInt32(this.romData, num);
				return num2 - 134217728U;
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x000082E8 File Offset: 0x000064E8
		private void LoadAndDisplayEggMoveTable()
		{
			this.lstEggMoveTable.BeginUpdate();
			this.lstEggMoveTable.Items.Clear();
			this.currentEggMoveTable.Clear();
			int num = this.EGG_MOVE_TABLE_OFFSET;
			for (;;)
			{
				ushort num2 = BitConverter.ToUInt16(this.romData, num);
				bool flag = num2 == ushort.MaxValue;
				if (flag)
				{
					break;
				}
				this.currentEggMoveTable.Add(num2);
				bool flag2 = num2 >= 20000;
				if (flag2)
				{
					int num3 = (int)(num2 - 20000);
					bool flag3 = this.pokemonDataList.ContainsKey(num3);
					if (flag3)
					{
						string name = this.pokemonDataList[num3].Name;
						this.lstEggMoveTable.Items.Add(new EggMoveEditor.EggMoveListItem(true, num3, name));
					}
				}
				else
				{
					int num4 = (int)num2;
					string text = MoveData.ExtractMoveNameFromRom(this.romData, num4);
					this.lstEggMoveTable.Items.Add(new EggMoveEditor.EggMoveListItem(false, num4, "  " + text));
				}
				checked
				{
					num += 2;
				}
			}
			this.currentEggMoveTable.Add(ushort.MaxValue);
			this.lstEggMoveTable.EndUpdate();
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x0000841C File Offset: 0x0000661C
		private void lstEggMoveTable_SelectedIndexChanged(object sender, EventArgs e)
		{
			EggMoveEditor.EggMoveListItem eggMoveListItem = (EggMoveEditor.EggMoveListItem)this.lstEggMoveTable.SelectedItem;
			bool isPokemon = eggMoveListItem.IsPokemon;
			checked
			{
				if (isPokemon)
				{
					this.cmbPokemonList.SelectedIndex = eggMoveListItem.Index - 1;
					this.cmbMoveList.SelectedIndex = 0;
					this.SetControlsEnabled(true, false);
				}
				else
				{
					int num = -1;
					int num2 = this.lstEggMoveTable.SelectedIndex - 1;
					for (int i = num2; i >= 0; i += -1)
					{
						EggMoveEditor.EggMoveListItem eggMoveListItem2 = (EggMoveEditor.EggMoveListItem)this.lstEggMoveTable.Items[i];
						bool isPokemon2 = eggMoveListItem2.IsPokemon;
						if (isPokemon2)
						{
							num = eggMoveListItem2.Index;
							break;
						}
					}
					bool flag = num != -1;
					if (flag)
					{
						this.cmbPokemonList.SelectedIndex = num - 1;
					}
					this.cmbMoveList.SelectedIndex = eggMoveListItem.Index;
					this.SetControlsEnabled(false, true);
				}
			}
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00008501 File Offset: 0x00006701
		private void SetControlsEnabled(bool pokemonControlsEnabled, bool moveControlsEnabled)
		{
			this.btnPokemonReplace.Enabled = pokemonControlsEnabled;
			this.btnDeletePokemon.Enabled = pokemonControlsEnabled;
			this.btnMoveReplace.Enabled = moveControlsEnabled;
			this.btnDeleteMove.Enabled = moveControlsEnabled;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00008538 File Offset: 0x00006738
		private void RefreshEggMoveTableDisplay()
		{
			int selectedIndex = this.lstEggMoveTable.SelectedIndex;
			int topIndex = this.lstEggMoveTable.TopIndex;
			this.lstEggMoveTable.BeginUpdate();
			this.lstEggMoveTable.Items.Clear();
			{
				foreach (ushort num in this.currentEggMoveTable)
				{
					bool flag = num == ushort.MaxValue;
					if (flag)
					{
						break;
					}
					bool flag2 = num >= 20000;
					if (flag2)
					{
						int num2 = (int)(num - 20000);
						bool flag3 = this.pokemonDataList.ContainsKey(num2);
						if (flag3)
						{
							string name = this.pokemonDataList[num2].Name;
							this.lstEggMoveTable.Items.Add(new EggMoveEditor.EggMoveListItem(true, num2, name));
						}
					}
					else
					{
						int num3 = (int)num;
						string text = MoveData.ExtractMoveNameFromRom(this.romData, num3);
						this.lstEggMoveTable.Items.Add(new EggMoveEditor.EggMoveListItem(false, num3, "  " + text));
					}
				}
			}
			this.lstEggMoveTable.EndUpdate();
			bool flag4 = selectedIndex >= 0 && selectedIndex < this.lstEggMoveTable.Items.Count;
			if (flag4)
			{
				this.lstEggMoveTable.SelectedIndex = selectedIndex;
			}
			else
			{
				bool flag5 = this.lstEggMoveTable.Items.Count > 0;
				if (flag5)
				{
					this.lstEggMoveTable.SelectedIndex = checked(this.lstEggMoveTable.Items.Count - 1);
				}
			}
			bool flag6 = topIndex < this.lstEggMoveTable.Items.Count;
			if (flag6)
			{
				this.lstEggMoveTable.TopIndex = topIndex;
			}
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00008710 File Offset: 0x00006910
		private void btnPokemonInsert_Click(object sender, EventArgs e)
		{
			checked
			{
				int num = this.cmbPokemonList.SelectedIndex + 1;
				ushort num2 = (ushort)(20000 + num);
				int num3 = ((this.lstEggMoveTable.SelectedIndex == -1) ? (this.currentEggMoveTable.Count - 1) : this.lstEggMoveTable.SelectedIndex);
				this.currentEggMoveTable.Insert(num3, num2);
				this.MarkDataModified();
				this.RefreshEggMoveTableDisplay();
				this.lstEggMoveTable.SelectedIndex = num3;
			}
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00008788 File Offset: 0x00006988
		private void btnMoveInsert_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.cmbMoveList.SelectedIndex;
			checked
			{
				ushort num = (ushort)selectedIndex;
				int num2 = ((this.lstEggMoveTable.SelectedIndex == -1) ? (this.currentEggMoveTable.Count - 1) : this.lstEggMoveTable.SelectedIndex);
				this.currentEggMoveTable.Insert(num2, num);
				this.MarkDataModified();
				this.RefreshEggMoveTableDisplay();
				this.lstEggMoveTable.SelectedIndex = num2;
			}
		}

		// Token: 0x060000CB RID: 203 RVA: 0x000087F7 File Offset: 0x000069F7
		private void btnDeletePokemon_Click(object sender, EventArgs e)
		{
			this.RemovePokemonOrMove();
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00008801 File Offset: 0x00006A01
		private void btnDeleteMove_Click(object sender, EventArgs e)
		{
			this.RemovePokemonOrMove();
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000880C File Offset: 0x00006A0C
		private void RemovePokemonOrMove()
		{
			int selectedIndex = this.lstEggMoveTable.SelectedIndex;
			this.currentEggMoveTable.RemoveAt(selectedIndex);
			this.MarkDataModified();
			this.RefreshEggMoveTableDisplay();
			this.lstEggMoveTable.SelectedIndex = Math.Min(selectedIndex, checked(this.lstEggMoveTable.Items.Count - 1));
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00008868 File Offset: 0x00006A68
		private void btnPokemonReplace_Click(object sender, EventArgs e)
		{
			EggMoveEditor.EggMoveListItem eggMoveListItem = (EggMoveEditor.EggMoveListItem)this.lstEggMoveTable.SelectedItem;
			bool flag = !eggMoveListItem.IsPokemon;
			checked
			{
				if (!flag)
				{
					int num = this.cmbPokemonList.SelectedIndex + 1;
					ushort num2 = (ushort)(20000 + num);
					bool flag2 = this.currentEggMoveTable[this.lstEggMoveTable.SelectedIndex] == num2;
					if (!flag2)
					{
						this.currentEggMoveTable[this.lstEggMoveTable.SelectedIndex] = num2;
						this.MarkDataModified();
						this.RefreshEggMoveTableDisplay();
					}
				}
			}
		}

		// Token: 0x060000CF RID: 207 RVA: 0x000088F4 File Offset: 0x00006AF4
		private void btnMoveReplace_Click(object sender, EventArgs e)
		{
			EggMoveEditor.EggMoveListItem eggMoveListItem = (EggMoveEditor.EggMoveListItem)this.lstEggMoveTable.SelectedItem;
			bool isPokemon = eggMoveListItem.IsPokemon;
			if (!isPokemon)
			{
				int selectedIndex = this.cmbMoveList.SelectedIndex;
				ushort num = checked((ushort)selectedIndex);
				bool flag = this.currentEggMoveTable[this.lstEggMoveTable.SelectedIndex] == num;
				if (!flag)
				{
					this.currentEggMoveTable[this.lstEggMoveTable.SelectedIndex] = num;
					this.MarkDataModified();
					this.RefreshEggMoveTableDisplay();
				}
			}
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00008975 File Offset: 0x00006B75
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveEggMoveTable();
			this.MarkDataSaved();
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00008988 File Offset: 0x00006B88
		private void SaveEggMoveTable()
		{
			int num = this.EGG_MOVE_TABLE_OFFSET;
			checked
			{
				{
					foreach (ushort num2 in this.currentEggMoveTable)
					{
						byte[] bytes = BitConverter.GetBytes(num2);
						Array.Copy(bytes, 0, this.romData, num, 2);
						num += 2;
					}
				}
				bool enable_EGG_MOVE_TABLE_SIZE_CALCULATE = this.ENABLE_EGG_MOVE_TABLE_SIZE_CALCULATE;
				if (enable_EGG_MOVE_TABLE_SIZE_CALCULATE)
				{
					int num3 = this.currentEggMoveTable.Count - 1;
					ushort num4 = (ushort)Math.Round((double)(num3 * 2 - 2) / 2.0);
					byte[] bytes2 = BitConverter.GetBytes(num4);
					Array.Copy(bytes2, 0, this.romData, this.EGG_MOVE_TABLE_SIZE_OFFSET, 2);
				}
			}
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00008A54 File Offset: 0x00006C54
		private void EggMoveEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.isDataModified;
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
						this.SaveEggMoveTable();
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0400006E RID: 110
		public readonly int EGG_MOVE_TABLE_OFFSET;

		// Token: 0x0400006F RID: 111
		public readonly bool ENABLE_EGG_MOVE_TABLE_SIZE_CALCULATE;

		// Token: 0x04000070 RID: 112
		public readonly int EGG_MOVE_TABLE_SIZE_OFFSET;

		// Token: 0x04000071 RID: 113
		public const ushort EGG_MOVE_TABLE_TERMINATOR = 65535;

		// Token: 0x04000072 RID: 114
		public const ushort POKEMON_CODE_THRESHOLD = 20000;

		// Token: 0x04000073 RID: 115
		private byte[] romData;

		// Token: 0x04000074 RID: 116
		private bool isDataModified;

		// Token: 0x04000075 RID: 117
		private Dictionary<int, PokemonData> pokemonDataList;

		// Token: 0x04000076 RID: 118
		private List<ushort> currentEggMoveTable;

		// Token: 0x02000033 RID: 51
		public class EggMoveListItem
		{
			// Token: 0x17000592 RID: 1426
			// (get) Token: 0x06000EBE RID: 3774 RVA: 0x0006AB6C File Offset: 0x00068D6C
			// (set) Token: 0x06000EBF RID: 3775 RVA: 0x0006AB76 File Offset: 0x00068D76
			public bool IsPokemon { get; set; }

			// Token: 0x17000593 RID: 1427
			// (get) Token: 0x06000EC0 RID: 3776 RVA: 0x0006AB7F File Offset: 0x00068D7F
			// (set) Token: 0x06000EC1 RID: 3777 RVA: 0x0006AB89 File Offset: 0x00068D89
			public int Index { get; set; }

			// Token: 0x17000594 RID: 1428
			// (get) Token: 0x06000EC2 RID: 3778 RVA: 0x0006AB92 File Offset: 0x00068D92
			// (set) Token: 0x06000EC3 RID: 3779 RVA: 0x0006AB9C File Offset: 0x00068D9C
			public string DisplayText { get; set; }

			// Token: 0x06000EC4 RID: 3780 RVA: 0x0006ABA5 File Offset: 0x00068DA5
			public EggMoveListItem(bool isPokemon, int index, string displayText)
			{
				this.IsPokemon = isPokemon;
				this.Index = index;
				this.DisplayText = displayText;
			}

			// Token: 0x06000EC5 RID: 3781 RVA: 0x0006ABC8 File Offset: 0x00068DC8
			public override string ToString()
			{
				return this.DisplayText;
			}
		}
	}
}
