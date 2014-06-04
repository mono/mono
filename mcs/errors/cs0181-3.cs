// CS0181: Attribute constructor parameter has type `System.Enum', which is not a valid attribute parameter type
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