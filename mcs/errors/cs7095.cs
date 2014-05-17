// CS7095: Exception filter expression is a constant
// Line: 12
// Compiler options: -warnaserror

using System;

class X
{
	public static int Main ()
	{
		try {
			throw new ApplicationException ();
		} catch if (true) {
			return 0;
		}
	}
}