using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「タイルアニメ再生」: 今のマップのタイルセットのタイルアニメ（花・水面・噴水など）を、ゲームと同じように動かして見せる処理
	// アニメの定義は、タイルセットの見出しの「アニメ処理」を TileAnimReader で読んで取り出す。
	// 動かすのは表示だけで、ROM・編集中のマップは変えない。描き直すのは、アニメするタイルを使うブロックとマスだけ
	// 「接続マップを表示」で出している接続先のマップも、その地形データのタイルセットのアニメで同じように動かす（材料は別に持つ）
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// ゲームの 1 フレーム（1/60 秒）を何フレームずつ進めるか、と、その間隔（ミリ秒）
		private const int TileAnimStepFrames = 4;
		private const int TileAnimIntervalMs = 66;

		private System.Windows.Forms.Timer tileAnimTimer;
		// 再生中の材料（タイルセットが変わったら作り直す）
		private List<TileAnimation> tileAnims;
		private byte[] tileAnimImage;
		private byte[] tileAnimOriginalImage;
		private Color[] tileAnimPalettes;
		private byte[] tileAnimPrimaryBlocks;
		private byte[] tileAnimSecondaryBlocks;
		private HashSet<int> tileAnimBlocks;
		private int[] tileAnimShownFrames;
		// ゲームのタイマー（フレーム数）
		private long tileAnimClock;
		// 接続先のマップ用の材料（接続先の地形データが変わったら作り直す）
		private MapFooter connAnimFooter;
		private List<TileAnimation> connAnims;
		private byte[] connAnimImage;
		private byte[] connAnimOriginalImage;
		private Color[] connAnimPalettes;
		private byte[] connAnimPrimaryBlocks;
		private byte[] connAnimSecondaryBlocks;
		private HashSet<int> connAnimBlocks;
		private int[] connAnimShownFrames;
		private Bitmap connAnimSheet;

		// 接続先のマップで見つかったアニメの数と、アニメするブロックの数（検証用）
		internal int ConnectedTileAnimationCount
		{
			get { return this.connAnims != null ? this.connAnims.Count : 0; }
		}

		internal int ConnectedAnimatedBlockCount
		{
			get { return this.connAnimBlocks != null ? this.connAnimBlocks.Count : 0; }
		}

		// 今のマップで見つかったアニメの数（検証用）
		internal int TileAnimationCount
		{
			get { return this.tileAnims != null ? this.tileAnims.Count : 0; }
		}

		//-------------------------------------------------------------------------------
		// 「タイルアニメ再生」のチェックを切り替えたとき
		//-------------------------------------------------------------------------------
		private void chkPlayTileAnimation_CheckedChanged(object sender, EventArgs e)
		{
			if (this.chkPlayTileAnimation.Checked)
			{
				this.StartTileAnimation();
			}
			else
			{
				this.StopTileAnimation(true);
			}
		}

		//-------------------------------------------------------------------------------
		// 再生を始める処理（材料を作り、タイマーを動かす。アニメが無いタイルセットでもタイマーは動かし、マップを替えたら拾う）
		//-------------------------------------------------------------------------------
		private void StartTileAnimation()
		{
			this.PrepareTileAnimation();
			if (this.tileAnimTimer == null)
			{
				this.tileAnimTimer = new System.Windows.Forms.Timer { Interval = TileAnimIntervalMs };
				this.tileAnimTimer.Tick += (sender, e) => this.AdvanceTileAnimation(TileAnimStepFrames);
			}
			this.tileAnimTimer.Start();
		}

		//-------------------------------------------------------------------------------
		// 再生を止める処理（restore なら、ROM のままの絵に戻す）
		//-------------------------------------------------------------------------------
		private void StopTileAnimation(bool restore)
		{
			if (this.tileAnimTimer != null)
			{
				this.tileAnimTimer.Stop();
			}
			if (restore && this.tileAnimOriginalImage != null && this.tileAnimImage != null)
			{
				Array.Copy(this.tileAnimOriginalImage, this.tileAnimImage, this.tileAnimImage.Length);
				this.RedrawAnimatedGraphics();
			}
			if (restore && this.connAnimOriginalImage != null && this.connAnimImage != null)
			{
				Array.Copy(this.connAnimOriginalImage, this.connAnimImage, this.connAnimImage.Length);
				this.RedrawConnectedAnimatedGraphics();
			}
			this.tileAnims = null;
			this.tileAnimImage = null;
			this.tileAnimOriginalImage = null;
			this.ClearConnectedTileAnimation();
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧の絵を作り直した後に呼ぶ処理（再生中なら、新しいタイルセットで材料を作り直す）
		//-------------------------------------------------------------------------------
		private void RefreshTileAnimationAfterRebuild()
		{
			if (this.chkPlayTileAnimation != null && this.chkPlayTileAnimation.Checked)
			{
				this.PrepareTileAnimation();
			}
		}

		//-------------------------------------------------------------------------------
		// 再生の材料（アニメの一覧・タイルの絵・パレット・ブロックのデータ・アニメするブロック）を作る処理
		// ブロック一覧の絵（CreateTilesetBitmap）と同じ材料を使う
		//-------------------------------------------------------------------------------
		internal void PrepareTileAnimation()
		{
			this.tileAnims = null;
			this.tileAnimImage = null;
			this.tileAnimOriginalImage = null;
			if (this.romData == null || this.tempTileset1 == null || this.tempTileset2 == null || this.blockPaletteBitmap == null)
			{
				return;
			}
			List<TileAnimation> anims = new List<TileAnimation>();
			anims.AddRange(TileAnimReader.Read(this.romData, this.tempTileset1.AnimationAddress));
			anims.AddRange(TileAnimReader.Read(this.romData, this.tempTileset2.AnimationAddress));
			byte[] image = this.BuildCombinedTileImage(this.tempTileset1, this.tempTileset2);
			// VRAM（1024 タイル）の外へはみ出すアニメは使わない
			anims.RemoveAll(a => a.DestTile < 0 || a.DestTile + a.TileCount > 1024);
			if (anims.Count == 0)
			{
				return;
			}
			// タイルセットの絵より後ろへ写すアニメ（ルネシティの水面など。ゲームでもアニメが初めて絵を入れる）のために、絵を広げる
			int needed = anims.Max(a => (a.DestTile + a.TileCount) * 32);
			if (needed > image.Length)
			{
				Array.Resize(ref image, needed);
			}
			this.tileAnimPalettes = this.LoadAllPalettes(this.tempTileset1, this.tempTileset2);
			int primaryUsed = this.GetPrimaryBlockCount(this.tempTileset1);
			byte[] primaryBlocks = this.LoadBlockData(this.tempTileset1, primaryUsed);
			Array.Resize(ref primaryBlocks, GameProfile.Current.PrimaryBlockCount * MapEditor.BLOCK_DATA_SIZE);
			this.tileAnimPrimaryBlocks = primaryBlocks;
			this.tileAnimSecondaryBlocks = this.LoadBlockData(this.tempTileset2, this.GetSecondaryBlockCount(Convert.ToInt32(this.nudTileset2Index.Value), this.tempTileset2));
			// アニメするタイルを 1 枚でも使うブロック（3 層のブロックの 3 層目は、前のブロックの絵に入る）
			HashSet<int> tiles = new HashSet<int>(anims.SelectMany(a => Enumerable.Range(a.DestTile, a.TileCount)));
			HashSet<int> blocks = new HashSet<int>();
			this.CollectAnimatedBlocks(this.tileAnimPrimaryBlocks, 0, tiles, blocks);
			this.CollectAnimatedBlocks(this.tileAnimSecondaryBlocks, GameProfile.Current.PrimaryBlockCount, tiles, blocks);
			this.tileAnimBlocks = blocks;
			this.tileAnims = anims;
			this.tileAnimImage = image;
			this.tileAnimOriginalImage = (byte[])image.Clone();
			// 初めの 1 回は必ず描く
			this.tileAnimShownFrames = Enumerable.Repeat(-1, anims.Count).ToArray();
		}

		//-------------------------------------------------------------------------------
		// 接続先のマップ用の材料を捨てる処理
		//-------------------------------------------------------------------------------
		private void ClearConnectedTileAnimation()
		{
			this.connAnimFooter = null;
			this.connAnims = null;
			this.connAnimImage = null;
			this.connAnimOriginalImage = null;
			this.connAnimBlocks = null;
			if (this.connAnimSheet != null)
			{
				this.connAnimSheet.Dispose();
				this.connAnimSheet = null;
			}
		}

		//-------------------------------------------------------------------------------
		// 接続先のマップ（「接続マップを表示」で出しているもの）用の材料を作る処理
		// 接続先の地形データのタイルセットからアニメを読み、そのブロック一覧の絵を自分で持って描き直す。接続先が無ければ材料を捨てる
		//-------------------------------------------------------------------------------
		private void PrepareConnectedTileAnimation()
		{
			this.ClearConnectedTileAnimation();
			if (this.romData == null || this.cachedConnFooter == null || this.cachedConnMatrix == null || this.connectedMapLayerBitmap == null)
			{
				return;
			}
			MapFooter footer = this.cachedConnFooter;
			TilesetHeader ts1 = this.ReadTilesetHeader((int)footer.Tileset1Address);
			TilesetHeader ts2 = this.ReadTilesetHeader((int)footer.Tileset2Address);
			if (ts1 == null || ts2 == null)
			{
				return;
			}
			List<TileAnimation> anims = new List<TileAnimation>();
			anims.AddRange(TileAnimReader.Read(this.romData, ts1.AnimationAddress));
			anims.AddRange(TileAnimReader.Read(this.romData, ts2.AnimationAddress));
			anims.RemoveAll(an => an.DestTile < 0 || an.DestTile + an.TileCount > 1024);
			this.connAnimFooter = footer;
			if (anims.Count == 0)
			{
				return;
			}
			byte[] image = this.BuildCombinedTileImage(ts1, ts2);
			int needed = anims.Max(an => (an.DestTile + an.TileCount) * 32);
			if (needed > image.Length)
			{
				Array.Resize(ref image, needed);
			}
			byte[] primaryBlocks = this.LoadBlockData(ts1, this.GetPrimaryBlockCount(ts1));
			Array.Resize(ref primaryBlocks, GameProfile.Current.PrimaryBlockCount * MapEditor.BLOCK_DATA_SIZE);
			byte[] secondaryBlocks = this.LoadBlockData(ts2, this.GetSecondaryBlockCount(this.AddressToTilesetIndex(footer.Tileset2Address), ts2));
			HashSet<int> tiles = new HashSet<int>(anims.SelectMany(an => Enumerable.Range(an.DestTile, an.TileCount)));
			HashSet<int> blocks = new HashSet<int>();
			this.CollectAnimatedBlocks(primaryBlocks, 0, tiles, blocks, false);
			this.CollectAnimatedBlocks(secondaryBlocks, GameProfile.Current.PrimaryBlockCount, tiles, blocks, false);
			Bitmap sheet = this.GeneratePaletteBitmapForMap(footer);
			if (sheet == null)
			{
				return;
			}
			this.connAnimSheet = sheet;
			this.connAnimPalettes = this.LoadAllPalettes(ts1, ts2);
			this.connAnimPrimaryBlocks = primaryBlocks;
			this.connAnimSecondaryBlocks = secondaryBlocks;
			this.connAnimBlocks = blocks;
			this.connAnims = anims;
			this.connAnimImage = image;
			this.connAnimOriginalImage = (byte[])image.Clone();
			this.connAnimShownFrames = Enumerable.Repeat(-1, anims.Count).ToArray();
		}

		//-------------------------------------------------------------------------------
		// ブロックのデータから、アニメするタイルを使うブロックの通し番号を集める処理
		// tripleLayer なら、3 層のブロックの前のブロック（3 層目がその絵に入る）も入れる（今のマップのタイルセットのときだけ）
		//-------------------------------------------------------------------------------
		private void CollectAnimatedBlocks(byte[] blockData, int startIndex, HashSet<int> tiles, HashSet<int> blocks, bool tripleLayer = true)
		{
			int blockSize = MapEditor.BLOCK_DATA_SIZE;
			for (int i = 0; i * blockSize + blockSize <= blockData.Length; i++)
			{
				for (int entry = 0; entry < blockSize / 2; entry++)
				{
					int tile = BitConverter.ToUInt16(blockData, i * blockSize + entry * 2) & 1023;
					if (!tiles.Contains(tile))
					{
						continue;
					}
					int id = startIndex + i;
					blocks.Add(id);
					if (tripleLayer && id > 0 && this.IsTripleLayerBlock(id - 1))
					{
						blocks.Add(id - 1);
					}
					break;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ゲームのタイマーを frames だけ進め、コマが変わったアニメがあれば絵を差し替えて描き直す処理
		//-------------------------------------------------------------------------------
		internal void AdvanceTileAnimation(int frames)
		{
			if (this.IsDisposed)
			{
				this.StopTileAnimation(false);
				return;
			}
			if (this.tileAnims == null || this.tileAnimImage == null || this.blockPaletteBitmap == null)
			{
				return;
			}
			this.tileAnimClock += frames;
			bool changed = false;
			for (int i = 0; i < this.tileAnims.Count; i++)
			{
				TileAnimation anim = this.tileAnims[i];
				int frame = anim.FrameAt(this.tileAnimClock);
				if (frame == this.tileAnimShownFrames[i])
				{
					continue;
				}
				this.tileAnimShownFrames[i] = frame;
				int bytes = anim.TileCount * 32;
				long source = anim.Frames[frame];
				if (source >= 0 && source + bytes <= this.romData.Length)
				{
					Array.Copy(this.romData, source, this.tileAnimImage, anim.DestTile * 32, bytes);
					changed = true;
				}
			}
			if (changed)
			{
				this.RedrawAnimatedGraphics();
			}
			this.AdvanceConnectedTileAnimation();
		}

		//-------------------------------------------------------------------------------
		// 接続先のマップのアニメを、同じタイマーで進める処理（接続先が変わっていれば材料を作り直す）
		//-------------------------------------------------------------------------------
		private void AdvanceConnectedTileAnimation()
		{
			bool shown = this.chkShowConnectedMap != null && this.chkShowConnectedMap.Checked && this.connectedMapLayerBitmap != null && this.cachedConnFooter != null;
			if (!shown)
			{
				if (this.connAnimFooter != null)
				{
					this.ClearConnectedTileAnimation();
				}
				return;
			}
			if (!ReferenceEquals(this.connAnimFooter, this.cachedConnFooter))
			{
				this.PrepareConnectedTileAnimation();
			}
			if (this.connAnims == null || this.connAnimImage == null)
			{
				return;
			}
			bool changed = false;
			for (int i = 0; i < this.connAnims.Count; i++)
			{
				TileAnimation anim = this.connAnims[i];
				int frame = anim.FrameAt(this.tileAnimClock);
				if (frame == this.connAnimShownFrames[i])
				{
					continue;
				}
				this.connAnimShownFrames[i] = frame;
				int bytes = anim.TileCount * 32;
				long source = anim.Frames[frame];
				if (source >= 0 && source + bytes <= this.romData.Length)
				{
					Array.Copy(this.romData, source, this.connAnimImage, anim.DestTile * 32, bytes);
					changed = true;
				}
			}
			if (changed)
			{
				this.RedrawConnectedAnimatedGraphics();
			}
		}

		//-------------------------------------------------------------------------------
		// 接続先のマップの、アニメするブロックを使っているマスを描き直す処理（接続先は灰色寄りで出しているので、同じ色の変換で描く）
		//-------------------------------------------------------------------------------
		private void RedrawConnectedAnimatedGraphics()
		{
			if (this.connAnimSheet == null || this.connAnimBlocks == null || this.connAnimBlocks.Count == 0 || this.connectedMapLayerBitmap == null || this.cachedConnMatrix == null)
			{
				return;
			}
			using (Graphics g = Graphics.FromImage(this.connAnimSheet))
			{
				this.DrawBlockBatch(g, this.connAnimPrimaryBlocks, this.connAnimImage, this.connAnimPalettes, 0, this.connAnimBlocks);
				this.DrawBlockBatch(g, this.connAnimSecondaryBlocks, this.connAnimImage, this.connAnimPalettes, GameProfile.Current.PrimaryBlockCount, this.connAnimBlocks);
			}
			float[][] matrix =
			{
				new float[] { 0.299f, 0.299f, 0.299f, 0f, 0f },
				new float[] { 0.587f, 0.587f, 0.587f, 0f, 0f },
				new float[] { 0.114f, 0.114f, 0.114f, 0f, 0f },
				new float[] { 0f, 0f, 0f, 1f, 0f },
				new float[] { 0f, 0f, 0f, 0f, 1f },
			};
			using (ImageAttributes attributes = new ImageAttributes())
			using (Graphics g = Graphics.FromImage(this.connectedMapLayerBitmap))
			{
				attributes.SetColorMatrix(new ColorMatrix(matrix));
				g.CompositingMode = CompositingMode.SourceCopy;
				int width = this.cachedConnMatrix.GetLength(0);
				int height = this.cachedConnMatrix.GetLength(1);
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						int id = this.cachedConnMatrix[x, y].BlockIndex;
						if (!this.connAnimBlocks.Contains(id) || id / 8 * 16 >= this.connAnimSheet.Height)
						{
							continue;
						}
						g.DrawImage(this.connAnimSheet, new Rectangle(x * 16, y * 16, 16, 16), id % 8 * 16, id / 8 * 16, 16, 16, GraphicsUnit.Pixel, attributes);
					}
				}
			}
			this.UpdateMapRender();
			this.RefreshMapCanvas();
		}

		//-------------------------------------------------------------------------------
		// 今のタイルの絵で、アニメするブロックと、それを使うマップ・ボーダーのマスを描き直す処理
		//-------------------------------------------------------------------------------
		private void RedrawAnimatedGraphics()
		{
			if (this.blockPaletteBitmap == null || this.tileAnimBlocks == null || this.tileAnimBlocks.Count == 0)
			{
				return;
			}
			using (Graphics g = Graphics.FromImage(this.blockPaletteBitmap))
			{
				this.DrawBlockBatch(g, this.tileAnimPrimaryBlocks, this.tileAnimImage, this.tileAnimPalettes, 0, this.tileAnimBlocks);
				this.DrawBlockBatch(g, this.tileAnimSecondaryBlocks, this.tileAnimImage, this.tileAnimPalettes, GameProfile.Current.PrimaryBlockCount, this.tileAnimBlocks);
			}
			if (this.mapMatrix != null && this.primaryMapLayerBitmap != null)
			{
				using (Graphics g = Graphics.FromImage(this.primaryMapLayerBitmap))
				{
					g.CompositingMode = CompositingMode.SourceCopy;
					int width = this.mapMatrix.GetLength(0);
					int height = this.mapMatrix.GetLength(1);
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < width; x++)
						{
							int id = this.mapMatrix[x, y].BlockIndex;
							if (!this.tileAnimBlocks.Contains(id) || id / 8 * 16 >= this.blockPaletteBitmap.Height)
							{
								continue;
							}
							g.DrawImage(this.blockPaletteBitmap, new Rectangle(x * 16, y * 16, 16, 16), new Rectangle(id % 8 * 16, id / 8 * 16, 16, 16), GraphicsUnit.Pixel);
						}
					}
				}
				this.UpdateMapRender();
			}
			// ボーダーは、アニメするブロックを使っているときだけ作り直す
			if (this.borderMatrix != null && this.borderMatrix.Cast<int>().Any(id => this.tileAnimBlocks.Contains(id)))
			{
				if (this.borderBitmap != null)
				{
					this.borderBitmap.Dispose();
					this.borderBitmap = null;
				}
				this.UpdateBorderRender();
			}
			this.RefreshMapCanvas();
		}
	}
}
