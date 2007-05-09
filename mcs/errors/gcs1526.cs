// CS1526: A new expression requires () or [] after type
// Line: 14
using System;

public class Test
{
	private class A
	{
		public string B;
	}
	
	static void Main ()
	{
		A a = new A { B = "foo" };
	}
}
