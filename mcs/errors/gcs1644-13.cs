// CS1644: Feature `lambda expressions' cannot be used because it is not part of the C# 2.0 language specification
// Line: 11
// Compiler options: -langversion:ISO-2

class C
{
	delegate void D ();
	
	public void Foo ()
	{
		D e = () => { };
	}
}

