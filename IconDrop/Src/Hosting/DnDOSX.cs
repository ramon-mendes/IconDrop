#if OSX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using CoreGraphics;

namespace IconDrop.Hosting
{
	class DnDOSX : NSDraggingSource
	{
		public static void CopyPath(CGPath cgpath)
		{
			var pdfpath = Path.GetTempPath() + ".pdf";
			var context = new CGContextPDF(NSUrl.FromFilename(pdfpath));

			CGRect rc = new CGRect(0, 0, 500, 500);
			context.BeginPage(rc);

			using(var gc = NSGraphicsContext.FromGraphicsPort(context, false))
			{
				var ctx = gc.CGContext;
				//ctx.SetStrokeColor(new CGColor(0, 0, 100));
				//ctx.SetLineWidth(2);
				//ctx.SetFillColor(new CGColor(100, 0, 0));

				ctx.AddPath(cgpath);
				ctx.DrawPath(CGPathDrawingMode.FillStroke);
			}

			context.EndPage();
			context.Close();

			var data = NSData.FromFile(pdfpath);
			NSPasteboard.GeneralPasteboard.ClearContents();
			NSPasteboard.GeneralPasteboard.SetDataForType(data, NSPasteboard.NSPdfType);
		}

		private Action _completed;

		public void StartDnD(string svgpath, int xView, int yView, Action completed)
		{
			Debug.Assert(File.Exists(svgpath));

			_completed = completed;

			NSDraggingItem di;
			var url_pdf = NSUrl.FromFilename(svgpath);
			di = new NSDraggingItem(url_pdf);
			di.SetDraggingFrame(new CGRect(xView, yView, 26, 26), new NSImage(Consts.AppDir_Resources + "cursor.png"));

			NSEvent evt = NSApplication.SharedApplication.CurrentEvent;
			var session = App.AppWnd._nsview.BeginDraggingSession(new[] { di }, evt, this);
			session.AnimatesToStartingPositionsOnCancelOrFail = true;
			session.DraggingFormation = NSDraggingFormation.None;
		}

		public override void DraggedImageMovedTo(NSImage image, CGPoint screenPoint)
		{
			NSEvent evt = NSApplication.SharedApplication.CurrentEvent;
		}

		public override void DraggedImageEndedAtOperation(NSImage image, CGPoint screenPoint, NSDragOperation operation)
		{
			Debug.WriteLine("DraggedImageEndedAtOperation");
			WindowSidebar.ShowPopup();

			_completed();
		}
	}
}
#endif