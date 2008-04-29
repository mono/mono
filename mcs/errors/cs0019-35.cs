// CS0019:  Operator `==' cannot be applied to operands of type `object' and `int'
// Line: 11

using System;

class Test {

	static void Main ()
	{
		object o = 2;
		if (o == 42)
			Console.WriteLine (o);
	}
}
