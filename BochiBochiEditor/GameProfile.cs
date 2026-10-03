using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ゲーム（ROM の種類）ごとの違いをまとめた定義
	// ROM ヘッダーのゲームコード（0xAC からの 4 文字）で判別する
	//-------------------------------------------------------------------------------
	internal sealed class GameProfile
	{
		// ゲームコード（例: BPRJ = ファイアレッド日本語版、BPEE = エメラルド英語版）
		public string Code;
		// 画面に出す名前
		public string DisplayName;
		// このエディタで編集できるか（false の場合は読み込まずに案内を出す）
		public bool IsSupported;
		// 未対応の場合の補足
		public string SupportNote;
		// アドレス定義ファイル（ini フォルダ内）
		public string IniFileName = "Rom.ini";
		// 文字表と、マップ構造・表示専用の切り替え
		public string CharTableFile;
		public bool HasBorderSize;
		// マップ名の文字数の上限（終端を除く）
		public int MapNameMaxLength = 11;
		public bool EmeraldHeaderLayout;
		public bool EmeraldRegionMap;
		public bool SupportsTripleLayer;
		public string MapBankLimitFile;
		public bool UseTileset2BlockLimitIni;
		public bool IsReadOnly;
		// マップの確定と ROM の保存だけ使える（ブロック編集・取り込み・新規作成などはまだ使えない）ゲーム
		public bool LimitedEditing;

		// ---- タイルセットの構造（ゲームによって異なる） ----
		// 第1タイルセットのタイル数・ブロック数
		public int PrimaryTileCount;
		public int PrimaryBlockCount;
		// 第1タイルセットが使うパレットの本数（残りが第2タイルセット用、合計 13 本）
		public int PrimaryPaletteCount;
		// ブロック 1 個あたりの挙動データのバイト数（FR は 4、エメラルドは 2）
		public int BehaviorBytes;
		// タイルセット見出し内の「アニメ処理の呼び出し先」「挙動データ」の位置
		public int TilesetCallbackOffset;
		public int TilesetBehaviorOffset;

		// ---- 既知のゲーム ----
		private static readonly List<GameProfile> known = new List<GameProfile>
		{
			FireRed("BPRJ", "ファイアレッド（日本語版）", true, null),
			FireRed("BPRE", "ファイアレッド（英語版）", false, "英語版はアドレスが日本語版と異なるため、定義ファイルの用意が必要です。"),
			FireRed("BPGJ", "リーフグリーン（日本語版）", false, "リーフグリーンは今後の対応候補です。"),
			FireRed("BPGE", "リーフグリーン（英語版）", false, "リーフグリーンは今後の対応候補です。"),
			Emerald("BPEJ", "エメラルド（日本語版）"),
			Emerald("BPEE", "エメラルド（英語版）"),
			RubySapphire("AXVJ", "ルビー（日本語版）"),
			RubySapphire("AXPJ", "サファイア（日本語版）"),
			RubySapphire("AXVE", "ルビー（英語版）"),
			RubySapphire("AXPE", "サファイア（英語版）")
		};

		// 現在読み込んでいる ROM の定義（未読込時はファイアレッド日本語版）
		public static GameProfile Current { get; private set; } = known[0];

		//-------------------------------------------------------------------------------
		// ファイアレッド／リーフグリーン系の定義を作る処理
		//-------------------------------------------------------------------------------
		private static GameProfile FireRed(string code, string name, bool supported, string note)
		{
			return new GameProfile
			{
				Code = code,
				DisplayName = name,
				IsSupported = supported,
				SupportNote = note,
				CharTableFile = "charmap.tbl",
				HasBorderSize = true,
				EmeraldHeaderLayout = false,
				EmeraldRegionMap = false,
				SupportsTripleLayer = true,
				MapBankLimitFile = "MapBankLimit.ini",
				UseTileset2BlockLimitIni = true,
				IsReadOnly = false,
				PrimaryTileCount = 640,
				PrimaryBlockCount = 640,
				PrimaryPaletteCount = 7,
				BehaviorBytes = 4,
				TilesetCallbackOffset = 16,
				TilesetBehaviorOffset = 20
			};
		}

		//-------------------------------------------------------------------------------
		// エメラルドの定義を作る処理（タイル・ブロックは 512:512、パレットは 6:7、挙動は 2 バイト）
		//-------------------------------------------------------------------------------
		private static GameProfile Emerald(string code, string name)
		{
			return new GameProfile
			{
				Code = code,
				DisplayName = name,
				IsSupported = code == "BPEE" || code == "BPEJ",
				SupportNote = (code == "BPEE" || code == "BPEJ") ? "エメラルドはマップの確定と ROM の保存に対応しています。" : "エメラルド対応は段階的に進めている途中です（現在は判別のみ）。",
				IniFileName = "Rom_" + code + ".ini",
				// 日本語版は FR と同じ日本語の文字表、英語版は英語の文字表
				CharTableFile = code == "BPEJ" ? "charmap.tbl" : "charmap_en.tbl",
				HasBorderSize = false,
				// 英語版の名前は長い（EVER GRANDE CITY など）。日本語版は FR と同じ長さまで
				MapNameMaxLength = code == "BPEJ" ? 11 : 16,
				EmeraldHeaderLayout = true,
				EmeraldRegionMap = true,
				SupportsTripleLayer = false,
				MapBankLimitFile = null,
				UseTileset2BlockLimitIni = false,
				// 元のエメラルドはマップの確定と ROM の保存まで対応（E3）。改造版の特殊な形は MapEditor.IsRomReadOnly で表示専用にする
				IsReadOnly = false,
				LimitedEditing = true,
				PrimaryTileCount = 512,
				PrimaryBlockCount = 512,
				PrimaryPaletteCount = 6,
				BehaviorBytes = 2,
				TilesetCallbackOffset = 20,
				TilesetBehaviorOffset = 16
			};
		}

		//-------------------------------------------------------------------------------
		// ルビー／サファイアの定義を作る処理（タイルセットの構造はエメラルドと同じ）
		//-------------------------------------------------------------------------------
		private static GameProfile RubySapphire(string code, string name)
		{
			GameProfile profile = Emerald(code, name);
			profile.SupportNote = "ルビー・サファイアは今後の対応候補です。";
			return profile;
		}

		//-------------------------------------------------------------------------------
		// ROM ヘッダーからゲームコードを読む処理（読めなければ空文字）
		//-------------------------------------------------------------------------------
		public static string ReadGameCode(byte[] rom)
		{
			if (rom == null || rom.Length < 0xC0)
			{
				return string.Empty;
			}
			string code = Encoding.ASCII.GetString(rom, 0xAC, 4);
			return code.All(c => c >= 0x20 && c < 0x7F) ? code : string.Empty;
		}

		//-------------------------------------------------------------------------------
		// ROM ヘッダーのタイトル（0xA0 からの 12 文字）を読む処理
		//-------------------------------------------------------------------------------
		public static string ReadTitle(byte[] rom)
		{
			if (rom == null || rom.Length < 0xC0)
			{
				return string.Empty;
			}
			return new string(Encoding.ASCII.GetString(rom, 0xA0, 12).Where(c => c >= 0x20 && c < 0x7F).ToArray()).Trim();
		}

		//-------------------------------------------------------------------------------
		// ROM に対応する定義を探す処理（知らないゲームなら null）
		//-------------------------------------------------------------------------------
		public static GameProfile Detect(byte[] rom)
		{
			string code = ReadGameCode(rom);
			return known.FirstOrDefault(p => p.Code == code);
		}

		//-------------------------------------------------------------------------------
		// 読み込んだ ROM の定義を「現在のゲーム」として設定する処理
		//-------------------------------------------------------------------------------
		public static void SetCurrent(GameProfile profile)
		{
			if (profile != null)
			{
				Current = profile;
			}
		}

		//-------------------------------------------------------------------------------
		// 対応していない ROM を開こうとしたときの案内文を作る処理
		//-------------------------------------------------------------------------------
		public static string BuildUnsupportedMessage(byte[] rom, GameProfile profile)
		{
			string code = ReadGameCode(rom);
			string title = ReadTitle(rom);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(Localizer.T("この ROM は現在のエディタでは開けません。"));
			sb.AppendLine();
			sb.AppendLine(Localizer.F("ROM のタイトル : {0}", string.IsNullOrEmpty(title) ? Localizer.T("（読めません）") : title));
			sb.AppendLine(Localizer.F("ゲームコード : {0}", string.IsNullOrEmpty(code) ? Localizer.T("（読めません）") : code));
			sb.AppendLine(Localizer.F("判別結果 : {0}", profile != null ? Localizer.T(profile.DisplayName) : Localizer.T("不明なゲーム（GBA の ROM ではない可能性があります）")));
			if (profile != null && !string.IsNullOrEmpty(profile.SupportNote))
			{
				sb.AppendLine();
				sb.AppendLine(Localizer.T(profile.SupportNote));
			}
			sb.AppendLine();
			sb.Append(Localizer.F("現在対応しているのは {0} です。", string.Join(Localizer.T("、"), known.Where(p => p.IsSupported).Select(p => Localizer.T(p.DisplayName) + " (" + p.Code + ")"))));
			return sb.ToString();
		}
	}
}
