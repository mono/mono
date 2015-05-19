// parser test

class X
{
	public static int T1 (int seconds)
	{
		return T1_Foo (value: seconds * 1000);
	}

	public static int T1_Foo (int value = 0)
	{
		return value;
	}

	public static void Main ()
	{
	}
}