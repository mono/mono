// CS1644: Feature `collection initializers' cannot be used because it is not part of the C# 2.0 language specification
// Line: 9
// Compiler options: -langversion:ISO-2

using System.Collections.Generic;

class A
{
	void Foo ()
	{
		object o = new List<int> { 1, 2, 3 };
	}
}
