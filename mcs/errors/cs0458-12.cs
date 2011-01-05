// CS0458: The result of the expression is always `null' of type `int?'
// Line: 9
// Compiler options: -warnaserror -warn:2

class C
{
	static void Foo()
	{
		var res = null >> 2;
	}
}
