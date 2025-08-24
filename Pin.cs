using System;
using System.Diagnostics;

public class Pin
{
	public Rectangle bounds;
	public Point offset;
	public float width;
	public float height;

	public bool signal = false;
	
	public Pin(Gate parent, Point offset, int width, int height) {
		this.offset = offset;
		this.width = width;
		this.height = height;
		bounds = new Rectangle(parent.bounds.X + offset.X , parent.bounds.Y + offset.Y, width, height);
	}

	public bool Contains(Point pt) {
		return bounds.Contains(pt);
	}

	public void MoveTo(Point newLocation) {
		newLocation.Offset(offset);
		bounds = new Rectangle(newLocation, bounds.Size);
	}

	public virtual void Connect(Pin endPoint) {

	}

	public virtual void Draw(Graphics g) {
		Brush fillBrush;
		if (signal) {
			fillBrush = new SolidBrush(Color.LightGreen);
		} else {
			fillBrush = new SolidBrush(Color.Tomato);
		}
			using (fillBrush)
			using (Pen borderPen = new Pen(Color.DarkBlue, 3)) {
				g.FillEllipse(fillBrush, bounds);
				g.DrawEllipse(borderPen, bounds);
			}
	}
}

public class InPin : Pin {
	public OutPin? connection;
	public InPin(Gate parent, Point offset, int width, int height) : base(parent, offset, width, height) {

	}

	public override void Connect(Pin p) {
		if (p is InPin) {
			throw new Exception("Error: connection of two inPins");
		}
		p.Connect(this);
	}
}

public class OutPin : Pin {
	public List<InPin> connections;
	public List<Line> connectionLines;
	public OutPin(Gate parent, Point offset, int width, int height) : base(parent, offset, width, height) {
		connections = new List<InPin>();
		connectionLines = new List<Line>();
	}

	public override void Connect(Pin p) {
		if (p is OutPin) {
			throw new Exception("Error: connection of two OutPins");
		}
		InPin endPoint = (InPin)p;
		if (endPoint.connection != null) {
			endPoint.connection.RemoveConnection(endPoint);
			endPoint.connection = null;
		}
		this.connections.Add(endPoint);
		this.CreateLine(endPoint);
		endPoint.connection = this;
	}

	public void CreateLine(Pin endPoint) {
		this.connectionLines.Add(new Line(this, endPoint));
	}

	public void RemoveConnection(InPin p) {
		for (int i = 0; i < this.connections.Count; i++) {
			if (this.connections[i] == p) {
				connections.RemoveAt(i);
				connectionLines.RemoveAt(i);
				break;
			}
		}
	}

	public override void Draw(Graphics g) {
		base.Draw(g);
		foreach (Line l in connectionLines) {
		//Debug.WriteLine("hello");
			l.Draw(g);
		}
	}
}