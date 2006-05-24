// cs0139-7.cs: No enclosing loop out of which to break or continue
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

