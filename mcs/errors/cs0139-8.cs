// CS0139: No enclosing loop out of which to break or continue
// Line: 9

class Foo {
	static void Main ()
	{
		try {}
		finally {
			break;
		}
	}
}

