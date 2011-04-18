// CS0121: The call is ambiguous between the following methods or properties: `D.Test(string)' and `D.Test(int, string)'
// Line: 16

public class D
{
	static void Test (string a = "s")
	{
	}

	static void Test (int i = 9, string a = "b")
	{
	}

	public static void Main ()
	{
		Test ();
	}
}
