// Compiler options: -r:gtest-553-lib.dll

class C
{
	public static int Main ()
	{
		new A.C<int> ();
		new B.C<byte> ();
		return 0;
	}
}