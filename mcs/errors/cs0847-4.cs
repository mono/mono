// CS0847: An array initializer of length `3' was expected
// Line: 9

class M
{
	public static void Main ()
	{
		int[,,] i = { { { 0, 0, 0 }, { 1, 1, 1 } },
			{ { 2 }, { 3, 3, 3 } } };
	}
}
