# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
dotnet build
dotnet run --project IPTracker
dotnet build -c Release
```

There are no tests.

## Architecture

IPTracker is a single-form WinForms app (.NET 10, `net10.0-windows`) that displays network devices from an XML file in a sortable `DataGridView` and persists UI state between sessions.

**Six classes, one data flow:**

- `ScanRange` — record with `BaseAddress`, `Start`, `End` (defaults: `"192.168.0."`, `1`, `255`). Stored as attributes on the `<rows>` root element of the XML file.
- `NetworkDevice` — plain data model with 5 string properties (`Name`, `IpAddress`, `Manufacturer`, `MacAddress`, `Comments`). `LoadFromXml(path)` returns `(List<NetworkDevice> Devices, ScanRange Range)`, parsing `<row>` elements and range attributes from the root. `SaveToXml(devices, range, path)` writes both back.
- `AppSettings` — reads/writes `%LOCALAPPDATA%\IPTracker\settings.xml` (XML via `XDocument`). Stores window geometry, `WindowState`, sort column/direction, splitter distance, last-opened `XmlFilePath`, and per-column widths keyed by `DataPropertyName`.
- `MainForm` — orchestrates everything. Constructor restores window position and size immediately (before handle creation) so there's no flicker; `WindowState` and `SplitterDistance` are applied in `OnLoad`. The form uses a `SplitContainer` (horizontal orientation) with the `DataGridView` in Panel1 and a read-only `RichTextBox` (`rtbOutput`) in Panel2 as a scrollable log output. The Comments column is editable in-place; `CellEndEdit` updates the device object and immediately saves the XML. All other columns are read-only at the column level (grid-level `ReadOnly` is not set). A timestamp is logged at the start of each scan. All scan change events are written to `rtbOutput` via `Log()`, which also appends timestamped entries to `IPTracker.log` in the same directory as the XML data file (file is extended across runs, never overwritten). After each scan (including cancelled ones), if any changes were made `NetworkDevice.SaveToXml` rewrites the data file. Sorting uses reflection for all columns except `IpAddress`, which uses a `uint` key built from the four octets via bit-shifts to ensure numeric ordering. A top-level "Settings" item opens `SettingsForm`, a `FixedDialog` with a "Scan Range" group box (Base Address, Start, End) that validates input and updates `_scanRange`, saving the XML if the file exists. Designed to be expanded with additional settings later. A top-level "Open" item shows an `OpenFileDialog` to load a different XML file, updating `XmlFilePath`, `_devices`, `_scanRange`, and refreshing the grid. `LogFilePath` is a property derived from `XmlFilePath` so it follows automatically. A top-level "Scan" item triggers `LanScanner.ScanAsync`; the menu item is disabled and relabeled "Scanning…" while in progress, and clicking it again cancels the previous scan. Each discovered `(ip, mac, hostName)` tuple is passed to `MergeDevice`, which: adds a new device if the MAC is unknown, updates the IP if the MAC moved, clears the IP from any existing device that held it under a different MAC, and updates `Name` from the hostname when it changes. If `SendARP` returns no MAC, the IP is still used to clear a displaced device but no new device is added. All changes are logged to `Debug.WriteLine` with MAC address leading.
- `SettingsForm` — `FixedDialog` (`FormBorderStyle.FixedDialog`, `StartPosition.CenterParent`) opened by the "Settings" menu item. Currently contains a "Scan Range" group box with Base Address (`TextBox`), Start and End (`NumericUpDown`, 1–254/255). Validates that base address ends with `'.'` and Start ≤ End before accepting. Returns the new `ScanRange` via its `ScanRange` property; designed to be extended with additional settings sections. VS designer compatibility requires `new Control[] { ... }` rather than C# 12 collection expressions in the Designer file.
- `OuiLookup` — static class that resolves the first 3 MAC octets (OUI) to a manufacturer name. On first scan, downloads the IEEE OUI CSV (`https://standards-oui.ieee.org/oui/oui.csv`) to `%LOCALAPPDATA%\IPTracker\oui.csv` and re-downloads if older than 30 days. Lookup is a dictionary keyed by 6-char uppercase hex OUI. `MergeDevice` calls `GetManufacturer` and sets `Manufacturer` only when the field is currently empty (preserves user-edited values).
- `LanScanner` — static class that pings the range defined by `ScanRange` in parallel (up to 50 concurrent via `SemaphoreSlim`), resolves MAC addresses via `SendARP` P/Invoke from `iphlpapi.dll`, and resolves hostnames via `Dns.GetHostEntryAsync`. Returns `IAsyncEnumerable<(string Ip, string? Mac, string? HostName)>` using a `Channel` so results stream back to the UI thread as they arrive. `Mac` and `HostName` are `null` when their respective lookups fail.

**Default data path:** `%USERPROFILE%\Documents\IPTracker.xml` (via `Environment.SpecialFolder.MyDocuments`), overridden by the last-opened path saved in `AppSettings`.
