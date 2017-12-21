// CS1644: Feature `readonly references' cannot be used because it is not part of the C# 7.0 language specification
// Line: 9
// Compiler options: -langversion:7

class X
{
	int i;

	ref readonly int Test ()
	{
		return ref i;
	}
}
