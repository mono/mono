using System;
using System.Collections.Generic;

class D : IDisposable
{
	public void Dispose ()
	{
		Console.WriteLine ("dispose");
	}
}

class C
{
	IEnumerable<int> Test ()
	{
		try {
			using (var d = new D ()) {
				Console.WriteLine (1);
			}
		} finally {
			Console.WriteLine (2);
		}

		yield break;
	}

	public static int Main ()
	{
		var c = new C ();
		foreach (var a in c.Test ()) {
			Console.WriteLine (a);
		}

		return 0;
	}
}