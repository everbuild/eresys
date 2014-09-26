using System.Collections;
using Microsoft.DirectX.DirectInput;

namespace Eresys
{
	/// <summary>
	/// Net zoals Graphics bied deze klasse een interface naar het onderliggende DirectX.
	/// </summary>
	/// <remarks>
	/// Alhoewel makkelijk kan worden nagegaan of een knop actief is via KeyDown() en MouseButtonDown() is het toch aan te
	/// raden hiervoor gebruik te maken van de klasse Button -> zie button.cs voor meer uitleg.
	/// </remarks>
	public interface IControls
	{
		void Update();
		ICollection GetActiveKeys();
		bool IsKeyActive(Key key);
		bool IsMouseButtonActive(MouseButton button);
		Point3D GetMousePosition();
	}
}
