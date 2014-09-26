using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;

namespace Eresys
{
	public enum ButtonType
	{
		Key,
		Trigger
	}

	/// <summary>
	/// Deze klasse representeert een enkele knop op zowel het toetsenbord als de muis. De klasse gebruikt Controls om de
	/// status van de betreffende knop na te gaan.
	/// Er bestaan 2 varianten (misschien wel meer, maar voorlopig alvast 2) van een knop die ik 'key' en 'trigger' heb
	/// gedoopt. Misschien (redelijk zeker?) is deze naamgeving nogal ongelukkig, en ik zou dan ook blij zijn mocht iemand
	/// een beter ingave krijgen! Een 'key' is eigenlijk gewoon hetzelfde als Controls.KeyDown() of
	/// Controls.MouseButtonDown() aanroepen, terwijl een 'trigger' slechts gedurende 1 frame actief zal zijn vanaf het
	/// moment dat die knop wordt ingedrukt. Dit kan bvb handig zijn voor een knop die bvb wireframe rendering aan of uit
	/// zet. Als dit een 'key' zou zijn en de gebruiker drukt er langer dan 1 frame op (makkelijk het geval als uw spel een
	/// beetje deftig bolt) dan zal het gevolg zijn  dat bij elk frame wordt geswitched tussen wireframe en gewone
	/// rendering. Redelijk irritant.
	/// </summary>
	public class Button
	{
		/// <summary>
		/// Hiermee kan de toets worden aan- of uitgeschakelt (standaard = aan).
		/// </summary>
		public bool enabled;

		/// <summary>
		/// Met deze constructor wordt een Button aangemaakt die een toets representeert.
		/// </summary>
		/// <param name="Eresys.Controls">Controls object dat zal worden gebruikt om de status van de toets op te vragen.</param>
		/// <param name="key">karakter v/d toets</param>
		/// <param name="type">type van de toets (key of trigger)</param>
		public Button(Key key, ButtonType type)
		{
			this.enabled	= true;
			this.mouse		= false;
			this.key		= key;
			this.type		= type;
			this.wasActive	= false;
		}

		/// <summary>
		/// Met deze constructor wordt een Button aangemaakt die een muisknop representeert.
		/// </summary>
		/// <param name="Eresys.Controls">Controls object dat zal worden gebruikt om de status van de muisknop op te vragen.</param>
		/// <param name="key">karakter v/d muisknop</param>
		/// <param name="type">type van de muisknop (key of trigger)</param>
		public Button(MouseButton button, ButtonType type)
		{
			this.enabled	= true;
			this.mouse		= true;
			this.button		= button;
			this.type		= type;
			this.wasActive	= false;
		}

		/// <summary>
		/// Geeft de status van de knop weer.
		/// </summary>
		/// <returns>
		/// TRUE als de knop actief is.
		/// FALSE als de knop inactief is.
		/// </returns>
		public bool Active()
		{
			if(!enabled) return false;
			bool active = mouse ? Kernel.Controls.IsMouseButtonActive(button) : Kernel.Controls.IsKeyActive(key);
			if(type == ButtonType.Key)
				return active;
			else
			{
				if(active)
				{
					if(wasActive) return false;
					wasActive = true;
					return true;
				}
				else
				{
					wasActive = false;
					return false;
				}
			}
		}

		private bool mouse;
		private Key key;
		private MouseButton button;
		private ButtonType type;
		private bool wasActive;
	}
}
