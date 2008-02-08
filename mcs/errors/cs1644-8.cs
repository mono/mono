// CS1644: Feature `null coalescing operator' cannot be used because it is not part of the C# 1.0 language specification
// Line: 10
// Compiler options: -langversion:ISO-1

class C
{
	string program;

	internal string Program {
		get { return program ?? string.Empty; }
	}
}