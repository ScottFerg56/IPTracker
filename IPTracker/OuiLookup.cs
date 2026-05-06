namespace IPTracker
{
    internal static class OuiLookup
    {
        private static readonly string CachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IPTracker", "oui.csv");

        private const string OuiUrl      = "https://standards-oui.ieee.org/oui/oui.csv";
        private const int    CacheAgeDays = 30;

        private static Dictionary<string, string>? _lookup;

        public static async Task EnsureInitializedAsync()
        {
            if (_lookup != null) return;
            try
            {
                if (!File.Exists(CachePath) ||
                    (DateTime.Now - File.GetLastWriteTime(CachePath)).TotalDays > CacheAgeDays)
                    await DownloadAsync();

                _lookup = ParseCsv(CachePath);
            }
            catch
            {
                _lookup ??= [];
            }
        }

        public static string? GetManufacturer(string mac)
        {
            if (_lookup == null) return null;
            var oui = mac.Replace(":", "").Replace("-", "");
            if (oui.Length < 6) return null;
            return _lookup.TryGetValue(oui[..6], out var name) ? name : null;
        }

        private static async Task DownloadAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath)!);
            using var client = new HttpClient();
            var data = await client.GetStringAsync(OuiUrl);
            await File.WriteAllTextAsync(CachePath, data);
        }

        private static Dictionary<string, string> ParseCsv(string path)
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool first = true;
            foreach (var line in File.ReadLines(path))
            {
                if (first) { first = false; continue; } // skip header
                // Format: Registry,Assignment,Organization Name,Organization Address
                var parts = line.Split(',', 4);
                if (parts.Length >= 3)
                {
                    var oui  = parts[1].Trim().Trim('"');
                    var name = parts[2].Trim().Trim('"');
                    if (oui.Length == 6)
                        lookup[oui] = name;
                }
            }
            return lookup;
        }
    }
}
