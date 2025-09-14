using Circuitry;
using System.CodeDom;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Windows.Forms.Design;

public class Gate 
{
	public Rectangle bounds { get; protected set; }
	public List<Pin> pins = new List<Pin> { };
	public String text;
	public Control parent;

	public Gate(int x, int y, Control parent) 
	{
		this.parent = parent;
	}

	public bool Contains(Point pt) 
	{
		return bounds.Contains(pt);
	}

	public virtual void MoveTo(Point newLocation) 
	{
		bounds = new Rectangle(newLocation, bounds.Size);
		foreach(Pin p in pins) 
		{
			p.MoveTo(newLocation);
		}
	}

	public virtual void Remove()
	{
		this.RemoveConnections();
	}

	public void RemoveConnections() 
	{
		foreach (Pin p in pins) 
		{
			if (p is InPin ip && ip.connection != null) 
			{
				for (int i = 0; i < ip.connection.connections.Count; i++) 
				{
					if (ip.connection.connections[i] == ip) 
					{
						ip.connection.connections.RemoveAt(i);
						ip.connection.connectionLines.RemoveAt(i);
						break;
					}
				}
				//ip.connection.connections.Remove(ip);
			} else if (p is OutPin op) 
			{
				foreach (InPin c in op.connections) 
				{
					c.connection = null;
				}
			}
		}
	}

	public virtual bool Transfer() 
	{
		bool changed = false; //for stopping the program
		foreach (Pin p in this.pins) 
		{
			if (p is InPin ip) 
			{
				if (ip.connection == null) 
				{
					if (ip.signal) 
					{
						changed = true;
						ip.signal = false;
						if (ip.parent.GetType() == typeof(BlankGate)) 
						{
							ip.parent.Process();
							ip.parent.Transfer();
						}
					}
					ip.signal = false;
				}
				continue;
			} else if (p is OutPin op) 
			{
				if (op.connections.Count == 0) continue;

				foreach (InPin c in op.connections) 
				{
					if (c.signal != op.signal) 
					{
						changed = true;
						c.signal = op.signal;
						if (c.parent.GetType() == typeof(BlankGate)) 
						{
							Debug.WriteLine("hello");
							c.parent.Process();
							c.parent.Transfer();
						}
					}
					c.signal = op.signal;
				}
			}
		}
		return changed;
	}	

	public virtual void Process() 
	{

	}

	public virtual void Draw(Graphics g) 
	{
		using (Brush fillBrush = new SolidBrush(Color.LightBlue))
		using (Pen borderPen = new Pen(Color.DarkBlue, 3)) 
		{
			g.FillRectangle(fillBrush, bounds);
			g.DrawRectangle(borderPen, bounds);
		}
		foreach (Pin p in pins) 
		{
			p.Draw(g);
		}

		using (Font font = new Font("Arial", 14))
		using (Brush brush = new SolidBrush(Color.Black)) 
		{
			StringFormat format = new StringFormat 
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};
			g.DrawString(text, font, brush, bounds, format);
		}
	}
}

public class AndGate : Gate 
{
	public AndGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 120, 80);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "AND";
	}

	public override void Process() 
	{
		pins[0].signal = pins[1].signal && pins[2].signal;
	}
}

public class OneGate : Gate 
	{
	public OneGate(int x, int y, Control parent) : base(x, y, parent) 
		{
		bounds = new Rectangle(x, y, 60, 60);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20)
		};
		pins[0].signal = true;
		text = "1";
	}

	public override void Process() 
	{
		pins[0].signal = true;
	}
}

public class ZeroGate : Gate 
{
	public ZeroGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 60, 60);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20)
		};
		text = "0";
	}

	public override void Process() 
	{
		pins[0].signal = false;
	}
}

public class OrGate : Gate 
{
	public OrGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 120, 80);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "OR";
	}

	public override void Process() 
	{
		pins[0].signal = pins[1].signal || pins[2].signal;
	}
}

public class NorGate : Gate 
{
	public NorGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 120, 80);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "NOR";
	}

	public override void Process() 
	{
		pins[0].signal = !(pins[1].signal || pins[2].signal);
	}
}

public class XorGate : Gate 
{
	public XorGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 120, 80);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "XOR";
	}

	public override void Process() 
	{
		pins[0].signal = pins[1].signal ^ pins[2].signal;
	}
}

public class NandGate : Gate 
{
	public NandGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 120, 80);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "NAND";
	}

	public override void Process() 
	{
		pins[0].signal = !(pins[1].signal && pins[2].signal);
	}
}

public class NotGate : Gate 
{
	public NotGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 60, 60);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(this.bounds.Width/2-10, this.bounds.Height-10), 20, 20)
		};
		text = "NOT";
	}

	public override void Process() 
	{
		pins[0].signal = !pins[1].signal;
	}
}

public class BlankGate : Gate 
{
	public BlankGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		bounds = new Rectangle(x, y, 60, 60);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(this.bounds.Width/2-10, this.bounds.Height-10), 20, 20)
		};
		text = "";
	}

	public override void Process() 
	{
		pins[0].signal = pins[1].signal;
	}
}

public class InputGate : Gate 
{
	public Button toggleButton;
	Point toggleButtonOffset;
	Control parent;
	public InputGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		this.parent = parent;
		bounds = new Rectangle(x, y, 60, 60);
		pins = new List<Pin> 
		{
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20)
		};
		pins[0].signal = false;
		text = "";

		toggleButtonOffset = new Point(15, 35);
		toggleButton = new Button 
		{
			Width = 30,
			Height = 20,
			BackColor = Color.DarkGray,
			Location = new Point(this.bounds.X + toggleButtonOffset.X, this.bounds.Y + toggleButtonOffset.Y)
		};
		parent.Controls.Add(toggleButton);

		toggleButton.Click += (s, e) => 
		{
			pins[0].signal = !pins[0].signal;
			parent.Invalidate();
		};

	}

	public override void Remove() 
	{
		base.Remove();
		this.parent.Controls.Remove(toggleButton);
	}

	public override void MoveTo(Point newLocation) 
	{
		base.MoveTo(newLocation);
		newLocation.Offset(toggleButtonOffset);
		toggleButton.Location = newLocation;
	}
}

public class OutputGate : Gate 
{
	public OutputGate(int x, int y, Control parent) : base(x, y, parent) 
	{
		this.parent = parent;
		bounds = new Rectangle(x, y, 60, 60);
		pins = new List<Pin> {
			new InPin(this, new Point(this.bounds.Width/2-10, this.bounds.Height-10), 20, 20)
		};
		text = "";
	}
}

public class CustomGate : Gate 
{
	public List<Gate> savedGates;
	public List<InPin> inPins = new List<InPin> { };
	public List<OutPin> outPins = new List<OutPin> { };
	List<InputGate> inputGates;
	List<OutputGate> outputGates;


	public CustomGate(int x, int y, Control parent, string name, List<Gate> gates, List<InputGate> selectedInputs, List<OutputGate> selectedOutputs) : base(x, y, parent) 
	{
		this.bounds = new Rectangle(x, y, 0, 0);
		this.savedGates = new List<Gate>(gates);
		this.inputGates = new List<InputGate>(selectedInputs);
		this.outputGates = new List<OutputGate>(selectedOutputs);

		this.text = name;
		//create Blankgates instead of input/output gates
		
		foreach (InputGate ig in this.inputGates) 
		{
			BlankGate gateInstance = (BlankGate)Activator.CreateInstance(typeof(BlankGate), 0, 0, parent);

			int index = savedGates.IndexOf(ig);
			if (index != -1) 
			{
				savedGates[index] = gateInstance;
				foreach (InPin p in ((OutPin)ig.pins[0]).connections)
				{
					p.connection = (OutPin)gateInstance.pins[0];
				}
			}
		}

		foreach (OutputGate og in this.outputGates) 
		{
			BlankGate gateInstance = (BlankGate)Activator.CreateInstance(typeof(BlankGate), 0, 0, parent);
			int index = savedGates.IndexOf(og);
			if (index != -1) 
			{
				savedGates[index] = gateInstance;
				((InPin)og.pins[0]).connection.connections.Remove((InPin)og.pins[0]);
				((InPin)og.pins[0]).connection.connections.Add((InPin)gateInstance.pins[1]);
			}
		}

		//fix connections
		foreach (InputGate ig in this.inputGates) 
		{
			int index = gates.IndexOf(ig);

			for (int i = 0; i < gates.Count; i++) 
			{
				for (int j = 0; j < gates[i].pins.Count; j++) 
				{
					if (((OutPin)ig.pins[0]).connections.Contains(gates[i].pins[j])) 
					{
						if (this.outputGates.Contains(gates[i])) 
						{
							((OutPin)savedGates[index].pins[0]).connections.Add((InPin)savedGates[i].pins[1]);
						} else 
						{
							((OutPin)savedGates[index].pins[0]).connections.Add((InPin)savedGates[i].pins[j]);
						}
					}
				}
			}

			inPins.Add((InPin)savedGates[index].pins[1]);
		}

		foreach (OutputGate og in this.outputGates) 
		{
			int index = gates.IndexOf(og);
			for (int i = 0; i < gates.Count; i++) 
			{
				for (int j = 0; j < gates[i].pins.Count; j++) 
				{
					if (((InPin)og.pins[0]).connection == gates[i].pins[j])
					{
						if (this.inputGates.Contains(gates[i])) 
						{
							((InPin)savedGates[index].pins[1]).connection = (OutPin)savedGates[i].pins[0];
						} else 
						{
							((InPin)savedGates[index].pins[1]).connection = (OutPin)savedGates[i].pins[j];
						}
					}
				}
			}
			outPins.Add((OutPin)savedGates[index].pins[0]);
		}

	}

	//this constructor creates a copy

	public CustomGate(int x, int y, CustomGate template) : base(template.bounds.X, template.bounds.Y, template.parent) 
	{
		this.text = template.text;

		this.savedGates = new List<Gate> { };
		Gate gateInstance;
		foreach(Gate g in template.savedGates) 
		{
			if (g is CustomGate cg) 
			{
				gateInstance = (Gate)Activator.CreateInstance(typeof(CustomGate), -500, -500, cg);
			} 
			else 
			{
				gateInstance = (Gate)Activator.CreateInstance(g.GetType(), -500, -500, this.parent);
			}
			this.savedGates.Add(gateInstance);
		}


		for (int i = 0; i < template.savedGates.Count; i++) 
		{
			for (int j = 0; j < template.savedGates[i].pins.Count; j++) 
			{
				if (this.savedGates[i].pins[j] is InPin ip) 
				{
					for (int k = 0; k < template.savedGates.Count; k++) 
					{
						for (int l = 0; l < template.savedGates[k].pins.Count; l++) 
						{
							if (((InPin)template.savedGates[i].pins[j]).connection == template.savedGates[k].pins[l]) 
							{
								ip.connection = (OutPin)this.savedGates[k].pins[l];
								((OutPin)this.savedGates[k].pins[l]).connections.Add(ip);
							}
						}
					}
					if (template.inPins.Contains(template.savedGates[i].pins[j])) 
					{
						this.inPins.Add(ip);
					}
				} 
				else if (this.savedGates[i].pins[j] is OutPin op) 
				{
					for (int k = 0; k < template.savedGates.Count; k++) 
					{
						for (int l = 0; l < template.savedGates[k].pins.Count; l++)
						{
							if (((OutPin)template.savedGates[i].pins[j]).connections.Contains(template.savedGates[k].pins[l])) 
							{
								op.connections.Add((InPin)this.savedGates[k].pins[l]);
								((InPin)this.savedGates[k].pins[l]).connection = op;
							}
						}
					}
					if (template.outPins.Contains(template.savedGates[i].pins[j])) 
					{
						this.outPins.Add(op);
					}

				}
			}
		}

		foreach (Gate g in this.savedGates) 
		{
			Debug.WriteLine("Gate: "+g);
			Debug.WriteLine(g.pins.Count);
			foreach (Pin p in g.pins) 
			{
				Debug.WriteLine("Pin: "+p);
				if (p is InPin ip)
				{
					Debug.WriteLine(ip.connection);
				} else if (p is OutPin op) 
				{
					Debug.WriteLine(op.connections);
					foreach (InPin con in op.connections)
					{
						Debug.WriteLine(con);
					}
				}
			}
		}


		this.bounds = new Rectangle(x, y, 60 * Math.Max(Math.Max(inPins.Count, outPins.Count), 1), 80);
		for (int i = 0; i < this.inPins.Count; i++) 
		{
			this.inPins[i].offset = new Point(this.bounds.Width / (inPins.Count + 1) * (i+1)-10, this.bounds.Height - 10);
			this.pins.Add(this.inPins[i]);
		}
		for (int i = 0; i < this.outPins.Count; i++) 
		{
			this.outPins[i].offset = new Point(this.bounds.Width / (inPins.Count + 1) * (i + 1)-10, -10);
			this.pins.Add(this.outPins[i]);
		}
		this.MoveTo(this.bounds.Location);

	}

	public override bool Transfer() 
	{
		bool ret = false;
		foreach (Gate g in this.savedGates) 
		{
			if(g.Transfer()) 
			{
				ret = true;
			}
		}
		return ret;
	}

	public override void Process() 
	{
		base.Process();
		foreach (Gate g in this.savedGates) 
		{
			g.Process();
		}
	}

	public CustomGate createInstance() 
	{
		return new CustomGate(500, 500, this);
	}
}

//this weird data saving is because of JsonSerializer
public class GateData
{
	public string name { get; set; } = "";
	public List<int> connectionGates { get; set; } = new();
	public List<int> connectionPins { get; set; } = new();
}

public class CustomGateData
{
	public string name { get; set; } = "";
	public List<GateData> gateList { get; set; } = new List<GateData>();
	// inputGateIndexes[0] is the index of the gate in the list, inputPinIndexes[0] is the index of the pin in the Gate
	public List<int> inputGateIndexes { get; set; } = new List<int>();
	public List<int> inputPinIndexes { get; set; } = new List<int>();
	public List<int> outputGateIndexes { get; set; } = new List<int>();
	public List<int> outputPinIndexes { get; set; } = new List<int>();
}