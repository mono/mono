using System;

class Foo<T>
{
	public int Test (Foo<T> foo)
	{
		return 1;
	}

	public int Test (Foo<int> foo)
	{
		return 2;
	}
}

class X
{
	public static int Main ()
	{
		Foo<long> foo = new Foo<long> ();
		Foo<int> bar = new Foo<int> ();
		if (foo.Test (foo) != 1)
			return 1;
		if (foo.Test (bar) != 2)
			return 2;
		if (bar.Test (bar) != 2)
			return 3;
		return 0;
	}
}
