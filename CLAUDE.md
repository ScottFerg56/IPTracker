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
- `AppSettings` — reads/writes `%LOCALAPPDATA%\IPTracker\settings.xml` (XML via `XDocument`). Stores window geometry, `WindowState`, sort column/direction, and per-column widths keyed by `DataPropertyName`.
- `MainForm` — orchestrates everything. Constructor restores window position and size immediately (before handle creation) so there's no flicker; `WindowState` is applied in `OnLoad` to avoid overriding `Location`/`Size`. Sorting uses reflection for all columns except `IpAddress`, which uses a `uint` key built from the four octets via bit-shifts to ensure numeric ordering. A top-level "Scan" item in the `MenuStrip` triggers `LanScanner.ScanAsync`; the menu item is disabled and relabeled "Scanning…" while in progress, and clicking it again cancels the previous scan. Each discovered `(ip, mac)` pair is passed to `MergeDevice`, which: adds a new device if the MAC is unknown, updates the IP if the MAC moved, and clears the IP from any existing device that held it under a different MAC. If `SendARP` returns no MAC, the IP is still used to clear a displaced device but no new device is added.
- `LanScanner` — static class that pings 192.168.0.1–255 in parallel (up to 50 concurrent via `SemaphoreSlim`) and resolves MAC addresses via `SendARP` P/Invoke from `iphlpapi.dll`. Returns `IAsyncEnumerable<(string Ip, string? Mac)>` using a `Channel` so results stream back to the UI thread as they arrive. `Mac` is `null` when `SendARP` fails.

**Hardcoded data path:** `C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml` — defined as `XmlFilePath` in `MainForm`.
