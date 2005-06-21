using System;

public class Foo
{
}

public class X
{
	public static Foo Test (Foo foo, Foo bar)
	{
		return foo ?? bar;
	}

	public static int Main()
	{
		Foo[] array = new Foo [1];

		Foo foo = new Foo ();
		Foo bar = Test (array [0], foo);

		if (bar == null)
			return 1;

		return 0;
	}
}
