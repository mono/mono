using System;
using System.Collections.Generic;

public class Wrap<U>
{
	public List<U> t;
}

public class Test
{
	public int Run<T> (Wrap<T> t)
	{
		Action f = () => { t.t = new List<T> (); };
		f ();
		return t.t != null ? 0 : 1;
	}

	public static int Main ()
	{
		return new Test ().Run (new Wrap <byte> ());
	}
}