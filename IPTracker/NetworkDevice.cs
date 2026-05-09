using System.Xml.Linq;

namespace IPTracker
{
    public record ScanRange(string BaseAddress = "192.168.0.", int Start = 1, int End = 255);

    public class NetworkDevice
    {
        public bool   Active       { get; set; } = false;
        public string MacAddress   { get; set; } = string.Empty;
        public string IpAddress    { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Name         { get; set; } = string.Empty;
        public string Comments     { get; set; } = string.Empty;

        public string LogTag() => string.IsNullOrEmpty(Comments)
            ? MacAddress
            : $"{MacAddress} ({Comments})";

        public static (List<NetworkDevice> Devices, ScanRange Range) LoadFromXml(string path)
        {
            if (!File.Exists(path))
                return ([], new ScanRange());

            var doc = XDocument.Load(path);
            var root = doc.Root!;

            var range = new ScanRange(
                BaseAddress: (string?)root.Attribute("base")  ?? "192.168.0.",
                Start:       (int?)root.Attribute("start")    ?? 1,
                End:         (int?)root.Attribute("end")      ?? 255
            );

            var devices = root
                .Elements("row")
                .Select(e => new NetworkDevice
                {
                    Active       = (bool?)e.Attribute("active")          ?? false,
                    MacAddress   = (string?)e.Attribute("mac")           ?? string.Empty,
                    IpAddress    = (string?)e.Attribute("ip")            ?? string.Empty,
                    Manufacturer = (string?)e.Attribute("manufacturer")  ?? string.Empty,
                    Name         = (string?)e.Attribute("name")         ?? string.Empty,
                    Comments     = (string?)e.Attribute("comments")      ?? string.Empty,
                })
                .ToList();

            return (devices, range);
        }

        public static void SaveToXml(List<NetworkDevice> devices, ScanRange range, string path)
        {
            new XDocument(
                new XElement("devices",
                    new XAttribute("base",  range.BaseAddress),
                    new XAttribute("start", range.Start),
                    new XAttribute("end",   range.End),
                    devices.Select(d =>
                        new XElement("row",
                            new XAttribute("active",       d.Active),
                            new XAttribute("mac",          d.MacAddress   ?? string.Empty),
                            new XAttribute("ip",           d.IpAddress    ?? string.Empty),
                            new XAttribute("manufacturer", d.Manufacturer ?? string.Empty),
                            new XAttribute("name",         d.Name         ?? string.Empty),
                            new XAttribute("comments",     d.Comments     ?? string.Empty)))))
            .Save(path);
        }
    }
}
