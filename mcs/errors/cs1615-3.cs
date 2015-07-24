// CS1615: Argument `#1' does not require `out' modifier. Consider removing `out' modifier
// Line: 8
// Compiler options: -langversion:experimental

public class C
{
	public static void Main ()
	{
		Foo (out var y);
	}

	static void Foo (int x)
	{
	}

	static void Foo (string x)
	{
	}
}