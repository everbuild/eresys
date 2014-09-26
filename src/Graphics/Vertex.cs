using System;

namespace Eresys
{
	/// <summary>
	/// Deze struct bevat de informatie van een vertex
	/// </summary>
	public struct Vertex
	{
		/// <summary>
		/// Positie van de ertex
		/// </summary>
		public Point3D position;

		/// <summary>
		/// texture coördinaat dat met deze vertex is verbonden. Bij deze coördinaten worden de afmetingen van alle
		/// textures gezien als 1x1. Stel dat deze vertex rechts onderaan een afbeelding ligt dan krijgt deze het
		/// coördinaat (1, 1). Moest deze net in het midden ligeen (0.5, 0.5). Moest deze in het midden bovenaan liggen
		/// (0.5, 0). En zo voort...
		/// </summary>
		public Point2D texCoord;

		/// <summary>
		/// texture coördinaat dat met deze vertex is verbonden. Bij deze coördinaten worden de afmetingen van alle
		/// textures gezien als 1x1. Stel dat deze vertex rechts onderaan een afbeelding ligt dan krijgt deze het
		/// coördinaat (1, 1). Moest deze net in het midden ligeen (0.5, 0.5). Moest deze in het midden bovenaan liggen
		/// (0.5, 0). En zo voort...
		/// </summary>
		public Point2D lightCoord;

		/// <summary>
		/// Initiliseert een Vertex met bepalde waarden
		/// </summary>
		/// <param name="position">Positie van de vertex</param>
		/// <param name="texCoord">Texture coördinaat van de vertex</param>
		public Vertex(Point3D position, Point2D texCoord, Point2D lightCoord)
		{
			this.position = position;
			this.texCoord = texCoord;
			this.lightCoord = lightCoord;
		}

		public Vertex(Point3D position, Point2D texCoord)
		{
			this.position = position;
			this.texCoord = texCoord;
			lightCoord = new Eresys.Point2D(0.0f, 0.0f);
		}
	}
}
