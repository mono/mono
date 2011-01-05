// CS0272: The property or indexer `Test.A.B' cannot be used in this context because the set accessor is inaccessible
// Line: 16

using System;

public class Test
{
	private class A
	{
		public string B { get; private set; }
	}
	
	static void Main ()
	{
		A a = new A ();
		a.B = "Foo";
	}
}
