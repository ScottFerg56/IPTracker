namespace IPTracker
{
	internal static class Program
	{
		[STAThread]
		static async Task Main(string[] args)
		{
			if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
			{
				await HeadlessScan.RunAsync();
				return;
			}

			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
		}
	}
}
