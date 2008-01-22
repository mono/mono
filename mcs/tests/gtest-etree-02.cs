using System;
using System.Linq.Expressions;

class M
{
	public static void Foo<T> (Expression<Func<T, T>> x)
	{
	}

	public static void Main ()
	{
		Foo<int> ((i) => i);

		Foo ((int i) => i);

		Expression<Func<int, int>> func = (i) => i;
		Foo (func);
	}
}

