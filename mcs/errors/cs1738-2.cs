// CS1738: Named arguments must appear after the positional arguments
// Line: 13

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
