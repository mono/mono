// Compiler options: -r:test-656-lib.dll;

// Trailing semicolon is part of the test

class Goo
{
	static void Main ()
	{
		string s = new Foo () ["foo"];
	}
}
