// CS1501: No overload for method `Test' takes `2' arguments
// Line: 16
// Compiler options: -langversion:future

static class S
{
	public static int Test (this int value)
	{
	return value;
	}
}

class M
{
	public static void Main ()
	{
		1.Test (value: 1);
	}
}
