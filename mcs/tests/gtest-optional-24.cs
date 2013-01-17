using System;
using System.Runtime.CompilerServices;
using System.IO;

class CallerFilePathTest
{
	static void TraceStatic2([CallerMemberName][CallerFilePath] object fp = null)
	{
	}
	
	public static void TraceStatic(string expected, [CallerFilePath] string filepath = "--")
	{
		filepath = Path.GetFileName (filepath);

		Console.WriteLine (filepath);
		
		if (expected != filepath)
			throw new ApplicationException (string.Format ("`{0}' !=  `{1}'", expected, filepath));
	}
	
	public static void Main ()
	{
		TraceStatic ("gtest-optional-24.cs");
		TraceStatic2 ();
	}
}