// CS1912: An object initializer includes more than one member `Count' initialization
// Line: 17


using System;
using System.Collections.Generic;

public class Test
{
	class Container
	{
		public int Count;
	}
	
	static void Main ()
	{
		var c = new Container { Count = 1, Count = 10 };
	}
}
