// CS1770: The expression being assigned to nullable optional parameter `d' must be default value
// Line: 8

using System;

class C
{
	public static void Foo (DateTime? d = new DateTime ())
	{
	}
}
