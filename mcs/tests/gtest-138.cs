using System;

class X
{
	public static void Main ()
	{
		int? a = 4;
		long b = 5;
		long? c = a * b;
		Console.WriteLine (c);
	}
}
