using System;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップエディタの「出現ポケモン」タブに組み込んで使うための処理
	// （マップエディタで選んだマップの出現ポケモンを、検索しなくても自動で表示する）
	//-------------------------------------------------------------------------------
	public partial class WildPokemonEditor
	{
		//-------------------------------------------------------------------------------
		// 別ウィンドウではなく、ほかの画面の中に部品として置けるようにする処理
		//-------------------------------------------------------------------------------
		internal void PrepareEmbedded()
		{
			this.TopLevel = false;
			this.FormBorderStyle = FormBorderStyle.None;
			this.Dock = DockStyle.None;
			this.Location = new System.Drawing.Point(0, 0);
		}

		// ミニアイコンの読み取り（ROM ごとに 1 つ作って使い回す）
		private PokemonIconReader iconReader;

		//-------------------------------------------------------------------------------
		// ミニアイコンの読み取りを用意する処理（別の ROM になっていれば作り直す）
		//-------------------------------------------------------------------------------
		private PokemonIconReader GetIconReader()
		{
			if (this.iconReader == null || this.iconReader.Rom != this.romData)
			{
				this.iconReader = PokemonIconReader.Create(this.romData);
				if (this.iconReader != null && this.nameTable != null)
				{
					this.iconReader.SpeciesLimit = this.nameTable.Count;
				}
			}
			return this.iconReader;
		}

		//-------------------------------------------------------------------------------
		// アイコンの枠を、右隣のプルダウンと同じ行にそろえる処理
		// 枠の高さを行の間隔いっぱい（プルダウン＋上下 7px）にし、縦の中心をプルダウンの中心に合わせる
		//-------------------------------------------------------------------------------
		private void LayoutIconsBesideCombos()
		{
			foreach (AreaControlSet set in this.areaControls)
			{
				for (int i = 0; i < set.Icons.Length && i < set.Combos.Length; i++)
				{
					PictureBox pic = set.Icons[i];
					ComboBox combo = set.Combos[i];
					int size = combo.Height + this.LogicalToDeviceUnits(14);
					pic.SizeMode = PictureBoxSizeMode.Normal;
					pic.Size = new System.Drawing.Size(size, size);
					pic.Location = new System.Drawing.Point(Math.Max(0, combo.Left - size - this.LogicalToDeviceUnits(4)), combo.Top + (combo.Height - size) / 2);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// アイコンの透明な余白を切り落とし、枠に収まる大きさまで拡大した絵を作る処理（ドット絵なので最近傍で拡大）
		//-------------------------------------------------------------------------------
		private static System.Drawing.Bitmap FitIconToBox(System.Drawing.Bitmap icon, System.Drawing.Size box)
		{
			// 透明でない部分を囲む範囲
			int left = icon.Width, top = icon.Height, right = -1, bottom = -1;
			for (int y = 0; y < icon.Height; y++)
			{
				for (int x = 0; x < icon.Width; x++)
				{
					if (icon.GetPixel(x, y).A != 0)
					{
						left = Math.Min(left, x);
						top = Math.Min(top, y);
						right = Math.Max(right, x);
						bottom = Math.Max(bottom, y);
					}
				}
			}
			System.Drawing.Bitmap result = new System.Drawing.Bitmap(Math.Max(1, box.Width), Math.Max(1, box.Height));
			if (right < 0)
			{
				return result;
			}
			int width = right - left + 1;
			int height = bottom - top + 1;
			float scale = Math.Min((float)box.Width / width, (float)box.Height / height);
			// 2 倍以上に広げられるときは、整数倍にしてドットの大きさをそろえる
			if (scale >= 2f)
			{
				scale = (float)Math.Floor(scale);
			}
			int drawWidth = (int)(width * scale);
			int drawHeight = (int)(height * scale);
			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(result))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
				g.DrawImage(icon, new System.Drawing.Rectangle((box.Width - drawWidth) / 2, (box.Height - drawHeight) / 2, drawWidth, drawHeight),
					new System.Drawing.Rectangle(left, top, width, height), System.Drawing.GraphicsUnit.Pixel);
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 保存していない変更があるか
		//-------------------------------------------------------------------------------
		internal bool HasUnsavedEdits
		{
			get { return this.hasUnsavedChanges; }
		}

		//-------------------------------------------------------------------------------
		// 保存していない変更があれば、保存するか確認する処理（キャンセルなら false）
		//-------------------------------------------------------------------------------
		internal bool ConfirmSaveBeforeLeave()
		{
			return this.ConfirmSaveIfNeeded();
		}

		//-------------------------------------------------------------------------------
		// 指定したマップの出現ポケモンを表示する処理（見つかれば最初のデータを開く）
		// 見つかれば true、見つからなければ false（「見つかりません」の画面は出さない）
		// 保存していない変更の確認でキャンセルされたら null を返し、今の表示をそのまま残す
		//-------------------------------------------------------------------------------
		internal bool? ShowMap(int bank, int number)
		{
			if (!this.ConfirmSaveIfNeeded())
			{
				return null;
			}
			this.nudMapBankSearch.Value = Math.Max(this.nudMapBankSearch.Minimum, Math.Min(this.nudMapBankSearch.Maximum, bank));
			this.nudMapNumberSearch.Value = Math.Max(this.nudMapNumberSearch.Minimum, Math.Min(this.nudMapNumberSearch.Maximum, number));
			this.lstResult.Items.Clear();
			this.ResetAllAreaTabs();
			this.previousSelectedIndex = -1;
			int time = this.GetSelectedSearchTimeIndex();
			if (!this.loadedEncounterTables.ContainsKey(time))
			{
				return false;
			}
			this.lstResult.BeginUpdate();
			foreach (WildPokemonEditor.WildEncounterEntry entry in this.loadedEncounterTables[time])
			{
				if (entry.MapBank == bank && entry.MapNumber == number)
				{
					this.lstResult.Items.Add(string.Format("マップ({0}, {1}) [{2}]", entry.MapBank, entry.MapNumber, entry.TableIndex));
				}
			}
			this.lstResult.EndUpdate();
			if (this.lstResult.Items.Count == 0)
			{
				return false;
			}
			this.lstResult.SelectedIndex = 0;
			return true;
		}
	}
}
