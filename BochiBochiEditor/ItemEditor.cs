using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor
{
	// Token: 0x02000018 RID: 24
	public partial class ItemEditor : Form
	{
		// Token: 0x0600032B RID: 811 RVA: 0x00018288 File Offset: 0x00016488
		public ItemEditor()
		{
			base.Load += this.ItemEditor_Load;
			base.FormClosing += this.ItemEditor_FormClosing;
			this.ITEM_EFFECT_ADDRESS_TABLE_OFFSET = RomIniReader.ReadHexOrDecimal("ITEM_EFFECT_ADDRESS_TABLE_OFFSET");
			this.ITEM_EFFECT_ADDRESS_FIRST_INDEX = RomIniReader.ReadHexOrDecimal("ITEM_EFFECT_ADDRESS_FIRST_INDEX");
			this.ITEM_EFFECT_ADDRESS_LAST_INDEX = RomIniReader.ReadHexOrDecimal("ITEM_EFFECT_ADDRESS_LAST_INDEX");
			this.isDataChanged = false;
			this.currentSelectedIndex = -1;
			this.itemDescriptionData = new Dictionary<int, string>();
			this.InitializeComponent();
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600032E RID: 814 RVA: 0x0001A582 File Offset: 0x00018782
		// (set) Token: 0x0600032F RID: 815 RVA: 0x0001A58C File Offset: 0x0001878C
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

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x06000330 RID: 816 RVA: 0x0001A5CF File Offset: 0x000187CF
		// (set) Token: 0x06000331 RID: 817 RVA: 0x0001A5D9 File Offset: 0x000187D9
		internal virtual GroupBox grpItemName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000332 RID: 818 RVA: 0x0001A5E2 File Offset: 0x000187E2
		// (set) Token: 0x06000333 RID: 819 RVA: 0x0001A5EC File Offset: 0x000187EC
		internal virtual ComboBox cmbItemName
		{
			[CompilerGenerated]
			get
			{
				return this._cmbItemName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbItemName_SelectedIndexChanged);
				ComboBox comboBox = this._cmbItemName;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbItemName = value;
				comboBox = this._cmbItemName;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0001A62F File Offset: 0x0001882F
		// (set) Token: 0x06000335 RID: 821 RVA: 0x0001A63C File Offset: 0x0001883C
		internal virtual Button btnChangeItemName
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeItemName;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeItemName_Click);
				Button button = this._btnChangeItemName;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeItemName = value;
				button = this._btnChangeItemName;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000336 RID: 822 RVA: 0x0001A67F File Offset: 0x0001887F
		// (set) Token: 0x06000337 RID: 823 RVA: 0x0001A689 File Offset: 0x00018889
		internal virtual TextBox txtItemName
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000338 RID: 824 RVA: 0x0001A692 File Offset: 0x00018892
		// (set) Token: 0x06000339 RID: 825 RVA: 0x0001A69C File Offset: 0x0001889C
		internal virtual GroupBox grpItemSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600033A RID: 826 RVA: 0x0001A6A5 File Offset: 0x000188A5
		// (set) Token: 0x0600033B RID: 827 RVA: 0x0001A6AF File Offset: 0x000188AF
		internal virtual PictureBox picItemSprite
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x0600033C RID: 828 RVA: 0x0001A6B8 File Offset: 0x000188B8
		// (set) Token: 0x0600033D RID: 829 RVA: 0x0001A6C4 File Offset: 0x000188C4
		internal virtual TextBox txtItemSpritePalleteAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtItemSpritePalleteAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtItemSpritePalleteAddress_Enter);
				TextBox textBox = this._txtItemSpritePalleteAddress;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtItemSpritePalleteAddress = value;
				textBox = this._txtItemSpritePalleteAddress;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x0600033E RID: 830 RVA: 0x0001A707 File Offset: 0x00018907
		// (set) Token: 0x0600033F RID: 831 RVA: 0x0001A714 File Offset: 0x00018914
		internal virtual TextBox txtItemSpriteImageAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtItemSpriteImageAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtItemSpriteImageAddress_Enter);
				TextBox textBox = this._txtItemSpriteImageAddress;
				if (textBox != null)
				{
					textBox.Enter -= eventHandler;
				}
				this._txtItemSpriteImageAddress = value;
				textBox = this._txtItemSpriteImageAddress;
				if (textBox != null)
				{
					textBox.Enter += eventHandler;
				}
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000340 RID: 832 RVA: 0x0001A757 File Offset: 0x00018957
		// (set) Token: 0x06000341 RID: 833 RVA: 0x0001A764 File Offset: 0x00018964
		internal virtual Button btnChangeItemSpriteAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeItemSpriteAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeItemSpriteAddress_Click);
				Button button = this._btnChangeItemSpriteAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeItemSpriteAddress = value;
				button = this._btnChangeItemSpriteAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000342 RID: 834 RVA: 0x0001A7A7 File Offset: 0x000189A7
		// (set) Token: 0x06000343 RID: 835 RVA: 0x0001A7B1 File Offset: 0x000189B1
		internal virtual GroupBox grpItemDescription
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000344 RID: 836 RVA: 0x0001A7BA File Offset: 0x000189BA
		// (set) Token: 0x06000345 RID: 837 RVA: 0x0001A7C4 File Offset: 0x000189C4
		internal virtual Button btnChangeItemDescriptionAddress
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeItemDescriptionAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeItemDescriptionAddress_Click);
				Button button = this._btnChangeItemDescriptionAddress;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeItemDescriptionAddress = value;
				button = this._btnChangeItemDescriptionAddress;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000346 RID: 838 RVA: 0x0001A807 File Offset: 0x00018A07
		// (set) Token: 0x06000347 RID: 839 RVA: 0x0001A811 File Offset: 0x00018A11
		internal virtual TextBox txtItemDescriptionAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000348 RID: 840 RVA: 0x0001A81A File Offset: 0x00018A1A
		// (set) Token: 0x06000349 RID: 841 RVA: 0x0001A824 File Offset: 0x00018A24
		internal virtual TextBox txtItemDescription
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600034A RID: 842 RVA: 0x0001A82D File Offset: 0x00018A2D
		// (set) Token: 0x0600034B RID: 843 RVA: 0x0001A838 File Offset: 0x00018A38
		internal virtual Button btnChangeItemDescription
		{
			[CompilerGenerated]
			get
			{
				return this._btnChangeItemDescription;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnChangeItemDescription_Click);
				Button button = this._btnChangeItemDescription;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnChangeItemDescription = value;
				button = this._btnChangeItemDescription;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x0600034C RID: 844 RVA: 0x0001A87B File Offset: 0x00018A7B
		// (set) Token: 0x0600034D RID: 845 RVA: 0x0001A885 File Offset: 0x00018A85
		internal virtual GroupBox grpItemData
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x0600034E RID: 846 RVA: 0x0001A88E File Offset: 0x00018A8E
		// (set) Token: 0x0600034F RID: 847 RVA: 0x0001A898 File Offset: 0x00018A98
		internal virtual Label lblItemPrice
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000350 RID: 848 RVA: 0x0001A8A1 File Offset: 0x00018AA1
		// (set) Token: 0x06000351 RID: 849 RVA: 0x0001A8AB File Offset: 0x00018AAB
		internal virtual Label lblItemID
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000352 RID: 850 RVA: 0x0001A8B4 File Offset: 0x00018AB4
		// (set) Token: 0x06000353 RID: 851 RVA: 0x0001A8BE File Offset: 0x00018ABE
		internal virtual Label lblUnknownValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000354 RID: 852 RVA: 0x0001A8C7 File Offset: 0x00018AC7
		// (set) Token: 0x06000355 RID: 853 RVA: 0x0001A8D1 File Offset: 0x00018AD1
		internal virtual Label lblItemHoldableValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x06000356 RID: 854 RVA: 0x0001A8DA File Offset: 0x00018ADA
		// (set) Token: 0x06000357 RID: 855 RVA: 0x0001A8E4 File Offset: 0x00018AE4
		internal virtual ComboBox cmbItemHoldableValue
		{
			[CompilerGenerated]
			get
			{
				return this._cmbItemHoldableValue;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbItemHoldableValue_SelectedIndexChanged);
				ComboBox comboBox = this._cmbItemHoldableValue;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbItemHoldableValue = value;
				comboBox = this._cmbItemHoldableValue;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x06000358 RID: 856 RVA: 0x0001A927 File Offset: 0x00018B27
		// (set) Token: 0x06000359 RID: 857 RVA: 0x0001A931 File Offset: 0x00018B31
		internal virtual Label lblExtraDescription1
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x0600035A RID: 858 RVA: 0x0001A93A File Offset: 0x00018B3A
		// (set) Token: 0x0600035B RID: 859 RVA: 0x0001A944 File Offset: 0x00018B44
		internal virtual GroupBox grpItemField
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0001A94D File Offset: 0x00018B4D
		// (set) Token: 0x0600035D RID: 861 RVA: 0x0001A958 File Offset: 0x00018B58
		internal virtual ComboBox cmbItemFieldType
		{
			[CompilerGenerated]
			get
			{
				return this._cmbItemFieldType;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbItemFieldType_SelectedIndexChanged);
				ComboBox comboBox = this._cmbItemFieldType;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbItemFieldType = value;
				comboBox = this._cmbItemFieldType;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x0600035E RID: 862 RVA: 0x0001A99B File Offset: 0x00018B9B
		// (set) Token: 0x0600035F RID: 863 RVA: 0x0001A9A5 File Offset: 0x00018BA5
		internal virtual Label lblItemFieldType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000360 RID: 864 RVA: 0x0001A9AE File Offset: 0x00018BAE
		// (set) Token: 0x06000361 RID: 865 RVA: 0x0001A9B8 File Offset: 0x00018BB8
		internal virtual TextBox txtItemFieldAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtItemFieldAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtItemFieldAddress_TextChanged);
				TextBox textBox = this._txtItemFieldAddress;
				if (textBox != null)
				{
					textBox.TextChanged -= eventHandler;
				}
				this._txtItemFieldAddress = value;
				textBox = this._txtItemFieldAddress;
				if (textBox != null)
				{
					textBox.TextChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000362 RID: 866 RVA: 0x0001A9FB File Offset: 0x00018BFB
		// (set) Token: 0x06000363 RID: 867 RVA: 0x0001AA05 File Offset: 0x00018C05
		internal virtual Label lblItemFieldAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000364 RID: 868 RVA: 0x0001AA0E File Offset: 0x00018C0E
		// (set) Token: 0x06000365 RID: 869 RVA: 0x0001AA18 File Offset: 0x00018C18
		internal virtual ComboBox cmbItemPocket
		{
			[CompilerGenerated]
			get
			{
				return this._cmbItemPocket;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbItemPocket_SelectedIndexChanged);
				ComboBox comboBox = this._cmbItemPocket;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbItemPocket = value;
				comboBox = this._cmbItemPocket;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06000366 RID: 870 RVA: 0x0001AA5B File Offset: 0x00018C5B
		// (set) Token: 0x06000367 RID: 871 RVA: 0x0001AA65 File Offset: 0x00018C65
		internal virtual Label lblItemPocket
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000368 RID: 872 RVA: 0x0001AA6E File Offset: 0x00018C6E
		// (set) Token: 0x06000369 RID: 873 RVA: 0x0001AA78 File Offset: 0x00018C78
		internal virtual GroupBox grpItemBattle
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x0600036A RID: 874 RVA: 0x0001AA81 File Offset: 0x00018C81
		// (set) Token: 0x0600036B RID: 875 RVA: 0x0001AA8C File Offset: 0x00018C8C
		internal virtual ComboBox cmbItemBattleType
		{
			[CompilerGenerated]
			get
			{
				return this._cmbItemBattleType;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbItemBattleType_SelectedIndexChanged);
				ComboBox comboBox = this._cmbItemBattleType;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbItemBattleType = value;
				comboBox = this._cmbItemBattleType;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x0600036C RID: 876 RVA: 0x0001AACF File Offset: 0x00018CCF
		// (set) Token: 0x0600036D RID: 877 RVA: 0x0001AAD9 File Offset: 0x00018CD9
		internal virtual Label lblItemBattleType
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x0600036E RID: 878 RVA: 0x0001AAE2 File Offset: 0x00018CE2
		// (set) Token: 0x0600036F RID: 879 RVA: 0x0001AAEC File Offset: 0x00018CEC
		internal virtual TextBox txtItemBattleAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtItemBattleAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtItemBattleAddress_TextChanged);
				TextBox textBox = this._txtItemBattleAddress;
				if (textBox != null)
				{
					textBox.TextChanged -= eventHandler;
				}
				this._txtItemBattleAddress = value;
				textBox = this._txtItemBattleAddress;
				if (textBox != null)
				{
					textBox.TextChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000370 RID: 880 RVA: 0x0001AB2F File Offset: 0x00018D2F
		// (set) Token: 0x06000371 RID: 881 RVA: 0x0001AB39 File Offset: 0x00018D39
		internal virtual Label lblItemBattleAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000372 RID: 882 RVA: 0x0001AB42 File Offset: 0x00018D42
		// (set) Token: 0x06000373 RID: 883 RVA: 0x0001AB4C File Offset: 0x00018D4C
		internal virtual Label lblExtraDescription2
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000374 RID: 884 RVA: 0x0001AB55 File Offset: 0x00018D55
		// (set) Token: 0x06000375 RID: 885 RVA: 0x0001AB5F File Offset: 0x00018D5F
		internal virtual Label lblExtraDescription3
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000376 RID: 886 RVA: 0x0001AB68 File Offset: 0x00018D68
		// (set) Token: 0x06000377 RID: 887 RVA: 0x0001AB72 File Offset: 0x00018D72
		internal virtual Label lblSpecialValue
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000378 RID: 888 RVA: 0x0001AB7B File Offset: 0x00018D7B
		// (set) Token: 0x06000379 RID: 889 RVA: 0x0001AB85 File Offset: 0x00018D85
		internal virtual Label lblItemPriceYen
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x0600037A RID: 890 RVA: 0x0001AB8E File Offset: 0x00018D8E
		// (set) Token: 0x0600037B RID: 891 RVA: 0x0001AB98 File Offset: 0x00018D98
		internal virtual TextBox txtItemId16
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x0600037C RID: 892 RVA: 0x0001ABA1 File Offset: 0x00018DA1
		// (set) Token: 0x0600037D RID: 893 RVA: 0x0001ABAC File Offset: 0x00018DAC
		internal virtual NumericUpDown nudItemPrice
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemPrice;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudItemPrice_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemPrice;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemPrice = value;
				numericUpDown = this._nudItemPrice;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0001ABEF File Offset: 0x00018DEF
		// (set) Token: 0x0600037F RID: 895 RVA: 0x0001ABFC File Offset: 0x00018DFC
		internal virtual NumericUpDown nudItemId
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemId;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudItemId_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemId;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemId = value;
				numericUpDown = this._nudItemId;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000380 RID: 896 RVA: 0x0001AC3F File Offset: 0x00018E3F
		// (set) Token: 0x06000381 RID: 897 RVA: 0x0001AC4C File Offset: 0x00018E4C
		internal virtual NumericUpDown nudSpecialValue
		{
			[CompilerGenerated]
			get
			{
				return this._nudSpecialValue;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudSpecialValue_ValueChanged);
				NumericUpDown numericUpDown = this._nudSpecialValue;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudSpecialValue = value;
				numericUpDown = this._nudSpecialValue;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000382 RID: 898 RVA: 0x0001AC8F File Offset: 0x00018E8F
		// (set) Token: 0x06000383 RID: 899 RVA: 0x0001AC9C File Offset: 0x00018E9C
		internal virtual NumericUpDown nudUnknownValue
		{
			[CompilerGenerated]
			get
			{
				return this._nudUnknownValue;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudUnknownValue_ValueChanged);
				NumericUpDown numericUpDown = this._nudUnknownValue;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudUnknownValue = value;
				numericUpDown = this._nudUnknownValue;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000384 RID: 900 RVA: 0x0001ACDF File Offset: 0x00018EDF
		// (set) Token: 0x06000385 RID: 901 RVA: 0x0001ACEC File Offset: 0x00018EEC
		internal virtual NumericUpDown nudItemFieldType
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemFieldType;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudItemFieldType_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemFieldType;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemFieldType = value;
				numericUpDown = this._nudItemFieldType;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000386 RID: 902 RVA: 0x0001AD2F File Offset: 0x00018F2F
		// (set) Token: 0x06000387 RID: 903 RVA: 0x0001AD3C File Offset: 0x00018F3C
		internal virtual NumericUpDown nudItemEffectParam
		{
			[CompilerGenerated]
			get
			{
				return this._nudItemEffectParam;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.nudItemEffectParam_ValueChanged);
				NumericUpDown numericUpDown = this._nudItemEffectParam;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged -= eventHandler;
				}
				this._nudItemEffectParam = value;
				numericUpDown = this._nudItemEffectParam;
				if (numericUpDown != null)
				{
					numericUpDown.ValueChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000388 RID: 904 RVA: 0x0001AD7F File Offset: 0x00018F7F
		// (set) Token: 0x06000389 RID: 905 RVA: 0x0001AD8C File Offset: 0x00018F8C
		internal virtual ComboBox cmbItemHoldEffectId
		{
			[CompilerGenerated]
			get
			{
				return this._cmbItemHoldEffectId;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.cmbItemHoldEffectId_SelectedIndexChanged);
				ComboBox comboBox = this._cmbItemHoldEffectId;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged -= eventHandler;
				}
				this._cmbItemHoldEffectId = value;
				comboBox = this._cmbItemHoldEffectId;
				if (comboBox != null)
				{
					comboBox.SelectedIndexChanged += eventHandler;
				}
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x0600038A RID: 906 RVA: 0x0001ADCF File Offset: 0x00018FCF
		// (set) Token: 0x0600038B RID: 907 RVA: 0x0001ADD9 File Offset: 0x00018FD9
		internal virtual Label lblItemEffectParam
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x0600038C RID: 908 RVA: 0x0001ADE2 File Offset: 0x00018FE2
		// (set) Token: 0x0600038D RID: 909 RVA: 0x0001ADEC File Offset: 0x00018FEC
		internal virtual Label lblItemHoldEffectId
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x0600038E RID: 910 RVA: 0x0001ADF5 File Offset: 0x00018FF5
		// (set) Token: 0x0600038F RID: 911 RVA: 0x0001ADFF File Offset: 0x00018FFF
		internal virtual GroupBox grpItemEffectAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000390 RID: 912 RVA: 0x0001AE08 File Offset: 0x00019008
		// (set) Token: 0x06000391 RID: 913 RVA: 0x0001AE14 File Offset: 0x00019014
		internal virtual TextBox txtItemEffectAddress
		{
			[CompilerGenerated]
			get
			{
				return this._txtItemEffectAddress;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.txtItemEffectAddress_TextChanged);
				TextBox textBox = this._txtItemEffectAddress;
				if (textBox != null)
				{
					textBox.TextChanged -= eventHandler;
				}
				this._txtItemEffectAddress = value;
				textBox = this._txtItemEffectAddress;
				if (textBox != null)
				{
					textBox.TextChanged += eventHandler;
				}
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000392 RID: 914 RVA: 0x0001AE57 File Offset: 0x00019057
		// (set) Token: 0x06000393 RID: 915 RVA: 0x0001AE61 File Offset: 0x00019061
		internal virtual Label lblItemEffectAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000394 RID: 916 RVA: 0x0001AE6A File Offset: 0x0001906A
		// (set) Token: 0x06000395 RID: 917 RVA: 0x0001AE74 File Offset: 0x00019074
		internal virtual TextBox txtItemIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000396 RID: 918 RVA: 0x0001AE7D File Offset: 0x0001907D
		// (set) Token: 0x06000397 RID: 919 RVA: 0x0001AE87 File Offset: 0x00019087
		internal virtual Label lblItemIndex
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000398 RID: 920 RVA: 0x0001AE90 File Offset: 0x00019090
		// (set) Token: 0x06000399 RID: 921 RVA: 0x0001AE9C File Offset: 0x0001909C
		internal virtual Button btnItemSpriteExport
		{
			[CompilerGenerated]
			get
			{
				return this._btnItemSpriteExport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnItemSpriteExport_Click);
				Button button = this._btnItemSpriteExport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnItemSpriteExport = value;
				button = this._btnItemSpriteExport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x0600039A RID: 922 RVA: 0x0001AEDF File Offset: 0x000190DF
		// (set) Token: 0x0600039B RID: 923 RVA: 0x0001AEE9 File Offset: 0x000190E9
		internal virtual TextBox txtItemSpriteImportAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x0600039C RID: 924 RVA: 0x0001AEF2 File Offset: 0x000190F2
		// (set) Token: 0x0600039D RID: 925 RVA: 0x0001AEFC File Offset: 0x000190FC
		internal virtual GroupBox grpItemSpritePreview
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600039E RID: 926 RVA: 0x0001AF05 File Offset: 0x00019105
		// (set) Token: 0x0600039F RID: 927 RVA: 0x0001AF0F File Offset: 0x0001910F
		internal virtual RadioButton rbItemSpritePalleteAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x060003A0 RID: 928 RVA: 0x0001AF18 File Offset: 0x00019118
		// (set) Token: 0x060003A1 RID: 929 RVA: 0x0001AF22 File Offset: 0x00019122
		internal virtual RadioButton rbItemSpriteImageAddress
		{
			get; [MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x060003A2 RID: 930 RVA: 0x0001AF2B File Offset: 0x0001912B
		// (set) Token: 0x060003A3 RID: 931 RVA: 0x0001AF38 File Offset: 0x00019138
		internal virtual Button btnItemSpriteImport
		{
			[CompilerGenerated]
			get
			{
				return this._btnItemSpriteImport;
			}
			[CompilerGenerated]
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				EventHandler eventHandler = new EventHandler(this.btnItemSpriteImport_Click);
				Button button = this._btnItemSpriteImport;
				if (button != null)
				{
					button.Click -= eventHandler;
				}
				this._btnItemSpriteImport = value;
				button = this._btnItemSpriteImport;
				if (button != null)
				{
					button.Click += eventHandler;
				}
			}
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x0001AF7C File Offset: 0x0001917C
		private void ItemEditor_Load(object sender, EventArgs e)
		{
			this.btnSave.Enabled = false;
			this.isDataChanged = false;
			this.itemInfoData = new List<ItemData.ItemInfo>();
			List<string> list = new List<string>();
			ushort num = checked((ushort)(ItemData.TOTAL_ITEM_COUNT - 1));
			for (ushort num2 = 0; num2 <= num; num2 += 1)
			{
				ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(MainForm.romData, num2);
				this.itemInfoData.Add(itemInfo);
				list.Add(itemInfo.Name);
				this.itemDescriptionData.Add((int)num2, this.GetItemDescription(itemInfo.DescriptionAddress));
			}
			this.cmbItemName.Items.AddRange(list.ToArray());
			this.LoadHoldEffectList();
			this.cmbItemName.SelectedIndex = 0;
			this.rbItemSpriteImageAddress.Checked = true;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x0001B03C File Offset: 0x0001923C
		private void cmbItemName_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = this.cmbItemName.SelectedIndex == this.currentSelectedIndex;
			if (!flag)
			{
				bool flag2 = this.isDataChanged;
				if (flag2)
				{
					DialogResult dialogResult = MessageBox.Show("保存されていない変更があります。変更を保存しますか？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					bool flag3 = dialogResult == DialogResult.Yes;
					if (flag3)
					{
						this.SaveChanges();
					}
					else
					{
						bool flag4 = dialogResult == DialogResult.No;
						if (!flag4)
						{
							this.cmbItemName.SelectedIndex = this.currentSelectedIndex;
							return;
						}
						this.RevertChanges();
					}
				}
				bool flag5 = this.currentSelectedIndex >= 0 && this.currentSelectedIndex < this.itemInfoData.Count;
				if (flag5)
				{
					ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
					itemInfo.TemporaryImageData = null;
					itemInfo.TemporaryPaletteData = null;
					this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				}
				this.currentSelectedIndex = this.cmbItemName.SelectedIndex;
				this.DisplayItemData(this.currentSelectedIndex);
				this.txtItemSpriteImportAddress.Text = "";
				this.isDataChanged = false;
				this.btnSave.Enabled = false;
			}
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0001B164 File Offset: 0x00019364
		private void btnChangeItemName_Click(object sender, EventArgs e)
		{
			string text = this.txtItemName.Text;
			bool flag = text.Length > ItemData.ITEM_NAME_MAX_DISPLAY_LENGTH;
			if (flag)
			{
				text = text.Substring(0, ItemData.ITEM_NAME_MAX_DISPLAY_LENGTH);
				this.txtItemName.Text = text;
			}
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			bool flag2 = Operators.CompareString(itemInfo.Name, text, false) == 0;
			if (!flag2)
			{
				itemInfo.Name = text;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.cmbItemName.Items[this.currentSelectedIndex] = text;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x0001B210 File Offset: 0x00019410
		private void SetDataChanged()
		{
			bool flag = !this.isDataChanged;
			if (flag)
			{
				this.isDataChanged = true;
				this.btnSave.Enabled = true;
			}
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0001B241 File Offset: 0x00019441
		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveChanges();
			this.isDataChanged = false;
			this.btnSave.Enabled = false;
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0001B260 File Offset: 0x00019460
		private void ItemEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = this.isDataChanged;
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

		// Token: 0x060003AA RID: 938 RVA: 0x0001B2B0 File Offset: 0x000194B0
		private void DisplayItemData(int id)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[id];
			this.txtItemIndex.Text = itemInfo.Index.ToString("X4");
			this.txtItemName.Text = itemInfo.Name;
			this.txtItemSpriteImageAddress.Text = itemInfo.ImageAddress.ToString("X8");
			this.txtItemSpritePalleteAddress.Text = itemInfo.PaletteAddress.ToString("X8");
			this.DisplayItemImage(id);
			this.txtItemDescriptionAddress.Text = itemInfo.DescriptionAddress.ToString("X8");
			this.txtItemDescription.Text = this.itemDescriptionData[id];
			this.nudItemId.Text = itemInfo.ItemId.ToString();
			this.txtItemId16.Text = itemInfo.ItemId.ToString("X4");
			this.nudItemPrice.Text = itemInfo.Price.ToString();
			this.cmbItemHoldEffectId.SelectedIndex = (int)itemInfo.HeldEffectId;
			this.nudItemEffectParam.Value = new decimal((int)itemInfo.EffectValue);
			this.cmbItemHoldableValue.SelectedIndex = (int)itemInfo.CanHold;
			this.nudUnknownValue.Value = new decimal((int)itemInfo.UnknownValue);
			this.cmbItemPocket.SelectedIndex = (int)(checked(itemInfo.PocketId - 1));
			this.nudItemFieldType.Value = new decimal((int)itemInfo.FieldUseType);
			this.txtItemFieldAddress.Text = itemInfo.FieldUseAddress.ToString("X8");
			this.UpdateFieldUseTypeControls(itemInfo.PocketId);
			this.cmbItemBattleType.SelectedIndex = (int)itemInfo.BattleUseType;
			this.txtItemBattleAddress.Text = itemInfo.BattleUseAddress.ToString("X8");
			this.nudSpecialValue.Value = new decimal((int)itemInfo.SpecialValue);
			this.DisplayItemEffectAddress(id);
		}

		// Token: 0x060003AB RID: 939 RVA: 0x0001B4B4 File Offset: 0x000196B4
		private void DisplayItemEffectAddress(int itemIndex)
		{
			bool flag = itemIndex >= this.ITEM_EFFECT_ADDRESS_FIRST_INDEX && itemIndex <= this.ITEM_EFFECT_ADDRESS_LAST_INDEX;
			if (flag)
			{
				uint itemEffectAddress = this.GetItemEffectAddress(itemIndex);
				this.txtItemEffectAddress.Text = (((ulong)itemEffectAddress == 0UL) ? "00000000" : itemEffectAddress.ToString("X8"));
				this.txtItemEffectAddress.Enabled = true;
			}
			else
			{
				this.txtItemEffectAddress.Text = "";
				this.txtItemEffectAddress.Enabled = false;
			}
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0001B53C File Offset: 0x0001973C
		private void SaveChanges()
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			int num;
			bool flag;
			checked
			{
				num = ItemData.ITEM_INFO_TABLE_OFFSET + this.currentSelectedIndex * ItemData.ITEM_INFO_ENTRY_LENGTH;
				int item_NAME_MAX_DISPLAY_LENGTH = ItemData.ITEM_NAME_MAX_DISPLAY_LENGTH;
				for (int i = 0; i <= item_NAME_MAX_DISPLAY_LENGTH; i++)
				{
					MainForm.romData[num + i] = byte.MaxValue;
				}
				byte[] array = TextConverter.PokemonStringToBytes(itemInfo.Name, ItemData.ITEM_NAME_MAX_DISPLAY_LENGTH);
				Array.Copy(array, 0, MainForm.romData, num + 0, Math.Min(array.Length, ItemData.ITEM_NAME_MAX_DISPLAY_LENGTH + 1));
				flag = itemInfo.TemporaryImageData != null;
			}
			if (flag)
			{
				Array.Copy(itemInfo.TemporaryImageData, 0L, MainForm.romData, (long)((ulong)itemInfo.ImageAddress), (long)itemInfo.TemporaryImageData.Length);
				itemInfo.TemporaryImageData = null;
			}
			bool flag2 = itemInfo.TemporaryPaletteData != null;
			if (flag2)
			{
				Array.Copy(itemInfo.TemporaryPaletteData, 0L, MainForm.romData, (long)((ulong)itemInfo.PaletteAddress), (long)itemInfo.TemporaryPaletteData.Length);
				itemInfo.TemporaryPaletteData = null;
			}
			byte[] array2;
			checked
			{
				int num2 = ItemData.ITEM_IMAGE_TABLE_OFFSET + this.currentSelectedIndex * ItemData.ITEM_IMAGE_ENTRY_LENGTH;
				uint num3 = itemInfo.ImageAddress + 134217728U;
				byte[] bytes = BitConverter.GetBytes(num3);
				Array.Copy(bytes, 0, MainForm.romData, num2, 4);
				uint num4 = itemInfo.PaletteAddress + 134217728U;
				byte[] bytes2 = BitConverter.GetBytes(num4);
				Array.Copy(bytes2, 0, MainForm.romData, num2 + 4, 4);
				uint num5 = itemInfo.DescriptionAddress + 134217728U;
				byte[] bytes3 = BitConverter.GetBytes(num5);
				Array.Copy(bytes3, 0, MainForm.romData, num + 16, 4);
				string text = this.itemDescriptionData[this.currentSelectedIndex];
				array2 = TextConverter.PokemonStringToBytes(text, 256);
			}
			Array.Copy(array2, 0L, MainForm.romData, (long)((ulong)itemInfo.DescriptionAddress), (long)array2.Length);
			ushort num6 = checked((ushort)(ItemData.TOTAL_ITEM_COUNT - 1));
			for (ushort num7 = 0; num7 <= num6; num7 += 1)
			{
				ItemData.ItemInfo itemInfo2 = ItemData.GetItemInfo(MainForm.romData, num7);
				this.itemDescriptionData[(int)num7] = this.GetItemDescription(itemInfo2.DescriptionAddress);
			}
			byte[] bytes4 = BitConverter.GetBytes(itemInfo.ItemId);
			checked
			{
				Array.Copy(bytes4, 0, MainForm.romData, num + 10, 2);
				byte[] bytes5 = BitConverter.GetBytes(itemInfo.Price);
				Array.Copy(bytes5, 0, MainForm.romData, num + 12, 2);
				byte[] array3 = new byte[] { itemInfo.HeldEffectId };
				Array.Copy(array3, 0, MainForm.romData, num + 14, 1);
				byte[] array4 = new byte[] { itemInfo.EffectValue };
				Array.Copy(array4, 0, MainForm.romData, num + 15, 1);
				byte[] array5 = new byte[] { itemInfo.CanHold };
				Array.Copy(array5, 0, MainForm.romData, num + 20, 1);
				byte[] array6 = new byte[] { itemInfo.UnknownValue };
				Array.Copy(array6, 0, MainForm.romData, num + 21, 1);
				byte[] array7 = new byte[] { itemInfo.PocketId };
				Array.Copy(array7, 0, MainForm.romData, num + 22, 1);
				byte[] array8 = new byte[] { itemInfo.FieldUseType };
				Array.Copy(array8, 0, MainForm.romData, num + 23, 1);
				uint num8 = Conversions.ToUInteger((unchecked((ulong)itemInfo.FieldUseAddress) == 0UL) ? 0 : (itemInfo.FieldUseAddress + 134217728U));
				byte[] bytes6 = BitConverter.GetBytes(num8);
				Array.Copy(bytes6, 0, MainForm.romData, num + 24, 4);
				byte[] array9 = new byte[] { itemInfo.BattleUseType };
				Array.Copy(array9, 0, MainForm.romData, num + 28, 1);
				uint num9 = Conversions.ToUInteger((unchecked((ulong)itemInfo.BattleUseAddress) == 0UL) ? 0 : (itemInfo.BattleUseAddress + 134217728U));
				byte[] bytes7 = BitConverter.GetBytes(num9);
				Array.Copy(bytes7, 0, MainForm.romData, num + 32, 4);
				byte[] array10 = new byte[] { itemInfo.SpecialValue };
				Array.Copy(array10, 0, MainForm.romData, num + 36, 1);
				bool flag3 = this.currentSelectedIndex >= this.ITEM_EFFECT_ADDRESS_FIRST_INDEX && this.currentSelectedIndex <= this.ITEM_EFFECT_ADDRESS_LAST_INDEX;
				if (flag3)
				{
					this.SaveItemEffectAddress(this.currentSelectedIndex);
				}
			}
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0001B970 File Offset: 0x00019B70
		private void SaveItemEffectAddress(int itemIndex)
		{
			bool flag = itemIndex < this.ITEM_EFFECT_ADDRESS_FIRST_INDEX || itemIndex > this.ITEM_EFFECT_ADDRESS_LAST_INDEX;
			checked
			{
				if (!flag)
				{
					int num = itemIndex - this.ITEM_EFFECT_ADDRESS_FIRST_INDEX;
					int num2 = this.ITEM_EFFECT_ADDRESS_TABLE_OFFSET + num * 4;
					string text = this.txtItemEffectAddress.Text.Trim();
					bool flag2 = string.IsNullOrEmpty(text);
					if (flag2)
					{
						text = "00000000";
					}
					uint num3 = uint.Parse(text, NumberStyles.HexNumber);
					uint num4 = Conversions.ToUInteger((unchecked((ulong)num3) == 0UL) ? 0 : (num3 + 134217728U));
					byte[] bytes = BitConverter.GetBytes(num4);
					Array.Copy(bytes, 0, MainForm.romData, num2, 4);
				}
			}
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0001BA1C File Offset: 0x00019C1C
		private void RevertChanges()
		{
			checked
			{
				ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(MainForm.romData, (ushort)this.currentSelectedIndex);
				itemInfo.TemporaryImageData = null;
				itemInfo.TemporaryPaletteData = null;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.cmbItemName.Items[this.currentSelectedIndex] = itemInfo.Name;
				this.txtItemSpriteImageAddress.Text = itemInfo.ImageAddress.ToString("X8");
				this.txtItemSpritePalleteAddress.Text = itemInfo.PaletteAddress.ToString("X8");
				this.DisplayItemImage(this.currentSelectedIndex);
				this.txtItemDescriptionAddress.Text = itemInfo.DescriptionAddress.ToString("X8");
				string itemDescription = this.GetItemDescription(itemInfo.DescriptionAddress);
				this.itemDescriptionData[this.currentSelectedIndex] = itemDescription;
				this.txtItemDescription.Text = itemDescription;
				this.nudItemId.Text = itemInfo.ItemId.ToString();
				this.txtItemId16.Text = itemInfo.ItemId.ToString("X4");
				this.nudItemPrice.Text = itemInfo.Price.ToString();
				this.cmbItemHoldEffectId.SelectedIndex = (int)itemInfo.HeldEffectId;
				this.nudItemEffectParam.Value = new decimal((int)itemInfo.EffectValue);
				this.cmbItemHoldableValue.SelectedIndex = (int)itemInfo.CanHold;
				this.nudUnknownValue.Value = new decimal((int)itemInfo.UnknownValue);
				this.cmbItemPocket.SelectedIndex = (int)(itemInfo.PocketId - 1);
				this.nudItemFieldType.Value = new decimal((int)itemInfo.FieldUseType);
				this.txtItemFieldAddress.Text = itemInfo.FieldUseAddress.ToString("X8");
				this.UpdateFieldUseTypeControls(itemInfo.PocketId);
				this.cmbItemBattleType.SelectedIndex = (int)itemInfo.BattleUseType;
				this.txtItemBattleAddress.Text = itemInfo.BattleUseAddress.ToString("X8");
				this.nudSpecialValue.Value = new decimal((int)itemInfo.SpecialValue);
				this.DisplayItemEffectAddress(this.currentSelectedIndex);
			}
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0001BC54 File Offset: 0x00019E54
		private void DisplayItemImage(int id)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[id];
			bool flag = itemInfo.TemporaryImageData != null;
			checked
			{
				byte[] array;
				if (flag)
				{
					int num = BitConverter.ToInt32(itemInfo.TemporaryImageData, 0) >> 8;
					array = new byte[num - 1 + 1];
					ImageProcessor.LZ77UnComp(itemInfo.TemporaryImageData, array);
				}
				else
				{
					array = ImageProcessor.LoadCompressedImagePaletteFromROM(MainForm.romData, itemInfo.ImageAddress, false);
				}
				bool flag2 = itemInfo.TemporaryPaletteData != null;
				byte[] array2;
				if (flag2)
				{
					int num2 = BitConverter.ToInt32(itemInfo.TemporaryPaletteData, 0) >> 8;
					array2 = new byte[num2 - 1 + 1];
					ImageProcessor.LZ77UnComp(itemInfo.TemporaryPaletteData, array2);
				}
				else
				{
					array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(MainForm.romData, itemInfo.PaletteAddress, true);
				}
				Color[] array3 = ImageProcessor.LoadPalette(array2, true);
				Bitmap bitmap = ImageProcessor.LoadSprite(ref array, array3, 24, 24, false);
				bool flag3 = this.picItemSprite.Image != null;
				if (flag3)
				{
					this.picItemSprite.Image.Dispose();
				}
				this.picItemSprite.Image = bitmap;
				this.picItemSprite.Refresh();
			}
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0001BD64 File Offset: 0x00019F64
		private void btnChangeItemSpriteAddress_Click(object sender, EventArgs e)
		{
			uint num = uint.Parse(this.txtItemSpriteImageAddress.Text.Replace("&H", ""), NumberStyles.HexNumber);
			uint num2 = uint.Parse(this.txtItemSpritePalleteAddress.Text.Replace("&H", ""), NumberStyles.HexNumber);
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			itemInfo.TemporaryImageData = null;
			itemInfo.TemporaryPaletteData = null;
			bool flag = itemInfo.ImageAddress == num && itemInfo.PaletteAddress == num2;
			if (!flag)
			{
				itemInfo.ImageAddress = num;
				itemInfo.PaletteAddress = num2;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
				this.DisplayItemImage(this.currentSelectedIndex);
			}
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0001BE30 File Offset: 0x0001A030
		private void txtItemSpriteImageAddress_Enter(object sender, EventArgs e)
		{
			this.rbItemSpriteImageAddress.Checked = true;
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x0001BE40 File Offset: 0x0001A040
		private void txtItemSpritePalleteAddress_Enter(object sender, EventArgs e)
		{
			this.rbItemSpritePalleteAddress.Checked = true;
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x0001BE50 File Offset: 0x0001A050
		private void btnItemSpriteImport_Click(object sender, EventArgs e)
		{
			string text = this.txtItemSpriteImportAddress.Text.Trim();
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
						openFileDialog.Title = "アイテム画像をインポート";
						bool flag3 = openFileDialog.ShowDialog() == DialogResult.OK;
						if (flag3)
						{
							using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
							{
								bool flag4 = bitmap.Width != 24 || bitmap.Height != 24;
								if (flag4)
								{
									MessageBox.Show(string.Format("サイズは{0}x{1}である必要があります。", 24, 24), "", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
										ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
										bool @checked = this.rbItemSpriteImageAddress.Checked;
										if (@checked)
										{
											byte[] array = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
											itemInfo.TemporaryImageData = ImageProcessor.LZ77Comp(array, false);
											itemInfo.ImageAddress = num;
											this.txtItemSpriteImageAddress.Text = text;
										}
										else
										{
											bool checked2 = this.rbItemSpritePalleteAddress.Checked;
											if (checked2)
											{
												byte[] array2 = ImageProcessor.ConvertPaletteToBytes(bitmap.Palette);
												itemInfo.TemporaryPaletteData = ImageProcessor.LZ77Comp(array2, true);
												itemInfo.PaletteAddress = num;
												this.txtItemSpritePalleteAddress.Text = text;
											}
										}
										this.itemInfoData[this.currentSelectedIndex] = itemInfo;
										this.DisplayItemImage(this.currentSelectedIndex);
										this.SetDataChanged();
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x0001C098 File Offset: 0x0001A298
		private void btnItemSpriteExport_Click(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			checked
			{
				using (SaveFileDialog saveFileDialog = new SaveFileDialog())
				{
					saveFileDialog.Filter = "PNG画像|*.png";
					saveFileDialog.Title = "アイテム画像をエクスポート";
					saveFileDialog.FileName = string.Format("item_{0:X4}.png", itemInfo.Index);
					bool flag = saveFileDialog.ShowDialog() == DialogResult.OK;
					if (flag)
					{
						bool flag2 = itemInfo.TemporaryImageData != null;
						byte[] array;
						if (flag2)
						{
							array = itemInfo.TemporaryImageData;
						}
						else
						{
							array = ImageProcessor.LoadCompressedImagePaletteFromROM(MainForm.romData, itemInfo.ImageAddress, false);
						}
						bool flag3 = itemInfo.TemporaryPaletteData != null;
						byte[] array2;
						if (flag3)
						{
							array2 = itemInfo.TemporaryPaletteData;
						}
						else
						{
							array2 = ImageProcessor.LoadCompressedImagePaletteFromROM(MainForm.romData, itemInfo.PaletteAddress, true);
						}
						int num = BitConverter.ToInt32(array, 0);
						bool flag4 = (num & 255) == 16;
						if (flag4)
						{
							int num2 = BitConverter.ToInt32(array, 0) >> 8;
							byte[] array3 = new byte[num2 - 1 + 1];
							ImageProcessor.LZ77UnComp(array, array3);
							array = array3;
						}
						num = BitConverter.ToInt32(array2, 0);
						bool flag5 = (num & 255) == 16;
						if (flag5)
						{
							int num3 = BitConverter.ToInt32(array2, 0) >> 8;
							byte[] array4 = new byte[num3 - 1 + 1];
							ImageProcessor.LZ77UnComp(array2, array4);
							array2 = array4;
						}
						Color[] array5 = ImageProcessor.LoadPalette(array2, false);
						ImageProcessor.ExportSpriteTo4bppPng(saveFileDialog.FileName, array, array5, 24, 24);
					}
				}
			}
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x0001C230 File Offset: 0x0001A430
		private string GetItemDescription(uint address)
		{
			List<byte> list = new List<byte>();
			checked
			{
				int num = (int)address;
				int num2 = 0;
				do
				{
					bool flag = num + num2 >= MainForm.romData.Length;
					if (flag)
					{
						break;
					}
					byte b = MainForm.romData[num + num2];
					bool flag2 = b == byte.MaxValue;
					if (flag2)
					{
						break;
					}
					list.Add(b);
					num2++;
				}
				while (num2 <= 255);
				return TextConverter.BytesToPokemonString(list.ToArray(), 0, list.Count);
			}
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0001C2A8 File Offset: 0x0001A4A8
		private void btnChangeItemDescriptionAddress_Click(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			uint num = uint.Parse(this.txtItemDescriptionAddress.Text.Replace("&H", ""), NumberStyles.HexNumber);
			bool flag = itemInfo.DescriptionAddress != num;
			if (flag)
			{
				itemInfo.DescriptionAddress = num;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				string itemDescription = this.GetItemDescription(num);
				this.itemDescriptionData[this.currentSelectedIndex] = itemDescription;
				this.txtItemDescription.Text = itemDescription;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0001C34C File Offset: 0x0001A54C
		private void btnChangeItemDescription_Click(object sender, EventArgs e)
		{
			string text = this.txtItemDescription.Text;
			bool flag = Operators.CompareString(this.itemDescriptionData[this.currentSelectedIndex], text, false) != 0;
			if (flag)
			{
				this.itemDescriptionData[this.currentSelectedIndex] = text;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0001C3A4 File Offset: 0x0001A5A4
		private void nudItemId_ValueChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			ushort num = Convert.ToUInt16(this.nudItemId.Value);
			bool flag = itemInfo.ItemId != num;
			if (flag)
			{
				itemInfo.ItemId = num;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.txtItemId16.Text = num.ToString("X4");
				this.SetDataChanged();
			}
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x0001C424 File Offset: 0x0001A624
		private void nudItemPrice_ValueChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			ushort num = Convert.ToUInt16(this.nudItemPrice.Value);
			bool flag = itemInfo.Price != num;
			if (flag)
			{
				itemInfo.Price = num;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0001C48C File Offset: 0x0001A68C
		private void LoadHoldEffectList()
		{
			this.cmbItemHoldEffectId.Items.Clear();
			string text = Path.Combine(Application.StartupPath, "txt\\ItemHoldEffectCode.txt");
			string[] array = File.ReadAllLines(text, Encoding.UTF8);
			foreach (string text2 in array)
			{
				bool flag = Operators.CompareString(text2.Trim(), "", false) != 0;
				if (flag)
				{
					this.cmbItemHoldEffectId.Items.Add(text2.Trim());
				}
			}
		}

		// Token: 0x060003BB RID: 955 RVA: 0x0001C518 File Offset: 0x0001A718
		private void cmbItemHoldEffectId_SelectedIndexChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = checked((byte)this.cmbItemHoldEffectId.SelectedIndex);
			bool flag = itemInfo.HeldEffectId != b;
			if (flag)
			{
				itemInfo.HeldEffectId = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003BC RID: 956 RVA: 0x0001C57C File Offset: 0x0001A77C
		private void nudItemEffectParam_ValueChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = Convert.ToByte(this.nudItemEffectParam.Value);
			bool flag = itemInfo.EffectValue != b;
			if (flag)
			{
				itemInfo.EffectValue = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0001C5E4 File Offset: 0x0001A7E4
		private void cmbItemHoldableValue_SelectedIndexChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = checked((byte)this.cmbItemHoldableValue.SelectedIndex);
			bool flag = itemInfo.CanHold != b;
			if (flag)
			{
				itemInfo.CanHold = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003BE RID: 958 RVA: 0x0001C648 File Offset: 0x0001A848
		private void nudUnknownValue_ValueChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = Convert.ToByte(this.nudUnknownValue.Value);
			bool flag = itemInfo.UnknownValue != b;
			if (flag)
			{
				itemInfo.UnknownValue = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003BF RID: 959 RVA: 0x0001C6B0 File Offset: 0x0001A8B0
		private void cmbItemPocket_SelectedIndexChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte pocketId = itemInfo.PocketId;
			byte b = checked((byte)(this.cmbItemPocket.SelectedIndex + 1));
			bool flag = itemInfo.PocketId != b;
			if (flag)
			{
				itemInfo.PocketId = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				bool flag2 = pocketId == 3 && b != 3;
				if (flag2)
				{
					this.nudItemFieldType.Value = 0m;
				}
				this.UpdateFieldUseTypeControls(b);
				this.SetDataChanged();
			}
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x0001C74C File Offset: 0x0001A94C
		private void UpdateFieldUseTypeControls(byte pocketId)
		{
			bool flag = pocketId == 3;
			if (flag)
			{
				this.cmbItemFieldType.Enabled = false;
				this.cmbItemFieldType.SelectedIndex = -1;
				this.nudItemFieldType.Enabled = true;
			}
			else
			{
				this.nudItemFieldType.Enabled = false;
				this.cmbItemFieldType.Enabled = true;
				ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
				this.cmbItemFieldType.SelectedIndex = (int)itemInfo.FieldUseType;
			}
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x0001C7CC File Offset: 0x0001A9CC
		private void nudItemFieldType_ValueChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = Convert.ToByte(this.nudItemFieldType.Value);
			bool flag = itemInfo.FieldUseType != b;
			if (flag)
			{
				itemInfo.FieldUseType = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x0001C834 File Offset: 0x0001AA34
		private void cmbItemFieldType_SelectedIndexChanged(object sender, EventArgs e)
		{
			checked
			{
				bool flag = this.cmbItemPocket.SelectedIndex + 1 == 3;
				if (!flag)
				{
					ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
					byte b = (byte)this.cmbItemFieldType.SelectedIndex;
					bool flag2 = itemInfo.FieldUseType != b;
					if (flag2)
					{
						itemInfo.FieldUseType = b;
						this.itemInfoData[this.currentSelectedIndex] = itemInfo;
						this.nudItemFieldType.Value = new decimal((int)b);
						this.SetDataChanged();
					}
				}
			}
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0001C8C0 File Offset: 0x0001AAC0
		private void txtItemFieldAddress_TextChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			uint num = uint.Parse(this.txtItemFieldAddress.Text, NumberStyles.HexNumber);
			bool flag = itemInfo.FieldUseAddress != num;
			if (flag)
			{
				itemInfo.FieldUseAddress = num;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0001C92C File Offset: 0x0001AB2C
		private void cmbItemBattleType_SelectedIndexChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = checked((byte)this.cmbItemBattleType.SelectedIndex);
			bool flag = itemInfo.BattleUseType != b;
			if (flag)
			{
				itemInfo.BattleUseType = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x0001C990 File Offset: 0x0001AB90
		private void txtItemBattleAddress_TextChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			uint num = uint.Parse(this.txtItemBattleAddress.Text.Replace("&H", ""), NumberStyles.HexNumber);
			bool flag = itemInfo.BattleUseAddress != num;
			if (flag)
			{
				itemInfo.BattleUseAddress = num;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x0001CA0C File Offset: 0x0001AC0C
		private void nudSpecialValue_ValueChanged(object sender, EventArgs e)
		{
			ItemData.ItemInfo itemInfo = this.itemInfoData[this.currentSelectedIndex];
			byte b = Convert.ToByte(this.nudSpecialValue.Value);
			bool flag = itemInfo.SpecialValue != b;
			if (flag)
			{
				itemInfo.SpecialValue = b;
				this.itemInfoData[this.currentSelectedIndex] = itemInfo;
				this.SetDataChanged();
			}
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0001CA74 File Offset: 0x0001AC74
		private uint GetItemEffectAddress(int itemIndex)
		{
			bool flag = itemIndex < this.ITEM_EFFECT_ADDRESS_FIRST_INDEX || itemIndex > this.ITEM_EFFECT_ADDRESS_LAST_INDEX;
			checked
			{
				uint num;
				if (flag)
				{
					num = 0U;
				}
				else
				{
					int num2 = itemIndex - this.ITEM_EFFECT_ADDRESS_FIRST_INDEX;
					int num3 = this.ITEM_EFFECT_ADDRESS_TABLE_OFFSET + num2 * 4;
					byte[] array = new byte[4];
					Array.Copy(MainForm.romData, num3, array, 0, 4);
					uint num4 = BitConverter.ToUInt32(array, 0);
					bool flag2 = unchecked((ulong)num4) == 0UL;
					if (flag2)
					{
						num = 0U;
					}
					else
					{
						num = num4 - 134217728U;
					}
				}
				return num;
			}
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x0001CAF4 File Offset: 0x0001ACF4
		private void txtItemEffectAddress_TextChanged(object sender, EventArgs e)
		{
			bool flag = !this.txtItemEffectAddress.Enabled;
			if (!flag)
			{
				int num = this.currentSelectedIndex;
				bool flag2 = num < this.ITEM_EFFECT_ADDRESS_FIRST_INDEX || num > this.ITEM_EFFECT_ADDRESS_LAST_INDEX;
				if (!flag2)
				{
					string text = this.txtItemEffectAddress.Text.Trim();
					bool flag3 = string.IsNullOrEmpty(text);
					if (flag3)
					{
						text = "000000";
					}
					uint num2 = uint.Parse(text, NumberStyles.HexNumber);
					uint itemEffectAddress = this.GetItemEffectAddress(num);
					bool flag4 = itemEffectAddress != num2;
					if (flag4)
					{
						this.SetDataChanged();
					}
				}
			}
		}

		// Token: 0x040001FA RID: 506
		public readonly int ITEM_EFFECT_ADDRESS_TABLE_OFFSET;

		// Token: 0x040001FB RID: 507
		public readonly int ITEM_EFFECT_ADDRESS_FIRST_INDEX;

		// Token: 0x040001FC RID: 508
		public readonly int ITEM_EFFECT_ADDRESS_LAST_INDEX;

		// Token: 0x040001FD RID: 509
		public const int DESCRIPTION_MAX_LENGTH = 256;

		// Token: 0x040001FE RID: 510
		private bool isDataChanged;

		// Token: 0x040001FF RID: 511
		private int currentSelectedIndex;

		// Token: 0x04000200 RID: 512
		private List<ItemData.ItemInfo> itemInfoData;

		// Token: 0x04000201 RID: 513
		private Dictionary<int, string> itemDescriptionData;
	}
}
