//
// cs0225.cs: params parameter have to be a single dimensional array.
// 

public class X
{
	public static void Test (params int a)
	{
	}

	public static void Main()
	{
		Test (1);
	}
}
