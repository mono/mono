namespace N1.N3.N4
{
	public class Bar
	{
	}
}

namespace N1.N2
{
	using N3.N4;

	public class Foo : Bar
	{
	}
}

namespace N5.N6
{
	using N7.N8;

	public class Foo : Bar
	{
	}
}

namespace N5.N7.N8
{
	public class Bar
	{
	}
}

namespace FirstOuter
{
	namespace FirstInner
	{
		public class First
		{
			public string MyIdentity { 
				get {
					return this.GetType().FullName;
				}		
			}
		}
	}
	
	public class Second : FirstInner.First {}
	
	namespace SecondInner
	{
		public class Third : FirstOuter.FirstInner.First {}
	}
	
	namespace FirstInner // purposefully again
	{
		public class Fourth : First {} // must understand First in the nom qualified form
	}
}

public class Fifth : FirstOuter.FirstInner.First {}

namespace M1
{
	using X = P1;
	namespace M2
	{
		using Y = X.P2;
		namespace M3
		{
			public class Foo : Y.Bar
			{ }
		}
	}
}

namespace P1
{
	namespace P2
	{
		public class Bar
		{ }
	}
}

class X
{
	public static int Main ()
	{
		// Compilation-only test.
		return 0;
	}
}
