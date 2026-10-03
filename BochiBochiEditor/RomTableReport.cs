using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 「開いている ROM のテーブル情報」の 1 行（1 つの表）
	//-------------------------------------------------------------------------------
	internal sealed class RomTableEntry
	{
		// 状態（数字が大きいほど注意が必要）
		public enum Level
		{
			Ok = 0,
			Info = 1,
			Warning = 2,
			Error = 3,
		}

		public string Category;
		public string Name;
		public string Usage;
		public string IniKey;
		// 表の場所（ROM 上。読めなければ -1）と、どうやって場所を知ったか
		public int Address = -1;
		public string Source;
		public string EntrySize;
		public string Count;
		public Level Status = Level.Ok;
		public readonly List<string> Notes = new List<string>();

		//-------------------------------------------------------------------------------
		// メモを足し、状態をより重いほうへ上げる処理
		//-------------------------------------------------------------------------------
		public void Add(Level level, string note)
		{
			if (level > this.Status)
			{
				this.Status = level;
			}
			this.Notes.Add(note);
		}
	}

	//-------------------------------------------------------------------------------
	// 開いている ROM の表を一通り調べて、「どこを・何バイト×何件で読んでいるか」と気になる点をまとめる処理
	// 場所は ini の設定（多くはポインタ経由）で探し、形（1 項目の長さ・件数）は ROM の中身から確かめて ini と見比べる
	//-------------------------------------------------------------------------------
	internal static class RomTableReport
	{
		// 表の調べ方
		private enum Kind
		{
			Location,   // 場所だけ確かめる
			Name,       // 名前の表（1 項目の長さと、名前として読めるかを確かめる）
			Tagged8,    // 8 バイト（ポインタ＋番号 2 つ）で、番号が 1 ずつ増えていく表（絵・パレットなど）
			Pointer4,   // 4 バイトのポインタが並ぶ表
			Pointer8,   // 8 バイトごとにポインタがある表
			MapName,    // マップ名の表（1 件の大きさとポインタの位置はゲームごと。ファイアレッド 4 バイト、エメラルド 8 バイトの +4）
			PointerPair,// 8 バイト（ポインタ 2 つ）の表
			Wild,       // 出現ポケモンの表（20 バイト、FF FF で終わる）
		}

		// 表の定義（分類、表の名前、使いみち、ini の場所の項目、調べ方、1 項目の長さの項目、件数の項目、元のゲームの件数）
		private sealed class Definition
		{
			public string Category;
			public string Name;
			public string Usage;
			public string Key;
			public Kind Kind;
			public string LengthKey;
			public string CountKey;
			public int OriginalCount;
		}

		private static readonly Definition[] definitions =
		{
			D("ポケモン", "ポケモンの名前", "図鑑・出現ポケモン・バトルなどに出る名前", "POKEMON_NAME_OFFSET", Kind.Name, "POKEMON_NAME_LENGTH", "TOTAL_POKEMON_COUNT", 0),
			D("ポケモン", "能力（種族値・タイプ・とくせい など）", "ポケモンごとの基本のデータ", "BASE_STATS_OFFSET", Kind.Location, null, "TOTAL_POKEMON_COUNT", 0),
			D("ポケモン", "正面の絵", "バトルで相手側に出る絵", "FRONT_IMAGE_TABLE_OFFSET", Kind.Tagged8, null, null, 440),
			D("ポケモン", "背面の絵", "バトルで味方側に出る絵", "BACK_IMAGE_TABLE_OFFSET", Kind.Tagged8, null, null, 440),
			D("ポケモン", "通常の色", "正面・背面の絵の色", "NORMAL_PALETTE_TABLE_OFFSET", Kind.Tagged8, null, null, 440),
			D("ポケモン", "色違いの色", "色違いのときの絵の色", "SHINY_PALETTE_TABLE_OFFSET", Kind.Tagged8, null, null, 440),
			D("ポケモン", "正面の絵の高さ", "正面の絵を表示する高さ", "FRONT_Y_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("ポケモン", "背面の絵の高さ", "背面の絵を表示する高さ", "BACK_Y_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("ポケモン", "影の設定", "浮いているポケモンの影など", "SHADOW_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("ポケモン", "ミニアイコンの絵", "手持ち・出現ポケモンなどの小さな絵", "ICON_IMAGE_TABLE_OFFSET", Kind.Pointer4, null, null, 440),
			D("ポケモン", "ミニアイコンのパレット番号", "ミニアイコンがどのパレットを使うか", "ICON_PALETTE_ID_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("ポケモン", "ミニアイコンのパレット", "ミニアイコンの色", "ICON_PALETTE_TABLE_OFFSET", Kind.Tagged8, null, "ICON_PALETTE_COUNT", 6),
			D("ポケモン", "足あと", "図鑑の足あと", "FOOTPRINT_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("ポケモン", "進化", "進化の条件と進化先", "EVOLUTION_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("ポケモン", "鳴き声（1）", "鳴き声の音", "CRY_DATA_TABLE_OFFSET_1", Kind.Location, null, null, 0),
			D("ポケモン", "鳴き声（2）", "鳴き声の音（逆再生など）", "CRY_DATA_TABLE_OFFSET_2", Kind.Location, null, null, 0),
			D("ポケモン", "鳴き声の対応表", "ホウエン以降のポケモンの鳴き声の番号", "EXTENDED_CRY_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("わざ", "わざの名前", "わざの名前", "MOVE_NAME_TABLE_OFFSET", Kind.Name, "MOVE_NAME_LENGTH", "TOTAL_MOVE_COUNT", 0),
			D("わざ", "レベルで覚えるわざ", "ポケモンごとの、レベルアップで覚えるわざ", "LEVEL_MOVE_TABLE_OFFSET", Kind.Pointer4, null, "TOTAL_POKEMON_COUNT", 412),
			D("わざ", "わざマシン・ひでんマシン", "わざマシンで覚えるわざ", "TM_HM_LIST_OFFSET", Kind.Location, null, null, 0),
			D("わざ", "わざマシンで覚えられるか", "ポケモンごとの、わざマシンの対応", "TM_HM_LEARN_OFFSET", Kind.Location, null, null, 0),
			D("わざ", "教えわざ", "教えわざの一覧", "MOVE_TUTOR_LIST_OFFSET", Kind.Location, null, null, 0),
			D("わざ", "教えわざを覚えられるか", "ポケモンごとの、教えわざの対応", "MOVE_TUTOR_LEARN_OFFSET", Kind.Location, null, null, 0),
			D("わざ", "タマゴわざ", "タマゴから生まれるときに覚えているわざ", "EGG_MOVE_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("わざ", "とくせいの名前", "とくせいの名前", "ABILITY_NAME_TABLE_OFFSET", Kind.Name, "ABILITY_NAME_LENGTH", "TOTAL_ABILITY_COUNT", 0),
			D("わざ", "タイプの名前", "タイプの名前", "TYPE_TABLE_OFFSET", Kind.Name, "TYPE_NAME_LENGTH", "TOTAL_TYPE_COUNT", 0),
			D("図鑑", "図鑑のデータ", "分類・高さ・重さ・説明", "POKEDEX_DATA_OFFSET", Kind.Location, null, null, 0),
			D("図鑑", "図鑑番号の対応", "ポケモンの番号と図鑑番号の対応", "POKEDEX_ORDER_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("図鑑", "生息地", "図鑑の生息地の分類", "HABITAT_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("図鑑", "並び（あいうえお順）", "図鑑の並べ替え", "AIUEO_ORDER_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("図鑑", "並び（軽い順）", "図鑑の並べ替え", "LIGHT_ORDER_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("図鑑", "並び（小さい順）", "図鑑の並べ替え", "SMALL_ORDER_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("図鑑", "並び（タイプ順）", "図鑑の並べ替え", "TYPE_ORDER_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("道具", "道具のデータ", "名前・値段・効果", "ITEM_INFO_TABLE_OFFSET", Kind.Location, null, "TOTAL_ITEM_COUNT", 0),
			D("道具", "道具の絵", "道具の絵と色", "ITEM_IMAGE_TABLE_OFFSET", Kind.PointerPair, null, "TOTAL_ITEM_COUNT", 376),
			D("道具", "道具を使ったときの処理", "道具を使ったときに動く処理", "ITEM_EFFECT_ADDRESS_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("道具", "道具を使うときの表示位置", "道具を使う画面での位置", "ITEM_USE_COORDINATE_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("道具", "持たせるメール", "ポケモンに持たせるメール", "HELD_ITEM_MAIL_OFFSET", Kind.Location, null, null, 0),
			D("トレーナー", "トレーナーのデータ", "名前・手持ちポケモン", "TRAINER_DATA_OFFSET", Kind.Location, null, "TRAINER_ENTRY_COUNT", 0),
			D("トレーナー", "トレーナーの種類名", "たんぱんこぞう などの肩書き", "TRAINER_CLASS_NAME_TABLE_OFFSET", Kind.Name, "TRAINER_CLASS_NAME_LENGTH", "TRAINER_CLASS_NAME_COUNT", 0),
			D("トレーナー", "トレーナーの絵", "バトルで出るトレーナーの絵", "TRAINER_SPRITE_TABLE_OFFSET", Kind.Tagged8, null, "MAX_TRAINER_SPRITE_COUNT", 148),
			D("トレーナー", "トレーナーの色", "トレーナーの絵の色", "TRAINER_PALETTE_TABLE_OFFSET", Kind.Tagged8, null, "MAX_TRAINER_SPRITE_COUNT", 148),
			D("トレーナー", "トレーナーの絵の高さ", "トレーナーの絵を表示する高さ", "TRAINER_Y_POSITION_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("トレーナー", "トレーナーの動き（表）", "トレーナーの絵の動き", "TRAINER_ANIMATION_POINTER_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("トレーナー", "トレーナーの動き（データ）", "トレーナーの絵の動き", "TRAINER_ANIMATION_DATA_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("トレーナー", "賞金", "トレーナーの種類ごとの賞金", "PRIZE_MONEY_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("マップ", "マップの一覧（バンク）", "バンクごとのマップの一覧", "MAP_BANK_TABLE_OFFSET", Kind.Pointer4, null, null, 43),
			D("マップ", "マップ名", "場所の名前（看板・タウンマップ）", "MAP_NAME_TABLE_OFFSET", Kind.MapName, null, "MAP_NAME_COUNT", 109),
			D("マップ", "地形データの一覧", "マップの地形データ（レイアウト）", "MAP_TERRAIN_ID_TABLE_OFFSET", Kind.Location, null, "MAP_TERRAIN_ID_COUNT", 0),
			D("マップ", "出現ポケモン", "マップごとの野生ポケモン", "WILD_ENCOUNTER_TABLE_DAY_OFFSET", Kind.Wild, null, null, 132),
			D("マップ", "人物（歩行グラフィック）", "マップに置く人物の絵と設定", "OVERWORLD_DATA_TABLE_OFFSET", Kind.Pointer4, null, null, 174),
			D("マップ", "人物の色", "人物の絵の色", "OVERWORLD_PALETTE_TABLE_OFFSET", Kind.Pointer8, null, null, 18),
			D("マップ", "人物の追加データ", "人物の表示に使う追加のデータ", "OVERWORLD_FONT_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("その他", "ゲーム内交換", "ゲームの中でできる交換", "IN_GAME_TRADE_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("その他", "性格の名前", "性格の名前", "PERSONALITY_TEXT_TABLE_OFFSET", Kind.Location, null, null, 0),
			D("その他", "かんたん会話", "かんたん会話の単語のグループ", "EASYCHAT_GROUP_TABLE_OFFSET", Kind.Location, null, null, 0),
		};

		// 元のファイアレッド（日本語版）での表の位置（移動しているかどうかの目安）
		private static readonly Dictionary<string, int> originalAddresses = new Dictionary<string, int>
		{
			{ "POKEMON_NAME_OFFSET", 0x203CB8 },
			{ "FRONT_IMAGE_TABLE_OFFSET", 0x1F4690 },
			{ "BACK_IMAGE_TABLE_OFFSET", 0x1F5B30 },
			{ "NORMAL_PALETTE_TABLE_OFFSET", 0x1F68F0 },
			{ "SHINY_PALETTE_TABLE_OFFSET", 0x1F76B0 },
			{ "FRONT_Y_TABLE_OFFSET", 0x1F3FB0 },
			{ "BACK_Y_TABLE_OFFSET", 0x1F5450 },
			{ "SHADOW_TABLE_OFFSET", 0x1F95E8 },
			{ "ICON_IMAGE_TABLE_OFFSET", 0x39BCA8 },
			{ "ICON_PALETTE_ID_TABLE_OFFSET", 0x39C388 },
			{ "ICON_PALETTE_TABLE_OFFSET", 0x39C540 },
			{ "FOOTPRINT_TABLE_OFFSET", 0x404164 },
			{ "BASE_STATS_OFFSET", 0x21118C },
			{ "ABILITY_NAME_TABLE_OFFSET", 0x20C274 },
			{ "TYPE_TABLE_OFFSET", 0x20C074 },
			{ "MOVE_NAME_TABLE_OFFSET", 0x204660 },
			{ "LEVEL_MOVE_TABLE_OFFSET", 0x21A1BC },
			{ "TM_HM_LIST_OFFSET", 0x419F9C },
			{ "TM_HM_LEARN_OFFSET", 0x20F5D0 },
			{ "MOVE_TUTOR_LIST_OFFSET", 0x4192F0 },
			{ "MOVE_TUTOR_LEARN_OFFSET", 0x41930E },
			{ "EVOLUTION_TABLE_OFFSET", 0x21615C },
			{ "POKEDEX_DATA_OFFSET", 0x40E2D0 },
			{ "CRY_DATA_TABLE_OFFSET_1", 0x451554 },
			{ "CRY_DATA_TABLE_OFFSET_2", 0x452784 },
			{ "EXTENDED_CRY_TABLE_OFFSET", 0x2103DC },
			{ "EGG_MOVE_TABLE_OFFSET", 0x21B918 },
			{ "POKEDEX_ORDER_TABLE_OFFSET", 0x20E9F6 },
			{ "HABITAT_TABLE_OFFSET", 0x411AB4 },
			{ "AIUEO_ORDER_TABLE_OFFSET", 0x408958 },
			{ "LIGHT_ORDER_TABLE_OFFSET", 0x408C8E },
			{ "SMALL_ORDER_TABLE_OFFSET", 0x408F92 },
			{ "TYPE_ORDER_TABLE_OFFSET", 0x409296 },
			{ "ITEM_INFO_TABLE_OFFSET", 0x3A06F8 },
			{ "ITEM_IMAGE_TABLE_OFFSET", 0x39C79C },
			{ "ITEM_EFFECT_ADDRESS_TABLE_OFFSET", 0x20F2C4 },
			{ "ITEM_USE_COORDINATE_TABLE_OFFSET", 0x422068 },
			{ "TRAINER_SPRITE_TABLE_OFFSET", 0x1F8B60 },
			{ "TRAINER_PALETTE_TABLE_OFFSET", 0x1F9000 },
			{ "TRAINER_Y_POSITION_TABLE_OFFSET", 0x1F8910 },
			{ "TRAINER_ANIMATION_POINTER_TABLE_OFFSET", 0x1F86C0 },
			{ "TRAINER_ANIMATION_DATA_TABLE_OFFSET", 0x1F8470 },
			{ "TRAINER_CLASS_NAME_TABLE_OFFSET", 0x1FDB3C },
			{ "PRIZE_MONEY_TABLE_OFFSET", 0x20C0D0 },
			{ "TRAINER_DATA_OFFSET", 0x1FDFD8 },
			{ "IN_GAME_TRADE_TABLE_OFFSET", 0x22D2F8 },
			{ "PERSONALITY_TEXT_TABLE_OFFSET", 0x42D1EC },
			{ "HELD_ITEM_MAIL_OFFSET", 0x22D514 },
			{ "EASYCHAT_GROUP_TABLE_OFFSET", 0x3B3A44 },
			{ "MAP_NAME_TABLE_OFFSET", 0x3B8834 },
			{ "MAP_BANK_TABLE_OFFSET", 0x316758 },
			{ "MAP_TERRAIN_ID_TABLE_OFFSET", 0x312C3C },
			{ "OVERWORLD_DATA_TABLE_OFFSET", 0x363E38 },
			{ "OVERWORLD_PALETTE_TABLE_OFFSET", 0x3691E0 },
			{ "OVERWORLD_FONT_TABLE_OFFSET", 0x42D68C },
			{ "WILD_ENCOUNTER_TABLE_DAY_OFFSET", 0x390B34 },
		};

		// 元のエメラルド（英語版・日本語版で同じ）での件数（この画面の数え方で数えたもの。改造版で増減しているかの目安）
		private static readonly Dictionary<string, int> emeraldOriginalCounts = new Dictionary<string, int>
		{
			{ "ICON_IMAGE_TABLE_OFFSET", 440 },
			{ "ICON_PALETTE_TABLE_OFFSET", 6 },
			{ "MAP_BANK_TABLE_OFFSET", 34 },
			{ "MAP_NAME_TABLE_OFFSET", 213 },
			{ "WILD_ENCOUNTER_TABLE_DAY_OFFSET", 124 },
			{ "OVERWORLD_DATA_TABLE_OFFSET", 284 },
			{ "OVERWORLD_PALETTE_TABLE_OFFSET", 35 },
		};

		// 数えるときの上限（壊れたデータで延々と読まないように）
		private const int MaxCount = 5000;

		//-------------------------------------------------------------------------------
		// 表の定義を 1 つ作る処理
		//-------------------------------------------------------------------------------
		private static Definition D(string category, string name, string usage, string key, Kind kind, string lengthKey, string countKey, int originalCount)
		{
			return new Definition { Category = category, Name = name, Usage = usage, Key = key, Kind = kind, LengthKey = lengthKey, CountKey = countKey, OriginalCount = originalCount };
		}

		//-------------------------------------------------------------------------------
		// 開いている ROM の表を一通り調べる処理
		//-------------------------------------------------------------------------------
		public static List<RomTableEntry> Build(byte[] rom)
		{
			List<RomTableEntry> entries = new List<RomTableEntry>();
			if (rom == null)
			{
				return entries;
			}
			bool original = GameProfile.Current != null && GameProfile.Current.Code == "BPRJ";
			bool emerald = GameProfile.Current != null && GameProfile.Current.EmeraldHeaderLayout;
			foreach (Definition definition in definitions)
			{
				// エメラルド系の設定ファイルに無い表（ファイアレッド用の旧エディタだけが使う表）は、一覧に出さない
				if (emerald && string.IsNullOrEmpty(RomIniReader.ReadValue(definition.Key)))
				{
					continue;
				}
				entries.Add(Inspect(rom, definition, original, emerald));
			}
			entries.Add(InspectSongs(rom));
			return entries;
		}

		//-------------------------------------------------------------------------------
		// 1 つの表を調べる処理
		//-------------------------------------------------------------------------------
		private static RomTableEntry Inspect(byte[] rom, Definition definition, bool original, bool emerald)
		{
			RomTableEntry entry = new RomTableEntry { Category = definition.Category, Name = definition.Name, Usage = definition.Usage, IniKey = definition.Key };
			string raw = RomIniReader.ReadValue(definition.Key);
			if (string.IsNullOrEmpty(raw))
			{
				entry.Add(RomTableEntry.Level.Warning, "ini に場所の設定がありません");
				return entry;
			}
			entry.Source = DescribeSource(raw);
			int address = ResolveOffset(rom, raw, definition.Key);
			entry.Address = address;
			if (address < 0 || address >= rom.Length)
			{
				entry.Add(RomTableEntry.Level.Error, "場所が ROM の外を指しているため読めません");
				return entry;
			}
			// 元のゲームと比べて場所が変わっていれば知らせる（拡張領域へ移された表の目印）
			int originalAddress;
			if (original && originalAddresses.TryGetValue(definition.Key, out originalAddress) && originalAddress != address)
			{
				entry.Add(RomTableEntry.Level.Info, string.Format("元の位置 0x{0:X6} から移動しています", originalAddress));
			}
			int iniCount = ReadIniNumber(definition.CountKey);
			// 元のゲームの件数（エメラルド系は別の表から取る。無ければ比べない）
			int originalCount = definition.OriginalCount;
			if (emerald && !emeraldOriginalCounts.TryGetValue(definition.Key, out originalCount))
			{
				originalCount = 0;
			}
			switch (definition.Kind)
			{
				case Kind.Name:
					InspectNames(rom, entry, address, ReadIniNumber(definition.LengthKey), iniCount, IsAuto(definition.CountKey) && definition.Key == "POKEMON_NAME_OFFSET");
					break;
				case Kind.Tagged8:
					ReportCount(entry, CountTagged8(rom, address), iniCount, originalCount, 8);
					break;
				case Kind.Pointer4:
					ReportCount(entry, CountPointers(rom, address, 4, false), iniCount, originalCount, 4);
					break;
				case Kind.Pointer8:
					ReportCount(entry, CountPointers(rom, address, 8, false), iniCount, originalCount, 8);
					break;
				case Kind.PointerPair:
					ReportCount(entry, CountPointers(rom, address, 8, true), iniCount, originalCount, 8);
					break;
				case Kind.MapName:
					ReportCount(entry, CountMapNames(rom, address), iniCount, originalCount, MapEditor.MAP_NAME_ENTRY_SIZE);
					break;
				case Kind.Wild:
					ReportCount(entry, CountWildEntries(rom, address), iniCount, originalCount, 20);
					break;
				default:
				{
					// 件数が auto の表は、エディタが ROM から数えた件数を出す
					int autoCount = IsAuto(definition.CountKey) ? ReadEditorNumber(definition.CountKey) : 0;
					entry.EntrySize = "-";
					entry.Count = iniCount > 0 ? string.Format("ini: {0}", iniCount) : (autoCount > 0 ? string.Format("ROM: {0}", autoCount) : "-");
					break;
				}
			}
			if (entry.Notes.Count == 0)
			{
				entry.Notes.Add(definition.Kind == Kind.Location ? "場所のみ確認（中身の判定は未対応）" : "問題なし");
			}
			return entry;
		}

		//-------------------------------------------------------------------------------
		// 名前の表を調べる処理（1 項目の長さを ROM から判定し、ini の件数ぶんが名前として読めるかを数える）
		// countFromRom: 件数が ini で auto のポケモン名の表（匹数を ROM から数える。PokemonNameTable）
		//-------------------------------------------------------------------------------
		private static void InspectNames(byte[] rom, RomTableEntry entry, int address, int iniLength, int iniCount, bool countFromRom)
		{
			int romCount = 0;
			if (countFromRom)
			{
				try
				{
					romCount = PokemonNameTable.Create(rom).Count;
				}
				catch (Exception)
				{
					romCount = 0;
				}
			}
			int count = iniCount > 1 ? iniCount : (romCount > 1 ? romCount : 100);
			int length = RomTableDetector.DetectNameLengthQuiet(rom, address, iniLength, count);
			int readable = RomTableDetector.CountReadableNames(rom, address, length, count);
			entry.EntrySize = length == iniLength || iniLength <= 0 ? length.ToString(CultureInfo.InvariantCulture) : string.Format("{0}（ini は {1}）", length, iniLength);
			entry.Count = iniCount > 0
				? string.Format("ini: {0}（名前として読める: {1}）", iniCount, readable)
				: (romCount > 0 ? string.Format("ROM: {0}（名前として読める: {1}）", romCount, readable) : string.Format("先頭 {0} 件のうち、名前として読める: {1}", count - 1, readable));
			if (length != iniLength && iniLength > 0)
			{
				entry.Add(RomTableEntry.Level.Warning, string.Format("1 項目の長さが ini と違います（ROM は {0} バイト、ini は {1} バイト）。この画面では ROM の長さで読んでいます", length, iniLength));
			}
			if (readable * 10 < (count - 1) * 9)
			{
				entry.Add(RomTableEntry.Level.Warning, "名前として読めない項目が多いため、場所か長さが合っていない可能性があります");
			}
		}

		//-------------------------------------------------------------------------------
		// 数えた件数を、ini の件数・元のゲームの件数と見比べて書く処理
		//-------------------------------------------------------------------------------
		private static void ReportCount(RomTableEntry entry, int detected, int iniCount, int originalCount, int size)
		{
			entry.EntrySize = size.ToString(CultureInfo.InvariantCulture);
			StringBuilder text = new StringBuilder();
			text.AppendFormat("ROM: {0}", detected < 0 ? "?" : detected.ToString(CultureInfo.InvariantCulture));
			if (iniCount > 0)
			{
				text.AppendFormat(" / ini: {0}", iniCount);
			}
			if (originalCount > 0)
			{
				text.AppendFormat(" / 元: {0}", originalCount);
			}
			entry.Count = text.ToString();
			if (detected < 0)
			{
				entry.Add(RomTableEntry.Level.Warning, "件数を ROM の中身から判定できませんでした");
				return;
			}
			if (originalCount > 0 && detected != originalCount)
			{
				entry.Add(RomTableEntry.Level.Info, detected > originalCount
					? string.Format("元のゲームより {0} 件多くなっています（拡張されています）", detected - originalCount)
					: string.Format("元のゲームより {0} 件少なくなっています", originalCount - detected));
			}
			// 元のゲームと同じ件数なら、ini との違いは元からのもの（知らせない）
			if (iniCount > 0 && detected > 0 && detected < iniCount && detected != originalCount)
			{
				entry.Add(RomTableEntry.Level.Warning, string.Format("ini の件数（{0}）より少ないため、ini のまま読むと表の外まで読んでしまいます", iniCount));
			}
			else if (iniCount > 0 && detected > iniCount + 1 && detected != originalCount)
			{
				entry.Add(RomTableEntry.Level.Info, string.Format("ini の件数（{0}）より多く入っています。ini の件数で読む画面では、増えた分は出ません", iniCount));
			}
		}

		//-------------------------------------------------------------------------------
		// 曲の表を調べる処理（曲の表は ini ではなく ROM の中身から探す）
		//-------------------------------------------------------------------------------
		private static RomTableEntry InspectSongs(byte[] rom)
		{
			RomTableEntry entry = new RomTableEntry { Category = "マップ", Name = "曲", Usage = "マップの BGM などの曲", IniKey = "-", Source = "ROM の中身から探索", EntrySize = "8" };
			SongTable table = SongTable.Find(rom);
			if (table == null)
			{
				entry.Add(RomTableEntry.Level.Warning, "曲の表が見つかりませんでした");
				return entry;
			}
			entry.Address = table.Offset;
			// 元のゲームの曲数と、曲（BGM）が始まる番号はゲームごと（BgmCatalog）
			int originalSongs = BgmCatalog.OriginalSongCount;
			entry.Count = string.Format("ROM: {0} / 元: {1}", table.Count, originalSongs);
			if (table.Count != originalSongs)
			{
				entry.Add(RomTableEntry.Level.Info, table.Count > originalSongs
					? string.Format("元のゲームより {0} 曲多くなっています（曲が追加されています）", table.Count - originalSongs)
					: string.Format("元のゲームより {0} 曲少なくなっています", originalSongs - table.Count));
			}
			int replaced = 0;
			for (int id = BgmCatalog.FirstMusicId; id < Math.Min(table.Count, originalSongs); id++)
			{
				if (table.GetKind(id) == SongTable.SongKind.Replaced)
				{
					replaced++;
				}
			}
			if (replaced > 0)
			{
				entry.Add(RomTableEntry.Level.Info, string.Format("元の曲のうち {0} 曲が別の曲に差し替えられています", replaced));
			}
			if (entry.Notes.Count == 0)
			{
				entry.Notes.Add("問題なし");
			}
			return entry;
		}

		//-------------------------------------------------------------------------------
		// ini の値（「*0x144」ならその位置に書かれたポインタ経由）がどう場所を示しているかの説明
		//-------------------------------------------------------------------------------
		private static string DescribeSource(string raw)
		{
			raw = raw.Trim();
			if (raw.StartsWith("*", StringComparison.Ordinal))
			{
				return string.Format("0x{0} に書かれたポインタ経由", raw.Substring(1).Trim().Replace("0x", string.Empty).TrimStart('0').PadLeft(1, '0'));
			}
			if (raw.StartsWith("\"", StringComparison.Ordinal))
			{
				return "バイト列を探して、その後ろのポインタ経由";
			}
			if (raw.Equals("auto", StringComparison.OrdinalIgnoreCase))
			{
				return "ROM の中身から自動で判定";
			}
			return "固定の位置";
		}

		//-------------------------------------------------------------------------------
		// ini の値を ROM 上の位置に直す処理（直せなければ -1）
		// auto は、エディタが読み込みのときに ROM から求めた値（MapEditor の同じ名前の値）を使う
		//-------------------------------------------------------------------------------
		private static int ResolveOffset(byte[] rom, string raw, string key)
		{
			raw = raw.Trim();
			try
			{
				if (raw.Equals("auto", StringComparison.OrdinalIgnoreCase))
				{
					int detected = ReadEditorNumber(key);
					return detected > 0 ? detected : -1;
				}
				if (raw.StartsWith("\"", StringComparison.Ordinal))
				{
					return ResolveMarker(rom, raw);
				}
				bool indirect = raw.StartsWith("*", StringComparison.Ordinal);
				string number = indirect ? raw.Substring(1).Trim() : raw;
				int offset = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? int.Parse(number.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
					: int.Parse(number, CultureInfo.InvariantCulture);
				if (!indirect)
				{
					return offset;
				}
				if (offset < 0 || offset + 4 > rom.Length)
				{
					return -1;
				}
				long target = (long)BitConverter.ToUInt32(rom, offset) - 0x08000000L;
				return target >= 0 && target < rom.Length ? (int)target : -1;
			}
			catch (Exception)
			{
				return -1;
			}
		}

		//-------------------------------------------------------------------------------
		// 「"目印のバイト列", 距離」の指定を ROM 上の位置に直す処理（目印の直後から距離だけ進んだ所のポインタを読む。直せなければ -1）
		//-------------------------------------------------------------------------------
		private static int ResolveMarker(byte[] rom, string raw)
		{
			string[] parts = raw.Split(',');
			string hex = parts[0].Trim().Trim('"').Replace(" ", string.Empty);
			int distance = 0;
			if (parts.Length > 1)
			{
				string number = parts[1].Trim();
				distance = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? int.Parse(number.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
					: int.Parse(number, CultureInfo.InvariantCulture);
			}
			byte[] marker = new byte[hex.Length / 2];
			for (int i = 0; i < marker.Length; i++)
			{
				marker[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			if (marker.Length == 0)
			{
				return -1;
			}
			for (int at = 0; at + marker.Length <= rom.Length; at++)
			{
				int k = 0;
				while (k < marker.Length && rom[at + k] == marker[k])
				{
					k++;
				}
				if (k < marker.Length)
				{
					continue;
				}
				int pointerAt = at + marker.Length + distance;
				if (pointerAt < 0 || pointerAt + 4 > rom.Length)
				{
					return -1;
				}
				long target = (long)BitConverter.ToUInt32(rom, pointerAt) - 0x08000000L;
				return target >= 0 && target < rom.Length ? (int)target : -1;
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// ini の項目が auto かを返す処理
		//-------------------------------------------------------------------------------
		private static bool IsAuto(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}
			string raw = RomIniReader.ReadValue(key);
			return raw != null && raw.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase);
		}

		//-------------------------------------------------------------------------------
		// エディタが読み込みのときに求めた値（MapEditor の同じ名前の値）を読む処理（無ければ 0）
		//-------------------------------------------------------------------------------
		private static int ReadEditorNumber(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return 0;
			}
			System.Reflection.FieldInfo field = typeof(MapEditor).GetField(key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			return field != null && field.FieldType == typeof(int) ? (int)field.GetValue(null) : 0;
		}

		//-------------------------------------------------------------------------------
		// マップ名の表の件数を数える処理（1 件の中の名前へのポインタが ROM の中を指している間）
		//-------------------------------------------------------------------------------
		private static int CountMapNames(byte[] rom, int address)
		{
			int stride = Math.Max(4, MapEditor.MAP_NAME_ENTRY_SIZE);
			int count = 0;
			while (count < MaxCount && IsPointer(rom, address + count * stride + MapEditor.MAP_NAME_POINTER_OFFSET))
			{
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// ini の数値の項目を読む処理（項目が無い・読めなければ 0）
		//-------------------------------------------------------------------------------
		private static int ReadIniNumber(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return 0;
			}
			string raw = RomIniReader.ReadValue(key);
			if (string.IsNullOrEmpty(raw))
			{
				return 0;
			}
			raw = raw.Trim();
			try
			{
				return raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
					? int.Parse(raw.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
					: int.Parse(raw, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				return 0;
			}
		}

		//-------------------------------------------------------------------------------
		// ROM の中を指すポインタかを判定する処理
		//-------------------------------------------------------------------------------
		private static bool IsPointer(byte[] rom, int offset)
		{
			if (offset < 0 || offset + 4 > rom.Length)
			{
				return false;
			}
			uint pointer = BitConverter.ToUInt32(rom, offset);
			return pointer >= 0x08000000u && pointer - 0x08000000u < (uint)rom.Length;
		}

		//-------------------------------------------------------------------------------
		// 8 バイト（ポインタ＋番号 2 つ）の表で、番号が 1 ずつ増えていく間を数える処理（どちらの番号も増えなければ -1）
		//-------------------------------------------------------------------------------
		private static int CountTagged8(byte[] rom, int address)
		{
			if (address + 16 > rom.Length)
			{
				return -1;
			}
			int field;
			if (BitConverter.ToUInt16(rom, address + 12) == (ushort)(BitConverter.ToUInt16(rom, address + 4) + 1))
			{
				field = 4;
			}
			else if (BitConverter.ToUInt16(rom, address + 14) == (ushort)(BitConverter.ToUInt16(rom, address + 6) + 1))
			{
				field = 6;
			}
			else
			{
				return -1;
			}
			ushort start = BitConverter.ToUInt16(rom, address + field);
			int count = 0;
			while (count < MaxCount && address + count * 8 + 8 <= rom.Length && IsPointer(rom, address + count * 8)
				&& BitConverter.ToUInt16(rom, address + count * 8 + field) == (ushort)(start + count))
			{
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// ポインタが続く間を数える処理（pair なら 8 バイトの両方がポインタである間）
		//-------------------------------------------------------------------------------
		private static int CountPointers(byte[] rom, int address, int stride, bool pair)
		{
			int count = 0;
			while (count < MaxCount && IsPointer(rom, address + count * stride) && (!pair || IsPointer(rom, address + count * stride + 4)))
			{
				count++;
			}
			return count;
		}

		//-------------------------------------------------------------------------------
		// 出現ポケモンの表（20 バイト、バンクとマップが FF FF の項目で終わる）の件数を数える処理
		//-------------------------------------------------------------------------------
		private static int CountWildEntries(byte[] rom, int address)
		{
			for (int count = 0; count < MaxCount; count++)
			{
				int at = address + count * 20;
				if (at + 2 > rom.Length)
				{
					return -1;
				}
				if (rom[at] == 0xFF && rom[at + 1] == 0xFF)
				{
					return count;
				}
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// 一覧を、タブ区切りの文字にする処理（コピーして報告などに使う）
		//-------------------------------------------------------------------------------
		public static string ToText(IEnumerable<RomTableEntry> entries)
		{
			StringBuilder text = new StringBuilder();
			text.AppendLine("状態\t分類\t表\t使いみち\t場所\t読み方\t1項目\t件数\tメモ\tini の項目");
			foreach (RomTableEntry entry in entries)
			{
				text.AppendLine(string.Join("\t", StatusText(entry.Status), entry.Category, entry.Name, entry.Usage,
					entry.Address >= 0 ? string.Format("0x{0:X6}", entry.Address) : "-", entry.Source ?? "-", entry.EntrySize ?? "-", entry.Count ?? "-",
					string.Join(" / ", entry.Notes), entry.IniKey));
			}
			return text.ToString();
		}

		//-------------------------------------------------------------------------------
		// 状態を表す文字を返す処理
		//-------------------------------------------------------------------------------
		public static string StatusText(RomTableEntry.Level level)
		{
			switch (level)
			{
				case RomTableEntry.Level.Error:
					return "読めない";
				case RomTableEntry.Level.Warning:
					return "要確認";
				case RomTableEntry.Level.Info:
					return "変更あり";
				default:
					return "OK";
			}
		}
	}
}
