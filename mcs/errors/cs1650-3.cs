// CS1650: Fields of static readonly field `C<T>.t' cannot be assigned to (except in a static constructor or a variable initializer)
// Line: 17

using System;

interface I
{
	int X { get; set; }
}

class C<T> where T : struct, I
{
	static readonly T t;

	public static void Foo ()
	{
		t.X = 42;
	}
}
