namespace IPTracker
{
	internal static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			int fi = Array.FindIndex(args, a => string.Equals(a, "--file", StringComparison.OrdinalIgnoreCase));
			string? fileArg = (fi >= 0 && fi + 1 < args.Length) ? args[fi + 1] : null;

			if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
			{
				HeadlessScan.RunAsync(fileArg).GetAwaiter().GetResult();
				return;
			}

			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm(fileArg));
		}
	}
}
