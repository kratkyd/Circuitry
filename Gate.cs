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
	public List<Gate> savedGates = new List<Gate> { };
	public List<InPin> inPins = new List<InPin> { };
	public List<OutPin> outPins = new List<OutPin> { };
	List<InputGate> inputGates;
	List<OutputGate> outputGates;


	//public CustomGate(Control parent, )
	public CustomGate (int x, int y, Control parent, CustomGateData data) : base (x, y, parent)
	{
		this.text = data.name;
		this.bounds = new Rectangle(x, y, Math.Max(80, 40 + 40*Math.Max(data.inputGateIndexes.Count, data.outputGateIndexes.Count)), 80);
		foreach (GateData gd in data.gateList)
		{
			Gate obj;
			if (gd.name == "InputGate" || gd.name == "OutputGate")
			{
				obj = (Gate)Activator.CreateInstance(typeof(BlankGate), x, y, parent);
			}
			else
			{
				obj = (Gate)Activator.CreateInstance(GateTypes.basicGates[gd.name], x, y, parent);
			}
			this.savedGates.Add(obj);
		}

		// recreate connections
		for (int i = 0; i < data.gateList.Count; i++)
		{
			if (data.gateList[i].name == "InputGate") { continue; }
			int j = 0;
			foreach (Pin p in savedGates[i].pins)
			{
				if (p is InPin ip)
				{
					Debug.WriteLine(i + " " + j);
					if (data.gateList[i].connectionGates[j] != -1)
					{
						ip.Connect(savedGates[data.gateList[i].connectionGates[j]].pins[data.gateList[i].connectionPins[j]]);
					}
					j++;
				}
			}
		}

		for (int i = 0; i < data.inputGateIndexes.Count; i++)
		{
			Pin p = this.savedGates[data.inputGateIndexes[i]].pins[1];
			p.offset = new Point((this.bounds.Width)/(data.inputGateIndexes.Count+1)*(i+1) - 10, this.bounds.Height-10);
			this.pins.Add(p);
		}

		for (int i = 0; i < data.outputGateIndexes.Count; i++)
		{
			Pin p = this.savedGates[data.outputGateIndexes[i]].pins[0];
			p.offset = new Point((this.bounds.Width) / (data.outputGateIndexes.Count + 1) * (i + 1) - 10, -10);
			this.pins.Add(p);
		}
		//to align pins
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
	public List<int> outputGateIndexes { get; set; } = new List<int>();
}

public static class GateTypes
{
	public static Dictionary<string, Type> basicGates = new Dictionary<string, Type>
		{
			{ "OneGate", typeof(OneGate) },
			{ "ZeroGate", typeof(ZeroGate) },
			{ "NotGate", typeof(NotGate) },
			{ "AndGate", typeof(AndGate) },
			{ "NandGate", typeof(NandGate) },
			{ "OrGate", typeof(OrGate) },
			{ "NorGate", typeof(NorGate) },
			{ "XorGate", typeof(XorGate) },
			{ "BlankGate", typeof(BlankGate) },
			{ "InputGate", typeof(InputGate) },
			{ "OutputGate", typeof(OutputGate) }
		};
}