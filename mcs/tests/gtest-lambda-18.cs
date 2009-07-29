using System;

static class Test
{
	public static void Foo<T1, T2, TResult> (
		T1 arg1, T2 arg2, Func<T1, T2, TResult> func)
	{
		Bar (arg1, arg2, (a, b, _) => func (a, b));
	}

	public static void Bar<T1, T2, TResult> (
		T1 arg1, T2 arg2, Func<T1, T2, int, TResult> func)
	{
	}

	public static void Main ()
	{
	}
}
