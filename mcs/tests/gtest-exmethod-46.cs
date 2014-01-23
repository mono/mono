using System;
using System.Collections.Generic;

namespace ExtensionTest.Two
{
	public delegate TResult AxFunc<in T1, out TResult> (T1 first);

	public static class Extensions
	{
		public static bool Contains<T> (this IEnumerable<T> source, T item)
		{
			return true;
		}

		public static bool All<T> (this IEnumerable<T> source, AxFunc<T, bool> predicate)
		{
			return true;
		}

	}
}

namespace ExtensionTest
{
	using ExtensionTest.Two;

	public static class MyClass
	{
		public static bool IsCharacters (this string text, params char[] chars)
		{
			return text.All (chars.Contains);
		}

		public static bool Contains (this string text, string value, StringComparison comp)
		{
			return text.IndexOf (value, comp) >= 0;
		}

		public static void Main ()
		{
		}
	}
}
