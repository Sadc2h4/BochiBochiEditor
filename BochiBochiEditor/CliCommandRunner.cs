using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// コマンドライン引数を解釈してCLI処理を実行する処理
	//-------------------------------------------------------------------------------
	internal static class CliCommandRunner
	{
		private const string DefaultCharTableFileName = "charmap.tbl";

		//-------------------------------------------------------------------------------
		// CLIモードとして実行するか判定して処理する処理
		//-------------------------------------------------------------------------------
		public static bool TryRun(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				return false;
			}
			string text = args[0].Trim();
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			try
			{
				Dictionary<string, string> dictionary = ParseOptions(args);
				if (!Execute(text, dictionary))
				{
					Console.Error.WriteLine("CLI error: 未知のコマンドです。");
					WriteText(BuildHelpText());
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("CLI error: " + ex.Message);
				return true;
			}
		}

		//-------------------------------------------------------------------------------
		// 指定コマンドを実行する処理
		//-------------------------------------------------------------------------------
		private static bool Execute(string command, Dictionary<string, string> options)
		{
			switch (command.ToLowerInvariant())
			{
			case "help":
			case "--help":
			case "-h":
				WriteText(BuildHelpText());
				return true;
			case "features":
				WriteOutput(GetFeatureInventory(), options);
				return true;
			case "rom-info":
				WriteOutput(GetRomInfo(RequireRom(options)), options);
				return true;
			case "find-free-space":
				WriteOutput(FindFreeSpace(RequireRom(options), RequireIntOption(options, "start"), RequireIntOption(options, "length")), options);
				return true;
			case "decode-text":
				WriteOutput(DecodeText(RequireRom(options), RequireIntOption(options, "offset"), RequireIntOption(options, "length")), options);
				return true;
			case "export-pokemon-names":
				WriteOutput(ExportPokemonNames(RequireRom(options)), options);
				return true;
			case "export-item-names":
				WriteOutput(ExportItemNames(RequireRom(options)), options);
				return true;
			case "export-move-names":
				WriteOutput(ExportMoveNames(RequireRom(options)), options);
				return true;
			case "export-trainer-class-names":
				WriteOutput(ExportTrainerClassNames(RequireRom(options)), options);
				return true;
			case "item-info":
				WriteOutput(GetItemInfo(RequireRom(options), RequireIntOption(options, "item")), options);
				return true;
			case "pokemon-stats":
				WriteOutput(CliDataEditor.GetPokemonStats(RequireRom(options), options), options);
				return true;
			case "export-pokemon-stats-csv":
				WriteOutput(CliDataEditor.ExportPokemonStatsCsv(RequireRom(options), options), options);
				return true;
			case "import-pokemon-stats-csv":
				WriteOutput(CliDataEditor.ImportPokemonStatsCsv(RequireRom(options), options), options);
				return true;
			case "update-pokemon-stats":
				WriteOutput(CliDataEditor.UpdatePokemonStats(RequireRom(options), options), options);
				return true;
			case "trainer-info":
				WriteOutput(CliDataEditor.GetTrainerInfo(RequireRom(options), options), options);
				return true;
			case "update-trainer":
				WriteOutput(CliDataEditor.UpdateTrainer(RequireRom(options), options), options);
				return true;
			case "import-images":
				WriteOutput(CliImageImporter.Import(RequireRom(options), options), options);
				return true;
			case "export-images":
				WriteOutput(CliImageImporter.Export(RequireRom(options), options), options);
				return true;
			case "export-image-sheet":
				WriteOutput(CliImageImporter.ExportSheet(RequireRom(options), options), options);
				return true;
			default:
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		// CLIオプションを解析する処理
		//-------------------------------------------------------------------------------
		private static Dictionary<string, string> ParseOptions(string[] args)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 1; i < args.Length; i++)
			{
				string text = args[i];
				if (!text.StartsWith("--", StringComparison.Ordinal))
				{
					continue;
				}
				string text2 = text.Substring(2);
				string text3 = "true";
				if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
				{
					text3 = args[i + 1];
					i++;
				}
				dictionary[text2] = text3;
			}
			return dictionary;
		}

		//-------------------------------------------------------------------------------
		// ROMを読み込んで共通初期化を行う処理
		//-------------------------------------------------------------------------------
		private static byte[] RequireRom(Dictionary<string, string> options)
		{
			string requiredOption = RequireOption(options, "rom");
			if (!File.Exists(requiredOption))
			{
				throw new FileNotFoundException("ROMファイルが見つかりません。", requiredOption);
			}
			byte[] array = File.ReadAllBytes(requiredOption);
			MainForm.romData = array;
			return array;
		}

		//-------------------------------------------------------------------------------
		// 文字テーブルを読み込む処理
		//-------------------------------------------------------------------------------
		private static void EnsureCharTableLoaded()
		{
			TextConverter.LoadCharTable(DefaultCharTableFileName);
		}

		//-------------------------------------------------------------------------------
		// 必須オプション文字列を取得する処理
		//-------------------------------------------------------------------------------
		private static string RequireOption(Dictionary<string, string> options, string key)
		{
			if (!options.TryGetValue(key, out string value) || string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException("--" + key + " を指定してください。");
			}
			return value;
		}

		//-------------------------------------------------------------------------------
		// 必須オプション数値を取得する処理
		//-------------------------------------------------------------------------------
		private static int RequireIntOption(Dictionary<string, string> options, string key)
		{
			string requiredOption = RequireOption(options, key);
			if (requiredOption.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return Convert.ToInt32(requiredOption.Substring(2), 16);
			}
			return Convert.ToInt32(requiredOption);
		}

		//-------------------------------------------------------------------------------
		// 機能棚卸情報を作成する処理
		//-------------------------------------------------------------------------------
		private static List<FeatureInventoryEntry> GetFeatureInventory()
		{
			return new List<FeatureInventoryEntry>
			{
				new FeatureInventoryEntry("ROM読込/保存", "メイン", "対応済み", "rom-info / decode-text / list系コマンドでROMを直接利用可能"),
				new FeatureInventoryEntry("空き領域検索", "メイン", "対応済み", "find-free-space で利用可能"),
				new FeatureInventoryEntry("ポケモン", "PokemonEditor", "一部対応", "名称一覧、種族値単体更新、CSV一括入出力に対応"),
				new FeatureInventoryEntry("TM/HM・教え技", "TmHmTutorEditor", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("タマゴ技", "EggMoveEditor", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("図鑑順", "PokedexOrderEditor", "未対応", "読取は可能だがCLI API未実装"),
				new FeatureInventoryEntry("生息地", "HabitatEditor", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("図鑑リスト", "PokedexListEditor", "未対応", "読取は可能だがCLI API未実装"),
				new FeatureInventoryEntry("アイテム", "ItemEditor", "一部対応", "名称一覧と個別アイテム情報の取得に対応"),
				new FeatureInventoryEntry("アイテム使用表示位置", "ItemUseCoordinate", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("トレーナー画像/肩書き", "TrainerSpriteEditor", "一部対応", "肩書き一覧の書き出しに対応"),
				new FeatureInventoryEntry("トレーナーデータ", "TrainerDataEditor", "一部対応", "trainer-info / update-trainer で基本情報と手持ち1枠更新に対応"),
				new FeatureInventoryEntry("NPCポケモン交換", "InGameTradeEditor", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("メール内容", "HeldItemMailEditor", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("マップ", "MapEditor", "未対応", "描画/画像/複合UI依存が強く段階的分離が必要"),
				new FeatureInventoryEntry("歩行グラフィック", "OverWorldEditor", "未対応", "画像UI依存が強く段階的分離が必要"),
				new FeatureInventoryEntry("野生ポケモン", "WildPokemonEditor", "未対応", "フォーム内部ロジックの分離が必要"),
				new FeatureInventoryEntry("タイルアニメ＆ドア", "MainFormボタンのみ", "未実装", "ボタンは存在するが接続先処理なし"),
				new FeatureInventoryEntry("タウンマップ", "MainFormボタンのみ", "未実装", "ボタンは存在するが接続先処理なし")
			};
		}

		//-------------------------------------------------------------------------------
		// ROM基本情報を取得する処理
		//-------------------------------------------------------------------------------
		private static object GetRomInfo(byte[] romData)
		{
			return new
			{
				Title = ReadAscii(romData, 160, 18).TrimEnd('\0', ' '),
				GameCode = ReadAscii(romData, 172, 4),
				MakerCode = ReadAscii(romData, 176, 2),
				UnitCode = romData[180],
				Version = romData[188],
				Size = romData.Length,
				Sha256 = Convert.ToHexString(SHA256.HashData(romData))
			};
		}

		//-------------------------------------------------------------------------------
		// 空き領域を検索する処理
		//-------------------------------------------------------------------------------
		private static object FindFreeSpace(byte[] romData, int startAddress, int length)
		{
			if (startAddress < 0 || startAddress >= romData.Length)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (length <= 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			int num = startAddress;
			int num2 = -1;
			while (num + length <= romData.Length)
			{
				bool flag = true;
				for (int i = 0; i < length; i++)
				{
					if (romData[num + i] != byte.MaxValue)
					{
						flag = false;
						num += i + 1;
						break;
					}
				}
				if (flag)
				{
					num2 = num;
					if (num2 % 4 != 0)
					{
						num2 = (num2 + 3) & -4;
					}
					break;
				}
			}
			return new
			{
				StartAddress = startAddress,
				Length = length,
				ResultAddress = (num2 >= 0) ? ("0x" + num2.ToString("X8")) : null,
				Found = (num2 >= 0)
			};
		}

		//-------------------------------------------------------------------------------
		// 指定範囲をポケモン文字列としてデコードする処理
		//-------------------------------------------------------------------------------
		private static object DecodeText(byte[] romData, int offset, int length)
		{
			EnsureCharTableLoaded();
			if (offset < 0 || offset >= romData.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			return new
			{
				Offset = "0x" + offset.ToString("X8"),
				Length = length,
				Text = TextConverter.BytesToPokemonString(romData, offset, length)
			};
		}

		//-------------------------------------------------------------------------------
		// ポケモン名一覧を取得する処理
		//-------------------------------------------------------------------------------
		private static List<ListEntry> ExportPokemonNames(byte[] romData)
		{
			EnsureCharTableLoaded();
			int num = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_OFFSET");
			int num2 = RomIniReader.ReadHexOrDecimal("POKEMON_NAME_LENGTH");
			int num3 = RomIniReader.ReadHexOrDecimal("TOTAL_POKEMON_COUNT");
			List<ListEntry> list = new List<ListEntry>();
			for (int i = 0; i < num3; i++)
			{
				int num4 = num + i * num2;
				list.Add(new ListEntry(i, TextConverter.BytesToPokemonString(romData, num4, num2)));
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		// アイテム名一覧を取得する処理
		//-------------------------------------------------------------------------------
		private static List<ListEntry> ExportItemNames(byte[] romData)
		{
			EnsureCharTableLoaded();
			List<string> itemNames = ItemData.GetItemNames(romData);
			List<ListEntry> list = new List<ListEntry>();
			for (int i = 0; i < itemNames.Count; i++)
			{
				list.Add(new ListEntry(i, itemNames[i]));
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		// 技名一覧を取得する処理
		//-------------------------------------------------------------------------------
		private static List<ListEntry> ExportMoveNames(byte[] romData)
		{
			EnsureCharTableLoaded();
			List<string> moveNames = MoveData.GetMoveNames(romData);
			List<ListEntry> list = new List<ListEntry>();
			for (int i = 0; i < moveNames.Count; i++)
			{
				list.Add(new ListEntry(i, moveNames[i]));
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		// トレーナー肩書き一覧を取得する処理
		//-------------------------------------------------------------------------------
		private static List<ListEntry> ExportTrainerClassNames(byte[] romData)
		{
			EnsureCharTableLoaded();
			int num = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_TABLE_OFFSET");
			int num2 = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_LENGTH");
			int num3 = RomIniReader.ReadHexOrDecimal("TRAINER_CLASS_NAME_COUNT");
			List<ListEntry> list = new List<ListEntry>();
			for (int i = 0; i < num3; i++)
			{
				int num4 = num + i * num2;
				list.Add(new ListEntry(i, TextConverter.BytesToPokemonString(romData, num4, num2)));
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		// 個別アイテム情報を取得する処理
		//-------------------------------------------------------------------------------
		private static object GetItemInfo(byte[] romData, int itemIndex)
		{
			EnsureCharTableLoaded();
			ItemData.ItemInfo itemInfo = ItemData.GetItemInfo(romData, checked((ushort)itemIndex));
			return new
			{
				Index = (int)itemInfo.Index,
				itemInfo.Name,
				itemInfo.ItemId,
				itemInfo.Price,
				itemInfo.HeldEffectId,
				itemInfo.EffectValue,
				DescriptionAddress = "0x" + itemInfo.DescriptionAddress.ToString("X8"),
				itemInfo.CanHold,
				itemInfo.UnknownValue,
				itemInfo.PocketId,
				itemInfo.FieldUseType,
				FieldUseAddress = "0x" + itemInfo.FieldUseAddress.ToString("X8"),
				itemInfo.BattleUseType,
				BattleUseAddress = "0x" + itemInfo.BattleUseAddress.ToString("X8"),
				itemInfo.SpecialValue,
				ImageAddress = "0x" + itemInfo.ImageAddress.ToString("X8"),
				PaletteAddress = "0x" + itemInfo.PaletteAddress.ToString("X8")
			};
		}

		//-------------------------------------------------------------------------------
		// 出力形式に応じて結果を書き出す処理
		//-------------------------------------------------------------------------------
		private static void WriteOutput(object value, Dictionary<string, string> options)
		{
			string text = options.ContainsKey("format") ? options["format"] : "json";
			string text2;
			if (string.Equals(text, "text", StringComparison.OrdinalIgnoreCase))
			{
				text2 = ConvertToText(value);
			}
			else
			{
				text2 = JsonSerializer.Serialize(value, new JsonSerializerOptions
				{
					WriteIndented = true
				});
			}
			WriteText(text2, options);
		}

		//-------------------------------------------------------------------------------
		// 文字列を標準出力またはファイルへ出力する処理
		//-------------------------------------------------------------------------------
		private static void WriteText(string text, Dictionary<string, string> options = null)
		{
			if (options != null && options.TryGetValue("out", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				string directoryName = Path.GetDirectoryName(Path.GetFullPath(value));
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.WriteAllText(value, text, new UTF8Encoding(false));
				Console.WriteLine(value);
				return;
			}
			Console.WriteLine(text);
		}

		//-------------------------------------------------------------------------------
		// 任意オブジェクトをテキスト形式へ変換する処理
		//-------------------------------------------------------------------------------
		private static string ConvertToText(object value)
		{
			if (value is IEnumerable<ListEntry> enumerable)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ListEntry listEntry in enumerable)
				{
					stringBuilder.Append(listEntry.Index);
					stringBuilder.Append('\t');
					stringBuilder.AppendLine(listEntry.Name);
				}
				return stringBuilder.ToString();
			}
			if (value is System.Collections.IEnumerable enumerable2 && value is not string)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				foreach (object obj in enumerable2)
				{
					stringBuilder2.AppendLine(ConvertObjectToLine(obj));
				}
				return stringBuilder2.ToString();
			}
			return ConvertObjectToLine(value);
		}

		//-------------------------------------------------------------------------------
		// 任意オブジェクトを1行テキストへ変換する処理
		//-------------------------------------------------------------------------------
		private static string ConvertObjectToLine(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			if (properties.Length == 0)
			{
				return value.ToString();
			}
			return string.Join("\t", properties.Select((PropertyInfo p) => p.Name + "=" + Convert.ToString(p.GetValue(value))));
		}

		//-------------------------------------------------------------------------------
		// 固定長ASCIIを抽出する処理
		//-------------------------------------------------------------------------------
		private static string ReadAscii(byte[] bytes, int offset, int length)
		{
			byte[] array = new byte[length];
			Array.Copy(bytes, offset, array, 0, length);
			return Encoding.ASCII.GetString(array);
		}

		//-------------------------------------------------------------------------------
		// CLIヘルプ文を作成する処理
		//-------------------------------------------------------------------------------
		private static string BuildHelpText()
		{
			return string.Join(Environment.NewLine, new string[]
			{
				"BochiBochiEditor CLI",
				"  help",
				"  features [--format json|text] [--out path]",
				"  rom-info --rom path [--format json|text] [--out path]",
				"  find-free-space --rom path --start 0x08000000 --length 2048 [--format json|text]",
				"  decode-text --rom path --offset 0x123456 --length 16 [--format json|text]",
				"  export-pokemon-names --rom path [--format json|text] [--out path]",
				"  export-item-names --rom path [--format json|text] [--out path]",
				"  export-move-names --rom path [--format json|text] [--out path]",
				"  export-trainer-class-names --rom path [--format json|text] [--out path]",
				"  item-info --rom path --item 1 [--format json|text] [--out path]",
				"  pokemon-stats --rom path --pokemon 25 [--format json|text] [--out path]",
				"  export-pokemon-stats-csv --rom path --csv-out path [--format json|text] [--out path]",
				"  import-pokemon-stats-csv --rom path --csv path --out-rom path [--format json|text] [--out path]",
				"  update-pokemon-stats --rom path --pokemon 25 [--hp 80 --attack 100 --defense 90 --speed 70 --sp-attack 110 --sp-defense 80 --type1 3 --type2 8 --ability1 65 --ability2 66 --hidden-ability 34 --hold-item1 1 --hold-item2 2 --catch-rate 45 --base-exp 200 --ev-hp 0 --ev-attack 2 --ev-defense 0 --ev-sp-attack 1 --ev-sp-defense 0 --ev-speed 0 --gender-value 127 --egg-step 20 --egg-group1 1 --egg-group2 1 --base-happiness 70 --growth-rate 3 --run-rate 5 --color 4 --flip 0 --out-rom path] [--format json|text] [--out path]",
				"  trainer-info --rom path --trainer 1 [--format json|text] [--out path]",
				"  update-trainer --rom path --trainer 1 [--class 3 --intro-music 12 --sprite 8 --name テスト --item1 1 --item2 2 --item3 0 --item4 0 --double-battle true --ai 7 --unknown 0 --data-type 3 --pokemon-count 2 --start 0x08700000|--pokemon-data-address 0x08700000 --slot 1 --slot-pokemon 25 --slot-level 50 --slot-iv 31 --slot-item 0 --slot-move1 85 --slot-move2 98 --slot-move3 0 --slot-move4 0 --out-rom path] [--format json|text] [--out path]",
				"  import-images --rom path --target pokemon-sprite|item-image|pokemon-icon|trainer-image --source-dir path --start 0x0A000000 [--vanilla|--neworder|--order vanilla|neworder] [--out-rom path] [--log-out path] [--icon-palette-id 0] [--format json|text] [--out path]",
				"  export-images --rom path --target pokemon-sprite|item-image|pokemon-icon|trainer-image --source-dir path [--vanilla|--neworder|--order vanilla|neworder] [--format json|text] [--out path]",
				"  export-image-sheet --rom path --target pokemon-sprite|item-image|pokemon-icon|trainer-image --sheet-out path [--variant front-normal|front-shiny|back-normal|back-shiny|frame1|frame2|full] [--vanilla|--neworder|--order vanilla|neworder] [--format json|text] [--out path]"
			});
		}

		private sealed class FeatureInventoryEntry
		{
			public FeatureInventoryEntry(string featureName, string source, string cliStatus, string notes)
			{
				this.FeatureName = featureName;
				this.Source = source;
				this.CliStatus = cliStatus;
				this.Notes = notes;
			}

			public string FeatureName { get; }

			public string Source { get; }

			public string CliStatus { get; }

			public string Notes { get; }
		}

		private sealed class ListEntry
		{
			public ListEntry(int index, string name)
			{
				this.Index = index;
				this.Name = name;
			}

			public int Index { get; }

			public string Name { get; }
		}
	}
}
