using System;

struct S (int x)
{
	public int y = x;

	public S (char x)
		: this (1)
	{
	}

	static S ()
	{
	}
}

class X
{
	public static int Main ()
	{
		if (new S (-5).y != -5)
			return 1;

		if (new S ('x').y != 1)
			return 2;

		return 0;
	}
}