using System;

public class C
{
	static int Foo (int a, int b = 1, int c = 1)
	{
		return a;
	}

	public static int Main ()
	{
		if (Foo (c: 5, a: 10) != 10)
			return 1;

		if (Foo (a: 10) != 10)
			return 2;

		return 0;
	}
}
