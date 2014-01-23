// Compiler options: -langversion:default

using System;

namespace Test1
{
	public partial class Foo
	{
	   internal static System.Collections.IEnumerable E ()
	   {
		   yield return "a";
	   }
	}
}

class X
{
	public static int Main ()
	{
		foreach (string s in Test1.Foo.E())
		{
			Console.WriteLine (s);
			if (s != "a")
				return 1;
			
			return 0;
		}
		return 2;
	}
}
