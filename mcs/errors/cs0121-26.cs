// CS0121: The call is ambiguous between the following methods or properties: `A.B.X.Test(this int)' and `A.C.X.Test(this int)'
// Line: 37

using System;

namespace A.B
{
	static class X
	{
		public static int Test (this int o)
		{
			return 1;
		}
	}
}

namespace A.C
{
	static class X
	{
		public static int Test (this int o)
		{
			return 2;
		}
	}
}

namespace C
{
	using A.B;
	using static A.C.X;

	class M
	{
		public static int Main ()
		{
			if (1.Test () != 1)
				return 1;

			return 0;
		}
	}
}