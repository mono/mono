// Compiler options: -r:gtest-278-3-lib.dll

using System;

class B
{
	public static int Main ()
	{
		if (C.Print () != "C")
			return 1;

		if (D.Print () != "D")
			return 2;
			
		if (G<int>.Test (5) != 5)
			return 3;

		return 0;
	}
}
