class X
{
	public static int Main ()
	{
		const string s = nameof (X);
		System.Console.WriteLine (s);
		if (s != "X")
			return 1;

		return 0;
	}
}