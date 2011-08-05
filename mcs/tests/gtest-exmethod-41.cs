using System.Collections.Generic;
using System;

static class TT
{
	static void Method<T> (this IEnumerable<T> e, Func<T, bool> f)
	{
	}
	
	public static void Test<U> (U u) where U : IList<string>
	{
		u.Method (l => l != null);
	}
}


class A
{
	public static void Main ()
	{
		TT.Test (new string[0]);
	}
}
