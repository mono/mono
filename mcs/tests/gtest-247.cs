using System;
using System.Diagnostics;
using SCG = System.Collections.Generic;

public abstract class EnumerableBase<T> : SCG.IEnumerable<T>
{
	public abstract SCG.IEnumerator<T> GetEnumerator ();

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}
}

public abstract class CollectionValueBase<T> : EnumerableBase<T>
{
	protected virtual void raiseItemsAdded (T item, int count)
	{ }

	protected class RaiseForRemoveAllHandler
	{
		CircularQueue<T> wasRemoved;
	}

	public override abstract SCG.IEnumerator<T> GetEnumerator();
}

public class CircularQueue<T> : EnumerableBase<T>
{
	public override SCG.IEnumerator<T> GetEnumerator()
	{
		yield break;
	}

	public virtual void Enqueue (T item)
	{ }
}

public class HashSet<T> : CollectionValueBase<T>
{
	private bool searchoradd (ref T item, bool add, bool update, bool raise)
	{
		return true;
	}

	public virtual void RemoveAll<U>(SCG.IEnumerable<U> items) where U : T
	{
		RaiseForRemoveAllHandler raiseHandler = new RaiseForRemoveAllHandler ();
	}

	public virtual void AddAll<U> (SCG.IEnumerable<U> items)
		where U : T
	{
		CircularQueue<T> wasAdded = new CircularQueue<T> ();

		foreach (T item in wasAdded)
			raiseItemsAdded (item, 1);
	}

	public override SCG.IEnumerator<T> GetEnumerator()
	{
		yield break;
	}
}

class X
{
	public static void Main ()
	{ }
}
