// cs0216.cs: The operator `X.operator <(X, int)' requires a matching operator `>' to also be defined
// Line:
class X {
	public static X operator < (X a, int b)
	{
		return null;
	}

	static void Main () {
	}
}
	
