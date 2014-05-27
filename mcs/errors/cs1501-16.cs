// CS1501: No overload for method `Block' takes `2' arguments
// Line: 12

class X
{
	public static void Block (object type, object variables, params object[] expressions)
	{
	}

	public static void Main ()
	{
		Block (variables: null, expressions: null);
	}
}