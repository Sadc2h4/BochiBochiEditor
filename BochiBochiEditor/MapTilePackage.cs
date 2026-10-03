using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// マップタイルの移植用に書き出すデータ（map_info.txt とタイル画像）の入れ物と、その読み書き
	// map_info.txt は「[区分]」と「名前=値」の行でできたテキスト。数値は 16 進（ブロック・挙動・パレットは空白区切り）
	// タイルの絵は map_info.txt ではなく、タイルセットごとの 16 色 PNG（tileset1.png・tileset2.png）に入れる
	// （絵を画像ソフトで直してから取り込めるように、取り込みでは PNG の方を読む）
	//-------------------------------------------------------------------------------
	internal sealed class MapTilePackage
	{
		// map_info.txt のファイル名と、書式の版
		public const string InfoFileName = "map_info.txt";
		public const int FormatVersion = 1;

		//-------------------------------------------------------------------------------
		// タイルセット 1 つ分のデータ
		//-------------------------------------------------------------------------------
		internal sealed class TilesetPart
		{
			// 書き出し元のタイルセット番号
			public int Index;
			// タイル画像を圧縮して持っていたか
			public bool Compressed;
			// タイルの枚数（PNG の高さから決まる）
			public int TileCount;
			// 4bpp のタイル画像（展開済み。1 枚 32 バイト）
			public byte[] TileImage;
			// タイル画像の PNG のファイル名
			public string ImageFile;
			// パレット 16 本分（16 本 × 32 バイト = 512 バイト。GBA の 15 ビット色）
			public byte[] Palettes = new byte[512];
			// ブロック数と、ブロック表・挙動表（ゲームの形のバイト数そのまま）
			public int BlockCount;
			public byte[] Blocks;
			public byte[] Behaviors;
			// 見出しの「アニメ処理」（ROM の中の位置。無ければ 0）と、そこから読めたタイルアニメ（コマの絵は PNG に入れる）
			public uint AnimationCallback;
			public readonly List<AnimationPart> Animations = new List<AnimationPart>();
		}

		//-------------------------------------------------------------------------------
		// タイルアニメ 1 つ分のデータ（差し替え先・枚数・速さ・コマごとの絵）
		//-------------------------------------------------------------------------------
		internal sealed class AnimationPart
		{
			public int DestTile;
			public int TileCount;
			public int Divisor;
			public int Delay;
			// コマの絵の PNG のファイル名（横 = 枚数 × 8、縦 = コマ数 × 8 の 16 色 PNG）
			public string ImageFile;
			// コマごとの絵（1 コマ = TileCount × 32 バイト）
			public List<byte[]> Frames = new List<byte[]>();
		}

		// 書き出したゲーム（BPRJ など）と、タイルセットの形
		public string GameCode = "";
		public int PrimaryTileCount;
		public int PrimaryBlockCount;
		public int PrimaryPaletteCount;
		public int BlockBytes;
		public int BehaviorBytes;

		// 書き出し元のマップ（参考用。取り込みでは使わない）
		public int Bank;
		public int Number;
		public string MapName = "";
		public int MapWidth;
		public int MapHeight;
		public int BorderWidth;
		public int BorderHeight;
		// マップの並びとボーダー（1 マス 2 バイト。下位 10 ビットがブロック番号、上位 6 ビットが移動の可否）
		public ushort[] Layout = new ushort[0];
		public ushort[] Border = new ushort[0];

		// [0] = タイルセット1、[1] = タイルセット2（無ければ null）
		public readonly TilesetPart[] Tilesets = new TilesetPart[2];

		//-------------------------------------------------------------------------------
		// 取り込み先のゲームとタイルセットの形が同じかを確かめる処理（同じなら null、違えば理由）
		//-------------------------------------------------------------------------------
		public string CheckCompatibleWithCurrentGame()
		{
			GameProfile profile = GameProfile.Current;
			if (this.PrimaryTileCount != profile.PrimaryTileCount || this.PrimaryBlockCount != profile.PrimaryBlockCount ||
				this.PrimaryPaletteCount != profile.PrimaryPaletteCount || this.BlockBytes != MapEditor.BLOCK_DATA_SIZE || this.BehaviorBytes != profile.BehaviorBytes)
			{
				return string.Format(Localizer.T("書き出し元（{0}：ブロック {1} バイト・挙動 {2} バイト・第1のタイル {3} 枚）と、開いている ROM（{4}：ブロック {5} バイト・挙動 {6} バイト・第1のタイル {7} 枚）のタイルセットの形が違うため、取り込めません。今は同じ種類のゲーム同士の移植だけに対応しています。"),
					this.GameCode, this.BlockBytes, this.BehaviorBytes, this.PrimaryTileCount,
					profile.Code, MapEditor.BLOCK_DATA_SIZE, profile.BehaviorBytes, profile.PrimaryTileCount);
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		// map_info.txt を書く処理（タイル画像の PNG は呼び出し側で書く）
		//-------------------------------------------------------------------------------
		public void SaveInfo(string folder)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("# BochiBochiEditor マップタイルの書き出し");
			sb.AppendLine("# 「マップタイル」タブの「マップタイルをインポート」から取り込めます。数値は 16 進です。");
			sb.AppendLine("# タイルの絵は tileset1.png / tileset2.png（16 色）から読みます。絵を直すときは色の並び（番号）を変えないでください。");
			sb.AppendLine();
			sb.AppendLine("[Package]");
			sb.AppendLine("Version=" + FormatVersion);
			sb.AppendLine("Game=" + this.GameCode);
			sb.AppendLine("PrimaryTileCount=" + this.PrimaryTileCount);
			sb.AppendLine("PrimaryBlockCount=" + this.PrimaryBlockCount);
			sb.AppendLine("PrimaryPaletteCount=" + this.PrimaryPaletteCount);
			sb.AppendLine("BlockBytes=" + this.BlockBytes);
			sb.AppendLine("BehaviorBytes=" + this.BehaviorBytes);
			sb.AppendLine();
			sb.AppendLine("[Map]");
			sb.AppendLine("Bank=" + this.Bank);
			sb.AppendLine("Number=" + this.Number);
			sb.AppendLine("Name=" + this.MapName);
			sb.AppendLine("Width=" + this.MapWidth);
			sb.AppendLine("Height=" + this.MapHeight);
			sb.AppendLine("BorderWidth=" + this.BorderWidth);
			sb.AppendLine("BorderHeight=" + this.BorderHeight);
			sb.AppendLine("# Layout の 1 行はマップの 1 段。1 マス = ブロック番号（下位 10 ビット）＋移動の可否（上位 6 ビット）");
			AppendRows(sb, "Layout", this.Layout, this.MapWidth);
			AppendRows(sb, "Border", this.Border, this.BorderWidth);
			for (int t = 0; t < 2; t++)
			{
				TilesetPart part = this.Tilesets[t];
				if (part == null)
				{
					continue;
				}
				sb.AppendLine();
				sb.AppendLine(t == 0 ? "[Tileset1]" : "[Tileset2]");
				sb.AppendLine("Index=" + part.Index);
				sb.AppendLine("Compressed=" + (part.Compressed ? 1 : 0));
				sb.AppendLine("TileCount=" + part.TileCount);
				sb.AppendLine("Image=" + part.ImageFile);
				sb.AppendLine("BlockCount=" + part.BlockCount);
				sb.AppendLine("# AnimationCallback: 見出しの「アニメ処理」の場所（ROM の中の位置、16 進。0 なら無し）。同じゲームへ取り込むときは、取り込み先に同じ処理があれば引き継ぐ");
				sb.AppendLine("AnimationCallback=" + part.AnimationCallback.ToString("X", CultureInfo.InvariantCulture));
				sb.AppendLine("# AnimNN: 差し替え先のタイル番号,枚数,何フレームごと,コマのずれ,コマ数,コマの絵の PNG（横 = 枚数 × 8、縦 = コマ数 × 8）");
				for (int a = 0; a < part.Animations.Count; a++)
				{
					AnimationPart anim = part.Animations[a];
					sb.Append("Anim").Append(a.ToString("D2", CultureInfo.InvariantCulture)).Append('=');
					sb.AppendLine(string.Join(",", new[] { anim.DestTile.ToString(CultureInfo.InvariantCulture), anim.TileCount.ToString(CultureInfo.InvariantCulture), anim.Divisor.ToString(CultureInfo.InvariantCulture), anim.Delay.ToString(CultureInfo.InvariantCulture), anim.Frames.Count.ToString(CultureInfo.InvariantCulture), anim.ImageFile }));
				}
				sb.AppendLine("# Palette00〜15: 1 本 16 色（GBA の 15 ビット色）。このタイルセットが受け持つのは " +
					(t == 0 ? "0〜" + (this.PrimaryPaletteCount - 1) : this.PrimaryPaletteCount + "〜12") + " 番");
				for (int p = 0; p < 16; p++)
				{
					sb.Append("Palette").Append(p.ToString("D2", CultureInfo.InvariantCulture)).Append('=');
					sb.AppendLine(JoinHex(part.Palettes, p * 32, 32, 2));
				}
				sb.AppendLine("# BlockNNN: 1 ブロックのタイル指定（1 つ 2 バイト：タイル番号 10 ビット・左右反転・上下反転・パレット番号 4 ビット）");
				sb.AppendLine("# BehaviorNNN: 挙動データ（" + this.BehaviorBytes + " バイト）");
				for (int b = 0; b < part.BlockCount; b++)
				{
					sb.Append("Block").Append(b.ToString("D3", CultureInfo.InvariantCulture)).Append('=');
					sb.AppendLine(JoinHex(part.Blocks, b * this.BlockBytes, this.BlockBytes, 2));
				}
				for (int b = 0; b < part.BlockCount; b++)
				{
					sb.Append("Behavior").Append(b.ToString("D3", CultureInfo.InvariantCulture)).Append('=');
					sb.AppendLine(JoinHex(part.Behaviors, b * this.BehaviorBytes, this.BehaviorBytes, this.BehaviorBytes));
				}
			}
			File.WriteAllText(Path.Combine(folder, InfoFileName), sb.ToString(), new UTF8Encoding(false));
		}

		//-------------------------------------------------------------------------------
		// 書き出したフォルダ（map_info.txt とタイル画像の PNG）を読む処理（読めなければ例外。メッセージは利用者向け）
		//-------------------------------------------------------------------------------
		public static MapTilePackage Load(string folder)
		{
			string path = Path.Combine(folder, InfoFileName);
			if (!File.Exists(path))
			{
				throw new InvalidDataException(string.Format(Localizer.T("{0} が見つかりません。「マップタイルをエクスポート」で作ったフォルダを選んでください。"), InfoFileName));
			}
			Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, string> current = null;
			foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
				{
					continue;
				}
				if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
				{
					current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
					sections[line.Substring(1, line.Length - 2)] = current;
					continue;
				}
				int eq = line.IndexOf('=');
				if (current != null && eq > 0)
				{
					current[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
				}
			}
			Dictionary<string, string> package = Section(sections, "Package");
			MapTilePackage result = new MapTilePackage
			{
				GameCode = Text(package, "Game"),
				PrimaryTileCount = Int(package, "PrimaryTileCount"),
				PrimaryBlockCount = Int(package, "PrimaryBlockCount"),
				PrimaryPaletteCount = Int(package, "PrimaryPaletteCount"),
				BlockBytes = Int(package, "BlockBytes"),
				BehaviorBytes = Int(package, "BehaviorBytes")
			};
			if (Int(package, "Version") > FormatVersion)
			{
				throw new InvalidDataException(Localizer.T("このエディタより新しい版で書き出したデータです。エディタを新しくしてください。"));
			}
			if (result.BlockBytes != 16 && result.BlockBytes != 24 || result.BehaviorBytes != 2 && result.BehaviorBytes != 4)
			{
				throw new InvalidDataException(Localizer.T("map_info.txt のブロック・挙動のバイト数が読めません。"));
			}
			Dictionary<string, string> map;
			if (sections.TryGetValue("Map", out map))
			{
				result.Bank = Int(map, "Bank", 0);
				result.Number = Int(map, "Number", 0);
				result.MapName = Text(map, "Name");
				result.MapWidth = Int(map, "Width", 0);
				result.MapHeight = Int(map, "Height", 0);
				result.BorderWidth = Int(map, "BorderWidth", 0);
				result.BorderHeight = Int(map, "BorderHeight", 0);
				result.Layout = ReadRows(map, "Layout", result.MapWidth, result.MapHeight);
				result.Border = ReadRows(map, "Border", result.BorderWidth, result.BorderHeight);
			}
			for (int t = 0; t < 2; t++)
			{
				Dictionary<string, string> s;
				if (!sections.TryGetValue(t == 0 ? "Tileset1" : "Tileset2", out s))
				{
					continue;
				}
				TilesetPart part = new TilesetPart
				{
					Index = Int(s, "Index", -1),
					Compressed = Int(s, "Compressed", 1) != 0,
					ImageFile = Text(s, "Image"),
					BlockCount = Int(s, "BlockCount")
				};
				for (int p = 0; p < 16; p++)
				{
					byte[] pal = ParseHex(Text(s, "Palette" + p.ToString("D2", CultureInfo.InvariantCulture)), 2, 16);
					Array.Copy(pal, 0, part.Palettes, p * 32, 32);
				}
				part.Blocks = new byte[part.BlockCount * result.BlockBytes];
				part.Behaviors = new byte[part.BlockCount * result.BehaviorBytes];
				for (int b = 0; b < part.BlockCount; b++)
				{
					string key = b.ToString("D3", CultureInfo.InvariantCulture);
					Array.Copy(ParseHex(Text(s, "Block" + key), 2, result.BlockBytes / 2), 0, part.Blocks, b * result.BlockBytes, result.BlockBytes);
					Array.Copy(ParseHex(Text(s, "Behavior" + key), result.BehaviorBytes, 1), 0, part.Behaviors, b * result.BehaviorBytes, result.BehaviorBytes);
				}
				// タイルアニメ（無い・読めないときは飛ばす。取り込みに必須ではない）
				string callback = Text(s, "AnimationCallback");
				uint callbackValue;
				if (uint.TryParse(callback, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out callbackValue))
				{
					part.AnimationCallback = callbackValue;
				}
				for (int a = 0; a < 64; a++)
				{
					string line = Text(s, "Anim" + a.ToString("D2", CultureInfo.InvariantCulture));
					if (line.Length == 0)
					{
						break;
					}
					string[] items = line.Split(',');
					if (items.Length < 6)
					{
						continue;
					}
					AnimationPart anim = new AnimationPart
					{
						DestTile = int.Parse(items[0], CultureInfo.InvariantCulture),
						TileCount = int.Parse(items[1], CultureInfo.InvariantCulture),
						Divisor = int.Parse(items[2], CultureInfo.InvariantCulture),
						Delay = int.Parse(items[3], CultureInfo.InvariantCulture),
						ImageFile = items[5].Trim(),
					};
					int frameCount = int.Parse(items[4], CultureInfo.InvariantCulture);
					string animPath = Path.Combine(folder, anim.ImageFile);
					if (anim.TileCount <= 0 || frameCount <= 0 || !File.Exists(animPath))
					{
						continue;
					}
					using (Bitmap bitmap = new Bitmap(animPath))
					{
						if (bitmap.PixelFormat != PixelFormat.Format4bppIndexed || bitmap.Width != anim.TileCount * 8 || bitmap.Height != frameCount * 8)
						{
							continue;
						}
						byte[] all = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
						for (int f = 0; f < frameCount; f++)
						{
							byte[] frame = new byte[anim.TileCount * 32];
							Array.Copy(all, f * frame.Length, frame, 0, frame.Length);
							anim.Frames.Add(frame);
						}
					}
					part.Animations.Add(anim);
				}
				string imagePath = Path.Combine(folder, part.ImageFile);
				if (!File.Exists(imagePath))
				{
					throw new InvalidDataException(string.Format(Localizer.T("タイル画像 {0} が見つかりません。"), part.ImageFile));
				}
				using (Bitmap bitmap = new Bitmap(imagePath))
				{
					if (bitmap.PixelFormat != PixelFormat.Format4bppIndexed || bitmap.Width != 128 || bitmap.Height % 8 != 0)
					{
						throw new InvalidDataException(string.Format(Localizer.T("タイル画像 {0} は、幅 128・高さ 8 の倍数の 16 色（4bpp）PNG である必要があります。"), part.ImageFile));
					}
					part.TileImage = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
					part.TileCount = part.TileImage.Length / 32;
				}
				result.Tilesets[t] = part;
			}
			if (result.Tilesets[0] == null && result.Tilesets[1] == null)
			{
				throw new InvalidDataException(Localizer.T("map_info.txt にタイルセットのデータがありません。"));
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// GBA の 15 ビット色の 16 本分のパレットのうち 1 本を、画面用の色 16 個にする処理
		//-------------------------------------------------------------------------------
		public static Color[] ToColors(byte[] palettes, int index)
		{
			Color[] colors = new Color[16];
			for (int c = 0; c < 16; c++)
			{
				ushort v = BitConverter.ToUInt16(palettes, index * 32 + c * 2);
				colors[c] = Color.FromArgb(255, (v & 0x1F) * 8, ((v >> 5) & 0x1F) * 8, ((v >> 10) & 0x1F) * 8);
			}
			return colors;
		}

		//-------------------------------------------------------------------------------
		// 2 バイト値の並びを「名前NN=…」の行にして足す処理（1 行 = columns 個）
		//-------------------------------------------------------------------------------
		private static void AppendRows(StringBuilder sb, string name, ushort[] values, int columns)
		{
			if (values == null || columns <= 0)
			{
				return;
			}
			for (int row = 0; row * columns < values.Length; row++)
			{
				sb.Append(name).Append(row.ToString("D3", CultureInfo.InvariantCulture)).Append('=');
				StringBuilder line = new StringBuilder();
				for (int x = 0; x < columns && row * columns + x < values.Length; x++)
				{
					if (x > 0)
					{
						line.Append(' ');
					}
					line.Append(values[row * columns + x].ToString("X4", CultureInfo.InvariantCulture));
				}
				sb.AppendLine(line.ToString());
			}
		}

		//-------------------------------------------------------------------------------
		// 「名前NNN=…」の行を読み、2 バイト値の並びに戻す処理（無い行は 0）
		//-------------------------------------------------------------------------------
		private static ushort[] ReadRows(Dictionary<string, string> section, string name, int width, int height)
		{
			if (width <= 0 || height <= 0)
			{
				return new ushort[0];
			}
			ushort[] values = new ushort[width * height];
			for (int row = 0; row < height; row++)
			{
				string text;
				if (!section.TryGetValue(name + row.ToString("D3", CultureInfo.InvariantCulture), out text))
				{
					continue;
				}
				byte[] bytes = ParseHex(text, 2, width);
				for (int x = 0; x < width; x++)
				{
					values[row * width + x] = BitConverter.ToUInt16(bytes, x * 2);
				}
			}
			return values;
		}

		//-------------------------------------------------------------------------------
		// バイト列を「size バイトずつの 16 進（リトルエンディアンの値）を空白でつないだ文字列」にする処理
		//-------------------------------------------------------------------------------
		private static string JoinHex(byte[] data, int offset, int length, int size)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < length; i += size)
			{
				if (i > 0)
				{
					sb.Append(' ');
				}
				ulong value = 0;
				for (int b = size - 1; b >= 0; b--)
				{
					value = (value << 8) | data[offset + i + b];
				}
				sb.Append(value.ToString("X" + (size * 2), CultureInfo.InvariantCulture));
			}
			return sb.ToString();
		}

		//-------------------------------------------------------------------------------
		// JoinHex の逆（size バイトの値を count 個読み、リトルエンディアンのバイト列にする）。数が合わなければ例外
		//-------------------------------------------------------------------------------
		private static byte[] ParseHex(string text, int size, int count)
		{
			string[] items = (text ?? "").Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (items.Length != count)
			{
				throw new InvalidDataException(string.Format(Localizer.T("map_info.txt の値の数が合いません（{0} 個のはずが {1} 個）: {2}"), count, items.Length, text));
			}
			byte[] result = new byte[size * count];
			for (int i = 0; i < count; i++)
			{
				ulong value = ulong.Parse(items[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				for (int b = 0; b < size; b++)
				{
					result[i * size + b] = (byte)(value >> (8 * b));
				}
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// 区分を取り出す処理（無ければ例外）
		//-------------------------------------------------------------------------------
		private static Dictionary<string, string> Section(Dictionary<string, Dictionary<string, string>> sections, string name)
		{
			Dictionary<string, string> s;
			if (!sections.TryGetValue(name, out s))
			{
				throw new InvalidDataException(string.Format(Localizer.T("map_info.txt に [{0}] がありません。"), name));
			}
			return s;
		}

		//-------------------------------------------------------------------------------
		// 文字の値を読む処理（無ければ空文字）
		//-------------------------------------------------------------------------------
		private static string Text(Dictionary<string, string> section, string key)
		{
			string value;
			return section.TryGetValue(key, out value) ? value : "";
		}

		//-------------------------------------------------------------------------------
		// 10 進の整数を読む処理（無ければ fallback。fallback が無ければ例外）
		//-------------------------------------------------------------------------------
		private static int Int(Dictionary<string, string> section, string key, int? fallback = null)
		{
			string value;
			int result;
			if (section.TryGetValue(key, out value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			if (fallback.HasValue)
			{
				return fallback.Value;
			}
			throw new InvalidDataException(string.Format(Localizer.T("map_info.txt の {0} が読めません。"), key));
		}
	}
}
