// CS0121: The call is ambiguous between the following methods or properties: `A.B.X.Test(int)' and `A.C.X.Test(int)'
// Line: 31

using A.B.X;
using A.C.X;

namespace A.B
{
	static class X
	{
		public static void Test (int o)
		{
		}
	}
}

namespace A.C
{
	static class X
	{
		public static int Test (int o)
		{
		}
	}
}

class M
{
	public static void Main ()
	{
		Test (0);
	}
}