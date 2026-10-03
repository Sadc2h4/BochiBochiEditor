using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// exe と同じフォルダの settings.ini（「名前=値」の行）を読み書きするクラス
	// 1 つの値を書き換えても、ほかの行はそのまま残す
	//-------------------------------------------------------------------------------
	internal static class AppSettings
	{
		//-------------------------------------------------------------------------------
		// 設定ファイルの場所
		//-------------------------------------------------------------------------------
		public static string SettingsPath
		{
			get { return Path.Combine(AppContext.BaseDirectory, "settings.ini"); }
		}

		//-------------------------------------------------------------------------------
		// 値を読む処理（無い・読めないときは fallback）
		//-------------------------------------------------------------------------------
		public static string Get(string name, string fallback = null)
		{
			try
			{
				if (!File.Exists(SettingsPath))
				{
					return fallback;
				}
				foreach (string line in File.ReadAllLines(SettingsPath, Encoding.UTF8))
				{
					if (line.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
					{
						return line.Substring(name.Length + 1).Trim();
					}
				}
			}
			catch (Exception)
			{
				// 読めなければ既定値を使う
			}
			return fallback;
		}

		//-------------------------------------------------------------------------------
		// 値を書く処理（同じ名前の行があれば置き換え、無ければ末尾に足す。書けなくても例外にしない）
		//-------------------------------------------------------------------------------
		public static void Set(string name, string value)
		{
			try
			{
				List<string> lines = File.Exists(SettingsPath) ? new List<string>(File.ReadAllLines(SettingsPath, Encoding.UTF8)) : new List<string>();
				int index = lines.FindIndex(line => line.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase));
				string entry = name + "=" + value;
				if (index >= 0)
				{
					lines[index] = entry;
				}
				else
				{
					lines.Add(entry);
				}
				File.WriteAllLines(SettingsPath, lines, Encoding.UTF8);
			}
			catch (Exception)
			{
				// 保存できなくても動作は続ける
			}
		}
	}
}
