using System;
using System.Collections.Generic;
// using System.Collections.Generic;
using System.IO;
// using System.Linq;
// using System.Reflection;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using NUnitLite.Runner;

public class Driver
{
	static TextUI Runner;

	public static void RunTests ()
	{
		string exclude = "NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,MobileNotWorking,AndroidNotWorking";
		if (IntPtr.Size == 4)
			exclude += ",LargeFileSupport";

		string assembly = null;
		if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/testassembly.txt"))
			assembly = File.ReadAllText ($"{AppDomain.CurrentDomain.BaseDirectory}/testassembly.txt");

		if (assembly != null) {
			assembly = $"{AppDomain.CurrentDomain.BaseDirectory}/{assembly}";
			Console.WriteLine ($"Testing single assembly \"{assembly}\"");
		} else {
			Console.WriteLine($"Looking for assemblies in ${AppDomain.CurrentDomain.BaseDirectory}");

			assembly = "";
			foreach (var file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "monodroid_*_test.dll", SearchOption.TopDirectoryOnly))
				assembly += $"{AppDomain.CurrentDomain.BaseDirectory}/{file} ";

			Console.WriteLine ($"Testing multiple assemblies \"{assembly}\"");
		}

		string[] args = new string [] {
			$"-labels",
			$"-exclude={exclude}",
			$"{assembly}",
		};

		Runner = new TextUI ();
		Runner.Execute (args);
	}

	public static void Main ()
	{
		Environment.Exit (1);
	}
}
