using System;
using System.Collections;
using System.Collections.Generic;

interface IFoo : IEnumerable<Foo>
{
}

class Foo : IFoo
{
	List<Foo> _fooList = new List<Foo> ();

	IEnumerator<Foo> IEnumerable<Foo>.GetEnumerator ()
	{
		return _fooList.GetEnumerator ();
	}

	public IEnumerator GetEnumerator ()
	{
		return _fooList.GetEnumerator ();
	}

	public static void Main ()
	{
	}
}
