// Compiler options: -warnaserror

class C
{
	[System.Diagnostics.Conditional("DEBUG")]
	public void Test (ref int i) {}

	public static void Main () {}
}
