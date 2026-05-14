using System;

namespace BochiBochiEditor
{
	// Token: 0x02000023 RID: 35
	public class LevelMove
	{
		// Token: 0x1700037A RID: 890
		// (get) Token: 0x0600093B RID: 2363 RVA: 0x00043E89 File Offset: 0x00042089
		// (set) Token: 0x0600093C RID: 2364 RVA: 0x00043E93 File Offset: 0x00042093
		public int Level { get; set; }

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x0600093D RID: 2365 RVA: 0x00043E9C File Offset: 0x0004209C
		// (set) Token: 0x0600093E RID: 2366 RVA: 0x00043EA6 File Offset: 0x000420A6
		public int MoveId { get; set; }

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x0600093F RID: 2367 RVA: 0x00043EAF File Offset: 0x000420AF
		// (set) Token: 0x06000940 RID: 2368 RVA: 0x00043EB9 File Offset: 0x000420B9
		public string MoveName { get; set; }

		// Token: 0x06000941 RID: 2369 RVA: 0x00043EC4 File Offset: 0x000420C4
		public override string ToString()
		{
			return string.Format("Lv.{0} {1}", this.Level, this.MoveName);
		}
	}
}
