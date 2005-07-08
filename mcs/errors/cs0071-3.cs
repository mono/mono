// cs0071-3.cs: An explicit interface implementation of an event must use property syntax
// Line: 12

using System;

interface IBlah
{
	event Delegate Foo;
}

class Test : IBlah {
	event MyEvent ITest.Foo;

	public static void Main ()
	{
	}
}
