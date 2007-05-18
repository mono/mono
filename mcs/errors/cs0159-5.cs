// CS0159: The label `a:' could not be found within the scope of the goto statement
// Line: 8

class Foo {
	static void Main ()
	{
		int i = 9;
		goto a;
		if (i == 9) {
		a:
			throw new System.Exception ("huh?");
		}
	}
}
