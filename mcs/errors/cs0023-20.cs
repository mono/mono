// CS0023: The `?' operator cannot be applied to operand of type `int'
// Line: 11

using System;

class C
{
	static void Main()
	{
		int k = 0;
		var r = k?.ToString ();
	}
}