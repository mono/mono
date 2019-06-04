// CS1644: Feature `byref locals and returns' cannot be used because it is not part of the C# 6.0 language specification
// Line: 9
// Compiler options: -langversion:6

class Text
{
	static ref long Foo ()
	{
		throw new System.NotImplementedException ();
	}
}
