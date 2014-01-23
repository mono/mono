using System;

class Foo<T>
{
	public int Test (T foo)
	{
		return 1;
	}

	public int Test (int foo)
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
		if (foo.Test (4L) != 1)
			return 1;
		if (foo.Test (3) != 2)
			return 2;
		if (bar.Test (3) != 2)
			return 3;
		return 0;
	}
}
