// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 18

using System;

enum E
{ 
	Value
}

class AAttribute : Attribute
{ 
	public AAttribute (Enum e)
	{
	}
}

[A (E.Value)]
class Test
{
}