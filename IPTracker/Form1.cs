namespace IPTracker
{
	public partial class MainForm : Form
	{
		private string _xmlFilePath = string.Empty;
		private string XmlFilePath
		{
			get => _xmlFilePath;
			set { _xmlFilePath = value; Text = $"IP Tracker — {Path.GetFileName(value)}"; }
		}
		private string LogFilePath => Path.ChangeExtension(XmlFilePath, ".log");

		private List<NetworkDevice> _devices = [];
		private ScanRange _scanRange = new();
		private string? _sortColumn;
		private bool _sortAscending = true;
		private readonly AppSettings _settings = AppSettings.Load();
		private CancellationTokenSource? _scanCts;
		private string? _editingOriginalComments;
		private HashSet<string> _activeBeforeScan = [];

		public MainForm(string? filePathOverride = null)
		{
			InitializeComponent();

			// Apply position/size before the handle is created so there's no flicker.
			// WindowState is deferred to OnLoad — setting it in the constructor can
			// prevent Location/Size from taking effect.
			XmlFilePath = filePathOverride
				?? (!string.IsNullOrEmpty(_settings.XmlFilePath)
					? _settings.XmlFilePath
					: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IPTracker.xml"));

			StartPosition = FormStartPosition.Manual;
			Location      = new Point(_settings.WindowX, _settings.WindowY);
			Size          = new Size(_settings.WindowWidth, _settings.WindowHeight);

			dgvDevices.Columns.AddRange(
				new DataGridViewCheckBoxColumn { HeaderText = "Active",       DataPropertyName = nameof(NetworkDevice.Active),       SortMode = DataGridViewColumnSortMode.Programmatic, ReadOnly = true },
				new DataGridViewTextBoxColumn  { HeaderText = "MAC Address",  DataPropertyName = nameof(NetworkDevice.MacAddress),   SortMode = DataGridViewColumnSortMode.Programmatic, ReadOnly = true },
				new DataGridViewTextBoxColumn  { HeaderText = "IP Address",   DataPropertyName = nameof(NetworkDevice.IpAddress),    SortMode = DataGridViewColumnSortMode.Programmatic, ReadOnly = true },
				new DataGridViewTextBoxColumn  { HeaderText = "Manufacturer", DataPropertyName = nameof(NetworkDevice.Manufacturer), SortMode = DataGridViewColumnSortMode.Programmatic, ReadOnly = true },
				new DataGridViewTextBoxColumn  { HeaderText = "Name",         DataPropertyName = nameof(NetworkDevice.Name),         SortMode = DataGridViewColumnSortMode.Programmatic, ReadOnly = true },
				new DataGridViewTextBoxColumn  { HeaderText = "Comments",     DataPropertyName = nameof(NetworkDevice.Comments),     SortMode = DataGridViewColumnSortMode.Programmatic }
			);

			(_devices, _scanRange) = NetworkDevice.LoadFromXml(XmlFilePath);
			dgvDevices.DataSource = _devices;

			if (_settings.ColumnWidths.Count > 0)
			{
				dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
				foreach (DataGridViewColumn col in dgvDevices.Columns)
				{
					if (_settings.ColumnWidths.TryGetValue(col.DataPropertyName, out int width))
						col.Width = width;
				}
			}

			if (!string.IsNullOrEmpty(_settings.SortColumn))
				ApplySort(_settings.SortColumn, _settings.SortAscending);

			dgvDevices.ColumnHeaderMouseClick += OnColumnHeaderMouseClick;
			dgvDevices.CellBeginEdit += OnCellBeginEdit;
			dgvDevices.CellEndEdit   += OnCellEndEdit;
			dgvDevices.KeyDown       += OnGridKeyDown;
			dgvDevices.MouseDown     += OnGridMouseDown;
			newMenuItem.Click      += OnNewClick;
			openMenuItem.Click     += OnOpenClick;
			settingsMenuItem.Click += OnSettingsClick;
			scanMenuItem.Click     += OnScanClick;
			FormClosing += OnFormClosing;
			Load += OnLoad;
		}

		private void OnLoad(object? sender, EventArgs e)
		{
			WindowState = _settings.WindowState;
			splitContainer.SplitterDistance = _settings.SplitterDistance;

			if (!string.IsNullOrEmpty(_sortColumn))
			{
				var col = dgvDevices.Columns.Cast<DataGridViewColumn>()
					.FirstOrDefault(c => c.DataPropertyName == _sortColumn);
				if (col != null)
					col.HeaderCell.SortGlyphDirection = _sortAscending ? SortOrder.Ascending : SortOrder.Descending;
			}
		}

		private void Log(string message)
		{
			rtbOutput.AppendText(message + Environment.NewLine);
			rtbOutput.ScrollToCaret();
			try
			{
				File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
			}
			catch { }
		}

		private static uint IpSortKey(string ip)
		{
			if (!System.Net.IPAddress.TryParse(ip, out var addr))
				return 0;
			var b = addr.GetAddressBytes();
			return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
		}

		private void ApplySort(string propName, bool ascending)
		{
			_sortColumn    = propName;
			_sortAscending = ascending;

			IOrderedEnumerable<NetworkDevice> sorted;
			if (propName == nameof(NetworkDevice.IpAddress))
			{
				sorted = ascending
					? _devices.OrderBy(d => IpSortKey(d.IpAddress))
					: _devices.OrderByDescending(d => IpSortKey(d.IpAddress));
			}
			else
			{
				var prop = typeof(NetworkDevice).GetProperty(propName)!;
				sorted = ascending
					? _devices.OrderBy(d => prop.GetValue(d))
					: _devices.OrderByDescending(d => prop.GetValue(d));
			}
			_devices = sorted.ToList();

			dgvDevices.DataSource = _devices;

			foreach (DataGridViewColumn c in dgvDevices.Columns)
				c.HeaderCell.SortGlyphDirection = SortOrder.None;

			var sortedCol = dgvDevices.Columns.Cast<DataGridViewColumn>()
				.FirstOrDefault(c => c.DataPropertyName == propName);
			if (sortedCol != null)
				sortedCol.HeaderCell.SortGlyphDirection = ascending ? SortOrder.Ascending : SortOrder.Descending;
		}

		private void OnGridKeyDown(object? sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete || dgvDevices.CurrentRow == null) return;

			var device = _devices[dgvDevices.CurrentRow.Index];
			var result = MessageBox.Show(
				$"Delete {device.MacAddress} ({device.IpAddress})?",
				"Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result != DialogResult.Yes) return;

			Log($"{device.LogTag()}  Deleted: IP='{device.IpAddress}' Name='{device.Name}' Manufacturer='{device.Manufacturer}' Active={device.Active} Comments='{device.Comments}'");
			_devices.Remove(device);
			NetworkDevice.SaveToXml(_devices, _scanRange, XmlFilePath);
			RefreshGrid();
			e.Handled = true;
		}

		private void OnGridMouseDown(object? sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;
			var hit = dgvDevices.HitTest(e.X, e.Y);
			if (hit.RowIndex < 0 || hit.RowIndex >= _devices.Count) return;

			dgvDevices.CurrentCell = dgvDevices[hit.ColumnIndex >= 0 ? hit.ColumnIndex : 0, hit.RowIndex];

			var d = _devices[hit.RowIndex];
			dgvContextMenu.Items.Clear();

			void AddItem(string label, string value)
			{
				var item = new ToolStripMenuItem($"{label}  {value}");
				item.Click += (_, _) => Clipboard.SetText(value);
				dgvContextMenu.Items.Add(item);
			}

			AddItem("MAC:",          d.MacAddress);
			AddItem("IP:",           d.IpAddress);
			AddItem("Manufacturer:", d.Manufacturer);
			AddItem("Name:",         d.Name);
			AddItem("Comments:",     d.Comments);
			AddItem("Active:",       d.Active.ToString());
			dgvContextMenu.Items.Add(new ToolStripSeparator());
			var copyAll = new ToolStripMenuItem("Copy All");
			copyAll.Click += (_, _) => Clipboard.SetText(
				$"{d.MacAddress}\t{d.IpAddress}\t{d.Manufacturer}\t{d.Name}\t{d.Comments}\t{d.Active}");
			dgvContextMenu.Items.Add(copyAll);

			dgvContextMenu.Show(dgvDevices, e.Location);
		}

		private void OnColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
		{
			var col = dgvDevices.Columns[e.ColumnIndex];
			ApplySort(col.DataPropertyName, _sortColumn == col.DataPropertyName ? !_sortAscending : true);
		}

		private void OnCellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
			if (dgvDevices.Columns[e.ColumnIndex].DataPropertyName != nameof(NetworkDevice.Comments)) return;
			_editingOriginalComments = _devices[e.RowIndex].Comments;
		}

		private void OnCellEndEdit(object? sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
			if (dgvDevices.Columns[e.ColumnIndex].DataPropertyName != nameof(NetworkDevice.Comments)) return;

			var device = _devices[e.RowIndex];
			device.Comments ??= string.Empty;
			if (string.Equals(device.Comments, _editingOriginalComments)) return;

			Log($"{device.LogTag()}  Comments: '{_editingOriginalComments}' -> '{device.Comments}'");
			_editingOriginalComments = null;
			NetworkDevice.SaveToXml(_devices, _scanRange, XmlFilePath);
		}

		private void OnNewClick(object? sender, EventArgs e)
		{
			using var dlg = new SaveFileDialog
			{
				Filter           = "XML Files|*.xml|All Files|*.*",
				Title            = "Create New Devices File",
				InitialDirectory = Path.GetDirectoryName(XmlFilePath),
				FileName         = "IPTracker.xml",
			};
			if (dlg.ShowDialog() != DialogResult.OK) return;

			XmlFilePath = dlg.FileName;
			_devices    = [];
			NetworkDevice.SaveToXml(_devices, _scanRange, XmlFilePath);
			RefreshGrid();
		}

		private void OnSettingsClick(object? sender, EventArgs e)
		{
			using var dlg = new SettingsForm(_scanRange);
			if (dlg.ShowDialog() != DialogResult.OK) return;
			_scanRange = dlg.ScanRange;
			if (File.Exists(XmlFilePath))
				NetworkDevice.SaveToXml(_devices, _scanRange, XmlFilePath);
		}

		private void OnOpenClick(object? sender, EventArgs e)
		{
			using var dlg = new OpenFileDialog
			{
				Filter           = "XML Files|*.xml|All Files|*.*",
				Title            = "Open Devices File",
				InitialDirectory = Path.GetDirectoryName(XmlFilePath),
				FileName         = Path.GetFileName(XmlFilePath),
			};
			if (dlg.ShowDialog() != DialogResult.OK) return;

			XmlFilePath = dlg.FileName;
			(_devices, _scanRange) = NetworkDevice.LoadFromXml(XmlFilePath);
			RefreshGrid();
		}

		private async void OnScanClick(object? sender, EventArgs e)
		{
			_scanCts?.Cancel();
			_scanCts = new CancellationTokenSource();
			scanMenuItem.Enabled = false;
			scanMenuItem.Text = "Scanning…";
			Log($"--- Scan started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
			_activeBeforeScan = _devices
				.Where(d => d.Active)
				.Select(d => d.MacAddress)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
			foreach (var d in _devices) d.Active = false;
			RefreshGrid();
			await OuiLookup.EnsureInitializedAsync();
			bool anyChanges = false;
			try
			{
				await foreach (var (ip, mac, hostName) in LanScanner.ScanAsync(_scanRange, _scanCts.Token))
					anyChanges |= MergeDevice(ip, mac, hostName);
			}
			catch (OperationCanceledException) { }
			finally
			{
				foreach (var d in _devices.Where(d => !d.Active && _activeBeforeScan.Contains(d.MacAddress)))
				{
					Log($"{d.LogTag()}  Inactive: {d.IpAddress}");
					anyChanges = true;
				}
				Log($"--- Scan finished {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
				if (anyChanges)
					NetworkDevice.SaveToXml(_devices, _scanRange, XmlFilePath);
				scanMenuItem.Text = "Scan";
				scanMenuItem.Enabled = true;
			}
		}

		private bool MergeDevice(string ip, string? mac, string? hostName)
		{
			var byMac = mac != null
				? _devices.FirstOrDefault(d => string.Equals(d.MacAddress, mac, StringComparison.OrdinalIgnoreCase))
				: null;
			var byIp = _devices.FirstOrDefault(d => string.Equals(d.IpAddress, ip, StringComparison.OrdinalIgnoreCase));

			// IP responded but no MAC — mark existing device active, leave IP intact
			if (mac == null)
			{
				if (byIp != null && !byIp.Active)
				{
					byIp.Active = true;
					if (!_activeBeforeScan.Contains(byIp.MacAddress))
						Log($"{byIp.LogTag()}  Active: {ip}");
					RefreshGrid();
					return true;
				}
				RefreshGrid();
				return false;
			}

			bool changed = false;
			NetworkDevice? device = null;
			if (byMac == null)
			{
				device = new NetworkDevice { IpAddress = ip, MacAddress = mac, Active = true };
				_devices.Add(device);
				Log($"{device.LogTag()}  Added: {ip}");
				changed = true;
			}
			else
			{
				device = byMac;
				if (!_activeBeforeScan.Contains(device.MacAddress))
				{
					Log($"{device.LogTag()}  Active: {ip}");
					changed = true;
				}
				device.Active = true;
				if (!string.Equals(byMac.IpAddress, ip, StringComparison.OrdinalIgnoreCase))
				{
					Log($"{device.LogTag()}  IP: {byMac.IpAddress} -> {ip}");
					byMac.IpAddress = ip;
					changed = true;
				}
			}

			if (byIp != null && byIp != byMac)
			{
				Log($"{byIp.LogTag()}  IP cleared: {ip}");
				byIp.IpAddress = string.Empty;
				changed = true;
			}

			if (device != null && hostName != null && !string.Equals(device.Name, hostName, StringComparison.OrdinalIgnoreCase))
			{
				Log($"{device.LogTag()}  Name: '{device.Name}' -> '{hostName}'");
				device.Name = hostName;
				changed = true;
			}

			if (device != null && mac != null && string.IsNullOrEmpty(device.Manufacturer))
			{
				var manufacturer = OuiLookup.GetManufacturer(mac);
				if (manufacturer != null)
				{
					Log($"{device.LogTag()}  Manufacturer: '{manufacturer}'");
					device.Manufacturer = manufacturer;
					changed = true;
				}
			}

			RefreshGrid();
			return changed;
		}

		private void RefreshGrid()
		{
			if (!string.IsNullOrEmpty(_sortColumn))
				ApplySort(_sortColumn, _sortAscending);
			else
			{
				dgvDevices.DataSource = null;
				dgvDevices.DataSource = _devices;
			}
		}

		private void OnFormClosing(object? sender, FormClosingEventArgs e)
		{
			var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
			var settings = new AppSettings
			{
				WindowState   = WindowState == FormWindowState.Minimized ? FormWindowState.Normal : WindowState,
				WindowX       = bounds.X,
				WindowY       = bounds.Y,
				WindowWidth   = bounds.Width,
				WindowHeight  = bounds.Height,
				SortColumn    = _sortColumn,
				SortAscending = _sortAscending,
			};

			foreach (DataGridViewColumn col in dgvDevices.Columns)
				settings.ColumnWidths[col.DataPropertyName] = col.Width;

			settings.SplitterDistance = splitContainer.SplitterDistance;
			settings.XmlFilePath      = XmlFilePath;
			settings.Save();
		}
	}
}
