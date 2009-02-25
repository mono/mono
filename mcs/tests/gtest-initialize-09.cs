class Test
{
	struct Foo { public int[] Data; }

	public static int Main ()
	{
		int[] res = new Foo () { Data = new int[] { 1, 2, 3 } }.Data;
		if (res.Length != 3)
			return 1;

		return 0;
	}
}