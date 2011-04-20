// CS0183: The given expression is always of the provided (`System.Enum') type
// Line: 13
// Compiler options: -warnaserror

using System;

enum E { Item };

class C
{
	static bool Check (E e)
	{
		return e is Enum;
	}
}
