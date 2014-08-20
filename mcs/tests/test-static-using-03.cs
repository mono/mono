// Compiler options: -langversion:6

using System;
using A.B.X;

namespace A.B
{
	static class X
	{
		public static int Test (int o)
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
	using A.C.X;

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