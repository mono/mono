// Compiler options: -unsafe

//
// Test for http://bugzilla.ximian.com/show_bug.cgi?id=62263
//
// We need to make sure that pointer arth uses the size of X
// not the size of X*
//

using System;
unsafe struct X {
	int x, y, z;
	
	public static int Main ()
	{
		X* foo = null;
		
		if ((int) (foo + 1) != sizeof (X))
			return 1;
		return 0;
	}
}
