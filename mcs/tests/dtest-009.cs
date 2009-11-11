partial class B
{
	static partial void Test (int a);

	static partial void Test (int x)
	{
	}

	public static int Main ()
	{
		Test (a: 5);

		dynamic d = -1;
		Test (a: d);

		return 0;
	}
}