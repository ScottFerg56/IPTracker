# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
dotnet build
dotnet run --project IPTracker
dotnet run --project IPTracker -- --file C:\path\to\devices.xml
dotnet run --project IPTracker -- --scan                          # headless scan, no UI
dotnet run --project IPTracker -- --scan --file C:\path\to\devices.xml
dotnet build -c Release
```


There are no tests.

## Architecture

IPTracker is a WinForms app (.NET 10, `net10.0-windows`) for viewing and scanning LAN devices and tracking evolving IP assignments, backed by a user-managed XML file.

**Classes:**

- `ScanRange` — record with `BaseAddress`, `Start`, `End` (defaults: `"192.168.0."`, `1`, `255`). Stored as attributes on the `<devices>` root element of the XML file.

- `NetworkDevice` — data model with properties in column order: `Active` (bool), `MacAddress`, `IpAddress`, `Manufacturer`, `Name`, `Comments`. All fields are persisted. `LoadFromXml(path)` returns `(List<NetworkDevice> Devices, ScanRange Range)`; returns empty defaults if the file does not exist. `SaveToXml(devices, range, path)` writes both back; all string attributes are null-guarded with `?? string.Empty`. `LogTag()` returns `"{MacAddress} ({Comments})"` when Comments is non-empty, else just `MacAddress`; used as the prefix on every log/trace line.

- `AppSettings` — reads/writes `%LOCALAPPDATA%\IPTracker\settings.xml` (XML via `XDocument`). Stores window geometry, `WindowState`, sort column/direction, splitter distance, last-opened `XmlFilePath`, and per-column widths keyed by `DataPropertyName`.

- `SettingsForm` — `FixedDialog` opened by the "Settings" menu item. Contains a "Scan Range" group box (Base Address `TextBox`, Start/End `NumericUpDown`). Validates base address ends with `'.'` and Start ≤ End. Returns result via `ScanRange` property. Designed for future expansion. VS designer requires `new Control[] { ... }` rather than C# 12 collection expressions in the Designer file.

- `OuiLookup` — static class resolving the first 3 MAC octets (OUI) to a manufacturer name. Downloads the IEEE OUI CSV (`https://standards-oui.ieee.org/oui/oui.csv`) to `%LOCALAPPDATA%\IPTracker\oui.csv` on first use, refreshing if older than 30 days. Dictionary keyed by 6-char uppercase hex OUI. Sets `Manufacturer` only when the field is currently empty, preserving user-edited values.

- `HeadlessScan` — static class invoked when the app is launched with `--scan`. Accepts an optional `filePathOverride` (from `--file`); if provided and different from the saved setting, updates and saves `AppSettings` so the path is remembered. Runs the full scan/merge/log cycle (including "Scan started"/"Scan finished" timestamps) without any UI, and saves the XML if changes were detected. Writes to the same `.log` file as the interactive app. Contains its own `Merge` method (parallel to `MainForm.MergeDevice`) since there is no grid to refresh.

- `LanScanner` — static class that pings the range defined by `ScanRange` in parallel (up to 50 concurrent via `SemaphoreSlim`), resolves MAC addresses via `SendARP` P/Invoke from `iphlpapi.dll`, and resolves hostnames via `Dns.GetHostEntryAsync` (`.local` suffix trimmed). Returns `IAsyncEnumerable<(string Ip, string? Mac, string? HostName)>` via a `Channel` so results stream to the UI thread as they arrive.

- `MainForm` — orchestrates everything. Key details:
  - `XmlFilePath` is a property whose setter updates the form title (`"IP Tracker — {filename}"`). `LogFilePath` is a computed property derived from `XmlFilePath`.
  - Window position/size are restored in the constructor before handle creation (no flicker); `WindowState` and `SplitterDistance` are applied in `OnLoad`.
  - Layout: `SplitContainer` (horizontal) with `DataGridView` in Panel1 and a read-only `RichTextBox` (`rtbOutput`) in Panel2. Menu items (left to right): **New**, **Open**, **Settings**, **Scan**.
  - Sorting uses reflection for all columns except `IpAddress`, which converts to a `uint` via bit-shifts for correct numeric ordering.
  - Comments column is editable in-place. `CellBeginEdit` captures the original value; `CellEndEdit` logs the change and saves the XML. All other columns are column-level read-only. Pressing Delete with a row selected shows a confirmation dialog; on confirm, the device is logged (all properties), removed from `_devices`, and the XML is saved.
  - Right-clicking a row shows a `ContextMenuStrip` (`dgvContextMenu`) built dynamically in `OnGridMouseDown` via `HitTest`. Each property gets its own item (label + value in the text; click copies just the value). "Copy All" copies all values tab-separated (MAC, IP, Manufacturer, Name, Comments, Active) for easy pasting into a spreadsheet.
  - `Log()` writes to `rtbOutput` and appends a timestamped line to the `.log` file alongside the XML file (never overwritten, extended across runs).
  - **Scan flow:** logs "Scan started" timestamp, captures `_activeBeforeScan` (MACs currently active), marks all devices `Active = false`, then streams results to `MergeDevice`. `MergeDevice` sets `Active = true` for found devices, logging only genuine state transitions. After the loop, newly-inactive devices (were in `_activeBeforeScan`, still `Active = false`) are logged, then "Scan finished" is logged. Any Active-state change, IP move, new device, or name/manufacturer update triggers `SaveToXml`. If `SendARP` returns no MAC, the device at that IP (if known) is marked active with its IP preserved; no new device is added.

**Default data path:** `%USERPROFILE%\Documents\IPTracker.xml` (via `Environment.SpecialFolder.MyDocuments`), overridden by the last-opened path saved in `AppSettings`.
