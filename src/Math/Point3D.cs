using System;

namespace Eresys
{
	/// <summary>
	/// Deze struct beschrijft een 3dimensioneel punt.
	/// </summary>
	public struct Point3D
	{
		/// <summary>
		/// De tolerantie om de nauwkeurigheid in te stellen.
		/// </summary>
		public static float tolerance = 0.0000001f;

		/// <summary>
		/// Variabelen x , y , z (de coördinaten)
		/// </summary>
		public float x,y,z;

		/// <summary>
		/// De constructor die de coörds meekrijgt
		/// </summary>
		/// <param name="x">Geef de x coördinaat mee.</param>
		/// <param name="y">Geef de y coördinaat mee.</param>
		/// <param name="z">Geef de z coördinaat mee.</param>
		public Point3D(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Operator om een vector en een punt op te tellen.
		/// </summary>
		/// <param name="p">Geef een punt mee om bij de vector op te tellen.</param>
		/// <param name="v">Geef een vector mee om bij het punt op te tellen.</param>
		/// <returns></returns>
		public static Point3D operator+ (Point3D p, Vector v)
		{
			Point3D w = new Point3D(p.x + v.x, p.y + v.y, p.z + v.z);
			return w;
		}
		
		/// <summary>
		/// Operator om het verschil tussen een punt en een vector te berekenen.
		/// </summary>
		/// <param name="p">Geef een punt mee.</param>
		/// <param name="v">Geef een vector mee om van het punt af te trekken.</param>
		/// <returns></returns>
		public static Point3D operator- (Point3D p, Vector v)
		{
			Point3D w = new Point3D(p.x - v.x, p.y - v.y, p.z - v.z);
			return w;
		}

		/// <summary>
		/// Operator om twee punten op te tellen.
		/// </summary>
		/// <param name="p1">Geef een punt mee om bij het andere op te tellen.</param>
		/// <param name="p2">Geef een punt mee om bij het andere op te tellen.</param>
		/// <returns></returns>
		public static Point3D operator+ (Point3D p1, Point3D p2)
		{
			Point3D w = new Point3D(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
			return w;
		}
		
		/// <summary>
		/// Operator om het verschil te berekenen tussen 2 punten.
		/// </summary>
		/// <param name="p1">Geef een punt mee.</param>
		/// <param name="p2">Geef een punt mee om van het andere punt af te trekken.</param>
		/// <returns></returns>
		public static Point3D operator- (Point3D p1, Point3D p2)
		{
			Point3D w = new Point3D(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
			return w;
		}

		/// <summary>
		/// Methode om een punt naar een vector te converteren.
		/// </summary>
		/// <param name="p">Geef een punt mee.</param>
		/// <returns></returns>
		public static implicit operator Vector (Point3D p)
		{
			return new Vector(p.x, p.y, p.z);
		}

		/// <summary>
		/// Methode om een punt te schalen.
		/// </summary>
		/// <param name="scalar">Geef een scaler me om het huidige punt mee te schalen.</param>
		public void Scale(float scalar)
		{
			x *= scalar;
			y *= scalar;
			z *= scalar;
		}

		/// <summary>
		/// Setter om de coordinaten van een punt te setten.
		/// </summary>
		/// <param name="xx">Geef de X-coordinaat mee van het punt.</param>
		/// <param name="yy">Geef de Y-coordinaat mee van het punt.</param>
		/// <param name="zz">Geef de Z-coordinaat mee van het punt.</param>
		public void setComponents(float xx, float yy, float zz)
		{
			this.x = xx;
			this.y = yy;
			this.z = zz;
		}

		/// <summary>
		/// Methode om het punt op de oorsprong te zetten. Alle coordinaten op 0.
		/// </summary>
		public void setOrigin()
		{
			this.x = 0.0F;
			this.y = 0.0F;
			this.z = 0.0F;
		}


		/// <summary>
		/// Methode om te testen of het punt op de oorsprong ligt. Of alle coordinaten 0 zijn.
		/// </summary>
		/// <returns></returns>
		public bool isOrigin()
		{
			return (this.x == 0 && this.y == 0 && this.z ==0);
		}

		/// <summary>
		/// Methode om te testen of de coordinaten van een punt binnen een tollerantie van 0 liggen
		/// </summary>
		/// <returns></returns>
		public bool isOriginTolerance()
		{
			return ((this.x <= 0+tolerance && this.y <= 0+tolerance && this.z <=0+tolerance)&&(this.x >= 0-tolerance && this.y >= 0-tolerance && this.z >=0-tolerance));
		}

		/// <summary>
		/// Methode om te testen of 2 punten gelijk zijn.
		/// </summary>
		/// <param name="v">Geef een punt mee.</param>
		/// <param name="w">Geef een punt mee om te testen of het gelijk is aan het vorige punt.</param>
		/// <returns></returns>
		public bool equalPoints(Point3D v, Point3D w)
		{
			return (v.x == w.x & v.y == w.y & v.z == w.z);
		}

		/// <summary>
		/// Methode om de afstand tussen 2 punten te berekenen. 
		/// </summary>
		/// <param name="v">Geef een punt mee.</param>
		/// <param name="w">Geef een punt mee.</param>
		/// <returns></returns>
		public float distance(Point3D v, Point3D w)
		{
			Vector z = new Vector(v.x - w.x, v.y - w.y, v.z - w.z);
			return z.Magnitude();
		}
	}
}
