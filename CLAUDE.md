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

**Four classes, one data flow:**

- `NetworkDevice` — plain data model with 5 string properties (`Name`, `IpAddress`, `Manufacturer`, `MacAddress`, `Comments`). Loaded via `NetworkDevice.LoadFromXml(path)` which parses `<row>` elements with matching attributes.
- `AppSettings` — reads/writes `%LOCALAPPDATA%\IPTracker\settings.xml` (XML via `XDocument`). Stores window geometry, `WindowState`, sort column/direction, splitter distance, and per-column widths keyed by `DataPropertyName`.
- `MainForm` — orchestrates everything. Constructor restores window position and size immediately (before handle creation) so there's no flicker; `WindowState` and `SplitterDistance` are applied in `OnLoad`. The form uses a `SplitContainer` (horizontal orientation) with the `DataGridView` in Panel1 and a read-only `RichTextBox` (`rtbOutput`) in Panel2 as a scrollable log output. All scan change events are written to `rtbOutput` via `Log()`, which also appends timestamped entries to `IPTracker.log` in the same directory as the XML data file (file is extended across runs, never overwritten). Sorting uses reflection for all columns except `IpAddress`, which uses a `uint` key built from the four octets via bit-shifts to ensure numeric ordering. A top-level "Scan" item in the `MenuStrip` triggers `LanScanner.ScanAsync`; the menu item is disabled and relabeled "Scanning…" while in progress, and clicking it again cancels the previous scan. Each discovered `(ip, mac, hostName)` tuple is passed to `MergeDevice`, which: adds a new device if the MAC is unknown, updates the IP if the MAC moved, clears the IP from any existing device that held it under a different MAC, and updates `Name` from the hostname when it changes. If `SendARP` returns no MAC, the IP is still used to clear a displaced device but no new device is added. All changes are logged to `Debug.WriteLine` with MAC address leading.
- `LanScanner` — static class that pings 192.168.0.1–255 in parallel (up to 50 concurrent via `SemaphoreSlim`), resolves MAC addresses via `SendARP` P/Invoke from `iphlpapi.dll`, and resolves hostnames via `Dns.GetHostEntryAsync`. Returns `IAsyncEnumerable<(string Ip, string? Mac, string? HostName)>` using a `Channel` so results stream back to the UI thread as they arrive. `Mac` and `HostName` are `null` when their respective lookups fail.

**Hardcoded data path:** `C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml` — defined as `XmlFilePath` in `MainForm`.
