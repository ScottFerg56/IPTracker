namespace IPTracker
{
	public partial class MainForm : Form
	{
		private const string XmlFilePath = @"C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml";
		private static readonly string LogFilePath = Path.ChangeExtension(XmlFilePath, ".log");

		private List<NetworkDevice> _devices = [];
		private string? _sortColumn;
		private bool _sortAscending = true;
		private readonly AppSettings _settings = AppSettings.Load();
		private CancellationTokenSource? _scanCts;

		public MainForm()
		{
			InitializeComponent();

			// Apply position/size before the handle is created so there's no flicker.
			// WindowState is deferred to OnLoad — setting it in the constructor can
			// prevent Location/Size from taking effect.
			StartPosition = FormStartPosition.Manual;
			Location      = new Point(_settings.WindowX, _settings.WindowY);
			Size          = new Size(_settings.WindowWidth, _settings.WindowHeight);

			dgvDevices.Columns.AddRange(
				new DataGridViewTextBoxColumn { HeaderText = "Name",         DataPropertyName = nameof(NetworkDevice.Name),         SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "IP Address",   DataPropertyName = nameof(NetworkDevice.IpAddress),    SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "Manufacturer", DataPropertyName = nameof(NetworkDevice.Manufacturer), SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "MAC Address",  DataPropertyName = nameof(NetworkDevice.MacAddress),   SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "Comments",     DataPropertyName = nameof(NetworkDevice.Comments),     SortMode = DataGridViewColumnSortMode.Programmatic }
			);

			_devices = NetworkDevice.LoadFromXml(XmlFilePath);
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
			scanMenuItem.Click += OnScanClick;
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

		private void OnColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
		{
			var col = dgvDevices.Columns[e.ColumnIndex];
			ApplySort(col.DataPropertyName, _sortColumn == col.DataPropertyName ? !_sortAscending : true);
		}

		private async void OnScanClick(object? sender, EventArgs e)
		{
			_scanCts?.Cancel();
			_scanCts = new CancellationTokenSource();
			scanMenuItem.Enabled = false;
			scanMenuItem.Text = "Scanning…";
			Log($"--- Scan started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
			bool anyChanges = false;
			try
			{
				await foreach (var (ip, mac, hostName) in LanScanner.ScanAsync(_scanCts.Token))
					anyChanges |= MergeDevice(ip, mac, hostName);
			}
			catch (OperationCanceledException) { }
			finally
			{
				if (anyChanges)
					NetworkDevice.SaveToXml(_devices, XmlFilePath);
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

			bool changed = false;
			NetworkDevice? device = null;
			if (mac != null)
			{
				if (byMac == null)
				{
					device = new NetworkDevice { IpAddress = ip, MacAddress = mac };
					_devices.Add(device);
					Log($"{mac}  Added: {ip}");
					changed = true;
				}
				else
				{
					device = byMac;
					if (!string.Equals(byMac.IpAddress, ip, StringComparison.OrdinalIgnoreCase))
					{
						Log($"{mac}  IP: {byMac.IpAddress} -> {ip}");
						byMac.IpAddress = ip;
						changed = true;
					}
				}
			}

			if (byIp != null && byIp != byMac)
			{
				Log($"{byIp.MacAddress}  IP cleared: {ip}");
				byIp.IpAddress = string.Empty;
				changed = true;
			}

			if (device != null && hostName != null && !string.Equals(device.Name, hostName, StringComparison.OrdinalIgnoreCase))
			{
				Log($"{device.MacAddress}  Name: '{device.Name}' -> '{hostName}'");
				device.Name = hostName;
				changed = true;
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
			settings.Save();
		}
	}
}
