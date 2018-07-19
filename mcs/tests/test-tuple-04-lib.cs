// Compiler options: -t:library

using System;
using System.Collections.Generic;

public class X
{
	public static (int a, string, bool b, object) Test1 ()
	{
		return ValueTuple.Create (1, "2", true, new X ());
	}

	public static (int x, (int x2, string y2), bool z) Field;
}
