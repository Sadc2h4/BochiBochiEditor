using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// CLI向けのトレーナーデータ/種族値データ操作をまとめて実行する処理
	//-------------------------------------------------------------------------------
	internal static class CliDataEditor
	{
		private const uint GbaRomPointerBase = 134217728U;
		private const int TrainerDataTypeBasic = 0;
		private const int TrainerDataTypeMoves = 1;
		private const int TrainerDataTypeItem = 2;
		private const int TrainerDataTypeItemMoves = 3;
		private static readonly Dictionary<string, int> iniIntCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, bool> iniBoolCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private static byte[] cachedItemNameRom;
		private static List<string> cachedItemNames;
		private static byte[] cachedMoveNameRom;
		private static List<string> cachedMoveNames;
		private static readonly string[] PokemonStatsCsvHeaders = new string[]
		{
			"Index", "Name", "Hp", "Attack", "Defense", "Speed", "SpAttack", "SpDefense",
			"Type1", "Type2", "CatchRate", "BaseExp",
			"EvHp", "EvAttack", "EvDefense", "EvSpAttack", "EvSpDefense", "EvSpeed",
			"HoldItem1", "HoldItem2", "GenderValue", "EggStep", "BaseHappiness", "GrowthRate",
			"EggGroup1", "EggGroup2", "Ability1", "Ability2", "RunRate", "Color", "Flip", "HiddenAbility"
		};

		//-------------------------------------------------------------------------------
		// ポケモン種族値情報を取得する処理
		//-------------------------------------------------------------------------------
		public static PokemonStatsRecord GetPokemonStats(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			int pokemonId = RequireIntOption(options, "pokemon");
			return ReadPokemonStats(romData, pokemonId);
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値情報を更新する処理
		//-------------------------------------------------------------------------------
		public static PokemonStatsUpdateResult UpdatePokemonStats(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			int pokemonId = RequireIntOption(options, "pokemon");
			PokemonStatsRecord before = ReadPokemonStats(romData, pokemonId);
			PokemonStatsRecord after = ClonePokemonStats(before);
			ApplyPokemonStatUpdates(after, options);
			if (!after.HasChangesComparedTo(before))
			{
				throw new InvalidOperationException("更新対象のオプションが指定されていません。");
			}
			WritePokemonStats(romData, after);
			MainForm.romData = romData;
			string romOutputPath = GetRomOutputPath(options, RequireOption(options, "rom"));
			File.WriteAllBytes(romOutputPath, romData);
			return new PokemonStatsUpdateResult
			{
				Target = "pokemon-stats",
				Pokemon = pokemonId,
				Name = after.Name,
				OutputRomPath = romOutputPath,
				Before = before,
				After = after
			};
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値一覧をCSVへ書き出す処理
		//-------------------------------------------------------------------------------
		public static CsvExportResult ExportPokemonStatsCsv(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			string requiredOption = RequireOption(options, "csv-out");
			int totalPokemonCount = ReadIniInt("TOTAL_POKEMON_COUNT");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Join(",", PokemonStatsCsvHeaders));
			for (int i = 0; i < totalPokemonCount; i++)
			{
				PokemonStatsRecord pokemonStatsRecord = ReadPokemonStats(romData, i);
				stringBuilder.AppendLine(BuildCsvLine(new string[]
				{
					pokemonStatsRecord.Index.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Name,
					pokemonStatsRecord.Hp.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Attack.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Defense.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Speed.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.SpAttack.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.SpDefense.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Type1.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Type2.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.CatchRate.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.BaseExp.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EvHp.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EvAttack.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EvDefense.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EvSpAttack.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EvSpDefense.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EvSpeed.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.HoldItem1.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.HoldItem2.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.GenderValue.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EggStep.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.BaseHappiness.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.GrowthRate.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EggGroup1.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.EggGroup2.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Ability1.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Ability2.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.RunRate.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Color.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.Flip.ToString(CultureInfo.InvariantCulture),
					pokemonStatsRecord.HiddenAbility.ToString(CultureInfo.InvariantCulture)
				}));
			}
			WriteUtf8File(requiredOption, stringBuilder.ToString());
			return new CsvExportResult
			{
				Target = "pokemon-stats-csv",
				OutputPath = Path.GetFullPath(requiredOption),
				RowCount = totalPokemonCount
			};
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値一覧をCSVから一括更新する処理
		//-------------------------------------------------------------------------------
		public static PokemonStatsCsvImportResult ImportPokemonStatsCsv(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			string requiredOption = RequireOption(options, "csv");
			if (!File.Exists(requiredOption))
			{
				throw new FileNotFoundException("CSVファイルが見つかりません。", requiredOption);
			}
			string[] lines = File.ReadAllLines(requiredOption, Encoding.UTF8);
			if (lines.Length <= 1)
			{
				throw new InvalidOperationException("CSVにデータ行がありません。");
			}
			Dictionary<string, int> headerIndexMap = BuildCsvHeaderIndexMap(lines[0]);
			ValidateCsvHeaders(headerIndexMap, PokemonStatsCsvHeaders);
			List<PokemonStatsCsvChange> list = new List<PokemonStatsCsvChange>();
			for (int i = 1; i < lines.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(lines[i]))
				{
					continue;
				}
				string[] array = ParseCsvLine(lines[i]);
				int num = ParseCsvInt(array, headerIndexMap, "Index");
				PokemonStatsRecord pokemonStatsRecord = ReadPokemonStats(romData, num);
				PokemonStatsRecord pokemonStatsRecord2 = ClonePokemonStats(pokemonStatsRecord);
				ApplyPokemonStatsCsvRow(array, headerIndexMap, pokemonStatsRecord2);
				if (!pokemonStatsRecord2.HasChangesComparedTo(pokemonStatsRecord))
				{
					continue;
				}
				WritePokemonStats(romData, pokemonStatsRecord2);
				list.Add(new PokemonStatsCsvChange
				{
					Index = num,
					Name = pokemonStatsRecord2.Name,
					BeforeHp = pokemonStatsRecord.Hp,
					AfterHp = pokemonStatsRecord2.Hp,
					BeforeAttack = pokemonStatsRecord.Attack,
					AfterAttack = pokemonStatsRecord2.Attack,
					BeforeDefense = pokemonStatsRecord.Defense,
					AfterDefense = pokemonStatsRecord2.Defense,
					BeforeSpeed = pokemonStatsRecord.Speed,
					AfterSpeed = pokemonStatsRecord2.Speed
				});
			}
			MainForm.romData = romData;
			string romOutputPath = GetRomOutputPath(options, RequireOption(options, "rom"));
			File.WriteAllBytes(romOutputPath, romData);
			return new PokemonStatsCsvImportResult
			{
				Target = "pokemon-stats-csv",
				InputCsvPath = Path.GetFullPath(requiredOption),
				OutputRomPath = romOutputPath,
				ChangedCount = list.Count,
				Changes = list
			};
		}

		//-------------------------------------------------------------------------------
		// トレーナーデータ情報を取得する処理
		//-------------------------------------------------------------------------------
		public static TrainerRecord GetTrainerInfo(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			int trainerId = RequireIntOption(options, "trainer");
			return ReadTrainer(romData, trainerId);
		}

		//-------------------------------------------------------------------------------
		// トレーナーデータ情報を更新する処理
		//-------------------------------------------------------------------------------
		public static TrainerUpdateResult UpdateTrainer(byte[] romData, Dictionary<string, string> options)
		{
			TextConverter.LoadCharTable("charmap.tbl");
			int trainerId = RequireIntOption(options, "trainer");
			TrainerRecord before = ReadTrainer(romData, trainerId);
			TrainerRecord after = CloneTrainer(before);
			ApplyTrainerHeaderUpdates(after, options);
			ApplyTrainerStructureUpdates(romData, before, after, options);
			ApplyTrainerSlotUpdates(after, options);
			if (!after.HasChangesComparedTo(before))
			{
				throw new InvalidOperationException("更新対象のオプションが指定されていません。");
			}
			WriteTrainer(romData, after);
			MainForm.romData = romData;
			string romOutputPath = GetRomOutputPath(options, RequireOption(options, "rom"));
			File.WriteAllBytes(romOutputPath, romData);
			return new TrainerUpdateResult
			{
				Target = "trainer-data",
				Trainer = trainerId,
				Name = after.Name,
				OutputRomPath = romOutputPath,
				Before = before,
				After = after
			};
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値1件をROMから読み出す処理
		//-------------------------------------------------------------------------------
		private static PokemonStatsRecord ReadPokemonStats(byte[] romData, int pokemonId)
		{
			int totalPokemonCount = ReadIniInt("TOTAL_POKEMON_COUNT");
			ValidateRange(pokemonId, 0, totalPokemonCount - 1, "pokemon");
			int baseStatsOffset = ReadIniInt("BASE_STATS_OFFSET");
			bool enableBaseStatsExpansion = ReadIniBool("ENABLE_BASE_STATS_EXPANSION");
			int baseStatsEntryLength = enableBaseStatsExpansion ? 32 : 28;
			int offset = checked(baseStatsOffset + pokemonId * baseStatsEntryLength);
			ValidateSlice(romData, offset, baseStatsEntryLength, "pokemon stats");
			ushort evValue = BitConverter.ToUInt16(romData, offset + 10);
			byte[] evs = DecodeEv(evValue);
			byte colorAndFlip = romData[offset + 25];
			int ability1 = enableBaseStatsExpansion ? BitConverter.ToUInt16(romData, offset + 22) : romData[offset + 22];
			int ability2 = enableBaseStatsExpansion ? BitConverter.ToUInt16(romData, offset + 26) : romData[offset + 23];
			int hiddenAbility = enableBaseStatsExpansion ? BitConverter.ToUInt16(romData, offset + 28) : romData[offset + 26];
			int baseExp = enableBaseStatsExpansion ? BitConverter.ToUInt16(romData, offset + 30) : romData[offset + 9];
			return new PokemonStatsRecord
			{
				Index = pokemonId,
				Name = ReadPokemonName(romData, pokemonId),
				Offset = FormatOffset(offset),
				EntryLength = baseStatsEntryLength,
				EnableBaseStatsExpansion = enableBaseStatsExpansion,
				Hp = romData[offset + 0],
				Attack = romData[offset + 1],
				Defense = romData[offset + 2],
				Speed = romData[offset + 3],
				SpAttack = romData[offset + 4],
				SpDefense = romData[offset + 5],
				Type1 = romData[offset + 6],
				Type1Name = ReadTypeName(romData, romData[offset + 6]),
				Type2 = romData[offset + 7],
				Type2Name = ReadTypeName(romData, romData[offset + 7]),
				CatchRate = romData[offset + 8],
				BaseExp = baseExp,
				EvHp = evs[0],
				EvAttack = evs[1],
				EvDefense = evs[2],
				EvSpAttack = evs[3],
				EvSpDefense = evs[4],
				EvSpeed = evs[5],
				HoldItem1 = BitConverter.ToUInt16(romData, offset + 12),
				HoldItem1Name = ReadItemName(romData, BitConverter.ToUInt16(romData, offset + 12)),
				HoldItem2 = BitConverter.ToUInt16(romData, offset + 14),
				HoldItem2Name = ReadItemName(romData, BitConverter.ToUInt16(romData, offset + 14)),
				GenderValue = romData[offset + 16],
				GenderLabel = GetGenderLabel(romData[offset + 16]),
				EggStep = romData[offset + 17],
				EggStepLabel = GetEggStepLabel(romData[offset + 17]),
				BaseHappiness = romData[offset + 18],
				GrowthRate = romData[offset + 19],
				GrowthRateLabel = GetGrowthRateLabel(romData[offset + 19]),
				EggGroup1 = romData[offset + 20],
				EggGroup1Label = GetEggGroupLabel(romData[offset + 20]),
				EggGroup2 = romData[offset + 21],
				EggGroup2Label = GetEggGroupLabel(romData[offset + 21]),
				Ability1 = ability1,
				Ability1Name = ReadAbilityName(romData, ability1),
				Ability2 = ability2,
				Ability2Name = ReadAbilityName(romData, ability2),
				RunRate = romData[offset + 24],
				Color = (byte)(colorAndFlip & 15),
				ColorLabel = GetColorLabel((byte)(colorAndFlip & 15)),
				Flip = (byte)((colorAndFlip & 128) >> 7),
				FlipLabel = GetFlipLabel((byte)((colorAndFlip & 128) >> 7)),
				HiddenAbility = hiddenAbility,
				HiddenAbilityName = ReadAbilityName(romData, hiddenAbility)
			};
		}

		//-------------------------------------------------------------------------------
		// トレーナーデータ1件をROMから読み出す処理
		//-------------------------------------------------------------------------------
		private static TrainerRecord ReadTrainer(byte[] romData, int trainerId)
		{
			int trainerDataOffset = ReadIniInt("TRAINER_DATA_OFFSET");
			int trainerDataLength = ReadIniInt("TRAINER_DATA_LENGTH");
			int trainerEntryCount = ReadIniInt("TRAINER_ENTRY_COUNT");
			int trainerNameLength = ReadIniInt("TRAINER_NAME_LENGTH");
			ValidateRange(trainerId, 0, trainerEntryCount - 1, "trainer");
			int offset = checked(trainerDataOffset + trainerId * trainerDataLength);
			ValidateSlice(romData, offset, trainerDataLength, "trainer");
			TrainerRecord trainerRecord = new TrainerRecord();
			trainerRecord.Index = trainerId;
			trainerRecord.Offset = FormatOffset(offset);
			trainerRecord.DataType = romData[offset + 0];
			trainerRecord.ClassId = romData[offset + 1];
			trainerRecord.ClassName = ReadTrainerClassName(romData, romData[offset + 1]);
			trainerRecord.IntroMusic = romData[offset + 2];
			trainerRecord.SpriteId = romData[offset + 3];
			trainerRecord.Name = TextConverter.BytesToPokemonString(romData, offset + 4, trainerNameLength);
			trainerRecord.Items = new ushort[4];
			trainerRecord.ItemNames = new string[4];
			for (int i = 0; i < 4; i++)
			{
				ushort num = BitConverter.ToUInt16(romData, offset + 10 + i * 2);
				trainerRecord.Items[i] = num;
				trainerRecord.ItemNames[i] = ReadItemName(romData, num);
			}
			trainerRecord.IsDoubleBattle = (romData[offset + 18] & 1) != 0;
			trainerRecord.Ai = romData[offset + 20];
			trainerRecord.PokemonCount = romData[offset + 24];
			trainerRecord.UnknownValue = BitConverter.ToUInt16(romData, offset + 26);
			uint rawPointer = BitConverter.ToUInt32(romData, offset + 28);
			trainerRecord.PokemonDataAddress = rawPointer >= GbaRomPointerBase ? rawPointer - GbaRomPointerBase : 0U;
			trainerRecord.PokemonDataAddressText = FormatOffset((int)trainerRecord.PokemonDataAddress);
			trainerRecord.Party = ReadTrainerParty(romData, trainerRecord);
			return trainerRecord;
		}

		//-------------------------------------------------------------------------------
		// トレーナー手持ちデータ一覧を読み出す処理
		//-------------------------------------------------------------------------------
		private static List<TrainerPokemonSlotRecord> ReadTrainerParty(byte[] romData, TrainerRecord trainer)
		{
			List<TrainerPokemonSlotRecord> list = new List<TrainerPokemonSlotRecord>();
			int pokemonSlotSize = GetPokemonSlotSize(trainer.DataType);
			for (int i = 0; i < trainer.PokemonCount; i++)
			{
				int offset = checked((int)trainer.PokemonDataAddress + i * pokemonSlotSize);
				ValidateSlice(romData, offset, pokemonSlotSize, "trainer party");
				TrainerPokemonSlotRecord trainerPokemonSlotRecord = new TrainerPokemonSlotRecord();
				trainerPokemonSlotRecord.Slot = i + 1;
				trainerPokemonSlotRecord.Offset = FormatOffset(offset);
				trainerPokemonSlotRecord.Iv = romData[offset + 0];
				trainerPokemonSlotRecord.UnknownValue1 = romData[offset + 1];
				trainerPokemonSlotRecord.Level = romData[offset + 2];
				trainerPokemonSlotRecord.UnknownValue2 = romData[offset + 3];
				trainerPokemonSlotRecord.PokemonCode = BitConverter.ToUInt16(romData, offset + 4);
				trainerPokemonSlotRecord.PokemonName = ReadPokemonName(romData, trainerPokemonSlotRecord.PokemonCode);
				trainerPokemonSlotRecord.ItemCode = 0;
				trainerPokemonSlotRecord.ItemName = string.Empty;
				trainerPokemonSlotRecord.Moves = new ushort[4];
				trainerPokemonSlotRecord.MoveNames = new string[4];
				switch (trainer.DataType)
				{
				case TrainerDataTypeMoves:
					for (int j = 0; j < 4; j++)
					{
						ushort num = BitConverter.ToUInt16(romData, offset + 6 + j * 2);
						trainerPokemonSlotRecord.Moves[j] = num;
						trainerPokemonSlotRecord.MoveNames[j] = ReadMoveName(romData, num);
					}
					break;
				case TrainerDataTypeItem:
					trainerPokemonSlotRecord.ItemCode = BitConverter.ToUInt16(romData, offset + 6);
					trainerPokemonSlotRecord.ItemName = ReadItemName(romData, trainerPokemonSlotRecord.ItemCode);
					break;
				case TrainerDataTypeItemMoves:
					trainerPokemonSlotRecord.ItemCode = BitConverter.ToUInt16(romData, offset + 6);
					trainerPokemonSlotRecord.ItemName = ReadItemName(romData, trainerPokemonSlotRecord.ItemCode);
					for (int k = 0; k < 4; k++)
					{
						ushort num2 = BitConverter.ToUInt16(romData, offset + 8 + k * 2);
						trainerPokemonSlotRecord.Moves[k] = num2;
						trainerPokemonSlotRecord.MoveNames[k] = ReadMoveName(romData, num2);
					}
					break;
				default:
					for (int l = 0; l < 4; l++)
					{
						trainerPokemonSlotRecord.MoveNames[l] = string.Empty;
					}
					break;
				}
				list.Add(trainerPokemonSlotRecord);
			}
			return list;
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値の更新内容を反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyPokemonStatUpdates(PokemonStatsRecord record, Dictionary<string, string> options)
		{
			byte hp = record.Hp;
			byte attack = record.Attack;
			byte defense = record.Defense;
			byte speed = record.Speed;
			byte spAttack = record.SpAttack;
			byte spDefense = record.SpDefense;
			byte type1 = record.Type1;
			byte type2 = record.Type2;
			byte catchRate = record.CatchRate;
			int baseExp = record.BaseExp;
			byte evHp = record.EvHp;
			byte evAttack = record.EvAttack;
			byte evDefense = record.EvDefense;
			byte evSpAttack = record.EvSpAttack;
			byte evSpDefense = record.EvSpDefense;
			byte evSpeed = record.EvSpeed;
			ushort holdItem1 = record.HoldItem1;
			ushort holdItem2 = record.HoldItem2;
			byte genderValue = record.GenderValue;
			byte eggStep = record.EggStep;
			byte baseHappiness = record.BaseHappiness;
			byte growthRate = record.GrowthRate;
			byte eggGroup1 = record.EggGroup1;
			byte eggGroup2 = record.EggGroup2;
			int ability1 = record.Ability1;
			int ability2 = record.Ability2;
			byte runRate = record.RunRate;
			byte color = record.Color;
			byte flip = record.Flip;
			int hiddenAbility = record.HiddenAbility;
			ApplyByteOption(options, "hp", ref hp);
			ApplyByteOption(options, "attack", ref attack);
			ApplyByteOption(options, "defense", ref defense);
			ApplyByteOption(options, "speed", ref speed);
			ApplyByteOption(options, "sp-attack", ref spAttack);
			ApplyByteOption(options, "sp-defense", ref spDefense);
			ApplyByteOption(options, "type1", ref type1);
			ApplyByteOption(options, "type2", ref type2);
			ApplyByteOption(options, "catch-rate", ref catchRate);
			ApplyIntOption(options, "base-exp", 0, 65535, ref baseExp);
			ApplyByteOption(options, "ev-hp", 0, 3, ref evHp);
			ApplyByteOption(options, "ev-attack", 0, 3, ref evAttack);
			ApplyByteOption(options, "ev-defense", 0, 3, ref evDefense);
			ApplyByteOption(options, "ev-sp-attack", 0, 3, ref evSpAttack);
			ApplyByteOption(options, "ev-sp-defense", 0, 3, ref evSpDefense);
			ApplyByteOption(options, "ev-speed", 0, 3, ref evSpeed);
			ApplyUShortOption(options, "hold-item1", ref holdItem1);
			ApplyUShortOption(options, "hold-item2", ref holdItem2);
			ApplyByteOption(options, "gender-value", ref genderValue);
			ApplyByteOption(options, "egg-step", ref eggStep);
			ApplyByteOption(options, "base-happiness", ref baseHappiness);
			ApplyByteOption(options, "growth-rate", ref growthRate);
			ApplyByteOption(options, "egg-group1", ref eggGroup1);
			ApplyByteOption(options, "egg-group2", ref eggGroup2);
			ApplyIntOption(options, "ability1", 0, 65535, ref ability1);
			ApplyIntOption(options, "ability2", 0, 65535, ref ability2);
			ApplyByteOption(options, "run-rate", ref runRate);
			ApplyByteOption(options, "color", 0, 15, ref color);
			ApplyByteOption(options, "flip", 0, 1, ref flip);
			ApplyIntOption(options, "hidden-ability", 0, 65535, ref hiddenAbility);
			record.Hp = hp;
			record.Attack = attack;
			record.Defense = defense;
			record.Speed = speed;
			record.SpAttack = spAttack;
			record.SpDefense = spDefense;
			record.Type1 = type1;
			record.Type2 = type2;
			record.CatchRate = catchRate;
			record.BaseExp = baseExp;
			record.EvHp = evHp;
			record.EvAttack = evAttack;
			record.EvDefense = evDefense;
			record.EvSpAttack = evSpAttack;
			record.EvSpDefense = evSpDefense;
			record.EvSpeed = evSpeed;
			record.HoldItem1 = holdItem1;
			record.HoldItem2 = holdItem2;
			record.GenderValue = genderValue;
			record.EggStep = eggStep;
			record.BaseHappiness = baseHappiness;
			record.GrowthRate = growthRate;
			record.EggGroup1 = eggGroup1;
			record.EggGroup2 = eggGroup2;
			record.Ability1 = ability1;
			record.Ability2 = ability2;
			record.RunRate = runRate;
			record.Color = color;
			record.Flip = flip;
			record.HiddenAbility = hiddenAbility;
			record.Type1Name = ReadTypeName(MainForm.romData, record.Type1);
			record.Type2Name = ReadTypeName(MainForm.romData, record.Type2);
			record.HoldItem1Name = ReadItemName(MainForm.romData, record.HoldItem1);
			record.HoldItem2Name = ReadItemName(MainForm.romData, record.HoldItem2);
			record.GenderLabel = GetGenderLabel(record.GenderValue);
			record.EggStepLabel = GetEggStepLabel(record.EggStep);
			record.GrowthRateLabel = GetGrowthRateLabel(record.GrowthRate);
			record.EggGroup1Label = GetEggGroupLabel(record.EggGroup1);
			record.EggGroup2Label = GetEggGroupLabel(record.EggGroup2);
			record.Ability1Name = ReadAbilityName(MainForm.romData, record.Ability1);
			record.Ability2Name = ReadAbilityName(MainForm.romData, record.Ability2);
			record.ColorLabel = GetColorLabel(record.Color);
			record.FlipLabel = GetFlipLabel(record.Flip);
			record.HiddenAbilityName = ReadAbilityName(MainForm.romData, record.HiddenAbility);
		}

		//-------------------------------------------------------------------------------
		// CSV1行の内容をポケモン種族値へ反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyPokemonStatsCsvRow(string[] values, Dictionary<string, int> headerIndexMap, PokemonStatsRecord record)
		{
			record.Hp = ParseCsvByte(values, headerIndexMap, "Hp");
			record.Attack = ParseCsvByte(values, headerIndexMap, "Attack");
			record.Defense = ParseCsvByte(values, headerIndexMap, "Defense");
			record.Speed = ParseCsvByte(values, headerIndexMap, "Speed");
			record.SpAttack = ParseCsvByte(values, headerIndexMap, "SpAttack");
			record.SpDefense = ParseCsvByte(values, headerIndexMap, "SpDefense");
			record.Type1 = ParseCsvByte(values, headerIndexMap, "Type1");
			record.Type2 = ParseCsvByte(values, headerIndexMap, "Type2");
			record.CatchRate = ParseCsvByte(values, headerIndexMap, "CatchRate");
			record.BaseExp = ParseCsvInt(values, headerIndexMap, "BaseExp");
			record.EvHp = ParseCsvByte(values, headerIndexMap, "EvHp");
			record.EvAttack = ParseCsvByte(values, headerIndexMap, "EvAttack");
			record.EvDefense = ParseCsvByte(values, headerIndexMap, "EvDefense");
			record.EvSpAttack = ParseCsvByte(values, headerIndexMap, "EvSpAttack");
			record.EvSpDefense = ParseCsvByte(values, headerIndexMap, "EvSpDefense");
			record.EvSpeed = ParseCsvByte(values, headerIndexMap, "EvSpeed");
			record.HoldItem1 = checked((ushort)ParseCsvInt(values, headerIndexMap, "HoldItem1"));
			record.HoldItem2 = checked((ushort)ParseCsvInt(values, headerIndexMap, "HoldItem2"));
			record.GenderValue = ParseCsvByte(values, headerIndexMap, "GenderValue");
			record.EggStep = ParseCsvByte(values, headerIndexMap, "EggStep");
			record.BaseHappiness = ParseCsvByte(values, headerIndexMap, "BaseHappiness");
			record.GrowthRate = ParseCsvByte(values, headerIndexMap, "GrowthRate");
			record.EggGroup1 = ParseCsvByte(values, headerIndexMap, "EggGroup1");
			record.EggGroup2 = ParseCsvByte(values, headerIndexMap, "EggGroup2");
			record.Ability1 = ParseCsvInt(values, headerIndexMap, "Ability1");
			record.Ability2 = ParseCsvInt(values, headerIndexMap, "Ability2");
			record.RunRate = ParseCsvByte(values, headerIndexMap, "RunRate");
			record.Color = ParseCsvByte(values, headerIndexMap, "Color");
			record.Flip = ParseCsvByte(values, headerIndexMap, "Flip");
			record.HiddenAbility = ParseCsvInt(values, headerIndexMap, "HiddenAbility");
			record.Type1Name = ReadTypeName(MainForm.romData, record.Type1);
			record.Type2Name = ReadTypeName(MainForm.romData, record.Type2);
			record.HoldItem1Name = ReadItemName(MainForm.romData, record.HoldItem1);
			record.HoldItem2Name = ReadItemName(MainForm.romData, record.HoldItem2);
			record.GenderLabel = GetGenderLabel(record.GenderValue);
			record.EggStepLabel = GetEggStepLabel(record.EggStep);
			record.GrowthRateLabel = GetGrowthRateLabel(record.GrowthRate);
			record.EggGroup1Label = GetEggGroupLabel(record.EggGroup1);
			record.EggGroup2Label = GetEggGroupLabel(record.EggGroup2);
			record.Ability1Name = ReadAbilityName(MainForm.romData, record.Ability1);
			record.Ability2Name = ReadAbilityName(MainForm.romData, record.Ability2);
			record.ColorLabel = GetColorLabel(record.Color);
			record.FlipLabel = GetFlipLabel(record.Flip);
			record.HiddenAbilityName = ReadAbilityName(MainForm.romData, record.HiddenAbility);
		}

		//-------------------------------------------------------------------------------
		// トレーナーヘッダ更新内容を反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyTrainerHeaderUpdates(TrainerRecord trainer, Dictionary<string, string> options)
		{
			byte classId = trainer.ClassId;
			byte introMusic = trainer.IntroMusic;
			byte spriteId = trainer.SpriteId;
			string name = trainer.Name;
			byte ai = trainer.Ai;
			ushort unknownValue = trainer.UnknownValue;
			ApplyByteOption(options, "class", ref classId);
			ApplyByteOption(options, "intro-music", ref introMusic);
			ApplyByteOption(options, "sprite", ref spriteId);
			ApplyStringOption(options, "name", ref name);
			for (int i = 0; i < 4; i++)
			{
				ushort value = trainer.Items[i];
				ApplyUShortOption(options, "item" + (i + 1).ToString(CultureInfo.InvariantCulture), ref value);
				trainer.Items[i] = value;
			}
			if (TryGetOption(options, "double-battle", out string valueText))
			{
				trainer.IsDoubleBattle = ParseBoolean(valueText, "double-battle");
			}
			ApplyByteOption(options, "ai", ref ai);
			ApplyUShortOption(options, "unknown", ref unknownValue);
			trainer.ClassId = classId;
			trainer.IntroMusic = introMusic;
			trainer.SpriteId = spriteId;
			trainer.Name = name;
			trainer.Ai = ai;
			trainer.UnknownValue = unknownValue;
			trainer.ClassName = ReadTrainerClassName(MainForm.romData, trainer.ClassId);
			for (int j = 0; j < trainer.Items.Length; j++)
			{
				trainer.ItemNames[j] = ReadItemName(MainForm.romData, trainer.Items[j]);
			}
		}

		//-------------------------------------------------------------------------------
		// トレーナー手持ち構造変更を反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyTrainerStructureUpdates(byte[] romData, TrainerRecord before, TrainerRecord after, Dictionary<string, string> options)
		{
			byte dataType = after.DataType;
			byte pokemonCount = after.PokemonCount;
			ApplyByteOption(options, "data-type", 0, 3, ref dataType);
			ApplyByteOption(options, "pokemon-count", 0, byte.MaxValue, ref pokemonCount);
			bool dataTypeChanged = dataType != before.DataType;
			bool pokemonCountChanged = pokemonCount != before.PokemonCount;
			after.DataType = dataType;
			after.PokemonCount = pokemonCount;
			ResizeTrainerParty(after);
			uint pokemonDataAddress = after.PokemonDataAddress;
			bool hasExplicitAddress = TryGetOption(options, "pokemon-data-address", out string addressText);
			bool hasStartAddress = TryGetOption(options, "start", out string startText);
			if (hasExplicitAddress)
			{
				pokemonDataAddress = ParseRomAddress(addressText, "pokemon-data-address");
			}
			else if (dataTypeChanged || pokemonCountChanged)
			{
				int oldLength = GetPokemonSlotSize(before.DataType) * before.Party.Count;
				int newLength = GetPokemonSlotSize(after.DataType) * after.Party.Count;
				if (newLength <= oldLength)
				{
					pokemonDataAddress = before.PokemonDataAddress;
				}
				else
				{
					if (!hasStartAddress)
					{
						throw new InvalidOperationException("手持ちデータを拡張する場合は --start か --pokemon-data-address を指定してください。");
					}
					uint startAddress = ParseRomAddress(startText, "start");
					pokemonDataAddress = FindFreeSpace(romData, startAddress, newLength);
				}
			}
			after.PokemonDataAddress = pokemonDataAddress;
			after.PokemonDataAddressText = FormatOffset((int)pokemonDataAddress);
		}

		//-------------------------------------------------------------------------------
		// トレーナー手持ち更新内容を反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyTrainerSlotUpdates(TrainerRecord trainer, Dictionary<string, string> options)
		{
			bool flag = HasAnyOption(options, "slot-pokemon", "slot-level", "slot-iv", "slot-unknown1", "slot-unknown2", "slot-item", "slot-move1", "slot-move2", "slot-move3", "slot-move4");
			if (!flag)
			{
				return;
			}
			int num = RequireIntOption(options, "slot");
			ValidateRange(num, 1, trainer.Party.Count, "slot");
			TrainerPokemonSlotRecord trainerPokemonSlotRecord = trainer.Party[num - 1];
			ushort pokemonCode = trainerPokemonSlotRecord.PokemonCode;
			byte level = trainerPokemonSlotRecord.Level;
			byte iv = trainerPokemonSlotRecord.Iv;
			byte unknownValue1 = trainerPokemonSlotRecord.UnknownValue1;
			byte unknownValue2 = trainerPokemonSlotRecord.UnknownValue2;
			ushort itemCode = trainerPokemonSlotRecord.ItemCode;
			ApplyUShortOption(options, "slot-pokemon", ref pokemonCode);
			ApplyByteOption(options, "slot-level", ref level);
			ApplyByteOption(options, "slot-iv", ref iv);
			ApplyByteOption(options, "slot-unknown1", ref unknownValue1);
			ApplyByteOption(options, "slot-unknown2", ref unknownValue2);
			if (trainer.DataType == TrainerDataTypeItem || trainer.DataType == TrainerDataTypeItemMoves)
			{
				ApplyUShortOption(options, "slot-item", ref itemCode);
			}
			else if (TryGetOption(options, "slot-item", out _))
			{
				throw new InvalidOperationException("このトレーナーデータ種別では item を持てません。");
			}
			bool hasMoves = trainer.DataType == TrainerDataTypeMoves || trainer.DataType == TrainerDataTypeItemMoves;
			for (int i = 0; i < 4; i++)
			{
				string key = "slot-move" + (i + 1).ToString(CultureInfo.InvariantCulture);
				if (hasMoves)
				{
					ushort value = trainerPokemonSlotRecord.Moves[i];
					ApplyUShortOption(options, key, ref value);
					trainerPokemonSlotRecord.Moves[i] = value;
				}
				else if (TryGetOption(options, key, out _))
				{
					throw new InvalidOperationException("このトレーナーデータ種別では move を持てません。");
				}
			}
			trainerPokemonSlotRecord.PokemonCode = pokemonCode;
			trainerPokemonSlotRecord.Level = level;
			trainerPokemonSlotRecord.Iv = iv;
			trainerPokemonSlotRecord.UnknownValue1 = unknownValue1;
			trainerPokemonSlotRecord.UnknownValue2 = unknownValue2;
			trainerPokemonSlotRecord.ItemCode = itemCode;
			trainerPokemonSlotRecord.PokemonName = ReadPokemonName(MainForm.romData, trainerPokemonSlotRecord.PokemonCode);
			trainerPokemonSlotRecord.ItemName = hasMoves || trainer.DataType == TrainerDataTypeItem || trainer.DataType == TrainerDataTypeItemMoves ? ReadItemName(MainForm.romData, trainerPokemonSlotRecord.ItemCode) : string.Empty;
			for (int j = 0; j < trainerPokemonSlotRecord.Moves.Length; j++)
			{
				trainerPokemonSlotRecord.MoveNames[j] = hasMoves ? ReadMoveName(MainForm.romData, trainerPokemonSlotRecord.Moves[j]) : string.Empty;
			}
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値1件をROMへ書き戻す処理
		//-------------------------------------------------------------------------------
		private static void WritePokemonStats(byte[] romData, PokemonStatsRecord record)
		{
			int offset = ParseOffset(record.Offset);
			romData[offset + 0] = record.Hp;
			romData[offset + 1] = record.Attack;
			romData[offset + 2] = record.Defense;
			romData[offset + 3] = record.Speed;
			romData[offset + 4] = record.SpAttack;
			romData[offset + 5] = record.SpDefense;
			romData[offset + 6] = record.Type1;
			romData[offset + 7] = record.Type2;
			romData[offset + 8] = record.CatchRate;
			if (record.EnableBaseStatsExpansion)
			{
				WriteUInt16(romData, offset + 30, checked((ushort)record.BaseExp));
			}
			else
			{
				romData[offset + 9] = checked((byte)record.BaseExp);
			}
			WriteUInt16(romData, offset + 10, EncodeEv(record));
			WriteUInt16(romData, offset + 12, record.HoldItem1);
			WriteUInt16(romData, offset + 14, record.HoldItem2);
			romData[offset + 16] = record.GenderValue;
			romData[offset + 17] = record.EggStep;
			romData[offset + 18] = record.BaseHappiness;
			romData[offset + 19] = record.GrowthRate;
			romData[offset + 20] = record.EggGroup1;
			romData[offset + 21] = record.EggGroup2;
			if (record.EnableBaseStatsExpansion)
			{
				WriteUInt16(romData, offset + 22, checked((ushort)record.Ability1));
				WriteUInt16(romData, offset + 26, checked((ushort)record.Ability2));
				WriteUInt16(romData, offset + 28, checked((ushort)record.HiddenAbility));
			}
			else
			{
				romData[offset + 22] = checked((byte)record.Ability1);
				romData[offset + 23] = checked((byte)record.Ability2);
				romData[offset + 26] = checked((byte)record.HiddenAbility);
			}
			romData[offset + 24] = record.RunRate;
			romData[offset + 25] = (byte)((record.Flip << 7) | (record.Color & 15));
		}

		//-------------------------------------------------------------------------------
		// トレーナーデータ1件をROMへ書き戻す処理
		//-------------------------------------------------------------------------------
		private static void WriteTrainer(byte[] romData, TrainerRecord trainer)
		{
			int trainerNameLength = ReadIniInt("TRAINER_NAME_LENGTH");
			int offset = ParseOffset(trainer.Offset);
			romData[offset + 0] = trainer.DataType;
			romData[offset + 1] = trainer.ClassId;
			romData[offset + 2] = trainer.IntroMusic;
			romData[offset + 3] = trainer.SpriteId;
			byte[] array = TextConverter.PokemonStringToBytes(trainer.Name ?? string.Empty, trainerNameLength);
			Array.Clear(romData, offset + 4, trainerNameLength);
			Array.Copy(array, 0, romData, offset + 4, Math.Min(array.Length, trainerNameLength));
			for (int i = 0; i < 4; i++)
			{
				WriteUInt16(romData, offset + 10 + i * 2, trainer.Items[i]);
			}
			romData[offset + 18] = trainer.IsDoubleBattle ? (byte)1 : (byte)0;
			romData[offset + 20] = trainer.Ai;
			romData[offset + 24] = trainer.PokemonCount;
			WriteUInt16(romData, offset + 26, trainer.UnknownValue);
			WriteUInt32(romData, offset + 28, trainer.PokemonDataAddress + GbaRomPointerBase);
			int pokemonSlotSize = GetPokemonSlotSize(trainer.DataType);
			for (int j = 0; j < trainer.Party.Count; j++)
			{
				TrainerPokemonSlotRecord trainerPokemonSlotRecord = trainer.Party[j];
				int num = checked((int)trainer.PokemonDataAddress + j * pokemonSlotSize);
				trainerPokemonSlotRecord.Offset = FormatOffset(num);
				Array.Clear(romData, num, pokemonSlotSize);
				romData[num + 0] = trainerPokemonSlotRecord.Iv;
				romData[num + 1] = trainerPokemonSlotRecord.UnknownValue1;
				romData[num + 2] = trainerPokemonSlotRecord.Level;
				romData[num + 3] = trainerPokemonSlotRecord.UnknownValue2;
				WriteUInt16(romData, num + 4, trainerPokemonSlotRecord.PokemonCode);
				switch (trainer.DataType)
				{
				case TrainerDataTypeBasic:
					Array.Clear(romData, num + 6, 2);
					break;
				case TrainerDataTypeMoves:
					for (int k = 0; k < 4; k++)
					{
						WriteUInt16(romData, num + 6 + k * 2, trainerPokemonSlotRecord.Moves[k]);
					}
					break;
				case TrainerDataTypeItem:
					WriteUInt16(romData, num + 6, trainerPokemonSlotRecord.ItemCode);
					break;
				case TrainerDataTypeItemMoves:
					WriteUInt16(romData, num + 6, trainerPokemonSlotRecord.ItemCode);
					for (int l = 0; l < 4; l++)
					{
						WriteUInt16(romData, num + 8 + l * 2, trainerPokemonSlotRecord.Moves[l]);
					}
					break;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 手持ちデータ種別からスロット長を取得する処理
		//-------------------------------------------------------------------------------
		private static int GetPokemonSlotSize(byte dataType)
		{
			switch (dataType)
			{
			case TrainerDataTypeBasic:
			case TrainerDataTypeItem:
				return 8;
			case TrainerDataTypeMoves:
			case TrainerDataTypeItemMoves:
				return 16;
			default:
				return 8;
			}
		}

		//-------------------------------------------------------------------------------
		// トレーナー手持ち件数に合わせてリストを調整する処理
		//-------------------------------------------------------------------------------
		private static void ResizeTrainerParty(TrainerRecord trainer)
		{
			int num = trainer.PokemonCount;
			if (num < trainer.Party.Count)
			{
				trainer.Party.RemoveRange(num, trainer.Party.Count - num);
			}
			while (trainer.Party.Count < num)
			{
				trainer.Party.Add(new TrainerPokemonSlotRecord
				{
					Slot = trainer.Party.Count + 1,
					Offset = string.Empty,
					Iv = 0,
					UnknownValue1 = 0,
					Level = 1,
					UnknownValue2 = 0,
					PokemonCode = 0,
					PokemonName = string.Empty,
					ItemCode = 0,
					ItemName = string.Empty,
					Moves = new ushort[4],
					MoveNames = new string[4]
				});
			}
			for (int i = 0; i < trainer.Party.Count; i++)
			{
				trainer.Party[i].Slot = i + 1;
			}
		}

		//-------------------------------------------------------------------------------
		// 努力値ビットフィールドをデコードする処理
		//-------------------------------------------------------------------------------
		private static byte[] DecodeEv(ushort evValue)
		{
			byte[] bytes = BitConverter.GetBytes(evValue);
			return new byte[]
			{
				(byte)(bytes[0] & 3),
				(byte)((bytes[0] & 12) >> 2),
				(byte)((bytes[0] & 48) >> 4),
				(byte)(bytes[1] & 3),
				(byte)((bytes[1] & 12) >> 2),
				(byte)((bytes[0] & 192) >> 6)
			};
		}

		//-------------------------------------------------------------------------------
		// 努力値をビットフィールドへエンコードする処理
		//-------------------------------------------------------------------------------
		private static ushort EncodeEv(PokemonStatsRecord record)
		{
			byte b = 0;
			byte b2 = 0;
			b = (byte)(b | (record.EvHp & 3));
			b = (byte)(b | ((record.EvAttack & 3) << 2));
			b = (byte)(b | ((record.EvDefense & 3) << 4));
			b = (byte)(b | ((record.EvSpeed & 3) << 6));
			b2 = (byte)(b2 | (record.EvSpAttack & 3));
			b2 = (byte)(b2 | ((record.EvSpDefense & 3) << 2));
			return BitConverter.ToUInt16(new byte[] { b, b2 }, 0);
		}

		//-------------------------------------------------------------------------------
		// ポケモン名を取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadPokemonName(byte[] romData, int pokemonId)
		{
			int pokemonNameOffset = ReadIniInt("POKEMON_NAME_OFFSET");
			int pokemonNameLength = ReadIniInt("POKEMON_NAME_LENGTH");
			int totalPokemonCount = ReadIniInt("TOTAL_POKEMON_COUNT");
			if (pokemonId < 0 || pokemonId >= totalPokemonCount)
			{
				return string.Empty;
			}
			int offset = checked(pokemonNameOffset + pokemonId * pokemonNameLength);
			return TextConverter.BytesToPokemonString(romData, offset, pokemonNameLength);
		}

		//-------------------------------------------------------------------------------
		// トレーナー肩書名を取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadTrainerClassName(byte[] romData, int classId)
		{
			int trainerClassNameTableOffset = ReadIniInt("TRAINER_CLASS_NAME_TABLE_OFFSET");
			int trainerClassNameLength = ReadIniInt("TRAINER_CLASS_NAME_LENGTH");
			int trainerClassNameCount = ReadIniInt("TRAINER_CLASS_NAME_COUNT");
			if (classId < 0 || classId >= trainerClassNameCount)
			{
				return string.Empty;
			}
			int offset = checked(trainerClassNameTableOffset + classId * trainerClassNameLength);
			return TextConverter.BytesToPokemonString(romData, offset, trainerClassNameLength);
		}

		//-------------------------------------------------------------------------------
		// 特性名を取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadAbilityName(byte[] romData, int abilityId)
		{
			int abilityNameTableOffset = ReadIniInt("ABILITY_NAME_TABLE_OFFSET");
			int abilityNameLength = ReadIniInt("ABILITY_NAME_LENGTH");
			int totalAbilityCount = ReadIniInt("TOTAL_ABILITY_COUNT");
			if (abilityId < 0 || abilityId >= totalAbilityCount)
			{
				return string.Empty;
			}
			int offset = checked(abilityNameTableOffset + abilityId * abilityNameLength);
			return TextConverter.BytesToPokemonString(romData, offset, abilityNameLength);
		}

		//-------------------------------------------------------------------------------
		// タイプ名を取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadTypeName(byte[] romData, int typeId)
		{
			int typeTableOffset = ReadIniInt("TYPE_TABLE_OFFSET");
			int typeNameLength = ReadIniInt("TYPE_NAME_LENGTH");
			int totalTypeCount = ReadIniInt("TOTAL_TYPE_COUNT");
			if (typeId < 0 || typeId >= totalTypeCount)
			{
				return string.Empty;
			}
			int offset = checked(typeTableOffset + typeId * typeNameLength);
			return TextConverter.BytesToPokemonString(romData, offset, typeNameLength);
		}

		//-------------------------------------------------------------------------------
		// アイテム名を取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadItemName(byte[] romData, ushort itemId)
		{
			if (itemId == 0)
			{
				return string.Empty;
			}
			List<string> itemNames = GetCachedItemNames(romData);
			return itemId < itemNames.Count ? itemNames[itemId] : string.Empty;
		}

		//-------------------------------------------------------------------------------
		// 技名を取得する処理
		//-------------------------------------------------------------------------------
		private static string ReadMoveName(byte[] romData, ushort moveId)
		{
			if (moveId == 0)
			{
				return string.Empty;
			}
			List<string> moveNames = GetCachedMoveNames(romData);
			return moveId < moveNames.Count ? moveNames[moveId] : string.Empty;
		}

		//-------------------------------------------------------------------------------
		// アイテム名一覧のキャッシュを取得する処理
		//-------------------------------------------------------------------------------
		private static List<string> GetCachedItemNames(byte[] romData)
		{
			if (!object.ReferenceEquals(cachedItemNameRom, romData) || cachedItemNames == null)
			{
				cachedItemNameRom = romData;
				cachedItemNames = ItemData.GetItemNames(romData);
			}
			return cachedItemNames;
		}

		//-------------------------------------------------------------------------------
		// 技名一覧のキャッシュを取得する処理
		//-------------------------------------------------------------------------------
		private static List<string> GetCachedMoveNames(byte[] romData)
		{
			if (!object.ReferenceEquals(cachedMoveNameRom, romData) || cachedMoveNames == null)
			{
				cachedMoveNameRom = romData;
				cachedMoveNames = MoveData.GetMoveNames(romData);
			}
			return cachedMoveNames;
		}

		//-------------------------------------------------------------------------------
		// 出力ROMの保存先を決定する処理
		//-------------------------------------------------------------------------------
		private static string GetRomOutputPath(Dictionary<string, string> options, string romPath)
		{
			if (options.TryGetValue("out-rom", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				string fullPath = Path.GetFullPath(value);
				string directoryName = Path.GetDirectoryName(fullPath);
				if (!string.IsNullOrEmpty(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				return fullPath;
			}
			return Path.GetFullPath(romPath);
		}

		//-------------------------------------------------------------------------------
		// オプション存在判定を行う処理
		//-------------------------------------------------------------------------------
		private static bool HasAnyOption(Dictionary<string, string> options, params string[] keys)
		{
			return keys.Any((string key) => TryGetOption(options, key, out _));
		}

		//-------------------------------------------------------------------------------
		// オプション文字列を取得する処理
		//-------------------------------------------------------------------------------
		private static bool TryGetOption(Dictionary<string, string> options, string key, out string value)
		{
			return options.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value);
		}

		//-------------------------------------------------------------------------------
		// 必須オプション文字列を取得する処理
		//-------------------------------------------------------------------------------
		private static string RequireOption(Dictionary<string, string> options, string key)
		{
			if (!TryGetOption(options, key, out string value))
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
			return ParseInt(requiredOption, key);
		}

		//-------------------------------------------------------------------------------
		// 数値オプションをbyteへ反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyByteOption(Dictionary<string, string> options, string key, ref byte currentValue)
		{
			ApplyByteOption(options, key, 0, byte.MaxValue, ref currentValue);
		}

		//-------------------------------------------------------------------------------
		// 範囲付き数値オプションをbyteへ反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyByteOption(Dictionary<string, string> options, string key, int min, int max, ref byte currentValue)
		{
			if (!TryGetOption(options, key, out string value))
			{
				return;
			}
			int num = ParseInt(value, key);
			ValidateRange(num, min, max, key);
			currentValue = checked((byte)num);
		}

		//-------------------------------------------------------------------------------
		// 数値オプションをushortへ反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyUShortOption(Dictionary<string, string> options, string key, ref ushort currentValue)
		{
			if (!TryGetOption(options, key, out string value))
			{
				return;
			}
			int num = ParseInt(value, key);
			ValidateRange(num, 0, ushort.MaxValue, key);
			currentValue = checked((ushort)num);
		}

		//-------------------------------------------------------------------------------
		// 数値オプションをintへ反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyIntOption(Dictionary<string, string> options, string key, int min, int max, ref int currentValue)
		{
			if (!TryGetOption(options, key, out string value))
			{
				return;
			}
			int num = ParseInt(value, key);
			ValidateRange(num, min, max, key);
			currentValue = num;
		}

		//-------------------------------------------------------------------------------
		// 文字列オプションを反映する処理
		//-------------------------------------------------------------------------------
		private static void ApplyStringOption(Dictionary<string, string> options, string key, ref string currentValue)
		{
			if (TryGetOption(options, key, out string value))
			{
				currentValue = value;
			}
		}

		//-------------------------------------------------------------------------------
		// 数値文字列を解析する処理
		//-------------------------------------------------------------------------------
		private static int ParseInt(string value, string key)
		{
			try
			{
				if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					return Convert.ToInt32(value.Substring(2), 16);
				}
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("--" + key + " の値が不正です。: " + value, ex);
			}
		}

		//-------------------------------------------------------------------------------
		// ROMアドレス表記をROMオフセットへ変換する処理
		//-------------------------------------------------------------------------------
		private static uint ParseRomAddress(string value, string key)
		{
			uint num;
			try
			{
				if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					num = Convert.ToUInt32(value.Substring(2), 16);
				}
				else
				{
					num = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
				}
			}
			catch (Exception ex)
			{
				throw new ArgumentException("--" + key + " の値が不正です。: " + value, ex);
			}
			return num >= GbaRomPointerBase ? num - GbaRomPointerBase : num;
		}

		//-------------------------------------------------------------------------------
		// FF埋め空き領域を検索する処理
		//-------------------------------------------------------------------------------
		private static uint FindFreeSpace(byte[] romData, uint startAddress, int length)
		{
			int num = checked((int)startAddress);
			ValidateSlice(romData, num, 0, "free-space");
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
					if (num % 4 != 0)
					{
						num = (num + 3) & -4;
					}
					ValidateSlice(romData, num, length, "free-space");
					return checked((uint)num);
				}
			}
			throw new InvalidOperationException("指定開始位置以降に十分な空き領域が見つかりません。");
		}

		//-------------------------------------------------------------------------------
		// 真偽値文字列を解析する処理
		//-------------------------------------------------------------------------------
		private static bool ParseBoolean(string value, string key)
		{
			if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			throw new ArgumentException("--" + key + " は true/false で指定してください。");
		}

		//-------------------------------------------------------------------------------
		// Rom.ini の数値設定をキャッシュ付きで取得する処理
		//-------------------------------------------------------------------------------
		private static int ReadIniInt(string key)
		{
			if (!iniIntCache.TryGetValue(key, out int value))
			{
				value = RomIniReader.ReadHexOrDecimal(key);
				iniIntCache[key] = value;
			}
			return value;
		}

		//-------------------------------------------------------------------------------
		// Rom.ini の真偽値設定をキャッシュ付きで取得する処理
		//-------------------------------------------------------------------------------
		private static bool ReadIniBool(string key)
		{
			if (!iniBoolCache.TryGetValue(key, out bool value))
			{
				value = RomIniReader.ReadBoolean(key);
				iniBoolCache[key] = value;
			}
			return value;
		}

		//-------------------------------------------------------------------------------
		// 数値範囲を検証する処理
		//-------------------------------------------------------------------------------
		private static void ValidateRange(int value, int min, int max, string key)
		{
			if (value < min || value > max)
			{
				throw new ArgumentOutOfRangeException(key, value, min.ToString(CultureInfo.InvariantCulture) + " から " + max.ToString(CultureInfo.InvariantCulture) + " の範囲で指定してください。");
			}
		}

		//-------------------------------------------------------------------------------
		// ROM範囲内のデータ長を検証する処理
		//-------------------------------------------------------------------------------
		private static void ValidateSlice(byte[] romData, int offset, int length, string label)
		{
			if (offset < 0 || length < 0 || offset + length > romData.Length)
			{
				throw new InvalidOperationException(label + " の参照範囲がROMサイズを超えています。");
			}
		}

		//-------------------------------------------------------------------------------
		// 16bit値を書き込む処理
		//-------------------------------------------------------------------------------
		private static void WriteUInt16(byte[] romData, int offset, ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, romData, offset, 2);
		}

		//-------------------------------------------------------------------------------
		// 32bit値を書き込む処理
		//-------------------------------------------------------------------------------
		private static void WriteUInt32(byte[] romData, int offset, uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Copy(bytes, 0, romData, offset, 4);
		}

		//-------------------------------------------------------------------------------
		// CSVヘッダと列番号の対応表を作成する処理
		//-------------------------------------------------------------------------------
		private static Dictionary<string, int> BuildCsvHeaderIndexMap(string headerLine)
		{
			string[] array = ParseCsvLine(headerLine);
			Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < array.Length; i++)
			{
				dictionary[array[i]] = i;
			}
			return dictionary;
		}

		//-------------------------------------------------------------------------------
		// 必須CSVヘッダを検証する処理
		//-------------------------------------------------------------------------------
		private static void ValidateCsvHeaders(Dictionary<string, int> headerIndexMap, IEnumerable<string> requiredHeaders)
		{
			foreach (string text in requiredHeaders)
			{
				if (!headerIndexMap.ContainsKey(text))
				{
					throw new InvalidOperationException("CSVヘッダが不足しています。: " + text);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// CSV1行を解析する処理
		//-------------------------------------------------------------------------------
		private static string[] ParseCsvLine(string line)
		{
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];
				if (c == '"')
				{
					if (flag && i + 1 < line.Length && line[i + 1] == '"')
					{
						stringBuilder.Append('"');
						i++;
					}
					else
					{
						flag = !flag;
					}
				}
				else if (c == ',' && !flag)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			list.Add(stringBuilder.ToString());
			return list.ToArray();
		}

		//-------------------------------------------------------------------------------
		// CSV1行を書式化する処理
		//-------------------------------------------------------------------------------
		private static string BuildCsvLine(IEnumerable<string> values)
		{
			return string.Join(",", values.Select(EscapeCsvValue));
		}

		//-------------------------------------------------------------------------------
		// CSVセルをエスケープする処理
		//-------------------------------------------------------------------------------
		private static string EscapeCsvValue(string value)
		{
			string text = value ?? string.Empty;
			if (text.Contains('"'))
			{
				text = text.Replace("\"", "\"\"");
			}
			if (text.IndexOfAny(new char[] { ',', '"', '\r', '\n' }) >= 0)
			{
				return "\"" + text + "\"";
			}
			return text;
		}

		//-------------------------------------------------------------------------------
		// CSVセルから数値を取得する処理
		//-------------------------------------------------------------------------------
		private static int ParseCsvInt(string[] values, Dictionary<string, int> headerIndexMap, string key)
		{
			if (!headerIndexMap.TryGetValue(key, out int value) || value >= values.Length)
			{
				throw new InvalidOperationException("CSV列が見つかりません。: " + key);
			}
			return ParseInt(values[value], key);
		}

		//-------------------------------------------------------------------------------
		// CSVセルからbyte値を取得する処理
		//-------------------------------------------------------------------------------
		private static byte ParseCsvByte(string[] values, Dictionary<string, int> headerIndexMap, string key)
		{
			int num = ParseCsvInt(values, headerIndexMap, key);
			ValidateRange(num, 0, byte.MaxValue, key);
			return checked((byte)num);
		}

		//-------------------------------------------------------------------------------
		// UTF-8テキストを書き出す処理
		//-------------------------------------------------------------------------------
		private static void WriteUtf8File(string path, string content)
		{
			string fullPath = Path.GetFullPath(path);
			string directoryName = Path.GetDirectoryName(fullPath);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllText(fullPath, content, new UTF8Encoding(false));
		}

		//-------------------------------------------------------------------------------
		// ROMオフセットを16進文字列へ変換する処理
		//-------------------------------------------------------------------------------
		private static string FormatOffset(int offset)
		{
			return "0x" + offset.ToString("X8", CultureInfo.InvariantCulture);
		}

		//-------------------------------------------------------------------------------
		// 16進文字列オフセットを数値へ変換する処理
		//-------------------------------------------------------------------------------
		private static int ParseOffset(string offsetText)
		{
			return ParseInt(offsetText, "offset");
		}

		//-------------------------------------------------------------------------------
		// ポケモン種族値記録を複製する処理
		//-------------------------------------------------------------------------------
		private static PokemonStatsRecord ClonePokemonStats(PokemonStatsRecord source)
		{
			return new PokemonStatsRecord
			{
				Index = source.Index,
				Name = source.Name,
				Offset = source.Offset,
				EntryLength = source.EntryLength,
				EnableBaseStatsExpansion = source.EnableBaseStatsExpansion,
				Hp = source.Hp,
				Attack = source.Attack,
				Defense = source.Defense,
				Speed = source.Speed,
				SpAttack = source.SpAttack,
				SpDefense = source.SpDefense,
				Type1 = source.Type1,
				Type1Name = source.Type1Name,
				Type2 = source.Type2,
				Type2Name = source.Type2Name,
				CatchRate = source.CatchRate,
				BaseExp = source.BaseExp,
				EvHp = source.EvHp,
				EvAttack = source.EvAttack,
				EvDefense = source.EvDefense,
				EvSpAttack = source.EvSpAttack,
				EvSpDefense = source.EvSpDefense,
				EvSpeed = source.EvSpeed,
				HoldItem1 = source.HoldItem1,
				HoldItem1Name = source.HoldItem1Name,
				HoldItem2 = source.HoldItem2,
				HoldItem2Name = source.HoldItem2Name,
				GenderValue = source.GenderValue,
				GenderLabel = source.GenderLabel,
				EggStep = source.EggStep,
				EggStepLabel = source.EggStepLabel,
				BaseHappiness = source.BaseHappiness,
				GrowthRate = source.GrowthRate,
				GrowthRateLabel = source.GrowthRateLabel,
				EggGroup1 = source.EggGroup1,
				EggGroup1Label = source.EggGroup1Label,
				EggGroup2 = source.EggGroup2,
				EggGroup2Label = source.EggGroup2Label,
				Ability1 = source.Ability1,
				Ability1Name = source.Ability1Name,
				Ability2 = source.Ability2,
				Ability2Name = source.Ability2Name,
				RunRate = source.RunRate,
				Color = source.Color,
				ColorLabel = source.ColorLabel,
				Flip = source.Flip,
				FlipLabel = source.FlipLabel,
				HiddenAbility = source.HiddenAbility,
				HiddenAbilityName = source.HiddenAbilityName
			};
		}

		//-------------------------------------------------------------------------------
		// トレーナー記録を複製する処理
		//-------------------------------------------------------------------------------
		private static TrainerRecord CloneTrainer(TrainerRecord source)
		{
			TrainerRecord trainerRecord = new TrainerRecord();
			trainerRecord.Index = source.Index;
			trainerRecord.Offset = source.Offset;
			trainerRecord.DataType = source.DataType;
			trainerRecord.ClassId = source.ClassId;
			trainerRecord.ClassName = source.ClassName;
			trainerRecord.IntroMusic = source.IntroMusic;
			trainerRecord.SpriteId = source.SpriteId;
			trainerRecord.Name = source.Name;
			trainerRecord.Items = source.Items.ToArray();
			trainerRecord.ItemNames = source.ItemNames.ToArray();
			trainerRecord.IsDoubleBattle = source.IsDoubleBattle;
			trainerRecord.Ai = source.Ai;
			trainerRecord.PokemonCount = source.PokemonCount;
			trainerRecord.UnknownValue = source.UnknownValue;
			trainerRecord.PokemonDataAddress = source.PokemonDataAddress;
			trainerRecord.PokemonDataAddressText = source.PokemonDataAddressText;
			trainerRecord.Party = source.Party.Select(CloneTrainerSlot).ToList();
			return trainerRecord;
		}

		//-------------------------------------------------------------------------------
		// トレーナー手持ち記録を複製する処理
		//-------------------------------------------------------------------------------
		private static TrainerPokemonSlotRecord CloneTrainerSlot(TrainerPokemonSlotRecord source)
		{
			return new TrainerPokemonSlotRecord
			{
				Slot = source.Slot,
				Offset = source.Offset,
				Iv = source.Iv,
				UnknownValue1 = source.UnknownValue1,
				Level = source.Level,
				UnknownValue2 = source.UnknownValue2,
				PokemonCode = source.PokemonCode,
				PokemonName = source.PokemonName,
				ItemCode = source.ItemCode,
				ItemName = source.ItemName,
				Moves = source.Moves.ToArray(),
				MoveNames = source.MoveNames.ToArray()
			};
		}

		//-------------------------------------------------------------------------------
		// 性別値ラベルを取得する処理
		//-------------------------------------------------------------------------------
		private static string GetGenderLabel(byte value)
		{
			switch (value)
			{
			case 0:
				return "♂のみ";
			case 31:
				return "♂:87.5% / ♀:12.5%";
			case 63:
				return "♂:75% / ♀:25%";
			case 127:
				return "♂:50% / ♀:50%";
			case 191:
				return "♂:25% / ♀:75%";
			case 223:
				return "♂:12.5% / ♀:87.5%";
			case 254:
				return "♀のみ";
			case byte.MaxValue:
				return "ふめい";
			default:
				return string.Empty;
			}
		}

		//-------------------------------------------------------------------------------
		// 孵化歩数ラベルを取得する処理
		//-------------------------------------------------------------------------------
		private static string GetEggStepLabel(byte value)
		{
			switch (value)
			{
			case 5:
				return "1280歩(サイクル5)";
			case 10:
				return "2560歩(サイクル10)";
			case 15:
				return "3840歩(サイクル15)";
			case 20:
				return "5120歩(サイクル20)";
			case 25:
				return "6400歩(サイクル25)";
			case 30:
				return "7680歩(サイクル30)";
			case 35:
				return "8960歩(サイクル35)";
			case 40:
				return "10240歩(サイクル40)";
			case 120:
				return "-(サイクル120)";
			default:
				return string.Empty;
			}
		}

		//-------------------------------------------------------------------------------
		// タマゴグループラベルを取得する処理
		//-------------------------------------------------------------------------------
		private static string GetEggGroupLabel(byte value)
		{
			switch (value)
			{
			case 1:
				return "怪獣";
			case 10:
				return "鉱物";
			case 7:
				return "植物";
			case 2:
				return "水中1";
			case 12:
				return "水中2";
			case 9:
				return "水中3";
			case 14:
				return "ドラゴン";
			case 4:
				return "飛行";
			case 8:
				return "人型";
			case 11:
				return "不定形";
			case 3:
				return "虫";
			case 6:
				return "妖精";
			case 5:
				return "陸上";
			case 13:
				return "メタモン";
			case 15:
				return "タマゴ未発見";
			default:
				return string.Empty;
			}
		}

		//-------------------------------------------------------------------------------
		// 成長率ラベルを取得する処理
		//-------------------------------------------------------------------------------
		private static string GetGrowthRateLabel(byte value)
		{
			switch (value)
			{
			case 0:
				return "Medium Fast";
			case 1:
				return "Erratic";
			case 2:
				return "Fluctuating";
			case 3:
				return "Medium Slow";
			case 4:
				return "Fast";
			case 5:
				return "Slow";
			default:
				return string.Empty;
			}
		}

		//-------------------------------------------------------------------------------
		// 体色ラベルを取得する処理
		//-------------------------------------------------------------------------------
		private static string GetColorLabel(byte value)
		{
			string[] array = new string[] { "赤", "青", "黄", "緑", "黒", "茶", "紫", "灰", "白", "桃" };
			return value < array.Length ? array[value] : string.Empty;
		}

		//-------------------------------------------------------------------------------
		// 図鑑向きラベルを取得する処理
		//-------------------------------------------------------------------------------
		private static string GetFlipLabel(byte value)
		{
			return value == 0 ? "通常" : "左右反転";
		}

		internal sealed class PokemonStatsUpdateResult
		{
			public string Target { get; set; }

			public int Pokemon { get; set; }

			public string Name { get; set; }

			public string OutputRomPath { get; set; }

			public PokemonStatsRecord Before { get; set; }

			public PokemonStatsRecord After { get; set; }
		}

		internal sealed class CsvExportResult
		{
			public string Target { get; set; }

			public string OutputPath { get; set; }

			public int RowCount { get; set; }
		}

		internal sealed class PokemonStatsCsvImportResult
		{
			public string Target { get; set; }

			public string InputCsvPath { get; set; }

			public string OutputRomPath { get; set; }

			public int ChangedCount { get; set; }

			public List<PokemonStatsCsvChange> Changes { get; set; }
		}

		internal sealed class PokemonStatsCsvChange
		{
			public int Index { get; set; }

			public string Name { get; set; }

			public byte BeforeHp { get; set; }

			public byte AfterHp { get; set; }

			public byte BeforeAttack { get; set; }

			public byte AfterAttack { get; set; }

			public byte BeforeDefense { get; set; }

			public byte AfterDefense { get; set; }

			public byte BeforeSpeed { get; set; }

			public byte AfterSpeed { get; set; }
		}

		internal sealed class TrainerUpdateResult
		{
			public string Target { get; set; }

			public int Trainer { get; set; }

			public string Name { get; set; }

			public string OutputRomPath { get; set; }

			public TrainerRecord Before { get; set; }

			public TrainerRecord After { get; set; }
		}

		internal sealed class PokemonStatsRecord
		{
			public int Index { get; set; }

			public string Name { get; set; }

			public string Offset { get; set; }

			public int EntryLength { get; set; }

			public bool EnableBaseStatsExpansion { get; set; }

			public byte Hp { get; set; }

			public byte Attack { get; set; }

			public byte Defense { get; set; }

			public byte Speed { get; set; }

			public byte SpAttack { get; set; }

			public byte SpDefense { get; set; }

			public byte Type1 { get; set; }

			public string Type1Name { get; set; }

			public byte Type2 { get; set; }

			public string Type2Name { get; set; }

			public byte CatchRate { get; set; }

			public int BaseExp { get; set; }

			public byte EvHp { get; set; }

			public byte EvAttack { get; set; }

			public byte EvDefense { get; set; }

			public byte EvSpAttack { get; set; }

			public byte EvSpDefense { get; set; }

			public byte EvSpeed { get; set; }

			public ushort HoldItem1 { get; set; }

			public string HoldItem1Name { get; set; }

			public ushort HoldItem2 { get; set; }

			public string HoldItem2Name { get; set; }

			public byte GenderValue { get; set; }

			public string GenderLabel { get; set; }

			public byte EggStep { get; set; }

			public string EggStepLabel { get; set; }

			public byte BaseHappiness { get; set; }

			public byte GrowthRate { get; set; }

			public string GrowthRateLabel { get; set; }

			public byte EggGroup1 { get; set; }

			public string EggGroup1Label { get; set; }

			public byte EggGroup2 { get; set; }

			public string EggGroup2Label { get; set; }

			public int Ability1 { get; set; }

			public string Ability1Name { get; set; }

			public int Ability2 { get; set; }

			public string Ability2Name { get; set; }

			public byte RunRate { get; set; }

			public byte Color { get; set; }

			public string ColorLabel { get; set; }

			public byte Flip { get; set; }

			public string FlipLabel { get; set; }

			public int HiddenAbility { get; set; }

			public string HiddenAbilityName { get; set; }

			public bool HasChangesComparedTo(PokemonStatsRecord other)
			{
				return this.Hp != other.Hp || this.Attack != other.Attack || this.Defense != other.Defense || this.Speed != other.Speed || this.SpAttack != other.SpAttack || this.SpDefense != other.SpDefense || this.Type1 != other.Type1 || this.Type2 != other.Type2 || this.CatchRate != other.CatchRate || this.BaseExp != other.BaseExp || this.EvHp != other.EvHp || this.EvAttack != other.EvAttack || this.EvDefense != other.EvDefense || this.EvSpAttack != other.EvSpAttack || this.EvSpDefense != other.EvSpDefense || this.EvSpeed != other.EvSpeed || this.HoldItem1 != other.HoldItem1 || this.HoldItem2 != other.HoldItem2 || this.GenderValue != other.GenderValue || this.EggStep != other.EggStep || this.BaseHappiness != other.BaseHappiness || this.GrowthRate != other.GrowthRate || this.EggGroup1 != other.EggGroup1 || this.EggGroup2 != other.EggGroup2 || this.Ability1 != other.Ability1 || this.Ability2 != other.Ability2 || this.RunRate != other.RunRate || this.Color != other.Color || this.Flip != other.Flip || this.HiddenAbility != other.HiddenAbility;
			}
		}

		internal sealed class TrainerRecord
		{
			public int Index { get; set; }

			public string Offset { get; set; }

			public byte DataType { get; set; }

			public byte ClassId { get; set; }

			public string ClassName { get; set; }

			public byte IntroMusic { get; set; }

			public byte SpriteId { get; set; }

			public string Name { get; set; }

			public ushort[] Items { get; set; }

			public string[] ItemNames { get; set; }

			public bool IsDoubleBattle { get; set; }

			public byte Ai { get; set; }

			public byte PokemonCount { get; set; }

			public ushort UnknownValue { get; set; }

			public uint PokemonDataAddress { get; set; }

			public string PokemonDataAddressText { get; set; }

			public List<TrainerPokemonSlotRecord> Party { get; set; }

			public bool HasChangesComparedTo(TrainerRecord other)
			{
				if (this.DataType != other.DataType || this.ClassId != other.ClassId || this.IntroMusic != other.IntroMusic || this.SpriteId != other.SpriteId || !string.Equals(this.Name, other.Name, StringComparison.Ordinal) || this.IsDoubleBattle != other.IsDoubleBattle || this.Ai != other.Ai || this.PokemonCount != other.PokemonCount || this.UnknownValue != other.UnknownValue || this.PokemonDataAddress != other.PokemonDataAddress)
				{
					return true;
				}
				for (int i = 0; i < this.Items.Length; i++)
				{
					if (this.Items[i] != other.Items[i])
					{
						return true;
					}
				}
				for (int j = 0; j < this.Party.Count; j++)
				{
					TrainerPokemonSlotRecord trainerPokemonSlotRecord = this.Party[j];
					TrainerPokemonSlotRecord trainerPokemonSlotRecord2 = other.Party[j];
					if (trainerPokemonSlotRecord.Iv != trainerPokemonSlotRecord2.Iv || trainerPokemonSlotRecord.UnknownValue1 != trainerPokemonSlotRecord2.UnknownValue1 || trainerPokemonSlotRecord.Level != trainerPokemonSlotRecord2.Level || trainerPokemonSlotRecord.UnknownValue2 != trainerPokemonSlotRecord2.UnknownValue2 || trainerPokemonSlotRecord.PokemonCode != trainerPokemonSlotRecord2.PokemonCode || trainerPokemonSlotRecord.ItemCode != trainerPokemonSlotRecord2.ItemCode)
					{
						return true;
					}
					for (int k = 0; k < trainerPokemonSlotRecord.Moves.Length; k++)
					{
						if (trainerPokemonSlotRecord.Moves[k] != trainerPokemonSlotRecord2.Moves[k])
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		internal sealed class TrainerPokemonSlotRecord
		{
			public int Slot { get; set; }

			public string Offset { get; set; }

			public byte Iv { get; set; }

			public byte UnknownValue1 { get; set; }

			public byte Level { get; set; }

			public byte UnknownValue2 { get; set; }

			public ushort PokemonCode { get; set; }

			public string PokemonName { get; set; }

			public ushort ItemCode { get; set; }

			public string ItemName { get; set; }

			public ushort[] Moves { get; set; }

			public string[] MoveNames { get; set; }
		}
	}
}
