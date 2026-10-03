using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップを読み込んだときの安全点検（マップスクリプトの終端・接続先のタイルセット）の処理群
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 点検で見つかった注意点 1 件分
		private sealed class MapSafetyIssue
		{
			public string Message;
			// 修復できるマップスクリプト一覧（種類 02／04）の場合に設定する
			public int ScriptHeaderIndex = -1;
			public uint ListPointer;
			public int ValidEntryCount;
			// 接続に関する注意の場合の方向（1:下 2:上 3:左 4:右、それ以外は 0）
			public byte Direction;
		}

		// マップスクリプト一覧 1 つ分の最大件数（これを超えたら終端が無いものとみなす）
		private const int MapScriptListEntryLimit = 64;

		private readonly List<MapSafetyIssue> mapSafetyIssues = new List<MapSafetyIssue>();

		// 終端が見つからず、保存時に書き戻してはいけない一覧のアドレス
		private readonly HashSet<uint> suspiciousMapScriptLists = new HashSet<uint>();

		// 画面の初期化（InitializeComponent 中の各種イベント）が終わるまでは点検しない
		private bool isMapSafetyReady;

		//-------------------------------------------------------------------------------
		// 安全点検の表示部品にイベントを設定する処理
		//-------------------------------------------------------------------------------
		private void InitializeMapSafety()
		{
			this.lblStatusCheck.Click += this.lblStatusCheck_Click;
			this.isMapSafetyReady = true;
			this.UpdateSafetyStatus();
		}

		//-------------------------------------------------------------------------------
		// 現在のマップを点検し、結果をステータスバーと接続ボタンへ反映する処理
		//-------------------------------------------------------------------------------
		private void RunMapSafetyChecks()
		{
			if (!this.isMapSafetyReady)
			{
				return;
			}
			this.mapSafetyIssues.Clear();
			this.suspiciousMapScriptLists.Clear();
			if (this.romData != null && this.tempHeader != null && !this.chkTerrainIdMode.Checked)
			{
				if (this.tempHeader.LoadIncomplete)
				{
					this.mapSafetyIssues.Add(new MapSafetyIssue
					{
						Message = Localizer.T("このマップは ROM 読み込み時に一部のデータ（接続・イベント・マップスクリプトのいずれか）を読めませんでした。データを壊さないよう、このマップは保存できません。詳細は error.log を確認してください。")
					});
				}
				this.CheckMapScriptTerminators();
				this.CheckConnectionTilesets();
			}
			this.UpdateSafetyStatus();
			this.UpdateConnectionButtonLabels();
		}

		//-------------------------------------------------------------------------------
		// マップスクリプトの一覧（種類 02／04）に終端 00 00 があるかを点検する処理
		// AdvanceMap は終端を書かないことがあり、その場合は後ろの無関係なデータまで一覧として読まれてしまう
		//-------------------------------------------------------------------------------
		private void CheckMapScriptTerminators()
		{
			uint headerAddress = this.tempHeader.MapScriptAddress;
			if (headerAddress == 0 || headerAddress >= this.romData.Length)
			{
				return;
			}
			int offset = (int)headerAddress;
			for (int index = 0; index < 16 && offset + 5 <= this.romData.Length; index++, offset += 5)
			{
				byte type = this.romData[offset];
				if (type == 0)
				{
					return;
				}
				if (type > 7)
				{
					this.mapSafetyIssues.Add(new MapSafetyIssue
					{
						Message = string.Format(Localizer.T("マップスクリプトの見出し（0x{0:X6}）に、あり得ない種類 0x{1:X2} があります。見出しの終端 00 が抜けている可能性があります。"), headerAddress, type)
					});
					return;
				}
				if (type != 2 && type != 4)
				{
					continue;
				}
				uint raw = BitConverter.ToUInt32(this.romData, offset + 1);
				if (!IsRomPointer(raw))
				{
					continue;
				}
				uint listAddress = raw - 0x08000000u;
				int validCount;
				if (!this.TryMeasureMapScriptList(listAddress, out validCount))
				{
					this.suspiciousMapScriptLists.Add(listAddress);
					this.mapSafetyIssues.Add(new MapSafetyIssue
					{
						Message = string.Format(Localizer.T("マップスクリプト [{0:X2}] の一覧（0x{1:X6}）に終端 00 00 が見当たりません。正しく読める項目は {2} 件です。このまま保存すると後ろのデータを壊すおそれがあるため、この一覧は保存時に書き戻しません。"), type, listAddress, validCount),
						ScriptHeaderIndex = index,
						ListPointer = listAddress,
						ValidEntryCount = validCount
					});
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 一覧を先頭から読み、終端までの項目がすべて妥当なら true を返す処理（validCount は妥当な件数）
		//-------------------------------------------------------------------------------
		private bool TryMeasureMapScriptList(uint listAddress, out int validCount)
		{
			validCount = 0;
			int offset = (int)listAddress;
			for (int i = 0; i < MapScriptListEntryLimit; i++, offset += 8)
			{
				if (offset + 2 > this.romData.Length)
				{
					return false;
				}
				ushort variable = BitConverter.ToUInt16(this.romData, offset);
				if (variable == 0)
				{
					return true;
				}
				if (offset + 8 > this.romData.Length)
				{
					return false;
				}
				uint script = BitConverter.ToUInt32(this.romData, offset + 4);
				bool variableOk = (variable >= 0x4000 && variable <= 0x40FF) || (variable >= 0x8000 && variable <= 0x80FF);
				// 新規作成直後のスクリプト未設定（0x08000000）は正常として扱う
				bool scriptOk = IsRomPointer(script);
				if (!variableOk || !scriptOk)
				{
					return false;
				}
				validCount++;
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// GBA の ROM 領域を指すポインタ（0x08000000〜0x09FFFFFF）か判定する処理
		//-------------------------------------------------------------------------------
		private bool IsRomPointer(uint pointer)
		{
			return pointer >= 0x08000000u && pointer < 0x08000000u + (uint)this.romData.Length;
		}

		//-------------------------------------------------------------------------------
		// 保存時に書き戻してはいけない（終端が無い）一覧か判定する処理
		//-------------------------------------------------------------------------------
		private bool IsSuspiciousMapScriptList(uint listAddress)
		{
			return this.suspiciousMapScriptLists.Contains(listAddress);
		}

		//-------------------------------------------------------------------------------
		// 接続先マップのタイルセットが現在のマップと同じかを点検する処理
		// GBA では接続先も「今いるマップ」のタイルセットで描かれるため、違うと境目の見た目が崩れる
		//-------------------------------------------------------------------------------
		private void CheckConnectionTilesets()
		{
			if (this.tempHeader.Connections == null || this.tempFooter == null)
			{
				return;
			}
			foreach (ConnectedMap connection in this.tempHeader.Connections)
			{
				if (connection.Direction < 1 || connection.Direction > 4)
				{
					continue;
				}
				MapHeader target = this.FindMapHeader(connection.Bank, connection.Number);
				if (target == null)
				{
					this.mapSafetyIssues.Add(new MapSafetyIssue
					{
						Direction = connection.Direction,
						Message = string.Format(Localizer.T("{0}の接続先 ({1}, {2}) のマップが見つかりません。"), DirectionLabel(connection.Direction), connection.Bank, connection.Number)
					});
					continue;
				}
				MapFooter footer = this.TryReadFooter(target);
				if (footer == null)
				{
					continue;
				}
				bool sameTileset1 = footer.Tileset1Address == this.tempFooter.Tileset1Address;
				bool sameTileset2 = footer.Tileset2Address == this.tempFooter.Tileset2Address;
				if (sameTileset1 && sameTileset2)
				{
					continue;
				}
				// 境目から見える範囲に、現在のマップでは別の絵になってしまうブロックがあるかを数える
				int mismatched = this.CountMismatchedBorderBlocks(connection, footer, !sameTileset1, !sameTileset2);
				if (mismatched > 0)
				{
					string which = !sameTileset1 ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2");
					this.mapSafetyIssues.Add(new MapSafetyIssue
					{
						Direction = connection.Direction,
						Message = string.Format(Localizer.T("{0}の接続先 ({1}, {2}) {3} は{4}が現在のマップと異なり、境目から見える範囲（横{5}・縦{6}マス）に、そのタイルセットのブロックが {7} マスあります。ゲーム中は境目付近が現在のマップのタイルセットで描かれるため、この部分の見た目が崩れます。"), DirectionLabel(connection.Direction), target.Bank, target.Number, target.GetMapName(this), which, ConnectionViewColumns, ConnectionViewRows, mismatched)
					});
				}
			}
		}

		// 境目の外側がゲーム画面に映る幅（Wiki の推奨: 横 8 マス・縦 6 マスは共通ブロックで緩衝する）
		private const int ConnectionViewColumns = 8;
		private const int ConnectionViewRows = 6;

		//-------------------------------------------------------------------------------
		// 接続先マップのうち境目から見える範囲で、タイルセットの違いにより絵が崩れるブロック数を数える処理
		// タイルセット1 が違えば全ブロック、タイルセット2 だけが違えば第2タイルセットのブロック（第1のブロック数以降。FR は 0x280、エメラルドは 0x200）が対象
		//-------------------------------------------------------------------------------
		private int CountMismatchedBorderBlocks(ConnectedMap connection, MapFooter target, bool tileset1Differs, bool tileset2Differs)
		{
			int width = target.MapWidth;
			int height = target.MapHeight;
			if (width == 0 || height == 0 || target.MapDataAddress == 0 || target.MapDataAddress + width * height * 2 > this.romData.Length)
			{
				return 0;
			}
			int currentWidth = this.tempFooter.MapWidth;
			int currentHeight = this.tempFooter.MapHeight;
			int count = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (!IsVisibleFromConnection(connection, x, y, width, height, currentWidth, currentHeight))
					{
						continue;
					}
					int block = BitConverter.ToUInt16(this.romData, (int)target.MapDataAddress + (y * width + x) * 2) & 0x3FF;
					bool isPrimary = block < this.primaryBlockCount;
					if (tileset1Differs || (tileset2Differs && !isPrimary))
					{
						count++;
					}
				}
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 接続先マップの (x, y) が、現在のマップの境目からゲーム画面に映る範囲にあるか判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsVisibleFromConnection(ConnectedMap connection, int x, int y, int width, int height, int currentWidth, int currentHeight)
		{
			switch (connection.Direction)
			{
				case 1: // 下: 接続先の上端の数行
					return y < ConnectionViewRows && x + connection.Shift >= -ConnectionViewColumns && x + connection.Shift < currentWidth + ConnectionViewColumns;
				case 2: // 上: 接続先の下端の数行
					return y >= height - ConnectionViewRows && x + connection.Shift >= -ConnectionViewColumns && x + connection.Shift < currentWidth + ConnectionViewColumns;
				case 3: // 左: 接続先の右端の数列
					return x >= width - ConnectionViewColumns && y + connection.Shift >= -ConnectionViewRows && y + connection.Shift < currentHeight + ConnectionViewRows;
				case 4: // 右: 接続先の左端の数列
					return x < ConnectionViewColumns && y + connection.Shift >= -ConnectionViewRows && y + connection.Shift < currentHeight + ConnectionViewRows;
				default:
					return false;
			}
		}

		//-------------------------------------------------------------------------------
		// バンク・番号からマップの見出しを探す処理
		//-------------------------------------------------------------------------------
		private MapHeader FindMapHeader(int bank, int number)
		{
			return this.mapHeaders.FirstOrDefault(h => h.Bank == bank && h.Number == number);
		}

		//-------------------------------------------------------------------------------
		// マップのフッター（サイズ・タイルセット）を安全に読む処理（読めなければ null）
		//-------------------------------------------------------------------------------
		private MapFooter TryReadFooter(MapHeader header)
		{
			if (header == null || header.FooterAddress == 0 || header.FooterAddress + 26 > this.romData.Length)
			{
				return null;
			}
			try
			{
				return this.ReadMapFooter((int)header.FooterAddress);
			}
			catch (Exception)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------
		// 接続方向の番号を日本語の表記にする処理
		//-------------------------------------------------------------------------------
		private static string DirectionLabel(byte direction)
		{
			switch (direction)
			{
				case 1: return Localizer.T("下");
				case 2: return Localizer.T("上");
				case 3: return Localizer.T("左");
				case 4: return Localizer.T("右");
				case 5: return Localizer.T("潜水");
				case 6: return Localizer.T("浮上");
				default: return Localizer.T("不明な方向");
			}
		}

		//-------------------------------------------------------------------------------
		// ステータスバーの点検結果の表示を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateSafetyStatus()
		{
			if (this.lblStatusCheck == null)
			{
				return;
			}
			if (this.tempHeader == null || this.romData == null)
			{
				this.lblStatusCheck.Text = Localizer.T("点検: -");
				this.lblStatusCheck.ForeColor = UiTheme.TextMuted;
				this.lblStatusCheck.ToolTipText = null;
				return;
			}
			if (this.mapSafetyIssues.Count == 0)
			{
				this.lblStatusCheck.Text = Localizer.T("✓ 点検OK");
				this.lblStatusCheck.ForeColor = UiTheme.TextMuted;
				this.lblStatusCheck.ToolTipText = Localizer.T("マップスクリプトの終端と接続先のタイルセットに問題は見つかりませんでした");
				return;
			}
			this.lblStatusCheck.Text = string.Format(Localizer.T("⚠ 注意 {0}件（クリックで詳細）"), this.mapSafetyIssues.Count);
			this.lblStatusCheck.ForeColor = UiTheme.Warning;
			this.lblStatusCheck.ToolTipText = string.Join("\n", this.mapSafetyIssues.Select(i => Localizer.T("・") + i.Message));
		}

		//-------------------------------------------------------------------------------
		// ステータスバーの点検結果をクリックしたら詳細を表示し、修復できるものは修復を提案する処理
		//-------------------------------------------------------------------------------
		private void lblStatusCheck_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.mapSafetyIssues.Count == 0)
			{
				return;
			}
			StringBuilder sb = new StringBuilder();
			foreach (MapSafetyIssue issue in this.mapSafetyIssues)
			{
				sb.AppendLine(Localizer.T("・") + issue.Message);
				sb.AppendLine();
			}
			List<MapSafetyIssue> repairable = this.mapSafetyIssues.Where(i => i.ScriptHeaderIndex >= 0).ToList();
			if (repairable.Count == 0)
			{
				MessageBox.Show(this, sb.ToString().TrimEnd(), Localizer.T("マップの点検結果"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			sb.AppendLine(Localizer.T("終端の無いマップスクリプト一覧を修復しますか？"));
			sb.AppendLine(Localizer.T("正しく読める項目だけを空き領域へ写し、終端 00 00 を付けて、見出しのポインタを付け替えます。元の場所は上書きしません。"));
			DialogResult answer = MessageBox.Show(this, sb.ToString().TrimEnd(), Localizer.T("マップの点検結果"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			if (answer != DialogResult.Yes)
			{
				return;
			}
			StringBuilder result = new StringBuilder();
			foreach (MapSafetyIssue issue in repairable)
			{
				uint newAddress;
				if (this.TryRelocateMapScriptList(issue, out newAddress))
				{
					result.AppendLine(string.Format(Localizer.T("0x{0:X6} → 0x{1:X6} に {2} 件を移しました。"), issue.ListPointer, newAddress, issue.ValidEntryCount));
				}
				else
				{
					result.AppendLine(string.Format(Localizer.T("0x{0:X6} は空き領域が見つからず修復できませんでした。"), issue.ListPointer));
				}
			}
			this.ReloadMapScriptsAfterRepair();
			result.AppendLine();
			result.AppendLine(Localizer.T("ファイルへ反映するには「ROMを保存」を押してください。"));
			MessageBox.Show(this, result.ToString().TrimEnd(), Localizer.T("修復結果"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		//-------------------------------------------------------------------------------
		// 終端の無い一覧を、妥当な項目＋終端 00 00 の形で空き領域へ写し、見出しのポインタを付け替える処理
		//-------------------------------------------------------------------------------
		private bool TryRelocateMapScriptList(MapSafetyIssue issue, out uint newAddress)
		{
			newAddress = 0;
			int length = issue.ValidEntryCount * 8 + 2;
			if (!this.TryFindFreeSpaceForNewEvent(length, ref newAddress))
			{
				return false;
			}
			Array.Copy(this.romData, (int)issue.ListPointer, this.romData, (int)newAddress, issue.ValidEntryCount * 8);
			this.romData[newAddress + issue.ValidEntryCount * 8] = 0;
			this.romData[newAddress + issue.ValidEntryCount * 8 + 1] = 0;
			int headerEntry = (int)this.tempHeader.MapScriptAddress + issue.ScriptHeaderIndex * 5;
			this.WritePointerToRom(headerEntry + 1, newAddress);
			return true;
		}

		//-------------------------------------------------------------------------------
		// 修復後にマップスクリプトを読み直し、画面と点検結果を更新する処理
		//-------------------------------------------------------------------------------
		private void ReloadMapScriptsAfterRepair()
		{
			this.ReadMapScripts(this.tempHeader);
			if (this.originalHeader != null)
			{
				this.ReadMapScripts(this.originalHeader);
			}
			this.RefreshMapScriptUI();
			this.RunMapSafetyChecks();
		}

		//-------------------------------------------------------------------------------
		// 上下左右の接続ボタンに接続先のマップ名を表示し、点検の注意をツールチップに出す処理
		//-------------------------------------------------------------------------------
		private void UpdateConnectionButtonLabels()
		{
			var buttons = new Dictionary<byte, Button>
			{
				{ 1, this.btnLoadMapDown },
				{ 2, this.btnLoadMapUp },
				{ 3, this.btnLoadMapLeft },
				{ 4, this.btnLoadMapRight },
				{ 5, this.btnLoadMapDive },
				{ 6, this.btnLoadMapEmerge }
			};
			var defaults = new Dictionary<byte, string>
			{
				{ 1, Localizer.T("下") }, { 2, Localizer.T("上") }, { 3, Localizer.T("左") }, { 4, Localizer.T("右") }, { 5, Localizer.T("潜水") }, { 6, Localizer.T("浮上") }
			};
			foreach (KeyValuePair<byte, Button> pair in buttons)
			{
				Button button = pair.Value;
				ConnectedMap connection = (this.tempHeader != null && this.tempHeader.Connections != null)
					? this.tempHeader.Connections.FirstOrDefault(c => c.Direction == pair.Key)
					: null;
				if (connection == null)
				{
					button.Text = (pair.Key == 3) ? "◀" : (pair.Key == 4 ? "▶" : defaults[pair.Key]);
					this.mapToolTip.SetToolTip(button, defaults[pair.Key] + Localizer.T("：接続なし"));
					continue;
				}
				MapHeader target = this.FindMapHeader(connection.Bank, connection.Number);
				string name = (target != null) ? target.GetMapName(this) : string.Format("({0}, {1})", connection.Bank, connection.Number);
				// 上下のボタンは横長なので名前も表示する（左右は縦長なので方向だけ）
				if (pair.Key == 1 || pair.Key == 2)
				{
					button.Text = string.Format("{0} {1}", pair.Key == 2 ? "▲" : "▼", name);
				}
				else if (pair.Key == 3 || pair.Key == 4)
				{
					button.Text = pair.Key == 3 ? "◀" : "▶";
				}
				else
				{
					button.Text = defaults[pair.Key];
				}
				string tip = string.Format(Localizer.T("{0}：({1}, {2}) {3} へ移動"), defaults[pair.Key], connection.Bank, connection.Number, name);
				MapSafetyIssue issue = this.mapSafetyIssues.FirstOrDefault(i => i.Direction == pair.Key);
				if (issue != null)
				{
					tip += "\n⚠ " + issue.Message;
				}
				this.mapToolTip.SetToolTip(button, tip);
			}
		}
	}
}
