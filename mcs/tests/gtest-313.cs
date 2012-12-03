using System;

class Foo<T>
{
	public enum TestEnum { One, Two, Three }

	public TestEnum Test;

	public Foo (TestEnum test)
	{
		this.Test = test;
	}
}

class X
{
	public static void Main ()
	{
		Foo<int>.TestEnum e = Foo<int>.TestEnum.One;
		Console.WriteLine (e);

		Foo<int> foo = new Foo<int> (e);
		foo.Test = e;
	}
}
