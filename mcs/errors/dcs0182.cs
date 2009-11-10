// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
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
