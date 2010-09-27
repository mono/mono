// CS0458: The result of the expression is always `null' of type `bool?' 
// Line: 8
// Compiler options: -warnaserror -warn:2

class C
{
	static void Foo()
	{
		bool? b = false | null;
	}
}
