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
			if (CliCommandRunner.TryRun(args))
			{
				return;
			}
			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
		}
	}
}
