using System;
using TestAlias = A.N<double>;

class C<T>
{
	public class Foo<U>
	{
	}

	public class Simple
	{
	}

	static Type foo = typeof (Foo<>);
	static Type simple = typeof (Simple);
}

class D<U> : C<U>
{
}

class A
{
	public class N<T>
	{
	}
}

class M
{
	public static int Main ()
	{
		new C<int> ();
		
		if (typeof (TestAlias).ToString () != "A+N`1[System.Double]")
			return 1;

		if (typeof (D<>.Simple).ToString () != "C`1+Simple[T]")
			return 2;
		
		return 0;
	}
}

