// cs0216: Missmatch of operators (true/false)
// Line:  4
class X {
	public static bool operator true (X i)
	{
		return true;
	}

	static void Main ()
	{
	}
}
