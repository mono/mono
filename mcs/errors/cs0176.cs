// cs0176.cs: cannot be accessed with an instance reference, use typename instead
// Line: 12
class X {
	public static void void_method ()
	{
	}
}

class Y {
	void m (X arg)
	{
		arg.void_method ();
	}
}
