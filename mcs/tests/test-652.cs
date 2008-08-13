// Compiler options: -unsafe

using System;

class C
{
	public static int Main ()
	{
		return Test ();
	}

	public static unsafe int Test ()
	{
		// Test: Unsafe local variable is initialized to 0
		int* v = stackalloc int [5];
		Console.WriteLine (v [0]);
		return v [0] + v [1] + v [4];
	}
}
