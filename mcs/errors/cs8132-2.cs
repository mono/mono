// CS8132: Cannot deconstruct a tuple of `2' elements into `3' variables
// Line: 11

class X
{
	static int xx;
	static long yy, zz;

	public static void Main ()
	{
		(xx, yy, zz) = Foo ();
	}

	static (int, long) Foo ()
	{
		return (1, 3);
	}
}