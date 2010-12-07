// Compiler options: -t:library -r:dlls/test-679-1/test-679-lib.dll -r:dlls/test-679-2/test-679-lib-2.dll

using System;

class Program
{
	void Main ()
	{
		LibB.A ();
		LibB.B ();
	}
}
