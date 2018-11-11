using System;
using SciterSharp;

namespace IconDrop.Hosting
{
	public class WindowUnittest : SciterWindow
	{
		public WindowUnittest()
		{
			var wnd = this;
			wnd.CreateMainWindow(800, 600);
			wnd.CenterTopLevelWindow();
			wnd.Title = Consts.AppName;
		}
	}
}