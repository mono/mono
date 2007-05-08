// Compiler options: -langversion:linq
// Tests implicit arrays
using System;

public class Test
{
	static int Main ()
	{
		string[] array = new [] { "Foo", "Bar", "Baz" };
		foreach (string s in array)
			if (s.Length != 3)
				return 1;
		
		return 0;
	}
}
