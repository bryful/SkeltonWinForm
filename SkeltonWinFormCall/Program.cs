
using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using static System.Net.Mime.MediaTypeNames;

//using System.Runtime.InteropServices;
//using System.Net.Http.Headers;

using BRY;

namespace SkeltonWinForm // Note: actual namespace depends on the project name.
{



	internal class Program
	{
		public static string CallExeName = "SkeltonWinForm";
		public static string MyExeName = "SkeltonWinFormCall";
		// ************************************************************************
		static int Main(string[] args)
		{
			CallExe ce = new CallExe(CallExeName,MyExeName);
			ce.Run(args);

			Console.WriteLine(ce.ResultString);
			return ce.Result;
		}
	}
	// ************************************************************************
}
