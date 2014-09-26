using System;
using System.Runtime.InteropServices;

namespace Eresys
{
	/// <summary>
	/// Summary description for Timer.
	/// </summary>
	public class Timer
	{
		public double Time
		{
			get { return time; }
		}

		public double Interval
		{
			get { return interval; }
		}

		public bool Pause
		{
			get
			{
				return paused;
			}
			set
			{
				if(value != paused)
				{
					paused = value;
					current = GetTime();
					if(paused)
					{
						interval = current - start;
						time += interval;
					}
					else
					{
						start = current;
					}
				}
			}
		}

		public Timer()
		{
			time = 0.0;
			interval = 0.0;
			paused = true;
			InitHpc();
		}

		public void Update() 
		{
			if(paused) return;
			current = GetTime();
			interval = current - start;
			time += interval;
			start = current;
		}

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("winmm.dll")]
		private static extern ulong timeGetTime();

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out ulong counter);

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out ulong frequency);

		private double GetTime()
		{
			double res;
			if(hpc)
			{
				ulong counter;
				QueryPerformanceCounter(out counter);
				res = (double)counter;
			}
			else
			{
				res = (double)timeGetTime();
			}
			return res/frequency;
		}

		private void InitHpc()
		{
			ulong freq;

			hpc = QueryPerformanceFrequency(out freq);

			if(hpc)
			{
				frequency = (double)freq;
			}
			else
			{
				frequency = 1000.0;
			}
		}

		private double frequency, time, interval, start, current;
		private bool hpc, paused;
	}
}
