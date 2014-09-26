
namespace Eresys
{
	/// <summary>
	/// Eenvoudige interface voor het renderen van graphics. Deze interface is toegangkelijk via Kernel.Graphics.
	/// Om te renderen moet eerst een Player worden ingesteld. Dit kan via Kernel.Scene.player.
	/// Er wordt eerst gerendert naar een buffer. Pas wanneer alles gerendert is wordt deze weergegeven op het scherm
	/// met een aanroep naar EndFrame.
	/// </summary>
	public interface IGraphics: System.IDisposable
	{
		/// <summary>
		/// Wire frame mode rendering (standaard uit)
		/// </summary>
		bool WireFrame { get; set; }

		/// <summary>
		/// Gebruik van statische belichting (met lightmaps dus) aan of uit zetten (staandaard aan)
		/// </summary>
		bool Lighting { get; set; }

		bool AlphaBlending { get; set; }
		
		bool TextureAlpha { get; set; }
		
		byte Alpha { get; set; }
		
		bool DepthBuffer { get; set; }

		bool Filtering { get; set; }

		/// <summary>
		/// Actieve camera die gebruikt zal worden om te renderen. Deze kan tijdens het renderen worden verandert.
		/// Standaard zal de camera van de player worden gebruikt die kan worden ingesteld via Kernel.Scene.player
		/// </summary>
		Camera Camera { get; set; }

		Matrix WorldMatrix { get; set; }

		float Brightness { get; set; }

		float Contrast { get; set; }

		float Gamma { get; set; }

		bool FrameClearing { get; set; }

		/// <summary>
		/// Voegt een vertex pool toe. Deze kan dan worden gebruikt bij het renderen. Dit heeft intern als gevolg dat de
		/// vertex data wordt ge-upload naar de grafische hardware.
		/// </summary>
		/// <param name="vertexPool">De vertex pool om toe te voegen</param>
		int AddVertexPool(VertexPool vertexPool);

		/// <summary>
		/// Voegt een afbeelding toe. Deze kan dan worden gebruikt bij het renderen. Dit heeft intern als gevolg dat de
		/// afbeelding wordt ge-upload naar de grafische hardware.
		/// </summary>
		/// <param name="vertexPool">De afbeelding om toe te voegen</param>
		int AddTexture(Texture texture);

		int AddFont(string name, float size, bool bold, bool italic);

		/// <summary>
		/// Verwijdert een eerder toegevoegde vertex pool. Intern zal het hiervoor gebruikte geheugen van de grafische
		/// hardware terug worden vrijgegeven.
		/// </summary>
		/// <param name="vertexPool">De vertex pool om te verwijderen</param>
		void RemoveVertexPool(int vertexPoolIdx);

		/// <summary>
		/// Verwijdert een eerder toegevoegde afbeelding. Intern zal het hiervoor gebruikte geheugen van de grafische
		/// hardware terug worden vrijgegeven.
		/// </summary>
		/// <param name="vertexPool">De afbeelding om te verwijderen</param>
		void RemoveTexture(int textureIdx);

		/// <summary>
		/// Rendert een triangle fan (zie oa. DXSDK)
		/// </summary>
		/// <param name="vertexPool">Bron van de te gebruiken vertex data</param>
		/// <param name="first">Eerste vertex van de fan</param>
		/// <param name="count">Aantal vertices van de fan</param>
		/// <param name="texture">Afbeelding die op de fan zal worden ge-mapped</param>
		void RenderTriangleFan(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx);

		void RenderTriangleStrip(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx);

		void RenderText(int fontIdx, Color color, Point2D position, string text);

		void RenderTexture(int textureIdx, float left, float top, float width, float height, float depth);

		/// <summary>
		/// Moet worden aangeroepen bij het begin van het renderen van elk frame, dus voor enige aanroep naar RenderTriangleXxx
		/// </summary>
		void BeginFrame();

		/// <summary>
		/// Moet worden aangeroepen wanneer alles gerendert is en het frame mag worden weergegeven op het scherm.
		/// </summary>
		void EndFrame();

		Texture TakeScreenshot();
	}
}
