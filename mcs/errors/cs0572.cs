// cs0572: Can't reference type `Foo' through an expression; try `Y.Foo' instead.
// Line: 13
using System;

class X
{
	private static Y y;

	public static void Main ()
	{
		y = new Y ();

		object o = y.Foo.Hello;
	}
}

class Y
{
	public enum Foo { Hello, World };

	public void Test (Foo foo)
	{
		Console.WriteLine (foo);
	}
}
