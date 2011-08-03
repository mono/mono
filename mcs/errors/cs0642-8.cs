// CS0642: Possible mistaken empty statement
// Line: 13
// Compiler options: /warnaserror /warn:3
using System;
public class C
{
	public static int p = 0;
	public static void Foo ()
	{
		if (p < 5)
			Console.WriteLine ();
		else
			;
		Console.WriteLine ();
	}
}

