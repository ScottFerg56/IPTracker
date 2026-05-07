# IPTracker

A Windows desktop app for viewing and scanning LAN devices and tracking evolving IP assignments.

## Features

- Create a new empty devices file via the **New** menu item, or open an existing one via **Open**; the last opened file is restored on next launch
- Displays network devices with Active status, MAC Address, IP Address, Manufacturer, Name, and Comments; Comments can be edited in place; press Delete to remove a device
- Configure the scan IP range via the **Settings** menu item
- Scan the LAN to discover active devices, resolving MAC addresses, hostnames, and manufacturer names (via IEEE OUI lookup), automatically updating the device list as results arrive
- Scrollable output panel shows scan activity log in real time, also persisted to a `.log` file alongside the data file
- Newly discovered devices and changes are written back to the data file automatically

## Command Line

Run with `--scan` to perform a headless scan and exit without showing the UI. Uses the last-opened file from saved settings. Results are written to the `.log` file alongside the data file.

```powershell
IPTracker.exe --scan
```

## Requirements

- Windows
- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

## Data File

The XML format uses a `<devices>` root element with scan range attributes and `<row>` child elements:

```xml
<devices base="192.168.0." start="1" end="255">
  <row active="false" mac="AA:BB:CC:DD:EE:FF" ip="192.168.0.1" manufacturer="Netgear" name="Router" comments="" />
</devices>
```
