

using System;
using System.Collections.Generic;
using System.Linq;

class IntoTest
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1 };
		
		var e = from i in int_array 
			where i > 0
			select i
				into x
				select x + 99;
		
		var l = e.ToList ();
		
		if (l.Count != 1)
			return 1;

		if (l [0] != 100)
			return 2;
			
		e = from int i in int_array 
			select i + 3
				into x
				where x == 3
				select x + 5;
		
		l = e.ToList ();
		
		if (l.Count != 1)
			return 1;

		if (l [0] != 8)
			return 2;			
				
		Console.WriteLine ("OK");
		return 0;
	}
}
