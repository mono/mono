// CS1954:  The best overloaded collection initalizer method `Data.Add(ref int)' cannot have `ref' or `out' modifier
// Line: 20

using System;
using System.Collections;

class Data : IEnumerable
{
	public IEnumerator GetEnumerator () { return null; }
	
	public void Add (ref int b)
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
