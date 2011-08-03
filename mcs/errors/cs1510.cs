// CS1510: A ref or out argument must be an assignable variable
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
