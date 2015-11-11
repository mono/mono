class X
{
	public static int Main ()
	{
		int foo = 4;
		string s = $@"{foo}";
		if (s != "4")
			return 1;

		string s2 = $@"c:\{foo}\temp";
		if (s2 != "c:\\4\\temp")
			return 2;

		string s3 = $@"""{foo}"" ""foo""";
		if (s3 != "\"4\" \"foo\"")
			return 3;

		return 0;
	}
}