// Compiler options: -unsafe

// this tests making a pointer to a pointer

using System;

unsafe class Foo
{
	public static int Main ()
	{
		int a;
		int *b;
		int **c;

		a = 42;
		b = &a;
		c = &b;
		
		Console.WriteLine ("*c == b : {0}", *c == b);
		Console.WriteLine ("**c == a : {0}", **c == a);

		if (*c == b && **c == a)
		{
			Console.WriteLine ("Test passed");
			return 0;
		}
		else
		{
			Console.WriteLine ("Test failed");
			return 1;
		}
	}
}
