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
}
