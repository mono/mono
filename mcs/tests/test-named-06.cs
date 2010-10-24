class C
{
	public static int Main ()
	{
		var d = new C ();
		if (d.Foo (x: 1, y : 2) != 3)
			return 1;
		
		return 0;
	}

	public int Foo (int x, long y, string a = "a")
	{
		return 1;
	}

	public int Foo (int x, long y, params string[] args)
	{
		return 2;
	}

	public int Foo (long y, int x)
	{
		return 3;
	}
}
