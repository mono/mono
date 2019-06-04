// Compiler options: -langversion:7.2 /unsafe

using System;

class X
{
	public static void Main ()
	{
		Span<int> stackSpan = stackalloc int[100];

		bool b = false;

		var r1 = !b ? stackalloc char[1] : throw null;
		var r2 = b ? throw null : stackalloc char[1];
		var r3 = b ? stackalloc char[1] : stackalloc char[2];
	}

	// Disables verifier
	unsafe void Foo ()
	{
	}
}
