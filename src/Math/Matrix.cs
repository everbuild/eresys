using System;

namespace Eresys
{
	/// <summary>
	/// Deze klasse beschrijft een 4x4 Matrix. Deze matrix kan dan gebruikt worden om 
	/// een vector te roteren, te schalen of te verplaatsen.
	/// </summary>
	public class Matrix 
	{
		/// <summary>
		/// De 4x4 matrix. Alle elementen zijn floats.
		/// </summary>
		public float[,] element = new float[4,4];

		/// <summary>
		/// Constructor
		/// </summary>
		public Matrix() 
		{
			this.reset();
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="m">Geef een bestaande Matrix mee om hem te copieren.</param>
		public Matrix(Matrix m) 
		{
			element = new float[4,4];

			for(int i = 0; i < 4; ++i) 
				for (int j = 0; j < 4; ++j) 
					element[i,j] = m.element[i,j];
		}

		/// <summary>
		/// De matrix resetten naar:
		/// 
		///		|1 0 0 0|
		///		|0 1 0 0|
		///		|0 0 1 0|
		///		|0 0 0 1|
		///		
		///	(De diagonaal moet 1 zijn want dit is de schaal. Als deze ook 0 waszou de vector ook 0 zijn.)	
		/// </summary>
		public void reset ()
		{
			for(int i = 0; i < 4; ++i)
				for(int j = 0; j < 4; ++j)
					if (i == j)
						element[i,j] = 1;
					else
						element[i,j] = 0;
		}

		/// <summary>
		/// Methode om een matrix met een andere matrix te vermenigvuldigen. Dit is nodig om
		/// de verschillende effecten van matrixen op een vector te combineren.
		/// Vb. Roteren over de X-as en verplaatsen = 2 matrixen vermenigvuldigen.
		/// </summary>
		/// <param name="m">Geef een matrix mee waarme je de huidige matrix mee wil vermenigvuldigen.</param>
		public void multiply(Matrix m) 
		{
			float[,] e = new float[4,4];
        
			for(int i = 0; i < 4; ++i)
				for(int j = 0; j < 4; ++j)
					for(int k = 0; k < 4; ++k)
						e[i,j] += element[k,j] * m.element[i,k];

			for(int i = 0; i < 4; ++i)
				for(int j = 0; j < 4; ++j)
					element[i,j] = e[i,j];
		}

		/// <summary>
		/// Methode om de schaal te wijzigen
		/// De schaal instellen:
		///
		///		|s 0 0 0|
		///		|0 s 0 0|
		///		|0 0 s 0|
		///		|0 0 0 s|
		///
		/// </summary>
		/// <param name="s">Geef een scalair mee die aangeeft hoeveel keer je de vector wil vergroten of verkleinen.</param>
		public void scale(float s) 
		{
			Matrix m = new Matrix();

			for(int i = 0; i < 4; ++i)
				element[i,i] = s;

			this.multiply(m);
		}

		/// <summary>
		/// Methode om de verplaatsing (translatie) in te stellen over de x, y of z as.
		///
		///		| 1  0  0 x|
		///		| 0  1  0 y|
		///		| 0  0  1 z|
		///		| 0  0  0 1|
		/// </summary>
		/// <param name="x">Geef de verplaatsing over de X-as mee.</param>
		/// <param name="y">Geef de verplaatsing over de Y-as mee.</param>
		/// <param name="z">Geef de verplaatsing over de Z-as mee.</param>
		public void translate (float x, float y, float z)
		{
			Matrix m = new Matrix();
			m.element[0,3] = x;
			m.element[1,3] = y;
			m.element[2,3] = z;

			this.multiply(m);
		}
		
		/// <summary>
		/// Methode om de verplaatsing in te stellen met een vector.
		/// </summary>
		/// <param name="vec">Geef een vector mee langs waar er verplaatst moet worden.</param>
		public void translate(Vector vec)
		{
			translate(vec.x, vec.y, vec.z);
		}

		/// <summary>
		/// Methode om de rotatie rond de X-as in te stellen.
		/// </summary>
		/// <param name="d">Geef een hoek in radialen mee.</param>
		public void rotatex(float d)
		{
			float cos, sin;

			sin = (float) Math.Sin(d);
			cos = (float) Math.Cos(d);

			rotatex(sin, cos);
		}

		/// <summary>
		/// Methode om de rotatie rond de X-as te berekenen
		/// </summary>
		/// <param name="sin">Geef sinus van de rotatiehoek mee.</param>
		/// <param name="cos">Geef cosinus van rotatiehoek mee.</param>
		public void rotatex(float sin, float cos)
		{
			Matrix m = new Matrix();

			m.element[1,1] =  cos;
			m.element[2,2] =  cos;
			m.element[1,2] =  -sin;
			m.element[2,1] =  sin;

			this.multiply(m);
		}

		/// <summary>
		/// Methode om de rotatie rond de Y-as in te stellen.
		/// </summary>
		/// <param name="d">Geef een hoek in radialen mee.</param>
		public void rotatey(float d)
		{
			float cos, sin;

			sin = (float) Math.Sin(d);
			cos = (float) Math.Cos(d);
			
			rotatey(sin, cos);
		}

		/// <summary>
		/// Methode om de rotatie rond de Y-as te berekenen
		/// </summary>
		/// <param name="sin">Geef sinus van de rotatiehoek mee.</param>
		/// <param name="cos">Geef cosinus van rotatiehoek mee.</param>
		public void rotatey(float sin, float cos)
		{
			Matrix m = new Matrix();

			m.element[0,0] =  cos;
			m.element[2,2] =  cos;
			m.element[0,2] =  sin;
			m.element[2,0] =  -sin;

			this.multiply(m);
		}

		/// <summary>
		/// Methode om de rotatie rond de Z-as in te stellen.
		/// </summary>
		/// <param name="d">Geef een hoek in radialen mee.</param>
		public void rotatez(float d)
		{
			float cos, sin;

			sin = (float) Math.Sin(d);
			cos = (float) Math.Cos(d);

			rotatez(sin, cos);
		}

		/// <summary>
		/// Methode om de rotatie rond de Z-as te berekenen
		/// </summary>
		/// <param name="sin">Geef sinus van de rotatiehoek mee.</param>
		/// <param name="cos">Geef cosinus van rotatiehoek mee.</param>
		public void rotatez(float sin, float cos)
		{
			Matrix m = new Matrix();

			m.element[0,0] =  cos;
			m.element[1,1] =  cos;
			m.element[0,1] =  -sin;
			m.element[1,0] =  sin;

			this.multiply(m);
		}

		/// <summary>
		/// Methode om te roteren rond de X, Y en Z as.
		/// </summary>
		/// <param name="RotY">Geef de rotatiehoek rond de Y-as mee.</param>
		/// <param name="RotX">Geef de rotatiehoek rond de Y-as mee.</param>
		/// <param name="RotZ">Geef de rotatiehoek rond de Y-as mee.</param>
		/// <returns></returns>
		public static Matrix RotationYawPitchRoll(float RotY, float RotX, float RotZ)
		{
			Matrix res = new Matrix();

			res.rotatez(RotZ);
			res.rotatex(RotX);
			res.rotatey(RotY);

			return res;		
		}
		
		/// <summary>
		/// Methode om de verplaatsing (translatie) in te stellen over de x, y of z as.
		///
		///		| 1  0  0 x|
		///		| 0  1  0 y|
		///		| 0  0  1 z|
		///		| 0  0  0 1|
		/// </summary>
		/// <param name="x">Geef de verplaatsing over de X-as mee.</param>
		/// <param name="y">Geef de verplaatsing over de Y-as mee.</param>
		/// <param name="z">Geef de verplaatsing over de Z-as mee.</param>
		public static Matrix Translation(float x, float y, float z)
		{
			Matrix res = new Matrix();
			res.translate(x, y, z);
			return res;
		}
	
		/// <summary>
		/// Methode om een left-handed view matrix aan te maken.  
		/// </summary>
		/// <param name="position">Geef de positie van het oogpunt weer.</param>
		/// <param name="target">Geef de positie mee naar waar gekeken wordt.</param>
		/// <param name="up">Geef de richting mee die naar boven wijst.</param>
		/// <returns></returns>
		public static Matrix LookAtLH(Vector position, Vector target, Vector up)
		{
			Matrix res = new Matrix();
			Vector xaxis, yaxis, zaxis;

			zaxis = target - position;
			zaxis.Normalize();

			xaxis = up.Cross(zaxis);
			xaxis.Normalize();

			yaxis = zaxis.Cross(xaxis);

			res.element[0, 0] = xaxis.x;
			res.element[1, 0] = yaxis.x;
			res.element[2, 0] = zaxis.x;
			res.element[3, 0] = 0;

			res.element[0, 1] = xaxis.y;
			res.element[1, 1] = yaxis.y;
			res.element[2, 1] = zaxis.y;
			res.element[3, 1] = 0;

			res.element[0, 2] = xaxis.z;
			res.element[1, 2] = yaxis.z;
			res.element[2, 2] = zaxis.z;
			res.element[3, 2] = 0;

			res.element[0, 3] = -xaxis.Dot(position);
			res.element[1, 3] = -yaxis.Dot(position);
			res.element[2, 3] = -zaxis.Dot(position);
			res.element[3, 3] = 1;

			return res;
		}

		/// <summary>
		/// De PerspectiveFovLHmethode converteert de 3D camera coordinaten in 2D schermcoordinaten. 
		/// </summary>
		/// <param name="fov">Geef de grootte van het gezichtsveld mee.</param>
		/// <param name="aspect">Geef de aspect ratio mee.</param>
		/// <param name="nearClipZ">Geef het nearclipping plane mee.</param>
		/// <param name="farClipZ">Geef het farclipping mee.</param>
		/// <returns></returns>
		public static Matrix PerspectiveFovLH(float fov, float aspect, float nearClipZ, float farClipZ)
		{
			Matrix res = new Matrix();

			float a, w, h;

			a = fov/2;
			h = (float)System.Math.Cos(a)/(float)System.Math.Sin(a);
			w = h/aspect;

			a = farClipZ/(farClipZ - nearClipZ);

			res.element[0, 0] = w;
			res.element[1, 0] = 0;
			res.element[2, 0] = 0;
			res.element[3, 0] = 0;

			res.element[0, 1] = 0;
			res.element[1, 1] = h;
			res.element[2, 1] = 0;
			res.element[3, 1] = 0;

			res.element[0, 2] = 0;
			res.element[1, 2] = 0;
			res.element[2, 2] = a;
			res.element[3, 2] = 1;

			res.element[0, 3] = 0;
			res.element[1, 3] = 0;
			res.element[2, 3] = -nearClipZ*a;
			res.element[3, 3] = 0;

			return res;
		}

	}

}
