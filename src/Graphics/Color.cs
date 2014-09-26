using System;

namespace Eresys
{
	/// <summary>
	/// Stelt een kleur voor in 32 bits (8 voor rood (R), groen (G), en blauw (B), en 8 voor het alphakanaal (A; = omgekeerde van transparantie)).
	/// </summary>
	public struct Color
	{
		public byte a, r, g, b;

		/// <summary>
		/// Initialiseert Color met R-, G-, en B-waarden. A wordt standaard op 255 (= ondoorzichtig) gezet.
		/// </summary>
		/// <param name="r">Rood</param>
		/// <param name="g">Groen</param>
		/// <param name="b">Blauw</param>
		public Color(byte r, byte g, byte b)
		{
			this.a = 255;
			this.r = r;
			this.g = g;
			this.b = b;
		}

		/// <summary>
		/// Initialiseer Color met A-, R-, G-, en B-waarden.
		/// </summary>
		/// <param name="a">Alpha</param>
		/// <param name="r">Rood</param>
		/// <param name="g">Groen</param>
		/// <param name="b">Blauw</param>
		public Color(byte a, byte r, byte g, byte b)
		{
			this.a = a;
			this.r = r;
			this.g = g;
			this.b = b;
		}

		/// <summary>
		/// Geeft een kleurcode in de vorm v/e DWORD zoals gebruikt in bvb HTML (vb: 0xFF3366CC of #FF3366CC)
		/// </summary>
		/// <returns></returns>
		public int ToColorCode()
		{
			return (a << 24) | (r << 16) | (g << 8) | b;
		}

		public static Color FromColorCode(int color)
		{
			return new Color((byte)(color >> 24), (byte)((0x00FF0000 & color) >> 16), (byte)((0x0000FF00 & color) >> 8), (byte)(0x000000FF & color));
		}
	}
}
