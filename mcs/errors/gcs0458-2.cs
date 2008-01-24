// CS0458: The result of the expression is always `null' of type `int?'
// Line: 8
// Compiler options: -warnaserror -warn:2

public class MainClass
{
	public static void Main()
	{
		object d = (int?)null as int?;
	}
}

