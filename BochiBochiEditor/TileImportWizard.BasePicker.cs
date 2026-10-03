using System;
using System.Drawing;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 下地ブロックの選択（番号の入力・一覧からの選択・下層だけの見本）
	//-------------------------------------------------------------------------------
	public partial class TileImportWizard
	{
		//-------------------------------------------------------------------------------
		// 「一覧から選ぶ」ボタンで、ブロック一覧のウィンドウを開いて下地ブロックを選ぶ処理
		//-------------------------------------------------------------------------------
		private void btnPickBaseBlock_Click(object sender, EventArgs e)
		{
			if (this.host == null)
			{
				return;
			}
			int total = this.host.TotalBlocks;
			if (total <= 0)
			{
				return;
			}
			bool[] hasTop;
			using (Bitmap full = this.host.GetBlockPaletteImage())
			using (Bitmap bottom = this.BuildBottomLayerAtlas(total, out hasTop))
			using (BlockPickerForm picker = new BlockPickerForm(full, bottom, hasTop, total, this.host.PrimaryBlocks, (int)this.nudBaseBlock.Value, this.GetBasePickerHint()))
			{
				UiTheme.Apply(picker);
				if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedBlock >= 0)
				{
					this.nudBaseBlock.Value = Math.Min(this.nudBaseBlock.Maximum, picker.SelectedBlock);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 番号を打ち込んでいる途中でも、手が止まったら見本に反映するため、入力のたびに待ち時間を数え直す処理
		//-------------------------------------------------------------------------------
		private void nudBaseBlock_TextChanged(object sender, EventArgs e)
		{
			this.tmrBaseBlockInput.Stop();
			this.tmrBaseBlockInput.Start();
		}

		//-------------------------------------------------------------------------------
		// 入力が止まったら、打ち込んだ番号を確定させる処理（確定すると ValueChanged で計画が作り直される）
		//-------------------------------------------------------------------------------
		private void tmrBaseBlockInput_Tick(object sender, EventArgs e)
		{
			this.tmrBaseBlockInput.Stop();
			// Value を読むと、打ち込み中の文字が数値として確定する
			decimal value = this.nudBaseBlock.Value;
			this.pnlBaseBlockPreview.Invalidate();
		}

		//-------------------------------------------------------------------------------
		// 下地として写す層（絵の層より下）のビットの集まりを返す処理（ビット 0 が下層。少なくとも下層は含める）
		//-------------------------------------------------------------------------------
		private int BaseCopyLayerMask
		{
			get
			{
				TileImportEngine.LayerStyle style = this.planOptions != null ? this.planOptions.Layer : TileImportEngine.LayerStyle.OverBaseBelowPlayer;
				int mask = (1 << this.format.ArtLayer(style)) - 1;
				return mask == 0 ? 1 : mask;
			}
		}

		//-------------------------------------------------------------------------------
		// 下地として写さない層のビットの集まりを返す処理
		//-------------------------------------------------------------------------------
		private int BaseDroppedLayerMask
		{
			get { return ((1 << this.format.LayersPerBlock) - 1) & ~this.BaseCopyLayerMask; }
		}

		//-------------------------------------------------------------------------------
		// ブロック一覧のウィンドウに出す、下地に使われる層の案内文を返す処理（2 層なら既定の文のまま null）
		//-------------------------------------------------------------------------------
		private string GetBasePickerHint()
		{
			if (this.format.LayersPerBlock < 3)
			{
				return null;
			}
			return this.BaseCopyLayerMask == 3
				? Localizer.T("右上の印は、上層にも絵があるブロックです（下地に使われるのは下層と中層だけ）。")
				: Localizer.T("右上の印は、中層か上層にも絵があるブロックです（下地に使われるのは下層だけ）。");
		}

		//-------------------------------------------------------------------------------
		// 全ブロックの「下地として写す層だけ」の絵を、横 8 ブロックの一覧画像にまとめる処理
		// 下地に使われるのは絵の層より下の層だけなので、一覧でもその層だけを見せられるようにする
		// hasTop には、写さない層にも絵があるブロックを記録する（計画が無ければ null を返す）
		//-------------------------------------------------------------------------------
		private Bitmap BuildBottomLayerAtlas(int total, out bool[] hasTop)
		{
			hasTop = null;
			if (this.plan == null || this.planOptions == null || this.plan.Target == null)
			{
				return null;
			}
			hasTop = new bool[total];
			int copyMask = this.BaseCopyLayerMask;
			int droppedMask = this.BaseDroppedLayerMask;
			int rows = (total + 7) / 8;
			Bitmap atlas = new Bitmap(8 * 16, rows * 16);
			using (Graphics g = Graphics.FromImage(atlas))
			{
				for (int id = 0; id < total; id++)
				{
					ushort[] entries = this.host.GetBlockEntries(id);
					if (entries == null)
					{
						continue;
					}
					using (Bitmap bottom = TileImportEngine.RenderBlock(entries, this.plan, this.planOptions, copyMask))
					{
						g.DrawImageUnscaled(bottom, id % 8 * 16, id / 8 * 16);
					}
					hasTop[id] = droppedMask != 0 && this.HasVisiblePixels(entries, droppedMask);
				}
			}
			return atlas;
		}

		//-------------------------------------------------------------------------------
		// ブロックの指定した層に、透明でない点が 1 つでもあるかを調べる処理
		//-------------------------------------------------------------------------------
		private bool HasVisiblePixels(ushort[] entries, int layerMask)
		{
			using (Bitmap layer = TileImportEngine.RenderBlock(entries, this.plan, this.planOptions, layerMask))
			{
				for (int y = 0; y < 16; y++)
				{
					for (int x = 0; x < 16; x++)
					{
						if (layer.GetPixel(x, y).A != 0)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 下地ブロックの上層に絵があれば、「下層だけが使われる」ことを計画の注意に足す処理
		//-------------------------------------------------------------------------------
		private void AddBaseBlockNotes()
		{
			if (this.plan == null || this.planOptions == null || this.planOptions.Layer == TileImportEngine.LayerStyle.ArtOnBottom)
			{
				return;
			}
			int id = (int)this.nudBaseBlock.Value;
			ushort[] entries = this.host.GetBlockEntries(id);
			if (entries == null)
			{
				this.plan.Notes.Add(string.Format(Localizer.T("下地ブロック 0x{0:X3} はありません。一覧から選び直してください。"), id));
				return;
			}
			int droppedMask = this.BaseDroppedLayerMask;
			if (droppedMask != 0 && this.HasVisiblePixels(entries, droppedMask))
			{
				string note;
				if (this.format.LayersPerBlock < 3)
				{
					note = Localizer.T("下地ブロック 0x{0:X3} は上層にも絵があります。下地に使われるのは下層の絵だけです（番号の横の見本は、実際に使われる下層だけを表示しています）。");
				}
				else if (this.BaseCopyLayerMask == 3)
				{
					note = Localizer.T("下地ブロック 0x{0:X3} は上層にも絵があります。下地に使われるのは下層と中層の絵だけです（番号の横の見本は、実際に使われる下層と中層だけを表示しています）。");
				}
				else
				{
					note = Localizer.T("下地ブロック 0x{0:X3} は中層か上層にも絵があります。下地に使われるのは下層の絵だけです（番号の横の見本は、実際に使われる下層だけを表示しています）。");
				}
				this.plan.Notes.Add(string.Format(note, id));
			}
		}

		//-------------------------------------------------------------------------------
		// 下地ブロックの見本を描く処理（実際に下地として使われる下層だけを描く）
		//-------------------------------------------------------------------------------
		private void pnlBaseBlockPreview_Paint(object sender, PaintEventArgs e)
		{
			if (this.host == null)
			{
				return;
			}
			Rectangle area = this.pnlBaseBlockPreview.ClientRectangle;
			int size = Math.Min(area.Width, area.Height) - 2;
			Rectangle dest = new Rectangle(area.X + (area.Width - size) / 2, area.Y + (area.Height - size) / 2, size, size);
			ushort[] entries = this.host.GetBlockEntries((int)this.nudBaseBlock.Value);
			Bitmap block = null;
			if (entries != null && this.plan != null && this.planOptions != null && this.plan.Target != null)
			{
				block = TileImportEngine.RenderBlock(entries, this.plan, this.planOptions, this.BaseCopyLayerMask);
			}
			else
			{
				// 計画がまだ無いときは、マップ画面のパレットと同じ絵で代わりに見せる
				block = this.host.GetBlockImage((int)this.nudBaseBlock.Value);
			}
			if (block == null)
			{
				return;
			}
			using (block)
			{
				e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
				e.Graphics.DrawImage(block, dest);
			}
		}
	}
}
