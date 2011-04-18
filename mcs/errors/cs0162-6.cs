// CS0162: Unreachable code detected
// Line: 9
// Compiler options: -warnaserror -warn:2

class Foo {
	static void Main ()
	{
		goto skip;
	a:
		throw new System.Exception ();
		goto a;
	skip:
		return;
	}
}
