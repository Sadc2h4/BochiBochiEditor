namespace BochiBochiEditor
{
	internal static class Program
	{
		//-------------------------------------------------------------------------------
		// GUI起動またはCLI起動へ振り分ける処理
		//-------------------------------------------------------------------------------
		[STAThread]
		static void Main(string[] args)
		{
			bool isRomFileArg = args != null && args.Length == 1 && File.Exists(args[0]) && string.Equals(Path.GetExtension(args[0]), ".gba", StringComparison.OrdinalIgnoreCase);
			if (isRomFileArg)
			{
				MapEditor.StartupRomPath = args[0];
			}
			else if (CliCommandRunner.TryRun(args))
			{
				return;
			}
			// 画面の処理で受け止められていない例外は、記録して案内を 1 回だけ出す（同じ窓が何百回も出て操作できなくなるのを防ぐ）
			AppErrorGuard.Install();
			ApplicationConfiguration.Initialize();
			Application.Run(new MapEditor());
		}
	}
}
