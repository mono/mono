// CS0162: Unreachable code detected
// Line: 9
// Compiler options: -warnaserror -warn:2

class Foo {
	static void Main ()
	{
		goto skip;
	a:
		goto a;
	skip:
		return;
	}
}
