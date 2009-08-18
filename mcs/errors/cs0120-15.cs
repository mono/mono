// CS0120: An object reference is required to access non-static member `C.Enum()'
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
