// CS1511: Keyword `base' is not available in a static method
// Line:

class Y {
	public int a;
}

class X : Y {

	static void Main ()
	{
		base.a = 1;
	}
}
