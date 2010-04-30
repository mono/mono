using System;
using System.Collections.Generic;

class C
{
	static int Test (List<int> arg)
	{
		return 10;
	}

	static int Test (string arg)
	{
		return 9;
	}

	static int Test (int arg)
	{
		return 8;
	}

	static R Method<T, R> (IEnumerable<T> t, Func<T, R> a)
	{
		return a (default (T));
	}

	static R Method2<T, R> (IEnumerable<T> t, Func<List<T>, R> a)
	{
		return a (default (List<T>));
	}

	public static int Main ()
	{
		if (Method (new int[] { 1 }, Test) != 8)
			return 1;

		if (Method2 (new int[] { 1 }, Test) != 10)
			return 2;

		return 0;
	}
}
