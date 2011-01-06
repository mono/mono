// CS0019: Operator `&&' cannot be applied to operands of type `bool?' and `bool?'
// Line: 10
using System;

class X
{
	static void Main ()
	{
		bool? a = false, b = false;
		Console.WriteLine (a && b);
	}
}
