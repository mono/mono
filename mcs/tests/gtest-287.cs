using System;
using System.Collections.Generic;

interface I {
	void D ();
}

class X : I
{
        public static void Main ()
        {
		List<object> l = new List<object> ();
		List<I> i = new List<I> ();

		i.Add (new X());

		l.AddRange (i.ToArray());
	}

	public void D () {}
}

