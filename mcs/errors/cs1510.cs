// cs1510: an lvalue is required for ref or out argument
// Line: 11
class X {
	public static void m (ref int i)
	{
		i++;
	}

	static void Main ()
	{
		m (ref 4);
	}
}
