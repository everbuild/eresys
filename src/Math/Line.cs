using System;

namespace Eresys
{
	public class Line 
	{
		public Point3D p1, p2;
		public Vector D;
		public float d;

		public Line(Point3D p1, Point3D p2)
		{
			this.p1 = p1;
			this.p2 = p2;
			D = p2 - p1;
			d = D.Magnitude();
			D.Normalize();
		}

		public Line(Point3D p, Vector v)
		{
			p1 = p;
			p2 = p + v;
			D = v;
			D.Normalize();
			d = v.Magnitude();
		}
	}

}
