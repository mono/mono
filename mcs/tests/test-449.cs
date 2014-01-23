//
// This is a test for bug 75925, to ensure that
// we do not accidentally introduce regressions on anonymous methods.
//
// This bug only needs to be compiled
//

using System;
using System.Threading;

class X {

	public static void Main () 
	{
	}
	
	void Z ()
	{
		ThreadPool.QueueUserWorkItem (delegate {
			Z ();
					
			ThreadPool.QueueUserWorkItem (delegate {
				Z ();
			});	
		});
	}	
}



