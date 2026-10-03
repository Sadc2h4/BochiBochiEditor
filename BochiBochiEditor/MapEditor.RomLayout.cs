using System;
using System.Collections.Generic;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ROM 内のデータ配置から、第2タイルセットの実際のブロック数を推定する処理群
	// 改造でブロック数を増やした ROM では、設定ファイル（Tileset2BlockLimit.ini）の値が実際より少ないため
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// ROM 内の既知のデータ開始位置（昇順）。ROM を読み直したり書き換えたりしたら作り直す
		private List<uint> knownDataStarts;

		// ROM 読込時に一部を読めなかったマップの記録
		private readonly List<string> mapLoadWarnings = new List<string>();
		// 画面を出す前に ROM を読んだとき（起動時の引数・ハーネス）は、案内を画面を出した後まで持ち越す
		private bool mapLoadWarningsPending;

		//-------------------------------------------------------------------------------
		// マップ 1 つ分の見出し・接続・イベント・マップスクリプトを、失敗しても他のマップに影響しないよう読む処理
		// 見出しが読めないマップは一覧に入れない。接続・イベント等だけ読めないマップは「読み込み不完全」として保存を禁止する
		//-------------------------------------------------------------------------------
		private void ReadOneMapHeaderSafely(int bank, int number, uint pointer)
		{
			MapHeader header;
			try
			{
				if (!this.IsRomPointer(pointer))
				{
					this.mapLoadWarnings.Add(string.Format(Localizer.T("({0}, {1}) 見出しの参照先 0x{2:X8} が ROM の範囲外のため読み飛ばしました。"), bank, number, pointer));
					return;
				}
				header = this.ReadMapHeader(bank, number, (int)(pointer - 0x08000000u));
			}
			catch (Exception ex)
			{
				this.mapLoadWarnings.Add(string.Format(Localizer.T("({0}, {1}) 見出しを読めないため読み飛ばしました: {2}"), bank, number, ex.Message));
				return;
			}
			if (header == null)
			{
				return;
			}
			var steps = new (string Name, Action<MapHeader> Read)[]
			{
				(Localizer.T("接続"), this.ReadConnections),
				(Localizer.T("イベント"), this.ReadEvents),
				(Localizer.T("マップスクリプト"), this.ReadMapScripts)
			};
			foreach (var step in steps)
			{
				try
				{
					step.Read(header);
				}
				catch (Exception ex)
				{
					header.LoadIncomplete = true;
					this.mapLoadWarnings.Add(string.Format(Localizer.T("({0}, {1}) {2}を読めませんでした（このマップは保存できません）: {3}"), bank, number, step.Name, ex.Message));
				}
			}
			// 読めなかった部分は空の一覧にして、表示処理が落ちないようにする
			header.Connections = header.Connections ?? new List<ConnectedMap>();
			header.Persons = header.Persons ?? new List<PersonEvent>();
			header.Warps = header.Warps ?? new List<WarpEvent>();
			header.Traps = header.Traps ?? new List<TrapEvent>();
			header.Signs = header.Signs ?? new List<SignEvent>();
			header.MapScripts = header.MapScripts ?? new List<MapScriptEvent>();
			this.mapHeaders.Add(header);
		}

		//-------------------------------------------------------------------------------
		// マップ見出しを指していそうなポインタか判定する処理（フッターの参照先と、マップの大きさが妥当か）
		//-------------------------------------------------------------------------------
		private bool LooksLikeMapHeaderPointer(uint pointer)
		{
			if (!this.IsRomPointer(pointer) || pointer - 0x08000000u + 4 > this.romData.Length)
			{
				return false;
			}
			uint footerPointer = BitConverter.ToUInt32(this.romData, (int)(pointer - 0x08000000u));
			if (!this.IsRomPointer(footerPointer) || footerPointer - 0x08000000u + 8 > this.romData.Length)
			{
				return false;
			}
			int footer = (int)(footerPointer - 0x08000000u);
			uint width = BitConverter.ToUInt32(this.romData, footer);
			uint height = BitConverter.ToUInt32(this.romData, footer + 4);
			return width > 0 && width <= 255 && height > 0 && height <= 255;
		}

		//-------------------------------------------------------------------------------
		// マップ表（バンクごとの見出しポインタ一覧）から実際のマップ数を数え、設定ファイルの値と合わせる処理
		// 数える範囲は「次のバンクの一覧の開始位置」の手前まで。見出しらしくない項目が出たらそこで止める
		//-------------------------------------------------------------------------------
		private Dictionary<int, int> MergeDetectedMapCounts(Dictionary<int, int> fromIni)
		{
			Dictionary<int, int> result = new Dictionary<int, int>(fromIni);
			try
			{
				int table = MapEditor.MAP_BANK_TABLE_OFFSET;
				List<KeyValuePair<int, uint>> banks = new List<KeyValuePair<int, uint>>();
				for (int bank = 0; bank < 256 && table + bank * 4 + 4 <= this.romData.Length; bank++)
				{
					uint pointer = BitConverter.ToUInt32(this.romData, table + bank * 4);
					if (!this.IsRomPointer(pointer))
					{
						break;
					}
					uint start = pointer - 0x08000000u;
					if (start + 4 > this.romData.Length || !this.LooksLikeMapHeaderPointer(BitConverter.ToUInt32(this.romData, (int)start)))
					{
						break;
					}
					banks.Add(new KeyValuePair<int, uint>(bank, start));
				}
				List<uint> boundaries = banks.Select(b => b.Value).Concat(new uint[] { (uint)table, (uint)this.romData.Length }).Distinct().OrderBy(v => v).ToList();
				foreach (KeyValuePair<int, uint> bank in banks)
				{
					uint limit = boundaries.First(v => v > bank.Value);
					int capacity = (int)Math.Min(256, (limit - bank.Value) / 4);
					int detected = 0;
					while (detected < capacity && this.LooksLikeMapHeaderPointer(BitConverter.ToUInt32(this.romData, (int)bank.Value + detected * 4)))
					{
						detected++;
					}
					int ini;
					fromIni.TryGetValue(bank.Key, out ini);
					result[bank.Key] = Math.Min(capacity, Math.Max(ini, detected));
				}
			}
			catch (Exception)
			{
				return new Dictionary<int, int>(fromIni);
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 第1・第2タイルセットのタイル画像を、実機と同じ並び（第2は必ず第1の枚数分の後ろから）で 1 つにつなぐ処理
		// 第1の画像が規定の枚数より少ない・多い ROM でも、第2タイルセットの絵がずれないように第1を規定の枚数にそろえる
		//-------------------------------------------------------------------------------
		private byte[] BuildCombinedTileImage(TilesetHeader primary, TilesetHeader secondary)
		{
			byte[] first = this.LoadTilesetRawImage(primary);
			byte[] second = this.LoadTilesetRawImage(secondary);
			int primaryBytes = GameProfile.Current.PrimaryTileCount * 32;
			byte[] combined = new byte[primaryBytes + second.Length];
			Array.Copy(first, 0, combined, 0, Math.Min(first.Length, primaryBytes));
			Array.Copy(second, 0, combined, primaryBytes, second.Length);
			return combined;
		}

		//-------------------------------------------------------------------------------
		// 一部を読めなかったマップがあれば、件数を案内して詳細を error.log に残す処理
		//-------------------------------------------------------------------------------
		private void ReportMapLoadWarnings()
		{
			if (this.mapLoadWarnings.Count == 0)
			{
				return;
			}
			// 画面がまだ見えていないときは、記録だけ残して案内は画面を出した後にする（見えない画面の後ろでダイアログが止まらないように）
			if (!this.Visible)
			{
				if (!this.mapLoadWarningsPending)
				{
					this.WriteErrorLog(Localizer.T("ROM 読み込み（一部のマップ）"), new InvalidOperationException(string.Join(Environment.NewLine, this.mapLoadWarnings)));
				}
				this.mapLoadWarningsPending = true;
				return;
			}
			this.mapLoadWarningsPending = false;
			string log = this.WriteErrorLog(Localizer.T("ROM 読み込み（一部のマップ）"), new InvalidOperationException(string.Join(Environment.NewLine, this.mapLoadWarnings)));
			string preview = string.Join("\n", this.mapLoadWarnings.Take(8));
			string more = this.mapLoadWarnings.Count > 8 ? string.Format(Localizer.T("\n…ほか {0} 件"), this.mapLoadWarnings.Count - 8) : string.Empty;
			System.Windows.Forms.MessageBox.Show(this,
				string.Format(Localizer.T("ROM は開けましたが、{0} 件のマップで一部のデータを読めませんでした。\n該当マップは表示できますが、保存はできません。\n\n{1}{2}{3}"), this.mapLoadWarnings.Count, preview, more, log != null ? Localizer.T("\n\n詳細: ") + log : string.Empty),
				Localizer.T("一部のマップを読めませんでした"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
		}

		//-------------------------------------------------------------------------------
		// データ配置の記録を破棄する処理（ROM 読込時・タイルセット書き換え後に呼ぶ）
		//-------------------------------------------------------------------------------
		private void InvalidateRomLayoutCache()
		{
			this.knownDataStarts = null;
		}

		//-------------------------------------------------------------------------------
		// 既知のデータ開始位置（タイルセットの各表・画像・パレット、マップのフッター・データ等）を集める処理
		//-------------------------------------------------------------------------------
		private List<uint> GetKnownDataStarts()
		{
			if (this.knownDataStarts != null)
			{
				return this.knownDataStarts;
			}
			HashSet<uint> starts = new HashSet<uint>();
			// タイルセット表（番号順）を、見出しとして読めなくなるまで調べる
			for (int index = 0; index < 256; index++)
			{
				int header = MapEditor.TILESET_INDEX_START_OFFSET + index * MapEditor.TILESET_HEADER_SIZE;
				if (header + 24 > this.romData.Length || this.romData[header + 1] > 1 || !this.IsRomPointer(BitConverter.ToUInt32(this.romData, header + 4)))
				{
					break;
				}
				this.AddTilesetDataStarts(starts, (uint)header);
			}
			// マップから参照されるデータ（別の場所へ移されたタイルセットも含む）
			foreach (MapHeader header in this.mapHeaders)
			{
				if (header.FooterAddress == 0)
				{
					continue;
				}
				starts.Add(header.FooterAddress);
				MapFooter footer = this.TryReadFooter(header);
				if (footer == null)
				{
					continue;
				}
				foreach (uint address in new uint[] { footer.MapDataAddress, footer.BorderDataAddress })
				{
					if (address != 0)
					{
						starts.Add(address);
					}
				}
				foreach (uint tileset in new uint[] { footer.Tileset1Address, footer.Tileset2Address })
				{
					if (tileset != 0 && tileset + 24 <= this.romData.Length)
					{
						this.AddTilesetDataStarts(starts, tileset);
					}
				}
			}
			this.knownDataStarts = starts.OrderBy(a => a).ToList();
			return this.knownDataStarts;
		}

		//-------------------------------------------------------------------------------
		// タイルセット見出しが指す各データ（画像・パレット・ブロック表・挙動表）の位置を登録する処理
		//-------------------------------------------------------------------------------
		private void AddTilesetDataStarts(HashSet<uint> starts, uint header)
		{
			starts.Add(header);
			foreach (int field in new int[] { 4, 8, 12, GameProfile.Current.TilesetBehaviorOffset })
			{
				uint pointer = BitConverter.ToUInt32(this.romData, (int)header + field);
				if (this.IsRomPointer(pointer))
				{
					starts.Add(pointer - 0x08000000u);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 指定位置より後ろで最も近い既知データの開始位置を返す処理（無ければ ROM の末尾）
		//-------------------------------------------------------------------------------
		private uint FindNextDataStart(uint address)
		{
			List<uint> starts = this.GetKnownDataStarts();
			int lo = 0;
			int hi = starts.Count;
			while (lo < hi)
			{
				int mid = (lo + hi) / 2;
				if (starts[mid] <= address)
				{
					lo = mid + 1;
				}
				else
				{
					hi = mid;
				}
			}
			return lo < starts.Count ? starts[lo] : (uint)this.romData.Length;
		}

		//-------------------------------------------------------------------------------
		// タイルセットのデータの、指定位置より後ろで最も近いデータの開始位置を返す処理
		// 既知のデータに加えて、そのタイルセット自身の画像・パレット・ブロック表・挙動表の位置も区切りに使う
		// （どのマップからも使われていない、作ったばかりのタイルセットでも正しく数えるため）
		//-------------------------------------------------------------------------------
		private uint FindNextTilesetDataStart(TilesetHeader tileset, uint address)
		{
			uint next = this.FindNextDataStart(address);
			foreach (uint own in new uint[] { tileset.ImageAddress, tileset.PaletteAddress, tileset.BlockImageAddress, tileset.BlockBehaviorAddress })
			{
				if (own > address && own < next)
				{
					next = own;
				}
			}
			return next;
		}

		//-------------------------------------------------------------------------------
		// 第1タイルセットで実際に使われているブロック数を返す処理
		// 第1のブロック番号の枠はゲームごとに固定（FR 640、エメラルド 512）だが、屋内用などはそれより少ないブロックしか持たない
		// ブロック表・挙動表の後ろにある次のデータまでの長さから求め、求められなければ枠の数を返す
		//-------------------------------------------------------------------------------
		private int GetPrimaryBlockCount(TilesetHeader tileset)
		{
			int max = GameProfile.Current.PrimaryBlockCount;
			// 拡張した形式は、容量の表にブロック数が書いてある
			int listed = CapacityBlockCount(tileset);
			if (listed > 0)
			{
				return Math.Min(max, listed);
			}
			if (this.romData == null || tileset == null || tileset.BlockImageAddress == 0 || tileset.BlockImageAddress >= this.romData.Length)
			{
				return max;
			}
			try
			{
				int byBlocks = (int)((this.FindNextTilesetDataStart(tileset, tileset.BlockImageAddress) - tileset.BlockImageAddress) / (uint)MapEditor.BLOCK_DATA_SIZE);
				int byBehavior = max;
				if (tileset.BlockBehaviorAddress != 0 && tileset.BlockBehaviorAddress < this.romData.Length)
				{
					byBehavior = (int)((this.FindNextTilesetDataStart(tileset, tileset.BlockBehaviorAddress) - tileset.BlockBehaviorAddress) / (uint)GameProfile.Current.BehaviorBytes);
				}
				int estimate = Math.Min(max, Math.Min(byBlocks, byBehavior));
				return estimate > 0 ? estimate : max;
			}
			catch (Exception)
			{
				return max;
			}
		}

		//-------------------------------------------------------------------------------
		// 第2タイルセットのブロック数を返す処理
		// ブロック表・挙動表の後ろにある次のデータまでの長さ、0xFF（空き領域）で埋まったブロック、上限 384 のうち最小を使う
		// 推定できない場合は設定ファイルの値（無ければ 384）を使う
		//-------------------------------------------------------------------------------
		private int GetSecondaryBlockCount(int tilesetIndex, TilesetHeader tileset)
		{
			int max = MapEditor.BlockIdCapacity - GameProfile.Current.PrimaryBlockCount;
			// 拡張した形式は、容量の表にブロック数が書いてある
			int listed = CapacityBlockCount(tileset);
			if (listed > 0)
			{
				return Math.Min(max, listed);
			}
			int fallback;
			if (!GameProfile.Current.UseTileset2BlockLimitIni || !this.tileset2BlockLimits.TryGetValue(tilesetIndex, out fallback))
			{
				fallback = max;
			}
			if (this.romData == null || tileset == null || tileset.BlockImageAddress == 0 || tileset.BlockImageAddress >= this.romData.Length)
			{
				return Math.Min(fallback, max);
			}
			try
			{
				int blockSize = MapEditor.BLOCK_DATA_SIZE;
				int byBlocks = (int)((this.FindNextTilesetDataStart(tileset, tileset.BlockImageAddress) - tileset.BlockImageAddress) / (uint)blockSize);
				int byBehavior = max;
				if (tileset.BlockBehaviorAddress != 0 && tileset.BlockBehaviorAddress < this.romData.Length)
				{
					byBehavior = (int)((this.FindNextTilesetDataStart(tileset, tileset.BlockBehaviorAddress) - tileset.BlockBehaviorAddress) / (uint)GameProfile.Current.BehaviorBytes);
				}
				int byFreeSpace = max;
				for (int i = 0; i < max; i++)
				{
					long offset = (long)tileset.BlockImageAddress + i * blockSize;
					if (offset + blockSize > this.romData.Length)
					{
						byFreeSpace = i;
						break;
					}
					bool allFF = true;
					for (int b = 0; b < blockSize && allFF; b++)
					{
						allFF = this.romData[offset + b] == 0xFF;
					}
					if (allFF)
					{
						byFreeSpace = i;
						break;
					}
				}
				int estimate = Math.Min(max, Math.Min(byBlocks, Math.Min(byBehavior, byFreeSpace)));
				return estimate > 0 ? estimate : Math.Min(fallback, max);
			}
			catch (Exception)
			{
				return Math.Min(fallback, max);
			}
		}
	}
}
