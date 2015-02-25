// Compiler options: -langversion:experimental
struct S (int arg)
{
	{
		v = arg;
	}

	public readonly int v;
}

class C (int arg)
{
	{
		v = arg;
	}

	public readonly int v;
}

class Test
{
	public static int Main ()
	{
		if (new C (4).v != 4)
			return 1;

		if (new S (3).v != 3)
			return 2;

		return 0;
	}
}