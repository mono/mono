// CS0184: The given expression is never of the provided (`object') type
// Line: 13
// Compiler options: -warnaserror -warn:1

public class Test
{
	static void Foo ()
	{
	}
	
	public static void Main()
	{
		bool b = Foo () is object;
	}
}
