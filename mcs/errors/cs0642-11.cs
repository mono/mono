// CS0642: Possible mistaken empty statement
// Line: 9
// Compiler options: -warnaserror -warn:3

public class C
{
	public void Test ()
	{
		using (System.IDisposable arg = null);
		{ }
	}
}
