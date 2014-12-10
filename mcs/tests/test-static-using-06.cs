// Compiler options: -langversion:6

using System;
using static A.B.X;
using static A.C.X;

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
		public static int Test<T> (T o)
		{
			if (typeof (T) != typeof (object))
				return -1;

			return 2;
		}
	}
}

namespace C
{
	class M
	{
		public static int Main ()
		{
			if (Test<object> ("") != 2)
				return 1;

			return 0;
		}
	}
}