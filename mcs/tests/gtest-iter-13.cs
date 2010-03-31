using System;
using System.Collections;
using System.Collections.Generic;

class C<T>
{
	public IEnumerator GetEnumerator ()
	{
		return new T[0].GetEnumerator ();
	}

	public IEnumerable<T> Filter (Func<T, bool> predicate)
	{
		foreach (T item in this)
			if (predicate (item))
				yield return item;
	}
}

class M
{
	public static void Main ()
	{
		foreach (var v in new C<long>().Filter(null)) {
		}
	}
}
