using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 画面の表示言語（日本語／英語）を切り替える仕組み
	// 画面の文字は日本語のまま持ち、lang フォルダの翻訳表（日本語<TAB>英語）で置き換える
	// 翻訳表に無い文字は日本語のまま表示する
	//-------------------------------------------------------------------------------
	internal static class Localizer
	{
		public const string Japanese = "ja";
		public const string English = "en";

		private static Dictionary<string, string> table = new Dictionary<string, string>();
		private static readonly List<ToolTip> toolTips = new List<ToolTip>();

		// 部品ごとの「元の日本語」と「最後に表示した文字」
		private sealed class TextState
		{
			public string Original;
			public string Applied;
		}

		private static readonly ConditionalWeakTable<Control, TextState> texts = new ConditionalWeakTable<Control, TextState>();
		private static readonly ConditionalWeakTable<TextBox, TextState> placeholders = new ConditionalWeakTable<TextBox, TextState>();
		private static readonly ConditionalWeakTable<ComboBox, List<string>> comboOriginals = new ConditionalWeakTable<ComboBox, List<string>>();
		private static readonly Dictionary<ToolTip, ConditionalWeakTable<Control, TextState>> tipStates = new Dictionary<ToolTip, ConditionalWeakTable<Control, TextState>>();

		// 現在の表示言語
		public static string Language { get; private set; } = Japanese;

		// 言語が切り替わったときに通知する（動的に作る文字を作り直すため）
		public static event EventHandler LanguageChanged;

		//-------------------------------------------------------------------------------
		// 日本語の文字を、現在の言語の文字にして返す処理（翻訳が無ければそのまま）
		//-------------------------------------------------------------------------------
		public static string T(string japanese)
		{
			if (Language == Japanese || string.IsNullOrEmpty(japanese))
			{
				return japanese;
			}
			string translated;
			// 画面部品の文字は改行が \r\n のことがあるため、\n にそろえて探す
			string key = japanese.Replace("\r\n", "\n");
			return table.TryGetValue(key, out translated) ? translated : japanese;
		}

		//-------------------------------------------------------------------------------
		// 書式付きの文字（{0} など）を翻訳してから値を埋め込む処理
		//-------------------------------------------------------------------------------
		public static string F(string japaneseFormat, params object[] args)
		{
			return string.Format(T(japaneseFormat), args);
		}

		//-------------------------------------------------------------------------------
		// 保存されている言語設定を読み込んで適用する処理（起動時に 1 回呼ぶ）
		//-------------------------------------------------------------------------------
		public static void LoadSavedLanguage()
		{
			string language = Japanese;
			try
			{
				string path = SettingsPath;
				if (File.Exists(path))
				{
					foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
					{
						if (line.StartsWith("Language=", StringComparison.OrdinalIgnoreCase))
						{
							language = line.Substring("Language=".Length).Trim();
						}
					}
				}
			}
			catch (Exception)
			{
				language = Japanese;
			}
			SetLanguage(language, false);
		}

		//-------------------------------------------------------------------------------
		// 表示言語を切り替える処理（save が true なら次回起動用に保存する）
		//-------------------------------------------------------------------------------
		public static void SetLanguage(string language, bool save)
		{
			language = (language == English) ? English : Japanese;
			table = (language == English) ? LoadTable(English) : new Dictionary<string, string>();
			Language = language;
			if (save)
			{
				// ほかの設定（タウンマップをたたむかどうかなど）を消さないよう、この行だけを書き換える
				AppSettings.Set("Language", language);
			}
			LanguageChanged?.Invoke(null, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------------
		// 設定ファイルの場所（exe と同じフォルダの settings.ini）
		//-------------------------------------------------------------------------------
		private static string SettingsPath
		{
			get { return Path.Combine(AppContext.BaseDirectory, "settings.ini"); }
		}

		//-------------------------------------------------------------------------------
		// 翻訳表（lang\<言語>.txt、1 行 1 件「日本語<TAB>英語」、# で始まる行は注釈）を読む処理
		// 改行は \n と書く
		//-------------------------------------------------------------------------------
		private static Dictionary<string, string> LoadTable(string language)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			try
			{
				string path = AppAssetLocator.FindRequiredFile(Path.Combine("lang", language + ".txt"));
				foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
				{
					if (raw.Length == 0 || raw.StartsWith("#"))
					{
						continue;
					}
					int tab = raw.IndexOf('\t');
					if (tab <= 0)
					{
						continue;
					}
					string key = raw.Substring(0, tab).Replace("\\n", "\n");
					string value = raw.Substring(tab + 1).Replace("\\n", "\n");
					if (value.Length > 0)
					{
						result[key] = value;
					}
				}
			}
			catch (Exception)
			{
				// 翻訳表が無い場合は日本語のまま表示する
			}
			return result;
		}

		//-------------------------------------------------------------------------------
		// ツールチップを翻訳の対象として登録する処理
		//-------------------------------------------------------------------------------
		public static void RegisterToolTip(ToolTip toolTip)
		{
			if (toolTip != null && !toolTips.Contains(toolTip))
			{
				toolTips.Add(toolTip);
				tipStates[toolTip] = new ConditionalWeakTable<Control, TextState>();
			}
		}

		//-------------------------------------------------------------------------------
		// プルダウンの選択肢（日本語）を記録し、現在の言語で表示し直す処理（読み込み直後に呼ぶ）
		//-------------------------------------------------------------------------------
		public static void RegisterComboItems(ComboBox combo)
		{
			List<string> originals = new List<string>();
			foreach (object item in combo.Items)
			{
				originals.Add(item as string);
			}
			comboOriginals.AddOrUpdate(combo, originals);
			ApplyComboItems(combo);
		}

		//-------------------------------------------------------------------------------
		// 指定した部品とその子部品の文字・ツールチップを現在の言語にする処理
		//-------------------------------------------------------------------------------
		public static void Apply(Control root)
		{
			if (root == null)
			{
				return;
			}
			ApplyText(root);
			if (root is TextBox textBox)
			{
				ApplyPlaceholder(textBox);
			}
			if (root is ComboBox combo)
			{
				ApplyComboItems(combo);
			}
			foreach (ToolTip toolTip in toolTips)
			{
				ApplyToolTip(toolTip, root);
			}
			foreach (Control child in root.Controls)
			{
				Apply(child);
			}
		}

		//-------------------------------------------------------------------------------
		// 部品の表示文字を翻訳する処理（入力欄など、データを表す部品は対象外）
		//-------------------------------------------------------------------------------
		private static void ApplyText(Control control)
		{
			bool isCaption = control is Label || control is ButtonBase || control is GroupBox || control is TabPage || control is Form;
			if (!isCaption)
			{
				return;
			}
			TextState state = texts.GetValue(control, c => new TextState { Original = c.Text, Applied = c.Text });
			// 前回の表示から文字が変わっていたら、コード側で書き換えたものなので元の文字として取り直す
			if (control.Text != state.Applied)
			{
				state.Original = control.Text;
			}
			state.Applied = T(state.Original);
			if (control.Text != state.Applied)
			{
				control.Text = state.Applied;
			}
		}

		//-------------------------------------------------------------------------------
		// 入力欄の案内文（PlaceholderText）を翻訳する処理
		//-------------------------------------------------------------------------------
		private static void ApplyPlaceholder(TextBox textBox)
		{
			TextState state = placeholders.GetValue(textBox, t => new TextState { Original = t.PlaceholderText, Applied = t.PlaceholderText });
			if (textBox.PlaceholderText != state.Applied)
			{
				state.Original = textBox.PlaceholderText;
			}
			state.Applied = T(state.Original);
			textBox.PlaceholderText = state.Applied;
		}

		//-------------------------------------------------------------------------------
		// ツールチップの文字を翻訳する処理
		//-------------------------------------------------------------------------------
		private static void ApplyToolTip(ToolTip toolTip, Control control)
		{
			string current = toolTip.GetToolTip(control);
			if (string.IsNullOrEmpty(current))
			{
				return;
			}
			ConditionalWeakTable<Control, TextState> states = tipStates[toolTip];
			TextState state = states.GetValue(control, c => new TextState { Original = current, Applied = current });
			if (current != state.Applied)
			{
				state.Original = current;
			}
			state.Applied = T(state.Original);
			if (current != state.Applied)
			{
				toolTip.SetToolTip(control, state.Applied);
			}
		}

		//-------------------------------------------------------------------------------
		// プルダウンの選択肢を翻訳する処理（選択位置は保つ。先頭の [00] などの番号はそのまま）
		//-------------------------------------------------------------------------------
		private static void ApplyComboItems(ComboBox combo)
		{
			List<string> originals;
			if (!comboOriginals.TryGetValue(combo, out originals) || originals.Count != combo.Items.Count)
			{
				return;
			}
			int selected = combo.SelectedIndex;
			combo.BeginUpdate();
			for (int i = 0; i < originals.Count; i++)
			{
				if (originals[i] == null)
				{
					continue;
				}
				string text = T(originals[i]);
				if (!Equals(combo.Items[i], text))
				{
					combo.Items[i] = text;
				}
			}
			combo.EndUpdate();
			if (combo.SelectedIndex != selected && selected < combo.Items.Count)
			{
				combo.SelectedIndex = selected;
			}
		}
	}
}
