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
	// Token: 0x0200001F RID: 31
	public partial class PokedexOrderEditor : Form
	{
		// Token: 0x06000884 RID: 2180 RVA: 0x000425D4 File Offset: 0x000407D4
		public PokedexOrderEditor()
		{
			base.Load += this.PokedexOrderEditor_Load;
			base.FormClosing += this.PokedexOrderEditor_FormClosing;
			this.POKEDEX_ORDER_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("POKEDEX_ORDER_TABLE_OFFSET");
			this.POKEDEX_ORDER_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("POKEDEX_ORDER_ENTRY_LENGTH");
			this.MAX_POKEDEX_COUNT = RomIniReader.ReadHexOrDecimal("MAX_POKEDEX_COUNT");
			this.pokemonList = new List<PokedexOrderEditor.PokemonPokedexEntry>();
			this.hasUnsavedChanges = false;
			this.InitializeComponent();
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06000887 RID: 2183 RVA: 0x00042E89 File Offset: 0x00041089
		// (set) Token: 0x06000888 RID: 2184 RVA: 0x00042E94 File Offset: 0x00041094
		internal virtual ListBox lstPokemonCodePokedexOrder
		{
			[CompilerGenerated]
			get
			{
				return this._lstPokemonCodePokedexOrder;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				DrawItemEventHandler drawItemEventHandler = new DrawItemEventHandler(this.lstPokemonCodePokedexOrder_DrawItem);
				EventHandler eventHandler = new EventHandler(this.lstPokemonCodePokedexOrder_SelectedIndexChanged);
				ListBox listBox = this._lstPokemonCodePokedexOrder;
				if (listBox != null)
				{
					listBox.DrawItem -= drawItemEventHandler;
					listBox.SelectedIndexChanged -= eventHandler;
				}
				this._lstPokemonCodePokedexOrder = value;
				listBox = this._lstPokemonCodePokedexOrder;
				if (listBox != null)
				{
					listBox.DrawItem += drawItemEventHandler;
					listBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06000889 RID: 2185 RVA: 0x00042EF2 File Offset: 0x000410F2
		// (set) Token: 0x0600088A RID: 2186 RVA: 0x00042EFC File Offset: 0x000410FC
		internal virtual TextBox txtPokemonCodeShow
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x0600088B RID: 2187 RVA: 0x00042F05 File Offset: 0x00041105
		// (set) Token: 0x0600088C RID: 2188 RVA: 0x00042F0F File Offset: 0x0004110F
		internal virtual PictureBox picPokedexIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x0600088D RID: 2189 RVA: 0x00042F18 File Offset: 0x00041118
		// (set) Token: 0x0600088E RID: 2190 RVA: 0x00042F24 File Offset: 0x00041124
		internal virtual Button btnUpdatePokedexOrder
		{
			[CompilerGenerated]
			get
			{
				return this._btnUpdatePokedexOrder;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnUpdatePokedexOrder_Click);
				Button button = this._btnUpdatePokedexOrder;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnUpdatePokedexOrder = value;
				button = this._btnUpdatePokedexOrder;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x0600088F RID: 2191 RVA: 0x00042F67 File Offset: 0x00041167
		// (set) Token: 0x06000890 RID: 2192 RVA: 0x00042F71 File Offset: 0x00041171
		internal virtual Label lblPokedexOrderNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06000891 RID: 2193 RVA: 0x00042F7A File Offset: 0x0004117A
		// (set) Token: 0x06000892 RID: 2194 RVA: 0x00042F84 File Offset: 0x00041184
		internal virtual GroupBox grpNote
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06000893 RID: 2195 RVA: 0x00042F8D File Offset: 0x0004118D
		// (set) Token: 0x06000894 RID: 2196 RVA: 0x00042F97 File Offset: 0x00041197
		internal virtual Label lblNote2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06000895 RID: 2197 RVA: 0x00042FA0 File Offset: 0x000411A0
		// (set) Token: 0x06000896 RID: 2198 RVA: 0x00042FAA File Offset: 0x000411AA
		internal virtual Label lblNote1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06000897 RID: 2199 RVA: 0x00042FB3 File Offset: 0x000411B3
		// (set) Token: 0x06000898 RID: 2200 RVA: 0x00042FBD File Offset: 0x000411BD
		internal virtual ListBox lstPokedexOrderUnused
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06000899 RID: 2201 RVA: 0x00042FC6 File Offset: 0x000411C6
		// (set) Token: 0x0600089A RID: 2202 RVA: 0x00042FD0 File Offset: 0x000411D0
		internal virtual Label lblPokedexOrderUnused
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x0600089B RID: 2203 RVA: 0x00042FD9 File Offset: 0x000411D9
		// (set) Token: 0x0600089C RID: 2204 RVA: 0x00042FE4 File Offset: 0x000411E4
		internal virtual Button btnChangePokedexOrder
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangePokedexOrder;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangePokedexOrder_Click);
				Button button = this._btnChangePokedexOrder;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangePokedexOrder = value;
				button = this._btnChangePokedexOrder;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x0600089D RID: 2205 RVA: 0x00043027 File Offset: 0x00041227
		// (set) Token: 0x0600089E RID: 2206 RVA: 0x00043031 File Offset: 0x00041231
		internal virtual Label lblWarning
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x0600089F RID: 2207 RVA: 0x0004303A File Offset: 0x0004123A
		// (set) Token: 0x060008A0 RID: 2208 RVA: 0x00043044 File Offset: 0x00041244
		internal virtual NumericUpDown nudPokedexOrderNumber
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x0004304D File Offset: 0x0004124D
		private void PokedexOrderEditor_Load(object sender, EventArgs e)
		{
			this.LoadPokemonPokedexData();
			this.PopulatePokemonList();
			this.UpdateStatusesAndUnusedList();
			this.hasUnsavedChanges = false;
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x0004306C File Offset: 0x0004126C
		private void LoadPokemonPokedexData()
		{
			this.pokemonList.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					int num2 = MyProject.Forms.PokemonEditor.POKEMON_NAME_OFFSET + i * MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH;
					byte[] array = new byte[MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH - 1 + 1];
					Array.Copy(MainForm.romData, num2, array, 0, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH);
					string text = TextConverter.BytesToPokemonString(array, 0, MyProject.Forms.PokemonEditor.POKEMON_NAME_LENGTH).Trim();
					int num3 = this.POKEDEX_ORDER_TABLE_OFFSET + (i - 1) * this.POKEDEX_ORDER_ENTRY_LENGTH;
					int num4 = (int)BitConverter.ToUInt16(MainForm.romData, num3);
					this.pokemonList.Add(new PokedexOrderEditor.PokemonPokedexEntry
					{
						PokemonCode = i,
						Name = text,
						PokedexOrder = num4,
						OriginalPokedexOrder = num4
					});
				}
			}
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x00043178 File Offset: 0x00041378
		private void PopulatePokemonList()
		{
			this.lstPokemonCodePokedexOrder.Items.Clear();
			{
				foreach (PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry in this.pokemonList)
				{
					this.lstPokemonCodePokedexOrder.Items.Add(pokemonPokedexEntry);
				}
			}
			this.lstPokemonCodePokedexOrder.SelectedIndex = 0;
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x000431FC File Offset: 0x000413FC
		private void UpdateStatusesAndUnusedList()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			checked
			{
				{
					foreach (PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry in this.pokemonList)
					{
						bool flag = !dictionary.ContainsKey(pokemonPokedexEntry.PokedexOrder);
						if (flag)
						{
							dictionary.Add(pokemonPokedexEntry.PokedexOrder, 0);
						}
						Dictionary<int, int> dictionary2;
						int pokedexOrder;
						(dictionary2 = dictionary)[pokedexOrder = pokemonPokedexEntry.PokedexOrder] = dictionary2[pokedexOrder] + 1;
					}
				}
				{
					foreach (PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry2 in this.pokemonList)
					{
						bool flag2 = pokemonPokedexEntry2.PokedexOrder > this.MAX_POKEDEX_COUNT;
						if (flag2)
						{
							pokemonPokedexEntry2.Status = PokedexOrderEditor.PokedexStatus.OutOfRange;
						}
						else
						{
							bool flag3 = dictionary.ContainsKey(pokemonPokedexEntry2.PokedexOrder) && dictionary[pokemonPokedexEntry2.PokedexOrder] > 1;
							if (flag3)
							{
								pokemonPokedexEntry2.Status = PokedexOrderEditor.PokedexStatus.Duplicate;
							}
							else
							{
								pokemonPokedexEntry2.Status = PokedexOrderEditor.PokedexStatus.Normal;
							}
						}
					}
				}
				this.lstPokedexOrderUnused.Items.Clear();
				HashSet<int> hashSet = new HashSet<int>(this.pokemonList.Select((PokedexOrderEditor.PokemonPokedexEntry p) => p.PokedexOrder));
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					bool flag4 = !hashSet.Contains(i);
					if (flag4)
					{
						this.lstPokedexOrderUnused.Items.Add(i);
					}
				}
				this.lstPokemonCodePokedexOrder.Invalidate();
			}
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x000433E0 File Offset: 0x000415E0
		private void lstPokemonCodePokedexOrder_DrawItem(object sender, DrawItemEventArgs e)
		{
			PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry = (PokedexOrderEditor.PokemonPokedexEntry)this.lstPokemonCodePokedexOrder.Items[e.Index];
			bool flag = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			Color color;
			if (flag)
			{
				color = SystemColors.Highlight;
			}
			else
			{
				PokedexOrderEditor.PokedexStatus status = pokemonPokedexEntry.Status;
				if (status != PokedexOrderEditor.PokedexStatus.Duplicate)
				{
					if (status != PokedexOrderEditor.PokedexStatus.OutOfRange)
					{
						color = e.BackColor;
					}
					else
					{
						color = Color.LightYellow;
					}
				}
				else
				{
					color = Color.LightCoral;
				}
			}
			using (SolidBrush solidBrush = new SolidBrush(color))
			{
				e.Graphics.FillRectangle(solidBrush, e.Bounds);
			}
			using (SolidBrush solidBrush2 = new SolidBrush(e.ForeColor))
			{
				float num = (float)((double)e.Bounds.Y + (double)(checked(e.Bounds.Height - e.Font.Height)) / 2.0);
				e.Graphics.DrawString(pokemonPokedexEntry.Name, e.Font, solidBrush2, new PointF((float)(checked(e.Bounds.X + 2)), num));
				string text = pokemonPokedexEntry.PokedexOrder.ToString();
				SizeF sizeF = e.Graphics.MeasureString(text, e.Font);
				e.Graphics.DrawString(text, e.Font, solidBrush2, new PointF((float)e.Bounds.Right - sizeF.Width - 5f, num));
				string text2 = "-";
				int num2 = 45;
				e.Graphics.DrawString(text2, e.Font, solidBrush2, new PointF((float)(checked(e.Bounds.Right - num2)), num));
			}
			e.DrawFocusRectangle();
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x000435E4 File Offset: 0x000417E4
		private void lstPokemonCodePokedexOrder_SelectedIndexChanged(object sender, EventArgs e)
		{
			PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry = this.pokemonList[this.lstPokemonCodePokedexOrder.SelectedIndex];
			this.txtPokemonCodeShow.Text = string.Format("ポケモンコード : {0}", pokemonPokedexEntry.PokemonCode.ToString("X4"));
			this.nudPokedexOrderNumber.Value = new decimal(pokemonPokedexEntry.PokedexOrder);
			this.DisplayPokemonIcon(pokemonPokedexEntry.PokemonCode);
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x00043658 File Offset: 0x00041858
		private void DisplayPokemonIcon(int pokemonCode)
		{
			uint num3;
			Color[] array2;
			byte[] array3;
			checked
			{
				int num = MyProject.Forms.PokemonEditor.ICON_IMAGE_TABLE_OFFSET + pokemonCode * 4;
				uint num2 = BitConverter.ToUInt32(MainForm.romData, num);
				num3 = num2 - 134217728U;
				int num4 = MyProject.Forms.PokemonEditor.ICON_PALETTE_ID_TABLE_OFFSET + pokemonCode;
				int num5 = (int)MainForm.romData[num4];
				byte[] array = this.LoadIconPalette(num5);
				array2 = ImageProcessor.LoadPalette(array, true);
				array3 = new byte[2048];
			}
			Array.Copy(MainForm.romData, (long)((ulong)num3), array3, 0L, (long)array3.Length);
			Bitmap bitmap = ImageProcessor.LoadSprite(ref array3, array2, 32, 32, false);
			this.picPokedexIcon.Image = bitmap;
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x00043700 File Offset: 0x00041900
		public byte[] LoadIconPalette(int paletteId)
		{
			uint num3;
			byte[] array;
			checked
			{
				int num = MyProject.Forms.PokemonEditor.ICON_PALETTE_TABLE_OFFSET + paletteId * 8;
				uint num2 = BitConverter.ToUInt32(MainForm.romData, num);
				num3 = num2 - 134217728U;
				array = new byte[32];
			}
			Array.Copy(MainForm.romData, (long)((ulong)num3), array, 0L, 32L);
			return array;
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x0004375C File Offset: 0x0004195C
		private void btnUpdatePokedexOrder_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.lstPokemonCodePokedexOrder.SelectedIndex;
			PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry = this.pokemonList[selectedIndex];
			int num = Convert.ToInt32(this.nudPokedexOrderNumber.Value);
			bool flag = pokemonPokedexEntry.PokedexOrder != num;
			if (flag)
			{
				pokemonPokedexEntry.PokedexOrder = num;
				this.hasUnsavedChanges = true;
				this.btnChangePokedexOrder.Enabled = true;
				this.UpdateStatusesAndUnusedList();
			}
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x000437CC File Offset: 0x000419CC
		private void SaveChanges()
		{
			checked
			{
				{
					foreach (PokedexOrderEditor.PokemonPokedexEntry pokemonPokedexEntry in this.pokemonList)
					{
						bool flag = pokemonPokedexEntry.PokedexOrder != pokemonPokedexEntry.OriginalPokedexOrder && pokemonPokedexEntry.PokemonCode > 0;
						if (flag)
						{
							int num = this.POKEDEX_ORDER_TABLE_OFFSET + (pokemonPokedexEntry.PokemonCode - 1) * this.POKEDEX_ORDER_ENTRY_LENGTH;
							byte[] bytes = BitConverter.GetBytes((ushort)pokemonPokedexEntry.PokedexOrder);
							MainForm.romData[num] = bytes[0];
							MainForm.romData[num + 1] = bytes[1];
							pokemonPokedexEntry.OriginalPokedexOrder = pokemonPokedexEntry.PokedexOrder;
						}
					}
				}
				this.hasUnsavedChanges = false;
				this.btnChangePokedexOrder.Enabled = false;
			}
		}

		// Token: 0x060008AB RID: 2219 RVA: 0x000438A0 File Offset: 0x00041AA0
		private void btnChangePokedexOrder_Click(object sender, EventArgs e)
		{
			this.SaveChanges();
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x000438AC File Offset: 0x00041AAC
		private void PokedexOrderEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。変更を保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				bool flag2 = dialogResult == DialogResult.Yes;
				if (flag2)
				{
					this.SaveChanges();
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

		// Token: 0x040004C7 RID: 1223
		public readonly int POKEDEX_ORDER_TABLE_OFFSET;

		// Token: 0x040004C8 RID: 1224
		public readonly int POKEDEX_ORDER_ENTRY_LENGTH;

		// Token: 0x040004C9 RID: 1225
		public readonly int MAX_POKEDEX_COUNT;

		// Token: 0x040004CA RID: 1226
		private List<PokedexOrderEditor.PokemonPokedexEntry> pokemonList;

		// Token: 0x040004CB RID: 1227
		private bool hasUnsavedChanges;

		// Token: 0x02000064 RID: 100
		public enum PokedexStatus
		{
			// Token: 0x0400090C RID: 2316
			Normal,
			// Token: 0x0400090D RID: 2317
			Duplicate,
			// Token: 0x0400090E RID: 2318
			OutOfRange
		}

		// Token: 0x02000065 RID: 101
		public class PokemonPokedexEntry
		{
			// Token: 0x06000FE1 RID: 4065 RVA: 0x0006C65B File Offset: 0x0006A85B
			public PokemonPokedexEntry()
			{
				this.Status = PokedexOrderEditor.PokedexStatus.Normal;
			}

			// Token: 0x170005F1 RID: 1521
			// (get) Token: 0x06000FE2 RID: 4066 RVA: 0x0006C66C File Offset: 0x0006A86C
			// (set) Token: 0x06000FE3 RID: 4067 RVA: 0x0006C676 File Offset: 0x0006A876
			public int PokemonCode { get; set; }

			// Token: 0x170005F2 RID: 1522
			// (get) Token: 0x06000FE4 RID: 4068 RVA: 0x0006C67F File Offset: 0x0006A87F
			// (set) Token: 0x06000FE5 RID: 4069 RVA: 0x0006C689 File Offset: 0x0006A889
			public string Name { get; set; }

			// Token: 0x170005F3 RID: 1523
			// (get) Token: 0x06000FE6 RID: 4070 RVA: 0x0006C692 File Offset: 0x0006A892
			// (set) Token: 0x06000FE7 RID: 4071 RVA: 0x0006C69C File Offset: 0x0006A89C
			public int PokedexOrder { get; set; }

			// Token: 0x170005F4 RID: 1524
			// (get) Token: 0x06000FE8 RID: 4072 RVA: 0x0006C6A5 File Offset: 0x0006A8A5
			// (set) Token: 0x06000FE9 RID: 4073 RVA: 0x0006C6AF File Offset: 0x0006A8AF
			public int OriginalPokedexOrder { get; set; }

			// Token: 0x170005F5 RID: 1525
			// (get) Token: 0x06000FEA RID: 4074 RVA: 0x0006C6B8 File Offset: 0x0006A8B8
			// (set) Token: 0x06000FEB RID: 4075 RVA: 0x0006C6C2 File Offset: 0x0006A8C2
			public PokedexOrderEditor.PokedexStatus Status { get; set; }

			// Token: 0x06000FEC RID: 4076 RVA: 0x0006C6CC File Offset: 0x0006A8CC
			public override string ToString()
			{
				return this.Name;
			}
		}
	}
}
