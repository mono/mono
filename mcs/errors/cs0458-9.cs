// CS0458: The result of the expression is always `null' of type `int?'
// Line: 9
// Compiler options: -warnaserror -warn:2

public class C
{
	public static void Main ()
	{
		b = (null & 0) != null;
	}
}
