// CS1913: Member `Data.Count()' cannot be initialized. An object initializer may only be used for fields, or properties
// Line: 17


using System;
using System.Collections.Generic;

class Data
{
	public int Count ()
	{
		return 1;
	}
}

public class Test
{
	static void Main ()
	{
		var c = new Data { Count = 10 };
	}
}
