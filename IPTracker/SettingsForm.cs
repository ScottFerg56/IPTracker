namespace IPTracker
{
    internal partial class SettingsForm : Form
    {
        public ScanRange ScanRange { get; private set; }

        public SettingsForm(ScanRange range)
        {
            InitializeComponent();
            txtBaseAddress.Text = range.BaseAddress;
            nudStart.Value      = range.Start;
            nudEnd.Value        = range.End;
            ScanRange = range;
        }

        private void OnOkClick(object? sender, EventArgs e)
        {
            var baseAddress = txtBaseAddress.Text.Trim();
            if (!baseAddress.EndsWith('.'))
            {
                MessageBox.Show("Base address must end with '.'", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBaseAddress.Focus();
                return;
            }

            var start = (int)nudStart.Value;
            var end   = (int)nudEnd.Value;
            if (start > end)
            {
                MessageBox.Show("Start must be less than or equal to End.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudStart.Focus();
                return;
            }

            ScanRange    = new ScanRange(baseAddress, start, end);
            DialogResult = DialogResult.OK;
        }
    }
}
