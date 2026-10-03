using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップ（とボーダー）の大きさを安全に変える処理
	// ・新しい並びは空き領域に作る（元の並びの場所は使い回さない。元のデータは「確定」するまで、そのまま残る）
	// ・今の内容は左上をそろえて残し、広がった部分は指定したブロックで埋める。左・上に足す（中身をずらす）こともできる
	// ・中身をずらしたときは、このマップのイベントの座標と接続のずれ、相手のマップの接続のずれ（確定のときに書く）も直す
	// ・地形データ（フッター）の幅・高さ・並びの場所は、編集中の値だけを変える。「編集中のMAPを確定」で ROM に書く
	// ・確定せずに別のマップへ移る・元に戻すなどで変更を捨てたときは、確保した領域を空き（0xFF）へ戻す
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// ゲームがマップを読み込むときの作業領域の大きさ（(幅 + 15) × (高さ + 14) がこれを超えると読み込めない）
		private const int MapLoadBufferCells = 10240;
		// 埋めるブロックが今のマップに無いときの移動エリアの値（通れる・高さ 3）
		private const int ResizeDefaultCollision = 0x0C;

		// 大きさの変更で確保して、まだ確定していない領域（場所と長さ）と、そのときの ROM
		private readonly List<KeyValuePair<int, int>> pendingResizeBlocks = new List<KeyValuePair<int, int>>();
		private byte[] pendingResizeRom;
		// 中身をずらしたときに直す、相手のマップの接続のずれ（ROM の場所 → 新しい値）。確定のときに書く
		private readonly Dictionary<int, int> pendingResizeCounterparts = new Dictionary<int, int>();

		//-------------------------------------------------------------------------------
		// 「大きさを変える…」: 新しい大きさを決めてもらい、並びを作り直す処理
		//-------------------------------------------------------------------------------
		private void btnResizeMap_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.romData == null || this.tempHeader == null || this.tempFooter == null || this.mapMatrix == null)
			{
				return;
			}
			bool borderEditable = GameProfile.Current.HasBorderSize && this.borderMatrix != null;
			using (MapResizeForm form = new MapResizeForm(this.tempFooter.MapWidth, this.tempFooter.MapHeight, Math.Max(1, (int)this.tempFooter.BorderWidth), Math.Max(1, (int)this.tempFooter.BorderHeight),
				borderEditable, this.GetMostUsedBlock(), this.totalBlocks, this.CreateBlockPreview, this.DescribeResize))
			{
				AppIconHelper.Apply(form);
				UiTheme.Apply(form);
				if (form.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				int oldWidth = this.tempFooter.MapWidth;
				int oldHeight = this.tempFooter.MapHeight;
				string error = this.ResizeCurrentMap(form.NewWidth, form.NewHeight, borderEditable ? form.NewBorderWidth : this.tempFooter.BorderWidth, borderEditable ? form.NewBorderHeight : this.tempFooter.BorderHeight, form.FillBlock, form.ShiftX, form.ShiftY);
				if (error != null)
				{
					if (error.Length > 0)
					{
						MessageBox.Show(this, error, Localizer.T("マップの大きさを変える"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					return;
				}
				string shifted = form.ShiftX != 0 || form.ShiftY != 0
					? string.Format(Localizer.T("{0}中身を右へ {1} マス・下へ {2} マスずらし、イベントの座標と接続のずれ（相手のマップ {3} 件も確定のときに）を直しました。"), Environment.NewLine, form.ShiftX, form.ShiftY, this.pendingResizeCounterparts.Count)
					: string.Empty;
				MessageBox.Show(this, string.Format(Localizer.T("マップの大きさを 幅 {0} × 高さ {1} から 幅 {2} × 高さ {3} に変えました。{4}新しい並びは 0x{5:X6} に置きました。{6}{4}「編集中のMAPを確定」を押すと ROM に反映されます（押さずに別のマップへ移ると、元の大きさのままです）。"),
					oldWidth, oldHeight, this.tempFooter.MapWidth, this.tempFooter.MapHeight, Environment.NewLine, this.tempFooter.MapDataAddress, shifted), Localizer.T("マップの大きさを変える"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		//-------------------------------------------------------------------------------
		// 今のマップでいちばん多く使っているブロックの番号を返す処理（広がった部分を埋めるブロックの初めの値）
		//-------------------------------------------------------------------------------
		private int GetMostUsedBlock()
		{
			if (this.mapMatrix == null)
			{
				return 0;
			}
			Dictionary<int, int> counts = new Dictionary<int, int>();
			foreach (MapEditor.MapCell cell in this.mapMatrix)
			{
				int count;
				counts.TryGetValue(cell.BlockIndex, out count);
				counts[cell.BlockIndex] = count + 1;
			}
			return counts.Count == 0 ? 0 : counts.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;
		}

		//-------------------------------------------------------------------------------
		// ブロック 1 個の絵（16×16）を作る処理（ブロック一覧の絵から切り出す。無ければ null）
		//-------------------------------------------------------------------------------
		private Bitmap CreateBlockPreview(int blockId)
		{
			if (this.blockPaletteBitmap == null || blockId < 0)
			{
				return null;
			}
			Rectangle source = new Rectangle(blockId % PaletteColumns * 16, blockId / PaletteColumns * 16, 16, 16);
			if (source.Right > this.blockPaletteBitmap.Width || source.Bottom > this.blockPaletteBitmap.Height)
			{
				return null;
			}
			Bitmap preview = new Bitmap(16, 16);
			using (Graphics g = Graphics.FromImage(preview))
			{
				g.DrawImage(this.blockPaletteBitmap, new Rectangle(0, 0, 16, 16), source, GraphicsUnit.Pixel);
			}
			return preview;
		}

		//-------------------------------------------------------------------------------
		// 新しい大きさについての説明（必要な空き領域・注意）を作る処理。allowed は実行してよいか
		// shiftX・shiftY は中身をずらすマス数（右・下が正。左側に足す列数・上側に足す行数）
		//-------------------------------------------------------------------------------
		private string DescribeResize(int width, int height, int borderWidth, int borderHeight, int shiftX, int shiftY, out bool allowed)
		{
			allowed = false;
			if (this.tempFooter == null || this.mapMatrix == null)
			{
				return string.Empty;
			}
			string problem = this.CheckResizeValues(width, height, borderWidth, borderHeight, shiftX, shiftY);
			if (problem != null)
			{
				return "⚠ " + problem;
			}
			bool mapChanged = width != this.tempFooter.MapWidth || height != this.tempFooter.MapHeight || shiftX != 0 || shiftY != 0;
			bool borderChanged = this.IsBorderResize(borderWidth, borderHeight);
			if (!mapChanged && !borderChanged)
			{
				return Localizer.T("大きさが今と同じです。");
			}
			allowed = true;
			List<string> lines = new List<string>();
			lines.Add(string.Format(Localizer.T("必要な空き領域: {0} バイト（マップの並び {1}、ボーダー {2}）"), this.GetResizeAllocationLength(width, height, borderWidth, borderHeight, shiftX, shiftY),
				mapChanged ? width * height * 2 : 0, borderChanged ? borderWidth * borderHeight * 2 : 0));
			Func<int, int, bool> outside = (x, y) => x + shiftX < 0 || y + shiftY < 0 || x + shiftX >= width || y + shiftY >= height;
			int persons = this.tempHeader.Persons != null ? this.tempHeader.Persons.Count(p => outside(p.X, p.Y)) : 0;
			int warps = this.tempHeader.Warps != null ? this.tempHeader.Warps.Count(p => outside(p.X, p.Y)) : 0;
			int traps = this.tempHeader.Traps != null ? this.tempHeader.Traps.Count(p => outside(p.X, p.Y)) : 0;
			int signs = this.tempHeader.Signs != null ? this.tempHeader.Signs.Count(p => outside(p.X, p.Y)) : 0;
			if (persons + warps + traps + signs > 0)
			{
				lines.Add(string.Format(Localizer.T("⚠ 新しい大きさの外に残るイベントが {0} 個あります（人物 {1}・ワープ {2}・踏むスクリプト {3}・看板 {4}）。消えませんが、位置を直すか消してください（左・上にはみ出すものは 0 に寄せます）。"),
					persons + warps + traps + signs, persons, warps, traps, signs));
			}
			if (shiftX != 0 || shiftY != 0)
			{
				int own = this.tempHeader.Connections != null ? this.tempHeader.Connections.Count(c => (c.Direction == 1 || c.Direction == 2) ? shiftX != 0 : ((c.Direction == 3 || c.Direction == 4) && shiftY != 0)) : 0;
				int counterparts = this.FindResizeCounterparts(shiftX, shiftY).Count;
				lines.Add(string.Format(Localizer.T("中身を右へ {0} マス・下へ {1} マスずらします。イベントの座標と、このマップの接続のずれ {2} 件・相手のマップの接続のずれ {3} 件を直します（相手の分は確定のときに書きます）。"), shiftX, shiftY, own, counterparts));
			}
			int shared = this.GetMapsSharingTerrainId(this.tempHeader.TerrainId).Count;
			if (shared > 0)
			{
				lines.Add(string.Format(Localizer.T("ℹ 同じ形を使い回しているマップがほかに {0} 個あり、確定するとそれらも同じ大きさになります。"), shared)
					+ (shiftX != 0 || shiftY != 0 ? Localizer.T("それらのイベント・接続は直しません。") : string.Empty));
			}
			return string.Join(Environment.NewLine, lines);
		}

		//-------------------------------------------------------------------------------
		// 中身をずらしたときに直す、相手のマップの接続（相手の見出しの接続の表にある、このマップへの項目）を集める処理
		// 返すのは「ずれの値の ROM の場所 → 新しい値」。上下の接続は横のずれ、左右の接続は縦のずれを引く（相手から見るとこのマップが左・上へ動くため）
		//-------------------------------------------------------------------------------
		private Dictionary<int, int> FindResizeCounterparts(int shiftX, int shiftY)
		{
			Dictionary<int, int> result = new Dictionary<int, int>();
			if (this.tempHeader == null || this.tempHeader.Connections == null || this.mapHeaders == null)
			{
				return result;
			}
			foreach (MapEditor.ConnectedMap connection in this.tempHeader.Connections)
			{
				if (connection.Direction < 1 || connection.Direction > 4)
				{
					continue;
				}
				int delta = connection.Direction <= 2 ? shiftX : shiftY;
				if (delta == 0 || (connection.Bank == this.tempHeader.Bank && connection.Number == this.tempHeader.Number))
				{
					continue;
				}
				byte opposite = (byte)(connection.Direction == 1 ? 2 : (connection.Direction == 2 ? 1 : (connection.Direction == 3 ? 4 : 3)));
				MapEditor.MapHeader other = this.mapHeaders.FirstOrDefault(h => h.Bank == connection.Bank && h.Number == connection.Number);
				if (other == null || other.ConnectionAddress == 0U || !this.IsRomRange(other.ConnectionAddress, 8))
				{
					continue;
				}
				int count = this.romData[(int)other.ConnectionAddress];
				int entries = (int)this.PointerToOffset(BitConverter.ToUInt32(this.romData, (int)other.ConnectionAddress + 4));
				if (entries == 0 || !this.IsRomRange((uint)entries, count * 12))
				{
					continue;
				}
				for (int i = 0; i < count; i++)
				{
					int entry = entries + i * 12;
					if (this.romData[entry] == opposite && this.romData[entry + 8] == this.tempHeader.Bank && this.romData[entry + 9] == this.tempHeader.Number)
					{
						result[entry + 4] = BitConverter.ToInt32(this.romData, entry + 4) - delta;
					}
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 新しい大きさが使える値かを確かめる処理（問題が無ければ null、あれば理由）
		//-------------------------------------------------------------------------------
		private string CheckResizeValues(int width, int height, int borderWidth, int borderHeight, int shiftX, int shiftY)
		{
			if (width < 1 || width > 255 || height < 1 || height > 255)
			{
				return Localizer.T("幅と高さは 1〜255 の範囲で指定してください。");
			}
			if (shiftX < -255 || shiftX > 255 || shiftY < -255 || shiftY > 255)
			{
				return Localizer.T("ずらすマス数は -255〜255 の範囲で指定してください。");
			}
			if ((width + 15) * (height + 14) > MapLoadBufferCells)
			{
				return string.Format(Localizer.T("ゲームが読み込める大きさを超えています（(幅 + 15) × (高さ + 14) が {0} 以下である必要があります。今の指定は {1}）。"), MapLoadBufferCells, (width + 15) * (height + 14));
			}
			if (GameProfile.Current.HasBorderSize && (borderWidth < 1 || borderWidth > 255 || borderHeight < 1 || borderHeight > 255))
			{
				return Localizer.T("ボーダーの幅と高さは 1〜255 の範囲で指定してください。");
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// ボーダーの大きさが変わるかを返す処理（ボーダーの大きさを持たないゲームでは変えない）
		//-------------------------------------------------------------------------------
		private bool IsBorderResize(int borderWidth, int borderHeight)
		{
			return GameProfile.Current.HasBorderSize && this.borderMatrix != null
				&& (borderWidth != this.tempFooter.BorderWidth || borderHeight != this.tempFooter.BorderHeight);
		}

		//-------------------------------------------------------------------------------
		// 新しく確保する領域の長さを返す処理（マップの並び + ボーダー。どちらも 4 バイト境界にそろえる）
		//-------------------------------------------------------------------------------
		private int GetResizeAllocationLength(int width, int height, int borderWidth, int borderHeight, int shiftX, int shiftY)
		{
			int length = 0;
			if (width != this.tempFooter.MapWidth || height != this.tempFooter.MapHeight || shiftX != 0 || shiftY != 0)
			{
				length += (width * height * 2 + 3) & ~3;
			}
			if (this.IsBorderResize(borderWidth, borderHeight))
			{
				length += (borderWidth * borderHeight * 2 + 3) & ~3;
			}
			return length;
		}

		//-------------------------------------------------------------------------------
		// 今のマップ（とボーダー）の大きさを変える処理（中身はずらさない。ハーネスと古い呼び出し向け）
		//-------------------------------------------------------------------------------
		internal string ResizeCurrentMap(int width, int height, int borderWidth, int borderHeight, int fillBlock)
		{
			return this.ResizeCurrentMap(width, height, borderWidth, borderHeight, fillBlock, 0, 0);
		}

		//-------------------------------------------------------------------------------
		// 今のマップ（とボーダー）の大きさを変える処理（成功なら null。失敗なら理由。書き込み先の選択をやめたときは空文字）
		// shiftX・shiftY は中身をずらすマス数（右・下が正）。ずらしたときは、イベントの座標と接続のずれも直す
		// 失敗・取り消しのときは、ROM も編集中の内容も変えない
		//-------------------------------------------------------------------------------
		internal string ResizeCurrentMap(int width, int height, int borderWidth, int borderHeight, int fillBlock, int shiftX, int shiftY)
		{
			if (this.romData == null || this.tempHeader == null || this.tempFooter == null || this.mapMatrix == null)
			{
				return Localizer.T("マップが読み込まれていません。");
			}
			string problem = this.CheckResizeValues(width, height, borderWidth, borderHeight, shiftX, shiftY);
			if (problem != null)
			{
				return problem;
			}
			if (fillBlock < 0 || fillBlock > 1023)
			{
				return Localizer.T("埋めるブロックの番号は 0〜3FF の範囲で指定してください。");
			}
			bool mapChanged = width != this.tempFooter.MapWidth || height != this.tempFooter.MapHeight || shiftX != 0 || shiftY != 0;
			bool borderChanged = this.IsBorderResize(borderWidth, borderHeight);
			if (!mapChanged && !borderChanged)
			{
				return Localizer.T("大きさが今と同じです。");
			}
			// 相手のマップの接続のずれは、ROM を書き換える前に（今の値から）求めておく
			Dictionary<int, int> counterparts = shiftX != 0 || shiftY != 0 ? this.FindResizeCounterparts(shiftX, shiftY) : new Dictionary<int, int>();

			// 新しい並びを作る（今の内容はずらした位置に写し、広がった部分は埋めるブロック）
			int oldWidth = this.mapMatrix.GetLength(0);
			int oldHeight = this.mapMatrix.GetLength(1);
			MapEditor.MapCell[,] newMap = this.mapMatrix;
			if (mapChanged)
			{
				int fillCollision = this.GetCollisionForFillBlock(fillBlock);
				newMap = new MapEditor.MapCell[width, height];
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						int sourceX = x - shiftX;
						int sourceY = y - shiftY;
						if (sourceX >= 0 && sourceY >= 0 && sourceX < oldWidth && sourceY < oldHeight)
						{
							newMap[x, y] = this.mapMatrix[sourceX, sourceY];
						}
						else
						{
							newMap[x, y].BlockIndex = fillBlock;
							newMap[x, y].Collision = fillCollision;
						}
					}
				}
			}
			int[,] newBorder = this.borderMatrix;
			if (borderChanged)
			{
				// ボーダーは今の模様をくり返して広げる
				int oldBorderWidth = this.borderMatrix.GetLength(0);
				int oldBorderHeight = this.borderMatrix.GetLength(1);
				newBorder = new int[borderWidth, borderHeight];
				for (int y = 0; y < borderHeight; y++)
				{
					for (int x = 0; x < borderWidth; x++)
					{
						newBorder[x, y] = this.borderMatrix[x % oldBorderWidth, y % oldBorderHeight];
					}
				}
			}

			// 前の（確定していない）大きさの変更で確保した領域があれば、空きへ戻してから確保し直す
			this.ReleasePendingResize();

			// 書き込み先を 1 回だけ選び、その中にマップの並び・ボーダーの順に置く
			int mapBytes = mapChanged ? width * height * 2 : 0;
			int borderBytes = borderChanged ? borderWidth * borderHeight * 2 : 0;
			int total = ((mapBytes + 3) & ~3) + ((borderBytes + 3) & ~3);
			uint picked;
			if (!this.PickFreeSpaceAddress(string.Format(Localizer.T("マップの並び（幅 {0} × 高さ {1}）"), width, height), total, this, out picked))
			{
				return string.Empty;
			}
			if (!this.IsRomRange(picked, total))
			{
				return string.Format(Localizer.T("書き込む範囲（0x{0:X6} から {1} バイト）が ROM の外にはみ出します。"), picked, total);
			}
			for (int i = 0; i < total; i++)
			{
				if (this.romData[picked + i] != 0xFF)
				{
					return string.Format(Localizer.T("書き込み先 0x{0:X6} から {1} バイトの範囲に、空き（0xFF）でないデータがあります。ROM は変更していません。"), picked, total);
				}
			}
			int mapAddress = (int)picked;
			int borderAddress = mapAddress + ((mapBytes + 3) & ~3);

			// 確保した場所へ新しい並びを書く（ほかの新規データと重ならないように、この時点で場所を埋める）
			this.pendingResizeRom = this.romData;
			if (mapChanged)
			{
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						ushort value = MapEditor.MakeCell(newMap[x, y].BlockIndex, newMap[x, y].Collision);
						int at = mapAddress + (y * width + x) * 2;
						this.romData[at] = (byte)(value & 0xFF);
						this.romData[at + 1] = (byte)(value >> 8);
					}
				}
				this.pendingResizeBlocks.Add(new KeyValuePair<int, int>(mapAddress, mapBytes));
			}
			if (borderChanged)
			{
				for (int y = 0; y < borderHeight; y++)
				{
					for (int x = 0; x < borderWidth; x++)
					{
						ushort value = (ushort)(newBorder[x, y] & (MapEditor.BlockIdCapacity - 1));
						int at = borderAddress + (y * borderWidth + x) * 2;
						this.romData[at] = (byte)(value & 0xFF);
						this.romData[at + 1] = (byte)(value >> 8);
					}
				}
				this.pendingResizeBlocks.Add(new KeyValuePair<int, int>(borderAddress, borderBytes));
			}
			this.AfterNewDataWritten();

			// 編集中の地形データを新しい大きさ・場所にする（ROM の地形データは「確定」で書く）
			bool updating = this.isUpdatingUI;
			this.isUpdatingUI = true;
			try
			{
				if (mapChanged)
				{
					this.tempFooter.MapWidth = (byte)width;
					this.tempFooter.MapHeight = (byte)height;
					this.tempFooter.MapDataAddress = (uint)mapAddress;
					this.mapMatrix = newMap;
					this.nudMapWidth.Value = width;
					this.nudMapHeight.Value = height;
					this.txtMapDataAddress.Text = string.Format("{0:X8}", this.tempFooter.MapDataAddress);
					if (shiftX != 0 || shiftY != 0)
					{
						this.ShiftEventsAndConnections(shiftX, shiftY);
						foreach (KeyValuePair<int, int> counterpart in counterparts)
						{
							this.pendingResizeCounterparts[counterpart.Key] = counterpart.Value;
						}
					}
				}
				if (borderChanged)
				{
					this.tempFooter.BorderWidth = (byte)borderWidth;
					this.tempFooter.BorderHeight = (byte)borderHeight;
					this.tempFooter.BorderDataAddress = (uint)borderAddress;
					this.borderMatrix = newBorder;
					this.nudBorderWidth.Value = borderWidth;
					this.nudBorderHeight.Value = borderHeight;
					this.txtBorderDataAddress.Text = string.Format("{0:X8}", this.tempFooter.BorderDataAddress);
				}
				this.ClearMapEditHistory();
			}
			finally
			{
				this.isUpdatingUI = updating;
			}
			this.SetUnsavedChanges(true);
			this.RefreshEditorView(MapEditor.ViewUpdateLevel.GraphicsOnly);
			if (shiftX != 0 || shiftY != 0)
			{
				// イベントの位置と接続の欄を今の値にする
				this.RefreshEditorView(MapEditor.ViewUpdateLevel.HeaderOnly);
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// 編集中のイベントの座標と、このマップの接続のずれを、ずらしたマス数ぶん直す処理
		// 左・上にはみ出す座標は 0 に寄せる。上下の接続は横のずれ、左右の接続は縦のずれを足す（確定のときに ROM へ書かれる）
		//-------------------------------------------------------------------------------
		private void ShiftEventsAndConnections(int shiftX, int shiftY)
		{
			Func<ushort, int, ushort> moved = (value, delta) => (ushort)Math.Max(0, Math.Min(65535, value + delta));
			if (this.tempHeader.Persons != null)
			{
				foreach (MapEditor.PersonEvent person in this.tempHeader.Persons)
				{
					person.X = moved(person.X, shiftX);
					person.Y = moved(person.Y, shiftY);
				}
			}
			if (this.tempHeader.Warps != null)
			{
				foreach (MapEditor.WarpEvent warp in this.tempHeader.Warps)
				{
					warp.X = moved(warp.X, shiftX);
					warp.Y = moved(warp.Y, shiftY);
				}
			}
			if (this.tempHeader.Traps != null)
			{
				foreach (MapEditor.TrapEvent trap in this.tempHeader.Traps)
				{
					trap.X = moved(trap.X, shiftX);
					trap.Y = moved(trap.Y, shiftY);
				}
			}
			if (this.tempHeader.Signs != null)
			{
				foreach (MapEditor.SignEvent sign in this.tempHeader.Signs)
				{
					sign.X = moved(sign.X, shiftX);
					sign.Y = moved(sign.Y, shiftY);
				}
			}
			if (this.tempHeader.Connections != null)
			{
				foreach (MapEditor.ConnectedMap connection in this.tempHeader.Connections)
				{
					if (connection.Direction == 1 || connection.Direction == 2)
					{
						connection.Shift += shiftX;
					}
					else if (connection.Direction == 3 || connection.Direction == 4)
					{
						connection.Shift += shiftY;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 埋めるブロックの移動エリアの値を決める処理
		// 同じブロックが今のマップにあれば、そのブロックでいちばん多い値。無ければ「通れる・高さ 3」
		//-------------------------------------------------------------------------------
		private int GetCollisionForFillBlock(int fillBlock)
		{
			Dictionary<int, int> counts = new Dictionary<int, int>();
			foreach (MapEditor.MapCell cell in this.mapMatrix)
			{
				if (cell.BlockIndex != fillBlock)
				{
					continue;
				}
				int count;
				counts.TryGetValue(cell.Collision, out count);
				counts[cell.Collision] = count + 1;
			}
			return counts.Count == 0 ? ResizeDefaultCollision : counts.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;
		}

		//-------------------------------------------------------------------------------
		// 大きさの変更で確保した領域を、空き（0xFF）へ戻す処理
		// （確定せずに変更を捨てたとき・もう一度大きさを変えるときに呼ぶ。別の ROM を開いた後は、何も書かずに記録だけ捨てる）
		//-------------------------------------------------------------------------------
		private void ReleasePendingResize()
		{
			if (this.pendingResizeBlocks.Count == 0)
			{
				return;
			}
			if (this.pendingResizeRom != null && this.pendingResizeRom == this.romData)
			{
				foreach (KeyValuePair<int, int> block in this.pendingResizeBlocks)
				{
					for (int i = 0; i < block.Value && block.Key + i < this.romData.Length; i++)
					{
						this.romData[block.Key + i] = 0xFF;
					}
				}
				this.AfterNewDataWritten();
			}
			this.pendingResizeBlocks.Clear();
			this.pendingResizeCounterparts.Clear();
			this.pendingResizeRom = null;
		}

		//-------------------------------------------------------------------------------
		// マップを確定したので、大きさの変更で確保した領域を使用中として確定し、相手のマップの接続のずれを ROM に書く処理
		//-------------------------------------------------------------------------------
		private void CommitPendingResize()
		{
			if (this.pendingResizeCounterparts.Count > 0 && this.pendingResizeRom != null && this.pendingResizeRom == this.romData)
			{
				foreach (KeyValuePair<int, int> counterpart in this.pendingResizeCounterparts)
				{
					if (this.IsRomRange((uint)counterpart.Key, 4))
					{
						Array.Copy(BitConverter.GetBytes(counterpart.Value), 0, this.romData, counterpart.Key, 4);
					}
				}
			}
			this.pendingResizeBlocks.Clear();
			this.pendingResizeCounterparts.Clear();
			this.pendingResizeRom = null;
		}
	}
}
