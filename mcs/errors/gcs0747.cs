// CS0747: Inconsistent `collection initializer' member declaration
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
		var c = new ArrayList { 1, Count = 1 };
	}
}
