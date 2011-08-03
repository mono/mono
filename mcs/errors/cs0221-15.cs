// CS0221: Constant value `256' cannot be converted to a `byte' (use `unchecked' syntax to override)
// Line: 13

class Test
{
	public static explicit operator Test (byte b)
	{
		return null;
	}

	static void Main ()
	{
		var a = (Test) 256UL;
	}
}
