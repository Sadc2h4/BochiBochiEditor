using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace BochiBochiEditor.My.Resources
{
	// Token: 0x02000007 RID: 7
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal sealed class Resources
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000013 RID: 19 RVA: 0x00002220 File Offset: 0x00000420
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				bool flag = object.ReferenceEquals(Resources.resourceMan, null);
				if (flag)
				{
					ResourceManager resourceManager = new ResourceManager("BochiBochiEditor.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000014 RID: 20 RVA: 0x00002268 File Offset: 0x00000468
		// (set) Token: 0x06000015 RID: 21 RVA: 0x0000227F File Offset: 0x0000047F
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x04000008 RID: 8
		private static ResourceManager resourceMan;

		// Token: 0x04000009 RID: 9
		private static CultureInfo resourceCulture;
	}
}
