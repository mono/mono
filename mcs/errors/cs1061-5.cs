// CS1061: Type `int' does not contain a definition for `Value' and no extension method `Value' of type `int' could be found. Are you missing an assembly reference?
// Line: 24

using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
	static class Ex
	{
		public static IEnumerable<TR> Foo<T, TR> (this IEnumerable<T> t, Func<T, TR> f)
		{
			return null;
		}
	}

	public class C
	{
		public static void Main ()
		{
			int[] i = null;
			int p;
            var prods = from pe in i.Foo (p9 => p.Value) select pe;
		}
	}
}
