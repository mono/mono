namespace Martin
{
	public class Test<T>
	{
		public static int Foo ()
		{
			return 0;
		}
	}
}

class Foo<T>
{
	public Foo (int a)
	{ }

	public Foo ()
		: this (Martin.Test<T>.Foo ())
	{ }
}

class X
{
	public static void Main ()
	{
	}
}
