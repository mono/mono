// Compiler options: -unsafe

using System;

class Program
{
	static unsafe public int Main ()
	{
		return Test ((sbyte*) (-1));
	}

	static unsafe int Test (sbyte* x)
	{
		if ((x + 1) < x) {
			Console.WriteLine ("OK");
			return 0;
		} else {
			Console.WriteLine ("BAD");
			return 1;
		}
	}
}

