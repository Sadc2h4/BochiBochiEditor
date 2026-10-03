using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 地形データの表（マップ地形ID の表）との一致を扱う処理
	// ゲームはマップのヘッダーにある「地形データの番号」から表を引いてマップの形を読み込むため、
	// ヘッダーが指すマップのデータと、表のその番号の欄が指す場所がずれていると、ゲームでは古い形のまま表示される
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 地形データの表と今のマップの関係
		private enum TerrainSyncState
		{
			NotApplicable,   // ROM 未読込・地形ID の一覧表示中・マップのデータが無いなど、調べられない
			OutOfRange,      // 地形データの番号が表の範囲外
			Match,           // 表とヘッダーが同じ場所を指している
			Mismatch         // ずれている
		}

		//-------------------------------------------------------------------------------
		// 表のその番号の欄が指す場所を読む処理（読めなければ null）
		//-------------------------------------------------------------------------------
		private uint? ReadTerrainTableEntry(int terrainId)
		{
			if (this.romData == null || terrainId < 1 || terrainId > MapEditor.MAP_TERRAIN_ID_COUNT || MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET <= 0)
			{
				return null;
			}
			int offset = MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET + (terrainId - 1) * 4;
			if (offset < 0 || offset + 4 > this.romData.Length)
			{
				return null;
			}
			uint value = BitConverter.ToUInt32(this.romData, offset);
			return value >= 0x08000000U ? value - 0x08000000U : value;
		}

		//-------------------------------------------------------------------------------
		// 今のマップ（画面の値）と表の関係を調べる処理
		//-------------------------------------------------------------------------------
		private TerrainSyncState GetTerrainSyncState(out uint tableAddress)
		{
			tableAddress = 0U;
			if (this.romData == null || this.tempHeader == null || this.chkTerrainIdMode.Checked || this.tempHeader.FooterAddress == 0U)
			{
				return TerrainSyncState.NotApplicable;
			}
			uint? entry = this.ReadTerrainTableEntry(this.tempHeader.TerrainId);
			if (entry == null)
			{
				return TerrainSyncState.OutOfRange;
			}
			tableAddress = entry.Value;
			return tableAddress == this.tempHeader.FooterAddress ? TerrainSyncState.Match : TerrainSyncState.Mismatch;
		}

		//-------------------------------------------------------------------------------
		// 同じ地形データの番号を使っている、ほかのマップを集める処理（ROM に確定済みの値で見る）
		//-------------------------------------------------------------------------------
		private List<MapEditor.MapHeader> GetMapsSharingTerrainId(int terrainId)
		{
			if (this.mapHeaders == null || this.tempHeader == null)
			{
				return new List<MapEditor.MapHeader>();
			}
			return this.mapHeaders
				.Where(h => h.TerrainId == terrainId && !(h.Bank == this.tempHeader.Bank && h.Number == this.tempHeader.Number))
				.ToList();
		}

		//-------------------------------------------------------------------------------
		// 「場所の情報」の枠にある、表との一致の表示を更新する処理
		//-------------------------------------------------------------------------------
		private void UpdateTerrainSyncStatus()
		{
			if (this.lblTerrainSyncStatus == null)
			{
				return;
			}
			uint tableAddress;
			TerrainSyncState state = this.GetTerrainSyncState(out tableAddress);
			string text;
			bool warn = false;
			switch (state)
			{
				case TerrainSyncState.Match:
					text = Localizer.T("✔ このマップの形は、ゲームにもそのまま反映される状態です。");
					break;
				case TerrainSyncState.Mismatch:
					text = string.Format(Localizer.T("⚠ このマップの形が、ゲームに反映されない状態です。ゲームは「地形データの表」からマップの形を探しますが、表がまだ別の形を指しています。「表を合わせる」で直せます。（表: 0x{0:X6}／このマップ: 0x{1:X6}）"), tableAddress, this.tempHeader.FooterAddress);
					warn = true;
					break;
				case TerrainSyncState.OutOfRange:
					text = string.Format(Localizer.T("⚠ 地形データの番号が範囲（1〜{0}）の外です。ゲームがこのマップの形を見つけられません。"), MapEditor.MAP_TERRAIN_ID_COUNT);
					warn = true;
					break;
				default:
					text = Localizer.T("地形データの表: -");
					break;
			}
			// 同じ番号（同じ形）を使い回しているマップがあれば、そのことを別の行で知らせ、一覧に出すボタンを出す
			int shared = (state == TerrainSyncState.Match || state == TerrainSyncState.Mismatch) ? this.GetMapsSharingTerrainId(this.tempHeader.TerrainId).Count : 0;
			if (shared > 0)
			{
				// 説明画像があれば、画像に書いてある内容は文から省いて 1 行にする（画像が読めないときは全文を出す）
				Image noteImage = this.LoadButtonImage("Map_ID_Note");
				this.picTerrainShareNote.Image = noteImage;
				if (state == TerrainSyncState.Match)
				{
					this.lblTerrainShareNote.Text = noteImage != null
						? string.Format(Localizer.T("ℹ このマップは、ほかの {0} 個のマップと同じ形を使い回しています。"), shared)
						: string.Format(Localizer.T("ℹ このマップは、ほかの {0} 個のマップと同じ形を使い回しています。形は共通のまま、人・イベント・看板などだけを変えて別の場所にしています（ポケモンセンターの中など）。ブロックを塗り替えると、それらのマップの形も一緒に変わります。"), shared);
				}
				else
				{
					this.lblTerrainShareNote.Text = string.Format(Localizer.T("ℹ 同じ地形データの番号を使うマップが、ほかに {0} 個あります（形を使い回しているマップです）。表を合わせると、それらのマップもゲームではこのマップの形になります。"), shared);
				}
				this.lblTerrainShareNote.ForeColor = UiTheme.Text;
				this.picTerrainShareNote.Visible = noteImage != null;
				this.FitTerrainShareNoteHeight();
			}
			else
			{
				this.picTerrainShareNote.Visible = false;
			}
			this.lblTerrainShareNote.Visible = shared > 0;
			this.btnShowSharedMaps.Text = string.Format(Localizer.T("同じ形のマップを左の一覧に出す（{0} 個）"), shared + 1);
			// 高さはほかのボタンにそろえ、幅だけ文字に合わせる
			this.btnShowSharedMaps.Width = TextRenderer.MeasureText(this.btnShowSharedMaps.Text, this.btnShowSharedMaps.Font).Width + this.btnShowSharedMaps.LogicalToDeviceUnits(28);
			this.btnShowSharedMaps.Visible = shared > 0;
			this.lblTerrainSyncStatus.Text = text;
			this.lblTerrainSyncStatus.ForeColor = warn ? UiTheme.Warning : UiTheme.TextMuted;
			this.btnAlignTerrainTable.Visible = state == TerrainSyncState.Mismatch;
		}

		//-------------------------------------------------------------------------------
		// 「同じ形のマップを左の一覧に出す」: 左の検索欄に「地形:番号」を入れて、同じ番号のマップだけに絞り込む処理
		//-------------------------------------------------------------------------------
		private void btnShowSharedMaps_Click(object sender, EventArgs e)
		{
			if (this.tempHeader == null)
			{
				return;
			}
			this.txtMapSearch.Text = string.Format("{0}:{1}", Localizer.T("地形"), this.tempHeader.TerrainId);
			this.mapSearchTimer.Stop();
			this.ApplyMapSearchFilter();
		}

		//-------------------------------------------------------------------------------
		// 表を書き換えてよいか確かめる処理（同じ番号を使うマップがほかにあるときだけ聞く）
		// 書き換えてよければ true
		//-------------------------------------------------------------------------------
		private bool ConfirmTerrainTableRewrite(int terrainId)
		{
			List<MapEditor.MapHeader> shared = this.GetMapsSharingTerrainId(terrainId);
			if (shared.Count == 0)
			{
				return true;
			}
			StringBuilder list = new StringBuilder();
			foreach (MapEditor.MapHeader header in shared.Take(10))
			{
				list.AppendLine(string.Format("  ({0}, {1}) {2}", header.Bank, header.Number, header.GetMapName(this)));
			}
			if (shared.Count > 10)
			{
				list.AppendLine(string.Format(Localizer.T("  ほか {0} 個"), shared.Count - 10));
			}
			string message = string.Format(Localizer.T("このマップの形（地形データ {0} 番）は、次のマップでも使い回されています。{1}{1}{2}{1}表を書き換えると、ゲームではこれらのマップもこのマップの形に変わります。{1}表も書き換えますか？{1}{1}「いいえ」: 表は書き換えません。"), terrainId, Environment.NewLine, list);
			return MessageBox.Show(this, message, Localizer.T("地形データの表"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
		}

		//-------------------------------------------------------------------------------
		// 確定するときに、表とずれていれば表を合わせる処理（「自動で合わせる」がオンのときだけ）
		//-------------------------------------------------------------------------------
		private void SyncTerrainTableOnSave()
		{
			if (!this.chkSyncTerrainId.Checked)
			{
				return;
			}
			uint tableAddress;
			if (this.GetTerrainSyncState(out tableAddress) != TerrainSyncState.Mismatch)
			{
				return;
			}
			int terrainId = this.tempHeader.TerrainId;
			if (!this.ConfirmTerrainTableRewrite(terrainId))
			{
				return;
			}
			this.WritePointerToRom(MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET + (terrainId - 1) * 4, this.tempHeader.FooterAddress);
		}

		//-------------------------------------------------------------------------------
		// 「表を合わせる」ボタン: 確定済みのマップについて、表をこのマップのデータに合わせる処理
		//-------------------------------------------------------------------------------
		private void btnAlignTerrainTable_Click(object sender, EventArgs e)
		{
			if (this.BlockIfReadOnly()) return;
			if (this.hasUnsavedChanges)
			{
				MessageBox.Show(this, Localizer.T("先に「編集中のMAPを確定」でこのマップの変更を確定してから、表を合わせてください。"), Localizer.T("地形データの表"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			uint tableAddress;
			if (this.GetTerrainSyncState(out tableAddress) != TerrainSyncState.Mismatch)
			{
				this.UpdateTerrainSyncStatus();
				return;
			}
			int terrainId = this.tempHeader.TerrainId;
			if (!this.ConfirmTerrainTableRewrite(terrainId))
			{
				return;
			}
			this.WritePointerToRom(MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET + (terrainId - 1) * 4, this.tempHeader.FooterAddress);
			MainForm.romData = this.romData;
			this.UpdateTerrainSyncStatus();
			MessageBox.Show(this, Localizer.T("地形データの表を、このマップのデータに合わせました。ファイルへ反映するには「ROMを保存」を押してください。"), Localizer.T("地形データの表"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
