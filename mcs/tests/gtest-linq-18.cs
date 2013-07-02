using System;
using System.Linq;

// LINQ and lambdas mix tests

public class C
{
	static bool Test (Func<int, bool> f)
	{
		return false;
	}
	
	static bool Test2 (Func<int, int> f)
	{
		return false;
	}

	public static int Main ()
	{
		var x = new int [] { 'a', 'b', 'c' };
		
 		var e = from ck in x
			let xy = Test(c => c == ck)
			where ck == 'v'
			select Test(c => c == ck);

 		var e2 = from ck in x
			where Test(c => c == ck)
			select Test(c => c == ck);
	
		int[] int_array = new int [] { 0, 1, 2, 3, 4 };
		var e3 = from int i in int_array group Test2 (gg => i + 2) by Test2 (g => i % 2);

		var e4 = from i in x
			let l = i + 4
			let g = l - 2
			where Test(c => c == l)
			where l > 0
			select l;
			
		var e5 = from a in x
			join b in x on Test (a2 => a2 == a) equals Test (b2 => b2 == b)
			select a;
			
		var e6 = from a in x
			join b in x on Test (a2 => a2 == a) equals Test (b2 => b2 == b) into re6
			select a;
	
		return 0;
	}
}
