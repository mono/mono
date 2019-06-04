// CS8323: Named argument `str' is used out of position but is followed by positional argument
// Line: 9
// Compiler options: -langversion:7.2

class X
{
	public static void Main ()
	{
		Test (str: "", "");
	}

	static void Test (int arg, string str)
	{
	}
}