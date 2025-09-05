using System.Diagnostics;

namespace Circuitry
{
	public partial class Level1 : UserControl 
	{
		private List<(string name, Type gateType)> options;
		Panel leftPanel;

		private List<Gate> gates = new List<Gate> { };
		private List<InputGate> inputGates;
		private List<OutputGate> outputGates;

		private List<Gate> targetGates;
		private List<InputGate> targetInputs;
		private List<OutputGate> targetOutputs;

		private Gate? draggedGate;
		private bool dragging = false;
		private Point dragStartMouse;
		private Point dragStartGateLocation;

		private bool drawingLine = false;
		private Pin? startPin;

		private Rectangle deleteSpot;
		public Level1() {
			gates = new List<Gate> { };
			inputGates = new List<InputGate>
			{
				new InputGate(General.clientSize.Width/2-200, General.clientSize.Height-100, this),
				new InputGate(General.clientSize.Width/2, General.clientSize.Height-100, this)
			};
			foreach (InputGate g in inputGates) 
			{
				gates.Add((Gate)g);
			}

			outputGates = new List<OutputGate>
			{
				new OutputGate(General.clientSize.Width/2-100, 100, this),
			};
			foreach (Gate g in outputGates) 
			{
				gates.Add(g);
			}

			targetInputs = new List<InputGate> 
			{
				new InputGate(0, 0, this),
				new InputGate(0, 0, this)
			};
			foreach (InputGate g in targetInputs) 
			{
				g.toggleButton.Visible = false;
			}
			targetOutputs = new List<OutputGate>
			{
				new OutputGate(0, 0, this)
			};
			targetGates = new List<Gate>
			{
				new NandGate(0, 0, this),
				new OrGate(0, 0, this),
				new AndGate(0, 0, this)
			};
			targetInputs[0].pins[0].Connect(targetGates[0].pins[1]);
			targetInputs[1].pins[0].Connect(targetGates[0].pins[2]);
			targetInputs[0].pins[0].Connect(targetGates[1].pins[1]);
			targetInputs[1].pins[0].Connect(targetGates[1].pins[2]);
			targetGates[0].pins[0].Connect(targetGates[2].pins[1]);
			targetGates[1].pins[0].Connect(targetGates[2].pins[2]);
			targetGates[2].pins[0].Connect(targetOutputs[0].pins[0]);

			options = new List<(string name, Type gateType)>
			{
				("NAND", typeof(NandGate)),
				("NOT", typeof(NotGate)),
				("AND", typeof(AndGate)),
				("OR", typeof(OrGate))
			};

			InitializeComponent();
			CreateMenu();
			leftPanel = CreateDynamicButtons();

			this.DoubleBuffered = true;

			deleteSpot = new Rectangle(General.clientSize.Width - 200, General.clientSize.Height - 200, 200, 200);

			this.MouseDown += Level_MouseDown;
			this.MouseMove += Level_MouseMove;
			this.MouseUp += Level_MouseUp;
			//this.KeyPress += Level_KeyPress;
		}

		private void ResetStates() 
		{
			foreach (Gate g in gates) 
			{
				foreach (Pin p in g.pins) 
				{
					p.signal = false;
				}
			}
			foreach (Gate g in targetGates) 
			{
				foreach (Pin p in g.pins) 
				{
					p.signal = false;
				}
			}
			foreach (Gate g in targetInputs) 
			{
				g.pins[0].signal = false;
			}
			foreach (Gate g in targetOutputs) 
			{
				g.pins[0].signal = false;
			}
		}

		private void TestLevel () 
		{
			Debug.WriteLine("hello");
			ResetStates();
			Invalidate();
		}

		private void Exit_Click(object sender, EventArgs e) 
		{
			//this.Close();
		}

		private void CreateMenu() 
		{
			MenuStrip menu = new MenuStrip();

			ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
			fileMenu.DropDownItems.Add("Exit", null, Exit_Click);

			menu.Items.Add(fileMenu);

			ToolStripButton runButton = new ToolStripButton("Run");
			runButton.Click += (s, e) => 
			{
				RunSignal();
			};
			menu.Items.Add(runButton);

			ToolStripButton stepButton = new ToolStripButton("Step");
			stepButton.Click += (s, e) => 
			{
				StepSignal();
			};
			menu.Items.Add(stepButton);

			ToolStripButton testButton = new ToolStripButton("Test");
			testButton.Click += (s, e) => 
			{
				TestLevel();
			};
			menu.Items.Add(testButton);

			ToolStripMenuItem levelMenu = new ToolStripMenuItem("Levels");
			levelMenu.DropDownItems.Add("Level 1", null, (s, e) => 
			{
				//OpenLevel(1);
			});
			levelMenu.DropDownItems.Add("Level 2", null, (s, e) => 
			{
				//Open_Level(2);
			});
			menu.Items.Add(levelMenu);

			ToolStripButton editorButton = new ToolStripButton("Editor");
			editorButton.Click += (s, e) => 
			{
				((MainForm)this.Parent).ShowControl(((MainForm)this.Parent).editor);
			};
			menu.Items.Add(editorButton);

			menu.Dock = DockStyle.Top;
			this.Controls.Add(menu);
		}

		private void RunSignal() 
		{
			bool changed;
			for (int i = 0; i < 1000; i++)
			{
				changed = false;
				foreach (Gate gate in gates) 
				{
					gate.Process();
				}
				foreach (Gate gate in gates) 
				{
					if (gate.Transfer()) 
					{
						changed = true;
					}
				}

				Invalidate();
				if (!changed) return;
			}
			Debug.WriteLine("Timed out");
		}

		private void StepSignal() 
		{
			foreach (Gate gate in gates) 
			{
				gate.Process();
			}
			foreach (Gate gate in gates)
			{
				gate.Transfer();
			}
			Invalidate();
		}

		private Panel CreateDynamicButtons() 
		{
			Panel leftPanel = new Panel 
			{
				Dock = DockStyle.Left,
				Width = 120,
				AutoScroll = true,
				BackColor = Color.LightGray
			};

			for (int i = 0; i < options.Count; i++) 
			{
				Button btn = new Button 
				{
					Text = options[i].name,
					Width = leftPanel.Width - 10,
					Height = 40,
					Location = new Point(5, i * 45)
				};

				Type gateType = options[i].gateType;

				btn.Click += (s, e) => 
				{
					object gateInstance = Activator.CreateInstance(gateType, 200, 100, this);
					gates.Insert(0, (Gate)gateInstance);
					Invalidate();
				};

				leftPanel.Controls.Add(btn);
			}
			this.Controls.Add(leftPanel);
			return leftPanel;
		}

		private void Level_MouseDown(object sender, MouseEventArgs e) 
		{
			foreach (Gate gate in gates) 
			{
				foreach (Pin p in gate.pins) 
				{
					if (p.Contains(e.Location) && e.Button == MouseButtons.Left) 
					{
						if (p is InPin pin && pin.connection != null) 
						{
							pin.connection.RemoveConnection(pin);
							pin.connection = null;
						}
						startPin = p;
						drawingLine = true;
						return;
					}
				}
				if (gate is not InputGate && gate is not OutputGate &&
					gate.Contains(e.Location) && e.Button == MouseButtons.Left) 
				{
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

		private void Level_MouseMove(object sender, MouseEventArgs e) 
		{
			if (dragging) 
			{
				int dx = e.X - dragStartMouse.X;
				int dy = e.Y - dragStartMouse.Y;
				Point newLocation = new Point(dragStartGateLocation.X + dx, dragStartGateLocation.Y + dy);
				if (draggedGate != null) 
				{
					draggedGate.MoveTo(newLocation);
				}
				Invalidate();
			}
			if (drawingLine) Invalidate();
		}

		private void Level_MouseUp(object sender, MouseEventArgs e) 
		{
			if (dragging && e.Button == MouseButtons.Left) 
			{
				dragging = false;
				this.Cursor = Cursors.Default;
				if (draggedGate != null && draggedGate.bounds.IntersectsWith(deleteSpot)) 
				{
					draggedGate.Remove();
					gates.Remove(draggedGate);
					Invalidate();
				}
			}
			if (drawingLine && e.Button == MouseButtons.Left) 
			{
				drawingLine = false;
				foreach (Gate gate in gates) 
				{
					foreach (Pin c in gate.pins) 
					{
						if (c.Contains(e.Location) && c.GetType() != startPin.GetType()) 
						{
							Debug.WriteLine("connection made"); // maybe move all to class
							startPin.Connect(c);
							//break; // something better!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
						}
					}
				}
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e) 
		{
			base.OnPaint(e);
			e.Graphics.Clear(Color.WhiteSmoke);
			using (Brush fillBrush = new SolidBrush(Color.Red))
			using (Pen borderPen = new Pen(Color.DarkRed, 3)) {
				e.Graphics.FillRectangle(fillBrush, deleteSpot);
				e.Graphics.DrawRectangle(borderPen, deleteSpot);
			}

			foreach (Gate gate in gates.AsEnumerable().Reverse().ToList()) 
			{
				gate.Draw(e.Graphics);
			}

			if (drawingLine) 
			{
				using (Pen blackPen = new Pen(Color.Black, 4)) 
				{
					Point startLoc = startPin.bounds.Location;
					startLoc.Offset(new Point(10, 10));
					e.Graphics.DrawLine(blackPen, startLoc, this.PointToClient(Cursor.Position));
				}
			}
		}
	}
}
