//
// This test does not pass peverify because we dont return properly
// from catch blocks
//
using System;

class X {

	bool v ()
	{
		try {
			throw new Exception ();
		} catch {
			return false;
		}
		return true;
	}

	public static int Main ()
	{
		return 0;
	}		
}
