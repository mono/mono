// CS1061: Type `A' does not contain a definition for `Foo' and no extension method `Foo' of type `A' could be found (are you missing a using directive or an assembly reference?)
// Line: 17

using System;
using System.Runtime.CompilerServices;

class A
{
	[IndexerName ("Foo")]
	public int this [int index] {
		get { return index; }
		set { ; }
	}

	static void Test (A a, int value)
	{
		a.Foo = value;
	}

	public static void Main ()
	{
		Test (new A (), 9);
	}
}
