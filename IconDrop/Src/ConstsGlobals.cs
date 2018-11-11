using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconDrop
{
	static partial class Consts
	{
		public const string AppName = "IconDrop";
		//public const EProduct ProductID = EProduct.ICONDROP;

		public static readonly string DirUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/MISoftware/IconDrop/";
		public static readonly string DirUserData_Store = DirUserData + "Store/";
		public static readonly string APP_EXE = Process.GetCurrentProcess().MainModule.FileName;
		public static readonly string APP_DIR = Path.GetDirectoryName(APP_EXE) + Path.DirectorySeparatorChar;
		public static readonly string APP_DIR_SLASH = APP_DIR.Replace('\\', '/');
        public static readonly string LOG_FILE = DirUserData + "log.txt";

		//public const string SERVER_ICONS = "http://localhost:55200/";
		public const string SERVER_ICONS = "http://icondrop.azurewebsites.net/";

		static Consts()
		{
			Directory.CreateDirectory(DirUserData);
			Directory.CreateDirectory(DirUserData_Store);
		}

#if WINDOWS
		public static readonly string AppDir_Shared = Path.GetFullPath(APP_DIR + "\\Shared\\");
#elif OSX
		public static readonly string AppDir_Shared = Path.GetFullPath(APP_DIR + "../Shared/");
        public static readonly string AppDir_Tmp = Path.GetTempPath() + "tmp/";
		public static readonly string AppDir_Resources = Foundation.NSBundle.MainBundle.ResourcePath + '/';
#endif
	}

	static class Globals
	{
		// so I can use in AppTests, AppMD
		//public static BaseHost GlobalHost;
		//public static SciterWindow GlobalWnd;

        public static void Log(string msg)
        {
            Console.WriteLine(msg);
            File.WriteAllText(Consts.LOG_FILE, msg);
        }
	}
}