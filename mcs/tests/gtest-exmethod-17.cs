// Compiler options: -r:gtest-exmethod-17-lib.dll

using System;
using Testy;

public static class MainClass
{
	public static void Main ()
	{
		Object o = new Object ();
		Console.WriteLine (o.MyFormat ("hello:{0}:{1}:", "there", "yak"));
	}
}

