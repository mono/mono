// CS0815: An implicitly typed local variable declaration cannot be initialized with `default'
// Line: 9
// Compiler options: -langversion:latest

static class X
{
	public static void Main ()
	{
		var x = default;
	}
}