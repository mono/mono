

namespace A
{
	public static class Test_A
	{
		public static string Test_1 (this string s)
		{
			return ":";
		}
	}
}

namespace A
{
	public static partial class Test_B
	{
		public static string Test_2 (this string s)
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
			"".Test_2();
		}
	}
}