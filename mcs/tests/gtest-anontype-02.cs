// Compiler options: -langversion:linq
// Tests anonymous types initialized with local variables
using System;
using System.Collections;

public class Test
{
	static int Main ()
	{
		string Foo = "Bar";
		int Baz = 42;
		var v = new { Foo, Baz };
		
		if (v.Foo != "Bar")
			return 1;
		if (v.Baz != 42)
			return 2;
		
		return 0;
	}
}
