// CS0133: The expression being assigned to `b' must be constant
// Line: 8

class X
{
	static void Main ()
	{
		const int b = true ? 1 : b;
	}
}
