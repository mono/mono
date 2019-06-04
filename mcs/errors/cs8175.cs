// CS8175: Cannot use by-reference variable `v' inside an anonymous method, lambda expression, or query expression
// Line: 14

using System;

public class Test
{
	public static void Main()
	{
		var arr = new int [1];
		ref var v = ref arr [0];

		Action a = delegate {
			ref var v2 = ref v;
		};
	}
}