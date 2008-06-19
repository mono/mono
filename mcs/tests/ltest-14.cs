// identity functions
using System;

class Test {
	public static void Main ()
	{
	}

	static void Foo<T> ()
	{
		Func<T,T> f = n => n;
	}
}
