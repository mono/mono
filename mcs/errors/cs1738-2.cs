// CS1738: Named arguments must appear after the positional arguments
// Line: 13
// Compiler options: -langversion:future

using System;

class MyAttribute : Attribute
{
	public MyAttribute (string s, int value)
	{
	}
}

[MyAttribute (s : "a", 1)]
class C
{
}
