class Z {
	int v;

	public int func (Z v)
	{
		return ++v.v;
	}

	static public int func2 (Z v)
	{
		return v.v++;
	}
}

class X {
	static int Main ()
	{
		Z z = new Z ();

		if (z.func (z) != 1)
			return 1;
		return 0;
	}
}
