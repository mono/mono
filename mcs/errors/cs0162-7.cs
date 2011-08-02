// CS0162: Unreachable code detected
// Line: 9
// Compiler options: -warnaserror -warn:2

// this requires a multi-pass algorithm for unreachable code detection
// punting for now

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
