using System;
using System.IO;

namespace Eresys
{
	/// <summary>
	/// Summary description for Log.
	/// </summary>
	public class Log
	{
		public Log(string logFileName)
		{
			this.logFileName = logFileName;
			logStarted = false;
		}

		public void Write(string text)
		{
			FileStream fs = new FileStream(logFileName, logStarted ? FileMode.Append : FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter(fs);
			sw.Write(text);
			sw.Close();
			fs.Close();
			logStarted = true;
		}

		public void WriteLine()
		{
			Write("\r\n");
		}

		public void WriteLine(string text)
		{
			Write(text);
			WriteLine();
		}

		public void WriteHeader(string title, bool time, bool system)
		{
			WriteLine(title);
			WriteLine(new string('-', title.Length));
			WriteLine();

			if(time)
			{
				WriteLine("Date And Time: " + DateTime.Now.ToString("MM/dd/yyyy") + " " + DateTime.Now.ToString("T"));
				WriteLine();
			}

			if(system)
			{
				WriteLine("System Info:");
				SystemInfo nfo = new SystemInfo();
				WriteLine("  Operating System: " + nfo.OperatingSystem);
				WriteLine("  Processor:        " + nfo.Processor);
				WriteLine("  Memory:           " + nfo.Memory);
				WriteLine("  Mainboard:        " + nfo.Mainboard);
				WriteLine("  Graphics Card:    " + nfo.GraphicsCard);
				WriteLine("  Sound Card:       " + nfo.SoundCard);
				WriteLine();
			}
		}

		private string logFileName;
		private bool logStarted;
	}
}
