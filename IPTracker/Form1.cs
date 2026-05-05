namespace IPTracker
{
	public partial class MainForm : Form
	{
		private const string XmlFilePath = @"C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml";

		private List<NetworkDevice> _devices = [];
		private string? _sortColumn;
		private bool _sortAscending = true;
		private readonly AppSettings _settings = AppSettings.Load();

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
			FormClosing += OnFormClosing;
			Load += OnLoad;
		}

		private void OnLoad(object? sender, EventArgs e)
		{
			WindowState = _settings.WindowState;

			if (!string.IsNullOrEmpty(_sortColumn))
			{
				var col = dgvDevices.Columns.Cast<DataGridViewColumn>()
					.FirstOrDefault(c => c.DataPropertyName == _sortColumn);
				if (col != null)
					col.HeaderCell.SortGlyphDirection = _sortAscending ? SortOrder.Ascending : SortOrder.Descending;
			}
		}

		private void ApplySort(string propName, bool ascending)
		{
			_sortColumn    = propName;
			_sortAscending = ascending;

			var prop = typeof(NetworkDevice).GetProperty(propName)!;
			_devices = (_sortAscending
				? _devices.OrderBy(d => prop.GetValue(d))
				: _devices.OrderByDescending(d => prop.GetValue(d)))
				.ToList();

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

			settings.Save();
		}
	}
}
