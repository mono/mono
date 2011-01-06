// CS0184: The given expression is never of the provided (`bool') type
// Line: 13
// Compiler options: -warnaserror -warn:1

using System;

class X
{
	void Foo ()
	{
		int? i = null;
		
		if (i is bool) {
		}
	}
}
