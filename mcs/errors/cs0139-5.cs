// CS0139: No enclosing loop out of which to break or continue
// Line: 8

class Foo {
	static void Main ()
	{
		try {
			continue;
		} finally {
			throw new System.Exception ();
		}
	}
}
