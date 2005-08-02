// cs0432.cs: Alias `fool' not found
// Line: 9

using foo = System;

class X {
	static void Main ()
	{
		fool::Console.WriteLine ("hello");
	}
}
