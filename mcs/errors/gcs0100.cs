// CS0100: The parameter name `a' is a duplicate
// Line: 10

using System;

class C
{
	static void Main ()
	{
		Func<int, int, int> l = (a, a) => 1;
	}
}
