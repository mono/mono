// CS1644: Feature `object initializers' cannot be used because it is not part of the C# 2.0 language specification
// Line: 14
// Compiler options: -langversion:ISO-2

class Data
{
	public int Value;
}

class A
{
	void Foo ()
	{
		new Data () { Value = 3 };
	}
}
