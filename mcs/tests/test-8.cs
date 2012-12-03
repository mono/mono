class X {
	public static int Main (string [] args)
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

		if (a++ != 10)
			return 5;
		if (++a != 12)
			return 6;
		if (b-- != 10)
			return 7;
		if (--b != 8)
			return 8;
				
		return 0;
	}
}
