// CS0117: `Data' does not contain a definition for `Count'
// Line: 15
// Compiler options: -langversion:linq

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
