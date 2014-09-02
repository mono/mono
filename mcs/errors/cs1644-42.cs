// CS1644: Feature `null propagating operator' cannot be used because it is not part of the C# 5.0 language specification
// Line: 10
// Compiler options: -langversion:5

class C
{
	static void Main ()
	{
		string[] a = null;
		var s = a?[0];
	}
}