// CS0747: Inconsistent `object initializer' member declaration
// Line: 16


using System;
using System.Collections;

class Data
{
}

public class Test
{
	static void Main ()
	{
		var c = new ArrayList { Count = 1, 1 };
	}
}
