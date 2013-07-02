class Test
{
	static int test1 ()
	{
		var s = "Nice";
		switch (s) {
		case "HI":
			const string x = "Nice";
			return 1;
		case x:
			return 2;
		}

		return 3;
	}

	public const string xx = "Not";
	static int test2 ()
	{
		var s = "Nice";
		switch (s) {
		case "HI":
			const string xx = "Nice";
			return 1;
		case xx:
			return 2;
		}

		return 3;
	}

	static int Main ()
	{
		if (test1 () != 2)
			return 1;

		if (test2 () != 2)
			return 2;

		return 0;
	}
}