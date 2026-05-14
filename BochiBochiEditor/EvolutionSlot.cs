using System;

namespace BochiBochiEditor
{
	// Token: 0x02000022 RID: 34
	public class EvolutionSlot
	{
		// Token: 0x17000373 RID: 883
		// (get) Token: 0x0600092B RID: 2347 RVA: 0x00043DD0 File Offset: 0x00041FD0
		// (set) Token: 0x0600092C RID: 2348 RVA: 0x00043DDA File Offset: 0x00041FDA
		public int SlotIndex { get; set; }

		// Token: 0x17000374 RID: 884
		// (get) Token: 0x0600092D RID: 2349 RVA: 0x00043DE3 File Offset: 0x00041FE3
		// (set) Token: 0x0600092E RID: 2350 RVA: 0x00043DED File Offset: 0x00041FED
		public byte EvolutionCode { get; set; }

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x0600092F RID: 2351 RVA: 0x00043DF6 File Offset: 0x00041FF6
		// (set) Token: 0x06000930 RID: 2352 RVA: 0x00043E00 File Offset: 0x00042000
		public byte Parameter1A { get; set; }

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x06000931 RID: 2353 RVA: 0x00043E09 File Offset: 0x00042009
		// (set) Token: 0x06000932 RID: 2354 RVA: 0x00043E13 File Offset: 0x00042013
		public byte Parameter1B { get; set; }

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x06000933 RID: 2355 RVA: 0x00043E1C File Offset: 0x0004201C
		// (set) Token: 0x06000934 RID: 2356 RVA: 0x00043E26 File Offset: 0x00042026
		public ushort EvolveToPokemonId { get; set; }

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x06000935 RID: 2357 RVA: 0x00043E2F File Offset: 0x0004202F
		// (set) Token: 0x06000936 RID: 2358 RVA: 0x00043E39 File Offset: 0x00042039
		public byte Parameter2A { get; set; }

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06000937 RID: 2359 RVA: 0x00043E42 File Offset: 0x00042042
		// (set) Token: 0x06000938 RID: 2360 RVA: 0x00043E4C File Offset: 0x0004204C
		public byte Parameter2B { get; set; }

		// Token: 0x06000939 RID: 2361 RVA: 0x00043E58 File Offset: 0x00042058
		public override string ToString()
		{
			return string.Format("進化先スロット{0}", checked(this.SlotIndex + 1));
		}
	}
}
