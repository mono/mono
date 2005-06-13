// cs0117-2.cs: `A' does not contain a definition for `Foo'
// Line: 15
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
