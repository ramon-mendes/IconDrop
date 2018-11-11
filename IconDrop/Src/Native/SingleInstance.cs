#if WINDOWS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PInvoke;

namespace IconDrop.Native
{
	static class SingleInstance
	{
		static Mutex mutex = new Mutex(true, "MISoftwareID");// este nome está harcoded no Setup Inno: previne instalar/desinstalar caso app esteja rodando

		static public bool IsRunningAndAcquire()
		{
			if(!mutex.WaitOne(TimeSpan.Zero, true))
			{
				// another instance is already running
				var p = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

				foreach(var t in p.Where(t => t.MainWindowHandle != IntPtr.Zero))
				{
					OpenIcon(t.MainWindowHandle);
					User32.SetForegroundWindow(t.MainWindowHandle);
					return true;
				}
				return true;
			}
			return false;
		}


		static public void Release()
		{
			mutex.ReleaseMutex();
		}

		[DllImport("user32.dll")]
		static extern bool OpenIcon(IntPtr hWnd);
	}
}
#endif