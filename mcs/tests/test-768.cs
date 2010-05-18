namespace A.N
{
	class Wrong
	{
	}
}

namespace N
{
	class C
	{
		public static string value;
	}
}

namespace X
{
	using A;
	
	public class TestClass
	{
		public static void Main ()
		{
			string s = N.C.value;
		}
	}
}
