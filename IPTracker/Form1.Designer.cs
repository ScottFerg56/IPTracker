namespace IPTracker
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			dgvDevices = new DataGridView();
			menuStrip = new MenuStrip();
			toolsMenuItem = new ToolStripMenuItem("Tools");
			scanMenuItem = new ToolStripMenuItem("Scan");
			((System.ComponentModel.ISupportInitialize)dgvDevices).BeginInit();
			menuStrip.SuspendLayout();
			SuspendLayout();

			dgvDevices.AutoGenerateColumns = false;
			dgvDevices.AllowUserToAddRows = false;
			dgvDevices.AllowUserToDeleteRows = false;
			dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvDevices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dgvDevices.Dock = DockStyle.Fill;
			dgvDevices.ReadOnly = true;
			dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvDevices.Name = "dgvDevices";
			dgvDevices.TabIndex = 0;

			toolsMenuItem.DropDownItems.Add(scanMenuItem);
			menuStrip.Items.Add(toolsMenuItem);

			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(900, 500);
			Controls.Add(dgvDevices);
			Controls.Add(menuStrip);
			MainMenuStrip = menuStrip;
			Text = "IP Tracker";

			((System.ComponentModel.ISupportInitialize)dgvDevices).EndInit();
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		private DataGridView dgvDevices = null!;
		private MenuStrip menuStrip = null!;
		private ToolStripMenuItem toolsMenuItem = null!;
		private ToolStripMenuItem scanMenuItem = null!;

		#endregion
	}
}
