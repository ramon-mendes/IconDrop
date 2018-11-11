using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

partial class Script
{
	public static string CWD;
	public static string _upload_output;

	static void GitPush()
	{
		SpawnProcess("git", "status");
		SpawnProcess("git", "add -A");
		SpawnProcess("git", "commit -a -mAutoPush", true);
		SpawnProcess("git", "pull");
		SpawnProcess("git", "push");
	}

	static void SpawnProcess(string exe, string args = null, bool ignore_error = false, bool wait = true)
	{
		var startInfo = new ProcessStartInfo(exe, args)
		{
			FileName = exe,
			Arguments = args,
			UseShellExecute = false,
			WorkingDirectory = CWD
		};

		var p = Process.Start(startInfo);
		if(wait)
		{
			p.WaitForExit();

			if(p.ExitCode != 0 && ignore_error == false)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				string msg = exe + ' ' + args;
				Console.WriteLine("");
				Console.WriteLine("-------------------------");
				Console.WriteLine("FAILED: " + msg);
				Console.WriteLine("EXIT CODE: " + p.ExitCode);
				Console.WriteLine("Press ENTER to exit");
				Console.WriteLine("-------------------------");

				Console.ReadLine();
				Environment.Exit(0);
			}
		}
	}

	static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);

		if(!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if(!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach(FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, false);
		}

		// If copying subdirectories, copy them and their contents to new location.
		if(copySubDirs)
		{
			foreach(DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}
}