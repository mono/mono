// CS1742: An element access expression cannot use named argument
// Line: 9
// Compiler options: -langversion:future

class C
{
	public static void Main ()
	{
		int[] o = new int[5];
		o [u:3] = 9;
	}
}
