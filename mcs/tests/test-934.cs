class X
{
	public static int Main ()
	{
		var a = new byte[] { };
		var b = new byte[] { };
		if (a.Equals (b))
			return 1;

		if (ReferenceEquals (a, b))
			return 2;

		a = new byte[0];
		b = new byte[0];
		if (a.Equals (b))
			return 3;

		if (ReferenceEquals (a, b))
			return 4;

		return 0;
	}
}