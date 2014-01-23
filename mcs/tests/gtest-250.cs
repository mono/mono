using System;
using SCG = System.Collections.Generic;

public delegate S Fun<R,S> (R r);

public interface IIndexedSorted<T>
{
	IIndexedSorted<V> Map<V> (Fun<T,V> mapper);
}

public class GuardedIndexedSorted<T> : IIndexedSorted<T>
{
	IIndexedSorted<T> indexedsorted;

	public IIndexedSorted<V> Map<V> (Fun<T,V> m)
	{
		return indexedsorted.Map (m);
	}
}

class X
{
	public static void Main ()
	{ }
}
