class X {
	public static int Main (string [] args)
	{
		decimal a, b, c, d;

		a = 0;
		b = 1;
		c = d = 3;

		if (b + b + b != c)
			return 1;

		if (a != (b - 1))
			return 2;

		if (c != d)
			return 3;

		if (!(c == d))
			return 4;
		
		return 0;
	}
}
