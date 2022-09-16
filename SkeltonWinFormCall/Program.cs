// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

using System.Runtime.InteropServices;
using System.Net.Http.Headers;

#pragma warning disable CS8600
namespace SkeltonWinForm // Note: actual namespace depends on the project name.
{
	public enum StartupOption
	{
		None=0,
		IsRunning,
		Call
	}

	internal class Program
	{
		public static string CallExeName = "SkeltonWinForm";
		// ************************************************************************
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		// ************************************************************************
		static string CallExePath(string nm)
		{
			string ret = "";
			string fullName = Environment.ProcessPath;
			string n = "";
			if (fullName != null)
			{
				n = Path.GetDirectoryName(fullName);

			}
			if ((n != null)&&(n != ""))
			{
				ret = Path.Combine(n, nm + ".exe");
			}
			else
			{
				ret = nm + ".exe";
			}
			return ret;
		}
		// ************************************************************************
		static StartupOption GetOption(string[] args)
		{
			StartupOption ret = StartupOption.None;
			if(args.Length>0)
			{
				for(int i=0; i< args.Length;i++)
				{
					if ((args[i][0] =='-')|| (args[i][0] == '/'))
					{
						string p = args[i].Substring(1).ToLower();
						switch (p)
						{
							case "isrunning":
							case "exenow":
							case "execnow":
								ret = StartupOption.IsRunning;
								break;
							case "call":
							case "execute":
							case "start":
								ret = StartupOption.Call;
								break;
						}

					}
				}
			}
			return ret;
		}
		// ************************************************************************
		static string ArgsString(string[] args)
		{
			string opts = "";
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (opts != "") opts += " ";
					opts += "\"" + args[i] + "\"";
				}
			}
			return opts;
		}
		// ************************************************************************
		static int Main(string[] args)
		{
			int Result = 0;
			string res = "";

			StartupOption so  = GetOption(args);
			if (so != StartupOption.None)
			{
				Process proc = null;
				Process[] ps = Process.GetProcessesByName(CallExeName);
				bool isRunnig = (ps.Length > 0);
				if (isRunnig) proc = ps[0];
				switch (so)
				{
					case StartupOption.IsRunning:
						if (isRunnig)
						{
							res = "true";
							Result = 1;
						}
						else
						{
							res = "false";
							Result = 0;
						}
						break;
					case StartupOption.Call:
						if (isRunnig)
						{
							if (proc != null)
								SetForegroundWindow(proc.MainWindowHandle);
						}
						else
						{
							Process exec = new Process();
							exec.StartInfo.FileName = CallExePath(CallExeName);
							exec.StartInfo.Arguments = "";// ArgsString(args);
							if (exec.Start())
							{
								Result = 1;
								res = "true";
							}
							else
							{
								Result = 0;
								res = "false";
							}
						}
						break;
				}
			}
			else
			{
				string exename = CallExePath(CallExeName);
				Result = 0;
				res = "false";
				if (File.Exists(exename) == true)
				{
					Process exec2 = new Process();
					exec2.StartInfo.FileName = exename;
					exec2.StartInfo.Arguments = ArgsString(args);
					try
					{
						if (exec2.Start())
						{
							Result = 1;
							res = "true";
						}

					}
					catch
					{
						//
					}
				}
			}
			Console.WriteLine(res);
			return Result;
		}
	}
	// ************************************************************************
}
#pragma warning restore CS8600
