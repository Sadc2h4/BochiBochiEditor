using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x0200001A RID: 26
	public partial class MainForm : Form
	{
		// Token: 0x06000402 RID: 1026 RVA: 0x0001E56C File Offset: 0x0001C76C
		public MainForm()
		{
			base.Load += this.MainForm_Load;
			base.FormClosing += this.MainForm_FormClosing;
			this.FREE_SPACE_FINDER_OFFSET = RomIniReader.ReadHexOrDecimal("FREE_SPACE_FINDER_OFFSET");
			this.editorForm = null;
			this.InitializeComponent();
			AppIconHelper.Apply(this);
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x0001E5C4 File Offset: 0x0001C7C4
		// (set) Token: 0x06000404 RID: 1028 RVA: 0x0001E5CD File Offset: 0x0001C7CD
		public static byte[] romData { get; set; }

		// Token: 0x06000405 RID: 1029 RVA: 0x0001E5D8 File Offset: 0x0001C7D8
		private void MainForm_Load(object sender, EventArgs e)
		{
			this.EnableEditorButtons(false);
			this.SetFreeSpaceFinderEnabled(false);
			this.lblRomInfo.Text = "ROM未選択";
			this.txtFreeSpaceFinderStartAddress.Text = this.FREE_SPACE_FINDER_OFFSET.ToString("X8");
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x0001E628 File Offset: 0x0001C828
		private void btnLoadRom_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "GBA ROMファイル|*.gba";
				openFileDialog.Title = "GBA ROMを選択";
				bool flag = openFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					this.loadedFilePath = openFileDialog.FileName;
					MainForm.romData = File.ReadAllBytes(this.loadedFilePath);
					this.romTitle = this.GetGBARomTitle(MainForm.romData);
					this.lblRomInfo.Text = string.Format("ゲームタイトル : {0}", this.romTitle);
					this.EnableEditorButtons(true);
					this.SetFreeSpaceFinderEnabled(true);
					TextConverter.LoadCharTable("charmap.tbl");
				}
			}
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x0001E6E8 File Offset: 0x0001C8E8
		private string GetGBARomTitle(byte[] romBytes)
		{
			int num = 160;
			int num2 = 18;
			byte[] array = new byte[checked(num2 - 1 + 1)];
			Array.Copy(romBytes, num, array, 0, num2);
			return Encoding.ASCII.GetString(array);
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x0001E728 File Offset: 0x0001C928
		private void EnableEditorButtons(bool enabled)
		{
			this.btnSaveRom.Enabled = enabled;
			this.btnMainPokemonEditor.Enabled = enabled;
			this.btnMainTmHmTutorEditor.Enabled = enabled;
			this.btnMainEggMoveEditor.Enabled = enabled;
			this.btnMainPokedexOrderEditor.Enabled = enabled;
			this.btnMainHabitatEditor.Enabled = enabled;
			this.btnMainPokedexListEditor.Enabled = enabled;
			this.btnMainItemEditor.Enabled = enabled;
			this.btnMainItemUseCoordinate.Enabled = enabled;
			this.btnMainTrainerSpriteEditor.Enabled = enabled;
			this.btnMainTrainerDataEditor.Enabled = enabled;
			this.btnMainInGameTradeEditor.Enabled = enabled;
			this.btnMainHeldItemMailEditor.Enabled = enabled;
			this.btnMainMapEditor.Enabled = enabled;
			this.btnMainTileAnimAndDoorEditor.Enabled = enabled;
			this.btnMainRegionEditor.Enabled = enabled;
			this.btnMainOverWorldEditor.Enabled = enabled;
			this.btnMainWIldPokemonEditor.Enabled = enabled;
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x0001E820 File Offset: 0x0001CA20
		private void SetFreeSpaceFinderEnabled(bool enabled)
		{
			this.txtFreeSpaceFinderStartAddress.Enabled = enabled;
			this.nudFreeSpaceFinderNeededByte.Enabled = enabled;
			this.btnFreeSpaceFinderSearch.Enabled = enabled;
			this.txtFreeSpaceFinderResultAddress.Enabled = enabled;
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x0001E857 File Offset: 0x0001CA57
		private void RestrictMainForm(bool flag)
		{
			this.EnableEditorButtons(flag);
			this.btnLoadRom.Enabled = flag;
			this.btnSaveRom.Enabled = flag;
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x0001E87C File Offset: 0x0001CA7C
		private void btnSaveRom_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = "GBA ROMファイル|*.gba";
				saveFileDialog.Title = "変更を保存するGBA ROMを選択";
				saveFileDialog.FileName = Path.GetFileName(this.loadedFilePath);
				bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
				if (flag)
				{
					File.WriteAllBytes(saveFileDialog.FileName, MainForm.romData);
				}
			}
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x0001E8FC File Offset: 0x0001CAFC
		private void btnFreeSpaceFinderSearch_Click(object sender, EventArgs e)
		{
			bool flag = string.IsNullOrWhiteSpace(this.txtFreeSpaceFinderStartAddress.Text);
			checked
			{
				if (flag)
				{
					MessageBox.Show("開始アドレスを入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					int num;
					try
					{
						num = Convert.ToInt32(this.txtFreeSpaceFinderStartAddress.Text, 16);
					}
					catch (Exception ex)
					{
						MessageBox.Show("開始アドレスは16進数で正しく入力してください。", "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
					int num2 = Convert.ToInt32(this.nudFreeSpaceFinderNeededByte.Value);
					int num3 = num;
					int num4 = -1;
					while (num3 + num2 <= MainForm.romData.Length)
					{
						bool flag2 = true;
						int num5 = num2 - 1;
						for (int i = 0; i <= num5; i++)
						{
							bool flag3 = MainForm.romData[num3 + i] != byte.MaxValue;
							if (flag3)
							{
								flag2 = false;
								num3 += i + 1;
								break;
							}
						}
						bool flag4 = flag2;
						if (flag4)
						{
							num4 = num3;
							bool flag5 = num4 % 4 != 0;
							if (flag5)
							{
								num4 = (num4 + 3) & -4;
							}
							break;
						}
					}
					this.txtFreeSpaceFinderResultAddress.Text = num4.ToString("X8");
				}
			}
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0001EA30 File Offset: 0x0001CC30
		private void OpenEditorForm(Form form)
		{
			bool flag = this.editorForm != null && !this.editorForm.IsDisposed;
			if (flag)
			{
				this.editorForm.Focus();
			}
			else
			{
				this.editorForm = form;
				AppIconHelper.Apply(this.editorForm);
				this.editorForm.FormClosed += this.EditorForm_FormClosed;
				this.RestrictMainForm(false);
				this.editorForm.Show();
			}
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x0001EA9D File Offset: 0x0001CC9D
		private void EditorForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.editorForm.FormClosed -= this.EditorForm_FormClosed;
			this.editorForm = null;
			this.RestrictMainForm(true);
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x0001EAC8 File Offset: 0x0001CCC8
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.editorForm != null && !this.editorForm.IsDisposed;
			if (flag)
			{
				e.Cancel = true;
				MessageBox.Show("編集画面を閉じてください。", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x0001EB10 File Offset: 0x0001CD10
		private void btnMainPokemonEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new PokemonEditor());
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x0001EB1F File Offset: 0x0001CD1F
		private void btnMainTmHmTutorEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new TmHmTutorEditor());
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x0001EB2E File Offset: 0x0001CD2E
		private void btnMainEggMoveEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new EggMoveEditor());
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x0001EB3D File Offset: 0x0001CD3D
		private void btnMainPokedexOrderEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new PokedexOrderEditor());
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x0001EB4C File Offset: 0x0001CD4C
		private void btnMainHabitatEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new HabitatEditor());
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x0001EB5B File Offset: 0x0001CD5B
		private void btnMainPokedexListEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new PokedexListEditor());
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x0001EB6A File Offset: 0x0001CD6A
		private void btnMainItemEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new ItemEditor());
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0001EB79 File Offset: 0x0001CD79
		private void btnMainItemUseCoordinate_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new ItemUseCoordinate());
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0001EB88 File Offset: 0x0001CD88
		private void btnMainTraineSpriteEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new TrainerSpriteEditor());
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x0001EB97 File Offset: 0x0001CD97
		private void btnMainTrainerDataEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new TrainerDataEditor());
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x0001EBA6 File Offset: 0x0001CDA6
		private void btnMainInGameTradeEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new InGameTradeEditor());
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x0001EBB5 File Offset: 0x0001CDB5
		private void btnMainHeldItemMailEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new HeldItemMailEditor());
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0001EBC4 File Offset: 0x0001CDC4
		private void btnMainMapEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new MapEditor());
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x0001EBD3 File Offset: 0x0001CDD3
		private void btnMainOverWorldEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new OverWorldEditor());
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x0001EBE2 File Offset: 0x0001CDE2
		private void btnMainWIldPokemonEditor_Click(object sender, EventArgs e)
		{
			this.OpenEditorForm(new WildPokemonEditor());
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000421 RID: 1057 RVA: 0x0001FF17 File Offset: 0x0001E117
		// (set) Token: 0x06000422 RID: 1058 RVA: 0x0001FF24 File Offset: 0x0001E124
		internal virtual Button btnLoadRom
		{
			[CompilerGenerated]
			get
			{
				return this._btnLoadRom;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnLoadRom_Click);
				Button button = this._btnLoadRom;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnLoadRom = value;
				button = this._btnLoadRom;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000423 RID: 1059 RVA: 0x0001FF67 File Offset: 0x0001E167
		// (set) Token: 0x06000424 RID: 1060 RVA: 0x0001FF71 File Offset: 0x0001E171
		internal virtual GroupBox grpRomInfo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000425 RID: 1061 RVA: 0x0001FF7A File Offset: 0x0001E17A
		// (set) Token: 0x06000426 RID: 1062 RVA: 0x0001FF84 File Offset: 0x0001E184
		internal virtual Button btnMainPokemonEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainPokemonEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainPokemonEditor_Click);
				Button button = this._btnMainPokemonEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainPokemonEditor = value;
				button = this._btnMainPokemonEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x0001FFC7 File Offset: 0x0001E1C7
		// (set) Token: 0x06000428 RID: 1064 RVA: 0x0001FFD1 File Offset: 0x0001E1D1
		internal virtual GroupBox grpMainSelect
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000429 RID: 1065 RVA: 0x0001FFDA File Offset: 0x0001E1DA
		// (set) Token: 0x0600042A RID: 1066 RVA: 0x0001FFE4 File Offset: 0x0001E1E4
		internal virtual Button btnMainTrainerSpriteEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainTrainerSpriteEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainTraineSpriteEditor_Click);
				Button button = this._btnMainTrainerSpriteEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainTrainerSpriteEditor = value;
				button = this._btnMainTrainerSpriteEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x00020027 File Offset: 0x0001E227
		// (set) Token: 0x0600042C RID: 1068 RVA: 0x00020034 File Offset: 0x0001E234
		internal virtual Button btnMainItemEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainItemEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainItemEditor_Click);
				Button button = this._btnMainItemEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainItemEditor = value;
				button = this._btnMainItemEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x0600042D RID: 1069 RVA: 0x00020077 File Offset: 0x0001E277
		// (set) Token: 0x0600042E RID: 1070 RVA: 0x00020084 File Offset: 0x0001E284
		internal virtual Button btnMainHabitatEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainHabitatEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainHabitatEditor_Click);
				Button button = this._btnMainHabitatEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainHabitatEditor = value;
				button = this._btnMainHabitatEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x0600042F RID: 1071 RVA: 0x000200C7 File Offset: 0x0001E2C7
		// (set) Token: 0x06000430 RID: 1072 RVA: 0x000200D4 File Offset: 0x0001E2D4
		internal virtual Button btnMainTmHmTutorEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainTmHmTutorEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainTmHmTutorEditor_Click);
				Button button = this._btnMainTmHmTutorEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainTmHmTutorEditor = value;
				button = this._btnMainTmHmTutorEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x06000431 RID: 1073 RVA: 0x00020117 File Offset: 0x0001E317
		// (set) Token: 0x06000432 RID: 1074 RVA: 0x00020124 File Offset: 0x0001E324
		internal virtual Button btnMainEggMoveEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainEggMoveEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainEggMoveEditor_Click);
				Button button = this._btnMainEggMoveEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainEggMoveEditor = value;
				button = this._btnMainEggMoveEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x06000433 RID: 1075 RVA: 0x00020167 File Offset: 0x0001E367
		// (set) Token: 0x06000434 RID: 1076 RVA: 0x00020174 File Offset: 0x0001E374
		internal virtual Button btnMainPokedexOrderEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainPokedexOrderEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainPokedexOrderEditor_Click);
				Button button = this._btnMainPokedexOrderEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainPokedexOrderEditor = value;
				button = this._btnMainPokedexOrderEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000435 RID: 1077 RVA: 0x000201B7 File Offset: 0x0001E3B7
		// (set) Token: 0x06000436 RID: 1078 RVA: 0x000201C1 File Offset: 0x0001E3C1
		internal virtual OpenFileDialog OpenFileDialog1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000437 RID: 1079 RVA: 0x000201CA File Offset: 0x0001E3CA
		// (set) Token: 0x06000438 RID: 1080 RVA: 0x000201D4 File Offset: 0x0001E3D4
		internal virtual Label lblRomInfo
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000439 RID: 1081 RVA: 0x000201DD File Offset: 0x0001E3DD
		// (set) Token: 0x0600043A RID: 1082 RVA: 0x000201E8 File Offset: 0x0001E3E8
		internal virtual Button btnSaveRom
		{
			[CompilerGenerated]
			get
			{
				return this._btnSaveRom;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnSaveRom_Click);
				Button button = this._btnSaveRom;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnSaveRom = value;
				button = this._btnSaveRom;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x0002022B File Offset: 0x0001E42B
		// (set) Token: 0x0600043C RID: 1084 RVA: 0x00020235 File Offset: 0x0001E435
		internal virtual Label lblVersion
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x0002023E File Offset: 0x0001E43E
		// (set) Token: 0x0600043E RID: 1086 RVA: 0x00020248 File Offset: 0x0001E448
		internal virtual Label lblCopyright
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x0600043F RID: 1087 RVA: 0x00020251 File Offset: 0x0001E451
		// (set) Token: 0x06000440 RID: 1088 RVA: 0x0002025C File Offset: 0x0001E45C
		internal virtual Button btnMainTrainerDataEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainTrainerDataEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainTrainerDataEditor_Click);
				Button button = this._btnMainTrainerDataEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainTrainerDataEditor = value;
				button = this._btnMainTrainerDataEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000441 RID: 1089 RVA: 0x0002029F File Offset: 0x0001E49F
		// (set) Token: 0x06000442 RID: 1090 RVA: 0x000202AC File Offset: 0x0001E4AC
		internal virtual Button btnMainPokedexListEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainPokedexListEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainPokedexListEditor_Click);
				Button button = this._btnMainPokedexListEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainPokedexListEditor = value;
				button = this._btnMainPokedexListEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000443 RID: 1091 RVA: 0x000202EF File Offset: 0x0001E4EF
		// (set) Token: 0x06000444 RID: 1092 RVA: 0x000202FC File Offset: 0x0001E4FC
		internal virtual Button btnMainWIldPokemonEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainWIldPokemonEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainWIldPokemonEditor_Click);
				Button button = this._btnMainWIldPokemonEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainWIldPokemonEditor = value;
				button = this._btnMainWIldPokemonEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000445 RID: 1093 RVA: 0x0002033F File Offset: 0x0001E53F
		// (set) Token: 0x06000446 RID: 1094 RVA: 0x0002034C File Offset: 0x0001E54C
		internal virtual Button btnMainInGameTradeEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainInGameTradeEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainInGameTradeEditor_Click);
				Button button = this._btnMainInGameTradeEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainInGameTradeEditor = value;
				button = this._btnMainInGameTradeEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000447 RID: 1095 RVA: 0x0002038F File Offset: 0x0001E58F
		// (set) Token: 0x06000448 RID: 1096 RVA: 0x0002039C File Offset: 0x0001E59C
		internal virtual Button btnMainOverWorldEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainOverWorldEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainOverWorldEditor_Click);
				Button button = this._btnMainOverWorldEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainOverWorldEditor = value;
				button = this._btnMainOverWorldEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000449 RID: 1097 RVA: 0x000203DF File Offset: 0x0001E5DF
		// (set) Token: 0x0600044A RID: 1098 RVA: 0x000203EC File Offset: 0x0001E5EC
		internal virtual Button btnMainItemUseCoordinate
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainItemUseCoordinate;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainItemUseCoordinate_Click);
				Button button = this._btnMainItemUseCoordinate;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainItemUseCoordinate = value;
				button = this._btnMainItemUseCoordinate;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x0600044B RID: 1099 RVA: 0x0002042F File Offset: 0x0001E62F
		// (set) Token: 0x0600044C RID: 1100 RVA: 0x0002043C File Offset: 0x0001E63C
		internal virtual Button btnMainHeldItemMailEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainHeldItemMailEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainHeldItemMailEditor_Click);
				Button button = this._btnMainHeldItemMailEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainHeldItemMailEditor = value;
				button = this._btnMainHeldItemMailEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x0600044D RID: 1101 RVA: 0x0002047F File Offset: 0x0001E67F
		// (set) Token: 0x0600044E RID: 1102 RVA: 0x00020489 File Offset: 0x0001E689
		internal virtual Label lblFreeSpaceFinderResultAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x0600044F RID: 1103 RVA: 0x00020492 File Offset: 0x0001E692
		// (set) Token: 0x06000450 RID: 1104 RVA: 0x0002049C File Offset: 0x0001E69C
		internal virtual Button btnFreeSpaceFinderSearch
		{
			[CompilerGenerated]
			get
			{
				return this._btnFreeSpaceFinderSearch;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnFreeSpaceFinderSearch_Click);
				Button button = this._btnFreeSpaceFinderSearch;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnFreeSpaceFinderSearch = value;
				button = this._btnFreeSpaceFinderSearch;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000451 RID: 1105 RVA: 0x000204DF File Offset: 0x0001E6DF
		// (set) Token: 0x06000452 RID: 1106 RVA: 0x000204E9 File Offset: 0x0001E6E9
		internal virtual TextBox txtFreeSpaceFinderResultAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000453 RID: 1107 RVA: 0x000204F2 File Offset: 0x0001E6F2
		// (set) Token: 0x06000454 RID: 1108 RVA: 0x000204FC File Offset: 0x0001E6FC
		internal virtual Label lblFreeSpaceFinderNeededByte
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000455 RID: 1109 RVA: 0x00020505 File Offset: 0x0001E705
		// (set) Token: 0x06000456 RID: 1110 RVA: 0x0002050F File Offset: 0x0001E70F
		internal virtual NumericUpDown nudFreeSpaceFinderNeededByte
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000457 RID: 1111 RVA: 0x00020518 File Offset: 0x0001E718
		// (set) Token: 0x06000458 RID: 1112 RVA: 0x00020522 File Offset: 0x0001E722
		internal virtual Label lblFreeSpaceFinderStartAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000459 RID: 1113 RVA: 0x0002052B File Offset: 0x0001E72B
		// (set) Token: 0x0600045A RID: 1114 RVA: 0x00020535 File Offset: 0x0001E735
		internal virtual TextBox txtFreeSpaceFinderStartAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x0600045B RID: 1115 RVA: 0x0002053E File Offset: 0x0001E73E
		// (set) Token: 0x0600045C RID: 1116 RVA: 0x00020548 File Offset: 0x0001E748
		internal virtual GroupBox grpFreeSpaceFinder
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x0600045D RID: 1117 RVA: 0x00020551 File Offset: 0x0001E751
		// (set) Token: 0x0600045E RID: 1118 RVA: 0x0002055B File Offset: 0x0001E75B
		internal virtual Button btnMainRegionEditor
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x0600045F RID: 1119 RVA: 0x00020564 File Offset: 0x0001E764
		// (set) Token: 0x06000460 RID: 1120 RVA: 0x0002056E File Offset: 0x0001E76E
		internal virtual Button btnMainTileAnimAndDoorEditor
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000461 RID: 1121 RVA: 0x00020577 File Offset: 0x0001E777
		// (set) Token: 0x06000462 RID: 1122 RVA: 0x00020584 File Offset: 0x0001E784
		internal virtual Button btnMainMapEditor
		{
			[CompilerGenerated]
			get
			{
				return this._btnMainMapEditor;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnMainMapEditor_Click);
				Button button = this._btnMainMapEditor;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnMainMapEditor = value;
				button = this._btnMainMapEditor;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x0400021F RID: 543
		public readonly int FREE_SPACE_FINDER_OFFSET;

		// Token: 0x04000221 RID: 545
		private string romTitle;

		// Token: 0x04000222 RID: 546
		private string loadedFilePath;

		// Token: 0x04000223 RID: 547
		private Form editorForm;
	}
}
