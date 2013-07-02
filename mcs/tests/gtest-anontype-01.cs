
// Tests anonymous types
using System;
using System.Collections;

public class Test
{
	public static int Main ()
	{
		var v = new { Foo = "Bar", Baz = 42 };
		
		if (v.Foo != "Bar")
			return 1;
		if (v.Baz != 42)
			return 2;
		
		return 0;
	}
}
