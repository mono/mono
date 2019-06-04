// CS8183: Cannot infer the type of implicitly-typed discard
// Line: 9
// Compiler options: -langversion:7.2

class X
{
	public static void Main ()
	{
		_ = default;
	}
}