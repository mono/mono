// CS0162: Unreachable code detected
// Line: 14
// Compiler options: -warnaserror

using System;

class X
{

	public static void Main ()
	{
		goto X;
	A:
		bool b = false;
		if (b) {
			goto A;
		}
	X:
		return;
	}
}