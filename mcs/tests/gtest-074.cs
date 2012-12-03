using System;

public struct Foo<T>
{
	public T Data, Data2;

	public Foo (T a, T b)
	{
		this.Data = a;
		this.Data2 = b;
	}
}

public class Test<T>
{
	public T Data, Data2;

	public Test (T a, T b)
	{
		this.Data = a;
		this.Data2 = b;
	}
}

class X
{
	public static int Main ()
	{
		Foo<long> foo = new Foo<long> (3, 5);
		if (foo.Data != 3)
			return 1;
		if (foo.Data2 != 5)
			return 2;

		Test<long> test = new Test<long> (3, 5);
		if (test.Data != 3)
			return 3;
		if (test.Data2 != 5)
			return 4;

		return 0;
	}
}
