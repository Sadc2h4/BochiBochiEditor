using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 画面の処理の中で起きた、受け止められていない例外をまとめて受け止める処理
	// .NET の標準の窓（「続行」を押すもの）は、描画のたびに同じ例外が起きると何百回も出て操作できなくなる。
	// ここでは、内容を error.log に書き、案内を 1 回だけ出す。同じ例外が続く間は、窓を出さずに記録だけにする
	//-------------------------------------------------------------------------------
	internal static class AppErrorGuard
	{
		// 同じ例外とみなす間隔（この間に同じ内容がまた起きたら、窓は出さない）
		private static readonly TimeSpan QuietSpan = TimeSpan.FromSeconds(30);

		private static string lastKey;
		private static DateTime lastShown = DateTime.MinValue;
		private static int suppressed;
		private static bool showing;

		//-------------------------------------------------------------------------------
		// 受け止めの設定をする処理（画面を作る前に 1 回呼ぶ）
		//-------------------------------------------------------------------------------
		public static void Install()
		{
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += OnThreadException;
		}

		//-------------------------------------------------------------------------------
		// 例外を error.log（exe と同じフォルダ）に追記する処理（書けなくても続ける）
		//-------------------------------------------------------------------------------
		private static void WriteLog(Exception ex)
		{
			try
			{
				string path = Path.Combine(AppContext.BaseDirectory, "error.log");
				File.AppendAllText(path, string.Format("[{0:yyyy-MM-dd HH:mm:ss}] 画面の処理での例外{1}{2}{1}{1}", DateTime.Now, Environment.NewLine, ex));
			}
			catch (Exception)
			{
			}
		}

		//-------------------------------------------------------------------------------
		// 受け止められていない例外が起きたときの処理
		// 初めての内容は記録して案内を出す。同じ内容が 30 秒以内に続いたら、数えるだけにする（記録は 1 回目と、止めた後の最初の 1 回）
		//-------------------------------------------------------------------------------
		private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Exception ex = e.Exception;
			string key = ex.GetType().FullName + "|" + ex.Message + "|" + (ex.TargetSite != null ? ex.TargetSite.Name : string.Empty);
			DateTime now = DateTime.Now;
			if (showing || (key == lastKey && now - lastShown < QuietSpan))
			{
				// 案内を出している最中・出した直後の同じ例外は、窓を重ねない
				suppressed++;
				lastShown = now;
				return;
			}
			lastKey = key;
			lastShown = now;
			suppressed = 0;
			WriteLog(ex);
			showing = true;
			try
			{
				MessageBox.Show(
					Localizer.T("処理の途中でエラーが起きました。編集中の内容はそのまま残っています。") + Environment.NewLine + Environment.NewLine
					+ ex.Message + Environment.NewLine + Environment.NewLine
					+ Localizer.T("詳しい内容は、exe と同じフォルダの error.log に書きました。同じエラーが続く間は、この案内は出しません。"),
					Localizer.T("エラー"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			finally
			{
				showing = false;
				lastShown = DateTime.Now;
			}
		}
	}
}
