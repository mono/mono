

using System;
using System.Collections.Generic;
using System.Linq;

class SelectMany
{
	public static int Main ()
	{
		int[] a1 = new int [] { 0, 1 };
		string[] a2 = new string [] { "10", "11" };

		int id = 0;

		// Explicitly typed
		var e = from int i1 in a1
			from string i2 in a2
			select new { i1, i2 };
		
		foreach (var item in e) {
			Console.WriteLine (item);
			
			if (item.i1 != id / 2)
				return 1;
			
			if (id % 2 == 0)
				if (item.i2 != "10")
					return 2;
				
			++id;
		}
		
		var e2 = from int i1 in a1
			where i1 > 0
			from string i2 in a2
			from int i3 in a1
			orderby i3
			select new { pp = 9, i1, i3 };
		
		id = 0;
		foreach (var item in e2) {
			Console.WriteLine (item);
			
			if (item.i1 != 1)
				return 3;
			
			if (id / 2 != item.i3)
				return 4;
			++id;
		}
		
		// Implicitly typed
		var e3 = from i1 in a1
			from i2 in a2
			select new { i1, i2 };
		
		id = 0;
		foreach (var item in e3) {
			Console.WriteLine (item);
			
			if (item.i1 != id / 2)
				return 1;
			
			if (id % 2 == 0)
				if (item.i2 != "10")
					return 2;
				
			++id;
		}

		Console.WriteLine ("OK");
		return 0;
	}
}

