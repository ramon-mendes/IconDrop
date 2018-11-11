#if WINDOWS || GTKMONO
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;
using SciterSharp.Interop;
using IconDrop.Hosting;
using IconDrop.Native;

namespace IconDrop
{
	class Program
	{
		public static Window AppWindow { get; private set; }// must keep a reference to survive GC
		public static Host AppHost { get; private set; }

		[STAThread]
		static void Main(string[] args)
		{
#if WINDOWS
			// Sciter needs this for drag'n'drop support; STAThread is required for OleInitialize succeess
			int oleres = PInvokeWindows.OleInitialize(IntPtr.Zero);
			Debug.Assert(oleres == 0);
#endif
#if GTKMONO
			PInvokeGTK.gtk_init(IntPtr.Zero, IntPtr.Zero);
			Mono.Setup();
#endif

#if false
//#if !DEBUG && WINDOWS
			if(SingleInstance.IsRunningAndAcquire())
				return;
#endif

			App.Run();

#if false
//#if !DEBUG && WINDOWS
			SingleInstance.Release();
#endif
		}
	}
}
#endif