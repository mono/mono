class Simple(int arg)
{
	int Property { get; } = arg;

	public static int Main ()
	{
		var c = new Simple (4);
		if (c.Property != 4)
			return 1;

		var s = new S (4.3m);
		if (s.Property != 4.3m)
			return 1;

		return 0;
	}
}

struct S(decimal arg)
{
	internal decimal Property { get; } = arg;
}