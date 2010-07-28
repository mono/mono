// CS0023: The `.' operator cannot be applied to operand of type `method group'
// Line: 20

using System;

enum Enum
{
	Test
}

class A : Attribute
{
	public A (object e)
	{
	}
}

class C
{
	[A (Enum.Test)]
	int Enum ()
	{
		return 0;
	}
}
