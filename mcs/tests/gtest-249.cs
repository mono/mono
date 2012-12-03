using System;
using SCG = System.Collections.Generic;

public abstract class EnumerableBase<T> : SCG.IEnumerable<T>
{
	public abstract SCG.IEnumerator<T> GetEnumerator();

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

public abstract class ArrayBase<T> : EnumerableBase<T>
{
	public override SCG.IEnumerator<T> GetEnumerator()
	{
		yield break;
	}

}

public class HashedArrayList<T> : ArrayBase<T>
{
	public override SCG.IEnumerator<T> GetEnumerator()
	{
		return base.GetEnumerator();
	}
}

class X
{
	public static void Main ()
	{ }
}
