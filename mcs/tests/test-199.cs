public class Test
{
	public static int Main ()
	{
		N1.Foo.Foo2 foo2 = new N1.Foo.Foo2 ();
		if (foo2.Talk () != 1)
			return 1;

		return 0;
	}
}

namespace N1
{
	public class Foo : N2.Bar
	{
		public class Foo2 : Bar2
		{
		}
	}

	public class Bar2
	{
		public int Talk ()
		{
			return 1;
		}
	}
}

namespace N2
{
	public class Bar
	{
		private class Bar2
		{
			public int Talk ()
			{
				return 2;
			}
		}
	}
}
