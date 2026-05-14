using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor.My
{
	// Token: 0x02000009 RID: 9
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal sealed class MySettingsProperty
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002370 File Offset: 0x00000570
		[HelpKeyword("My.Settings")]
		internal static MySettings Settings
		{
			get
			{
				return MySettings.Default;
			}
		}
	}
}
