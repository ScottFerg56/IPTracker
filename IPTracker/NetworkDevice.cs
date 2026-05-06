using System.Xml.Linq;

namespace IPTracker
{
    public record ScanRange(string BaseAddress = "192.168.0.", int Start = 1, int End = 255);

    public class NetworkDevice
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;

        public static (List<NetworkDevice> Devices, ScanRange Range) LoadFromXml(string path)
        {
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
                    Name         = (string?)e.Attribute("name")         ?? string.Empty,
                    IpAddress    = (string?)e.Attribute("ip")            ?? string.Empty,
                    Manufacturer = (string?)e.Attribute("manufacturer")  ?? string.Empty,
                    MacAddress   = (string?)e.Attribute("mac")           ?? string.Empty,
                    Comments     = (string?)e.Attribute("comments")      ?? string.Empty,
                })
                .ToList();

            return (devices, range);
        }

        public static void SaveToXml(List<NetworkDevice> devices, ScanRange range, string path)
        {
            new XDocument(
                new XElement("rows",
                    new XAttribute("base",  range.BaseAddress),
                    new XAttribute("start", range.Start),
                    new XAttribute("end",   range.End),
                    devices.Select(d =>
                        new XElement("row",
                            new XAttribute("name",         d.Name),
                            new XAttribute("ip",           d.IpAddress),
                            new XAttribute("manufacturer", d.Manufacturer),
                            new XAttribute("mac",          d.MacAddress),
                            new XAttribute("comments",     d.Comments)))))
            .Save(path);
        }
    }
}
