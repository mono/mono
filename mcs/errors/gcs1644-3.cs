// CS1644: Feature `anonymous types' cannot be used because it is not part of the C# 1.0 language specification
// Line: 9
// Compiler options: -langversion:ISO-1

class A
{
	void Foo ()
	{
		var v = new { X = "Bar" };
	}
}
