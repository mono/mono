// CS0201: Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement
// Line: 10

using System;

class X
{
	public static void Main ()
	{
		new Func<int> (() => 0);
	}
}