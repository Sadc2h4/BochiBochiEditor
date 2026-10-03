using System;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップチップ取り込みで使う、タイルセットの形（第1の枚数・ブロックのバイト数・挙動のバイト数など）をまとめた入れ物
	// 取り込みを始めるときに 1 回だけ作り、計画と書き込みで同じ値を使う
	//-------------------------------------------------------------------------------
	internal sealed record TileImportFormat(
		int PrimaryTileCount,
		int PrimaryBlockCount,
		int PrimaryPaletteCount,
		int BlockBytes,
		int BehaviorBytes,
		bool SupportsTripleLayer)
	{
		// タイル番号・ブロック番号の上限（10bit）と、パレットの合計本数
		public const int TileIdCapacity = 1024;
		public const int BlockIdCapacity = 1024;
		public const int TotalPaletteCount = 13;

		//-------------------------------------------------------------------------------
		// 1 ブロックのタイル指定の数（2 層なら 8、3 層なら 12）を返す処理
		//-------------------------------------------------------------------------------
		public int EntriesPerBlock
		{
			get { return this.BlockBytes / 2; }
		}

		//-------------------------------------------------------------------------------
		// 1 ブロックの層の数（2 層なら 2、3 層なら 3）を返す処理
		//-------------------------------------------------------------------------------
		public int LayersPerBlock
		{
			get { return this.BlockBytes / 8; }
		}

		//-------------------------------------------------------------------------------
		// ファイアレッドの「次のブロックを 3 層目に使う」方式かを返す処理
		//-------------------------------------------------------------------------------
		public bool TripleLayerByNextBlock
		{
			get { return this.SupportsTripleLayer && this.BlockBytes == 16 && this.BehaviorBytes == 4; }
		}

		//-------------------------------------------------------------------------------
		// 取り込みで扱える組み合わせかを返す処理
		//-------------------------------------------------------------------------------
		public bool IsSupported
		{
			get
			{
				return (this.BlockBytes == 16 || this.BlockBytes == 24)
					&& (this.BehaviorBytes == 4 || this.BehaviorBytes == 2)
					&& this.PrimaryTileCount >= 1 && this.PrimaryTileCount <= 1023
					&& this.PrimaryBlockCount >= 1 && this.PrimaryBlockCount <= 1023
					&& this.PrimaryPaletteCount >= 1 && this.PrimaryPaletteCount <= 12;
			}
		}

		//-------------------------------------------------------------------------------
		// タイルセットのタイルの通し番号の始まりを返す処理
		//-------------------------------------------------------------------------------
		public int TileBase(bool secondary)
		{
			return secondary ? this.PrimaryTileCount : 0;
		}

		//-------------------------------------------------------------------------------
		// タイルセットに置けるタイルの枚数（第2は 1024 - 第1の枚数）を返す処理
		//-------------------------------------------------------------------------------
		public int TileCapacity(bool secondary)
		{
			return secondary ? TileIdCapacity - this.PrimaryTileCount : this.PrimaryTileCount;
		}

		//-------------------------------------------------------------------------------
		// タイルセットのブロックの通し番号の始まりを返す処理
		//-------------------------------------------------------------------------------
		public int BlockBase(bool secondary)
		{
			return secondary ? this.PrimaryBlockCount : 0;
		}

		//-------------------------------------------------------------------------------
		// タイルセットが持つパレット枠の最初の番号を返す処理
		//-------------------------------------------------------------------------------
		public int PaletteFirst(bool secondary)
		{
			return secondary ? this.PrimaryPaletteCount : 0;
		}

		//-------------------------------------------------------------------------------
		// タイルセットが持つパレット枠の終わり（この番号は含まない）を返す処理
		//-------------------------------------------------------------------------------
		public int PaletteEndExclusive(bool secondary)
		{
			return secondary ? TotalPaletteCount : this.PrimaryPaletteCount;
		}

		//-------------------------------------------------------------------------------
		// 重ね方に応じて、絵を置く層の番号（0 が下層）を返す処理
		//-------------------------------------------------------------------------------
		public int ArtLayer(TileImportEngine.LayerStyle style)
		{
			switch (style)
			{
				case TileImportEngine.LayerStyle.ArtOnBottom:
					return 0;
				case TileImportEngine.LayerStyle.OverBaseBelowPlayer:
					return 1;
				case TileImportEngine.LayerStyle.OverBaseAbovePlayer:
					return this.LayersPerBlock - 1;
				default:
					throw new ArgumentOutOfRangeException(nameof(style));
			}
		}

		//-------------------------------------------------------------------------------
		// 今開いているゲームと、検出したブロックのバイト数から形を作る処理
		//-------------------------------------------------------------------------------
		public static TileImportFormat FromCurrentGame()
		{
			GameProfile profile = GameProfile.Current;
			return new TileImportFormat(profile.PrimaryTileCount, profile.PrimaryBlockCount, profile.PrimaryPaletteCount, MapEditor.BLOCK_DATA_SIZE, profile.BehaviorBytes, profile.SupportsTripleLayer);
		}

		//-------------------------------------------------------------------------------
		// ファイアレッドの形（640, 640, 7, 16, 4, 3 層あり）を返す処理（検証用・デザイナー用）
		//-------------------------------------------------------------------------------
		public static TileImportFormat FireRed()
		{
			return new TileImportFormat(640, 640, 7, 16, 4, true);
		}
	}
}
