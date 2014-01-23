using System;
using SCG = System.Collections.Generic;

public interface IExtensible<T>
{
	void AddAll<U> (SCG.IEnumerable<U> items)
		where U : T;
}

public abstract class CollectionValueTester<R,S>
	where R : IExtensible<S>
{
	protected R collection;
}

public class ExtensibleTester<U> : CollectionValueTester<U,int>
	where U : IExtensible<int>
{
	public ExtensibleTester (U u)
	{
		this.collection = u;
	}

	public void Direct()
	{
		collection.AddAll<int> (new int[] { });
	}
}

public class Extensible<V> : IExtensible<V>
{
	public void AddAll<W> (SCG.IEnumerable<W> items)
		where W : V
	{ }
}

class X
{
	public static void Main ()
	{
		Extensible<int> ext = new Extensible<int> ();
		ExtensibleTester<Extensible<int>> tester = new ExtensibleTester<Extensible<int>> (ext);
		tester.Direct ();
	}
}
