

using System;
using System.Collections.Generic;
using System.Linq;

class Let
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1 };
		
		IEnumerable<int> e;
		int pos;

		// Explicitly typed
		e = from int i in int_array
			let u = i * 2
			select u;
		pos = 0;
		foreach (int actual in e) {
			Console.WriteLine (actual);
			if (int_array [pos++] * 2 != actual)
				return pos;
		}		
		
		// Implicitly typed
		e = from i in int_array
			let u = i * 2
			let v = u * 3
			where u != 0
			select v;
		pos = 1;
		foreach (int actual in e) {
			Console.WriteLine (actual);
			if (int_array [pos++] * 6 != actual)
				return pos;
		}
		
		Console.WriteLine ("OK");
		return 0;
	}
}
