using System;
using System.Collections.Generic;

class X
{
	public IEnumerable<T> Test<T> (T a, T b)
	{
		yield return b;
		b = a;
		yield return a;
        }

	static int Main ()
	{
		X x = new X ();
		long sum = 0;
		foreach (long i in x.Test (3, 5)) {
			Console.WriteLine (i);
			sum += i;
		}

		Console.WriteLine (sum);
		return sum == 8 ? 0 : 1;
	}
}
