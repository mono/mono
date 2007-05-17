// CS0159: No such label `a' in this scope
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
