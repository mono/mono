using System;
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
		string assembly = File.ReadAllText ($"{AppDomain.CurrentDomain.BaseDirectory}/testassembly.txt");

		Console.WriteLine ($"Testing assembly \"{assembly}\"");
		Console.WriteLine ($"");

		string exclude = "NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,MobileNotWorking,SatelliteAssembliesNotWorking,AndroidNotWorking";
		if (IntPtr.Size == 4)
			exclude += ",LargeFileSupport";

		string[] args = new string[] {
			$"-labels",
			$"-exclude={exclude}",
			$"{AppDomain.CurrentDomain.BaseDirectory}/{assembly}",
		};

		Runner = new TextUI ();
		Runner.Execute (args);
	}

	public static void Main ()
	{
		Environment.Exit (1);
	}
}
