// Compiler options: -r:gtest-464-lib.dll

class Test
{
	public static int Main ()
	{
		if ((int) M.Test () != 22)
			return 1;
		
		return 0;
	}
}
