namespace IPTracker
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

		private void InitializeComponent()
		{
			grpScanRange = new GroupBox();
			lblBaseAddress = new Label();
			txtBaseAddress = new TextBox();
			lblStart = new Label();
			nudStart = new NumericUpDown();
			lblEnd = new Label();
			nudEnd = new NumericUpDown();
			btnOk = new Button();
			btnCancel = new Button();
			grpScanRange.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)nudStart).BeginInit();
			((System.ComponentModel.ISupportInitialize)nudEnd).BeginInit();
			SuspendLayout();
			// 
			// grpScanRange
			// 
			grpScanRange.Controls.Add(lblBaseAddress);
			grpScanRange.Controls.Add(txtBaseAddress);
			grpScanRange.Controls.Add(lblStart);
			grpScanRange.Controls.Add(nudStart);
			grpScanRange.Controls.Add(lblEnd);
			grpScanRange.Controls.Add(nudEnd);
			grpScanRange.Location = new Point(20, 17);
			grpScanRange.Name = "grpScanRange";
			grpScanRange.Size = new Size(292, 171);
			grpScanRange.TabIndex = 0;
			grpScanRange.TabStop = false;
			grpScanRange.Text = "Scan Range";
			// 
			// lblBaseAddress
			// 
			lblBaseAddress.Location = new Point(12, 40);
			lblBaseAddress.Name = "lblBaseAddress";
			lblBaseAddress.Size = new Size(90, 23);
			lblBaseAddress.TabIndex = 0;
			lblBaseAddress.Text = "Base:";
			lblBaseAddress.TextAlign = ContentAlignment.MiddleRight;
			// 
			// txtBaseAddress
			// 
			txtBaseAddress.Location = new Point(108, 38);
			txtBaseAddress.Name = "txtBaseAddress";
			txtBaseAddress.Size = new Size(160, 31);
			txtBaseAddress.TabIndex = 1;
			// 
			// lblStart
			// 
			lblStart.Location = new Point(12, 91);
			lblStart.Name = "lblStart";
			lblStart.Size = new Size(90, 23);
			lblStart.TabIndex = 2;
			lblStart.Text = "Start:";
			lblStart.TextAlign = ContentAlignment.MiddleRight;
			// 
			// nudStart
			// 
			nudStart.Location = new Point(108, 89);
			nudStart.Maximum = new decimal(new int[] { 254, 0, 0, 0 });
			nudStart.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			nudStart.Name = "nudStart";
			nudStart.Size = new Size(80, 31);
			nudStart.TabIndex = 3;
			nudStart.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// lblEnd
			// 
			lblEnd.Location = new Point(12, 132);
			lblEnd.Name = "lblEnd";
			lblEnd.Size = new Size(90, 23);
			lblEnd.TabIndex = 4;
			lblEnd.Text = "End:";
			lblEnd.TextAlign = ContentAlignment.MiddleRight;
			// 
			// nudEnd
			// 
			nudEnd.Location = new Point(108, 130);
			nudEnd.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			nudEnd.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			nudEnd.Name = "nudEnd";
			nudEnd.Size = new Size(80, 31);
			nudEnd.TabIndex = 5;
			nudEnd.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// btnOk
			// 
			btnOk.Location = new Point(102, 210);
			btnOk.Name = "btnOk";
			btnOk.Size = new Size(102, 42);
			btnOk.TabIndex = 1;
			btnOk.Text = "OK";
			btnOk.Click += OnOkClick;
			// 
			// btnCancel
			// 
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Location = new Point(210, 210);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(102, 42);
			btnCancel.TabIndex = 2;
			btnCancel.Text = "Cancel";
			// 
			// SettingsForm
			// 
			AcceptButton = btnOk;
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = btnCancel;
			ClientSize = new Size(341, 283);
			Controls.Add(grpScanRange);
			Controls.Add(btnOk);
			Controls.Add(btnCancel);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SettingsForm";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Settings";
			grpScanRange.ResumeLayout(false);
			grpScanRange.PerformLayout();
			((System.ComponentModel.ISupportInitialize)nudStart).EndInit();
			((System.ComponentModel.ISupportInitialize)nudEnd).EndInit();
			ResumeLayout(false);
		}

		private GroupBox grpScanRange = null!;
        private Label lblBaseAddress  = null!;
        private TextBox txtBaseAddress = null!;
        private Label lblStart        = null!;
        private NumericUpDown nudStart = null!;
        private Label lblEnd          = null!;
        private NumericUpDown nudEnd  = null!;
        private Button btnOk          = null!;
        private Button btnCancel      = null!;
    }
}
