// CS0458: The result of the expression is always `null' of type `ulong?'
// Line: 10
// Compiler options: -warnaserror -warn:2

class C
{
	static void Foo()
	{
		ulong a = 100;
		var res = a << null;
	}
}
