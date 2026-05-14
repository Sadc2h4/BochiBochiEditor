using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace BochiBochiEditor.My
{
	// Token: 0x02000004 RID: 4
	[GeneratedCode("MyTemplate", "11.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class MyApplication : WindowsFormsApplicationBase
	{
		[DebuggerHidden]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		internal static void RunApp(string[] Args)
		{
			Application.SetCompatibleTextRenderingDefault(WindowsFormsApplicationBase.UseCompatibleTextRendering);
			MyProject.Application.Run(Args);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002114 File Offset: 0x00000314
		[DebuggerStepThrough]
		public MyApplication()
			: base(AuthenticationMode.Windows)
		{
			base.IsSingleInstance = false;
			base.EnableVisualStyles = true;
			base.SaveMySettingsOnExit = true;
			base.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000213F File Offset: 0x0000033F
		[DebuggerStepThrough]
		protected override void OnCreateMainForm()
		{
			base.MainForm = MyProject.Forms.MainForm;
		}
	}
}
