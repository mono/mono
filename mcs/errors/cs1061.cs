// CS1061: Type `A' does not contain a definition for `Foo' and no extension method `Foo' of type `A' could be found (are you missing a using directive or an assembly reference?)
// Line: 16

using System;
using System.Runtime.CompilerServices;

class A
{
	[IndexerName ("Foo")]
	public int this [int index] {
		get { return index; }
	}

	static int Test (A a)
	{
		return a.Foo;
	}

	public static void Main ()
	{
		Test (new A ());
	}
}
