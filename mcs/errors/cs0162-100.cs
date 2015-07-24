// CS0162: Unreachable code detected
// Line: 18
// Compiler options: -warnaserror -warn:2

using System;

class X
{
	public static int Main ()
	{
		try {
			throw new ApplicationException ();
		} catch when (false) {
			return 0;
		}
	}
}