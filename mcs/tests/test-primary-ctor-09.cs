using System;

class A (Func<int, int> barg)
{
	public Func<int, int> BaseArg = barg;
}

partial class PC
{
	public Func<int, int> f1 = (a) => arg;
}

partial class PC (int arg) 
	: A ((a) => arg)
{
}

class X
{
	public static int Main ()
	{
		if (new PC (3).f1 (4) != 3)
			return 1;

		if (new PC (3).BaseArg (4) != 3)
			return 2;

		return 0;
	}
}