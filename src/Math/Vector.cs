using System;
using Microsoft.DirectX;

namespace Eresys
{
	/// <summary>
	/// Deze struct beschrijft een vector.
	/// </summary>
	public struct Vector
	{

		/// <summary>
		/// De tolerantie om de nauwkeurigheid in te stellen.
		/// </summary>
		public static float tolerance = 0.0000001f;

		
		/// <summary>
		/// Variabelen x , y , z (de coördinaten)
		/// </summary>
		public float x, y, z;

		/// <summary>
		/// De constructor die de coörds meekrijgt
		/// </summary>
		/// <param name="x">Geef de x coördinaat mee.</param>
		/// <param name="y">Geef de y coördinaat mee.</param>
		/// <param name="z">Geef de z coördinaat mee.</param>
		public Vector(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Methode die de inverse van de vector geeft.
		/// </summary>
		public void Inverse()
		{
			x = -x;
			y = -y;
			z = -z;
		}

		/// <summary>
		/// Tel twee vectoren op.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="w">Geef een vector mee</param>
		/// <returns></returns>
		public static Vector operator + (Vector v, Vector w)
		{
			Vector res;
			res.x = v.x + w.x;
			res.y = v.y + w.y;
			res.z = v.z + w.z;
			return res;
		}

		/// <summary>
		/// Tel een vector en een punt op.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="p">Geef een punt mee.</param>
		/// <returns></returns>
		public static Vector operator + (Vector v, Point3D p)
		{
			Vector res;
			res.x = v.x + p.x;
			res.y = v.y + p.y;
			res.z = v.z + p.z;
			return res;
		}

		/// <summary>
		/// Trek twee vectoren van elkaar af.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="w">Geef een vector mee.</param>
		/// <returns></returns>
		public static Vector operator - (Vector v, Vector w)
		{
			Vector res;
			res.x = v.x - w.x;
			res.y = v.y - w.y;
			res.z = v.z - w.z;
			return res;
		}

		/// <summary>
		/// Trek een vector van een punt af.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="p">Geef een punt mee.</param>
		/// <returns></returns>
		public static Vector operator - (Vector v, Point3D p)
		{
			Vector res;
			res.x = v.x - p.x;
			res.y = v.y - p.y;
			res.z = v.z - p.z;
			return res;
		}

		/// <summary>
		/// Vermenigvuldig een vector met een scalair. (Schaal de vector)
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="r">Geef een scalair mee.</param>
		/// <returns></returns>
		public static Vector operator * (Vector v, float r)
		{
			Vector res;
			res.x = v.x*r;
			res.y = v.y*r;
			res.z = v.z*r;
			return res;
		}

		public static Vector operator * (float r, Vector v)
		{
			return v*r;
		}

		/// <summary>
		/// Methode om van de vector een nulvector te maken.
		/// </summary>
		public void NullVector()
		{
			this.x = 0.0f;
			this.y = 0.0f;
			this.z = 0.0f;
		}

		/// <summary>
		/// Methode om na te gaan of een vector een nukvector is.
		/// </summary>
		/// <returns></returns>
		public bool IsNullVector()
		{
			return (float)System.Math.Abs(x) <= tolerance && (float)System.Math.Abs(y) <= tolerance && (float)System.Math.Abs(z) <= tolerance;
		}

		/// <summary>
		/// Methode om de lengte van de vector te geven.
		/// </summary>
		/// <returns></returns>
		public float Magnitude()
		{
			return (float)System.Math.Sqrt(x*x + y*y + z*z);
		}
		
		/// <summary>
		/// Methode die de kadratische (squared) lengte geeft van de vector.
		/// </summary>
		/// <returns></returns>
		public float MagnitudeSquare()
		{
			return x*x + y*y + z*z;
		}

		/// <summary>
		/// Methode om van de vector een eenheidsvector te maken zodat de norm een lengte 1 heeft
		/// </summary>
		public void Normalize()
		{
			float mag = Magnitude();
			if(mag <= tolerance)
			{
				x = 0.0f;
				y = 0.0f;
				z = 0.0f;
			}
			else
			{
				x /= mag;
				y /= mag;
				z /= mag;
			}
		}
		
		/// <summary>
		/// Methode om te bepalen of de vector een eenheidsvector is
		/// </summary>
		/// <returns></returns>
		public bool IsUnitVector()
		{
			return (System.Math.Abs(MagnitudeSquare() - 1.0f) <= tolerance);
		}

		/// <summary>
		/// Operator om te controleren of 2 vectoren gelijk zijn.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="w">Geef een vector mee.</param>
		/// <returns></returns>
		public static bool operator == (Vector v, Vector w)
		{
			return (System.Math.Abs(v.x - w.x) <= tolerance && System.Math.Abs(v.y - w.y) <= tolerance && System.Math.Abs(v.z - w.z) <= tolerance);
		}

		/// <summary>
		/// Operator om te controleren of 2 vectoren niet gelijk zijn.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <param name="w">Geef een vector mee.</param>
		/// <returns></returns>
		public static bool operator != (Vector v, Vector w)
		{
			return !(v == w);
		}
		
		/// <summary>
		/// Mothode om een punt van een vector te maken.
		/// </summary>
		/// <param name="v">Geef een vector mee.</param>
		/// <returns></returns>
		public static implicit operator Point3D (Vector v)
		{
			return new Point3D(v.x, v.y, v.z);
		}

		/// <summary>
		/// Methode om het dot product van twee vectoren te berekenen.
		/// </summary>
		/// <param name="v">Geef een vector mee om de huidige vector mee te vermenigvuldigen (dot product)</param>
		/// <returns></returns>
		public float Dot(Vector v)
		{
			return x*v.x + y*v.y + z*v.z;	
		}

		/// <summary>
		/// Methode om het dot product van twee vectoren te berekenen.
		/// </summary>
		/// <param name="v">Geef een vector mee om de andere vector mee te vermenigvuldigen (dot product)</param>
		/// <param name="w">Geef een vector mee om de andere vector mee te vermenigvuldigen (dot product)</param>
		/// <returns></returns>
		public static float Dot(Vector v, Vector w)
		{
			return v.x*w.x + v.y*w.y + v.z*w.z;	
		}

		/// <summary>
		/// Methode om het cross product van twee vectoren te berekenen.
		/// </summary>
		/// <param name="v">Geef een vector mee om de huidige vector mee te vermenigvuldigen (cross product)</param>
		/// <returns></returns>
		public Vector Cross(Vector v)
		{
			Vector res;
			res.x = y*v.z - z*v.y;
			res.y = z*v.x - x*v.z;
			res.z = x*v.y - y*v.x;
			return res;
		}

		/// <summary>
		/// Methode om het cross product van twee vectoren te berekenen.
		/// </summary>
		/// <param name="v">Geef een vector mee om de andere vector mee te vermenigvuldigen (cross product)</param>
		/// <param name="w">Geef een vector mee om de andere vector mee te vermenigvuldigen (cross product)</param>
		/// <returns></returns>
		public static Vector Cross(Vector v, Vector w)
		{
			Vector res;
			res.x = v.y*w.z - v.z*w.y;
			res.y = v.z*w.x - v.x*w.z;
			res.z = v.x*w.y - v.y*w.x;
			return res;
		}

		/// <summary>
		/// Methode die het triple product van 3 vectoren uitrekend uvw = u.(v X w)
		/// </summary>
		/// <param name="v">Geef een vector mee om de andere 2 vectoren mee te vermenigvuldigen (triple product)</param>
		/// <param name="w">Geef een vector mee om de andere 2 vectoren mee te vermenigvuldigen (triple product)</param>
		/// <param name="x">Geef een vector mee om de andere 2 vectoren mee te vermenigvuldigen (triple product)</param>
		/// <returns></returns>
		 
		/// <summary>
		/// Methode die het mixed product van 3 vectoren uitrekend uvw = u.(v X w)
		/// </summary>
		/// <param name="v">Geef een vector mee om de andere 2 vectoren mee te vermenigvuldigen (mixed product)</param>
		/// <param name="w">Geef een vector mee om de andere 2 vectoren mee te vermenigvuldigen (mixed product)</param>
		/// <param name="x">Geef een vector mee om de andere 2 vectoren mee te vermenigvuldigen (mixed product)</param>
		/// <returns></returns>
		public float Mixed(Vector v, Vector w, Vector x)
		{
			return Dot(v, Cross(w, x));
		}

		/// <summary>
		/// Methode die het puntsgebonden product van 2 vectoren uirekent.
		/// </summary>
		/// <param name="v">Geef een vector mee om de andere vector mee te vermenigvuldigen (puntsgebonden product)</param>
		/// <param name="w">Geef een vector mee om de andere vector mee te vermenigvuldigen (puntsgebonden product)</param>
		/// <returns></returns>
		public static Vector operator * (Vector v, Vector w)
		{
			Vector res;
			res.x = v.x*w.x;
			res.y = v.y*w.y;
			res.z = v.z*w.z;
			return res;
		}

		/// <summary>
		/// Methode die het puntsgebonden product van een vector en een punt uirekent.
		/// </summary>
		/// <param name="v">Geef een vector mee het punt mee te vermenigvuldigen (puntsgebonden product)</param>
		/// <param name="p">Geef een punt mee om de vector mee te vermenigvuldigen (puntsgebonden product)</param>
		/// <returns></returns>
		public static Vector operator * (Vector v, Point3D p)
		{
			Vector res;
			res.x = v.x*p.x;
			res.y = v.y*p.y;
			res.z = v.z*p.z;
			return res;
		}

		/// <summary>
		/// Methode die het puntsgebonden deling van 2 vectoren uirekent.
		/// </summary>
		/// <param name="v">Geef een vector mee om de andere vector mee te delen (puntsgebonden deling)</param>
		/// <param name="w">Geef een vector mee om de andere vector mee te delen (puntsgebonden deling)</param>
		/// <returns></returns>
		public static Vector operator / (Vector v, Vector w)
		{
			Vector res;
			res.x = v.x/w.x;
			res.y = v.y/w.y;
			res.z = v.z/w.z;
			return res;
		}

		/// <summary>
		/// Operator om een vector te delen.
		/// </summary>
		/// <param name="v">Geef een vector mee die gedeeld moet worden.</param>
		/// <param name="r">Geef een float mee als deler.</param>
		/// <returns></returns>
		public static Vector operator / (Vector v, float r)
		{
			Vector res = new Vector(0.0f, 0.0f, 0.0f);
			//Eerst kijken dat de deler geen 0 is.
			if(v.x != 0.0f) res.x = v.x/r;
			if(v.y != 0.0f) res.y = v.y/r;
			if(v.z != 0.0f) res.z = v.z/r;
			return res;
		}

		/// <summary>
		/// Operator om een vector door een punt te delen.
		/// </summary>
		/// <param name="v">Geef een vector mee die gedeeld moet worden.</param>
		/// <param name="p">Geef een punt mee als deler.</param>
		/// <returns></returns>
		public static Vector operator / (Vector v, Point3D p)
		{
			Vector res;
			res.x = v.x/p.x;
			res.y = v.y/p.y;
			res.z = v.z/p.z;
			return res;
		}

		/// <summary>
		/// Methode om te kijken of twee vectoren gelijk zijn.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return (Vector)obj == this;
		}

		/// <summary>
		/// Methode om de Hashcode terug te krijgen.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return 0;
		}
		
		/// <summary>
		/// Methode om de vector met een matrix te vermenigvuldigen. 
		/// Om de vector te transformen. 
		/// </summary>
		/// <param name="vec">Geef de vector mee die getransformeerd moet worden.</param>
		/// <param name="m">Geef een transformatie matrix mee, om de vector mee te transformeren.</param>
		/// <returns></returns>
		public static Vector TransformNormal(Vector vec, Matrix m)
		{
			float[] e = new float[4];
			float[] element = {vec.x,vec.y,vec.z,1};

			for (int i = 0; i < 4; i++)
			{
				e[i] = 0;
				for (int j = 0; j < 4; j++)
					e[i] += element[j] * m.element[i,j];
			}

			for (int i = 0; i < 4; i++)
				element[i] = e[i];

			Vector r = new Vector(element[0],element[1], element[2]);
			return r; 
		}
	}
}
