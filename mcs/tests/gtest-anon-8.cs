using System;
using System.Collections.Generic;

delegate int Foo ();

class X
{
	static void Main ()
	{
		Test ("Hello World", 8);
	}

	public static void Test<R> (R r, int a)
	{
		for (int b = a; b > 0; b--) {
			R s = r;
			Foo foo = delegate {
				Console.WriteLine (b);
				Console.WriteLine (s);
				return 3;
			};
			a -= foo ();
		}
	}
}
