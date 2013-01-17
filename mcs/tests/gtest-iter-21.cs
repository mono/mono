using System;
using System.Collections.Generic;

class C
{
	IEnumerable<int> Test ()
	{
		Console.WriteLine ("init");
		try {
			yield return 1;
		} finally {
			int oo = 4;
			Action a = () => Console.WriteLine (oo);
		}
		
		yield return 2;
	}

	public static int Main ()
	{
		var c = new C ();
		foreach (var a in c.Test ())
		{
		}
		
		return 0;
	}
}