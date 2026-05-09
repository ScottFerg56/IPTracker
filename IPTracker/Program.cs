namespace IPTracker
{
	internal static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
			{
				HeadlessScan.RunAsync().GetAwaiter().GetResult();
				return;
			}

			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
		}
	}
}
