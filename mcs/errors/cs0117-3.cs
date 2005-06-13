// cs0117-3.cs: `A' does not contain a definition for `Foo'
// Line: 16
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
