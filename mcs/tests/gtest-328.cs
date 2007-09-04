using System;
using System.Collections.Generic;

public class App
{
	class MyClass
	{ }
  
	public static void Main ()
	{
		MyClass mc = new MyClass ();
		List<string> l = new List<string> ();
		TestMethod ("Some format {0}", l, mc);
	}

	static void TestMethod (string format, List<string> l, params MyClass[] parms)
	{
		Console.WriteLine (String.Format (format, parms));
	}
}
