using System.Diagnostics;
using System.Net.NetworkInformation;

namespace IPTracker
{
    internal static class LanScanner
    {
        private const string BaseAddress = "192.168.0.";
        private const int PingTimeout = 1000;
        private const int MaxConcurrency = 50;

        public static async Task ScanAsync(CancellationToken cancellationToken = default)
        {
            var semaphore = new SemaphoreSlim(MaxConcurrency);

            var tasks = Enumerable.Range(1, 255).Select(async i =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var ip = BaseAddress + i;
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(
                        System.Net.IPAddress.Parse(ip),
                        TimeSpan.FromMilliseconds(PingTimeout),
                        cancellationToken: cancellationToken);
                    if (reply.Status == IPStatus.Success)
                    {
                        var mac = GetMacAddress(ip);
                        Console.WriteLine($"{ip}  {mac}");
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch { }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        private static string GetMacAddress(string ip)
        {
            try
            {
                var psi = new ProcessStartInfo("arp", $"-a {ip}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var proc = Process.Start(psi)!;
                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                foreach (var line in output.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith(ip))
                    {
                        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                            return parts[1];
                    }
                }
            }
            catch { }
            return "unknown";
        }
    }
}
