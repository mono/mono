using System;
using System.Collections;
using System.Collections.Generic;

public class Qux<X,V> : IEnumerable<V>
	where V : IComparable<V>
{
	public IEnumerator<V> GetEnumerator()
	{
		yield break;
	}

        IEnumerator IEnumerable.GetEnumerator()
        {
        	yield break;
        }
}

public class Foo<X,V> : Qux<X,V>
	where V : IComparable<V>
{
}

public class Test<T> : IComparable<Test<T>>
{
	public int CompareTo (Test<T> t)
	{
		return 0;
	}
}

class X
{
	public static void Main ()
	{
		Foo<X,Test<X>> foo = new Foo<X,Test<X>> ();
		foreach (Test<X> test in foo)
			;
	}
}
