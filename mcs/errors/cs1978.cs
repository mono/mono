// CS1978: An expression of type `__arglist' cannot be used as an argument of dynamic operation
// Line: 9

class C
{
	public static void Main ()
	{
		dynamic d = null;
		d (__arglist (111));
	}
}
