namespace IPTracker
{
	public partial class MainForm : Form
	{
		private const string XmlFilePath = @"C:\Users\Scott\SynologyDrive\Documents\IPTracker.xml";

		private List<NetworkDevice> _devices = [];
		private string? _sortColumn;
		private bool _sortAscending = true;

		public MainForm()
		{
			InitializeComponent();

			dgvDevices.Columns.AddRange(
				new DataGridViewTextBoxColumn { HeaderText = "Name",         DataPropertyName = nameof(NetworkDevice.Name),         SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "IP Address",   DataPropertyName = nameof(NetworkDevice.IpAddress),    SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "Manufacturer", DataPropertyName = nameof(NetworkDevice.Manufacturer), SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "MAC Address",  DataPropertyName = nameof(NetworkDevice.MacAddress),   SortMode = DataGridViewColumnSortMode.Programmatic },
				new DataGridViewTextBoxColumn { HeaderText = "Comments",     DataPropertyName = nameof(NetworkDevice.Comments),     SortMode = DataGridViewColumnSortMode.Programmatic }
			);

			_devices = NetworkDevice.LoadFromXml(XmlFilePath);
			dgvDevices.DataSource = _devices;

			dgvDevices.ColumnHeaderMouseClick += OnColumnHeaderMouseClick;
		}

		private void OnColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
		{
			var col = dgvDevices.Columns[e.ColumnIndex];
			var propName = col.DataPropertyName;

			_sortAscending = _sortColumn == propName ? !_sortAscending : true;
			_sortColumn = propName;

			var prop = typeof(NetworkDevice).GetProperty(propName)!;
			_devices = (_sortAscending
				? _devices.OrderBy(d => prop.GetValue(d))
				: _devices.OrderByDescending(d => prop.GetValue(d)))
				.ToList();

			dgvDevices.DataSource = _devices;

			foreach (DataGridViewColumn c in dgvDevices.Columns)
				c.HeaderCell.SortGlyphDirection = SortOrder.None;
			col.HeaderCell.SortGlyphDirection = _sortAscending ? SortOrder.Ascending : SortOrder.Descending;
		}
	}
}
