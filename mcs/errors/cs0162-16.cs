// CS0162: Unreachable code detected
// Line: 10
// Compiler options: -warnaserror

using System;

class C
{
	void Test ()
	{
		return;
		const int a = 0;
		if (a > 0) {
			int x = a + 20;
			return;
		}
	}
}