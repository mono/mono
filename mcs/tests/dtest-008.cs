using System;
using System.Collections.Generic;

// Dynamic statements

class Disposable : IDisposable
{
	public int Counter;

	public void Dispose ()
	{
		++Counter;
	}

	public void Test ()
	{
	}
}

public class Test
{
	bool ForEachTest ()
	{
		dynamic d = new List<int> { 5, 10, 7 };
		dynamic res = 9;
		foreach (var v in d) {
			res += v;
		}

		Console.WriteLine (res);
		return res == 31;
	}
	
	bool ForEachTest_2()
	{
		dynamic c = new int [2] { 5, 7 };
		int total = 0;
		foreach (var v in c)
		{
			total += v;
		}
		
		return total == 12;
	}

	bool UsingTest ()
	{
		dynamic d = new Disposable ();
		try {
			using (d) {
				d.VV ();
			}
		} catch { }

		if (d.Counter != 1)
			return false;

		try {
			using (dynamic u = new Disposable ()) {
				u.VV ();
			}
		} catch { }

		if (d.Counter != 1)
			return false;

		return true;
	}

	public static int Main ()
	{
		var t = new Test ();
		if (!t.ForEachTest ())
			return 1;

		if (!t.ForEachTest_2 ())
			return 2;
		
		if (!t.UsingTest ())
			return 3;

		Console.WriteLine ("ok");
		return 0;
	}
}
