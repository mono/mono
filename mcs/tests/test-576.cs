class Foo {
	public static void Main ()
	{
		int a = 0;
		int b = 5;
		a += -b;
		if (a != -5)
			throw new System.Exception ();
	}
}
