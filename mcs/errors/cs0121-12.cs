// CS0121: The call is ambiguous between the following methods or properties: `D.Test(bool, string)' and `D.Test(bool, int, string)'
// Line: 16

public class D
{
	static void Test (bool b, string a = "s")
	{
	}

	static void Test (bool b, int i = 9, string a = "b")
	{
	}

	public static void Main ()
	{
		Test (false);
	}
}
