using System;
using System.Collections.Generic;
using System.Linq;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップタイルの書き出しデータを、形の違うゲーム（ファイアレッド ⇔ エメラルド）向けに変換する処理
	//
	//   第 1 のタイル数   … FR 640 / エメラルド 512。タイルセット2 のタイル番号は「第 1 の数 + 自分の中の番号」なので付け替える。
	//                        置ける枚数を超えるときは、ブロックが使っているタイルだけを詰め直す（使っていないタイルを落とす）。
	//                        それでも超えるときは、書き出したマップが使っているブロックだけを残し（ほかのブロックは空にして）、そのタイルだけを詰め直す
	//   パレットの受け持ち … FR は 0〜6 がタイルセット1・7〜12 がタイルセット2、エメラルドは 0〜5 と 6〜12。
	//                        番号そのものは変わらないので、6 番のパレットのデータをもう片方のタイルセットへ移すだけ
	//   挙動               … FR 4 バイト（挙動 9 ビット・地形 5 ビット・出現 3 ビット・層 2 ビット）、エメラルド 2 バイト（挙動 8 ビット・層 4 ビット）。
	//                        挙動の番号は両方とも同じ並び（ルビーの表が元）とみなして写す。FR 向けの地形・出現は挙動から決める
	//   ブロック数         … 第 1 は FR 640 / エメラルド 512、第 2 は取り込み先の上限。超える分は、書き出したマップが使っていなければ落とす
	//   マップの並び       … タイルセット2 のブロック番号を付け替える
	//
	// 変換した結果は、取り込み先と同じ形の MapTilePackage として返す（元のデータは変えない）
	//-------------------------------------------------------------------------------
	internal static class MapTileConverter
	{
		// タイル番号の上限（10 ビット）と、1 つのタイル指定のバイト数
		private const int TileIdMask = 0x3FF;

		//-------------------------------------------------------------------------------
		// 変換が要るか（取り込み先と形が違うか）を返す処理
		//-------------------------------------------------------------------------------
		public static bool NeedsConversion(MapTilePackage package)
		{
			return package.CheckCompatibleWithCurrentGame() != null;
		}

		//-------------------------------------------------------------------------------
		// 取り込み先（今のゲーム）向けに変換する処理。変換できなければ理由を返し、できれば null（result に結果、notes に注意）
		// primaryBlockLimit・secondaryBlockLimit は、取り込み先で作れるブロック数の上限
		//-------------------------------------------------------------------------------
		public static string Convert(MapTilePackage source, int primaryBlockLimit, int secondaryBlockLimit, out MapTilePackage result, out List<string> notes)
		{
			result = null;
			notes = new List<string>();
			GameProfile target = GameProfile.Current;
			int srcTiles = source.PrimaryTileCount;
			int dstTiles = target.PrimaryTileCount;
			int srcPalettes = source.PrimaryPaletteCount;
			int dstPalettes = target.PrimaryPaletteCount;
			int srcBlocks = source.PrimaryBlockCount;
			int dstBlocks = target.PrimaryBlockCount;
			int blockBytes = source.BlockBytes;
			if (blockBytes != MapEditor.BLOCK_DATA_SIZE)
			{
				return string.Format(Localizer.T("ブロックのバイト数が違う（書き出し元 {0}・取り込み先 {1}）ため、変換できません。"), blockBytes, MapEditor.BLOCK_DATA_SIZE);
			}
			if ((source.BehaviorBytes != 2 && source.BehaviorBytes != 4) || (target.BehaviorBytes != 2 && target.BehaviorBytes != 4))
			{
				return Localizer.T("挙動のバイト数が 2 でも 4 でもないため、変換できません。");
			}
			int refsPerBlock = blockBytes / 2;
			MapTilePackage.TilesetPart[] parts = source.Tilesets;

			// 1. 書き出したマップが使っているブロック（ブロック数を落とす・ブロックを空にするときの判断に使う）
			int maxPrimaryUsed = -1;
			int maxSecondaryUsed = -1;
			HashSet<int>[] usedBlocks = { new HashSet<int>(), new HashSet<int>() };
			foreach (ushort value in (source.Layout ?? new ushort[0]).Concat(source.Border ?? new ushort[0]))
			{
				int id = value & TileIdMask;
				if (id < srcBlocks)
				{
					maxPrimaryUsed = Math.Max(maxPrimaryUsed, id);
					usedBlocks[0].Add(id);
				}
				else
				{
					maxSecondaryUsed = Math.Max(maxSecondaryUsed, id - srcBlocks);
					usedBlocks[1].Add(id - srcBlocks);
				}
			}
			bool hasLayout = usedBlocks[0].Count + usedBlocks[1].Count > 0;
			// 2. ブロックが使っているタイルを集める（タイルセット1 のタイルはタイルセット2 のブロックからも使われる）。
			//    まず全部のブロックで試し、置ける枚数に収まらなければ、書き出したマップが使っているブロックだけで数え直す（ほかのブロックは空にする）
			bool[] blankUnused = { false, false };
			int[] primaryMap = null;
			int[] secondaryMap = null;
			string error = null;
			for (int attempt = 0; attempt < 2; attempt++)
			{
				bool onlyUsed = attempt == 1;
				HashSet<int> usedPrimary = new HashSet<int> { 0 };
				HashSet<int> usedSecondary = new HashSet<int>();
				for (int t = 0; t < 2; t++)
				{
					if (parts[t] == null)
					{
						continue;
					}
					for (int b = 0; b < parts[t].BlockCount; b++)
					{
						if (onlyUsed && !usedBlocks[t].Contains(b))
						{
							continue;
						}
						for (int k = 0; k < refsPerBlock; k++)
						{
							int id = BitConverter.ToUInt16(parts[t].Blocks, b * blockBytes + k * 2) & TileIdMask;
							if (id < srcTiles)
							{
								usedPrimary.Add(id);
							}
							else
							{
								usedSecondary.Add(id - srcTiles);
							}
						}
					}
				}
				List<string> tileNotes = new List<string>();
				error = BuildTileMap(parts[0], srcTiles, dstTiles, usedPrimary, Localizer.T("タイルセット1"), parts[0] != null, tileNotes, out primaryMap);
				if (error == null)
				{
					error = BuildTileMap(parts[1], 1024 - srcTiles, 1024 - dstTiles, usedSecondary, Localizer.T("タイルセット2"), parts[1] != null, tileNotes, out secondaryMap);
				}
				if (error == null)
				{
					notes.AddRange(tileNotes);
					if (onlyUsed)
					{
						for (int t = 0; t < 2; t++)
						{
							blankUnused[t] = parts[t] != null;
						}
						notes.Add(string.Format(Localizer.T("タイルが多すぎるため、書き出したマップが使っているブロック（タイルセット1 は {0} 個・タイルセット2 は {1} 個）だけを残し、ほかのブロックは空にしました。"), usedBlocks[0].Count, usedBlocks[1].Count));
					}
					break;
				}
				if (!hasLayout)
				{
					break;
				}
			}
			if (error != null)
			{
				return error;
			}
			MapTilePackage converted = new MapTilePackage
			{
				GameCode = source.GameCode,
				PrimaryTileCount = dstTiles,
				PrimaryBlockCount = dstBlocks,
				PrimaryPaletteCount = dstPalettes,
				BlockBytes = blockBytes,
				BehaviorBytes = target.BehaviorBytes,
				Bank = source.Bank,
				Number = source.Number,
				MapName = source.MapName,
				MapWidth = source.MapWidth,
				MapHeight = source.MapHeight,
				BorderWidth = source.BorderWidth,
				BorderHeight = source.BorderHeight,
			};
			for (int t = 0; t < 2; t++)
			{
				MapTilePackage.TilesetPart part = parts[t];
				if (part == null)
				{
					continue;
				}
				int[] map = t == 0 ? primaryMap : secondaryMap;
				int limit = t == 0 ? Math.Min(dstBlocks, primaryBlockLimit) : secondaryBlockLimit;
				int blockCount = part.BlockCount;
				if (blockCount > limit)
				{
					int maxUsed = t == 0 ? maxPrimaryUsed : maxSecondaryUsed;
					if (maxUsed >= limit)
					{
						return string.Format(Localizer.T("{0}のブロック数 {1} が取り込み先の上限 {2} を超えていて、書き出したマップが {3} 番のブロックを使っているため落とせません。"),
							t == 0 ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"), blockCount, limit, maxUsed);
					}
					notes.Add(string.Format(Localizer.T("{0}のブロックを {1} 個から {2} 個に減らしました（書き出したマップは {3} 番までしか使っていません）。"),
						t == 0 ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"), blockCount, limit, maxUsed));
					blockCount = limit;
				}
				MapTilePackage.TilesetPart next = new MapTilePackage.TilesetPart
				{
					Index = part.Index,
					Compressed = part.Compressed,
					ImageFile = part.ImageFile,
					BlockCount = blockCount,
					Blocks = new byte[blockCount * blockBytes],
					Behaviors = new byte[blockCount * target.BehaviorBytes],
				};
				// タイルの絵（詰め直し）。アニメの処理はゲームのプログラムなので、変換したデータには持ち越さない
				next.TileImage = RemapImage(part.TileImage, map);
				next.AnimationCallback = 0U;
				next.TileCount = next.TileImage.Length / 32;
				// ブロックのタイル指定（空にするブロックは、タイル 0・パレット 0 で埋めて挙動も 0）
				for (int b = 0; b < blockCount; b++)
				{
					if (blankUnused[t] && !usedBlocks[t].Contains(b))
					{
						continue;
					}
					for (int k = 0; k < refsPerBlock; k++)
					{
						int at = b * blockBytes + k * 2;
						ushort value = BitConverter.ToUInt16(part.Blocks, at);
						int id = value & TileIdMask;
						int newId = id < srcTiles ? primaryMap[id] : dstTiles + secondaryMap[id - srcTiles];
						ushort newValue = (ushort)((value & ~TileIdMask) | (newId & TileIdMask));
						next.Blocks[at] = (byte)(newValue & 0xFF);
						next.Blocks[at + 1] = (byte)(newValue >> 8);
					}
					ConvertBehavior(part.Behaviors, b, source.BehaviorBytes, next.Behaviors, target.BehaviorBytes);
				}
				// パレット: 受け持ちの番号のデータを、持っている方のタイルセットから集める
				for (int p = 0; p < 16; p++)
				{
					bool mine = t == 0 ? p < dstPalettes : p >= dstPalettes;
					MapTilePackage.TilesetPart from = mine ? (p < srcPalettes ? parts[0] : parts[1]) : part;
					if (from == null)
					{
						// 相手のタイルセットが書き出しに無く、そのパレットが取れない（6 番など）
						if (mine && p <= 12)
						{
							notes.Add(string.Format(Localizer.T("パレット {0} 番のデータが書き出しに無いため（{1}に入っていた番号）、空のままです。両方のタイルセットを書き出したデータを使ってください。"), p, p < srcPalettes ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2")));
						}
						continue;
					}
					Array.Copy(from.Palettes, p * 32, next.Palettes, p * 32, 32);
				}
				converted.Tilesets[t] = next;
			}
			// マップの並び・ボーダーのブロック番号
			converted.Layout = ConvertLayout(source.Layout, srcBlocks, dstBlocks, out error);
			if (error != null)
			{
				return error;
			}
			converted.Border = ConvertLayout(source.Border, srcBlocks, dstBlocks, out error);
			if (error != null)
			{
				return error;
			}
			notes.Insert(0, string.Format(Localizer.T("形の違うゲーム（{0} → {1}）向けに変換しました: タイルセット2 のタイル番号は {2} から {3} 始まりに、パレット {4} 番は{5}へ、挙動は {6} バイトから {7} バイトにしました。"),
				source.GameCode, target.Code, srcTiles, dstTiles, Math.Min(srcPalettes, dstPalettes), dstPalettes > srcPalettes ? Localizer.T("タイルセット1") : Localizer.T("タイルセット2"), source.BehaviorBytes, target.BehaviorBytes));
			result = converted;
			return null;
		}

		//-------------------------------------------------------------------------------
		// タイルの付け替え表を作る処理（元の番号 → 新しい番号）
		// 置ける枚数に収まっていれば番号はそのまま。超えていれば、使っているタイルだけを前から詰める（0 番は必ず残す）。それでも収まらなければ理由を返す
		//-------------------------------------------------------------------------------
		private static string BuildTileMap(MapTilePackage.TilesetPart part, int sourceCapacity, int targetCapacity, HashSet<int> used, string label, bool present, List<string> notes, out int[] map)
		{
			int count = present ? part.TileCount : sourceCapacity;
			map = new int[Math.Max(count, sourceCapacity)];
			for (int i = 0; i < map.Length; i++)
			{
				map[i] = i;
			}
			int highest = used.Count > 0 ? used.Max() : -1;
			if ((present ? count : highest + 1) <= targetCapacity)
			{
				return null;
			}
			if (!present)
			{
				return string.Format(Localizer.T("{0}のタイル {1} 番が使われていますが、取り込み先には {2} 枚までしか置けず、{0}が書き出しに無いので詰め直せません。両方のタイルセットを書き出したデータを使ってください。"), label, highest, targetCapacity);
			}
			List<int> keep = Enumerable.Range(0, count).Where(id => id == 0 || used.Contains(id)).ToList();
			if (keep.Count > targetCapacity)
			{
				return string.Format(Localizer.T("{0}は、ブロックが使っているタイルだけでも {1} 枚あり、取り込み先に置ける {2} 枚を超えるため変換できません。"), label, keep.Count, targetCapacity);
			}
			for (int i = 0; i < map.Length; i++)
			{
				map[i] = 0;
			}
			for (int i = 0; i < keep.Count; i++)
			{
				map[keep[i]] = i;
			}
			notes.Add(string.Format(Localizer.T("{0}のタイルを {1} 枚から、使っている {2} 枚に詰め直しました（番号が変わるので、タイルアニメは付け直しが必要です）。"), label, count, keep.Count));
			return null;
		}

		//-------------------------------------------------------------------------------
		// 付け替え表に従ってタイルの絵を並べ直す処理（番号が変わらなければそのまま）
		//-------------------------------------------------------------------------------
		private static byte[] RemapImage(byte[] image, int[] map)
		{
			int count = image.Length / 32;
			bool identity = true;
			int newCount = 0;
			for (int id = 0; id < count; id++)
			{
				if (map[id] != id)
				{
					identity = false;
				}
				newCount = Math.Max(newCount, map[id] + 1);
			}
			if (identity)
			{
				return (byte[])image.Clone();
			}
			// 16 枚（1 行）の倍数にそろえる
			newCount = (newCount + 15) / 16 * 16;
			byte[] result = new byte[newCount * 32];
			bool[] placed = new bool[newCount];
			for (int id = 0; id < count; id++)
			{
				int to = map[id];
				if (to == 0 && id != 0)
				{
					continue;
				}
				if (!placed[to])
				{
					Array.Copy(image, id * 32, result, to * 32, 32);
					placed[to] = true;
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 挙動 1 つを変換する処理
		//   4 → 2: 挙動の下位 8 ビットと層（ビット 29〜30）を写す
		//   2 → 4: 挙動と層（ビット 12〜13）を写し、地形（草 1・水 2・滝 3）と出現（草 1・水 2）は挙動から決める
		//-------------------------------------------------------------------------------
		private static void ConvertBehavior(byte[] source, int index, int sourceBytes, byte[] target, int targetBytes)
		{
			if (sourceBytes == targetBytes)
			{
				Array.Copy(source, index * sourceBytes, target, index * targetBytes, sourceBytes);
				return;
			}
			if (sourceBytes == 4)
			{
				uint attr = BitConverter.ToUInt32(source, index * 4);
				int behavior = (int)(attr & 0xFF);
				int layer = (int)((attr >> 29) & 3);
				ushort value = (ushort)(behavior | (layer << 12));
				target[index * 2] = (byte)(value & 0xFF);
				target[index * 2 + 1] = (byte)(value >> 8);
				return;
			}
			ushort attr16 = BitConverter.ToUInt16(source, index * 2);
			int behavior16 = attr16 & 0xFF;
			int layer16 = (attr16 >> 12) & 3;
			int terrain = 0;
			int encounter = 0;
			if (behavior16 == 0x02 || behavior16 == 0x03)
			{
				terrain = 1;
				encounter = 1;
			}
			else if (behavior16 == 0x13)
			{
				terrain = 3;
			}
			else if (behavior16 >= 0x10 && behavior16 <= 0x17)
			{
				terrain = 2;
				encounter = behavior16 == 0x16 || behavior16 == 0x17 ? 0 : 2;
			}
			uint value32 = (uint)(behavior16 | (terrain << 9) | (encounter << 24) | (layer16 << 29));
			Array.Copy(BitConverter.GetBytes(value32), 0, target, index * 4, 4);
		}

		//-------------------------------------------------------------------------------
		// マップの並び（ボーダー）のブロック番号を付け替える処理（タイルセット2 のブロックは「第 1 の数 + 自分の中の番号」）
		//-------------------------------------------------------------------------------
		private static ushort[] ConvertLayout(ushort[] layout, int sourceBlocks, int targetBlocks, out string error)
		{
			error = null;
			if (layout == null)
			{
				return new ushort[0];
			}
			ushort[] result = new ushort[layout.Length];
			for (int i = 0; i < layout.Length; i++)
			{
				int id = layout[i] & TileIdMask;
				int newId = id < sourceBlocks ? id : id - sourceBlocks + targetBlocks;
				if (newId > TileIdMask || (id < sourceBlocks && id >= targetBlocks))
				{
					error = string.Format(Localizer.T("書き出したマップのブロック番号 {0} は、取り込み先のタイルセットの形では表せません。"), id);
					return null;
				}
				result[i] = (ushort)((layout[i] & ~TileIdMask) | newId);
			}
			return result;
		}
	}
}
