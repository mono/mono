
// Tests variable type inference with the var keyword when assigning to build-in types
using System;

public class Test
{
	public static int Main ()
	{
		var i = 5;
		var b = true;
		var s = "foobar";
		
		if (!b)
			return 1;
		if (i > 5)
			return 2;
		if (s != "foobar")
			return 3;
		
		return 0;
	}
}
