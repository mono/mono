using System;
using System.Linq;
using System.Collections.Generic;

class C
{
	public static void Main ()
	{
		Test_1 (5);
		Test_2 ();
	}
	
	static void Test_1 (int x)
	{
		Func<IEnumerable<int>> v = () =>
			from a in new int[] { 5, 10 }
			let b = a
			select b + x;
	}
	
	static void Test_2 ()
	{
		Func<int, Func<IEnumerable<int>>> vv = (x) =>
			() =>
				from a in new int[] { 5, 10 }
				let b = a
				select b + x;
	}
}