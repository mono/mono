// cs0201: Only assignment, call, increment, decrement, and new object expressions can be used as a statement
// Line: 14
using System;

public class Test
{
	private class A
	{
		public string B;
	}

	static void Main ()
	{
		A a = new A () { B = "foo" };
	}
}
