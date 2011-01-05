// CS0271: The property or indexer `Test.A.B' cannot be used in this context because the get accessor is inaccessible
// Line: 17

using System;

public class Test
{
	private class A
	{
		public string B { protected get; set; }
	}
	
	static void Main ()
	{
		A a = new A ();
		a.B = "foo";
		string b = a.B;
	}
}
