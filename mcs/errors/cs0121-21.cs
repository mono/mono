// CS0121: The call is ambiguous between the following methods or properties: `C.C(double[], int, object)' and `C.C(double[], int, string[])'
// Line: 16

class C
{
	C (double[] o, int i = -1, object ii = null)
	{
	}
	
	C (double[] o, int i = -1, string[] s = null)
	{
	}
	
	public static void Main ()
	{
		new C (null, 1);
	}
}
