// Compiler options: -langversion:6

using System;

namespace A.B
{
	static class X
	{
		public static int Test (object o)
		{
			return 1;
		}
	}
}

namespace A.C
{
	static class X
	{
		public static int Test (int o)
		{
			return 2;
		}
	}
}

namespace C
{
	using static A.B.X;
	using static A.C.X;

	class M
	{
		public static int Main ()
		{
			if (Test (3) != 2)
				return 1;

			return 0;
		}
	}
}