// CS0173: Type of conditional expression cannot be determined because there is no implicit conversion between `dynamic' and `void'
// Line: 19

class X
{
	static void Main ()
	{
		dynamic d = null;
		dynamic t = true ? d : Main ();
	}
}
