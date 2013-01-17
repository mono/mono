// CS1644: Feature `asynchronous functions' cannot be used because it is not part of the C# 4.0 language specification
// Line: 10
// Compiler options: -langversion:4

using System;

class C
{
	public void Foo ()
	{
		Action a = async () => { };
	}
}

