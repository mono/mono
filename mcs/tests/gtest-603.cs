using System;

public class A<T>
{
	T value;

	public A (T value)
	{
		this.value = value;
	}

	public static explicit operator T (A<T> source)
	{
		return source.value;
	}
}

public class Test
{
	public static int Main ()
	{
		var source = new A<int?> (3);
		if (N ((int)source) != 3)
			return 1;

		return 0;
	}

	static int N (int value)
	{
		return value;
	}
}