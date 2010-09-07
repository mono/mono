// Compiler options: -r:gtest-534-lib.dll

class A : IA
{
	public void Method (IG<double[][]> arg)
	{
	}
	
	public static int Main ()
	{
		new A ().Method (null);
		return 0;
	}
}
