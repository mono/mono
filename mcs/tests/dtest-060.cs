namespace Test
{
	public class Program
	{
		public static int Main ()
		{
			dynamic d = 0L;
			return C.M<Program> (d);
		}
	}

	public class C
	{
		public static int M<T> (int i) where T : C
		{
			return 1;
		}

		public static int M<T> (long l)
		{
			return 0;
		}
	}
}
