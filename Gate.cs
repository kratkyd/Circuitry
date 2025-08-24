using System.Diagnostics;
using System.Drawing;

public abstract class Gate {
	public Rectangle bounds { get; protected set; }
	public List<Pin> pins;
	public String text;

	public Gate(int x, int y, int width, int height) {
		//does this need to be here?
	}

	public bool Contains(Point pt) {
		return bounds.Contains(pt);
	}

	public void MoveTo(Point newLocation) {
		bounds = new Rectangle(newLocation, bounds.Size);
		foreach(Pin p in pins) {
			p.MoveTo(newLocation);
		}
	}

	public void Transfer() {
		foreach (Pin p in pins) {
			if (p is not OutPin pin || pin.connections.Count == 0) {
				continue;
			}
			foreach (InPin c in pin.connections) {
				c.signal = p.signal;
			}
		}
	}	

	public virtual void Process() {

	}

	public virtual void Draw(Graphics g) {
		using (Brush fillBrush = new SolidBrush(Color.LightBlue))
		using (Pen borderPen = new Pen(Color.DarkBlue, 3)) {
			g.FillRectangle(fillBrush, bounds);
			g.DrawRectangle(borderPen, bounds);
		}
		foreach (Pin p in pins) {
			p.Draw(g);
		}

		using (Font font = new Font("Arial", 14))
		using (Brush brush = new SolidBrush(Color.Black)) {
			StringFormat format = new StringFormat {
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};

			g.DrawString(text, font, brush, bounds, format);
		}
	}
}

public class AndGate : Gate {
	public AndGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "AND";
	}

	public override void Process() {
		pins[0].signal = pins[1].signal && pins[2].signal;
	}
}

public class OneGate : Gate {
	public OneGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20)
		};
		pins[0].signal = true;
		text = "1";
	}
}

public class ZeroGate : Gate {
	public ZeroGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20)
		};
		text = "0";
	}
}

public class OrGate : Gate {
	public OrGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "OR";
	}

	public override void Process() {
		pins[0].signal = pins[1].signal || pins[2].signal;
	}
}

public class XorGate : Gate {
	public XorGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "XOR";
	}

	public override void Process() {
		pins[0].signal = pins[1].signal ^ pins[2].signal;
	}
}

public class NandGate : Gate {
	public NandGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(10, this.bounds.Height-10), 20, 20),
			new InPin(this, new Point(this.bounds.Width-30, this.bounds.Height-10), 20, 20)
		};
		text = "NAND";
	}

	public override void Process() {
		pins[0].signal = !(pins[1].signal && pins[2].signal);
	}
}

public class NotGate : Gate {
	public NotGate(int x, int y, int width, int height) : base(x, y, width, height) {
		bounds = new Rectangle(x, y, width, height);
		pins = new List<Pin> {
			new OutPin(this, new Point(this.bounds.Width/2-10, -10), 20, 20),
			new InPin(this, new Point(this.bounds.Width/2-10, this.bounds.Height-10), 20, 20)
		};
		text = "NOT";
	}

	public override void Process() {
		pins[0].signal = !pins[1].signal;
	}
}