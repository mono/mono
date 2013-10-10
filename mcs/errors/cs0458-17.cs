// CS0458: The result of the expression is always `null' of type `int?'
// Line: 10
// Compiler options: -warnaserror -warn:2

class C
{
	public static void Main ()
	{
		int i = 44;
		i <<= null;
	}
}
