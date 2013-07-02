using System;

class Test<T>
{
	public void Foo (T t, out int a)
	{
		a = 5;
	}

	public void Hello (T t)
	{
		int a;

		Foo (t, out a);
	}
}

class X
{
	public static void Main ()
	{ }
}
