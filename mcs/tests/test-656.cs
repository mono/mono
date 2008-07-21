// Compiler options: -r:test-656-lib.dll

class Goo
{
	static void Main ()
	{
		string s = new Foo () ["foo"];
	}
}
