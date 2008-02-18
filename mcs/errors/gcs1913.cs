// CS1913: Member `Data.Count' cannot be initialized. An object initializer may only be used for fields, or properties
// Line: 17


using System;
using System.Collections.Generic;

class Data
{
	public delegate int Count ();
}

public class Test
{
	delegate void S ();
	
	static void Main ()
	{
		//S s = new S ();
		//string drawBackLabel = string.Length("absDrawBack");
		var c = new Data { Count = 10 };
	}
}
