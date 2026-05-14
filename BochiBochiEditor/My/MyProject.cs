using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor.My
{
	// Token: 0x02000006 RID: 6
	[GeneratedCode("MyTemplate", "11.0.0.0")]
	internal sealed class MyProject
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000E RID: 14 RVA: 0x00002194 File Offset: 0x00000394
		[HelpKeyword("My.Computer")]
		internal static MyComputer Computer
		{
			[DebuggerHidden]
			get
			{
				return MyProject.m_ComputerObjectProvider.GetInstance;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000F RID: 15 RVA: 0x000021B0 File Offset: 0x000003B0
		[HelpKeyword("My.Application")]
		internal static MyApplication Application
		{
			[DebuggerHidden]
			get
			{
				return MyProject.m_AppObjectProvider.GetInstance;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000010 RID: 16 RVA: 0x000021CC File Offset: 0x000003CC
		[HelpKeyword("My.User")]
		internal static User User
		{
			[DebuggerHidden]
			get
			{
				return MyProject.m_UserObjectProvider.GetInstance;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000021E8 File Offset: 0x000003E8
		[HelpKeyword("My.Forms")]
		internal static MyProject.MyForms Forms
		{
			[DebuggerHidden]
			get
			{
				return MyProject.m_MyFormsObjectProvider.GetInstance;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002204 File Offset: 0x00000404
		[HelpKeyword("My.WebServices")]
		internal static MyProject.MyWebServices WebServices
		{
			[DebuggerHidden]
			get
			{
				return MyProject.m_MyWebServicesObjectProvider.GetInstance;
			}
		}

		// Token: 0x04000003 RID: 3
		private static readonly MyProject.ThreadSafeObjectProvider<MyComputer> m_ComputerObjectProvider = new MyProject.ThreadSafeObjectProvider<MyComputer>();

		// Token: 0x04000004 RID: 4
		private static readonly MyProject.ThreadSafeObjectProvider<MyApplication> m_AppObjectProvider = new MyProject.ThreadSafeObjectProvider<MyApplication>();

		// Token: 0x04000005 RID: 5
		private static readonly MyProject.ThreadSafeObjectProvider<User> m_UserObjectProvider = new MyProject.ThreadSafeObjectProvider<User>();

		// Token: 0x04000006 RID: 6
		private static MyProject.ThreadSafeObjectProvider<MyProject.MyForms> m_MyFormsObjectProvider = new MyProject.ThreadSafeObjectProvider<MyProject.MyForms>();

		// Token: 0x04000007 RID: 7
		private static readonly MyProject.ThreadSafeObjectProvider<MyProject.MyWebServices> m_MyWebServicesObjectProvider = new MyProject.ThreadSafeObjectProvider<MyProject.MyWebServices>();

		// Token: 0x0200002C RID: 44
		[EditorBrowsable(EditorBrowsableState.Never)]
		[MyGroupCollection("System.Windows.Forms.Form", "Create__Instance__", "Dispose__Instance__", "My.MyProject.Forms")]
		internal sealed class MyForms
		{
			// Token: 0x06000E74 RID: 3700 RVA: 0x00069E34 File Offset: 0x00068034
			[DebuggerHidden]
			private static T Create__Instance__<T>(T Instance) where T : Form, new()
			{
				bool flag = Instance == null || Instance.IsDisposed;
				if (flag)
				{
					bool flag2 = MyProject.MyForms.m_FormBeingCreated != null;
					if (flag2)
					{
						bool flag3 = MyProject.MyForms.m_FormBeingCreated.ContainsKey(typeof(T));
						if (flag3)
						{
							throw new InvalidOperationException(Utils.GetResourceString("WinForms_RecursiveFormCreate", new string[0]));
						}
					}
					else
					{
						MyProject.MyForms.m_FormBeingCreated = new Hashtable();
					}
					MyProject.MyForms.m_FormBeingCreated.Add(typeof(T), null);
					try
					{
						return new T();
					}
					catch (TargetInvocationException ex) when (ex.InnerException != null)
					{
						string resourceString = Utils.GetResourceString("WinForms_SeeInnerException", new string[] { ex.InnerException.Message });
						throw new InvalidOperationException(resourceString, ex.InnerException);
					}
					finally
					{
						MyProject.MyForms.m_FormBeingCreated.Remove(typeof(T));
					}
				}
				return Instance;
			}

			// Token: 0x06000E75 RID: 3701 RVA: 0x00069F5C File Offset: 0x0006815C
			[DebuggerHidden]
			private void Dispose__Instance__<T>(ref T instance) where T : Form
			{
				instance.Dispose();
				instance = default(T);
			}

			// Token: 0x06000E76 RID: 3702 RVA: 0x00069F73 File Offset: 0x00068173
			[DebuggerHidden]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public MyForms()
			{
			}

			// Token: 0x06000E77 RID: 3703 RVA: 0x00069F80 File Offset: 0x00068180
			[EditorBrowsable(EditorBrowsableState.Never)]
			public override bool Equals(object o)
			{
				return base.Equals(RuntimeHelpers.GetObjectValue(o));
			}

			// Token: 0x06000E78 RID: 3704 RVA: 0x00069FA0 File Offset: 0x000681A0
			[EditorBrowsable(EditorBrowsableState.Never)]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			// Token: 0x06000E79 RID: 3705 RVA: 0x00069FB8 File Offset: 0x000681B8
			[EditorBrowsable(EditorBrowsableState.Never)]
			internal new Type GetType()
			{
				return typeof(MyProject.MyForms);
			}

			// Token: 0x06000E7A RID: 3706 RVA: 0x00069FD4 File Offset: 0x000681D4
			[EditorBrowsable(EditorBrowsableState.Never)]
			public override string ToString()
			{
				return base.ToString();
			}

			// Token: 0x1700057A RID: 1402
			// (get) Token: 0x06000E7B RID: 3707 RVA: 0x00069FEC File Offset: 0x000681EC
			// (set) Token: 0x06000E90 RID: 3728 RVA: 0x0006A223 File Offset: 0x00068423
			public BlockEditor BlockEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_BlockEditor = MyProject.MyForms.Create__Instance__<BlockEditor>(this.m_BlockEditor);
					return this.m_BlockEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_BlockEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<BlockEditor>(ref this.m_BlockEditor);
					}
				}
			}

			// Token: 0x1700057B RID: 1403
			// (get) Token: 0x06000E7C RID: 3708 RVA: 0x0006A007 File Offset: 0x00068207
			// (set) Token: 0x06000E91 RID: 3729 RVA: 0x0006A24F File Offset: 0x0006844F
			public EggMoveEditor EggMoveEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_EggMoveEditor = MyProject.MyForms.Create__Instance__<EggMoveEditor>(this.m_EggMoveEditor);
					return this.m_EggMoveEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_EggMoveEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<EggMoveEditor>(ref this.m_EggMoveEditor);
					}
				}
			}

			// Token: 0x1700057C RID: 1404
			// (get) Token: 0x06000E7D RID: 3709 RVA: 0x0006A022 File Offset: 0x00068222
			// (set) Token: 0x06000E92 RID: 3730 RVA: 0x0006A27B File Offset: 0x0006847B
			public HabitatEditor HabitatEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_HabitatEditor = MyProject.MyForms.Create__Instance__<HabitatEditor>(this.m_HabitatEditor);
					return this.m_HabitatEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_HabitatEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<HabitatEditor>(ref this.m_HabitatEditor);
					}
				}
			}

			// Token: 0x1700057D RID: 1405
			// (get) Token: 0x06000E7E RID: 3710 RVA: 0x0006A03D File Offset: 0x0006823D
			// (set) Token: 0x06000E93 RID: 3731 RVA: 0x0006A2A7 File Offset: 0x000684A7
			public HeldItemMailEditor HeldItemMailEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_HeldItemMailEditor = MyProject.MyForms.Create__Instance__<HeldItemMailEditor>(this.m_HeldItemMailEditor);
					return this.m_HeldItemMailEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_HeldItemMailEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<HeldItemMailEditor>(ref this.m_HeldItemMailEditor);
					}
				}
			}

			// Token: 0x1700057E RID: 1406
			// (get) Token: 0x06000E7F RID: 3711 RVA: 0x0006A058 File Offset: 0x00068258
			// (set) Token: 0x06000E94 RID: 3732 RVA: 0x0006A2D3 File Offset: 0x000684D3
			public InGameTradeEditor InGameTradeEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_InGameTradeEditor = MyProject.MyForms.Create__Instance__<InGameTradeEditor>(this.m_InGameTradeEditor);
					return this.m_InGameTradeEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_InGameTradeEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<InGameTradeEditor>(ref this.m_InGameTradeEditor);
					}
				}
			}

			// Token: 0x1700057F RID: 1407
			// (get) Token: 0x06000E80 RID: 3712 RVA: 0x0006A073 File Offset: 0x00068273
			// (set) Token: 0x06000E95 RID: 3733 RVA: 0x0006A2FF File Offset: 0x000684FF
			public InsertNewLevelMoveList InsertNewLevelMoveList
			{
				[DebuggerHidden]
				get
				{
					this.m_InsertNewLevelMoveList = MyProject.MyForms.Create__Instance__<InsertNewLevelMoveList>(this.m_InsertNewLevelMoveList);
					return this.m_InsertNewLevelMoveList;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_InsertNewLevelMoveList)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<InsertNewLevelMoveList>(ref this.m_InsertNewLevelMoveList);
					}
				}
			}

			// Token: 0x17000580 RID: 1408
			// (get) Token: 0x06000E81 RID: 3713 RVA: 0x0006A08E File Offset: 0x0006828E
			// (set) Token: 0x06000E96 RID: 3734 RVA: 0x0006A32B File Offset: 0x0006852B
			public InsertNewPokedexData InsertNewPokedexData
			{
				[DebuggerHidden]
				get
				{
					this.m_InsertNewPokedexData = MyProject.MyForms.Create__Instance__<InsertNewPokedexData>(this.m_InsertNewPokedexData);
					return this.m_InsertNewPokedexData;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_InsertNewPokedexData)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<InsertNewPokedexData>(ref this.m_InsertNewPokedexData);
					}
				}
			}

			// Token: 0x17000581 RID: 1409
			// (get) Token: 0x06000E82 RID: 3714 RVA: 0x0006A0A9 File Offset: 0x000682A9
			// (set) Token: 0x06000E97 RID: 3735 RVA: 0x0006A357 File Offset: 0x00068557
			public InsertNewPokedexTable InsertNewPokedexTable
			{
				[DebuggerHidden]
				get
				{
					this.m_InsertNewPokedexTable = MyProject.MyForms.Create__Instance__<InsertNewPokedexTable>(this.m_InsertNewPokedexTable);
					return this.m_InsertNewPokedexTable;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_InsertNewPokedexTable)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<InsertNewPokedexTable>(ref this.m_InsertNewPokedexTable);
					}
				}
			}

			// Token: 0x17000582 RID: 1410
			// (get) Token: 0x06000E83 RID: 3715 RVA: 0x0006A0C4 File Offset: 0x000682C4
			// (set) Token: 0x06000E98 RID: 3736 RVA: 0x0006A383 File Offset: 0x00068583
			public InsertNewPokemonData InsertNewPokemonData
			{
				[DebuggerHidden]
				get
				{
					this.m_InsertNewPokemonData = MyProject.MyForms.Create__Instance__<InsertNewPokemonData>(this.m_InsertNewPokemonData);
					return this.m_InsertNewPokemonData;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_InsertNewPokemonData)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<InsertNewPokemonData>(ref this.m_InsertNewPokemonData);
					}
				}
			}

			// Token: 0x17000583 RID: 1411
			// (get) Token: 0x06000E84 RID: 3716 RVA: 0x0006A0DF File Offset: 0x000682DF
			// (set) Token: 0x06000E99 RID: 3737 RVA: 0x0006A3AF File Offset: 0x000685AF
			public ItemEditor ItemEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_ItemEditor = MyProject.MyForms.Create__Instance__<ItemEditor>(this.m_ItemEditor);
					return this.m_ItemEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_ItemEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<ItemEditor>(ref this.m_ItemEditor);
					}
				}
			}

			// Token: 0x17000584 RID: 1412
			// (get) Token: 0x06000E85 RID: 3717 RVA: 0x0006A0FA File Offset: 0x000682FA
			// (set) Token: 0x06000E9A RID: 3738 RVA: 0x0006A3DB File Offset: 0x000685DB
			public ItemUseCoordinate ItemUseCoordinate
			{
				[DebuggerHidden]
				get
				{
					this.m_ItemUseCoordinate = MyProject.MyForms.Create__Instance__<ItemUseCoordinate>(this.m_ItemUseCoordinate);
					return this.m_ItemUseCoordinate;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_ItemUseCoordinate)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<ItemUseCoordinate>(ref this.m_ItemUseCoordinate);
					}
				}
			}

			// Token: 0x17000585 RID: 1413
			// (get) Token: 0x06000E86 RID: 3718 RVA: 0x0006A115 File Offset: 0x00068315
			// (set) Token: 0x06000E9B RID: 3739 RVA: 0x0006A407 File Offset: 0x00068607
			public MainForm MainForm
			{
				[DebuggerHidden]
				get
				{
					this.m_MainForm = MyProject.MyForms.Create__Instance__<MainForm>(this.m_MainForm);
					return this.m_MainForm;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_MainForm)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<MainForm>(ref this.m_MainForm);
					}
				}
			}

			// Token: 0x17000586 RID: 1414
			// (get) Token: 0x06000E87 RID: 3719 RVA: 0x0006A130 File Offset: 0x00068330
			// (set) Token: 0x06000E9C RID: 3740 RVA: 0x0006A433 File Offset: 0x00068633
			public MapEditor MapEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_MapEditor = MyProject.MyForms.Create__Instance__<MapEditor>(this.m_MapEditor);
					return this.m_MapEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_MapEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<MapEditor>(ref this.m_MapEditor);
					}
				}
			}

			// Token: 0x17000587 RID: 1415
			// (get) Token: 0x06000E88 RID: 3720 RVA: 0x0006A14B File Offset: 0x0006834B
			// (set) Token: 0x06000E9D RID: 3741 RVA: 0x0006A45F File Offset: 0x0006865F
			public OverWorldEditor OverWorldEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_OverWorldEditor = MyProject.MyForms.Create__Instance__<OverWorldEditor>(this.m_OverWorldEditor);
					return this.m_OverWorldEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_OverWorldEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<OverWorldEditor>(ref this.m_OverWorldEditor);
					}
				}
			}

			// Token: 0x17000588 RID: 1416
			// (get) Token: 0x06000E89 RID: 3721 RVA: 0x0006A166 File Offset: 0x00068366
			// (set) Token: 0x06000E9E RID: 3742 RVA: 0x0006A48B File Offset: 0x0006868B
			public PokedexListEditor PokedexListEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_PokedexListEditor = MyProject.MyForms.Create__Instance__<PokedexListEditor>(this.m_PokedexListEditor);
					return this.m_PokedexListEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_PokedexListEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<PokedexListEditor>(ref this.m_PokedexListEditor);
					}
				}
			}

			// Token: 0x17000589 RID: 1417
			// (get) Token: 0x06000E8A RID: 3722 RVA: 0x0006A181 File Offset: 0x00068381
			// (set) Token: 0x06000E9F RID: 3743 RVA: 0x0006A4B7 File Offset: 0x000686B7
			public PokedexOrderEditor PokedexOrderEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_PokedexOrderEditor = MyProject.MyForms.Create__Instance__<PokedexOrderEditor>(this.m_PokedexOrderEditor);
					return this.m_PokedexOrderEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_PokedexOrderEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<PokedexOrderEditor>(ref this.m_PokedexOrderEditor);
					}
				}
			}

			// Token: 0x1700058A RID: 1418
			// (get) Token: 0x06000E8B RID: 3723 RVA: 0x0006A19C File Offset: 0x0006839C
			// (set) Token: 0x06000EA0 RID: 3744 RVA: 0x0006A4E3 File Offset: 0x000686E3
			public PokemonEditor PokemonEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_PokemonEditor = MyProject.MyForms.Create__Instance__<PokemonEditor>(this.m_PokemonEditor);
					return this.m_PokemonEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_PokemonEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<PokemonEditor>(ref this.m_PokemonEditor);
					}
				}
			}

			// Token: 0x1700058B RID: 1419
			// (get) Token: 0x06000E8C RID: 3724 RVA: 0x0006A1B7 File Offset: 0x000683B7
			// (set) Token: 0x06000EA1 RID: 3745 RVA: 0x0006A50F File Offset: 0x0006870F
			public TmHmTutorEditor TmHmTutorEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_TmHmTutorEditor = MyProject.MyForms.Create__Instance__<TmHmTutorEditor>(this.m_TmHmTutorEditor);
					return this.m_TmHmTutorEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_TmHmTutorEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<TmHmTutorEditor>(ref this.m_TmHmTutorEditor);
					}
				}
			}

			// Token: 0x1700058C RID: 1420
			// (get) Token: 0x06000E8D RID: 3725 RVA: 0x0006A1D2 File Offset: 0x000683D2
			// (set) Token: 0x06000EA2 RID: 3746 RVA: 0x0006A53B File Offset: 0x0006873B
			public TrainerDataEditor TrainerDataEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_TrainerDataEditor = MyProject.MyForms.Create__Instance__<TrainerDataEditor>(this.m_TrainerDataEditor);
					return this.m_TrainerDataEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_TrainerDataEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<TrainerDataEditor>(ref this.m_TrainerDataEditor);
					}
				}
			}

			// Token: 0x1700058D RID: 1421
			// (get) Token: 0x06000E8E RID: 3726 RVA: 0x0006A1ED File Offset: 0x000683ED
			// (set) Token: 0x06000EA3 RID: 3747 RVA: 0x0006A567 File Offset: 0x00068767
			public TrainerSpriteEditor TrainerSpriteEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_TrainerSpriteEditor = MyProject.MyForms.Create__Instance__<TrainerSpriteEditor>(this.m_TrainerSpriteEditor);
					return this.m_TrainerSpriteEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_TrainerSpriteEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<TrainerSpriteEditor>(ref this.m_TrainerSpriteEditor);
					}
				}
			}

			// Token: 0x1700058E RID: 1422
			// (get) Token: 0x06000E8F RID: 3727 RVA: 0x0006A208 File Offset: 0x00068408
			// (set) Token: 0x06000EA4 RID: 3748 RVA: 0x0006A593 File Offset: 0x00068793
			public WildPokemonEditor WildPokemonEditor
			{
				[DebuggerHidden]
				get
				{
					this.m_WildPokemonEditor = MyProject.MyForms.Create__Instance__<WildPokemonEditor>(this.m_WildPokemonEditor);
					return this.m_WildPokemonEditor;
				}
				[DebuggerHidden]
				set
				{
					if (value != this.m_WildPokemonEditor)
					{
						if (value != null)
						{
							throw new ArgumentException("Property can only be set to Nothing");
						}
						this.Dispose__Instance__<WildPokemonEditor>(ref this.m_WildPokemonEditor);
					}
				}
			}

			// Token: 0x040007E7 RID: 2023
			[ThreadStatic]
			private static Hashtable m_FormBeingCreated;

			// Token: 0x040007E8 RID: 2024
			[EditorBrowsable(EditorBrowsableState.Never)]
			public BlockEditor m_BlockEditor;

			// Token: 0x040007E9 RID: 2025
			[EditorBrowsable(EditorBrowsableState.Never)]
			public EggMoveEditor m_EggMoveEditor;

			// Token: 0x040007EA RID: 2026
			[EditorBrowsable(EditorBrowsableState.Never)]
			public HabitatEditor m_HabitatEditor;

			// Token: 0x040007EB RID: 2027
			[EditorBrowsable(EditorBrowsableState.Never)]
			public HeldItemMailEditor m_HeldItemMailEditor;

			// Token: 0x040007EC RID: 2028
			[EditorBrowsable(EditorBrowsableState.Never)]
			public InGameTradeEditor m_InGameTradeEditor;

			// Token: 0x040007ED RID: 2029
			[EditorBrowsable(EditorBrowsableState.Never)]
			public InsertNewLevelMoveList m_InsertNewLevelMoveList;

			// Token: 0x040007EE RID: 2030
			[EditorBrowsable(EditorBrowsableState.Never)]
			public InsertNewPokedexData m_InsertNewPokedexData;

			// Token: 0x040007EF RID: 2031
			[EditorBrowsable(EditorBrowsableState.Never)]
			public InsertNewPokedexTable m_InsertNewPokedexTable;

			// Token: 0x040007F0 RID: 2032
			[EditorBrowsable(EditorBrowsableState.Never)]
			public InsertNewPokemonData m_InsertNewPokemonData;

			// Token: 0x040007F1 RID: 2033
			[EditorBrowsable(EditorBrowsableState.Never)]
			public ItemEditor m_ItemEditor;

			// Token: 0x040007F2 RID: 2034
			[EditorBrowsable(EditorBrowsableState.Never)]
			public ItemUseCoordinate m_ItemUseCoordinate;

			// Token: 0x040007F3 RID: 2035
			[EditorBrowsable(EditorBrowsableState.Never)]
			public MainForm m_MainForm;

			// Token: 0x040007F4 RID: 2036
			[EditorBrowsable(EditorBrowsableState.Never)]
			public MapEditor m_MapEditor;

			// Token: 0x040007F5 RID: 2037
			[EditorBrowsable(EditorBrowsableState.Never)]
			public OverWorldEditor m_OverWorldEditor;

			// Token: 0x040007F6 RID: 2038
			[EditorBrowsable(EditorBrowsableState.Never)]
			public PokedexListEditor m_PokedexListEditor;

			// Token: 0x040007F7 RID: 2039
			[EditorBrowsable(EditorBrowsableState.Never)]
			public PokedexOrderEditor m_PokedexOrderEditor;

			// Token: 0x040007F8 RID: 2040
			[EditorBrowsable(EditorBrowsableState.Never)]
			public PokemonEditor m_PokemonEditor;

			// Token: 0x040007F9 RID: 2041
			[EditorBrowsable(EditorBrowsableState.Never)]
			public TmHmTutorEditor m_TmHmTutorEditor;

			// Token: 0x040007FA RID: 2042
			[EditorBrowsable(EditorBrowsableState.Never)]
			public TrainerDataEditor m_TrainerDataEditor;

			// Token: 0x040007FB RID: 2043
			[EditorBrowsable(EditorBrowsableState.Never)]
			public TrainerSpriteEditor m_TrainerSpriteEditor;

			// Token: 0x040007FC RID: 2044
			[EditorBrowsable(EditorBrowsableState.Never)]
			public WildPokemonEditor m_WildPokemonEditor;
		}

		// Token: 0x0200002D RID: 45
		[EditorBrowsable(EditorBrowsableState.Never)]
		[MyGroupCollection("System.Web.Services.Protocols.SoapHttpClientProtocol", "Create__Instance__", "Dispose__Instance__", "")]
		internal sealed class MyWebServices
		{
			// Token: 0x06000EA5 RID: 3749 RVA: 0x0006A5C0 File Offset: 0x000687C0
			[EditorBrowsable(EditorBrowsableState.Never)]
			[DebuggerHidden]
			public override bool Equals(object o)
			{
				return base.Equals(RuntimeHelpers.GetObjectValue(o));
			}

			// Token: 0x06000EA6 RID: 3750 RVA: 0x0006A5E0 File Offset: 0x000687E0
			[EditorBrowsable(EditorBrowsableState.Never)]
			[DebuggerHidden]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			// Token: 0x06000EA7 RID: 3751 RVA: 0x0006A5F8 File Offset: 0x000687F8
			[EditorBrowsable(EditorBrowsableState.Never)]
			[DebuggerHidden]
			internal new Type GetType()
			{
				return typeof(MyProject.MyWebServices);
			}

			// Token: 0x06000EA8 RID: 3752 RVA: 0x0006A614 File Offset: 0x00068814
			[EditorBrowsable(EditorBrowsableState.Never)]
			[DebuggerHidden]
			public override string ToString()
			{
				return base.ToString();
			}

			// Token: 0x06000EA9 RID: 3753 RVA: 0x0006A62C File Offset: 0x0006882C
			[DebuggerHidden]
			private static T Create__Instance__<T>(T instance) where T : new()
			{
				bool flag = instance == null;
				T t;
				if (flag)
				{
					t = new T();
				}
				else
				{
					t = instance;
				}
				return t;
			}

			// Token: 0x06000EAA RID: 3754 RVA: 0x0006A655 File Offset: 0x00068855
			[DebuggerHidden]
			private void Dispose__Instance__<T>(ref T instance)
			{
				instance = default(T);
			}

			// Token: 0x06000EAB RID: 3755 RVA: 0x0006A65F File Offset: 0x0006885F
			[DebuggerHidden]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public MyWebServices()
			{
			}
		}

		// Token: 0x0200002E RID: 46
		[EditorBrowsable(EditorBrowsableState.Never)]
		[ComVisible(false)]
		internal sealed class ThreadSafeObjectProvider<T> where T : new()
		{
			// Token: 0x1700058F RID: 1423
			// (get) Token: 0x06000EAC RID: 3756 RVA: 0x0006A66C File Offset: 0x0006886C
			internal T GetInstance
			{
				[DebuggerHidden]
				get
				{
					bool flag = MyProject.ThreadSafeObjectProvider<T>.m_ThreadStaticValue == null;
					if (flag)
					{
						MyProject.ThreadSafeObjectProvider<T>.m_ThreadStaticValue = new T();
					}
					return MyProject.ThreadSafeObjectProvider<T>.m_ThreadStaticValue;
				}
			}

			// Token: 0x06000EAD RID: 3757 RVA: 0x0006A69E File Offset: 0x0006889E
			[DebuggerHidden]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public ThreadSafeObjectProvider()
			{
			}

			// Token: 0x040007FD RID: 2045
			[CompilerGenerated]
			[ThreadStatic]
			private static T m_ThreadStaticValue;
		}
	}
}
