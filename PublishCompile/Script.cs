using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

partial class Script
{
	const string APPNAME = "IconDrop";
	const string APPNAME_EXE = APPNAME + ".exe";
	const string CONFIG = "Release";

	static void Main(string[] args)
	{
		if(Environment.OSVersion.Platform == PlatformID.Win32NT)
			CWD = Path.GetFullPath(Environment.CurrentDirectory + "/../../../");
		else
			CWD = $"/Users/midiway/Documents/{APPNAME}/";

		Environment.CurrentDirectory = CWD;

		string exe_test;
		if(true)
		{
			GitPush();
			exe_test = BuildAndDeploy();

			// Run with -test
			Console.WriteLine("### RUN + TESTS (WAITS FOR EXIT) ###");
			SpawnProcess(exe_test, "-test");
		}
		else
		{
			_upload_output = CWD + "ReleaseInfo/Latest/IconDrop.zip";
		}

        // Move to DB
        string dbfile;
		if(Environment.OSVersion.Platform == PlatformID.Unix)
			dbfile = "/Users/midiway/Dropbox/Apps/" + Path.GetFileName(_upload_output);
        else
			dbfile = "D:\\Dropbox\\Apps\\" + Path.GetFileName(_upload_output);

		File.Delete(dbfile);
		File.Move(_upload_output, dbfile);
	}

	static string BuildAndDeploy()
	{
		Console.WriteLine("### BUILD ###");
		if(Environment.OSVersion.Platform == PlatformID.Unix)
		{
			SpawnProcess("sh", CWD + $"{APPNAME}/scripts/preBuildOSX.sh");
			SpawnProcess("msbuild", CWD + $"{APPNAME}/{APPNAME}OSX.csproj /t:Build /p:Configuration=Release");

			string APP_DIR = CWD + $"{APPNAME}/bin/Release/IconDrop.app";
			string APP_RI = CWD + $"ReleaseInfo/";
			string APP_OUTPUTDIR = APP_RI + $"Output/";
			string APP_LATEST = APP_OUTPUTDIR + $"{APPNAME}.app/";

			if(Directory.Exists(APP_LATEST))
				Directory.Delete(APP_LATEST, true);
			Directory.CreateDirectory(APP_OUTPUTDIR);
			Directory.Move(APP_DIR, APP_LATEST);

			_upload_output = APP_RI + "IconDropOSX.zip";
			if(File.Exists(_upload_output))
				File.Delete(_upload_output);
			ZipFile.CreateFromDirectory(APP_OUTPUTDIR, _upload_output);

			return APP_LATEST + "Contents/MacOS/IconDrop";
		}
		else
		{
			string how = "Clean,Build";
			SpawnProcess(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe",
					CWD + $"..\\{APPNAME}\\{APPNAME}Windows.csproj /t:{how} /p:Configuration={CONFIG} /p:Platform=x64");

			#region Pack
			var WORK_DIR = $"{CWD}Latest\\";

			// Delete and create these dirs
			if(Directory.Exists(WORK_DIR))
				Directory.Delete(WORK_DIR, true);
			Directory.CreateDirectory(WORK_DIR);

			// Copy \bin\Release to WORK_DIR
			string BIN_DIR = Path.GetFullPath(CWD + "..\\" + APPNAME + "\\bin\\Release");

			var files1 = Directory.EnumerateFiles(BIN_DIR, "*.exe", SearchOption.AllDirectories);
			var files2 = Directory.EnumerateFiles(BIN_DIR, "*.dll", SearchOption.AllDirectories);
			foreach(var file in files1.Union(files2))
			{
				string subpath = file.Substring(BIN_DIR.Length);
				string outpath = WORK_DIR + subpath;
				Directory.CreateDirectory(Path.GetDirectoryName(outpath));
				File.Copy(file, outpath);
			}

			// Copy \Shared to WORK_DIR
			string SHARED_DIR = Path.GetFullPath(CWD + "..\\" + APPNAME + "\\Shared");
			DirectoryCopy(SHARED_DIR, WORK_DIR + "\\Shared", true);

			// Generate installer
			//SpawnProcess("iscc", "installer.iss");
			#endregion

			// Zip WORK_DIR
			_upload_output = CWD + "IconDropWIN.zip";
			File.Delete(_upload_output);
			ZipFile.CreateFromDirectory(WORK_DIR, _upload_output);

			return WORK_DIR + APPNAME_EXE;
		}
	}
}