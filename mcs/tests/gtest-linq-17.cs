using System;
using System.Collections.Generic;
using System.Linq;

//
// Tests whether group i by i is optimized
//
class TestGroupBy
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1, 2, 3, 4 };
		
		var e = from i in int_array group i by i;

		int c = 0;
		foreach (var ig in e) {
			Console.WriteLine (ig.Key);
			if (ig.Key != c++)
				return c;
		}
		
		Console.WriteLine ("OK");
		return 0;
	}
}
