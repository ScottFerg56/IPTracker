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
			dgvDevices    = new DataGridView();
			splitContainer = new SplitContainer();
			rtbOutput     = new RichTextBox();
			menuStrip     = new MenuStrip();
			scanMenuItem  = new ToolStripMenuItem("Scan");
			((System.ComponentModel.ISupportInitialize)dgvDevices).BeginInit();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
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

			rtbOutput.Dock = DockStyle.Fill;
			rtbOutput.ReadOnly = true;
			rtbOutput.ScrollBars = RichTextBoxScrollBars.Vertical;
			rtbOutput.BackColor = SystemColors.Window;
			rtbOutput.Font = new Font("Consolas", 9F);
			rtbOutput.Name = "rtbOutput";
			rtbOutput.TabIndex = 0;

			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Orientation = Orientation.Horizontal;
			splitContainer.Panel1MinSize = 100;
			splitContainer.Panel2MinSize = 50;
			splitContainer.Name = "splitContainer";
			splitContainer.TabIndex = 0;
			splitContainer.Panel1.Controls.Add(dgvDevices);
			splitContainer.Panel2.Controls.Add(rtbOutput);

			menuStrip.Items.Add(scanMenuItem);

			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(900, 600);
			Controls.Add(splitContainer);
			Controls.Add(menuStrip);
			MainMenuStrip = menuStrip;
			Text = "IP Tracker";

			((System.ComponentModel.ISupportInitialize)dgvDevices).EndInit();
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		private DataGridView dgvDevices = null!;
		private SplitContainer splitContainer = null!;
		private RichTextBox rtbOutput = null!;
		private MenuStrip menuStrip = null!;
		private ToolStripMenuItem scanMenuItem = null!;

		#endregion
	}
}
