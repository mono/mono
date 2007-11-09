// CS0184: The given expression is never of the provided (`B') type
// Line: 9
// Compiler options: -warnaserror -warn:1

class B
{
	static void Foo ()
	{
		if (1 is B) {
		}
	}
}
