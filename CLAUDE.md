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

**Three classes, one data flow:**

- `NetworkDevice` — plain data model with 5 string properties (`Name`, `IpAddress`, `Manufacturer`, `MacAddress`, `Comments`). Loaded via `NetworkDevice.LoadFromXml(path)` which parses `<row>` elements with matching attributes.
- `AppSettings` — reads/writes `%LOCALAPPDATA%\IPTracker\settings.xml` (XML via `XDocument`). Stores window geometry, `WindowState`, sort column/direction, and per-column widths keyed by `DataPropertyName`.
- `MainForm` — orchestrates everything. Constructor restores window position and size immediately (before handle creation) so there's no flicker; `WindowState` is applied in `OnLoad` to avoid overriding `Location`/`Size`. Sorting uses reflection (`typeof(NetworkDevice).GetProperty(propName)`) to sort `_devices` in place, then resets `DataSource` to refresh the grid.

**Hardcoded data path:** `C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml` — defined as `XmlFilePath` in `MainForm`.
