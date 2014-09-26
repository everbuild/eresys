using System;

namespace Eresys
{
	/// <summary>
	/// Deze klasse bevat een aantal methodes voor rekenkundige bewerkingen evenals enkele klassen voor representatie van
	/// bvb een vector.
	/// </summary>
	/// <remarks>
	/// De preciese vorm van deze klasse ligt nog niet vast omdat nog nader moet worden bekeken wat hier precies allemaal
	/// nodig is.
	/// </remarks>
	public class Math
	{
		public struct Point2D
		{
			public float x, y;

			public Point2D(float x, float y)
			{
				this.x = x;
				this.y = y;
			}
		}

		public struct Point
		{
			public float x, y, z;

			public Point(float x, float y, float z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public static Point operator+ (Point p, Vector v)
			{
				p.x += v.x;
				p.y += v.y;
				p.z += v.z;
				return p;
			}
		}

		public class Vector
		{
			public float x, y, z;

			public Vector()
			{
				this.x = 0.0f;
				this.y = 0.0f;
				this.z = 0.0f;
			}

			public Vector(float x, float y, float z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
		}

		public class Matrix
		{
		}
	}
}
