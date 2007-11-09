// CS0183: The given expression is always of the provided (`object') type
// Line: 10
// Compiler options: -warnaserror -warn:1

class X
{
	public void Foo ()
	{
		int x = 1;
		if (x is object) {
		}
	}
}
