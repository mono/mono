using System;

class X
{
	static int Foo (params X[] p)
	{
		return 1;
	}

	static int Foo (object p)
	{
		return 0;
	}

	static int Main ()
	{
		if (Foo ((X[]) null) != 1)
			return 1;

		return 0;
	}
}