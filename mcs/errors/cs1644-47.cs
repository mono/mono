// CS1644: Feature `out variable declaration' cannot be used because it is not part of the C# 5.0 language specification
// Line: 9
// Compiler options: -langversion:5

class C
{
	public static void Main ()
	{
		int.TryParse ("0", out var v);
	}
}