// cs1673.cs: Not possible to use `this' in anonymous methods inside structs
// Line: 
using System;

class S {
	delegate void T ();

	T t;

	int f;

	public void Test ()
	{
		// The loop just forces the creation of a helper class, so
		// that the anonymous method is not placed side-by-side this
		// method.
		int a = 1;
		for (int i = a; i < 10; i++){
			int j = i;
			t = delegate {
				Console.WriteLine ("Before: {0} {1} {2}", f, i, j);
				f = i;
			};
		}
	}
	
	static int Main ()
	{
	    S s = new S ();
	    s.Test ();
	    s.t ();
	    if (s.f == 10)
		    return 0;
	    Console.WriteLine ("Failed:" + s.f);
	    return 1;
	}
}
	
		
