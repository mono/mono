// CS1977: An anonymous method or lambda expression cannot be used as an argument of dynamic operation. Consider using a cast
// Line: 9

class C
{
	public static void Main ()
	{
		dynamic d = null;
		d (delegate {});
	}
}
