using System;

class C
{
	static U Test<T, U>(T[] args, Func<T, U> f)
	{
		return f (args [1]);
	}

	public static int Main ()
	{
		var s = new string [] { "aaa", "bbb" };
		var foo = Test (s, i => { try { return i; } catch { return null; } });
		if (foo != s [1])
			return 1;

		return 0;
	}
}
