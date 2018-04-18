// CS0173: Type of conditional expression cannot be determined because there is no implicit conversion between `throw expression' and `throw expression'
// Line: 8

class C
{
	public static void Test (bool b)
	{
		var s = b ? throw null : throw null;
	}
}