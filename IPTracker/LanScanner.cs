using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace IPTracker
{
    internal static class LanScanner
    {
        private const string BaseAddress = "192.168.0.";
        private const int PingTimeout = 1000;
        private const int MaxConcurrency = 50;

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(uint destIp, uint srcIp, byte[] macAddr, ref int macAddrLen);

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
                        IPAddress.Parse(ip),
                        TimeSpan.FromMilliseconds(PingTimeout),
                        cancellationToken: cancellationToken);
                    if (reply.Status == IPStatus.Success)
                    {
                        var mac = GetMacAddress(ip);
                        Debug.WriteLine($"{ip}  {mac}");
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
            var macAddr = new byte[6];
            var macAddrLen = macAddr.Length;
            var destIp = BitConverter.ToUInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);

            if (SendARP(destIp, 0, macAddr, ref macAddrLen) != 0)
                return "unknown";

            return string.Join(":", macAddr.Take(macAddrLen).Select(b => b.ToString("X2")));
        }
    }
}
