using System;

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

class M
{
	public static void Main ()
	{
		new C<int> ();
	}
}

