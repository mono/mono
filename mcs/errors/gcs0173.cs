// CS0173: Type of conditional expression cannot be determined because there is no implicit conversion between `lambda expression' and `lambda expression'
// Line: 11

using System;

class Test
{
	public static void Main ()
	{
		bool descending = false;
		Comparison<int> comp = descending ? ((e1, e2) => e2 < e1) : ((e1, e2) => e1 < e2);
	}
}
