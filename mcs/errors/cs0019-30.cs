// CS0019: Operator `!=' cannot be applied to operands of type `Test.DelegateA' and `Test.DelegateB'
// Line: 13

using System;

public class Test
{
	public delegate int DelegateA(bool b);
	public delegate int DelegateB(bool b);

	static bool TestCompare (DelegateA a, DelegateB b)
	{
		return a != b;
	}
}
