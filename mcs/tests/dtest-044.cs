using System;
using System.Collections.Generic;

class C
{
	public static int Test<T, U>(T a, IComparable<U> b) where T: IComparable<U>
	{
		return 1;
	}

	public static int Test_2<T>(IList<T> a, T b)
	{
		return 2;
	}
	
	static int Main ()
	{
		dynamic d = 1;
		if (Test (1, d) != 1)
			return 1;
		
		if (Test (d, 1) != 1)
			return 2;
		
		if (Test_2 (new int [0], d) != 2)
			return 3;
		
		return 0;
	}
}
