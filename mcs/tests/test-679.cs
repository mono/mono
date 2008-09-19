// Compiler options: -r:dlls/test-679-1/test-679-lib.dll -r:dlls/test-679-2/test-679-lib-2.dll

using System;

class Program {

	static int Main ()
	{
		LibB.A ();
		LibB.B ();
		return 0;
	}
}
