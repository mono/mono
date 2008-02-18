// CS1914: Static field or property `Data.Count' cannot be assigned in an object initializer
// Line: 17


using System;
using System.Collections.Generic;

class Data
{
	public static int Count;
}

public class Test
{
	static void Main ()
	{
		var c = new Data { Count = 10 };
	}
}
