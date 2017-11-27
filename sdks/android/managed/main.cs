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
		Console.SetOut (TextWriter.Synchronized (new AndroidIntrumentationWriter (Console.Out)));
		Console.SetError (TextWriter.Synchronized (new AndroidIntrumentationWriter (Console.Error)));

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

	class AndroidIntrumentationWriter : TextWriter
	{
		readonly StringBuilder Line = new StringBuilder ();

		readonly TextWriter Inner;

		public override Encoding Encoding => Inner.Encoding;

		public AndroidIntrumentationWriter (TextWriter inner)
		{
			Inner = inner;
		}

		public override void Write(char value)
		{
			if (value == '\n')
				WriteLine ();
			else
				Line.Append (value);
		}

		public override void WriteLine ()
		{
			var l = Line.ToString ();
			Line.Clear ();

			unsafe
			{
				fixed (byte *chars = Encoding.UTF8.GetBytes (l + '\0'))
				{
					WriteLineToInstrumentation (chars);
				}
			}

			Inner.WriteLine (l);
		}

		[DllImport ("__Internal", EntryPoint = "AndroidIntrumentationWriter_WriteLineToInstrumentation", CharSet = CharSet.Unicode)]
		static unsafe extern void WriteLineToInstrumentation (byte *chars);
	}
}
