// CS0165: Use of unassigned local variable `a'
// Line: 13

using System;
using System.Diagnostics;

class C
{
	static int Main ()
	{
		int a;
		Foo (a = 9);
		return a;
	}

	[Conditional ("MISSING")]
	static void Foo (int value)
	{
	}
}