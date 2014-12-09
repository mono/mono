using System;

class X
{
	public static U Foo<U> (double? value, Func<double?, U> f, int dv = 0)
	{
		throw new ApplicationException ();
	}

	public static U Foo<T, U> (T? source, Func<T, U> f) where T : struct
	{
		return default (U);
	}

	static void Main (string[] args)
	{
		Foo (default (double?), v => v / 100);
	}
}
