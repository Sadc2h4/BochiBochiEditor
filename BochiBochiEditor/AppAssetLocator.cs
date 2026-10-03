using System;
using System.Collections.Generic;
using System.IO;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// アプリ実行位置から設定ファイルや補助ファイルを探索する処理
	//-------------------------------------------------------------------------------
	internal static class AppAssetLocator
	{
		// 配布フォルダで exe 以外の補助ファイル（img・ini・lang・txt・アイコン）をまとめるフォルダの名前
		public const string ResourceFolderName = "resource";

		//-------------------------------------------------------------------------------
		// 補助ファイルをまとめるフォルダ（exe と同じ場所の resource）のフルパスを返す処理
		//-------------------------------------------------------------------------------
		public static string ResourceDirectory
		{
			get { return Path.Combine(AppContext.BaseDirectory, ResourceFolderName); }
		}

		//-------------------------------------------------------------------------------
		// 相対パスのファイルを探し、見つからなければ resource の中の既定の場所を返す処理（任意のファイル・書き込み先用）
		//-------------------------------------------------------------------------------
		public static string GetPathOrDefault(string relativePath)
		{
			foreach (string text in EnumerateSearchRoots())
			{
				string text2 = Path.Combine(text, relativePath);
				if (File.Exists(text2))
				{
					return text2;
				}
			}
			return Path.Combine(ResourceDirectory, relativePath);
		}

		//-------------------------------------------------------------------------------
		// 相対パスの必須ファイルを探索してフルパスを返す処理
		//-------------------------------------------------------------------------------
		public static string FindRequiredFile(string relativePath)
		{
			foreach (string text in EnumerateSearchRoots())
			{
				string text2 = Path.Combine(text, relativePath);
				if (File.Exists(text2))
				{
					return text2;
				}
			}
			string fileName = Path.GetFileName(relativePath);
			foreach (string text3 in EnumerateSearchRoots())
			{
				string text4 = Path.Combine(text3, fileName);
				if (File.Exists(text4))
				{
					return text4;
				}
			}
			throw new FileNotFoundException("必要なファイルが見つかりません。", relativePath);
		}

		//-------------------------------------------------------------------------------
		// 相対パスの存在可否を返す処理
		//-------------------------------------------------------------------------------
		public static bool Exists(string relativePath)
		{
			foreach (string text in EnumerateSearchRoots())
			{
				if (File.Exists(Path.Combine(text, relativePath)))
				{
					return true;
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 探索対象のルート候補を列挙する処理（各階層で resource の中 → その階層そのものの順）
		//-------------------------------------------------------------------------------
		private static IEnumerable<string> EnumerateSearchRoots()
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string startPath in new string[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
			{
				foreach (string text in EnumerateParentDirectories(startPath))
				{
					string text2 = Path.Combine(text, ResourceFolderName);
					if (hashSet.Add(text2))
					{
						yield return text2;
					}
					if (hashSet.Add(text))
					{
						yield return text;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 指定ディレクトリから親階層へ順に探索候補を列挙する処理
		//-------------------------------------------------------------------------------
		private static IEnumerable<string> EnumerateParentDirectories(string startPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetFullPath(startPath));
			while (directoryInfo != null)
			{
				yield return directoryInfo.FullName;
				directoryInfo = directoryInfo.Parent;
			}
		}

	}
}
