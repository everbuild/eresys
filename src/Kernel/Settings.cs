using System;
using System.Collections;
using System.IO;

namespace Eresys
{
	/// <summary>
	/// Bevat een verzameling instellingen. Door gebruik van een indexer is deze klasse ahw een associatieve array van
	/// naam-waarde paren die de instellingen bevatten.
	/// </summary>
	public class Settings
	{
		/// <summary>
		/// Deze array bevat alle instellingen en hun default-waarden. De array bestaat uit een lijst van telkens 2
		/// elementen: de naam en de default-waarde v/d instelling.
		/// Om instellingen toe te voegen moet hier dus telkens 1 lijn worden toegevoegd (= 2 nieuwe array elementen).
		/// </summary>
		private static string[] SETTINGS =
		{
			"graphics",		"directx",
			"controls",		"directx",
			"fullscreen",	"true",
			"width",		"800",
			"height",		"600",
			"depth",		"16",
			"vsync",		"false",
			"hal",			"true",
			"tnl",			"false",
			"brightness",	"0.6",
			"contrast",		"0.5",
			"gamma",		"1",
			"map",			"sky.bsp",
			"profiler",		"false",
		};

		public string this[string index]
		{
			get
			{
				index = index.ToLower();
				string val = ini.Read(section, index);
				if(val == "")
				{
					for(int i = 0; i < SETTINGS.Length; i += 2)
					{
						if(SETTINGS[i] == index)
						{
							val = SETTINGS[i + 1];
							ini.Write(section, index, val);
							break;
						}
					}
				}
				return val;
			}
			set
			{
				ini.Write(section, index, value);
			}
		}

		/// <summary>
		/// De constructor laad eerst de default instellingen. Vervolgens wordt geprobeert om de instellingen te laden
		/// vanuit een .ini bestand. Als dit niet lukt (bvb als het bestand nog niet bestaat) of als niet alle waarden
		/// in het bestand zijn opgenomen is dit geen enkel probleem gezien de defaults reeds zijn geladen.
		/// </summary>
		/// <param name="iniFileName">De naam van het .ini bestand dat zal worden geassociëert met deze klasse</param>
		public Settings(string iniFileName)
		{
			ini = new Ini(iniFileName);
		}

		private static string section = "settings";
		private Ini ini;
	}
}
