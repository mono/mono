namespace N1
{
	using System;
	using RVA = N2.G<N2.Test>;
}

namespace N2
{
	using System;

	[Obsolete ("old version", true)]
	class Test
	{
	}

	class G<T>
	{
	}

	class C
	{
		public static void Main ()
		{
		}
	}
}