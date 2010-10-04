// Compiler options: -r:gtest-165-lib.dll

class C
{
	public static int Main ()
	{
		var a = new A<string>();
		if (a ["s"] != 2)
			return 1;
		
		return 0;
	}
}