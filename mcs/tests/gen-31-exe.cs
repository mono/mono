// Compiler options: -r:gen-31-dll.dll

public class X
{
	public static void Test (Bar<int,string> bar)
	{
		bar.Hello ("Test");
		bar.Test (7, "Hello");
	}

	static void Main ()
	{ }
}
