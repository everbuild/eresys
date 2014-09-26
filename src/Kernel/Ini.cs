using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Eresys
{
	/// <summary>
	/// Met deze klasse kan worden gewerkt met .INI bestanden. Een ini-bestand is een tekstbestand met een lijst naam-waarde
	/// paren, ingedeeld in verschillende secties. Ze kan makkelijk worden gebruikt om allerlij instellingen in bij te
	/// houden.
	/// Deze code was geïnspireerd door: http://www.codeproject.com/csharp/cs_ini.asp. Nof info kan worden gevonden op:
	/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/getprivateprofilestring.asp
	/// </summary>
	public class Ini
	{
		/// <summary>
		/// Initialiseerd een Ini object dat wordt verbonden met het opgegeven bestand. Dit hoeft geen bestaand bestand te zijn.
		/// In dat geval zal het worden aangemaakt wanneer de eerste schrijfbewerking ernaar wordt uitgevoerd.
		/// </summary>
		/// <param name="iniFileName">De naam van het ini-bestand</param>
		public Ini(string iniFileName)
		{
			if(iniFileName.IndexOf(':') >= 0)
				this.iniFileName = iniFileName;
			else
				this.iniFileName = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + iniFileName;
		}

		/// <summary>
		/// Schrijft een waarde naar het ini-bestand
		/// </summary>
		/// <param name="section">Naam van de sectie waarin de waarde geplaatst moet worden</param>
		/// <param name="key">Naam van de waarde</param>
		/// <param name="val">De waarde zelf, als een string voorgesteld. Andere typen moeten worden geconverteerd naar string.</param>
		public void Write(string section, string key, string val)
		{
			WritePrivateProfileString(section, key, val, iniFileName);
		}

		/// <summary>
		/// Leest een waarde uit een ini-bestand. De terug gegeven waarde is als een string voorgesteld. Als de waarde van een
		/// ander type is moet deze worden geparsed.
		/// </summary>
		/// <param name="section">Naam van de sectie waarin de waarde geplaatst moet worden</param>
		/// <param name="key">Naam van de waarde</param>
		/// <returns>De waarde</returns>
		public string Read(string section, string key)
		{
			// We moeten een buffer gebruiken met een bepaalde grootte. Dit wil zeggen dat waarden met meer kaarkters dan
			// die grootte niet volledig ingelezen zullen worden!
			StringBuilder temp = new StringBuilder(1024);
			int i = GetPrivateProfileString(section, key, "", temp, 1024, iniFileName);
			return temp.ToString();
		}

		private string iniFileName;

		// De volgende methoden worde gebruikt om te schrijven en te lezen naar/van het ini bestand

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
	}
}
