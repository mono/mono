//
// Versioning test: make sure that we output a warning, but still call the derived
// method
//
class Base {
	public int which;
	
	virtual void A ()
	{
		which = 1;
	}
}

class Derived {
	public virtual void A ()
	{
		which = 2;
	}
}

class Test {
	int Main ()
	{
		Derived d = new Derived ();

		//
		// This should call Derived.A and output a warning.
		//
		d.A ();

		
		if (d.which == 1)
			return 1;

		return 0;
	}
}
