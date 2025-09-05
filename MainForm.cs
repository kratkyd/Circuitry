namespace Circuitry {
	public partial class MainForm : Form {
		public Editor editor;
		public Level1 level1;
		public MainForm() {
			InitializeComponent();
			this.DoubleBuffered = true;
			this.ClientSize = General.clientSize;
			this.Text = "Circuitry";

			editor = new Editor();
			level1 = new Level1();

			editor.Dock = DockStyle.Fill;
			level1.Dock = DockStyle.Fill;

			this.Controls.Add(editor);
			this.Controls.Add(level1);

			ShowControl(editor);
		}
	public void ShowControl(UserControl control) {
			foreach (Control c in this.Controls)
				c.Visible = false;

			control.Visible = true;
		}
	}
}
