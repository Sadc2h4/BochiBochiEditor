using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップ一覧のサムネイル表示・作成待ち・キャッシュを管理する処理群
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		private const int GWL_STYLE = -16;
		private const int TVS_NOHSCROLL = 0x8000;
		private readonly Dictionary<uint, Bitmap> mapThumbnailImages = new Dictionary<uint, Bitmap>();
		private readonly HashSet<uint> thumbnailDone = new HashSet<uint>();
		private CancellationTokenSource thumbnailCancel;
		private int thumbnailSize;
		private int defaultTreeItemHeight;
		private int thumbnailPendingCount;

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		internal int MapThumbnailPendingCount
		{
			get { return Volatile.Read(ref this.thumbnailPendingCount); }
		}

		internal int MapThumbnailDoneCount
		{
			get { return this.thumbnailDone.Count; }
		}

		internal IReadOnlyDictionary<uint, Bitmap> MapThumbnailImages
		{
			get { return this.mapThumbnailImages; }
		}

		//-------------------------------------------------------------------------------
		// マップ一覧のサムネイル用部品と保存済み設定を初期化する処理
		//-------------------------------------------------------------------------------
		private void InitializeMapThumbnails()
		{
			this.defaultTreeItemHeight = this.tvwMapSelector.ItemHeight;
			this.thumbnailSize = this.LogicalToDeviceUnits(48);
			this.tvwMapSelector.HandleCreated -= this.tvwMapSelector_HandleCreated;
			this.tvwMapSelector.HandleCreated += this.tvwMapSelector_HandleCreated;
			this.chkMapThumbnail.Checked = AppSettings.Get("MapThumbnails", "1") == "1";
			this.chkMapThumbnail.CheckedChanged += this.chkMapThumbnail_CheckedChanged;
			this.mapToolTip.SetToolTip(this.chkMapThumbnail, Localizer.T("マップの一覧に、マップ全体の縮小画像を出します。"));
			this.ApplyMapThumbnailMode();
		}

		//-------------------------------------------------------------------------------
		// サムネイル表示設定が変わったときに保存して一覧へ反映する処理
		//-------------------------------------------------------------------------------
		private void chkMapThumbnail_CheckedChanged(object sender, EventArgs e)
		{
			AppSettings.Set("MapThumbnails", this.chkMapThumbnail.Checked ? "1" : "0");
			this.ApplyMapThumbnailMode();
		}

		//-------------------------------------------------------------------------------
		// サムネイルの有無に合わせて TreeView の描画方法と行の高さを切り替える処理
		//-------------------------------------------------------------------------------
		private void ApplyMapThumbnailMode()
		{
			if (this.chkMapThumbnail.Checked)
			{
				this.tvwMapSelector.ImageList = null;
				this.tvwMapSelector.DrawNode -= this.tvwMapSelector_DrawThumbnailNode;
				this.tvwMapSelector.DrawNode += this.tvwMapSelector_DrawThumbnailNode;
				this.tvwMapSelector.DrawMode = TreeViewDrawMode.OwnerDrawAll;
				this.tvwMapSelector.ItemHeight = this.thumbnailSize + 4;
			}
			else
			{
				this.CancelMapThumbnailGeneration();
				this.tvwMapSelector.DrawNode -= this.tvwMapSelector_DrawThumbnailNode;
				this.tvwMapSelector.DrawMode = TreeViewDrawMode.Normal;
				this.tvwMapSelector.ImageList = null;
				this.tvwMapSelector.ItemHeight = this.defaultTreeItemHeight;
			}
			this.UpdateMapThumbnailHorizontalScrollStyle();
			this.AssignMapThumbnailKeys(this.tvwMapSelector.Nodes);
			this.StartMapThumbnailGeneration();
			this.tvwMapSelector.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// サムネイル表示中のツリーを描き直す処理
		//-------------------------------------------------------------------------------
		private void AssignMapThumbnailKeys(TreeNodeCollection nodes)
		{
			if (this.chkMapThumbnail.Checked)
			{
				this.tvwMapSelector.Invalidate();
			}
		}

		//-------------------------------------------------------------------------------
		// サムネイル表示用にツリーの各行を自前で描く処理
		//-------------------------------------------------------------------------------
		private void tvwMapSelector_DrawThumbnailNode(object sender, DrawTreeNodeEventArgs e)
		{
			if (e.Bounds.IsEmpty || e.Bounds.Height <= 0)
			{
				return;
			}

			e.DrawDefault = false;
			Rectangle rowBounds = new Rectangle(0, e.Bounds.Top, this.tvwMapSelector.ClientSize.Width, e.Bounds.Height);
			Color backColor = e.Node.BackColor.IsEmpty ? this.tvwMapSelector.BackColor : e.Node.BackColor;
			Color foreColor = e.Node.ForeColor.IsEmpty ? this.tvwMapSelector.ForeColor : e.Node.ForeColor;
			using (SolidBrush background = new SolidBrush(backColor))
			{
				e.Graphics.FillRectangle(background, rowBounds);
			}

			int x = e.Node.Bounds.Left;
			Font font = e.Node.NodeFont ?? this.tvwMapSelector.Font;
			if (e.Node.Nodes.Count > 0)
			{
				Rectangle glyphBounds = new Rectangle(x - 16, rowBounds.Top, 16, rowBounds.Height);
				TextRenderer.DrawText(e.Graphics, e.Node.IsExpanded ? "▾" : "▸", font, glyphBounds, foreColor,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
				Rectangle textBounds = new Rectangle(x, rowBounds.Top, Math.Max(0, rowBounds.Right - x - 4), rowBounds.Height);
				if (textBounds.Width > 0)
				{
					TextRenderer.DrawText(e.Graphics, e.Node.Text, font, textBounds, foreColor,
						TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
				}
				return;
			}

			if (TryGetThumbnailFooterOffset(e.Node, out uint footerOffset))
			{
				int thumbnailTop = rowBounds.Top + Math.Max(0, (rowBounds.Height - this.thumbnailSize) / 2);
				Rectangle thumbnailBounds = new Rectangle(x, thumbnailTop, this.thumbnailSize, this.thumbnailSize);
				if (this.mapThumbnailImages.TryGetValue(footerOffset, out Bitmap bitmap))
				{
					InterpolationMode oldInterpolationMode = e.Graphics.InterpolationMode;
					e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					e.Graphics.DrawImage(bitmap, thumbnailBounds);
					e.Graphics.InterpolationMode = oldInterpolationMode;
				}
				else if (footerOffset != 0 && !this.thumbnailDone.Contains(footerOffset))
				{
					using (Pen pen = new Pen(Color.FromArgb(72, SystemColors.ControlDark), 1f))
					{
						e.Graphics.DrawRectangle(pen, thumbnailBounds.X, thumbnailBounds.Y,
							Math.Max(0, thumbnailBounds.Width - 1), Math.Max(0, thumbnailBounds.Height - 1));
					}
				}

				int textLeft = x + this.thumbnailSize + 6;
				int textHeight = Math.Min(rowBounds.Height, font.Height * 2);
				int textTop = rowBounds.Top + Math.Max(0, (rowBounds.Height - textHeight) / 2);
				Rectangle textBounds = new Rectangle(textLeft, textTop, Math.Max(0, rowBounds.Right - textLeft - 4), textHeight);
				if (textBounds.Width > 0)
				{
					TextRenderer.DrawText(e.Graphics, e.Node.Text, font, textBounds, foreColor,
						TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
				}
				return;
			}

			Rectangle fallbackTextBounds = new Rectangle(x, rowBounds.Top, Math.Max(0, rowBounds.Right - x - 4), rowBounds.Height);
			if (fallbackTextBounds.Width > 0)
			{
				TextRenderer.DrawText(e.Graphics, e.Node.Text, font, fallbackTextBounds, foreColor,
					TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
			}
		}

		//-------------------------------------------------------------------------------
		// TreeView のハンドルが作り直されたときに横スクロール抑止を再適用する処理
		//-------------------------------------------------------------------------------
		private void tvwMapSelector_HandleCreated(object sender, EventArgs e)
		{
			this.UpdateMapThumbnailHorizontalScrollStyle();
		}

		//-------------------------------------------------------------------------------
		// サムネイル表示中だけ TreeView の横スクロールを抑止する処理
		//-------------------------------------------------------------------------------
		private void UpdateMapThumbnailHorizontalScrollStyle()
		{
			if (!this.tvwMapSelector.IsHandleCreated)
			{
				return;
			}
			int style = GetWindowLong(this.tvwMapSelector.Handle, GWL_STYLE);
			int newStyle = this.chkMapThumbnail.Checked ? style | TVS_NOHSCROLL : style & ~TVS_NOHSCROLL;
			if (newStyle != style)
			{
				SetWindowLong(this.tvwMapSelector.Handle, GWL_STYLE, newStyle);
			}
		}

		//-------------------------------------------------------------------------------
		// 未作成の地形データを選択中のマップから順に背景処理へ送る処理
		//-------------------------------------------------------------------------------
		private void StartMapThumbnailGeneration()
		{
			if (!this.chkMapThumbnail.Checked || this.romData == null)
			{
				return;
			}

			this.CancelMapThumbnailGeneration();
			List<uint> footerOffsets = new List<uint>();
			HashSet<uint> seen = new HashSet<uint>();
			if (TryGetThumbnailFooterOffset(this.tvwMapSelector.SelectedNode, out uint selectedOffset))
			{
				AddPendingThumbnailOffset(selectedOffset, footerOffsets, seen, this.thumbnailDone);
			}
			foreach (MapEditor.MapHeader header in this.mapHeaders)
			{
				AddPendingThumbnailOffset(header.FooterAddress, footerOffsets, seen, this.thumbnailDone);
			}

			if (footerOffsets.Count == 0)
			{
				return;
			}

			MapThumbnailRenderer.Settings settings = MapThumbnailRenderer.Settings.FromCurrentGame();
			byte[] rom = this.romData;
			CancellationTokenSource cancel = new CancellationTokenSource();
			this.thumbnailCancel = cancel;
			Interlocked.Exchange(ref this.thumbnailPendingCount, footerOffsets.Count);

			Task.Run(() =>
			{
				MapThumbnailRenderer.TilesetCache cache = new MapThumbnailRenderer.TilesetCache();
				List<KeyValuePair<uint, Bitmap>> batch = new List<KeyValuePair<uint, Bitmap>>(16);
				foreach (uint footerOffset in footerOffsets)
				{
					if (cancel.IsCancellationRequested)
					{
						DisposeThumbnailBatch(batch);
						return;
					}
					Bitmap bitmap = MapThumbnailRenderer.Render(rom, footerOffset, settings, cache, this.thumbnailSize);
					batch.Add(new KeyValuePair<uint, Bitmap>(footerOffset, bitmap));
					if (batch.Count == 16)
					{
						this.PostThumbnailBatch(batch, cancel);
						batch = new List<KeyValuePair<uint, Bitmap>>(16);
					}
				}
				if (batch.Count > 0)
				{
					this.PostThumbnailBatch(batch, cancel);
				}
			}, cancel.Token);
		}

		//-------------------------------------------------------------------------------
		// 作成済みの画像を UI スレッドへ渡して辞書とツリーへ反映する処理
		//-------------------------------------------------------------------------------
		private void PostThumbnailBatch(List<KeyValuePair<uint, Bitmap>> batch, CancellationTokenSource cancel)
		{
			if (cancel.IsCancellationRequested || this.IsDisposed || !this.IsHandleCreated)
			{
				DisposeThumbnailBatch(batch);
				if (!cancel.IsCancellationRequested)
				{
					Interlocked.Exchange(ref this.thumbnailPendingCount, 0);
				}
				return;
			}

			try
			{
				this.BeginInvoke(new Action(() =>
				{
					if (cancel.IsCancellationRequested || this.IsDisposed || this.thumbnailCancel != cancel)
					{
						DisposeThumbnailBatch(batch);
						return;
					}
					foreach (KeyValuePair<uint, Bitmap> item in batch)
					{
						if (this.mapThumbnailImages.TryGetValue(item.Key, out Bitmap oldBitmap))
						{
							this.mapThumbnailImages.Remove(item.Key);
							oldBitmap.Dispose();
						}
						if (item.Value != null)
						{
							this.mapThumbnailImages.Add(item.Key, item.Value);
						}
						this.thumbnailDone.Add(item.Key);
					}
					Interlocked.Add(ref this.thumbnailPendingCount, -batch.Count);
					this.tvwMapSelector.Invalidate();
				}));
			}
			catch (InvalidOperationException)
			{
				DisposeThumbnailBatch(batch);
				if (!cancel.IsCancellationRequested)
				{
					Interlocked.Exchange(ref this.thumbnailPendingCount, 0);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ROM の読み直しや共有タイルセット変更時に全サムネイルを破棄する処理
		//-------------------------------------------------------------------------------
		private void ResetMapThumbnails()
		{
			this.CancelMapThumbnailGeneration();
			foreach (Bitmap bitmap in this.mapThumbnailImages.Values)
			{
				bitmap.Dispose();
			}
			this.mapThumbnailImages.Clear();
			this.thumbnailDone.Clear();
			this.tvwMapSelector.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 指定した地形データのサムネイルだけを消して作り直す処理
		//-------------------------------------------------------------------------------
		private void InvalidateMapThumbnail(uint footerOffset)
		{
			if (footerOffset == 0)
			{
				return;
			}
			if (this.mapThumbnailImages.TryGetValue(footerOffset, out Bitmap bitmap))
			{
				this.mapThumbnailImages.Remove(footerOffset);
				bitmap.Dispose();
			}
			this.thumbnailDone.Remove(footerOffset);
			this.AssignMapThumbnailKeys(this.tvwMapSelector.Nodes);
			this.StartMapThumbnailGeneration();
		}

		//-------------------------------------------------------------------------------
		// 実行中のサムネイル作成を取り消して待ち件数を空にする処理
		//-------------------------------------------------------------------------------
		private void CancelMapThumbnailGeneration()
		{
			CancellationTokenSource cancel = this.thumbnailCancel;
			this.thumbnailCancel = null;
			if (cancel != null)
			{
				cancel.Cancel();
			}
			Interlocked.Exchange(ref this.thumbnailPendingCount, 0);
		}

		//-------------------------------------------------------------------------------
		// ツリーノードの Tag からサムネイル対象の地形データ位置を得る処理
		//-------------------------------------------------------------------------------
		private static bool TryGetThumbnailFooterOffset(TreeNode node, out uint footerOffset)
		{
			footerOffset = 0;
			if (node?.Tag is MapEditor.MapHeader header)
			{
				footerOffset = header.FooterAddress;
				return true;
			}
			if (node?.Tag is uint address)
			{
				footerOffset = address;
				return true;
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 重複・作成済み・位置 0 を除いて作成待ちの末尾へ追加する処理
		//-------------------------------------------------------------------------------
		private static void AddPendingThumbnailOffset(uint footerOffset, List<uint> offsets, HashSet<uint> seen, HashSet<uint> done)
		{
			if (footerOffset != 0 && !done.Contains(footerOffset) && seen.Add(footerOffset))
			{
				offsets.Add(footerOffset);
			}
		}

		//-------------------------------------------------------------------------------
		// UI に渡せなかった Bitmap の組を破棄する処理
		//-------------------------------------------------------------------------------
		private static void DisposeThumbnailBatch(List<KeyValuePair<uint, Bitmap>> batch)
		{
			foreach (KeyValuePair<uint, Bitmap> item in batch)
			{
				item.Value?.Dispose();
			}
		}
	}
}
