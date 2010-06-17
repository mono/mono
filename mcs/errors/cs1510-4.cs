// CS1510: A ref or out argument must be an assignable variable
// Line: 13

class M
{
	static void Test (ref byte b)
	{
	}
	
	public static void Main ()
	{
		byte b = 1;
		Test (ref (byte) b);
	}
}
