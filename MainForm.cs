namespace Circuitry {
	public partial class MainForm : Form {
		public Editor editor;
		public Level1 level1;
		public MainForm() {
			InitializeComponent();
			this.DoubleBuffered = true;
			this.ClientSize = Data.clientSize;
			this.Text = "Circuitry";

			editor = new Editor();
			level1 = new Level1();

			// Dock them to fill the main form
			editor.Dock = DockStyle.Fill;
			level1.Dock = DockStyle.Fill;

			// Add them to the main form
			this.Controls.Add(editor);
			this.Controls.Add(level1);

			ShowControl(editor);
		}
	public void ShowControl(UserControl control) {
			// Hide all controls
			foreach (Control c in this.Controls)
				c.Visible = false;

			// Show the desired control
			control.Visible = true;
		}
	}
}
