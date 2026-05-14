using System;

namespace BochiBochiEditor
{
	// Token: 0x02000020 RID: 32
	public class PokemonData
	{
		// Token: 0x17000336 RID: 822
		// (get) Token: 0x060008AD RID: 2221 RVA: 0x000438FC File Offset: 0x00041AFC
		// (set) Token: 0x060008AE RID: 2222 RVA: 0x00043906 File Offset: 0x00041B06
		public int Index { get; set; }

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x060008AF RID: 2223 RVA: 0x0004390F File Offset: 0x00041B0F
		// (set) Token: 0x060008B0 RID: 2224 RVA: 0x00043919 File Offset: 0x00041B19
		public string Name { get; set; }

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x060008B1 RID: 2225 RVA: 0x00043922 File Offset: 0x00041B22
		// (set) Token: 0x060008B2 RID: 2226 RVA: 0x0004392C File Offset: 0x00041B2C
		public string OriginalName { get; set; }

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x060008B3 RID: 2227 RVA: 0x00043935 File Offset: 0x00041B35
		// (set) Token: 0x060008B4 RID: 2228 RVA: 0x0004393F File Offset: 0x00041B3F
		public uint FrontImageAddress { get; set; }

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x060008B5 RID: 2229 RVA: 0x00043948 File Offset: 0x00041B48
		// (set) Token: 0x060008B6 RID: 2230 RVA: 0x00043952 File Offset: 0x00041B52
		public uint BackImageAddress { get; set; }

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x060008B7 RID: 2231 RVA: 0x0004395B File Offset: 0x00041B5B
		// (set) Token: 0x060008B8 RID: 2232 RVA: 0x00043965 File Offset: 0x00041B65
		public uint NormalPaletteAddress { get; set; }

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x060008B9 RID: 2233 RVA: 0x0004396E File Offset: 0x00041B6E
		// (set) Token: 0x060008BA RID: 2234 RVA: 0x00043978 File Offset: 0x00041B78
		public uint ShinyPaletteAddress { get; set; }

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x060008BB RID: 2235 RVA: 0x00043981 File Offset: 0x00041B81
		// (set) Token: 0x060008BC RID: 2236 RVA: 0x0004398B File Offset: 0x00041B8B
		public byte[] TemporaryFrontImageData { get; set; }

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x060008BD RID: 2237 RVA: 0x00043994 File Offset: 0x00041B94
		// (set) Token: 0x060008BE RID: 2238 RVA: 0x0004399E File Offset: 0x00041B9E
		public byte[] TemporaryBackImageData { get; set; }

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x060008BF RID: 2239 RVA: 0x000439A7 File Offset: 0x00041BA7
		// (set) Token: 0x060008C0 RID: 2240 RVA: 0x000439B1 File Offset: 0x00041BB1
		public byte[] TemporaryNormalPaletteData { get; set; }

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x000439BA File Offset: 0x00041BBA
		// (set) Token: 0x060008C2 RID: 2242 RVA: 0x000439C4 File Offset: 0x00041BC4
		public byte[] TemporaryShinyPaletteData { get; set; }

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x060008C3 RID: 2243 RVA: 0x000439CD File Offset: 0x00041BCD
		// (set) Token: 0x060008C4 RID: 2244 RVA: 0x000439D7 File Offset: 0x00041BD7
		public uint IconImageAddress { get; set; }

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x060008C5 RID: 2245 RVA: 0x000439E0 File Offset: 0x00041BE0
		// (set) Token: 0x060008C6 RID: 2246 RVA: 0x000439EA File Offset: 0x00041BEA
		public int IconPaletteId { get; set; }

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x060008C7 RID: 2247 RVA: 0x000439F3 File Offset: 0x00041BF3
		// (set) Token: 0x060008C8 RID: 2248 RVA: 0x000439FD File Offset: 0x00041BFD
		public byte[] TemporaryIconData { get; set; }

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x00043A06 File Offset: 0x00041C06
		// (set) Token: 0x060008CA RID: 2250 RVA: 0x00043A10 File Offset: 0x00041C10
		public uint FootprintAddress { get; set; }

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x060008CB RID: 2251 RVA: 0x00043A19 File Offset: 0x00041C19
		// (set) Token: 0x060008CC RID: 2252 RVA: 0x00043A23 File Offset: 0x00041C23
		public byte[] TemporaryFootprintData { get; set; }

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x060008CD RID: 2253 RVA: 0x00043A2C File Offset: 0x00041C2C
		// (set) Token: 0x060008CE RID: 2254 RVA: 0x00043A36 File Offset: 0x00041C36
		public byte OriginalGenderValue { get; set; }

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x060008CF RID: 2255 RVA: 0x00043A3F File Offset: 0x00041C3F
		// (set) Token: 0x060008D0 RID: 2256 RVA: 0x00043A49 File Offset: 0x00041C49
		public byte OriginalEggStepValue { get; set; }

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x060008D1 RID: 2257 RVA: 0x00043A52 File Offset: 0x00041C52
		// (set) Token: 0x060008D2 RID: 2258 RVA: 0x00043A5C File Offset: 0x00041C5C
		public byte OriginalEggGroup1Value { get; set; }

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x060008D3 RID: 2259 RVA: 0x00043A65 File Offset: 0x00041C65
		// (set) Token: 0x060008D4 RID: 2260 RVA: 0x00043A6F File Offset: 0x00041C6F
		public byte OriginalEggGroup2Value { get; set; }

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x060008D5 RID: 2261 RVA: 0x00043A78 File Offset: 0x00041C78
		// (set) Token: 0x060008D6 RID: 2262 RVA: 0x00043A82 File Offset: 0x00041C82
		public byte OriginalGrowthRateValue { get; set; }

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x060008D7 RID: 2263 RVA: 0x00043A8B File Offset: 0x00041C8B
		// (set) Token: 0x060008D8 RID: 2264 RVA: 0x00043A95 File Offset: 0x00041C95
		public byte OriginalPokemonColorValue { get; set; }

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x060008D9 RID: 2265 RVA: 0x00043A9E File Offset: 0x00041C9E
		// (set) Token: 0x060008DA RID: 2266 RVA: 0x00043AA8 File Offset: 0x00041CA8
		public byte OriginalPokemonDirectionValue { get; set; }

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x060008DB RID: 2267 RVA: 0x00043AB1 File Offset: 0x00041CB1
		// (set) Token: 0x060008DC RID: 2268 RVA: 0x00043ABB File Offset: 0x00041CBB
		public int OriginalAbility1Id { get; set; }

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x060008DD RID: 2269 RVA: 0x00043AC4 File Offset: 0x00041CC4
		// (set) Token: 0x060008DE RID: 2270 RVA: 0x00043ACE File Offset: 0x00041CCE
		public int OriginalAbility2Id { get; set; }

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x060008DF RID: 2271 RVA: 0x00043AD7 File Offset: 0x00041CD7
		// (set) Token: 0x060008E0 RID: 2272 RVA: 0x00043AE1 File Offset: 0x00041CE1
		public int OriginalAbilityHiddenId { get; set; }

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x060008E1 RID: 2273 RVA: 0x00043AEA File Offset: 0x00041CEA
		// (set) Token: 0x060008E2 RID: 2274 RVA: 0x00043AF4 File Offset: 0x00041CF4
		public int OriginalHoldItem1Id { get; set; }

		// Token: 0x17000351 RID: 849
		// (get) Token: 0x060008E3 RID: 2275 RVA: 0x00043AFD File Offset: 0x00041CFD
		// (set) Token: 0x060008E4 RID: 2276 RVA: 0x00043B07 File Offset: 0x00041D07
		public int OriginalHoldItem2Id { get; set; }

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x060008E5 RID: 2277 RVA: 0x00043B10 File Offset: 0x00041D10
		// (set) Token: 0x060008E6 RID: 2278 RVA: 0x00043B1A File Offset: 0x00041D1A
		public byte OriginalType1Id { get; set; }

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x060008E7 RID: 2279 RVA: 0x00043B23 File Offset: 0x00041D23
		// (set) Token: 0x060008E8 RID: 2280 RVA: 0x00043B2D File Offset: 0x00041D2D
		public byte OriginalType2Id { get; set; }

		// Token: 0x17000354 RID: 852
		// (get) Token: 0x060008E9 RID: 2281 RVA: 0x00043B36 File Offset: 0x00041D36
		// (set) Token: 0x060008EA RID: 2282 RVA: 0x00043B40 File Offset: 0x00041D40
		public byte[] TemporaryLevelMoveData { get; set; }

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x060008EB RID: 2283 RVA: 0x00043B49 File Offset: 0x00041D49
		// (set) Token: 0x060008EC RID: 2284 RVA: 0x00043B53 File Offset: 0x00041D53
		public uint TemporaryLevelMoveAddress { get; set; }

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x060008ED RID: 2285 RVA: 0x00043B5C File Offset: 0x00041D5C
		// (set) Token: 0x060008EE RID: 2286 RVA: 0x00043B66 File Offset: 0x00041D66
		public int PokedexOrder { get; set; }

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x060008EF RID: 2287 RVA: 0x00043B6F File Offset: 0x00041D6F
		// (set) Token: 0x060008F0 RID: 2288 RVA: 0x00043B79 File Offset: 0x00041D79
		public string PokedexCategory { get; set; }

		// Token: 0x17000358 RID: 856
		// (get) Token: 0x060008F1 RID: 2289 RVA: 0x00043B82 File Offset: 0x00041D82
		// (set) Token: 0x060008F2 RID: 2290 RVA: 0x00043B8C File Offset: 0x00041D8C
		public string OriginalPokedexCategory { get; set; }

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x060008F3 RID: 2291 RVA: 0x00043B95 File Offset: 0x00041D95
		// (set) Token: 0x060008F4 RID: 2292 RVA: 0x00043B9F File Offset: 0x00041D9F
		public ushort Height { get; set; }

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x060008F5 RID: 2293 RVA: 0x00043BA8 File Offset: 0x00041DA8
		// (set) Token: 0x060008F6 RID: 2294 RVA: 0x00043BB2 File Offset: 0x00041DB2
		public ushort Weight { get; set; }

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x060008F7 RID: 2295 RVA: 0x00043BBB File Offset: 0x00041DBB
		// (set) Token: 0x060008F8 RID: 2296 RVA: 0x00043BC5 File Offset: 0x00041DC5
		public ushort OriginalHeight { get; set; }

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x060008F9 RID: 2297 RVA: 0x00043BCE File Offset: 0x00041DCE
		// (set) Token: 0x060008FA RID: 2298 RVA: 0x00043BD8 File Offset: 0x00041DD8
		public ushort OriginalWeight { get; set; }

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x060008FB RID: 2299 RVA: 0x00043BE1 File Offset: 0x00041DE1
		// (set) Token: 0x060008FC RID: 2300 RVA: 0x00043BEB File Offset: 0x00041DEB
		public uint PokedexDescriptionAddress { get; set; }

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x060008FD RID: 2301 RVA: 0x00043BF4 File Offset: 0x00041DF4
		// (set) Token: 0x060008FE RID: 2302 RVA: 0x00043BFE File Offset: 0x00041DFE
		public uint OriginalPokedexDescriptionAddress { get; set; }

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x060008FF RID: 2303 RVA: 0x00043C07 File Offset: 0x00041E07
		// (set) Token: 0x06000900 RID: 2304 RVA: 0x00043C11 File Offset: 0x00041E11
		public string PokedexDescription { get; set; }

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x06000901 RID: 2305 RVA: 0x00043C1A File Offset: 0x00041E1A
		// (set) Token: 0x06000902 RID: 2306 RVA: 0x00043C24 File Offset: 0x00041E24
		public string OriginalPokedexDescription { get; set; }

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x06000903 RID: 2307 RVA: 0x00043C2D File Offset: 0x00041E2D
		// (set) Token: 0x06000904 RID: 2308 RVA: 0x00043C37 File Offset: 0x00041E37
		public ushort SizeComparison1 { get; set; }

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x06000905 RID: 2309 RVA: 0x00043C40 File Offset: 0x00041E40
		// (set) Token: 0x06000906 RID: 2310 RVA: 0x00043C4A File Offset: 0x00041E4A
		public short SizeComparison2 { get; set; }

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x06000907 RID: 2311 RVA: 0x00043C53 File Offset: 0x00041E53
		// (set) Token: 0x06000908 RID: 2312 RVA: 0x00043C5D File Offset: 0x00041E5D
		public ushort SizeComparison3 { get; set; }

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x06000909 RID: 2313 RVA: 0x00043C66 File Offset: 0x00041E66
		// (set) Token: 0x0600090A RID: 2314 RVA: 0x00043C70 File Offset: 0x00041E70
		public short SizeComparison4 { get; set; }

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x0600090B RID: 2315 RVA: 0x00043C79 File Offset: 0x00041E79
		// (set) Token: 0x0600090C RID: 2316 RVA: 0x00043C83 File Offset: 0x00041E83
		public ushort OriginalSizeComparison1 { get; set; }

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x0600090D RID: 2317 RVA: 0x00043C8C File Offset: 0x00041E8C
		// (set) Token: 0x0600090E RID: 2318 RVA: 0x00043C96 File Offset: 0x00041E96
		public short OriginalSizeComparison2 { get; set; }

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x0600090F RID: 2319 RVA: 0x00043C9F File Offset: 0x00041E9F
		// (set) Token: 0x06000910 RID: 2320 RVA: 0x00043CA9 File Offset: 0x00041EA9
		public ushort OriginalSizeComparison3 { get; set; }

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x06000911 RID: 2321 RVA: 0x00043CB2 File Offset: 0x00041EB2
		// (set) Token: 0x06000912 RID: 2322 RVA: 0x00043CBC File Offset: 0x00041EBC
		public short OriginalSizeComparison4 { get; set; }

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06000913 RID: 2323 RVA: 0x00043CC5 File Offset: 0x00041EC5
		// (set) Token: 0x06000914 RID: 2324 RVA: 0x00043CCF File Offset: 0x00041ECF
		public uint CryDataAddress { get; set; }

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06000915 RID: 2325 RVA: 0x00043CD8 File Offset: 0x00041ED8
		// (set) Token: 0x06000916 RID: 2326 RVA: 0x00043CE2 File Offset: 0x00041EE2
		public uint OriginalCryDataAddress { get; set; }

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06000917 RID: 2327 RVA: 0x00043CEB File Offset: 0x00041EEB
		// (set) Token: 0x06000918 RID: 2328 RVA: 0x00043CF5 File Offset: 0x00041EF5
		public Cry TemporaryCry { get; set; }

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06000919 RID: 2329 RVA: 0x00043CFE File Offset: 0x00041EFE
		// (set) Token: 0x0600091A RID: 2330 RVA: 0x00043D08 File Offset: 0x00041F08
		public uint TemporaryCryAddress { get; set; }

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x0600091B RID: 2331 RVA: 0x00043D11 File Offset: 0x00041F11
		// (set) Token: 0x0600091C RID: 2332 RVA: 0x00043D1B File Offset: 0x00041F1B
		public ushort Gen3CryId { get; set; }

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x0600091D RID: 2333 RVA: 0x00043D24 File Offset: 0x00041F24
		// (set) Token: 0x0600091E RID: 2334 RVA: 0x00043D2E File Offset: 0x00041F2E
		public ushort OriginalGen3CryId { get; set; }

		// Token: 0x0600091F RID: 2335 RVA: 0x00043D37 File Offset: 0x00041F37
		public PokemonData(int index, string name)
		{
			this.Index = index;
			this.Name = name;
			this.OriginalName = name;
		}
	}
}
