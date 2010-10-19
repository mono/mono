using System;
using System.Collections.Generic;

interface I<T> : I2<T>, IEnumerable<T>
{
}

interface I2<T2>
{
	void Foo<U> (IEnumerable<U> list) where U : T2;
}

class Impl<T> : I<T>
{
	public void Foo<U> (IEnumerable<U> list) where U : T
	{
	}
	
	public IEnumerator<T> GetEnumerator ()
	{
		return null;
	}
	
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		return null;
	}
}

class A<K>
{
	public I<K> Value = new Impl<K> ();
}

class Test<TT> : A<TT>
{
	public void Foo ()
	{
		var a = new Test<TT> ();
		a.Value.Foo (Value);
	}
}

class M 
{
	public static void Main ()
	{
		new Test<ulong> ().Foo ();
	}
}

