using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace IPTracker
{
    internal static class LanScanner
    {
        private const int PingTimeout = 1000;
        private const int MaxConcurrency = 50;

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(uint destIp, uint srcIp, byte[] macAddr, ref int macAddrLen);

        public static async IAsyncEnumerable<(string Ip, string? Mac, string? HostName)> ScanAsync(
            ScanRange range,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateUnbounded<(string Ip, string? Mac, string? HostName)>();
            var semaphore = new SemaphoreSlim(MaxConcurrency);

            var producer = Task.Run(async () =>
            {
                try
                {
                    var tasks = Enumerable.Range(range.Start, range.End - range.Start + 1).Select(async i =>
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            var ip = range.BaseAddress + i;
                            using var ping = new Ping();
                            var reply = await ping.SendPingAsync(
                                IPAddress.Parse(ip),
                                TimeSpan.FromMilliseconds(PingTimeout),
                                cancellationToken: cancellationToken);
                            if (reply.Status == IPStatus.Success)
                            {
                                var mac      = GetMacAddress(ip);
                                var hostName = await GetHostNameAsync(ip, cancellationToken);
                                await channel.Writer.WriteAsync((ip, mac, hostName), cancellationToken);
                            }
                        }
                        catch (OperationCanceledException) { throw; }
                        catch { }
                        finally { semaphore.Release(); }
                    });
                    await Task.WhenAll(tasks);
                }
                finally
                {
                    channel.Writer.Complete();
                }
            }, cancellationToken);

            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
                yield return item;

            await producer;
        }

        private static string? GetMacAddress(string ip)
        {
            var macAddr = new byte[6];
            var macAddrLen = macAddr.Length;
            var destIp = BitConverter.ToUInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);

            if (SendARP(destIp, 0, macAddr, ref macAddrLen) != 0)
                return null;

            return string.Join(":", macAddr.Take(macAddrLen).Select(b => b.ToString("X2")));
        }

        private static async Task<string?> GetHostNameAsync(string ip, CancellationToken cancellationToken)
        {
            try
            {
                var entry    = await Dns.GetHostEntryAsync(ip, cancellationToken);
                var hostName = entry.HostName;
                if (hostName.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
                    hostName = hostName[..^6];
                return hostName;
            }
            catch
            {
                return null;
            }
        }
    }
}
