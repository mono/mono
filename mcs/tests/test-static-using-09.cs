// Compiler options: -r:test-static-using-09-lib.dll

using static Constants;

static class Program
{
	static void Main ()
	{
		System.Console.WriteLine (One);
	}
}