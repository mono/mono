// CS8307: The first operand of an `as' operator may not be a tuple literal without a natural type
// Line: 8

class X
{
	public static void Main ()
	{
		var g = (1, Main) as object;
	}
}