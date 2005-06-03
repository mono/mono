// cs0654: Method `Test.foo()' is referenced without parentheses
// Line: 9
// Compiler options: -langversion:ISO-1

class Test {
	static int foo () { return 0; }
	static void Main ()
	{
		int i = foo;
	}
}
