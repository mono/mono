// CS0071: `Test.IBlah.Foo': An explicit interface implementation of an event must use property syntax
// Line: 14

using System;

delegate void Delegate ();

interface IBlah
{
	event Delegate Foo;
}

class Test : IBlah {
	event Delegate IBlah.Foo;

	public static void Main ()
	{
	}
}
