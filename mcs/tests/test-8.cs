class X {
	static public int Main (string [] args)
	{
		int a, b, c, d;

		a = b = 10;
		c = d = 14;

		if ((a + b) != 20)
			return 1;
		if ((a + d) != 24)
			return 2;
		if ((c + d) != 28)
			return 3;
		if ((b + c) != 24)
			return 4;

		return 0;
	}
}
