using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Circuitry {
	
	public partial class Form1 : Form {

		private List<Gate> gates;
		private Gate? draggedGate;
		public enum connectionType { IN, OUT };

		private bool dragging = false;
		private Point dragStartMouse;
		private Point dragStartGateLocation;

		private bool drawingLine = false;
		private Pin? startPin;


		public Form1() {
			InitializeComponent();
			CreateDynamicButtons();
			CreateMenu();

			this.DoubleBuffered = true;
			this.ClientSize = new Size(1600, 720);
			this.Text = "Circuitry";

			gates = new List<Gate> {
				new AndGate(50, 50, 120, 80),
				new AndGate(200, 150, 120, 80),
				new AndGate(350, 80, 120, 80),
				new OrGate(400, 350, 120, 80),
				new OneGate(400, 250, 60, 60),
				new ZeroGate(200, 300, 60, 60),
				new XorGate(50, 500, 120, 80),
				new NandGate(250, 500, 120, 80),
				new NotGate(450, 500, 60, 60)
			};

			this.KeyPreview = true;
			this.MouseDown += Form1_MouseDown;
			this.MouseMove += Form1_MouseMove;
			this.MouseUp += Form1_MouseUp;
			this.KeyPress += Form1_KeyPress;
		}

		private void CreateMenu() {
			MenuStrip menu = new MenuStrip();

			ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
			fileMenu.DropDownItems.Add("Exit", null, Exit_Click);

			menu.Items.Add(fileMenu);
			menu.Dock = DockStyle.Top;

			this.MainMenuStrip = menu;
			this.Controls.Add(menu);
		}

		private void CreateDynamicButtons() {
			Panel leftPanel = new Panel {
				Dock = DockStyle.Left,      // Stick to the left side
				Width = 120,                // Set width of the panel
				AutoScroll = true,          // Allow scrolling if too many buttons
				BackColor = Color.LightGray // Optional: background color
			};
			// Add buttons vertically
			for (int i = 0; i < 10; i++) // Example: 10 buttons
			{
				System.Windows.Forms.Button btn = new System.Windows.Forms.Button {
					Text = $"Button {i}",
					Width = leftPanel.Width - 10,  // Slight margin inside panel
					Height = 40,
					Location = new Point(5, i * 45) // Stack vertically with spacing
				};

				// Click event for the button
				btn.Click += (s, e) =>
				{
					MessageBox.Show($"{btn.Text} clicked");
				};

				leftPanel.Controls.Add(btn);
			}

			// Add the panel to the form
			this.Controls.Add(leftPanel);
		}

		private void Exit_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e) {

			foreach (Gate gate in gates) {
				foreach (Pin p in gate.pins) {
					if (p.Contains(e.Location) && e.Button == MouseButtons.Left) {
						if (p is InPin pin && pin.connection != null) {
							pin.connection.RemoveConnection(pin);
							pin.connection = null;
						}
						startPin = p;
						drawingLine = true;
						return;
					}
				}
				if (gate.Contains(e.Location) && e.Button == MouseButtons.Left) {
					dragging = true;
					draggedGate = gate;
					dragStartMouse = e.Location;
					dragStartGateLocation = gate.bounds.Location;
					this.Cursor = Cursors.Hand;

					gates.Remove(gate);
					gates.Insert(0, gate);
					return;
				}
			}
		}

		private void Form1_MouseMove(object sender, MouseEventArgs e) {
			if (dragging) {
				int dx = e.X - dragStartMouse.X;
				int dy = e.Y - dragStartMouse.Y;
				Point newLocation = new Point(dragStartGateLocation.X + dx, dragStartGateLocation.Y + dy);
				draggedGate.MoveTo(newLocation);
				Invalidate();
			}
			if (drawingLine) {
				Invalidate();
			}
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e) {
			if (dragging && e.Button == MouseButtons.Left) {
				dragging = false;
				this.Cursor = Cursors.Default;
			}
			if (drawingLine && e.Button == MouseButtons.Left) {
				drawingLine = false;
				foreach (Gate gate in gates) {
					foreach (Pin c in gate.pins) {
						if (c.Contains(e.Location) && c.GetType() != startPin.GetType()) {
							Debug.WriteLine("connection made"); // maybe move all to class
							startPin.Connect(c);
							//break; // something better!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
						}
					}
				}
				Invalidate();
			}
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == '1') {
				foreach (Gate gate in gates) {
					gate.Transfer();
				}
				e.Handled = true;
				Debug.WriteLine("Transfer");
				Invalidate();
			} else if (e.KeyChar == '2') {
				foreach (Gate gate in gates) {
					gate.Process();
				}
				e.Handled = true;
				Debug.WriteLine("Process");
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			e.Graphics.Clear(Color.WhiteSmoke);
			foreach (Gate gate in gates.AsEnumerable().Reverse().ToList()) {
				gate.Draw(e.Graphics);
			}

			if (drawingLine) {
				using (Pen blackPen = new Pen(Color.Black, 4)) {
					Point startLoc = startPin.bounds.Location;
					startLoc.Offset(new Point(10, 10));
					e.Graphics.DrawLine(blackPen, startLoc, this.PointToClient(Cursor.Position));
				}
			}
		}
	}
}
