// CS0119: Expression denotes a `method group', where a `variable', `value' or `type' was expected
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
