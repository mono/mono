using System;
using System.Collections;
using System.Collections.Generic;

class MyList<T> : IEnumerable<T>
{
	public IEnumerator<T> GetEnumerator ()
	{
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}
}

struct Foo<T>
{
	public readonly T Data;
  
	public Foo (T data)
	{
		this.Data = data;
	}
}

class X
{
	public static void Main ()
	{
		MyList<Foo<int>> list = new MyList <Foo<int>> ();
		foreach (Foo<int> foo in list)
			;
	}
}
