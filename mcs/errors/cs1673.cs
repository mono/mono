// cs1673.cs: Not possible to use `this' in anonymous methods inside structs
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
	
		
