// CS0133: The expression being assigned to `b' must be a constant or default value
// Line: 8

class X
{
	static void Main ()
	{
		const int b = true ? 1 : b;
	}
}
