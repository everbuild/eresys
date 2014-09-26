using System;

namespace Eresys
{
	public class Plane
	{
		public static float tolerance = 0.0000001f;

		public float a, b, c, d;

		public Plane()
		{
			this.a = 0.0f;
			this.b = 0.0f;
			this.c = 0.0f;
			this.d = 0.0f;
		}

		public Plane(float a, float b, float c, float d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}

		public void Normalize()
		{
			float mag = (float)System.Math.Sqrt(a*a + b*b + c*c);
			if(mag <= tolerance) return;
			a /= mag;
			b /= mag;
			c /= mag;
			d /= mag;
		}

		public float Distance(Point3D p)
		{
			return a*p.x + b*p.y + c*p.z - d;
		}

		public Vector GetNormal()
		{
			return new Vector(a, b, c);
		}

		public bool Intersect(Line line, ref Point3D point)
		{
			float num, denom, d;
			Vector normal = GetNormal();
			Vector v;

			v = normal*line.D;
			denom = v.x + v.y + v.z;

			if(System.Math.Abs(denom) <= tolerance) return false;

			v = normal*line.p1;
			num = v.x + v.y + v.z - this.d;

			d = -num/denom;

			point = line.p1 + line.D*d;

			return true;
		}
	}
}
