using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ブロック一覧でマウスを乗せたブロックの「挙動（踏んだときの効果）」と「重ね方」を調べて、情報欄に出す処理
	// 値は読み込み中の ROM の挙動データから読み、名前はブロックエディターと同じ一覧（txt フォルダ）を使う
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 挙動・重ね方の名前の一覧（番号 → 「[02]草むら」の形の文字）。ゲームが変わったら読み直す
		private Dictionary<int, string> blockBehaviorNames;
		private Dictionary<int, string> blockLayerNames;
		private string blockNameListGame;

		//-------------------------------------------------------------------------------
		// 「[XX]名前」が並んだ一覧を読む処理（読めなければ空の一覧）
		//-------------------------------------------------------------------------------
		private static Dictionary<int, string> LoadBlockNameList(string fileName)
		{
			Dictionary<int, string> names = new Dictionary<int, string>();
			try
			{
				string path = AppAssetLocator.GetPathOrDefault(Path.Combine("txt", fileName));
				if (!File.Exists(path))
				{
					return names;
				}
				foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
				{
					int close = line.IndexOf(']');
					if (!line.StartsWith("[", StringComparison.Ordinal) || close < 2)
					{
						continue;
					}
					int value;
					if (int.TryParse(line.Substring(1, close - 1), System.Globalization.NumberStyles.HexNumber, null, out value))
					{
						names[value] = line.TrimEnd();
					}
				}
			}
			catch (Exception)
			{
				// 一覧が読めないときは、番号だけを出す
			}
			return names;
		}

		//-------------------------------------------------------------------------------
		// 一覧から名前を返す処理（一覧に無い番号・名前が空の番号は「[XX]」だけ）
		//-------------------------------------------------------------------------------
		private static string GetBlockNameOrNumber(Dictionary<int, string> names, int value)
		{
			string name;
			return names != null && names.TryGetValue(value, out name) ? Localizer.T(name) : string.Format("[{0:X2}]", value);
		}

		//-------------------------------------------------------------------------------
		// ブロックの挙動と重ね方の説明を作る処理（そのブロックのデータが無い・読めないときは null）
		// 例: 「挙動: [02]草むら　重ね方: [00]下BG2･上BG1」（3 層ブロックの改造版は挙動だけ）
		//-------------------------------------------------------------------------------
		private string DescribeBlockBehavior(int blockId)
		{
			if (this.romData == null || this.tempTileset1 == null || this.tempTileset2 == null || blockId < 0 || blockId >= this.totalBlocks)
			{
				return null;
			}
			bool primary = blockId < this.primaryBlockCount;
			// タイルセット1 の実際のブロック数より後ろの枠は、データが無い
			if (primary && blockId >= this.primaryUsedBlockCount)
			{
				return null;
			}
			int behaviorBytes = GameProfile.Current.BehaviorBytes;
			uint table = primary ? this.tempTileset1.BlockBehaviorAddress : this.tempTileset2.BlockBehaviorAddress;
			long address = (long)table + (long)(primary ? blockId : blockId - this.primaryBlockCount) * behaviorBytes;
			if (table == 0U || address < 0 || address + behaviorBytes > this.romData.Length)
			{
				return null;
			}
			string game = GameProfile.Current.EmeraldHeaderLayout ? "EM" : "FR";
			if (this.blockBehaviorNames == null || this.blockNameListGame != game)
			{
				this.blockBehaviorNames = LoadBlockNameList(game == "EM" ? "BlockTileAction_EM.txt" : "BlockTileAction.txt");
				this.blockLayerNames = LoadBlockNameList(game == "EM" ? "BlockLayer_EM.txt" : "BlockLayer.txt");
				this.blockNameListGame = game;
			}
			int action = this.romData[address];
			// 重ね方は、2 バイトの形（エメラルド）では 2 バイト目の上位 4 ビット、4 バイトの形（ファイアレッド）では 4 バイト目の上位 6 ビット
			int last = this.romData[address + behaviorBytes - 1];
			int layer = behaviorBytes == 2 ? (last & 0xF0) : (last & 0xFC);
			// 3 層ブロック（24 バイト）の改造版は重ね方の値を使わないので、挙動だけを出す
			string text = MapEditor.BLOCK_DATA_SIZE == 24
				? string.Format(Localizer.T("挙動: {0}"), GetBlockNameOrNumber(this.blockBehaviorNames, action))
				: string.Format(Localizer.T("挙動: {0}　重ね方: {1}"), GetBlockNameOrNumber(this.blockBehaviorNames, action), GetBlockNameOrNumber(this.blockLayerNames, layer));
			if (behaviorBytes != 2 && (last & 3) != 0)
			{
				// ファイアレッドは、野生のポケモンが出る種類（草むら・水上）もここに入っている
				text += "　" + string.Format(Localizer.T("野生: {0}"), (last & 1) != 0 ? Localizer.T("草むら") : Localizer.T("水上"));
			}
			return text;
		}
	}
}
