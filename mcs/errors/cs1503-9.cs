// CS1503: Argument `#1' cannot convert `__arglist' expression to type `object'
// Line: 14

using System;

class Program
{
	static void Foo (object o)
	{
	}

	static void Main ()
	{
		Foo (__arglist (null));
	}
}
