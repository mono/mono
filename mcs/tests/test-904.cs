using System;

class Test
{
	public static bool Foo (out int v)
	{
		v = 0;
		return false;
	}

	static void Main()
	{
		bool b = false;
	
		int a1;
		var r1 = (false || Foo (out a1)) ? a1 : 1;

		int a2;
		var r2 = (true && Foo (out a2)) ? 2 : a2;

		int a3;
		var r3 = (b || Foo (out a3)) && Foo (out a3);
		int b3 = a3;

		int a4;
		var r4 = ((b || Foo (out a4)) && Foo (out a4));
		int b4 = a4;

		int a5;
		if ((b || Foo (out a5)) && (b || Foo (out a5)))
			Console.WriteLine ();
		else
			Console.WriteLine (a5);
	}
}
