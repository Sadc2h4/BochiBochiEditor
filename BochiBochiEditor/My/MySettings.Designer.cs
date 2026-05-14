using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor.My
{
	// Token: 0x02000008 RID: 8
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal sealed partial class MySettings : ApplicationSettingsBase
	{
		// Token: 0x06000018 RID: 24 RVA: 0x000022B8 File Offset: 0x000004B8
		[DebuggerNonUserCode]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		private static void AutoSaveSettings(object sender, EventArgs e)
		{
			bool saveMySettingsOnExit = MyProject.Application.SaveMySettingsOnExit;
			if (saveMySettingsOnExit)
			{
				MySettingsProperty.Settings.Save();
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000019 RID: 25 RVA: 0x000022E4 File Offset: 0x000004E4
		public static MySettings Default
		{
			get
			{
				bool flag = !MySettings.addedHandler;
				if (flag)
				{
					object obj = MySettings.addedHandlerLockObject;
					ObjectFlowControl.CheckForSyncLockOnValueType(obj);
					lock (obj)
					{
						bool flag3 = !MySettings.addedHandler;
						if (flag3)
						{
							MyProject.Application.Shutdown += MySettings.AutoSaveSettings;
							MySettings.addedHandler = true;
						}
					}
				}
				return MySettings.defaultInstance;
			}
		}

		// Token: 0x0400000A RID: 10
		private static MySettings defaultInstance = (MySettings)SettingsBase.Synchronized(new MySettings());

		// Token: 0x0400000B RID: 11
		private static bool addedHandler;

		// Token: 0x0400000C RID: 12
		private static object addedHandlerLockObject = RuntimeHelpers.GetObjectValue(new object());
	}
}
