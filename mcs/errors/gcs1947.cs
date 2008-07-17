// CS1947: A range variable `v' cannot be assigned to. Consider using `let' clause to store the value
// Line: 15

using System;
using System.Linq;

class Test
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1 };
		
		var e = from int i in int_array
			let v = true
			where v = false
			select v;
	}
}
