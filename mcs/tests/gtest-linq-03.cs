

using System;
using System.Collections.Generic;
using System.Linq;

class WhereTest
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1 };
		
		IEnumerable<int> e;
		
		e = from int i in int_array where i > 0 select i;
		
		if (e.ToList ()[0] != 1)
			return 1;

		e = from int i in int_array where i == 0 select i + 1;
		
		if (e.ToList ()[0] != 1)
			return 2;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
