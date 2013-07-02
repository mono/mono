class Foo {
	public static int Main ()
	{
		try {
			f ();
			return 1;
		} catch {
			return 0;
		}
	}
	static void f ()
	{
		try {
			goto skip;
		} catch {
			goto skip;
		} finally {
			throw new System.Exception ();
		}
	skip:
		;
	}
}
