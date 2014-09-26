using System;
using System.Management;
using System.Globalization;

namespace Eresys
{
	/// <summary>
	/// Summary description for SystemInfo.
	/// </summary>
	public class SystemInfo
	{
		public string OperatingSystem { get { return os; } }
		public string Processor { get { return cpu; } }
		public string Memory { get { return mem; } }
		public string Mainboard { get { return mobo; } }
		public string GraphicsCard { get { return vga; } }
		public string SoundCard { get { return sound; } }

		public SystemInfo()
		{
			ManagementObjectSearcher searcher;
			ObjectQuery query;
			uint i;
               
			os		= "N/A";
			cpu		= "N/A";
			mem		= "N/A";
			mobo	= "N/A";
			vga		= "N/A";
			sound	= "N/A";

			os = Environment.OSVersion.ToString();

			ConnectionOptions conOptions = new ConnectionOptions();                         
			ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName  + "\\root\\cimv2", conOptions);

			query = new System.Management.ObjectQuery("SELECT * FROM Win32_Processor");
			searcher = new ManagementObjectSearcher(scope, query);
			i = 0;
			foreach(ManagementObject item in searcher.Get())
			{
				cpu = (i > 0 ? cpu + ", " : "") + item["Name"] + " @ " + Int64.Parse(item["MaxClockSpeed"].ToString()) + " MHz";
				i ++;
			}

			query = new System.Management.ObjectQuery("SELECT * FROM Win32_ComputerSystem");
			searcher = new ManagementObjectSearcher(scope, query);
			i = 0;
			foreach(ManagementObject item in searcher.Get())
			{
				mobo = (i > 0 ? mobo + ", " : "") + item["model"] + " (" + item["Manufacturer"] + ")";
				mem = (i > 0 ? mem + ", " : "") + (uint)System.Math.Ceiling(Double.Parse(item["totalphysicalmemory"].ToString())/1048576.0) + " MB";
				i ++;
			}

			query = new System.Management.ObjectQuery("SELECT * FROM Win32_VideoController");
			searcher = new ManagementObjectSearcher(scope, query);
			i = 0;
			foreach(ManagementObject item in searcher.Get())
			{
				vga = (i > 0 ? vga + ", " : "") + item["Caption"];
				i ++;
			}

			query = new System.Management.ObjectQuery("SELECT * FROM Win32_SoundDevice");
			searcher = new ManagementObjectSearcher(scope, query);
			i = 0;
			foreach(ManagementObject item in searcher.Get())
			{
				sound = (i > 0 ? sound + ", " : "") + item["Caption"];
				i ++;
			}
		}

		private string os, cpu, mem, mobo, vga, sound;
	}
}
