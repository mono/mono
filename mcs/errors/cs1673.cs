// CS1673: Anonymous methods inside structs cannot access instance members of `this'. Consider copying `this' to a local variable outside the anonymous method and using the local instead
// Line: 
using System;

struct S {
	delegate void T ();

	int f;

	public int Test ()
	{
		T t = delegate {
			f = 1;
		};
		return 0;
	}
	
	static void Main ()
	{
	}
}
	
		
