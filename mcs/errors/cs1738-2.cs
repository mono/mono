// CS1738: Named arguments must appear after the positional arguments when using language version older than 7.2
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
