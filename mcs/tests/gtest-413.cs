using System;

struct S
{
	public static long operator + (S f, long n)
	{
		return n;
	}
}

class Test
{
	public static int Main ()
	{
		int? n = 1;
		S f = new S ();

		return (f + n) != 1 ? 1 : 0;
	}
}
