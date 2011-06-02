// CS1644: Feature `lambda expressions' cannot be used because it is not part of the C# 2.0 language specification
// Line: 11
// Compiler options: -langversion:ISO-2

using System;

class C
{
	public void Foo ()
	{
		Func<int, int> e = l => 1;
	}
}

