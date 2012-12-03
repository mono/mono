//
// Tests rethrowing an exception
//
using System;

class X {
	public static int Main ()
	{
		bool one = false, two = false;

		try {
			try {
				throw new Exception ();
			} catch (Exception e) {
				one = true;
				Console.WriteLine ("Caught");
				throw;
			} 
		} catch {
			two = true;
			Console.WriteLine ("Again");
		}
		
		if (one && two){
			Console.WriteLine ("Ok");
			return 0;
		} else
			Console.WriteLine ("Failed");
		return 1;
	}
}
