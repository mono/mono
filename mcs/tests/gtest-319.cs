// Bug #80314
using System.Collections.Generic;
using System.Collections;

public class Temp<T> : IEnumerable<Temp<T>.Foo>
{
	public class Foo { }

	public IEnumerator<Temp<T>.Foo> GetEnumerator()
	{
		yield return new Foo ();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

class X
{
	public static void Main ()
	{ }
}
