#if OSX
using System;
using System.Diagnostics;
using AppKit;
using Foundation;
using SciterSharp;
using SciterSharp.Interop;
using IconDrop.Hosting;

namespace IconDrop
{
	class Program
	{
		static void Main(string[] args)
		{
			// Default GFX in Sciter v4 is Skia, switch to CoreGraphics (seems more stable)
			SciterX.API.SciterSetOption(IntPtr.Zero, SciterXDef.SCITER_RT_OPTIONS.SCITER_SET_GFX_LAYER, new IntPtr((int) SciterXDef.GFX_LAYER.GFX_LAYER_CG));

			NSApplication.Init();

			using(var p = new NSAutoreleasePool())
			{
				var application = NSApplication.SharedApplication;
				application.Delegate = new AppDelegate();
				application.Run();
			}
		}
	}

	[Register("AppDelegate")]
	class AppDelegate : NSApplicationDelegate
	{
		static readonly SciterMessages sm = new SciterMessages();

		public override void DidFinishLaunching(NSNotification notification)
		{
			Mono.Setup();
			App.Run();
		}

		public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
		{
			WindowSidebar.ShowPopup();
			return true;
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
		{
			return false;
		}

		public override void DidResignActive(NSNotification notification)
		{
			WindowSidebar.HidePopup();
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}
#endif