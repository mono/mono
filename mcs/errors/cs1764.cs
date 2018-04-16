// CS1764: Cannot use fixed variable `p' inside an anonymous method, lambda expression or query expression
// Line: 10
// Compiler options: -unsafe

using System;

unsafe class Test
{
	static int x;

	static void Main ()
	{
		fixed (int* p = &x) {
			Action a = () => { var pp = p; };
		}
	}
}
