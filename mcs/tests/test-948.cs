// Compiler options: -langversion:7.2 -unsafe

using System;

class X
{
	public static void Main ()
	{
		Span<int> stackSpan = stackalloc int[100];
	}

	unsafe void Foo ()
	{

	}
}