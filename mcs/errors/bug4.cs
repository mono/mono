//
// Fixed
//
using System;

class X {
	static void Main ()
	{
		try {
			throw new Exception ();
		} catch (Exception e) {
			Console.WriteLine ("Caught");
			throw;
		} catch {
			Console.WriteLine ("Again");
		}
	}
}
