// CS8184: A deconstruction cannot mix declarations and expressions on the left-hand-side
// Line: 8

class X
{
	public static void Main ()
	{
		(int a, b) = (1, 2);
	}
}