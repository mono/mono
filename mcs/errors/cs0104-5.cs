// CS0104: `N' is an ambiguous reference between `C.N' and `A.T.N'
// Line: 32

namespace A
{
	public class T
	{
		public class N
		{

		}
	}
}

namespace C
{
	struct N
	{

	}
}

namespace B
{
	using static A.T;
	using C;

	static class Program
	{
		static void Main ()
		{
			var u = new N ();
		}
	}
}
