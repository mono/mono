// cs0176.cs: cannot be accessed with an instance reference, use typename instead
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
