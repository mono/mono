// Compiler options: -r:gtest-exmethod-25-lib.dll

using lib1;

public class Bar
{
	public static void Main ()
	{
		"a".Extend ();
	}
}
