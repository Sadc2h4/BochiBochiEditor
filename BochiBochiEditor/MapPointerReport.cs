using System;
using System.Collections.Generic;
using System.Globalization;

namespace BochiBochiEditor
{
	internal sealed class MapPointerEntry
	{
		public string Group;
		public string Name;
		public uint Location;
		public uint RawValue;
		public bool IsPointer;
		public string EventKind;
		public int EventIndex = -1;
	}

	internal static class MapPointerReport
	{
		private const uint RomBase = 0x08000000;

		//-------------------------------------------------------------------------------
		// アドレス・ROM 内の位置・空白区切りの 4 バイトを ROM 内の位置へ直す処理
		//-------------------------------------------------------------------------------
		public static bool TryParseAddress(string text, out uint romOffset)
		{
			romOffset = 0;
			if (string.IsNullOrWhiteSpace(text)) return false;
			string[] parts = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			uint value = 0;
			if (parts.Length == 4 && Array.TrueForAll(parts, part => part.Length == 2))
			{
				for (int i = 0; i < parts.Length; i++)
				{
					if (!byte.TryParse(parts[i], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte part)) return false;
					value |= (uint)part << (i * 8);
				}
			}
			else
			{
				string address = string.Concat(parts);
				if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) address = address.Substring(2);
				else if (address.StartsWith("$", StringComparison.Ordinal)) address = address.Substring(1);
				if (!uint.TryParse(address, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value)) return false;
			}
			if (value == 0) return false;
			if (value >= RomBase && value <= 0x09FFFFFF) romOffset = value - RomBase;
			else if (value < 0x02000000) romOffset = value;
			else return false;
			return true;
		}

		//-------------------------------------------------------------------------------
		// ROM 全体からポインタの 4 バイトを境界に関係なく探し、置き場所を返す処理
		//-------------------------------------------------------------------------------
		public static List<uint> FindReferences(byte[] rom, uint romOffset, int limit = 500)
		{
			List<uint> locations = new List<uint>();
			if (rom == null || rom.Length < 4 || romOffset >= 0x02000000 || limit <= 0) return locations;
			uint pointer = romOffset + RomBase;
			ReadOnlySpan<byte> bytes = stackalloc byte[] { (byte)pointer, (byte)(pointer >> 8), (byte)(pointer >> 16), (byte)(pointer >> 24) };
			int start = 0;
			while (start <= rom.Length - 4 && locations.Count < limit)
			{
				int match = rom.AsSpan(start).IndexOf(bytes);
				if (match < 0) break;
				int location = start + match;
				locations.Add((uint)location);
				start = location + 1;
			}
			return locations;
		}

		//-------------------------------------------------------------------------------
		// ROM 内の位置をポインタのリトルエンディアン 4 バイトに整える処理
		//-------------------------------------------------------------------------------
		public static string FormatBytes(uint romOffset)
		{
			return FormatBytes(new MapPointerEntry { RawValue = romOffset + RomBase });
		}

		//-------------------------------------------------------------------------------
		// ROM に確定済みのマップに関係するポインタを、表から順に集める処理
		//-------------------------------------------------------------------------------
		public static List<MapPointerEntry> Collect(byte[] rom, int bank, int number)
		{
			List<MapPointerEntry> entries = new List<MapPointerEntry>();
			if (rom == null || bank < 0 || number < 0 || MapEditor.MAP_BANK_TABLE_OFFSET < 0)
			{
				return entries;
			}
			MapPointerEntry bankEntry = AddEntry(entries, rom, "表", "バンクの表のこのバンクの項目", (long)MapEditor.MAP_BANK_TABLE_OFFSET + (long)bank * 4);
			if (!TryGetTarget(bankEntry, out long mapTable)) return entries;
			MapPointerEntry headerEntry = AddEntry(entries, rom, "表", "マップの表のこのマップの項目", mapTable + (long)number * 4);
			if (!TryGetTarget(headerEntry, out long header)) return entries;
			if (IsRange(rom, header + 18, 2) && MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET >= 0)
			{
				ushort terrainId = BitConverter.ToUInt16(rom, (int)(header + 18));
				if (terrainId != 0)
				{
					AddEntry(entries, rom, "表", "地形データの表の項目", (long)MapEditor.MAP_TERRAIN_ID_TABLE_OFFSET + (terrainId - 1L) * 4);
				}
			}
			MapPointerEntry footerEntry = AddEntry(entries, rom, "マップ見出し", "地形データ", header);
			MapPointerEntry eventsEntry = AddEntry(entries, rom, "マップ見出し", "イベント", header + 4);
			MapPointerEntry scriptsEntry = AddEntry(entries, rom, "マップ見出し", "マップスクリプト", header + 8);
			MapPointerEntry connectionsEntry = AddEntry(entries, rom, "マップ見出し", "接続", header + 12);
			if (TryGetTarget(footerEntry, out long footer))
			{
				AddEntry(entries, rom, "地形データ", "ボーダー", footer + 8);
				AddEntry(entries, rom, "地形データ", "マップデータ", footer + 12);
				MapPointerEntry tileset1 = AddEntry(entries, rom, "地形データ", "タイルセット1 の見出し", footer + 16);
				MapPointerEntry tileset2 = AddEntry(entries, rom, "地形データ", "タイルセット2 の見出し", footer + 20);
				CollectTileset(entries, rom, tileset1, "タイルセット1");
				CollectTileset(entries, rom, tileset2, "タイルセット2");
			}
			if (TryGetTarget(eventsEntry, out long events)) CollectEvents(entries, rom, events);
			if (TryGetTarget(scriptsEntry, out long scripts)) CollectMapScripts(entries, rom, scripts);
			if (TryGetTarget(connectionsEntry, out long connections))
			{
				AddEntry(entries, rom, "接続", "接続データの表", connections + 4);
			}
			return entries;
		}

		//-------------------------------------------------------------------------------
		// タイルセット見出しの各ポインタをゲームごとの配置に合わせて集める処理
		//-------------------------------------------------------------------------------
		private static void CollectTileset(List<MapPointerEntry> entries, byte[] rom, MapPointerEntry entry, string group)
		{
			if (!TryGetTarget(entry, out long tileset)) return;
			AddEntry(entries, rom, group, "絵", tileset + 4);
			AddEntry(entries, rom, group, "パレット", tileset + 8);
			AddEntry(entries, rom, group, "ブロック表", tileset + 12);
			GameProfile profile = GameProfile.Current;
			if (profile == null) return;
			if (profile.TilesetCallbackOffset >= 0) AddEntry(entries, rom, group, "アニメ処理", tileset + profile.TilesetCallbackOffset);
			if (profile.TilesetBehaviorOffset >= 0) AddEntry(entries, rom, group, "挙動表", tileset + profile.TilesetBehaviorOffset);
		}

		//-------------------------------------------------------------------------------
		// イベントの表と、人物・踏むスクリプト・看板の全件を集める処理
		//-------------------------------------------------------------------------------
		private static void CollectEvents(List<MapPointerEntry> entries, byte[] rom, long events)
		{
			if (!IsRange(rom, events, 4)) return;
			MapPointerEntry persons = AddEntry(entries, rom, "イベント", "人物の表", events + 4);
			AddEntry(entries, rom, "イベント", "ワープの表", events + 8);
			MapPointerEntry traps = AddEntry(entries, rom, "イベント", "踏むスクリプトの表", events + 12);
			MapPointerEntry signs = AddEntry(entries, rom, "イベント", "看板の表", events + 16);
			CollectEventScripts(entries, rom, persons, rom[(int)events], 24, 16, "人物", "person");
			CollectEventScripts(entries, rom, traps, rom[(int)events + 2], 16, 12, "踏むスクリプト", "trap");
			CollectEventScripts(entries, rom, signs, rom[(int)events + 3], 12, 8, "看板", "sign");
		}

		//-------------------------------------------------------------------------------
		// イベント配列のスクリプト欄を集め、隠しアイテムの欄には内容を明記する処理
		//-------------------------------------------------------------------------------
		private static void CollectEventScripts(List<MapPointerEntry> entries, byte[] rom, MapPointerEntry table, int count, int size, int scriptOffset, string group, string kind)
		{
			if (!TryGetTarget(table, out long start)) return;
			for (int i = 0; i < count; i++)
			{
				long record = start + (long)i * size;
				if (!IsRange(rom, record, size)) break;
				bool hiddenItem = kind == "sign" && rom[(int)record + 5] >= 5 && rom[(int)record + 5] <= 8;
				string name = group + " " + (i + 1) + (hiddenItem ? " の隠しアイテム（道具番号など）" : " のスクリプト");
				MapPointerEntry entry = AddEntry(entries, rom, group, name, record + scriptOffset);
				entry.EventKind = kind;
				entry.EventIndex = i;
			}
		}

		//-------------------------------------------------------------------------------
		// マップスクリプトと種類 2・4 の条件付き一覧を上限まで集める処理
		//-------------------------------------------------------------------------------
		private static void CollectMapScripts(List<MapPointerEntry> entries, byte[] rom, long scripts)
		{
			for (int i = 0; i < 16; i++)
			{
				long record = scripts + i * 5L;
				if (!IsRange(rom, record, 1)) break;
				byte type = rom[(int)record];
				if (type == 0 || !IsRange(rom, record, 5)) break;
				bool isList = type == 2 || type == 4;
				string name = string.Format("種類 {0:X2}", type);
				MapPointerEntry entry = AddEntry(entries, rom, "マップスクリプト", name + (isList ? " の一覧" : " のスクリプト"), record + 1);
				if (!isList || !TryGetTarget(entry, out long list)) continue;
				for (int j = 0; j < 64; j++)
				{
					long item = list + j * 8L;
					if (!IsRange(rom, item, 2) || BitConverter.ToUInt16(rom, (int)item) == 0) break;
					if (!IsRange(rom, item, 8)) break;
					AddEntry(entries, rom, "マップスクリプト", name + " の一覧 " + (j + 1) + " 件目のスクリプト", item + 4);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// ROM 内で読み取れる 4 バイトを一覧へ追加する処理（読めない場所は飛ばす）
		//-------------------------------------------------------------------------------
		private static MapPointerEntry AddEntry(List<MapPointerEntry> entries, byte[] rom, string group, string name, long location)
		{
			if (!IsRange(rom, location, 4)) return null;
			uint raw = BitConverter.ToUInt32(rom, (int)location);
			MapPointerEntry entry = new MapPointerEntry
			{
				Group = group,
				Name = name,
				Location = (uint)location,
				RawValue = raw,
				IsPointer = raw >= RomBase && (long)raw - RomBase < rom.LongLength
			};
			entries.Add(entry);
			return entry;
		}

		//-------------------------------------------------------------------------------
		// 加算や読み取りであふれないよう ROM の範囲を確かめる処理
		//-------------------------------------------------------------------------------
		private static bool IsRange(byte[] rom, long location, int size)
		{
			return rom != null && location >= 0 && size >= 0 && location <= rom.LongLength - size;
		}

		//-------------------------------------------------------------------------------
		// 有効なポインタだけを ROM 内の位置へ変換する処理（位置 0 も有効）
		//-------------------------------------------------------------------------------
		private static bool TryGetTarget(MapPointerEntry entry, out long target)
		{
			target = 0;
			if (entry == null || !entry.IsPointer) return false;
			target = (long)entry.RawValue - RomBase;
			return true;
		}

		//-------------------------------------------------------------------------------
		// ポインタの置き場所を GBA の 8 桁のアドレスに整える処理
		//-------------------------------------------------------------------------------
		public static string FormatAddress(uint location)
		{
			return (location + RomBase).ToString("X8");
		}

		//-------------------------------------------------------------------------------
		// 書かれている値をそのまま 8 桁の 16 進数に整える処理
		//-------------------------------------------------------------------------------
		public static string FormatValue(MapPointerEntry e)
		{
			return e.RawValue.ToString("X8");
		}

		//-------------------------------------------------------------------------------
		// 書かれている値をリトルエンディアンの 4 バイトに整える処理
		//-------------------------------------------------------------------------------
		public static string FormatBytes(MapPointerEntry e)
		{
			return string.Format("{0:X2} {1:X2} {2:X2} {3:X2}", e.RawValue & 0xFF, (e.RawValue >> 8) & 0xFF, (e.RawValue >> 16) & 0xFF, (e.RawValue >> 24) & 0xFF);
		}
	}
}
