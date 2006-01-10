using System;
using System.Collections.Generic;

public class H
{
	public static void Main(String[] args) { }

	public static IEnumerable<T> Merge<T> (IEnumerator<T> xEtor)
		where T : IComparable<T>
	{
		int order = xEtor.Current.CompareTo (xEtor.Current);
		yield return xEtor.Current;
	}
}
