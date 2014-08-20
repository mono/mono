// CS8071: Type arguments are not allowed in the nameof operator
// Line: 10

using SCGL = System.Collections.Generic.List<int>;

class X
{
	public static int Main ()
	{
		var x = nameof (SCGL.Contains);
		return 0;
	}
}