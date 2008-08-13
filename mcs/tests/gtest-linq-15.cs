using System;
using System.Collections.Generic;
using System.Linq;

namespace NameCollisionTest
{
	class Data
	{
		public int Value;
	}

	static class Ex
	{
		public static IEnumerable<TR> Foo<T, TR> (this IEnumerable<T> t, Func<T, TR> f)
		{
			yield return f (t.First ());
		}
	}

	public class C
	{
		public static void Main ()
		{
			Data [] i = new Data [0];
			var prods = from pe in i.Foo (pe => pe.Value) where pe > 0 select pe;
		}
	}
}