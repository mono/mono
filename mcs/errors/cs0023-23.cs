// CS0023: The `?' operator cannot be applied to operand of type `void'
// Line: 10

using System;

class C
{
	static void Main ()
	{
		var v = Console.WriteLine ()?[0];
	}
}