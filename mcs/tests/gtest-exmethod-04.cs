// Compiler options: -r:gtest-exmethod-04-lib.dll

namespace A
{
	public static class Test
	{
		public static string Test_1 (this bool t)
		{
			return ":";
		}
	}
}

namespace B
{
	using A;
	
	public class M
	{
		public static void Main ()
		{
			"".Test_1();
		}
	}
}