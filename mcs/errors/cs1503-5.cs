// CS1503: Argument `#5' cannot convert `void' expression to type `object'
// Line: 14

using System;

public class foo
{
	public static void voidfunc()
	{
	}

	public static void Main()
	{
		Console.WriteLine ("Whoops: {0} {1}", 0, 1, 2, voidfunc());
	}
}
