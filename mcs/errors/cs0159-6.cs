// CS0159: The label `a:' could not be found within the scope of the goto statement
// Line: 9

class Foo {
	static void Main ()
	{
		int i = 9;
		goto a;
		while (i != 9) {
		a:
			throw new System.Exception ("huh?");
		}
	}
}
