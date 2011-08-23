// CS0019: Operator `==' cannot be applied to operands of type `void' and `null'
// Line: 11

using System;

class C
{
	public static void Main ()
	{
		Action a = () => {};
		bool b = a () == null;
	}
}
