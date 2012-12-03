//
// Versioning test: make sure that we output a warning, but still call the derived
// method
//
using System;

class Base {
	public int which;
	
	public virtual void A ()
	{
		which = 1;
	}
}

class Derived :Base {
	public virtual void A ()
	{
		which = 2;
	}
}

class Test {
	public static int Main ()
	{
		Derived d = new Derived ();

		//
		// This should call Derived.A and output a warning.
		//
		d.A ();

		
		if (d.which == 1)
			return 1;

		Console.WriteLine ("Test passes");
		
		return 0;
	}
}
