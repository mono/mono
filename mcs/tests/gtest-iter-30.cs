using System;
using System.Collections.Generic;

public class Program
{
	public static void Main ()
	{
		foreach (var x in new M ().Test ()) {
			Console.WriteLine (x);
		}
	}
}

class M
{
	public IEnumerable<int> Test ()
	{
		Action a = delegate {
			int k = 0;
			Action x = delegate {
				Console.WriteLine (this);
				Console.WriteLine (k);
			};

			x ();
			Console.WriteLine (this);
		};

		a ();
		
		yield return 1;
	}
}
