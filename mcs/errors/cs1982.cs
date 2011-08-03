// CS1982: An attribute argument cannot be dynamic expression
// Line: 13

using System;

class AAttribute : Attribute
{
	public AAttribute (dynamic X)
	{
	}
}

[A (Test.B)]
class Test
{
	public static dynamic B;

	static void Main ()
	{
	}
}
