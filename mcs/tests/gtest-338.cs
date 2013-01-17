using System;
using System.Collections;
using System.Collections.Generic;

class Test {

	public static void Main ()
	{
		FooList<string> l = new FooList<string> ();
		Foo<string> (l);
	}

	static void Foo<T> (IList<T> list)
	{
		ICollection coll = list as ICollection;
		if (coll != null)
			Console.WriteLine (coll.Count);
	}
}

public class FooList<T> : IList<T> {

	public int IndexOf (T item)
	{
		throw new NotImplementedException ();
	}

	public void Insert (int index, T item)
	{
		throw new NotImplementedException ();
	}

	public void RemoveAt (int index)
	{
		throw new NotImplementedException ();
	}

	public T this [int index]
	{
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	public void Add (T item)
	{
		throw new NotImplementedException ();
	}

	public void Clear ()
	{
		throw new NotImplementedException ();
	}

	public bool Contains (T item)
	{
		throw new NotImplementedException ();
	}

	public void CopyTo (T [] array, int arrayIndex)
	{
		throw new NotImplementedException ();
	}

	public bool Remove (T item)
	{
		throw new NotImplementedException ();
	}

	public int Count
	{
		get { throw new NotImplementedException (); }
	}

	public bool IsReadOnly
	{
		get { throw new NotImplementedException (); }
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator ()
	{
		throw new NotImplementedException ();
	}

	public IEnumerator GetEnumerator ()
	{
		throw new NotImplementedException ();
	}
}
