class MyTest {
	static void f (bool a, bool b)
	{
		if (a != b)
			throw new System.Exception ("Something wrong: " + a + " vs. " + b);
	}
	public static void Main()
	{
		int? w = null;
		int? x = null;
		int? y = 0;
		int? z = 1;

		f (false, x == 0);
		f (true,  x != 0);
		f (false, x == y);
		f (true,  x != y);
		f (true,  x == w);
		f (false, x != w);
		f (true,  x == null);
		f (false, x != null);

		f (true,  0 == y);
		f (false, 0 != y);
		f (false, z == y);
		f (true,  z != y);
		f (false, null == y);
		f (true,  null != y);
	}
}
