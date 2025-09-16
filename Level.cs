using System.Diagnostics;

namespace Circuitry
{
	public partial class Level : UserControl
	{
		public List<(string name, Type gateType)> options;
		public Panel leftPanel;

		public List<Gate> gates = new List<Gate> { };
		public List<InputGate> inputGates;
		public List<OutputGate> outputGates;

		public List<Gate> targetGates;
		public List<InputGate> targetInputs;
		public List<OutputGate> targetOutputs;

		public Gate? draggedGate;
		public bool dragging = false;
		public Point dragStartMouse;
		public Point dragStartGateLocation;

		public bool drawingLine = false;
		public Pin? startPin;

		public Rectangle deleteSpot;
		public string topRightText;
		public string successText = "";
		public Level()
		{
			InitializeComponent();
		}

		public void ResetStates()
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

		public int Pow(int x, int y)
		{
			if (y == 0) return 1;
			return x * Pow(x, y - 1);
		}

		public bool TestLevel()
		{
			for (int i = 0; i < Pow(2, inputGates.Count); i++)
			{
				ResetStates();
				for (int j = 0; j < inputGates.Count; j++)
				{
					inputGates[j].pins[0].signal = (i / Pow(2, j)) % 2 == 1;
					targetInputs[j].pins[0].signal = (i / Pow(2, j)) % 2 == 1;
				}
				RunSignal();
				for (int j = 0; j < outputGates.Count; j++)
				{
					Debug.WriteLine(i);
					Debug.WriteLine(((OutPin)targetInputs[j].pins[0]).connections.Count);
					if (outputGates[j].pins[0].signal != targetOutputs[j].pins[0].signal)
					{
						Debug.WriteLine("failed on state " + i);
						ResetStates();
						Invalidate();
						successText = "Incorrect";
						return false;
					}
				}
			}
			ResetStates();
			Invalidate();
			successText = "Correct";
			return true;
		}

		public void Exit_Click(object sender, EventArgs e)
		{
			//this.Close();
		}

		public void CreateMenu()
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
				Debug.WriteLine(TestLevel());
			};
			menu.Items.Add(testButton);

			ToolStripMenuItem levelMenu = new ToolStripMenuItem("Levels");
			levelMenu.DropDownItems.Add("Level 1", null, (s, e) =>
			{
				((MainForm)this.Parent).ShowControl(((MainForm)this.Parent).level1);
			});
			levelMenu.DropDownItems.Add("Level 2", null, (s, e) =>
			{
				((MainForm)this.Parent).ShowControl(((MainForm)this.Parent).level2);
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

		public bool RunSignal()
		{
			bool finished = false;
			for (int i = 0; i < 1000; i++)
			{
				if (!StepSignal())
				{
					return true;
				}
			}
			Debug.WriteLine("Timed out");
			return false;
		}

		public bool StepSignal()
		{
			bool changed = false;
			foreach (Gate gate in gates)
			{
				gate.Process();
			}
			foreach (Gate gate in targetGates)
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
			foreach (Gate gate in targetInputs)
			{
				if (gate.Transfer())
				{
					changed = true;
				}
			}
			foreach (Gate gate in targetOutputs)
			{
				if (gate.Transfer())
				{
					changed = true;
				}
			}
			foreach (Gate gate in targetGates)
			{
				if (gate.Transfer())
				{
					changed = true;
				}
			}
			Invalidate();
			return changed;
		}

		public Panel CreateDynamicButtons()
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

		public void Level_MouseDown(object sender, MouseEventArgs e)
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

		public void Level_MouseMove(object sender, MouseEventArgs e)
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

		public void Level_MouseUp(object sender, MouseEventArgs e)
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
			using (Pen borderPen = new Pen(Color.DarkRed, 3))
			{
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

			using (Font font = new Font("Arial", 16, FontStyle.Bold))
			using (Brush brush = new SolidBrush(Color.Black))
			{
				var textSize = e.Graphics.MeasureString(topRightText, font);
				var x = this.ClientSize.Width - textSize.Width - 20;
				var y = 50;
				e.Graphics.DrawString(topRightText, font, brush, x, y);
			}

			using (Font font = new Font("Arial", 16, FontStyle.Bold))
			using (Brush brush = new SolidBrush(Color.Black))
			{
				var textSize = e.Graphics.MeasureString(successText, font);
				var x = this.ClientSize.Width - textSize.Width - 20;
				var y = 100;
				e.Graphics.DrawString(successText, font, brush, x, y);
			}
		}
	}

	public partial class Level1 : Level
	{
		public Level1() : base()
		{
			this.topRightText = "Goal: Build a XOR";
			this.gates = new List<Gate> { };
			this.inputGates = new List<InputGate>
			{
				new InputGate(General.clientSize.Width/2-200, General.clientSize.Height-100, this),
				new InputGate(General.clientSize.Width/2, General.clientSize.Height-100, this)
			};
			foreach (InputGate g in this.inputGates)
			{
				this.gates.Add((Gate)g);
			}

			this.outputGates = new List<OutputGate>
			{
				new OutputGate(General.clientSize.Width/2-100, 100, this),
			};
			foreach (Gate g in this.outputGates)
			{
				this.gates.Add(g);
			}

			this.targetInputs = new List<InputGate>
			{
				new InputGate(0, 0, this),
				new InputGate(0, 0, this)
			};
			foreach (InputGate g in this.targetInputs)
			{
				g.toggleButton.Visible = false;
			}
			this.targetOutputs = new List<OutputGate>
			{
				new OutputGate(0, 0, this)
			};
			this.targetGates = new List<Gate>
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

			this.options = new List<(string name, Type gateType)>
			{
				("NAND", typeof(NandGate)),
				("NOT", typeof(NotGate)),
				("AND", typeof(AndGate)),
				("OR", typeof(OrGate))
			};

			CreateMenu();
			leftPanel = CreateDynamicButtons();

			this.DoubleBuffered = true;

			deleteSpot = new Rectangle(General.clientSize.Width - 200, General.clientSize.Height - 200, 200, 200);

			this.MouseDown += Level_MouseDown;
			this.MouseMove += Level_MouseMove;
			this.MouseUp += Level_MouseUp;
			//this.KeyPress += Level_KeyPress;
		}
	}

	public partial class Level2 : Level
	{
		public Level2() : base()
		{
			this.topRightText = "Goal: Build a XOR";
			this.gates = new List<Gate> { };
			this.inputGates = new List<InputGate>
			{
				new InputGate(General.clientSize.Width/2-200, General.clientSize.Height-100, this),
				new InputGate(General.clientSize.Width/2, General.clientSize.Height-100, this)
			};
			foreach (InputGate g in this.inputGates)
			{
				this.gates.Add((Gate)g);
			}

			this.outputGates = new List<OutputGate>
			{
				new OutputGate(General.clientSize.Width/2-100, 100, this),
			};
			foreach (Gate g in this.outputGates)
			{
				this.gates.Add(g);
			}

			this.targetInputs = new List<InputGate>
			{
				new InputGate(0, 0, this),
				new InputGate(0, 0, this)
			};
			foreach (InputGate g in this.targetInputs)
			{
				g.toggleButton.Visible = false;
			}
			this.targetOutputs = new List<OutputGate>
			{
				new OutputGate(0, 0, this)
			};
			this.targetGates = new List<Gate>
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

			this.options = new List<(string name, Type gateType)>
			{
				("NAND", typeof(NandGate)),
			};

			CreateMenu();
			leftPanel = CreateDynamicButtons();

			this.DoubleBuffered = true;

			deleteSpot = new Rectangle(General.clientSize.Width - 200, General.clientSize.Height - 200, 200, 200);

			this.MouseDown += Level_MouseDown;
			this.MouseMove += Level_MouseMove;
			this.MouseUp += Level_MouseUp;
			//this.KeyPress += Level_KeyPress;
		}
	}
}
