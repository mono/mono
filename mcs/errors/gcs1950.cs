// CS1954: The best overloaded collection initalizer method `Data.Add(__arglist)' has some invalid arguments
// Line: 16


using System;
using System.Collections;

class Data : IEnumerable
{
	public IEnumerator GetEnumerator () { return null; }
	
	public void Add (__arglist)
	{
	}
}

public class Test
{
	static void Main ()
	{
		var c = new Data { 1 };
	}
}
