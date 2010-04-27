using System;
using System.Collections.Generic;

interface I<T> : ICollection<T>, IEnumerable<T>
{
}

class C
{
	void Foo ()
	{
		I<object> o = null;
		foreach (var v in o)
			Console.WriteLine (v);
	}
	
	public static void Main ()
	{
		IList<int> list = new List<int> { 1, 3 };
		var g = list.GetEnumerator ();
	}
}
