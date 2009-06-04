// Compiler options: -r:gtest-447-2-lib.dll -r:gtest-447-3-lib.dll

using System;

class B
{
	public static int Main ()
	{
		if (C.Print ("x") != "x")
			return 1;

		return 0;
	}
}
