// CS1061: Type `X' does not contain a definition for `Test' and no extension method `Test' of type `X' could be found (are you missing a using directive or an assembly reference?)
// Line: 12

class A<X>
{
	//
	// This is to test the lookup rules for SimpleNames:
	// `X' is the type parameter, not the class.
	//
	public void Test (X x)
	{
		x.Test ();
	}
}

class X
{
	public void Test ()
	{ }

	public static void Main ()
	{ }
}
