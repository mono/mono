// CS1742: An element access expression cannot use named argument
// Line: 13
// Compiler options: -unsafe

using System;

unsafe class C
{
	static void Main ()
	{
		int *p = null;

		if (p [value:10] == 4)
			return;
	}
}
