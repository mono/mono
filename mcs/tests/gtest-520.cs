using System;
using System.Collections;
using System.Collections.Generic;

class A
{
	public void Method ()
	{
	}
}

class C : IEnumerable, IEnumerable<A>
{
	public class GetEnumerator
	{
	}
	
	IEnumerator IEnumerable.GetEnumerator ()
	{
		throw new ApplicationException ();
	}

	IEnumerator<A> IEnumerable<A>.GetEnumerator ()
	{
		return new List<A> ().GetEnumerator ();
	}
}

class D : C
{
}

public class Test
{
	public static int Main ()
	{
		foreach (var v in new C ()) {
			v.Method ();
		}

		foreach (var v in new D ()) {
			v.Method ();
		}

		return 0;
	}
}