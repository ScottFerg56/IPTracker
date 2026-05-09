namespace IPTracker
{
    internal static class HeadlessScan
    {
        public static async Task RunAsync(string? filePathOverride = null)
        {
            var settings = AppSettings.Load();
            var xmlPath  = filePathOverride
                ?? (!string.IsNullOrEmpty(settings.XmlFilePath)
                    ? settings.XmlFilePath
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IPTracker.xml"));

            if (filePathOverride != null &&
                !string.Equals(settings.XmlFilePath, filePathOverride, StringComparison.OrdinalIgnoreCase))
            {
                settings.XmlFilePath = filePathOverride;
                settings.Save();
            }

            if (!File.Exists(xmlPath))
            {
                Log(xmlPath, "Headless scan aborted: file not found");
                return;
            }

            var (devices, scanRange) = NetworkDevice.LoadFromXml(xmlPath);

            Log(xmlPath, $"--- Scan started {DateTime.Now:yyyy-MM-dd HH:mm:ss} (headless) ---");

            var activeBeforeScan = devices
                .Where(d => d.Active)
                .Select(d => d.MacAddress)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var d in devices) d.Active = false;

            await OuiLookup.EnsureInitializedAsync();

            bool anyChanges = false;

            await foreach (var (ip, mac, hostName) in LanScanner.ScanAsync(scanRange))
                anyChanges |= Merge(devices, ip, mac, hostName, activeBeforeScan, xmlPath);

            foreach (var d in devices.Where(d => !d.Active && activeBeforeScan.Contains(d.MacAddress)))
            {
                Log(xmlPath, $"{d.LogTag()}  Inactive: {d.IpAddress}");
                anyChanges = true;
            }

            Log(xmlPath, $"--- Scan finished {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");

            if (anyChanges)
                NetworkDevice.SaveToXml(devices, scanRange, xmlPath);
        }

        private static void Log(string xmlPath, string message)
        {
            try
            {
                File.AppendAllText(
                    Path.ChangeExtension(xmlPath, ".log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
            catch { }
        }

        private static bool Merge(List<NetworkDevice> devices, string ip, string? mac, string? hostName,
            HashSet<string> activeBeforeScan, string xmlPath)
        {
            void L(string msg) => Log(xmlPath, msg);

            var byMac = mac != null
                ? devices.FirstOrDefault(d => string.Equals(d.MacAddress, mac, StringComparison.OrdinalIgnoreCase))
                : null;
            var byIp = devices.FirstOrDefault(d => string.Equals(d.IpAddress, ip, StringComparison.OrdinalIgnoreCase));

            if (mac == null)
            {
                if (byIp != null && !byIp.Active)
                {
                    byIp.Active = true;
                    if (!activeBeforeScan.Contains(byIp.MacAddress))
                        L($"{byIp.LogTag()}  Active: {ip}");
                    return true;
                }
                return false;
            }

            bool changed = false;
            NetworkDevice? device = null;

            if (byMac == null)
            {
                device = new NetworkDevice { IpAddress = ip, MacAddress = mac, Active = true };
                devices.Add(device);
                L($"{device.LogTag()}  Added: {ip}");
                changed = true;
            }
            else
            {
                device = byMac;
                if (!activeBeforeScan.Contains(device.MacAddress))
                {
                    L($"{device.LogTag()}  Active: {ip}");
                    changed = true;
                }
                device.Active = true;
                if (!string.Equals(byMac.IpAddress, ip, StringComparison.OrdinalIgnoreCase))
                {
                    L($"{device.LogTag()}  IP: {byMac.IpAddress} -> {ip}");
                    byMac.IpAddress = ip;
                    changed = true;
                }
            }

            if (byIp != null && byIp != byMac)
            {
                L($"{byIp.LogTag()}  IP cleared: {ip}");
                byIp.IpAddress = string.Empty;
                changed = true;
            }

            if (device != null && hostName != null &&
                !string.Equals(device.Name, hostName, StringComparison.OrdinalIgnoreCase))
            {
                L($"{device.LogTag()}  Name: '{device.Name}' -> '{hostName}'");
                device.Name = hostName;
                changed = true;
            }

            if (device != null && string.IsNullOrEmpty(device.Manufacturer))
            {
                var manufacturer = OuiLookup.GetManufacturer(mac);
                if (manufacturer != null)
                {
                    L($"{device.LogTag()}  Manufacturer: '{manufacturer}'");
                    device.Manufacturer = manufacturer;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
