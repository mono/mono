// Compiler options: -langversion:linq
// Tests collection initialization
using System;
using System.Collections;
using System.Collections.Generic;

public class Test
{
	static int Main ()
	{
		ArrayList collection = new ArrayList { "Foo", "Bar", "Baz" };
		foreach (string s in collection)
			if (s.Length != 3)
				return 1;
		
		List<string> generic_collection = new List<string> { "Hello", "World" };
		foreach (string s in generic_collection)
			if (s.Length != 5)
				return 2;
		
		return 0;
	}
}
