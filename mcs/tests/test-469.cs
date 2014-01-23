//
// Do not extend this test
//
// This test copes with the case where a parameter was already captured
// and a second anonymous method on the same scope captured a parameter
//
using System;

delegate void Del (int n);

class Lambda {

	static int v;

	static void f (int va)
	{
		v = va;
	}
	
	static Del[] Make2 (int x) { 
		return new Del[] {
			delegate (int a) { f(x += a); },
			delegate (int b) { f(x += b); }
		};
	}
  
	public static int Main () { 
		Del[] d = Make2(10);
		d[0](10);
		if (v != 20)
			return 1;
				     
		d[1](11);
		if (v != 31)
			return 2;
		Console.WriteLine ("OK");
		return 0;
	}
}
