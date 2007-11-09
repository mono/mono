// CS0184: The given expression is never of the provided (`Y') type
// Line: 10
// Compiler options: -warnaserror -warn:1

class Y
{
}

class X
{
	public void Foo ()
	{
		X x = null;
		if (x is Y) {
		}
	}
}
