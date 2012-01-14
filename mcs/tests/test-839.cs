namespace N1
{
	using IndexingChain = N2.D.IndexingChain;

	class M
	{
		public static void Main ()
		{
		}
	}
}

namespace N2
{
	using N3;

	public class D : C
	{
	}
}

namespace N3
{
	using System.Collections.Generic;

	public class C
	{
		class Foo : List<int>
		{
		}

		public class IndexingChain
		{
		}
	}

}

