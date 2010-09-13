using System;
using System.Collections.Generic;

// Dynamic statements

class Disposable : IDisposable
{
	public static int Counter;

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

		return res == 31;
	}
	
	bool ForEachTest_2()
	{
		dynamic c = new int [] { 5, 7 };
		int total = 0;
		foreach (var v in c)
		{
			total += v;
		}
		
		return total == 12;
	}
	
	bool ForEachTest_3()
	{
		dynamic[] c = new dynamic [] { (byte) 1, 7 };
		int total = 0;
		foreach (var v in c)
		{
			total += v;
		}

		Console.WriteLine (total);
		return total == 8;
	}

	bool UsingTest ()
	{
		dynamic d = new Disposable ();
		try {
			using (d) {
				d.VV ();
			}
		} catch { }

		if (Disposable.Counter != 1)
			return false;

		try {
			using (dynamic u = new Disposable ()) {
				u.VV ();
			}
		} catch { }

		if (Disposable.Counter != 2)
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
		
		if (!t.ForEachTest_3 ())
			return 3;
		
		if (!t.UsingTest ())
			return 10;

		Console.WriteLine ("ok");
		return 0;
	}
}
