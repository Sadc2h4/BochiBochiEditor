using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using BochiBochiEditor.My;

namespace BochiBochiEditor
{
	// Token: 0x02000019 RID: 25
	public partial class ItemUseCoordinate : Form
	{
		// Token: 0x060003C9 RID: 969 RVA: 0x0001CB8C File Offset: 0x0001AD8C
		public ItemUseCoordinate()
		{
			base.Load += this.ItemUseCoordinate_Load;
			base.FormClosing += this.ItemUseCoordinate_FormClosing;
			this.ITEM_USE_COORDINATE_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ITEM_USE_COORDINATE_TABLE_OFFSET");
			this.ITEM_USE_COORDINATE_ENTRY_LENGTH = RomIniReader.ReadHexOrDecimal("ITEM_USE_COORDINATE_ENTRY_LENGTH");
			this.ITEM_USE_COORDINATE_COUNT = RomIniReader.ReadHexOrDecimal("ITEM_USE_COORDINATE_COUNT");
			this.hasUnsavedChanges = false;
			this.currentPokemonIndex = 0;
			this.pokemonIconList = new Dictionary<int, PokemonData>();
			this.itemUseBackGround1 = null;
			this.itemUseBackGround2 = null;
			this.InitializeComponent();
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x060003CC RID: 972 RVA: 0x0001D5FD File Offset: 0x0001B7FD
		// (set) Token: 0x060003CD RID: 973 RVA: 0x0001D608 File Offset: 0x0001B808
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

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x060003CE RID: 974 RVA: 0x0001D64B File Offset: 0x0001B84B
		// (set) Token: 0x060003CF RID: 975 RVA: 0x0001D655 File Offset: 0x0001B855
		internal virtual GroupBox grpItemUse1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x060003D0 RID: 976 RVA: 0x0001D65E File Offset: 0x0001B85E
		// (set) Token: 0x060003D1 RID: 977 RVA: 0x0001D668 File Offset: 0x0001B868
		internal virtual PictureBox picItemUse1BackGround
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x060003D2 RID: 978 RVA: 0x0001D671 File Offset: 0x0001B871
		// (set) Token: 0x060003D3 RID: 979 RVA: 0x0001D67C File Offset: 0x0001B87C
		internal virtual NumericUpDown nudItemUse1Y
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemUse1Y;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemUse1Y;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemUse1Y = value;
				numericUpDown = this._nudItemUse1Y;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x060003D4 RID: 980 RVA: 0x0001D6BF File Offset: 0x0001B8BF
		// (set) Token: 0x060003D5 RID: 981 RVA: 0x0001D6CC File Offset: 0x0001B8CC
		internal virtual NumericUpDown nudItemUse1X
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemUse1X;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemUse1X;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemUse1X = value;
				numericUpDown = this._nudItemUse1X;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x060003D6 RID: 982 RVA: 0x0001D70F File Offset: 0x0001B90F
		// (set) Token: 0x060003D7 RID: 983 RVA: 0x0001D719 File Offset: 0x0001B919
		internal virtual GroupBox grpItemUse2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x060003D8 RID: 984 RVA: 0x0001D722 File Offset: 0x0001B922
		// (set) Token: 0x060003D9 RID: 985 RVA: 0x0001D72C File Offset: 0x0001B92C
		internal virtual PictureBox picItemUse2BackGround1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x060003DA RID: 986 RVA: 0x0001D735 File Offset: 0x0001B935
		// (set) Token: 0x060003DB RID: 987 RVA: 0x0001D740 File Offset: 0x0001B940
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

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x060003DC RID: 988 RVA: 0x0001D783 File Offset: 0x0001B983
		// (set) Token: 0x060003DD RID: 989 RVA: 0x0001D790 File Offset: 0x0001B990
		internal virtual NumericUpDown nudItemUse2Y
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemUse2Y;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemUse2Y;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemUse2Y = value;
				numericUpDown = this._nudItemUse2Y;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x060003DE RID: 990 RVA: 0x0001D7D3 File Offset: 0x0001B9D3
		// (set) Token: 0x060003DF RID: 991 RVA: 0x0001D7E0 File Offset: 0x0001B9E0
		internal virtual NumericUpDown nudItemUse2X
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemUse2X;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemUse2X;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemUse2X = value;
				numericUpDown = this._nudItemUse2X;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x060003E0 RID: 992 RVA: 0x0001D823 File Offset: 0x0001BA23
		// (set) Token: 0x060003E1 RID: 993 RVA: 0x0001D82D File Offset: 0x0001BA2D
		internal virtual GroupBox grpItemUse2Zoom
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x060003E2 RID: 994 RVA: 0x0001D836 File Offset: 0x0001BA36
		// (set) Token: 0x060003E3 RID: 995 RVA: 0x0001D840 File Offset: 0x0001BA40
		internal virtual PictureBox picItemUse2BackGround2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x060003E4 RID: 996 RVA: 0x0001D849 File Offset: 0x0001BA49
		// (set) Token: 0x060003E5 RID: 997 RVA: 0x0001D854 File Offset: 0x0001BA54
		internal virtual NumericUpDown nudItemUse2Zoom
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemUse2Zoom;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nud_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemUse2Zoom;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemUse2Zoom = value;
				numericUpDown = this._nudItemUse2Zoom;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x0001D897 File Offset: 0x0001BA97
		// (set) Token: 0x060003E7 RID: 999 RVA: 0x0001D8A1 File Offset: 0x0001BAA1
		internal virtual TextBox txtPokemonCodeShow
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060003E8 RID: 1000 RVA: 0x0001D8AA File Offset: 0x0001BAAA
		// (set) Token: 0x060003E9 RID: 1001 RVA: 0x0001D8B4 File Offset: 0x0001BAB4
		internal virtual PictureBox picPokemonIcon
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x0001D8BD File Offset: 0x0001BABD
		private void ItemUseCoordinate_Load(object sender, EventArgs e)
		{
			this.romData = MainForm.romData;
			this.InitializePokemonList();
			this.LoadAllPokemonIconData();
			this.LoadBackgroundImage();
			this.cmbPokemonCode.SelectedIndex = 0;
			this.UpdateDisplay();
			this.ResetChangeFlag();
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x0001D8FC File Offset: 0x0001BAFC
		private void InitializePokemonList()
		{
			this.cmbPokemonCode.BeginUpdate();
			this.cmbPokemonCode.Items.Clear();
			checked
			{
				int num = MyProject.Forms.PokemonEditor.TOTAL_POKEMON_COUNT - 1;
				for (int i = 1; i <= num; i++)
				{
					this.cmbPokemonCode.Items.Add(this.GetPokemonNameFromRom(i));
				}
				this.cmbPokemonCode.EndUpdate();
			}
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0001D96C File Offset: 0x0001BB6C
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

		// Token: 0x060003ED RID: 1005 RVA: 0x0001D9F0 File Offset: 0x0001BBF0
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

		// Token: 0x060003EE RID: 1006 RVA: 0x0001DAB8 File Offset: 0x0001BCB8
		private void DisplayPokemonIcon(PokemonData pokemonData)
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
			bool flag = this.picPokemonIcon.Image != null;
			if (flag)
			{
				this.picPokemonIcon.Image.Dispose();
			}
			this.picPokemonIcon.Image = bitmap;
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0001DB60 File Offset: 0x0001BD60
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

		// Token: 0x060003F0 RID: 1008 RVA: 0x0001DBB8 File Offset: 0x0001BDB8
		private void LoadBackgroundImage()
		{
			bool flag = this.itemUseBackGround1 != null;
			if (flag)
			{
				this.itemUseBackGround1.Dispose();
			}
			this.itemUseBackGround1 = (Bitmap)Image.FromFile("img/ItemUseBackGround1.png");
			bool flag2 = this.itemUseBackGround2 != null;
			if (flag2)
			{
				this.itemUseBackGround2.Dispose();
			}
			this.itemUseBackGround2 = (Bitmap)Image.FromFile("img/ItemUseBackGround2.png");
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0001DC22 File Offset: 0x0001BE22
		private void UpdateDisplay()
		{
			this.UpdatePokemonInfoDisplay();
			this.UpdateItemUseCoordinates();
			this.UpdatePreviewImages();
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x0001DC3C File Offset: 0x0001BE3C
		private void UpdatePokemonInfoDisplay()
		{
			int num = checked(this.cmbPokemonCode.SelectedIndex + 1);
			this.txtPokemonCodeShow.Text = string.Format("ポケモンコード : {0}", num.ToString("X4"));
			this.DisplayPokemonIcon(this.pokemonIconList[num]);
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x0001DC90 File Offset: 0x0001BE90
		private void UpdateItemUseCoordinates()
		{
			int selectedIndex = this.cmbPokemonCode.SelectedIndex;
			checked
			{
				int num = this.ITEM_USE_COORDINATE_TABLE_OFFSET + selectedIndex * this.ITEM_USE_COORDINATE_ENTRY_LENGTH;
				this.nudItemUse2X.Value = new decimal((int)this.romData[num]);
				this.nudItemUse2Y.Value = new decimal((int)this.romData[num + 1]);
				this.nudItemUse2Zoom.Value = new decimal((int)this.romData[num + 2]);
				this.nudItemUse1X.Value = new decimal((int)this.romData[num + 3]);
				this.nudItemUse1Y.Value = new decimal((int)this.romData[num + 4]);
			}
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x0001DD3F File Offset: 0x0001BF3F
		private void UpdatePreviewImages()
		{
			this.UpdateItemUse1Preview();
			this.UpdateItemUse2Preview();
			this.UpdateItemUse2ZoomPreview();
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x0001DD58 File Offset: 0x0001BF58
		private void UpdateItemUse1Preview()
		{
			checked
			{
				using (Bitmap bitmap = this.CreateBaseImageWithPokemon(this.itemUseBackGround1, false))
				{
					Bitmap itemImage = this.GetItemImage(13);
					using (Bitmap bitmap2 = new Bitmap(bitmap))
					{
						using (Graphics graphics = Graphics.FromImage(bitmap2))
						{
							int num = 76 + Convert.ToInt32(this.nudItemUse1X.Value);
							int num2 = 24 + Convert.ToInt32(this.nudItemUse1Y.Value);
							graphics.DrawImage(itemImage, num, num2);
						}
						bool flag = this.picItemUse1BackGround.Image != null;
						if (flag)
						{
							this.picItemUse1BackGround.Image.Dispose();
						}
						this.picItemUse1BackGround.Image = (Bitmap)bitmap2.Clone();
					}
					itemImage.Dispose();
				}
			}
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x0001DE5C File Offset: 0x0001C05C
		private void UpdateItemUse2Preview()
		{
			checked
			{
				using (Bitmap bitmap = this.CreateBaseImageWithPokemon(this.itemUseBackGround2, false))
				{
					Bitmap itemImage = this.GetItemImage(289);
					using (Bitmap bitmap2 = new Bitmap(bitmap))
					{
						using (Graphics graphics = Graphics.FromImage(bitmap2))
						{
							int num = 76 + Convert.ToInt32(this.nudItemUse2X.Value);
							int num2 = 24 + Convert.ToInt32(this.nudItemUse2Y.Value);
							graphics.DrawImage(itemImage, num, num2);
						}
						bool flag = this.picItemUse2BackGround1.Image != null;
						if (flag)
						{
							this.picItemUse2BackGround1.Image.Dispose();
						}
						this.picItemUse2BackGround1.Image = (Bitmap)bitmap2.Clone();
					}
					itemImage.Dispose();
				}
			}
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0001DF60 File Offset: 0x0001C160
		private void UpdateItemUse2ZoomPreview()
		{
			checked
			{
				using (Bitmap bitmap = new Bitmap(this.picItemUse2BackGround2.Width, this.picItemUse2BackGround2.Height))
				{
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
						graphics.PixelOffsetMode = PixelOffsetMode.Half;
						graphics.DrawImage(this.itemUseBackGround2, 0, 0);
						int num = this.cmbPokemonCode.SelectedIndex + 1;
						using (Bitmap bitmap2 = this.LoadPokemonFrontNormalImage(num))
						{
							using (Bitmap itemImage = this.GetItemImage(289))
							{
								int num2 = 120;
								int num3 = 72;
								int num4 = num3 + Convert.ToInt32(this.nudItemUse2Zoom.Value);
								int num5 = this.itemUseBackGround2.Width * 2;
								int num6 = this.itemUseBackGround2.Height * 2;
								int num7 = num2 - num5 / 2;
								int num8 = num4 - num6 / 2 - 8;
								graphics.DrawImage(bitmap2, num7 + 176, num8 + 80, bitmap2.Width * 2, bitmap2.Height * 2);
								int num9 = num7 + (76 + Convert.ToInt32(this.nudItemUse2X.Value) - 2) * 2;
								int num10 = num8 + (24 + Convert.ToInt32(this.nudItemUse2Y.Value)) * 2;
								graphics.DrawImage(itemImage, num9, num10, itemImage.Width * 2, itemImage.Height * 2);
							}
						}
					}
					bool flag = this.picItemUse2BackGround2.Image != null;
					if (flag)
					{
						this.picItemUse2BackGround2.Image.Dispose();
					}
					this.picItemUse2BackGround2.Image = (Bitmap)bitmap.Clone();
				}
			}
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0001E17C File Offset: 0x0001C37C
		private Bitmap CreateBaseImageWithPokemon(Bitmap backgroundImage, bool zoom)
		{
			Bitmap bitmap = new Bitmap(backgroundImage);
			checked
			{
				int num = this.cmbPokemonCode.SelectedIndex + 1;
				Bitmap bitmap2 = this.LoadPokemonFrontNormalImage(num);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					if (zoom)
					{
						graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
						graphics.PixelOffsetMode = PixelOffsetMode.Half;
						int num2 = bitmap2.Width * 2;
						int num3 = bitmap2.Height * 2;
						int num4 = 88 - (num2 - bitmap2.Width) / 2;
						int num5 = 40 - (num3 - bitmap2.Height) / 2 + Convert.ToInt32(this.nudItemUse2Zoom.Value) / 2;
						graphics.DrawImage(bitmap2, num4, num5, num2, num3);
					}
					else
					{
						graphics.DrawImage(bitmap2, 88, 40);
					}
				}
				bitmap2.Dispose();
				return bitmap;
			}
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x0001E260 File Offset: 0x0001C460
		private Bitmap LoadPokemonFrontNormalImage(int pokemonIndex)
		{
			checked
			{
				int num = MyProject.Forms.PokemonEditor.FRONT_IMAGE_TABLE_OFFSET + pokemonIndex * 8;
				uint num2 = BitConverter.ToUInt32(this.romData, num) - 134217728U;
				int num3 = MyProject.Forms.PokemonEditor.NORMAL_PALETTE_TABLE_OFFSET + pokemonIndex * 8;
				uint num4 = BitConverter.ToUInt32(this.romData, num3) - 134217728U;
				byte[] array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num2, false);
				byte[] array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, num4, true);
				Color[] array3 = ImageProcessor.LoadPalette(array2, true);
				return ImageProcessor.LoadSprite(ref array, array3, 64, 64, false);
			}
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x0001E2F8 File Offset: 0x0001C4F8
		private Bitmap GetItemImage(ushort itemId)
		{
			ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(this.romData, itemId);
			byte[] array = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, itemInfo.ImageAddress, false);
			byte[] array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(this.romData, itemInfo.PaletteAddress, true);
			Color[] array3 = ImageProcessor.LoadPalette(array2, true);
			return ImageProcessor.LoadSprite(ref array, array3, 24, 24, false);
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x0001E354 File Offset: 0x0001C554
		private void cmbPokemonCode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.cmbPokemonCode.SelectedIndex == this.currentPokemonIndex;
			if (!flag)
			{
				bool flag2 = this.hasUnsavedChanges;
				if (flag2)
				{
					DialogResult dialogResult = MessageBox.Show("現在の変更は保存されていません。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Cancel)
					{
						this.cmbPokemonCode.SelectedIndex = this.currentPokemonIndex;
						return;
					}
					if (dialogResult != DialogResult.Yes)
					{
						if (dialogResult == DialogResult.No)
						{
							this.LoadCurrentPokemonData();
						}
					}
					else
					{
						this.SaveCurrentPokemonChanges();
					}
				}
				this.currentPokemonIndex = this.cmbPokemonCode.SelectedIndex;
				this.UpdateDisplay();
				this.ResetChangeFlag();
			}
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x0001E3F8 File Offset: 0x0001C5F8
		private void SaveCurrentPokemonChanges()
		{
			int num = this.currentPokemonIndex;
			bool flag = num < 0 || num >= this.ITEM_USE_COORDINATE_COUNT;
			checked
			{
				if (!flag)
				{
					int num2 = this.ITEM_USE_COORDINATE_TABLE_OFFSET + num * this.ITEM_USE_COORDINATE_ENTRY_LENGTH;
					this.romData[num2] = Convert.ToByte(this.nudItemUse2X.Value);
					this.romData[num2 + 1] = Convert.ToByte(this.nudItemUse2Y.Value);
					this.romData[num2 + 2] = Convert.ToByte(this.nudItemUse2Zoom.Value);
					this.romData[num2 + 3] = Convert.ToByte(this.nudItemUse1X.Value);
					this.romData[num2 + 4] = Convert.ToByte(this.nudItemUse1Y.Value);
					MainForm.romData = this.romData;
					this.ResetChangeFlag();
				}
			}
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x0001E4CC File Offset: 0x0001C6CC
		private void LoadCurrentPokemonData()
		{
			this.UpdateItemUseCoordinates();
			this.UpdateDisplay();
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x0001E4DD File Offset: 0x0001C6DD
		private void ResetChangeFlag()
		{
			this.hasUnsavedChanges = false;
			this.btnSave.Enabled = false;
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x0001E4F4 File Offset: 0x0001C6F4
		private void nud_ValueChanged(object sender, EventArgs e)
		{
			this.UpdatePreviewImages();
			this.hasUnsavedChanges = true;
			this.btnSave.Enabled = true;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x0001E512 File Offset: 0x0001C712
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveCurrentPokemonChanges();
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x0001E51C File Offset: 0x0001C71C
		private void ItemUseCoordinate_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.hasUnsavedChanges;
			if (flag)
			{
				DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Cancel)
				{
					if (dialogResult == DialogResult.Yes)
					{
						this.SaveCurrentPokemonChanges();
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x04000212 RID: 530
		public readonly int ITEM_USE_COORDINATE_TABLE_OFFSET;

		// Token: 0x04000213 RID: 531
		public readonly int ITEM_USE_COORDINATE_ENTRY_LENGTH;

		// Token: 0x04000214 RID: 532
		public readonly int ITEM_USE_COORDINATE_COUNT;

		// Token: 0x04000215 RID: 533
		private byte[] romData;

		// Token: 0x04000216 RID: 534
		private bool hasUnsavedChanges;

		// Token: 0x04000217 RID: 535
		private int currentPokemonIndex;

		// Token: 0x04000218 RID: 536
		private Dictionary<int, PokemonData> pokemonIconList;

		// Token: 0x04000219 RID: 537
		private Bitmap itemUseBackGround1;

		// Token: 0x0400021A RID: 538
		private Bitmap itemUseBackGround2;

		// Token: 0x0400021B RID: 539
		private const int BASE_POKEMON_X = 88;

		// Token: 0x0400021C RID: 540
		private const int BASE_POKEMON_Y = 40;

		// Token: 0x0400021D RID: 541
		private const int BASE_ITEM_X = 76;

		// Token: 0x0400021E RID: 542
		private const int BASE_ITEM_Y = 24;
	}
}
