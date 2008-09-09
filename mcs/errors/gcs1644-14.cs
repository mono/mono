// CS1644: Feature `nullable types' cannot be used because it is not part of the C# 1.0 language specification
// Line: 9
// Compiler options: -langversion:ISO-1

struct C
{
	void G ()
	{
		object o = (C?[]) this;
	}
}
