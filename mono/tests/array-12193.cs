using System;

class T
{
	// These are separate functions so that they are
	// JITed in a different context.

	static object f0 () { return new int[] {1,2}; }
	static object f1 () { return new int[,] {{2,3}}; }
	static object f2 () { return new int[,,] {{{3}}}; }
	static object f3 () { return new int[,,,] {{{{4}}}}; }
	static object f4 () { return new int[,,,,] {{{{{5}}}}}; }
	static object f5 () { return new int[,,,,,] {{{{{{6}}}}}}; }

	static int Main ()
	{
		f0 ();
		f1 ();
		f2 ();
		f3 ();
		f4 ();
		f5 ();
		return 0;
	}
}
