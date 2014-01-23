//
// It is possible to invoke object methods in an interface.
//
using System;

interface Iface {
	void Method ();
}

class X : Iface {

	void Iface.Method () {} 
	
	public static int Main ()
	{
		X x = new X ();
		Iface f = x;

		if (f.ToString () != "X")
			return 1;

		return 0;
	}
}
