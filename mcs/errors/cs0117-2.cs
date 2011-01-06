// CS0117: `Data' does not contain a definition for `Count'
// Line: 15


using System;

class Data
{
}

public class Test
{
	static void Main ()
	{
		var c = new Data { Count = 10 };
	}
}
