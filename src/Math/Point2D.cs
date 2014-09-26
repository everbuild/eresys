using System;

namespace Eresys
{
	/// <summary>
	/// Deze struct beschrijft een 2dimensioneel punt.
	/// </summary>
	public struct Point2D
	{
		/// <summary>
		/// Variabelen x , y (de coördinaten)
		/// </summary>
		public float x, y;

		/// <summary>
		/// De constructor die de coörds meekrijgt
		/// </summary>
		/// <param name="x">Geef de x coördinaat mee.</param>
		/// <param name="y">Geef de y coördinaat mee.</param>
		public Point2D(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}
}
