using System;

class X
{
	static void Main ()
	{
		bool? a = true;
		int? b = a ? 3 : 4;
		Console.WriteLine (b);
	}
}
