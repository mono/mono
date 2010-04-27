// CS1931: A range variable `ii' conflicts with a previous declaration of `ii'
// Line: 14


using System;
using System.Collections.Generic;
using System.Linq;

class Test
{
	public static void Main ()
	{
		int[] int_array = null;
		int ii = 0;
		IEnumerable<int> e = from int ii in int_array select "1";
	}
}
