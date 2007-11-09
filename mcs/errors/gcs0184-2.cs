// CS0184: The given expression is never of the provided (`T') type
// Line: 9
// Compiler options: -warnaserror -warn:1

class X<T> where T : class
{
	static void Foo ()
	{
		if (1 is T) {
		}
	}
}
