using System;

public class Line
{
	private Pin startPoint;
	private Pin endPoint;
	public Line(Pin startPoint, Pin endPoint) {
		this.startPoint = startPoint;
		this.endPoint = endPoint;
	}

	public void Draw(Graphics g) {
		using (Pen linePen = new Pen(Color.Gray, 5)) {
			Point startPointLoc = startPoint.bounds.Location;
			Point endPointLoc = endPoint.bounds.Location;
			startPointLoc.Offset(10, 10);
			endPointLoc.Offset(10, 10);
			g.DrawLine(linePen, startPointLoc, endPointLoc);
		}
	}
}