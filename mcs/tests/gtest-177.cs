using System;
using System.Collections.Generic;

class X
{
	static int[] x = new int[] {100, 200};

	public static int Main ()
	{
		IEnumerator<int> enumerator = X<int>.Y (x);
		int sum = 0;
		while (enumerator.MoveNext ())
			sum += enumerator.Current;

		if (sum != 300)
			return 1;

		if (X<int>.Z (x, 0) != 100)
			return 2;

		if (X<int>.Z (x, 1) != 200)
			return 3;

		return 0;
	}
}

class X <T>
{
	public static IEnumerator<T> Y (IEnumerable <T> x)
	{
		return x.GetEnumerator ();
	}

	public static T Z (IList<T> x, int index)
	{
		return x [index];
	}
}
