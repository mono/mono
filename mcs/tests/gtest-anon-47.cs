using System;

static class C
{
	public static Func<T1, Func<T2, Action<T3>>> Curry<T1, T2, T3> (this Action<T1, T2, T3> self)
	{
		return value1 => value2 => value3 => self (value1, value2, value3);
	}
}

class Test
{
	static int Main ()
	{
		Action<int, int, int> test = (x, y, z) => {
			int i = x + y + z;
			Console.WriteLine (i);
			if (i != 19)
				throw null;
		};
		Func<int, Func<int, Action<int>>> f = test.Curry ();

		f (3) (5) (11);

		return 0;
	}
}
