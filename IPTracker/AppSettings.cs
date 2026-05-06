using System.Xml.Linq;

namespace IPTracker
{
    internal class AppSettings
    {
        public FormWindowState WindowState { get; set; } = FormWindowState.Normal;
        public int WindowX { get; set; } = 100;
        public int WindowY { get; set; } = 100;
        public int WindowWidth { get; set; } = 900;
        public int WindowHeight { get; set; } = 500;
        public string? SortColumn { get; set; }
        public bool SortAscending { get; set; } = true;
        public int SplitterDistance { get; set; } = 300;
        public Dictionary<string, int> ColumnWidths { get; set; } = [];

        private static string DefaultPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IPTracker", "settings.xml");

        public static AppSettings Load(string? path = null)
        {
            path ??= DefaultPath;
            if (!File.Exists(path))
                return new AppSettings();
            try
            {
                var root = XDocument.Load(path).Root!;
                var s = new AppSettings();

                var win = root.Element("Window");
                if (win != null)
                {
                    s.WindowState  = Enum.TryParse<FormWindowState>((string?)win.Attribute("State"), out var st) ? st : FormWindowState.Normal;
                    s.WindowX      = (int?)win.Attribute("X")      ?? s.WindowX;
                    s.WindowY      = (int?)win.Attribute("Y")      ?? s.WindowY;
                    s.WindowWidth  = (int?)win.Attribute("Width")  ?? s.WindowWidth;
                    s.WindowHeight = (int?)win.Attribute("Height") ?? s.WindowHeight;
                }

                var sort = root.Element("Sort");
                if (sort != null)
                {
                    s.SortColumn    = (string?)sort.Attribute("Column");
                    s.SortAscending = (bool?)sort.Attribute("Ascending") ?? true;
                }

                var splitter = root.Element("Splitter");
                if (splitter != null)
                    s.SplitterDistance = (int?)splitter.Attribute("Distance") ?? s.SplitterDistance;

                foreach (var col in root.Element("Columns")?.Elements("Column") ?? [])
                {
                    var name  = (string?)col.Attribute("Name");
                    var width = (int?)col.Attribute("Width");
                    if (name != null && width.HasValue)
                        s.ColumnWidths[name] = width.Value;
                }

                return s;
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(string? path = null)
        {
            path ??= DefaultPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            new XDocument(
                new XElement("Settings",
                    new XElement("Window",
                        new XAttribute("State",  WindowState),
                        new XAttribute("X",      WindowX),
                        new XAttribute("Y",      WindowY),
                        new XAttribute("Width",  WindowWidth),
                        new XAttribute("Height", WindowHeight)),
                    new XElement("Sort",
                        new XAttribute("Column",    SortColumn ?? string.Empty),
                        new XAttribute("Ascending", SortAscending)),
                    new XElement("Splitter",
                        new XAttribute("Distance", SplitterDistance)),
                    new XElement("Columns",
                        ColumnWidths.Select(kvp =>
                            new XElement("Column",
                                new XAttribute("Name",  kvp.Key),
                                new XAttribute("Width", kvp.Value))))))
            .Save(path);
        }
    }
}
