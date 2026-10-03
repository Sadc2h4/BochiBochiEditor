using System;
using System.IO;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// ROM ファイルの読み書き（ほかのアプリが同じ ROM を開いていても読めるようにし、失敗したら案内を出す）
	//-------------------------------------------------------------------------------
	internal static class RomFile
	{
		//-------------------------------------------------------------------------------
		// ROM を読み込む処理（AdvanceMap などが開いたままでも読めるよう、共有を許可して開く）
		// 読めなかったときは理由を表示して false を返す
		//-------------------------------------------------------------------------------
		public static bool TryRead(IWin32Window owner, string path, out byte[] data)
		{
			data = null;
			try
			{
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				{
					data = new byte[stream.Length];
					int offset = 0;
					while (offset < data.Length)
					{
						int read = stream.Read(data, offset, data.Length - offset);
						if (read <= 0)
						{
							break;
						}
						offset += read;
					}
				}
				return true;
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				MessageBox.Show(owner, string.Format(Localizer.T("ROM を読み込めませんでした。{0}{0}{1}{0}{0}{2}"), Environment.NewLine, path, ex.Message), Localizer.T("ROMを選択"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}

		//-------------------------------------------------------------------------------
		// ROM を書き出す処理（ほかのアプリが開いていて書けないときは、閉じてからやり直すよう案内して false を返す）
		//-------------------------------------------------------------------------------
		public static bool TryWrite(IWin32Window owner, string path, byte[] data)
		{
			try
			{
				File.WriteAllBytes(path, data);
				return true;
			}
			catch (IOException ex)
			{
				MessageBox.Show(owner, string.Format(Localizer.T("ROM を保存できませんでした。ほかのアプリ（AdvanceMap やエミュレーターなど）がこの ROM を開いている可能性があります。そのアプリで ROM を閉じてから、もう一度保存してください。{0}{0}{1}{0}{0}{2}"), Environment.NewLine, path, ex.Message), Localizer.T("ROMを保存"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
			catch (UnauthorizedAccessException ex)
			{
				MessageBox.Show(owner, string.Format(Localizer.T("ROM を保存できませんでした。書き込みが許可されていない場所か、読み取り専用のファイルです。{0}{0}{1}{0}{0}{2}"), Environment.NewLine, path, ex.Message), Localizer.T("ROMを保存"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}
	}
}
