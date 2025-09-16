namespace Circuitry {
	public partial class MainForm : Form {
		public Editor editor;
		public Level1 level1;
		public Level2 level2;
		public MainForm() {
			InitializeComponent();
			this.DoubleBuffered = true;
			this.ClientSize = General.clientSize;
			this.Text = "Circuitry";

			editor = new Editor();
			level1 = new Level1();
			level2 = new Level2();

			editor.Dock = DockStyle.Fill;
			level1.Dock = DockStyle.Fill;
			level2.Dock = DockStyle.Fill;

			this.Controls.Add(editor);
			this.Controls.Add(level1);
			this.Controls.Add(level2);

			ShowControl(editor);
		}
	public void ShowControl(UserControl control) {
			foreach (Control c in this.Controls)
				c.Visible = false;

			control.Visible = true;
		}
	}
}
