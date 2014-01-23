using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

class Program
{
	public static int Main ()
	{
		foreach (int o in Test<bool> (1)) {
		}

		return 0;
	}

	static IEnumerable<int> Test<T> (int i)
	{
		Expression<Func<int>> e = () => i;
		yield return 1;
	}
}
