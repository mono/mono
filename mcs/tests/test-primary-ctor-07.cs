// Compiler options: -langversion:experimental

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

struct S2 (int arg)
{
	public readonly int v = arg;
}

struct S3 (string s = "arg")
{
	public readonly string V2 = s;

	public S3 (int i, string s = "arg2")
		: this (s)
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

		if (new S2 (2).v != 2)
			return 3;

		if (new S3 ("x").V2 != "x")
			return 4;

		if (new S3 (0).V2 != "arg2")
			return 5;

		return 0;
	}
}