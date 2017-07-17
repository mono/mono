// CS8132: Cannot deconstruct a tuple of `2' elements into `3' variables
// Line: 8

class C
{
	public static void Main ()
	{
		var (t, u, v) = (1, 2);
	}
}