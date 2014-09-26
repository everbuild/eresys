using System;
using System.Windows.Forms;

namespace Eresys
{
	/// <summary>
	/// Dit is zowat de belangerijkste klasse van de engine. Het bundelt namelijk alles. Het is via deze klasse dat de engine
	/// wordt opgestart, en dat alle componenten kunnen worden aangesproken (zoals graphics, controls, scene,...)
	/// </summary>
	public class Kernel
	{
		/// <summary>
		/// Geeft aan of de kernel gestart en geïnitilaseerd is
		/// </summary>
		public static bool		Running		{ get { return running; } }

		/// <summary>
		/// Geeft aan of de toepassing actief (= heeft focus) is
		/// </summary>
		public static bool		Active		{ get { return active; } }

		/// <summary>
		/// Het hoofd-logbestand dat door de kernel wordt gebruikt, maar ook door andere componenten mag worden gebruikt
		/// </summary>
		public static Log		Log			{ get { return log; } }

		/// <summary>
		/// Het hoofdvenster
		/// </summary>
		public static Form		Form		{ get { return form; } }

		/// <summary>
		/// De settings
		/// </summary>
		public static Settings	Settings	{ get { return settings; } }

		/// <summary>
		/// Geeft aan hoelang de applicatie in totaal al actief is, in seconden
		/// </summary>
		public static double	Time		{ get { return timer.Time; } }

		/// <summary>
		/// Geeft aan hoelang het laatst gerenderde frame geduurt heeft
		/// </summary>
		public static double	Interval	{ get { return timer.Interval; } }

		/// <summary>
		/// De Graphics component
		/// </summary>
		public static IGraphics	Graphics	{ get { return graphics; } }

		/// <summary>
		/// De Controls component
		/// </summary>
		public static IControls	Controls	{ get { return controls; } }

		/// <summary>
		/// De scene
		/// </summary>
		public static Scene		Scene		{ get { return scene; } }

		/// <summary>
		/// Het aantal frames per seconde dat op dit moment gehaald wordt
		/// </summary>
		public static float		FPS			{ get { return fps; } }

		public static long FramesDrawn { get { return frames; } }

		/// <summary>
		/// Het gemiddelde aantal frames per seconde gemeten sinds de engine werd opgestart
		/// </summary>
		public static float		AverageFPS
		{
			get
			{
				float res = 0.0f;
				if(frames > fpsMeasDelay) res = (float)((double)(frames - fpsMeasDelay)/(timer.Time - fpsOfsTime));
				return res;
			}
		}

		public static Profiler Profiler { get { return profiler; } }

		/// <summary>
		/// Start de kernel
		/// </summary>
		/// <param name="app">De applicatie, een object van een klasse die IApplication implementeerd</param>
		/// <param name="logFileName">logbestand</param>
		/// <param name="iniFileName">inibestand</param>
		/// <returns></returns>
		public static int Startup(IApplication app, string logFileName, string iniFileName)
		{
			try
			{
				// init log
				log = new Log(logFileName);
				log.WriteHeader("Eresys.Log", true, true);

				// init form
				form = new Form();
				form.Activated += new EventHandler(Activate);
				form.Deactivate += new EventHandler(Deactivate);

				// init settings
				settings = new Settings(iniFileName);

				// init kernel (=main) timer
				timer = new Timer();

				// init graphics
				switch(settings["graphics"])
				{
					case "directx":
						graphics = new DXGraphics();
						break;
					default:
						throw new Exception("Graphics renderer " + settings["graphics"] + " invalid!");
				}
				
				// init controls
				switch(settings["controls"])
				{
					case "directx":
						controls = new DXControls();
						break;
					default:
						throw new Exception("Controls manager " + settings["controls"] + " invalid!");
				}

				// init scene
				scene = new Scene();

				// init profiler
				Profiler.Enabled = Boolean.Parse(settings["profiler"]);
				profiler = new Profiler();

				// start application
				app.Startup();

				// start main loop
				log.WriteLine();
				log.WriteLine("Entering Main Loop...");
				log.WriteLine();
				running = true;
				timer.Pause = false;
				while(running)
				{
					profiler.StartSample("MAIN");
					if(active)
					{
						// render frame
						profiler.StartSample("rendering");

						profiler.StartSample("begin frame");
						graphics.BeginFrame();
						profiler.StopSample();

						profiler.StartSample("render scene");
						scene.Render();
						profiler.StopSample();

						profiler.StartSample("render app");
						app.Render();
						profiler.StopSample();

						profiler.StartSample("end frame");
						graphics.EndFrame();
						profiler.StopSample();

						profiler.StopSample();

						// frame timing
						profiler.StartSample("frame timing");
						timer.Update();
						fps = 1.0f/(float)timer.Interval;
						if(frames == fpsMeasDelay) fpsOfsTime = timer.Time;
						frames ++;
						profiler.StopSample();
					}

					// process application events
					System.Windows.Forms.Application.DoEvents();

					if(active)
					{
						// upate controls
						profiler.StartSample("update controls");
						controls.Update();
						profiler.StopSample();

						// update gamestate
						profiler.StartSample("update app");
						app.Update();
						profiler.StopSample();

						// update scene
						profiler.StartSample("update scene");
						scene.Update();
						profiler.StopSample();
					}
					profiler.StopSample();
				}
				timer.Pause = true;

				// terminate application
				app.Terminate();

				// finish some business
				graphics.Dispose();
				form.Dispose();

				// write kernel performance report
				profiler.WriteReport(new Log("kernel-profile.log"), "kernel main loop");

				// some logging...
				log.WriteLine("Eresys normally terminated.");
				log.WriteLine();
				log.WriteLine("Running time:      " + timer.Time.ToString("F4") + " seconds");
				log.WriteLine("Frames rendered:   " + frames);
				log.WriteLine("Average FPS:       " + AverageFPS.ToString("F4"));
				log.WriteLine("Last Measured FPS: " + fps.ToString("F4"));
			}
			catch(Exception e)
			{
				// als er een exceptie is opgedoken sluit deze code de engine af
				running = false;
				Cursor.Show();
				form.Hide();
				string message = "Eresys terminated due to fatal exception!\r\n\r\nException:\r\n" + e;
				log.WriteLine(message);
				MessageBox.Show(message, "Eresys.FatalException", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return e.GetHashCode(); // geef een waarde terug. Dit geeft aan dat de toepassing is beëindigt door een fout
			}

			return 0; // geef nul terug. Dit geeft aan dat de toepassing normaal is beëindigt
		}

		/// <summary>
		/// Beëindigt de engine, en dus ook de toepassing
		/// </summary>
		public static void Terminate()
		{
			running = false;
		}

		// activeert de kernel
		private static void Activate(object sender, EventArgs e)
		{
			active = true;
		}

		// zet de kernel in een soort slaapstand
		private static void Deactivate(object sender, EventArgs e)
		{
			active = false;
		}

		private static long fpsMeasDelay = 30;

		private static bool			running		= false;
		private static bool			active		= false;
		private static Log			log			= null;
		private static Form			form		= null;
		private static Settings		settings	= null;
		private static Timer		timer		= null;
		private static IGraphics	graphics	= null;
		private static IControls	controls	= null;
		private static Scene		scene		= null;
		private static float		fps			= 0.0f;
		private static long			frames		= 0;
		private static Profiler		profiler	= null;
		private static double		fpsOfsTime	= 0.0;
	}
}
