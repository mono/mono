// CS1644: Feature `extension methods' cannot be used because it is not part of the C# 2.0 language specification
// Line: 17
// Compiler options: -langversion:ISO-2

static class Extensions
{
	static string Foo (string s, this bool b, int i)
	{
		return s;
	}
}
