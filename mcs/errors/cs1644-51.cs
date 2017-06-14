// CS1644: Feature `throw expression' cannot be used because it is not part of the C# 6.0 language specification
// Line: 5
// Compiler options: -langversion:6

static class Class
{
	int Prop => throw null;
}
