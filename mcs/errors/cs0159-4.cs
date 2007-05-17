// cs0159-4.cs: No such label `skip' in this scope
// Line: 9

class Foo {
	static void Main ()
	{
		try {}
		finally {
			goto skip;
		}
	}
}

