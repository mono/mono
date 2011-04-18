// CS1976: The method group `Main' cannot be used as an argument of dynamic operation. Consider using parentheses to invoke the method
// Line: 9

class C
{
	public static void Main ()
	{
		dynamic d = null;
		d (Main);
	}
}
