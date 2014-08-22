// CS7023: The second operand of `is' or `as' operator cannot be static type `X'
// Line: 8

static class X
{
	public static void Main ()
	{
		var v = null as X;
	}
}