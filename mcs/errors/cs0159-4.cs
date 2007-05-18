// CS0159: The label `skip:' could not be found within the scope of the goto statement
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

