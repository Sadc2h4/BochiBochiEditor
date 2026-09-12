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
			ApplicationConfiguration.Initialize();
			Application.Run(new MapEditor());
		}
	}
}
