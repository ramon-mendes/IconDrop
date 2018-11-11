using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SciterSharp;
using SciterSharp.Interop;
using IconDrop.Data;
using IconDrop.Hosting;
using IconDrop.Svg;

namespace IconDrop
{
	class SciterMessages : SciterDebugOutputHandler
	{
		protected override void OnOutput(SciterXDef.OUTPUT_SUBSYTEM subsystem, SciterXDef.OUTPUT_SEVERITY severity, string text)
		{
			Debug.Write(text);// so I can see Debug output even if 'native debugging' is off
		}
	}

	static class App
	{
		private static SciterMessages sm = new SciterMessages();
		public static SciterWindow AppWnd { get; private set; }
		public static Host AppHost { get; private set; }

		public static void Run()
		{
            Debug.WriteLine("Sciter: " + SciterX.Version);

			Joiner.Setup();

			double tooktime = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;
			Debug.WriteLine($"{tooktime}ms to start");

			if(true)
				CreateApp();
			else
				CreateUnittest();

			//AppHost.DebugInspect();

#if !OSX
			PInvokeUtils.RunMsgLoop();
#endif
		}

		public static void CreateApp()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

#if WINDOWS
			AppWnd = new Window();
			AppHost = new Host(AppWnd);

			AppHost.SetupPage("index.html");
			AppWnd.Show();
#else
			AppWnd = new WindowSidebar();
			AppHost = new Host(AppWnd);

			AppHost.SetupPage("index.html");
			AppWnd.Show(false);

			WindowSidebar.ShowPopup();
#endif
		}

		public static void CreateUnittest()
		{
			AppWnd = new WindowUnittest();
			AppHost = new Host(AppWnd);

			AppHost.SetupPage("unittest.html");
			AppWnd.Show();
		}

		public static void Exit()
		{
#if WINDOWS
			Window.Dispose();
			AppWnd.Destroy();
			Environment.Exit(0);
			//PInvoke.User32.PostQuitMessage(0);
#else
			AppKit.NSApplication.SharedApplication.Terminate(null);
#endif
		}
	}
}