// CS0176: Static member `X.void_method()' cannot be accessed with an instance reference, qualify it with a type name instead
// Line: 15
class X {
	public static void void_method ()
	{
	}
	public void void_method (int i)
	{
	}
}

class Y {
	void m (X arg)
	{
		arg.void_method ();
	}
}
