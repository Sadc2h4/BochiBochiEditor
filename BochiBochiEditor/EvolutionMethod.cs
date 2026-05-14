using System;

namespace BochiBochiEditor
{
	// Token: 0x02000021 RID: 33
	public class EvolutionMethod
	{
		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06000921 RID: 2337 RVA: 0x00043D61 File Offset: 0x00041F61
		// (set) Token: 0x06000922 RID: 2338 RVA: 0x00043D6B File Offset: 0x00041F6B
		public int Code { get; set; }

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06000923 RID: 2339 RVA: 0x00043D74 File Offset: 0x00041F74
		// (set) Token: 0x06000924 RID: 2340 RVA: 0x00043D7E File Offset: 0x00041F7E
		public string MethodName { get; set; }

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06000925 RID: 2341 RVA: 0x00043D87 File Offset: 0x00041F87
		// (set) Token: 0x06000926 RID: 2342 RVA: 0x00043D91 File Offset: 0x00041F91
		public string Parameter1Description { get; set; }

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06000927 RID: 2343 RVA: 0x00043D9A File Offset: 0x00041F9A
		// (set) Token: 0x06000928 RID: 2344 RVA: 0x00043DA4 File Offset: 0x00041FA4
		public string Parameter2Description { get; set; }

		// Token: 0x06000929 RID: 2345 RVA: 0x00043DB0 File Offset: 0x00041FB0
		public override string ToString()
		{
			return this.MethodName;
		}
	}
}
