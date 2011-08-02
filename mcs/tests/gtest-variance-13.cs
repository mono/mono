using System;
using System.Collections.Generic;

interface I<in T>
{
}

class A
{
	static void Foo<T> (T a, IList<T> c)
	{
	}
	
	public static void Test ()
	{
		Foo ("aaaa", new object[0]);
	}
}

class B
{
	static void Foo<T> (T a, I<T> c)
	{
	}

	static void Test<U> (U u, I<U> x)
	{
		Foo (u, x);
	}
}

class M
{
	public static int Main ()
	{
		return 0;
	}
}
