using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	internal static class AppIconHelper
	{
		private static Icon appIcon;

		//-------------------------------------------------------------------------------
		// フォームにBochiBochiEditor共通アイコンを設定する処理
		//-------------------------------------------------------------------------------
		public static void Apply(Form form)
		{
			bool flag = form == null;
			if (flag)
			{
				return;
			}
			Icon icon = LoadIcon();
			bool flag2 = icon != null;
			if (flag2)
			{
				form.Icon = icon;
			}
		}

		//-------------------------------------------------------------------------------
		// Bochi_icon.icoを探索して読み込む処理
		//-------------------------------------------------------------------------------
		private static Icon LoadIcon()
		{
			bool flag = AppIconHelper.appIcon != null;
			if (flag)
			{
				return AppIconHelper.appIcon;
			}
			try
			{
				string text = AppAssetLocator.FindRequiredFile("Bochi_icon.ico");
				bool flag2 = File.Exists(text);
				if (flag2)
				{
					AppIconHelper.appIcon = new Icon(text);
				}
			}
			catch (Exception)
			{
				AppIconHelper.appIcon = null;
			}
			return AppIconHelper.appIcon;
		}
	}
}
