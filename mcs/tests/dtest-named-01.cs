public class Test
{
	public void Foo (out int arg)
	{
		arg = 5;
	}

	public static int Main ()
	{
		dynamic d = new Test ();
		int x;
		d.Foo (arg: out x);
		if (x != 5)
			return 1;

		return 0;
	}
}