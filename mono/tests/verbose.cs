using System;

namespace N1
{
	public class Test
	{
		public static void Method()
		{
		}

		public static int Main ()
		{
			Method();
			N2.Test.Method(0);
			N2.Test.Method();
			N2.Test.Method(0, new string[0]);
			return 0;
		}
	}
}

namespace N2
{
	public class Test
	{
		public static void Method(int n)
		{
		}

		public static void Method()
		{
		}

		public static void Method(int n, string[] args)
		{
		}
	}
}


