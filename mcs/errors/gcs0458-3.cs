// CS0458: The result of the expression is always `null' of type `int?' 
// Line: 8
// Compiler options: -warnaserror -warn:2

class C
{
	static void Main ()
	{
		int? i = 2;
		i *= null;
	}
}
