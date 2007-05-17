// CS0159: No such label `a' in this scope
// Line: 8

class Foo {
	static void Main ()
	{
		int i = 9;
		goto a;
		do {
		a:
			throw new System.Exception ("huh?");
		} while (i != 9);
	}
}
