class Test
{
	static string V;

	static int f (int a)
	{
		V += a;
		return a;
	}

	static void m (int a, int b, int c)
	{
	}

	static void m (int a, int b, int c, int d)
	{
	}

	static int Main ()
	{
		V = "";
		m (f (1), b: f (2), c: f (3));
		if (V != "123")
			return 1;

		V = "";
		m (a: f (1), c: f (2), b: f (3));
		if (V != "123")
			return 2;

		V = "";
		m (f (1), c: f (2), b: f (3));
		if (V != "123")
			return 3;

		V = "";
		m (f (1), f (2), c: f (3), d: f (4));
		if (V != "1234")
			return 4;

		V = "";
		m (f (1), f (2), d: f (3), c: f (4));
		if (V != "1234")
			return 5;

		return 0;
	}
}
