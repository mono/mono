// CS8346: Cannot convert a stackalloc expression of type `byte' to type `System.Span<int>'
// Line: 11
// Compiler options: -langversion:7.2

using System;

class X
{
	public static void Main ()
	{
		Span<int> stackSpan = stackalloc byte[1];
	}
}