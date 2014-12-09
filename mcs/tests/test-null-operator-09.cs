delegate int D (int t);

class X
{
	D d = delegate { return 4; };

	public static int Main ()
	{
		X x = null;

		var res = x?.d (55);
		if (res != null)
			return 1;
			
		x?.d (1);

		return 0;
	}
}