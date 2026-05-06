# IPTracker

A Windows desktop app for viewing network devices from an XML data file in a sortable, resizable grid.

## Features

- Displays network devices with Name, IP Address, Manufacturer, MAC Address, and Comments
- Click any column header to sort ascending/descending
- Persists window position, size, sort state, and column widths between sessions
- Scan the LAN (192.168.0.1–255) to discover active devices, resolving MAC addresses and hostnames, automatically updating the device list as results arrive
- Scrollable output panel shows scan activity log in real time, also persisted to `IPTracker.log` alongside the data file
- Newly discovered devices and changes are written back to the data file automatically

## Requirements

- Windows
- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

## Data File

The app reads device data from a hardcoded path:

```
C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml
```

The XML format expects `<row>` elements with the following attributes:

```xml
<rows>
  <row name="Router" ip="192.168.1.1" manufacturer="Netgear" mac="AA:BB:CC:DD:EE:FF" comments="" />
</rows>
```

## Build

```powershell
dotnet build
dotnet run --project IPTracker
```
