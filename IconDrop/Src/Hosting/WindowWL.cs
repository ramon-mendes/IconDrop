#if WINDOWS
using System;
using System.Drawing;
using System.Windows.Forms;
using SciterSharp;
using PInvoke;

namespace IconDrop.Hosting
{
	public class Window : SciterWindow
	{
		private static NotifyIcon _ni;

		public Window()
		{
			const float factorx = .5f;
			const float factory = .85f;

			int width = (int)(User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN) * factorx);
			int height = (int)(User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN) * factory);

			const int minx = 500;
			const int maxx = 820;
			const int miny = 800;

			if(width < minx) width = minx;
			if(width > maxx) width = maxx;
			if(height < miny) width = miny;

			var wnd = this;
			wnd.CreateMainWindow(width, height);
			wnd.CenterTopLevelWindow();
			wnd.Title = Consts.AppName;
			wnd.Icon = Properties.Resources.IconMain;

			var menu = new ContextMenu();
			menu.MenuItems.Add(new MenuItem("Quit", (e, a) => App.Exit()));

			_ni = new NotifyIcon();
			_ni.Icon = Icon.FromHandle(Properties.Resources.dropBW.GetHicon());
			_ni.Visible = true;
			_ni.ContextMenu = menu;
			_ni.Click += (s, e) =>
			{
				if((e as MouseEventArgs).Button == MouseButtons.Left)
				{
					User32.ShowWindow(App.AppWnd._hwnd, User32.WindowShowStyle.SW_RESTORE);
					User32.SetForegroundWindow(App.AppWnd._hwnd);
					wnd.Icon = Properties.Resources.IconMain;
				}
			};
		}

		public static void Dispose()
		{
			_ni.Dispose();
		}
	}
}
#endif