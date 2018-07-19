// CS0121: The call is ambiguous between the following methods or properties: `Program.Foo(System.Func<string,dynamic>)' and `Program.Foo(System.Func<object>)'
// Line: 10

using System;

public static class Program
{
	public static void Main ()
	{
		Foo (Bar);
	}

	public static dynamic Bar (string s1)
	{
		return 1;
	}
	
	public static object Bar () {
		return  2;
	}

	public static void Foo (Func<string, dynamic> input)
	{
	}

	public static void Foo (Func<object> input)
	{
	}
}