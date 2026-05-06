using System.Xml.Linq;

namespace IPTracker
{
    public class NetworkDevice
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;

        public static List<NetworkDevice> LoadFromXml(string path)
        {
            var doc = XDocument.Load(path);
            return doc.Root!
                .Elements("row")
                .Select(e => new NetworkDevice
                {
                    Name         = (string?)e.Attribute("name")        ?? string.Empty,
                    IpAddress    = (string?)e.Attribute("ip")           ?? string.Empty,
                    Manufacturer = (string?)e.Attribute("manufacturer") ?? string.Empty,
                    MacAddress   = (string?)e.Attribute("mac")          ?? string.Empty,
                    Comments     = (string?)e.Attribute("comments")     ?? string.Empty,
                })
                .ToList();
        }

        public static void SaveToXml(List<NetworkDevice> devices, string path)
        {
            new XDocument(
                new XElement("rows",
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
