// CS0216: The operator `X.operator true(X)' requires a matching operator `false' to also be defined
// Line: 5

class X {
	public static bool operator true (X i)
	{
		return true;
	}

	static void Main ()
	{
	}
}
