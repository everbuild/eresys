using System;

namespace Eresys
{
	/// <summary>
	/// Deze interface moet worden geïmplementeerd om de Eresys engine te gebruiken. Bij het opstarten van de kenrel moet
	/// een instantie van een implementatie van IApplication worden megegeven als parameter. De interface bevat een aantal
	/// methodes die dan door de kernel zullen worden opgeroepen op bepaalde momenten, een soort events dus.
	/// </summary>
	public interface IApplication
	{
		/// <summary>
		/// Wordt aangeroepen wanneer de kernel opgestart is, voordat deze begint te lopen.
		/// </summary>
		void Startup();

		/// <summary>
		/// Wordt aangeroepen wanneer de kernel normaal afgesloten wordt (om veiligheidsredenen kan dit niet wanneer de
		/// kenrel afsluit door een exceptie). Merk op dat alle compononten van de kenrel op dit ogenblik nog beschikbaar
		/// zijn.
		/// </summary>
		void Terminate();

		/// <summary>
		/// Wordt aangeroepen wanneer de kernel timer wordt gepauseerd of terug gestart
		/// </summary>
		/// <param name="pause">Geeft aan of het om pauseren (true) of terug starten (false) gaat</param>
		void Pause(bool pause);

		/// <summary>
		/// Wodrt opgeroepen wanneer de content van het spel moet worden ge-update. Hier kan bvb de status van verschillende
		/// toetsen worden nagegaan om dan aan de hand daarvan de scene aan te passen.
		/// </summary>
		void Update();

		/// <summary>
		/// Wordt aangeroepen wanneer de scene gerendert is. Op dit moment kunnen door de applicatie nog bepaalde zaken aan
		/// het zopas gerenderde frame worden toegevoegd.
		/// </summary>
		void Render();
	}
}
