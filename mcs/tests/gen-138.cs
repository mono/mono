using System;

class X
{
	static void Main ()
	{
		int? a = 4;
		long b = 5;
		long? c = a * b;
		Console.WriteLine (c);
	}
}
