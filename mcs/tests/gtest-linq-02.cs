

using System;
using System.Collections.Generic;
using System.Linq;

class Test
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1 };
		
		IEnumerable<int> e;
		int pos;

		// Explicitly typed
		e = from int i in int_array select i;
		pos = 0;
		foreach (int actual in e) {
			Console.WriteLine (actual);
			if (int_array [pos++] != actual)
				return pos;
		}
		
		e = from int i in int_array select 19;
		pos = 0;
		foreach (int actual in e) {
			Console.WriteLine (actual);
			if (actual != 19)
				return actual;
		}

		// Implicitly typed
		e = from i in int_array select i;
		pos = 0;
		foreach (int actual in e) {
			Console.WriteLine (actual);
			if (int_array [pos++] != actual)
				return pos;
		}
		
		e = from i in int_array select 19;
		pos = 0;
		foreach (int actual in e) {
			Console.WriteLine (actual);
			if (actual != 19)
				return actual;
		}
		
		Console.WriteLine ("OK");
		return 0;
	}
}
