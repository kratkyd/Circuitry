using System.Diagnostics;


namespace Circuitry
	{

	public partial class Form1 : Form 
		{
		private List<(string name, Type gateType)> options;

		Panel leftPanel;

		private List<Gate> gates;
		private List<CustomGate> customGates;
		public Gate? draggedGate;

		private bool dragging = false;
		private Point dragStartMouse;
		private Point dragStartGateLocation;

		private bool drawingLine = false;
		private Pin? startPin;

		private bool selectingInputs = false;
		private bool selectingOutputs = false;
		private List<InputGate> selectedInputs;
		private List<OutputGate> selectedOutputs;

		private Rectangle deleteSpot;

		public CheckBox toggleButton;
		public Form1() 
		{

			gates = new List<Gate> { };
			selectedInputs = new List<InputGate> { };
			selectedOutputs = new List<OutputGate> { };
			options = new List<(string name, Type gateType)> 
			{
				("1", typeof(OneGate)),
				("0", typeof(ZeroGate)),
				("NOT", typeof(NotGate)),
				("AND", typeof(AndGate)),
				("NAND", typeof(NandGate)),
				("OR", typeof(OrGate)),
				("NOR", typeof(NorGate)),
				("XOR", typeof(XorGate)),
				("Blank", typeof(BlankGate)),
				("Input", typeof(InputGate)),
				("Output", typeof(OutputGate))
			};
			customGates = new List<CustomGate> { };

			InitializeComponent();
			CreateMenu();
			leftPanel = CreateDynamicButtons();

			this.DoubleBuffered = true;
			this.ClientSize = new Size(1280, 720);
			this.Text = "Circuitry";

			deleteSpot = new Rectangle(this.ClientSize.Width-200, this.ClientSize.Height-200, 200, 200);

			this.KeyPreview = true;
			this.MouseDown += Form1_MouseDown;
			this.MouseMove += Form1_MouseMove;
			this.MouseUp += Form1_MouseUp;
			this.KeyPress += Form1_KeyPress;
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

		private void CreateNewGate(string gateName) 
		{
			Debug.WriteLine("create new gate");
			selectedInputs.Reverse();
			selectedOutputs.Reverse();
			CustomGate newGate = new CustomGate(500, 500, this, gateName, gates, selectedInputs, selectedOutputs);
			selectedInputs = new List<InputGate> { };
			selectedOutputs = new List<OutputGate> { };
			foreach(Gate g in gates) 
			{
				if (g is InputGate ig) 
				{
					ig.toggleButton.Visible = false;
				}
			}
			gates = new List<Gate> { };
			customGates.Add(newGate);
			Invalidate();
			this.Controls.Remove(leftPanel);
			leftPanel = CreateDynamicButtons();
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

			ToolStripButton newGateButton = new ToolStripButton("New");
			newGateButton.Click += (s, e) => 
			{
				selectingInputs = true;
				AddMessageBlock("Gate order: Select input gates (unused gates will get 0 signal)", (s, ev) => 
				{
					selectingInputs = false;
					selectingOutputs = true;
					AddMessageBlock("Gate order: Select output gates", (s, ev) => 
					{
						selectingOutputs = false;
						AddInputBlock("Name the new gate", (text) => 
						{
							CreateNewGate(text);
						});
						Invalidate();
					});
					Invalidate();
				});
			};
			menu.Items.Add(newGateButton);

			menu.Dock = DockStyle.Top;

			this.MainMenuStrip = menu;
			this.Controls.Add(menu);
		}

		private Panel CreateDynamicButtons() 
		{
			Panel leftPanel = new Panel 
			{
				Dock = DockStyle.Left,      // Stick to the left side
				Width = 120,                // Set width of the panel
				AutoScroll = true,          // Allow scrolling if too many buttons
				BackColor = Color.LightGray // Optional: background color
			};
			// Add buttons vertically

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
					gates.Add((Gate)gateInstance);
					Invalidate();
				};

				leftPanel.Controls.Add(btn);
			}

			for (int i = 0; i < customGates.Count; i++) 
			{
				Button btn = new Button 
				{
					Text = customGates[i].text,
					Width = leftPanel.Width - 10,
					Height = 40,
					Location = new Point(5, options.Count*45 + 10 + i*45)
				};

				CustomGate cg = customGates[i];

				btn.Click += (s, e) => 
				{
					Gate gateInstance = cg.createInstance();
					gates.Add(gateInstance);
					Invalidate();
				};

				leftPanel.Controls.Add(btn);
			}

			// Add the panel to the form
			this.Controls.Add(leftPanel);
			return leftPanel;
		}

		private void AddMessageBlock(string message, EventHandler buttonClickHandler) 
		{
			// Create the panel (message block)
			Panel block = new Panel 
			{
				Size = new Size(250, 120),
				BorderStyle = BorderStyle.FixedSingle,
				BackColor = Color.LightGray,
				Location = new Point(900, 50) 
			};

			// Create label
			Label lbl = new Label 
			{
				Text = message,
				AutoSize = false,
				Size = new Size(230, 70),
				Location = new Point(10, 10),
				TextAlign = ContentAlignment.MiddleLeft
			};

			// Create button
			Button btn = new Button 
			{
				Text = "OK",
				Size = new Size(80, 30),
				Location = new Point(10, 80)
			};

			btn.Click += (s, e) =>
			{
				// Call custom handler passed by caller
				buttonClickHandler?.Invoke(s, e);

				// Remove the message block
				this.Controls.Remove(block);
				block.Dispose();
			};

			block.Controls.Add(lbl);
			block.Controls.Add(btn);

			// Add to the form itself
			this.Controls.Add(block);
		}

		private void AddInputBlock(string labelText, Action<string> buttonHandler) 
		{
			// Create panel (input block)
			Panel block = new Panel 
			{
				Size = new Size(300, 100),
				BorderStyle = BorderStyle.FixedSingle,
				BackColor = Color.LightBlue,
				Location = new Point(900, 50)
			};

			Label lbl = new Label 
			{
				Text = labelText,
				AutoSize = false,
				Size = new Size(280, 20),
				Location = new Point(10, 10),
				TextAlign = ContentAlignment.MiddleLeft
			};

			TextBox input = new TextBox 
			{
				Size = new Size(280, 25),
				Location = new Point(10, 35)
			};

			Button btn = new Button 
			{
				Text = "Submit",
				Size = new Size(80, 30),
				Location = new Point(10, 65)
			};

			btn.Click += (s, e) =>
			{
				// Call the handler with the input text
				buttonHandler?.Invoke(input.Text);

				// Remove block from form
				this.Controls.Remove(block);
				block.Dispose();
			};

			block.Controls.Add(lbl);
			block.Controls.Add(input);
			block.Controls.Add(btn);
			this.Controls.Add(block);
		}


		private void Exit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e) 
		{
			if (selectingInputs) 
			{
				foreach (Gate g in gates) 
				{
					if (g is not InputGate ig) continue;
					if (g.Contains(e.Location) && e.Button == MouseButtons.Left) 
					{
						if (selectedInputs.Contains(ig)) 
						{
							selectedInputs.Remove(ig);
						} else 
						{
							selectedInputs.Add(ig);
						}
						Invalidate();
						return;
					}
				}
				return;
			}

			if (selectingOutputs) 
				{
				foreach (Gate g in gates) 
					{
					if (g is not OutputGate og) continue;
					if (g.Contains(e.Location) && e.Button == MouseButtons.Left) 
						{
						if (selectedOutputs.Contains(og)) 
						{
							selectedOutputs.Remove(og);
						}
						else 
						{
							selectedOutputs.Add(og);
						}
						Invalidate();
						return;
					}
				}
				return;
			}

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
				if (gate.Contains(e.Location) && e.Button == MouseButtons.Left) 
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

		private void Form1_MouseMove(object sender, MouseEventArgs e) 
		{
			if (dragging) 
			{
				int dx = e.X - dragStartMouse.X;
				int dy = e.Y - dragStartMouse.Y;
				Point newLocation = new Point(dragStartGateLocation.X + dx, dragStartGateLocation.Y + dy);
				draggedGate.MoveTo(newLocation);
				Invalidate();
			}
			if (drawingLine) Invalidate();
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e) 
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
				foreach (Gate gate in gates) {
					foreach (Pin c in gate.pins) {
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

		private void Form1_KeyPress(object sender, KeyPressEventArgs e) 
		{
			if (e.KeyChar == '1') 
			{
				bool changed = false;
				foreach (Gate gate in gates) 
				{
					if (gate.Transfer()) 
					{
						changed = true;
					}
				}
				e.Handled = true;
				Debug.WriteLine("Transfer: " + changed);
				Invalidate();
			} else if (e.KeyChar == '2') 
			{
				foreach (Gate gate in gates) gate.Process();
				e.Handled = true;
				Debug.WriteLine("Process");
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
			
			if (selectingInputs) 
			{
				for (int i = 0; i < selectedInputs.Count; i++) 
				{
					using (Font font = new Font("Arial", 10, FontStyle.Bold)) // Small font size
					using (Brush brush = new SolidBrush(Color.Black))       // Text color
					{
						InputGate ig = selectedInputs[i];
						PointF location = new PointF(ig.bounds.X + 2, ig.bounds.Y + 2);
						e.Graphics.DrawString((i + 1).ToString(), font, brush, location);
					}
				}
			}

			if (selectingOutputs) 
			{
				for (int i = 0; i < selectedOutputs.Count; i++) 
				{
					using (Font font = new Font("Arial", 10, FontStyle.Bold)) // Small font size
					using (Brush brush = new SolidBrush(Color.Black))       // Text color
					{
						OutputGate og = selectedOutputs[i];
						PointF location = new PointF(og.bounds.X + 2, og.bounds.Y + 2);
						e.Graphics.DrawString((i + 1).ToString(), font, brush, location);
					}
				}
			}
		}
	}
}
