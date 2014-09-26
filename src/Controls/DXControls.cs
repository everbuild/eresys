using System;
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
	public class DXControls: IControls
	{
		/// <summary>
		/// De constructor initialiseert DirectInput.
		/// </summary>
		public DXControls()
		{
			Kernel.Log.WriteLine("Initialising DXControls...");
			Kernel.Log.WriteLine();
			keyboardDevice = new Device(SystemGuid.Keyboard);
			keyboardDevice.SetCooperativeLevel(Kernel.Form, CooperativeLevelFlags.Exclusive | CooperativeLevelFlags.Foreground);
			keyboardDevice.Acquire();
			mouseDevice = new Device(SystemGuid.Mouse);
			mouseDevice.SetCooperativeLevel(Kernel.Form, CooperativeLevelFlags.Exclusive | CooperativeLevelFlags.Foreground);
			mouseDevice.Acquire();
			Update();
		}

		/// <summary>
		/// Pollt de status van het toetsenbord en muis. Deze methode zal normaal 1 x per frame worden aangeroepen.
		/// </summary>
		public void Update()
		{
			try
			{
				keyboardState = keyboardDevice.GetCurrentKeyboardState();
			}
			catch(Exception)
			{
				keyboardDevice.Acquire();
				keyboardState = keyboardDevice.GetCurrentKeyboardState();
			}
			try
			{
				mouseState = mouseDevice.CurrentMouseState;
			}
			catch(Exception)
			{
				mouseDevice.Acquire();
				mouseState = mouseDevice.CurrentMouseState;
			}
		}

		/// <summary>
		/// Geeft een lijst van alle ingedrukte toetsen.
		/// </summary>
		public ICollection GetActiveKeys()
		{
			ArrayList res = new ArrayList();
			for(Key key = Key.Escape; key <= Key.MediaSelect; key ++) 
				if(keyboardState[key])
					res.Add(key);
			return res;
		}

		/// <summary>
		/// Geeft aan of een toets wel of niet actief (= ingedrukt) is.
		/// </summary>
		public bool IsKeyActive(Key key)
		{
			return keyboardState[key];
		}

		/// <summary>
		/// Geeft aan of een muisknop wel of niet actief is.
		/// </summary>
		public bool IsMouseButtonActive(MouseButton button)
		{
			byte[] buttons = mouseState.GetMouseButtons();
			return buttons[(int)button] != 0;
		}

		public Point3D GetMousePosition()
		{
			return new Point3D(mouseState.X, mouseState.Y, mouseState.Z);
		}

		private Device keyboardDevice = null;
		private Device mouseDevice;
		private KeyboardState keyboardState;
		private MouseState mouseState;
	}
}
