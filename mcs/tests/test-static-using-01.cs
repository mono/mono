// Compiler options: -langversion:6

using A.B.X;

namespace A.B
{
	static class X
	{
		public static int Test ()
		{
			return 5;
		}
	}
}

namespace C
{
	class M
	{
		public static int Main ()
		{
			if (Test () != 5)
				return 1;

			return 0;
		}
	}
}